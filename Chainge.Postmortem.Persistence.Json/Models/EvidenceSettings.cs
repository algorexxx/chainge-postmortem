using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record EvidenceSettings
{
    public required string Asset { get; init; }
    public required MatchPolicy Policy { get; init; }
    public required IReadOnlyList<EvidenceSelection> Sources { get; init; }
    public required IReadOnlyList<string> Limitations { get; init; }
}
