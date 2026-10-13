namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CaptureResponse
{
    public required int HttpStatus { get; init; }
    public required bool ValidJson { get; init; }
    public required byte[] Body { get; init; }
}
