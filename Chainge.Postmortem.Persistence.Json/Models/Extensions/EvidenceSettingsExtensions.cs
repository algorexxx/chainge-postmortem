namespace Chainge.Postmortem.Persistence.Json.Models.Extensions;

public static class EvidenceSettingsExtensions
{
    public static EvidenceSettings NormalizeAddresses(this EvidenceSettings settings)
    {
        return settings with
        {
            Sources = settings.Sources.Select(s => s with { TokenDecimals = s.TokenDecimals.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value, StringComparer.Ordinal) }).ToList(),
            Policy = settings.Policy with { OperatingAddresses = settings.Policy.OperatingAddresses.ToDictionary(k => k.Key, v => v.Value.Select(a => a.ToLowerInvariant()).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal) }
        };
    }
}
