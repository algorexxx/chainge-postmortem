using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class MovementRiskScanner
{
    public static IReadOnlyList<MovementReview> FlagMovements(IReadOnlyList<ChainEvent> events, IReadOnlyList<MovementReview> movements, MatchPolicy policy)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(policy.DepletionBasisPoints, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(policy.DepletionBasisPoints, 10000);
        var balances = new Dictionary<string, BigInteger>(StringComparer.Ordinal);
        var reviews = movements.ToDictionary(m => m.SourceEventId, StringComparer.Ordinal);
        var output = new List<MovementReview>();
        var threshold = BigInteger.Parse(policy.LargeOutflowRaw, CultureInfo.InvariantCulture) * BigInteger.Pow(10, 255 - policy.Decimals);
        foreach (var e in events.OrderBy(e => e.Chain, StringComparer.Ordinal).ThenBy(e => e.Block).ThenBy(e => e.LogIndex))
        {
            var from = e.Chain + ":" + e.Token + ":" + e.From;
            var to = e.Chain + ":" + e.Token + ":" + e.To;
            var before = balances.GetValueOrDefault(from);
            var value = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
            balances[from] = before - value;
            balances[to] = balances.GetValueOrDefault(to) + value;
            if (!reviews.TryGetValue(e.Id, out var review)) { continue; }
            var outgoing = review.Direction == "outgoing";
            var flags = new List<string>();
            if (review.Classification is "possible-asset-exchange" or "reported-asset-exchange")
            {
                flags.Add("backing-disposition-unresolved");
                if (outgoing && e.GetComparableUnits() >= threshold) { flags.Add("large-asset-exchange-outflow"); }
            }
            if (review.Classification == "unmatched" && outgoing && e.GetComparableUnits() >= threshold) { flags.Add("large-unmatched-outflow"); }
            if (outgoing && before > 0 && value * 10000 >= before * policy.DepletionBasisPoints) { flags.Add("large-balance-reduction"); }
            if (review.Classification == "possible-source-chain-transfer") { flags.Add("backing-disposition-unresolved"); }
            if (review.SourceCandidates.Any(c => c.Competing)) { flags.Add("competing-source-matches"); }
            if (review.Candidates.Any(c => c.Rule == "short-delay-absolute-fee")) { flags.Add("fee-exceeds-relative-bound"); }
            if (review.Candidates.Any(c => c.Competing)) { flags.Add("competing-matches"); }
            if (!e.ReceiptVerified) { flags.Add("source-event-not-receipt-verified"); }
            var tracked = review.Direction == "incoming" ? to : from;
            var after = balances[tracked];
            if (after < 0) { flags.Add("negative-reconstructed-balance"); }
            output.Add(review with { ReconstructedBalanceBeforeRaw = (review.Direction == "incoming" ? after - value : before).ToString(CultureInfo.InvariantCulture), ReconstructedBalanceAfterRaw = after.ToString(CultureInfo.InvariantCulture), Flags = flags });
        }
        return output;
    }
}
