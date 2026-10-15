param([string]$ReportPath = 'archives/controller-links/usdc.json')
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    $root = 'evidence/2026-10-15-controller-funding-links'
    foreach ($entry in (Read-Json "$root/manifest.json").files.GetEnumerator()) {
        Assert-Condition ((Get-FileHash "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) 'Controller-context evidence changed.'
    }
    $context = @{}
    foreach ($file in Get-ChildItem $root -Filter '*.response.json') {
        $chain = $file.Name.Split('-')[0].Split('.')[0]
        foreach ($row in (Read-Json $file.FullName)) { if ($row.ContainsKey('result')) { $context["${chain}:$($row.id)"] = $row.result } }
    }
    foreach ($chain in @{ ethereum='0x1'; arbitrum='0xa4b1'; avalanche='0xa86a'; rollux='0x23a' }.GetEnumerator()) {
        Assert-Condition ($context["$($chain.Key):chain"] -eq $chain.Value) 'Source chain identity mismatch.'
    }
    $fusion = @{}
    foreach ($file in Get-ChildItem evidence/2026-10-15-newer-holder-review -Filter '*.response.json') {
        foreach ($row in (Read-Json $file.FullName)) { if ($row.ContainsKey('result')) { $fusion[$row.id] = $row.result } }
    }
    $report = Read-Json $ReportPath
    Assert-Condition ($report.observations.Count -eq 9) 'Reviewed controller selection changed.'
    $memoCount = 0
    foreach ($observation in $report.observations) {
        Assert-Condition ($observation.sourceCandidates.Count -eq 1) 'Identifier is missing or ambiguous; review all candidates.'
        $source = $observation.sourceCandidates[0]
        $chain = $source.chain.ToLowerInvariant()
        $tx = $context["${chain}:$($source.transaction)"]
        $header = $context["${chain}:$($tx.blockNumber)"]
        $saved = Read-Json $source.evidence
        $receipt = @($saved | Where-Object { $_.result.transactionHash -eq $source.transaction })[0].result
        Assert-Condition ($receipt.status -eq '0x1' -and $receipt.blockHash -eq $tx.blockHash -and $header.hash -eq $tx.blockHash -and $source.matchingTransferCount -eq 1) 'Source transfer, receipt or header disagrees.'
        $mintTx = $fusion[$observation.fusionTransaction + '-eth_getTransactionByHash']
        $mintHeader = $fusion[$mintTx.blockNumber]
        $delay = [Convert]::ToInt64($mintHeader.timestamp,16) - [Convert]::ToInt64($header.timestamp,16)
        Assert-Condition ($delay -ge 0) 'Source event occurs after the mint.'
        if ($source.memo) {
            $memo = $source.memo
            if ($memo.StartsWith('{')) {
                $decoded = $memo | ConvertFrom-Json -AsHashtable
                Assert-Condition ($decoded.toChain -eq 'FSN' -and $decoded.toAddr -eq $observation.recipient -and $decoded.toIndex -eq '6') 'JSON memo destination differs; index 6 is an observed index, not an independently mapped token.'
            }
            else {
                $parts = $memo.Split(':')
                Assert-Condition ($parts[1] -eq 'FSN' -and $parts[2] -eq $observation.token -and $parts[3] -eq $observation.recipient) 'Memo token or recipient differs.'
            }
            Assert-Condition ($memo -match ('(?:^|[:";])1_' + [regex]::Escape($observation.rawMint) + ';')) 'Quoted output differs from minted amount.'
            $memoCount++
        }
        Write-Output "$($source.chain): $($observation.fusionTransaction), source precedes mint by $delay seconds; $($source.layout)."
    }
    Assert-Condition ($memoCount -eq 4) 'Memo-supported link count changed.'
    Write-Output 'Verified nine identifier links, including four destination/output memos. Five swap-output links remain narrower identifier correlations; no automatic settlement or backing claim.'
}
finally { Pop-Location }
