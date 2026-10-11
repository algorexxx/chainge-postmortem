# January 2024 BUSD transfer to a Binance-labelled address

Observed 2026-10-11. The **204567.658073647823395109 BUSD** transfer from the Ethereum controller on January 1, 2024 was forwarded in full through two intermediate addresses to **`0x28c6c06298d514db089934071355e5743bf21d60`**, which Etherscan labels **Binance 14**. The terminal transfer also includes an earlier **92.645784413941010945 BUSD** payment. Both intermediate addresses have zero BUSD at the pinned observation block.

This establishes a public path to an exchange-labelled wallet. It does not identify the credited Binance account, its owner, any internal conversion, later withdrawal or recoverable balance. No assertion is made that the original amount remains segregated at Binance or that this was an unauthorized payment. The reason for the controller's payment and its relationship to outstanding Fusion tokens remain unresolved.

## Verified path

All amounts below are Ethereum BUSD at `0x4fabb145d64652a948d72533023f6e7a623c7c53`, with 18 decimals. Each transfer is independently matched to a successful node receipt, token address, sender, recipient and exact integer amount. Timestamps and block hashes are verified against node headers.

| Role in this path | Ethereum address |
| --- | --- |
| Historical controller | `0x456325f2ac7067234dd71e01bebe032b0255e039` |
| First intermediary | `0x99d1a2227b17ca28ed0287da301a91d0e1006571` |
| Second intermediary | `0x7c329d8deaeb8eb622e6b4b08dacc24d0afa94db` |
| Terminal address, Etherscan label Binance 14 | `0x28c6c06298d514db089934071355e5743bf21d60` |

| UTC | Movement | BUSD | Transaction |
| --- | --- | ---: | --- |
| 2023-12-20 05:45:11 | Controller to first intermediary | 92.645784413941010945 | [d3cbcb84...](https://etherscan.io/tx/0xd3cbcb84c314b58d4d18f806409e59b5b6a7fb2eada8139761a299313be2a7ad) |
| 2023-12-20 05:45:47 | First to second intermediary | 92.645784413941010945 | [5719d8b9...](https://etherscan.io/tx/0x5719d8b988b4ed7151c8e2b5589a96d1f3d4327c54230f04084aa9c876f4d053) |
| 2024-01-01 01:31:11 | Controller to first intermediary | 204567.658073647823395109 | [f32d2fd0...](https://etherscan.io/tx/0xf32d2fd024c70b88b051d744d7741c948a2fef088a247e358d72a2783d370e40) |
| 2024-01-01 01:31:59 | First to second intermediary | 204567.658073647823395109 | [5642dee5...](https://etherscan.io/tx/0x5642dee52b6c0a6a870209f78024f99ba84af5986f0f47d97b8ee810bd1799ad) |
| 2024-01-01 03:00:11 | Second intermediary to Binance-14-labelled address | 204660.303858061764406054 | [938dfb0a...](https://etherscan.io/tx/0x938dfb0a96b975e8c1b94147b5102b3030baa2be6054aab35dbfa292220c0354) |

The large amount reaches the second intermediary after **48 seconds** and the terminal address **1 hour 29 minutes** after leaving the controller. The terminal amount is exactly **204567.658073647823395109 + 92.645784413941010945 = 204660.303858061764406054 BUSD**. There is no BUSD deduction between these observed hops. These are token amounts; transaction gas is separate.

The existing controller ledger shows **214444.677838314895073224 BUSD** immediately before the large transfer and **9877.019764667071678115 BUSD** afterward. That running balance is derived from the 232 returned, receipt-verified controller events described in [source-chain flows](busd-source-chain-flows.md). It is not a historical state read or a complete reserve total.

## Balances and coverage limits

At Ethereum block **26170614**, hash **`0x71994af72514a8d5bc4267f4169b8d620c2bce06227552068a75de35e3563b5a`**, dated **2026-10-11 17:10:23 UTC**, direct `balanceOf` reads return **zero BUSD for both intermediaries**. The amount is therefore not still sitting as BUSD at either of those two addresses at that cutoff.

Routescan discovery returned four BUSD events for the first intermediary and three for the second, including the shared transfers. The five unique transfers all match direct node receipts. The returned histories net to zero at both addresses, matching pinned state. However, earlier control queries demonstrated incomplete Ethereum indexing at Routescan; this is not proof that no other offsetting events ever occurred. Historical `balanceOf` attempts around the transfers were rejected because the endpoint requires a personal token for archive requests. Those failures are preserved and are not interpreted as balances.

The [Binance 14 label](https://etherscan.io/address/0x28c6c06298d514db089934071355e5743bf21d60?amt=1-10) was independently captured from Etherscan. It is an attributed explorer label. The second intermediary's sweep pattern is consistent with an exchange deposit address, but its ownership and credited customer account have not been independently verified. Tracing unrelated outflows from the pooled exchange address would not establish where this particular customer's credit went.

## Comparison with the earlier cross-chain movement

Internal rebalancing remains a possible explanation. A transfer to an exchange-labelled wallet establishes the on-chain destination, not the ultimate economic destination. Funds could remain as exchange credit or leave as a different asset, on another chain, or to an address not yet attributed. A customer redemption is another unresolved explanation; none is established by this path alone.

The November 2023 example has additional evidence: **187290.327160557405335843 BUSD** reaches a Binance-labelled BSC address, and **187285.72716055 BUSD** subsequently arrives at the same controller address on Ethereum through the same intermediary address string. That return reaches the controller **25 minutes 31 seconds** after the exchange-labelled receipt. This supports a rebalancing inference, although the exchange-internal link is not proven. The January path uses different intermediaries and currently lacks an identified corresponding return leg. Different intermediaries alone do not distinguish legitimate rebalancing from another purpose.

A bounded check of the preserved histories covers **2023-12-31 00:00:00 through 2024-01-15 00:00:00 UTC (end exclusive)** at controller `0x456325f2ac7067234dd71e01bebe032b0255e039`:

| Chain / token checked | Returned incoming transfers in that window |
| --- | --- |
| BSC Binance-Peg BUSD | None |
| Ethereum BUSD | One: **2989.24318158**, from `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c`, on January 1 at 16:36:35 UTC; transaction `0x3e00c45d9ac11fc01690032cbcf50f8fcddf91737c0199390b8fb4a852c75804` |
| Avalanche BUSD.e | None; last returned event is November 2, 2023 |

The BSC and Ethereum checks filter the previously preserved receipt logs by token, recipient and block timestamp; the Avalanche check filters the receipt-verified discovery rows in the [source-chain evidence package](../evidence/2026-10-11-busd-source-review/README.md). This finds no comparable BUSD return into that controller during the stated window. It does **not** search all assets, all vaults, all chains, earlier prefunding or later returns, and retains the existing discovery-coverage limits. Rebalancing through another vault is therefore neither established nor excluded. Resolving it requires a receipt into an independently attributed custody address, with an evidenced asset conversion and amount/timing relationship, or authenticated exchange records.

## Evidence and remaining question

The subsequent [FDUSD follow-up](busd-fdusd-followup.md) records the Binance conversion timetable and tests known candidate addresses for a replacement-asset return. No such return was identified within its stated coverage; conversion inside the exchange remains unverified.

The [evidence package](../evidence/2026-10-11-busd-204k-trace/README.md) preserves discovery responses, direct requests and receipts, headers, pinned balances, archive-query failures and the explorer-label observation with an integrity manifest. The previous report's unresolved destination for this specific 204567.658073647823395109 BUSD transfer is now resolved to an exchange-labelled address. Its economic purpose and exchange-internal outcome remain unresolved; the [overall BUSD reconciliation](busd-case-assessment.md) remains incomplete.
