using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class LedgerPositionBuilder
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<HolderPosition> BuildPositions(IReadOnlyList<ChainEvent> events, string chain, long cutoffBlock)
    {
        var result = new List<HolderPosition>();
        foreach (var group in events.Where(e => e.Chain == chain && e.Block <= cutoffBlock).GroupBy(e => e.Token))
        {
            var balances = new Dictionary<string, BigInteger>(StringComparer.Ordinal);
            var inconsistent = new HashSet<string>(StringComparer.Ordinal);
            var decimals = group.First().Decimals;
            if (group.Any(e => e.Decimals != decimals || e.Decimals is < 0 or > 255)) { throw new InvalidDataException("Inconsistent claim precision."); }
            foreach (var e in group.OrderBy(e => e.Block).ThenBy(e => e.LogIndex))
            {
                var amount = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
                if (amount < 0) { throw new InvalidDataException("Negative claim event amount."); }
                ApplyBalance(balances, inconsistent, e.From, -amount);
                ApplyBalance(balances, inconsistent, e.To, amount);
            }
            result.AddRange(balances.Where(p => p.Value != 0 || inconsistent.Contains(p.Key)).Select(p => new HolderPosition { Token = group.Key, Holder = p.Key, Decimals = decimals, RawAmount = p.Value.ToString(CultureInfo.InvariantCulture), IntervalStatus = "not-assessed", HistoryConsistent = !inconsistent.Contains(p.Key) }));
        }
        return result.OrderBy(p => p.Token, StringComparer.Ordinal).ThenBy(p => p.Holder, StringComparer.Ordinal).ToList();
    }

    private static void ApplyBalance(Dictionary<string, BigInteger> balances, HashSet<string> inconsistent, string holder, BigInteger delta)
    {
        if (holder == Zero) { return; }
        balances[holder] = balances.GetValueOrDefault(holder) + delta;
        if (balances[holder] < 0) { inconsistent.Add(holder); }
    }

}
