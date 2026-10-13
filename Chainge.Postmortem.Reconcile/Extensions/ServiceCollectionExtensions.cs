using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;
using Chainge.Postmortem.Rpc.Http;

namespace Chainge.Postmortem.Reconcile.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAcquisition(this IServiceCollection services, CapturePlan plan, string output)
    {
        return services.AddSingleton(plan).AddSingleton(new CaptureStoreSettings { Directory = output, Endpoint = plan.Endpoint }).AddSingleton(TimeProvider.System)
            .AddSingleton(_ => new HttpClient { Timeout = plan.Timeout }).AddTransient<ICaptureClient, CaptureClient>().AddTransient<ICaptureRepository, CaptureRepository>().AddTransient<EvidenceCapturer>();
    }

    public static IServiceCollection AddReconciliation(this IServiceCollection services, EvidenceSettings settings)
    {
        return services.AddSingleton(settings).AddTransient<EvidenceRepository>().AddTransient<ExplorerDiscoveryRepository>().AddTransient<LogQueryRepository>().AddTransient<BalanceRepository>();
    }
}
