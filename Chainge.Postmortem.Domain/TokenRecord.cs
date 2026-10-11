namespace Chainge.Postmortem.Domain;

public sealed class TokenRecord
{
    public required string Symbol { get; set; }
    public required string Address { get; set; }
    public required int Row { get; set; }
}
