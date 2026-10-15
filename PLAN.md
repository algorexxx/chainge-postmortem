# Chainge assets and custody research plan

Created: 2026-10-11. Status: BUSD supporting ledgers and custody findings recorded; complete per-asset outstanding-token/vault reconciliation remains pending.

## Purpose

Preserve evidence and document Chainge-related assets, candidate custody addresses, service history and subsequent on-chain activity. Produce a reproducible, strictly informational public record. Conclusions must follow evidence, with assumptions and unresolved questions clearly marked.

The project does not presume a cause for service disruption, address ownership, intent or responsibility. Use neutral language in documentation, data labels, site text, issues and commit messages.

## Required outcome for every asset

The deliverable is a complete, reproducible ledger of outstanding Fusion tokens versus balances at the source-chain vault addresses for that asset. Transaction tracing and narrative reports support this reconciliation; they do not establish completion by themselves. BUSD remains incomplete under this standard.

Each asset must have:

1. **Fusion outstanding-token ledger.** Inventory all current and historical representations, issuance, burns and holder balances. Reconcile event-derived balances to pinned state and supply where the deployed accounting model permits. Show gross balances separately from any adjusted outstanding total. Document the treatment of migrations, blocked holdings, address-one positions and time slices; do not silently remove balances or count overlapping representations twice. Unproven adjustments remain unresolved.
2. **Vault inventory and balance ledgers.** Identify each relevant source-chain token and vault address, its role and validity period, including predecessor and successor addresses. Preserve attribution evidence separately from balance evidence. For each address/token, reconcile opening balance plus inflows minus outflows, with any documented token-specific adjustments, to closing state. Keep candidates separate from verified custody positions and document the search coverage for undiscovered vaults.
3. **Comparable snapshots.** Choose a common UTC cutoff and record each chain's canonical block number, hash, timestamp and distance from that cutoff, using the latest available block at or before it. Do not compare different dates as if contemporaneous. Where newer Fusion state is unavailable, produce the historical matched reconciliation first and show subsequent source-chain movements and balances separately.
4. **Per-asset comparison.** Publish the gross Fusion amount, evidence-backed adjustments, adjusted outstanding amount, each verified vault balance, eligible vault total, difference (vault total minus outstanding) and coverage ratio (vault total divided by outstanding, undefined when outstanding is zero). Only combine demonstrably equivalent asset units. Separate wrapped assets, other tokens received in swaps and unverified exchange credits; avoid counting both bridge collateral and its wrapped representation as separate backing.
5. **Evidence and completeness status.** Link every position, adjustment and movement to reproducible evidence, including collection ranges and gaps. A swap, exchange deposit or bridge transfer does not itself extinguish Fusion tokens. Unknown custody coverage or outstanding amounts must remain unknown, never zero. Partial observed totals may be published with explicit scope, but cannot establish an overall surplus or shortfall.

An asset is complete only when the representation inventory, outstanding-token accounting, vault inventory, movement coverage and matched balances are supported and reconciled. Any material unresolved component keeps its status incomplete, with the missing evidence and next action stated. Record gross on-chain tokens separately from any interpretation of customer entitlement.

## Agreed boundaries

- Budget is zero. Use free public sources and existing local resources. Record limits rather than silently substituting incomplete data. No paid subscriptions or infrastructure.
- Investigations and results may be published as soon as they meet the evidence and privacy rules below; unfinished work may be public if clearly labelled.
- Never commit material received directly from community members: messages, names, exports, screenshots, personal transaction records or attachments. This also applies to issues, pull requests, build logs, artifacts and the public site.
- A private lead may guide independent research. Only independently retrieved public evidence may support a resulting public record; never publish the contributor's identity or link a person to a wallet from their submission.
- Public Telegram information from ordinary participants may be used only after removing identifying information. DJ Qian, verified staff acting in an official capacity and official announcements may retain relevant public attribution. Verify the account and role rather than relying on a display name.
- Do not copy raw chat exports into the repository, even temporarily or in an ignored directory. Keep original private material outside the checkout and public build inputs.
- No incident date is established yet. Derive a timeline from preserved public announcements, accessible X/Twitter and Telegram history, and independently verifiable chain records.
- Initial chain and asset selection is evidence-driven. Begin by assessing BNB Smart Chain and Fusion access, then select approximately 5â€“10 assets for a pilot if the evidence supports them.
- This plan does not start an automation, create a remote repository or publish a site. Those are later implementation milestones.

## Evidence and publication policy

Keep these record types distinct:

| Type | Meaning | Minimum support |
| --- | --- | --- |
| Observation | Directly retrieved source content or on-chain event | Source, retrieval time and preserved evidence |
| Attributed statement | What an identified official account stated | Verified attribution and source; not proof the statement is true |
| Inference | Interpretation of observations | Linked evidence, reasoning, alternatives and limitations |
| Hypothesis | A question or possible relationship to investigate | Explicit uncertainty and a proposed verification method |
| Unresolved or contradicted | Insufficient or conflicting support | The gap or conflicting evidence and revision history |

Address roles are time-dependent assertions, not permanent labels. Distinguish a token contract, deposit address, custody address, router, liquidity pool and transaction counterparty. A transfer alone does not prove common control, custody responsibility or the purpose of a movement. Similar amounts, timestamps, symbols or funding patterns are leads, not sufficient attribution.

Use chain-qualified identifiers. Permit many-to-many relationships between Fusion representations, source-chain assets and candidate custody addresses. Preserve historical migrations and competing interpretations. Never imply that every Fusion token has its own source-chain vault.

Each published finding must include evidence references, observation time or block range, method, status, limitations and what would strengthen or disprove it. Retain corrections and superseded findings. A capture hash establishes file integrity, not the truth of its contents or an independently certified capture date.

Privacy review must cover text, filenames, metadata, screenshots, reply and forward headers, user IDs, handles, profile links, wallet-to-person associations and attachments. Ordinary-user message links, exact timestamps and searchable quotations can reidentify a speaker; omit or generalize them where necessary. If safe anonymization would destroy the evidence needed to support a claim, do not publish the claim from that source. Keep any private provenance mapping outside the repository.

## Phase 1: Preserve sources and establish coverage

1. Inventory each source with URL, source type, access method, capture time in UTC, available date range and access limitations.
2. Preserve public web documentation, relevant API descriptions and accessible official communications. Save machine-readable content where available, with rendered captures when presentation matters.
3. Export the spreadsheet, including all accessible tabs, and record tab identifiers, headers, export method and observed row counts. Preserve the original export separately from normalized records. Classify entries before treating them as distinct assets.
4. Mirror relevant public Git repositories with history, branches and tags. Inventory releases and supporting documentation separately; a Git mirror alone does not preserve issues, release assets or external dependencies. Record commit IDs. Review content before including archival material in public distribution.
5. Obtain a local Telegram export. Preserve originals and media outside the repository, then produce a separately reviewed public subset. Capture official X/Twitter records through available free access and record any missing history.
6. Hash preserved files with SHA-256. Record original and sanitized derivatives separately, including the sanitization method. Keep a second copy on existing separate storage where available; record if no independent backup exists.

Completion: a source inventory, integrity manifest for publishable captures, private archive arrangement and explicit coverage gaps. A web search result or cached excerpt is a discovery aid, not a substitute for an original capture.

### Telegram collection handoff

Use Telegram Desktop, open the Chainge group, and select the chat menu's **Export chat history** option. Export all accessible dates as JSON, including relevant photos, files and video/audio where practical. If media volume is large, preserve text first and record omitted media for a follow-up pass. An HTML export can provide a human-readable companion.

Choose a destination outside this repository. Record the export time, selected options, available date range and any errors privately. Do not upload or commit the raw export. Once its location is known, build a local extraction and redaction workflow before producing public records. Exported history may omit deleted messages and inaccessible older history; absence is not proof a message never existed.

Reference: [Telegram's export documentation](https://telegram.org/blog/export-and-more).

## Phase 2: Reconstruct the service timeline and asset model

1. Identify official accounts and staff roles with public supporting evidence.
2. Record dated announcements and claims about withdrawals, asset access, maintenance and restoration. Distinguish when an event allegedly happened, when it was announced and when it was captured.
3. Keep multiple incident milestones rather than imposing one shutdown date. If a date is uncertain, record an interval and the reason.
4. Inspect token lists, deployment code, SDK/API examples and historical configuration for chain IDs, asset identifiers, contract addresses and custody/deposit routing.
5. Classify Fusion entries: native asset identifiers, token contracts, time-sliced representations, liquidity positions, legacy deployments and unknowns, as evidence permits. Verify distinctions against deployed behavior and chain data.
6. Document the normal deposit, mint, burn and withdrawal paths from historical successful examples. A code repository describes code; it does not prove that code was deployed or operated as described.

Completion: a sourced preliminary timeline, token classification method and a documented set of architecture questions.

## Phase 3: Trace a small pilot

1. Test free explorer and RPC access for Fusion and BNB Smart Chain, including historical ranges, pagination and available event types. Record reproducible queries and limits.
2. Select approximately 5â€“10 assets based on verified source-chain identity, usable history and evidence quality. Stablecoins and widely used assets are candidate categories, not confirmed holdings.
3. Retrieve public on-chain evidence independently for any candidate suggested privately. Keep the lead itself outside published records. Do not label a candidate as Chainge-controlled without separate support.
4. Trace historical deposits and withdrawals, token mint/burn events, deployments, administrative changes and transfers. Inspect contract code, proxy implementations and historical configuration where available.
5. For each proposed cross-chain relationship, record the evidence and alternatives: unrelated transfers, shared infrastructure, routers, pools or other custodians. Use timing and amount correlations only with corroboration.
6. Establish opening observations and follow candidate balances and movements through relevant timeline milestones. Follow counterparties only as evidence warrants, documenting where attribution stops.
7. Reconcile observed movements against balance changes where feasible. Include native assets and internal transfers when accessible; disclose missing trace coverage, token-specific behavior and unobserved chains.

Completion: each pilot asset meets the required outstanding-token/vault reconciliation above, with reproducible ledgers and a method applicable to further assets. Publish partial case studies as progress, retaining incomplete status for unresolved assets. Do not force a vault attribution to meet a numerical target.

## Phase 4: Build a public registry and static site

The first site should provide:

- A neutral overview, methodology, publication policy and sourced timeline.
- Searchable token and chain-qualified address records, including proposed roles, validity periods and evidence status.
- Per-asset outstanding-token/vault ledgers, individual vault balances, explicit adjustments, matched cutoffs, comparison totals and completeness status.
- Transaction histories with explorer links, timestamps, raw amounts and correctly interpreted decimals.
- Evidence references and a clear separation between official statements, chain observations and research interpretations.
- Per-chain indexed block ranges, last successful refresh, missing history and collection failures.
- Downloadable public data and correction history.

Use GitHub Pages as the initial hosting target, subject to verifying current free hosting and workflow limits during implementation. Generate the site from reviewed public records. Keep private archives entirely outside the build. Start with a small static site; a continuously running backend is not required.

Follow the existing global C#/.NET and layered-solution instructions when implementing the collector and generator. Choose exact packages and project names at implementation time. Do not scaffold speculative infrastructure during the planning phase.

## Phase 5: Repeatable refresh and wider research

- Begin with a manual collector command. Add a weekly scheduled run after the pilot demonstrates reliable collection within free limits; support manual runs between schedules. Increase frequency only if practical and useful.
- Use bounded requests, explicit pagination, retries, backoff and resumable per-chain checkpoints. An interrupted or rate-limited query must not be reported as complete.
- Store block hashes and chain-specific finality rules; recheck an overlap window and reconcile reorganizations. Deduplicate by chain and the appropriate transaction/event identity.
- Preserve source observations. Derive summaries and balances where possible; document any justified persistence of derived snapshots. Use integer base-unit amounts rather than floating-point arithmetic.
- Publish new observations automatically only after validation. Changes to address attribution require evidence review; a refresh must not silently promote a hypothesis to an established role.
- Report collection coverage separately from observed activity. No retrieved events is not evidence of no movements when collection is incomplete.
- Add community contribution guidance that directs personal records to an agreed private intake outside the repo. Public issues should contain independently sourced public evidence, not copied submissions or personal histories.
- Expand to additional assets and chains using the pilot method. Record disproved candidates and unresolved questions as useful results.

Completion: a reproducible public build and refresh process, demonstrated recovery from partial failures, visible freshness information and a documented contribution process.

## Logical records

Define exact schemas after inspecting the first real sources:

| Record | Required information |
| --- | --- |
| Source capture | Source URL, retrieval UTC, method, content hash, coverage, public/private handling |
| Asset | Chain and native/contract identifier, observed metadata, decimals and evidence |
| Address | Chain and address; roles held in separate evidence-backed assertions |
| Relationship assertion | Related records, proposed relationship, evidence, status, validity period, alternatives |
| Chain event | Chain, block number/hash, transaction hash, event/trace index when applicable, time, asset, raw amount, participants |
| Reconciliation cutoff | Common UTC cutoff; per-chain block number, hash, timestamp, lag and coverage |
| Representation adjustment | Gross position, proposed adjustment, reason, evidence, status and effect on outstanding total |
| Vault position | Chain-qualified token and address, role evidence, opening balance, movements, closing balance and cutoff |
| Asset reconciliation | Representation and vault inventories, gross and adjusted outstanding amounts, eligible holdings, comparison, evidence and unresolved gaps |
| Timeline entry | Event time or interval, publication time, evidence type, supported description |
| Collection run | Tool version, source parameters excluding secrets, range, completion status, gaps and checkpoint |

No public record should contain a private contributor identifier or a pointer that reveals private source material.

## Initial public source inventory

These are discovery sources, not endorsements or verified statements about current operations:

- [Fusionite](https://www.fusionite.info/)
- [Linked asset spreadsheet](https://docs.google.com/spreadsheets/d/1pyQlI4oGOgaRx-5UWA68meb85KtsnZ98BZrYDrHsqhk/edit?gid=1161132641#gid=1161132641)
- [Chainge 2.0 API documentation](https://chainge-finance.gitbook.io/chainge-finance/get-started/chainge-2.0-api-documentation)
- [ChaingeFinance GitHub organization](https://github.com/ChaingeFinance)
- [Chainge token list](https://github.com/ChaingeFinance/TokenList)
- [Chainge token code](https://github.com/ChaingeFinance/ChaingeToken)
- [Chainge Telegram group](https://t.me/chaingefinance)

During the initial planning review, Fusionite and GitHub pages were accessible through the web tool. The spreadsheet could not be retrieved through that tool and the API documentation timed out. Subsequent preservation captured the documentation and inspected the locally supplied public workbook: 2,185 populated data rows and 2,184 distinct addresses. See [SOURCES.md](SOURCES.md) for provenance and limits; live sheet completeness remains unverified. Official X/Twitter account URLs remain to be verified.

## Immediate next steps

- [x] Agree on neutral language, evidence standards, privacy boundaries and zero budget.
- [x] Create this plan without private submissions or candidate addresses copied from them.
- [x] Export Telegram history to a private location outside the repository. Chainge JSON and Fusion HTML were supplied; the first Chainge timeline pass is documented below.
- [x] Establish local public-archive location, ignore rules and reviewed-evidence publication boundaries. Private exports remain outside the checkout; independent backup is still pending.
- [ ] Complete public-source preservation. Initial workbook, web captures and six Git histories are saved locally; see [SOURCES.md](SOURCES.md) for coverage and gaps.
- [ ] Verify official social accounts and begin the sourced timeline.
- [x] Perform initial bounded chain-access checks and a six-token contract sample. See [initial contract findings](research/initial-contract-findings.md); complete historical coverage remains unqualified.
- [x] Select BUSD as the first tracing pilot based on the initial contract and transaction evidence.
- [x] Record the factory deployment, BUSD creation event and a bounded mint-permission simulation with [reviewed evidence](research/factory-and-mint-authority.md).
- [x] Inspect listed factory administrative transactions and reconcile BUSD mint/burn event totals with retained supply; see [history findings](research/factory-history-and-busd-supply.md). Internal-call completeness and source-chain backing remain unresolved.
- [x] Examine the earliest BUSD mint's migration context and reconcile the equal-amount burn/mint addresses; see [balance-flow findings](research/busd-migration-and-balance-flow.md).
- [x] Trace the funding address to its newer-BUSD mint and verify the legacy parent/full-time-child event structure; see [legacy findings](research/legacy-busd-and-funding-origin.md). Returned legacy child ledgers have subsequently reconciled; the earlier representation still lacks a validated replay model.
- [x] Verify the legacy ownership/controller event sequence, reconcile one original address's full-time-child balance and distinguish a slice conversion from external issuance; see [slice accounting](research/legacy-authority-and-slice-accounting.md).
- [x] Preserve BUSD supporting findings with raw query evidence. The [case assessment](research/busd-case-assessment.md) records partial coverage and remaining reconciliation work; BUSD completion and public site/collector implementation are still pending.
- [x] Reconcile returned newer and legacy full-time BUSD Transfer ledgers against all observed address balances; classify all newer issuance receipts. See [ledger reconciliation](research/busd-ledger-reconciliation.md). The subsequent [slice-coverage report](research/busd-time-slice-coverage.md) reconciles all four other returned legacy children. The earlier representation still lacks a validated historical replay model; source-chain reserve attribution remains separate.

- [x] Reconcile the identified BSC, Ethereum and Avalanche BUSD controller ledgers, investigate material branches, and verify Avalanche liquidity-address configuration changes. See [source-chain flows](research/busd-source-chain-flows.md). Cross-chain message linkage, off-chain accounting and a service-disruption timeline remain unresolved.
- [ ] Complete the BUSD representation inventory and validate the outstanding-token denominator, with gross balances and proposed adjustments shown separately.
- [ ] Complete the BUSD vault inventory and asset mapping, with role evidence and explicit treatment of successor addresses, swaps and pooled holdings.
- [ ] Produce BUSD vault balance ledgers at a common cutoff with Fusion, then separately track later source-chain movements. Publish the resulting per-asset comparison with evidence and coverage status; retain incomplete status while material gaps remain.

Repository: [algorexxx/chainge-postmortem](https://github.com/algorexxx/chainge-postmortem), maintained as independent research under a personal account. Local Git history starts with the reviewed baseline. Continue committing coherent research steps locally; the maintainer handles all pushes. See [README.md](README.md) for the history and correction policy.

Open implementation decisions: site URL; external archive storage and backup availability; private intake location; actual free endpoint coverage; weekly refresh feasibility. None changes the agreed evidence and privacy policy.

## Local UI implementation (2026-10-11)

The first static registry is implemented with a layered .NET generator and browser-local navigation. It indexes 2,184 distinct source-listed Fusion addresses, with contract-specific unassessed states. The BUSD view presents newer-token issuance, incomplete backing coverage, six selected chain-qualified positions and four selected movement cases. Filters distinguish unmatched outflows from possible redemption and rebalancing. Exact amounts and cited reports remain available; no automatic burn matcher, consolidated reserve ratio or exhaustive outflow ranking is claimed. See [build instructions and data boundaries](registry/README.md).

Publication, automated refresh, completed reconciliations and extension to additional researched assets remain pending. The maintainer continues to handle remote pushes.

## Asset-by-asset reconciliation pipeline

Publish one asset at a time after reviewing its stated coverage. The offline pipeline now normalizes retained evidence, preserves provenance, proposes exact holder/amount/time matches, excludes conversion candidates and prioritizes large unresolved outflows. See `reconciliation/README.md` for the first BUSD run and remaining limitations. Candidate pairs are not automatically promoted to reviewed findings. UI movement cards represent individual reviewed cases; possible redemptions are hidden by default. Next: review competing links and largest unresolved outflows, expand earlier-token/slice and custody coverage, and validate claims versus backing at common cutoffs before calling an asset reconciliation complete.


## Fusion purpose classification and delayed candidates (2026-10-11)

Receipt and call data now cover all 5,249 retained Fusion issuance transactions. The reusable classifier separates slice conversions, cross-representation mint/burn pairs, inventory-transfer hypotheses, direct issuance calls and unresolved purposes before external matching. A distinct 24-hour review exports additional candidates without silently promoting them. See [the fifth reconciliation checkpoint](reconciliation/README.md#fifth-pass-fusion-purposes-and-delayed-review). Remaining work includes reviewing the 1,434 unpaired mints and 447 burns against broader custody and routing evidence; those event counts are not outstanding supply or missing backing.


## Payout discovery and inventory review (2026-10-11)

The offline pipeline now searches unpaired holder burns for payments from previously unconfigured source addresses and preserves explorer-only leads separately from the ledger. The first 148 Ethereum/Avalanche lookups produced no new qualifying payer; BSC bulk coverage remains a gap. A separate replay identified all 363 controller-held unpaired burns as clearing its observed full-time-child inventory to zero, leaving 84 other holder burns for ordinary payout investigation. Every inventory burn and its preceding transfer evidence remain in the unresolved accounting totals. See [the sixth checkpoint](reconciliation/README.md#sixth-pass-payout-discovery-and-controller-inventory). Next evidence needs: trace the inventory's incoming transfers to their economic purpose and settlements, extend BSC discovery, and decode recipient/routing instructions before proposing batch or changed-recipient matches.

### Automatic burn review queues

Each reconciliation run now exports `inventory-settlement-review.csv` and `unresolved-burn-review.csv`, includes their event IDs in `burnReviewQueues` in the JSON report, and prints both queue counts. Only `controller-observed-balance-clearing` cases enter the inventory settlement queue; partial, unverified, inconsistent-history, holder, and unknown classifications remain in the unresolved burn queue. `burn-inventory.csv` retains all cases and includes their review queue.

The BUSD rerun partitions 447 externally unpaired burns into 363 inventory settlement cases and 84 other unresolved burns, with no omitted or duplicated events. This reduces the generic review queue, not the external settlement gap: inventory clearing does not prove a payout or redemption. The 224 unmatched source outflows and 1,434 unpaired Fusion mints are unchanged. Both CSV partitions were checked against the JSON report; all 122 tests pass with full Application line coverage.

### Generated movements UI checkpoint

The reconciliation CLI now emits `site-movements.json`; `reconcile.ps1` refreshes the committed `registry/<asset>-movements.json` distribution artifact after success. The static build embeds the report referenced by each study. All 5,710 BUSD movements and 224 unmatched outflows are preserved, alongside 538 per-chain/token/recipient gross-outflow summaries. Reconstructed balance percentages, ranking and filters assist review without assigning intent or proving custody. The five traced cases remain separate, with transaction links from matching automated cards; delayed candidate edges remain a separate review tier. See [site report semantics and reproducibility](registry/README.md#generated-movement-ledger) for limitations and verification.

### Terminal outflow review checkpoint

The site report now includes per-address/token terminal summaries, last-ten outflow IDs, last-substantial and final-payment IDs, complete final-sequence event IDs, reconstructed opening/closing balances, replenishment and recipient-convergence leads. The default movements view uses these summaries; the complete ledger remains available. BUSD has six histories and three cross-chain recipient-address reuse groups. Thresholds reuse the existing asset policy; timing, address reuse and final balances do not establish intent or complete custody coverage. Independent replay checks and 141 tests pass with full Application line coverage. Detailed semantics are documented in `registry/README.md`.

### Script review corrections

An independent code review reproduced three defects with synthetic receipts and balance histories. Pool transfer matching now uses the first subsequent swap emitted by the same pool and restricts the other transfer leg to the interval after that pool's previous swap. Multiple fitting counterpart legs are withheld rather than attributed as multiple verified routes. This is conservative for prefunded or otherwise nonstandard multi-operation receipts; those may remain unresolved.

The movement scanner retains a per-chain/token/account history inconsistency after any negative reconstructed balance, including events outside the displayed movement set. Later recovery does not restore depletion confidence: the `inconsistent-balance-history` flag persists, the large-balance-reduction flag is suppressed, and the site percentage remains unavailable. Existing negative-balance flags also suppress percentages. Raw before/after reconstructions remain available for audit.

Explicit exchange evidence now takes precedence over the generic counter-asset heuristic when no Fusion candidate exists. Internal transfers, missing timestamps and other existing classification gates remain unchanged. The BUSD rerun relabels 1,582 movements from `possible-asset-exchange` to `reported-asset-exchange`; all 5,710 movement IDs, 224 unmatched outflows, retained amounts, and terminal balance summaries remain unchanged. Neither the pool-boundary correction nor the persistent-history correction changes BUSD exchange-evidence counts or depletion percentages in this run. These are code corrections, not new transaction findings.

Regression coverage includes incoming/outgoing repeated same-pool swaps, reversed receipt-swap order, competing counterpart legs, no later swap, recovered negative histories, independent account histories, and explicit swap evidence alongside the generic heuristic. All 152 tests pass; Application line coverage is 458/458.


## Communications timeline checkpoint (2026-10-13)

A first founder-statement chronology is published in `registry/timeline.json` and rendered in the project-wide Timeline view. Nineteen milestones reference 29 selected messages, with UTC publication/edit times, exact reviewed excerpts, conditional target dates and event-specific limitations. Raw Telegram exports remain outside the repository. Ordinary-member messages and unverified staff attributions are not published. The JSON export's hash, retained range and attribution method are recorded; export completion time and independent backup remain unknown.

The March 2025 blacklisting explanation remains an attributed claim without identified issuers, restricted addresses or restriction events. The March AWS/domain interruption is separate from withdrawal settlement; the May suspension is an explicit statement but not an established first-disruption date. Earlier routes and the BUSD outflows require separate chronology. See [timeline method and next evidence needs](research/service-timeline.md).


## Closing review and repeatable workflow (2026-10-15)

The [current assessment](research/closeout-assessment.md) supersedes earlier next-step lists for BUSD/USDC. The closing pass extends public histories, reviews all selected material late outflows plus older and post-snapshot exceptions, and consolidates explicit accounting treatments without inventing customer-liability totals. Remaining rights, custody and public-data boundaries stay visible.

[CONTRIBUTING.md](CONTRIBUTING.md) is the ordered procedure for people and LLMs investigating the next token. Asset packages use the updated template, a common configuration and UI, and an event-ID-keyed closing decision register. The reusable gate regenerates its selection when evidence changes and fails verification for missing assessments. A reviewed case can still have unresolved purpose; a finished bounded pass does not certify a completed reconciliation.
