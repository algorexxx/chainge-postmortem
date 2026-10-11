using Chainge.Postmortem.Application.Extensions;
using Chainge.Postmortem.Persistence.Json.Extensions;
using Chainge.Postmortem.Persistence.Json.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chainge.Postmortem.Site.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResearchSite(this IServiceCollection services, CatalogueSettings settings)
    {
        return services.AddResearchCatalogue().AddJsonCatalogue(settings);
    }
}
