$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    $root = 'evidence/2026-10-15-newer-holder-review'
    $manifest = Read-Json "$root/manifest.json"
    foreach ($entry in $manifest.files.GetEnumerator()) {
        Assert-Condition ((Get-FileHash -LiteralPath "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) "Evidence hash mismatch: $($entry.Key)"
    }
    $rows = @{}
    foreach ($file in Get-ChildItem -LiteralPath $root -Filter '*.response.json') {
        foreach ($row in (Read-Json $file.FullName)) { $rows[$row.id] = $row }
    }
    Assert-Condition ($rows.chain.result -eq '0x7f93' -and $rows.snapshot.result.hash -eq '0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f') 'Wrong Fusion snapshot.'
    $factory = '0xda30862467559b8b6e46d587aa9af5f48088c98b'
    $checked = 0
    $blocked = 0
    foreach ($item in (Read-Json 'registry/studies.json').GetEnumerator()) {
        if ($item.Key -notin @('0x9d7005f9175856bda5deb153f86a11153d9d0fa7','0xfc9e57d7543e0591824121fda6461fe2148a5966')) { continue }
        Assert-Condition ($item.Value.claimReview.referenceBlock -eq 15130080) 'Holder snapshot changed.'
        foreach ($position in $item.Value.claimReview.positions | Where-Object token -EQ $item.Key) {
            $id = $item.Value.title + '-blocked-' + $position.holder
            $response = @(Read-Json $position.restrictionSource | Where-Object id -EQ $id)
            Assert-Condition ($response.Count -eq 1 -and $response[0].result -in @(('0x' + ('0' * 64)), ('0x' + ('0' * 63) + '1'))) "Missing flag: $id"
            Assert-Condition ($position.blocked -eq $response[0].result.EndsWith('1')) "Published flag differs: $id"
            $requests = Read-Json ($position.restrictionSource.Replace('.response.json','.request.json'))
            $request = @($requests | Where-Object id -EQ $id)[0]
            $expected = '0x86c58d3e' + $position.token.Substring(2).PadLeft(64,'0') + $position.holder.Substring(2).PadLeft(64,'0')
            Assert-Condition ($request.method -eq 'eth_call' -and $request.params[1] -eq '0xe6dde0' -and $request.params[0].to -eq $factory -and $request.params[0].data -eq $expected) 'Flag targets a different token, holder or block.'
            if ($position.blocked) { $blocked++ }
            $checked++
        }
    }
    Assert-Condition ($checked -eq 523 -and $blocked -eq 1) 'Published newer restriction inventory changed; review findings.'
    Assert-Condition ($rows['singleTokenBlocked(address,address)'].result.EndsWith('1') -and $rows['allTokenBlocked(address)'].result -eq ('0x' + ('0' * 64)) -and $rows['USDC-paused'].result -eq ('0x' + ('0' * 64))) 'Account-specific restriction differs from the reported scope.'
    Assert-Condition ($rows.owner.result -eq ('0x' + '1d9a17f3097cee804b3c3717222a22b8eb55bcb1'.PadLeft(64,'0')) -and $rows['isController(address)'].result.EndsWith('1')) 'Reported controller authority changed.'
    $blockTx = $rows['0xb0cc36b54b76e32fd982e9de51bd781f741c95310f7cd8cde2d0c103cda0d85c-eth_getTransactionByHash'].result
    $blockInput = '0x167d42fe' + 'fc9e57d7543e0591824121fda6461fe2148a5966'.PadLeft(64,'0') + 'e4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b'.PadLeft(64,'0') + ('0' * 63) + '1'
    Assert-Condition ($blockTx.to -eq $factory -and $blockTx.from -eq '0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1' -and $blockTx.input -eq $blockInput) 'Historical USDC block call differs.'
    Assert-Condition ($rows['controller-clear-block'].result -eq '0x' -and $rows['control-clear-block'].error.message -eq 'execution reverted: Factory: Only all token controller!') 'Permission simulation changed.'
    Assert-Condition ($rows['blocked-after-simulation'].result.EndsWith('1')) 'Block readback differs.'
    $trace = $rows['restricted-transfer'].result
    Assert-Condition ($trace.error -eq 'execution reverted' -and $trace.calls.Count -eq 3) 'Restriction trace changed.'
    Assert-Condition ($trace.calls[1].to -eq $factory -and $trace.calls[1].input -eq ('0x86c58d3e' + 'fc9e57d7543e0591824121fda6461fe2148a5966'.PadLeft(64,'0') + 'e4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b'.PadLeft(64,'0')) -and $trace.calls[1].output.EndsWith('1')) 'Transfer trace does not confirm the holder block.'
    $receipts = 0
    foreach ($row in $rows.Values | Where-Object { $_.id.EndsWith('-eth_getTransactionReceipt') }) {
        $receipt = $row.result
        $tx = $rows[$receipt.transactionHash + '-eth_getTransactionByHash'].result
        $header = $rows[$receipt.blockNumber].result
        Assert-Condition ($receipt.status -eq '0x1' -and $tx.blockHash -eq $receipt.blockHash -and $header.hash -eq $receipt.blockHash) 'Selected transaction lacks matching successful receipt and header.'
        $receipts++
    }
    Write-Output "Verified $checked newer-holder flags ($blocked blocked), restriction trace, permission simulations and $receipts successful transaction receipts. No blocked balances deducted."
}
finally { Pop-Location }
