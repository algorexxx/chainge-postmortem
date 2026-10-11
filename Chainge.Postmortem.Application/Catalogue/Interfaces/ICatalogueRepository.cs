using Chainge.Postmortem.Application.Catalogue.Models;

namespace Chainge.Postmortem.Application.Catalogue.Interfaces;

public interface ICatalogueRepository
{
    Task<CatalogueSource> ReadCatalogueAsync(CancellationToken cancellationToken);
}
