namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BurnInventoryReview
{
    public required string BurnEventId { get; init; }
    public required string Classification { get; init; }
    public string? ObservedBalanceBeforeRaw { get; init; }
    public string? ObservedBalanceAfterRaw { get; init; }
    public IReadOnlyList<string> InventoryEventIds { get; init; } = [];
}
