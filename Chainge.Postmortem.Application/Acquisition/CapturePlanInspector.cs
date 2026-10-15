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
                if (method is not ("eth_chainId" or "eth_getLogs" or "eth_call" or "eth_getBlockByHash" or "eth_getBlockByNumber" or "eth_getTransactionReceipt" or "eth_getTransactionByHash" or "eth_getCode")) { throw new InvalidDataException("Only read-only acquisition methods are supported."); }
            }
        }
    }
}
