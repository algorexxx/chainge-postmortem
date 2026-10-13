# USDC public evidence — 2026-10-13

[Assessment](../../research/assets/usdc.md). Requests, responses and capture sidecars preserve public evidence. `manifest.json` hashes retained bytes; integrity does not establish completeness. No private connection details, Telegram exports or community submissions are included.

## Groups

- `fusion-new-transfers-*`: creation through block 15130080. Four full 10-page windows, then a partial fifth. Initial page 11 preserves the pagination-limit error.
- `fusion-legacy-transfers-*`: creation through the same block; fifteen windows, terminal page 15/3 has 193 rows.
- `fusion-slice-*`: four dated child histories. Terminal pages have 237, 473, 319 and 147 rows. The redundant `b9ee89` group re-queries the full-time child; it was stopped after recognizing null slice bounds and is not selected by reconciliation.
- `fusion-*-metadata*`, `fusion-*-holders*`, creation and named transaction details: native explorer context. Holder captures are first pages only. Native detail is not a node receipt. Two URLs omitting `/api` retained SPA HTML, excluded from ledger inputs.
- `fusion-baseline*`: failed authorized-gateway probe. Only a sanitized source description and error type are retained.
- `source-discovery-*`, `closing-recipient-*`, `successor-discovery-*`, `large-payout-recipient-*`: Routescan discovery. Synthetic row identities are not ledger event indices. Empty/unsupported responses do not prove no transfers.
- `ethereum-corroboration-*`, `ethereum-retry-*`, `avalanche-corroboration-*`, `recipient-corroboration-*`, `recipient-retry-*`: receipts and headers. Failed/null responses remain beside successful retries. A descriptive user agent resolved HTTP failures; an alternate public provider filled remaining null receipts.
- `successor-review-*`: up to five largest and five latest discovery rows per chain/token/address, deduplicated by transaction. Selected context, not full histories.
- `extended-custody-*`, `avalanche-baseline*`, `avalanche-balances*`, `arbitrum-*`: chain identity, pinned headers, decimals and balances.
- `cutoff-block-*`, `cutoff-state-*`: blocks at/before the Fusion reference time, adjacent headers and historical balances. Archive and batch-limit failures are preserved beside successful alternate-provider reads.
- `eth-closing*`, `avax-closing*`, `ethereum-416050-payout*`, `ethereum-large-payout-context*`: path/context probes. A current code read does not establish historical bytecode.

## Replay and interpretation

GET sidecars preserve exact public URLs, parameters, capture UTC and HTTP results. RPC request files preserve methods and block tags; sidecars identify public endpoints. Payloads remain as received. Retries do not overwrite earlier results. Capture UTC and block UTC are separate.

Fusion pagination uses ten pages of 1,000 rows per window. Each next window begins inclusively at the preceding last block. Reconciliation deduplicates by chain, transaction and log index and rejects conflicting duplicates. A terminal short page exhausts that index query, not an independent chain inventory.

All 719/2,131 discovery rows for the earlier current Ethereum/Avalanche candidate and 763/43 for the January recipient match successful receipt transfers by token, sender, recipient and integer amount. Receipt/header counts and remaining gaps are in the assessment. BSC and Arbitrum have no full acquired movement histories.

`reconciliation/usdc.json` selects the supported input groups. It excludes native explorer detail, dated-child ERC-20 logs from full-time matching and balance probes. Other responses remain available for review. Reproduction generates ledger/input hashes, not a certification of liabilities, authority or custody.
