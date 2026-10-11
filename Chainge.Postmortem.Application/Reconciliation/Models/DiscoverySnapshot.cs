namespace Chainge.Postmortem.Application.Reconciliation.Models;

public sealed record DiscoverySnapshot
{
    public required IReadOnlyList<ChainEvent> Observations { get; init; }
    public required IReadOnlyList<DiscoveryCoverage> Coverage { get; init; }
    public required IReadOnlyDictionary<string, string> InputHashes { get; init; }
}
