using Chainge.Postmortem.Application.Catalogue.Interfaces;
using Chainge.Postmortem.Application.Catalogue.Models;

namespace Chainge.Postmortem.Application.Tests.Catalogue;

public sealed class FixedCatalogueRepository : ICatalogueRepository
{
    public required CatalogueSource Source { get; init; }

    public Task<CatalogueSource> ReadCatalogueAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Source);
    }
}
