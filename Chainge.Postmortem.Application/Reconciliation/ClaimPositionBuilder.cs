using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class ClaimPositionBuilder
{
    public static IReadOnlyList<HolderPosition> BuildPositions(IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        return LedgerPositionBuilder.BuildPositions(events, policy.FusionChain, policy.ClaimCutoffBlock).Select(p => p with { IntervalStatus = ClassifyInterval(p.Token, policy) }).ToList();
    }

    private static string ClassifyInterval(string token, MatchPolicy policy)
    {
        if (!policy.ClaimIntervals.TryGetValue(token, out var interval)) { return "unknown-interval"; }
        if (interval.Start >= interval.End) { throw new InvalidDataException("Invalid claim interval."); }
        if (interval.End <= policy.ClaimCutoffUnixTime) { return "expired-interval"; }
        if (interval.Start > policy.ClaimCutoffUnixTime) { return "future-interval"; }
        return interval.End == ulong.MaxValue ? "remaining-perpetual-interval" : "active-finite-interval";
    }
}
