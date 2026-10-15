using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public static class CaptureResponseInspector
{
    public static bool IsSuccessful(CaptureQuery query, CaptureResponse response, string? expectedResult = null)
    {
        if (response.HttpStatus is < 200 or >= 300 || !response.ValidJson) { return false; }
        using var document = JsonDocument.Parse(response.Body);
        var replies = ReadElements(document.RootElement);
        var requests = ReadElements(query.Request);
        if (replies.Count != requests.Count || replies.Any(r => r.ValueKind != JsonValueKind.Object || !r.TryGetProperty("id", out _))) { return false; }
        if (replies.Select(r => r.GetProperty("id").GetRawText()).Distinct(StringComparer.Ordinal).Count() != replies.Count) { return false; }
        foreach (var request in requests)
        {
            var id = request.GetProperty("id").GetRawText();
            var matching = replies.Where(r => r.GetProperty("id").GetRawText() == id).ToList();
            if (matching.Count != 1) { return false; }
            var reply = matching[0];
            if (reply.TryGetProperty("error", out _) || !reply.TryGetProperty("result", out var result) || result.ValueKind == JsonValueKind.Null) { return false; }
            if (!HasExpectedShape(request.GetProperty("method").GetString(), result)) { return false; }
            if (expectedResult is not null && result.ToString() != expectedResult) { return false; }
        }
        return true;
    }

    private static bool HasExpectedShape(string? method, JsonElement result)
    {
        return method switch
        {
            "eth_getLogs" => result.ValueKind == JsonValueKind.Array,
            "eth_getBlockByHash" or "eth_getBlockByNumber" or "eth_getTransactionReceipt" or "eth_getTransactionByHash" or "debug_traceTransaction" => result.ValueKind == JsonValueKind.Object,
            "eth_chainId" or "eth_call" or "eth_getCode" or "eth_getBalance" => result.ValueKind == JsonValueKind.String && result.GetString()!.StartsWith("0x", StringComparison.Ordinal),
            _ => false
        };
    }

    public static IReadOnlyList<JsonElement> ReadElements(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().ToList() : [element];
    }
}
