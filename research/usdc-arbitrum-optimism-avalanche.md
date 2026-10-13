# USDC: Arbitrum, Optimism and late Avalanche follow-up

Reviewed 2026-10-13. This covers eight shortlisted vault outflows, using anonymous public RPC and the previously retained Fusion receipts. It extends the [terminal review](usdc-terminal-review.md). Amounts are token units, not current dollar valuations. A possible redemption is not authenticated settlement; an exchange label does not identify the credited customer. Automated classifications and the complete retained ledger are unchanged.

## Burn/payment candidates

Five payments have successful Fusion burns from the same holder, for slightly larger amounts. Four follow within 38–45 seconds. The Arbitrum 261,490.095828 payment is delayed by **13 hours 1 minute 2 seconds**, making that association materially weaker. The legacy burns do not supply a verified source-chain routing memo or shared settlement identifier. Amount differences are approximately 0.05%, but no fee schedule is authenticated and no new fee rule has been installed.

| Source chain | Payment | Fusion burn | Delay | Difference |
| --- | ---: | ---: | ---: | ---: |
| Arbitrum | [300000.000000](https://arbiscan.io/tx/0xfcdb1918461bcd659caf8f0889cd45aba9fc6dc5f1c4be0b8a5ebe7bfcefb0b8) | [300150.199949](https://fsnex.net/transaction/0x16e180fedc0b5ae7c40870933f48317162d656f0af0e8abb9e95deca8d72993f) | 38 seconds | 150.199949 |
| Arbitrum | [261490.095828](https://arbiscan.io/tx/0x191c1acdb037896edd1fd8fa340bdad279ade7c634f1ea5664c31c6a3ec07fbe) | [261621.106333](https://fsnex.net/transaction/0x6dcd1ab9e88e59617a69eecdfc3f3b3622c433793f157cd38722dc94fa6c0a4d) | 46862 seconds | 131.010505 |
| Avalanche | [40000.000000](https://snowtrace.io/tx/0x55bf5c8c3dc5bdddd36fff067e03e1e77909c72e59b3687beef29f701508807e) | [40020.044495](https://fsnex.net/transaction/0xd8629a043ba29c0ae79034a7eea506779f2eff981a2d384ba698ddad13cf9d7d) | 45 seconds | 20.044495 |
| Optimism | [60299.971401](https://optimistic.etherscan.io/tx/0x11c4ace0060b8a4a82189d7a905d1864e1b50fab8d142f13dd747e330d0f7c02) | [60330.336471](https://fsnex.net/transaction/0x9787728adb738d626f7b44bf72c9095b82b8d8970405bbfa01fdd9585bdfeb95) | 45 seconds | 30.365070 |
| Optimism | [110000.000000](https://optimistic.etherscan.io/tx/0xd392b9b948698fc13d18de861ecbbc36c2e66fd8960275cc02e3af1894afd7b3) | [110055.199975](https://fsnex.net/transaction/0xdf75022c790c8ae25095b67fd32ea17caae1928ffc556cef724c73c01ef26962) | 42 seconds | 55.199975 |

Both sides' token emitter, sender, recipient, integer amount and successful receipt status were checked. The source payment recipients equal the burned-token holders. Competing activity and missing source histories still prevent treating these observations as independently settled claims. The table's links and retained receipt references make the hypotheses reviewable; no burn or payment has been removed from accounting.

## Arbitrum 261,490.095828: swaps and liquidity provision

The [January 2 payment](https://arbiscan.io/tx/0x191c1acdb037896edd1fd8fa340bdad279ade7c634f1ea5664c31c6a3ec07fbe) went from [4563…e039](https://arbiscan.io/address/0x456325f2ac7067234dd71e01bebe032b0255e039) to [f842…de66](https://arbiscan.io/address/0xf84225927948fbbe418c5e0c242fe3b26438de66) in bridged USDC.e. Bounded logs and successful receipts show these later uses on January 2:

| UTC | USDC.e out | Observed result in the same receipt |
| --- | ---: | --- |
| 13:47:55 | 90,000 | [89,991.722496 native USDC returns to the same recipient](https://arbiscan.io/tx/0xddb936f39c6aede689abedad916896fbfac76335dbfb9e753e34dd2ff9f8c1d2) |
| 14:13:02 | 10 | [Contract swap route with wrapped-native-token withdrawal](https://arbiscan.io/tx/0x8db5f4d0b514a0c68e3d2b4e7c76040640cf60bbbcbc1417a440e3aab83bab54); native proceeds not traced here |
| 14:33:42 | 60,000 | [60,009,818,447,576,098,279,989 raw DAI-token units return](https://arbiscan.io/tx/0x4eb8974efe8594f6f340631b36298b989b3f30333b342a9ff225805be1f2ec0d); raw integer amount shown without assuming decimals |
| 16:16:09 | 66,000 | [65,994.878406 native USDC returns to the recipient](https://arbiscan.io/tx/0x530d706eb52236ef28f71c9b145a81985a7ed94c137de8701530305abe93c3fa) |
| 16:16:31 | 44,560.656099 | [Deposited alongside 78,481.618869 native USDC](https://arbiscan.io/tx/0x524b732eac3b62feccf85f72a4b85542ddd741330a25b7146ed631957438c54a) into 8070…1774; that contract mints 122,904,054,747 raw units of its own token to the recipient |

The two relevant contracts are [native USDC](https://arbiscan.io/address/0xaf88d065e77c8cc2239327c5edb3a432268e5831) and [the DAI token](https://arbiscan.io/address/0xda10009cbd5d07dd0cecc66161fc93d7c9000da1). These receipt paths show asset conversions and a pool position, not a simple transfer to a centralized exchange. The shared intermediary e37e…bd09 is a route hop; it must not be treated as a final exchange destination merely because several payments touch it.

[Gamma's January 9 update on the Arbitrum forum](https://forum.arbitrum.foundation/t/gamma-stip-program-updates/19614/6) identifies 8070…1774 as its Ramses USDC–USDC.e vault. The same update reports an incident affecting it on January 4. This is protocol context only: this investigation has not determined this holder's withdrawals, incident exposure, compensation or losses. It does not establish the holder as a Chainge-controlled address.

## Arbitrum 14,982: representation conversion, settlement unresolved

The [January 5 native-USDC outflow](https://arbiscan.io/tx/0xb2f997383979b7c27a84e6d36c7f0b50532cf14dd1ed42ebad5a4dc65e05d16f) reached [dfc4…71a0](https://arbiscan.io/address/0xdfc4ca96eb97521bb66367900a14fea0c0a571a0). At 06:28:41 UTC, **14 minutes 49 seconds later**, [the recipient routed 14,982 native USDC through contracts and received 14,981.489630 USDC.e back](https://arbiscan.io/tx/0xf1c38ae4bbe6bca1ba0d72845e34ed4cc2db55dfc13ee6d3a5834bd253e928c5). This is a verified representation-changing route. No same-recipient burn was found in the one-day proximity review, and no authenticated settlement or final CEX destination is established. Later USDC.e use remains open.

## Avalanche 35,993.090970: full onward transfer to a Binance-labelled address

At 03:54:05 UTC on August 23, [8a4a…64c1 paid 35,993.090970 native USDC](https://snowtrace.io/tx/0x0606c91961052186d254bbbf37d428673bf72ed4adb8b5e790a3aed40f20635f) to [9b2993…2763](https://snowtrace.io/address/0x9b29937d3b1489e7142c6905bb3e61b3cd472763). At 04:50:43, **56 minutes 38 seconds later**, that address [sent the identical amount](https://snowtrace.io/tx/0xc43aa9634bf3ff2d15871df4d3a08669848582a60a7500f38721b36f6ea0f4d3) to [9f8c…c8b9](https://snowtrace.io/address/0x9f8c163cba728e99993abe7495f06c0a3c8ac8b9), which Snowtrace labels **Binance: 49** on Avalanche itself.

Historical `balanceOf` observations at blocks 49,598,475, 49,598,476 and 49,600,099 are respectively **0**, **35,993.090970** and **0**. Bounded both-direction logs and successful receipts corroborate the incoming amount and full outgoing amount. Unlike a label copied from Ethereum, this conclusion uses an Avalanche explorer label and Avalanche receipts. It does not establish the exchange account, authorization or a loss. No accepted Fusion burn link is established for the original payment.

The same intermediary received another **1,498.799975 USDC** from the next candidate on August 30 and forwarded the same amount to the same endpoint. These two additional receipt-confirmed transfers are context, not part of the August 23 payment: [incoming](https://snowtrace.io/tx/0x3eb872632d6949e698c8f15cf2953bb2b76ae2c9ac0b576747f5142dc3fa3a1d), [outgoing](https://snowtrace.io/tx/0xfbaa39a4347117acbdf35906a6b13990c9fe33d41d08ca74e94d4d3f25f56473).

## Avalanche 21,774.015699: mixed balance converted to BTC.b

The [July 17 payment](https://snowtrace.io/tx/0xcc911ae356a702a5d60a409d8e2a143c45cfd2533576821a72a6e2ca4201467c) reached [6d58…b15a](https://snowtrace.io/address/0x6d58c8987577c067cf8f103dc52ab453bedcb15a) at 08:21:46 UTC. Pinned state shows **40,965.411028 USDC already present**, then **62,739.426727** after receipt. At 08:25:51, the recipient [spent 32,522.137672 USDC through a contract route](https://snowtrace.io/tx/0x36fb260b151e952653f85f5a6faa4f068622074447f9d636837dbf6041caf48e), receiving **0.49908123 BTC.b** back and retaining **30,217.289055 USDC**. The output contract's `symbol()` and `decimals()` return BTC.b and 8 at the historical block. This verifies conversion from mixed funds, not conversion of precisely the original vault amount. Later BTC.b disposition remains open.

On July 23, the recipient [sent another 20,000 USDC to 1205…1fe1](https://snowtrace.io/tx/0xb330cd0fb156c487f81acdeb4a7a84ec237b881228d390aa61e1e52df49b0cb6). The receipt is retained; destination-chain settlement has not been traced. This later transaction is not an additional Chainge vault outflow.

Eight seconds before the original 21,774 payment, another address deposited [24,999 USDC into 8a4a](https://snowtrace.io/tx/0x89b570480160e8f1251f6e03c53b5a1a071d650fdc1891558c1fd239fcff09dc). Its input requests Fusion issuance for 4408…0a80, and a [24,983.990601-unit Fusion mint](https://fsnex.net/transaction/0x54841ffb3e092bdbe5c099673170cdfd1674b2d08735f9cda7c4caed45abef3c) to that holder follows. This is separate funding/issuance context, not an instruction to pay 6d58. No accepted burn link explains the original outflow.

## Evidence limits

Eight original payments and five associated burns were verified against their retained successful receipts. Fifteen newly queried downstream token logs were corroborated against successful receipts and matching block-hash/timestamp headers: eight Arbitrum and seven Avalanche logs. The discovery windows are explicit in saved requests; they do not claim lifetime history. The Arbitrum explorer request failed and is recorded as failed discovery, never as evidence of zero activity. Official anonymous RPC supplied the follow-up instead.

Evidence packages: [Arbitrum inputs](../evidence/2026-10-13-usdc-other-arbitrum/), [Arbitrum bounded logs](../evidence/2026-10-13-usdc-other-arbitrum-context/), [Arbitrum receipts/headers](../evidence/2026-10-13-usdc-other-arbitrum-receipts/), [Avalanche inputs/logs](../evidence/2026-10-13-usdc-other-avalanche/), [Avalanche receipts/headers](../evidence/2026-10-13-usdc-other-avalanche-context/), [Avalanche historical balances and token metadata](../evidence/2026-10-13-usdc-other-avalanche-balances/), and [attributed label observation](../evidence/2026-10-13-usdc-avalanche-label/).

The eight shortlisted cases are now assessed at this level. Outstanding questions are the delayed burn association, authenticated settlement and fee rules, ownership and authorization, later investment/bridge destinations, and existing source-history gaps. None of these findings establishes a consolidated USDC deficit.
