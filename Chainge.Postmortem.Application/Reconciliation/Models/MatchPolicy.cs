namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record MatchPolicy
{
    public IReadOnlyList<string> RoutingCallTargets { get; init; } = [];
    public IReadOnlyDictionary<string, string> RoutingChainNames { get; init; } = new Dictionary<string, string>();
    public TimeSpan RoutingReviewMaximumDelay { get; init; }
    public IReadOnlyDictionary<string, string[]> FusionInventoryAccounts { get; init; } = new Dictionary<string, string[]>();
    public IReadOnlyDictionary<string, string[]> IssuanceCallTargets { get; init; } = new Dictionary<string, string[]>();
    public IReadOnlyDictionary<string, string> ContractCallPurposes { get; init; } = new Dictionary<string, string>();
    public TimeSpan DelayedReviewMaximumDelay { get; init; }
    public IReadOnlyDictionary<string, string[]> SliceCounterparts { get; init; } = new Dictionary<string, string[]>();
    public IReadOnlyDictionary<string, VariableFeeRule> DepositFeeRules { get; init; } = new Dictionary<string, VariableFeeRule>();
    public SourceChainRule? SourceChainRule { get; init; }
    public IReadOnlyDictionary<string, VariableFeeRule> VariableFeeRules { get; init; } = new Dictionary<string, VariableFeeRule>();
    public required string LargeOutflowRaw { get; init; }
    public required int DepletionBasisPoints { get; init; }
    public required int Decimals { get; init; }
    public required string FusionChain { get; init; }
    public required IReadOnlyDictionary<string, string[]> OperatingAddresses { get; init; }
    public required TimeSpan MaximumDelay { get; init; }
    public required TimeSpan ClockTolerance { get; init; }
    public required IReadOnlyList<string> RedemptionFeeRawHypotheses { get; init; }
}
