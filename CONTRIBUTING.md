# Investigating one asset

This procedure is shared by people and LLMs. Start with one asset and a bounded, stated scope. Public evidence additions are welcome before the full investigation is complete. Never describe a successful script run as proof of custody, settlement or adequate backing.

## 1. Open an investigation package

Fork the repository and create a branch such as `research/usdc-ethereum`. Copy [the package template](research/assets/TEMPLATE.md) to `research/assets/<asset>.md`. Use a lowercase asset slug. See [BUSD](research/assets/busd.md) for the reference package and [USDC](research/assets/usdc.md) for a partial investigation with explicit acquisition and interpretation gaps.

Record exact chain IDs, contract addresses, representation relationships, observed decimals and supporting evidence. A symbol or identical address on another chain is not proof of asset equivalence or common control. Start from the Fusion catalogue, verify deployed code/getters and discover predecessors, parent/child tokens, time slices and authority changes before adding their supplies together.

## 2. Preserve public evidence and declare coverage

Save reviewed public inputs in `evidence/<date>-<asset>-<purpose>/`. Retain requests, responses (including errors), retrieval UTC, public source URLs, chain IDs, block numbers/hashes/timestamps and pagination/range boundaries. Private RPC addresses and credentials must be omitted from public metadata; describe the method and access limitation instead. Never query a credential found in third-party material.

For each token/address history, record the requested and actually returned range, page size, terminal-page check, receipt/header corroboration and gaps. An empty or failed explorer response does not establish no activity. Save successful pinned `balanceOf` reads separately from transfer-history totals. Use one common UTC cutoff with chain-specific blocks at or before it; record their actual times and finality assumptions. If historical state is unavailable, keep that gap explicit.

Add a `manifest.json` containing `{"algorithm":"SHA-256","files":{"response.json":"<saved-byte SHA-256>"}}`. Existing array manifests with `file` and `sha256` are also supported. Entries resolve relative to the manifest directory. Hash request/capture companions too; the pipeline enforces hashes for selected inputs, not every unselected attachment. Hashes prove byte integrity, not authenticity. Keep original evidence immutable; document sanitized derivatives and corrections.

Do not commit raw Telegram exports, private messages, community submissions, identifying community quotes, screenshots, API keys, wallet secrets or local infrastructure details—even under an ignored directory. Keep private working material outside the checkout. Public official/staff statements can be attributed; ordinary community context must be anonymized. Inspect URLs, binary metadata and the staged diff before committing. See [archive policy](ARCHIVING.md).

## 3. Configure and run the common pipeline

Install the .NET SDK selected in `global.json` and PowerShell 7. Add `reconciliation/<asset>.json`, using [the USDC configuration](reconciliation/usdc.json) for its structure. Replace all asset-specific inputs and hypotheses; do not copy BUSD fees, operating addresses, migration mappings or cutoffs as facts.

The configuration declares `asset`, `policy`, `sources`, optional `discoverySources`, and `limitations`. Each source declares `chain`, `directory`, filename `pattern` and a contract-to-decimals `tokenDecimals` map. Select response files, not arbitrary archives. The adapter supports EVM ERC-20/Fusion timed Transfer logs, receipts, transaction calls and block headers. Native assets, UTXO chains, rebasing/fee-on-transfer semantics and different event schemas require explicit support rather than fabricated ERC-20 rows.

The minimum policy supplies the Fusion chain label, candidate operating addresses by chain, amount precision, a large-outflow threshold, depletion basis points, time/clock windows and explicit fee hypotheses. Optional rules are off unless configured. Explain every enabled rule in the package with evidence, its uncertainty and expected failure cases. Thresholds prioritize review; they must never delete small or late events.

```powershell
./reconcile.ps1 -Asset usdc
```

Outputs go to `archives/reconciliation/usdc/`; this command does not modify the site registry. The pipeline deduplicates events, checks selected-input manifest hashes and rejects missing/mismatched entries. Review `accounting-summary.json` first: it separates token/chain/interval totals, movement classifications, address inflows/outflows, provenance and diagnostics. Then inspect the full `reconciliation.json` and CSV review queues. Discovery observations remain separate from verified event accounting.

## 4. Resolve exceptions without hiding them

Review closing outflows, large unmatched payments and their downstream destinations first, but retain the full ledger. Check missing timestamps, receipt failures, incomplete histories and negative reconstructed balances before interpreting unmatched counts. Inspect competing candidates, conversions, possible exchanges, inventory clearing and internal transfers as well as the generic unmatched queue.

A candidate requires the actual event IDs, holder/recipient, exact amounts, timing and receipt context. An amount/time match is a hypothesis; exchange entry does not identify an account or a later withdrawal. Document accepted interpretations and rejected candidates in the assessment with source links. The current engine does not consume manual decisions as settled accounting: do not overwrite its classifications to make a report appear resolved.

For each representation, establish gross issuance, destruction, holder balances and adjustments under its actual contract semantics. Separate time slices and migrations to avoid counting one claim twice. For each verified vault, reconcile opening balance + receipts - payments + explicit adjustments = closing balance at the stated cutoff. Event net flow is not a closing balance unless the opening state and full coverage are proven.

## 5. Verify and optionally prepare the site

```powershell
./verify.ps1
```

This builds, runs tests, executes every configured asset, verifies selected-input integrity, checks committed movement reports against regeneration, and builds the static site. It makes no chain calls. It does not certify custody, complete history, secrets absence or asset reconciliation. GitHub runs the same checks on PRs and before Pages deployment.

After reviewing a proposed site report, explicitly promote it:

```powershell
./reconcile.ps1 -Asset usdc -UpdateRegistry
./verify.ps1
```

This creates `registry/usdc-movements.json`; it does not add a study or publish anything remotely. For site inclusion, add a study keyed by the Fusion contract in `registry/studies.json` with `title`, `reviewed` (date), `status`, `scope`, `assessment` (public report URL), `vaults`, `flows`, and optional `movementsFile`. Empty arrays and unknown balances (`null`) are valid; invented zeros are not. Generic studies get a scope/assessment overview. Do not set BUSD's `presentation: "busd-pilot"`. Preview every tab and follow the explorer/evidence links before including a study. Partial publication must say exactly what remains unknown.

## 6. Submit a pull request

Stage named evidence, configuration, assessment and reviewed site files; exclude `archives/`, `dist/`, build products and private material. Review `git diff --cached --stat` and `git diff --cached`, then commit. Push your research branch to your fork and open a PR against `algorexxx/chainge-postmortem:main`. Use the [PR template](.github/PULL_REQUEST_TEMPLATE.md): state scope, evidence, coverage gaps, rule changes and verification results. One asset or coherent correction per PR. Do not rewrite project history or force-push `main` as part of ordinary contribution work.

A **reproducible partial investigation** has traceable public inputs, stated gaps, a passing offline run and cautious conclusions. A **completed reconciliation** additionally requires validated consolidated Fusion claims, a justified complete vault inventory, independently observed balances at a common cutoff, reconciled histories and every material exclusion/adjustment accounted for. Neither status is awarded automatically by CI. Maintainers decide acceptance; LLMs must report unavailable evidence and stop short of unsupported conclusions rather than silently relaxing the standard.


Routing review is optional per asset: configure `routingCallTargets`, `routingChainNames` and `routingReviewMaximumDelay` only with documented evidence. Inspect generated `routing-review.json` for decoded and unsupported calls. The current decoder covers the observed `0x3f163c47` colon-memo layout and requires receipt/call/burn agreement. It does not authenticate the ABI, memo fee semantics or settlement. Exact destination candidates remain separate from normal matching; missing destination-chain history is unknown, not a failed payout. Keep unsupported JSON and address formats visible until supported by tested decoders.
