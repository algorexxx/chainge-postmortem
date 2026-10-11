namespace Chainge.Postmortem.Application.Catalogue.Models;

public sealed record TokenSummary
{
    public required string Symbol { get; init; }
    public required string Address { get; init; }
    public required IReadOnlyList<int> SourceRows { get; init; }
    public required bool HasStudy { get; init; }
}
