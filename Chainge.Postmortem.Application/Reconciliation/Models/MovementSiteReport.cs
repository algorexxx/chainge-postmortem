namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record MovementSiteReport
{
    public required IReadOnlyList<MovementSiteCase> Cases { get; init; }
    public required IReadOnlyList<DestinationSummary> Destinations { get; init; }
}
