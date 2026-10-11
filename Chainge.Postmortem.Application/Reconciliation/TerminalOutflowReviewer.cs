using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class TerminalOutflowReviewer
{
    public static IReadOnlyList<TerminalOutflowSummary> ReviewAddresses(IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        var threshold = BigInteger.Parse(policy.LargeOutflowRaw, CultureInfo.InvariantCulture) * BigInteger.Pow(10, 255 - policy.Decimals);
        var output = new List<TerminalOutflowSummary>();
        foreach (var chain in policy.OperatingAddresses)
        {
            foreach (var address in chain.Value)
            {
                var histories = events.Where(e => e.Chain == chain.Key && (e.From == address || e.To == address)).GroupBy(e => new { e.Token, e.Decimals });
                foreach (var history in histories)
                {
                    output.Add(ReviewHistory(history.OrderBy(e => e.Block).ThenBy(e => e.LogIndex).ToList(), address, threshold));
                }
            }
        }
        return output;
    }

    private static TerminalOutflowSummary ReviewHistory(List<ChainEvent> history, string address, BigInteger threshold)
    {
        var outflows = history.Where(e => e.From == address && e.To != address && e.GetComparableUnits() > 0).ToList();
        var substantial = outflows.LastOrDefault(e => e.GetComparableUnits() >= threshold);
        var last = outflows.TakeLast(10).ToList();
        var anchor = substantial ?? last.FirstOrDefault();
        var balance = BigInteger.Zero;
        BigInteger? opening = null;
        var inflow = BigInteger.Zero;
        var outflow = BigInteger.Zero;
        var inflowCount = 0;
        var consistent = true;
        var sequence = new List<string>();
        foreach (var e in history)
        {
            if (e.Id == anchor?.Id) { opening = balance; }
            var raw = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
            var incoming = e.To == address && e.From != address;
            var outgoing = e.From == address && e.To != address;
            balance += incoming ? raw : outgoing ? -raw : BigInteger.Zero;
            consistent &= balance >= 0;
            if (opening is null) { continue; }
            sequence.Add(e.Id);
            if (incoming && raw > 0) { inflow += raw; inflowCount++; }
            if (outgoing && raw > 0) { outflow += raw; }
        }
        return new TerminalOutflowSummary
        {
            Chain = history[0].Chain, Token = history[0].Token, Address = address, Decimals = history[0].Decimals,
            FirstEventId = history[0].Id, LastEventId = history[^1].Id,
            FinalOutflowId = outflows.LastOrDefault()?.Id, LastSubstantialOutflowId = substantial?.Id,
            LastOutflowIds = last.Select(e => e.Id).ToList(), SequenceEventIds = sequence,
            SequenceOpeningBalanceRaw = opening?.ToString(CultureInfo.InvariantCulture), ClosingBalanceRaw = balance.ToString(CultureInfo.InvariantCulture),
            SequenceInflowRaw = inflow.ToString(CultureInfo.InvariantCulture), SequenceOutflowRaw = outflow.ToString(CultureInfo.InvariantCulture), SequenceInflowCount = inflowCount,
            NetReductionBasisPoints = GetReduction(opening, balance, consistent), NonnegativeHistory = consistent, AllReceiptsVerified = history.All(e => e.ReceiptVerified)
        };
    }

    private static int? GetReduction(BigInteger? opening, BigInteger closing, bool consistent)
    {
        if (!consistent || opening is null || opening <= 0 || closing > opening) { return null; }
        return (int)((opening.Value - closing) * 10000 / opening.Value);
    }

    public static IReadOnlyList<TerminalDestination> FindSharedDestinations(IReadOnlyList<ChainEvent> events, IReadOnlyList<TerminalOutflowSummary> summaries)
    {
        return GetTerminalPayments(events, summaries).GroupBy(e => new { e.Chain, e.Token, e.To })
            .Where(g => g.Select(e => e.From).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => new TerminalDestination { Chain = g.Key.Chain, Token = g.Key.Token, Address = g.Key.To, SourceAddresses = g.Select(e => e.From).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(), EventIds = g.Select(e => e.Id).ToList() }).ToList();
    }

    private static List<ChainEvent> GetTerminalPayments(IReadOnlyList<ChainEvent> events, IReadOnlyList<TerminalOutflowSummary> summaries)
    {
        var byId = events.ToDictionary(e => e.Id, StringComparer.Ordinal);
        return summaries.SelectMany(s => s.SequenceEventIds.Select(id => byId[id]).Where(e => e.From == s.Address && e.To != s.Address && e.GetComparableUnits() > 0)).DistinctBy(e => e.Id).ToList();
    }

    public static IReadOnlyList<TerminalRecipientReuse> FindCrossChainRecipients(IReadOnlyList<ChainEvent> events, IReadOnlyList<TerminalOutflowSummary> summaries)
    {
        return GetTerminalPayments(events, summaries).GroupBy(e => e.To)
            .Where(g => g.Select(e => e.Chain).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => new TerminalRecipientReuse { Address = g.Key, EventIds = g.Select(e => e.Id).Order(StringComparer.Ordinal).ToList() }).ToList();
    }
}
