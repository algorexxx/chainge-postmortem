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

## Second pass: variable fees and receipt context

The fixed 0/0.5-fee model missed repeated same-holder burns followed seconds later by payments with small variable differences. Version 2 retains that original pass and adds a separately labelled `bounded-variable-fee` candidate rule for still-unmatched outgoing movements. It requires a burn 0–300 seconds before payment, a positive difference at most 1% of the payment and an absolute cap of 0.5 BUSD on BSC, 0.05 on Avalanche or 50 on Ethereum. Every candidate includes the exact difference, delay and rule. These empirically selected limits are hypotheses, not authenticated fee schedules; no widening of the original 24-hour window was necessary.

Example preserved pairs motivating this search include BSC payment `0xea57749a1a02c0cfd0577a9ba6a8ca99a42a3edfeaba5d056253fe436aa5452e` and Fusion burn `0x9768b8fcd48c0859dee44180998e195a7d05fafdf46e5c9e005db9a235ec87b4` (22 seconds; 0.091838544451368446 difference), and Avalanche payment `0xc0371faa3ca6407ed25b6664f516862abef5688826045caf086ac393f231a654` and burn `0xa7be21ca55d7b2f782daeb4510e2fe20dff5b359bf48cc39e5de2ed713ef6f3b` (19 seconds; 0.018862577439034158). Their event IDs and file references are retained in the generated report. Fee hypotheses are asset configuration, not token-specific code.

Successful receipts now retain other-token Transfer logs as `relatedTransfers`, without admitting those assets to the BUSD ledger. For still-unmatched BUSD movements, a net credit of another token to the paying operating address (or debit from the receiving address) creates a `possible-asset-exchange` classification. Raw counter-asset amounts, exact token contracts and log indices remain available; token prices/decimals and beneficial ownership are not inferred. The rule does not rely on explorer router labels or a counterparty allowlist. Conflicting copies of receipt context fail the run. A mere unrelated transfer elsewhere in the receipt, unverified logs or a net-zero round trip cannot satisfy the rule.

| Movement classification | First pass | Second pass |
| --- | ---: | ---: |
| Unmatched outgoing | 1,964 | 408 |
| Unmatched incoming | 1,230 | 508 |
| Possible redemptions | 577 | 1,137 |
| Possible deposits | 1,784 | 1,784 |
| Possible asset exchanges, outgoing | 0 | 996 |
| Possible asset exchanges, incoming | 0 | 722 |

All 5,710 source movement IDs and all previously proposed links are preserved. The new rules add 560 redemption candidates; 179 movements now have competing links (previously 172). The December 2021 1.1M and 100k outflows and January 2024 204,567.658… outflow remain unmatched. These are comparisons on the same retained input files, not an expansion of chain coverage.

**Asset-exchange classification does not settle the backing question.** Every such movement carries `backing-disposition-unresolved`; large outgoing exchanges also carry `large-asset-exchange-outflow`. They remain a separate review queue even though they no longer sit in the generic unmatched bucket. An alternative asset arriving in a transaction does not establish that it was retained or covers Fusion BUSD obligations. This reduction therefore improves categorization, not the established reserve position.

## Third pass: swap recipients and small-payment fees

This pass leaves **232 outgoing and 452 incoming movements unmatched**. It uses the same 47,808 event IDs and 5,710 operating-address movement IDs. All previous candidate edges remain. Acquisition of 69 successful Fusion burn receipts and their block headers corroborates existing events rather than adding movements; the four `fusion-small-payment-*.request.json` batches and response/capture sidecars are retained in `evidence/2026-10-11-busd-source-review/`. Connection details are omitted. The run has 1,206 possible redemptions, 1,784 possible deposits and 179 movements with competing links.

| Change | Outgoing | Incoming |
| --- | ---: | ---: |
| Second-pass unmatched | 408 | 508 |
| Newly classified reported asset exchanges | 107 | 56 |
| Newly proposed small-payment redemptions | 69 | 0 |
| Remaining unmatched | 232 | 452 |

Remaining outgoing counts are BSC 209, Ethereum 16 and Avalanche 7. Incoming counts are BSC 406, Ethereum 32 and Avalanche 14. None is discarded based on size. The December 2021 1.1M/100k and January 2024 204,567.658073647823395109 payments remain unmatched. These counts do not measure missing reserves.

The first new check decodes the six non-indexed words of `Swapped(address,address,address,address,uint256,uint256)` (topic `0xd6d4f5681c246c9f42c203e287975af1601f8df8035a9251f79aab5c8f09e2f8`): sender, source token, destination token, recipient, spent amount and returned amount. A receipt must be successful; the relevant participant, token and exact raw amount must match the BUSD movement. This recognizes native-coin and third-party recipient reports that do not produce a counter-token credit to the operating address. The emitter and recipient remain explicit. The classification reports what the log says; it does not independently authenticate the contract implementation or prove final native-coin delivery.

The second check recognizes the [V2 pool Swap event layout](https://github.com/Uniswap/v2-core/blob/master/contracts/interfaces/IUniswapV2Pair.sol). It requires the movement counterparty to be the emitting pool, the asset transfer and other-token transfer to precede the swap, opposite input/output slots to match both exact raw amounts, and the output recipient to match. Both unused amount slots must be zero. A different address merely transferring some token elsewhere in the receipt cannot qualify. Routed or split pool legs that fail these exact checks remain exceptions. Neither check proves a Fusion burn, bridge instruction or ownership of the output recipient.

`exchange-evidence.csv` provides the movement ID, chain, transaction, rule, emitter/log index, sender, both token identifiers, recipient and raw amounts. One movement may have more than one supporting swap record; do not sum this table as separate outflows. The same supporting records are linked in the JSON review. Existing direct-counter-asset cases can also acquire these additional details without changing their classification. All 163 newly classified movements retain `backing-disposition-unresolved`, alongside the previous exchange queue.

The additional fee hypothesis addresses payments where a small absolute fee exceeds 1% of a small principal. `absoluteFeeMaximumDelay` is opt-in per chain (zero disables it), configured to 60 seconds for BUSD. It applies only after the earlier fee rules fail and requires **both** the source payment and same-holder full-time Fusion burn to be receipt-verified. The existing absolute caps remain BSC 0.5, Avalanche 0.05 and Ethereum 50 BUSD. Positive differences and nonnegative delay are required. Its candidate rule is `short-delay-absolute-fee`, with `fee-exceeds-relative-bound` on each movement; it is a separately disclosed hypothesis, not a verified fee schedule or an accepted accounting match. All competing edges are preserved.

This is a 43.1% reduction in the generic outgoing unmatched queue (408 to 232), composed of 107 exchange classifications and 69 additional burn/payment hypotheses. Exchanges still require destination/backing investigation. Identifying transfer mechanics does not establish collateralization, and no new cases are automatically published to the site.

## Fourth pass: relay paths, source-chain pairs and net mints

The fourth checkpoint leaves **224 outgoing and 430 incoming movements unmatched**. All 47,808 input event IDs, 5,710 operating-address movement IDs and previous Fusion candidate edges remain. Three additional Fusion receipt/header reads corroborate existing mints. There are now 1,206 possible-redemption movements and 1,787 possible-deposit movements; 179 movements still have competing Fusion links.

| Refinement | Newly classified outgoing | Newly classified incoming |
| --- | ---: | ---: |
| Same-receipt pool swap through one relay | 0 | 11 |
| Possible source-chain transfer pairs | 8 | 8 |
| Bounded deposit-to-mint fee candidates | 0 | 3 |

Remaining unmatched outflows are BSC 207, Ethereum 10 and Avalanche 7; inflows are BSC 400, Ethereum 27 and Avalanche 3. The December 2021 1.1M and 100k payments and January 2024 204,567.658073647823395109 payment remain unmatched. All eight source-chain pairs and all eleven newly identified swap inflows retain `backing-disposition-unresolved`.

**Relay paths.** The original pool rule required a transfer directly between the pool and operating address. `single-relay-pool-swap` now follows one intermediate address in the same transaction, chain and asset. Both transfer legs must be receipt-verified, equal in raw amount and precision, and ordered correctly. Exactly two nonzero transfers of that asset may involve the relay; fan-in, fan-out and partial forwarding are rejected. The pool-side leg must independently pass the existing swap/transfer checks. Both asset event IDs appear in `routeEventIds` and `exchange-evidence.csv`. For example, Avalanche transaction `0x98d8f7eed22ba1d04fe09d514fb4841a6a1742723ba5513b1282a01c809095f8` has a pool-to-router BUSD.e transfer followed by an equal router-to-operating-address transfer, supported by the pool's counter-token input and swap event. The rule does not infer beneficial ownership of any participant.

**Source-chain pairs.** An optional `sourceChainRule` compares only still-unmatched, receipt-verified movements with timestamps. The incoming sender must equal the outgoing recipient, the chains must differ, and the incoming event must precede the payment by at most five minutes. The nonnegative difference is bounded by 50 BUSD and 5% of the incoming amount. These are disclosed search assumptions. Amounts use exact decimal normalization and differences must be representable in the policy precision. Every eligible pair is retained; competition is flagged on both sides rather than resolved greedily. Existing deposit/mint and burn/payment candidates cannot be reused by this pass. `source-chain-pairs.csv` writes each proposed pair once, keyed by both event IDs; JSON links are bidirectional. No Fusion burn is implied, and the asset configuration must already establish which contracts are being compared as representations of the same asset. These are candidates for user bridging or source-chain routing, not proven treasury rebalancing.

**Net mints.** `depositFeeRules` is separate from the outgoing fee policy and disabled unless configured. For BUSD it applies only to Ethereum: both source and Fusion receipts, same holder, 0–300-second delay, positive difference at most 0.05 BUSD and 1% of the deposit. It proposes three new links involving a combined 113699.980580294796922512 BUSD deposited, with the exact hypotheses below. The requests, successful receipts and headers are retained under `fusion-deposit-fees.*` in the source-review evidence directory.

| Ethereum deposit transaction | Deposit minus mint (BUSD) | Delay |
| --- | ---: | ---: |
| `0x08d38ee6244df95e11dc1b4d94180f70b40fa99ea220b77771ea972be1a1916f` | 0.019980377732618752 | 99 seconds |
| `0xc485a6b6e6c1c5557114cb1656c29a70beed304c5c95793cbc82148dc026b1ab` | 0.020125625447486227 | 126 seconds |
| `0x87e6cd4f2da6312e26de78c9edef9b5f251b7b8be53020e2f361a4bec1fdc2db` | 0.020497509834977995 | 95 seconds |

This does not authenticate a fee schedule or prove the economic relationship. The candidate rule is explicitly `bounded-deposit-fee`; the precise mint event IDs and supporting files remain in `candidate-pairs.csv`.

**Coverage limits discovered.** Before this pass, none of the 232 unmatched outflows had a full-time, non-conversion Fusion burn within the preceding five minutes whose amount exceeded the payment by no more than the configured chain fee cap, even without requiring the same holder. Further reductions cannot responsibly be obtained just by ignoring holder identity. Other-token funding is a useful unresolved lead, but bounded historical BSC log probes were rejected by the tested PublicNode, BNB dataseed and [dRPC public endpoint](https://drpc.org/chainlist/bsc-mainnet-rpc). Exact request/response pairs are preserved in `evidence/2026-10-11-busd-routing-review/`; provider errors are not zero activity. No paid service was used. Broader other-token history, changed-recipient instructions, split payments and exchange records remain separate evidence needs.
