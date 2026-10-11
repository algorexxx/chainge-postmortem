namespace Chainge.Postmortem.Application.Catalogue.Models;

public sealed record TokenListing
{
    public required string Symbol { get; init; }
    public required string Address { get; init; }
    public required int Row { get; init; }
}
