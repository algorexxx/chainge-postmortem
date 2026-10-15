# BAT: partial investigation

Reviewed 2026-10-15. [Configuration](../../reconciliation/bat.json). This is a retained-evidence investigation, not a completed reconciliation or a finding of loss. Fusion RPC access is unavailable during this pass, BSC history remains incomplete, and the vault inventory is not proven exhaustive.

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

Reference: [block 15130080](https://fsnex.net/block/15130080), 2025-10-07 08:43:50 UTC. This is the highest known recovered backup, not independently established network finality. The [115-page catalogue](../../evidence/2026-10-15-bat-token-inventory/) and [token/log/holder responses](../../evidence/2026-10-15-bat-fusion-public/) preserve discovery. Public indexed transaction details are [retained separately](../../evidence/2026-10-15-bat-fusion-context/). They are not presented as successful RPC receipts.

The [newer token](https://fsnex.net/address/0x8083d1465ea684ce18888bd58b182a33b52a6647) has 77 indexed Transfer events and reported supply **6,483.855833284028158463 BAT**. This is token supply, not a USD valuation or consolidated customer liability. Direct receipt, pinned holder-balance and restriction checks remain pending node access.

The [legacy full-time child](https://fsnex.net/address/0x3a28a0e473cccae138e542846151384b28e5aefd) belongs to [parent a735…f048](https://fsnex.net/address/0xa7357c06aa73558d5927a83f24fe7f9075fdf048). Its indexed balances include **10,188,476.534765708790214724** at address one. Address one is not the zero-address burn mechanism. Four dated children represent overlapping rights; neither those balances nor the parent's supply counter can be added to newer supply as independent customer claims. Earlier FRC758 parents and apparently unused FRC759 families are retained for further semantic review.

The five active legacy child runtime bodies match the previously compiled Slice runtime after removing trailing compiler metadata. This supports the interval-token interpretation, but does not verify current state, ownership, transfer restrictions or economic treatment. A parent-source match has not been established by this comparison.

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

The [271.441796283111524564 BAT payment](https://etherscan.io/tx/0x5140207400b30e9ebec50ec768bda57976bbf565813924dd190c9c650423c58b) follows an exact-amount indexed Fusion burn by the same holder by 63 seconds. It is a possible redemption, pending Fusion receipt and call verification. The recipient still holds that BAT at the later pinned Ethereum observation.

The [1,000 BAT payment on August 23, 2024](https://etherscan.io/tx/0xde9bc7f0bfcdabe891c77239886a55414982f244794a6e0a18b25e4fc1da0f8b) has no matched burn in the retained Fusion data. Its recipient had zero BAT immediately before the payment. Four later swaps spend 300, 300, 190 and 200 BAT; **10 BAT remains** at the pinned later observation. The swap receipts show BAT entering pools and wrapped ETH routing. Public Blockscout internal-transaction records show native ETH delivered back to the recipient after fees; anonymous RPC trace requests failed. No exchange-deposit endpoint or beneficial owner is established. Later native-asset spending is outside this trace, so the ETH is not described as still retained.

Evidence: [Ethereum follow-up](../../evidence/2026-10-15-bat-ethereum-followup/), [swap receipts](../../evidence/2026-10-15-bat-1000-path/), [successful retry and opening balance](../../evidence/2026-10-15-bat-1000-final/), [indexed native transfers](../../evidence/2026-10-15-bat-swap-native/).

## Avalanche outflows

The 52.497553137772762957 BAT.e recipient retains that balance at the later pinned observation; it is a downstream holding, not newly discovered vault backing. Other recipients swap BAT.e through pools and routing contracts. The 41.92711185 BAT.e payment is followed by a [swap](https://snowtrace.io/tx/0xb2832d235cbdbd3b893049dc3aa7be9c78784bfec7d60fc565d972ff5169b33c) delivering **18.500521 USDC** to the same recipient after the recorded fee. Other retained swap receipts show wrapped AVAX routing; final native delivery requires trace evidence and is not inferred from ERC-20 logs alone.

Recipients also have other deposits and withdrawals. Amount similarity does not allocate every later payment to an earlier vault outflow. In particular, the 35.30097794 BAT.e recipient's later swap spends slightly more, and later large burns from that mixed account must not be assigned entirely to that vault payment.

Two direct vault-to-router BAT.e payments, 23.582411852602717275 and 15.781180362789637973, follow equal-amount newer Fusion burns within two minutes. The router differs from the burning holder. These are candidate routed exchanges, not automatically accepted BAT redemptions.

Evidence: [recipient histories and balances](../../evidence/2026-10-15-bat-avalanche-followup/), [onward receipts](../../evidence/2026-10-15-bat-avalanche-onward/). Downstream swaps remain within the original vault case rather than becoming separate outflow cards.

## BSC recovery

Historical state localization recovers internal funding into the later vault and three payments to the same recipient, including 74.270413138824698715, 11.590329225544257336 and 39.341717061383004810 BAT. The later vault's recovered net agrees with its pinned 161.524143066016869989 balance. Earlier vault history remains under investigation; this does not certify chain-wide coverage.

The [11.590329225544257336 BAT payment](https://bscscan.com/tx/0x6bfcf9663b0d558fa70d1f8e6be156450bebc80ddc49fea1fc4f42c31d49f46d) follows a [12.461221548641540416 Fusion burn](https://fsnex.net/transaction/0x876ace16cb5b9743cb3501809540e8decce6ed5d25ad0bcd5c8ae5fc4ed896ec) by the same holder by 58 seconds. It is a manual candidate with an unexplained amount difference, not an authenticated fee or accepted redemption. The exact-amount matcher remains unchanged. Downstream disposition of these BSC payments is not established by the retained evidence.

## Reproduction and remaining gates

```powershell
./reconcile.ps1 -Asset bat
./capture-plan.ps1 -Asset bat
```

Generated accounting, coverage, balance checks, matching candidates and complete retained movement lists are derived from the configuration. Exact amount/time matches remain hypotheses. No stablecoin fee assumptions are imported into BAT.

The [case register](bat-closeout.json) records each retained external vault payment, including smaller Avalanche/BSC cases. Its reviewed-with-limits status records the evidence boundary; it does not certify that missing history or settlement questions are resolved.

Completion still requires Fusion receipt/input/state and restriction checks, resolution of older representation and migration semantics, broader vault discovery, and fuller BSC coverage. General Chainge announcements provide chronology, not a BAT-specific freeze date or explanation for a transfer. Reopen individual conclusions on authenticated settlement identifiers, verified additional custody, new receipt/state evidence or reliable destination attribution.
