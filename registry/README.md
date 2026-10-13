# Site data and local preview

The static registry presents reviewed research alongside an explicitly provisional, generated movement ledger. Matching runs offline in the reconciliation script; the browser does not collect chain data or complete an asset reconciliation.

## Local preview

Run `./preview.ps1` from the repository root, then open http://127.0.0.1:8765 in your browser. Keep the terminal running; Ctrl+C stops the server. This rebuilds the site and serves the generated HTML using the installed .NET SDK. The server binds only to loopback and exposes only `/` and `/index.html`, not repository or archive files. Use this HTTP preview for the interactive UI; the Codex file preview has not rendered the scripts/styles reliably.

## Build

From the repository root, with .NET SDK 10.0.401 or a later compatible patch:

```powershell
dotnet build Chainge.Postmortem.slnx
dotnet test --solution Chainge.Postmortem.slnx --no-build --coverage --coverage-output-format cobertura
dotnet run --project Chainge.Postmortem.Site --no-build -- evidence/2026-10-11-token-catalogue/tokens.csv registry/studies.json dist
```

For local use, prefer `./preview.ps1` above. For deployment, serve the generated `dist/index.html` using a static HTTP host. The generated HTML embeds its styles, JavaScript and registry data and needs no companion files. If a file preview does not run scripts, open the file directly in your browser instead. All runtime assets are embedded; no RPC connection, API key, analytics or remote font is required. Evidence links open public source reports and explorers. Only the generated `dist/` directory is intended for static hosting. It contains no archive files.

The output uses embedded assets and hash-based token selection, so it can be served under a GitHub Pages repository subdirectory without a server-side route fallback. Publication and remote Git pushes remain with the maintainer. No deployment workflow or automatic refresh is enabled.

## Sources and interpretation

- The generator reads the independently downloaded [public CSV](../evidence/2026-10-11-token-catalogue/tokens.csv) directly. It validates its headers/address syntax and groups addresses case-insensitively. All source row numbers and distinct source labels are retained. This produces 2,184 contract entries from 2,185 rows. Four hash-only source labels are retained verbatim.
- `studies.json` is an editorial presentation of cited reports. It is deliberately limited to selected BUSD positions and five reviewed outflow cases. Amounts are decimal strings, never floating-point ledger values. Numeric summaries truncate to two decimals; exact values remain in the data, detailed ledger/path views and source reports. Tiny nonzero balances display their full exact value beside the abbreviated amount.
- The other 2,183 contracts are selectable but unassessed. They have no invented supply, vaults or zero-balance defaults.
- Gross inflows include routing and internal transfers. They are not unique deposits. Newer-token supply is not a consolidated outstanding-claims denominator. No backing ratio or deficit is calculated with incompatible cutoffs or incomplete custody coverage.
- Movement views default to unmatched first, newest first, then amount; newest, largest, reconstructed balance reduction and destination unmatched-total ordering are available in the automated ledger. The 1.2-million case aggregates two separate outflows, not a single transfer. Repeated hops are not additive losses. An unmatched case has no verified burn linkage; this is not an allegation about its purpose. Cross-chain and redemption relationships remain explicitly inferred.
- Exchange destinations use attributed explorer labels; tracing stops at the boundary where public evidence cannot identify account credit or further disposition.

`studies.json` is an authored display selection, not a second accounting ledger. Changes to the research should be reviewed into this selection explicitly. The separate generated movement report covers every configured operating-address movement in the retained evidence, not every possible custody address or every historical transfer. Authored traced cases are not added to its totals.

## Implementation and checks

The Domain project holds the source token record. Application groups catalogue entries and attaches studies by contract address. Persistence.Json reads the local CSV/JSON sources and maps source records to Application models. Site composes dependencies and emits static assets; browser JavaScript controls presentation, search and selection. JSON serialization escapes untrusted token labels, which are also HTML-escaped when rendered.

Application tests cover duplicate-address grouping, source-row/alias retention, reviewed-first ordering and rejection of studies pointing outside the catalogue. The builder reached 100% line coverage; DI registration is excluded. Build completed with zero warnings. Manual data validation checked exact mint-minus-burn arithmetic, case ordering, transaction hashes and amounts against the cited reports.

Browser checks covered symbol search, no-result state, unassessed-token selection, investigation-only filtering, tabs and keyboard navigation, vault dust precision, outflow filtering and path selection. Overview and movements were checked at a 390-pixel viewport with no page-level horizontal overflow. No browser warnings or errors were observed. The UI does not test completeness of the underlying investigation.

The standalone-output regression check served only the generated index.html from an otherwise empty directory. The full catalogue, BUSD overview, movements and token search loaded without companion files or browser errors. This verifies HTTP browser execution; the Codex file preview may impose separate script restrictions.

Each reviewed case has its own movement card. Possible redemptions are hidden by default and can be included using the explanation filter. The list states how many reviewed cases are shown. The two reviewed January 2022 pairs are partial coverage; remaining payments are not presumed redemptions. The [offline reconciliation pipeline](../reconciliation/README.md) generates a separate candidate queue; generated proposals are not automatically published as reviewed cases.

## Generated movement ledger

Run `./reconcile.ps1 -Asset busd` before rebuilding the preview to refresh `registry/busd-movements.json`. The wrapper copies the CLI's `archives/reconciliation/busd/site-movements.json` only after a successful run. The study's `movementsFile` resolves relative to `studies.json`; a missing or invalid declared file fails the build instead of silently omitting movements. A fresh clone can build from the committed public report without RPC access or the ignored archives. For another asset, configure its evidence, run reconciliation and reference `<asset>-movements.json` from its study. No publication or network refresh is scheduled.

The generated public report is a static distribution artifact, not a second hand-maintained ledger. The full local reconciliation JSON is approximately 78.5 MB; the compact site report is approximately 7.3 MB and the self-contained HTML approximately 7.6 MB for BUSD. This avoids embedding unrelated event payloads in the public UI. Both the configuration SHA-256 and input-file hashes are retained for provenance; all financial amounts remain integer strings with explicit decimals. Regenerate the report rather than editing amounts or classifications by hand.

The current report contains 5,710 unique movements: 2,665 outgoing, 3,031 incoming and 14 internal. All 224 normally unmatched outflows are present. Cards paginate in batches of 50; hidden/matching counts distinguish filtering from pagination. Filters cover direction, classification, chain, token contract, recipient and minimum amount. There is no minimum amount by default. Possible redemptions are hidden by default but remain accessible. The five authored traced cases remain in a separate view and matching transaction hashes link to them from automated details.

Destination summaries group outgoing events by chain, token and immediate recipient, excluding incoming and internal movements. All 538 groups retain gross and unmatched counts and amounts. They include small payments hidden by card filters and do not deduct returns. The top-ten destination table applies only the chain filter and states this explicitly; selecting a row sets its chain, token and recipient filters. Gross totals are not unique capital, balances, beneficial ownership or losses.

Balance reduction is payment divided by reconstructed pre-payment sender balance, floored to hundredths of a percent. Nonpositive opening balances, missing values, negative payments and payments exceeding the reconstructed balance produce unavailable ratios. Otherwise, the percentage is conditional on coverage and the scanner's zero-opening-balance assumption, not a verified vault balance. Normal and additional delayed candidate relationships remain separate and do not establish final settlement. Detail panels retain receipt verification, event IDs, evidence links, candidate rules, competition flags, fee hypotheses and report limitations.

Validation: 132 passing tests and 383/383 Application lines covered. Independent report checks confirmed exact event-ID preservation, no duplicates, and every destination aggregate against source movements. Browser checks cover all-movement and unmatched counts, amount/direction/chain/token/recipient filters, sorting, pagination, destination drill-down and the traced-case link.

## Terminal outflow review

The movements tab opens a combined Closing transfers list, newest first across all configured operating addresses and chains. One expandable card represents each distinct last substantial or final payment; a transfer meeting both criteria appears once. Traced-case explanations are shown when available, with automated matching and vault balance context inside expandable evidence sections. Reconciliation regenerates these summaries automatically. No new threshold is implicit: “substantial” reuses `policy.largeOutflowRaw` and its decimals (10,000 BUSD); the net-reduction indicator reuses `policy.depletionBasisPoints` (90%). No classification is changed or excluded from this view, including possible redemptions and internal operating-address transfers.

Each observed history is ordered by block and log index. The generated report retains the last substantial positive withdrawal, final positive withdrawal and last ten positive outflows. Zero-value and self-transfers do not displace these withdrawals. The final sequence starts with the last substantial withdrawal, or the oldest of the last ten positive outflows if there is none, and continues through the last retained event. It includes replenishment, internal transfers, self-transfers and zero-value events. It is a deterministic review window, not proof of shutdown or an optimized grouping of every possible withdrawal pattern. Missing histories are not invented.

Balances are independently replayed per address from zero, including incoming internal transfers to that address. Opening plus gross receipts minus gross payments must equal the final reconstruction. Net reduction is floored to hundredths of a percent and withheld for absent/nonpositive opening balances, net increases, or any negative reconstructed balance in the history. Receipt verification remains available in transaction evidence; a nonnegative replay or final zero does not prove completeness or reserve ownership.

Recipient convergence compares positive payments within these sequences. Same-chain/token groups require distinct source addresses. A separate view reports identical recipient address bytes across chains without combining amounts or asserting common control. It covers immediate recipients only; later intermediaries remain in the linked traced cases. Every listed transfer opens its existing evidence, normal/delayed candidates and linked narrative findings. The complete automated ledger remains selectable.

The BUSD run produces six histories. The controller's last substantial/final payments are 204,567.658073647823395109 / 143.724135682808759163 on Ethereum and 187,290.327160557405335843 / 1.187246977608202752 on BSC. Three cross-chain recipient-address groups are observed, including the successor controller, the November recipient and the January cleanup recipient. No same-chain/token recipient is shared by distinct source addresses within the selected sequences. These findings are scoped to the retained histories and the sequence rule.

Validation: 141 passing tests, 453/453 Application lines covered, six independent balance replays, unchanged 5,710 movement IDs, and browser checks of address selection, final-payment evidence, recipient reuse, and switching back to the full ledger.

### Script review corrections

An independent code review reproduced three defects with synthetic receipts and balance histories. Pool transfer matching now uses the first subsequent swap emitted by the same pool and restricts the other transfer leg to the interval after that pool's previous swap. Multiple fitting counterpart legs are withheld rather than attributed as multiple verified routes. This is conservative for prefunded or otherwise nonstandard multi-operation receipts; those may remain unresolved.

The movement scanner retains a per-chain/token/account history inconsistency after any negative reconstructed balance, including events outside the displayed movement set. Later recovery does not restore depletion confidence: the `inconsistent-balance-history` flag persists, the large-balance-reduction flag is suppressed, and the site percentage remains unavailable. Existing negative-balance flags also suppress percentages. Raw before/after reconstructions remain available for audit.

Explicit exchange evidence now takes precedence over the generic counter-asset heuristic when no Fusion candidate exists. Internal transfers, missing timestamps and other existing classification gates remain unchanged. The BUSD rerun relabels 1,582 movements from `possible-asset-exchange` to `reported-asset-exchange`; all 5,710 movement IDs, 224 unmatched outflows, retained amounts, and terminal balance summaries remain unchanged. Neither the pool-boundary correction nor the persistent-history correction changes BUSD exchange-evidence counts or depletion percentages in this run. These are code corrections, not new transaction findings.

Regression coverage includes incoming/outgoing repeated same-pool swaps, reversed receipt-swap order, competing counterpart legs, no later swap, recovered negative histories, independent account histories, and explicit swap evidence alongside the generic heuristic. All 152 tests pass; Application line coverage is 458/458.

### Combined closing-transfer presentation

The default BUSD shortlist has eight distinct cards across six histories, with no vault selector or side-by-side evidence panel. Full ledger retains all 5,710 movements and its existing filters. This presentation change does not change classifications, matching rules or retained evidence.


## Project-wide timeline

`timeline.json` is the reviewed source for the Timeline navigation item (`#timeline`), independent of token selection. The generator loads it beside `studies.json` and embeds it in the self-contained page. It contains milestone summaries and selected founder excerpts, never the raw chat export. All dates use UTC Unix timestamps; publication, retained edit time and described event/target dates are separate. See `research/service-timeline.md` for attribution and privacy boundaries. `timeline.sha256` records the reviewed derivative. Update the manifest after a reviewed timeline change.
