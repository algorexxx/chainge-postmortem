using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class FusionCandidateFinder
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<PairCandidate> FindCandidates(ChainEvent e, IEnumerable<ChainEvent> issuance, MatchPolicy policy, bool outgoing)
    {
        var candidates = new List<PairCandidate>();
        foreach (var f in issuance.Where(f => f.Timestamp is not null && (outgoing ? f.To == Zero : f.From == Zero)))
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
        if (candidates.Count == 0 && outgoing && policy.VariableFeeRules.TryGetValue(e.Chain, out var rule))
        {
            candidates = FindVariableFeeCandidates(e, issuance, policy, rule, false, true);
            if (candidates.Count == 0 && e.ReceiptVerified && rule.AbsoluteFeeMaximumDelay > TimeSpan.Zero)
            {
                candidates = FindVariableFeeCandidates(e, issuance, policy, rule, true, true);
            }
        }
        if (candidates.Count == 0 && !outgoing && e.ReceiptVerified && policy.DepositFeeRules.TryGetValue(e.Chain, out var depositRule))
        {
            candidates = FindVariableFeeCandidates(e, issuance, policy, depositRule, false, false);
        }
        return candidates;
    }

    private static List<PairCandidate> FindVariableFeeCandidates(ChainEvent e, IEnumerable<ChainEvent> issuance, MatchPolicy policy, VariableFeeRule rule, bool absoluteOnly, bool outgoing)
    {
        var result = new List<PairCandidate>();
        var scale = BigInteger.Pow(10, 255 - policy.Decimals);
        var maximumFee = BigInteger.Parse(rule.MaximumFeeRaw, CultureInfo.InvariantCulture) * scale;
        foreach (var f in issuance.Where(f => (outgoing ? f.To == Zero : f.From == Zero && f.ReceiptVerified) && f.Timestamp is not null))
        {
            var delay = outgoing ? e.Timestamp!.Value - f.Timestamp!.Value : f.Timestamp!.Value - e.Timestamp!.Value;
            var fee = outgoing ? f.GetComparableUnits() - e.GetComparableUnits() : e.GetComparableUnits() - f.GetComparableUnits();
            var maximumDelay = absoluteOnly ? rule.AbsoluteFeeMaximumDelay : rule.MaximumDelay;
            var relativeFeeAccepted = absoluteOnly ? f.ReceiptVerified : fee * 10000 <= e.GetComparableUnits() * rule.MaximumFeeBasisPoints;
            if (delay < TimeSpan.Zero || delay > maximumDelay || fee <= 0 || fee > maximumFee || !relativeFeeAccepted || fee % scale != 0) { continue; }
            result.Add(new PairCandidate { FusionEventId = f.Id, FeeRawHypothesis = (fee / scale).ToString(CultureInfo.InvariantCulture), Delay = delay, Competing = false, Rule = !outgoing ? "bounded-deposit-fee" : absoluteOnly ? "short-delay-absolute-fee" : "bounded-variable-fee" });
        }
        return result;
    }

}
