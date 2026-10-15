param([Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $path = 'registry/studies.json'
    $text = Get-Content $path -Raw -Encoding utf8
    $studies = $text | ConvertFrom-Json -AsHashtable
    $matches = @($studies.Values | Where-Object movementsFile -EQ "$Asset-movements.json")
    if ($matches.Count -ne 1 -or -not $matches[0].fusionStatistics -or -not $matches[0].claimReview) { throw 'One published Fusion snapshot and claim inventory is required.' }
    $study = $matches[0]
    $directory = "archives/reconciliation/$Asset"
    $summary = Get-Content "$directory/accounting-summary.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $hash = (Get-FileHash "reconciliation/$Asset.json" -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($summary.configurationSha256 -ne $hash) { throw 'Reconcile the current configuration before refreshing provenance.' }
    $tokens = @($summary.accounting.tokens | Where-Object chain -EQ 'Fusion')
    $claims = Get-Content "$directory/claim-positions.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $expectedAddresses = @($claims.claimIntervals.Keys | Sort-Object)
    $publishedAddresses = @($study.fusionStatistics.tokens | ForEach-Object { $_['address'] } | Sort-Object -Unique)
    if (($expectedAddresses -join ',') -cne ($publishedAddresses -join ',') -or $publishedAddresses.Count -ne $study.fusionStatistics.tokens.Count) { throw 'Known Fusion token inventory changed; review the published snapshot.' }
    foreach ($published in $study.fusionStatistics.tokens) {
        $token = @($tokens | Where-Object token -EQ $published.address)
        if ($token.Count -ne 1) { throw 'Fusion token is missing or duplicated.' }
        foreach ($field in @('decimals','fullTime','eventCount','mintedRaw','burnedRaw')) {
            if ($token[0][$field] -cne $published[$field]) { throw "Fusion statistics changed: $($published.address)/$field. Review the snapshot; do not merely refresh its hash." }
        }
    }
    $oldHash = $study.fusionStatistics.configurationSha256
    $study.fusionStatistics.configurationSha256 = $hash
    & ./registry/verify-claim-snapshot.ps1 -ReportDirectory $directory -Study $study
    if ($oldHash -eq $hash) { Write-Output "$Asset Fusion provenance is already current."; return }
    $old = '"configurationSha256": "' + $oldHash + '"'
    $replacement = '"configurationSha256": "' + $hash + '"'
    if ([regex]::Matches($text, [regex]::Escape($old)).Count -ne 1) { throw 'Expected exactly one provenance hash; preserve the file and review its format.' }
    [IO.File]::WriteAllText((Join-Path (Get-Location) $path), $text.Replace($old, $replacement), [Text.UTF8Encoding]::new($false))
    Write-Output "$Asset provenance refreshed after checking unchanged Fusion statistics and pinned holder positions."
}
finally { Pop-Location }
