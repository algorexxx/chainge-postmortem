$ErrorActionPreference = 'Stop'
dotnet run --project (Join-Path $PSScriptRoot 'Chainge.Postmortem.Site') -- (Join-Path $PSScriptRoot 'evidence/2026-10-11-token-catalogue/tokens.csv') (Join-Path $PSScriptRoot 'registry/studies.json') (Join-Path $PSScriptRoot 'dist') --serve
exit $LASTEXITCODE
