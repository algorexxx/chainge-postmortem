using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class FusionPurposeClassifierTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";
    private const string Holder = "0x1111111111111111111111111111111111111111";

    [Theory]
    [InlineData(true, "receipt-slice-conversion")]
    [InlineData(false, "unresolved-contract-purpose")]
    public void ClassifyEvents_SiblingReceipt_ShouldRequireVerification(bool verified, string classification)
    {
        var e = Event("mint", Zero, Holder) with { ReceiptVerified = verified, RelatedTransfers = [new() { Token = "slice", From = Holder, To = Zero, RawAmount = "100", LogIndex = 2 }] };
        var policy = Policy() with { SliceCounterparts = new Dictionary<string, string[]> { ["token"] = ["slice"] } };
        var expected = new FusionPurpose { EventId = "mint", Classification = classification, ExcludeFromExternalMatching = verified, CounterpartEventIds = verified ? ["Fusion:mint:2"] : [] };

        var actual = FusionPurposeClassifier.ClassifyEvents([e], policy);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ClassifyEvents_SplitAndUnrelatedToken_ShouldExcludeOnlyConfiguredSibling()
    {
        var e = Event("burn", Holder, Zero) with { RelatedTransfers = [new() { Token = "slice", From = Zero, To = Holder, RawAmount = "100", LogIndex = 2 }, new() { Token = "unrelated", From = Zero, To = Holder, RawAmount = "100", LogIndex = 3 }] };
        var policy = Policy() with { SliceCounterparts = new Dictionary<string, string[]> { ["token"] = ["slice"] } };
        var expected = new FusionPurpose { EventId = "burn", Classification = "receipt-slice-conversion", ExcludeFromExternalMatching = true, CounterpartEventIds = ["Fusion:burn:2"] };

        var actual = FusionPurposeClassifier.ClassifyEvents([e], policy);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Theory]
    [InlineData(true, "direct-mint-call")]
    [InlineData(false, "direct-burn-call")]
    public void ClassifyEvents_ParentCallWithExactCalldata_ShouldDescribeDirectIssuance(bool mint, string classification)
    {
        var e = Event("call", mint ? Zero : Holder, mint ? Holder : Zero) with { ReceiptTarget = "parent", TransactionInput = (mint ? "0x40c10f19" : "0x9dc29fac") + new string('0', 24) + Holder[2..] + new string('0', 62) + "64" };
        var policy = Policy() with { IssuanceCallTargets = new Dictionary<string, string[]> { ["token"] = ["parent"] } };
        var expected = new FusionPurpose { EventId = "call", Classification = classification, ExcludeFromExternalMatching = false, CounterpartEventIds = [] };

        var actual = FusionPurposeClassifier.ClassifyEvents([e], policy);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ClassifyEvents_WrongAmountAndConfiguredPath_ShouldRetainExternalQueue()
    {
        var e = Event("call", Zero, Holder) with { ReceiptTarget = "token", TransactionInput = "0x40c10f19" + new string('0', 24) + Holder[2..] + new string('0', 62) + "65" };
        var policy = Policy() with { ContractCallPurposes = new Dictionary<string, string> { ["token:0x40c10f19"] = "configured-call-candidate" } };
        var expected = new FusionPurpose { EventId = "call", Classification = "configured-call-candidate", ExcludeFromExternalMatching = false, CounterpartEventIds = [] };

        var actual = FusionPurposeClassifier.ClassifyEvents([e], policy);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ClassifyEvents_InventoryConversion_ShouldKeepCounterpartEvidence()
    {
        var burn = Event("burn", Holder, Zero);
        var inventory = Event("transfer", "inventory", Holder) with { Token = "new", Transaction = "burn" };
        var expected = new FusionPurpose { EventId = "burn", Classification = "cross-representation-inventory-transfer", ExcludeFromExternalMatching = true, CounterpartEventIds = ["transfer"] };

        var actual = FusionPurposeClassifier.ClassifyEvents([burn, inventory], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ClassifyEvents_UnverifiedMigrationCounterpart_ShouldRemainUnresolved()
    {
        var burn = Event("burn", Holder, Zero);
        var mint = Event("mint", Zero, Holder) with { Token = "new", Transaction = "burn", ReceiptVerified = false };
        var expected = new[] {
            new FusionPurpose { EventId = "burn", Classification = "unresolved-contract-purpose", ExcludeFromExternalMatching = false, CounterpartEventIds = [] },
            new FusionPurpose { EventId = "mint", Classification = "unresolved-contract-purpose", ExcludeFromExternalMatching = false, CounterpartEventIds = [] }
        };

        var actual = FusionPurposeClassifier.ClassifyEvents([burn, mint], Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string from, string to)
    {
        return new ChainEvent { Id = id, Chain = "Fusion", Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = "100", Decimals = 18, TimestampSource = "fixture", ReceiptVerified = true, ReceiptContextCaptured = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 18, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
