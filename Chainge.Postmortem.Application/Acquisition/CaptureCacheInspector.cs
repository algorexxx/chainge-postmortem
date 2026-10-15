using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public static class CaptureCacheInspector
{
    public static bool CanReuse(CaptureQuery query)
    {
        return CaptureResponseInspector.ReadElements(query.Request).All(IsPinned);
    }

    private static bool IsPinned(JsonElement request)
    {
        var parameters = request.GetProperty("params");
        return request.GetProperty("method").GetString() switch
        {
            "eth_getTransactionReceipt" or "eth_getTransactionByHash" or "eth_getBlockByHash" or "debug_traceTransaction" => parameters.GetArrayLength() > 0 && IsHex(parameters[0]),
            "eth_getBlockByNumber" => parameters.GetArrayLength() > 0 && IsHex(parameters[0]),
            "eth_call" or "eth_getCode" or "eth_getBalance" => parameters.GetArrayLength() > 1 && IsHex(parameters[1]),
            "eth_getLogs" => parameters.GetArrayLength() > 0 && HasPinnedRange(parameters[0]),
            _ => false
        };
    }

    private static bool IsHex(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String && value.GetString() is { Length: > 2 } text && text.StartsWith("0x", StringComparison.Ordinal) && text.Skip(2).All(char.IsAsciiHexDigit);
    }

    private static bool HasPinnedRange(JsonElement filter)
    {
        if (filter.TryGetProperty("blockHash", out var hash)) { return IsHex(hash); }
        return filter.TryGetProperty("fromBlock", out var start) && IsHex(start) && filter.TryGetProperty("toBlock", out var end) && IsHex(end);
    }
}
