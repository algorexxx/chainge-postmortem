namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record CaptureMetadata
{
    public required Uri SourceUrl { get; init; }
    public required DateTimeOffset CapturedUtc { get; init; }
    public required int HttpStatus { get; init; }
    public required bool ValidJson { get; init; }
    public required bool Succeeded { get; init; }
    public required string RequestSha256 { get; init; }
    public required string ResponseSha256 { get; init; }
}
