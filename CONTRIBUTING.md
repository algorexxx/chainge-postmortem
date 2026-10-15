# Investigating one token

This is the shared procedure for contributors and LLMs. Work on one asset at a time, using anonymous free sources. Preserve the full ledger, publish cautious conclusions, and distinguish **a reproducible partial investigation** from **a completed reconciliation**. Passing CI does not establish custody, settlement or customer liabilities.

## 1. Define identity, scope and the cutoff

Fork the repository, create a research branch and copy [the asset template](research/assets/TEMPLATE.md) to `research/assets/<asset>.md`. Use a lowercase slug. Record the exact chain ID, contract, observed decimals, deployed code and representation relationship for every token. Equal symbols, equal addresses on different chains and similar amounts do not establish equivalence.

Inventory Fusion predecessors, child tokens, dated rights, migrations, controllers and mint/burn methods. Inventory candidate **vault addresses** separately on each source chain, with evidence for their role and active period. A downstream recipient is not a vault merely because it received a payment. Keep unverified candidates and alternative explanations visible.

Screen the full retained token catalogue for predecessor and dated representations, and test known vault candidates on each plausible source chain. An issuer FAQ or market-data chain list is a discovery lead, not an exhaustive bridge inventory; BAT screening missed BSC and older Fusion representations until this wider check.

Choose a common UTC comparison time and chain-specific blocks at or before it. Preserve their hashes and adjacent headers. Record the provenance and limits of the Fusion snapshot: the highest known recovered backup is not independently proven network finality. Later source-chain payments cannot be judged against missing later Fusion burns.

## 2. Preserve evidence and configure acquisition

Use `evidence/<date>-<asset>-<purpose>/` for independently retrieved public evidence. Preserve exact requests, response bytes, failures, retrieval UTC, public URLs, chain identity, numeric blocks, pagination cursors and query filters. Add a scope README and a SHA-256 manifest covering every retained file except the manifest itself. Original evidence is immutable; corrections are new captures. Hashes establish byte integrity, not source truth.

Keep private submissions, Telegram exports, community identities, screenshots supplied by users, keys and local infrastructure details **outside the checkout**. Public official/staff statements may be attributed; community context must be anonymized. Never query credentials discovered in someone else's material. See [ARCHIVING.md](ARCHIVING.md).

Create `reconciliation/<asset>.json` using the [configuration reference](reconciliation/README.md), with these explicit inputs:

| Input | Required decision |
| --- | --- |
| `sources`, `discoverySources` | Select reviewed response files and contract-to-decimals maps; distinguish discovery from receipt evidence. |
| `policy.operatingAddresses` | Candidate vaults by chain, justified individually. |
| Matching and terminal policies | Document time, fee, amount and depletion hypotheses; never inherit another asset's fees as facts. |
| `logQuerySources`, `logHistoryTargets` | Required numeric inclusive block ranges, each token/holder and both directions. |
| `balanceSources`, `sourceCutoffBlocks` | Pinned state calls and aligned source-chain blocks. |
| Fusion cutoff, `claimIntervals`, `datedTokens` | Snapshot and representation/time-right semantics. |
| `capturePolicies` | Anonymous endpoint, expected chain ID, block span, batch size, delay, timeout and failure limit. |

The adapter supports the documented EVM/Fusion event formats. Native assets, UTXO chains, rebasing or fee-on-transfer semantics need explicit support; do not manufacture ERC-20 events.

## 3. Acquire, resume and corroborate

Install the .NET SDK from `global.json`, PowerShell 7 and Node.js 22. Planning and reconciliation are offline; capture makes read-only network requests.

**Acquisition order is mandatory for people and LLMs:** reuse preserved evidence; query token-filtered incoming/outgoing vault histories from an index; check pagination and corroborate discovered transactions with exact receipts, headers and pinned balances. Try another anonymous index or a reviewed public explorer export if the first source fails. Follow the [indexed-history procedure](research/indexed-history-workflow.md#before-any-broad-scan).

Broad block scans are a last resort for a named remaining gap, not the default response to rate limits or missing RPC coverage. Before launching one, record request count, measured throughput, expected elapsed time including cooldowns, alternatives tried and the specific evidence it would add. Do not launch or automatically resume a scan estimated at 24 hours or longer without an explicit user-approved exception for that plan. A general instruction to finish an investigation or automate retries is not that exception. Reassess if measured performance crosses this limit; splitting the same scan into chunks or scheduling a heartbeat does not change the rule.

The commands below generate and execute fallback plans; generating a plan does not authorize running it without the acquisition review above.

```powershell
./reconcile.ps1 -Asset usdc
./capture-plan.ps1 -Asset usdc
./capture.ps1 -Plan archives/capture-plans/usdc/ethereum.json -Output evidence/YYYY-MM-DD-usdc-ethereum
```

Inspect plans before executing. Anonymous providers have different limits for logs, receipts, headers, OR-holder filters and batch sizes. Save failures, reduce batch/range size or increase delay, and resume the **same plan and output directory**. Successful hash-checked pinned requests are reused; dynamic tags are not. Do not overwrite attempts or edit the meaning of existing batches. Use a new package for a materially different plan. An interrupted capture needs a successful resume to finalize its manifest before selection.

Save reconciliation configuration files as UTF-8 with LF line endings before generating reports. Their hashes refer to exact bytes; Git normalizes these configuration files to LF, so generating from CRLF working files can produce stale provenance after a Linux checkout. Preserve raw evidence bytes under the existing evidence attributes.

After capture, add its README, update its manifest and explicitly select reviewed inputs. Reconcile and generate a **second plan**: newly found transfers introduce receipts, transaction inputs and headers absent from the first plan. The planner requests transaction inputs even when a receipt exists, and a retained explorer timestamp does not replace a hash-bound block header. A provider may serve old headers but return null receipts; null and errors are unavailable evidence, never zero activity.

For explorer acquisition, save all pages and the terminal cursor. Explorer pagination is not RPC range coverage. Get successful receipts and block-hash-bound headers for material events. Validate exact chain, emitting contract, from/to, raw integer amount and log index; ignore zero-value lookalike transfers when following value, without deleting them from the ledger.

For Fusion, start with the [public FSNEX workflow](research/fsnex-public-workflow.md) and `research/capture-fsnex-transactions.ps1`. Select complete transaction details with `format: "fsnex-transaction"`; the importer preserves public receipt provenance and checks conflicts. Private RPC is not required for indexed logs, receipt context or calldata. Historical state and unavailable traces remain separate requirements.

## 4. Check history and accounting before interpreting matches

An instruction to investigate an asset authorizes this entire workflow. Continue through every stage, repair reusable tooling when a stage fails, and rerun downstream stages after adding evidence. Do not stop merely because a shortlist is reviewed, a page renders, or an unmatched count has fallen. Ask only for a genuinely missing decision or access that cannot be obtained within the authorized scope.

Keep `research/assets/<asset>-workflow.json` alongside the case register. Run `./research/review-workflow.ps1 -Asset <asset>` to see unfinished stages, missing pinned Fusion positions, history gaps and observed routing targets omitted from configuration. Before describing the bounded investigation work as finished, run it with `-RequireFinished`. Each required stage must be `verified` or an explicit `evidence-boundary`, with retained references, methods actually attempted, a reason for the boundary and a reopening condition. `pending` means work still to do. An evidence boundary is not a successful reconciliation. Time-consuming work and an untried existing tool are not evidence boundaries.

For Fusion, enumerate every known representation and verify the parent/child relationships and deployed code before setting `sliceCounterparts`. An absent mapping silently leaves genuine slice conversions in the burn queue. Review observed controller targets and chain codes before setting `routingCallTargets` and `routingChainNames`; inspect `routing-review.json` even when amount matching found nothing. A memo can specify a different delivered amount or asset. Do not infer a fee from the difference.

Capture all retained Fusion transactions when the bounded dataset is manageable: `./research/capture-fsnex-transactions.ps1 -Asset <asset> -OutputDirectory <evidence-directory> -IncludeAllTransfers`. The default captures mint/burn transactions; the switch also captures ordinary transfers needed for holder funding and inventory interpretation. Both the holder-funding and controller-link reviewers accept native public FSNEX transaction details. Preserve public block headers separately and verify pinned balances and restrictions through an available historical-state source. Public receipt access does not imply public historical-state access.

Inspect these generated files in `archives/reconciliation/<asset>/`:

| Report | Question |
| --- | --- |
| `accounting-summary.json`, `reconciliation.json` | What events, classifications, diagnostics and provenance were retained? |
| `history-coverage.json` | Which requested intervals lack successful unrestricted incoming/outgoing logs? |
| `balance-checks.json` | Does retained replay agree with independently pinned state? |
| `balance-intervals.json` | Does the change between consecutive states equal intervening transfers? |
| `claim-positions.json`, `source-positions-<chain>.json` | Which representations, dated rights, vaults and cutoffs are actually covered? |
| `routing-review.json` | Which supported memos identify destination candidates, and which calls remain unsupported? |

Resolve missing timestamps, failed receipts, negative reconstructed balances and state disagreements first. Historical balance bisection can locate a discrepancy; then fetch the boundary logs, receipt and before/after state. Bisection alone does not prove the first change or complete history because balances need not change monotonically. Equal endpoint balances can conceal cancelling omissions.

For each vault, establish opening balance + receipts - payments + explicit adjustments = closing balance. Keep gross movements, internal transfers and amounts below review thresholds. Neither event net flow nor an absent row establishes a zero balance. Wrapped-token balances require separate backing evidence and must not be counted twice with their underlying reserves.

For Fusion, keep standard supply, address-one balances, blocked balances, dated rights and vault inventory separate. A block flag does not burn a token. Equal legacy/newer amounts support a migration hypothesis but do not prove extinguished rights. A holder being a vault does not establish whether its tokens are customer property, inventory or overlapping claims. Use [holder funding](research/review-holder-funding.ps1), [controller links](research/review-controller-links.ps1) and the [accounting treatment review](research/fusion-claims-review.md) where their documented formats apply. Do not sum every representation into customer liabilities or subtract balances without evidence.

Before broad RPC scans, use the [indexed vault-history workflow](research/indexed-history-workflow.md): token-filtered address history, pagination checks, then exact receipts and pinned balances. Estimate the cost of any remaining range scan before launching it. For anonymous sources with tight quotas, use the [paced capture procedure](research/paced-capture.md): singleton requests, provider-aware automatic retries and resumable immutable captures. A rate-limit error is not an empty history. Keep incomplete runs private until sealed and reviewed.

## 5. Review material outflows and their endpoints

Prioritize all unmatched outflows above the configured threshold in the stated terminal window for **each chain/token/vault**, plus older material cases and small post-announcement exceptions. Do not limit review to the largest two. Missing dates retain material cases for review. Check the rest of the ledger and competing candidates; a small unmatched count is not the objective.

For each selected case, record the original event ID, observation, competing explanations, evidence, unresolved question and condition that would reopen it. Compare successful Fusion calls/receipts, exact holder/recipient, amounts, chronology and authenticated routing fields. A close amount/time match, assumed fee or decoded memo is a candidate, not accepted settlement. Do not widen rules merely to clear the queue. The current engine does not consume manual assessments as settled accounting.

Follow value through receipts, stopping at a documented evidence boundary:

| Endpoint | Defensible conclusion / next evidence needed |
| --- | --- |
| Retained token balance | Pin `balanceOf`; state block/time and ownership unknown. No later returned transfer is not proof of continued possession forever. |
| Labelled exchange | Preserve the label and deposit path. Public chain data usually cannot identify the credited account or an internal conversion/withdrawal. |
| Bridge | Bind source/destination protocol messages and token logs. For CCTP verify domains, nonce, sender and the complete message, not amount/time alone. |
| Swap or different token | Verify actual returned token/recipient. This explains disposition, not equivalent backing or reimbursement. |
| Mixed balance | Record pre-existing and additional funds; do not attribute every onward payment to the original outflow. |
| Unlabelled recipient / inaccessible history | State the observed boundary and the specific missing evidence. Do not invent ownership or report an unavailable balance as zero. |

Use `research/review-address-history.ps1` for its documented paired incoming/outgoing/balance RPC captures. Follow destination recipients, not the entire unrelated history of a shared protocol. Keep official announcements as dated attributed statements; chronology does not establish purpose, and general announcements may not identify an asset-specific onset.

Create `research/assets/<asset>-closeout.json` with `scope`, positive `windowDays`, optional `extraEventIds`, and `decisions`. Each decision has `eventId`, `finding`, `limitation`, and repository-relative evidence `references`. Store conclusions here, not copies of generated transaction rows. Run:

```powershell
./research/review-closeout.ps1 -Asset usdc -RequireReviewed
```

This derives the material shortlist from the current report, writes an ignored report with its input hash and fails if any selected case lacks an assessment. **Reviewed does not mean resolved.** New history can change the selection; rerun after every evidence update. A documented lack of evidence is an honest result, but not proof the investigation is complete.

## 6. Publish the assessment and common UI

Update the asset package and its assessment: established observations, plausible interpretations, unresolved gaps, attempted methods, cutoff and reopening conditions. A bounded closing pass may be finished while the reconciliation remains partial. To call reconciliation complete requires justified full vault discovery, validated consolidated Fusion claims, comparable redeemable assets, aligned independently observed balances, reconciled histories and accounted material adjustments/exceptions. State which condition is unmet.

Use [the standard study format](registry/STUDY-FORMAT.md) and [study template](registry/study-template.json); all assets share one UI. Show Fusion supply versus included vault balances, with unverified backing separately. Do not present the arithmetic difference as proven loss or customer liabilities. Only original likely-vault outflows get cards; put downstream payments, exchange deposits and bridge legs inside that case's `steps`/`branches`. Link exact addresses and transaction hashes to the relevant explorer. Connect `movementsFile` so every retained movement remains accessible behind the default filters.

```powershell
./reconcile.ps1 -Asset usdc -UpdateRegistry
./verify.ps1
```

For an existing published Fusion snapshot whose statistics and holder positions are unchanged, run `./registry/refresh-fusion-provenance.ps1 -Asset usdc` after reconciliation and before verification. It checks the current configuration, known token inventory, exact mint/burn statistics and pinned holder positions before changing only the provenance hash. Changed statistics must be reviewed into the snapshot; do not bypass the guard.

Promote only after reviewing the regenerated output. Verification builds, tests, checks selected evidence and snapshots, regenerates reports, enforces report freshness and builds/checks the common site. It makes no chain calls and does not certify custody, privacy or completeness. Preview every changed tab and verify links where a browser is available.

## 7. Review privacy and submit

Stage named evidence, scripts, configuration, assessment and site files. Exclude private material, `archives/`, `dist/` and build products. Inspect staged diffs, URLs, metadata and filenames for secrets and identifying community material; ignore rules alone are insufficient. Record actual verification results and remaining gaps.

Commit, push your branch to your fork and open a PR against `algorexxx/chainge-postmortem:main`, following [the PR template](.github/PULL_REQUEST_TEMPLATE.md). One asset or coherent correction per PR. Do not rewrite history or force-push main during ordinary contributions. Maintainers decide acceptance; LLMs must report unavailable evidence and must not lower the completion standard to finish a task.
