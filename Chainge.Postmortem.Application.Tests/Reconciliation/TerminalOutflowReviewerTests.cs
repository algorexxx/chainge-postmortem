using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class TerminalOutflowReviewerTests
{
    [Fact]
    public void ReviewAddresses_ReplenishmentAndCleanup_ShouldKeepSubstantialPaymentAndCompleteSequence()
    {
        var events = new[] { Event("cleanup", "vault", "receiver", "15", 5), Event("fund", "outside", "vault", "100", 1), Event("large", "vault", "receiver", "90", 2), Event("refill", "outside", "vault", "5", 3), Event("self", "vault", "vault", "10", 4), Event("zero", "vault", "receiver", "0", 6) };
        var expected = new TerminalOutflowSummary { Chain = "BSC", Token = "token", Address = "vault", Decimals = 0, FirstEventId = "fund", LastEventId = "zero", FinalOutflowId = "cleanup", LastSubstantialOutflowId = "large", LastOutflowIds = ["large", "cleanup"], SequenceEventIds = ["large", "refill", "self", "cleanup", "zero"], SequenceOpeningBalanceRaw = "100", ClosingBalanceRaw = "0", SequenceInflowRaw = "5", SequenceOutflowRaw = "105", SequenceInflowCount = 1, NetReductionBasisPoints = 10000, NonnegativeHistory = true, AllReceiptsVerified = true };

        var actual = TerminalOutflowReviewer.ReviewAddresses(events, Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewAddresses_OnlyIncoming_ShouldAvoidInventingTerminalOutflow()
    {
        var events = new[] { Event("fund", "outside", "vault", "100", 1) with { ReceiptVerified = false, Timestamp = null } };
        var expected = new TerminalOutflowSummary { Chain = "BSC", Token = "token", Address = "vault", Decimals = 0, FirstEventId = "fund", LastEventId = "fund", LastOutflowIds = [], SequenceEventIds = [], ClosingBalanceRaw = "100", SequenceInflowRaw = "0", SequenceOutflowRaw = "0", SequenceInflowCount = 0, NonnegativeHistory = true, AllReceiptsVerified = false };

        var actual = TerminalOutflowReviewer.ReviewAddresses(events, Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewAddresses_NoSubstantialPayment_ShouldUseLastTenPositiveOutflows()
    {
        var events = new[] { Event("fund", "outside", "vault", "20", 1) }.Concat(Enumerable.Range(1, 12).Select(i => Event("pay" + i, "vault", "receiver", "1", i + 1))).ToList();
        var expected = new TerminalOutflowSummary { Chain = "BSC", Token = "token", Address = "vault", Decimals = 0, FirstEventId = "fund", LastEventId = "pay12", FinalOutflowId = "pay12", LastOutflowIds = ["pay3", "pay4", "pay5", "pay6", "pay7", "pay8", "pay9", "pay10", "pay11", "pay12"], SequenceEventIds = ["pay3", "pay4", "pay5", "pay6", "pay7", "pay8", "pay9", "pay10", "pay11", "pay12"], SequenceOpeningBalanceRaw = "18", ClosingBalanceRaw = "8", SequenceInflowRaw = "0", SequenceOutflowRaw = "10", SequenceInflowCount = 0, NetReductionBasisPoints = 5555, NonnegativeHistory = true, AllReceiptsVerified = true };

        var actual = TerminalOutflowReviewer.ReviewAddresses(events, Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Theory]
    [InlineData("0", "1", "0")]
    [InlineData("100", "60", "200")]
    public void ReviewAddresses_InvalidOpeningOrNetIncrease_ShouldNotReportDepletion(string funding, string payment, string refill)
    {
        var events = new[] { Event("fund", "outside", "vault", funding, 1), Event("pay", "vault", "receiver", payment, 2), Event("refill", "outside", "vault", refill, 3) };
        int? expected = null;

        var actual = TerminalOutflowReviewer.ReviewAddresses(events, Policy()).Single().NetReductionBasisPoints;

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ReviewAddresses_NegativeHistoryBeforeRecovery_ShouldRetainCoverageWarning()
    {
        var events = new[] { Event("early", "vault", "receiver", "10", 1), Event("fund", "outside", "vault", "110", 2), Event("pay", "vault", "receiver", "100", 3) };
        var expected = new TerminalOutflowSummary { Chain = "BSC", Token = "token", Address = "vault", Decimals = 0, FirstEventId = "early", LastEventId = "pay", FinalOutflowId = "pay", LastSubstantialOutflowId = "pay", LastOutflowIds = ["early", "pay"], SequenceEventIds = ["pay"], SequenceOpeningBalanceRaw = "100", ClosingBalanceRaw = "0", SequenceInflowRaw = "0", SequenceOutflowRaw = "100", SequenceInflowCount = 0, NonnegativeHistory = false, AllReceiptsVerified = true };

        var actual = TerminalOutflowReviewer.ReviewAddresses(events, Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void FindSharedDestinations_MultipleVaults_ShouldKeepChainAndTokenSeparate()
    {
        var events = new[] { Event("a", "vault", "recipient", "60", 1), Event("b", "second", "recipient", "70", 2), Event("eth", "vault", "recipient", "80", 3) with { Chain = "Ethereum" }, Event("other", "vault", "recipient", "90", 4) with { Token = "other" }, Event("self", "vault", "vault", "100", 5) };
        var policy = Policy() with { OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["vault", "second"], ["Ethereum"] = ["vault"] } };
        var summaries = TerminalOutflowReviewer.ReviewAddresses(events, policy);
        var expected = new TerminalDestination { Chain = "BSC", Token = "token", Address = "recipient", SourceAddresses = ["second", "vault"], EventIds = ["a", "b"] };

        var actual = TerminalOutflowReviewer.FindSharedDestinations(events, summaries);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewAddresses_EmptyEvidence_ShouldReturnNoObservedHistories()
    {
        TerminalOutflowSummary[] expected = [];

        var actual = TerminalOutflowReviewer.ReviewAddresses([], Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FindCrossChainRecipients_SameHexAddress_ShouldPreserveSeparateChainEventsWithoutOwnershipClaim()
    {
        var events = new[] { Event("bsc", "vault", "recipient", "60", 1), Event("eth", "vault", "recipient", "70", 2) with { Chain = "Ethereum" }, Event("unshared", "vault", "different", "80", 3) };
        var policy = Policy() with { OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["vault"], ["Ethereum"] = ["vault"] } };
        var summaries = TerminalOutflowReviewer.ReviewAddresses(events, policy with { LargeOutflowRaw = "100000000000000000000" });
        var expected = new TerminalRecipientReuse { Address = "recipient", EventIds = ["bsc", "eth"] };

        var actual = TerminalOutflowReviewer.FindCrossChainRecipients(events, summaries);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    private static ChainEvent Event(string id, string from, string to, string raw, long block)
    {
        return new ChainEvent { Id = id, Chain = "BSC", Token = "token", Transaction = id, BlockHash = "block", Block = block, LogIndex = 0, From = from, To = to, RawAmount = raw, Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { LargeOutflowRaw = "50000000000000000000", Decimals = 18, DepletionBasisPoints = 9000, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["vault"] }, MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
