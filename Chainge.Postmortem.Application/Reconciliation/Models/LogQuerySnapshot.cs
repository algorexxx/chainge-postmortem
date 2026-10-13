namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record LogQuerySnapshot
{
    public required IReadOnlyList<LogQueryWindow> Windows { get; init; }
    public required IReadOnlyDictionary<string, string> InputHashes { get; init; }
    public required IReadOnlyList<string> Diagnostics { get; init; }
}
