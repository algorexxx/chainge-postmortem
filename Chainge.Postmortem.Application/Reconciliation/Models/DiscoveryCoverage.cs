namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record DiscoveryCoverage
{
    public required string Source { get; init; }
    public required string Chain { get; init; }
    public required string Status { get; init; }
    public required int ReturnedRows { get; init; }
}
