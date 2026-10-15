$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $root = 'evidence/2026-10-15-rollux-distribution'
    $manifest = Get-Content "$root/manifest.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    foreach ($entry in $manifest.files.GetEnumerator()) {
        if ((Get-FileHash "$root/$($entry.Key)" -Algorithm SHA256).Hash -ne $entry.Value) { throw 'Rollux distribution evidence changed.' }
    }
    $headRows = Get-Content "$root/head.response.json" -Raw -Encoding utf8 | ConvertFrom-Json
    $head = ($headRows | Where-Object id -EQ head).result
    if (($headRows | Where-Object id -EQ chain).result -ne '0x23a') { throw 'Unexpected chain ID.' }
    foreach ($prefix in @('127e7766','3f0e40bc','66ff2f0a','8a4aa176')) {
        $report = & ./research/review-address-history.ps1 -CapturePrefix "$root/$prefix-history" | ConvertFrom-Json
        if ($report.block -ne $head.number) { throw 'History is not pinned to the retained head.' }
        Write-Output "Verified $($report.address): $($report.incomingCount) incoming, $($report.outgoingCount) outgoing, balance $($report.balanceRaw) raw units."
    }
    $rows = @{}
    foreach ($file in @('selected-receipts','selected-headers')) {
        foreach ($row in (Get-Content "$root/$file.response.json" -Raw -Encoding utf8 | ConvertFrom-Json)) { $rows[$row.id] = $row.result }
    }
    foreach ($entry in $rows.GetEnumerator() | Where-Object { $_.Key.EndsWith('-eth_getTransactionReceipt') }) {
        $receipt = $entry.Value
        $transaction = $rows[$receipt.transactionHash + '-eth_getTransactionByHash']
        if ($receipt.status -ne '0x1' -or $receipt.blockHash -ne $transaction.blockHash -or $rows[$receipt.blockNumber].hash -ne $receipt.blockHash) { throw 'Selected receipt/header mismatch.' }
    }
}
finally { Pop-Location }
