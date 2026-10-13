using System.Security.Cryptography;
using System.Text.Json;

namespace Chainge.Postmortem.Persistence.Json;

public static class EvidenceIntegrityVerifier
{
    public static async Task<IReadOnlyDictionary<string, string>> VerifyAsync(string root, IReadOnlyDictionary<string, string> inputHashes, CancellationToken cancellationToken)
    {
        root = Path.GetFullPath(root);
        var manifests = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var input in inputHashes)
        {
            var file = Path.GetFullPath(Path.Combine(root, input.Key));
            var relative = Path.GetRelativePath(root, file);
            if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)) { throw new InvalidDataException("Evidence must stay inside the supplied root."); }
            var manifest = FindManifest(root, Path.GetDirectoryName(file)!);
            if (!manifests.TryGetValue(manifest, out var entries))
            {
                var bytes = await File.ReadAllBytesAsync(manifest, cancellationToken);
                entries = ReadManifest(bytes);
                manifests.Add(manifest, entries);
                hashes[Path.GetRelativePath(root, manifest).Replace('\\', '/')] = Convert.ToHexStringLower(SHA256.HashData(bytes));
            }
            var key = Path.GetRelativePath(Path.GetDirectoryName(manifest)!, file).Replace('\\', '/');
            if (!entries.TryGetValue(key, out var expected) || !string.Equals(expected, input.Value, StringComparison.OrdinalIgnoreCase)) { throw new InvalidDataException($"Evidence checksum missing or mismatched: {input.Key}"); }
        }
        return hashes;
    }

    private static string FindManifest(string root, string directory)
    {
        while (true)
        {
            var path = Path.Combine(directory, "manifest.json");
            if (File.Exists(path)) { return path; }
            if (directory == root) { throw new InvalidDataException("Evidence input has no integrity manifest."); }
            directory = Path.GetDirectoryName(directory)!;
        }
    }

    private static Dictionary<string, string> ReadManifest(byte[] bytes)
    {
        using var document = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'));
        var data = document.RootElement;
        if (data.ValueKind == JsonValueKind.Array)
        {
            return data.EnumerateArray().ToDictionary(e => e.GetProperty("file").GetString()!.Replace('\\', '/'), e => e.GetProperty("sha256").GetString()!, StringComparer.Ordinal);
        }
        return data.GetProperty("files").EnumerateObject().ToDictionary(e => e.Name.Replace('\\', '/'), e => e.Value.GetString()!, StringComparer.Ordinal);
    }
}
