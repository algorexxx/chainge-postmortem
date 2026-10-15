# Per-asset workflow audit

Reviewed 2026-10-13. Scope: offline pipeline, input integrity, accounting coverage, publication handoff and contribution procedure. This is a code/workflow audit, not a new certification of the underlying BUSD findings.

## Findings and changes

| Finding | Resolution | Remaining boundary |
| --- | --- | --- |
| Inputs were hashed after loading but never compared with preserved manifests | The CLI now verifies selected evidence and discovery input hashes against their nearest ancestor manifest and records manifest hashes in the accounting summary | Hashes do not authenticate the provider; unselected attachments still require review |
| The source-review manifest omitted later BUSD refinements | Appended 15 checksum entries for five existing response batches and their request/capture companions; no capture bytes or prior entries changed | This establishes the current saved bytes, not a retroactive capture-time signature |
| No short, exhaustive event accounting output | Added `accounting-summary.json`: per-chain/token/interval issuance, movement classification and per-address flow totals; exact integer amounts; reject missing/extra movement IDs and invalid precision/negative amounts | Observed net issuance is not consolidated claims; net flow is not a closing balance; dated interval sums are not time-normalized |
| Running reconciliation immediately overwrote the public report | `reconcile.ps1` now requires `-UpdateRegistry` for the copy | Updating the registry still requires evidence and presentation review |
| Empty Fusion queues could be mistaken for complete settlement | Explicit console warning when no Fusion events are supplied; summary states missing rows are unknown | Other incomplete windows still need package coverage declarations |
| No common contribution verification gate | `verify.ps1` builds/tests, runs every configured asset, verifies selected-input hashes, checks existing published movement reports byte-for-byte, and generates the site; PR and Pages workflows invoke it | GitHub-hosted execution remains to be verified after push; local tests ran on Windows |
| Site assumed every assessed asset was BUSD | Preserved BUSD's bespoke overview behind an explicit presentation setting; generic studies use their own scope, report link, date and token labels; null vault balances render Unknown | Adding a public study remains an editorial step; the later USDC investigation is explicitly partial |
| Investigation acceptance criteria were spread across documents | Added shared contributor guide, package template, BUSD reference, USDC trial and PR template | Reproducible partial research and completed reconciliation remain different statuses |

## Second-asset validation

The initial USDC trial exercised a six-decimal Ethereum contract with no BUSD fee/migration rules: 55 events and 12 candidate-address movements, plus decimals and two pinned balances. It supplied no Fusion history. This was configuration reuse over a bounded sample, not complete settlement matching.

The subsequent [USDC investigation](assets/usdc.md) retains 42,183 newer and 142,178 legacy full-time Fusion transfers, source receipts, dated-slice context and balance probes across four source chains. The engine still needed no asset-specific code. The subsequent bulk Fusion import corroborates every observed mint/burn transaction and identifies 1,366 conversion-related issuance events without changing accounting totals. Concrete limits remain: routing-input decoding, predecessor/other-chain coverage, dated operating roles and representation consolidation. The initially reported gateway blocker was a connection-path error; the documented SSH tunnel works (see the access correction linked from the assessment). Explorer transaction detail is not silently promoted to a verified receipt. The site publishes selected reviewed cases and explicitly partial balances; the unfinished automated queue remains reproducible locally. Traces now support explicit per-step asset labels so USDT outputs are not rendered as USDC, and Arbitrum addresses have explorer links.

[BUSD](assets/busd.md) retains all 47,808 events and 5,710 movements. Its generated site report remains byte-identical to the committed report. No rule window was widened and no unmatched event was hidden by this change.

## Verification

Release build: zero warnings/errors. All 169 tests passed. Application line coverage: 100%, including the new accounting builder. Tests cover internal/self-transfer flow arithmetic, full-time/slice separation, large integer precision, missing/extra movements, invalid amounts/precision, manifest formats, missing/mismatched hashes and escaped paths. An additional delayed-source-pair test confirms ordinary candidates are not re-emitted as delayed findings.

`./verify.ps1 -SkipBuildAndTests` passed after the tested Release build. JavaScript syntax and focused generic-study/null-balance/USDC-trace rendering checks passed. Local tests do not exercise GitHub environment permissions. No remote push or deployment was performed.

Both asset runs also passed from a temporary export of the committed Git tree, without private archives or untracked inputs; BUSD's public report remained byte-identical. Configuration and registry JSON line endings are pinned to LF so checkout conversion cannot change configuration hashes or stale-report comparisons across platforms. Original evidence remains exempt from conversion.

## What is still manual

Public-source acquisition, complete range/pagination checks, contract semantics, predecessor discovery, custody attribution, common-cutoff balances and reviewed settlement decisions still require investigation. There is no automatic live collector or accepted-decision accounting store. Full private export processing is deliberately outside this workflow. New event types need a supported adapter and tests; do not force native or non-EVM assets into the ERC-20 schema.

The next improvements should be driven by actual missing evidence or a demonstrated unsupported case in the next full asset investigation. Reducing the unmatched count by itself is not an acceptance criterion. The guide is a repeatable first version, not a claim that every token or investigative uncertainty is automated.


## Routing follow-up

A reusable, configuration-selected routing reviewer now emits `routing-review.json` without changing normal matching or accounting. It validates the observed burn-call layout and receipt binding, retains unsupported formats, and compares decoded destination fields against successful source receipts. USDC yielded 676 decoded memos and three exact destination/amount/time payout candidates, including the newly captured Arbitrum payout. JSON and Kaspa address variants remain unsupported; the router has no verified ABI in the retained explorer response. See the USDC assessment for evidence and limits.

Follow-up validation: Release build clean, all 188 tests passed, Application line coverage 100%. Full workflow verification passed; published BUSD movement output remains byte-identical. The generated USDC site renders five reviewed cases, including the Arbitrum candidate.


## Anonymous-source expansion and automated audit

The later USDC pass supports 678 colon memos and 119 flat JSON memos, including the formerly unsupported Kaspa variants. JSON token indices are deliberately not resolved by guesswork. Broader source evidence produces 32 exact destination candidates without altering automatic settlement rules.

Reusable additions now audit numeric log-query coverage in both directions, replay per-representation holder positions, label claim intervals, preserve dated-child exclusions, and compare saved pinned state calls with ledger replay. Failed/unpinned/constrained queries receive no coverage credit; balance agreement with incomplete history remains explicitly incomplete. This codifies checks previously performed in one-off acquisition analysis. It does not consolidate customer claims or establish source-token backing.

An anonymous second Ethereum explorer disclosed 2,960 rows for a candidate whose original provider returned 719. Paginated explorer rows are now imported without receipt-verification promotion, deduplicated against actual log identities, and checked for contradictions. All five candidate cutoff nets match historical node balances. A free BSC endpoint supplied historical state after six public endpoints failed; log access remains limited to inclusive 10,000-block windows.

The updated suite passes 231 tests. Release build has no warnings or errors. Handwritten Application coverage is 618/618 lines; generated regex implementation is reported separately by the coverage tool. The final full workflow verification passed, including input-manifest checks, static-site generation and BUSD's byte-identical public movement report. Public-source acquisition scheduling, representation consolidation, custody attribution and economic interpretation remain manual. The completion status is still partial.


## October 15 closing workflow

The ordered [contributor/LLM guide](../CONTRIBUTING.md) now covers identity and rights inventory, aligned cutoffs, anonymous resumable acquisition, second-plan receipts/headers, state/replay discrepancy localization, material-case tracing, explicit stopping/reopening conditions, common UI and PR hygiene. Evidence failures and observation limits remain first-class results.

The new closing-review gate uses configured exact-integer materiality and a declared per-vault terminal window, retains undated cases, includes explicit older/post-snapshot exceptions, and requires findings, limitations and evidence references. It does not change matching classifications. Verification invokes the gate for asset registers and tests amount/date boundaries, missing dates, explicit exceptions and rejection of incomplete/duplicate decisions. The receipt/state check protects the BUSD gap correction and USDC retained downstream balance.

The build and 299 offline .NET tests passed during this pass. Final end-to-end verification results belong to the corresponding commit/turn; this checkpoint does not imply complete history, custody or consolidated claims. See [the current assessment](closeout-assessment.md) for the remaining public-evidence boundaries.

The expanded ledger also exposed a console-summary bottleneck: membership tests scanned the full unmatched-ID list for each event. The summary now uses an ordinal hash set, preserving output while avoiding repeated linear scans. No matching or accounting rule changed.

The provenance refresh helper checks unchanged published Fusion statistics and the complete known holder inventory before updating its configuration hash. This closes the manual source-configuration refresh step without accepting changed supply or balances.

## BAT workflow trial

BAT exposed two missing acquisition-plan checks. A successful receipt did not ensure transaction input was retained, and an explorer timestamp prevented a block-header request. The planner now requests missing transaction inputs and tracks header provenance separately from timestamp provenance. Equal timestamps preserve existing header evidence regardless of file order; conflicting timestamps remain subject to the existing quarantine rule. Empty calldata (`0x`) is valid retained input. Regression tests cover these cases; no matching rule is relaxed.

The full catalogue and source-chain screen also overturned the initial narrow-chain assumption. Future tokens must inventory predecessor and dated representations and test plausible bridge contracts rather than treating an issuer FAQ or market-data list as exhaustive. BAT's node-access and BSC coverage gaps remain explicit; source-chain balance agreement alone does not close them.

Full regression verification then ran out of memory while serializing USDC's reconciliation JSON to a single string. That report now streams directly to a file with the same serializer options, removing the full intermediate string without changing accounting or the report schema. The build and 304 offline tests pass after this change; the complete asset verification also checks generated public-report freshness.

The final full verification passed for BAT, BUSD and USDC. The streamed BAT reconciliation is byte-identical to the former serializer output, and the 1,074,883,289-byte USDC report was written successfully. Shared UI smoke checks now enumerate every published asset rather than only BUSD and USDC. These are workflow checks, not evidence of a completed BAT reconciliation.
