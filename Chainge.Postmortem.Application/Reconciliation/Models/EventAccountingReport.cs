namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record EventAccountingReport
{
    public required IReadOnlyList<TokenEventTotals> Tokens { get; init; }
    public required IReadOnlyList<MovementTotals> Movements { get; init; }
    public required IReadOnlyList<AddressFlowTotals> Addresses { get; init; }
}
