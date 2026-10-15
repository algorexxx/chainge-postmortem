# Slow anonymous history capture

Use [indexed vault history](indexed-history-workflow.md) first. Pacing makes an unavoidable scan reliable; it does not make scanning thousands of empty ranges the preferred discovery method. BAT's initial broad scan was stopped after a working anonymous address index returned its candidates in five calls.

Apply the mandatory [acquisition review](indexed-history-workflow.md#before-any-broad-scan) before executing or resuming a plan. Provider-aware retries are not a reason to accept a days-long scan. Estimates of 24 hours or longer require an explicit user-approved exception for that plan; monitor actual throughput and stop/reassess if the estimate crosses that limit. These are workflow requirements, not an automatic time limit implemented by the capture executable.

Use the existing capture engine with one RPC request per batch, a small pinned block range and an explicit retry policy. Do not rotate endpoints or run parallel workers to evade quotas.

In the asset's `capturePolicies` entry, set `batchSize: 1`, keep `combineHolderFilters: true` where supported, and choose `maximumLogBlocks` from a successful provider test. In `template`, set `requestDelay`, `maximumRetries` and `retryDelay`, for example:

```json
{
  "requestDelay": "00:00:25",
  "maximumRetries": 32,
  "retryDelay": "00:01:05",
  "maximumConsecutiveFailures": 1
}
```

Generate a plan with `capture-plan.ps1` and run `capture.ps1`. To adapt an existing plan without changing published accounting configuration, use `./research/prepare-paced-capture.ps1 -Plan <existing-plan> -OutputPlan <new-plan> -NewestFirst`. It flattens batches into singleton requests, sets the pacing/retry policy above and optionally prioritizes recent log ranges while preserving every request. It refuses to overwrite an existing resumable plan. Verify the anonymous endpoint in the input plan first.

For a long background job, keep a fixed copy of the plan and Release executable under ignored `archives/`; run that executable with `capture <plan> <output-directory>`. Keep one writer per output directory. On Windows, launch with a hidden window and redirect stdout/stderr to ignored files. Preserve its PID and process start time locally so a later monitor can identify the correct process. Keep incomplete acquisition packages under `archives/` until sealed and reviewed.

The engine retries the same request after HTTP 408/429/500/502/503/504, transport failures, or explicit RPC quota responses. It waits at least the configured delay, increases that delay exponentially up to 16 times the baseline, and honors longer HTTP `Retry-After` or RPC `retry_after_ms` instructions. Each attempt is saved separately. Retry limits apply per query, including the chain-identity check. Permanent errors and a wrong chain are not automatically retried. Old plans default to no retries.

Restart using the **same plan and output directory**. Successful pinned requests are reused only when their endpoint, request bytes and stored hashes match. Failures never count as coverage. The terminal JSON summary must report verified identity and zero remaining queries before calling acquisition complete. The manifest is written when the capture run returns; a forcibly interrupted package is not sealed until a subsequent run completes. A sleeping/offline computer cannot make progress.

BAT's anonymous BSC test on 2026-10-15 completed six singleton requests, including a quota failure followed by a successful automatic retry. Its retained block range is 10,000 blocks per request. This is evidence that slower acquisition works, not proof that the entire historical endpoint is complete or that future ranges will succeed.

After capture: register successful logs and paired coverage requests, reconcile, capture newly discovered receipts/headers, review newly selected outflows and refresh the workflow/closeout assessments. Run publication and privacy checks before committing. Finishing the network scan alone does not finish the asset investigation.
