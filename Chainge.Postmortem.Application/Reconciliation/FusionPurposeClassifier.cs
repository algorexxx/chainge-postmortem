using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class FusionPurposeClassifier
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<FusionPurpose> ClassifyEvents(IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        var fusion = events.Where(e => e.Chain == policy.FusionChain).ToLookup(e => e.Transaction);
        return fusion.SelectMany(g => g.Where(e => e.From == Zero || e.To == Zero).Select(e => ClassifyEvent(e, g, policy))).OrderBy(p => p.EventId, StringComparer.Ordinal).ToList();
    }

    private static FusionPurpose ClassifyEvent(ChainEvent e, IEnumerable<ChainEvent> transaction, MatchPolicy policy)
    {
        var holder = e.From == Zero ? e.To : e.From;
        var counterparts = transaction.Where(other => e.ReceiptVerified && other.ReceiptVerified && other.Token != e.Token && other.GetComparableUnits() == e.GetComparableUnits() && other.FullTime && e.FullTime && (e.From == Zero ? other.From == holder : other.To == holder)).ToList();
        var classification = ClassifyCall(e, policy);
        var ids = counterparts.Select(c => c.Id).ToList();
        if (ids.Count > 0)
        {
            classification = counterparts.All(c => c.From == Zero || c.To == Zero) ? "cross-representation-mint-burn" : "cross-representation-inventory-transfer";
        }
        if (e.FullTime && e.ReceiptVerified && e.ReceiptContextCaptured && policy.SliceCounterparts.TryGetValue(e.Token, out var slices))
        {
            var sliceIds = e.RelatedTransfers.Where(t => slices.Contains(t.Token) && t.RawAmount == e.RawAmount && (e.From == Zero ? t.From == holder && t.To == Zero : t.From == Zero && t.To == holder)).Select(t => $"{e.Chain}:{e.Transaction}:{t.LogIndex}").ToList();
            if (sliceIds.Count > 0)
            {
                ids.AddRange(sliceIds);
                classification = "receipt-slice-conversion";
            }
        }
        return new FusionPurpose { EventId = e.Id, Classification = classification, ExcludeFromExternalMatching = ids.Count > 0, CounterpartEventIds = ids.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList() };
    }
    private static string ClassifyCall(ChainEvent e, MatchPolicy policy)
    {
        if (!e.FullTime) { return "dated-interval-event"; }
        if (!e.ReceiptVerified || !e.ReceiptContextCaptured) { return "unresolved-contract-purpose"; }
        var selector = e.TransactionInput.Length >= 10 ? e.TransactionInput[..10] : string.Empty;
        var knownTarget = e.ReceiptTarget == e.Token || policy.IssuanceCallTargets.TryGetValue(e.Token, out var targets) && targets.Contains(e.ReceiptTarget);
        var mint = e.From == Zero;
        if (knownTarget && e.TransactionInput.Length == 138 && selector == (mint ? "0x40c10f19" : "0x9dc29fac"))
        {
            var encodedHolder = e.TransactionInput.Substring(10, 64);
            var amount = BigInteger.Parse("0" + e.TransactionInput[74..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (encodedHolder[..24] == new string('0', 24) && "0x" + encodedHolder[24..] == (mint ? e.To : e.From) && amount == BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture)) { return mint ? "direct-mint-call" : "direct-burn-call"; }
        }
        return policy.ContractCallPurposes.TryGetValue(e.ReceiptTarget + ":" + selector, out var purpose) ? purpose : "unresolved-contract-purpose";
    }

}
