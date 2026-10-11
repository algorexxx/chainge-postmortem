# BUSD ledger reconciliation and migration classification

Observed 2026-10-11. Fusion state is pinned to block **15,130,080**, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`, dated 2025-10-07 08:43:50 UTC. It is not a present-day snapshot. [Evidence and integrity manifest](../evidence/2026-10-11-busd-ledger-review/) preserve the requests, responses and collection limits.

## Two event ledgers reconcile to retained balances

| Representation | Transfer events returned | Nonzero addresses appearing in events | Positive balances at retained block |
| --- | ---: | ---: | ---: |
| New BUSD `0x9d7005f9175856bda5deb153f86a11153d9d0fa7` | 1,756 | 134 | 112 |
| Legacy full-time child `0x8f8c247a300312b805692a3bc49414e35dec64ce` | 35,552 | 776 | 23 |

For each ledger, subtract each event amount from its sender and add it to its recipient, using integer raw units. Every observed nonzero address's resulting balance, including zero balances, agrees exactly with `balanceOf` at the retained block. No nonzero address has a negative reconstructed balance. This is a consistency check between returned history and state, not independent proof of a complete index: missing offsetting transfers could evade it. The explorer and node may share upstream data.

New-token retrieval covers deployment block 11,025,309 through the retained head, with two pages of 1,000 and 756 records. The last returned Transfer is 2025-07-21 10:04:50 UTC, transaction `0x9a9b61208035e9f1fe18ced29c7f8b3028945f47f3b36532ac08710bae0109a8`. This replaces the earlier first-page-only coverage in the balance-flow report.

Legacy retrieval covers deployment block 6,450,428 through the same head. The first two million-block windows returned 4,572 and 6,479 events. A third window reached the explorer's 10,000-result pagination ceiling; its failed page and partial results are preserved but **excluded from the final ledger**. Replacement contiguous 250,000-block windows begin at 8,450,428 and end at the retained head. Each finishes with a page shorter than 1,000. Deduplication checks found no repeated transaction-hash/log-index pairs in the selected ledger. The last returned full-time Transfer is 2024-04-07 15:08:17 UTC, transaction `0x60bee25e177385b60e5d03c03eafa70a8fa879c49c1ecca4f1d9d2e4dc21efc5`. These last-event times are explorer observations, not an incident-date determination.

The legacy ledger covers the full-time child only. Other dated slices and the earlier representation are separate contracts. Do not add these balances to newer BUSD as if they were independent liabilities.

## All newer-token mint and burn events classified by transaction path

All 125 issuance transactions have successful node receipts; their transaction inputs and block headers are preserved. Each contains one relevant newer-BUSD mint or burn event.

| Observed path | Events | Token units (18 decimals) |
| --- | ---: | ---: |
| Calls to `0x61b54d2263b9249ae5aa4acd5acac880ed815efc`, selector `0xba009887` | 103 mints | 176944.796905633011413528 |
| Calls to `0x9851034eca59466ec346425a4674f93ef14d261f`, selector `0x85a6a020` | 12 mints | 4931.290923681360912096 |
| Direct `mint(address,uint256)` calls to newer BUSD | 5 mints | 1720071.223071900493846300 |
| Direct `burn(address,uint256)` calls to newer BUSD | 5 burns | 859866.890200848809065438 |

The mint total minus the burn total is **1042080.420700366057106486**, matching the previously checked newer-token supply and the sum of reconstructed nonzero-address balances.

Every one of the 103 calls to `0x61b5…5efc` pairs a legacy full-time burn with a newer-token mint of the **same raw amount to the same holder**, in the same successful receipt. This supports the classification *legacy-to-new migration*. All 103 are sent by `0x07281e30b8ef4985e75233aafdf5d3f27a09ceb2`. The migration contract's retained owner is `0xda810a9d52a86ce51ee28defbd8f026d4e363541`; its runtime includes migration-related revert text. Exact source-code verification and selector naming remain unresolved.

This includes the 0.030048708641095100 remainder discussed in the earlier [slice-accounting report](legacy-authority-and-slice-accounting.md): transaction `0x2134366509de7e8adc1e8999fb30adfda19794303d71f52be8de243385708fa9` burns the old representation and credits the new one to `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c`. The prior report described only the burn; the preserved receipt establishes both legs.

The twelve `0x9851…261f` calls follow the [previously investigated liquidity-migration path](busd-migration-and-balance-flow.md). **None contains a Transfer from the legacy full-time child.** They must not be classified as the same burn-and-mint mechanism as the 103 migrations. Historical pair state or a full execution trace would be needed to explain their copied amounts completely.

The five direct mints and five direct burns remain administrative issuance/destruction observations. The earlier large amount-match hypothesis is not upgraded to a proven migration merely because another contract has a proven migration path.

## Newer-token balance concentration

| Address | Retained balance | Share of newer supply, rounded |
| --- | ---: | ---: |
| `0xf11b4db13148d56e2a10ad701ad0ff338c62c52c` | 859506.153699140493846300 | 82.48% |
| `0x874de5761900ee526950d707d8cbd4b93092e290` | 139902.745508395973606435 | 13.43% |
| `0x12fc5784c47a0be3c3867aa945010bca6db55177` | 26526.709473305390600466 | 2.55% |
| `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1` | 5632.503199653598171015 | 0.54% |
| `0x58ff7d134482b54d910134330bd4c6162c78afb5` | 5421.484279591560571283 | 0.52% |

All five return empty code at the retained block. Empty code does not identify a human controller. Thirteen of the 112 positive-balance addresses return nonempty code; contract code alone does not establish a vault role. The second-largest address is also an observed legacy controller, as documented in the earlier authority report.

## Legacy address-one balance is not a zero-address burn

Address `0x0000000000000000000000000000000000000001` holds **1007499579.438009673238337766** legacy full-time units at the retained block. Two successful ordinary `transfer(address,uint256)` calls account for it:

| Explorer UTC | Sender | Amount | Transaction |
| --- | --- | ---: | --- |
| 2022-03-20 07:18:37 | `0x716daec7a67036a509d8786a7c4115f80b5cc92e` | 1007338952.115621937994980826 | `0x1bbb170177612b72f87233c3a53d472ef7c77ce55c36aaf5d5869517e5d43b35` |
| 2022-03-20 07:47:56 | `0x829a3efcd90d237bbcd9a021d748705ae05b188e` | 160627.322387735243356940 | `0xec3e0588d6a64b2900901edd1538e54573e0f52469e2ef430b28df0892171b1b` |

Do not count these as zero-address burns, externally backed issuance, recoverable reserves or customer claims. Their purpose is not established here. The blocked `0x7bc4…6695e` retains 859506.1537 units separately; that position is unchanged from the earlier report.

## Earlier representation identified, role still bounded

The first incoming legacy transfer previously traced uses `0xcb02f42d8790738a78d47b4f67d242cce48f1de6`. At the retained block it returns name `BUSD Token`, symbol `BUSD`, decimals 18 and owner `0x1e1d8d811c11d33b75ab66573282fba28904bc66`. Its receipt topic matches node-derived `Transfer(address,address,uint256,uint256,uint256)`. In the 2022-03-05 receipt, that earlier contract emits a holder-to-zero event for 899999.5 units over interval 0 through 2^64−1, while the full-time child transfers the same amount from `0x716d…c92e` to that holder.

This supports a conversion between an earlier time-aware representation and the full-time child. It does not establish a source-chain deposit. The `0x87dc…9b42` contract coordinating that receipt has the same retained owner as the earlier token. Its exact source and complete historical mapping remain unverified.
