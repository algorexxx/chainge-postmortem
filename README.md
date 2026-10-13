# Chainge asset research

Independent, evidence-based research into Chainge-related tokens, contract administration, candidate custody relationships and asset movements. This repository is not an official publication of Chainge, Fusion or fusion-community.

The work does not presume intent, responsibility or the cause of service disruption. Reports distinguish observations, attributed statements, inferences and hypotheses. Addresses have chain-specific, time-dependent roles; a transfer or matching address alone does not establish ownership or custody.

The required result for every asset is a complete ledger comparing outstanding Fusion tokens with balances at its verified source-chain vault addresses at a common cutoff. Gross token balances, accounting adjustments, individual vault holdings and unresolved gaps must remain visible. See the [completion standard](PLAN.md#required-outcome-for-every-asset).

## Start here

- [Investigate an asset and submit a PR (people and LLMs)](CONTRIBUTING.md)
- [Build, preview and publish the token registry UI](registry/README.md)
- [Service timeline: sources, attribution and open questions](research/service-timeline.md)

- [BUSD case assessment and remaining questions](research/busd-case-assessment.md)
- [January 2024 BUSD transfer to a Binance-labelled address](research/busd-204k-exchange-path.md)
- [FDUSD replacement-reserve follow-up and search limits](research/busd-fdusd-followup.md)
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

The initial investigation covers a small Fusion token sample and a BUSD pilot. **BUSD is incomplete: the outstanding-token/vault reconciliation has not yet been delivered.** Fusion state observations use a retained block dated 2025-10-07; they do not establish present-day state. Reports document historical-query limitations and incomplete coverage. An Avalanche router's configured liquidity-address role and two subsequent changes are verified; segregated reserves and complete source-chain custody remain unconfirmed. A local static registry UI now covers all 2,184 source-listed contracts, with detailed BUSD positions, selected traced paths and a provisional ledger of all 5,710 retained operating-address movements. The offline script proposes burn/payment matches; those candidates do not establish settlement. A GitHub Pages workflow is prepared for publication after maintainer setup and push; a repeatable live chain collector is not implemented.

Reviewed query evidence is stored under `evidence/`, with request/response pairs, capture metadata and integrity manifests where available. Some source-inventory entries refer to local archives that are not distributed in this repository. A clone therefore does not contain the complete preservation archive.

## History and corrections

The first commit records the existing research as a baseline. Capture timestamps and report dates describe earlier work; Git history begins with that baseline and does not reconstruct prior editing sessions.

Commit each completed investigation or coherent correction with its supporting reviewed evidence. Explain what changed and why. Preserve original captures, including failed requests; add new evidence rather than silently replacing old observations. Corrections to published findings should identify the affected conclusion and its replacement. Keep routine research history rather than rewriting it to remove superseded interpretations.

Evidence files are exempt from Git line-ending conversion so their saved-byte SHA-256 hashes survive commits and checkouts. Hash verification checks file integrity, not the truth or completeness of the source.

## Contributions and privacy

Public contributions must use independently retrieved public evidence and clearly mark uncertainty. Do not submit private messages, chat exports, personal transaction records, contributor identities or other direct community submissions through issues, pull requests or repository files. No private intake mechanism has been established here.

Raw Telegram exports stay outside the checkout. The ignored `archives/` directory holds unreviewed public captures and is not part of the published repository. Follow the publication policy in [PLAN.md](PLAN.md) before adding material.

## Offline asset reconciliation

Run `./reconcile.ps1 -Asset busd` to normalize retained public events, propose deposit/redemption matches and flag unresolved outflows for review. See [configuration, outputs and limitations](reconciliation/README.md). New assets use their own configuration; publication remains one asset at a time and generated candidates remain separate from reviewed findings.
