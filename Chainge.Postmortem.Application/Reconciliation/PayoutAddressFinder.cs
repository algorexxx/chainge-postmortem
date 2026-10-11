using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class PayoutAddressFinder
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<PayoutAddressCandidate> FindCandidates(IReadOnlyList<ChainEvent> events, IReadOnlyList<ChainEvent> discoveries, ReconciliationResult reconciliation, IReadOnlyList<MovementReview> delayed, MatchPolicy policy)
    {
        var unpaired = reconciliation.UnpairedFusionEventIds.ToHashSet(StringComparer.Ordinal);
        var burns = events.Where(e => e.Chain == policy.FusionChain && e.To == Zero && e.ReceiptVerified && e.Timestamp is not null && unpaired.Contains(e.Id))
            .Where(e => !policy.FusionInventoryAccounts.TryGetValue(e.Token, out var accounts) || !accounts.Contains(e.From)).ToLookup(e => e.From);
        var wider = policy.ExpandReviewWindow();
        var previous = reconciliation.Movements.ToDictionary(m => m.SourceEventId);
        var candidates = new List<PayoutAddressCandidate>();
        foreach (var e in events.Concat(discoveries).Where(e => e.Chain != policy.FusionChain && e.Timestamp is not null && e.RawAmount != "0" && e.From != Zero && e.From != e.To))
        {
            if (!policy.OperatingAddresses.TryGetValue(e.Chain, out var known) || known.Contains(e.From)) { continue; }
            foreach (var pair in FusionCandidateFinder.FindCandidates(e, burns[e.To], wider, true))
            {
                candidates.Add(new PayoutAddressCandidate { SourceEventId = e.Id, Pair = pair, ExistingMovementClassification = previous.TryGetValue(e.Id, out var movement) ? movement.Classification : e.ReceiptVerified ? "outside-operating-scope" : "explorer-only-discovery" });
            }
        }
        var usage = candidates.Select(c => new { c.SourceEventId, c.Pair.FusionEventId }).Concat(delayed.SelectMany(m => m.Candidates.Select(c => new { m.SourceEventId, c.FusionEventId }))).Distinct().GroupBy(c => c.FusionEventId).ToDictionary(g => g.Key, g => g.Count());
        var sourceUsage = candidates.GroupBy(c => c.SourceEventId).ToDictionary(g => g.Key, g => g.Count());
        return candidates.Select(c => c with { Pair = c.Pair with { Competing = usage[c.Pair.FusionEventId] > 1 || sourceUsage[c.SourceEventId] > 1 } }).OrderBy(c => c.SourceEventId, StringComparer.Ordinal).ThenBy(c => c.Pair.FusionEventId, StringComparer.Ordinal).ToList();
    }
}
