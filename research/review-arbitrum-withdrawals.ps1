param(
    [Parameter(Mandatory)][string]$ChildDirectory,
    [Parameter(Mandatory)][string]$ParentDirectory,
    [Parameter(Mandatory)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
function Read-Receipts([string]$Directory) {
    foreach ($file in Get-ChildItem -LiteralPath $Directory -Filter '*.response.json') {
        $requestPath = $file.FullName.Replace('.response.json','.request.json')
        if (-not (Test-Path -LiteralPath $requestPath)) { continue }
        $requests = @(Get-Content $requestPath -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable)
        foreach ($reply in @(Get-Content $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable)) {
            $receipt = $reply.result
            if ($receipt -isnot [Collections.IDictionary] -or -not $receipt.transactionHash -or -not $receipt.logs) { continue }
            if ($receipt.status -ne '0x1') { continue }
            $request = @($requests | Where-Object { $_.method -eq 'eth_getTransactionReceipt' -and $_.id -ceq $reply.id })
            if ($request.Count -ne 1 -or $request[0].params[0] -ine $receipt.transactionHash) { throw 'Receipt does not match its captured request.' }
            foreach ($log in $receipt.logs) {
                if ($log.removed -eq $true -or $log.transactionHash -ine $receipt.transactionHash -or $log.blockHash -ine $receipt.blockHash) { throw 'Receipt/log identity mismatch.' }
            }
            @{ receipt=$receipt; source=[IO.Path]::GetRelativePath((Get-Location).Path,$file.FullName).Replace('\','/'); sha256=(Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
        }
    }
}
function Read-Word([string]$Data,[int]$Index) {
    if ($Data -notmatch '^0x([0-9a-fA-F]{64})+$' -or $Data.Length -lt (2 + 64 * ($Index + 1))) { throw 'Malformed ABI words.' }
    $Data.Substring(2 + 64 * $Index,64).ToLowerInvariant()
}
function Read-Bytes([string]$Data,[int]$OffsetWord) {
    $offset = [Convert]::ToInt32((Read-Word $Data $OffsetWord),16)
    if ($offset % 32 -ne 0 -or $offset -lt (($OffsetWord + 1) * 32)) { throw 'Invalid dynamic ABI offset.' }
    $length = [Convert]::ToInt32((Read-Word $Data ($offset / 32)),16)
    if ($length -lt 0 -or $Data.Length -lt (2 + 2 * ($offset + 32 + $length))) { throw 'Truncated dynamic ABI bytes.' }
    '0x' + $Data.Substring(2 + 2 * ($offset + 32),2 * $length).ToLowerInvariant()
}
$parents = @(Read-Receipts $ParentDirectory)
$matches = @()
$unmatched = @()
foreach ($child in Read-Receipts $ChildDirectory) {
    foreach ($message in $child.receipt.logs | Where-Object { $_.address -ieq '0x0000000000000000000000000000000000000064' -and $_.topics.Count -eq 4 -and $_.topics[0] -eq '0x3e7aafa77dbf186b7fd488006beff893744caa3c4f6f299e8a709fa2087374fc' }) {
        $body = Read-Bytes $message.data 5
        if (-not $body.StartsWith('0x2e567b36')) { continue }
        $arguments = '0x' + $body.Substring(10)
        $tokenWord = Read-Word $arguments 0
        $fromWord = Read-Word $arguments 1
        $toWord = Read-Word $arguments 2
        if (@($tokenWord,$fromWord,$toWord | Where-Object { -not $_.StartsWith(('0' * 24)) }).Count) { throw 'Noncanonical token withdrawal address.' }
        $amount = Read-Word $arguments 3
        $caller = Read-Word $message.data 0
        $position = $message.topics[3].ToLowerInvariant()
        $candidates = @()
        foreach ($parent in $parents) {
            $executions = @($parent.receipt.logs | Where-Object { $_.topics.Count -eq 4 -and $_.topics[0] -eq '0x20af7f3bbfe38132b8900ae295cd9c8d1914be7052d061a511f3f728dab18964' -and $_.topics[1] -ieq $message.topics[1] -and $_.topics[2] -ieq ('0x'+$caller) -and $_.data -ieq $position })
            if (-not $executions.Count) { continue }
            $calls = @($parent.receipt.logs | Where-Object { $_.topics.Count -eq 3 -and $_.topics[0] -eq '0x2d9d115ef3e4a606d698913b1eae831a3cdfe20d9a83d48007b0526749c3d466' -and $_.topics[2] -ieq $message.topics[1] -and (Read-Bytes $_.data 1) -ceq $body })
            $payments = @($parent.receipt.logs | Where-Object { $_.address -ieq ('0x'+$tokenWord.Substring(24)) -and $_.topics.Count -eq 3 -and $_.topics[0] -eq '0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef' -and $_.topics[1] -ieq $message.topics[1] -and $_.topics[2] -ieq ('0x'+$toWord) -and $_.data -ieq ('0x'+$amount) })
            if ($calls.Count -ne 1 -or $payments.Count -ne 1 -or $executions.Count -ne 1) { throw 'Ambiguous or missing executed message/payment.' }
            $candidates += @{ childTransaction=$child.receipt.transactionHash; parentTransaction=$parent.receipt.transactionHash; position=$position; token=('0x'+$tokenWord.Substring(24)); sender=('0x'+$fromWord.Substring(24)); recipient=('0x'+$toWord.Substring(24)); rawAmount=([Numerics.BigInteger]::Parse(('0'+$amount),[Globalization.NumberStyles]::HexNumber)).ToString(); messageBody=$body; childEvidence=$child.source; parentEvidence=$parent.source; childSha256=$child.sha256; parentSha256=$parent.sha256 }
        }
        if ($candidates.Count -gt 1) { throw 'More than one parent execution for a withdrawal position.' }
        if ($candidates.Count) { $matches += $candidates[0] } else { $unmatched += $child.receipt.transactionHash }
    }
}
$report = @{ scope='Nitro ERC-20 withdrawal receipt correlation: matching position, caller, target, full message bytes and exact parent token payment. Independently verify chain identity and gateway deployments. Does not establish beneficial ownership, Fusion settlement or exchange credit.'; matches=$matches; unmatched=$unmatched }
$report | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Write-Output "Matched $($matches.Count) withdrawals; $($unmatched.Count) lack a retained parent execution."
