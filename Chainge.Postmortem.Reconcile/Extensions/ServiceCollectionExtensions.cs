using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Chainge.Postmortem.Reconcile.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services, EvidenceSettings settings)
    {
        return services.AddSingleton(settings).AddTransient<EvidenceRepository>();
    }
}
