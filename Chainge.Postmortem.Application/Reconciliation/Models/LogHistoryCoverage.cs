namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record LogHistoryCoverage
{
    public required LogHistoryTarget Target { get; init; }
    public required string Direction { get; init; }
    public required IReadOnlyList<BlockRange> MissingRanges { get; init; }
    public required IReadOnlyList<string> Sources { get; init; }
}
