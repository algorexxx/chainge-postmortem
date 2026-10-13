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
| Site assumed every assessed asset was BUSD | Preserved BUSD's bespoke overview behind an explicit presentation setting; generic studies use their own scope, report link, date and token labels; null vault balances render Unknown | Adding a public study is still an editorial step; USDC is not added to the site |
| Investigation acceptance criteria were spread across documents | Added shared contributor guide, package template, BUSD reference, USDC trial and PR template | Reproducible partial research and completed reconciliation remain different statuses |

## Second-asset validation

[USDC](assets/usdc.md) exercises a six-decimal Ethereum contract with no BUSD fee/migration rules. Retained public receipts provide 55 events and 12 candidate-address movements; new public-node calls corroborate decimals and two pinned balances. The same matcher ran without asset-specific code changes. The trial deliberately supplies no Fusion USDC issuance history and claims no complete custody inventory. It proves configuration reuse over a bounded real sample, not all-chain collector coverage or complete end-to-end settlement matching for a second asset.

[BUSD](assets/busd.md) retains all 47,808 events and 5,710 movements. Its generated site report remains byte-identical to the committed report. No rule window was widened and no unmatched event was hidden by this change.

## Verification

Release build: zero warnings/errors. All 169 tests passed. Application line coverage: 100%, including the new accounting builder. Tests cover internal/self-transfer flow arithmetic, full-time/slice separation, large integer precision, missing/extra movements, invalid amounts/precision, manifest formats, missing/mismatched hashes and escaped paths. An additional delayed-source-pair test confirms ordinary candidates are not re-emitted as delayed findings.

`./verify.ps1 -SkipBuildAndTests` passed after the tested Release build. JavaScript syntax and focused generic-study/null-balance/USDC-trace rendering checks passed. Local tests do not exercise GitHub environment permissions. No remote push or deployment was performed.

Both asset runs also passed from a temporary export of the committed Git tree, without private archives or untracked inputs; BUSD's public report remained byte-identical. Configuration and registry JSON line endings are pinned to LF so checkout conversion cannot change configuration hashes or stale-report comparisons across platforms. Original evidence remains exempt from conversion.

## What is still manual

Public-source acquisition, complete range/pagination checks, contract semantics, predecessor discovery, custody attribution, common-cutoff balances and reviewed settlement decisions still require investigation. There is no automatic live collector or accepted-decision accounting store. Full private export processing is deliberately outside this workflow. New event types need a supported adapter and tests; do not force native or non-EVM assets into the ERC-20 schema.

The next improvements should be driven by actual missing evidence or a demonstrated unsupported case in the next full asset investigation. Reducing the unmatched count by itself is not an acceptance criterion. The guide is a repeatable first version, not a claim that every token or investigative uncertainty is automated.
