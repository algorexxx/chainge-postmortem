param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [string]$ReportDirectory,
    [string]$ReviewPath,
    [string]$OutputDirectory = 'archives/closeout',
    [switch]$RequireReviewed
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    if (-not $ReportDirectory) { $ReportDirectory = "archives/reconciliation/$Asset" }
    $report = Get-Content "$ReportDirectory/site-movements.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if (-not $ReviewPath) { $ReviewPath = "research/assets/$Asset-closeout.json" }
    $review = Get-Content $ReviewPath -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if ($review.windowDays -le 0) { throw 'A positive terminal review window is required.' }
    $decisions = @{}
    foreach ($decision in $review.decisions) {
        if ($decisions.ContainsKey($decision.eventId)) { throw 'Duplicate closeout decision.' }
        if (-not $decision.finding -or -not $decision.limitation -or -not $decision.references.Count) { throw 'Every decision requires a finding, limitation and references.' }
        foreach ($reference in $decision.references) {
            if (-not (Test-Path -LiteralPath $reference -PathType Leaf)) { throw "Missing decision evidence: $reference" }
        }
        $decisions[$decision.eventId] = $decision
    }
    $cases = @{}
    foreach ($case in $report.cases) { $cases[$case.id] = $case }
    $ends = @{}
    foreach ($terminal in $report.terminal) {
        if ($terminal.finalOutflowId -and $cases.ContainsKey($terminal.finalOutflowId)) {
            $ends["$($terminal.chain)|$($terminal.token)|$($terminal.address)"] = $cases[$terminal.finalOutflowId].timestamp
        }
    }
    $selected = @{}
    foreach ($case in $report.cases) {
        if ($case.review.direction -ne 'outgoing' -or $case.review.classification -ne 'unmatched') { continue }
        $scaledAmount = [bigint]::Parse($case.rawAmount) * [bigint]::Pow(10, $report.terminalPolicy.decimals)
        $scaledThreshold = [bigint]::Parse($report.terminalPolicy.largeOutflowRaw) * [bigint]::Pow(10, $case.decimals)
        if ($scaledAmount -lt $scaledThreshold) { continue }
        $end = $ends["$($case.chain)|$($case.token)|$($case.from)"]
        if ($case.timestamp -and $end) {
            $elapsed = [DateTimeOffset]::Parse($end) - [DateTimeOffset]::Parse($case.timestamp)
            if ($elapsed.TotalSeconds -lt 0) { throw 'A selected outflow is later than its terminal endpoint.' }
            if ($elapsed.TotalDays -gt $review.windowDays) { continue }
        }
        $selected[$case.id] = $case
    }
    foreach ($id in $review.extraEventIds) {
        if (-not $cases.ContainsKey($id)) { throw "Additional closeout event is absent: $id" }
        if ($cases[$id].review.direction -ne 'outgoing') { throw 'An additional closeout event must be a vault outflow.' }
        $selected[$id] = $cases[$id]
    }
    $results = @($selected.Values | Sort-Object timestamp, id | ForEach-Object {
        [ordered]@{
            event = $_
            assessment = $decisions[$_.id]
            reviewStatus = if ($decisions.ContainsKey($_.id)) { 'reviewed-with-stated-limits' } else { 'needs-review' }
        }
    })
    $pending = @($results | Where-Object reviewStatus -EQ 'needs-review')
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    [ordered]@{
        asset = $Asset
        scope = $review.scope
        windowDays = $review.windowDays
        reportSha256 = (Get-FileHash "$ReportDirectory/site-movements.json" -Algorithm SHA256).Hash.ToLowerInvariant()
        selectedCount = $results.Count
        pendingCount = $pending.Count
        cases = $results
        limitation = 'Reviewed does not mean settled. The selection is bounded by retained vaults, histories, the stated window and the configured amount threshold. Missing dates retain material cases for review.'
    } | ConvertTo-Json -Depth 40 | Set-Content "$OutputDirectory/$Asset.json" -Encoding utf8
    Write-Output "$Asset closeout: $($results.Count) material/additional outflows; $($pending.Count) lack an assessment."
    if ($RequireReviewed -and $pending.Count) { throw 'Material outflows still need documented review.' }
}
finally { Pop-Location }
