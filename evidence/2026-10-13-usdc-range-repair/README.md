# USDC evidence: range repair

Captured 2026-10-13. Arbitrum pinned head, tail-log queries and ten balanceOf calls, plus split Optimism repair attempts. All ten Arbitrum token/address balances match retained-log replay. Optimism still has failed outgoing ranges; its incomplete replay is not a closing balance.

## Reproduction and interpretation

Request and capture sidecars preserve methods, arguments, source URLs (except owner-authorized Fusion connection details), and capture times. Responses are original bytes. `manifest.json` hashes each package file with SHA-256; it detects later changes, not source truth. Failed requests are not zero balances or absent transfers. No account credentials were used for the public-source captures.

Run `./reconcile.ps1 -Asset usdc` from the repository root to regenerate the configured ledger, routing candidates, holder positions and range-coverage audit. Not every probe or downstream token is selected into the USDC ledger. See the [USDC assessment](../../research/assets/usdc.md) and [configuration](../../reconciliation/usdc.json) for selection and limitations. No wallet owner, intent, claim adjustment or complete custody inventory is inferred solely from these files.
