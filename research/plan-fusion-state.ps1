param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset,
    [Parameter(Mandatory)][string]$Endpoint,
    [ValidateRange(1,100)][int]$BatchSize = 20
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $claims = Get-Content "archives/reconciliation/$Asset/claim-positions.json" -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    $configurationHash = (Get-FileHash "reconciliation/$Asset.json" -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($claims.configurationSha256 -cne $configurationHash) { throw 'Reconcile the current configuration first.' }
    if (-not $claims.claimCutoffBlock -or -not $claims.claimIntervals.Count) { throw 'Explicit Fusion cutoff and reviewed standard-token intervals are required.' }
    $block = '0x' + ([long]$claims.claimCutoffBlock).ToString('x')
    $requests = [Collections.Generic.List[object]]::new()
    $requests.Add(@{ jsonrpc = '2.0'; id = 'reference'; method = 'eth_getBlockByNumber'; params = @($block,$false) })
    foreach ($token in @($claims.claimIntervals.Keys | Sort-Object)) {
        $requests.Add(@{ jsonrpc = '2.0'; id = "code-$token"; method = 'eth_getCode'; params = @($token,$block) })
    }
    $reviewPath = "research/assets/$Asset-fusion-state.json"
    $review = if (Test-Path $reviewPath) { Get-Content $reviewPath -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable } else { $null }
    foreach ($position in $claims.positions | Where-Object { $claims.claimIntervals.ContainsKey($_.token) }) {
        if (-not $position.historyConsistent -or [bigint]$position.rawAmount -le 0) { throw 'Resolve inconsistent standard-token positions before claiming a holder inventory.' }
        $word = $position.holder.Substring(2).PadLeft(64,'0')
        $id = $position.token + '-' + $position.holder
        $requests.Add(@{ jsonrpc = '2.0'; id = "balance-$id"; method = 'eth_call'; params = @(@{ to = $position.token; data = '0x70a08231' + $word },$block) })
        if ($review -and $review.contracts.ContainsKey($position.token)) {
            $rule = $review.contracts[$position.token]
            if ($rule.kind -notin @('legacy-parent','newer-factory') -or $review.referenceBlock -ne $claims.claimCutoffBlock) { throw 'Review restriction contract mapping and cutoff before planning.' }
            $data = if ($rule.kind -eq 'legacy-parent') { '0xe5962195' + $word } else { $review.factorySelector + $position.token.Substring(2).PadLeft(64,'0') + $word }
            $requests.Add(@{ jsonrpc = '2.0'; id = "blocked-$id"; method = 'eth_call'; params = @(@{ to = $rule.address; data = $data },$block) })
        }
    }
    $queries = for ($index = 0; $index -lt $requests.Count; $index += $BatchSize) {
        $batch = @($requests.GetRange($index,[Math]::Min($BatchSize,$requests.Count - $index)))
        @{ name = 'state-' + $index.ToString('D5'); request = $batch }
    }
    $plan = @{ endpoint = $Endpoint; expectedChainId = '0x7f93'; userAgent = 'Chainge-public-research/1.0'; requestDelay = '00:00:02'; timeout = '00:01:00'; maximumConsecutiveFailures = 3; queries = @($queries) }
    $directory = 'archives/capture-plans/' + $Asset
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    [IO.File]::WriteAllText((Join-Path $directory 'fusion-state.json'), ($plan | ConvertTo-Json -Depth 20) + "`n", [Text.UTF8Encoding]::new($false))
    Write-Output "Prepared $($requests.Count) pinned state/code queries in ignored $directory/fusion-state.json. Do not commit connection settings."
    Write-Output 'The capture runner records its endpoint in metadata. For private RPC, keep raw output in archives/ and publish only a reviewed public copy with connection details omitted.'
    if (-not $review) { Write-Warning 'Restriction mapping is missing. Balance capture alone does not complete the Fusion state review.' }
}
finally { Pop-Location }
