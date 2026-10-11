# Asset reconciliation pipeline

Run `./reconcile.ps1 -Asset busd` from PowerShell. It uses the existing .NET 10 SDK and reads retained public evidence without making network calls. Results go to ignored `archives/reconciliation/busd/`. The static site remains a separate publication step; running this command does not publish proposals, update reviewed case cards, or push Git.

## Structure for each asset

1. **Asset configuration** (`busd.json`): exact chain/token contracts and decimals, candidate operating addresses per chain, evidence file selections, fee/time hypotheses, review thresholds and known coverage limitations. Add a new `<asset>.json` and run the same command with `-Asset <asset>`; no matching code changes are needed for supported EVM token evidence.
2. **Evidence ingestion** (`Persistence.Json`): read successful RPC receipts and explorer Transfer logs; recognize standard ERC-20 and Fusion time-aware Transfer events. Deduplicate by chain/transaction/log index, reject conflicting duplicates and removed logs, preserve file hashes and receipt-verification status. Timed events retain whether they represent the full-time interval. Source contract identity is allowlisted; symbols are never identity keys.
3. **Candidate matching** (`Application`): index Fusion issuance/destruction by holder and compare source-chain transfers against that holder, amount and time. All arithmetic is integer; different token decimal precisions are normalized exactly. Same-transaction equal-amount cross-representation transfers involving the holder are excluded as conversion candidates. Dated slices are excluded from external matching. Address-one transfers are not burns.
4. **Review prioritization** (`Application`): retain incoming/outgoing/internal direction, missing timestamps, zero-value events, unmatched events and every competing candidate. Flag large unmatched outflows and large reductions in reconstructed operating-address balances. These are review priorities, not findings of misconduct.
5. **Review output and publication**: use the generated queue to investigate exceptions, corroborate uniquely proposed pairs and record reviewed interpretations in the asset research and case registry. Keep unreviewed proposals distinct from reviewed findings. Release an asset only when its stated publication scope is ready.

## Outputs

- `reconciliation.json`: normalized events, evidence references, input SHA-256 hashes, exact policy and configuration hash, provider/timestamp diagnostics, every movement classification and candidate link, excluded conversion event IDs, and unpaired full-time Fusion issuance/burn events.
- `movements.csv`: one row per deduplicated event involving a configured operating address, including direction, exact amount/raw units, candidate count, flags and reconstructed balance before/after. Filter **outgoing + unmatched** to review unexplained payments; incoming unmatched events are a separate queue.
- `candidate-pairs.csv`: one row per proposed source/Fusion link. Many-to-many candidates remain separate rows marked competing; never sum the pair table to calculate payments. A movement is counted once in the movement table regardless of its candidate count.

Outputs are disposable views of retained evidence and configuration, not a second source of truth. They stay outside Git to avoid committing redundant ledgers on every run. Amounts in CSV are exact text: spreadsheet software may round them when automatically converting to numbers; the JSON/raw-unit fields preserve the authoritative generated values. Existing evidence files are never modified.

## BUSD policy and initial run

The first policy searches a delay of -120 seconds through 24 hours, matching deposits to equal-amount mints and payments to burns with a difference of 0 or 0.5 BUSD. The negative tolerance accommodates cross-chain clock differences. Neither the time window nor the 0.5 difference is a verified bridge rule. Candidate results are sensitive to these inputs; changing them requires review, not silent widening until a match appears.

Triage thresholds are 10,000 BUSD for a large unmatched outflow and 90% for a reconstructed balance reduction. A legitimate redemption can trigger the second flag. An unmatched transfer below either threshold remains in the output. Balance reconstruction starts at zero before the first retained event; it does not establish coverage or a current reserve balance. Cross-chain totals must not be treated as unique funds or losses.

The retained-data run contains 47,808 token events, of which 5,710 involve the two configured operating addresses. It proposes 577 redemption candidates and 1,784 deposit candidates; 172 source movements have competing links. There are 1,964 unmatched outgoing movements and 1,230 unmatched incoming movements under this policy, 14 internal movements and 141 zero-value events. The scanner flags 187 large unmatched outflows and 13 large balance reductions. These counts are a reproducible research checkpoint, not accepted matches or loss estimates.

The existing January 1 and January 11, 2022 redemption cases are recovered with their respective 9h55m28s and 29-second gaps and 0.5 differences. The December 2021 1.1M and 100k payments remain unmatched. Legacy full-time and newer-token event counts reproduce 35,552 and 1,756; the earlier time-aware representation contributes 1,385 retained events and is not asserted to be a complete claim ledger.

The source selection also includes counterparty receipts; these improve evidence context but only configured operating-address events enter the movement queue. There are 62 ingestion diagnostics in the initial run, including retained failed responses/retries and conflicting explorer timestamps. Receipt/header timestamps take priority over explorer timestamps. Conflicts at equal priority quarantine the block timestamp and prevent time-based matching. A failed response is not evidence of no activity.

## Supported limits and next work

This is an offline reconciliation and triage pipeline, not a live collector or proof of custody. It cannot resolve exchange-internal balances, detect undiscovered vaults, automatically prove a cross-chain rebalance, infer beneficiary ownership, or match split/batched payouts and changed recipient addresses. Those remain review exceptions. Matching a burn to a payment does not validate the backing of all other claims.

For the next asset, preserve its public evidence, identify contract versions and operating-address roles, record token decimals and explicit coverage limits, then supply the configuration. The existing adapter accepts RPC JSON receipt batches and Etherscan-style event-log results. A different event schema requires an adapter; do not coerce it into an ERC-20 transfer silently.

Reviewed interpretations remain in research and `registry/studies.json`, referenced by transaction hashes. The pipeline currently emits proposed edges only; it does not silently consume narrative case labels as accepted accounting decisions. A future review-decision store should reference event IDs, supporting evidence and reasons without duplicating amounts, and validate those IDs against each refreshed run.

Validation covers exact fees, mixed decimals, clock/window boundaries, competing candidates in both directions, migration/slice exclusion, balance-reduction flags, duplicate evidence integrity, failed receipts, timestamp prioritization and conflict quarantine. Application line coverage is measured with the repository test command. No publication workflow or live refresh is enabled.
