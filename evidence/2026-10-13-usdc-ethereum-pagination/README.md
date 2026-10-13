# USDC evidence: ethereum pagination

Captured 2026-10-13. Anonymous Ethereum Blockscout v2 ERC-20 pagination for five candidates. Each request URL and capture time is retained; next_page_params is followed until null. Results are mutable explorer observations, not pinned RPC log coverage. All five cutoff net positions agree with retained historical balanceOf observations, but a matching net cannot prove that cancelling omissions do not exist. Repeated cursors or transport failures stop acquisition rather than imply completion.

## Reproduction and interpretation

Request and capture sidecars preserve methods, arguments, source URLs (except owner-authorized Fusion connection details), and capture times. Responses are original bytes. `manifest.json` hashes each package file with SHA-256; it detects later changes, not source truth. Failed requests are not zero balances or absent transfers. No account credentials were used for the public-source captures.

Run `./reconcile.ps1 -Asset usdc` from the repository root to regenerate the configured ledger, routing candidates, holder positions and range-coverage audit. Not every probe or downstream token is selected into the USDC ledger. See the [USDC assessment](../../research/assets/usdc.md) and [configuration](../../reconciliation/usdc.json) for selection and limitations. No wallet owner, intent, claim adjustment or complete custody inventory is inferred solely from these files.
