using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class DelayedCandidateFinderTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    [Fact]
    public void FindCandidates_Disabled_ShouldReturnEmpty()
    {
        var baseline = new ReconciliationResult { Movements = [], ConversionEventIds = [], UnpairedFusionEventIds = [] };
        var expected = Array.Empty<MovementReview>();

        var actual = DelayedCandidateFinder.FindCandidates([], Policy() with { DelayedReviewMaximumDelay = TimeSpan.Zero }, baseline);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FindCandidates_DelayedBurnAlsoUsedNormally_ShouldRetainCompetitionWithoutChangingBaseline()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero, "101", 0), Event("normal", "BSC", "vault", "holder", "100", 60), Event("late", "BSC", "vault", "holder", "100", 600) };
        var policy = Policy();
        var baseline = MovementMatcher.MatchMovements(events, policy);
        var expected = new MovementReview { SourceEventId = "late", Direction = "outgoing", Classification = "delayed-candidate-review", Candidates = [new() { FusionEventId = "burn", Delay = TimeSpan.FromMinutes(10), FeeRawHypothesis = "1", Rule = "bounded-variable-fee", Competing = true }] };
        var expectedBaseline = new[] { "possible-redemption", "unmatched" };

        var actual = DelayedCandidateFinder.FindCandidates(events, policy, baseline);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
        Assert.Equivalent(expectedBaseline, baseline.Movements.Select(m => m.Classification).ToArray(), strict: true);
    }

    [Fact]
    public void FindCandidates_DepositAndSourceTransfer_ShouldRetainSeparateEdges()
    {
        var events = new[] { Event("deposit", "BSC", "holder", "vault", "100", 0), Event("mint", "Fusion", Zero, "holder", "99", 600), Event("in", "BSC", "other", "vault", "100", 0), Event("out", "Ethereum", "vault", "other", "99", 600) };
        var policy = Policy() with { DepositFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumFeeRaw = "1", MaximumFeeBasisPoints = 100, MaximumDelay = TimeSpan.FromMinutes(5) } }, SourceChainRule = new() { MaximumFeeRaw = "1", MaximumFeeBasisPoints = 100, MaximumDelay = TimeSpan.FromMinutes(5) } };
        var baseline = MovementMatcher.MatchMovements(events, policy);
        var expected = new[] {
            new MovementReview { SourceEventId = "deposit", Direction = "incoming", Classification = "delayed-candidate-review", Candidates = [new() { FusionEventId = "mint", FeeRawHypothesis = "1", Delay = TimeSpan.FromMinutes(10), Rule = "bounded-deposit-fee", Competing = false }] },
            new MovementReview { SourceEventId = "in", Direction = "incoming", Classification = "delayed-candidate-review", Candidates = [], SourceCandidates = [new() { OtherEventId = "out", FeeRawHypothesis = "1", Delay = TimeSpan.FromMinutes(10), Competing = false }] },
            new MovementReview { SourceEventId = "out", Direction = "outgoing", Classification = "delayed-candidate-review", Candidates = [], SourceCandidates = [new() { OtherEventId = "in", FeeRawHypothesis = "1", Delay = TimeSpan.FromMinutes(10), Competing = false }] }
        };

        var actual = DelayedCandidateFinder.FindCandidates(events, policy, baseline);

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string chain, string from, string to, string amount, int seconds)
    {
        return new ChainEvent { Id = id, Chain = chain, Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch.AddSeconds(seconds), TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { DelayedReviewMaximumDelay = TimeSpan.FromDays(1), LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 0, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["vault"], ["Ethereum"] = ["vault"] }, MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"], VariableFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumFeeRaw = "1", MaximumFeeBasisPoints = 100, MaximumDelay = TimeSpan.FromMinutes(5) } } };
    }
}
