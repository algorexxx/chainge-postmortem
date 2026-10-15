# <Asset>: investigation package

Status: reproducible partial investigation / completed reconciliation (choose and justify).
Reviewed date and contribution scope:
Configuration: `reconciliation/<asset>.json`
Assessment and publication status:

## Identity and representations

| Chain / chain ID | Contract / explorer | Decimals and evidence | Role / representation | Relationship evidence |
| --- | --- | --- | --- | --- |

List unknown predecessor, migration, native-wrapper and time-slice relationships. Do not consolidate by symbol.

## Candidate vault roles

| Chain | Address / explorer | Role and active interval | Supporting evidence | Confidence / alternatives |
| --- | --- | --- | --- | --- |

## Acquisition method and cost

Follow the mandatory [indexed-first acquisition order](../indexed-history-workflow.md#before-any-broad-scan). Record reused evidence, indexed vault/token sources tried, pagination limits, targeted receipt/header checks and any remaining gap. Do not start with a full-chain block scan.

For any proposed broad fallback scan, record uncached request count, measured throughput, estimated elapsed time including cooldowns, evidence expected, alternatives tried and a stopping/reassessment condition. A scan estimated at 24 hours or longer requires explicit user approval of that specific exception before launch or automatic resumption. Keep approval details private if they contain user information; do not infer approval from a general request to complete the investigation.

## Coverage and cutoffs

| Dataset / scope | Requested range | Returned range / pages | Block hashes and actual UTC cutoffs | Verification / gaps |
| --- | --- | --- | --- | --- |

State the common UTC cutoff or explicitly state that none exists. Link successful balance reads and preserve failures.

## Accounting and interpretation

Link generated-output reproduction commands rather than duplicating the full generated ledger. Describe mint/burn semantics, migration/slice adjustments, opening balances and internal transfer handling. Outstanding claims and verified vault balances are unknown until independently established. Separate observed transfers, attributed statements, candidate matches and reviewed interpretations.

## Policy justification and review cases

Explain fee/time/threshold hypotheses and each optional rule. List event IDs and evidence for material accepted/rejected interpretations. Report unresolved terminal transfers, competing matches, missing receipt/timestamp coverage and alternative-asset dispositions.

## Reproduction and acceptance

`./reconcile.ps1 -Asset <asset>` then `./verify.ps1`.

List tested commit/platform, result changes, remaining limitations, privacy review and whether site publication is requested. To claim completion, demonstrate every completion condition in CONTRIBUTING.md with evidence; otherwise keep the status partial.


## Closing review and reopening conditions

Use `research/assets/<asset>-closeout.json` for event-ID-keyed findings, limitations and local evidence references. Do not copy generated ledger rows into the decision register. Declare the terminal window, configured material threshold, and additional older or post-announcement cases. Run `./research/review-closeout.ps1 -Asset <asset> -RequireReviewed` after the final reconciliation.

| Gate | Result / evidence | Remaining uncertainty / reopening condition |
| --- | --- | --- |
| Exact token and representation identity | | |
| Justified vault inventory by chain | | |
| Numeric two-direction history coverage | | |
| Receipt and block-header corroboration | | |
| Opening/closing state and interval agreement | | |
| Fusion rights, migrations, restrictions and inventory treatment | | |
| Comparable backing at aligned cutoffs; unverified backing separated | | |
| Every selected material outflow assessed; downstream mixing handled | | |
| Announcement context and post-snapshot limits | | |
| Common UI, report freshness, tests and privacy review | | |

Report the status of the bounded work separately from the status of the reconciliation. An assessment may finish at an exchange-account boundary, unknown ownership or an unavailable dataset. Those are explicit limits, not settled transactions. List methods actually attempted; do not claim all public sources are exhausted.

Keep a machine-readable `<asset>-workflow.json` with a `gates` array. Required IDs are `identity`, `vault-discovery`, `history`, `receipts`, `source-balances`, `fusion-state`, `fusion-semantics`, `holder-funding`, `outflow-disposition`, `announcements`, and `publication`. Each gate contains `id`, `status` (`pending`, `verified`, or `evidence-boundary`), `finding`, `nextAction`, and repository-relative file `references`. An evidence boundary also requires `attemptedMethods` and `boundaryReason`. Run `research/review-workflow.ps1 -Asset <asset> -RequireFinished` before claiming the bounded work finished. It cannot certify ownership or factual interpretation; the evidence still needs review.

For small ledgers use closeout `selection: "all-outflows"`; every retained external vault payment then requires a decision automatically. For a larger terminal selection, disclose excluded scope. Follow full swap receipts to the final output recipient and require successful traces for native delivery.
