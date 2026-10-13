using System.Globalization;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Models.Extensions;
using Chainge.Postmortem.Reconcile.Extensions;
using Chainge.Postmortem.Reconcile;
using Microsoft.Extensions.DependencyInjection;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Chainge.Postmortem.Persistence.Json;
using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Acquisition.Models;

ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, 3);
if (args[0] == "plan-capture")
{
    var captureSettings = (JsonSerializer.Deserialize<EvidenceSettings>(await File.ReadAllBytesAsync(args[1]), JsonSerializerOptions.Web) ?? throw new InvalidDataException("Missing settings.")).NormalizeAddresses();
    await using var planningProvider = new ServiceCollection().AddReconciliation(captureSettings).BuildServiceProvider();
    var evidence = await planningProvider.GetRequiredService<EvidenceRepository>().ReadEvidenceAsync(Directory.GetCurrentDirectory(), CancellationToken.None);
    var logQueries = await planningProvider.GetRequiredService<LogQueryRepository>().ReadQueriesAsync(Directory.GetCurrentDirectory(), CancellationToken.None);
    await EvidenceIntegrityVerifier.VerifyAsync(Directory.GetCurrentDirectory(), evidence.InputHashes, CancellationToken.None);
    await EvidenceIntegrityVerifier.VerifyAsync(Directory.GetCurrentDirectory(), logQueries.InputHashes, CancellationToken.None);
    var coverage = LogHistoryReviewer.ReviewCoverage(captureSettings.LogHistoryTargets, logQueries.Windows);
    Directory.CreateDirectory(args[2]);
    var planOptions = new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true };
    foreach (var item in CapturePlanBuilder.BuildPlans(evidence.Events, coverage, captureSettings.CapturePolicies))
    {
        var file = Path.Combine(args[2], item.Key.ToLowerInvariant() + ".json");
        await File.WriteAllBytesAsync(file, JsonSerializer.SerializeToUtf8Bytes(item.Value, planOptions));
        Console.WriteLine($"{item.Key}: {item.Value.Queries.Count} capture queries -> {file}");
    }
    return;
}
if (args[0] == "capture")
{
    var plan = JsonSerializer.Deserialize<CapturePlan>(await File.ReadAllBytesAsync(args[1]), JsonSerializerOptions.Web) ?? throw new InvalidDataException("Missing capture plan.");
    await using var captureProvider = new ServiceCollection().AddAcquisition(plan, Path.GetFullPath(args[2])).BuildServiceProvider();
    var progress = new Progress<CaptureProgress>(p => Console.WriteLine($"{p.Name}: {p.Status}"));
    var captured = await captureProvider.GetRequiredService<EvidenceCapturer>().CaptureEvidence(plan, progress, CancellationToken.None);
    Console.WriteLine(JsonSerializer.Serialize(captured, JsonSerializerOptions.Web));
    Environment.ExitCode = captured.IdentityVerified && captured.Remaining == 0 ? 0 : 2;
    return;
}
var root = Path.GetFullPath(args[0]);
var configBytes = await File.ReadAllBytesAsync(Path.GetFullPath(args[1]));
var settings = JsonSerializer.Deserialize<EvidenceSettings>(configBytes, JsonSerializerOptions.Web) ?? throw new InvalidDataException("Missing settings");
settings = settings.NormalizeAddresses();
var services = new ServiceCollection().AddReconciliation(settings);
await using var provider = services.BuildServiceProvider();
var snapshot = await provider.GetRequiredService<EvidenceRepository>().ReadEvidenceAsync(root, CancellationToken.None);
var discovery = await provider.GetRequiredService<ExplorerDiscoveryRepository>().ReadDiscoveryAsync(root, CancellationToken.None);
var queries = await provider.GetRequiredService<LogQueryRepository>().ReadQueriesAsync(root, CancellationToken.None);
var balances = await provider.GetRequiredService<BalanceRepository>().ReadBalancesAsync(root, CancellationToken.None);
await EvidenceIntegrityVerifier.VerifyAsync(root, balances.InputHashes, CancellationToken.None);
await EvidenceIntegrityVerifier.VerifyAsync(root, queries.InputHashes, CancellationToken.None);
var evidenceManifestHashes = await EvidenceIntegrityVerifier.VerifyAsync(root, snapshot.InputHashes.Concat(discovery.InputHashes).GroupBy(p => p.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Select(p => p.Value).Distinct(StringComparer.Ordinal).Single(), StringComparer.Ordinal), CancellationToken.None);
if (!snapshot.Events.Any(e => e.Chain == settings.Policy.FusionChain)) { Console.WriteLine("Coverage warning: no Fusion events supplied. Empty issuance/burn queues are not evidence of zero claims or complete settlement."); }
var result = MovementMatcher.MatchMovements(snapshot.Events, settings.Policy);
result = result with { Movements = MovementRiskScanner.FlagMovements(snapshot.Events, result.Movements, settings.Policy) };
var purposes = FusionPurposeClassifier.ClassifyEvents(snapshot.Events, settings.Policy);
var delayed = DelayedCandidateFinder.FindCandidates(snapshot.Events, settings.Policy, result);
var inventory = BurnInventoryReviewer.ReviewBurns(snapshot.Events, result, settings.Policy);
var payoutCandidates = PayoutAddressFinder.FindCandidates(snapshot.Events, discovery.Observations, result, delayed, settings.Policy);
var output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(output);
var options = new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true };
await File.WriteAllTextAsync(Path.Combine(output, "balance-checks.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Pinned balanceOf observations compared with zero-opening Transfer replay. Agreement does not prove custody, backing, absence of cancelling omissions or claim eligibility. Query coverage and balance agreement are separate checks. Failed state reads remain unavailable.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), BalanceInputHashes = balances.InputHashes, EventInputHashes = snapshot.InputHashes, QueryInputHashes = queries.InputHashes, Checks = BalanceReviewer.ReviewBalances(snapshot.Events, balances.Observations, queries.Windows) }, options));
var historyCoverage = LogHistoryReviewer.ReviewCoverage(settings.LogHistoryTargets, queries.Windows);
await File.WriteAllTextAsync(Path.Combine(output, "balance-intervals.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Changes between consecutive pinned balance blocks, compared with retained transfers after the opening block through the closing block. All same-block observations remain visible; conflicting states are not resolved by choosing a source. Net agreement does not rule out cancelling omissions or certify earlier history, custody or backing.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), BalanceInputHashes = balances.InputHashes, EventInputHashes = snapshot.InputHashes, QueryInputHashes = queries.InputHashes, Checks = BalanceIntervalReviewer.ReviewIntervals(snapshot.Events, balances.Observations, queries.Windows) }, options));
await File.WriteAllTextAsync(Path.Combine(output, "history-coverage.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Bounded Transfer-log query coverage for explicitly configured token/address targets. Successful provider responses are not independent proof that a provider never omits logs. Both directions, balance checks and chain continuity still matter. Unpinned and unsupported queries receive no coverage credit.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), queries.InputHashes, queries.Diagnostics, Coverage = historyCoverage }, options));
foreach (var source in SourcePositionBuilder.BuildSnapshots(snapshot.Events, settings.LogHistoryTargets, queries.Windows, settings.SourceCutoffBlocks))
{
    await File.WriteAllTextAsync(Path.Combine(output, $"source-positions-{source.Chain.ToLowerInvariant()}.json"), JsonSerializer.Serialize(new { settings.Asset, source.Chain, source.Block, Scope = "Observed Transfer-log positions starting from zero, not certified reserves. Review both-direction query coverage and independent balanceOf checks before interpreting positions as balances. Zero positions are omitted; absent histories remain unknown. No representations are added together.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, QueryInputHashes = queries.InputHashes, source.Coverage, source.Positions }, options));
}
var accounting = EventAccountingBuilder.BuildReport(snapshot.Events, result, settings.Policy);
var routing = RoutingInputReviewer.ReviewInputs(snapshot.Events, settings.Policy);
var positions = ClaimPositionBuilder.BuildPositions(snapshot.Events, settings.Policy);
await File.WriteAllTextAsync(Path.Combine(output, "claim-positions.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Per-token observed balances at the configured Fusion cutoff. Interval status is mechanical, not proof of claim eligibility. Sink, treasury and migration positions are retained; representations are not summed into a liability. Missing histories remain unknown.", settings.Policy.ClaimCutoffBlock, settings.Policy.ClaimCutoffUnixTime, settings.Policy.ClaimIntervals, ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, Positions = positions }, options));
await File.WriteAllTextAsync(Path.Combine(output, "routing-review.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Observed calldata layout, not a verified contract ABI. Memo amounts are unverified quoted values. Exact destination/amount/time candidates do not establish settlement; no accounting or normal classifications are changed. Unsupported formats remain unresolved.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, Observations = routing }, options));
foreach (var group in routing.GroupBy(r => r.Status)) { Console.WriteLine($"Routing input {group.Key}: {group.Count()}"); }
await File.WriteAllTextAsync(Path.Combine(output, "accounting-summary.json"), JsonSerializer.Serialize(new { settings.Asset, Scope = "Retained events only. Net issuance is not consolidated outstanding claims; net flows are not observed vault balances. Slice totals are event sums, not time-normalized claims. Missing chain/token rows mean no retained events, not zero supply or balance.", ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), EvidenceManifestHashes = evidenceManifestHashes, snapshot.InputHashes, DiscoveryInputHashes = discovery.InputHashes, snapshot.Diagnostics, settings.Limitations, Accounting = accounting }, options));
await File.WriteAllTextAsync(Path.Combine(output, "reconciliation.json"), JsonSerializer.Serialize(new { settings.Asset, Policy = settings.Policy, settings.Limitations, ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, snapshot.Diagnostics, snapshot.Events, Review = result, FusionPurposes = purposes, DelayedReview = delayed, BurnInventory = inventory, BurnReviewQueues = inventory.GroupBy(r => r.GetReviewQueue()).ToDictionary(g => g.Key, g => g.Select(r => r.BurnEventId).ToList()), PayoutCandidates = payoutCandidates, Discovery = discovery }, options));
var siteReport = MovementSiteBuilder.BuildReport(snapshot.Events, result);
var terminal = TerminalOutflowReviewer.ReviewAddresses(snapshot.Events, settings.Policy);
var crossChainTerminalRecipients = TerminalOutflowReviewer.FindCrossChainRecipients(snapshot.Events, terminal);
var sharedTerminalDestinations = TerminalOutflowReviewer.FindSharedDestinations(snapshot.Events, terminal);
await File.WriteAllTextAsync(Path.Combine(output, "site-movements.json"), JsonSerializer.Serialize(new { settings.Asset, ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, settings.Limitations, siteReport.Cases, siteReport.Destinations, DelayedReview = delayed, Terminal = terminal, SharedTerminalDestinations = sharedTerminalDestinations, CrossChainTerminalRecipients = crossChainTerminalRecipients, TerminalPolicy = new { settings.Policy.LargeOutflowRaw, settings.Policy.Decimals, settings.Policy.DepletionBasisPoints } }, JsonSerializerOptions.Web));
await ReviewExporter.WriteReviewFiles(output, snapshot.Events, result, CancellationToken.None);
await ReviewExporter.WritePurposeFile(output, snapshot.Events, purposes, result, CancellationToken.None);
await ReviewExporter.WriteReviewFiles(Path.Combine(output, "delayed-review"), snapshot.Events, result with { Movements = delayed }, CancellationToken.None);
await ReviewExporter.WriteDiscoveryFiles(output, snapshot.Events.Concat(discovery.Observations).ToList(), inventory, payoutCandidates, discovery.Coverage, CancellationToken.None);
Console.WriteLine($"Payout discovery: {payoutCandidates.Count} candidate edges; {discovery.Coverage.Count} retained explorer lookups. No address role automatically promoted.");
foreach (var group in inventory.GroupBy(r => r.Classification)) { Console.WriteLine($"Burn review {group.Key}: {group.Count()}"); }
Console.WriteLine($"Delayed review: {delayed.Sum(m => m.Candidates.Count)} additional Fusion edges; {delayed.Where(m => m.Direction == "outgoing").Sum(m => m.SourceCandidates.Count)} additional source-chain edges. Normal classifications unchanged.");
Console.WriteLine($"{settings.Asset}: {snapshot.Events.Count} retained events; {result.Movements.Count} operating-address movements; {result.ConversionEventIds.Count} conversion-related issuance events excluded from external matching.");
var byId = snapshot.Events.ToDictionary(e => e.Id, StringComparer.Ordinal);
foreach (var group in result.Movements.GroupBy(m => new { byId[m.SourceEventId].Chain, byId[m.SourceEventId].Token, m.Classification, m.Direction }))
{
    var total = group.Aggregate(BigInteger.Zero, (sum, m) => sum + BigInteger.Parse(byId[m.SourceEventId].RawAmount, CultureInfo.InvariantCulture));
    Console.WriteLine($"{group.Key.Chain} {group.Key.Token} {group.Key.Direction} {group.Key.Classification}: {group.Count()} events; {total} raw units");
}
Console.WriteLine($"{snapshot.Events.Count(e => e.Timestamp is null)} events lack timestamps; {result.Movements.Count(m => m.Candidates.Any(c => c.Competing))} movements have competing matches. All proposed pairings require review.");

foreach (var group in purposes.GroupBy(p => p.Classification)) { Console.WriteLine($"Fusion purpose {group.Key}: {group.Count()}"); }
foreach (var group in snapshot.Events.Where(e => result.UnpairedFusionEventIds.Contains(e.Id)).GroupBy(e => e.From == "0x0000000000000000000000000000000000000000" ? "mint" : "burn")) { Console.WriteLine($"Unpaired Fusion {group.Key}: {group.Count()}"); }

foreach (var group in inventory.GroupBy(r => r.GetReviewQueue())) { Console.WriteLine($"Fusion burn queue {group.Key}: {group.Count()}"); }
Console.WriteLine($"Total Fusion burns without an external settlement pairing: {inventory.Count}. Inventory classification explains the balance change, not the external settlement; no events removed from accounting.");
