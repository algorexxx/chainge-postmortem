using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public sealed class EvidenceCapturer
{
    private readonly ICaptureClient client;
    private readonly ICaptureRepository repository;

    public EvidenceCapturer(ICaptureClient client, ICaptureRepository repository)
    {
        this.client = client;
        this.repository = repository;
    }

    public async Task<CaptureSummary> CaptureEvidence(CapturePlan plan, IProgress<CaptureProgress>? progress, CancellationToken cancellationToken)
    {
        CapturePlanInspector.ValidatePlan(plan);
        var identity = new CaptureQuery { Name = "chain-id", Request = JsonSerializer.SerializeToElement(new { jsonrpc = "2.0", id = "chain-id", method = "eth_chainId", @params = Array.Empty<string>() }) };
        var identityVerified = await CaptureQuery(identity, plan.ExpectedChainId, cancellationToken);
        var captured = 0;
        var reused = 0;
        var failed = 0;
        var consecutiveFailures = 0;
        if (identityVerified)
        {
            foreach (var query in plan.Queries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await repository.HasSuccessfulCaptureAsync(query, cancellationToken)) { reused++; progress?.Report(new CaptureProgress { Name = query.Name, Status = "reused" }); continue; }
                var success = await CaptureQuery(query, null, cancellationToken);
                if (success) { captured++; consecutiveFailures = 0; }
                else { failed++; consecutiveFailures++; }
                progress?.Report(new CaptureProgress { Name = query.Name, Status = success ? "captured" : "failed" });
                if (consecutiveFailures >= plan.MaximumConsecutiveFailures) { break; }
            }
        }
        await repository.WriteManifestAsync(cancellationToken);
        return new CaptureSummary { IdentityVerified = identityVerified, Captured = captured, Reused = reused, Failed = failed, Remaining = plan.Queries.Count - captured - reused };
    }

    private async Task<bool> CaptureQuery(CaptureQuery query, string? expectedResult, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync(query, cancellationToken);
        var succeeded = CaptureResponseInspector.IsSuccessful(query, response, expectedResult);
        await repository.SaveAttemptAsync(query, response, succeeded, cancellationToken);
        return succeeded;
    }
}
