param(
    [ValidatePattern('^[a-z0-9-]+$')]
    [string]$Asset = 'busd'
)
$ErrorActionPreference = 'Stop'
$reportPath = Join-Path $PSScriptRoot "archives/reconciliation/$Asset"
dotnet run --project (Join-Path $PSScriptRoot 'Chainge.Postmortem.Reconcile') -- $PSScriptRoot (Join-Path $PSScriptRoot "reconciliation/$Asset.json") $reportPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item -LiteralPath (Join-Path $reportPath 'site-movements.json') -Destination (Join-Path $PSScriptRoot "registry/$Asset-movements.json")
exit 0
