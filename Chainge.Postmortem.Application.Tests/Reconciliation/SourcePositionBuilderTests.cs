using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class SourcePositionBuilderTests
{
    [Fact]
    public void BuildSnapshots_IncompleteHistory_ShouldPreserveNetPositionAndExplicitGaps()
    {
        var first = new ChainEvent { Id = "one", Chain = "source", Token = "token", Transaction = "tx", BlockHash = "block", Block = 1, LogIndex = 0, From = "0x0000000000000000000000000000000000000000", To = "holder", RawAmount = "100", Decimals = 6, TimestampSource = "fixture", ReceiptVerified = false, FullTime = true, Evidence = [] };
        var events = new[] { first, first with { Id = "two", To = "other" }, first with { Id = "three", Token = "other-token" }, first with { Id = "later", Block = 3 } };
        var target = new LogHistoryTarget { Chain = "source", Token = "token", Holder = "holder", Range = new BlockRange { From = 0, Through = 2 } };
        var expected = new[] { new SourceLedgerSnapshot { Chain = "source", Block = 2, Positions = [new HolderPosition { Token = "token", Holder = "holder", RawAmount = "100", Decimals = 6, HistoryConsistent = true, IntervalStatus = "not-assessed" }], Coverage = [new LogHistoryCoverage { Target = target, Direction = "incoming", MissingRanges = [new BlockRange { From = 0, Through = 2 }], Sources = [] }, new LogHistoryCoverage { Target = target, Direction = "outgoing", MissingRanges = [new BlockRange { From = 0, Through = 2 }], Sources = [] }] } };

        var actual = SourcePositionBuilder.BuildSnapshots(events, [target with { Range = new BlockRange { From = 0, Through = 10 } }], [], new Dictionary<string, long> { ["source"] = 2 });

        Assert.Equivalent(expected, actual, strict: true);
    }
}
