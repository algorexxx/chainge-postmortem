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
                ReadElement(document.RootElement, selection, source, false, events, timestamps, diagnostics);
            }
        }
        return new EvidenceSnapshot
        {
            Events = events.Values.Select(e => timestamps.TryGetValue(e.Chain + ":" + e.BlockHash, out var timestamp) ? e with { Timestamp = timestamp.Value, TimestampSource = timestamp.Source } : e)
                .OrderBy(e => e.Chain, StringComparer.Ordinal).ThenBy(e => e.Block).ThenBy(e => e.LogIndex).ToList(),
            InputHashes = hashes,
            Diagnostics = diagnostics
        };
    }

    private static void ReadElement(JsonElement element, EvidenceSelection selection, string source, bool verified, Dictionary<string, ChainEvent> events, Dictionary<string, TimestampObservation> timestamps, List<string> diagnostics)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray()) { ReadElement(child, selection, source, verified, events, timestamps, diagnostics); }
            return;
        }
        if (element.ValueKind != JsonValueKind.Object) { return; }
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
        ReadTimestamp(element, selection.Chain, source, verified, timestamps, diagnostics);
        if (element.TryGetProperty("topics", out _) && selection.TokenDecimals.ContainsKey(GetText(element, "address")))
        {
            ReadLog(element, selection, source, verified, events);
        }
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name is "result" or "logs") { ReadElement(property.Value, selection, source, property.Name == "logs" || verified, events, timestamps, diagnostics); }
        }
        if (element.TryGetProperty("logs", out var logs)) { AttachReceiptContext(logs, selection.Chain, events); }
    }

    private static void AttachReceiptContext(JsonElement logs, string chain, Dictionary<string, ChainEvent> events)
    {
        var transfers = new List<ReceiptTransfer>();
        var ids = new List<string>();
        foreach (var log in logs.EnumerateArray())
        {
            var topics = log.GetProperty("topics").EnumerateArray().Select(t => t.GetString()!.ToLowerInvariant()).ToList();
            var data = GetText(log, "data");
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
            if (e.ReceiptContextCaptured && !e.RelatedTransfers.SequenceEqual(related)) { throw new InvalidDataException($"Conflicting receipt context: {id}"); }
            events[id] = e with { RelatedTransfers = related, ReceiptContextCaptured = true };
        }
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
            FullTime = !timed || ReadInteger(string.Concat("0x", data.AsSpan(66, 64))).IsZero && ReadInteger(string.Concat("0x", data.AsSpan(130, 64))) == ulong.MaxValue,
            ReceiptVerified = verified, TimestampSource = "missing", Evidence = [source]
        };
        if (events.TryGetValue(e.Id, out var previous))
        {
            if (previous.Decimals != e.Decimals || previous.Token != e.Token || previous.BlockHash != e.BlockHash || previous.Block != e.Block || previous.From != e.From || previous.To != e.To || previous.RawAmount != e.RawAmount || previous.FullTime != e.FullTime)
            { throw new InvalidDataException($"Conflicting duplicate event: {e.Id}"); }
            e = e with { Evidence = previous.Evidence.Append(source).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(), ReceiptVerified = previous.ReceiptVerified || verified, RelatedTransfers = previous.RelatedTransfers, ReceiptContextCaptured = previous.ReceiptContextCaptured };
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
