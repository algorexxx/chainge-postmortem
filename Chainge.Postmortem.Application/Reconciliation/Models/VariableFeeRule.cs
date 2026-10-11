namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record VariableFeeRule
{
    public required string MaximumFeeRaw { get; init; }
    public required int MaximumFeeBasisPoints { get; init; }
    public required TimeSpan MaximumDelay { get; init; }
}
