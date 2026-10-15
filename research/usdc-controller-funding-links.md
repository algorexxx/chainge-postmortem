# USDC controller mints linked to source events

Reviewed 2026-10-15. This follow-up narrows the unassigned controller-mint findings in the [holder review](newer-holder-funding-and-restrictions.md). It changes no balances, classifications or custody assumptions.

## Results

**All nine selected controller mints have a matching identifier in a successful source-chain receipt.** Four have destination/output memos that agree with the Fusion mint; five are narrower swap-output correlations. These are observed field relationships, not a verified contract ABI or proof of backing.

| Source chain | Source event amount | Fusion mint | Source receipt | Fusion transaction |
| --- | ---: | ---: | --- | --- |
| Arbitrum | 9999.000016 | 9992.990616 | [Receipt](https://arbiscan.io/tx/0x50c4fbdbb8dde9ff954a38107922393e904b1b9cc820f88088f537956e6d0692) | [Mint](https://fsnex.net/transaction/0x28b52ea47a7ab4d9d41287e3996f3e16c5d79d3866f144a1cb2fa226e97b1669) |
| Arbitrum | 9999.423247 | 9993.413594 | [Receipt](https://arbiscan.io/tx/0xa673a3ec8bdf89242296adca73789952052f677b17d3e2bdbd66327f657d1e2d) | [Mint](https://fsnex.net/transaction/0x7082e85c56894e6d6ee65ea986d0cce5cfffc84a7e10bad177bd8929d15a27b1) |
| Avalanche | 9999.699935 | 9993.690116 | [Receipt](https://snowtrace.io/tx/0x0da5c43fd04be3e3dbdce840c76b7c1ab09a543162f3b52b4734a66857d917e1) | [Mint](https://fsnex.net/transaction/0xf93bf015f23dd8b7fb43cba2ef25efa7f4210f6e1a7018b873879369d2ac938d) |
| Avalanche | 19999.900184 | 19987.890251 | [Receipt](https://snowtrace.io/tx/0xbfbe46fbc66cd99f41a2bb661e5609aaf1f07b2cb4eab2b0f70b31065b1baf5a) | [Mint](https://fsnex.net/transaction/0xd8303cde9dc97adf13b7571f231da535f49f88a08d4f34ffc10c811b7c4e169d) |
| Avalanche | 20283.428478 | 20271.248422 | [Receipt](https://snowtrace.io/tx/0xc896a288197367af2f8c6a8b3d2ad6dda3f09add0cc670bb5e79c6301ef235ca) | [Mint](https://fsnex.net/transaction/0x600de33cb283a698043dce83af2fdf0ffe1304d439e3fd75fae1ec4d4372e2c7) |
| Ethereum | 14764.425876 | 14755.557220 | [Receipt](https://etherscan.io/tx/0x1ea159804175ede00d8c9927474e3bcb25690578b36c2f10f495b55b6d13d205) | [Mint](https://fsnex.net/transaction/0xd98313ac0a9a07854b2bdf46442b2a143503346c6a6aee017d3764a2dbb54188) |
| Ethereum | 20000.000000 | 19987.989999 | [Receipt](https://etherscan.io/tx/0x5d4b2faf4b90827d7109b1bccf488775a6c5a91f0c5a6d7791972e74060b5c3f) | [Mint](https://fsnex.net/transaction/0xd2aeb0927f459ed4eeddfeb6770c61d9531b23d2026b34a50d76460a37ec1e93) |
| Ethereum | 416154.953803 | 415905.250831 | [Receipt](https://etherscan.io/tx/0x37640e7333727a764f7766393985d7f5367d206a06f24a6a60ff0e02613c1136) | [Mint](https://fsnex.net/transaction/0x5b60672ac7c54f29d0f7dcae51f32f281f37c82c31337538f2d214598943e6e1) |
| Rollux | 2008011.232650 | 2006806.415910 | [Receipt](https://explorer.rollux.com/tx/0x010d8150a55a50a1bcd72900e0f4c83f03d7992047866ca6129df8dc9d9427fc) | [Mint](https://fsnex.net/transaction/0x5f838c567c6942f143b46ff76bfeebe205ce92f53e73d5dcf618f582adcacbc9) |

Amounts use six decimals for the specific source token contracts in these receipts. The source amount is the deposit or main swap-output event amount. It does not include a separate swap-fee transfer where one exists. Differences from minted amounts are observed differences; their fee semantics have not been authenticated.

## Two large mints

Follow-up: the Rollux deposit now has a documented [Fusion → Syscoin → Rollux → Fusion route](usdc-rollux-roundtrip.md). Its traced input was a Fusion token transfer, not an independently verified USDC deposit.

The **2006806.415910** mint to [f11b…c52c](https://fsnex.net/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c) follows a **2008011.232650** deposit on Rollux. The receipt transfers the [Rollux token](https://explorer.rollux.com/address/0x368433cac2a0b8d76e64681a9835502a1f2a8a30) from [8a85…4e84](https://explorer.rollux.com/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84) into [66ff…ecf1](https://explorer.rollux.com/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1). Its router event carries the same identifier as the Fusion controller call. The memo names FSN, the exact newer USDC contract, f11b, and the exact minted amount. Block timestamps put the mint 50 seconds later.

**This explains a source of Fusion issuance, not reserve quality.** The Rollux asset has unverified backing and remains excluded from the main vault total. Its receipt does not prove Circle-issued USDC was deposited, nor does it establish an independent economic claim to add to the Fusion supply. No claim that the same deposit remains wholly in the current Rollux balance follows after subsequent mixing and transfers.

The **415905.250831** mint to f11b follows an Ethereum transfer of **416154.953803 USDC** from f11b into [3f0e…aa8e](https://etherscan.io/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e). The matching identifier, exact Fusion contract/recipient in the memo, quoted output equal to the mint, successful receipts and 41-second timestamp difference support the linkage. This replaces the earlier description of its source as unassigned. It does not establish ownership or whether the deposit itself came from a prior withdrawal.

## Blocked holder: seven later mints

The two Ethereum cases send **20000** and **14764.425876 USDC** from [e4a2…4c1b](https://etherscan.io/address/0xe4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b) to [8a4a…64c1](https://etherscan.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1). Their JSON memos specify FSN, that same destination holder, observed token index 6 and outputs **19987.989999** and **14755.557220**. Those outputs match the mints exactly. Index 6 is not independently mapped to a token by this review; actual source transfers and the Fusion receipt establish the contracts observed on each side.

The remaining five identifiers occur in successful USDT-to-USDC swap receipts on Arbitrum (two) and Avalanche (three). They show USDT leaving 8a4a, being swapped through router/pool contracts, and USDC returning to that address. Each matched output event has exactly one equal token transfer to its stated recipient in the receipt. The subsequent Fusion controller mint carries the same identifier. These source swap events do not themselves name the Fusion destination holder; their interpretation therefore remains narrower than the four memo-supported deposits.

Across all nine links, reported block timestamps place the mint 0–50 seconds after the source event. Cross-chain timestamps are not a precise shared clock; a zero-second difference does not prove simultaneous execution. Identifiers may be reused across chains, routers or time. No global uniqueness or authenticated order semantics is assumed.

## Evidence and repeatable checks

Source receipts were already in the public investigation inputs. New [transaction/header captures](../evidence/2026-10-15-controller-funding-links/) corroborate chain IDs, transaction block hashes and source block timestamps using anonymous free RPCs. Ethereum initially rejected the oversized batch; successful smaller requests and the failures are preserved. The [Fusion evidence](../evidence/2026-10-15-newer-holder-review/) supplies successful matching mint receipts and headers.

Run:

```powershell
./research/review-controller-links.ps1 -Asset usdc -FusionEvidenceDirectory evidence/2026-10-15-newer-holder-review -Controller 0x3668c219b1fa8fe8175158f6ce91ded36fde9152
./research/check-controller-links.ps1
```

The reusable reviewer requires a five-word controller call and exactly one agreeing mint log, scans the asset configuration’s source receipts for two observed event layouts, rejects conflicting duplicate events and keeps all identifier candidates. Reports, decoded memos and input hashes go to ignored `archives/controller-links/`. The selected nine-case check verifies source receipt/header agreement, token transfers, chronology and the four destination/output memos. Both run in `verify.ps1`.

This remains a review report rather than an automatic redemption/deposit classifier. Extending automatic matching requires explicit token/route validation and tests for identifier collisions, unsupported layouts, competing matches and fee handling. Ownership, complete custody, Rollux backing and consolidated customer liabilities remain unresolved.
