using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class EventAccountingBuilder
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static EventAccountingReport BuildReport(IReadOnlyList<ChainEvent> events, ReconciliationResult review, MatchPolicy policy)
    {
        var byId = events.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var movements = review.Movements.ToDictionary(m => m.SourceEventId, StringComparer.Ordinal);
        var expectedIds = events.Where(e => e.Chain != policy.FusionChain && policy.OperatingAddresses.TryGetValue(e.Chain, out var addresses) && (addresses.Contains(e.From) || addresses.Contains(e.To))).Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        if (!expectedIds.SetEquals(movements.Keys)) { throw new InvalidDataException("Movement coverage differs from configured operating-address events."); }
        var ordered = events.OrderBy(e => e.Chain, StringComparer.Ordinal).ThenBy(e => e.Token, StringComparer.Ordinal).ThenBy(e => e.FullTime).ToList();
        var tokens = ordered.GroupBy(e => new { e.Chain, e.Token, e.FullTime }).Select(g => BuildTokenTotals(g.ToList())).ToList();
        var totals = review.Movements.OrderBy(m => m.SourceEventId, StringComparer.Ordinal).GroupBy(m => new { byId[m.SourceEventId].Chain, byId[m.SourceEventId].Token, byId[m.SourceEventId].Decimals, byId[m.SourceEventId].FullTime, m.Direction, m.Classification })
            .Select(g => new MovementTotals { Chain = g.Key.Chain, Token = g.Key.Token, Decimals = g.Key.Decimals, FullTime = g.Key.FullTime, Direction = g.Key.Direction, Classification = g.Key.Classification, Count = g.Count(), RawAmount = SumAmounts(g.Select(m => byId[m.SourceEventId])).ToString(CultureInfo.InvariantCulture) }).ToList();
        return new EventAccountingReport { Tokens = tokens, Movements = totals, Addresses = BuildAddressTotals(ordered.Where(e => expectedIds.Contains(e.Id)).ToList(), policy) };
    }

    private static TokenEventTotals BuildTokenTotals(List<ChainEvent> events)
    {
        var first = events[0];
        if (events.Any(e => e.Decimals != first.Decimals || e.Decimals is < 0 or > 255)) { throw new InvalidDataException("Inconsistent token precision."); }
        _ = SumAmounts(events);
        var minted = SumAmounts(events.Where(e => e.From == Zero));
        var burned = SumAmounts(events.Where(e => e.To == Zero));
        return new TokenEventTotals { Chain = first.Chain, Token = first.Token, Decimals = first.Decimals, FullTime = first.FullTime, EventCount = events.Count, UnverifiedCount = events.Count(e => !e.ReceiptVerified), MissingTimestampCount = events.Count(e => e.Timestamp is null), MintedRaw = minted.ToString(CultureInfo.InvariantCulture), BurnedRaw = burned.ToString(CultureInfo.InvariantCulture), ObservedNetIssuanceRaw = (minted - burned).ToString(CultureInfo.InvariantCulture) };
    }

    private static BigInteger SumAmounts(IEnumerable<ChainEvent> events)
    {
        var total = BigInteger.Zero;
        foreach (var e in events)
        {
            var amount = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
            if (amount < 0) { throw new InvalidDataException("Negative event amount."); }
            total += amount;
        }
        return total;
    }

    private static List<AddressFlowTotals> BuildAddressTotals(List<ChainEvent> events, MatchPolicy policy)
    {
        var result = new List<AddressFlowTotals>();
        foreach (var group in events.GroupBy(e => new { e.Chain, e.Token, e.Decimals, e.FullTime }))
        {
            foreach (var address in policy.OperatingAddresses[group.Key.Chain].Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                var incoming = SumAmounts(group.Where(e => e.To == address));
                var outgoing = SumAmounts(group.Where(e => e.From == address));
                result.Add(new AddressFlowTotals { Chain = group.Key.Chain, Token = group.Key.Token, Decimals = group.Key.Decimals, FullTime = group.Key.FullTime, Address = address, InflowRaw = incoming.ToString(CultureInfo.InvariantCulture), OutflowRaw = outgoing.ToString(CultureInfo.InvariantCulture), ObservedNetFlowRaw = (incoming - outgoing).ToString(CultureInfo.InvariantCulture) });
            }
        }
        return result;
    }
}
