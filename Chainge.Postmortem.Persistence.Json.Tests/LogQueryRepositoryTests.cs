using System.Text.Json;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class LogQueryRepositoryTests
{
    private const string Token = "0x1111111111111111111111111111111111111111";
    private const string Holder = "0x2222222222222222222222222222222222222222";
    private const string HolderTopic = "0x0000000000000000000000002222222222222222222222222222222222222222";
    private const string Transfer = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";

    [Theory]
    [InlineData("{\"id\":1,\"result\":[]}", true)]
    [InlineData("{\"id\":1,\"error\":{\"code\":-1}}", false)]
    [InlineData(null, false)]
    public async Task ReadQueriesAsync_EmptyOrFailedResponse_ShouldDistinguishCoverage(string? response, bool succeeded)
    {
        var root = await CreateFixture(Request("0xa"), response);
        var expected = new { Chain = "chain", Token, Holder, Direction = "incoming", From = 0L, Through = 10L, Succeeded = succeeded };

        var result = await CreateReader().ReadQueriesAsync(root, TestContext.Current.CancellationToken);
        var actual = result.Windows.Select(w => new { w.Chain, w.Token, w.Holder, w.Direction, w.Range.From, w.Range.Through, w.Succeeded }).Single();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task ReadQueriesAsync_Latest_ShouldNotCreditUnpinnedRange()
    {
        var root = await CreateFixture(Request("latest"), "{\"id\":1,\"result\":[]}");
        var expected = new[] { "capture.request.json: unsupported or unpinned query; no coverage credit" };

        var result = await CreateReader().ReadQueriesAsync(root, TestContext.Current.CancellationToken);

        Assert.Empty(result.Windows);
        Assert.Equivalent(expected, result.Diagnostics, strict: true);
    }

    [Theory]
    [InlineData(2, "0x1", false)]
    [InlineData(1, "0xb", false)]
    [InlineData(1, "0x1", true)]
    public async Task ReadQueriesAsync_MismatchedResponse_ShouldReject(int id, string block, bool removed)
    {
        var response = JsonSerializer.Serialize(new { id, result = new[] { new { address = Token, topics = new[] { Transfer, HolderTopic, HolderTopic }, blockNumber = block, removed } } });
        var root = await CreateFixture(Request("0xa"), response);

        var actual = await Record.ExceptionAsync(() => CreateReader().ReadQueriesAsync(root, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Fact]
    public async Task ReadQueriesAsync_ConstrainedCounterparty_ShouldNotCreditFullAddressHistory()
    {
        var request = Request("0xa").Replace("null", "\"" + HolderTopic + "\"", StringComparison.Ordinal);
        var root = await CreateFixture(request, "{\"id\":1,\"result\":[]}");

        var actual = await CreateReader().ReadQueriesAsync(root, TestContext.Current.CancellationToken);

        Assert.Empty(actual.Windows);
    }

    private static string Request(string through)
    {
        return JsonSerializer.Serialize(new { id = 1, method = "eth_getLogs", @params = new[] { new { address = new[] { Token }, fromBlock = "0x0", toBlock = through, topics = new object?[] { Transfer, null, new[] { HolderTopic } } } } });
    }

    private static LogQueryRepository CreateReader()
    {
        var json = """
        {"asset":"fixture","policy":{"largeOutflowRaw":"100","depletionBasisPoints":9000,"decimals":6,"fusionChain":"Fusion","operatingAddresses":{},"maximumDelay":"00:05:00","clockTolerance":"00:02:00","redemptionFeeRawHypotheses":["0"]},"sources":[],"limitations":[],"logQuerySources":[{"chain":"chain","directory":".","pattern":"*.request.json","tokenDecimals":{}}]}
        """;
        return new LogQueryRepository(JsonSerializer.Deserialize<EvidenceSettings>(json, JsonSerializerOptions.Web)!);
    }

    private static async Task<string> CreateFixture(string request, string? response)
    {
        var root = Path.Combine(Path.GetTempPath(), "chainge-log-query-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "capture.request.json"), request, TestContext.Current.CancellationToken);
        if (response is not null) { await File.WriteAllTextAsync(Path.Combine(root, "capture.response.json"), response, TestContext.Current.CancellationToken); }
        return root;
    }
}
