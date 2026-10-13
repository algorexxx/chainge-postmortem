using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class BalanceIntervalReviewer
{
    public static IReadOnlyList<BalanceIntervalCheck> ReviewIntervals(IReadOnlyList<ChainEvent> events, IReadOnlyList<BalanceObservation> observations, IReadOnlyList<LogQueryWindow> windows)
    {
        var results = new List<BalanceIntervalCheck>();
        foreach (var group in observations.GroupBy(o => new { o.Chain, Token = o.Token.ToLowerInvariant(), Holder = o.Holder.ToLowerInvariant() }))
        {
            var states = group.GroupBy(o => o.Block).OrderBy(g => g.Key).Select(g => g.ToList()).ToList();
            if (states.Count < 2) { continue; }
            var selected = events.Where(e => e.Chain == group.Key.Chain && e.Token.Equals(group.Key.Token, StringComparison.OrdinalIgnoreCase) && (e.From.Equals(group.Key.Holder, StringComparison.OrdinalIgnoreCase) || e.To.Equals(group.Key.Holder, StringComparison.OrdinalIgnoreCase))).OrderBy(e => e.Block).ThenBy(e => e.LogIndex).ToList();
            for (var i = 1; i < states.Count; i++)
            {
                results.Add(ReviewInterval(selected, states[i - 1], states[i], windows));
            }
        }
        return results;
    }

    private static BalanceIntervalCheck ReviewInterval(List<ChainEvent> events, List<BalanceObservation> opening, List<BalanceObservation> closing, IReadOnlyList<LogQueryWindow> windows)
    {
        var first = opening[0];
        var last = closing[0];
        var amountsBefore = opening.Where(o => o.RawAmount is not null).Select(o => o.RawAmount!).Distinct(StringComparer.Ordinal).ToList();
        var amountsAfter = closing.Where(o => o.RawAmount is not null).Select(o => o.RawAmount!).Distinct(StringComparer.Ordinal).ToList();
        var available = amountsBefore.Count == 1 && amountsAfter.Count == 1;
        var balance = available ? BigInteger.Parse(amountsBefore[0], CultureInfo.InvariantCulture) : BigInteger.Zero;
        var consistent = balance >= 0;
        var change = BigInteger.Zero;
        foreach (var e in events.Where(e => e.Block > first.Block && e.Block <= last.Block))
        {
            var amount = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
            var delta = (e.To.Equals(first.Holder, StringComparison.OrdinalIgnoreCase) ? amount : BigInteger.Zero) - (e.From.Equals(first.Holder, StringComparison.OrdinalIgnoreCase) ? amount : BigInteger.Zero);
            change += delta;
            balance += delta;
            consistent &= balance >= 0;
        }
        var target = new LogHistoryTarget { Chain = first.Chain, Token = first.Token, Holder = first.Holder, Range = new BlockRange { From = first.Block + 1, Through = last.Block } };
        var covered = LogHistoryReviewer.ReviewCoverage([target], windows).All(c => c.MissingRanges.Count == 0);
        var observed = available ? BigInteger.Parse(amountsAfter[0], CultureInfo.InvariantCulture) - BigInteger.Parse(amountsBefore[0], CultureInfo.InvariantCulture) : (BigInteger?)null;
        return new BalanceIntervalCheck { Opening = opening, Closing = closing, ReplayedChangeRaw = change.ToString(CultureInfo.InvariantCulture), ObservedChangeRaw = observed?.ToString(CultureInfo.InvariantCulture), HistoryCovered = covered, HistoryConsistent = available ? consistent : null, Status = ClassifyInterval(amountsBefore.Count, amountsAfter.Count, observed == change, consistent, covered) };
    }

    private static string ClassifyInterval(int openingCount, int closingCount, bool agrees, bool consistent, bool covered)
    {
        if (openingCount > 1 || closingCount > 1) { return "conflicting-state"; }
        if (openingCount == 0 || closingCount == 0) { return "state-unavailable"; }
        if (!agrees) { return "balance-change-mismatch"; }
        if (!consistent) { return "net-change-agrees-inconsistent-history"; }
        return covered ? "net-change-agrees-covered" : "net-change-agrees-coverage-incomplete";
    }
}
