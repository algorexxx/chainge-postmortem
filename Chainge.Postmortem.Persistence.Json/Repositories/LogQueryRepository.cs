using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Persistence.Json.Models;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class LogQueryRepository
{
    private const string Transfer = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
    private static readonly SearchValues<char> HexDigits = SearchValues.Create("0123456789abcdef");
    private readonly EvidenceSettings settings;

    public LogQueryRepository(EvidenceSettings settings)
    {
        this.settings = settings;
    }

    public async Task<LogQuerySnapshot> ReadQueriesAsync(string root, CancellationToken cancellationToken)
    {
        var windows = new List<LogQueryWindow>();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var diagnostics = new List<string>();
        foreach (var selection in settings.LogQuerySources)
        {
            var files = Directory.GetFiles(Path.Combine(root, selection.Directory), selection.Pattern).Order(StringComparer.Ordinal).ToList();
            if (files.Count == 0) { throw new InvalidDataException($"No log queries matched {selection.Directory}/{selection.Pattern}"); }
            foreach (var file in files)
            {
                using var request = await ReadDocument(root, file, hashes, cancellationToken);
                var responseFile = file.Replace(".request.json", ".response.json", StringComparison.Ordinal);
                using var response = File.Exists(responseFile) ? await ReadDocument(root, responseFile, hashes, cancellationToken) : JsonDocument.Parse("null");
                var requests = CaptureResponseInspector.ReadElements(request.RootElement);
                if (requests.Select(q => q.GetProperty("id").GetRawText()).Distinct(StringComparer.Ordinal).Count() != requests.Count) { throw new InvalidDataException("Duplicate log query identifiers."); }
                foreach (var query in requests)
                {
                    if (query.ValueKind != JsonValueKind.Object || query.GetProperty("method").GetString() != "eth_getLogs") { continue; }
                    var source = Path.GetRelativePath(root, file).Replace('\\', '/');
                    var filter = query.GetProperty("params")[0];
                    var topics = filter.GetProperty("topics");
                    var supported = topics.GetArrayLength() is 2 or 3 && topics[0].ValueKind == JsonValueKind.String && topics[0].GetString() == Transfer;
                    var from = ReadBlock(filter.GetProperty("fromBlock"));
                    var through = ReadBlock(filter.GetProperty("toBlock"));
                    var incoming = topics.GetArrayLength() == 3 && topics[1].ValueKind == JsonValueKind.Null;
                    supported &= incoming || topics.GetArrayLength() == 2 || topics.GetArrayLength() == 3 && topics[2].ValueKind == JsonValueKind.Null;
                    if (!supported || from < 0 || through < from) { diagnostics.Add($"{source}: unsupported or unpinned query; no coverage credit"); continue; }
                    var tokens = ReadAlternatives(filter.GetProperty("address"));
                    var holders = ReadAlternatives(topics[incoming ? 2 : 1]);
                    if (tokens.Any(t => !IsHex(t, 42)) || holders.Any(h => !IsHex(h, 66) || h[2..26] != new string('0', 24))) { throw new InvalidDataException($"Malformed query address: {source}"); }
                    var succeeded = ValidateResponse(query, response.RootElement, tokens, holders, incoming, from, through);
                    foreach (var token in tokens)
                    {
                        foreach (var holder in holders) { windows.Add(new LogQueryWindow { Chain = selection.Chain, Token = token, Holder = "0x" + holder[26..], Direction = incoming ? "incoming" : "outgoing", Range = new BlockRange { From = from, Through = through }, Succeeded = succeeded, Source = source }); }
                    }
                }
            }
        }
        return new LogQuerySnapshot { Windows = windows, InputHashes = hashes, Diagnostics = diagnostics };
    }

    private static async Task<JsonDocument> ReadDocument(string root, string file, SortedDictionary<string, string> hashes, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
        hashes[Path.GetRelativePath(root, file).Replace('\\', '/')] = Convert.ToHexStringLower(SHA256.HashData(bytes));
        return JsonDocument.Parse(bytes);
    }

    private static long ReadBlock(JsonElement value)
    {
        var text = value.GetString();
        if (text is null || !text.StartsWith("0x", StringComparison.Ordinal)) { return -1; }
        return long.Parse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    private static List<string> ReadAlternatives(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(v => v.GetString()!.ToLowerInvariant()).ToList() : [value.GetString()!.ToLowerInvariant()];
    }

    private static bool IsHex(string value, int length)
    {
        return value.Length == length && value.StartsWith("0x", StringComparison.Ordinal) && !value.AsSpan(2).ContainsAnyExcept(HexDigits);
    }

    private static bool ValidateResponse(JsonElement query, JsonElement response, List<string> tokens, List<string> holders, bool incoming, long from, long through)
    {
        if (response.ValueKind == JsonValueKind.Array)
        {
            var matching = response.EnumerateArray().Where(r => r.ValueKind == JsonValueKind.Object && r.TryGetProperty("id", out var id) && id.GetRawText() == query.GetProperty("id").GetRawText()).ToList();
            if (matching.Count > 1) { throw new InvalidDataException("Duplicate log response identifiers."); }
            if (matching.Count == 0) { return false; }
            response = matching[0];
        }
        if (response.ValueKind != JsonValueKind.Object) { return false; }
        if (response.TryGetProperty("error", out _) || !response.TryGetProperty("result", out var rows) || rows.ValueKind != JsonValueKind.Array) { return false; }
        if (!response.TryGetProperty("id", out var id) || id.GetRawText() != query.GetProperty("id").GetRawText()) { throw new InvalidDataException("Log response identifier does not match request."); }
        foreach (var row in rows.EnumerateArray())
        {
            var topics = row.GetProperty("topics");
            var block = ReadBlock(row.GetProperty("blockNumber"));
            var valid = block >= from && block <= through && tokens.Contains(row.GetProperty("address").GetString()!.ToLowerInvariant()) && topics.GetArrayLength() == 3 && topics[0].GetString() == Transfer && holders.Contains(topics[incoming ? 2 : 1].GetString()!.ToLowerInvariant());
            if (!valid || row.TryGetProperty("removed", out var removed) && removed.GetBoolean()) { throw new InvalidDataException("Log response falls outside its query or contains removed logs."); }
        }
        return true;
    }
}
