param(
    [Parameter(Mandatory)][string]$Asset,
    [ValidateRange(1,1000)][int]$TopCount = 3,
    [string]$OutputDirectory = 'archives/holder-funding'
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    function Convert-HexInteger([string]$Value) { [System.Numerics.BigInteger]::Parse(('0' + $Value.Substring(2)), [System.Globalization.NumberStyles]::AllowHexSpecifier) }
    function Get-TransferLogs($Value) {
        if ($Value -is [System.Collections.IDictionary]) {
            if ($Value.topics -and $Value.topics.Count -eq 3 -and $Value.topics[0] -eq '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef') { $Value; return }
            foreach ($key in @('result','logs')) { if ($Value.Contains($key)) { Get-TransferLogs $Value[$key] } }
            return
        }
        if ($Value -is [array]) { foreach ($entry in $Value) { Get-TransferLogs $entry } }
    }
    $assetName = $Asset.ToLowerInvariant()
    $config = Read-Json "reconciliation/$assetName.json"
    $matches = @((Read-Json 'registry/studies.json').GetEnumerator() | Where-Object { $_.Value.movementsFile -eq "$assetName-movements.json" })
    if ($matches.Count -ne 1 -or -not $matches[0].Value.claimReview) { throw 'Exactly one published holder inventory is required.' }
    $study = $matches[0].Value
    $token = $matches[0].Key
    $events = @{}
    $inputHashes = @{}
    foreach ($source in $config.sources | Where-Object chain -EQ Fusion) {
        foreach ($file in Get-ChildItem -LiteralPath $source.directory -Filter $source.pattern | Sort-Object Name) {
            $relative = [System.IO.Path]::GetRelativePath((Get-Location).Path,$file.FullName).Replace('\','/')
            $inputHashes[$relative] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            foreach ($log in Get-TransferLogs (Read-Json $file.FullName)) {
                if ($log.address -ne $token -or $log.removed -eq $true) { continue }
                $block = Convert-HexInteger $log.blockNumber
                if ($block -gt $study.claimReview.referenceBlock) { continue }
                $id = $log.transactionHash + ':' + (Convert-HexInteger $log.logIndex).ToString()
                $event = [ordered]@{ id = $id; block = [long]$block; sender = ('0x' + $log.topics[1].Substring(26)).ToLowerInvariant(); recipient = ('0x' + $log.topics[2].Substring(26)).ToLowerInvariant(); rawAmount = (Convert-HexInteger $log.data).ToString(); evidence = $relative }
                if ($events.ContainsKey($id)) {
                    $prior = $events[$id]
                    if ($prior.block -ne $event.block -or $prior.sender -ne $event.sender -or $prior.recipient -ne $event.recipient -or $prior.rawAmount -ne $event.rawAmount) { throw "Conflicting transfer evidence: $id" }
                    continue
                }
                $events[$id] = $event
            }
        }
    }
    $holders = @()
    $positions = $study.claimReview.positions | Where-Object token -EQ $token | Sort-Object { [System.Numerics.BigInteger]::Parse($_.rawAmount) } -Descending | Select-Object -First $TopCount
    foreach ($position in $positions) {
        $incoming = @($events.Values | Where-Object recipient -EQ $position.holder | Sort-Object block,id)
        $outgoing = @($events.Values | Where-Object sender -EQ $position.holder | Sort-Object block,id)
        $received = [System.Numerics.BigInteger]::Zero
        $sent = [System.Numerics.BigInteger]::Zero
        foreach ($event in $incoming) { $received += [System.Numerics.BigInteger]::Parse($event.rawAmount) }
        foreach ($event in $outgoing) { $sent += [System.Numerics.BigInteger]::Parse($event.rawAmount) }
        if (($received - $sent).ToString() -ne $position.rawAmount) { throw "Funding history does not reconcile to published balance: $($position.holder)" }
        $holders += [ordered]@{ holder = $position.holder; decimals = $position.decimals; rawBalance = $position.rawAmount; rawReceived = $received.ToString(); rawSent = $sent.ToString(); incoming = $incoming; outgoing = $outgoing }
    }
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    $report = [ordered]@{ asset = $study.title; token = $token; referenceBlock = $study.claimReview.referenceBlock; scope = 'Observed Fusion transfers only. Minting is not proof of a source-chain deposit; equal amounts do not prove common ownership or replacement. Reconciled balances do not prove complete custody.'; inputHashes = $inputHashes; holders = $holders }
    $path = Join-Path $OutputDirectory "$assetName.json"
    $report | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $path -Encoding utf8
    Write-Output "Reconciled $($holders.Count) largest $($study.title) holders from $($events.Count) distinct Fusion transfers: $path"
}
finally { Pop-Location }
