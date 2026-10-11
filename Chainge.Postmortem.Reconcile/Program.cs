using System.Globalization;
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

ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, 3);
var root = Path.GetFullPath(args[0]);
var configBytes = await File.ReadAllBytesAsync(Path.GetFullPath(args[1]));
var settings = JsonSerializer.Deserialize<EvidenceSettings>(configBytes, JsonSerializerOptions.Web) ?? throw new InvalidDataException("Missing settings");
settings = settings.NormalizeAddresses();
var services = new ServiceCollection().AddReconciliation(settings);
await using var provider = services.BuildServiceProvider();
var snapshot = await provider.GetRequiredService<EvidenceRepository>().ReadEvidenceAsync(root, CancellationToken.None);
var result = MovementMatcher.MatchMovements(snapshot.Events, settings.Policy);
result = result with { Movements = MovementRiskScanner.FlagMovements(snapshot.Events, result.Movements, settings.Policy) };
var output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(output);
var options = new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true };
await File.WriteAllTextAsync(Path.Combine(output, "reconciliation.json"), JsonSerializer.Serialize(new { settings.Asset, Policy = settings.Policy, settings.Limitations, ConfigurationSha256 = Convert.ToHexStringLower(SHA256.HashData(configBytes)), snapshot.InputHashes, snapshot.Diagnostics, snapshot.Events, Review = result }, options));
await ReviewExporter.WriteReviewFiles(output, snapshot.Events, result, CancellationToken.None);
Console.WriteLine($"{settings.Asset}: {snapshot.Events.Count} retained events; {result.Movements.Count} operating-address movements; {result.ConversionEventIds.Count} conversion-related issuance events excluded from external matching.");
var byId = snapshot.Events.ToDictionary(e => e.Id, StringComparer.Ordinal);
foreach (var group in result.Movements.GroupBy(m => new { byId[m.SourceEventId].Chain, byId[m.SourceEventId].Token, m.Classification, m.Direction }))
{
    var total = group.Aggregate(BigInteger.Zero, (sum, m) => sum + BigInteger.Parse(byId[m.SourceEventId].RawAmount, CultureInfo.InvariantCulture));
    Console.WriteLine($"{group.Key.Chain} {group.Key.Token} {group.Key.Direction} {group.Key.Classification}: {group.Count()} events; {total} raw units");
}
Console.WriteLine($"{snapshot.Events.Count(e => e.Timestamp is null)} events lack timestamps; {result.Movements.Count(m => m.Candidates.Any(c => c.Competing))} movements have competing matches. All proposed pairings require review.");
