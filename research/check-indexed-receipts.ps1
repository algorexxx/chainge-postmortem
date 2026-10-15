$ErrorActionPreference = 'Stop'
$directory = Join-Path (Split-Path $PSScriptRoot -Parent) ('archives/indexed-plan-test-' + [guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($directory) | Out-Null
$holder = '0x1111111111111111111111111111111111111111'
$token = '0x2222222222222222222222222222222222222222'
$transaction = '0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
$operation = @{type='transfer';tokenInfo=@{address=$token};from=$holder;to='0x3333333333333333333333333333333333333333';transactionHash=$transaction;value='not-used'}
$source = Join-Path $directory "$holder.response.json"
@{operations=@($operation,$operation)} | ConvertTo-Json -Depth 10 | Set-Content $source -Encoding utf8
$plan = Join-Path $directory 'plan.json'
& (Join-Path $PSScriptRoot 'plan-indexed-receipts.ps1') -SourceDirectory $directory -Token $token -OutputPlan $plan -RpcEndpoint https://example.invalid -ChainId 0x38 | Out-Null
$actual = Get-Content $plan -Raw | ConvertFrom-Json -AsHashtable
if ($actual.queries.Count -ne 1 -or $actual.queries[0].request.method -ne 'eth_getTransactionReceipt' -or $actual.queries[0].request.params[0] -cne $transaction) { throw 'Indexed transactions were not deduplicated into receipt requests.' }
@{operations=@($operation)*100} | ConvertTo-Json -Depth 10 | Set-Content $source -Encoding utf8
$summary = & (Join-Path $PSScriptRoot 'plan-indexed-receipts.ps1') -SourceDirectory $directory -Token $token -OutputPlan (Join-Path $directory 'full-page.json') -RpcEndpoint https://example.invalid -ChainId 0x38 | ConvertFrom-Json
if ($summary.holders[0].status -cne 'possibly-truncated') { throw 'Full page was not flagged.' }
$operation.tokenInfo.address = '0x4444444444444444444444444444444444444444'
@{operations=@($operation)} | ConvertTo-Json -Depth 10 | Set-Content $source -Encoding utf8
$rejected = $false
try { & (Join-Path $PSScriptRoot 'plan-indexed-receipts.ps1') -SourceDirectory $directory -Token $token -OutputPlan (Join-Path $directory 'invalid.json') -RpcEndpoint https://example.invalid -ChainId 0x38 | Out-Null }
catch { if ($_.Exception.Message -notmatch 'outside its requested') { throw }; $rejected=$true }
if (-not $rejected) { throw 'Wrong-token index rows were accepted.' }
Write-Output 'Indexed planner checks passed: deduplication, ignored quantities, truncation and scope validation.'
