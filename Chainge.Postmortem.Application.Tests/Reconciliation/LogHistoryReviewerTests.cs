using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class LogHistoryReviewerTests
{
    [Fact]
    public void ReviewCoverage_OverlapsRetriesAndFailures_ShouldReportOnlyUncoveredInclusiveRanges()
    {
        var target = new LogHistoryTarget { Chain = "chain", Token = "TOKEN", Holder = "HOLDER", Range = new BlockRange { From = 10, Through = 30 } };
        var windows = new[] { Window(0, 12), Window(12, 15), Window(13, 14), Window(20, 25), Window(26, 40), Window(16, 19) with { Succeeded = false }, Window(16, 19) with { Holder = "other" }, Window(16, 19) with { Token = "other" }, Window(16, 19) with { Chain = "other" }, Window(0, 9), Window(41, 45) };
        var expected = new[] { new LogHistoryCoverage { Target = target, Direction = "incoming", MissingRanges = [new BlockRange { From = 16, Through = 19 }], Sources = ["source"] }, new LogHistoryCoverage { Target = target, Direction = "outgoing", MissingRanges = [new BlockRange { From = 10, Through = 30 }], Sources = [] } };

        var actual = LogHistoryReviewer.ReviewCoverage([target], windows);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void ReviewCoverage_SuccessfulRetryAndBothDirections_ShouldCloseGaps()
    {
        var target = new LogHistoryTarget { Chain = "chain", Token = "token", Holder = "holder", Range = new BlockRange { From = 0, Through = 10 } };
        var windows = new[] { Window(0, 10) with { Succeeded = false }, Window(0, 10), Window(0, 10) with { Direction = "outgoing" } };
        var expected = new[] { new LogHistoryCoverage { Target = target, Direction = "incoming", MissingRanges = [], Sources = ["source"] }, new LogHistoryCoverage { Target = target, Direction = "outgoing", MissingRanges = [], Sources = ["source"] } };

        var actual = LogHistoryReviewer.ReviewCoverage([target], windows);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(10, 9)]
    [InlineData(0, long.MaxValue)]
    public void ReviewCoverage_InvalidRange_ShouldReject(long from, long through)
    {
        var target = new LogHistoryTarget { Chain = "chain", Token = "token", Holder = "holder", Range = new BlockRange { From = from, Through = through } };

        var actual = Record.Exception(() => LogHistoryReviewer.ReviewCoverage([target], []));

        Assert.IsType<InvalidDataException>(actual);
    }

    private static LogQueryWindow Window(long from, long through)
    {
        return new LogQueryWindow { Chain = "chain", Token = "token", Holder = "holder", Direction = "incoming", Range = new BlockRange { From = from, Through = through }, Succeeded = true, Source = "source" };
    }
}
