namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record PairCandidate
{
    public required string FusionEventId { get; init; }
    public required string FeeRawHypothesis { get; init; }
    public required TimeSpan Delay { get; init; }
    public required bool Competing { get; init; }
}
