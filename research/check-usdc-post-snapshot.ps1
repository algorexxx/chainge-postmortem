param([string]$ReportDirectory = 'archives/workflow-check/usdc')
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Convert-Hex([string]$Value) { [bigint]::Parse('0' + $Value.Substring(2), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    function Assert-Transfer($Receipt, [string]$From, [string]$To, [string]$Amount) {
        Assert-Condition ($Receipt.status -eq '0x1') 'Receipt did not succeed.'
        $matches = @($Receipt.logs | Where-Object {
            $_.address -eq $token -and $_.topics.Count -eq 3 -and $_.topics[0] -eq $transfer -and
            $_.topics[1].EndsWith($From.Substring(2)) -and $_.topics[2].EndsWith($To.Substring(2)) -and
            (Convert-Hex $_.data) -eq [bigint]$Amount
        })
        Assert-Condition ($matches.Count -eq 1) "Expected one exact USDC transfer: $From -> $To, $Amount"
    }
    $rpc = 'evidence/2026-10-15-usdc-post-snapshot-rpc'
    $explorer = 'evidence/2026-10-15-usdc-post-snapshot-explorer'
    $onward = 'evidence/2026-10-15-usdc-post-snapshot-onward'
    $label = 'evidence/2026-10-15-usdc-post-snapshot-label'
    foreach ($directory in @($rpc, $explorer, $onward, $label)) {
        $manifest = Read-Json "$directory/manifest.json"
        foreach ($entry in $manifest.files.GetEnumerator()) {
            Assert-Condition ((Get-FileHash -LiteralPath "$directory/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) "Evidence hash mismatch: $directory/$($entry.Key)"
        }
    }
    $token = '0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48'
    $transfer = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
    $vault = '0x456325f2ac7067234dd71e01bebe032b0255e039'
    $recipients = @('0x88e6a0c2ddd26feeb64f039a2c41296fcb3f5640','0x3416cf6c708da44db2624d63ea0aaef7113527c6','0x186fa8ffffb1b744f875e99239716b3f6acddfc8')
    $amounts = @('12938852','542761951','634222907')
    $receiptFiles = @('005','049','016')
    $ethReturns = @('2989706321703827','131560072533994001')
    $ledger = Read-Json 'registry/usdc-movements.json'
    foreach ($i in 0..2) {
        $tx = (Read-Json "$rpc/tx-$i-0001.response.json").result
        Assert-Condition ($tx.from -eq $vault) 'Original transaction sender differs.'
        $receipt = @((Read-Json "evidence/2026-10-13-usdc-investigation/ethereum-corroboration-v3-$($receiptFiles[$i]).response.json") | Where-Object { $_.result.transactionHash -eq $tx.hash })[0].result
        Assert-Transfer $receipt $vault $recipients[$i] $amounts[$i]
        $header = (Read-Json "$rpc/header-$($receipt.blockNumber)-0001.response.json").result
        Assert-Condition ($header.hash -eq $receipt.blockHash -and $tx.blockHash -eq $header.hash) 'Original transaction/header mismatch.'
        $movement = @($ledger.cases | Where-Object transaction -EQ $tx.hash)[0]
        Assert-Condition (([DateTimeOffset]$movement.timestamp).ToUnixTimeSeconds() -eq (Convert-Hex $header.timestamp)) 'Ledger timestamp mismatch.'
        $funding = (Read-Json "$rpc/funding-receipt-$i-0001.response.json").result
        $attempt = if ($i -eq 2) { '0002' } else { '0001' }; $fundingTx = (Read-Json "$rpc/funding-tx-$i-$attempt.response.json").result
        Assert-Condition ($funding.status -eq '0x1' -and $fundingTx.from -eq $vault -and $fundingTx.hash -eq $funding.transactionHash -and $fundingTx.blockHash -eq $funding.blockHash) 'Funding sender or receipt mismatch.'
        $incoming = @($funding.logs | Where-Object { $_.address -eq $token -and $_.topics.Count -eq 3 -and $_.topics[2].EndsWith($vault.Substring(2)) })
        Assert-Condition ($incoming.Count -eq 1 -and (Convert-Hex $incoming[0].data) -eq [bigint]$amounts[$i]) 'Funding amount mismatch.'
        Assert-Condition ((Convert-Hex $funding.blockNumber) -gt 23524761 -and (Convert-Hex $funding.blockNumber) -lt (Convert-Hex $receipt.blockNumber)) 'Funding is not after the reference and before payment.'
        Assert-Condition ($movement.review.reconstructedBalanceBeforeRaw -eq $amounts[$i] -and $movement.review.reconstructedBalanceAfterRaw -eq '0') 'Retained balance reconstruction differs.'
        if ($i -lt 2) {
            $calls = Read-Json "$explorer/internal-$i.response.json"
            $returns = @($calls.items | Where-Object { $_.to.hash -eq $vault -and $_.success -eq $true -and $_.value -eq $ethReturns[$i] })
            Assert-Condition ($null -eq $calls.next_page_params -and $returns.Count -eq 1) 'Explorer ETH return or pagination differs.'
        }
    }
    $payment = (Read-Json "$onward/onward-receipt-0001.response.json").result
    Assert-Transfer $payment $recipients[2] '0x445f16314284b43dfa1fd3cd77b9dea4a1bebd97' $amounts[2]
    $header = (Read-Json "$onward/onward-header-0001.response.json").result
    Assert-Condition ($header.hash -eq $payment.blockHash) 'Onward header mismatch.'
    $originalHeader = (Read-Json "$rpc/header-0x1733448-0001.response.json").result
    Assert-Condition (((Convert-Hex $header.timestamp) - (Convert-Hex $originalHeader.timestamp)) -eq 108) 'Onward delay differs.'
    $balanceRequest = (Read-Json "$onward/recipient-balance-0001.request.json")
    Assert-Condition ($balanceRequest.params[1] -eq $header.number -and $balanceRequest.params[0].to -eq $token -and $balanceRequest.params[0].data -eq ('0x70a08231' + $recipients[2].Substring(2).PadLeft(64,'0'))) 'Balance request is not pinned to the intermediary and onward block.'
    Assert-Condition ((Convert-Hex (Read-Json "$onward/recipient-balance-0001.response.json").result) -eq 0) 'Intermediary balance is not zero.'
    $burns = @(Import-Csv -LiteralPath "$ReportDirectory/fusion-purposes.csv" -Encoding utf8 | Where-Object direction -EQ 'burn')
    $observations = (Read-Json "$ReportDirectory/routing-review.json").observations
    foreach ($amount in $amounts) {
        $exact = @($burns | Where-Object { [decimal]$_.amount * 1000000 -eq [decimal]$amount })
        Assert-Condition ($exact.Count -eq 0) 'New exact burn match requires review.'
    }
    $memoMatches = @($observations | Where-Object { $_.recipient -in ($recipients + $vault) -or $_.memoAmountRaw -in $amounts })
    Assert-Condition ($memoMatches.Count -eq 0) 'New routing candidate requires review.'
    Write-Output "Verified three late USDC cases, equal post-snapshot funding, explorer ETH returns, onward receipt and zero balance. No exact amount in $($burns.Count) retained burns or destination/amount hit in $($observations.Count) decoded memos. This is not proof of absent settlement."
}
finally { Pop-Location }


