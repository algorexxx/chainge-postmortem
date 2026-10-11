# Initial source preservation

Observation date: 2026-10-11. These are preservation and inspection results, not findings about asset custody or service status.

## Saved locally

Raw captures are under `archives/public/2026-10-11/`, excluded from Git pending review. Capture sidecars record URLs, UTC acquisition times where known, sizes and SHA-256 hashes. The public metadata inventory is [evidence/source-captures.json](evidence/source-captures.json). Raw sources are not yet distributed or independently backed up.

| Source | Saved material | Coverage and limits |
| --- | --- | --- |
| [Public token spreadsheet](https://docs.google.com/spreadsheets/d/1pyQlI4oGOgaRx-5UWA68meb85KtsnZ98BZrYDrHsqhk/edit?gid=1161132641#gid=1161132641) | Unchanged user-downloaded XLSX | One exported tab; download time unknown; not independently re-exported from Google |
| [Fusionite home](https://www.fusionite.info/) | HTML response | Page content; linked media and all articles not captured |
| [Universal Assets](https://www.fusionite.info/fusion-assets) | HTML response | Title/content checked; statements require corroboration |
| [sMPC Interoperability](https://www.fusionite.info/smpc-interoperability-dcrm) | HTML response | Title/content checked; statements require corroboration |
| [Chainge Finance](https://www.fusionite.info/chng) | HTML response | Title/content checked; statements require corroboration |
| [Chain Data](https://www.fusionite.info/chaindata) | HTML response | Linked datasets require separate captures |
| [Historical References](https://www.fusionite.info/historical-references) | HTML response | Linked references require separate captures |
| [DCRM Alliance/Nodes](https://www.fusionite.info/dcrm-nodes) | HTML response | Historical descriptive source; no current operator attribution established |
| [Chainge API documentation](https://chainge-finance.gitbook.io/chainge-finance/get-started/chainge-2.0-api-documentation) | HTML response | Main linked page only, not the complete GitBook |
| [ChaingeFinance repository inventory](https://api.github.com/orgs/ChaingeFinance/repos?per_page=100&page=1) | JSON response | 17 public repositories returned; no next-page Link header; not evidence of private or deleted repositories |

The initial sandbox HTTP attempts failed with local socket-access errors. A subsequent permitted network attempt retrieved the web pages and repository inventory successfully. Both attempts remain in the local record. Earlier web-tool access failures are therefore not evidence of source disappearance.

## Workbook inspection

Update for the UI: an independent public CSV export was successfully retrieved on 2026-10-11 and is distributed in [the catalogue evidence package](evidence/2026-10-11-token-catalogue/README.md). Its 2,185 rows, 2,184 distinct addresses and duplicate GLCH source rows agree with the earlier workbook inspection. The static-site generator reads the exported CSV directly. This verifies a current source export, not the underlying contracts' authenticity or custody.

File: `New smart contract addresses on Fusion.xlsx`. Sheet: `Sheet2`.

- `A1:B1` contains the headers `Asset` and `Address on Fusion`.
- `A2:B2186` contains 2,185 nonempty data rows.
- All 2,185 address cells match the textual form `0x` followed by 40 hexadecimal characters. This checks syntax only, not checksum validity, deployment, chain identity or issuer.
- Case-insensitive comparison yields 2,184 distinct addresses.
- Rows 700 and 1760 both contain GLCH and the same address.
- Rows 538, 718, 1482 and 1866 contain an asset label made entirely of `#` characters. These are literal stored strings, not an Excel column-width display issue.
- No formulas were found in the populated cells. No workbook changes or recalculation were performed.

Method: read the XLSX with `openpyxl` in read-only mode, inspect every populated row, apply the address syntax check, and group addresses case-insensitively. The source workbook remains the authority for row content. Counts are an inspection summary, not a count of distinct underlying assets or custody addresses. Live Google Sheets tab completeness remains unverified.

## Preserved Git histories

Six public repositories were mirrored without checking out or executing their code. Each mirror passed `git fsck --full`; a portable bundle was created from all local refs and hashed. The capture metadata records each default HEAD commit.

- [TokenList](https://github.com/ChaingeFinance/TokenList)
- [ChaingeToken](https://github.com/ChaingeFinance/ChaingeToken)
- [FRC759](https://github.com/ChaingeFinance/FRC759)
- [OpenAPIExample](https://github.com/ChaingeFinance/OpenAPIExample)
- [Chainge_SDK_V1](https://github.com/ChaingeFinance/Chainge_SDK_V1)
- [Chainge_Sign_Example](https://github.com/ChaingeFinance/Chainge_Sign_Example)

At the captured default HEAD, TokenList contains image assets under `tokens/`; its name should not be taken as evidence that it provides a contract-address registry. FRC759 includes a Solidity file and a README that names a mainnet contract address; deployment and role are not yet independently verified. These are code-review leads only.

Other repositories, release attachments, issues, pull requests, LFS objects and external dependencies have not been comprehensively preserved. A mirror and bundle preserve retrieved Git objects and refs, not every item on GitHub.

## Documented API access checks

The captured API documentation names the read-only endpoints `https://api2.chainge.finance/v1/getChain` and `https://api2.chainge.finance/v1/getAssets`. Both unauthenticated retrieval attempts failed DNS resolution in this environment on the observation date. No asset or chain API payload was obtained. This single vantage-point observation does not establish global availability, when any change occurred, or the status of other endpoints or services.

## Next collection priorities

The [legacy authority and time-slice report](research/legacy-authority-and-slice-accounting.md) verifies ownership/controller events against receipts and headers, reconciles 45 full-time-child events for the original balance address, and demonstrates a slice conversion that must not be counted as a new external deposit. Query evidence is in `evidence/2026-10-11-legacy-authority-review/`.

The [legacy balance-match report](research/legacy-balance-match-and-admin-transition.md) adds two bounded transaction windows, a matching legacy balance, blocking-call evidence and a January 2024 administrative transition. The second Fusionite-linked asset/chain workbook was downloaded successfully into the ignored public archive; it supplies chain-support leads, not a vault-address map. Eight reviewed JSON files plus a manifest are in `evidence/2026-10-11-bridge-review/`.

The [legacy BUSD and funding-origin report](research/legacy-busd-and-funding-origin.md) traces the funding address back to a direct newer-BUSD mint and verifies that legacy BUSD delegates balance reads to a full-time child that emits mint Transfer events. Evidence is preserved in `evidence/2026-10-11-legacy-review/`, including explicit broad-scan failures and corrected malformed queries. Full legacy history remains incomplete.

The [BUSD migration and balance-flow report](research/busd-migration-and-balance-flow.md) documents evidence consistent with legacy-to-new Fusion liquidity migration and reconciles the equal-amount burn/mint addresses against retained balances. Queries are preserved in `evidence/2026-10-11-flow-review/`; incomplete broad captures are explicitly marked. This adds Fusion-side context, not a confirmed source-chain vault.

The [factory history and BUSD supply report](research/factory-history-and-busd-supply.md) adds 3,986 explorer-listed factory transactions, receipt checks for 52 administrative calls, state checks for 16 observed controller addresses, and BUSD mint/burn totals reconciled against retained supply. Twenty-two reviewed JSON files are preserved in `evidence/2026-10-11-history-review/`. Historical internal tracing remains unavailable for the tested mint transaction; source-chain custody is unresolved.

The follow-up [factory and mint-authority report](research/factory-and-mint-authority.md) records the shared factory deployment, BUSD creation event and a read-only mint-permission trace at the retained block. Nineteen reviewed JSON evidence files and their integrity manifest are in `evidence/2026-10-11-factory-review/`. These observations do not establish source-chain custody or current network state.

The first contract review and bounded node probes are recorded in [initial contract findings](research/initial-contract-findings.md). Reviewed raw query evidence has been promoted to `evidence/2026-10-11-contract-review/`; the remaining raw archive stays excluded from Git. This adds deployed-code observations and access limitations without establishing any custody attribution.

1. Preserve the separate Chainge and Fusion Telegram exports outside the repository, then build a reviewed extraction process.
2. Follow Fusionite's historical contract-migration and transaction-tracking references and preserve original downloadable attachments where accessible.
3. Inspect the saved token contracts and SDKs for historical configuration and asset mapping. Independently verify any proposed deployment against chain data.
4. Locate historical API responses or alternate documented endpoints if available. Record provenance and dates rather than presenting historical configuration as current.
5. Review captures for public distribution and establish an independent backup. The current local archive is not a complete site or ecosystem backup.

## BUSD source-chain investigation pass (2026-10-11)

The [FDUSD follow-up](research/busd-fdusd-followup.md) adds 20-address checks on Ethereum, BSC and opBNB, Ethereum/BSC explorer observations, and attributed Binance conversion-source notes. Reviewed requests, responses, observations and hashes are distributed in `evidence/2026-10-11-fdusd-review/`. Explorer negatives and present balances do not establish complete history or exclude unknown custody.

The [case assessment](research/busd-case-assessment.md) consolidates Fusion accounting and the [source-chain investigation](research/busd-source-chain-flows.md). Reviewed public RPC receipts, state, headers, Routescan responses and explorer discovery observations are distributed in `evidence/2026-10-11-busd-source-review/`. Raw unrelated block-receipt discovery dumps remain in the ignored local archive. The package records coverage failures, including a demonstrably partial Ethereum index, and distinguishes verified Avalanche liquidity configuration from unverified reserve attribution.
