using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Acquisition.Models;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class CapturePlanBuilderTests
{
    [Fact]
    public void BuildPlans_ReceiptAndExplorerTimeWithoutInputOrHeader_ShouldRequestMissingContext()
    {
        var e = new ChainEvent { Id = "one", Chain = "chain", Token = "token", Transaction = "tx1", BlockHash = "block1", Block = 1, LogIndex = 0, From = "sender", To = "holder", RawAmount = "100", Decimals = 6, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "explorer.json", ReceiptVerified = true, FullTime = true, Evidence = [] };
        var policy = new CapturePolicy { Template = CaptureFixture.Plan(), MaximumLogBlocks = 10, BatchSize = 1 };
        var expected = new[] { new { Method = "eth_getBlockByHash", Identifier = "block1" }, new { Method = "eth_getTransactionByHash", Identifier = "tx1" } };

        var result = CapturePlanBuilder.BuildPlans([e, e with { Id = "duplicate" }], [], new Dictionary<string, CapturePolicy> { ["chain"] = policy });
        var actual = result["chain"].Queries.Select(q => new { Method = q.Request.GetProperty("method").GetString(), Identifier = q.Request.GetProperty("params")[0].GetString() }).ToArray();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void BuildPlans_CompleteContextWithEmptyCalldata_ShouldRequestNothing()
    {
        var e = new ChainEvent { Id = "one", Chain = "chain", Token = "token", Transaction = "tx1", BlockHash = "block1", Block = 1, LogIndex = 0, From = "sender", To = "holder", RawAmount = "100", Decimals = 6, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "header.json", BlockHeaderSource = "header.json", TransactionInput = "0x", ReceiptVerified = true, FullTime = true, Evidence = [] };
        var policy = new CapturePolicy { Template = CaptureFixture.Plan(), MaximumLogBlocks = 10, BatchSize = 1 };
        CaptureQuery[] expected = [];

        var result = CapturePlanBuilder.BuildPlans([e], [], new Dictionary<string, CapturePolicy> { ["chain"] = policy });
        var actual = result["chain"].Queries;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void BuildPlans_SharedGapsAndMissingContext_ShouldSplitInclusiveWindowsAndBatchUniqueHashes()
    {
        var first = new LogHistoryTarget { Chain = "chain", Token = "token", Holder = "0x1111111111111111111111111111111111111111", Range = new BlockRange { From = 0, Through = 4 } };
        var incoming = new LogHistoryCoverage { Target = first, Direction = "incoming", MissingRanges = [new BlockRange { From = 0, Through = 4 }], Sources = [] };
        var second = incoming with { Target = first with { Holder = "0x2222222222222222222222222222222222222222" } };
        var outgoing = incoming with { Direction = "outgoing", MissingRanges = [new BlockRange { From = 4, Through = 4 }] };
        var e = new ChainEvent { Id = "one", Chain = "chain", Token = "token", Transaction = "tx1", BlockHash = "block1", Block = 1, LogIndex = 0, From = "sender", To = "holder", RawAmount = "100", Decimals = 6, TimestampSource = "missing", ReceiptVerified = false, FullTime = true, Evidence = [] };
        var policy = new CapturePolicy { Template = CaptureFixture.Plan(), MaximumLogBlocks = 3, BatchSize = 2, CombineHolderFilters = true };
        var expectedNames = new[] { "log-batch-000000", "log-batch-000001", "receipts-000000", "receipts-000001", "headers-000000", "transactions-000000", "transactions-000001" };
        var expectedBounds = new[] { "0x0:0x2", "0x3:0x4", "0x4:0x4" };
        var expectedBatch = new[] { "tx1", "tx2" };
        var expectedHeader = "block1";

        var result = CapturePlanBuilder.BuildPlans([e, e with { Id = "duplicate" }, e with { Id = "two", Transaction = "tx2", Timestamp = DateTimeOffset.UnixEpoch }, e with { Id = "three", Transaction = "tx3", Timestamp = DateTimeOffset.UnixEpoch }], [incoming, second, outgoing], new Dictionary<string, CapturePolicy> { ["chain"] = policy });
        var queries = result["chain"].Queries;
        var names = queries.Select(q => q.Name).ToArray();
        var bounds = queries.Take(2).SelectMany(q => CaptureResponseInspector.ReadElements(q.Request)).Select(q => q.GetProperty("params")[0]).Select(f => f.GetProperty("fromBlock").GetString() + ":" + f.GetProperty("toBlock").GetString()).ToArray();
        var batch = queries[2].Request.EnumerateArray().Select(r => r.GetProperty("params")[0].GetString()).ToArray();
        var header = queries[4].Request.GetProperty("params")[0].GetString();

        Assert.Equivalent(expectedNames, names, strict: true);
        Assert.Equivalent(expectedBounds, bounds, strict: true);
        Assert.Equivalent(expectedBatch, batch, strict: true);
        Assert.Equivalent(expectedHeader, header);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void BuildPlans_HolderFilterPolicy_ShouldControlOrFilters(bool combine, int expected)
    {
        var first = new LogHistoryTarget { Chain = "chain", Token = "token", Holder = "0x1111111111111111111111111111111111111111", Range = new BlockRange { From = 0, Through = 4 } };
        var incoming = new LogHistoryCoverage { Target = first, Direction = "incoming", MissingRanges = [first.Range], Sources = [] };
        var second = incoming with { Target = first with { Holder = "0x2222222222222222222222222222222222222222" } };
        var policy = new CapturePolicy { Template = CaptureFixture.Plan(), MaximumLogBlocks = 10, BatchSize = 1, CombineHolderFilters = combine };

        var result = CapturePlanBuilder.BuildPlans([], [incoming, second], new Dictionary<string, CapturePolicy> { ["chain"] = policy });

        Assert.Equivalent(expected, result["chain"].Queries.Count);
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 1, -1)]
    public void BuildPlans_InvalidLimitsOrRanges_ShouldReject(long blocks, int batchSize, long from)
    {
        var policy = new CapturePolicy { Template = CaptureFixture.Plan(), MaximumLogBlocks = blocks, BatchSize = batchSize };
        var target = new LogHistoryTarget { Chain = "chain", Token = "token", Holder = "0x1111111111111111111111111111111111111111", Range = new BlockRange { From = from, Through = 1 } };
        var coverage = new LogHistoryCoverage { Target = target, Direction = "incoming", MissingRanges = [target.Range], Sources = [] };

        var actual = Record.Exception(() => CapturePlanBuilder.BuildPlans([], [coverage], new Dictionary<string, CapturePolicy> { ["chain"] = policy }));

        Assert.NotNull(actual);
    }
}
