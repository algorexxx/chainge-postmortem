namespace Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

public static class MatchPolicyExtensions
{
    public static MatchPolicy ExpandReviewWindow(this MatchPolicy policy)
    {
        return policy with
        {
            VariableFeeRules = policy.VariableFeeRules.ToDictionary(p => p.Key, p => p.Value with { MaximumDelay = MaxDelay(p.Value.MaximumDelay, policy.DelayedReviewMaximumDelay) }),
            DepositFeeRules = policy.DepositFeeRules.ToDictionary(p => p.Key, p => p.Value with { MaximumDelay = MaxDelay(p.Value.MaximumDelay, policy.DelayedReviewMaximumDelay) }),
            SourceChainRule = policy.SourceChainRule is null ? null : policy.SourceChainRule with { MaximumDelay = MaxDelay(policy.SourceChainRule.MaximumDelay, policy.DelayedReviewMaximumDelay) }
        };
    }

    private static TimeSpan MaxDelay(TimeSpan current, TimeSpan proposed)
    {
        return current > proposed ? current : proposed;
    }
}
