namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record EvidenceSelection
{
    public string Format { get; init; } = "auto";
    public IReadOnlyList<string> DatedTokens { get; init; } = [];
    public required IReadOnlyDictionary<string, int> TokenDecimals { get; init; }
    public required string Chain { get; init; }
    public required string Directory { get; init; }
    public required string Pattern { get; init; }
}
