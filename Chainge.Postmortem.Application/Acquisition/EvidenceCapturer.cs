using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public sealed class EvidenceCapturer
{
    private readonly ICaptureClient client;
    private readonly ICaptureRepository repository;
    private readonly TimeProvider clock;

    public EvidenceCapturer(ICaptureClient client, ICaptureRepository repository, TimeProvider clock)
    {
        this.client = client;
        this.repository = repository;
        this.clock = clock;
    }

    public async Task<CaptureSummary> CaptureEvidence(CapturePlan plan, IProgress<CaptureProgress>? progress, CancellationToken cancellationToken)
    {
        CapturePlanInspector.ValidatePlan(plan);
        var identity = new CaptureQuery { Name = "chain-id", Request = JsonSerializer.SerializeToElement(new { jsonrpc = "2.0", id = "chain-id", method = "eth_chainId", @params = Array.Empty<string>() }) };
        var identityVerified = await CaptureQuery(identity, plan.ExpectedChainId, plan, progress, cancellationToken);
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
                var success = await CaptureQuery(query, null, plan, progress, cancellationToken);
                if (success) { captured++; consecutiveFailures = 0; }
                else { failed++; consecutiveFailures++; }
                progress?.Report(new CaptureProgress { Name = query.Name, Status = success ? "captured" : "failed" });
                if (consecutiveFailures >= plan.MaximumConsecutiveFailures) { break; }
            }
        }
        await repository.WriteManifestAsync(cancellationToken);
        return new CaptureSummary { IdentityVerified = identityVerified, Captured = captured, Reused = reused, Failed = failed, Remaining = plan.Queries.Count - captured - reused };
    }

    private async Task<bool> CaptureQuery(CaptureQuery query, string? expectedResult, CapturePlan plan, IProgress<CaptureProgress>? progress, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await client.PostAsync(query, cancellationToken);
            var succeeded = CaptureResponseInspector.IsSuccessful(query, response, expectedResult);
            await repository.SaveAttemptAsync(query, response, succeeded, cancellationToken);
            if (succeeded || attempt >= plan.MaximumRetries) { return succeeded; }
            var delay = CaptureRetryInspector.GetRetryDelay(response, plan.RetryDelay, attempt);
            if (delay is null) { return false; }
            progress?.Report(new CaptureProgress { Name = query.Name, Status = $"retry {attempt + 1}/{plan.MaximumRetries} after {delay.Value}" });
            await Task.Delay(delay.Value, clock, cancellationToken);
        }
    }
}
