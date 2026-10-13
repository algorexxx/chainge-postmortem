# USDC evidence: fusion claims

Captured 2026-10-13. Pinned Fusion balanceOf checks for 529 positive reconstructed positions across six token representations, plus contract bytecode. Connection details are deliberately omitted. The comparison FRC759 source is from the previously preserved official repository commit 6cfe366b26de7b613a1bd5de306da7bab606de9e; it has not been compiled and matched to deployed bytecode. Address-one and dated balances are not consolidated claims.

## Reproduction and interpretation

Request and capture sidecars preserve methods, arguments, source URLs (except owner-authorized Fusion connection details), and capture times. Responses are original bytes. `manifest.json` hashes each package file with SHA-256; it detects later changes, not source truth. Failed requests are not zero balances or absent transfers. No account credentials were used for the public-source captures.

Run `./reconcile.ps1 -Asset usdc` from the repository root to regenerate the configured ledger, routing candidates, holder positions and range-coverage audit. Not every probe or downstream token is selected into the USDC ledger. See the [USDC assessment](../../research/assets/usdc.md) and [configuration](../../reconciliation/usdc.json) for selection and limitations. No wallet owner, intent, claim adjustment or complete custody inventory is inferred solely from these files.
