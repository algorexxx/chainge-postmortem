namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ReconciliationResult
{
    public required IReadOnlyList<MovementReview> Movements { get; init; }
    public required IReadOnlyList<string> ConversionEventIds { get; init; }
    public required IReadOnlyList<string> UnpairedFusionEventIds { get; init; }
}
