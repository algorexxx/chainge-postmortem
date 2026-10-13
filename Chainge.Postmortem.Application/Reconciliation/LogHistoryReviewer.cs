using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class LogHistoryReviewer
{
    public static IReadOnlyList<LogHistoryCoverage> ReviewCoverage(IReadOnlyList<LogHistoryTarget> targets, IReadOnlyList<LogQueryWindow> windows)
    {
        var results = new List<LogHistoryCoverage>();
        foreach (var target in targets)
        {
            ValidateRange(target.Range);
            foreach (var direction in new[] { "incoming", "outgoing" })
            {
                var relevant = windows.Where(w => w.Succeeded && w.Chain == target.Chain && w.Token.Equals(target.Token, StringComparison.OrdinalIgnoreCase) && w.Holder.Equals(target.Holder, StringComparison.OrdinalIgnoreCase) && w.Direction == direction).ToList();
                results.Add(new LogHistoryCoverage { Target = target, Direction = direction, MissingRanges = FindGaps(target.Range, relevant), Sources = relevant.Select(w => w.Source).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList() });
            }
        }
        return results;
    }

    private static void ValidateRange(BlockRange range)
    {
        if (range.From < 0 || range.Through < range.From || range.Through == long.MaxValue) { throw new InvalidDataException("Invalid inclusive block range."); }
    }

    private static List<BlockRange> FindGaps(BlockRange target, List<LogQueryWindow> windows)
    {
        var result = new List<BlockRange>();
        var next = target.From;
        foreach (var window in windows.OrderBy(w => w.Range.From).ThenBy(w => w.Range.Through))
        {
            ValidateRange(window.Range);
            var from = Math.Max(target.From, window.Range.From);
            var through = Math.Min(target.Through, window.Range.Through);
            if (through < next || from > target.Through) { continue; }
            if (from > next) { result.Add(new BlockRange { From = next, Through = from - 1 }); }
            next = through + 1;
        }
        if (next <= target.Through) { result.Add(new BlockRange { From = next, Through = target.Through }); }
        return result;
    }
}
