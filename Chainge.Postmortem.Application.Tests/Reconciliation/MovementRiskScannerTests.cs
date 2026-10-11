using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class MovementRiskScannerTests
{
    [Fact]
    public void FlagMovements_LargeUnmatchedDrain_ShouldFlagWithoutDeclaringLoss()
    {
        var events = new[] { Event("deposit", "holder", "vault", "100", 1), Event("pay", "vault", "holder", "95", 2), Event("irrelevant", "elsewhere", "another", "1", 3) };
        var movements = new[] { Review("deposit", "incoming"), Review("pay", "outgoing") };
        var expected = new[] { movements[0] with { ReconstructedBalanceBeforeRaw = "0", ReconstructedBalanceAfterRaw = "100" }, movements[1] with { ReconstructedBalanceBeforeRaw = "100", ReconstructedBalanceAfterRaw = "5", Flags = ["large-unmatched-outflow", "large-balance-reduction"] } };

        var actual = MovementRiskScanner.FlagMovements(events, movements, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FlagMovements_AmbiguousUnverifiedPaymentWithoutOpeningBalance_ShouldFlagDataGaps()
    {
        var events = new[] { Event("pay", "vault", "holder", "1", 1) with { ReceiptVerified = false } };
        var movements = new[] { Review("pay", "outgoing") with { Candidates = [new() { FusionEventId = "burn", Competing = true, Delay = TimeSpan.Zero, FeeRawHypothesis = "0" }] } };
        var expected = new[] { movements[0] with { ReconstructedBalanceBeforeRaw = "0", ReconstructedBalanceAfterRaw = "-1", Flags = ["competing-matches", "source-event-not-receipt-verified", "negative-reconstructed-balance"] } };

        var actual = MovementRiskScanner.FlagMovements(events, movements, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void FlagMovements_InvalidThreshold_ShouldReject(int basisPoints)
    {
        var policy = Policy() with { DepletionBasisPoints = basisPoints };

        var actual = Record.Exception(() => MovementRiskScanner.FlagMovements([], [], policy));

        Assert.IsType<ArgumentOutOfRangeException>(actual);
    }

    [Theory]
    [InlineData("possible-asset-exchange")]
    [InlineData("reported-asset-exchange")]
    public void FlagMovements_LargeAssetExchange_ShouldRemainInBackingReviewQueue(string classification)
    {
        var events = new[] { Event("deposit", "holder", "vault", "100", 1), Event("pay", "vault", "pool", "50", 2) };
        var movements = new[] { Review("pay", "outgoing") with { Classification = classification } };
        var expected = new[] { "backing-disposition-unresolved", "large-asset-exchange-outflow" };

        var actual = MovementRiskScanner.FlagMovements(events, movements, Policy()).Single().Flags;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FlagMovements_AbsoluteFeeFallback_ShouldRetainHypothesisWarning()
    {
        var events = new[] { Event("pay", "vault", "holder", "1", 1) };
        var movements = new[] { Review("pay", "outgoing") with { Classification = "possible-redemption", Candidates = [new() { FusionEventId = "burn", Rule = "short-delay-absolute-fee", Competing = false, Delay = TimeSpan.FromSeconds(20), FeeRawHypothesis = "1" }] } };
        var expected = new[] { "fee-exceeds-relative-bound", "negative-reconstructed-balance" };

        var actual = MovementRiskScanner.FlagMovements(events, movements, Policy()).Single().Flags;

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string from, string to, string amount, int block)
    {
        return new ChainEvent { Id = id, Chain = "BSC", Token = "token", Transaction = id, BlockHash = "block", Block = block, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static MovementReview Review(string id, string direction)
    {
        return new MovementReview { SourceEventId = id, Direction = direction, Classification = "unmatched", Candidates = [] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { Decimals = 0, LargeOutflowRaw = "50", DepletionBasisPoints = 9000, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
