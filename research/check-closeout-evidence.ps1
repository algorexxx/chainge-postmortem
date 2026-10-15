$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Assert-Condition([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
    function Read-Hex([string]$Value) { [bigint]::Parse(('0' + $Value.Replace('0x','')), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    $roots = @('evidence/2026-10-15-busd-gap-localization', 'evidence/2026-10-15-usdc-terminal-closeout', 'evidence/2026-10-15-terminal-recipient-receipts')
    $receipts = @{}
    $headers = @{}
    foreach ($root in $roots) {
        foreach ($entry in (Read-Json "$root/manifest.json").files.GetEnumerator()) {
            Assert-Condition ((Get-FileHash "$root/$($entry.Key)" -Algorithm SHA256).Hash -eq $entry.Value) 'Closeout evidence hash changed.'
        }
        foreach ($file in Get-ChildItem $root -Filter '*.response.json') {
            if ($file.Name -like '*label*') { continue }
            foreach ($row in (Read-Json $file.FullName)) {
                $result = $row.result
                if ($result -isnot [System.Collections.IDictionary]) { continue }
                if ($result.Contains('logs') -and $result.transactionHash) { $receipts[$result.transactionHash] = $result }
                if ($result.Contains('timestamp') -and $result.number) { $headers[$result.hash] = $result }
            }
        }
    }
    $topic = '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef'
    $transfers = @(
        @('0x9f61b9cce465032ae57b4e9dda7bc857cbe5b04fa977461011a7e17210bd99f4','0xe9e7cea3dedca5984780bafc599bd69add087d56','8a4aa176007196d48d39c89402d3753c39ae64c1','a74d49d38df7092688686973c468c94a542294b3','1107024317425808905'),
        @('0x5215e0d6f88bfd1ab376f9ff8511d1a5a1e9b8e385ce4096d303444078fdb7dc','0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48','53e2cb064e2de9594584c9dba24ef071f110dd28','16a37f695c797ffd0c0215756a5c3ceb4d5e59c9','18150000000'),
        @('0x0709eeaa2135b716b4b8f6bed292a3780df32ec169781d08d590fa2d25bf5eba','0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48','16a37f695c797ffd0c0215756a5c3ceb4d5e59c9','859bfc94624f1f36ae6aed248ee5347f4ee86552','18150000000')
    )
    foreach ($transfer in $transfers) {
        $receipt = $receipts[$transfer[0]]
        Assert-Condition ($receipt.status -eq '0x1' -and $headers.ContainsKey($receipt.blockHash)) 'Missing successful receipt/header.'
        Assert-Condition ($headers[$receipt.blockHash].number -eq $receipt.blockNumber) 'Receipt/header block mismatch.'
        $matches = @($receipt.logs | Where-Object {
            $_.address -eq $transfer[1] -and $_.topics.Count -eq 3 -and $_.topics[0] -eq $topic -and
            $_.topics[1].Substring(26) -eq $transfer[2] -and $_.topics[2].Substring(26) -eq $transfer[3] -and
            (Read-Hex $_.data) -eq [bigint]::Parse($transfer[4]) -and -not $_.removed
        })
        Assert-Condition ($matches.Count -eq 1) 'Expected exact token transfer not found uniquely.'
    }
    $busdRoot = $roots[0]
    $before = Read-Hex (Read-Json "$busdRoot/balance-35938492.response.json").result
    $after = Read-Hex (Read-Json "$busdRoot/balance-35938493.response.json").result
    $cutoff = Read-Hex (Read-Json "$busdRoot/balance-63769742.response.json").result
    Assert-Condition (($before - $after) -eq [bigint]::Parse('1107024317425808905') -and $after -eq $cutoff -and $after -eq [bigint]::Parse('80222660182393847')) 'BUSD boundary state does not reconcile.'
    foreach ($block in @(35938492,35938493,63769742)) {
        $request = Read-Json "$busdRoot/balance-$block.request.json"
        Assert-Condition ($request.method -eq 'eth_call' -and $request.params[0].to -eq $transfers[0][1] -and $request.params[0].data -eq ('0x70a08231' + $transfers[0][2].PadLeft(64,'0')) -and (Read-Hex $request.params[1]) -eq $block) 'BUSD state request scope differs.'
    }
    $busdIdentity = @(Read-Json "$busdRoot/boundary-logs.response.json" | Where-Object id -EQ 'chain')
    Assert-Condition ($busdIdentity.Count -eq 1 -and $busdIdentity[0].result -eq '0x38') 'BSC chain identity differs.'
    $usdcRoot = $roots[1]
    $balances = @{}
    foreach ($row in (Read-Json "$usdcRoot/final-balances.response.json")) { $balances[$row.id] = Read-Hex $row.result }
    Assert-Condition ($balances['24033557'] -eq 0 -and $balances['24033558'] -eq 18150000000 -and $balances['0x18fc028'] -eq 18150000000) 'USDC retained balance differs.'
    foreach ($request in (Read-Json "$usdcRoot/final-balances.request.json")) {
        Assert-Condition ($request.method -eq 'eth_call' -and $request.params[0].to -eq $transfers[1][1] -and $request.params[0].data -eq ('0x70a08231' + '859bfc94624f1f36ae6aed248ee5347f4ee86552'.PadLeft(64,'0'))) 'USDC balance request scope differs.'
        Assert-Condition ((Read-Hex $request.params[1]) -eq $(if ($request.id -eq '0x18fc028') { 26198056 } else { [int]$request.id })) 'USDC balance request block differs.'
    }
    $headRows = Read-Json "$usdcRoot/balance-head.response.json"
    $identity = @($headRows | Where-Object id -EQ 'chain')
    $head = @($headRows | Where-Object id -EQ 'head')
    Assert-Condition ($identity.Count -eq 1 -and $identity[0].result -eq '0x1' -and $head.Count -eq 1 -and $head[0].result.number -eq '0x18fc028' -and $head[0].result.hash -eq '0xb142f8a088eee1fc3d5029ce05fa235c242ce668ba269820603e7d40171632ee') 'Retained Ethereum balance cutoff differs.'
    Write-Output 'Closeout receipts, headers and pinned boundary balances verified; ownership and settlement remain separate questions.'
}
finally { Pop-Location }
