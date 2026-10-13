using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class BalanceReviewer
{
    public static IReadOnlyList<BalanceCheck> ReviewBalances(IReadOnlyList<ChainEvent> events, IReadOnlyList<BalanceObservation> observations, IReadOnlyList<LogQueryWindow> windows)
    {
        var results = new List<BalanceCheck>();
        foreach (var group in observations.GroupBy(o => new { o.Chain, o.Block }))
        {
            var positions = LedgerPositionBuilder.BuildPositions(events, group.Key.Chain, group.Key.Block);
            foreach (var observation in group)
            {
                var position = positions.SingleOrDefault(p => p.Token.Equals(observation.Token, StringComparison.OrdinalIgnoreCase) && p.Holder.Equals(observation.Holder, StringComparison.OrdinalIgnoreCase));
                var raw = position?.RawAmount ?? "0";
                var agrees = observation.RawAmount == raw;
                var consistent = position?.HistoryConsistent ?? true;
                var target = new LogHistoryTarget { Chain = observation.Chain, Token = observation.Token, Holder = observation.Holder, Range = new BlockRange { From = 0, Through = observation.Block } };
                var covered = LogHistoryReviewer.ReviewCoverage([target], windows).All(c => c.MissingRanges.Count == 0);
                results.Add(new BalanceCheck { Observation = observation, ReplayedRawAmount = raw, NetAgrees = agrees, HistoryConsistent = consistent, Status = ClassifyBalance(observation.RawAmount, agrees, consistent, covered) });
            }
        }
        return results;
    }

    private static string ClassifyBalance(string? observed, bool agrees, bool consistent, bool covered)
    {
        if (observed is null) { return "state-unavailable"; }
        if (!agrees) { return "balance-mismatch"; }
        if (!consistent) { return "net-agrees-inconsistent-history"; }
        return covered ? "net-agrees-covered" : "net-agrees-coverage-incomplete";
    }
}
