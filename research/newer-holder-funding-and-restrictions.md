# Newer Fusion holders: funding and transfer restrictions

Reviewed 2026-10-15 against Fusion block **15,130,080**, **2025-10-07 08:43:50 UTC**, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`. This is the [restored-chain reference](fusion-snapshot-provenance.md), not an independently established final network block. Amounts below are token units, not verified customer liabilities or dollar valuations.

## Findings

All **523 positive newer-token positions** in the published BUSD and USDC inventories were checked independently against the factory block list. Only one is blocked: [USDC holder e4a2…4c1b](https://fsnex.net/address/0xe4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b), with **622376.614316 USDC**. No blocked balances are subtracted from supply. A false blocking flag does not establish key access, ownership, redemption access or backing.

The three largest holders of each asset reconcile exactly from the captured transfer histories:

| Asset / holder | Incoming count / amount | Outgoing count / amount | Balance |
| --- | ---: | ---: | ---: |
| BUSD [f11b…c52c](https://fsnex.net/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c) | 1 / 859506.1536991404938463 | 0 / 0 | 859506.1536991404938463 |
| BUSD [874d…e290](https://fsnex.net/address/0x874de5761900ee526950d707d8cbd4b93092e290) | 1 / 139902.745508395973606435 | 0 / 0 | 139902.745508395973606435 |
| BUSD [12fc…5177](https://fsnex.net/address/0x12fc5784c47a0be3c3867aa945010bca6db55177) | 1 / 26526.709473305390600466 | 0 / 0 | 26526.709473305390600466 |
| USDC [66ff…ecf1](https://fsnex.net/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) | 910 / 3774928.107623 | 36 / 328737.4 | 3446190.707623 |
| USDC [f11b…c52c](https://fsnex.net/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c) | 99 / 5697339.19329 | 62 / 3616427.080952 | 2080912.112338 |
| USDC [e4a2…4c1b](https://fsnex.net/address/0xe4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b) | 32 / 622376.614316 | 0 / 0 | 622376.614316 |

These are Fusion holder positions. An address also appearing as a likely vault on another chain does not turn its Fusion balance into source-chain backing. Beneficial ownership is not established.

## Why the USDC transfer fails

The successful [USDC block transaction](https://fsnex.net/transaction/0xb0cc36b54b76e32fd982e9de51bd781f741c95310f7cd8cde2d0c103cda0d85c) occurred on **2024-11-05 04:22:33 UTC**, block **12,905,242**. It was sent by [1d9a…bcb1](https://fsnex.net/address/0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1) to the [token factory](https://fsnex.net/address/0xda30862467559b8b6e46d587aa9af5f48088c98b), with calldata matching `setBlock(USDC, e4a2…4c1b, true)`. The sender is also the recorded factory owner and a controller at the snapshot; this does not identify a person.

The same sender made two preceding block calls targeting the same holder: [USDT at 04:21:54](https://fsnex.net/transaction/0xd50885a14b77e68907ef517a0ae76ccc646e396c76cb42792acee3f410963b09), and [another token at 04:22:20](https://fsnex.net/transaction/0x90a73540180fd9371a1abc9513091eb47455ae543eaa39faeb93c1a2fef4d5a7). The three successful receipts and matching block headers are retained. This is a Fusion token restriction, not evidence of Circle blocking a source-chain USDC vault.

At the snapshot, the USDC-specific block getter returns true, the holder's all-token block getter returns false, and USDC is not globally paused. A trace of a one-raw-unit transfer from the holder shows the token asking the factory about its pause state and both transfer parties. The sender check returns true, the recipient check false, and the transfer reverts with `Token: token transfer not allow!`.

A read-only simulation using the recorded controller as hypothetical sender accepts clearing the block. A control address is rejected with `Factory: Only all token controller!`. The follow-up getter still returns true. No transaction was signed, no key access was demonstrated, and no state was changed. The evidence supports controller authority to change the restriction; it does not establish why the restriction was imposed or continuous blocking throughout the intervening period. This deployed-behavior result does not depend on treating the comparison Base source as verified Fusion source.

## Funding of the blocked balance

All 32 incoming transfers occurred between **2024-01-29 16:47:30 UTC** and **2024-10-26 04:21:33 UTC**, before the observed block call. No newer-USDC outgoing transfer from this holder appears through the snapshot.

| Funding category | USDC |
| --- | ---: |
| Same-transaction legacy burn and newer mint | 328973.737129 |
| Seven later mints through a controller contract | 104982.780218 |
| Transfers from [8a4a…64c1](https://fsnex.net/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1) | 155125.095614 |
| Transfers from [0985…6d035](https://fsnex.net/address/0x0985d161a36730ef0315ead01ad3ad644c56d035) | 33295.001355 |
| Total | 622376.614316 |

The [initial conversion receipt](https://fsnex.net/transaction/0x1baa6e0909589cac84be88ac1ecef3e91ae1aadf6d6b4b2b8e90d01ea5774ed7) burns exactly 328973.737129 from this holder's legacy full-time token and mints the same amount of newer USDC to the same holder. That portion is a verified representation conversion, not evidence of a fresh external deposit. Its burn and mint are already included in their respective ledgers.

The seven later mints are [9993.413594](https://fsnex.net/transaction/0x7082e85c56894e6d6ee65ea986d0cce5cfffc84a7e10bad177bd8929d15a27b1), [9992.990616](https://fsnex.net/transaction/0x28b52ea47a7ab4d9d41287e3996f3e16c5d79d3866f144a1cb2fa226e97b1669), [9993.690116](https://fsnex.net/transaction/0xf93bf015f23dd8b7fb43cba2ef25efa7f4210f6e1a7018b873879369d2ac938d), [19987.890251](https://fsnex.net/transaction/0xd8303cde9dc97adf13b7571f231da535f49f88a08d4f34ffc10c811b7c4e169d), [20271.248422](https://fsnex.net/transaction/0x600de33cb283a698043dce83af2fdf0ffe1304d439e3fd75fae1ec4d4372e2c7), [19987.989999](https://fsnex.net/transaction/0xd2aeb0927f459ed4eeddfeb6770c61d9531b23d2026b34a50d76460a37ec1e93), and [14755.557220](https://fsnex.net/transaction/0xd98313ac0a9a07854b2bdf46442b2a143503346c6a6aee017d3764a2dbb54188). They call [3668…9152](https://fsnex.net/address/0x3668c219b1fa8fe8175158f6ce91ded36fde9152) with selector `0x5b7dd1bb`; the captured input does not contain an explicit source transaction hash. Their source-chain funding was initially unassigned. The [identifier follow-up](usdc-controller-funding-links.md) now locates source events for all seven, distinguishing memo-supported deposits from narrower swap-output correlations. An exact-amount search within 24 hours in the retained USDC incoming-vault ledger found no candidates; this does not exclude fees, other assets, other vaults, aggregation or incomplete coverage.

## Other large holders and migration links

The entire BUSD balances of 874d and 12fc came from same-holder legacy burns and equal newer mints in [this transaction](https://fsnex.net/transaction/0x7387373b01a7f5e6640e93def4830860ab4b2102fb1ca2a6b446de3fa08f5b70) and [this transaction](https://fsnex.net/transaction/0x0fa2efc5f57419078e9cec90eaa69cf89b5583a32b8bd2e28062865482e0b489), both at block 11,072,308. The f11b BUSD position remains the separately documented [burn/remint candidate](busd-migration-and-balance-flow.md), not a proven additional deposit.

For USDC, the previously identified 2677846.972295 mint to [8a85…4e84](https://fsnex.net/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84) was [transferred onward](https://fsnex.net/transaction/0xc179f902d2b76f927cc4102c0437d707db7802eb3f059e5d87f91be6d57d5356) to [35cb…8c65](https://fsnex.net/address/0x35cbec6a804556b460cd6007b23608329ccf8c65). That address later [burned 2127366.696507](https://fsnex.net/transaction/0x1d7afa0ebc00490f2c856486fc351b681ffd5ca4f32bdb32b2c15f1cdab3b1fc), followed by an [equal mint to f11b](https://fsnex.net/transaction/0xc6a7e68286e1690e4d1cb727763602ecbbc335dbe20313cf7a14e775500f1159). This resembles the BUSD path but is a candidate association between separate transactions, not proof of common ownership or cancellation of the blocked legacy balance.

Later, f11b sent [1000000](https://fsnex.net/transaction/0x6b14f396a422d7d2942193006ce5ef956cf757faf71f8335ad57c828969ce251) and [1059247.080952](https://fsnex.net/transaction/0xe5f989bbf0550aeba4ea5cf8163e8cdc3f2aa67e9643ca82f31c3936d60861cb) to 8a85, which then sent [2059247.080950 to 66ff](https://fsnex.net/transaction/0x16ec6fd9136b92937086e3727dd5d1ca049ea3db48db238ca330b0cfaa2a9318). These are transfers within the newer Fusion token ledger. The 0.000002 difference is retained, not rounded into an exact match.

Another [956507.192675 reached 66ff](https://fsnex.net/transaction/0xdaa2ab8a56d2b3e039607043bdef3b0410d75c04a0468fcfeaeb985051680a61) from [3f0e…aa8e](https://fsnex.net/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e). Together these two transfers explain 3015754.273625 of its gross receipts. Its 36 outgoing transfers total 328737.4, all to [4408…0a80](https://fsnex.net/address/0x440872cfddac749d9f201234395fc20ed5480a80). Gross receipts cannot all be assigned to the final balance after mixing and outgoing transfers.

The other two largest f11b mints, [415905.250831](https://fsnex.net/transaction/0x5b60672ac7c54f29d0f7dcae51f32f281f37c82c31337538f2d214598943e6e1) and [2006806.415910](https://fsnex.net/transaction/0x5f838c567c6942f143b46ff76bfeebe205ce92f53e73d5dcf618f582adcacbc9), also use the controller contract. No equal earlier newer-USDC burns or exact incoming-vault amounts within 24 hours were found in the reviewed ledgers. The [identifier follow-up](usdc-controller-funding-links.md) now links the first to an Ethereum deposit and the second to the unverified Rollux token. Identifying these source events does not establish complete backing. No automatic reclassification is applied.

## Official migration guidance

A [third-party PDF copy of “Navigating the Chainge Migration”](https://resources.cryptocompare.com/asset-management/15681/1720529979779.pdf), attributed to Chainge and dated **29 January 2024**, says other Fusion assets, explicitly including USDC, retain their functionality under replacement contract addresses. The explicit old-token burn statement names CHNG. The guide supports a general replacement interpretation but does not identify these blocked BUSD/USDC positions or settle their treatment. The original Medium publication was not retrieved. Direct PDF download returned HTTP 403; [review metadata](../evidence/2026-10-15-newer-holder-review/migration-guide-review.json) preserves the accessible excerpt and provenance limit, not a claimed byte archive.

A bounded search of the private export's confirmed founder/admin messages from 2023–2025 found discussions of wallet-mode migration, new infrastructure and other assets' migrations, but no address-specific cancellation rule for these positions. Community records and identities were not published. This is not a claim that no other migration instructions exist. The [legacy migration review](fusion-migration-semantics.md) therefore continues to apply **zero additional deduction** for the two large blocked legacy/newer overlaps.

## Reproduce

The [evidence package](../evidence/2026-10-15-newer-holder-review/) retains 53 selected successful transactions with matching receipts/headers, all newer-holder flag calls, the failed-transfer trace, permission simulations and SHA-256 hashes. Existing reconciliation inputs retain the complete observed transfer histories. No private connection details or Telegram records are included.

Run `./research/review-holder-funding.ps1 -Asset usdc` or `-Asset busd`. The shared script deduplicates captured Fusion transfers, rejects conflicting events, retains exact integer amounts and checks each largest holder's incoming minus outgoing total against its published snapshot balance. `-TopCount` extends the review. Reports go to ignored `archives/holder-funding/` and include event-level evidence references and input hashes. No ownership or deposit classification is inferred from a mint.

`verify.ps1` runs both holder reports and `research/check-newer-holders.ps1`, which checks published block flags against the exact token/holder/block requests and validates the selected receipts and restriction evidence. Source-chain funding not identified above, ownership, and consolidated customer liabilities remain open evidence gaps.

Follow-up: [controller identifier review](usdc-controller-funding-links.md) now links all nine previously unassigned selected mints to source events, with four matching destination/output memos. The 2.006806 million mint traces to the separately unverified Rollux token. This supersedes the unassigned-source statements above without establishing backing or changing ledger classifications.
