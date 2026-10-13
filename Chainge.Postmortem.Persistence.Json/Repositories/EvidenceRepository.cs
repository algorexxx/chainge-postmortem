using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Persistence.Json.Models;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class EvidenceRepository
{
    private const string Transfer = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
    private const string TimedTransfer = "0xc817d75deaa278e988c41cf549ef38fe195ab65e0e64cd67ba47f0d185690d58";
    private readonly EvidenceSettings settings;

    public EvidenceRepository(EvidenceSettings settings)
    {
        this.settings = settings;
    }

    public async Task<EvidenceSnapshot> ReadEvidenceAsync(string root, CancellationToken cancellationToken)
    {
        var events = new Dictionary<string, ChainEvent>(StringComparer.Ordinal);
        var calls = new Dictionary<string, TransactionCall>(StringComparer.Ordinal);
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var diagnostics = new List<string>();
        var timestamps = new Dictionary<string, TimestampObservation>(StringComparer.Ordinal);
        foreach (var selection in settings.Sources)
        {
            var files = Directory.GetFiles(Path.Combine(root, selection.Directory), selection.Pattern).Where(f => !f.EndsWith(".capture.json", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToList();
            if (files.Count == 0) { throw new InvalidDataException($"No evidence files matched {selection.Directory}/{selection.Pattern}"); }
            foreach (var file in files)
            {
                var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
                var source = Path.GetRelativePath(root, file).Replace('\\', '/');
                hashes[source] = Convert.ToHexStringLower(SHA256.HashData(bytes));
                var text = System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
                if (!text.StartsWith('{') && !text.StartsWith('['))
                {
                    diagnostics.Add($"{source}: non-JSON failure response excluded");
                    continue;
                }
                using var document = JsonDocument.Parse(text);
                ReadElement(document.RootElement, selection, source, false, events, timestamps, calls, diagnostics);
            }
        }
        return new EvidenceSnapshot
        {
            Events = events.Values.Select(e => AttachTransaction(e, calls)).Select(e => timestamps.TryGetValue(e.Chain + ":" + e.BlockHash, out var timestamp) ? e with { Timestamp = timestamp.Value, TimestampSource = timestamp.Source } : e)
                .OrderBy(e => e.Chain, StringComparer.Ordinal).ThenBy(e => e.Block).ThenBy(e => e.LogIndex).ToList(),
            InputHashes = hashes,
            Diagnostics = diagnostics
        };
    }

    private static void ReadElement(JsonElement element, EvidenceSelection selection, string source, bool verified, Dictionary<string, ChainEvent> events, Dictionary<string, TimestampObservation> timestamps, Dictionary<string, TransactionCall> calls, List<string> diagnostics)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray()) { ReadElement(child, selection, source, verified, events, timestamps, calls, diagnostics); }
            return;
        }
        if (element.ValueKind != JsonValueKind.Object) { return; }
        if (element.TryGetProperty("items", out var items) && element.TryGetProperty("next_page_params", out _))
        {
            foreach (var item in items.EnumerateArray()) { ReadExplorerTransfer(item, selection, source, events, timestamps); }
            return;
        }
        if (element.TryGetProperty("error", out _) || element.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Null)
        {
            diagnostics.Add($"{source}: failed/null response {GetText(element, "id")}");
            return;
        }
        if (element.TryGetProperty("logs", out _) && GetText(element, "status") != "0x1")
        {
            diagnostics.Add($"{source}: unsuccessful receipt {GetText(element, "transactionHash")}");
            return;
        }
        ReadTransaction(element, selection.Chain, source, calls);
        ReadTimestamp(element, selection.Chain, source, verified, timestamps, diagnostics);
        if (element.TryGetProperty("topics", out _) && selection.TokenDecimals.ContainsKey(GetText(element, "address")))
        {
            ReadLog(element, selection, source, verified, events);
        }
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name is "result" or "logs") { ReadElement(property.Value, selection, source, property.Name == "logs" || verified, events, timestamps, calls, diagnostics); }
        }
        if (element.TryGetProperty("logs", out var logs)) { AttachReceiptContext(logs, selection.Chain, GetText(element, "to"), events); }
    }

    private static void ReadExplorerTransfer(JsonElement item, EvidenceSelection selection, string source, Dictionary<string, ChainEvent> events, Dictionary<string, TimestampObservation> timestamps)
    {
        var token = GetText(item.GetProperty("token"), "address_hash");
        if (!selection.TokenDecimals.TryGetValue(token, out var decimals)) { return; }
        if (GetText(item, "token_type") != "erc-20" || item.GetProperty("total").GetProperty("decimals").ToString() != decimals.ToString(CultureInfo.InvariantCulture)) { throw new InvalidDataException($"Unexpected explorer token type or precision: {source}"); }
        var transaction = GetText(item, "transaction_hash");
        var logIndex = item.GetProperty("log_index").GetInt64();
        var rawAmount = BigInteger.Parse(GetText(item.GetProperty("total"), "value"), CultureInfo.InvariantCulture);
        if (logIndex < 0 || rawAmount < 0) { throw new InvalidDataException($"Invalid explorer transfer: {source}"); }
        var e = new ChainEvent
        {
            Id = $"{selection.Chain}:{transaction}:{logIndex}", Chain = selection.Chain, Token = token, Transaction = transaction,
            BlockHash = GetText(item, "block_hash"), Block = item.GetProperty("block_number").GetInt64(), LogIndex = logIndex,
            Decimals = decimals, From = GetText(item.GetProperty("from"), "hash"), To = GetText(item.GetProperty("to"), "hash"), RawAmount = rawAmount.ToString(CultureInfo.InvariantCulture),
            FullTime = !selection.DatedTokens.Contains(token, StringComparer.OrdinalIgnoreCase), ReceiptVerified = false, TimestampSource = "missing", Evidence = [source]
        };
        StoreEvent(e, source, events);
        var key = selection.Chain + ":" + e.BlockHash;
        var timestamp = DateTimeOffset.Parse(item.GetProperty("timestamp").GetString()!, CultureInfo.InvariantCulture);
        if (!timestamps.TryGetValue(key, out var previous)) { timestamps[key] = new TimestampObservation { Value = timestamp, Source = source, Node = false }; }
        else if (!previous.Node && previous.Value != timestamp) { timestamps[key] = new TimestampObservation { Value = null, Source = previous.Source + "; " + source, Node = false }; }
    }

    private static void ReadTransaction(JsonElement element, string chain, string source, Dictionary<string, TransactionCall> calls)
    {
        if (!element.TryGetProperty("input", out _) || !element.TryGetProperty("hash", out _) || !element.TryGetProperty("r", out _) || !element.TryGetProperty("s", out _)) { return; }
        var key = chain + ":" + GetText(element, "hash");
        var call = new TransactionCall { Target = GetText(element, "to"), Sender = GetText(element, "from"), Input = GetText(element, "input"), BlockHash = GetText(element, "blockHash"), Evidence = source };
        if (calls.TryGetValue(key, out var previous) && previous with { Evidence = source } != call) { throw new InvalidDataException($"Conflicting transaction: {key}"); }
        calls[key] = call;
    }

    private static ChainEvent AttachTransaction(ChainEvent e, Dictionary<string, TransactionCall> calls)
    {
        if (!calls.TryGetValue(e.Chain + ":" + e.Transaction, out var call)) { return e; }
        if (call.BlockHash != e.BlockHash || e.ReceiptContextCaptured && call.Target != e.ReceiptTarget) { throw new InvalidDataException($"Transaction/receipt disagreement: {e.Id}"); }
        return e with { TransactionInput = call.Input, TransactionSender = call.Sender, Evidence = e.Evidence.Append(call.Evidence).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList() };
    }

    private static void AttachReceiptContext(JsonElement logs, string chain, string target, Dictionary<string, ChainEvent> events)
    {
        var exchanges = ReadReportedExchanges(logs);
        var swaps = ReadPoolSwaps(logs);
        var transfers = new List<ReceiptTransfer>();
        var ids = new List<string>();
        foreach (var log in logs.EnumerateArray())
        {
            var topics = log.GetProperty("topics").EnumerateArray().Select(t => t.GetString()!.ToLowerInvariant()).ToList();
            var data = GetText(log, "data");
            if (topics.Count == 3 && topics[0] == TimedTransfer) { ids.Add($"{chain}:{GetText(log, "transactionHash")}:{(long)ReadInteger(GetText(log, "logIndex"))}"); }
            if (topics.Count != 3 || topics[0] != Transfer || data.Length != 66) { continue; }
            if (log.TryGetProperty("removed", out var removed) && removed.GetBoolean()) { throw new InvalidDataException("Removed receipt-context log"); }
            var index = (long)ReadInteger(GetText(log, "logIndex"));
            transfers.Add(new ReceiptTransfer { Token = GetText(log, "address"), From = "0x" + topics[1][^40..], To = "0x" + topics[2][^40..], RawAmount = ReadInteger(data).ToString(CultureInfo.InvariantCulture), LogIndex = index });
            ids.Add($"{chain}:{GetText(log, "transactionHash")}:{index}");
        }
        foreach (var id in ids.Where(events.ContainsKey))
        {
            var e = events[id];
            var related = transfers.Where(t => t.Token != e.Token).OrderBy(t => t.LogIndex).ToList();
            if (e.ReceiptContextCaptured && (e.ReceiptTarget != target || !e.RelatedTransfers.SequenceEqual(related) || !e.ReportedExchanges.SequenceEqual(exchanges) || !e.PoolSwaps.SequenceEqual(swaps))) { throw new InvalidDataException($"Conflicting receipt context: {id}"); }
            events[id] = e with { ReceiptTarget = target, RelatedTransfers = related, ReportedExchanges = exchanges, PoolSwaps = swaps, ReceiptContextCaptured = true };
        }
    }

    private static List<ReceiptPoolSwap> ReadPoolSwaps(JsonElement logs)
    {
        var swaps = new List<ReceiptPoolSwap>();
        foreach (var log in logs.EnumerateArray())
        {
            var topics = log.GetProperty("topics").EnumerateArray().Select(t => t.GetString()!.ToLowerInvariant()).ToList();
            if (topics.Count != 3 || topics[0] != "0xd78ad95fa46c994b6551d0da85fc275fe613ce37657fb8d5e3d130840159d822") { continue; }
            var data = GetText(log, "data");
            if (data.Length != 258 || topics[2].Length != 66 || topics[2][2..26] != new string('0', 24) || log.TryGetProperty("removed", out var removed) && removed.GetBoolean()) { throw new InvalidDataException("Invalid pool Swap receipt-context log"); }
            var words = Enumerable.Range(0, 4).Select(i => ReadInteger(string.Concat("0x", data.AsSpan(2 + i * 64, 64))).ToString(CultureInfo.InvariantCulture)).ToList();
            swaps.Add(new ReceiptPoolSwap { Pool = GetText(log, "address"), Recipient = "0x" + topics[2][26..], LogIndex = (long)ReadInteger(GetText(log, "logIndex")), Input0Raw = words[0], Input1Raw = words[1], Output0Raw = words[2], Output1Raw = words[3] });
        }
        return swaps.OrderBy(s => s.LogIndex).ToList();
    }

    private static List<ReceiptExchange> ReadReportedExchanges(JsonElement logs)
    {
        var exchanges = new List<ReceiptExchange>();
        foreach (var log in logs.EnumerateArray())
        {
            var topics = log.GetProperty("topics").EnumerateArray().Select(t => t.GetString()!.ToLowerInvariant()).ToList();
            if (topics.Count != 1 || topics[0] != "0xd6d4f5681c246c9f42c203e287975af1601f8df8035a9251f79aab5c8f09e2f8") { continue; }
            var data = GetText(log, "data");
            if (data.Length != 386 || log.TryGetProperty("removed", out var removed) && removed.GetBoolean()) { throw new InvalidDataException("Invalid Swapped receipt-context log"); }
            var words = Enumerable.Range(0, 6).Select(i => data.Substring(2 + i * 64, 64)).ToList();
            if (words.Take(4).Any(w => w[..24] != new string('0', 24))) { throw new InvalidDataException("Noncanonical Swapped address"); }
            exchanges.Add(new ReceiptExchange { Emitter = GetText(log, "address"), LogIndex = (long)ReadInteger(GetText(log, "logIndex")), Sender = "0x" + words[0][24..], SourceToken = "0x" + words[1][24..], DestinationToken = "0x" + words[2][24..], Recipient = "0x" + words[3][24..], SpentRaw = ReadInteger("0x" + words[4]).ToString(CultureInfo.InvariantCulture), ReturnedRaw = ReadInteger("0x" + words[5]).ToString(CultureInfo.InvariantCulture) });
        }
        return exchanges.OrderBy(e => e.LogIndex).ToList();
    }

    private static void ReadTimestamp(JsonElement element, string chain, string source, bool verified, Dictionary<string, TimestampObservation> timestamps, List<string> diagnostics)
    {
        var raw = GetText(element, "blockTimestamp");
        if (raw.Length == 0) { raw = GetText(element, "timeStamp"); }
        if (raw.Length == 0) { raw = GetText(element, "timestamp"); }
        if (raw.Length == 0) { return; }
        var blockHash = GetText(element, "blockHash");
        if (blockHash.Length == 0 && element.TryGetProperty("number", out _)) { blockHash = GetText(element, "hash"); }
        if (blockHash.Length == 0) { return; }
        var key = chain + ":" + blockHash;
        var timestamp = DateTimeOffset.FromUnixTimeSeconds((long)ReadInteger(raw));
        var node = verified || element.TryGetProperty("number", out _);
        var observation = new TimestampObservation { Value = timestamp, Source = source, Node = node };
        if (timestamps.TryGetValue(key, out var previous))
        {
            if (previous.Value != timestamp)
            {
                diagnostics.Add($"Timestamp disagreement {key}: {previous.Source} vs {source}; prefer node evidence, quarantine equal-priority conflicts.");
                if (previous.Node == node) { observation = observation with { Value = null, Source = previous.Source + "; " + source }; }
            }
            if (previous.Node && !node) { return; }
        }
        timestamps[key] = observation;
    }

    private static void ReadLog(JsonElement log, EvidenceSelection selection, string source, bool verified, Dictionary<string, ChainEvent> events)
    {
        var chain = selection.Chain;
        var topics = log.GetProperty("topics").EnumerateArray().Select(t => t.GetString()!.ToLowerInvariant()).ToList();
        if (topics.Count != 3 || topics[0] != Transfer && topics[0] != TimedTransfer) { return; }
        if (log.TryGetProperty("removed", out var removed) && removed.GetBoolean()) { throw new InvalidDataException($"Removed log in {source}"); }
        var data = GetText(log, "data");
        var timed = topics[0] == TimedTransfer;
        if (data.Length != (timed ? 194 : 66)) { throw new InvalidDataException($"Unexpected Transfer data length in {source}"); }
        var transaction = GetText(log, "transactionHash");
        var logIndex = (long)ReadInteger(GetText(log, "logIndex"));
        var e = new ChainEvent
        {
            Id = $"{chain}:{transaction}:{logIndex}", Chain = chain, Token = GetText(log, "address"), Transaction = transaction,
            BlockHash = GetText(log, "blockHash"), Block = (long)ReadInteger(GetText(log, "blockNumber")), LogIndex = logIndex,
            Decimals = selection.TokenDecimals[GetText(log, "address")], From = "0x" + topics[1][^40..], To = "0x" + topics[2][^40..], RawAmount = ReadInteger(data[..66]).ToString(CultureInfo.InvariantCulture),
            FullTime = !selection.DatedTokens.Contains(GetText(log, "address"), StringComparer.OrdinalIgnoreCase) && (!timed || ReadInteger(string.Concat("0x", data.AsSpan(66, 64))).IsZero && ReadInteger(string.Concat("0x", data.AsSpan(130, 64))) == ulong.MaxValue),
            ReceiptVerified = verified, TimestampSource = "missing", Evidence = [source]
        };
        StoreEvent(e, source, events);
    }

    private static void StoreEvent(ChainEvent e, string source, Dictionary<string, ChainEvent> events)
    {
        if (events.TryGetValue(e.Id, out var previous))
        {
            if (previous.Decimals != e.Decimals || previous.Token != e.Token || previous.BlockHash != e.BlockHash || previous.Block != e.Block || previous.From != e.From || previous.To != e.To || previous.RawAmount != e.RawAmount || previous.FullTime != e.FullTime)
            { throw new InvalidDataException($"Conflicting duplicate event: {e.Id}"); }
            e = e with { Evidence = previous.Evidence.Append(source).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(), ReceiptVerified = previous.ReceiptVerified || e.ReceiptVerified, ReceiptTarget = previous.ReceiptTarget, RelatedTransfers = previous.RelatedTransfers, ReportedExchanges = previous.ReportedExchanges, PoolSwaps = previous.PoolSwaps, ReceiptContextCaptured = previous.ReceiptContextCaptured };
        }
        events[e.Id] = e;
    }

    private static string GetText(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) ? property.ToString().ToLowerInvariant() : string.Empty;
    }

    private static BigInteger ReadInteger(string value)
    {
        return value.StartsWith("0x", StringComparison.Ordinal) ? BigInteger.Parse("0" + value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture) : BigInteger.Parse(value, CultureInfo.InvariantCulture);
    }
}
