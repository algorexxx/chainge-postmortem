namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record SourcePairCandidate
{
    public required string OtherEventId { get; init; }
    public required string FeeRawHypothesis { get; init; }
    public required TimeSpan Delay { get; init; }
    public required bool Competing { get; init; }
}
