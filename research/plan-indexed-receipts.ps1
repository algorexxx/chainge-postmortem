param(
    [Parameter(Mandatory)][string]$SourceDirectory,
    [Parameter(Mandatory)][ValidatePattern('^0x[0-9a-fA-F]{40}$')][string]$Token,
    [Parameter(Mandatory)][string]$OutputPlan,
    [Parameter(Mandatory)][uri]$RpcEndpoint,
    [Parameter(Mandatory)][ValidatePattern('^0x[0-9a-fA-F]+$')][string]$ChainId
)
$ErrorActionPreference = 'Stop'
if ($RpcEndpoint.Scheme -ne 'https' -or $RpcEndpoint.UserInfo -or $RpcEndpoint.Query) { throw 'Use an anonymous HTTPS RPC endpoint.' }
if (Test-Path -LiteralPath $OutputPlan) { throw 'Do not overwrite a resumable plan.' }
$transactions = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$summary = @(foreach ($file in Get-ChildItem -LiteralPath $SourceDirectory -Filter '0x*.response.json') {
    $holder = $file.Name.Replace('.response.json','')
    if ($holder -notmatch '^0x[0-9a-fA-F]{40}$') { throw 'Expected one address-named response per holder.' }
    $page = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if ($page.ContainsKey('error') -or -not $page.ContainsKey('operations') -or $page.operations -isnot [array]) { throw "Invalid indexed history: $($file.Name)" }
    foreach ($operation in $page.operations) {
        if ($operation.type -ne 'transfer' -or $operation.tokenInfo.address -ine $Token -or ($operation.from -ine $holder -and $operation.to -ine $holder)) { throw 'An indexed operation falls outside its requested holder/token scope.' }
        if ($operation.transactionHash -notmatch '^0x[0-9a-fA-F]{64}$') { throw 'Invalid transaction hash.' }
        $transactions.Add($operation.transactionHash.ToLowerInvariant()) | Out-Null
    }
    @{ holder=$holder; returnedRows=$page.operations.Count; status=$(if($page.operations.Count -ge 100){'possibly-truncated'}elseif($page.operations.Count -eq 0){'empty-index-response-not-proof'}else{'below-page-limit-not-independent-completeness'}) }
})
if ($summary.Count -eq 0) { throw 'No indexed history responses found.' }
$plan = @{
    endpoint=$RpcEndpoint.AbsoluteUri; expectedChainId=$ChainId; userAgent='Chainge-public-research/1.0'
    requestDelay='00:00:04'; timeout='00:00:40'; maximumConsecutiveFailures=1; maximumRetries=8; retryDelay='00:01:05'
    queries=@(foreach($transaction in ($transactions | Sort-Object)) {
        @{ name=('receipt-'+$transaction.Substring(2)); request=@{jsonrpc='2.0';id=$transaction;method='eth_getTransactionReceipt';params=@($transaction)} }
    })
}
$path = [IO.Path]::GetFullPath($OutputPlan)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
[IO.File]::WriteAllText($path, ($plan | ConvertTo-Json -Depth 20), [Text.UTF8Encoding]::new($false))
@{ holders=$summary; uniqueTransactions=$transactions.Count; amounts='Ignored: exact quantities must come from successful RPC receipts.' } | ConvertTo-Json -Depth 10
