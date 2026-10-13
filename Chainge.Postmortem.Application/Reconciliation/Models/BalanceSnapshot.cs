namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record BalanceSnapshot
{
    public required IReadOnlyList<BalanceObservation> Observations { get; init; }
    public required IReadOnlyDictionary<string, string> InputHashes { get; init; }
}
