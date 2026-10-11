using Chainge.Postmortem.Application.Catalogue.Interfaces;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Chainge.Postmortem.Persistence.Json.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonCatalogue(this IServiceCollection services, CatalogueSettings settings)
    {
        services.AddSingleton(settings);
        return services.AddTransient<ICatalogueRepository, CatalogueRepository>();
    }
}
