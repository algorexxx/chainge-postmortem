# Reviewed BUSD source-chain evidence

Captured on **2026-10-11**. See [source-chain findings](../../research/busd-source-chain-flows.md) and [case assessment](../../research/busd-case-assessment.md). This package contains independently retrieved public evidence, without private contributor records, Telegram exports, credentials or private node connection details.

## Contents and interpretation

- `bsc-receipts/`: direct receipt batches for the later controller's history. `bsc-predecessor-receipts/`: the older controller's direct receipts. Top-level `bsc-*` files add pinned state, headers, traced branches and failures.
- `ethereum-current-receipts-*`: the later controller's 232 transactions. Initial batch 20 returned nulls; `ethereum-current-receipts-20-retry.*` supplies successful responses using the original request file. `ethereum-predecessor-receipts-retry.*` similarly supersedes null results with nine successful receipts, without deleting the failed attempt. Top-level files also cover funding, successor balances and failed trace attempts.
- `avaxe-*`: Avalanche **BUSD.e**, including indexed discovery, individual node receipts, state and MinterProxyV2 configuration. `avax-*`: the distinct Avalanche **Binance-Peg BUSD** state and history checks. These tokens must not be conflated.
- `fusion-cross-chain-pairs.*` and `fusion-second-pairs.*`: Fusion transactions, receipts and headers for deposit/issuance and burn/payment comparisons. The private gateway's connection details are intentionally absent.
- `*.observed.*` and `explorer-observations.json`: transcribed public explorer discovery records, including attributed labels. These are not raw node attestations or proof that explorer indexing is complete.
- `bsc-funder-export.csv`: public BscScan holder-table export, 2,800 rows. The rendered holder source is recorded in `explorer-observations.json`. The export ends in March 2023 and is incomplete; all exported rows match preserved node receipts. The later history was discovered separately.
- `capture-interpretation.json`: clarifications for failures, incomplete captures and misleading historical filename shorthand. An empty or null response never establishes no events.
- `manifest.json`: saved-byte SHA-256 and file size for each other file. Hashes verify integrity, not correctness, authorship or completeness of the source.

Across the three source chains, this package contains **5,146 unique successful BSC receipts, 262 Ethereum receipts and 397 Avalanche receipts**. These include overlapping address histories, counterparties and administrative checks. They are not counts of independent deposits. Large block-receipt discovery dumps and transient research scripts remain in the ignored local public archive; relevant transactions were preserved separately here. No production collector is shipped in this package.

Some requests lack an identically named response or capture: retries explicitly reference the original request, and `bsc-followup-head.*` is associated with the adjacent follow-up capture. Prepared/unexecuted requests and failures are described in `capture-interpretation.json`. `ethereum-receipts/` holds early probe attempts referenced by provider-test captures. The source-chain RPC responses are provider observations, not independently verified consensus proofs.

The published Avalanche source is an explorer-provided **MinterProxyV2** verification result. It has not been independently compiled against runtime. Historical and current runtime bytes match each other; historical getters and transaction receipts separately establish the reported configuration changes. The filename `avaxe-vault-source` refers to the initial lead address, which returned no verified source and empty runtime; the actual called contract is in `avaxe-router-source`.

## Reproduction from the preserved receipts

1. Select one chain and exact token contract. Load successful transaction-receipt results, including successful retries; retain failed requests as coverage evidence. Deduplicate receipts by transaction hash and require consistent block hashes and logs across duplicates.
2. Select logs from that exact token with the standard Transfer topic `0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef`. The last 20 bytes of topics 1 and 2 are the sender and recipient. Decode data as an unsigned integer. Do not use floating point.
3. Deduplicate by chain, transaction hash and log index; sort by block number and log index. Add incoming and subtract outgoing amounts for each address. Self-transfers contribute to both gross sums and cancel in the balance; zero-value events remain events.
4. Compare the ending integer balance to the matching token's pinned `balanceOf` result. For displayed amounts in this case, divide by 10^18 exactly. Require nonnegative running balances for the reviewed ordinary-token ledgers. This check cannot rule out omitted offsetting events.
5. For explorer rows, verify transaction hash, exact token, sender, recipient and raw amount against the receipt. Do not trust an explorer method label as transaction decoding. Hashes and timestamps for material transitions are separately checked against headers.
6. For the Avalanche pool transitions, decode the returned address word in `avaxe-pool-transition.response.json`; the reads at blocks 49639245/49639246 and 50151485/50151486 show the before/after configuration. Successful administrative receipts and inputs are in `avaxe-router-admin.*`.

Validation on the saved package reconciled the six controller ledgers, the selected original-holder/redemption/routing branches and all 2,800 CSV rows. Every retrieved Avalanche BUSD.e discovery row matches a successful node receipt. The three additional zero-value original-holder BSC events are distinguishable from its 75 nonzero events.

## Known discovery limits

BSC broad log queries were unavailable; explorer tables and direct node receipts supplied the histories. Routescan's Ethereum API returned only 53 of 232 known controller events, and returned no predecessor events despite nine subsequently verified through Etherscan and the node. Do not use that API's empty results as proof of absence. The successor API results are positive leads with this same coverage limit.

Routescan's Avalanche responses returned fewer than the requested row limit. Explicit tail requests after the final returned blocks are empty through 97290509. A page-2 attempt returned null and is recorded as a failure, not a coverage confirmation. State queries and administrative checks also use later pinned blocks, as specified in each request. No single global observation time is implied.

Ethereum and BSC native-output tracing failed for the attempted historical swaps. The November cross-chain pairings lack a verified shared bridge message identifier. Exchange labels remain attributed explorer statements, and exchange-internal credits are outside the observable chain data.
