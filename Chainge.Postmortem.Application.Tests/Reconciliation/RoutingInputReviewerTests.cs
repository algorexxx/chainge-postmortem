using System.Globalization;
using System.Text;
using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class RoutingInputReviewerTests
{
    private const string Token = "0xfc9e57d7543e0591824121fda6461fe2148a5966";
    private const string Holder = "0xf11b4db13148d56e2a10ad701ad0ff338c62c52c";
    private const string Target = "0x3668c219b1fa8fe8175158f6ce91ded36fde9152";
    private const string Recipient = "0x35cba6ab1f851cdbf9accf17aa7fd98679bf3e3f";
    private const string DestinationToken = "0xaf88d065e77c8cc2239327c5edb3a432268e5831";
    private const string CapturedInput = "0x3f163c47000000000000000000000000fc9e57d7543e0591824121fda6461fe2148a596600000000000000000000000000000000000000000000000000000060ed886780000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000000000000000000000000000000000000800000000000000000000000000000000000000000000000000000000000000090636861696e676561707076313a4152423a3078616638386430363565373763386363323233393332376335656462336134333232363865353833313a3078333563626136616231663835316364626639616363663137616137666439383637396266336533663a303a315f3431363035323131383830363b335f323b345f3431363035323131383830363b355f41524200000000000000000000000000000000";

    [Fact]
    public void ReviewInputs_CapturedBurn_ShouldDecodeOnlyExactDestinationCandidates()
    {
        var burn = Burn();
        var payment = burn with { Id = "payment", Chain = "Arbitrum", Token = DestinationToken, From = "vault", To = Recipient, RawAmount = "416052118806", Timestamp = burn.Timestamp!.Value.AddMinutes(2) };
        var events = new[] { burn, payment, payment with { Id = "wrong-chain", Chain = "Ethereum" }, payment with { Id = "wrong-recipient", To = Holder }, payment with { Id = "wrong-token", Token = Token }, payment with { Id = "wrong-amount", RawAmount = "416050404450" }, payment with { Id = "unverified", ReceiptVerified = false }, payment with { Id = "early", Timestamp = burn.Timestamp!.Value.AddSeconds(-1) }, payment with { Id = "late", Timestamp = burn.Timestamp!.Value.AddDays(8) } };
        var expected = new RoutingObservation { EventId = "burn", Status = "observed-colon-memo", ChainCode = "ARB", Token = DestinationToken, Recipient = Recipient, MemoAmountRaw = "416052118806", CandidateEventIds = ["payment"] };

        var actual = RoutingInputReviewer.ReviewInputs(events, Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("bad-hex")]
    [InlineData("wrong-amount")]
    [InlineData("wrong-token")]
    [InlineData("wrong-offset")]
    [InlineData("wrong-length")]
    [InlineData("padding")]
    [InlineData("non-ascii")]
    [InlineData("sender")]
    [InlineData("unverified")]
    public void ReviewInputs_InvalidBindingOrEncoding_ShouldRetainRejectedObservation(string mutation)
    {
        var burn = MutateBurn(mutation);
        var expected = new RoutingObservation { EventId = "burn", Status = "invalid-or-unbound-call" };

        var actual = RoutingInputReviewer.ReviewInputs([burn], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    private static ChainEvent MutateBurn(string mutation)
    {
        var burn = Burn();
        return mutation switch
        {
            "short" => burn with { TransactionInput = "0x3f163c47" },
            "bad-hex" => burn with { TransactionInput = CapturedInput[..^1] + "z" },
            "wrong-amount" => burn with { RawAmount = "1" },
            "wrong-token" => burn with { Token = DestinationToken },
            "wrong-offset" => burn with { TransactionInput = CapturedInput[..202] + new string('0', 64) + CapturedInput[266..] },
            "wrong-length" => burn with { TransactionInput = CapturedInput[..266] + new string('f', 64) + CapturedInput[330..] },
            "padding" => burn with { TransactionInput = CapturedInput[..^1] + "1" },
            "non-ascii" => burn with { TransactionInput = EncodeMemo("\u0001") },
            "sender" => burn with { TransactionSender = Recipient },
            _ => burn with { ReceiptVerified = false }
        };
    }

    [Theory]
    [InlineData("{}", "unsupported-json-memo")]
    [InlineData("other", "unsupported-memo")]
    public void ReviewInputs_UnsupportedMemo_ShouldKeepExplicitStatus(string memo, string status)
    {
        var burn = Burn() with { TransactionInput = EncodeMemo(memo) };
        var expected = new RoutingObservation { EventId = "burn", Status = status };

        var actual = RoutingInputReviewer.ReviewInputs([burn], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Theory]
    [InlineData("1_bad", null)]
    [InlineData("1_2;1_3", null)]
    [InlineData("", null)]
    [InlineData("1_0", "0")]
    public void ReviewInputs_AmbiguousOrZeroMemoAmount_ShouldNotProposeSettlement(string extra, string? amount)
    {
        var burn = Burn() with { TransactionInput = EncodeMemo("chaingeappv1:ARB:token:recipient:0:" + extra) };
        var expected = new RoutingObservation { EventId = "burn", Status = "observed-colon-memo", ChainCode = "ARB", Token = "token", Recipient = "recipient", MemoAmountRaw = amount };

        var actual = RoutingInputReviewer.ReviewInputs([burn], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewInputs_MissingTimestamp_ShouldKeepMemoWithoutCandidates()
    {
        var burn = Burn() with { Timestamp = null };
        var expected = new RoutingObservation { EventId = "burn", Status = "observed-colon-memo", ChainCode = "ARB", Token = DestinationToken, Recipient = Recipient, MemoAmountRaw = "416052118806" };

        var actual = RoutingInputReviewer.ReviewInputs([burn], Policy());

        Assert.Equivalent(new[] { expected }, actual, strict: true);
    }

    [Fact]
    public void ReviewInputs_UnknownTarget_ShouldIgnoreCall()
    {
        var burn = Burn() with { ReceiptTarget = "other" };
        RoutingObservation[] expected = [];

        var actual = RoutingInputReviewer.ReviewInputs([burn], Policy());

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static string EncodeMemo(string memo)
    {
        var bytes = Encoding.ASCII.GetBytes(memo);
        return CapturedInput[..266] + bytes.Length.ToString("x64", CultureInfo.InvariantCulture) + Convert.ToHexStringLower(bytes).PadRight((bytes.Length + 31) / 32 * 64, '0');
    }

    private static ChainEvent Burn()
    {
        return new ChainEvent { Id = "burn", Chain = "Fusion", Token = Token, Transaction = "0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802", BlockHash = "block", Block = 1, LogIndex = 0, From = Holder, To = "0x0000000000000000000000000000000000000000", RawAmount = "416302000000", Decimals = 6, Timestamp = DateTimeOffset.Parse("2024-09-01T22:15:40Z", CultureInfo.InvariantCulture), TimestampSource = "fixture", ReceiptVerified = true, ReceiptContextCaptured = true, FullTime = true, Evidence = ["retained-node-receipt"], TransactionInput = CapturedInput, TransactionSender = Holder, ReceiptTarget = Target };
    }

    private static MatchPolicy Policy()
    {
        return new MatchPolicy { RoutingCallTargets = [Target], RoutingChainNames = new Dictionary<string, string> { ["ARB"] = "Arbitrum" }, RoutingReviewMaximumDelay = TimeSpan.FromDays(7), LargeOutflowRaw = "10000", DepletionBasisPoints = 9000, Decimals = 6, FusionChain = "Fusion", OperatingAddresses = new Dictionary<string, string[]>(), MaximumDelay = TimeSpan.FromMinutes(5), ClockTolerance = TimeSpan.Zero, RedemptionFeeRawHypotheses = ["0"] };
    }
}
