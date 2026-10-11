using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class MovementSiteBuilderTests
{
    [Fact]
    public void BuildReport_RepeatedDestination_ShouldSeparateChainTokenAndExcludeIncomingAndInternal()
    {
        var events = new[] { Event("a", "BSC", "token", "95"), Event("b", "BSC", "token", "5"), Event("c", "Ethereum", "token", "20"), Event("d", "BSC", "other", "30"), Event("in", "BSC", "token", "999"), Event("internal", "BSC", "token", "888") };
        var reviews = new[] { Review("a"), Review("b") with { Classification = "possible-redemption" }, Review("c"), Review("d"), Review("in") with { Direction = "incoming" }, Review("internal") with { Direction = "internal" } };
        var expected = new[] {
            new DestinationSummary { Chain = "BSC", Token = "token", Address = "recipient", Decimals = 0, Count = 2, GrossOutflowRaw = "100", UnmatchedCount = 1, UnmatchedOutflowRaw = "95" },
            new DestinationSummary { Chain = "Ethereum", Token = "token", Address = "recipient", Decimals = 0, Count = 1, GrossOutflowRaw = "20", UnmatchedCount = 1, UnmatchedOutflowRaw = "20" },
            new DestinationSummary { Chain = "BSC", Token = "other", Address = "recipient", Decimals = 0, Count = 1, GrossOutflowRaw = "30", UnmatchedCount = 1, UnmatchedOutflowRaw = "30" }
        };
        var expectedIds = new[] { "a", "b", "c", "d", "in", "internal" };
        var expectedCase = new MovementSiteCase { Id = "a", Chain = "BSC", Token = "token", Transaction = "a", From = "vault", To = "recipient", RawAmount = "95", Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch, ReceiptVerified = true, Evidence = ["public.json"], Review = reviews[0], DepletionBasisPoints = 9500 };

        var actual = MovementSiteBuilder.BuildReport(events, Result(reviews));

        Assert.Equivalent(expected, actual.Destinations, strict: true);
        Assert.Equivalent(expectedCase, actual.Cases[0], strict: true);
        Assert.Equivalent(expectedIds, actual.Cases.Select(c => c.Id).ToArray(), strict: true);
    }

    [Theory]
    [InlineData(null, "95", null)]
    [InlineData("0", "95", null)]
    [InlineData("-1", "95", null)]
    [InlineData("50", "95", null)]
    [InlineData("100", "-1", null)]
    [InlineData("100", "100", 10000)]
    [InlineData("3", "1", 3333)]
    [InlineData("1000000000000000000000000000000", "500000000000000000000000000000", 5000)]
    public void BuildReport_BalanceReconstruction_ShouldOnlyCalculatePlausiblePercentages(string? before, string raw, int? expected)
    {
        var events = new[] { Event("a", "BSC", "token", raw) };
        var result = Result([Review("a") with { ReconstructedBalanceBeforeRaw = before }]);

        var actual = MovementSiteBuilder.BuildReport(events, result).Cases[0].DepletionBasisPoints;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void BuildReport_NoMovements_ShouldReturnEmptyReport()
    {
        var expected = new MovementSiteReport { Cases = [], Destinations = [] };

        var actual = MovementSiteBuilder.BuildReport([], Result([]));

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string chain, string token, string raw)
    {
        return new ChainEvent { Id = id, Chain = chain, Token = token, Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = "vault", To = "recipient", RawAmount = raw, Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["public.json"] };
    }

    private static MovementReview Review(string id)
    {
        return new MovementReview { SourceEventId = id, Direction = "outgoing", Classification = "unmatched", Candidates = [], ReconstructedBalanceBeforeRaw = "100" };
    }

    private static ReconciliationResult Result(MovementReview[] reviews)
    {
        return new ReconciliationResult { Movements = reviews, ConversionEventIds = [], UnpairedFusionEventIds = [] };
    }
}
