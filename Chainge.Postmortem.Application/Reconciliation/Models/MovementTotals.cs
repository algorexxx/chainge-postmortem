namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record MovementTotals
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required int Decimals { get; init; }
    public required bool FullTime { get; init; }
    public required string Direction { get; init; }
    public required string Classification { get; init; }
    public required int Count { get; init; }
    public required string RawAmount { get; init; }
}
