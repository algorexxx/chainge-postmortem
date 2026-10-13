using System.Text.Json;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class BalanceRepositoryTests
{
    [Theory]
    [InlineData(false, "100")]
    [InlineData(true, null)]
    public async Task ReadBalancesAsync_UnorderedBatchWithFailures_ShouldBindByRequestId(bool failed, string? expectedRaw)
    {
        var root = Path.Combine(Path.GetTempPath(), "chainge-balance-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var query = """[{"id":1,"method":"eth_call","params":[{"to":"token","data":"0x70a082310000000000000000000000001111111111111111111111111111111111111111"},"0x2"]},{"id":2,"method":"eth_blockNumber","params":[]}]""";
        var response = failed ? """[{"id":2,"result":"0x9"},{"id":1,"error":{"code":-1}}]""" : """[{"id":2,"result":"0x9"},{"id":1,"result":"0x0000000000000000000000000000000000000000000000000000000000000064"}]""";
        await File.WriteAllTextAsync(Path.Combine(root, "balances.request.json"), query, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(root, "balances.response.json"), response, TestContext.Current.CancellationToken);
        var settings = JsonSerializer.Deserialize<EvidenceSettings>("""{"asset":"fixture","policy":{"largeOutflowRaw":"100","depletionBasisPoints":9000,"decimals":18,"fusionChain":"Fusion","operatingAddresses":{},"maximumDelay":"00:05:00","clockTolerance":"00:02:00","redemptionFeeRawHypotheses":["0"]},"sources":[],"balanceSources":[{"chain":"chain","directory":".","pattern":"*.request.json","tokenDecimals":{}}],"limitations":[]}""", JsonSerializerOptions.Web)!;
        var reader = new BalanceRepository(settings);
        var expected = new[] { new BalanceObservation { Chain = "chain", Token = "token", Holder = "0x1111111111111111111111111111111111111111", Block = 2, RawAmount = expectedRaw, Source = "balances.request.json" } };
        var expectedFiles = new[] { "balances.request.json", "balances.response.json" };

        var result = await reader.ReadBalancesAsync(root, TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, result.Observations, strict: true);
        Assert.Equivalent(expectedFiles, result.InputHashes.Keys.ToArray(), strict: true);
    }
}
