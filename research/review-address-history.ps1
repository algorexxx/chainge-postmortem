param([Parameter(Mandatory)][string]$CapturePrefix)
$ErrorActionPreference = 'Stop'
function Read-Hex([string]$Value) { [System.Numerics.BigInteger]::Parse(('0' + $Value.Replace('0x','')), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
$requests = Get-Content "$CapturePrefix.request.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$responses = Get-Content "$CapturePrefix.response.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$query = @{}
$results = @{}
foreach ($request in $requests) { $query[$request.id] = $request }
foreach ($response in $responses) {
    Assert-Condition (-not $response.ContainsKey('error') -and $response.ContainsKey('result')) 'RPC response failed.'
    Assert-Condition (-not $results.ContainsKey($response.id)) 'Duplicate response identifier.'
    $results[$response.id] = $response.result
}
$incomingFilter = $query['in'].params[0]
$outgoingFilter = $query['out'].params[0]
$holderWord = $incomingFilter.topics[2]
$token = $incomingFilter.address
$block = $incomingFilter.toBlock
$transferTopic = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
Assert-Condition ($holderWord -eq $outgoingFilter.topics[1] -and $token -eq $outgoingFilter.address) 'Address or token filters differ.'
Assert-Condition ($incomingFilter.fromBlock -eq '0x0' -and $outgoingFilter.fromBlock -eq '0x0' -and $outgoingFilter.toBlock -eq $block -and $block -match '^0x[0-9a-f]+$') 'History must start at zero and end at the same numeric block.'
Assert-Condition ($query['balance'].params[0].to -eq $token -and $query['balance'].params[0].data -eq ('0x70a08231' + $holderWord.Substring(2)) -and $query['balance'].params[1] -eq $block) 'Balance query does not match history.'
$totals = @{}
$recipients = @{}
foreach ($direction in @('in','out')) {
    Assert-Condition ($results.ContainsKey($direction)) 'Missing history response.'
    $seen = @{}
    $total = [bigint]0
    foreach ($log in $results[$direction]) {
        $key = "$($log.transactionHash):$($log.logIndex)"
        Assert-Condition (-not $seen.ContainsKey($key)) 'Duplicate event in one history direction.'
        $seen[$key] = $true
        $topicIndex = if ($direction -eq 'in') { 2 } else { 1 }
        Assert-Condition (-not $log.removed -and $log.address -eq $token -and $log.topics[0] -eq $transferTopic -and $log.topics[$topicIndex] -eq $holderWord -and (Read-Hex $log.blockNumber) -le (Read-Hex $block)) 'Event falls outside the requested history.'
        $amount = Read-Hex $log.data
        $total += $amount
        if ($direction -eq 'out') {
            $recipient = '0x' + $log.topics[2].Substring(26)
            if (-not $recipients.ContainsKey($recipient)) { $recipients[$recipient] = [bigint]0 }
            $recipients[$recipient] += $amount
        }
    }
    $totals[$direction] = $total
}
$balance = Read-Hex $results['balance']
Assert-Condition (($totals['in'] - $totals['out']) -eq $balance) 'Transfer history does not reconcile with pinned balance.'
[ordered]@{
    token = $token
    address = '0x' + $holderWord.Substring(26)
    block = $block
    incomingCount = @($results['in']).Count
    outgoingCount = @($results['out']).Count
    incomingRaw = $totals['in'].ToString()
    outgoingRaw = $totals['out'].ToString()
    balanceRaw = $balance.ToString()
    recipients = @($recipients.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { [ordered]@{ address = $_.Key; amountRaw = $_.Value.ToString() } })
} | ConvertTo-Json -Depth 5
