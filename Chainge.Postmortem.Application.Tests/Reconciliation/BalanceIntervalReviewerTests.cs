using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class BalanceIntervalReviewerTests
{
    [Theory]
    [InlineData("20", false, false, "-20", true, "net-change-agrees-coverage-incomplete")]
    [InlineData("20", true, false, "-20", true, "net-change-agrees-covered")]
    [InlineData("21", true, false, "-19", true, "balance-change-mismatch")]
    [InlineData(null, true, false, null, null, "state-unavailable")]
    [InlineData("20", true, true, "-20", false, "net-change-agrees-inconsistent-history")]
    public void ReviewIntervals_PinnedWindow_ShouldCompareChangesAndKeepCoverageSeparate(string? closingAmount, bool covered, bool overdraft, string? observedChange, bool? consistent, string status)
    {
        var opening = CreateObservation(10, "40");
        var closing = CreateObservation(12, closingAmount);
        var outgoing = CreateEvent(11, "holder", "other", overdraft ? "50" : "30");
        var incoming = CreateEvent(12, "other", "holder", overdraft ? "30" : "10");
        var before = CreateEvent(10, "other", "holder", "500");
        var after = CreateEvent(13, "other", "holder", "600");
        var self = CreateEvent(11, "holder", "holder", "1") with { LogIndex = 1 };
        var query = new LogQueryWindow { Chain = "chain", Token = "token", Holder = "holder", Direction = "incoming", Range = new BlockRange { From = 11, Through = 12 }, Succeeded = covered, Source = "query" };
        var expected = new[] { new BalanceIntervalCheck { Opening = [opening], Closing = [closing], ReplayedChangeRaw = "-20", ObservedChangeRaw = observedChange, HistoryCovered = covered, HistoryConsistent = consistent, Status = status } };

        var actual = BalanceIntervalReviewer.ReviewIntervals([after, incoming, outgoing, before, self, incoming with { Chain = "different" }, incoming with { Token = "different" }, incoming with { To = "different" }], [closing, opening], [query, query with { Direction = "outgoing" }]);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void ReviewIntervals_ConflictingSnapshots_ShouldKeepAllSourcesAndRejectComparison()
    {
        var opening = CreateObservation(10, "40");
        var conflict = opening with { RawAmount = "41", Source = "other-node" };
        var closing = CreateObservation(12, "40");
        var expected = new[] { new BalanceIntervalCheck { Opening = [opening, conflict], Closing = [closing], ReplayedChangeRaw = "0", ObservedChangeRaw = null, HistoryCovered = false, HistoryConsistent = null, Status = "conflicting-state" } };

        var actual = BalanceIntervalReviewer.ReviewIntervals([], [opening, conflict, closing], []);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void ReviewIntervals_DuplicateAndFailedReads_ShouldKeepProvenanceWithoutDuplicatingIntervals()
    {
        var opening = CreateObservation(10, "40");
        var duplicate = opening with { Source = "other-node", Token = "TOKEN", Holder = "HOLDER" };
        var failed = opening with { RawAmount = null, Source = "failed-node" };
        var closing = CreateObservation(12, "40");
        var expected = new[] { new BalanceIntervalCheck { Opening = [opening, duplicate, failed], Closing = [closing], ReplayedChangeRaw = "0", ObservedChangeRaw = "0", HistoryCovered = false, HistoryConsistent = true, Status = "net-change-agrees-coverage-incomplete" } };

        var actual = BalanceIntervalReviewer.ReviewIntervals([], [opening, duplicate, failed, closing, closing with { Holder = "other" }], []);

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static BalanceObservation CreateObservation(long block, string? amount)
    {
        return new BalanceObservation { Chain = "chain", Token = "token", Holder = "holder", Block = block, RawAmount = amount, Source = "state" };
    }

    private static ChainEvent CreateEvent(long block, string from, string to, string amount)
    {
        return new ChainEvent { Id = block + from + to, Chain = "chain", Token = "token", Transaction = "tx", BlockHash = "hash", Block = block, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 6, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
    }
}
