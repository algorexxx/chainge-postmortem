namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CapturePlan
{
    public required Uri Endpoint { get; init; }
    public required string ExpectedChainId { get; init; }
    public required string UserAgent { get; init; }
    public required TimeSpan RequestDelay { get; init; }
    public required TimeSpan Timeout { get; init; }
    public required int MaximumConsecutiveFailures { get; init; }
    public int MaximumRetries { get; init; }
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMinutes(1);
    public required IReadOnlyList<CaptureQuery> Queries { get; init; }
}
