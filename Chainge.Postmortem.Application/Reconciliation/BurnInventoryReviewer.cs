using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class BurnInventoryReviewer
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<BurnInventoryReview> ReviewBurns(IReadOnlyList<ChainEvent> events, ReconciliationResult reconciliation, MatchPolicy policy)
    {
        var unpaired = reconciliation.UnpairedFusionEventIds.ToHashSet(StringComparer.Ordinal);
        var burns = events.Where(e => e.Chain == policy.FusionChain && e.To == Zero && unpaired.Contains(e.Id)).ToList();
        var result = burns.ToDictionary(e => e.Id, e => new BurnInventoryReview { BurnEventId = e.Id, Classification = "holder-burn-unresolved" });
        foreach (var account in policy.FusionInventoryAccounts)
        {
            foreach (var holder in account.Value)
            {
                var history = events.Where(e => e.Chain == policy.FusionChain && e.Token == account.Key && e.FullTime && (e.From == holder || e.To == holder)).OrderBy(e => e.Block).ThenBy(e => e.LogIndex).ToList();
                ReviewInventory(history, holder, result);
            }
        }
        return result.Values.OrderBy(r => r.BurnEventId, StringComparer.Ordinal).ToList();
    }

    private static void ReviewInventory(List<ChainEvent> history, string holder, Dictionary<string, BurnInventoryReview> result)
    {
        var balance = BigInteger.Zero;
        var consistent = true;
        var inventory = new List<string>();
        foreach (var e in history)
        {
            var before = balance;
            var amount = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
            balance += (e.To == holder ? amount : BigInteger.Zero) - (e.From == holder ? amount : BigInteger.Zero);
            consistent &= balance >= 0;
            if (e.To != Zero)
            {
                inventory.Add(e.Id);
                continue;
            }
            if (result.ContainsKey(e.Id))
            {
                var clears = e.ReceiptVerified && amount > 0 && balance.IsZero && consistent;
                result[e.Id] = new BurnInventoryReview { BurnEventId = e.Id, Classification = clears ? "controller-observed-balance-clearing" : "controller-burn-unresolved", ObservedBalanceBeforeRaw = before.ToString(CultureInfo.InvariantCulture), ObservedBalanceAfterRaw = balance.ToString(CultureInfo.InvariantCulture), InventoryEventIds = inventory.ToList() };
            }
            inventory.Clear();
        }
    }
}
