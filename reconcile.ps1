param(
    [ValidatePattern('^[a-z0-9-]+$')]
    [string]$Asset = 'busd'
)
$ErrorActionPreference = 'Stop'
$reportPath = Join-Path $PSScriptRoot "archives/reconciliation/$Asset"
dotnet run --project (Join-Path $PSScriptRoot 'Chainge.Postmortem.Reconcile') -- $PSScriptRoot (Join-Path $PSScriptRoot "reconciliation/$Asset.json") $reportPath
exit $LASTEXITCODE
