namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record LogQueryWindow
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Holder { get; init; }
    public required string Direction { get; init; }
    public required BlockRange Range { get; init; }
    public required bool Succeeded { get; init; }
    public required string Source { get; init; }
}
