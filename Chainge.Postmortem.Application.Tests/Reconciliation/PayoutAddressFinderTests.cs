using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class PayoutAddressFinderTests
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    [Fact]
    public void FindCandidates_UnknownPayer_ShouldKeepDelayedCompetitionAndDiscoveryLabel()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero), Event("payment", "BSC", "unknown", "holder") };
        var discoveries = new[] { Event("explorer", "BSC", "other", "holder") with { ReceiptVerified = false } };
        var result = Result(["burn"]);
        var delayed = new[] { new MovementReview { SourceEventId = "known-payment", Direction = "outgoing", Classification = "delayed-candidate-review", Candidates = [new() { FusionEventId = "burn", Delay = TimeSpan.Zero, FeeRawHypothesis = "0", Competing = false }] } };
        var expected = new[] {
            new PayoutAddressCandidate { SourceEventId = "explorer", ExistingMovementClassification = "explorer-only-discovery", Pair = new() { FusionEventId = "burn", Delay = TimeSpan.Zero, FeeRawHypothesis = "0", Competing = true } },
            new PayoutAddressCandidate { SourceEventId = "payment", ExistingMovementClassification = "outside-operating-scope", Pair = new() { FusionEventId = "burn", Delay = TimeSpan.Zero, FeeRawHypothesis = "0", Competing = true } }
        };

        var actual = PayoutAddressFinder.FindCandidates(events, discoveries, result, delayed, Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FindCandidates_ControllerKnownPayerAndOtherChain_ShouldNotGenerateNewAddressClaims()
    {
        var events = new[] { Event("controller-burn", "Fusion", "controller", Zero), Event("burn", "Fusion", "holder", Zero), Event("controller-payment", "BSC", "unknown", "controller"), Event("known-payment", "BSC", "known", "holder"), Event("unsupported", "Other", "unknown", "holder"), Event("self", "BSC", "holder", "holder"), Event("mint", "BSC", Zero, "holder") };
        var expected = Array.Empty<PayoutAddressCandidate>();

        var actual = PayoutAddressFinder.FindCandidates(events, [], Result(["controller-burn", "burn"]), [], Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FindCandidates_SourceAlreadyClassified_ShouldPreserveItsExistingMeaning()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero), Event("payment", "BSC", "unknown", "holder") };
        var result = Result(["burn"]) with { Movements = [new() { SourceEventId = "payment", Direction = "incoming", Classification = "possible-asset-exchange", Candidates = [] }] };
        var expected = new PayoutAddressCandidate { SourceEventId = "payment", ExistingMovementClassification = "possible-asset-exchange", Pair = new() { FusionEventId = "burn", Delay = TimeSpan.Zero, FeeRawHypothesis = "0", Competing = false } };

        var actual = PayoutAddressFinder.FindCandidates(events, [], result, [], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void FindCandidates_DelayedVariableFee_ShouldUseExistingBoundedRule()
    {
        var events = new[] { Event("burn", "Fusion", "holder", Zero) with { RawAmount = "101", Decimals = 0 }, Event("payment", "BSC", "unknown", "holder") with { Decimals = 0, Timestamp = DateTimeOffset.UnixEpoch.AddMinutes(10) } };
        var policy = Policy() with { Decimals = 0, DelayedReviewMaximumDelay = TimeSpan.FromDays(1), VariableFeeRules = new Dictionary<string, VariableFeeRule> { ["BSC"] = new() { MaximumDelay = TimeSpan.FromMinutes(5), MaximumFeeRaw = "1", MaximumFeeBasisPoints = 100 } } };
        var expected = new PayoutAddressCandidate { SourceEventId = "payment", ExistingMovementClassification = "outside-operating-scope", Pair = new() { FusionEventId = "burn", Delay = TimeSpan.FromMinutes(10), FeeRawHypothesis = "1", Rule = "bounded-variable-fee", Competing = false } };

        var actual = PayoutAddressFinder.FindCandidates(events, [], Result(["burn"]), [], policy);

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    private static ChainEvent Event(string id, string chain, string from, string to)
    {
        return new ChainEvent { Id = id, Chain = chain, Token = "token", Transaction = id, BlockHash = "block", Block = 1, LogIndex = 0, From = from, To = to, RawAmount = "100", Decimals = 18, Timestamp = DateTimeOffset.UnixEpoch, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = ["fixture"] };
    }

    private static ReconciliationResult Result(string[] ids)
    {
        return new ReconciliationResult { Movements = [], ConversionEventIds = [], UnpairedFusionEventIds = ids };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { FusionInventoryAccounts = new Dictionary<string, string[]> { ["token"] = ["controller"] }, LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 18, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]> { ["BSC"] = ["known"] }, MaximumDelay = TimeSpan.FromDays(1), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
