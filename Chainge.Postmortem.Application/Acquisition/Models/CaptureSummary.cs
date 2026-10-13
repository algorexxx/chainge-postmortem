namespace Chainge.Postmortem.Application.Acquisition.Models;

public sealed record CaptureSummary
{
    public required bool IdentityVerified { get; init; }
    public required int Captured { get; init; }
    public required int Reused { get; init; }
    public required int Failed { get; init; }
    public required int Remaining { get; init; }
}
