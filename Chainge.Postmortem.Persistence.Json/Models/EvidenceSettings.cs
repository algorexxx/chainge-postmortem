using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Persistence.Json.Models;

public sealed record EvidenceSettings
{
    public IReadOnlyDictionary<string, Chainge.Postmortem.Application.Acquisition.Models.CapturePolicy> CapturePolicies { get; init; } = new Dictionary<string, Chainge.Postmortem.Application.Acquisition.Models.CapturePolicy>();
    public IReadOnlyList<EvidenceSelection> BalanceSources { get; init; } = [];
    public IReadOnlyList<EvidenceSelection> LogQuerySources { get; init; } = [];
    public IReadOnlyList<LogHistoryTarget> LogHistoryTargets { get; init; } = [];
    public IReadOnlyDictionary<string, long> SourceCutoffBlocks { get; init; } = new Dictionary<string, long>();
    public IReadOnlyList<EvidenceSelection> DiscoverySources { get; init; } = [];
    public required string Asset { get; init; }
    public required MatchPolicy Policy { get; init; }
    public required IReadOnlyList<EvidenceSelection> Sources { get; init; }
    public required IReadOnlyList<string> Limitations { get; init; }
}
