using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class MovementMatcherTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    [Fact]
    public void MatchMovements_KnownFeeAndDifferentDecimals_ShouldRetainCandidate()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, "300000500000000000000000"), Event("payment", "BSC", "vault", "holder", "300000000000") with { Decimals = 6, Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(29) } };
        var expected = new ReconciliationResult { Movements = [new() { SourceEventId = "payment", Direction = "outgoing", Classification = "possible-redemption", Candidates = [new() { FusionEventId = "burn", FeeRawHypothesis = "500000000000000000", Delay = TimeSpan.FromSeconds(29), Competing = false }] }], ConversionEventIds = [], UnpairedFusionEventIds = [] };

        var actual = MovementMatcher.MatchMovements(events, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_CompetingPayments_ShouldPreserveBothCandidates()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, "100"), Event("pay1", "BSC", "vault", "holder", "100"), Event("pay2", "BSC", "vault", "holder", "100") };
        var expected = new[] { new PairCandidate { FusionEventId = "burn", FeeRawHypothesis = "0", Delay = TimeSpan.Zero, Competing = true }, new PairCandidate { FusionEventId = "burn", FeeRawHypothesis = "0", Delay = TimeSpan.Zero, Competing = true } };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.SelectMany(m => m.Candidates).ToArray();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_TwoBurnsForOnePayment_ShouldKeepAmbiguity()
    {
        var events = new[] { Event("burn-b", "Fusion", "holder", Zero, "100"), Event("burn-a", "Fusion", "holder", Zero, "100"), Event("pay", "BSC", "vault", "holder", "100") };
        var expected = new[] { new PairCandidate { FusionEventId = "burn-a", FeeRawHypothesis = "0", Delay = TimeSpan.Zero, Competing = true }, new PairCandidate { FusionEventId = "burn-b", FeeRawHypothesis = "0", Delay = TimeSpan.Zero, Competing = true } };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Candidates;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_MigrationAndPartialSlice_ShouldExcludeExternalPairing()
    {
        var events = new[] { Event("old-burn", "Fusion", "holder", Zero, "100") with { Transaction = "conversion", Token = "old" }, Event("new-mint", "Fusion", Zero, "holder", "100") with { Transaction = "conversion", Token = "new" }, Event("slice", "Fusion", "holder", Zero, "100") with { FullTime = false }, Event("payment", "BSC", "vault", "holder", "100"), Event("unused", "Fusion", "other", Zero, "200") };
        var expected = new ReconciliationResult { Movements = [new() { SourceEventId = "payment", Direction = "outgoing", Classification = "unmatched", Candidates = [] }], ConversionEventIds = ["new-mint", "old-burn"], UnpairedFusionEventIds = ["unused"] };

        var actual = MovementMatcher.MatchMovements(events, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_DirectionAndDataGaps_ShouldRemainExplicit()
    {
        var events = new[] { Event("zero", "BSC", "vault", "holder", "0"), Event("internal", "BSC", "vault", "vault2", "3"), Event("missing", "BSC", "vault", "holder", "5") with { Timestamp = null }, Event("mint", "Fusion", Zero, "holder", "9") with { Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(-1) }, Event("deposit", "BSC", "holder", "vault", "9"), Event("irrelevant", "BSC", "holder", "another", "9"), Event("unknown-chain", "Other", "holder", "vault", "9") };
        var expected = new[] { "zero-value", "operating-address-transfer", "missing-timestamp", "possible-deposit" };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Select(m => m.Classification).ToArray();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(-121, "100", "unmatched")]
    [InlineData(-120, "100", "possible-redemption")]
    [InlineData(86400, "100", "possible-redemption")]
    [InlineData(86401, "100", "unmatched")]
    [InlineData(0, "101", "unmatched")]
    public void MatchMovements_WindowAndAmountRules_ShouldApplyExactBoundaries(int delay, string paid, string expected)
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, "100"), Event("pay", "BSC", "vault", "holder", paid) with { Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(delay) } };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void GetComparableUnits_InvalidDecimals_ShouldReject(int decimals)
    {
        var input = Event("bad", "Fusion", Zero, "holder", "1") with { Decimals = decimals };

        var actual = Record.Exception(() => input.GetComparableUnits());

        Assert.IsType<ArgumentOutOfRangeException>(actual);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void MatchMovements_InvalidPolicyDecimals_ShouldReject(int decimals)
    {
        var policy = Policy() with { Decimals = decimals };

        var actual = Record.Exception(() => MovementMatcher.MatchMovements([], policy));

        Assert.IsType<ArgumentOutOfRangeException>(actual);
    }

    [Theory]
    [InlineData(20, "100090000000000000000", "possible-redemption")]
    [InlineData(300, "100090000000000000000", "possible-redemption")]
    [InlineData(301, "100090000000000000000", "unmatched")]
    [InlineData(-1, "100090000000000000000", "unmatched")]
    [InlineData(20, "100600000000000000000", "unmatched")]
    [InlineData(20, "99900000000000000000", "unmatched")]
    public void MatchMovements_VariableFee_ShouldApplyShortWindowAndAbsoluteCap(int delay, string burned, string expected)
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, burned), Event("pay", "BSC", "vault", "holder", "100000000000000000000") with { Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(delay) } };
        var policy = Policy() with { VariableFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumDelay = TimeSpan.FromMinutes(5), MaximumFeeBasisPoints = 100, MaximumFeeRaw = "500000000000000000" } } };

        var actual = MovementMatcher.MatchMovements(events, policy).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void MatchMovements_VariableFeeTooLargeRelativeToPayment_ShouldRemainUnmatched()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, "1090000000000000000"), Event("pay", "BSC", "vault", "holder", "1000000000000000000") };
        var policy = Policy() with { VariableFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumDelay = TimeSpan.FromMinutes(5), MaximumFeeBasisPoints = 100, MaximumFeeRaw = "500000000000000000" } } };
        var expected = "unmatched";

        var actual = MovementMatcher.MatchMovements(events, policy).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(true, true, "vault", "possible-asset-exchange")]
    [InlineData(true, false, "vault", "unmatched")]
    [InlineData(false, true, "vault", "unmatched")]
    [InlineData(true, true, "someone-else", "unmatched")]
    public void MatchMovements_CounterAssetReceipt_ShouldRequireVerifiedCreditToOperatingAddress(bool verified, bool context, string recipient, string expected)
    {
        var events = new[] { Event("pay", "BSC", "vault", "pool", "100") with { ReceiptVerified = verified, ReceiptContextCaptured = context, RelatedTransfers = [new() { Token = "other-token", From = "pool", To = recipient, RawAmount = "7", LogIndex = 1 }] } };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void MatchMovements_CounterAssetNetZero_ShouldRemainUnmatched()
    {
        var events = new[] { Event("pay", "BSC", "vault", "pool", "100") with { ReceiptContextCaptured = true, RelatedTransfers = [new() { Token = "other-token", From = "pool", To = "vault", RawAmount = "7", LogIndex = 1 }, new() { Token = "other-token", From = "vault", To = "pool", RawAmount = "7", LogIndex = 2 }] } };
        var expected = "unmatched";

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void MatchMovements_IncomingSwapProceeds_ShouldRequireCounterAssetDebit()
    {
        var events = new[] { Event("pay", "BSC", "pool", "vault", "100") with { ReceiptContextCaptured = true, RelatedTransfers = [new() { Token = "other-token", From = "vault", To = "pool", RawAmount = "7", LogIndex = 1 }] } };
        var expected = "possible-asset-exchange";

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    private static ChainEvent Event(string id, string chain, string from, string to, string amount)
    {
        return new ChainEvent { Id = id, Chain = chain, Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 18, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { LargeOutflowRaw = "10000000000000000000000", DepletionBasisPoints = 9000, Decimals = 18, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["vault", "vault2"] }, MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.FromSeconds(120), RedemptionFeeRawHypotheses = ["0", "500000000000000000"] };
    }
}
