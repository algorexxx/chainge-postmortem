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

    [Theory]
    [InlineData(true, true, true, "100", "reported-asset-exchange")]
    [InlineData(false, true, true, "100", "reported-asset-exchange")]
    [InlineData(true, false, true, "100", "unmatched")]
    [InlineData(true, true, false, "100", "unmatched")]
    [InlineData(true, true, true, "99", "unmatched")]
    public void MatchMovements_ReportedSwap_ShouldRequireExactLegAndReceipt(bool outgoing, bool verified, bool context, string amount, string expected)
    {
        var swap = new ReceiptExchange { Emitter = "router", LogIndex = 3, Sender = outgoing ? "vault" : "other", SourceToken = outgoing ? "token" : "native", DestinationToken = outgoing ? "native" : "token", Recipient = outgoing ? "other" : "vault", SpentRaw = "100", ReturnedRaw = "100" };
        var events = new[] { Event("pay", "BSC", outgoing ? "vault" : "router", outgoing ? "pool" : "vault", amount) with { ReceiptVerified = verified, ReceiptContextCaptured = context, ReportedExchanges = [swap] } };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void MatchMovements_ReportedSwap_ShouldPreserveRecipientEvidenceWithoutCreatingBurnMatch()
    {
        var swap = new ReceiptExchange { Emitter = "router", LogIndex = 3, Sender = "vault", SourceToken = "token", DestinationToken = "native", Recipient = "third-party", SpentRaw = "100", ReturnedRaw = "7" };
        var events = new[] { Event("pay", "BSC", "vault", "pool", "100") with { ReceiptContextCaptured = true, ReportedExchanges = [swap] } };
        var expected = new MovementReview { SourceEventId = "pay", Direction = "outgoing", Classification = "reported-asset-exchange", Candidates = [], ExchangeEvidence = [swap] };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(60, true, true, "1090000000000000000", "possible-redemption")]
    [InlineData(61, true, true, "1090000000000000000", "unmatched")]
    [InlineData(-1, true, true, "1090000000000000000", "unmatched")]
    [InlineData(20, false, true, "1090000000000000000", "unmatched")]
    [InlineData(20, true, false, "1090000000000000000", "unmatched")]
    [InlineData(20, true, true, "1500000000000000001", "unmatched")]
    public void MatchMovements_AbsoluteFeeFallback_ShouldRequireVerifiedReceiptsAndTighterTiming(int delay, bool sourceVerified, bool burnVerified, string burned, string expected)
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, burned) with { ReceiptVerified = burnVerified }, Event("pay", "BSC", "vault", "holder", "1000000000000000000") with { ReceiptVerified = sourceVerified, Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(delay) } };
        var policy = Policy() with { VariableFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumDelay = TimeSpan.FromMinutes(5), AbsoluteFeeMaximumDelay = TimeSpan.FromMinutes(1), MaximumFeeBasisPoints = 100, MaximumFeeRaw = "500000000000000000" } } };

        var actual = MovementMatcher.MatchMovements(events, policy).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(99, true, "99980000000000000000", "possible-deposit")]
    [InlineData(301, true, "99980000000000000000", "unmatched")]
    [InlineData(-1, true, "99980000000000000000", "unmatched")]
    [InlineData(99, false, "99980000000000000000", "unmatched")]
    [InlineData(99, true, "99940000000000000000", "unmatched")]
    public void MatchMovements_DepositFee_ShouldRequireMintReceiptAndConfiguredBounds(int delay, bool verified, string minted, string expected)
    {
        var events = new[] { Event("deposit", "BSC", "holder", "vault", "100000000000000000000"), Event("mint", "Fusion", Zero, "holder", minted) with { ReceiptVerified = verified, Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(delay) } };
        var policy = Policy() with { DepositFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumDelay = TimeSpan.FromMinutes(5), MaximumFeeBasisPoints = 100, MaximumFeeRaw = "50000000000000000" } } };

        var actual = MovementMatcher.MatchMovements(events, policy).Movements.Single().Classification;

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MatchMovements_ReportedSwapAndCounterAsset_ShouldPreferExplicitEvidence(bool outgoing)
    {
        var swap = new ReceiptExchange { Emitter = "router", LogIndex = 3, Sender = "vault", SourceToken = outgoing ? "token" : "counter", DestinationToken = outgoing ? "counter" : "token", Recipient = "vault", SpentRaw = "100", ReturnedRaw = "100" };
        var events = new[] { Event("pay", "BSC", outgoing ? "vault" : "pool", outgoing ? "pool" : "vault", "100") with { ReceiptContextCaptured = true, ReportedExchanges = [swap], RelatedTransfers = [new() { Token = "counter", From = outgoing ? "pool" : "vault", To = outgoing ? "vault" : "pool", RawAmount = "100", LogIndex = 2 }] } };
        var expected = new MovementReview { SourceEventId = "pay", Direction = outgoing ? "outgoing" : "incoming", Classification = "reported-asset-exchange", Candidates = [], ExchangeEvidence = [swap] };

        var actual = MovementMatcher.MatchMovements(events, Policy()).Movements.Single();

        Assert.Equivalent(expected, actual, strict: true);
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
