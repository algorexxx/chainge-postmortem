using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Persistence.Json.Models;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class BalanceRepository
{
    private readonly EvidenceSettings settings;

    public BalanceRepository(EvidenceSettings settings)
    {
        this.settings = settings;
    }

    public async Task<BalanceSnapshot> ReadBalancesAsync(string root, CancellationToken cancellationToken)
    {
        var observations = new List<BalanceObservation>();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var selection in settings.BalanceSources)
        {
            var files = Directory.GetFiles(Path.Combine(root, selection.Directory), selection.Pattern).Order(StringComparer.Ordinal).ToList();
            if (files.Count == 0) { throw new InvalidDataException($"No balance requests matched {selection.Directory}/{selection.Pattern}"); }
            foreach (var file in files)
            {
                using var request = await ReadDocument(root, file, hashes, cancellationToken);
                var responseFile = file.Replace(".request.json", ".response.json", StringComparison.Ordinal);
                using var response = File.Exists(responseFile) ? await ReadDocument(root, responseFile, hashes, cancellationToken) : JsonDocument.Parse("[]");
                var responses = ReadElements(response.RootElement).Where(e => e.TryGetProperty("id", out _)).ToDictionary(e => e.GetProperty("id").GetRawText());
                foreach (var query in ReadElements(request.RootElement))
                {
                    var observation = ReadObservation(query, responses, selection.Chain, Path.GetRelativePath(root, file).Replace('\\', '/'));
                    if (observation is not null) { observations.Add(observation); }
                }
            }
        }
        return new BalanceSnapshot { Observations = observations, InputHashes = hashes };
    }

    private static async Task<JsonDocument> ReadDocument(string root, string file, SortedDictionary<string, string> hashes, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
        hashes[Path.GetRelativePath(root, file).Replace('\\', '/')] = Convert.ToHexStringLower(SHA256.HashData(bytes));
        return JsonDocument.Parse(bytes);
    }

    private static List<JsonElement> ReadElements(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().ToList() : [element];
    }

    private static BalanceObservation? ReadObservation(JsonElement query, Dictionary<string, JsonElement> responses, string chain, string source)
    {
        if (query.GetProperty("method").GetString() != "eth_call") { return null; }
        var parameters = query.GetProperty("params");
        var call = parameters[0];
        var data = call.GetProperty("data").GetString()!.ToLowerInvariant();
        var block = parameters[1].GetString()!;
        if (data.Length != 74 || !data.StartsWith("0x70a08231", StringComparison.Ordinal) || data[10..34] != new string('0', 24) || !block.StartsWith("0x", StringComparison.Ordinal)) { return null; }
        string? raw = null;
        if (responses.TryGetValue(query.GetProperty("id").GetRawText(), out var response) && !response.TryGetProperty("error", out _) && response.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.String)
        {
            var value = result.GetString()!;
            if (value.Length != 66 || !value.StartsWith("0x", StringComparison.Ordinal)) { throw new InvalidDataException($"Invalid balance result: {source}"); }
            raw = BigInteger.Parse("0" + value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }
        return new BalanceObservation { Chain = chain, Token = call.GetProperty("to").GetString()!.ToLowerInvariant(), Holder = "0x" + data[34..], Block = long.Parse(block.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture), RawAmount = raw, Source = source };
    }
}
