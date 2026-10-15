# Selected Rollux deposit origin

Captured 2026-10-15 using anonymous public Syscoin and Rollux RPC/explorer endpoints. Each capture preserves its request, response and acquisition metadata; `manifest.json` records SHA-256 hashes of those files.

- `rollux-holder-history`: incoming and outgoing token Transfer logs for the selected holder, block zero through the September 2024 vault deposit; not a current holder history.
- `rollux-funding-receipts` and `rollux-funding-headers`: selected bridge mint and split transactions, receipts and block headers.
- `syscoin-parent` and `syscoin-creation`: explorer contract/deployment metadata.
- `syscoin-token-history`: first explorer page only; not a complete explorer history.
- `syscoin-head` and `syscoin-state`: numeric pinned head, token state and RPC Transfer logs from zero through that head.
- `syscoin-mint-context` and `syscoin-mint-headers`: selected parent mint, bridge lock and deployment transactions, receipts and headers.

The initial Fusion receipt is retained separately in `../2026-10-15-newer-holder-review/`. The final Rollux-to-Fusion linkage is checked by the existing controller-funding review. See [the route analysis](../../research/usdc-rollux-roundtrip.md) and run `./research/check-rollux-roundtrip.ps1` from the repository root.

Successful RPC responses and matching receipts establish the selected on-chain route, not ownership, independent USDC backing, or complete custody.
