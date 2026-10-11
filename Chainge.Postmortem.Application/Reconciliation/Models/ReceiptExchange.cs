namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ReceiptExchange
{
    public IReadOnlyList<string> RouteEventIds { get; init; } = [];
    public string Rule { get; init; } = "reported-swapped-event";
    public required string Emitter { get; init; }
    public required long LogIndex { get; init; }
    public required string Sender { get; init; }
    public required string SourceToken { get; init; }
    public required string DestinationToken { get; init; }
    public required string Recipient { get; init; }
    public required string SpentRaw { get; init; }
    public required string ReturnedRaw { get; init; }
}
