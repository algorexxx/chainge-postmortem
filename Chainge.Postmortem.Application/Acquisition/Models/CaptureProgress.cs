namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CaptureProgress
{
    public required string Name { get; init; }
    public required string Status { get; init; }
}
