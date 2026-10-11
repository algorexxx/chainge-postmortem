namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record PayoutAddressCandidate
{
    public required string SourceEventId { get; init; }
    public required PairCandidate Pair { get; init; }
    public required string ExistingMovementClassification { get; init; }
}
