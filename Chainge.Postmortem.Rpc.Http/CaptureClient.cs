using System.Text;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;
using Sentry;

namespace Chainge.Postmortem.Rpc.Http;

public sealed class CaptureClient : ICaptureClient
{
    private readonly HttpClient client;
    private readonly CapturePlan plan;
    private readonly TimeProvider clock;

    public CaptureClient(HttpClient client, CapturePlan plan, TimeProvider clock)
    {
        this.client = client;
        this.plan = plan;
        this.clock = clock;
    }

    public async Task<CaptureResponse> PostAsync(CaptureQuery query, CancellationToken cancellationToken)
    {
        await Task.Delay(plan.RequestDelay, clock, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, plan.Endpoint) { Content = new StringContent(query.Request.GetRawText(), Encoding.UTF8, "application/json") };
        request.Headers.UserAgent.ParseAdd(plan.UserAgent);
        var body = Array.Empty<byte>();
        var status = 0;
        TimeSpan? retryAfter = null;
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            status = (int)response.StatusCode;
            retryAfter = response.Headers.RetryAfter?.Delta;
            if (response.Headers.RetryAfter?.Date is { } date) { retryAfter = date - clock.GetUtcNow(); }
            body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            return new CaptureResponse { HttpStatus = status, ValidJson = true, Body = body, RetryAfter = retryAfter };
        }
        catch (Exception exception) when (exception is JsonException or HttpRequestException || exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            SentrySdk.CaptureException(exception);
            if (body.Length == 0) { body = JsonSerializer.SerializeToUtf8Bytes(new { transportError = exception.GetType().Name }); }
            return new CaptureResponse { HttpStatus = status, ValidJson = false, Body = body, RetryAfter = retryAfter };
        }
    }
}
