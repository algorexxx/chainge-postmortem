using System.Text.Json;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class ExplorerDiscoveryRepositoryTests
{
    [Fact]
    public async Task ReadDiscoveryAsync_ExplorerRows_ShouldStayUnverifiedOutsideLedger()
    {
        var root = Path.Combine(Path.GetTempPath(), "chainge-discovery-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "capture.json"), """{"status":"1","result":[{"contractAddress":"TOKEN","hash":"TX","blockHash":"BLOCK","blockNumber":"1","from":"PAYER","to":"HOLDER","value":"100","timeStamp":"5"}]}""", TestContext.Current.CancellationToken);
        var settings = CreateSettings();
        var expected = new { Id = "explorer:BSC:capture.json:1", From = "payer", To = "holder", ReceiptVerified = false, LogIndex = -1L, RawAmount = "100", Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(5) };

        var result = await new ExplorerDiscoveryRepository(settings).ReadDiscoveryAsync(root, TestContext.Current.CancellationToken);
        var actual = result.Observations.Select(e => new { e.Id, e.From, e.To, e.ReceiptVerified, e.LogIndex, e.RawAmount, e.Timestamp }).Single();

        Assert.Equivalent(expected, actual, strict: true);
    }
    [Theory]
    [InlineData("0", "null", "failed-discovery-response")]
    [InlineData("1", "[]", "returned-rows-not-completeness")]
    public async Task ReadDiscoveryAsync_EmptyAndFailedResults_ShouldPreserveCoverageDifference(string status, string rows, string expectedStatus)
    {
        var root = Path.Combine(Path.GetTempPath(), "chainge-discovery-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "capture.json"), "{\"status\":\"" + status + "\",\"result\":" + rows + "}", TestContext.Current.CancellationToken);
        var expected = new Chainge.Postmortem.Application.Reconciliation.Models.DiscoveryCoverage { Source = "capture.json", Chain = "BSC", Status = expectedStatus, ReturnedRows = 0 };

        var actual = await new ExplorerDiscoveryRepository(CreateSettings()).ReadDiscoveryAsync(root, TestContext.Current.CancellationToken);

        Assert.Equivalent(new[] { expected }, actual.Coverage, strict: true);
        Assert.Empty(actual.Observations);
    }

    private static EvidenceSettings CreateSettings()
    {
        return JsonSerializer.Deserialize<EvidenceSettings>("""{"asset":"fixture","policy":{"largeOutflowRaw":"1","depletionBasisPoints":9000,"decimals":18,"fusionChain":"Fusion","operatingAddresses":{},"maximumDelay":"1.00:00:00","clockTolerance":"00:00:00","redemptionFeeRawHypotheses":["0"]},"sources":[],"discoverySources":[{"chain":"BSC","directory":".","pattern":"*.json","tokenDecimals":{"token":18}}],"limitations":[]}""", JsonSerializerOptions.Web)!;
    }
}
