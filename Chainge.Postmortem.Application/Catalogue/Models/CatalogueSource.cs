using System.Text.Json;

namespace Chainge.Postmortem.Application.Catalogue.Models;

public sealed record CatalogueSource
{
    public required IReadOnlyList<TokenListing> Tokens { get; init; }
    public required IReadOnlyDictionary<string, JsonElement> Studies { get; init; }
    public JsonElement? Timeline { get; init; }
}
