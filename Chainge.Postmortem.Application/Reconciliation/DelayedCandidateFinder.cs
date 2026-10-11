using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class DelayedCandidateFinder
{
    public static IReadOnlyList<MovementReview> FindCandidates(IReadOnlyList<ChainEvent> events, MatchPolicy policy, ReconciliationResult baseline)
    {
        if (policy.DelayedReviewMaximumDelay <= TimeSpan.Zero) { return []; }
        var wider = policy.ExpandReviewWindow();
        var expanded = MovementMatcher.MatchMovements(events, wider);
        var previous = baseline.Movements.ToDictionary(m => m.SourceEventId);
        var all = baseline.Movements.Concat(expanded.Movements).ToList();
        var fusionUsage = all.SelectMany(m => m.Candidates.Select(c => new { m.SourceEventId, c.FusionEventId })).Distinct().GroupBy(e => e.FusionEventId).ToDictionary(g => g.Key, g => g.Count());
        var sourceUsage = all.SelectMany(m => m.SourceCandidates.Select(c => new { m.SourceEventId, c.OtherEventId })).Distinct().GroupBy(e => e.SourceEventId).ToDictionary(g => g.Key, g => g.Count());
        return expanded.Movements.Select(m => m with
        {
            Classification = "delayed-candidate-review",
            Candidates = m.Candidates.Where(c => !previous[m.SourceEventId].Candidates.Any(p => p.FusionEventId == c.FusionEventId)).Select(c => c with { Competing = c.Competing || fusionUsage[c.FusionEventId] > 1 || previous[m.SourceEventId].Candidates.Count > 0 }).ToList(),
            SourceCandidates = m.SourceCandidates.Where(c => !previous[m.SourceEventId].SourceCandidates.Any(p => p.OtherEventId == c.OtherEventId)).Select(c => c with { Competing = c.Competing || sourceUsage[m.SourceEventId] > 1 || sourceUsage[c.OtherEventId] > 1 }).ToList()
        }).Where(m => m.Candidates.Count > 0 || m.SourceCandidates.Count > 0).ToList();
    }

}
