$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    function Normalize-Runtime([string]$Code) { [regex]::Replace($Code.Trim().Replace('0x',''), 'a2646970667358221220[0-9a-f]{64}64736f6c634300080c0033', ('a2646970667358221220' + ('0' * 64) + '64736f6c634300080c0033')) }
    $root = 'evidence/2026-10-15-fusion-migration-state'
    $manifest = Read-Json "$root/manifest.json"
    foreach ($entry in $manifest.files.GetEnumerator()) {
        Assert-Condition ((Get-FileHash -LiteralPath "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) "Evidence hash mismatch: $($entry.Key)"
    }
    $rows = @{}
    foreach ($file in Get-ChildItem -LiteralPath $root -Filter '*.response.json') {
        foreach ($row in (Read-Json $file.FullName)) { if ($row.id) { $rows[$row.id] = $row } }
    }
    Assert-Condition ($rows.chain.result -eq '0x7f93' -and $rows.head.result.number -eq '0xe6dde0' -and $rows.head.result.hash -eq '0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f') 'Fusion reference changed.'
    $compiledParent = Get-Content evidence/2026-10-13-usdc-fusion-source-match/compiled-runtime.hex -Raw -Encoding utf8
    $compiledChild = Get-Content evidence/2026-10-13-usdc-fusion-child-code/compiled-slice-runtime.hex -Raw -Encoding utf8
    foreach ($comparison in (Read-Json "$root/runtime-comparison.json")) {
        $compiled = if ($comparison.contract -eq 'Slice') { $compiledChild } else { $compiledParent }
        Assert-Condition ((Normalize-Runtime $rows[$comparison.id].result) -eq (Normalize-Runtime $compiled)) "Compiled runtime mismatch: $($comparison.id)"
        Assert-Condition ((Get-FileHash -LiteralPath $comparison.source -Algorithm SHA256).Hash -eq $comparison.sourceSha256) 'Comparison source changed.'
    }
    $holder = '0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e'
    $studies = Read-Json 'registry/studies.json'
    $checked = 0
    foreach ($item in $studies.GetEnumerator()) {
        if ($item.Key -notin @('0x9d7005f9175856bda5deb153f86a11153d9d0fa7','0xfc9e57d7543e0591824121fda6461fe2148a5966')) { continue }
        $study = $item.Value
        if (-not $study.claimReview) { continue }
        Assert-Condition ($study.claimReview.referenceBlock -eq 15130080) 'Restriction snapshot no longer matches claims.'
        foreach ($position in $study.claimReview.positions) {
            if ($position.token -eq $item.Key) { continue }
            $id = $study.title + '-blocked-' + $position.holder
            $response = @(Read-Json $position.restrictionSource | Where-Object id -EQ $id)
            Assert-Condition ($response.Count -eq 1 -and $response[0].result -in @(('0x' + ('0' * 64)), ('0x' + ('0' * 63) + '1'))) 'Missing or invalid flag evidence.'
            $blocked = $response[0].result.EndsWith('1')
            Assert-Condition ($position.blocked -eq $blocked) "Published restriction differs: $id"
            $requests = Read-Json ($position.restrictionSource.Replace('.response.json','.request.json'))
            $request = @($requests | Where-Object id -EQ $id)[0]
            $parent = if ($study.title -eq 'BUSD') { '0xd169bfa08ae2c9bea4123a8760ba058afd647ed2' } else { '0x6b52048a01c41d1625a6893c80fbe4aa2c22bb54' }
            Assert-Condition ($request.method -eq 'eth_call' -and $request.params[1] -eq '0xe6dde0' -and $request.params[0].to -eq $parent -and $request.params[0].data -eq ('0xe5962195' + $position.holder.Substring(2).PadLeft(64,'0'))) 'Flag call targets a different account or block.'
            $checked++
        }
        $asset = $study.title.ToLowerInvariant()
        Assert-Condition ($rows["$asset-simulate-unblockUser(address)"].result -eq '0x' -and $rows["$asset-simulate-burn(address,uint256)"].result -eq '0x') 'Controller simulation result changed.'
        Assert-Condition ($rows["$asset-unchanged-blocked(address)"].result.EndsWith('1') -and $rows["$asset-unchanged-balanceOf(address)"].result -eq $rows["$asset-balanceOf(address)-$holder"].result) 'Simulation changed observed snapshot values.'
    }
    foreach ($name in @('transactions','extra-transactions')) {
        $captured = Read-Json "$root/$name.response.json"
        $headersName = if ($name -eq 'transactions') { 'headers' } else { 'extra-headers' }
        $headers = Read-Json "$root/$headersName.response.json"
        foreach ($row in $captured | Where-Object { $_.id.EndsWith('-eth_getTransactionReceipt') }) {
            $receipt = $row.result
            $tx = @($captured | Where-Object id -EQ ($receipt.transactionHash + '-eth_getTransactionByHash'))[0].result
            $header = @($headers | Where-Object { $_.result.hash -eq $receipt.blockHash })[0].result
            Assert-Condition ($receipt.status -eq '0x1' -and $tx.blockHash -eq $receipt.blockHash -and $null -ne $header) 'Migration transaction not corroborated by successful receipt and header.'
        }
    }
    Write-Output "Verified $checked published legacy restrictions, four runtime comparisons and selected migration receipts. Blocking remains distinct from burning and no liability deduction is applied."
}
finally { Pop-Location }
