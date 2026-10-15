$ErrorActionPreference = 'Stop'
$directory = 'archives/arbitrum-withdrawal-tests'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$child = 'evidence/2026-10-15-comp-recipient-context'
$parent = 'evidence/2026-10-15-comp-ethereum-bridge-context'
& ./research/review-arbitrum-withdrawals.ps1 -ChildDirectory $child -ParentDirectory $parent -OutputPath "$directory/valid.json"
$report = Get-Content "$directory/valid.json" -Raw -Encoding utf8 | ConvertFrom-Json
if ($report.matches.Count -ne 6 -or $report.unmatched.Count -ne 0 -or (($report.matches | ForEach-Object { [decimal]$_.rawAmount } | Measure-Object -Sum).Sum -ne [decimal]'314000000000000000000')) { throw 'The six retained withdrawals do not reconcile to 314 COMP.' }
$mutated = Join-Path $directory 'mutated'
New-Item -ItemType Directory -Force -Path $mutated | Out-Null
foreach ($file in Get-ChildItem $parent -Filter '*.response.json') {
    $text = Get-Content $file.FullName -Raw -Encoding utf8
    $text.Replace('2e567b36','2e567b37') | Set-Content (Join-Path $mutated $file.Name) -Encoding utf8
}
foreach ($file in Get-ChildItem $parent -Filter '*.request.json') { Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $mutated $file.Name) -Force }
$rejected = $false
try { & ./research/review-arbitrum-withdrawals.ps1 -ChildDirectory $child -ParentDirectory $mutated -OutputPath "$directory/invalid.json" }
catch { if (-not $_.Exception.Message.Contains('Ambiguous or missing executed message/payment')) { throw }; $rejected = $true }
if (-not $rejected) { throw 'Changed message bytes were accepted as executed withdrawals.' }
& ./research/review-arbitrum-withdrawals.ps1 -ChildDirectory evidence/2026-10-15-comp-remaining-arbitrum-routes -ParentDirectory evidence/2026-10-15-comp-arbitrum-return-context -OutputPath "$directory/return-route.json"
$returnRoute = Get-Content "$directory/return-route.json" -Raw -Encoding utf8 | ConvertFrom-Json
if ($returnRoute.matches.Count -ne 1 -or $returnRoute.matches[0].rawAmount -cne '30000000000000000000' -or $returnRoute.unmatched.Count) { throw 'The distinct 30-COMP withdrawal does not match.' }
Write-Output 'Arbitrum withdrawal review binds positions and complete message bytes; altered messages do not match.'
