using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class SourcePositionBuilder
{
    public static IReadOnlyList<SourceLedgerSnapshot> BuildSnapshots(IReadOnlyList<ChainEvent> events, IReadOnlyList<LogHistoryTarget> targets, IReadOnlyList<LogQueryWindow> windows, IReadOnlyDictionary<string, long> cutoffs)
    {
        var results = new List<SourceLedgerSnapshot>();
        foreach (var cutoff in cutoffs.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var selected = targets.Where(t => t.Chain == cutoff.Key).Select(t => t with { Range = new BlockRange { From = 0, Through = cutoff.Value } }).ToList();
            var positions = LedgerPositionBuilder.BuildPositions(events, cutoff.Key, cutoff.Value).Where(p => selected.Any(t => t.Token.Equals(p.Token, StringComparison.OrdinalIgnoreCase) && t.Holder.Equals(p.Holder, StringComparison.OrdinalIgnoreCase))).ToList();
            results.Add(new SourceLedgerSnapshot { Chain = cutoff.Key, Block = cutoff.Value, Positions = positions, Coverage = LogHistoryReviewer.ReviewCoverage(selected, windows) });
        }
        return results;
    }
}
