namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record SourceLedgerSnapshot
{
    public required string Chain { get; init; }
    public required long Block { get; init; }
    public required IReadOnlyList<LogHistoryCoverage> Coverage { get; init; }
    public required IReadOnlyList<HolderPosition> Positions { get; init; }
}
