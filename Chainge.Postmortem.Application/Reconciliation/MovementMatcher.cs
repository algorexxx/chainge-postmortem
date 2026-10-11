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
        var conversions = FindConversions(fusion);
        var issuance = fusion.Where(e => e.FullTime && e.RawAmount != "0" && (e.From == Zero || e.To == Zero) && !conversions.Contains(e.Id)).ToList();
        var index = issuance.ToLookup(e => e.From == Zero ? e.To : e.From);
        var movements = events.Where(e => e.Chain != policy.FusionChain && policy.OperatingAddresses.ContainsKey(e.Chain))
            .Where(e => policy.OperatingAddresses[e.Chain].Contains(e.From) || policy.OperatingAddresses[e.Chain].Contains(e.To))
            .Select(e => MatchMovement(e, index, policy)).ToList();
        var usage = movements.SelectMany(m => m.Candidates).GroupBy(c => c.FusionEventId).ToDictionary(g => g.Key, g => g.Count());
        return new ReconciliationResult
        {
            Movements = movements.Select(m => m with { Candidates = m.Candidates.Select(c => c with { Competing = m.Candidates.Count > 1 || usage[c.FusionEventId] > 1 }).ToList() }).ToList(),
            ConversionEventIds = conversions.Order(StringComparer.Ordinal).ToList(),
            UnpairedFusionEventIds = issuance.Where(e => !usage.ContainsKey(e.Id)).Select(e => e.Id).Order(StringComparer.Ordinal).ToList()
        };
    }

    private static HashSet<string> FindConversions(List<ChainEvent> fusion)
    {
        var conversions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var transaction in fusion.GroupBy(e => e.Transaction))
        {
            foreach (var e in transaction.Where(e => e.From == Zero || e.To == Zero))
            {
                var holder = e.From == Zero ? e.To : e.From;
                if (transaction.Any(other => other.Token != e.Token && other.GetComparableUnits() == e.GetComparableUnits() && other.FullTime && e.FullTime && (e.From == Zero ? other.From == holder : other.To == holder)))
                {
                    conversions.Add(e.Id);
                }
            }
        }
        return conversions;
    }

    private static MovementReview MatchMovement(ChainEvent e, ILookup<string, ChainEvent> index, MatchPolicy policy)
    {
        var addresses = policy.OperatingAddresses[e.Chain];
        var outgoing = addresses.Contains(e.From);
        var internalTransfer = outgoing && addresses.Contains(e.To);
        var classification = e.RawAmount == "0" ? "zero-value" : internalTransfer ? "operating-address-transfer" : e.Timestamp is null ? "missing-timestamp" : "unmatched";
        var candidates = new List<PairCandidate>();
        if (classification == "unmatched")
        {
            foreach (var f in index[outgoing ? e.To : e.From].Where(f => f.Timestamp is not null && (outgoing ? f.To == Zero : f.From == Zero)))
            {
                var delay = outgoing ? e.Timestamp!.Value - f.Timestamp!.Value : f.Timestamp!.Value - e.Timestamp!.Value;
                var fee = f.GetComparableUnits() - e.GetComparableUnits();
                var feeScale = BigInteger.Pow(10, 255 - policy.Decimals);
                var acceptedFee = outgoing ? policy.RedemptionFeeRawHypotheses.Any(h => BigInteger.Parse(h, CultureInfo.InvariantCulture) * feeScale == fee) : fee.IsZero;
                if (acceptedFee && delay >= -policy.ClockTolerance && delay <= policy.MaximumDelay)
                {
                    candidates.Add(new PairCandidate { FusionEventId = f.Id, FeeRawHypothesis = (fee / feeScale).ToString(CultureInfo.InvariantCulture), Delay = delay, Competing = false });
                }
            }
        }
        return new MovementReview { SourceEventId = e.Id, Direction = internalTransfer ? "internal" : outgoing ? "outgoing" : "incoming", Classification = candidates.Count == 0 ? classification : outgoing ? "possible-redemption" : "possible-deposit", Candidates = candidates.OrderBy(c => c.FusionEventId, StringComparer.Ordinal).ToList() };
    }
}
