# Usdc ethereum closeout receipts

Resumable anonymous Ethereum receipt acquisition for newly recovered vault history. Original rate-limit failures remain alongside successful retries. Chain identity, request hashes and raw successful receipts are preserved. This supplies event corroboration, not proof of custody or settlement.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
