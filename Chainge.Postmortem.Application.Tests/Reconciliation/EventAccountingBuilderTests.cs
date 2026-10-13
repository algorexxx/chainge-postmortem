using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class EventAccountingBuilderTests
{
    [Fact]
    public void BuildReport_InternalAndSelfTransfers_ShouldPreserveAmountsWithoutInflatingNetFlows()
    {
        var events = new[] { Event("a", "user", "vault", "100"), Event("b", "vault", "second", "30"), Event("c", "vault", "vault", "7"), Event("d", "vault", "user", "20") };
        var review = Result([Review("a", "incoming"), Review("b", "internal"), Review("c", "internal"), Review("d", "outgoing")]);
        var expected = new EventAccountingReport
        {
            Tokens = [new TokenEventTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, EventCount = 4, UnverifiedCount = 0, MissingTimestampCount = 0, MintedRaw = "0", BurnedRaw = "0", ObservedNetIssuanceRaw = "0" }],
            Movements = [new MovementTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, Direction = "incoming", Classification = "unmatched", Count = 1, RawAmount = "100" }, new MovementTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, Direction = "internal", Classification = "unmatched", Count = 2, RawAmount = "37" }, new MovementTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, Direction = "outgoing", Classification = "unmatched", Count = 1, RawAmount = "20" }],
            Addresses = [new AddressFlowTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, Address = "second", InflowRaw = "30", OutflowRaw = "0", ObservedNetFlowRaw = "30" }, new AddressFlowTotals { Chain = "Ethereum", Token = "token", Decimals = 6, FullTime = true, Address = "vault", InflowRaw = "107", OutflowRaw = "57", ObservedNetFlowRaw = "50" }]
        };

        var actual = EventAccountingBuilder.BuildReport(events, review, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void BuildReport_FusionSlicesAndPrecision_ShouldKeepRepresentationsSeparate()
    {
        const string zero = "0x0000000000000000000000000000000000000000";
        var events = new[] { Event("a", zero, "user", "100000000000000000001") with { Chain = "Fusion", Decimals = 18 }, Event("b", "user", zero, "1") with { Chain = "Fusion", Decimals = 18 }, Event("c", zero, "user", "50") with { Chain = "Fusion", Decimals = 18, FullTime = false, ReceiptVerified = false, Timestamp = null }, Event("d", "a", "b", "4") with { Token = "other" } };
        var expected = new[] { new TokenEventTotals { Chain = "Ethereum", Token = "other", Decimals = 6, FullTime = true, EventCount = 1, UnverifiedCount = 0, MissingTimestampCount = 0, MintedRaw = "0", BurnedRaw = "0", ObservedNetIssuanceRaw = "0" }, new TokenEventTotals { Chain = "Fusion", Token = "token", Decimals = 18, FullTime = false, EventCount = 1, UnverifiedCount = 1, MissingTimestampCount = 1, MintedRaw = "50", BurnedRaw = "0", ObservedNetIssuanceRaw = "50" }, new TokenEventTotals { Chain = "Fusion", Token = "token", Decimals = 18, FullTime = true, EventCount = 2, UnverifiedCount = 0, MissingTimestampCount = 0, MintedRaw = "100000000000000000001", BurnedRaw = "1", ObservedNetIssuanceRaw = "100000000000000000000" } };

        var actual = EventAccountingBuilder.BuildReport(events, Result([]), Policy()).Tokens;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void BuildReport_EmptyEvidence_ShouldReturnEmptyRowsRatherThanInventZeroBalances()
    {
        var expected = new EventAccountingReport { Tokens = [], Movements = [], Addresses = [] };

        var actual = EventAccountingBuilder.BuildReport([], Result([]), Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildReport_MissingOrExtraMovement_ShouldReject(bool missing)
    {
        var events = new[] { Event("a", "user", "vault", "1") };
        var review = Result(missing ? [] : [Review("a", "incoming"), Review("extra", "incoming")]);

        var actual = Record.Exception(() => EventAccountingBuilder.BuildReport(events, review, Policy()));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Theory]
    [InlineData(-1, "1")]
    [InlineData(256, "1")]
    [InlineData(18, "1")]
    [InlineData(6, "-1")]
    public void BuildReport_InvalidPrecisionOrAmount_ShouldReject(int decimals, string amount)
    {
        var events = new[] { Event("a", "a", "b", "1"), Event("b", "a", "b", amount) with { Decimals = decimals } };

        var actual = Record.Exception(() => EventAccountingBuilder.BuildReport(events, Result([]), Policy()));

        Assert.IsType<InvalidDataException>(actual);
    }

    private static ChainEvent Event(string id, string from, string to, string amount)
    {
        return new ChainEvent { Id = id, Chain = "Ethereum", Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 6, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
    }

    private static ReconciliationResult Result(IReadOnlyList<MovementReview> movements)
    {
        return new ReconciliationResult { Movements = movements, ConversionEventIds = [], UnpairedFusionEventIds = [] };
    }

    private static MovementReview Review(string id, string direction)
    {
        return new MovementReview { SourceEventId = id, Direction = direction, Classification = "unmatched", Candidates = [] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]> { ["Ethereum"] = ["vault", "second"] }, Decimals = 6, LargeOutflowRaw = "100", DepletionBasisPoints = 9000, MaximumDelay = TimeSpan.Zero, ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
