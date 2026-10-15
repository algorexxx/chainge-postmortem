param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [string]$ReportDirectory,
    [string]$ReviewPath,
    [string]$OutputDirectory = 'archives/workflow-review',
    [switch]$RequireFinished
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    if (-not $ReportDirectory) { $ReportDirectory = "archives/reconciliation/$Asset" }
    if (-not $ReviewPath) { $ReviewPath = "research/assets/$Asset-workflow.json" }
    $required = @('identity','vault-discovery','history','receipts','source-balances','fusion-state','fusion-semantics','holder-funding','outflow-disposition','announcements','publication')
    $summary = Read-Json "$ReportDirectory/accounting-summary.json"
    $configuration = Read-Json "reconciliation/$Asset.json"
    $configurationHash = (Get-FileHash "reconciliation/$Asset.json" -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($summary.configurationSha256 -cne $configurationHash) { throw 'Reconcile the current configuration before reviewing completion.' }
    $review = if (Test-Path -LiteralPath $ReviewPath) { Read-Json $ReviewPath } else { @{ gates = @() } }
    $seen = @{}
    $gates = foreach ($gate in $review.gates) {
        if ($gate.id -notin $required -or $seen.ContainsKey($gate.id)) { throw 'Unknown or duplicate workflow gate.' }
        $seen[$gate.id] = $true
        if ($gate.status -notin @('pending','verified','evidence-boundary')) { throw 'Invalid workflow status.' }
        if (-not $gate.finding -or -not $gate.nextAction) { throw 'Every gate needs a finding and next action or reopening condition.' }
        if ($gate.status -ne 'pending' -and -not $gate.references.Count) { throw 'Finished gates require retained references.' }
        $hashes = @{}
        foreach ($reference in $gate.references) {
            $fullPath = [IO.Path]::GetFullPath((Join-Path (Get-Location) $reference))
            $rootPath = (Get-Location).Path + [IO.Path]::DirectorySeparatorChar
            if (-not $fullPath.StartsWith($rootPath, [StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { throw "Missing or non-repository workflow reference: $reference" }
            $hashes[$reference] = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
        }
        if ($gate.status -eq 'evidence-boundary' -and (-not $gate.attemptedMethods.Count -or -not $gate.boundaryReason)) { throw 'Evidence boundaries need actual attempted methods and a reason further work cannot currently resolve the question.' }
        [ordered]@{ id = $gate.id; status = $gate.status; finding = $gate.finding; nextAction = $gate.nextAction; references = $hashes; attemptedMethods = $gate.attemptedMethods; boundaryReason = $gate.boundaryReason }
    }
    $gates = @($gates)
    foreach ($id in $required) {
        if (-not $seen.ContainsKey($id)) { $gates += [ordered]@{ id = $id; status = 'pending'; finding = 'Required stage has no assessment.'; nextAction = 'Perform this stage using CONTRIBUTING.md; record evidence and remaining limitations.' } }
    }
    $coverage = Read-Json "$ReportDirectory/history-coverage.json"
    $balances = Read-Json "$ReportDirectory/balance-checks.json"
    $claims = Read-Json "$ReportDirectory/claim-positions.json"
    $positions = @($claims.positions | Where-Object { $claims.claimIntervals.ContainsKey($_.token) -and [bigint]$_.rawAmount -gt 0 })
    $unobserved = @($positions | Where-Object {
        $position = $_
        -not @($balances.checks | Where-Object { $_.observation.chain -eq 'Fusion' -and $_.observation.token -eq $position.token -and $_.observation.holder -eq $position.holder -and $_.observation.block -eq $claims.claimCutoffBlock -and $_.observation.rawAmount -ceq $position.rawAmount -and $_.netAgrees }).Count
    })
    $pending = @($gates | Where-Object status -EQ pending)
    $missingHistory = @($coverage.coverage | Where-Object { $_.missingRanges.Count })
    $routingTargets = @(Import-Csv "$ReportDirectory/fusion-purposes.csv" | Where-Object { $_.transaction_input.StartsWith('0x3f163c47') -and $_.receipt_target -notin $configuration.policy.routingCallTargets } | ForEach-Object receipt_target | Sort-Object -Unique)
    $report = [ordered]@{ asset = $Asset; configurationSha256 = $configurationHash; scope = 'Work completion checklist, not a certificate of complete reconciliation. Verified assessments and documented evidence boundaries remain distinct.'; pendingCount = $pending.Count; unobservedFusionPositions = $unobserved.Count; incompleteHistoryDirections = $missingHistory.Count; unreviewedRoutingTargets = $routingTargets; gates = $gates }
    if (@($gates | Where-Object { $_.id -eq 'fusion-state' -and $_.status -eq 'verified' }).Count -and $unobserved.Count) { throw 'Fusion state is marked verified but positive configured positions lack matching pinned observations.' }
    if (@($gates | Where-Object { $_.id -eq 'fusion-state' -and $_.status -eq 'verified' }).Count) {
        if (-not (Test-Path "research/assets/$Asset-fusion-state.json")) { throw 'Verified Fusion state requires a reproducible restriction review, not just balances.' }
        & ./research/check-fusion-restrictions.ps1 -Asset $Asset
    }
    if (@($gates | Where-Object { $_.id -eq 'history' -and $_.status -eq 'verified' }).Count -and $missingHistory.Count) { throw 'History is marked verified but configured directions have coverage gaps.' }
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    [IO.File]::WriteAllText((Join-Path $OutputDirectory "$Asset.json"), ($report | ConvertTo-Json -Depth 30) + "`n", [Text.UTF8Encoding]::new($false))
    Write-Output "$Asset workflow: $($pending.Count) pending stages; $($unobserved.Count) positive configured Fusion positions without matching state."
    foreach ($gate in $pending) { Write-Output "  $($gate.id): $($gate.nextAction)" }
    if ($routingTargets.Count) { Write-Output "Routing calls need target review/configuration: $($routingTargets -join ', ')" }
    if ($RequireFinished -and ($pending.Count -or $routingTargets.Count)) { throw 'Investigation work remains pending. A reviewed outflow shortlist alone is not workflow completion.' }
}
finally { Pop-Location }
