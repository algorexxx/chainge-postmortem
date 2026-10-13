namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CapturePolicy
{
    public required CapturePlan Template { get; init; }
    public required long MaximumLogBlocks { get; init; }
    public required int BatchSize { get; init; }
    public bool CombineHolderFilters { get; init; }
}
