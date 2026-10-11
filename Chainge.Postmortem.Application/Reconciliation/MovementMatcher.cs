using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class MovementMatcher
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static ReconciliationResult MatchMovements(IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(policy.Decimals, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(policy.Decimals, 255);
        var fusion = events.Where(e => e.Chain == policy.FusionChain).ToList();
        var conversions = FusionPurposeClassifier.ClassifyEvents(events, policy).Where(p => p.ExcludeFromExternalMatching).Select(p => p.EventId).ToHashSet(StringComparer.Ordinal);
        var issuance = fusion.Where(e => e.FullTime && e.RawAmount != "0" && (e.From == Zero || e.To == Zero) && !conversions.Contains(e.Id)).ToList();
        var index = issuance.ToLookup(e => e.From == Zero ? e.To : e.From);
        var transactions = events.ToLookup(e => e.Chain + ":" + e.Transaction);
        var movements = events.Where(e => e.Chain != policy.FusionChain && policy.OperatingAddresses.ContainsKey(e.Chain))
            .Where(e => policy.OperatingAddresses[e.Chain].Contains(e.From) || policy.OperatingAddresses[e.Chain].Contains(e.To))
            .Select(e => MatchMovement(e, index, transactions[e.Chain + ":" + e.Transaction].ToList(), policy)).ToList();
        var usage = movements.SelectMany(m => m.Candidates).GroupBy(c => c.FusionEventId).ToDictionary(g => g.Key, g => g.Count());
        return new ReconciliationResult
        {
            Movements = SourceMovementMatcher.MatchMovements(events, movements.Select(m => m with { Candidates = m.Candidates.Select(c => c with { Competing = m.Candidates.Count > 1 || usage[c.FusionEventId] > 1 }).ToList() }).ToList(), policy),
            ConversionEventIds = conversions.Order(StringComparer.Ordinal).ToList(),
            UnpairedFusionEventIds = issuance.Where(e => !usage.ContainsKey(e.Id)).Select(e => e.Id).Order(StringComparer.Ordinal).ToList()
        };
    }

    private static MovementReview MatchMovement(ChainEvent e, ILookup<string, ChainEvent> index, IReadOnlyList<ChainEvent> transaction, MatchPolicy policy)
    {
        var addresses = policy.OperatingAddresses[e.Chain];
        var outgoing = addresses.Contains(e.From);
        var internalTransfer = outgoing && addresses.Contains(e.To);
        var classification = e.RawAmount == "0" ? "zero-value" : internalTransfer ? "operating-address-transfer" : e.Timestamp is null ? "missing-timestamp" : "unmatched";
        var candidates = classification == "unmatched" ? FusionCandidateFinder.FindCandidates(e, index[outgoing ? e.To : e.From], policy, outgoing) : [];
        if (classification == "unmatched" && candidates.Count == 0 && HasCounterAsset(e, outgoing)) { classification = "possible-asset-exchange"; }
        var exchanges = FindReportedExchanges(e, outgoing);
        exchanges.AddRange(PoolExchangeMatcher.FindExchanges(e, outgoing));
        exchanges.AddRange(RelayExchangeMatcher.FindExchanges(e, transaction, outgoing));
        if (classification == "unmatched" && candidates.Count == 0 && exchanges.Count > 0) { classification = "reported-asset-exchange"; }
        return new MovementReview { ExchangeEvidence = exchanges, SourceEventId = e.Id, Direction = internalTransfer ? "internal" : outgoing ? "outgoing" : "incoming", Classification = candidates.Count == 0 ? classification : outgoing ? "possible-redemption" : "possible-deposit", Candidates = candidates.OrderBy(c => c.FusionEventId, StringComparer.Ordinal).ToList() };
    }
    private static List<ReceiptExchange> FindReportedExchanges(ChainEvent e, bool outgoing)
    {
        if (!e.ReceiptVerified || !e.ReceiptContextCaptured) { return []; }
        return e.ReportedExchanges.Where(s => s.SourceToken != s.DestinationToken && s.SpentRaw != "0" && s.ReturnedRaw != "0")
            .Where(s => outgoing ? s.Sender == e.From && s.SourceToken == e.Token && s.SpentRaw == e.RawAmount : s.Recipient == e.To && s.DestinationToken == e.Token && s.ReturnedRaw == e.RawAmount).ToList();
    }

    private static bool HasCounterAsset(ChainEvent e, bool outgoing)
    {
        if (!e.ReceiptVerified || !e.ReceiptContextCaptured) { return false; }
        var holder = outgoing ? e.From : e.To;
        return e.RelatedTransfers.GroupBy(t => t.Token).Any(group =>
        {
            var net = group.Aggregate(BigInteger.Zero, (sum, t) => sum + (t.To == holder ? BigInteger.Parse(t.RawAmount, CultureInfo.InvariantCulture) : 0) - (t.From == holder ? BigInteger.Parse(t.RawAmount, CultureInfo.InvariantCulture) : 0));
            return outgoing ? net > 0 : net < 0;
        });
    }

}
