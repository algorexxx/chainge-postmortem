# Fusion USDC mint/burn receipts — 2026-10-13

Public-chain evidence acquired through the owner-authorized Fusion gateway. Connection details are omitted. FSNEX and this node share infrastructure; agreement is corroboration of the retained data, not independent-provider verification.

## Scope and selection

Take the union of successful `fusion-new-transfers-*.response.json` and `fusion-legacy-transfers-*.response.json` rows in the preceding [investigation package](../2026-10-13-usdc-investigation/README.md). Deduplicate by transaction hash and log index. Select transactions with a Transfer from or to the zero address in either selected full-time USDC token.

This produces **45,544 mint/burn events in 45,149 transactions and 43,345 blocks**, through Fusion block 15130080. It does not independently discover missing explorer events or cover every ordinary-transfer transaction.

- `transactions-*`: batches of up to 40 transaction hashes, each requesting `eth_getTransactionReceipt` and `eth_getTransactionByHash`.
- `headers-*`: batches of up to 80 block hashes using `eth_getBlockByHash` with full transactions disabled.
- Each group retains the exact request and response bytes and a capture-time sidecar. `manifest.json` hashes all package files other than itself.

## Checks performed

Every selected hash returned a successful receipt and a transaction. Receipt and transaction hash, block hash, sender and target agree. Each transaction belongs to the returned block's transaction list. Every selected mint/burn log is present. For all **46,309** original logs encountered in these receipts, token, topics, data, block hash, block number and transaction index agree with the earlier explorer row; its timestamp agrees with the node header. This includes 765 ordinary transfers alongside the mint/burn events.

The two full-time tokens retain six-decimal accounting. Dated child transfers are available as receipt context but are not imported as additional full-time tokens. Contract-purpose classifications and remaining gaps are described in the [assessment](../../research/assets/usdc.md).

## Reproduction

`reconciliation/usdc.json` selects these responses. Run `./reconcile.ps1 -Asset usdc`, then `./verify.ps1`. Raw JSON-RPC requests can be replayed against an authorized Fusion node; access settings belong outside this repository. Public reproduction of the offline reconciliation does not require node access.

Receipt success proves execution, not authorization, ownership, custody, correct customer settlement or complete liabilities. Ordinary transfers outside the selected transactions retain explorer-only provenance. The latest retained Fusion cutoff is earlier than current source-chain balance probes.
