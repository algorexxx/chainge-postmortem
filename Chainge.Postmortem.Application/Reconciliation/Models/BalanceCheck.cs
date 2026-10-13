namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BalanceCheck
{
    public required BalanceObservation Observation { get; init; }
    public required string ReplayedRawAmount { get; init; }
    public required bool NetAgrees { get; init; }
    public required bool HistoryConsistent { get; init; }
    public required string Status { get; init; }
}
