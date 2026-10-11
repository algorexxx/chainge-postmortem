using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class BurnInventoryReviewerTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    [Fact]
    public void ReviewBurns_TwoClearingCycles_ShouldKeepFundingSeparateAndPreserveQueue()
    {
        var events = new[] { Event("credit", "holder", "controller", "120", 1), Event("debit", "controller", "holder", "20", 2), Event("burn", "controller", Zero, "100", 3), Event("credit2", Zero, "controller", "50", 4), Event("burn2", "controller", Zero, "50", 5), Event("other", "holder", Zero, "10", 6) };
        var result = Result(["burn", "burn2", "other"]);
        var expected = new[] {
            new BurnInventoryReview { BurnEventId = "burn", Classification = "controller-observed-balance-clearing", ObservedBalanceBeforeRaw = "100", ObservedBalanceAfterRaw = "0", InventoryEventIds = ["credit", "debit"] },
            new BurnInventoryReview { BurnEventId = "burn2", Classification = "controller-observed-balance-clearing", ObservedBalanceBeforeRaw = "50", ObservedBalanceAfterRaw = "0", InventoryEventIds = ["credit2"] },
            new BurnInventoryReview { BurnEventId = "other", Classification = "holder-burn-unresolved" }
        };
        var expectedIds = new[] { "burn", "burn2", "other" };

        var actual = BurnInventoryReviewer.ReviewBurns(events, result, Policy());

        Assert.Equivalent(expected, actual, strict: true);
        Assert.Equivalent(expectedIds, result.UnpairedFusionEventIds, strict: true);
    }

    [Theory]
    [InlineData("99", true, "1")]
    [InlineData("100", false, "0")]
    public void ReviewBurns_PartialOrUnverifiedBurn_ShouldStayUnresolved(string burned, bool verified, string after)
    {
        var events = new[] { Event("credit", Zero, "controller", "100", 1), Event("burn", "controller", Zero, burned, 2) with { ReceiptVerified = verified } };
        var expected = new BurnInventoryReview { BurnEventId = "burn", Classification = "controller-burn-unresolved", ObservedBalanceBeforeRaw = "100", ObservedBalanceAfterRaw = after, InventoryEventIds = ["credit"] };

        var actual = BurnInventoryReviewer.ReviewBurns(events, Result(["burn"]), Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewBurns_NegativeHistory_ShouldNotInferClearingEvenAfterRecovery()
    {
        var events = new[] { Event("early-burn", "controller", Zero, "10", 1), Event("credit", Zero, "controller", "110", 2), Event("burn", "controller", Zero, "100", 3) };
        var expected = new BurnInventoryReview { BurnEventId = "burn", Classification = "controller-burn-unresolved", ObservedBalanceBeforeRaw = "100", ObservedBalanceAfterRaw = "0", InventoryEventIds = ["credit"] };

        var actual = BurnInventoryReviewer.ReviewBurns(events, Result(["burn"]), Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    private static ChainEvent Event(string id, string from, string to, string amount, long block)
    {
        return new ChainEvent { Id = id, Chain = "Fusion", Token = "token", Transaction = id, BlockHash = "block", Block = block, LogIndex = 0, From = from, To = to, RawAmount = amount, Decimals = 18, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static ReconciliationResult Result(string[] ids)
    {
        return new ReconciliationResult { Movements = [], ConversionEventIds = [], UnpairedFusionEventIds = ids };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { FusionInventoryAccounts = new Dictionary<string, string[]> { ["token"] = ["controller"] }, LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 18, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
