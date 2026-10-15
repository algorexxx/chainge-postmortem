param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [Parameter(Mandatory)][ValidatePattern('^0x[0-9a-fA-F]{40}$')][string]$Token,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string[]]$Addresses
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$configuration = Get-Content (Join-Path $root "reconciliation/$Asset.json") -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$holders = if ($Addresses) { @($Addresses | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique) } else { @($configuration.policy.operatingAddresses.BSC) }
if ($holders.Count -eq 0) { throw 'No configured BSC vault addresses.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Choose a new evidence directory; existing captures are immutable.' }
[IO.Directory]::CreateDirectory($output) | Out-Null
foreach ($holder in $holders) {
    if ($holder -notmatch '^0x[0-9a-fA-F]{40}$') { throw 'Invalid vault address.' }
    $url = "https://api.binplorer.com/getAddressHistory/$($holder)?apiKey=freekey&type=transfer&token=$Token&limit=100"
    $responsePath = Join-Path $output "$holder.response.json"
    $response = Invoke-WebRequest -Uri $url -UserAgent 'Chainge-public-research/1.0' -OutFile $responsePath -PassThru -SkipHttpErrorCheck -TimeoutSec 40
    @{ sourceUrl=$url; capturedUtc=[DateTimeOffset]::UtcNow.ToString('O'); httpStatus=[int]$response.StatusCode } | ConvertTo-Json | Set-Content (Join-Path $output "$holder.capture.json") -Encoding utf8
    if ([int]$response.StatusCode -ne 200) { throw 'Index request failed; preserve the failure and respect provider limits before retrying.' }
    Start-Sleep -Seconds 4
}
$hashes = [ordered]@{}
foreach ($file in Get-ChildItem -LiteralPath $output -File | Sort-Object Name) { $hashes[$file.Name]=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
@{algorithm='SHA-256';files=$hashes} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $output 'manifest.json') -Encoding utf8
Write-Output 'Discovery captured. Build a receipt plan; do not import indexed amounts into accounting.'
