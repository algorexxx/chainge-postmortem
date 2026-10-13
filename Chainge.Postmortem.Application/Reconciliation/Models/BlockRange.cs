namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BlockRange
{
    public required long From { get; init; }
    public required long Through { get; init; }
}
