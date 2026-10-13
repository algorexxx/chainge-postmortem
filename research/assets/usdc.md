# USDC: partial investigation

Reviewed 2026-10-13. [Configuration](../../reconciliation/usdc.json), [capture inventory](../../evidence/2026-10-13-usdc-investigation/README.md). This extends the workflow trial into a retained Fusion ledger and source-chain investigation. It does **not** establish consolidated customer claims, complete custody, or a missing-asset amount. The site presents selected reviewed cases; the full retained-event reconciliation can be regenerated locally.

## Fusion accounting

Reference time: **2025-10-07 08:43:50 UTC**, [Fusion block 15130080](https://fsnex.net/block/15130080), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`. FSNEX and the previously retained node baseline share infrastructure. The first acquisition attempted the LAN listener, which intentionally does not accept this PC. The documented local SSH tunnel was subsequently restored and two transactions were corroborated directly; see the [access correction](../../evidence/2026-10-13-fusion-rpc-recovery/README.md). Bulk Fusion receipt import remains outstanding; the current generated ledger still uses explorer logs.

| Representation | Transfer events | Gross minted | Gross burned | Mint minus burn |
| --- | ---: | ---: | ---: | ---: |
| [Newer USDC](https://fsnex.net/address/0xfc9e57d7543e0591824121fda6461fe2148a5966) | 42,183 | 11,458,899.022430 | 4,397,871.503941 | 7,061,027.518489 |
| [Legacy full-time child](https://fsnex.net/address/0x7f0a22b5d7b3a9a52b83e955c1d7f127b3b9ee89) | 142,178 | 2,185,789,652.986973 | 1,186,689,889.469145 | 999,099,763.517828 |

These are six-decimal units, not USD valuations or consolidated liabilities. Histories run from creation through the reference block. Inclusive overlapping windows avoid the explorer's 10,000-row pagination ceiling; duplicate event identities are collapsed. Reconstructed holder balances are nonnegative. The newer net equals reported supply; its largest reconstructed holders agree with the retained first holder page.

The legacy net includes **996,416,657.619439** units at [address one](https://fsnex.net/address/0x0000000000000000000000000000000000000001). This is not a zero-address burn; economic treatment requires contract-semantics review. The [legacy parent](https://fsnex.net/address/0x6b52048a01c41d1625a6893c80fbe4aa2c22bb54) has a different reported supply counter. Neither counter is substituted for customer claims.

Four dated children have retained histories: [55be…70cf](https://fsnex.net/address/0x55be5afdbd9e9cb8be10385d02da6d7e56ec70cf), [9673…3a02](https://fsnex.net/address/0x9673123f25a3807cb6937865af7f74115d633a02), [dcc0…ca98](https://fsnex.net/address/0xdcc0978e684acc8ea1abc135bb7b3905b5dbca98), and [544b…9293](https://fsnex.net/address/0x544b0bb334eb34dd4e69d1722abd79fdecdf9293). Complementary time rights must not be added as independent full-time claims. Their child ERC-20 logs are retained separately and excluded from full-time matching.

The [first newer mint](https://fsnex.net/transaction/0x1baa6e0909589cac84be88ac1ecef3e91ae1aadf6d6b4b2b8e90d01ea5774ed7) has a legacy burn and newer mint of **328,973.737129** to the same holder in the explorer detail: representation conversion, not a fresh external deposit. The script does not relabel explorer details as node receipts or automatically exclude conversions without its required corroboration.

A [January 20, 2024 legacy transfer](https://fsnex.net/transaction/0x349d41aeb3198d652cf7758d5012ab48c258eb14c4e67c6fde5ba649dcad1cdf) placed **2,677,846.972295** at [7bc4…695e](https://fsnex.net/address/0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e). A [February 4 newer mint](https://fsnex.net/transaction/0xa7c577ee69d7329a46796b4abac3d3f3b616c4da178f5c8eb6bbbb0b885d07a8) issued exactly that amount to [8a85…4e84](https://fsnex.net/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84). An inventory/migration relationship is plausible, but different transactions and holders do not prove it. No liability adjustment is asserted.

## Candidate custody and balances

[Prior research](../busd-source-chain-flows.md) identifies the earlier candidates and preserves Avalanche router getters/admin transactions establishing later liquidity-address configuration. This supports investigation, not segregated custody for every token/chain. Equivalent Ethereum/BSC/Arbitrum roles remain candidates. Five addresses are explicitly included on Ethereum and Avalanche; internal-transfer classification depends on this candidate boundary and does not prove common human control.

Current pinned observations in token units; row links refer to Ethereum, while the site links each address to its respective chain:

| Candidate | Ethereum USDC | Avalanche USDC.e | Avalanche native USDC | BSC Binance-Peg USDC | Arbitrum USDC |
| --- | ---: | ---: | ---: | ---: | ---: |
| [874d…e290](https://etherscan.io/address/0x874de5761900ee526950d707d8cbd4b93092e290) | 0 | 0 | 0 | 0 | 0 |
| [4563…e039](https://etherscan.io/address/0x456325f2ac7067234dd71e01bebe032b0255e039) | 0 | 0 | 0 | 0 | 0 |
| [8a4a…64c1](https://etherscan.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1) | 0.014095 | 0 | 0.774669 | 0 | 0 |
| [3f0e…aa8e](https://etherscan.io/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e) | 0.014318 | 0 | 0 | 0 | 0 |
| [66ff…ecf1](https://etherscan.io/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) | 2,254.807763 | 8.035128 | 1.866669 | 254.382949325249263627 | 258.747601 |

Contracts: [Ethereum a0b8…eb48](https://etherscan.io/token/0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48), [Avalanche a7d7…c664](https://snowtrace.io/token/0xa7d7079b0fead91f3e65f86e8915cb59c1a4c664), [Avalanche b97e…8a6e](https://snowtrace.io/token/0xb97ef9ef8734c71904d8002f8b6bc66dd9c48a6e), [BSC 8ac7…580d](https://bscscan.com/token/0x8ac76a51cc950d9822d68b83fe1ad97b32cd580d), [Arbitrum af88…5831](https://arbiscan.io/token/0xaf88d065e77c8cc2239327c5edb3a432268e5831). Retained calls return six decimals except BSC's eighteen. Symbols do not establish interchangeable backing.

Pinned blocks on 2026-10-13: Ethereum **26182886** (10:11:23 UTC), Avalanche **97436077** (10:11:29), BSC **127384504** (10:11:31), Arbitrum **514397849** (10:14:17). Headers, hashes and exact call tags are in `extended-custody-*` and `arbitrum-*`. These later balances are not a reserve ratio against the October 2025 Fusion ledger.

At the reference time, Ethereum **23524761** (08:43:47 UTC) and Avalanche **69908002** (08:43:50 UTC) return **zero for both earlier candidates** for the listed Ethereum/Avalanche tokens. Adjacent headers bound the timestamp selection. Ethereum's first provider denied archive reads; a second returned balances with matching block hashes. Historical reads cover three addresses, not the complete candidate inventory. Other aligned balances and opening balances remain unresolved.

## Source coverage

| Scope | Discovery rows | Receipt corroboration |
| --- | ---: | --- |
| Earlier current candidate, Ethereum | 719 | All rows match 711 successful receipts; 691 headers |
| Earlier current candidate, Avalanche USDC.e | 2,131 | All rows match 2,106 successful receipts; 2,105 headers |
| January recipient, Ethereum | 763 | All rows match 720 successful receipts; 717 headers |
| January recipient, Avalanche USDC.e | 43 | All rows match 41 successful receipts; 41 headers |
| Later 3f0e… / 66ff… candidates | Ethereum 31 / 715; Avalanche USDC.e 0 / 3; native USDC 10 / 76 | Selected largest/latest only: 18 Ethereum and 18 Avalanche receipts |

Checks compare token, sender, recipient and integer amount with receipt logs. This verifies returned events, not absent events. Routescan's incomplete Ethereum discovery is demonstrated in the BUSD control query. Empty predecessor results are not accepted as no history; receipt context itself reveals predecessor activity. BSC discovery through Routescan was unsupported. BSC/Arbitrum currently have balance probes rather than full histories. No universal USDC chain/address census is claimed.

## Reviewed movements

### January 2024 transition candidate

On January 20, [52,464.903010 Ethereum USDC](https://etherscan.io/tx/0x3d04f41ab5fbdc1149803dd9e14840f650a7e718e82ab98465cbe8328278f8f4) and [21,356.767181 Avalanche USDC.e](https://snowtrace.io/tx/0x67363ba6866f53e21d9b4da88ff75f03c6ae9aca474d851ff510e2fbf2e0bed5) moved from the earlier current candidate to [8a4a…64c1](https://etherscan.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1). Both receipts succeed. Later Avalanche liquidity configuration and continued two-way activity support a possible operational transition, without proving common control or a contemporaneous migration instruction.

The recipient did not retain either amount intact through the reference time: its Ethereum USDC balance was **0.014095**, Avalanche USDC.e **zero**. Returned histories have matching net changes, but mixed funds prevent assigning each later payment to this funding. No CEX endpoint is established for these USDC paths.

### September asset exchange

In [transaction 3e0a…217e](https://etherscan.io/tx/0x3e0a6a69ce57f3706916f04a050f2a1d53c13353d397d3c62f28e5639905217e), **274,999 USDC** left the latest candidate via [4c5f…85a5](https://etherscan.io/address/0x4c5f53015f3adb1b1d15ddf4e17edaae6fa185a5). The same receipt records the USDC route and **272,239.479401 USDT** returning through contracts, then **272,074.135714 USDT** to [5fcb…b03b](https://etherscan.io/address/0x5fcb0eb5958383136768675f9ac593289de9b03b) and **165.343687 USDT** to the latest candidate. The latter looks fee-like; its economic purpose is not authenticated. This establishes an asset-changing route, not customer authorization or backing. Current balances and ultimate disposition of those USDT outputs remain unestablished.

### A near-amount match rejected

The [416,050.404450-USDC Ethereum payment](https://etherscan.io/tx/0x626d2b4a805005e7556d0180233f3b2913c4d6f185a5fd7a91b6557c17e0392e) to [584d…fea6](https://etherscan.io/address/0x584de4fe0aea85fa45f5c4d7f50e09deeeecfea6) follows a [416,302-unit Fusion burn](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802) by **1,687 seconds**. But the burn's explorer-retained input requests **Arbitrum**, af88…5831, [35cb…e3f](https://arbiscan.io/address/0x35cba6ab1f851cdbf9accf17aa7fd98679bf3e3f), and raw output **416052118806**. Chain, recipient and amount differ. Direct settlement is not accepted; an indirect relationship remains unproven.

Another [100,000-unit burn](https://fsnex.net/transaction/0xa86a558cafb8b11eab004832fad2f69e4d60bf3e45d23c7a3fb72e7f96a1e27f) explicitly names BNB and the BSC contract. Ethereum/Avalanche alone therefore cannot resolve all requested settlements.

## Reproduction and completion gates

Run `./reconcile.ps1 -Asset usdc` and `./verify.ps1`. Accounting, movements, discovery gaps and burn review remain in ignored `archives/`. Five-minute equal-amount matching, two-minute clock tolerance and a 10,000-token triage threshold remain hypotheses. BUSD fee and inventory exclusions are not inherited. The public site uses reviewed cases, not the unfinished automated queue.

The expanded dataset reuses the engine without relaxing thresholds to reduce unmatched counts. Missing Fusion receipt context leaves conversions and burn settlement unresolved; it does not show that every unresolved operation was abnormal.

Fusion receipt/transaction access is now working through the documented tunnel. Completion requires bulk receipt/transaction acquisition and import, migration and time-slice review, address-one treatment, predecessor discovery, BSC/Arbitrum and other requested chains, complete later-candidate receipts, opening/closing balance checks, terminal/output tracing, dated custody roles and consolidated claims without double-counting. Beneficial owners and intent are not established by these observations.
