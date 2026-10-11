namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record DestinationSummary
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Address { get; init; }
    public required int Decimals { get; init; }
    public required int Count { get; init; }
    public required string GrossOutflowRaw { get; init; }
    public required int UnmatchedCount { get; init; }
    public required string UnmatchedOutflowRaw { get; init; }
}
