# BUSD source-chain flows

Observed 2026-10-11. This report distinguishes token transfers, contract relationships and explorer labels from custody attribution. A matching address on two chains does not establish a reserve relationship. A configured Avalanche liquidity address is verified below. This does not establish segregated reserves or a complete custody inventory.

## BSC controller-address history

The BSC address `0x456325f2ac7067234dd71e01bebe032b0255e039` matches a historically authorized controller of legacy Fusion BUSD. Its returned Binance-Peg BUSD history contains **3,564 Transfer events in 3,533 transactions**, including **141 zero-value events**. All events have successful node receipts. The 3,423 nonzero events match the count displayed by BscScan on the observation date.

The initial CSV contained 2,800 rows and stopped on 2023-03-18. It was incomplete despite the requested wider date range. Every CSV row was matched to a node receipt by transaction, token, sender, recipient and exact raw amount. The newer portion was located through the rendered explorer table, recovered using block receipts, and then preserved again through direct transaction-receipt requests. Deduplication uses transaction hash and log index.

Incoming and outgoing sums are both **5627912.884153641388215982 BUSD**. The running reconstructed balance never becomes negative and finishes at zero, matching the pinned state query. The largest reconstructed balance after an event is **391322.294825617360416333 BUSD**. These are turnover and balance observations, not estimates of customer claims, reserves, losses or unique deposits. Offsetting missing events could still evade a balance check.

The returned history begins at block 19,478,892 on 2022-07-12 and ends at block 35,490,317 on 2024-01-23. The first event credits four raw units from `0x874de5761900ee526950d707d8cbd4b93092e290`. Total incoming BUSD from that predecessor address is **81412.125049046742042887**. Both address strings also occur as legacy Fusion controllers. This is evidence of a cross-chain operational relationship, not proof that either address held segregated reserves.

## Strong candidate deposit and redemption pairs

Two January 2022 sequences connect the earlier Fusion BUSD representation to source-chain BUSD. Node transaction inputs, successful receipts and block timestamps establish the individual legs:

| UTC | Chain | Observed action | Transaction |
| --- | --- | --- | --- |
| 2022-01-01 17:20:49 | BSC | `0xda832…cA2c` transfers **1200000 BUSD** to `0x874d…e290` | `0xcaf80257a37f91406ad022425245602b05e6911a5405b56cece6fd2421aeff66` |
| 2022-01-01 17:21:01 | Fusion | `0x874d…e290` calls `mint(address,uint256)` on earlier BUSD, crediting **1200000** to `0xda832…cA2c` | `0xfc0c466027c0cefd8844fe77362faec0a2fdaf94ff244d36b2cd80bb3417b778` |
| 2022-01-11 09:03:50 | Fusion | `0x874d…e290` calls `burn(address,uint256)`, debiting **300000.5** from `0xda832…cA2c` | `0xbf1d587aa11f50dec81c547d39de0e472ee366a60cd53f2f29e258031d625c0d` |
| 2022-01-11 09:04:19 | BSC | `0x874d…e290` transfers **300000 BUSD** to `0xda832…cA2c` | `0x7365f0385ecd84d954b60ffe6a1124dccf01c6ce64454abd2dec9cd91aeaadef` |

The first pair is separated by **12 seconds**, with the same holder and exact amount. The second is separated by **29 seconds**, with the same holder and a **0.5-unit difference**. In both, the BSC receiving/paying address is also the Fusion mint/burn transaction sender. This is substantially stronger evidence than a symbol or balance match.

**Inference:** these are highly consistent with a BSC deposit/Fusion issuance and a Fusion redemption/BSC release operated through `0x874de5761900ee526950d707d8cbd4b93092e290`. The 0.5 difference is consistent with a fee, but no fee policy or cross-chain message identifier has been verified. Classify `0x874d…e290` as a **strong candidate historical BUSD bridge operating address**, not a confirmed segregated reserve vault. Transaction signing does not identify the humans, threshold-signing arrangement or organization controlling a key.

The arithmetic **1200000 − 300000.5 = 899999.5** connects these events to the [subsequently observed Fusion representation conversion](busd-ledger-reconciliation.md). That later conversion is verified within a Fusion receipt. The two cross-chain pairings above remain evidence-based inferences because their receipts do not provide a verified common bridge identifier. Neither establishes continued collateralization of later balances.

The matching BSC address `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c` has 75 returned nonzero BUSD events plus three additional zero-value events in the preserved receipts, reconciled to **0.007998 BUSD** at the queried head. Its public history includes **392215.959241128574864408** incoming from, and **250999.746** outgoing to, `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84`. The same address string occurs in the newer Fusion token flow. These repeated relationships strengthen the operational linkage, but do not establish a human identity or a reserve total.

## Major counterparties include swap routes

| BSC counterparty | Direction relative to `0x4563…e039` | BUSD total | Receipt observation |
| --- | --- | ---: | --- |
| `0x1111111254fb6c44bac0bed2854e76f90643097d` | Incoming | 3187201.634291222998679529 | Contract counterparty; do not count routed receipts as unique deposits |
| `0x05ad60d9a2f1aa30ba0cdbaf1e0a0a145fbea16f` | Outgoing | 1990092.462204505929596217 | All 192 events occur in calls to `0x1111…3097d` and return another token to the sender in the same receipt |
| `0x18101ac1d35230f1a3c005e2abaaeb25cae79e7f` | Outgoing | 859485.774903194481955258 | All 205 events have the same routing and other-token-return pattern |
| `0x7f9e3430880580713b03670046c7e89a0a91403b` | Outgoing | 272763.928918793216171658 | All 110 events call `0x1111…3097d`; 108 include another-token Transfer back to the sender |
| `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c` | Incoming / outgoing | 703216.712977558574864408 / 600000 | Direct BUSD transfers; the address string also occurs in the Fusion representation history |

The first three outgoing contract patterns support swap-related routing. They do not establish exact pricing, trade intent or ultimate beneficial ownership. A failure of `token0()`, `token1()` or `factory()` is not evidence that a contract is a custody wallet. Native-asset output also requires traces, rather than an ERC-20-only accounting rule.

## November 2023 transfer path

The following successful BUSD transfers form a concrete public path:

1. `0x456325f2ac7067234dd71e01bebe032b0255e039` sends **187290.327160557405335843** to `0x5ce8eaf2bba018688ade0f0a264f0a572e56d3a3`, transaction `0x18f90ba76942138254dee5b1882a4edd49ddc5f7228e72d2993afbfbed68bc3a`, block 33,128,672.
2. On 2023-11-02, the recipient sends **100** and then **187190.327160557405335843** to `0x983868374f46b78473ba05396ccd593192ed074e`, transactions `0xe68f5379894fd0205de0734d14fe01e7c27c1c9e94d98cd67038fc96b584c9c8` and `0x378f129abf672350623b4c7b7738b34e042b561bf580f3b1a58d2d5fb480bef6`.
3. At 2023-11-02 02:30:04 UTC, `0x9838…074e` sends their combined amount to `0x8894e0a0c962cb723c1976a4421c95949be2d4e3`, transaction `0xa72423f29141d8da4dce0493abc299ac932005ccba07cf137ae999344042bec5`, block 33,129,340.

BscScan displayed the destination label **“Binance 51”**. That is an attributed explorer label, not a node field or proof of an exchange customer's identity. Public token transfers do not expose subsequent internal exchange credits. An Ethereum counterpart must be considered alongside this path: at **02:45:47 UTC**, `0x21a31ee1afc51d94c2efccaa2092ad1028285549` (Etherscan label **Binance 15**) pays **187285.72716055 BUSD** to `0x5ce8...d3a3`, transaction `0x8cdbc747c45c2a317d62f0dd2c1d1acbfce8c287363211ea1aaeece4a4971b56`. At **02:55:35**, that address pays exactly this amount to `0x4563...e039`, transaction `0xcc074047330416fc0f61c8fbbdc4cea39d48bc2f693e89538b62294fef66b8b3`. The Ethereum controller receipt is **25 minutes 31 seconds** after the BSC exchange-labelled receipt; the amount difference is **4.600000007405335843**. **Inference:** cross-chain rebalancing through an exchange is consistent with the timing, amounts and repeated intermediary. Internal exchange linkage and fee treatment are unverified. The BSC transfer alone cannot establish a loss. The date is not classified as before or after Chainge's disruption, whose timeline has not yet been established.

The intermediate `0x9838…074e` also forwards **2999.743181581174398810** to the same labelled address on 2024-01-01, after receiving that amount from `0xd2e6dff14b015a5457b28f3ba6600d93d08655d0`. This separate flow must not be merged with the November amount. A small later transfer into `0x5ce8…d3a3` comes from a different address with a similar displayed suffix; compare full addresses, never truncated labels.

## Small-balance address and routing receipt

For `0x8a4aa176007196d48d39c89402d3753c39ae64c1`, BscScan returned two BUSD transfers:

| UTC | Direction | BUSD | Transaction |
| --- | --- | ---: | --- |
| 2024-01-23 07:53:39 | `0x4563…e039` → `0x8a4a…64c1` | 1.187246977608202752 | `0x91841d45a6db7f4d98838c9616c3a33e752dba64c0f09bd28ecb019db4b15f2f` |
| 2024-02-07 | `0x8a4a…64c1` → `0xa74d49d38df7092688686973c468c94a542294b3` | 1.107024317425808905 | `0x9f61b9cce465032ae57b4e9dda7bc857cbe5b04fa977461011a7e17210bd99f4` |

Both receipts were verified. Their difference, **0.080222660182393847**, equals the queried BUSD balance at BSC block 127,036,378 (2026-10-11 14:33:23 UTC). Later activity involving other tokens does not establish later BUSD movement.

The February transaction is sent by `0x026151a366235fb32b6bedc9f45f154f17846986` to `0xa74d…94b3`. Its receipt forwards the BUSD into a swap route and includes a routing event containing the text `"Source":"chaingefinance"`. The transaction sender's address string is also an observed Fusion controller. These are concrete routing links; neither the event text nor the address match proves the extent of reserves or ownership.

At the 2026 BSC snapshot, `0x8a4a…64c1` returns delegation-designator code beginning `0xef0100`, pointing to `0x63c0c19a282a1b52b07dd5a65b58948a07dae32b`. Interpret this using [BEP-441](https://github.com/bnb-chain/BEPs/blob/master/BEPs/BEP-441.md); it must not be misclassified as an ordinary deployed vault contract or projected backwards onto its 2024 behavior.

## Identical routing runtime on four chains

The retained Fusion runtime at `0x1e0cc9b982c7c3fb1ec704c28f48c35e7bed08a1` exactly matches the runtime at `0xa74d49d38df7092688686973c468c94a542294b3` on BSC, Ethereum and Avalanche at the captured blocks. Each is **6,155 bytes**, decoded-byte SHA-256 `7b4e75aec2261db5801b426300142b0e5d2f5a1f278909d8bb11085412a56135`.

The Fusion, BSC and Avalanche `owner()` calls return `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875`. Ethereum instead returns `0xca0eafd099c3b9a30682adbd3bb832bdb25b5b1c`. Administration therefore differs despite identical runtime. Identical runtime and matching administration support a code/administrative relationship. They do not establish a one-to-one reserve mapping, balances held elsewhere, or all contracts deployed by the project.

Ethereum and Avalanche checks of `0x8a4a…64c1`, `0x4563…e039`, `0xa74d…94b3` and `0x874d…e290` returned zero for the selected BUSD contract on each chain. These are **four-address snapshots**, not whole-chain reserve audits. Additional reads of `0xf11b4db13148d56e2a10ad701ad0ff338c62c52c`, `0x35cbec6a804556b460cd6007b23608329ccf8c65`, `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84` and `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c` also returned zero on Ethereum and for this Avalanche token. The distinct Avalanche BUSD.e contract has relevant history, documented below. The selected contracts are Ethereum `0x4fabb145d64652a948d72533023f6e7a623c7c53` and Avalanche `0x9c9e5fd8bbc25984b178fdce6117defa39d2db39`; BSC uses `0xe9e7cea3dedca5984780bafc599bd69add087d56`. Historical asset documentation must not be used to assert present-day collateral arrangements.

## Coverage and reproduction

Receipt queries prove what the returned transactions emitted; discovery still depends on explorer coverage. Public BSC `eth_getLogs` attempts were rejected, while historical receipt queries worked. Old pinned BSC state queries sometimes failed with missing-trie errors; these are preserved as failures, and fresh-head reads are separate captures. No failed query is treated as a zero balance.

The Fusion retained block is dated 2025-10-07. BSC, Ethereum and Avalanche state captures are dated 2026-10-11. Cross-chain snapshots are not simultaneous, and none determines when Chainge's service disruption began. Source-chain balances cannot be compared directly with combined legacy/new/time-slice token amounts as a solvency calculation.

## Earlier controller and additional redemption evidence

The returned BSC history of `0x874de5761900ee526950d707d8cbd4b93092e290` contains **1,543 nonzero events in 1,543 successful receipts**, from **2021-11-08 15:17:33** to **2022-07-12 08:54:16 UTC**. Incoming and outgoing totals are each **3820108.282482786630649033**, with a reconstructed peak of **1207953.596540818165324992** and final balance zero, matching pinned state. All 16 explorer pages were covered. The ledger never becomes negative.

A second public holder, `0xf96fea9f56f008b6dccb1bba4a59803cf53bd34b`, provides four additional BSC/Fusion amount matches:

| BSC payment to `0x874d...e290` | Fusion mint by `0x874d...e290` to the same holder | Amount | Fusion time minus BSC time |
| --- | --- | ---: | ---: |
| `0x7de2c88666743fa6cad961ed2ea4424012102f5e6452d42caa23dcbda582280f` | `0x243292a67f4f913fe8b03981475f25b1775fce110ab95b1efb35043da8252699` | 248000 | 9 seconds |
| `0xe6535e54dbf13a52d06cfb3bdc01065f45c6355b3305ab1921a1eea484772414` | `0xce63c4cf8e2fb96298d2f1a68f711ac13af9ec4419661583720202ddb85a5ac9` | 240610.706402606271021456 | -1 second |
| `0x777ff2969702c081f799c82ce60b05bd363fda8c87766ca25deff7e5746bb10b` | `0xaa7d79c0ecc982638993507c84325e50a914cc5edc8003cb818b9b8d09313533` | 305000 | 7 minutes 26 seconds |
| `0xca82b5612724ef95444bebf3cc74d169f5e576408ad255b2eb89f7ae92a4a187` | `0xc7c4d197639446d89761f4d64b85cbb7c0e3962052512d20fa9fa2c0b684620d` | 295829.227822658672609059 | 7 minutes 4 seconds |

These occurred on December 5-6, 2021. The negative one-second timestamp difference is retained, not silently corrected: timestamps from separate chains are not a synchronized causal clock. Each individual leg is node-verified; their pairing remains an inference.

On **2022-01-01 07:28:15 UTC**, the controller burns **1093232.344870668851674158** from this Fusion holder, transaction `0x3c68959f063cb5a062708313bae8959fd8f9742b78dcaab0b2e169caf6541c7b`. At **17:23:43**, it pays the same BSC holder **1093231.844870668851674158**, transaction `0x2342819c3d81dede23f263ac409546b62f5891e3fa66b49a7c11a7d68a2be870`. The difference is again **0.5**, but the gap is **9 hours 55 minutes 28 seconds**. This is consistent with redemption; a shared bridge identifier and fee rule remain unverified. A 1.2 million BSC deposit arrived shortly before this payment, but fungibility prevents assigning particular deposited units to that redemption.

The holder's nine returned BSC events finish at zero. Its **1102691.910645403908043643** payment to `0xb87f5110f19a42058908343ee10e5337a2b11187` combines the redemption with an existing balance. That recipient later forwards **1103791.910645403908043643** (including a separate earlier 1100 transfer) through `0x4c93251abe89ac983a26c53aa2dd1db5cdf82cb4` to `0x41772edd47d9ddf9ef848cdb34fe76143908c7ad`, labelled **FTX 4** by BscScan. Terminal transactions are `0x3f1c3fb29cf9f5b39e49b4de3aeb9246a29495ff715d1aff80a51f070683147e` and `0x0bcad18b6bed70b90f08450103f72e712a062af7da60dac333c4693f0e21f011`. This is an observed exchange-labelled path, not proof of the beneficiary, exchange credit or relationship to the later exchange failure.

The older controller's other largest outgoing branch is **1200000 BUSD**, sent as 1100000 on December 7 and 100000 on December 20, 2021 to `0x4e2be742c076f7e0a86192a25151c2bbeba0386f`. Both pass through `0xbe117e30c35f74302cea0ab7f553de08575f75a8` and then to the Binance-51-labelled address. The final transactions are `0x35c92b1ab61420fee44fcab205b2180f4d7947ef55dcb1ee98663c80e787de12` and `0x419e86fc4b55518db0a213a4f7a88d344bac3cdcd5945b9c1a071df238e73fbd`. No corresponding issuance/redemption or internal exchange account has been verified for these two movements. Later 100000 and 60000 receipts at `0x4e2b...386f` enter swaps that return another token to that address; they are separate flows.

## Ethereum controller history and later balances

Follow-up: the **204567.658073647823395109 BUSD** outflow on January 1, 2024 is traced to a **Binance 14-labelled address**, through two intermediaries, in the [exchange-path report](busd-204k-exchange-path.md). This resolves that on-chain destination while leaving the payment's purpose and exchange-account outcome open.

The returned Ethereum history of `0x4563...e039` contains **232 BUSD events in 232 successful receipts**, from **2022-07-12 06:34:53** to **2024-01-20 14:02:23 UTC**. Incoming and outgoing totals are each **878608.060806010088336945**, the reconstructed peak is **408224.513949397594602851**, and the final balance is zero, matching pinned state. All three Etherscan pages were covered; every discovered transaction has a successful node receipt after retrying null responses.

Its predecessor `0x874d...e290` has **nine** returned Ethereum BUSD events, from **2021-12-22 02:57:08** to **2022-07-12 08:53:54 UTC**. All nine receipts were verified. Incoming and outgoing totals are each **157670.172885877183**, ending at zero. The final **37670.172885877182999999** transfer follows a one-raw-unit transfer to `0x4563...e039` earlier that day.

The January 2024 controller payment to `0x8a4a...64c1` is **143.724135682808759163**, transaction `0x38206da5f0ddf00735e347744d2b7e4c9ae7d437fb25a52788ce3335302009fe`. Its later disposition is:

- **2024-08-23 19:10:35 UTC:** 120 to `0xbac20528aceee277cdd02600f06e767f7094fc8e`, transaction `0x4a2b7885e457a9f8744067452e92b90fc8134d72618f1951008fc0ff11318380`. Transaction `0x50c02c9b137a4db5d22be00f424001b3a31c7577ffc1b87fd445e412e2517190` then routes the 120 through `0x2f2dd99235cb728fc79af575f1325eaa270f0c99` and onward to `0x4ff7e1e713e30b0d1fb9cd00477cef399ff9d493`, with WETH movement in the same receipt. Native-output attribution remains unverified because the public trace endpoint requires an account token.
- **2024-08-26 08:58:59 UTC:** 23.724135682808759163 to `0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e`, transaction `0x2dfe3207a8e996fc2ce1e9d3c8329ca895a6494b3c6d197e861e8c189163f8c2`.
- **2024-09-10:** the latter amount moves to `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1`, transaction `0x0b45771f491fca735945acf511823fc60003efb661bae721613f50ee19ae6a69`. Its pinned BUSD balance is still **23.724135682808759163** at Ethereum block **26170361**, hash `0xed347d8cbe1cea023833fc983752b576c848b6e1e695296d7efb9e676cb09a5f`, **2026-10-11 16:19:11 UTC**. The two intermediaries and `0x2f2d...0c99` return zero.

These are BUSD movements. They do not identify beneficial owners or prove that swap outputs remain available for recovery.

## Avalanche BUSD.e and the November bridge leg

Avalanche has distinct token contracts: Binance-Peg BUSD at `0x9c9e5fd8bbc25984b178fdce6117defa39d2db39`, and **BUSD.e** at `0x19860ccb0a68fd4213ab9d8266f7bbf05a8dde98`. [Avalanche's BUSD documentation](https://support.avax.network/en/articles/6576980-busd-faq) distinguishes the representations. An empty history for the first must not be generalized to the second.

Routescan returned **347 BUSD.e events** for `0x4563...e039`, **22** for `0x874d...e290` and **three** for `0x5ce8...d3a3`. Every returned row matches a successful node receipt. Tail queries after each last returned block are empty through block 97290509. Reconstructed balances finish at zero and match later pinned state. Incoming and outgoing sums are each **19569.129245861096569942** for `0x4563...e039` and **6313.986456565598745915** for `0x874d...e290`; self-transfers are included in both sides. These are gross flow sums, not unique capital.

The following November 2, 2023 sequence complements the BSC/Ethereum path:

| UTC | Chain | Observed movement |
| --- | --- | --- |
| 02:11:44 | Avalanche | `0x4563...e039` sends **5701.441708207748481983 BUSD.e** to `0x5ce8...d3a3`, transaction `0x6c2654afc28766cf40e47a2b928215166904c1923fc6d030b5e3860a3a3d51ee` |
| 02:13:25 | Avalanche | **3.999 BUSD.e** goes from `0x5ce8...d3a3` to `0x9838...074e`, transaction `0xf09aeda88c7ef478c92297fc7152433a94d8fd07d977da43f24ecd48d67d963b` |
| 03:22:15 | Avalanche | `0x5ce8...d3a3` calls the token's unwrap selector and burns **5697.442708207748481983 BUSD.e**, transaction `0xd54bc9e18a6805474d1fc18a5f873d8c246af61fa6f53f2c327c908d452b8f69` |
| 03:22:35 | Ethereum | `0x8eb8a3b98659cce290402893d0123abb75e3ab28`, labelled **Avalanche: Bridge** by Etherscan, sends **5681.060059194158425856 BUSD** to the same `0x5ce8...d3a3` address, transaction `0x6318aa129d5222b5048f5263610a9c318d6bfb0714147b75829d3be5e644f828` |
| 03:25:23 | Ethereum | The full received amount goes to `0x4563...e039`, transaction `0xf41a08c00c3c4956997cc279ed5ff42c6723392bad8f3deb41fd2ff54c732b0d` |

The burn/release gap is **20 seconds**, and the difference is **16.382649013590056127**. Bridge processing and fees are a strong inference, not a verified common message-ID match. The four returned Ethereum intermediary events reconcile exactly to zero. The Avalanche recipient `0x9838...074e` still holds **3.999 BUSD.e** at the final pinned snapshot.

## Verified Avalanche liquidity-address configuration

Successful May 2024 calls from `0x6a01dd8e6384fe7835cb5cf124201bbd8a670056` to **`0x0b662d54824825725be1e76c0a2b1c8f49b7419b`** use the `vaultOut(address,uint256,bool,bytes)` selector and transfer BUSD.e from the caller to `0x8a4a...64c1`. The explorer publishes the called contract as **MinterProxyV2**. Its source describes non-burnable vaultOut tokens being transferred to its configured liquidity address. We have not independently compiled that source to match runtime; the decisive evidence here is node receipts and historical getters.

At Avalanche block **46122438**, `_liquidpool()` returns **`0x8a4aa176007196d48d39c89402d3753c39ae64c1`**, and `owner()` returns **`0xcd66ae3529d74cd1d7185fe6882a3b80b791f875`**. This verifies a **configured liquidity-address role on Avalanche at that block**, not a segregated reserve or human identity. `0x6a01...0056` returns empty code at the final snapshot and must not be described as the vault contract merely because it sent a vaultOut transaction.

Two later configuration changes are verified by successful transactions and `_liquidpool()` reads immediately before and after each block:

| UTC | Block | New configured liquidity address | Transaction |
| --- | ---: | --- | --- |
| 2024-08-24 03:23:50 | 49639246 | `0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e` | `0xc1f2baa7dc6de5f6b3a3a8ab64b59a6cd1fd6936f21b53c9c0a079149c379523` |
| 2024-09-05 12:43:22 | 50151486 | `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1` | `0xfaffc37d3d16b5c6f6596d54d8ba2abca02a1ed53800d5030bf573814a68f927` |

Ownership transfers on **2024-08-24 01:40:31** to `0x02e8f2ad8221cb59fb15248afc265bb8d32ed4e3`, then on **2024-09-05 09:44:33** to `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1`, are preserved in `avaxe-router-admin.*`. The latter matches the retained Fusion factory/legacy ownership address. This sequence also matches the address strings receiving the later Ethereum BUSD transfers. The cross-chain relationship is strong; equivalent Ethereum contract configuration is not established.

Current and historical Avalanche runtime bytes for this router match, SHA-256 **`4fc381fe865366ce6959474c14216e7568678ab0beedd6577215585292fb18f3`**. At the final Avalanche block **97292331**, hash `0x2232a6238efb9c467c0007f0b9c835bfdb4df8168c84d49deb5c14ca5fcab735`, **2026-10-11 16:12:49 UTC**, the pool remains `0x66ff...ecf1` and its BUSD.e balance is zero. `0x8a4a...64c1` retains **0.000000000000087769 BUSD.e**; its four returned events reconcile exactly. `0x6a01...0056` has 20 returned BUSD.e events, including later swaps/burns, with final balance **0.010325562989512130** matching state. Its last returned event is a small incoming transfer on **2026-05-11**; that observation does not establish resumed Chainge service.

The identical `0x0b66...419b` address on BSC returns different runtime and different current owner/pool values, and Ethereum returns empty code. Address equality is therefore not a safe way to extend the Avalanche contract's attribution to those chains.

## Evidence package and boundaries

Reviewed material is in [the source-chain evidence package](../evidence/2026-10-11-busd-source-review/README.md), with byte-integrity [manifest](../evidence/2026-10-11-busd-source-review/manifest.json). Direct receipts, requests, pinned state, relevant headers, configuration changes, discovery records and failed attempts are preserved. Large unrelated block-receipt discovery dumps remain outside the repository; the relevant transactions were captured separately.

A Routescan Ethereum control query returned only **53 of the 232 known controller events**. Its zero-result predecessor query is therefore rejected as a full-history conclusion; Etherscan subsequently supplied nine events and all nine were node-verified. Ethereum successor searches through this API have the same completeness limit. Null receipt responses were retried successfully. BSC native tracing failed with missing-trie data; Ethereum native tracing required a personal access token. Neither failure proves absence of an internal transfer.

The scope is the identified BUSD representations, controllers, liquidity configuration and material traced branches. It does not include a census of all customer wallets, every token received in swaps, every chain supporting a similarly named asset, or exchange-internal records. Transfers alone cannot establish intent, customer entitlements, total reserves or loss. The [case assessment](busd-case-assessment.md) records which conclusions are established and what additional evidence could change them.
