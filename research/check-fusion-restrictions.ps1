param([Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$Asset)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    function Read-Json([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable }
    $review = Read-Json "research/assets/$Asset-fusion-state.json"
    $studies = Read-Json 'registry/studies.json'
    $study = @($studies.Values | Where-Object movementsFile -EQ "$Asset-movements.json")
    if ($study.Count -ne 1 -or -not $study[0].claimReview) { throw 'One published holder review is required.' }
    $claims = $study[0].claimReview
    if ($review.referenceBlock -ne $claims.referenceBlock) { throw 'Restriction review and holder inventory use different blocks.' }
    $checked = 0
    foreach ($position in $claims.positions) {
        $rule = $review.contracts[$position.token]
        if (-not $rule -or $rule.kind -notin @('legacy-parent','newer-factory')) { throw 'Every published position requires an explicit restriction contract.' }
        $responsePath = $position.restrictionSource
        $requestPath = $responsePath.Replace('.response.json','.request.json')
        $word = $position.holder.Substring(2).PadLeft(64,'0')
        $data = if ($rule.kind -eq 'legacy-parent') { '0xe5962195' + $word } else { $review.factorySelector + $position.token.Substring(2).PadLeft(64,'0') + $word }
        $requests = @(Read-Json $requestPath | Where-Object { $_.method -eq 'eth_call' -and $_.params[0].to -eq $rule.address -and $_.params[0].data -eq $data -and $_.params[1] -eq ('0x' + ([long]$review.referenceBlock).ToString('x')) })
        if ($requests.Count -eq 0) { throw 'Restriction query does not bind the holder, token, contract and block.' }
        foreach ($request in $requests) {
            $response = @(Read-Json $responsePath | Where-Object id -EQ $request.id)
            if ($response.Count -ne 1 -or $response[0].error -or $response[0].result -notin @(('0x' + ('0' * 64)),('0x' + ('0' * 63) + '1'))) { throw 'Restriction result is unavailable or not a canonical Boolean.' }
            if ($position.blocked -ne $response[0].result.EndsWith('1')) { throw 'Published restriction differs from the retained result.' }
        }
        $checked++
    }
    foreach ($comparison in $review.runtimeComparisons) {
        $response = @(Read-Json $comparison.response | Where-Object id -EQ $comparison.id)
        if ($response.Count -ne 1 -or -not $response[0].result) { throw 'Missing runtime response.' }
        $compiled = Get-Content -LiteralPath $comparison.compiled -Raw -Encoding utf8
        $pattern = 'a2646970667358221220[0-9a-f]{64}64736f6c634300080c0033'
        $replacement = 'a2646970667358221220' + ('0' * 64) + '64736f6c634300080c0033'
        $deployed = [regex]::Replace($response[0].result.Replace('0x','').Trim(),$pattern,$replacement)
        $expected = [regex]::Replace($compiled.Replace('0x','').Trim(),$pattern,$replacement)
        if ($deployed -cne $expected) { throw 'Runtime does not match the stated compiled source after exact compiler metadata normalization.' }
    }
    Write-Output "Verified $checked $Asset holder restrictions and $($review.runtimeComparisons.Count) runtime comparisons. Blocking does not cancel balances."
}
finally { Pop-Location }
