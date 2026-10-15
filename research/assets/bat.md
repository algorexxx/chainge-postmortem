# BAT: partial investigation

## Indexed BSC history update

The anonymous Binplorer address/token index returned 0, 27, 14, 2 and 8 operations for the five configured vaults, all below its 100-row response cap. These 51 address-view rows identify **47 distinct transactions**. Successful RPC receipts and block headers recover **48 vault BAT Transfer events**, compared with 12 previously retained. Internal transfers appear in both address views; receipts also recover an additional log within a listed transaction. All indexed rows have a corresponding receipt log. Thirty-six indexed row amounts differ from the exact receipt quantities, so indexed amounts are never used in accounting.

The receipt-derived nets agree with all five pinned balances at BSC block 63769742: four zeros and **161.524143066016869989 BAT** at the 66ff vault. The combined site ledger now contains 91 vault movements across the configured source chains. The newly discovered 17 older BSC external outflows have now been assessed individually. All 36 retained external vault outflows pass the all-outflows review; observed disposition does not certify settlement.

Evidence: [indexed histories](../../evidence/2026-10-15-bat-binplorer-discovery/), [47 successful receipts](../../evidence/2026-10-15-bat-indexed-receipts/), [headers and transaction inputs](../../evidence/2026-10-15-bat-indexed-context/). Reproduce the index/receipt comparison with `research/review-indexed-receipts.ps1`, using these first two directories, the BSC BAT contract and cutoff 63769742. See the [indexed-first procedure](../indexed-history-workflow.md).

The 12,754-request broad scan was stopped. Five index calls plus targeted verification supplied the retained history in minutes. This remains indexed-history corroboration, not full RPC interval coverage or proof against index omissions. The zero-row predecessor is not independently proven inactive. Earlier sections describe the initial investigation and its evidence boundaries; this update supersedes the earlier BSC acquisition status.

Reviewed 2026-10-15. [Configuration](../../reconciliation/bat.json). This is a retained-evidence investigation, not a completed reconciliation or a finding of loss. All 491 retained Fusion events now have public receipt corroboration, and all 43 positive configured standard-token positions have matching pinned state and checked restriction flags. BSC history remains incomplete, and the vault inventory is not proven exhaustive.

## Public access correction

The earlier claim that Fusion access was unavailable was too broad. Public FSNEX already exposed receipt-derived data; this repository lacked its importer. Existing SSH access and node chain/head queries also succeed. The [public-first workflow](../fsnex-public-workflow.md) now supports anonymous receipt capture and explicit import, without requiring contributors to have private RPC. Receipt-aware classification identifies 48 migration mint/burn events. Completing receipt coverage and configuring verified slice relationships reduces the unresolved Fusion burn queue from 99 to 40 without relaxing matching rules.

## Identity and scope

The initial screening understated the complexity. The full Fusion catalogue contains older parents and dated children in addition to the newer token. Issuer and market-data chain lists are not exhaustive inventories of bridged representations.

| Chain | Contract | Treatment |
| --- | --- | --- |
| Ethereum | [BAT](https://etherscan.io/token/0x0d8775f648430679a709e98d2b0cb6250d2887ef) | Original token; 18 decimals |
| Avalanche | [BAT.e](https://snowtrace.io/address/0x98443b96ea4b0858fdf3219cd13e98c7a4690588) | Bridged representation; backing not independently reconciled |
| BSC | [Binance-Peg BAT](https://bscscan.com/token/0x101d82428437127bf1608f699cd651e6abf9766e) | Pegged representation; backing not independently reconciled |
| Polygon | [BAT](https://polygonscan.com/token/0x3cef98bb43d732e2f285ee605a8158cde967d219) | Checked candidate representation; no activity at the five tested addresses in the captured range |

Solana, NEAR, Harmony, Energi and BNB Beacon representations remain discovery leads. Their existence does not establish Chainge custody. No balances on those chains are assumed zero.

## Fusion ledger

Reference: [block 15130080](https://fsnex.net/block/15130080), 2025-10-07 08:43:50 UTC. This is the highest known recovered backup, not independently established network finality. The [115-page catalogue](../../evidence/2026-10-15-bat-token-inventory/) and [token/log/holder responses](../../evidence/2026-10-15-bat-fusion-public/) preserve discovery. Public indexed transaction details are [retained separately](../../evidence/2026-10-15-bat-fusion-context/). They are not presented as independently obtained RPC receipts. A later [public receipt package](../../evidence/2026-10-15-bat-fsnex-receipts/) supplies 469 complete transaction details, corroborating all 491 retained Fusion events. Public block headers are retained in the [header package](../../evidence/2026-10-15-bat-public-headers/).

The [newer token](https://fsnex.net/address/0x8083d1465ea684ce18888bd58b182a33b52a6647) has 77 indexed Transfer events and reported supply **6,483.855833284028158463 BAT**. This is token supply, not a USD valuation or consolidated customer liability. All 30 positive newer holder balances agree with pinned state. Their factory block flags are false and the token is unpaused at the reference. Existing private RPC was used only for state unavailable through the public API; node and explorer share infrastructure.

The [legacy full-time child](https://fsnex.net/address/0x3a28a0e473cccae138e542846151384b28e5aefd) belongs to [parent a735…f048](https://fsnex.net/address/0xa7357c06aa73558d5927a83f24fe7f9075fdf048). Its indexed balances include **10,188,476.534765708790214724** at address one. Address one is not the zero-address burn mechanism. Four dated children represent overlapping rights; neither those balances nor the parent's supply counter can be added to newer supply as independent customer claims. Earlier FRC758 parents and apparently unused FRC759 families are retained for further semantic review.

The legacy parent and all five child runtime bodies match the preserved compiled FRC759/Slice source after normalizing only the exact Solidity 0.8.12 compiler metadata digests. All 13 positive standard legacy positions agree with pinned balances; their parent blocking flags are false. Parent/child interval getters agree with the configured relationships. The parent is unpaused. This does not establish ownership, practical redemption or economic treatment. [State evidence](../../evidence/2026-10-15-bat-pinned-fusion/) and [reproducible checks](../check-fusion-restrictions.ps1) preserve the distinction.

## Vault balances and coverage

Five candidate addresses from earlier investigations are checked per configured chain. Their inclusion is a research boundary, not proof of a segregated vault or common beneficial ownership. Internal-transfer classification depends on that boundary.

At source-chain blocks immediately before the Fusion reference time, the tested addresses hold:

| Representation | Pinned block | Combined observed balance |
| --- | ---: | ---: |
| Ethereum BAT | 23524761 | 285.066027757181258231 |
| Avalanche BAT.e | 69908002 | 9.877548098291534566 |
| BSC Binance-Peg BAT | 63769742 | 161.524143066016869989 |
| Polygon BAT | 77364542 | 0 |

All positive balances above are at [66ff…ecf1](https://etherscan.io/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1), on their respective chains. The other four tested addresses returned zero at those blocks. Bridged and pegged holdings are shown separately from original Ethereum BAT; their redeemability and backing have not been established here. These observations do not establish a missing amount.

Ethereum and Avalanche two-direction numeric log captures and receipt/header checks support the retained vault histories. Ethereum has 11 unique movements and Avalanche 32 before later BSC additions. Ethereum and Avalanche tail queries find no later vault BAT movements through their separately pinned October 15 heads. Polygon returned no movements for the tested addresses and contract. BSC balance bisection recovers specific missing transfers, but cannot prove complete history: equal endpoint balances can conceal cancelling omissions. Failed queries and unindexed explorer responses are preserved and never treated as zero activity.

## Ethereum outflows

The [271.441796283111524564 BAT payment](https://etherscan.io/tx/0x5140207400b30e9ebec50ec768bda57976bbf565813924dd190c9c650423c58b) follows an exact-amount indexed Fusion burn by the same holder by 63 seconds. It is a possible redemption, with successful public Fusion receipt and call corroboration; an authenticated settlement identifier remains unavailable. The recipient still holds that BAT at the later pinned Ethereum observation.

The [1,000 BAT payment on August 23, 2024](https://etherscan.io/tx/0xde9bc7f0bfcdabe891c77239886a55414982f244794a6e0a18b25e4fc1da0f8b) has no matched burn in the retained Fusion data. Its recipient had zero BAT immediately before the payment. Four later swaps spend 300, 300, 190 and 200 BAT; **10 BAT remains** at the pinned later observation. The swap receipts show BAT entering pools and wrapped ETH routing. Public Blockscout internal-transaction records show native ETH delivered back to the recipient after fees; anonymous RPC trace requests failed. No exchange-deposit endpoint or beneficial owner is established. Later native-asset spending is outside this trace, so the ETH is not described as still retained.

Evidence: [Ethereum follow-up](../../evidence/2026-10-15-bat-ethereum-followup/), [swap receipts](../../evidence/2026-10-15-bat-1000-path/), [successful retry and opening balance](../../evidence/2026-10-15-bat-1000-final/), [indexed native transfers](../../evidence/2026-10-15-bat-swap-native/).

## Avalanche outflows

The 52.497553137772762957 BAT.e recipient retains that balance at the later pinned observation; it is a downstream holding, not newly discovered vault backing. Other recipients swap BAT.e through pools and routing contracts. The 41.92711185 BAT.e payment is followed by a [swap](https://snowtrace.io/tx/0xb2832d235cbdbd3b893049dc3aa7be9c78784bfec7d60fc565d972ff5169b33c) delivering **18.500521 USDC** to the same recipient after the recorded fee. Other retained swap receipts show wrapped AVAX routing; final native delivery requires trace evidence and is not inferred from ERC-20 logs alone.

Recipients also have other deposits and withdrawals. Amount similarity does not allocate every later payment to an earlier vault outflow. In particular, the 35.30097794 BAT.e recipient's later swap spends slightly more, and later large burns from that mixed account must not be assigned entirely to that vault payment.

Two direct vault-to-router BAT.e payments, 23.582411852602717275 and 15.781180362789637973, follow equal-amount newer Fusion burns within two minutes. The router differs from the burning holder. These are candidate routed exchanges, not automatically accepted BAT redemptions.

Evidence: [recipient histories and balances](../../evidence/2026-10-15-bat-avalanche-followup/), [onward receipts](../../evidence/2026-10-15-bat-avalanche-onward/). Downstream swaps remain within the original vault case rather than becoming separate outflow cards.

## BSC recovery

Historical state localization recovers internal funding into the later vault and three payments to the same recipient, including 74.270413138824698715, 11.590329225544257336 and 39.341717061383004810 BAT. The later vault's recovered net agrees with its pinned 161.524143066016869989 balance. Earlier vault history was subsequently recovered through the indexed review below; this does not certify chain-wide coverage.

The [11.590329225544257336 BAT payment](https://bscscan.com/tx/0x6bfcf9663b0d558fa70d1f8e6be156450bebc80ddc49fea1fc4f42c31d49f46d) follows a [12.461221548641540416 Fusion burn](https://fsnex.net/transaction/0x876ace16cb5b9743cb3501809540e8decce6ed5d25ad0bcd5c8ae5fc4ed896ec) by the same holder by 58 seconds. The burn memo explicitly names the BNB chain, BSC BAT contract, same recipient and exactly 11.590329225544257336 delivered units. This is now a memo-supported candidate rather than an unexplained fee difference. The exact-amount matcher remains unchanged and no settlement is automatically accepted. Downstream disposition is documented below.

## Reproduction and remaining gates

```powershell
./reconcile.ps1 -Asset bat
./capture-plan.ps1 -Asset bat
```

Generated accounting, coverage, balance checks, matching candidates and complete retained movement lists are derived from the configuration. Exact amount/time matches remain hypotheses. No stablecoin fee assumptions are imported into BAT.

The [case register](bat-closeout.json) records each retained external vault payment, including smaller Avalanche/BSC cases. Its reviewed-with-limits status records the evidence boundary; it does not certify that missing history or settlement questions are resolved.

Full reconciliation still requires a validated earlier FRC758 historical model, proof of economic rights and migration treatment, broader vault discovery, and fuller BSC coverage. Receipt coverage and standard holder state/restriction checks are complete within the retained inventory. General Chainge announcements provide chronology, not a BAT-specific freeze date or explanation for a transfer. Reopen individual conclusions on authenticated settlement identifiers, verified additional custody, new receipt/state evidence or reliable destination attribution.

## Closing follow-up: holders, routing and recipient paths

The three largest newer positions reconcile from retained transfers. The 3,495.865849470520717329 BAT position at `0x05d8d08ef14fa12fef140f81734e6cf2b3b586a6` receives four transfers from `0x8a4aa176007196d48d39c89402d3753c39ae64c1` and has no retained outgoing transfer. The 2,622.273258479123134403 and 112.417265913965349909 positions each originate in a same-holder legacy burn/newer mint in the same successful receipt. The first holder's upstream vault inventory contains direct mints and other transfers; a mint is not evidence of external funding. No ownership or backing inference follows from this inventory.

The reusable controller-link reviewer finds a source-event identifier shared by Fusion mint `0xe256592ae37d8257c597c63933472c7dc430a019ae4257acf9773abfe568657c` and BSC transaction `0x48c2f6afa13e81b5de6b29c968cfe62c43be8f9771e0b04497021dca8a295392`. The source receipt reports a 23.720811314140811856 BAT swap output and the Fusion mint is 23.582411852602717275. This is a source-event candidate, not a fee rule or independent reserve proof.

The two newer burns followed by Avalanche router payments have JSON memos requesting Fusion token index 5 with amounts 6,856,156 and 3,280,655 raw units. This is evidence of an intended different-output route. An index alone does not authenticate a token contract, and the two BAT burns are not relabelled as ordinary BAT redemptions.

Three BSC recipient paths are now retained:

- The 74.270413138824698715 BAT recipient later sends 74.270422978164807171 BAT to [ea54…4ba6](https://bscscan.com/address/0xea5412ed90053a07054ddeba55f3ef4215ac4ba6). That address holds 87.510970592236726368 BAT at block 63769742. This is a mixed downstream balance, not newly discovered vault backing or an allocation of the entire payment.
- The 11.590329225544257336 BAT payment enters a later [multi-asset swap](https://bscscan.com/tx/0xfee8a1861a04465b7e1fc9a25a8b3651d0567a9c01e419d18bc91d872d1caf24). The transaction returns 38.018570699797398603 Binance-Peg USDT to the recipient, but three other input assets contribute. Do not assign the whole output to BAT.
- The 39.341717061383004810 BAT payment is followed by an [equal BAT swap](https://bscscan.com/tx/0x83389d6c9616a71c202b4aba9b9bf44e2378c331f560ec673c8db35234a1f901). A successful call trace records 0.018626786348580600 native BNB delivered back to the recipient. Later native spending is not established here.

[Recipient evidence](../../evidence/2026-10-15-bat-bsc-recipient-followup/) includes opening/intermediate/closing state localization, successful receipts and block headers. Cancelling omissions remain possible. [Additional anonymous provider attempts](../../evidence/2026-10-15-bat-bsc-public-retry/) preserve denied, rate-limited and range-limited queries; none is counted as an empty history.

Earlier FRC758 ordinary balances and `sliceOf` arrays were queried for observed addresses. The first old contract has three positive ordinary balances; the second has two, including a 100,000-token position. They are separate predecessor/discovery evidence, not additions to the six standard-token ledgers or proof of customer claims. The interval-event replay still lacks a validated historical accounting model, as in the earlier BUSD investigation. Failed legacy-child `totalSupply` calls are retained as failures, never zero supply.

Reproduce the added reviews with `research/review-holder-funding.ps1 -Asset bat`, `research/review-controller-links.ps1 -Asset bat -FusionEvidenceDirectory evidence/2026-10-15-bat-fsnex-receipts -Controller 0x3668c219b1fa8fe8175158f6ce91ded36fde9152`, and `research/check-fusion-restrictions.ps1 -Asset bat`. The workflow checklist distinguishes unfinished acquisition from evidence-limited conclusions; reviewed outflows alone do not establish completion.


## Indexed BSC closing review (October 15)

The bounded BAT review now covers **all 36 retained external vault outflows**, including all 17 newly indexed BSC payments. The ledger retains 91 source-chain movements (48 BSC). All five BSC vault replay positions agree with pinned balances. The five initial index requests replaced a proposed 12,754-request discovery scan; no day-scale scan was resumed. Below-cap index results and net agreement cannot exclude cancelling omissions.

The 17 additional cases comprise:

- Eight vault swaps with receipt-observed Binance-Peg USDT returned to the same vault. The eight BAT inputs range from 11.700779673983138212 to 153 BAT. This explains immediate disposition, not settlement of Fusion BAT rights.
- A [535.512619206112929011 BAT route](https://bscscan.com/tx/0x6343be5267edaff5b9462843438eecf358c8c5bfb3fd530204b6fc35fb2988f3) whose successful native trace delivers 0.285098816006027525 BNB to `0x998b73d9780f3d2f18354d39a77179310f9ec4ab`, rather than back to the sending vault.
- A [3,348.801071337764116072 BAT payment](https://bscscan.com/tx/0xd348b5fc00b26fc1457b70d02f9cb5e5a66a564cc4eba2136a473866a47dac08) sent alongside other assets. A [later BAT-only swap](https://bscscan.com/tx/0xe013d489e3899f27a7a82b826b88c6844c0578982ee427e88d45947f871d151b) spends the exact BAT amount and returns 1.501186295073880652 native BNB to the recipient. Its BAT balance is zero at block 63769742. Other assets in the original payment do not contribute to this particular swap; later native spending is not established.
- Three payments of 1, 11.41 and 0.830547614071919197 BAT that combine exactly into [one 13.240547614071919197 BAT onward transfer](https://bscscan.com/tx/0x05154ddb8a880ca1735920ec6a8a48ba2a0b91f489e88f0fa0c6c80d5c0617ae). The downstream 87.510970592236726368 BAT balance includes a later payment too. It is neither three separate balances nor additional vault backing.
- Three larger recipient payments: 1,128.278737819847633439, 529.497341015268153586 and 1,986.897389170329410722 BAT. All reach `0x8894e0a0c962cb723c1976a4421c95949be2d4e3` through intermediate addresses. The first and third paths include prior funds; their final transfers are larger. The 529.497341015268153586 BAT path preserves the exact amount through the retained hops. These paths do not establish a credited exchange account, ownership or payment purpose.
- One 198.466296866432420251 BAT payment to a recipient with prior funds, a reversible onward transfer and subsequent swaps. Exact receipts show 107.836015048607388851 BAT exchanged for 22 Binance-Peg BUSD, followed later by 90.768589780159181550 BAT routed to a swap returning 0.074636195663480918 native BNB. Mixed funds prevent attributing every output exclusively to the vault payment.

BscScan's indexed public [transaction page](https://bscscan.com/tx/0xd16c69ccebadf9da63f42649eeff64fa2e073dda05c905d2206ac9a0a4576869), reviewed through web search on October 15, labels `0x8894e0a0c962cb723c1976a4421c95949be2d4e3` **Binance 51**. This is an explorer attribution observed in 2026, not independently verified historical control in 2023 or an identification of the deposit beneficiary. Direct retrieval of the case-specific explorer page was unavailable; exact transfer claims come from retained RPC receipts instead of that label.

Six of the new payments have nearby Fusion burns within ten minutes, but amounts differ and the retained evidence does not authenticate a fee or settlement relationship. These are documented leads, not accepted redemptions. No tolerance was widened. All 17 payments predate the retained general 2025 disruption announcements; chronology does not establish purpose, and no BAT-specific freeze date was authenticated.

Evidence: [recipient discovery](../../evidence/2026-10-15-bat-new-recipients-index/), [exact receipts and pinned balances](../../evidence/2026-10-15-bat-new-recipients-verified/), [headers](../../evidence/2026-10-15-bat-new-headers/), [native traces](../../evidence/2026-10-15-bat-new-native-traces/), [downstream receipts](../../evidence/2026-10-15-bat-downstream-verified/), and [final hop and mixed swap](../../evidence/2026-10-15-bat-final-recipient-verified/). Two router/pool index responses reached the 100-row cap; those histories are explicitly truncated and were not used as complete ledgers. Anonymous PublicNode rejects historical receipts/state while supplying block headers; the retained failed calls are not zero balances or empty histories.

The [case register](bat-closeout.json) stores conclusions, while `selection: "all-outflows"` derives the scope from current ledger events. Adding any new outflow makes closeout fail until assessed. `review-workflow.ps1 -RequireFinished` also enforces closeout. Bounded research is finished with stated limits; BAT remains a **partial investigation**, not a completed reserve reconciliation. Outstanding boundaries include historical FRC758 semantics, consolidated economic claims, exhaustive custody discovery, independent history completeness, destination ownership and authenticated settlement. Reopen on specific new evidence rather than automatically restarting a broad scan.

Closing verification: zero build warnings/errors, 323 tests passing, all three asset reports reproducible, shared UI checks passing, and all 4,602 BAT evidence files matching manifests and staged bytes. Privacy pattern review found zero matches; only public evidence and reviewed findings were staged.
