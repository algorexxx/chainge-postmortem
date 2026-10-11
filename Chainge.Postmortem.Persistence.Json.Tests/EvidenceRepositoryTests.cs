using System.Text.Json;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class EvidenceRepositoryTests
{
    [Fact]
    public async Task ReadEvidenceAsync_DuplicateReceipts_ShouldDeduplicateAndPreserveSources()
    {
        var root = await CreateFixture(Receipt("0x1", "0x64"), Receipt("0x1", "0x64"));
        var reader = CreateReader();
        var expected = new { RawAmount = "100", ReceiptVerified = true, Timestamp = DateTimeOffset.FromUnixTimeSeconds(100), Evidence = new[] { "a.json", "b.json" } };

        var result = await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken);
        var e = result.Events.Single();
        var actual = new { e.RawAmount, e.ReceiptVerified, e.Timestamp, e.Evidence };

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task ReadEvidenceAsync_ConflictingAmounts_ShouldReject()
    {
        var root = await CreateFixture(Receipt("0x1", "0x64"), Receipt("0x1", "0x64", 101));
        var reader = CreateReader();

        var actual = await Record.ExceptionAsync(() => reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Fact]
    public async Task ReadEvidenceAsync_ConflictingNodeTimes_ShouldQuarantineTimestamp()
    {
        var root = await CreateFixture(Receipt("0x1", "0x64"), Receipt("0x1", "0x65"));
        var reader = CreateReader();
        DateTimeOffset? expected = null;

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Events.Single().Timestamp;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ReadEvidenceAsync_FailedAndNonJsonResponses_ShouldRetainDiagnostics()
    {
        var root = await CreateFixture(Receipt("0x0", "0x64"), "error: provider unavailable");
        var reader = CreateReader();
        var expected = new[] { "a.json: unsuccessful receipt tx", "b.json: non-JSON failure response excluded" };

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Diagnostics;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task ReadEvidenceAsync_ExplorerTimestampDisagrees_ShouldPreferNode()
    {
        var root = await CreateFixture(Receipt("0x1", "0x64"), "{\"result\":[{\"blockHash\":\"block\",\"timeStamp\":\"101\"}]}");
        var reader = CreateReader();
        var expected = DateTimeOffset.FromUnixTimeSeconds(100);

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Events.Single().Timestamp;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ReadEvidenceAsync_OtherTokenInReceipt_ShouldRetainContextWithoutAddingAssetEvents()
    {
        var root = await CreateFixture(MixedReceipt(7), MixedReceipt(7));
        var reader = CreateReader();
        var expected = new[] { new Chainge.Postmortem.Application.Reconciliation.Models.ReceiptTransfer { Token = "other-token", From = "0x0000000000000000000000000000000000000000", To = "0x1111111111111111111111111111111111111111", RawAmount = "7", LogIndex = 1 } };

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Events.Single().RelatedTransfers;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task ReadEvidenceAsync_ConflictingOtherTokenLogs_ShouldReject()
    {
        var root = await CreateFixture(MixedReceipt(7), MixedReceipt(8));
        var reader = CreateReader();

        var actual = await Record.ExceptionAsync(() => reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Fact]
    public async Task ReadEvidenceAsync_SwapLogs_ShouldRetainAmountsParticipantsAndEmitter()
    {
        var root = await CreateFixture(SwapReceipt(false), SwapReceipt(false));
        var reader = CreateReader();
        var expected = new[] { new Chainge.Postmortem.Application.Reconciliation.Models.ReceiptExchange { Emitter = "router", LogIndex = 2, Sender = "0x1111111111111111111111111111111111111111", SourceToken = "0x2222222222222222222222222222222222222222", DestinationToken = "0x3333333333333333333333333333333333333333", Recipient = "0x4444444444444444444444444444444444444444", SpentRaw = "100", ReturnedRaw = "7" } };

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Events.Single().ReportedExchanges;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task ReadEvidenceAsync_PoolSwap_ShouldDecodeBothSides()
    {
        var root = await CreateFixture(SwapReceipt(true), SwapReceipt(true));
        var reader = CreateReader();
        var expected = new[] { new Chainge.Postmortem.Application.Reconciliation.Models.ReceiptPoolSwap { Pool = "pool", LogIndex = 2, Recipient = "0x4444444444444444444444444444444444444444", Input0Raw = "100", Input1Raw = "0", Output0Raw = "0", Output1Raw = "7" } };

        var actual = (await reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken)).Events.Single().PoolSwaps;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadEvidenceAsync_MalformedSwap_ShouldReject(bool pool)
    {
        var receipt = System.Text.Json.Nodes.JsonNode.Parse(SwapReceipt(pool))!;
        receipt[0]!["result"]!["logs"]![1]!["data"] = "0x00";
        var root = await CreateFixture(receipt.ToJsonString(), Receipt("0x1", "0x64"));
        var reader = CreateReader();

        var actual = await Record.ExceptionAsync(() => reader.ReadEvidenceAsync(root, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }

    private static string SwapReceipt(bool pool)
    {
        var receipt = System.Text.Json.Nodes.JsonNode.Parse(Receipt("0x1", "0x64"))!;
        var logs = receipt[0]!["result"]!["logs"]!.AsArray();
        var addresses = Enumerable.Range(1, 4).Select(i => new string('0', 24) + new string((char)('0' + i), 40)).ToArray();
        int[] amounts = pool ? [100, 0, 0, 7] : [100, 7];
        var encoded = amounts.Select(i => i.ToString("x64", System.Globalization.CultureInfo.InvariantCulture));
        var words = pool ? encoded.ToArray() : addresses.Concat(encoded).ToArray();
        var topics = pool ? new[] { "0xd78ad95fa46c994b6551d0da85fc275fe613ce37657fb8d5e3d130840159d822", "0x" + addresses[0], "0x" + addresses[3] } : new[] { "0xd6d4f5681c246c9f42c203e287975af1601f8df8035a9251f79aab5c8f09e2f8" };
        logs.Add(System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(new { address = pool ? "pool" : "router", topics, data = "0x" + string.Concat(words), logIndex = "0x2", transactionHash = "tx" })));
        return receipt.ToJsonString();
    }

    private static string MixedReceipt(int amount)
    {
        var receipt = System.Text.Json.Nodes.JsonNode.Parse(Receipt("0x1", "0x64"))!;
        var logs = receipt[0]!["result"]!["logs"]!.AsArray();
        var extra = logs[0]!.DeepClone();
        extra["address"] = "other-token";
        extra["data"] = "0x" + amount.ToString("x64", System.Globalization.CultureInfo.InvariantCulture);
        extra["logIndex"] = "0x1";
        logs.Add(extra);
        return receipt.ToJsonString();
    }

    private static EvidenceRepository CreateReader()
    {
        var json = """
        {"asset":"fixture","policy":{"largeOutflowRaw":"10000000000000000000000","depletionBasisPoints":9000,"decimals":18,"fusionChain":"Fusion","operatingAddresses":{},"maximumDelay":"1.00:00:00","clockTolerance":"00:02:00","redemptionFeeRawHypotheses":["0"]},"sources":[{"chain":"BSC","directory":".","pattern":"*.json","tokenDecimals":{"token":18}}],"limitations":[]}
        """;
        return new EvidenceRepository(JsonSerializer.Deserialize<EvidenceSettings>(json, JsonSerializerOptions.Web)!);
    }

    private static async Task<string> CreateFixture(string first, string second)
    {
        var root = Path.Combine(Path.GetTempPath(), "chainge-reconciliation-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "a.json"), first, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(root, "b.json"), second, TestContext.Current.CancellationToken);
        return root;
    }

    private static string Receipt(string status, string timestamp, int amount = 100)
    {
        return JsonSerializer.Serialize(new[] { new { result = new { status, transactionHash = "tx", logs = new[] { new { address = "token", topics = new[] { "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef", "0x" + new string('0', 64), "0x" + new string('1', 64) }, data = "0x" + amount.ToString("x64", System.Globalization.CultureInfo.InvariantCulture), blockHash = "block", blockNumber = "0x1", logIndex = "0x0", transactionHash = "tx", blockTimestamp = timestamp } } } } });
    }
}
