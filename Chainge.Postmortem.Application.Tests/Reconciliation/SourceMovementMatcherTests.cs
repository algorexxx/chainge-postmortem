using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class SourceMovementMatcherTests
{
    [Fact]
    public void MatchMovements_CrossChainPayment_ShouldLinkBothSidesWithoutFusionCandidate()
    {
        var events = new[] { Event("deposit", "A", "holder", "vault", "100", 0), Event("pay", "B", "vault", "holder", "97", 60) };
        var movements = new[] { Review("deposit", "incoming"), Review("pay", "outgoing") };
        var expected = new[] { movements[0] with { Classification = "possible-source-chain-transfer", SourceCandidates = [new() { OtherEventId = "pay", FeeRawHypothesis = "3", Delay = TimeSpan.FromSeconds(60), Competing = false }] }, movements[1] with { Classification = "possible-source-chain-transfer", SourceCandidates = [new() { OtherEventId = "deposit", FeeRawHypothesis = "3", Delay = TimeSpan.FromSeconds(60), Competing = false }] } };

        var actual = SourceMovementMatcher.MatchMovements(events, movements, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(-1, "97", "B", "holder", true)]
    [InlineData(301, "97", "B", "holder", true)]
    [InlineData(60, "94", "B", "holder", true)]
    [InlineData(60, "101", "B", "holder", true)]
    [InlineData(60, "97", "A", "holder", true)]
    [InlineData(60, "97", "B", "other", true)]
    [InlineData(60, "97", "B", "holder", false)]
    public void MatchMovements_InsufficientEvidence_ShouldKeepUnmatched(int delay, string paid, string chain, string recipient, bool verified)
    {
        var events = new[] { Event("deposit", "A", "holder", "vault", "100", 0), Event("pay", chain, "vault", recipient, paid, delay) with { ReceiptVerified = verified } };
        var expected = new[] { Review("deposit", "incoming"), Review("pay", "outgoing") };

        var actual = SourceMovementMatcher.MatchMovements(events, expected, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_CompetingPayments_ShouldRetainBothLinks()
    {
        var events = new[] { Event("deposit", "A", "holder", "vault", "100", 0), Event("pay1", "B", "vault", "holder", "97", 60), Event("pay2", "B", "vault", "holder", "97", 70) };
        var movements = new[] { Review("deposit", "incoming"), Review("pay1", "outgoing"), Review("pay2", "outgoing") };
        var expected = new[] { new SourcePairCandidate { OtherEventId = "pay1", FeeRawHypothesis = "3", Delay = TimeSpan.FromSeconds(60), Competing = true }, new SourcePairCandidate { OtherEventId = "pay2", FeeRawHypothesis = "3", Delay = TimeSpan.FromSeconds(70), Competing = true } };

        var actual = SourceMovementMatcher.MatchMovements(events, movements, Policy())[0].SourceCandidates;

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void MatchMovements_AlreadyLinkedDeposit_ShouldNotReuseFunding()
    {
        var events = new[] { Event("deposit", "A", "holder", "vault", "100", 0), Event("pay", "B", "vault", "holder", "97", 60) };
        var expected = new[] { Review("deposit", "incoming") with { Classification = "possible-deposit" }, Review("pay", "outgoing") };

        var actual = SourceMovementMatcher.MatchMovements(events, expected, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string chain, string from, string to, string amount, int delay)
    {
        return new ChainEvent { Id = id, Chain = chain, Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(delay), TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static MovementReview Review(string id, string direction)
    {
        return new MovementReview { SourceEventId = id, Direction = direction, Classification = "unmatched", Candidates = [] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { SourceChainRule = new() { MaximumDelay = TimeSpan.FromMinutes(5), MaximumFeeRaw = "5", MaximumFeeBasisPoints = 500 }, Decimals = 0, LargeOutflowRaw = "50", DepletionBasisPoints = 9000, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
