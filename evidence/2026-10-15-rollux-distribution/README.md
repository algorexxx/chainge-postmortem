# Rollux address histories and selected receipts

Anonymous public RPC captures from `https://rpc.rollux.com`, acquired 2026-10-15. The four address histories independently query incoming/outgoing Transfer logs from block zero through **52,350,517**, plus `balanceOf` at that numeric block. Its timestamp is **2026-10-15 12:11:15 UTC**. Selected transaction receipts are paired with block headers. Requests, raw responses and acquisition metadata are retained with SHA-256 hashes in `manifest.json`.

An initial token-wide exploratory response was about 240 MB. It remains in ignored local archives; the independently requested, much smaller address histories are the reproducible public evidence for this review. No claim relies on unpublished token-wide totals.

Run `./research/check-rollux-distribution.ps1` to check hashes, chain/block alignment, selected receipts and address balances. `./research/review-address-history.ps1 -CapturePrefix evidence/2026-10-15-rollux-distribution/127e7766-history` generates the recipient totals without storing a second ledger. It rejects duplicate logs within a direction, out-of-scope events and balance mismatches. Self-transfers count in both directions and net to zero. Reconciliation to a balance is useful corroboration, not a guarantee that offsetting omitted events cannot exist.

See [the findings](../../research/usdc-rollux-distribution.md). Balances are Rollux token units with unresolved independent backing, not verified dollar reserves.
