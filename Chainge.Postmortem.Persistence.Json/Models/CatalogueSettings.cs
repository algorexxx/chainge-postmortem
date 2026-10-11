namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record CatalogueSettings
{
    public required string CataloguePath { get; init; }
    public required string StudiesPath { get; init; }
}
