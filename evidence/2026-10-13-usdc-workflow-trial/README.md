# USDC workflow trial: public Ethereum state

Read-only JSON-RPC capture on 2026-10-13 from `https://ethereum-rpc.publicnode.com`. Requests, raw response bodies and capture metadata are retained with saved-byte SHA-256 hashes in `manifest.json`.

The first batch queries chain ID and the finalized block. The second pins the USDC `decimals()` and two candidate-address `balanceOf` calls to that block number and re-reads its header to confirm the hash. Chain ID returned 1, block 26182536, hash `0xefd17b5282b448dc3d3b865285ed9fcac1dd2a5c2327c337edb4f9625d2ffd9f`, timestamp 2026-10-13 09:01:11 UTC. Decimals returned 6 and both balances returned a valid zero word.

These are provider observations at a stated block, not proof of complete custody, no earlier transfers or common-cutoff Fusion backing. Candidate roles and receipt-sample limitations are documented in the [USDC package](../../research/assets/usdc.md). No credential or private endpoint was used.
