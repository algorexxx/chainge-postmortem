using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class CaptureCacheInspectorTests
{
    [Theory]
    [InlineData("eth_getLogs", "[{\"fromBlock\":\"0x0\",\"toBlock\":\"0x10\"}]", true)]
    [InlineData("eth_getLogs", "[{\"fromBlock\":\"0x0\",\"toBlock\":\"latest\"}]", false)]
    [InlineData("eth_getLogs", "[{\"blockHash\":\"0xab\"}]", true)]
    [InlineData("eth_getLogs", "[{}]", false)]
    [InlineData("eth_getLogs", "[]", false)]
    [InlineData("eth_getTransactionReceipt", "[\"0xab\"]", true)]
    [InlineData("debug_traceTransaction", "[\"0xab\",{\"tracer\":\"callTracer\"}]", true)]
    [InlineData("eth_getBalance", "[\"0xab\",\"0x10\"]", true)]
    [InlineData("eth_getBalance", "[\"0xab\",\"latest\"]", false)]
    [InlineData("eth_getBlockByNumber", "[\"0x10\",false]", true)]
    [InlineData("eth_getBlockByNumber", "[\"latest\",false]", false)]
    [InlineData("eth_call", "[{},\"0x10\"]", true)]
    [InlineData("eth_call", "[{},\"pending\"]", false)]
    [InlineData("eth_call", "[{}]", false)]
    [InlineData("eth_call", "[{},10]", false)]
    [InlineData("eth_call", "[{},\"0xzz\"]", false)]
    [InlineData("eth_chainId", "[]", false)]
    public void CanReuse_BlockSelection_ShouldRequirePinnedRequests(string method, string parameters, bool expected)
    {
        var query = CaptureFixture.Query() with { Request = JsonSerializer.SerializeToElement(new { method, @params = JsonSerializer.Deserialize<JsonElement>(parameters) }) };

        var actual = CaptureCacheInspector.CanReuse(query);

        Assert.Equivalent(expected, actual);
    }
}
