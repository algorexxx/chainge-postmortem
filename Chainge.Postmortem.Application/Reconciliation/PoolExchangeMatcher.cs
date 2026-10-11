using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class PoolExchangeMatcher
{
    public static IReadOnlyList<ReceiptExchange> FindExchanges(ChainEvent e, bool outgoing)
    {
        if (!e.ReceiptVerified || !e.ReceiptContextCaptured || e.RawAmount == "0") { return []; }
        var swaps = e.PoolSwaps.Where(s => s.Pool == (outgoing ? e.To : e.From)).OrderBy(s => s.LogIndex).ToList();
        var swap = swaps.FirstOrDefault(s => s.LogIndex > e.LogIndex);
        if (swap is null) { return []; }
        var previous = swaps.Where(s => s.LogIndex < swap.LogIndex).Select(s => s.LogIndex).DefaultIfEmpty(-1).Max();
        var result = new List<ReceiptExchange>();
        foreach (var transfer in e.RelatedTransfers.Where(t => t.Token != e.Token && t.RawAmount != "0" && t.LogIndex > previous && t.LogIndex < swap.LogIndex))
        {
            var endpointMatches = outgoing ? transfer.From == swap.Pool && transfer.To == swap.Recipient : transfer.To == swap.Pool && e.To == swap.Recipient;
            if (!endpointMatches || !MatchesAmounts(swap, outgoing ? e.RawAmount : transfer.RawAmount, outgoing ? transfer.RawAmount : e.RawAmount)) { continue; }
            result.Add(new ReceiptExchange { Rule = "pool-swap-with-transfer-legs", Emitter = swap.Pool, LogIndex = swap.LogIndex, Sender = outgoing ? e.From : transfer.From, Recipient = outgoing ? transfer.To : e.To, SourceToken = outgoing ? e.Token : transfer.Token, DestinationToken = outgoing ? transfer.Token : e.Token, SpentRaw = outgoing ? e.RawAmount : transfer.RawAmount, ReturnedRaw = outgoing ? transfer.RawAmount : e.RawAmount });
        }
        return result.Count == 1 ? result : [];
    }

    private static bool MatchesAmounts(ReceiptPoolSwap swap, string input, string output)
    {
        return swap.Input0Raw == input && swap.Input1Raw == "0" && swap.Output0Raw == "0" && swap.Output1Raw == output
            || swap.Input0Raw == "0" && swap.Input1Raw == input && swap.Output0Raw == output && swap.Output1Raw == "0";
    }
}
