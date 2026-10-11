using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Persistence.Json.Models;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class ExplorerDiscoveryRepository
{
    private readonly EvidenceSettings settings;

    public ExplorerDiscoveryRepository(EvidenceSettings settings)
    {
        this.settings = settings;
    }

    public async Task<DiscoverySnapshot> ReadDiscoveryAsync(string root, CancellationToken cancellationToken)
    {
        var observations = new List<ChainEvent>();
        var coverage = new List<DiscoveryCoverage>();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var selection in settings.DiscoverySources)
        {
            var files = Directory.GetFiles(Path.Combine(root, selection.Directory), selection.Pattern).Order(StringComparer.Ordinal).ToList();
            if (files.Count == 0) { throw new InvalidDataException($"No discovery files matched {selection.Directory}/{selection.Pattern}"); }
            foreach (var file in files)
            {
                var source = Path.GetRelativePath(root, file).Replace('\\', '/');
                var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
                hashes[source] = Convert.ToHexStringLower(SHA256.HashData(bytes));
                using var document = JsonDocument.Parse(bytes);
                var data = document.RootElement;
                var success = data.TryGetProperty("status", out var status) && status.GetString() == "1" && data.TryGetProperty("result", out var rows) && rows.ValueKind == JsonValueKind.Array;
                var count = success ? data.GetProperty("result").GetArrayLength() : 0;
                coverage.Add(new DiscoveryCoverage { Source = source, Chain = selection.Chain, Status = success ? "returned-rows-not-completeness" : "failed-discovery-response", ReturnedRows = count });
                if (!success) { continue; }
                var index = 0;
                foreach (var row in data.GetProperty("result").EnumerateArray())
                {
                    index++;
                    var token = row.GetProperty("contractAddress").GetString()!.ToLowerInvariant();
                    if (!selection.TokenDecimals.TryGetValue(token, out var decimals)) { continue; }
                    observations.Add(ReadObservation(row, selection.Chain, token, decimals, source, index));
                }
            }
        }
        return new DiscoverySnapshot { Observations = observations, Coverage = coverage, InputHashes = hashes };
    }

    private static ChainEvent ReadObservation(JsonElement row, string chain, string token, int decimals, string source, int index)
    {
        return new ChainEvent
        {
            Id = $"explorer:{chain}:{source}:{index}", Chain = chain, Token = token, Decimals = decimals,
            Transaction = row.GetProperty("hash").GetString()!.ToLowerInvariant(), BlockHash = row.GetProperty("blockHash").GetString()!.ToLowerInvariant(),
            Block = long.Parse(row.GetProperty("blockNumber").GetString()!, CultureInfo.InvariantCulture), LogIndex = -1,
            From = row.GetProperty("from").GetString()!.ToLowerInvariant(), To = row.GetProperty("to").GetString()!.ToLowerInvariant(), RawAmount = row.GetProperty("value").GetString()!,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(row.GetProperty("timeStamp").GetString()!, CultureInfo.InvariantCulture)), TimestampSource = source,
            ReceiptVerified = false, FullTime = true, Evidence = [source]
        };
    }
}
