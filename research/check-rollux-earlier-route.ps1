$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    function Read-Hex([string]$Value) { [System.Numerics.BigInteger]::Parse(('0' + $Value.Replace('0x','')), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    $root = 'evidence/2026-10-15-rollux-earlier-route'
    foreach ($entry in (Read-Json "$root/manifest.json").files.GetEnumerator()) {
        Assert-Condition ((Get-FileHash "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) 'Earlier-route evidence changed.'
    }
    $rows = @{}
    foreach ($file in @('syscoin-receipts','syscoin-headers','rollux-receipts','rollux-headers')) {
        foreach ($row in (Read-Json "$root/$file.response.json")) { $rows[$row.id] = $row.result }
    }
    $mintHash = '0xbb956778aa0cdcbf4014c33b372bfdfedae00da6ca356f4aaa35d6f0b8f09884'
    $lockHash = '0x698265c8f572bab325f9105e4f226e55ade7adcba1c5a6d1112ab9b375387d75'
    $relayHash = '0x97694881ac32e6b0ba1e27033ccdc181abba40bc63456ab76d6b23e834a1cdc8'
    $transferTopic = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
    $expected = @(
        @($mintHash,'0x3fca6743e2fb55759fee767f3a68b2c06d699dc4','0000000000000000000000000000000000000000','8a4aa176007196d48d39c89402d3753c39ae64c1',[bigint]199899700003),
        @('0xfad5f7fc062392f4d36a73096032bb6217b89df2d67b4d4ec5f01943939b9578','0x3fca6743e2fb55759fee767f3a68b2c06d699dc4','8a4aa176007196d48d39c89402d3753c39ae64c1','8a85c29369ac6a12bfad9da45b9c9311d1e14e84',[bigint]199899699967),
        @($lockHash,'0x3fca6743e2fb55759fee767f3a68b2c06d699dc4','8a85c29369ac6a12bfad9da45b9c9311d1e14e84','9cc66f9b7b07f72a487ff751a7cbe281976fce7c',[bigint]199899699967),
        @($relayHash,'0x368433cac2a0b8d76e64681a9835502a1f2a8a30','0000000000000000000000000000000000000000','8a85c29369ac6a12bfad9da45b9c9311d1e14e84',[bigint]199899699967),
        @('0x1f685ba848b09b251c0b98e999f5d3f4fbdf7e90cad5c205dcdc309083bcb073','0x368433cac2a0b8d76e64681a9835502a1f2a8a30','8a85c29369ac6a12bfad9da45b9c9311d1e14e84','35cbec6a804556b460cd6007b23608329ccf8c65',[bigint]199899699967)
    )
    foreach ($item in $expected) {
        $receipt = $rows[$item[0] + '-eth_getTransactionReceipt']
        $tx = $rows[$item[0] + '-eth_getTransactionByHash']
        Assert-Condition ($receipt.status -eq '0x1' -and $receipt.blockHash -eq $tx.blockHash -and $rows[$receipt.blockNumber].hash -eq $receipt.blockHash) 'Receipt/header mismatch.'
        $transfers = @($receipt.logs | Where-Object { $_.topics[0] -eq $transferTopic })
        Assert-Condition ($transfers.Count -eq 1) 'Unexpected transfer count.'
        $log = $transfers[0]
        Assert-Condition ($log.address -eq $item[1] -and $log.topics[1].Substring(26) -eq $item[2] -and $log.topics[2].Substring(26) -eq $item[3] -and (Read-Hex $log.data) -eq $item[4]) 'Route transfer differs.'
    }
    $mintTx = $rows[$mintHash + '-eth_getTransactionByHash']
    Assert-Condition ($mintTx.to -eq '0x3fca6743e2fb55759fee767f3a68b2c06d699dc4' -and $mintTx.input -match '^0x40c10f19[0-9a-f]{128}$') 'Direct mint call shape differs.'
    $portal = @($rows[$lockHash + '-eth_getTransactionReceipt'].logs | Where-Object address -EQ '0xfe43b2c8a481c412481bc5a36261380edc417266')[0]
    Assert-Condition ($portal.data.Contains($rows[$relayHash + '-eth_getTransactionByHash'].input.Substring(2))) 'Bridge relay payload differs.'
    $history = & ./research/review-address-history.ps1 -CapturePrefix "$root/intermediate-history" | ConvertFrom-Json
    Assert-Condition ($history.incomingCount -eq 1 -and $history.outgoingCount -eq 1 -and $history.incomingRaw -eq '199899699967' -and $history.balanceRaw -eq '0' -and $history.recipients[0].address -eq '0x8a4aa176007196d48d39c89402d3753c39ae64c1') 'Intermediate pass-through differs.'
    Write-Output 'Verified earlier Syscoin mint, exact bridge relay and Rollux pass-through. No Fusion source or independent backing is established.'
}
finally { Pop-Location }
