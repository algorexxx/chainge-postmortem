param(
    [ValidatePattern('^[a-z0-9-]+$')]
    [string]$Asset = 'busd',
    [switch]$UpdateRegistry
)
$ErrorActionPreference = 'Stop'
$reportPath = Join-Path $PSScriptRoot "archives/reconciliation/$Asset"
dotnet run --configuration Release --project (Join-Path $PSScriptRoot 'Chainge.Postmortem.Reconcile') -- $PSScriptRoot (Join-Path $PSScriptRoot "reconciliation/$Asset.json") $reportPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($UpdateRegistry) {
    Copy-Item -LiteralPath (Join-Path $reportPath 'site-movements.json') -Destination (Join-Path $PSScriptRoot "registry/$Asset-movements.json")
}
exit 0
