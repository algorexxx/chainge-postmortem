namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record RoutingObservation
{
    public required string EventId { get; init; }
    public required string Status { get; init; }
    public string ChainCode { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
    public string? MemoAmountRaw { get; init; }
    public IReadOnlyList<string> CandidateEventIds { get; init; } = [];
}
