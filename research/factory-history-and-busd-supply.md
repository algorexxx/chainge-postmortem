# Factory history and BUSD supply reconciliation

Observed: 2026-10-11. Scope: Fusion chain ID 32659 through retained block **15,130,080**, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f` (2025-10-07 08:43:50 UTC). This is not a present-day network or reserve audit.

This follow-up to [factory and mint authority](factory-and-mint-authority.md) finds historical administrative transactions and reconciles BUSD's zero-address Transfer events with its retained supply. [Raw evidence and capture times](../evidence/2026-10-11-history-review/) and the [SHA-256 manifest](../evidence/2026-10-11-history-review/manifest.json) support the observations below. All retrievals were read-only.

## Collection coverage

Factory `0xda30862467559b8b6e46d587aa9af5f48088c98b`: four ascending explorer transaction pages contain 1,000 + 1,000 + 1,000 + 986 records, with 3,986 unique hashes. A separate descending 1,000-record page agrees with the corresponding hashes. The requested range was block 0 through 15,130,080. This exhausts the returned top-level transaction listing under the observed pagination behavior; it does not cover internal calls or independently establish index completeness.

| Input classification | Count |
| --- | ---: |
| Factory creation transaction | 1 |
| `createToken(string,string,uint8,uint256)` selector | 3,933 |
| `setController(address,bool)` selector | 38 |
| `transferOwnerShip(address)` selector | 3 |
| `pauseToken(address,bool)` selector | 8 |
| `setBlock(address,address,bool)` selector | 3 |

Counts classify transaction inputs, not proven token deployments. Function names are interpreted from candidate interfaces and matching Ethereum Keccak selectors; the full Fusion source remains unverified. All 52 administrative transactions above have successful node-retrieved receipts. Their success does not by itself establish every intended storage effect.

BUSD `0x9d7005f9175856bda5deb153f86a11153d9d0fa7`: explorer Transfer queries requested blocks 11,025,309 through 15,130,080, filtering zero sender or zero recipient. The mint query returned 120 records at limit 1,000; the burn query returned five at limit 100. No duplicate transaction-hash/log-index pairs were found within either set. The initial 100-record mint capture is an overlapping discovery page and must not be added again to the totals.

Explorer and gateway may share upstream data. Their agreement is not independent-provider corroboration. Receipt checks cover all administrative calls and seven selected BUSD transactions, not every mint transaction. Dates below use explorer timestamps; corresponding block headers were not separately retrieved in this pass.

## Ownership-transfer calls

The following successful calls form a consistent sequence from the deployment sender to the owner observed at the retained block:

| UTC | Block | Sender | Encoded new owner |
| --- | ---: | --- | --- |
| 2024-01-27 07:05:39 | 11,030,038 | `0xda810a9d52a86ce51ee28defbd8f026d4e363541` | `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875` |
| 2024-08-23 20:48:31 | 12,419,429 | `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875` | `0x02e8f2ad8221cb59fb15248afc265bb8d32ed4e3` |
| 2024-09-05 07:38:49 | 12,501,793 | `0x02e8f2ad8221cb59fb15248afc265bb8d32ed4e3` | `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1` |

Transaction hashes, in table order:

1. `0x0e0973bcb3b13045bd04ce4e5998be4770754adc305e93a8a9d7d4cd4106be8c`
2. `0xf9c2b3493500a94067f27a29be124bac4f34f77b23d04deb6a5febb7155b2fbd`
3. `0x4a9e9c708134e583ece06fb32b7c22c706416f3cc1025b4e42bd4ae5ecabe37a`

**Inference:** these represent successive ownership changes. The calldata, successful receipts, subsequent senders and final getter agree. Historical pre/post storage reads and complete internal-call coverage are unavailable, so this is not a fully reconstructed storage history. No human identity is assigned to these addresses.

## Controller settings

The 38 controller-setting calls name 16 distinct addresses. Sorting successful calls by block and transaction index and taking each address's last encoded boolean agrees with `isController(address)` for all 16 at the pinned block. Four return true:

- `0x09a9bf7de279304953f7d4f70f57f874db77e77c`
- `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1`
- `0x3668c219b1fa8fe8175158f6ce91ded36fde9152`
- `0xa858be972e166411e409b67553ea7dd9867d3059`

This is a verified subset discovered from top-level calls, not proof there are no other controllers. The recorded owner enabled itself on 2024-09-11, and the latest observed controller-setting call enabled `0x09a9bf7de279304953f7d4f70f57f874db77e77c` on 2024-10-18. Ownership and controller status must remain separate roles.

## BUSD event totals

Amounts use the token's observed 18 decimals. These are Fusion token units, not a statement of dollar value or source-chain collateral.

| Quantity | Events | Exact raw units | BUSD token units |
| --- | ---: | ---: | ---: |
| Transfer from zero address | 120 | 1901947310901214866171924 | 1901947.310901214866171924 |
| Transfer to zero address | 5 | 859866890200848809065438 | 859866.890200848809065438 |
| Difference | — | 1042080420700366057106486 | 1042080.420700366057106486 |
| `totalSupply()` at retained block | — | 1042080420700366057106486 | 1042080.420700366057106486 |

The exact integer difference equals the node's `totalSupply()` result. This supports aggregate consistency of the collected mint/burn interpretation. It does not prove every event was collected: offsetting omissions could still reconcile, and source-chain assets are outside this calculation.

The returned mint events span 2024-02-02 11:31:18 through 2024-05-17 03:17:32 UTC. The five burn events are:

| UTC | Block | Raw amount | Address debited by event |
| --- | ---: | ---: | --- |
| 2024-05-13 04:23:02 | 11,738,186 | 859506153700 | `0x35cbec6a804556b460cd6007b23608329ccf8c65` |
| 2024-05-13 04:56:40 | 11,738,341 | 859506153699140493846300 | `0x35cbec6a804556b460cd6007b23608329ccf8c65` |
| 2024-06-24 13:55:00 | 12,019,463 | 221743652889408768851 | `0x16e86410250731c03bff31515ceca34def4a8e4d` |
| 2024-12-08 11:41:03 | 13,125,052 | 136823229278454247587 | `0xd4ec25e27c55d858e020534eb4ff6209998e9a78` |
| 2024-12-08 11:45:37 | 13,125,073 | 2169618680946049000 | `0x87756658e967d7be329c8b6852f1150c5327b68c` |

All five have successful receipts containing the matching BUSD log. Their top-level calls target BUSD with selector `0x9dc29fac`, matching `burn(address,uint256)`. The first three were sent by `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875`; the final two by `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1`. An event's debited address is not necessarily its transaction sender or initiator.

## Two concrete tracing leads

**Equal-amount burn and mint.** The second burn above is transaction `0x465081d8e38ac19522989181c963ba54bdb35438a79ff06f4d29ffebed7803ac`. Four days later, transaction `0x57c44db8ecb46343edb6856c74b40f3212f025c07e3c0b2ae4fa1fb0edc6fa01` mints exactly the same 859506.153699140493846300 units to `0xf11b4db13148d56e2a10ad701ad0ff338c62c52c`. Both transactions are sent by `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875`, directly to BUSD. **Hypothesis:** these operations may be related to a balance reallocation. Their purpose, authorization outside the contract, and any source-chain movement are unknown; equal amounts do not establish a bridge withdrawal or deposit.

**Earliest returned mint uses another contract.** Transaction `0x2a579e739a7cee8d3eaa5a6a6cd2a21cc80a375d8f012893b1b49c86e209635e` is sent by the factory deployer to `0x9851034eca59466ec346425a4674f93ef14d261f` with selector `0x85a6a020`. Its receipt contains a BUSD zero-sender Transfer for 3.048703795875905039 units to that same destination address. This makes the destination a contract-interaction lead; it is not established as a source-chain vault. A `debug_traceTransaction` attempt with `callTracer`, timeout 3 seconds and `reexec: 0` returned `required historical state unavailable (reexec=0)`. The exact internal call path remains unverified.

## Reproduction and next steps

Follow-up: [BUSD migration and balance flow](busd-migration-and-balance-flow.md) now examines the earliest mint's liquidity context and reconciles both addresses in the equal-amount pair.

Replay the recorded explorer URLs and node requests only against the stated chain and retained block. Preserve pagination, response errors and block hashes. Sum event `data` as unsigned integers; deduplicate by transaction hash and log index. Subtract zero-recipient totals from zero-sender totals and compare with `verification.response.json` entry `busd-totalSupply`. Pair receipts by hash and compare event address, topics, data, block hash and log index.

Next, inspect the earlier contract-interaction lead's deployed code and historical transactions, and follow both addresses in the equal-amount burn/mint pair on Fusion. Source-chain correlation needs independently retrieved public evidence; no custody attribution or service-disruption date follows from these observations.
