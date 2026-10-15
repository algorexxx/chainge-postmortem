# Usdc bsc balance gaps retry

Slower bounded continuation of the BSC state/replay discrepancy search, reusing earlier successful balance observations. Each new boundary is checked with token logs, receipts and a header. The pass is deliberately bounded and does not claim all discrepancies or offsetting omissions resolved.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
