namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record TimestampObservation
{
    public string HeaderSource { get; init; } = string.Empty;
    public required DateTimeOffset? Value { get; init; }
    public required string Source { get; init; }
    public required bool Node { get; init; }
}
