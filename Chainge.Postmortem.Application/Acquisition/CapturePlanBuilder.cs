using System.Globalization;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Acquisition;

public static class CapturePlanBuilder
{
    private const string Transfer = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";

    public static IReadOnlyDictionary<string, CapturePlan> BuildPlans(IReadOnlyList<ChainEvent> events, IReadOnlyList<LogHistoryCoverage> coverage, IReadOnlyDictionary<string, CapturePolicy> policies)
    {
        var result = new SortedDictionary<string, CapturePlan>(StringComparer.Ordinal);
        foreach (var pair in policies)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pair.Value.BatchSize, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pair.Value.MaximumLogBlocks, 1);
            var selected = events.Where(e => e.Chain == pair.Key).ToList();
            var logs = BatchLogQueries(PlanLogQueries(coverage.Where(c => c.Target.Chain == pair.Key).ToList(), pair.Value), pair.Value.BatchSize);
            var receipts = PlanBatches(selected.Where(e => !e.ReceiptVerified).Select(e => e.Transaction).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(), "receipts", "eth_getTransactionReceipt", pair.Value.BatchSize);
            var headers = PlanBatches(selected.Where(e => e.Timestamp is null).Select(e => e.BlockHash).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(), "headers", "eth_getBlockByHash", pair.Value.BatchSize);
            result[pair.Key] = pair.Value.Template with { Queries = logs.Concat(receipts).Concat(headers).ToList() };
        }
        return result;
    }

    private static List<CaptureQuery> BatchLogQueries(List<CaptureQuery> queries, int batchSize)
    {
        return queries.Chunk(batchSize).Select((batch, index) => new CaptureQuery
        {
            Name = "log-batch-" + index.ToString("D6", CultureInfo.InvariantCulture),
            Request = batch.Length == 1 ? batch[0].Request : JsonSerializer.SerializeToElement(batch.Select(q => q.Request).ToList())
        }).ToList();
    }

    private static List<CaptureQuery> PlanLogQueries(List<LogHistoryCoverage> coverage, CapturePolicy policy)
    {
        var queries = new List<CaptureQuery>();
        var groups = coverage.SelectMany(c => c.MissingRanges.Select(r => new { c.Target.Token, c.Target.Holder, c.Direction, r.From, r.Through }))
            .GroupBy(r => new { r.Token, r.Direction, r.From, r.Through, Holder = policy.CombineHolderFilters ? "" : r.Holder }).OrderBy(g => g.Key.Token, StringComparer.Ordinal).ThenBy(g => g.Key.Direction, StringComparer.Ordinal).ThenBy(g => g.Key.From).ThenBy(g => g.Key.Through).ThenBy(g => g.Key.Holder, StringComparer.Ordinal);
        foreach (var group in groups)
        {
            var holders = group.Select(r => "0x" + new string('0', 24) + r.Holder[2..].ToLowerInvariant()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            object holderFilter = holders.Length == 1 ? holders[0] : holders;
            object?[] topics = group.Key.Direction == "incoming" ? [Transfer, null, holderFilter] : [Transfer, holderFilter];
            if (group.Key.From < 0 || group.Key.Through < group.Key.From || group.Key.Through == long.MaxValue) { throw new InvalidDataException("Invalid capture gap range."); }
            for (var from = group.Key.From; from <= group.Key.Through;)
            {
                var through = from + Math.Min(policy.MaximumLogBlocks - 1, group.Key.Through - from);
                var name = "logs-" + queries.Count.ToString("D6", CultureInfo.InvariantCulture);
                var filter = new { address = group.Key.Token, fromBlock = "0x" + from.ToString("x", CultureInfo.InvariantCulture), toBlock = "0x" + through.ToString("x", CultureInfo.InvariantCulture), topics };
                queries.Add(new CaptureQuery { Name = name, Request = JsonSerializer.SerializeToElement(new { jsonrpc = "2.0", id = name, method = "eth_getLogs", @params = new[] { filter } }) });
                from = through + 1;
            }
        }
        return queries;
    }

    private static List<CaptureQuery> PlanBatches(List<string> values, string prefix, string method, int batchSize)
    {
        var result = new List<CaptureQuery>();
        foreach (var batch in values.Chunk(batchSize))
        {
            var requests = batch.Select(value => new { jsonrpc = "2.0", id = value, method, @params = method == "eth_getBlockByHash" ? new object[] { value, false } : [value] }).ToList();
            result.Add(new CaptureQuery { Name = prefix + "-" + result.Count.ToString("D6", CultureInfo.InvariantCulture), Request = requests.Count == 1 ? JsonSerializer.SerializeToElement(requests[0]) : JsonSerializer.SerializeToElement(requests) });
        }
        return result;
    }
}
