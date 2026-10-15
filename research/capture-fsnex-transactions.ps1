param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateRange(1000, 60000)][int]$DelayMilliseconds = 1100
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$purposes = Join-Path $root "archives/reconciliation/$Asset/fusion-purposes.csv"
$transactions = @(Import-Csv -LiteralPath $purposes | ForEach-Object {
    if ($_.event_id -notmatch '^Fusion:(0x[0-9a-f]{64}):[0-9]+$') { throw 'Invalid Fusion event identity.' }
    $Matches[1]
} | Sort-Object -Unique)
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null
$client = [Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(60)
$client.DefaultRequestHeaders.UserAgent.ParseAdd('Chainge-public-research/1.0')
$failed = 0
try {
    $networkUrl = 'https://fsnex.net/api/v1/network'
    $networkResponse = $client.GetAsync($networkUrl).GetAwaiter().GetResult()
    try {
        $networkBytes = $networkResponse.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
        $networkName = 'network-' + [Guid]::NewGuid().ToString('N')
        [IO.File]::WriteAllBytes((Join-Path $output "$networkName.response.json"), $networkBytes)
        $networkMetadata = [ordered]@{ sourceUrl = $networkUrl; capturedUtc = [DateTimeOffset]::UtcNow.ToString('o'); httpStatus = [int]$networkResponse.StatusCode; sha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($networkBytes)) }
        [IO.File]::WriteAllText((Join-Path $output "$networkName.capture.json"), ($networkMetadata | ConvertTo-Json) + "`n", [Text.UTF8Encoding]::new($false))
        if (-not $networkResponse.IsSuccessStatusCode) { throw 'Public network identity lookup failed.' }
        $network = [Text.Encoding]::UTF8.GetString($networkBytes) | ConvertFrom-Json
        if ($network.chainId -ne 32659) { throw 'Public endpoint does not identify Fusion chain 32659.' }
        Write-Output "Fusion indexed range: $($network.indexedFromBlock)..$($network.indexedToBlock)"
    }
    finally { $networkResponse.Dispose() }
    foreach ($transaction in $transactions) {
        $responsePath = Join-Path $output "$transaction.response.json"
        $capturePath = Join-Path $output "$transaction.capture.json"
        $url = "https://fsnex.net/api/v1/transactions/$transaction"
        if (Test-Path -LiteralPath $responsePath) {
            $saved = Get-Content -LiteralPath $capturePath -Raw -Encoding utf8 | ConvertFrom-Json
            if ($saved.sourceUrl -cne $url -or $saved.httpStatus -ne 200 -or (Get-FileHash -LiteralPath $responsePath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $saved.sha256) { throw 'Existing capture is failed or changed; use a new output directory for retries.' }
            $detail = Get-Content -LiteralPath $responsePath -Raw -Encoding utf8 | ConvertFrom-Json
            if ($detail.hash -cne $transaction -or $detail.logCount -ne $detail.logs.Count) { throw 'Saved transaction identity or complete log count does not agree.' }
            continue
        }
        $response = $client.GetAsync($url).GetAwaiter().GetResult()
        try {
            $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
            [IO.File]::WriteAllBytes($responsePath, $bytes)
            $metadata = [ordered]@{ sourceUrl = $url; capturedUtc = [DateTimeOffset]::UtcNow.ToString('o'); httpStatus = [int]$response.StatusCode; sha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) }
            [IO.File]::WriteAllText($capturePath, ($metadata | ConvertTo-Json) + "`n", [Text.UTF8Encoding]::new($false))
            if (-not $response.IsSuccessStatusCode) { $failed++; Write-Warning "HTTP $([int]$response.StatusCode) for $transaction" }
            else {
                $detail = [Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json
                if ($detail.hash -cne $transaction -or $detail.logCount -ne $detail.logs.Count) { throw 'Transaction identity or complete log count does not agree.' }
                Write-Output "Captured $transaction"
                $failed = 0
            }
        }
        finally { $response.Dispose() }
        if ($failed -ge 3) { throw 'Three HTTP failures; preserve this attempt and retry separately after checking the provider.' }
        Start-Sleep -Milliseconds $DelayMilliseconds
    }
}
finally {
    $client.Dispose()
    $files = [ordered]@{}
    foreach ($file in Get-ChildItem -LiteralPath $output -File | Where-Object Name -NE 'manifest.json' | Sort-Object Name) { $files[$file.Name] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    [IO.File]::WriteAllText((Join-Path $output 'manifest.json'), (([ordered]@{ algorithm = 'SHA-256'; files = $files } | ConvertTo-Json -Depth 5) + "`n"), [Text.UTF8Encoding]::new($false))
}
