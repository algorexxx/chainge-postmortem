# Chainge asset research

Independent, evidence-based research into Chainge-related tokens, contract administration, candidate custody relationships and asset movements. This repository is not an official publication of Chainge, Fusion or fusion-community.

The work does not presume intent, responsibility or the cause of service disruption. Reports distinguish observations, attributed statements, inferences and hypotheses. Addresses have chain-specific, time-dependent roles; a transfer or matching address alone does not establish ownership or custody.

## Start here

- [BUSD case assessment and remaining questions](research/busd-case-assessment.md)
- [BUSD source-chain flows and liquidity addresses](research/busd-source-chain-flows.md)
- [Research plan and evidence policy](PLAN.md)
- [Source inventory and preservation coverage](SOURCES.md)
- [Archive handling](ARCHIVING.md)
- [Initial contract findings](research/initial-contract-findings.md)
- [Factory and mint authority](research/factory-and-mint-authority.md)
- [Factory history and BUSD supply](research/factory-history-and-busd-supply.md)
- [BUSD migration and balance flow](research/busd-migration-and-balance-flow.md)
- [Legacy BUSD and funding origin](research/legacy-busd-and-funding-origin.md)
- [Legacy balance match and administrative transition](research/legacy-balance-match-and-admin-transition.md)
- [Legacy authority and time-slice accounting](research/legacy-authority-and-slice-accounting.md)
- [BUSD ledger reconciliation and migration classification](research/busd-ledger-reconciliation.md)
- [BUSD time-slice coverage and earlier representation](research/busd-time-slice-coverage.md)

## Current coverage

The initial investigation covers a small Fusion token sample and a BUSD pilot. Fusion state observations use a retained block dated 2025-10-07; they do not establish present-day state. Reports document historical-query limitations and incomplete coverage. An Avalanche router's configured liquidity-address role and two subsequent changes are verified; segregated reserves and complete source-chain custody remain unconfirmed. A public site and repeatable collector are planned, not implemented.

Reviewed query evidence is stored under `evidence/`, with request/response pairs, capture metadata and integrity manifests where available. Some source-inventory entries refer to local archives that are not distributed in this repository. A clone therefore does not contain the complete preservation archive.

## History and corrections

The first commit records the existing research as a baseline. Capture timestamps and report dates describe earlier work; Git history begins with that baseline and does not reconstruct prior editing sessions.

Commit each completed investigation or coherent correction with its supporting reviewed evidence. Explain what changed and why. Preserve original captures, including failed requests; add new evidence rather than silently replacing old observations. Corrections to published findings should identify the affected conclusion and its replacement. Keep routine research history rather than rewriting it to remove superseded interpretations.

Evidence files are exempt from Git line-ending conversion so their saved-byte SHA-256 hashes survive commits and checkouts. Hash verification checks file integrity, not the truth or completeness of the source.

## Contributions and privacy

Public contributions must use independently retrieved public evidence and clearly mark uncertainty. Do not submit private messages, chat exports, personal transaction records, contributor identities or other direct community submissions through issues, pull requests or repository files. No private intake mechanism has been established here.

Raw Telegram exports stay outside the checkout. The ignored `archives/` directory holds unreviewed public captures and is not part of the published repository. Follow the publication policy in [PLAN.md](PLAN.md) before adding material.
