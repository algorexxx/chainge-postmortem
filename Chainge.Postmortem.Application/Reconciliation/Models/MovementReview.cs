namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record MovementReview
{
    public IReadOnlyList<ReceiptExchange> ExchangeEvidence { get; init; } = [];
    public string? ReconstructedBalanceBeforeRaw { get; init; }
    public string? ReconstructedBalanceAfterRaw { get; init; }
    public IReadOnlyList<string> Flags { get; init; } = [];
    public required string SourceEventId { get; init; }
    public required string Direction { get; init; }
    public required string Classification { get; init; }
    public required IReadOnlyList<PairCandidate> Candidates { get; init; }
}
