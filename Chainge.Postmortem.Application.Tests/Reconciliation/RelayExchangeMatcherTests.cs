using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class RelayExchangeMatcherTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FindExchanges_SingleRelay_ShouldPreserveBothTransferIds(bool outgoing)
    {
        var movement = Event("vault-leg", outgoing ? "vault" : "relay", outgoing ? "relay" : "vault", outgoing ? 0 : 4);
        var pool = Event("pool-leg", outgoing ? "relay" : "pool", outgoing ? "pool" : "relay", 1) with { PoolSwaps = [new() { Pool = "pool", Recipient = outgoing ? "other" : "relay", LogIndex = 3, Input0Raw = outgoing ? "100" : "7", Input1Raw = "0", Output0Raw = "0", Output1Raw = outgoing ? "7" : "100" }], RelatedTransfers = [new() { Token = "counter", From = outgoing ? "pool" : "other", To = outgoing ? "other" : "pool", RawAmount = "7", LogIndex = 2 }] };
        var expected = new[] { new ReceiptExchange { Rule = "single-relay-pool-swap", RouteEventIds = outgoing ? ["vault-leg", "pool-leg"] : ["pool-leg", "vault-leg"], Emitter = "pool", LogIndex = 3, Sender = outgoing ? "vault" : "other", Recipient = outgoing ? "other" : "vault", SourceToken = outgoing ? "token" : "counter", DestinationToken = outgoing ? "counter" : "token", SpentRaw = outgoing ? "100" : "7", ReturnedRaw = outgoing ? "7" : "100" } };

        var actual = RelayExchangeMatcher.FindExchanges(movement, [movement, pool], outgoing);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData(false, "100", 2)]
    [InlineData(true, "0", 2)]
    [InlineData(true, "100", 1)]
    [InlineData(true, "100", 3)]
    public void FindExchanges_MissingOrAmbiguousRoute_ShouldReject(bool verified, string amount, int legCount)
    {
        var movement = Event("vault-leg", "relay", "vault", 4) with { ReceiptVerified = verified, RawAmount = amount };
        var legs = new[] { movement, Event("pool-leg", "pool", "relay", 1), Event("extra", "other", "relay", 2) }.Take(legCount).ToList();
        ReceiptExchange[] expected = [];

        var actual = RelayExchangeMatcher.FindExchanges(movement, legs, false);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void FindExchanges_WrongLogOrder_ShouldReject()
    {
        var movement = Event("vault-leg", "relay", "vault", 4);
        var pool = Event("pool-leg", "pool", "relay", 5);
        ReceiptExchange[] expected = [];

        var actual = RelayExchangeMatcher.FindExchanges(movement, [movement, pool], false);

        Assert.Equivalent(expected, actual, strict: true);
    }

    private static ChainEvent Event(string id, string from, string to, long index)
    {
        return new ChainEvent { Id = id, Chain = "A", Token = "token", Transaction = "tx", BlockHash = "block", Block = 1, LogIndex = index, From = from, To = to, RawAmount = "100", Decimals = 18, TimestampSource = "fixture", ReceiptVerified = true, ReceiptContextCaptured = true, FullTime = true, Evidence = ["fixture"] };
    }
}
