using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public static class CapturePlanInspector
{
    public static void ValidatePlan(CapturePlan plan)
    {
        if (plan.MaximumRetries < 0 || plan.RetryDelay < TimeSpan.Zero) { throw new InvalidDataException("Invalid retry settings."); }
        if (plan.Endpoint.Scheme != "https" || plan.Endpoint.UserInfo.Length != 0 || plan.Endpoint.Query.Length != 0) { throw new InvalidDataException("Capture requires an anonymous HTTPS endpoint without credentials or query parameters."); }
        if (plan.MaximumConsecutiveFailures < 1 || plan.Timeout <= TimeSpan.Zero || plan.RequestDelay < TimeSpan.Zero || string.IsNullOrWhiteSpace(plan.UserAgent) || !plan.ExpectedChainId.StartsWith("0x", StringComparison.Ordinal)) { throw new InvalidDataException("Invalid capture limits or identity settings."); }
        if (plan.Queries.Select(q => q.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != plan.Queries.Count) { throw new InvalidDataException("Duplicate capture names."); }
        foreach (var query in plan.Queries)
        {
            if (query.Name.Length is 0 or > 100 || query.Name == "chain-id" || query.Name.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-')) { throw new InvalidDataException("Capture names must be unique safe file prefixes; chain-id is reserved."); }
            var requests = CaptureResponseInspector.ReadElements(query.Request);
            if (requests.Count == 0 || requests.Select(r => r.GetProperty("id").GetRawText()).Distinct(StringComparer.Ordinal).Count() != requests.Count) { throw new InvalidDataException("Empty request batch or duplicate IDs."); }
            foreach (var request in requests)
            {
                var method = request.GetProperty("method").GetString();
                if (method is not ("eth_chainId" or "eth_getLogs" or "eth_call" or "eth_getBlockByHash" or "eth_getBlockByNumber" or "eth_getTransactionReceipt" or "eth_getTransactionByHash" or "eth_getCode" or "eth_getBalance" or "debug_traceTransaction")) { throw new InvalidDataException("Only read-only acquisition methods are supported."); }
                if (method == "debug_traceTransaction") { ValidateCallTrace(request); }
            }
        }
    }

    private static void ValidateCallTrace(System.Text.Json.JsonElement request)
    {
        if (!request.TryGetProperty("params", out var parameters) || parameters.ValueKind != System.Text.Json.JsonValueKind.Array || parameters.GetArrayLength() != 2) { throw new InvalidDataException("A call trace requires a transaction hash and callTracer options."); }
        var hash = parameters[0].ValueKind == System.Text.Json.JsonValueKind.String ? parameters[0].GetString() : null;
        if (hash is null || hash.Length != 66 || !hash.StartsWith("0x", StringComparison.Ordinal) || !hash.Skip(2).All(char.IsAsciiHexDigit)) { throw new InvalidDataException("A call trace requires an exact transaction hash."); }
        var options = parameters[1];
        if (options.ValueKind != System.Text.Json.JsonValueKind.Object || options.EnumerateObject().Count() != 1 || !options.TryGetProperty("tracer", out var tracer) || tracer.ValueKind != System.Text.Json.JsonValueKind.String || tracer.GetString() != "callTracer") { throw new InvalidDataException("Only the built-in callTracer without custom options is supported."); }
    }
}
