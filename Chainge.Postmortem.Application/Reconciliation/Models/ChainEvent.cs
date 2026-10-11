namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ChainEvent
{
    public required string Id { get; init; }
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Transaction { get; init; }
    public required string BlockHash { get; init; }
    public required long Block { get; init; }
    public required long LogIndex { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required int Decimals { get; init; }
    public required string RawAmount { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public required string TimestampSource { get; init; }
    public required bool ReceiptVerified { get; init; }
    public required bool FullTime { get; init; }
    public required IReadOnlyList<string> Evidence { get; init; }
}
