namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record HolderPosition
{
    public required string Token { get; init; }
    public required string Holder { get; init; }
    public required int Decimals { get; init; }
    public required string RawAmount { get; init; }
    public required string IntervalStatus { get; init; }
    public required bool HistoryConsistent { get; init; }
}
