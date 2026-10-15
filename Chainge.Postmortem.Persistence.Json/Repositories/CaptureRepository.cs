using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;
using Chainge.Postmortem.Persistence.Json.Models;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class CaptureRepository : ICaptureRepository
{
    private static readonly JsonSerializerOptions IndentedOptions = new(JsonSerializerOptions.Web) { WriteIndented = true };
    private readonly CaptureStoreSettings settings;
    private readonly TimeProvider clock;

    public CaptureRepository(CaptureStoreSettings settings, TimeProvider clock)
    {
        this.settings = settings;
        this.clock = clock;
    }

    public async Task<bool> HasSuccessfulCaptureAsync(CaptureQuery query, CancellationToken cancellationToken)
    {
        if (!CaptureCacheInspector.CanReuse(query) || !Directory.Exists(settings.Directory)) { return false; }
        var requestHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(query.Request.GetRawText())));
        foreach (var file in Directory.GetFiles(settings.Directory, query.Name + "-*.capture.json").OrderDescending(StringComparer.Ordinal))
        {
            var metadata = JsonSerializer.Deserialize<CaptureMetadata>(await File.ReadAllBytesAsync(file, cancellationToken), JsonSerializerOptions.Web)!;
            if (!metadata.Succeeded || metadata.RequestSha256 != requestHash || metadata.SourceUrl != settings.Endpoint) { continue; }
            var prefix = file[..^".capture.json".Length];
            var request = await File.ReadAllBytesAsync(prefix + ".request.json", cancellationToken);
            var response = await File.ReadAllBytesAsync(prefix + ".response.json", cancellationToken);
            if (Convert.ToHexStringLower(SHA256.HashData(request)) != requestHash || Convert.ToHexStringLower(SHA256.HashData(response)) != metadata.ResponseSha256) { throw new InvalidDataException("Changed capture bytes cannot be reused."); }
            if (CaptureResponseInspector.IsSuccessful(query, new CaptureResponse { HttpStatus = metadata.HttpStatus, ValidJson = metadata.ValidJson, Body = response })) { return true; }
        }
        return false;
    }

    public async Task SaveAttemptAsync(CaptureQuery query, CaptureResponse response, bool succeeded, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(settings.Directory);
        var attempt = 1;
        while (Directory.GetFiles(settings.Directory, $"{query.Name}-{attempt:D4}.*").Length != 0) { attempt++; }
        var prefix = Path.Combine(settings.Directory, $"{query.Name}-{attempt:D4}");
        var request = Encoding.UTF8.GetBytes(query.Request.GetRawText());
        await WriteNewFile(prefix + ".request.json", request, cancellationToken);
        await WriteNewFile(prefix + ".response.json", response.Body, cancellationToken);
        var metadata = new CaptureMetadata { SourceUrl = settings.Endpoint, CapturedUtc = clock.GetUtcNow(), HttpStatus = response.HttpStatus, ValidJson = response.ValidJson, Succeeded = succeeded, RetryAfter = response.RetryAfter, RequestSha256 = Convert.ToHexStringLower(SHA256.HashData(request)), ResponseSha256 = Convert.ToHexStringLower(SHA256.HashData(response.Body)) };
        await WriteNewFile(prefix + ".capture.json", JsonSerializer.SerializeToUtf8Bytes(metadata, JsonSerializerOptions.Web), cancellationToken);
    }

    private static async Task WriteNewFile(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    public async Task WriteManifestAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(settings.Directory);
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Directory.GetFiles(settings.Directory).Where(p => Path.GetFileName(p) != "manifest.json").Order(StringComparer.Ordinal))
        {
            hashes[Path.GetFileName(file)] = Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(file, cancellationToken)));
        }
        await File.WriteAllBytesAsync(Path.Combine(settings.Directory, "manifest.json"), JsonSerializer.SerializeToUtf8Bytes(new { algorithm = "SHA-256", files = hashes }, IndentedOptions), cancellationToken);
    }
}
