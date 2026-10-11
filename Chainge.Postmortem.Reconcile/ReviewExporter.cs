using System.Globalization;
using System.Numerics;
using System.Text;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Reconcile;

public static class ReviewExporter
{
    public static async Task WriteReviewFiles(string output, IReadOnlyList<ChainEvent> events, ReconciliationResult result, CancellationToken cancellationToken)
    {
        var byId = events.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var movements = new StringBuilder("event_id,chain,token,decimals,utc,from,to,direction,amount,raw_amount,classification,candidate_count,source_candidate_count,flags,balance_before_raw,balance_after_raw,receipt_verified,evidence\n");
        var exchanges = new StringBuilder("source_event_id,chain,transaction,rule,emitter,swap_log_index,sender,source_token,destination_token,recipient,spent_raw,returned_raw,route_event_ids,evidence\n");
        var sourcePairs = new StringBuilder("outgoing_event_id,incoming_event_id,delay,fee_raw_hypothesis,competing\n");
        var pairs = new StringBuilder("source_event_id,fusion_event_id,source_amount,fusion_amount,delay,fee_raw_hypothesis,rule,competing,fusion_receipt_verified,fusion_evidence\n");
        foreach (var movement in result.Movements)
        {
            var e = byId[movement.SourceEventId];
            AppendRow(movements, [e.Id, e.Chain, e.Token, e.Decimals.ToString(CultureInfo.InvariantCulture), e.Timestamp?.ToString("O", CultureInfo.InvariantCulture) ?? "", e.From, e.To, movement.Direction, FormatAmount(e), e.RawAmount, movement.Classification, movement.Candidates.Count.ToString(CultureInfo.InvariantCulture), movement.SourceCandidates.Count.ToString(CultureInfo.InvariantCulture), string.Join(";", movement.Flags), movement.ReconstructedBalanceBeforeRaw ?? "", movement.ReconstructedBalanceAfterRaw ?? "", e.ReceiptVerified.ToString(), string.Join(";", e.Evidence)]);
            foreach (var exchange in movement.ExchangeEvidence)
            {
                AppendRow(exchanges, [e.Id, e.Chain, e.Transaction, exchange.Rule, exchange.Emitter, exchange.LogIndex.ToString(CultureInfo.InvariantCulture), exchange.Sender, exchange.SourceToken, exchange.DestinationToken, exchange.Recipient, exchange.SpentRaw, exchange.ReturnedRaw, string.Join(";", exchange.RouteEventIds), string.Join(";", e.Evidence)]);
            }
            if (movement.Direction == "outgoing")
            {
                foreach (var candidate in movement.SourceCandidates)
                {
                    AppendRow(sourcePairs, [e.Id, candidate.OtherEventId, candidate.Delay.ToString(), candidate.FeeRawHypothesis, candidate.Competing.ToString()]);
                }
            }
            foreach (var candidate in movement.Candidates)
            {
                var f = byId[candidate.FusionEventId];
                AppendRow(pairs, [e.Id, f.Id, FormatAmount(e), FormatAmount(f), candidate.Delay.ToString(), candidate.FeeRawHypothesis, candidate.Rule, candidate.Competing.ToString(), f.ReceiptVerified.ToString(), string.Join(";", f.Evidence)]);
            }
        }
        await File.WriteAllTextAsync(Path.Combine(output, "movements.csv"), movements.ToString(), Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(output, "exchange-evidence.csv"), exchanges.ToString(), Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(output, "source-chain-pairs.csv"), sourcePairs.ToString(), Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(output, "candidate-pairs.csv"), pairs.ToString(), Encoding.UTF8, cancellationToken);
    }

    private static void AppendRow(StringBuilder target, string[] fields)
    {
        target.AppendLine(string.Join(",", fields.Select(field => "\"" + field.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"")));
    }

    private static string FormatAmount(ChainEvent e)
    {
        var raw = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
        var scale = BigInteger.Pow(10, e.Decimals);
        return e.Decimals == 0 ? raw.ToString(CultureInfo.InvariantCulture) : (raw / scale).ToString(CultureInfo.InvariantCulture) + "." + (raw % scale).ToString(CultureInfo.InvariantCulture).PadLeft(e.Decimals, '0');
    }
}
