param(
    [Parameter(Mandatory)][string]$Asset,
    [Parameter(Mandatory)][string]$FusionEvidenceDirectory,
    [Parameter(Mandatory)][ValidatePattern('^0x[0-9a-fA-F]{40}$')][string]$Controller,
    [string]$OutputDirectory = 'archives/controller-links'
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Read-HexInteger([string]$Value) { [System.Numerics.BigInteger]::Parse(('0' + $Value.Replace('0x','')), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    function Get-Results($Value) {
        if ($Value -is [array]) { foreach ($entry in $Value) { Get-Results $entry }; return }
        if ($Value -is [System.Collections.IDictionary] -and $Value.Contains('succeeded') -and $Value.Contains('logCount') -and $Value.hash) {
            if ($Value.succeeded -ne $true -or $Value.logCount -ne $Value.logs.Count) { throw 'Failed or incomplete FSNEX receipt in controller evidence.' }
            $Value
            @{ transactionHash = $Value.hash; blockHash = $Value.blockHash; status = '0x1'; logs = $Value.logs }
            return
        }
        if ($Value -is [System.Collections.IDictionary] -and $Value.Contains('result')) { $Value.result }
    }
    $transactions = @{}
    $receipts = @{}
    $hashes = @{}
    foreach ($file in Get-ChildItem -LiteralPath $FusionEvidenceDirectory -Filter '*.response.json' | Sort-Object Name) {
        $relative = [System.IO.Path]::GetRelativePath((Get-Location).Path,$file.FullName).Replace('\','/')
        $hashes[$relative] = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        foreach ($value in Get-Results (Read-Json $file.FullName)) {
            if ($value -isnot [System.Collections.IDictionary]) { continue }
            if ($value.hash -and $value.input -and $value.to -eq $Controller -and $value.input -match '^0x5b7dd1bb[0-9a-fA-F]{320}$') { $transactions[$value.hash] = @{ value = $value; evidence = $relative } }
            if ($value.transactionHash -and $value.logs) { $receipts[$value.transactionHash] = $value }
        }
    }
    $mintTopic = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
    $orders = @{}
    foreach ($entry in $transactions.GetEnumerator()) {
        $tx = $entry.Value.value
        $receipt = $receipts[$entry.Key]
        if (-not $receipt -or $receipt.status -ne '0x1' -or $receipt.blockHash -ne $tx.blockHash) { throw "Controller call lacks matching successful receipt: $($entry.Key)" }
        $words = @(0..4 | ForEach-Object { $tx.input.Substring(10 + 64 * $_,64).ToLowerInvariant() })
        if ($words[1].Substring(0,24) -ne ('0' * 24) -or $words[2].Substring(0,24) -ne ('0' * 24)) { throw 'Non-canonical address in controller call.' }
        $token = '0x' + $words[1].Substring(24)
        $recipient = '0x' + $words[2].Substring(24)
        $mint = @($receipt.logs | Where-Object { $_.address -eq $token -and $_.topics.Count -eq 3 -and $_.topics[0] -eq $mintTopic -and $_.topics[1] -eq ('0x' + ('0' * 64)) -and $_.topics[2] -eq ('0x' + $words[2]) -and (Read-HexInteger $_.data) -eq (Read-HexInteger $words[4]) })
        if ($mint.Count -ne 1) { throw "Controller call does not agree with exactly one mint: $($entry.Key)" }
        $candidate = [ordered]@{ fusionTransaction = $entry.Key; fusionEvidence = $entry.Value.evidence; token = $token; recipient = $recipient; rawMint = (Read-HexInteger $words[4]).ToString(); observedIdentifier = '0x' + $words[0]; sourceCandidates = @() }
        if (-not $orders.ContainsKey($words[0])) { $orders[$words[0]] = @() }
        $orders[$words[0]] += $candidate
    }
    $seen = @{}
    $config = Read-Json "reconciliation/$($Asset.ToLowerInvariant()).json"
    foreach ($source in $config.sources | Where-Object chain -NE Fusion) {
        foreach ($file in Get-ChildItem -LiteralPath $source.directory -Filter $source.pattern | Sort-Object Name) {
            $relative = [System.IO.Path]::GetRelativePath((Get-Location).Path,$file.FullName).Replace('\','/')
            if ($hashes.ContainsKey($relative)) { continue }
            $hashes[$relative] = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            $text = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
            if (-not $text -or -not @($orders.Keys | Where-Object { $text.Contains($_) }).Count) { continue }
            foreach ($receipt in Get-Results ($text | ConvertFrom-Json -AsHashtable)) {
                if ($receipt -isnot [System.Collections.IDictionary] -or $receipt.status -ne '0x1' -or -not $receipt.logs) { continue }
                foreach ($log in $receipt.logs) {
                    if ($log.topics.Count -ne 4 -or $log.removed -eq $true) { continue }
                    $layout = switch ($log.topics[0]) {
                        '0x873a952504255baba041a0843512f2748152611db0d25da60657af220f7540ba' { 3 }
                        '0x74aaf2992820754761f714bb3dcf12d7e3bb2e5f41949629e2e88a4a219e2047' { 2 }
                        default { 0 }
                    }
                    if (-not $layout) { continue }
                    $identifier = $log.topics[$layout].Substring(2).ToLowerInvariant()
                    if (-not $orders.ContainsKey($identifier)) { continue }
                    $key = $source.chain + ':' + $receipt.transactionHash + ':' + $log.logIndex
                    $fingerprint = $log.address + ':' + ($log.topics -join ':') + ':' + $log.data + ':' + $receipt.blockHash
                    if ($seen.ContainsKey($key)) {
                        if ($seen[$key] -ne $fingerprint) { throw "Conflicting source event: $key" }
                        continue
                    }
                    $seen[$key] = $fingerprint
                    if ($log.data.Length -lt 194) { throw "Truncated source event: $key" }
                    $sourceToken = '0x' + $log.topics[1].Substring(26)
                    $vault = if ($layout -eq 3) { '0x' + $log.data.Substring(90,40) } else { '0x' + $log.topics[3].Substring(26) }
                    $rawAmount = (Read-HexInteger $log.data.Substring(2,64)).ToString()
                    $transfers = @($receipt.logs | Where-Object { $_.address -eq $sourceToken -and $_.topics.Count -eq 3 -and $_.topics[0] -eq $mintTopic -and $_.topics[2] -eq ('0x' + $vault.Substring(2).PadLeft(64,'0')) -and (Read-HexInteger $_.data).ToString() -eq $rawAmount })
                    $memo = ''
                    if ($layout -eq 3) {
                        $offset = [int](Read-HexInteger $log.data.Substring(130,64)) * 2 + 2
                        if ($offset + 64 -gt $log.data.Length) { throw "Invalid memo offset: $key" }
                        $length = [int](Read-HexInteger $log.data.Substring($offset,64)) * 2
                        if ($offset + 64 + $length -gt $log.data.Length) { throw "Invalid memo length: $key" }
                        $memo = [System.Text.Encoding]::UTF8.GetString([Convert]::FromHexString($log.data.Substring($offset + 64,$length)))
                    }
                    foreach ($candidate in $orders[$identifier]) {
                        $candidate.sourceCandidates += [ordered]@{ chain = $source.chain; transaction = $receipt.transactionHash; logIndex = $log.logIndex; emitter = $log.address; token = $sourceToken; vault = $vault; rawEventAmount = $rawAmount; matchingTransferCount = $transfers.Count; layout = if ($layout -eq 3) { 'observed-deposit-memo' } else { 'observed-swap-output' }; memo = $memo; evidence = $relative }
                    }
                }
            }
        }
    }
    $results = @($orders.Values | ForEach-Object { $_ } | Sort-Object { $_.fusionTransaction })
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    [ordered]@{ scope = 'Observed identifier correlations, not a verified ABI or an automatic settlement classification. IDs may repeat across routers or chains; all candidates remain visible. Inspect token, recipient, memo, chronology, receipt transfers and backing independently.'; inputHashes = $hashes; observations = $results } | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $OutputDirectory ($Asset.ToLowerInvariant() + '.json')) -Encoding utf8
    Write-Output "Reviewed $($results.Count) receipt-supported controller mints; $(@($results | Where-Object { $_.sourceCandidates.Count -gt 0 }).Count) have source-event identifier candidates. No ledger classifications changed."
}
finally { Pop-Location }
