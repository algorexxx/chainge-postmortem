# Public Fusion evidence through FSNEX

Use the anonymous public API by default. No account, API key, private gateway or SSH connection is needed for indexed Fusion history. FSNEX and the recovered node share infrastructure; agreement between them is not independent-provider corroboration.

For bounded datasets, add `-IncludeAllTransfers` to `capture-fsnex-transactions.ps1` to capture ordinary transfers as well as mint/burn transactions. This matters for identifying inventory conversions and tracing large holders. The holder-funding and controller-link reviewers now consume the native public response format too. Reuse successful immutable captures; regenerate manifests after additions, and finish acquisition before reconciling a selected evidence directory.

For state the public API cannot supply, `research/plan-fusion-state.ps1 -Asset <asset> -Endpoint <authorized-rpc>` generates pinned balance/code queries from the current configured standard-token inventory. Its plan stays under ignored `archives/` because the endpoint may be private. Existing reviewed `<asset>-fusion-state.json` mappings also generate restriction queries; missing mappings are explicitly warned about. Execute using `capture.ps1` and run `check-fusion-restrictions.ps1` where applicable. For a private endpoint, the capture output must also stay under `archives/`: the runner records the endpoint in capture metadata. Publish a separate reviewed copy containing unchanged public RPC request/response bytes, capture UTC and hashes, but explicitly omitted connection details; regenerate its public manifest and retain the original privately. Never infer unsupported predecessor/slice balances from the standard inventory. Connection setup remains local; publish only the chain evidence.

## Public endpoints

| Evidence | GET endpoint |
| --- | --- |
| Chain identity and indexed range | `https://fsnex.net/api/v1/network` |
| Block details by number or hash | `https://fsnex.net/api/v1/blocks/{number-or-hash}` |
| Transaction input, success status and complete receipt logs | `https://fsnex.net/api/v1/transactions/{hash}` |
| Contract code and observation block | `https://fsnex.net/api/v1/contracts/{address}/code` |
| Verified contract functions | `https://fsnex.net/api/v1/contracts/{address}/functions` |

The Etherscan-compatible `/api?module=logs&action=getLogs&address=...&fromBlock=...&toBlock=...` endpoint supplies bounded logs. Respect its 10,000-row window, retain overlapping range boundaries and deduplicate exact event identities. Check the network's indexed range; a requested range does not prove indexed coverage. Do not use decoded token-transfer descriptions as substitutes for raw log fields.

## Reusable transaction capture

After initial log import and reconciliation:

```powershell
./research/capture-fsnex-transactions.ps1 -Asset bat -OutputDirectory evidence/YYYY-MM-DD-bat-fsnex-receipts
```

The script takes unique transaction hashes from the generated `fusion-purposes.csv`, covering issuance/destruction context rather than every ordinary holder transfer. It retains original response bytes, public URL, retrieval time, transport status and checksums. Successful existing captures are checked before reuse. Failed attempts remain evidence; retry failures in a new package. Add a scope README and refresh the manifest to include it before selecting the package.

Add a `sources` entry with `chain: "Fusion"`, `format: "fsnex-transaction"`, the capture directory, `pattern: "0x*.response.json"`, reviewed `tokenDecimals` and the same `datedTokens` used for the initial log source. Reconcile again. This explicit format accepts only successful transactions with complete, uniquely indexed, nonremoved logs. It binds each raw log to the enclosing transaction and block, retains calldata and receipt context, and rejects conflicts with existing events. Failed or missing responses do not establish zero activity.

`receiptVerified` means the event is corroborated by a successful complete receipt record, which may be delivered through FSNEX. It does not mean an independent RPC provider confirmed it. Original public response files and the explicit source format preserve that distinction. Transaction timestamps do not become separate block-header evidence; capture the referenced public block details when validating time and block identity.

## State and tracing limits

FSNEX does not offer a general public JSON-RPC proxy. Its contract-read endpoint accepts ABI-declared read functions for verified contracts, currently at `latest`; it is not an arbitrary historical `eth_call` endpoint. The BAT newer contract's function lookup returned 404 during this check. Stored token supply/holder balances and contract code carry their own observation/projection limits and must not be mislabeled as fresh pinned RPC calls.

For pinned holder balances, restrictions or unavailable traces, preserve contributed node evidence separately, or state the gap. A contributor can still reproduce the public history and receipt analysis without private access. Do not claim that all Fusion evidence is inaccessible merely because a local tunnel is closed.

## Access correction

The BAT pass initially repeated an earlier access-diagnosis mistake: a closed local tunnel and a deliberately site-restricted LAN listener were treated as missing access. Existing SSH authentication was subsequently confirmed to work. More importantly, FSNEX's public transaction-detail API already exposed receipt-derived data; the missing capability was this repository's importer. The public importer and capture script remove that prerequisite for history/receipt work. No gateway firewall or public RPC exposure was changed.
