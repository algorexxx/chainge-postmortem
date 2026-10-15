$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $root = 'archives/closeout-selection-check'
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    $cases = @(
        @{ id = 'last'; timestamp = '2025-04-01T00:00:00Z'; rawAmount = '1'; decimals = 18; review = @{ direction = 'outgoing'; classification = 'unmatched' } },
        @{ id = 'boundary'; timestamp = '2025-01-01T00:00:00Z'; rawAmount = '10000000000000000000000'; decimals = 18; review = @{ direction = 'outgoing'; classification = 'unmatched' } },
        @{ id = 'too-old'; timestamp = '2024-12-31T23:59:59Z'; rawAmount = '10000000000'; decimals = 6; review = @{ direction = 'outgoing'; classification = 'unmatched' } },
        @{ id = 'below'; timestamp = '2025-03-01T00:00:00Z'; rawAmount = '9999999999999999999999'; decimals = 18; review = @{ direction = 'outgoing'; classification = 'unmatched' } },
        @{ id = 'undated'; timestamp = $null; rawAmount = '10000000000'; decimals = 6; review = @{ direction = 'outgoing'; classification = 'unmatched' } },
        @{ id = 'incoming'; timestamp = '2025-03-01T00:00:00Z'; rawAmount = '10000000000'; decimals = 6; review = @{ direction = 'incoming'; classification = 'unmatched' } },
        @{ id = 'candidate'; timestamp = '2025-03-01T00:00:00Z'; rawAmount = '10000000000'; decimals = 6; review = @{ direction = 'outgoing'; classification = 'possible-redemption' } }
    )
    foreach ($case in $cases) { $case.chain = 'Example'; $case.token = 'token'; $case.from = 'vault' }
    $report = @{ cases = $cases; terminal = @(@{ chain = 'Example'; token = 'token'; address = 'vault'; finalOutflowId = 'last' }); terminalPolicy = @{ largeOutflowRaw = '10000000000'; decimals = 6 } }
    $review = @{ scope = 'Fixture'; windowDays = 90; extraEventIds = @('too-old'); decisions = @() }
    $report | ConvertTo-Json -Depth 12 | Set-Content "$root/site-movements.json" -Encoding utf8
    $review | ConvertTo-Json -Depth 12 | Set-Content "$root/review.json" -Encoding utf8
    & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root
    $result = Get-Content "$root/fixture.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $ids = @($result.cases.event.id | Sort-Object)
    if (($ids -join ',') -ne 'boundary,too-old,undated' -or $result.pendingCount -ne 3) { throw 'Closeout boundary, precision or missing-date selection failed.' }
    $rejected = $false
    try { & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root -RequireReviewed }
    catch { $rejected = $_.Exception.Message -eq 'Material outflows still need documented review.' }
    if (-not $rejected) { throw 'An unreviewed material case passed the closing gate.' }
    $review.decisions = @($ids | ForEach-Object { @{ eventId = $_; finding = 'Observed'; limitation = 'Unresolved'; references = @('README.md') } })
    $review | ConvertTo-Json -Depth 12 | Set-Content "$root/review.json" -Encoding utf8
    & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root -RequireReviewed
    $review.decisions += $review.decisions[0]
    $review | ConvertTo-Json -Depth 12 | Set-Content "$root/review.json" -Encoding utf8
    $rejected = $false
    try { & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root }
    catch { $rejected = $_.Exception.Message -eq 'Duplicate closeout decision.' }
    if (-not $rejected) { throw 'Duplicate closeout decisions were accepted.' }
    $review.selection = 'all-outflows'
    $review.decisions = @()
    $review | ConvertTo-Json -Depth 12 | Set-Content "$root/review.json" -Encoding utf8
    & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root
    $result = Get-Content "$root/fixture.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $ids = @($result.cases.event.id | Sort-Object)
    if (($ids -join ',') -ne 'below,boundary,candidate,last,too-old,undated' -or $result.pendingCount -ne 6) { throw 'All-outflows omitted a small, old or matched outgoing case, or included an incoming case.' }
    $rejected = $false
    try { & ./research/review-closeout.ps1 -Asset fixture -ReportDirectory $root -ReviewPath "$root/review.json" -OutputDirectory $root -RequireReviewed }
    catch { $rejected = $_.Exception.Message -eq 'Material outflows still need documented review.' }
    if (-not $rejected) { throw 'All-outflows accepted unreviewed cases.' }
    Write-Output 'Closeout selection checks passed: exact amount/date boundary, missing dates, explicit exceptions and review enforcement.'
}
finally { Pop-Location }
