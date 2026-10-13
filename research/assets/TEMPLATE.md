# <Asset>: investigation package

Status: reproducible partial investigation / completed reconciliation (choose and justify).
Reviewed date and contribution scope:
Configuration: `reconciliation/<asset>.json`
Assessment and publication status:

## Identity and representations

| Chain / chain ID | Contract / explorer | Decimals and evidence | Role / representation | Relationship evidence |
| --- | --- | --- | --- | --- |

List unknown predecessor, migration, native-wrapper and time-slice relationships. Do not consolidate by symbol.

## Candidate custody roles

| Chain | Address / explorer | Role and active interval | Supporting evidence | Confidence / alternatives |
| --- | --- | --- | --- | --- |

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
