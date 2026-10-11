namespace Chainge.Postmortem.Persistence.Json.Models.Extensions;

public static class EvidenceSettingsExtensions
{
    public static EvidenceSettings NormalizeAddresses(this EvidenceSettings settings)
    {
        return settings with
        {
            DiscoverySources = settings.DiscoverySources.Select(s => s with { TokenDecimals = s.TokenDecimals.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value, StringComparer.Ordinal) }).ToList(),
            Sources = settings.Sources.Select(s => s with { TokenDecimals = s.TokenDecimals.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value, StringComparer.Ordinal) }).ToList(),
            Policy = settings.Policy with { FusionInventoryAccounts = settings.Policy.FusionInventoryAccounts.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value.Select(a => a.ToLowerInvariant()).ToArray(), StringComparer.Ordinal), IssuanceCallTargets = settings.Policy.IssuanceCallTargets.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value.Select(a => a.ToLowerInvariant()).ToArray(), StringComparer.Ordinal), ContractCallPurposes = settings.Policy.ContractCallPurposes.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value, StringComparer.Ordinal), SliceCounterparts = settings.Policy.SliceCounterparts.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value.Select(a => a.ToLowerInvariant()).ToArray(), StringComparer.Ordinal), OperatingAddresses = settings.Policy.OperatingAddresses.ToDictionary(k => k.Key, v => v.Value.Select(a => a.ToLowerInvariant()).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal) }
        };
    }
}
