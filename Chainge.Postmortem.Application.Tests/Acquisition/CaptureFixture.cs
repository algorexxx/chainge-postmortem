using System.Text;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public static class CaptureFixture
{
    public static CaptureQuery Query(string name = "query")
    {
        return new CaptureQuery { Name = name, Request = JsonSerializer.SerializeToElement(new { jsonrpc = "2.0", id = name, method = "eth_getLogs", @params = Array.Empty<string>() }) };
    }

    public static CapturePlan Plan(params CaptureQuery[] queries)
    {
        return new CapturePlan { Endpoint = new Uri("https://example.invalid"), ExpectedChainId = "0x1", UserAgent = "test/1.0", RequestDelay = TimeSpan.Zero, Timeout = TimeSpan.FromSeconds(1), MaximumConsecutiveFailures = 2, Queries = queries };
    }

    public static CaptureResponse Response(string body, int status = 200, bool validJson = true)
    {
        return new CaptureResponse { HttpStatus = status, ValidJson = validJson, Body = Encoding.UTF8.GetBytes(body) };
    }
}
