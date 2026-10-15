# USDC: large outflows near the end of retained activity

Reviewed 2026-10-13. This is a continuation of the [USDC assessment](assets/usdc.md), not a finding of loss or ownership. Selection uses each chain/token/sender separately: the two largest unmatched external outflows of at least 10,000 token units in the 90 days preceding that sender's last retained outflow. This window is a research priority, not a settlement rule. Incomplete histories can change the endpoint and ranking. Internal transfers remain in the ledger but are excluded from this particular shortlist.

## 69,955.999671 Ethereum USDC: near-exact routing candidate

[Payment d2b5…a288](https://etherscan.io/tx/0xd2b58bd63e5ee5a477ed769f5ffbd68063dfb88724ee1c016686ba190709a288) left [3f0e…aa8e](https://etherscan.io/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e) for [f11b…c52c](https://etherscan.io/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c) on September 5, 2024 at 06:48:11 UTC. Reconstructed sender balance fell from 74,274.423715 to 4,318.424044 USDC, conditional on retained history.

The same recipient initiated a [70,000-unit Fusion burn](https://fsnex.net/transaction/0xe331a649aebe51fa9b341e0e38a42606a14e0fed0e89ea1976fb001c477c9250) at 06:46:35, **96 seconds earlier**. Its successful receipt and calldata name ETH, the correct Ethereum USDC contract and this recipient. The memo quotes **69,955.999869**, whereas the actual payment is **69,955.999671**: the payment is **0.000198 USDC lower**. This is a strong possible-redemption relationship, not an exact memo match or authenticated settlement. No matching threshold or accounting classification has been relaxed. Source: retained [Fusion receipt/input](../evidence/2026-10-13-usdc-fusion-receipts/transactions-1000.response.json) and [Ethereum receipt](../evidence/2026-10-13-usdc-investigation/successor-review-1-001.response.json).

The recipient later received other funds, including 84,945 USDC on September 5. Its September 22 [154,887.164464-USDC transfer](https://etherscan.io/tx/0x10bf830c856adbd3d7f878f733324b8f8f0ca91d59e2984026576cca9260e84f) reached [9b2993…2763](https://etherscan.io/address/0x9b29937d3b1489e7142c6905bb3e61b3cd472763), then [Binance-14-labelled 28c6…1d60](https://etherscan.io/address/0x28c6c06298d514db089934071355e5743bf21d60) five minutes 36 seconds later ([receipt](https://etherscan.io/tx/0xe62fb6a44211bdeafea70397676301e3f8b2aa2aea83edbb005584c5db0eace2)). This is a verified later combined-balance route; **154,887.164464 is not an additional vault outflow or an amount wholly attributable to the 69,955.999671 payment**. The credited exchange account is unknown. Label evidence was preserved in the [BUSD exchange-path assessment](busd-204k-exchange-path.md).

## 17,000 Ethereum USDC: late distribution with mixed funds

On May 22, 2025 at 01:43:35 UTC, [66ff…ecf1](https://etherscan.io/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) paid [17,000 USDC](https://etherscan.io/tx/0xd061965ff4a96f2fc1eb0ef09fb3c7d70fc03ae3d6983c38b434b17cac420082) to [4408…0a80](https://etherscan.io/address/0x440872cfddac749d9f201234395fc20ed5480a80). This is about 97.48% of the reconstructed sender balance, and one day before its last retained Ethereum USDC outflow. The calldata is a direct ERC-20 transfer, with no routing memo. No accepted Fusion settlement pairing is established.

The recipient's returned history reconstructs a pre-existing **720.651071 USDC** balance. The following receipt-confirmed transfers therefore distribute mixed funds:

- At 02:03:59, [15,000 USDC](https://etherscan.io/tx/0x983be5fb3858bc0ae4ead9cba0211cb8abd0257fd8d612b96dbaf743a074a4ae) went to [de0a…94e0](https://etherscan.io/address/0xde0a5ee32ed914c2d3f13128c2b375decdc394e0). That address later received 3,150 more USDC and sent [18,150 USDC](https://etherscan.io/tx/0xbca797ac32f1ac29c76bdb6c72794cfb5ded19667214e9d4c67fc93255650128) to [53e2…dd28](https://etherscan.io/address/0x53e2cb064e2de9594584c9dba24ef071f110dd28) on August 14. The October 15 follow-up below extends this route to an independently observed retained balance; ownership remains unresolved.
- At 02:09:23, [2,700 USDC](https://etherscan.io/tx/0x2cec663be097f0ca85ec19d55b730fd3d20af2a71773677da471f3d064220809) went to 9b2993…2763; at 02:13:11 it [forwarded 2,700](https://etherscan.io/tx/0xdf54bddba2744cb1266ad0ec6fcd505200fc4808f306270ff16c8965d05abd6a) to the Binance-14-labelled address. These receipts establish the route, not which part of the 17,000 funded it or who received the exchange credit.

Thus the 17,000 was not simply retained intact by the first recipient. The two later payments sum to 17,700 because the recipient already had funds. Do not interpret that sum as a second loss figure.

## Funding context does not resolve the earlier 416k case

At 22:43:23 UTC on September 1, f11b…c52c [funded the 3f0e candidate with 416,154.953803 USDC](https://etherscan.io/tx/0x37640e7333727a764f7766393985d7f5367d206a06f24a6a60ff0e02613c1136), 24 seconds before the already-reviewed [416,050.404450-USDC outflow](https://etherscan.io/tx/0x626d2b4a805005e7556d0180233f3b2913c4d6f185a5fd7a91b6557c17e0392e).

The funding transaction input requests **FSN**, the newer Fusion USDC token, and **f11b…c52c**, with quoted output **415,905.250831**. A [Fusion mint](https://fsnex.net/transaction/0x5b60672ac7c54f29d0f7dcae51f32f281f37c82c31337538f2d214598943e6e1) to that holder for exactly that amount follows 41 seconds later. Its successful receipt/input is retained in [transactions-0400](../evidence/2026-10-13-usdc-fusion-receipts/transactions-0400.response.json). This supports a deposit/issuance interpretation for the incoming funding, **not** a payout instruction to the unrelated 584d recipient. The outgoing 416k case remains unexplained by an accepted settlement link. Its [Netcoins/Hyperliquid trace](usdc-hyperliquid-trace.md) remains valid.

## Evidence and remaining work

[Recipient pages](../evidence/2026-10-13-usdc-late-recipients/) contain 34 f11b and 364 4408 explorer rows; [endpoint pages](../evidence/2026-10-13-usdc-late-endpoints/) contain 5 de0a and 161 9b2993 rows. All returned pages were acquired, but explorer pagination is not independently certified complete chain history. Exact full addresses and token contract addresses were compared; similar-looking dust addresses were not merged with the real counterparties.

[Late-context](../evidence/2026-10-13-usdc-late-context/) and [endpoint-context](../evidence/2026-10-13-usdc-late-endpoint-context/) contain 88 successful read-only RPC captures. **42 unique selected transactions** were checked against successful receipts, exact USDC emitter/from/to/raw-amount logs, and block-hash-bound timestamp headers. A [separate input capture](../evidence/2026-10-13-usdc-last-outflow-input/) confirms the 17,000 payment is a direct transfer. Discovery-only rows outside that selection have not been promoted to receipt-verified evidence. No new custody address is asserted.

The Arbitrum, Optimism and late Avalanche shortlist cases are now assessed in the [eight-case follow-up](usdc-arbitrum-optimism-avalanche.md). Avalanche receipts and its own explorer label now support the full 35,993.090970-USDC onward payment to a Binance-labelled address. Ethereum 37,503.237342 (August 23, 2024) remains a separate follow-up. Candidate redemption links do not authenticate settlement or ownership.


## October 15 follow-up: retained 18,150-USDC branch

The mixed-funds branch continued from 53e2…dd28 to [16a3…59c9](https://etherscan.io/address/0x16a37f695c797ffd0c0215756a5c3ceb4d5e59c9) for [18,150 USDC on August 14, 2025 at 14:27:23 UTC](https://etherscan.io/tx/0x5215e0d6f88bfd1ab376f9ff8511d1a5a1e9b8e385ce4096d303444078fdb7dc). That address then sent [18,150 USDC on December 17 at 16:34:47 UTC](https://etherscan.io/tx/0x0709eeaa2135b716b4b8f6bed292a3780df32ec169781d08d590fa2d25bf5eba) to [859b…6552](https://etherscan.io/address/0x859bfc94624f1f36ae6aed248ee5347f4ee86552).

Pinned balance calls show zero immediately before the latter transaction's block, 18,150 immediately after, and **18,150 USDC at block 26,198,056, October 15, 2026 12:56:59 UTC**. The returned token-filtered explorer history contains the incoming payment and no outgoing payment. Successful receipts support the transfers; the balance call independently supports the retained amount. The public pages inspected do not establish exchange attribution or ownership.

Evidence: [route, history and balances](../evidence/2026-10-15-usdc-terminal-closeout/), [receipts and headers](../evidence/2026-10-15-terminal-recipient-receipts/). This remains a downstream branch of the original 17,000 case. Additional funding prevents attributing all 18,150 to it; it is neither a new vault outflow nor an additional loss amount. Reopen on a later balance/transfer, reliable attribution or evidence connecting settlement to the original payment.

The original two-per-vault selection above is historical. The [closing review](closeout-assessment.md) expands review to all material unmatched outflows in the stated window, including newly recovered Polygon history.
