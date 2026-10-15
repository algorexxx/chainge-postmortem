param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateRange(500, 60000)][int]$DelayMilliseconds = 1100
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$configuration = Get-Content (Join-Path $root "reconciliation/$Asset.json") -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$blocks = @{}
function Add-BlockReferences($Value) {
    if ($Value -is [array]) { foreach ($entry in $Value) { Add-BlockReferences $entry }; return }
    if ($Value -isnot [System.Collections.IDictionary]) { return }
    if ($Value.blockNumber -and $Value.blockHash) {
        $number = if ([string]$Value.blockNumber -match '^0x') { [Convert]::ToInt64($Value.blockNumber.Substring(2),16) } else { [long]$Value.blockNumber }
        if ($blocks.ContainsKey($number) -and $blocks[$number] -cne $Value.blockHash) { throw 'Conflicting retained block hashes.' }
        $blocks[$number] = $Value.blockHash
    }
    foreach ($key in @('result','logs')) { if ($Value.Contains($key)) { Add-BlockReferences $Value[$key] } }
}
foreach ($source in $configuration.sources | Where-Object chain -EQ Fusion) {
    foreach ($file in Get-ChildItem -LiteralPath (Join-Path $root $source.directory) -Filter $source.pattern) {
        Add-BlockReferences (Get-Content $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable)
    }
}
if (-not $blocks.Count) { throw 'No retained Fusion block references.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null
foreach ($number in $blocks.Keys | Sort-Object) {
    $url = "https://fsnex.net/api/v1/blocks/$number"
    $path = Join-Path $output "$number.response.json"
    $capture = Join-Path $output "$number.capture.json"
    if (Test-Path -LiteralPath $path) {
        $metadata = Get-Content $capture -Raw -Encoding utf8 | ConvertFrom-Json
        if ($metadata.sourceUrl -cne $url -or $metadata.httpStatus -ne 200 -or $metadata.sha256 -cne (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()) { throw 'Saved header is failed or changed; preserve it and choose a new directory.' }
    }
    else {
        $response = Invoke-WebRequest -Uri $url -OutFile $path -PassThru -SkipHttpErrorCheck -TimeoutSec 40
        @{ sourceUrl=$url; capturedUtc=[DateTimeOffset]::UtcNow.ToString('O'); httpStatus=[int]$response.StatusCode; sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() } | ConvertTo-Json | Set-Content $capture -Encoding utf8
        if ([int]$response.StatusCode -ne 200) { throw 'Header unavailable; preserve the response and respect provider cooldowns before a new attempt.' }
        Start-Sleep -Milliseconds $DelayMilliseconds
    }
    $header = Get-Content $path -Raw -Encoding utf8 | ConvertFrom-Json
    if ($header.number -ne $number -or $header.hash -cne $blocks[$number]) { throw 'Public header does not match the retained block identity.' }
    Write-Output "Verified block $number"
}
$hashes = [ordered]@{}
foreach ($file in Get-ChildItem -LiteralPath $output -File | Where-Object Name -NE manifest.json | Sort-Object Name) { $hashes[$file.Name]=(Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
@{ algorithm='SHA-256'; files=$hashes } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $output 'manifest.json') -Encoding utf8
