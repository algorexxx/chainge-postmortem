using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class RelayExchangeMatcher
{
    public static IReadOnlyList<ReceiptExchange> FindExchanges(ChainEvent e, IReadOnlyList<ChainEvent> transaction, bool outgoing)
    {
        if (!e.ReceiptVerified || e.RawAmount == "0") { return []; }
        var relay = outgoing ? e.To : e.From;
        var legs = transaction.Where(t => t.Chain == e.Chain && t.Transaction == e.Transaction && t.Token == e.Token && t.RawAmount != "0" && (t.From == relay || t.To == relay)).ToList();
        if (legs.Count != 2 || legs.Any(t => !t.ReceiptVerified)) { return []; }
        var result = new List<ReceiptExchange>();
        foreach (var leg in legs.Where(t => t.Id != e.Id && t.RawAmount == e.RawAmount && t.Decimals == e.Decimals))
        {
            var connected = outgoing ? leg.From == relay && leg.LogIndex > e.LogIndex : leg.To == relay && leg.LogIndex < e.LogIndex;
            if (!connected) { continue; }
            result.AddRange(PoolExchangeMatcher.FindExchanges(leg, outgoing).Select(s => s with { Rule = "single-relay-pool-swap", Sender = outgoing ? e.From : s.Sender, Recipient = outgoing ? s.Recipient : e.To, RouteEventIds = outgoing ? [e.Id, leg.Id] : [leg.Id, e.Id] }));
        }
        return result;
    }
}
