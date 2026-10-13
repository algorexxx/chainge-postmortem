namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ClaimInterval
{
    public required ulong Start { get; init; }
    public required ulong End { get; init; }
}
