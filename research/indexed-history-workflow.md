# Indexed vault history before block scanning

Start with an address-and-token index. Retrieve incoming and outgoing token transfers for each candidate vault, preserve the raw pages and record the page limit, filters, returned rows, oldest/latest entries and any pagination cursor. Ordinary transaction lists can miss token transfers initiated by routers; use token-transfer history.

For BSC, the anonymous [Binplorer API](https://github.com/EverexIO/Ethplorer/wiki/ethplorer-api) accepts the documented shared `freekey`. It requires no account or private credential. Its published limits are 2 requests/second, 20/minute, 100/hour, 1,000/day and 3,000/week, with at most 100 operations per response and restrictions on older timestamp pagination. Do not rotate keys to bypass these limits. Check current documentation before a large acquisition.

```powershell
./research/capture-binplorer.ps1 -Asset bat -Token 0x101d82428437127bf1608f699cd651e6abf9766e -OutputDirectory evidence/YYYY-MM-DD-bat-index
./research/plan-indexed-receipts.ps1 -SourceDirectory evidence/YYYY-MM-DD-bat-index -Token 0x101d82428437127bf1608f699cd651e6abf9766e -RpcEndpoint https://rpc-bnb.blockmachine.io -ChainId 0x38 -OutputPlan archives/bat-indexed-receipts.json
./capture.ps1 -Plan archives/bat-indexed-receipts.json -Output evidence/YYYY-MM-DD-bat-receipts
./research/review-indexed-receipts.ps1 -SourceDirectory evidence/YYYY-MM-DD-bat-index -ReceiptDirectory evidence/YYYY-MM-DD-bat-receipts -Token 0x101d82428437127bf1608f699cd651e6abf9766e -CutoffBlock 63769742
```

The capture script uses configured BSC vaults. The planner validates token, holder, transfer type and transaction-hash syntax, deduplicates transactions, and ignores indexed amounts. BAT's indexed values contain rounding differences even when supplied as strings; never use them for exact accounting. Capture successful RPC receipts and block headers for exact raw units, event identities and timestamps, and import those into the existing ledger. Reuse existing verified receipts where available.

The planner explicitly flags a full 100-row result as possibly truncated. It still generates useful receipt requests, but that result must not be treated as the complete history. If older pagination is unavailable under the anonymous tier, use another independently accessible index or a user-saved public explorer export. Preserve exports outside git until reviewed for session fields and private information. Do not bypass access challenges or silently treat denied queries as empty histories.

Below-limit results mean the index did not fill the requested page, not independent completeness. Check that known transactions are present, that each listed transfer is corroborated by receipt logs, and that the reconstructed net agrees with pinned balances at the same cutoff. An empty result remains an empty index response, not proof of no activity. Index agreement and balance agreement cannot exclude cancelling omissions. Do not turn indexed discoveries into artificial successful `eth_getLogs` coverage windows.

## Before any broad scan

This acquisition order is required for human contributors and LLMs:

1. Reuse preserved, verified requests and receipts.
2. Fetch token-filtered transfers to and from each vault from an index. Check pagination, truncation and known transactions.
3. If that source fails, try another anonymous index or a reviewed public explorer export. A failed BscScan request does not establish that indexed discovery is unavailable.
4. Verify discovered transactions through exact receipts and block headers, then compare replay with pinned balances.
5. Use targeted RPC queries for specific discrepancies or missing transactions. Consider broad ranges only for a clearly stated remaining evidence gap.

Before launching a broad scan, record in the asset's acquisition section: alternatives actually tried, exact remaining gap, number of uncached requests, measured requests per minute, estimated elapsed time including provider cooldowns, and the proposed stopping/reassessment condition. Estimate log requests by summing `ceil(inclusive missing block span / permitted range)` across directions and independent filters; count individual RPC calls even when batched. Add receipt/header requests and account for their different quota costs. A small successful pilot establishes realistic throughput; configured delay alone is only a lower bound.

**Do not launch or automatically resume a scan estimated at 24 hours or longer unless the user explicitly approves that particular exception after seeing the estimate and alternatives.** General authorization to finish an investigation, automate retries or work autonomously does not waive this rule. Do not evade it by splitting work into smaller jobs, switching endpoints to evade quotas, or scheduling a heartbeat. If observed throughput pushes an existing scan over the limit, preserve progress, stop the broad scan and reassess the acquisition method. Continue useful indexed and targeted work independently.

Do not lower the evidence standard to avoid a slow scan: keep an unresolved coverage gap visible, distinguish indexed-history corroboration from full RPC range coverage, and document the specific evidence boundary. A generated capture plan is a list of possible requests, not an instruction to execute all of them.

BAT validation on 2026-10-15: five vault queries returned 0, 27, 14, 2 and 8 operations, containing 47 distinct transactions. This replaced the proposed 12,754-request discovery scan with five index calls and targeted receipt/header verification. The old scan was stopped and retained under ignored archives. This does not establish exhaustive custody or independently complete BSC coverage.


## Recipient follow-up

Use `capture-binplorer.ps1 -Addresses <address1>,<address2>` for explicit downstream recipients; omitted addresses still select the configured vaults. Index amounts remain discovery hints only. Preserve exact onward receipts and block headers, check pinned balances and keep mixed funds explicit. Walk all receipt transfers to identify the actual output recipient; an intermediate WBNB or pool transfer is not the final destination. Native delivery requires a successful trace, including absence of errors in its ancestor calls. Do not count downstream transfers as additional vault outflows or assign one mixed balance to multiple cases.

Probe one historical request before batching a provider/method combination: support for headers does not imply archive receipt/state support. Stop that method on an explicit access denial and use a permitted anonymous source; retries are for transient errors. A below-cap index is not independent completeness, and a full page requires a visible truncation boundary. For small ledgers, use closeout `selection: "all-outflows"` so swaps, old payments and small cases cannot fall outside review.
