namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BalanceIntervalCheck
{
    public required IReadOnlyList<BalanceObservation> Opening { get; init; }
    public required IReadOnlyList<BalanceObservation> Closing { get; init; }
    public required string ReplayedChangeRaw { get; init; }
    public required string? ObservedChangeRaw { get; init; }
    public required bool HistoryCovered { get; init; }
    public required bool? HistoryConsistent { get; init; }
    public required string Status { get; init; }
}
