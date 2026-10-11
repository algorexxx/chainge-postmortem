namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ReceiptTransfer
{
    public required string Token { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string RawAmount { get; init; }
    public required long LogIndex { get; init; }
}
