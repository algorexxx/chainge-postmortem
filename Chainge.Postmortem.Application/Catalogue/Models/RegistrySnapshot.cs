using System.Text.Json;

namespace Chainge.Postmortem.Application.Catalogue.Models;

public sealed record RegistrySnapshot
{
    public required IReadOnlyList<TokenSummary> Tokens { get; init; }
    public required IReadOnlyDictionary<string, JsonElement> Studies { get; init; }
    public JsonElement? Timeline { get; init; }
}
