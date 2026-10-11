using Chainge.Postmortem.Application.Catalogue.Interfaces;
using Chainge.Postmortem.Application.Catalogue.Models;

namespace Chainge.Postmortem.Application.Catalogue;

public sealed class RegistryBuilder
{
    private readonly ICatalogueRepository repository;

    public RegistryBuilder(ICatalogueRepository repository)
    {
        this.repository = repository;
    }

    public async Task<RegistrySnapshot> BuildRegistry(CancellationToken cancellationToken)
    {
        var source = await repository.ReadCatalogueAsync(cancellationToken);
        var studies = source.Studies.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);
        var tokens = source.Tokens.GroupBy(token => token.Address.ToLowerInvariant())
            .Select(group => new TokenSummary
            {
                Address = group.Key,
                Symbol = string.Join(" / ", group.Select(token => token.Symbol).Distinct(StringComparer.Ordinal)),
                SourceRows = group.Select(token => token.Row).Order().ToList(),
                HasStudy = studies.ContainsKey(group.Key)
            })
            .OrderByDescending(token => token.HasStudy)
            .ThenBy(token => token.Symbol, StringComparer.OrdinalIgnoreCase)
            .ThenBy(token => token.Address, StringComparer.Ordinal)
            .ToList();
        if (studies.Keys.Except(tokens.Select(token => token.Address), StringComparer.Ordinal).Any())
        {
            throw new InvalidDataException("A case study references a contract absent from the catalogue.");
        }

        return new RegistrySnapshot { Tokens = tokens, Studies = studies };
    }
}
