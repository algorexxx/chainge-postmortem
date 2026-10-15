param([switch]$SkipBuildAndTests)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not (Test-Json -Path registry/studies.json -SchemaFile registry/studies.schema.json)) { throw 'The reviewed studies do not match the standard site data format.' }
    $studies = Get-Content registry/studies.json -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    & ./research/check-migration-state.ps1
    & ./research/check-newer-holders.ps1
    & ./research/review-controller-links.ps1 -Asset usdc -FusionEvidenceDirectory evidence/2026-10-15-newer-holder-review -Controller 0x3668c219b1fa8fe8175158f6ce91ded36fde9152
    & ./research/check-controller-links.ps1
    if (-not $SkipBuildAndTests) {
        dotnet build Chainge.Postmortem.slnx --configuration Release
        if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
        dotnet test --solution Chainge.Postmortem.slnx --configuration Release --no-build --filter-not-trait Category=Integration
        if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    }
    foreach ($config in Get-ChildItem -LiteralPath reconciliation -Filter '*.json' | Sort-Object Name) {
        $assetName = $config.BaseName
        $outputPath = Join-Path 'archives/workflow-check' $assetName
        dotnet run --project Chainge.Postmortem.Reconcile --configuration Release --no-build -- . $config.FullName $outputPath
        if ($LASTEXITCODE -ne 0) { throw "Reconciliation failed: $assetName" }
        foreach ($study in $studies.Values | Where-Object { $_.movementsFile -eq "$assetName-movements.json" }) {
            & ./registry/verify-claim-snapshot.ps1 -ReportDirectory $outputPath -Study $study
            if ($study.claimReview) { & ./research/review-holder-funding.ps1 -Asset $assetName }
        }
        if ($assetName -eq 'usdc') { & ./research/check-usdc-post-snapshot.ps1 -ReportDirectory $outputPath }
        $publishedPath = Join-Path registry "$assetName-movements.json"
        if (Test-Path -LiteralPath $publishedPath) {
            $generatedHash = (Get-FileHash -LiteralPath (Join-Path $outputPath 'site-movements.json') -Algorithm SHA256).Hash
            $publishedHash = (Get-FileHash -LiteralPath $publishedPath -Algorithm SHA256).Hash
            if ($generatedHash -ne $publishedHash) { throw "Stale public report: $publishedPath. Review the changes, then run ./reconcile.ps1 -Asset $assetName -UpdateRegistry." }
        }
    }
    dotnet run --project Chainge.Postmortem.Site --configuration Release --no-build -- evidence/2026-10-11-token-catalogue/tokens.csv registry/studies.json dist
    if ($LASTEXITCODE -ne 0) { throw 'Site generation failed.' }
    node registry/check-ui.cjs
    if ($LASTEXITCODE -ne 0) { throw 'Shared asset UI checks failed.' }
    Write-Output 'Verification passed. Generated reports are reproducible; this does not certify evidence completeness or custody.'
}
finally {
    Pop-Location
}
