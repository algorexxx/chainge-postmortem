namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BalanceObservation
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Holder { get; init; }
    public required long Block { get; init; }
    public required string? RawAmount { get; init; }
    public required string Source { get; init; }
}
