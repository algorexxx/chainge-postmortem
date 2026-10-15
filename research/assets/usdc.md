# USDC: partial investigation

Reviewed 2026-10-15. Earlier acquisition tables and event counts below describe the October 13 checkpoint unless a later date is stated. [Configuration](../../reconciliation/usdc.json), [capture inventory](../../evidence/2026-10-13-usdc-investigation/README.md). This extends the workflow trial into a retained Fusion ledger and source-chain investigation. It does **not** establish consolidated customer claims, complete custody, or a missing-asset amount. The site presents selected reviewed cases; the full retained-event reconciliation can be regenerated locally.

## Current closing pass

The [October 15 assessment](../closeout-assessment.md) is the current statement of completed work and remaining limits. Anonymous queries now cover the configured Ethereum and Polygon token/holder history targets; recovered Polygon events have receipt/header corroboration and material cases have transaction-input and bounded recipient follow-ups. BSC probes recover additional transfers but do not close all state/replay disagreements; BSC and Base retain history coverage gaps. A failed query is not zero activity.

The [event-ID-keyed closing register](usdc-closeout.json) records findings, limits and evidence references. Use the command below from the repository root to derive the current selection; new evidence can change its size. Reviewed cases are not automatically accepted redemptions. The full retained movement list remains available through the common UI.

```powershell
./reconcile.ps1 -Asset usdc
./research/review-closeout.ps1 -Asset usdc -RequireReviewed
```

See [accounting treatment](../fusion-claims-review.md#current-accounting-treatment-october-15-closing-pass) for migration rights, blocked balances, address-one and vault inventory. The Rollux balance remains separate because its backing is unverified. Consolidated customer claims, full vault discovery and accepted settlement of material exceptions remain unestablished.

## Fusion accounting

The [2026-10-15 Rollux origin review](../usdc-rollux-roundtrip.md) links a large Fusion mint back through Rollux and Syscoin to an earlier Fusion transfer. This strengthens the reason to keep Rollux separate from verified backing; it changes no reported balances or customer-claim assumptions.

Reference time: **2025-10-07 08:43:50 UTC**, [Fusion block 15130080](https://fsnex.net/block/15130080), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`. FSNEX and the previously retained node baseline share infrastructure. The first acquisition attempted the LAN listener, which intentionally does not accept this PC. The documented local SSH tunnel was subsequently restored and two transactions were corroborated directly; see the [access correction](../../evidence/2026-10-13-fusion-rpc-recovery/README.md). The [bulk receipt package](../../evidence/2026-10-13-usdc-fusion-receipts/README.md) now corroborates all 45,544 observed mint/burn events in 45,149 transactions, with 43,345 block headers. Ordinary transfers outside those transactions still use explorer logs.

| Representation | Transfer events | Gross minted | Gross burned | Mint minus burn |
| --- | ---: | ---: | ---: | ---: |
| [Newer USDC](https://fsnex.net/address/0xfc9e57d7543e0591824121fda6461fe2148a5966) | 42,183 | 11,458,899.022430 | 4,397,871.503941 | 7,061,027.518489 |
| [Legacy full-time child](https://fsnex.net/address/0x7f0a22b5d7b3a9a52b83e955c1d7f127b3b9ee89) | 142,178 | 2,185,789,652.986973 | 1,186,689,889.469145 | 999,099,763.517828 |

These are six-decimal units, not USD valuations or consolidated liabilities. Histories run from creation through the reference block. Inclusive overlapping windows avoid the explorer's 10,000-row pagination ceiling; duplicate event identities are collapsed. Reconstructed holder balances are nonnegative. The newer net equals reported supply. All **529 positive holder positions** across the newer, legacy and four dated children now agree exactly with pinned node `balanceOf` calls. The [holder-check package](../../evidence/2026-10-13-usdc-fusion-claims/) preserves those calls. This validates token positions, not beneficial ownership or consolidated customer claims.

The legacy net includes **996,416,657.619439** units at [address one](https://fsnex.net/address/0x0000000000000000000000000000000000000001). This is not a zero-address burn; economic treatment requires contract-semantics review. The [legacy parent](https://fsnex.net/address/0x6b52048a01c41d1625a6893c80fbe4aa2c22bb54) has a different reported supply counter. Neither counter is substituted for customer claims.

The preserved official parent source now has a [compiled runtime comparison](../../evidence/2026-10-13-usdc-fusion-source-match/README.md): Solidity 0.8.12 with optimization at 200 runs reproduces all **15,419 bytes** except two 32-byte IPFS metadata digests. This supports the source-level finding that the parent increments its supply counter on mint but does not decrement that counter when burning the full-time child. Splitting and merging dated rights also changes child supplies without creating independent full-time claims. The matched source has no special address-one transfer/burn branch. Constructor arguments, ownership, redemption terms and the economic treatment of address-one or duplicate migration balances remain separate questions; this is not an exact metadata match or a consolidated liability figure.

Four dated children have retained histories: [55be…70cf](https://fsnex.net/address/0x55be5afdbd9e9cb8be10385d02da6d7e56ec70cf), [9673…3a02](https://fsnex.net/address/0x9673123f25a3807cb6937865af7f74115d633a02), [dcc0…ca98](https://fsnex.net/address/0xdcc0978e684acc8ea1abc135bb7b3905b5dbca98), and [544b…9293](https://fsnex.net/address/0x544b0bb334eb34dd4e69d1722abd79fdecdf9293). Complementary time rights must not be added as independent full-time claims. Their child ERC-20 logs are retained separately and excluded from full-time matching.

All five legacy children also [match the compiled `Slice` runtime](../../evidence/2026-10-13-usdc-fusion-child-code/README.md) apart from its metadata digest. Their deployed runtime bytes are identical; their interval and balance state is not. The newer token has different bytecode and remains outside this source match.

The [first newer mint](https://fsnex.net/transaction/0x1baa6e0909589cac84be88ac1ecef3e91ae1aadf6d6b4b2b8e90d01ea5774ed7) has a legacy burn and newer mint of **328,973.737129** to the same holder, now corroborated by the raw node receipt: representation conversion, not a fresh external deposit. The script excludes both issuance events from external matching and retains both in the ledger.

A [January 20, 2024 legacy transfer](https://fsnex.net/transaction/0x349d41aeb3198d652cf7758d5012ab48c258eb14c4e67c6fde5ba649dcad1cdf) placed **2,677,846.972295** at [7bc4…695e](https://fsnex.net/address/0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e). A [February 4 newer mint](https://fsnex.net/transaction/0xa7c577ee69d7329a46796b4abac3d3f3b616c4da178f5c8eb6bbbb0b885d07a8) issued exactly that amount to [8a85…4e84](https://fsnex.net/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84). An inventory/migration relationship is plausible, but different transactions and holders do not prove it. No liability adjustment is asserted.

## Candidate custody and balances

[Prior research](../busd-source-chain-flows.md) identifies the earlier candidates and preserves Avalanche router getters/admin transactions establishing later liquidity-address configuration. This supports investigation, not segregated custody for every token/chain. Equivalent Ethereum/BSC/Arbitrum roles remain candidates. Five addresses are explicitly investigated across the configured source chains; internal-transfer classification depends on this candidate boundary and does not prove common human control.

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

### Balances aligned to the Fusion reference time

Historical calls now cover all five candidates for Ethereum, Avalanche, BSC, Polygon, Optimism, Base, Aurora and Rollux. Arbitrum positions at the same cutoff are reconstructed from bounded log history; all ten later pinned token/address balances match that replay. A further anonymous BSC endpoint supplied the missing historical state after six listed endpoints failed. These observations must not be mixed with the later-date table above to calculate a reserve ratio.

| Chain / representation | Cutoff block | Nonzero candidate positions (token units) |
| --- | ---: | --- |
| Ethereum USDC | 23524761 | 8a4a: 0.014095; 3f0e: 0.014318; 66ff: 2,254.807663 |
| Avalanche USDC.e | 69908002 | 66ff: 8.035128 |
| Avalanche native USDC | 69908002 | 8a4a: 0.774669; 66ff: 1.866669 |
| Arbitrum native USDC | 386963544 | 66ff: 258.747601 |
| Arbitrum USDC.e | 386963544 | 4563: 0.002815; 66ff: 5.600507 |
| Polygon native USDC | 77364542 | 66ff: 249.339821 |
| Polygon USDC.e | 77364542 | 66ff: 45.370745 |
| Optimism native USDC | 142113926 | 8a4a: 0.003495; 66ff: 0.800343 |
| Optimism USDC.e | 142113926 | 4563: 0.003933; 66ff: 111.037397 |
| Base USDC | 36518641 | All five queried candidates: zero |
| Aurora USDC-labelled token | 167240245 | 8a4a: 0.131653; 66ff: 0.793990 |
| Rollux USDC-labelled token | 36230694 | 66ff: 2,284,499.383095; backing unresolved |
| BSC Binance-Peg USDC | 63769742 | 66ff: 254.382949325249263627; other four candidates: zero |

Exact contracts, addresses, block hashes and adjacent timestamp headers are retained in the [common-cutoff](../../evidence/2026-10-13-usdc-common-cutoff/), [first retry](../../evidence/2026-10-13-usdc-common-cutoff-retry/), [second retry](../../evidence/2026-10-13-usdc-common-cutoff-retry2/) [range-repair](../../evidence/2026-10-13-usdc-range-repair/) and [BSC historical-state](../../evidence/2026-10-13-usdc-bsc-history/) packages. Addresses in this table abbreviate the candidates linked above. Zero applies only to an actually queried position; no other-chain zero is inferred.

The large [Rollux balance](https://explorer.rollux.com/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) is particularly important to qualify. The [token](https://explorer.rollux.com/address/0x368433cac2a0b8d76e64681a9835502a1f2a8a30) identifies [a Syscoin parent](https://explorer.syscoin.org/address/0x3fca6743e2fb55759fee767f3a68b2c06d699dc4) and the [standard bridge](https://explorer.rollux.com/address/0x4200000000000000000000000000000000000010). The parent is unverified in the retained explorer response; its backing has not been established. **Do not count 2.284M independently backed dollars from the token name or balance.** See [contract calls](../../evidence/2026-10-13-usdc-pinned-followup/) and [parent metadata](../../evidence/2026-10-13-usdc-cctp-followup/).

## Source coverage

The engine now checks saved `eth_getLogs` requests and responses against explicit token/address/direction/range targets. `history-coverage.json` lists every missing inclusive block interval. Failed, unpinned, removed-log or counterparty-constrained queries do not establish complete address history. Successful provider responses still require state checks and are not independent proof against provider omissions.

Both directions now have bounded coverage for all five candidates on **Arbitrum (native and bridged), Avalanche (native and bridged), Optimism (native and bridged), Aurora and Rollux**. Newly recovered Avalanche and Optimism logs close the earlier reference-time discrepancies: replay for the 8a4a candidate now matches **0.774669 Avalanche USDC** and **0.003495 Optimism USDC**. The later Optimism outgoing-range gap is also closed. Arbitrum replay matches all ten pinned current balances; Aurora and Rollux also match the reference-time states. Completed provider ranges remain distinct from independent completeness and receipt verification.

The reusable collector completed **1,515 Arbitrum receipt batches**, **1,123 Optimism log/receipt batches**, and **124 Avalanche data requests**. A second plan found one additional Arbitrum transaction from downstream discovery; its receipt is now retained. Regenerating the plans then returns **zero remaining requests for Arbitrum, Avalanche and Optimism**, within the configured contracts, addresses and historical cutoffs. This does not include unknown custody addresses or later blocks. The coverage reader binds unordered batched log replies by typed RPC ID and keeps failed or missing members separate. Evidence: [Arbitrum](../../evidence/2026-10-13-usdc-arbitrum-capture/), [second-pass receipt](../../evidence/2026-10-13-usdc-arbitrum-last-receipt/), [Avalanche](../../evidence/2026-10-13-usdc-avalanche-capture/), and [Optimism](../../evidence/2026-10-13-usdc-optimism-batched/).

Ethereum has expanded receipt corroboration but incomplete bounded RPC discovery; Avalanche now has complete configured range coverage. A second anonymous Ethereum explorer returned all paginated pages for five candidates: 0, 2,960, 763, 31 and 717 rows respectively. All five cutoff net positions match node balance checks. For the earlier candidate this substantially exceeds the original 719-row discovery set. The [raw pages](../../evidence/2026-10-13-usdc-ethereum-pagination/) are now imported as unverified events, deduplicated against receipts and rejected on conflicting amounts; matching net balances cannot rule out cancelling omissions. BSC, Polygon and Base remain incomplete histories. Public endpoints reject some historical queries, restrict ranges, or rate-limit batches; these failures are retained. Empty predecessor explorer results do not establish no activity. There is no universal USDC chain/address census. Acquisition uses anonymous free sources only.

Raw source logs and successful receipts are distinct evidence levels. The raw-log expansion does not silently mark every transaction receipt-verified. `source-positions-<chain>.json` pairs observed net positions with coverage gaps; it must not be read as a certified reserve statement. `balance-checks.json` now automatically binds saved single/batched `balanceOf` calls by response ID and compares them with replay at the exact requested block. The run includes 604 observations, including all 529 Fusion positions. Missing state, net differences, negative-history flags and incomplete query coverage have distinct statuses. A matching balance with missing history is never marked covered.

### BSC terminal window

The public BscScan table displayed **949 transfers** for the latest candidate. All ten pages' block references are [preserved as discovery](../../evidence/2026-10-13-usdc-bsc-discovery/), but only selected windows have node corroboration. Five-request log batches exceeded the anonymous endpoint's compute allowance; ten individual terminal queries succeeded and returned **30 transfers**, all now checked against [receipts and headers](../../evidence/2026-10-13-usdc-bsc-terminal-context/).

The [opening balance](../../evidence/2026-10-13-usdc-bsc-terminal-balances/) at block 48598559 was **0.015477097849263627 USDC**. The recovered window has **4,458.40726 incoming** and **4,204.0397877726 outgoing**. Their change leads exactly to the independently observed **254.382949325249263627 USDC** at the common cutoff. The last displayed outflow is [10.2910589534 USDC on May 19, 2025](https://bscscan.com/tx/0xdb2e1519a3c1a89cc5881f980e6479cd1d6f7ce399495e29c39cae358dbae018), leaving a node-observed **0.629419325249263627 USDC** at that block. A later [253.75353-USDC deposit on July 5](https://bscscan.com/tx/0x2bc5d0d44349f9b06740dca168ce6fb33b885cc8785aaef7fc387440cb9cd8e0) produces the remaining balance. The largest outgoing transfer in these 30 rows is [1,680 USDC](https://bscscan.com/tx/0x0aaf75e1e8a7fae69ae3795c6652dee97bddd42222c7f99e34b5d54a8f04a306).

The new `balance-intervals.json` verifies these changes automatically from consecutive pinned states, excluding the opening block and including the closing block. The BSC intervals remain **net-change-agrees-coverage-incomplete**: matching the observed net change cannot exclude cancelling omissions, certify earlier history or explain the payments' purpose. The full-history BSC replay still has gaps and must not be interpreted as a negative actual balance.

## Reviewed movements

### January 2024 transition candidate

On January 20, [52,464.903010 Ethereum USDC](https://etherscan.io/tx/0x3d04f41ab5fbdc1149803dd9e14840f650a7e718e82ab98465cbe8328278f8f4) and [21,356.767181 Avalanche USDC.e](https://snowtrace.io/tx/0x67363ba6866f53e21d9b4da88ff75f03c6ae9aca474d851ff510e2fbf2e0bed5) moved from the earlier current candidate to [8a4a…64c1](https://etherscan.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1). Both receipts succeed. Later Avalanche liquidity configuration and continued two-way activity support a possible operational transition, without proving common control or a contemporaneous migration instruction.

The recipient did not retain either amount intact through the reference time: its Ethereum USDC balance was **0.014095**, Avalanche USDC.e **zero**. Returned histories have matching net changes, but mixed funds prevent assigning each later payment to this funding. No CEX endpoint is established for these USDC paths.

### September asset exchange

In [transaction 3e0a…217e](https://etherscan.io/tx/0x3e0a6a69ce57f3706916f04a050f2a1d53c13353d397d3c62f28e5639905217e), **274,999 USDC** left the latest candidate via [4c5f…85a5](https://etherscan.io/address/0x4c5f53015f3adb1b1d15ddf4e17edaae6fa185a5). The same receipt records the USDC route and **272,239.479401 USDT** returning through contracts, then **272,074.135714 USDT** to [5fcb…b03b](https://etherscan.io/address/0x5fcb0eb5958383136768675f9ac593289de9b03b) and **165.343687 USDT** to the latest candidate. The latter looks fee-like; its economic purpose is not authenticated. This establishes an asset-changing route, not customer authorization or backing. The main USDT recipient subsequently paid **177,100 USDT in four transfers** to [a389…e623](https://etherscan.io/address/0xa3895eeeb52a04002d58214d46ee5c5a26fce623), and returned **46,100 USDT** to the latest candidate: [25,100](https://etherscan.io/tx/0x77c334cf423d12082d220fb0ccc2451fbd941536cd474c3d21f0b60e9fba77a8) and [21,000](https://etherscan.io/tx/0x071df403d74b638ca71a51ed82f4b1823ae3860808ba6300abce32029ab25de1). Its later pinned balance is **0.166778 USDT**. All 29 returned discovery rows match successful receipt logs in the [output-receipt package](../../evidence/2026-10-13-usdc-output-receipts/). This demonstrates distribution, not ownership or purpose; later unrelated funding prevents assigning every subsequent payment to the original output. Returned explorer pages are not independently certified full history.

The [Arbitrum follow-up](../usdc-hyperliquid-trace.md) now supplies earlier funding context: the same recipient returned **274,999 USDC to the Arbitrum candidate** 79 minutes 53 seconds before the Ethereum exchange, using a memo naming Ethereum USDT and itself as recipient. This supports a cross-chain exchange interpretation; memo output-related values and actual USDT differ slightly, so an exact authenticated settlement is not claimed.

### A near-amount match rejected

The [416,050.404450-USDC Ethereum payment](https://etherscan.io/tx/0x626d2b4a805005e7556d0180233f3b2913c4d6f185a5fd7a91b6557c17e0392e) to [584d…fea6](https://etherscan.io/address/0x584de4fe0aea85fa45f5c4d7f50e09deeeecfea6) follows a [416,302-unit Fusion burn](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802) by **1,687 seconds**. But the burn's node-corroborated input requests **Arbitrum**, af88…5831, [35cb…e3f](https://arbiscan.io/address/0x35cba6ab1f851cdbf9accf17aa7fd98679bf3e3f), and raw output **416052118806**. Chain, recipient and amount differ. Direct settlement of that Ethereum transfer is not accepted. A subsequent targeted check found the exact memo-named Arbitrum payout, described below; an indirect relationship to the separate Ethereum payment remains unproven.

### Where the separate Ethereum payment went

The 416,050.404450-USDC Ethereum recipient split the observed amount into two branches:

- **100,000 USDC** moved to [a4b6…8673](https://etherscan.io/address/0xa4b6aa5f955f3a944cca20e65b22280a47cc8673) on September 2, 2024 ([receipt](https://etherscan.io/tx/0xaefaa10bc8b2f9a19ce384847772da89b8788a28d3a91c91445e2eb072c15dd5)), then to [4044…2e32](https://etherscan.io/address/0x404460039499c774c48248552f802ce5dd482e32) on September 4 ([receipt](https://etherscan.io/tx/0xe09bd19377ec43944066120535e00f8dac552fa6c37937694390a41b1705e474)). Saved Etherscan HTML labels the latter **Netcoins 1**. This supports an exchange-labelled destination, not the credited account or beneficial owner.
- **316,050.404450 USDC** entered a Circle CCTP route on September 4 ([Ethereum receipt](https://etherscan.io/tx/0xd292b5ec3edc809ae826ad69db887862b9a6459fd64750c03e6c00480a7f48ac)). It burned **316,050.304450 USDC**, with a 0.1-USDC difference, and [minted 316,050.304450 native USDC on Arbitrum](https://arbiscan.io/tx/0xc7661ff75f5b8c9db1209aabd4957441674688128ee8e7042ee60cefec588e6a) to the same recipient. Source domain 0, destination domain 3, nonce **104217** and the byte-identical 132-byte message body link the successful source and destination receipts. The destination timestamp is September 4, 2024 02:57:02 UTC. This is verified protocol-level bridging. The recipient subsequently deposited **316,050.300000 USDC into Hyperliquid**; its public venue ledger records the identical transaction, later withdrawals and further deposits. The captured account states and pinned Arbitrum balance are zero. See the [downstream trace](../usdc-hyperliquid-trace.md) for receipts, the available trading history, later recipients and limitations.

Evidence: [initial receipts](../../evidence/2026-10-13-usdc-terminal-get/), [CCTP destination](../../evidence/2026-10-13-usdc-cctp-followup/), [output discovery and official Circle documentation](../../evidence/2026-10-13-usdc-output-discovery/), and [saved explorer label / direct node receipt](../../evidence/2026-10-13-usdc-trace-sources/). These two branches account for the observed original recipient outflows; they do not authenticate a customer redemption or explain custody authorization.

Another [100,000-unit burn](https://fsnex.net/transaction/0xa86a558cafb8b11eab004832fad2f69e4d60bf3e45d23c7a3fb72e7f96a1e27f) explicitly names BNB and the BSC contract. Ethereum/Avalanche alone therefore cannot resolve all requested settlements. The subsequent [BSC check](../../evidence/2026-10-13-usdc-bsc-routing/) found a successful [99,939.699936610000000000-USDC payment](https://bscscan.com/tx/0xb2e35fc2efdffb5008bdef6d3e2f2fb72d6432fac9d447850093ea1be371be0e) from [66ff…ecf1](https://bscscan.com/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) to the exact memo recipient [4408…0a80](https://bscscan.com/address/0x440872cfddac749d9f201234395fc20ed5480a80), **44 seconds after the Fusion burn**. Chain, token, recipient and raw memo output all match. This is a strong payout candidate; the 60.30006339-unit difference is not an authenticated fee rule, and a targeted recipient query does not establish full BSC vault history.

## Reproduction and completion gates

Run `./reconcile.ps1 -Asset usdc` and `./verify.ps1`. Accounting, discovery gaps and burn review remain in ignored `archives/`; the reproducible public movement ledger is `registry/usdc-movements.json`. Five-minute equal-amount matching, two-minute clock tolerance and a 10,000-token triage threshold remain hypotheses. BUSD fee and inventory exclusions are not inherited. The public site starts with reviewed cases and offers the full retained movement queue on demand, with provisional classifications and coverage limitations.

The expanded dataset reuses the engine without relaxing thresholds. All 46,309 explorer events encountered in the acquired receipts match their node logs and header timestamps; 45,544 are mint/burn events. Total events and gross/net issuance remain unchanged.

| Fusion purpose after receipt import | Events |
| --- | ---: |
| Direct mint call | 35,785 |
| Direct burn call | 6,473 |
| Cross-representation mint/burn | 790 |
| Receipt-backed slice conversion | 576 |
| Contract purpose still unresolved | 1,920 |

The **1,366 conversion-related events** remain in accounting but are excluded from external matching. This includes 395 transactions that each burn legacy full-time USDC and mint newer USDC. Three additional newer mints burn the known legacy remaining-time child [9673…3a02](https://fsnex.net/address/0x9673123f25a3807cb6937865af7f74115d633a02) in the same receipt, for the same holder and integer amount: [6313…5dc3](https://fsnex.net/transaction/0x6313eafcf11509389d7da88c8d039a95ea035e924b0d06180d9ed689b8d55dc3), [9767…0a98](https://fsnex.net/transaction/0x9767161b2c09a32884f7090bfd76791dddd19b63e3833b7c75fa9bdbb4ee0a98), and [a4d4…78f4](https://fsnex.net/transaction/0xa4d4388c0df0b3671927f72e72522423aca36c0a3e416ac436b32233b30e78f4). The configuration now supplies this observed slice counterpart for the newer token. This identifies conversion mechanics, not economic equivalence of every dated claim.

The expanded retained dataset contains **277,800 events and 35,112 operating-address movements**. There are **35,190 unpaired Fusion mints and 7,270 unpaired burns** after conversion exclusions. These counts are not missing funds or liabilities. Receipt import also retains relevant token logs for counterparties within those transactions, so total retained events are not a count of vault movements. The much larger source ledger contains normal trading and incomplete address histories; counts cannot be compared with the earlier two-chain sample as though the scope were unchanged.

Completion still requires consolidated treatment of address one, migrations and complementary time rights; justified custody roles and complete source-chain inventory; missing histories and state checks; remaining receipt verification; and downstream follow-up of material terminal cases. The current dataset is a reproducible **partial investigation**, not a completed reconciliation. No deficit is asserted.

## Routing-input review

`routing-review.json` is now generated for every asset. Configuration selects routing targets, chain-code mappings and a separate seven-day candidate-review window. The first decoder validates successful receipt context, full-time burn, caller/holder, token, amount, observed mode and canonical ABI offset/length/padding before interpreting colon or supported flat JSON memos. It does not scan arbitrary calldata for a matching string. The [interface checks](../../evidence/2026-10-13-routing-interface/README.md) found no verified router ABI, so these remain observations of an empirically established layout.

USDC now decodes **797 observed memos: 678 colon and 119 JSON**, including colon-containing Kaspa recipients. JSON token indices remain indices; without an authenticated index-to-contract mapping they cannot create an exact-token settlement candidate. Unsupported, malformed and unbound calls remain visible. Some requests name other output assets, so amount proximity alone cannot make them same-token redemptions.

There are **58 exact chain/token/recipient/memo-amount candidates** in the expanded retained receipt evidence. They remain separate review edges, with no automatic promotion into settled accounting. Three illustrative cases:

| Fusion burn | Candidate payment | Burned USDC | Paid USDC | Delay |
| --- | --- | ---: | ---: | ---: |
| [48dc…8354](https://fsnex.net/transaction/0x48dc81e4e03d868a3b0b1ba7fa07064a67defd8fef948df4b85e81855fb28354) | [Ethereum ff9c…fa3d](https://etherscan.io/tx/0xff9ce2008d73b3bc2187acd9407871ed301e16932c16758275c071e48cfafa3d) | 8,166.613549 | 8,159.713408 | 87 seconds |
| [f7fe…c463](https://fsnex.net/transaction/0xf7fea78db3d3f74737f7ba834560c0c3e7b73405805d10ab0c9227f83de3c463) | [Ethereum 4a7a…a540](https://etherscan.io/tx/0x4a7ae286a7dd868742339dd049c8d77e77cec053a706b498b43aef0ac1a2a540) | 76,600 | 76,552.039816 | 93 seconds |
| [9a10…d802](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802) | [Arbitrum 564a…447e](https://arbiscan.io/tx/0x564a63fa0898a580f15770865d17e7dedf75af222f93c9e71f31773658e2447e) | 416,302 | 416,052.118806 | 79 seconds |

The first two payments came from [8a4a…64c1](https://etherscan.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1); the Arbitrum payment came from [3f0e…aa8e](https://arbiscan.io/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e). Its [targeted evidence package](../../evidence/2026-10-13-usdc-arbitrum-routing/README.md) retains the token/recipient log query, receipt and timestamp header. This strongly supports a payout relationship for the 416,302-unit burn and is distinct from the earlier Ethereum payment.

These candidate links do not establish a shared bridge identifier, authenticated fee rule or full source-chain coverage. They are reported separately and do not remove burns or source outflows from normal queues. The expanded Arbitrum history is retained separately from this targeted payment. No address role is established solely by a payout, and no exact memo candidate automatically removes a source outflow or Fusion burn from the normal queues.

## Large late-outflow follow-up

The [terminal review](../usdc-terminal-review.md) documents a near-exact routing candidate for the 69,955.999671-USDC September payment, and receipt-confirmed downstream distribution of the 17,000-USDC May 2025 payment. Later combined funds on both paths reach the Binance-14-labelled address; the larger mixed-funds branch of the May distribution is now traced to a pinned 18,150-USDC retained balance, with ownership unresolved. The reviewed possible-redemption label does not change strict automated classifications. The report also identifies the incoming funding immediately before the 416k payment and its separate matching Fusion issuance; that funding does not resolve the outgoing payment's purpose.
The [Arbitrum, Optimism and Avalanche follow-up](../usdc-arbitrum-optimism-avalanche.md) assesses eight further outflows: five possible same-holder burn counterparts, a native-USDC to USDC.e conversion, a full 35,993.090970-USDC transfer to an Avalanche Binance-labelled address, and a mixed-balance conversion to BTC.b after the 21,774.015699-USDC payment. These are reviewed cases, not changes to strict automated matching or proof of a consolidated deficit.

## Holder inventory and announcement context

See [Fusion holder review](../fusion-claims-review.md) for checked positive positions, address-one separation, unresolved migration overlap and outgoing-transfer timing against the reviewed Chainge statements. Consolidated customer liabilities remain unestablished.

## Post-snapshot follow-up

The [three-case review](../usdc-post-snapshot-outflows.md) identifies new funding before each late USDC payment, two swaps returning ETH to the same vault, and the full January payment reaching an OKX-labelled endpoint. See [snapshot provenance](../fusion-snapshot-provenance.md) for why later Fusion settlement remains unknown.

## Migration semantics follow-up

The [contract and state review](../fusion-migration-semantics.md) confirms coordinated BUSD/USDC blocking, matches both legacy runtimes to source, and checks all positive legacy-holder flags. Blocked balances remain recorded and reversible; no additional overlap deduction is applied. The reviewed UI separates the blocked subset from other balances.
