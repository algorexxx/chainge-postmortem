using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record EvidenceSnapshot
{
    public required IReadOnlyList<ChainEvent> Events { get; init; }
    public required IReadOnlyDictionary<string, string> InputHashes { get; init; }
    public required IReadOnlyList<string> Diagnostics { get; init; }
}
