namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record FusionPurpose
{
    public required string EventId { get; init; }
    public required string Classification { get; init; }
    public required bool ExcludeFromExternalMatching { get; init; }
    public required IReadOnlyList<string> CounterpartEventIds { get; init; }
}
