namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record SourceChainRule
{
    public required TimeSpan MaximumDelay { get; init; }
    public required string MaximumFeeRaw { get; init; }
    public required int MaximumFeeBasisPoints { get; init; }
}
