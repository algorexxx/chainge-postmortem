using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class SourceMovementMatcher
{
    public static IReadOnlyList<MovementReview> MatchMovements(IReadOnlyList<ChainEvent> events, IReadOnlyList<MovementReview> movements, MatchPolicy policy)
    {
        if (policy.SourceChainRule is not { } rule) { return movements; }
        var byId = events.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var eligible = movements.Where(m => m.Classification == "unmatched" && byId[m.SourceEventId].ReceiptVerified && byId[m.SourceEventId].Timestamp is not null).ToList();
        var incoming = eligible.Where(m => m.Direction == "incoming").Select(m => byId[m.SourceEventId]).ToLookup(e => e.From);
        var links = movements.ToDictionary(m => m.SourceEventId, _ => new List<SourcePairCandidate>(), StringComparer.Ordinal);
        var scale = BigInteger.Pow(10, 255 - policy.Decimals);
        var cap = BigInteger.Parse(rule.MaximumFeeRaw, CultureInfo.InvariantCulture) * scale;
        foreach (var e in eligible.Where(m => m.Direction == "outgoing").Select(m => byId[m.SourceEventId]))
        {
            foreach (var deposit in incoming[e.To].Where(d => d.Chain != e.Chain && d.RawAmount != "0" && e.RawAmount != "0"))
            {
                var delay = e.Timestamp!.Value - deposit.Timestamp!.Value;
                var fee = deposit.GetComparableUnits() - e.GetComparableUnits();
                if (delay < TimeSpan.Zero || delay > rule.MaximumDelay || fee < 0 || fee > cap || fee * 10000 > deposit.GetComparableUnits() * rule.MaximumFeeBasisPoints || fee % scale != 0) { continue; }
                var raw = (fee / scale).ToString(CultureInfo.InvariantCulture);
                links[e.Id].Add(new SourcePairCandidate { OtherEventId = deposit.Id, FeeRawHypothesis = raw, Delay = delay, Competing = false });
                links[deposit.Id].Add(new SourcePairCandidate { OtherEventId = e.Id, FeeRawHypothesis = raw, Delay = delay, Competing = false });
            }
        }
        return movements.Select(m => links[m.SourceEventId].Count == 0 ? m : m with { Classification = "possible-source-chain-transfer", SourceCandidates = links[m.SourceEventId].OrderBy(c => c.OtherEventId, StringComparer.Ordinal).Select(c => c with { Competing = links[m.SourceEventId].Count > 1 || links[c.OtherEventId].Count > 1 }).ToList() }).ToList();
    }
}
