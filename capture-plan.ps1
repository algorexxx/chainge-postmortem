param([ValidatePattern('^[a-z0-9-]+$')][string]$Asset = 'usdc')
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet run --configuration Release --project Chainge.Postmortem.Reconcile -- plan-capture "reconciliation/$Asset.json" "archives/capture-plans/$Asset"
    if ($LASTEXITCODE -ne 0) { throw 'Capture planning failed.' }
}
finally { Pop-Location }
