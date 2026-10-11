namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record TerminalOutflowSummary
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Address { get; init; }
    public required int Decimals { get; init; }
    public required string FirstEventId { get; init; }
    public required string LastEventId { get; init; }
    public string? FinalOutflowId { get; init; }
    public string? LastSubstantialOutflowId { get; init; }
    public required IReadOnlyList<string> LastOutflowIds { get; init; }
    public required IReadOnlyList<string> SequenceEventIds { get; init; }
    public string? SequenceOpeningBalanceRaw { get; init; }
    public required string ClosingBalanceRaw { get; init; }
    public required string SequenceInflowRaw { get; init; }
    public required string SequenceOutflowRaw { get; init; }
    public required int SequenceInflowCount { get; init; }
    public int? NetReductionBasisPoints { get; init; }
    public required bool NonnegativeHistory { get; init; }
    public required bool AllReceiptsVerified { get; init; }
}
