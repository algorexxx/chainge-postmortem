param([Parameter(Mandatory)][string]$Plan, [Parameter(Mandatory)][string]$Output)
$ErrorActionPreference = 'Stop'
dotnet run --configuration Release --project (Join-Path $PSScriptRoot 'Chainge.Postmortem.Reconcile') -- capture $Plan $Output
exit $LASTEXITCODE
