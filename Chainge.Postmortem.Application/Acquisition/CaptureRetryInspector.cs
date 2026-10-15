using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public static class CaptureRetryInspector
{
    public static TimeSpan? GetRetryDelay(CaptureResponse response, TimeSpan minimumDelay, int attempt)
    {
        var transient = response.HttpStatus is 0 or 408 or 429 or 500 or 502 or 503 or 504;
        var requested = response.RetryAfter ?? TimeSpan.Zero;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            foreach (var reply in CaptureResponseInspector.ReadElements(document.RootElement))
            {
                if (reply.ValueKind != JsonValueKind.Object || !reply.TryGetProperty("error", out var error) || error.ValueKind != JsonValueKind.Object) { continue; }
                if (error.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var number) && number == -32029) { transient = true; }
                if (error.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object && data.TryGetProperty("retry_after_ms", out var retry) && retry.ValueKind == JsonValueKind.Number && retry.TryGetDouble(out var milliseconds) && milliseconds >= 0 && milliseconds <= TimeSpan.FromDays(1).TotalMilliseconds)
                {
                    transient = true;
                    requested = TimeSpan.FromMilliseconds(Math.Max(requested.TotalMilliseconds, milliseconds));
                }
            }
        }
        catch (JsonException) { }
        if (!transient) { return null; }
        var backoff = minimumDelay * Math.Pow(2, Math.Min(attempt, 4));
        return requested > backoff ? requested : backoff;
    }
}
