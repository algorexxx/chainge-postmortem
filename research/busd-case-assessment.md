# BUSD case assessment

Assessment date: **2026-10-11**. **Status: incomplete.** The required ledger comparing outstanding Fusion BUSD tokens with verified source-chain vault balances has not yet been delivered. This report records supporting research, not completion of the BUSD investigation or a finding about intent, responsibility or recoverability. See the [per-asset completion standard](../PLAN.md#required-outcome-for-every-asset).

Correction: the earlier description of this report as closing the investigation pass was too broad. Reconciled controller histories do not establish total outstanding tokens, a complete vault inventory or matched backing.

## Work required to complete BUSD

1. Finish the Fusion representation inventory and validate the earlier representation's accounting model. Publish all gross holder balances and evidence-backed adjustments separately, including the unresolved relationship between the large legacy and newer positions, address-one holdings and time slices.
2. Complete the chain-qualified BUSD vault inventory, separating verified custody roles from candidates and ordinary counterparties. Establish which underlying tokens and holdings can be included without double-counting wrapped assets, pooled reserves or successor transfers.
3. Reconstruct each vault's opening balance, inflows, outflows and closing balance at a common cutoff with Fusion. The retained Fusion head is dated **2025-10-07 08:43:50 UTC**; the October 2026 source-chain snapshots cannot serve as contemporaneous backing for it. Record the corresponding source-chain blocks and evidence, or explicit gaps where historical balances cannot be established.
4. Publish the outstanding-token versus vault-balance ledger, with individual vault rows, adjustments, totals and coverage. Calculate an overall difference and coverage ratio only when the outstanding amount, asset equivalence and custody scope are established. Show later source-chain movements separately until a later Fusion snapshot can be justified.

These are continuing investigation tasks. Unknown balances or missing vaults are not zero balances, and partial observed holdings do not establish an overall deficit.

## What the evidence establishes

1. **Fusion representation accounting is substantially reconstructed.** The newer BUSD token and all five returned legacy child ledgers reconcile to every observed holder's retained balance. All 125 newer mint/burn receipts are classified. The earlier time-aware representation has preserved events and state for all 237 observed addresses, but no validated historical replay model. See [ledger reconciliation](busd-ledger-reconciliation.md) and [time-slice coverage](busd-time-slice-coverage.md).
2. **Historical BUSD operations are linked across chains.** Five BSC deposit/Fusion mint amount matches and two Fusion burn/BSC payment matches involve the same holder and controller addresses. Each leg is verified; cross-chain pairing remains an inference without a verified shared message identifier. These support `0x874de5761900ee526950d707d8cbd4b93092e290` as a strong candidate historical BUSD bridge operating address.
3. **A source-chain liquidity role is directly verified.** On Avalanche, MinterProxyV2 at `0x0b662d54824825725be1e76c0a2b1c8f49b7419b` returned `0x8a4aa176007196d48d39c89402d3753c39ae64c1` as its configured liquidity address at the May 2024 transaction block. Successful transactions and adjacent-block reads verify changes to `0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e` in August and `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1` in September. This confirms configuration, not segregated backing or human control.
4. **Several large outflows have countervailing context.** Major BSC destinations participate in swaps that return other tokens. The November 2023 BSC transfer to a Binance-labelled address is followed by a closely matching Ethereum payment into the controller. A separate Avalanche BUSD.e burn is followed by an Ethereum bridge-labelled payment into the same controller. These flows must be presented together.
5. **The later Ethereum BUSD balance can be followed.** Of 143.724135682808759163 sent to `0x8a4a...64c1` in January 2024, 120 subsequently enters a swap and 23.724135682808759163 passes through `0x3f0e...aa8e` to `0x66ff...ecf1`. The latter amount remains at that Ethereum address in the pinned October 2026 state.

All source-chain findings, full addresses, transaction hashes, timestamps and qualifications are in [BUSD source-chain flows](busd-source-chain-flows.md). No private community submission or contributor identity is needed to reproduce these observations.

## Coverage achieved

| Chain / token | Address | Returned Transfer events verified | Reconstructed final balance |
| --- | --- | ---: | ---: |
| BSC Binance-Peg BUSD | `0x874d...e290` | 1543 | 0 |
| BSC Binance-Peg BUSD | `0x4563...e039` | 3564, including 141 zero-value events | 0 |
| Ethereum BUSD | `0x874d...e290` | 9 | 0 |
| Ethereum BUSD | `0x4563...e039` | 232 | 0 |
| Avalanche BUSD.e | `0x874d...e290` | 22 | 0 |
| Avalanche BUSD.e | `0x4563...e039` | 347 | 0 |

Each controller ledger ends at a balance matching pinned state. Counts are per address; shared transactions must not be summed as independent deposits. Discovery relies on explorer indexes, while individual events are confirmed by node receipts. A reconciled balance does not exclude missing offsetting events.

Additional reviewed branches include the original large Fusion holder's BSC history, the 2021-22 large redemption and exchange-labelled paths, the November 2023 intermediaries, the 2024 liquidity-address transitions and the later small BUSD/BUSD.e positions. General-purpose exchange wallets and swap contracts are attribution boundaries, not invitations to assign all their subsequent activity to this case.

## Remaining observed positions are not a reserve total

| Chain / asset | Address | Pinned balance | Interpretation limit |
| --- | --- | ---: | --- |
| Fusion newer BUSD | `0xf11b4db13148d56e2a10ad701ad0ff338c62c52c` | 859506.153699140493846300 | Representation balance at retained 2025-10-07 state |
| Fusion legacy full-time child | `0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e` | 859506.1537 | Blocked legacy position; independence from or replacement by the newer amount is unresolved |
| Ethereum BUSD | `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1` | 23.724135682808759163 | Observed balance; beneficiary and entitlement unverified |
| BSC Binance-Peg BUSD | `0x8a4aa176007196d48d39c89402d3753c39ae64c1` | 0.080222660182393847 | Separate token and chain from Ethereum BUSD |
| BSC Binance-Peg BUSD | `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c` | 0.007998 | No personal ownership assertion |
| Avalanche BUSD.e | `0x983868374f46b78473ba05396ccd593192ed074e` | 3.999 | Residual from the observed intermediary transfer |
| Avalanche BUSD.e | `0x8a4aa176007196d48d39c89402d3753c39ae64c1` | 0.000000000000087769 | Dust after four returned movements |
| Avalanche BUSD.e | `0x6a01dd8e6384fe7835cb5cf124201bbd8a670056` | 0.010325562989512130 | Transaction counterparty; not itself the called vault contract |

This is a selected-position table, not an exhaustive list or valuation. It excludes other Fusion holders and assets received in swaps. The retained Fusion state predates the source-chain snapshots by a year. Different representations, expired slices, address-one positions and matched legacy/new amounts cannot be summed as customer liabilities or backing.

## Chronology and interpretation

| Period | Evidence-based description |
| --- | --- |
| 2021-12 to 2022-01 | Observed BSC deposits, Fusion issuance/burns and BSC payments support historical bridge operation |
| 2022-03 | Earlier Fusion representation is converted into legacy BUSD in a verified same-chain receipt |
| 2022-07-12 | The earlier controller transfers BUSD/BUSD.e to the later controller on BSC, Ethereum and Avalanche |
| 2023-11-02 | Controller funds leave BSC and Avalanche; closely related Ethereum funding follows |
| 2024-01 | Last returned BUSD events for the later BSC and Ethereum controller addresses |
| 2024-02 to 2024-05 | Newer Fusion issuance/burn/reissue history; large legacy/new amount relationship remains a hypothesis |
| 2024-05 to 2024-09 | Verified Avalanche liquidity-address role and subsequent configuration changes; related Ethereum BUSD transfers |
| 2025-07-21 | Last returned newer-Fusion BUSD Transfer event in the retained index |
| 2026-05-11 | Small incoming BUSD.e event at a reviewed counterparty; does not establish resumed service |
| 2026-10-11 | Source-chain balances and public discovery endpoints observed |

The service-disruption timeline is **not established**. No row above is labelled as occurring before or after the disruption merely because its date seems suggestive. Public announcements and staff communications must be independently sourced and reviewed before making that comparison.

## Questions that remain open and what would resolve them

| Question | Why current evidence is insufficient | Evidence needed to resolve |
| --- | --- | --- |
| Did the large newer Fusion issuance replace a particular legacy position? | Amount/route similarities are not a common migration identifier; both representations remain visible | Verified migration records, transaction payload decoding or authenticated implementation documentation |
| What were total customer BUSD claims and total backing? | Token supply, duplicate representations, time slices, swaps and pooled funds do not identify liabilities or reserves | A complete asset/chain inventory and authenticated accounting records, with an independently justified reconciliation model |
| Who controlled the operating addresses or exchange accounts? | Signed transactions identify addresses, not humans or exchange beneficiaries | Verifiable public attribution or appropriately obtained off-chain records; no personal submissions in this repo |
| What happened to exchange-internal credits? | Public chains stop at the exchange-labelled address | Exchange records or subsequent publicly verifiable account disclosures |
| Who ultimately received native swap outputs? | Historical BSC trace failed; Ethereum trace required a personal token | Historical call traces from a suitable provider or archive node; keep fees and recipient proof separate |
| Can the earlier Fusion representation be replayed exactly? | The naive interval model fails and the candidate source is not runtime-matched | Matched deployed implementation plus validated time-slice accounting, or historical state/trace access |
| Were there later Fusion events than the retained head? | Available Fusion data ends at block 15130080, dated 2025-10-07 | A later canonical chain source, if one exists |
| When did service restrictions begin, and what did official communications claim? | Chain activity alone cannot date a service outage or validate an announcement | Reviewed official announcements and staff communications with timestamps and public provenance |
| Are other chains or vaults relevant to BUSD? | The discovered addresses and selected tokens are not a global census | Additional documented deployments, token-list mappings or independently sourced transaction leads |

No conclusion here requires an allegation. Failed queries, contradicted discovery results and rejected accounting models are preserved alongside positive findings. The next BUSD deliverable is the outstanding-token/vault reconciliation above. A public site and repeatable collector must expose its completeness status; neither is implemented by this research pass.
