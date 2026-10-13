namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record TokenEventTotals
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required int Decimals { get; init; }
    public required bool FullTime { get; init; }
    public required int EventCount { get; init; }
    public required int UnverifiedCount { get; init; }
    public required int MissingTimestampCount { get; init; }
    public required string MintedRaw { get; init; }
    public required string BurnedRaw { get; init; }
    public required string ObservedNetIssuanceRaw { get; init; }
}
