namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record TerminalRecipientReuse
{
    public required string Address { get; init; }
    public required IReadOnlyList<string> EventIds { get; init; }
}
