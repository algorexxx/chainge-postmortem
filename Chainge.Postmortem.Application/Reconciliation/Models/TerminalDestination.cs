namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record TerminalDestination
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Address { get; init; }
    public required IReadOnlyList<string> SourceAddresses { get; init; }
    public required IReadOnlyList<string> EventIds { get; init; }
}
