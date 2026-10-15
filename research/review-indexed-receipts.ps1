param(
    [Parameter(Mandatory)][string]$SourceDirectory,
    [Parameter(Mandatory)][string]$ReceiptDirectory,
    [Parameter(Mandatory)][ValidatePattern('^0x[0-9a-fA-F]{40}$')][string]$Token,
    [Parameter(Mandatory)][long]$CutoffBlock
)
$ErrorActionPreference = 'Stop'
$transfer = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
$receipts = @{}
foreach ($file in Get-ChildItem -LiteralPath $ReceiptDirectory -Filter 'receipt-*.response.json') {
    $prefix = $file.FullName.Replace('.response.json','')
    $metadata = Get-Content "$prefix.capture.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if (-not $metadata.succeeded) { continue }
    foreach ($kind in @('request','response')) {
        if ((Get-FileHash "$prefix.$kind.json" -Algorithm SHA256).Hash.ToLowerInvariant() -cne $metadata["$($kind)Sha256"]) { throw 'Changed receipt capture bytes.' }
    }
    $request = Get-Content "$prefix.request.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $reply = Get-Content $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $receipt = $reply.result
    if ($request.method -ne 'eth_getTransactionReceipt' -or $reply.id -cne $request.id -or $receipt.transactionHash -ine $request.params[0] -or $receipt.status -ne '0x1') { throw 'Receipt identity or success mismatch.' }
    $receipts[$receipt.transactionHash] = $receipt
}
$holders = @{}
$events = @{}
$listed = 0
$roundingDifferences = 0
foreach ($file in Get-ChildItem -LiteralPath $SourceDirectory -Filter '0x*.response.json') {
    $holder = $file.Name.Replace('.response.json','')
    $holders[$holder] = [Numerics.BigInteger]::Zero
    $page = Get-Content $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if ($page.ContainsKey('error') -or -not $page.ContainsKey('operations')) { throw 'Invalid discovery page.' }
    foreach ($operation in $page.operations) {
        if ($operation.type -ne 'transfer' -or $operation.tokenInfo.address -ine $Token -or ($operation.from -ine $holder -and $operation.to -ine $holder)) { throw 'Indexed transfer scope mismatch.' }
        $listed++
        $receipt = $receipts[$operation.transactionHash]
        if (-not $receipt) { throw "Missing successful receipt: $($operation.transactionHash)" }
        $matches = @($receipt.logs | Where-Object { $_.address -ieq $Token -and $_.topics.Count -eq 3 -and $_.topics[0] -ceq $transfer -and ('0x'+$_.topics[1].Substring(26)) -ieq $operation.from -and ('0x'+$_.topics[2].Substring(26)) -ieq $operation.to })
        if ($matches.Count -eq 0) { throw 'An indexed transfer has no corresponding receipt log.' }
        if ($matches.Count -eq 1) {
            $raw = [Numerics.BigInteger]::Parse('0'+$matches[0].data.Substring(2), [Globalization.NumberStyles]::HexNumber)
            if ($raw.ToString() -cne [string]$operation.value) { $roundingDifferences++ }
        }
    }
}
foreach ($receipt in $receipts.Values) {
    if ([Convert]::ToInt64($receipt.blockNumber.Substring(2),16) -gt $CutoffBlock) { continue }
    foreach ($log in $receipt.logs) {
        if ($log.address -ine $Token -or $log.topics.Count -ne 3 -or $log.topics[0] -cne $transfer) { continue }
        $from = '0x'+$log.topics[1].Substring(26)
        $to = '0x'+$log.topics[2].Substring(26)
        if (-not $holders.ContainsKey($from) -and -not $holders.ContainsKey($to)) { continue }
        $id = $log.transactionHash+':'+$log.logIndex
        if ($events.ContainsKey($id)) { continue }
        $events[$id] = $true
        $amount = [Numerics.BigInteger]::Parse('0'+$log.data.Substring(2), [Globalization.NumberStyles]::HexNumber)
        if ($holders.ContainsKey($from)) { $holders[$from] -= $amount }
        if ($holders.ContainsKey($to)) { $holders[$to] += $amount }
    }
}
@{
    indexedRows=$listed; successfulTransactions=$receipts.Count; vaultTransfersAtCutoff=$events.Count
    indexedAmountDifferences=$roundingDifferences; cutoffBlock=$CutoffBlock
    netRaw=@($holders.GetEnumerator() | Sort-Object Key | ForEach-Object { @{holder=$_.Key;rawAmount=$_.Value.ToString()} })
    limitation='Net values must be compared with pinned state. Receipt corroboration of indexed rows does not certify complete address history.'
} | ConvertTo-Json -Depth 10
