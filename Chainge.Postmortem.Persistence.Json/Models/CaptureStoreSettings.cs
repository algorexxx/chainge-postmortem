namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record CaptureStoreSettings
{
    public required string Directory { get; init; }
    public required Uri Endpoint { get; init; }
}
