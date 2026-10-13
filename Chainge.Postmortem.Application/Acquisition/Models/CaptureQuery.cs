using System.Text.Json;

namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CaptureQuery
{
    public required string Name { get; init; }
    public required JsonElement Request { get; init; }
}
