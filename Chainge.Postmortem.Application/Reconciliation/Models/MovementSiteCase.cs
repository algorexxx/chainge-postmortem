namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record MovementSiteCase
{
    public required string Id { get; init; }
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required string Transaction { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string RawAmount { get; init; }
    public required int Decimals { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public required bool ReceiptVerified { get; init; }
    public required IReadOnlyList<string> Evidence { get; init; }
    public required MovementReview Review { get; init; }
    public int? DepletionBasisPoints { get; init; }
}
