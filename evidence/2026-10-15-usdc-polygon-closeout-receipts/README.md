# Usdc polygon closeout receipts

Resumable anonymous Polygon receipt acquisition for newly recovered vault history. The saved plan also requested numeric tail log windows. Rate-limit failures remain alongside retries; successful responses are merged by the offline reader. No history is inferred from failed requests.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
