using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class CapturePlanInspectorTests
{
    [Theory]
    [InlineData("http://example.invalid", "query", "eth_call", 2)]
    [InlineData("https://user:password@example.invalid", "query", "eth_call", 2)]
    [InlineData("https://example.invalid?key=value", "query", "eth_call", 2)]
    [InlineData("https://example.invalid", "../escape", "eth_call", 2)]
    [InlineData("https://example.invalid", "chain-id", "eth_call", 2)]
    [InlineData("https://example.invalid", "query", "eth_sendTransaction", 2)]
    [InlineData("https://example.invalid", "query", "eth_call", 0)]
    public void ValidatePlan_UnsafeOrInvalidSettings_ShouldReject(string endpoint, string name, string method, int failures)
    {
        var query = CaptureFixture.Query(name) with { Request = JsonSerializer.SerializeToElement(new { id = name, method, @params = Array.Empty<string>() }) };
        var plan = CaptureFixture.Plan(query) with { Endpoint = new Uri(endpoint), MaximumConsecutiveFailures = failures };

        var actual = Record.Exception(() => CapturePlanInspector.ValidatePlan(plan));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidatePlan_DuplicateNamesOrBatchIds_ShouldReject(bool batch)
    {
        var query = CaptureFixture.Query();
        var plan = batch ? CaptureFixture.Plan(query with { Request = JsonSerializer.SerializeToElement(new[] { query.Request, query.Request }) }) : CaptureFixture.Plan(query, query);

        var actual = Record.Exception(() => CapturePlanInspector.ValidatePlan(plan));

        Assert.IsType<InvalidDataException>(actual);
    }
}
