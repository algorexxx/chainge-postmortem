using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class PoolExchangeMatcherTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void FindExchanges_TwoTransferLegs_ShouldRetainCounterAssetRecipient(bool outgoing, bool firstToken)
    {
        var input = CreateEvent(outgoing, firstToken);
        var expected = new[] { new ReceiptExchange { Rule = "pool-swap-with-transfer-legs", Emitter = "pool", LogIndex = 3, Sender = outgoing ? "vault" : "other", Recipient = outgoing ? "other" : "vault", SourceToken = outgoing ? "token" : "counter", DestinationToken = outgoing ? "counter" : "token", SpentRaw = outgoing ? "100" : "7", ReturnedRaw = outgoing ? "7" : "100" } };

        var actual = PoolExchangeMatcher.FindExchanges(input, outgoing);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(false, true, "100", "pool", "7")]
    [InlineData(true, false, "100", "pool", "7")]
    [InlineData(true, true, "0", "pool", "7")]
    [InlineData(true, true, "100", "unrelated", "7")]
    [InlineData(true, true, "100", "pool", "8")]
    public void FindExchanges_MissingOrUnrelatedEvidence_ShouldRemainUnmatched(bool verified, bool context, string amount, string from, string counterAmount)
    {
        var fixture = CreateEvent(true, true);
        var input = fixture with { ReceiptVerified = verified, ReceiptContextCaptured = context, RawAmount = amount, RelatedTransfers = [fixture.RelatedTransfers[0] with { From = from, RawAmount = counterAmount }] };
        ReceiptExchange[] expected = [];

        var actual = PoolExchangeMatcher.FindExchanges(input, true);

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent CreateEvent(bool outgoing, bool firstToken)
    {
        var input = outgoing ? "100" : "7";
        var output = outgoing ? "7" : "100";
        return new ChainEvent { Id = "pay", Chain = "BSC", Token = "token", Transaction = "tx", BlockHash = "block", Block = 1, LogIndex = 1, From = outgoing ? "vault" : "pool", To = outgoing ? "pool" : "vault", RawAmount = "100", Decimals = 18, TimestampSource = "fixture", ReceiptVerified = true, ReceiptContextCaptured = true, FullTime = true, Evidence = ["fixture"], RelatedTransfers = [new() { Token = "counter", From = outgoing ? "pool" : "other", To = outgoing ? "other" : "pool", RawAmount = "7", LogIndex = 2 }], PoolSwaps = [new() { Pool = "pool", Recipient = outgoing ? "other" : "vault", LogIndex = 3, Input0Raw = firstToken ? input : "0", Input1Raw = firstToken ? "0" : input, Output0Raw = firstToken ? "0" : output, Output1Raw = firstToken ? output : "0" }] };
    }
}
