# USDC evidence: common cutoff retry2

Captured 2026-10-13. Further aligned historical-state attempts. Successful responses cover additional chains; BSC and early Arbitrum archive-state attempts remain unsuccessful.

## Reproduction and interpretation

Request and capture sidecars preserve methods, arguments, source URLs (except owner-authorized Fusion connection details), and capture times. Responses are original bytes. `manifest.json` hashes each package file with SHA-256; it detects later changes, not source truth. Failed requests are not zero balances or absent transfers. No account credentials were used for the public-source captures.

Run `./reconcile.ps1 -Asset usdc` from the repository root to regenerate the configured ledger, routing candidates, holder positions and range-coverage audit. Not every probe or downstream token is selected into the USDC ledger. See the [USDC assessment](../../research/assets/usdc.md) and [configuration](../../reconciliation/usdc.json) for selection and limitations. No wallet owner, intent, claim adjustment or complete custody inventory is inferred solely from these files.
