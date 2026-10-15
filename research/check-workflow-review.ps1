$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $directory = 'archives/workflow-review-tests'
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $hash = (Get-FileHash reconciliation/bat.json -Algorithm SHA256).Hash.ToLowerInvariant()
    $fixtures = @{
        'accounting-summary.json' = (@{ configurationSha256 = $hash } | ConvertTo-Json)
        'claim-positions.json' = '{"claimIntervals":{},"positions":[]}'
        'balance-checks.json' = '{"checks":[]}'
        'history-coverage.json' = '{"coverage":[{"missingRanges":[{"from":1,"through":2}]}]}'
        'fusion-purposes.csv' = 'transaction_input,receipt_target'
    }
    foreach ($entry in $fixtures.GetEnumerator()) { [IO.File]::WriteAllText((Join-Path $directory $entry.Key),$entry.Value,[Text.UTF8Encoding]::new($false)) }
    $path = Join-Path $directory 'missing-stages.json'
    [IO.File]::WriteAllText($path, '{"gates":[]}', [Text.UTF8Encoding]::new($false))
    $rejected = $false
    try { & ./research/review-workflow.ps1 -Asset bat -ReviewPath $path -ReportDirectory $directory -OutputDirectory $directory -RequireFinished | Out-Null }
    catch { if (-not $_.Exception.Message.Contains('Investigation work remains pending')) { throw }; $rejected = $true }
    if (-not $rejected) { throw 'Missing workflow stages were accepted as finished.' }
    $path = Join-Path $directory 'false-history.json'
    $fixture = @{ gates = @(@{ id = 'history'; status = 'verified'; finding = 'Negative test: falsely claims complete history.'; nextAction = 'This fixture must be rejected.'; references = @('research/assets/bat.md') }) }
    [IO.File]::WriteAllText($path, ($fixture | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
    $rejected = $false
    try { & ./research/review-workflow.ps1 -Asset bat -ReviewPath $path -ReportDirectory $directory -OutputDirectory $directory | Out-Null }
    catch { if (-not $_.Exception.Message.Contains('configured directions have coverage gaps')) { throw }; $rejected = $true }
    if (-not $rejected) { throw 'Known coverage gaps were accepted as verified history.' }
    $path = Join-Path $directory 'unreviewed-outflow.json'
    $ids = @('identity','vault-discovery','history','receipts','source-balances','fusion-state','fusion-semantics','holder-funding','outflow-disposition','announcements','publication')
    $fixture = @{ gates = @($ids | ForEach-Object { @{ id = $_; status = 'evidence-boundary'; finding = 'Fixture boundary'; nextAction = 'Reopen with evidence'; references = @('README.md'); attemptedMethods = @('Fixture'); boundaryReason = 'Fixture only' } }) }
    [IO.File]::WriteAllText($path, ($fixture | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
    $movement = @{ cases = @(@{ id = 'unreviewed-fixture'; rawAmount = '1'; decimals = 18; review = @{ direction = 'outgoing'; classification = 'unmatched' } }); terminal = @(); terminalPolicy = @{ decimals = 18; largeOutflowRaw = '0' } }
    $movement | ConvertTo-Json -Depth 10 | Set-Content "$directory/site-movements.json" -Encoding utf8
    $rejected = $false
    try { & ./research/review-workflow.ps1 -Asset bat -ReviewPath $path -ReportDirectory $directory -OutputDirectory $directory -RequireFinished | Out-Null }
    catch { if (-not ($_.Exception.Message.Contains('Additional closeout event is absent') -or $_.Exception.Message.Contains('outflows still need documented review'))) { throw }; $rejected = $true }
    if (-not $rejected) { throw 'Workflow completion skipped the closeout review.' }
    & ./research/review-holder-funding.ps1 -Asset bat | Out-Null
    $funding = Get-Content archives/holder-funding/bat.json -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if ($funding.holders.Count -ne 3 -or $funding.holders[0].rawBalance -cne '3495865849470520717329' -or $funding.holders[0].incoming.Count -ne 4) { throw 'Public FSNEX holder funding regression: independently pinned largest BAT position differs.' }
    Write-Output 'Workflow guards reject missing stages and false history completion; public FSNEX funding agrees with the pinned BAT fixture.'
}
finally { Pop-Location }
