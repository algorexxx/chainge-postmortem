namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record LogHistoryTarget
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Holder { get; init; }
    public required BlockRange Range { get; init; }
}
