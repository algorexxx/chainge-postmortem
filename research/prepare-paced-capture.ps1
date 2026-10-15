param(
    [Parameter(Mandatory)][string]$Plan,
    [Parameter(Mandatory)][string]$OutputPlan,
    [TimeSpan]$RequestDelay = '00:00:25',
    [TimeSpan]$RetryDelay = '00:01:05',
    [ValidateRange(1, 100)][int]$MaximumRetries = 32,
    [switch]$NewestFirst
)
$ErrorActionPreference = 'Stop'
if ($RequestDelay -lt [TimeSpan]::Zero -or $RetryDelay -le [TimeSpan]::Zero) { throw 'Invalid capture delays.' }
if (Test-Path -LiteralPath $OutputPlan) { throw 'Keep existing resumable plans immutable; choose a new output plan.' }
$capture = Get-Content -LiteralPath $Plan -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$queries = @(foreach ($query in $capture.queries) {
    $index = 0
    foreach ($request in @($query.request)) {
        @{ name = ('{0}-{1:D3}' -f $query.name, $index); request = $request }
        $index++
    }
})
if ($NewestFirst) {
    $queries = @($queries | Sort-Object -Stable -Descending -Property @{ Expression = {
        if ($_.request.method -eq 'eth_getLogs') { [Convert]::ToInt64($_.request.params[0].fromBlock.Substring(2), 16) }
        else { [long]::MinValue }
    } })
}
$capture.queries = $queries
$capture.requestDelay = $RequestDelay.ToString('c')
$capture.retryDelay = $RetryDelay.ToString('c')
$capture.maximumRetries = $MaximumRetries
$capture.maximumConsecutiveFailures = 1
$destination = [IO.Path]::GetFullPath($OutputPlan)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
[IO.File]::WriteAllText($destination, ($capture | ConvertTo-Json -Depth 50), [Text.UTF8Encoding]::new($false))
Write-Output ('Prepared {0} singleton requests. Resume using this exact plan.' -f $queries.Count)
