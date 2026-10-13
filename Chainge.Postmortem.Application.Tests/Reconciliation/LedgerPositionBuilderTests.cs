using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class LedgerPositionBuilderTests
{
    [Fact]
    public void BuildPositions_ChainAndCutoff_ShouldKeepIndependentTokenLedgers()
    {
        var first = new ChainEvent { Id = "one", Chain = "source", Token = "token", Transaction = "tx", BlockHash = "block", Block = 1, LogIndex = 0, From = "0x0000000000000000000000000000000000000000", To = "holder", RawAmount = "100", Decimals = 6, TimestampSource = "fixture", ReceiptVerified = false, FullTime = true, Evidence = [] };
        var events = new[] { first, first with { Id = "two", Chain = "other", RawAmount = "500" }, first with { Id = "three", Block = 3, RawAmount = "900" }, first with { Id = "four", Token = "other-token", RawAmount = "20" } };
        var expected = new[] { new HolderPosition { Token = "other-token", Holder = "holder", Decimals = 6, RawAmount = "20", IntervalStatus = "not-assessed", HistoryConsistent = true }, new HolderPosition { Token = "token", Holder = "holder", Decimals = 6, RawAmount = "100", IntervalStatus = "not-assessed", HistoryConsistent = true } };

        var actual = LedgerPositionBuilder.BuildPositions(events, "source", 2);

        Assert.Equivalent(expected, actual, strict: true);
    }
}
