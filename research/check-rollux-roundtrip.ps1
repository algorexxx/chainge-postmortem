$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    function Read-Hex([string]$Value) { [System.Numerics.BigInteger]::Parse(('0' + $Value.Replace('0x','')), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    function Read-Rows([string]$Directory, [string]$Pattern) {
        $rows = @{}
        foreach ($file in Get-ChildItem -LiteralPath $Directory -Filter $Pattern) {
            foreach ($row in (Read-Json $file.FullName)) { if ($row.ContainsKey('id')) { $rows[$row.id] = $row.result } }
        }
        return $rows
    }
    $root = 'evidence/2026-10-15-rollux-origin'
    foreach ($entry in (Read-Json "$root/manifest.json").files.GetEnumerator()) {
        Assert-Condition ((Get-FileHash "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) 'Rollux-origin evidence changed.'
    }
    $rows = Read-Rows $root '*.response.json'
    $fusion = Read-Rows 'evidence/2026-10-15-newer-holder-review' 'funding-*.response.json'
    $l1 = $rows['0x48826ef744c815480b960c1a4067ce61cc664d15068c907e18d8ca24ef7db157-eth_getTransactionByHash']
    $start = $fusion['0x16ec6fd9136b92937086e3727dd5d1ca049ea3db48db238ca330b0cfaa2a9318-eth_getTransactionReceipt']
    $event = @($start.logs | Where-Object { $_.topics[0] -eq '0x873a952504255baba041a0843512f2748152611db0d25da60657af220f7540ba' })[0]
    Assert-Condition ($event.topics[3].Substring(2) -eq $l1.input.Substring(10,64)) 'Fusion deposit and Syscoin mint identifiers differ.'
    $length = [int](Read-Hex $event.data.Substring(194,64)) * 2
    $memo = [System.Text.Encoding]::UTF8.GetString([Convert]::FromHexString($event.data.Substring(258,$length)))
    Assert-Condition ($memo -eq 'chainge:SYSEVM:0x3fca6743e2fb55759fee767f3a68b2c06d699dc4:0x8a85c29369ac6a12BfaD9da45B9c9311D1E14E84:0:1_2058011232659;3_2;4_2058011232659;5_SYSEVM') 'Syscoin destination memo differs.'
    $transferTopic = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
    $zero = '0x' + ('0' * 64)
    $startTransfers = @($start.logs | Where-Object { $_.topics[0] -eq $transferTopic })
    Assert-Condition ($start.status -eq '0x1' -and $startTransfers.Count -eq 1 -and (Read-Hex $startTransfers[0].data) -eq [bigint]2059247080950 -and $startTransfers[0].topics[2] -eq ('0x' + '66ff2f0ac3214758d1e61b16b41e3d5e62caecf1'.PadLeft(64,'0'))) 'Fusion source transfer changed.'
    Assert-Condition ($startTransfers[0].topics[2] -ne $zero) 'Fusion source event is a burn.'
    $mintReceipt = $rows[$l1.hash + '-eth_getTransactionReceipt']
    $mint = @($mintReceipt.logs | Where-Object { $_.topics[0] -eq $transferTopic })[0]
    Assert-Condition ($mint.address -eq '0x3fca6743e2fb55759fee767f3a68b2c06d699dc4' -and $mint.topics[1] -eq $zero -and (Read-Hex $mint.data) -eq [bigint]2058011232659) 'Syscoin mint changed.'
    $bridge = $rows['0xf80a706e84b85902a5b7956180935ae74a03ec4103ca3fa89c8e85c82af00d54-eth_getTransactionReceipt']
    $bridgeTransfer = @($bridge.logs | Where-Object { $_.topics[0] -eq $transferTopic })[0]
    Assert-Condition ($bridgeTransfer.address -eq $mint.address -and $bridgeTransfer.data -eq $mint.data -and $bridgeTransfer.topics[1] -eq $mint.topics[2] -and $bridgeTransfer.topics[2] -eq ('0x' + '9cc66f9b7b07f72a487ff751a7cbe281976fce7c'.PadLeft(64,'0'))) 'Syscoin bridge lock differs.'
    $relay = $rows['0xcfc074fd463be3deeb6ee9b29fa13660fd81efc25ef2387c84667445e8ce2767-eth_getTransactionByHash']
    $portal = @($bridge.logs | Where-Object address -EQ '0xfe43b2c8a481c412481bc5a36261380edc417266')[0]
    Assert-Condition ($portal.data.Contains($relay.input.Substring(2))) 'Rollux relay input is not contained in the Syscoin portal event.'
    $relayReceipt = $rows[$relay.hash + '-eth_getTransactionReceipt']
    $relayMint = @($relayReceipt.logs | Where-Object { $_.topics[0] -eq $transferTopic })[0]
    Assert-Condition ($relayMint.address -eq '0x368433cac2a0b8d76e64681a9835502a1f2a8a30' -and $relayMint.topics[1] -eq $zero -and $relayMint.topics[2] -eq $mint.topics[2] -and $relayMint.data -eq $mint.data) 'Rollux mint does not agree with the locked parent amount.'
    $history = Read-Json "$root/rollux-holder-history.response.json"
    Assert-Condition (@($history | Where-Object id -EQ chain)[0].result -eq '0x23a') 'Unexpected Rollux chain ID.'
    $syscoinHead = Read-Json "$root/syscoin-head.response.json"
    Assert-Condition (@($syscoinHead | Where-Object id -EQ chain)[0].result -eq '0x39') 'Unexpected Syscoin chain ID.'
    $outgoing = @($history | Where-Object id -EQ out)[0].result
    $split = @($outgoing | Where-Object transactionHash -EQ '0x5a096a26d4f53f45d4ef2efadd4f1cf6f964477d1dad27553a96800a3243efdd')
    $deposit = @($outgoing | Where-Object transactionHash -EQ '0x010d8150a55a50a1bcd72900e0f4c83f03d7992047866ca6129df8dc9d9427fc')
    Assert-Condition ($split.Count -eq 1 -and (Read-Hex $split[0].data) -eq [bigint]50000000000 -and $split[0].topics[2] -eq ('0x' + '127e7766f65c7a35e73177aa104999a189048e9d'.PadLeft(64,'0'))) 'Rollux split transfer differs.'
    Assert-Condition ($deposit.Count -eq 1 -and (Read-Hex $deposit[0].data) -eq [bigint]2008011232650 -and $deposit[0].topics[2] -eq ('0x' + '66ff2f0ac3214758d1e61b16b41e3d5e62caecf1'.PadLeft(64,'0'))) 'Rollux vault deposit differs.'
    $balance = [bigint]0
    foreach ($log in @($history | Where-Object id -EQ in)[0].result) { $balance += Read-Hex $log.data }
    foreach ($log in @($history | Where-Object id -EQ out)[0].result) { $balance -= Read-Hex $log.data }
    Assert-Condition ($balance -eq 9) 'Rollux holder closing balance differs.'
    Assert-Condition (([bigint]2058011232659 - [bigint]50000000000 - [bigint]2008011232650) -eq $balance) 'Rollux split does not reconcile.'
    foreach ($row in $rows.GetEnumerator() | Where-Object { $_.Key.EndsWith('-eth_getTransactionReceipt') }) {
        $receipt = $row.Value
        $tx = $rows[$receipt.transactionHash + '-eth_getTransactionByHash']
        Assert-Condition ($receipt.status -eq '0x1' -and $receipt.blockHash -eq $tx.blockHash -and $rows[$receipt.blockNumber].hash -eq $receipt.blockHash) 'Selected receipt/header mismatch.'
    }
    Write-Output 'Verified Fusion-to-Syscoin identifier/memo, parent-token mint and lock, exact bridge relay payload, Rollux mint and split. Backing remains unverified; no balances deducted.'
}
finally { Pop-Location }
