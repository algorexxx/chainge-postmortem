# Usdc bsc balance gaps

Bounded historical-state versus retained-replay probes at two BSC candidate vaults. The search locates omitted incoming and outgoing transfers, then requests boundary logs and receipts. The run stopped on a rate-limited boundary request. See the slower retry package. This is not exhaustive history; an apparent net discrepancy can combine multiple missing transfers.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
