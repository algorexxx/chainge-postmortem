namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record TransactionCall
{
    public required string Target { get; init; }
    public required string Sender { get; init; }
    public required string Input { get; init; }
    public required string BlockHash { get; init; }
    public required string Evidence { get; init; }
}
