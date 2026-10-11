using Chainge.Postmortem.Application.Catalogue;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Chainge.Postmortem.Application.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResearchCatalogue(this IServiceCollection services)
    {
        return services.AddTransient<RegistryBuilder>();
    }
}
