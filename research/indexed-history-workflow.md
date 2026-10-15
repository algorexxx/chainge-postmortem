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

Use broad block-range RPC scans only for a specific remaining completeness question. Estimate request count before launching: inclusive block span divided by permitted range, multiplied by directions and independent filters. Compare that cost with indexed discovery first. A multi-day scan is not the default token workflow.

BAT validation on 2026-10-15: five vault queries returned 0, 27, 14, 2 and 8 operations, containing 47 distinct transactions. This replaced the proposed 12,754-request discovery scan with five index calls and targeted receipt/header verification. The old scan was stopped and retained under ignored archives. This does not establish exhaustive custody or independently complete BSC coverage.
