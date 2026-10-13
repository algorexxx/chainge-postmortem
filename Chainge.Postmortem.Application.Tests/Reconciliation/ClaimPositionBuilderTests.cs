using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class ClaimPositionBuilderTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    [Fact]
    public void BuildPositions_CutoffAndSink_ShouldPreservePositionsWithoutLiabilityAdjustment()
    {
        var events = new[] { Event("mint", Zero, "holder", "100", 1), Event("sink", "holder", "one", "40", 2), Event("later", "holder", Zero, "10", 3) };
        var policy = Policy();
        var expected = new[] { Position("holder", "60", "remaining-perpetual-interval", true), Position("one", "40", "remaining-perpetual-interval", true) };

        var actual = ClaimPositionBuilder.BuildPositions(events, policy);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(0UL, 100UL, "expired-interval")]
    [InlineData(101UL, 200UL, "future-interval")]
    [InlineData(0UL, 200UL, "active-finite-interval")]
    public void BuildPositions_DatedInterval_ShouldClassifyAtCutoff(ulong start, ulong end, string status)
    {
        var events = new[] { Event("mint", Zero, "holder", "100", 1) };
        var policy = Policy() with { ClaimIntervals = new Dictionary<string, ClaimInterval> { ["token"] = new() { Start = start, End = end } } };
        var expected = new[] { Position("holder", "100", status, true) };

        var actual = ClaimPositionBuilder.BuildPositions(events, policy);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void BuildPositions_RecoveredNegativeBalance_ShouldKeepInconsistency()
    {
        var events = new[] { Event("missing", "holder", Zero, "100", 1), Event("mint", Zero, "holder", "100", 2) };
        var policy = Policy() with { ClaimIntervals = new Dictionary<string, ClaimInterval>() };
        var expected = new[] { Position("holder", "0", "unknown-interval", false) };

        var actual = ClaimPositionBuilder.BuildPositions(events, policy);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData("precision")]
    [InlineData("negative")]
    [InlineData("interval")]
    public void BuildPositions_InvalidInputs_ShouldReject(string kind)
    {
        var events = new[] { Event("mint", Zero, "holder", kind == "negative" ? "-1" : "100", 1) with { Decimals = kind == "precision" ? -1 : 6 } };
        var policy = kind == "interval" ? Policy() with { ClaimIntervals = new Dictionary<string, ClaimInterval> { ["token"] = new() { Start = 100, End = 100 } } } : Policy();

        var actual = Record.Exception(() => ClaimPositionBuilder.BuildPositions(events, policy));

        Assert.IsType<InvalidDataException>(actual);
    }

    private static HolderPosition Position(string holder, string amount, string status, bool consistent)
    {
        return new HolderPosition { Token = "token", Holder = holder, Decimals = 6, RawAmount = amount, IntervalStatus = status, HistoryConsistent = consistent };
    }

    private static ChainEvent Event(string id, string from, string to, string amount, long block)
    {
        return new ChainEvent { Id = id, Chain = "Fusion", Token = "token", Transaction = id, BlockHash = "block", Block = block, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 6, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { ClaimCutoffBlock = 2, ClaimCutoffUnixTime = 100, ClaimIntervals = new Dictionary<string, ClaimInterval> { ["token"] = new() { Start = 0, End = ulong.MaxValue } }, LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 6, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.Zero, ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = [] };
    }
}
