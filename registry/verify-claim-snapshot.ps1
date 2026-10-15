param(
    [Parameter(Mandatory)][string]$ReportDirectory,
    [Parameter(Mandatory)][hashtable]$Study
)
$ErrorActionPreference = 'Stop'
if (-not $Study.ContainsKey('claimReview')) { return }
$claims = Get-Content (Join-Path $ReportDirectory 'claim-positions.json') -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$balances = Get-Content (Join-Path $ReportDirectory 'balance-checks.json') -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
if ($Study.claimReview.referenceBlock -ne $claims.claimCutoffBlock -or $Study.fusionStatistics.referenceBlock -ne $claims.claimCutoffBlock) { throw 'Claim snapshot blocks do not agree.' }
if ($Study.fusionStatistics.configurationSha256 -ne $claims.configurationSha256) { throw 'Claim snapshot configuration hash is stale.' }
$expected = @{}
foreach ($position in $claims.positions) {
    if (-not $claims.claimIntervals.ContainsKey($position.token)) { continue }
    if (-not $position.historyConsistent -or [bigint]$position.rawAmount -le 0) { throw 'Known claim ledger contains an inconsistent or non-positive position requiring review.' }
    $expected.Add(($position.token + ':' + $position.holder), $position)
}
if ($expected.Count -ne $Study.claimReview.positions.Count) { throw 'Published claim inventory differs from the known generated ledgers.' }
$seen = @{}
foreach ($position in $Study.claimReview.positions) {
    $key = $position.token + ':' + $position.holder
    if ($seen.ContainsKey($key) -or -not $expected.ContainsKey($key)) { throw "Duplicate or unknown claim position: $key" }
    $seen.Add($key, $true)
    foreach ($field in @('rawAmount', 'decimals', 'intervalStatus', 'historyConsistent')) {
        if ($position[$field] -cne $expected[$key][$field]) { throw "Claim position differs from generated evidence: $key/$field" }
    }
    $matches = @($balances.checks | Where-Object {
        $_.observation.chain -eq 'Fusion' -and $_.observation.token -eq $position.token -and
        $_.observation.holder -eq $position.holder -and $_.observation.block -eq $claims.claimCutoffBlock -and
        $_.observation.rawAmount -ceq $position.rawAmount -and $_.observation.source -ceq $position.balanceSource -and $_.netAgrees
    })
    if ($matches.Count -eq 0) { throw "No matching pinned balance evidence: $key" }
}
Write-Output "Verified $($seen.Count) published claim positions against generated history and pinned balances."
