namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record AddressFlowTotals
{
    public required string Chain { get; init; }
    public required string Token { get; init; }
    public required int Decimals { get; init; }
    public required bool FullTime { get; init; }
    public required string Address { get; init; }
    public required string InflowRaw { get; init; }
    public required string OutflowRaw { get; init; }
    public required string ObservedNetFlowRaw { get; init; }
}
