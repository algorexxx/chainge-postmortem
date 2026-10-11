namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record ReceiptPoolSwap
{
    public required string Pool { get; init; }
    public required string Recipient { get; init; }
    public required long LogIndex { get; init; }
    public required string Input0Raw { get; init; }
    public required string Input1Raw { get; init; }
    public required string Output0Raw { get; init; }
    public required string Output1Raw { get; init; }
}
