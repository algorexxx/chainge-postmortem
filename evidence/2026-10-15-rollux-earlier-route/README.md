# Earlier Rollux route evidence

Captured 2026-10-15 from anonymous public Syscoin and Rollux RPC endpoints. Requests, raw responses, acquisition metadata and SHA-256 hashes are retained. Selected transactions cover the February 2024 parent mint, forwarding transfer, bridge lock, Rollux mint and onward transfer.

`intermediate-history` independently queries incoming/outgoing token logs from zero through Rollux block `0x9e3050`, with `balanceOf` pinned to that block. This supports the intermediate pass-through, not complete history after that date. The final vault receipt and its header were already captured in `../2026-10-15-rollux-distribution/`.

See [the analysis](../../research/usdc-rollux-earlier-route.md). Run `./research/check-rollux-earlier-route.ps1` from the repository root. This package does not establish a Fusion funding source or independent USDC backing.
