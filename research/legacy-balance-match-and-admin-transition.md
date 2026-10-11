# Legacy balance match and administrative transition

Observed: 2026-10-11. Fusion chain ID 32659. State reads use retained block 15,130,080, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f` (2025-10-07 08:43:50 UTC). Findings are historical and do not establish present-day balances or source-chain reserves.

This follows [legacy BUSD and funding origin](legacy-busd-and-funding-origin.md). [Evidence](../evidence/2026-10-11-bridge-review/) and its [manifest](../evidence/2026-10-11-bridge-review/manifest.json) preserve bounded transaction queries, node verification and a new public-source acquisition record.

## Exact amount found in the legacy token

The previously traced newer-BUSD issuance of **859506.153700000000000000 units** has an exact raw-amount match in two earlier legacy-BUSD transfers. Both representations report 18 decimals; the common raw amount is `859506153700000000000000`.

The legacy parent is `0xd169bfa08ae2c9bea4123a8760ba058afd647ed2`; its full-time child is `0x8f8c247a300312b805692a3bc49414e35dec64ce`. Successful node receipts confirm these transfers, with events emitted by the child:

| UTC from explorer | Block | Operation |
| --- | ---: | --- |
| 2024-01-19 07:55:51 | 10,977,225 | Direct parent `transfer` from `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c` to `0xe47de19563392a7aa5360ffb51352d0c1444abdd` for the full amount |
| 2024-01-20 04:12:06 | 10,982,823 | Parent `transferFrom` debits `0xe47de19563392a7aa5360ffb51352d0c1444abdd` and credits `0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e` for the same amount |
| 2024-01-20 04:38:37 | 10,982,945 | Successful call matching `blockUser(address)` for `0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e` |
| 2024-01-20 04:48:10 | 10,982,989 | Successful call matching `blockUser(address)` for `0xe47de19563392a7aa5360ffb51352d0c1444abdd` |

Transaction hashes in table order:

1. `0x031e34c79fa24b5aad720d8cc239326d4e5160e98cf32d1bf0833847d64d3a33`
2. `0x3646f153d092ca1c088f69fe5c705a87b9cc888049d19412c611f3dddf4309fd`
3. `0xbd7f46a14b21437abd1a29e70bb14393c201ddcf705df7b71be2402a51474699`
4. `0x667ddbcc4c4e617a0f6cf2224bbe8293d37e378d5b5f0685b9827ec5a1b8d9ce`

The `transferFrom` transaction sender is `0x15a9741998a96f3a9f9feb4c46634c4d546008e9`, distinct from the debited address. Its receipt also contains an allowance-update Approval event. This establishes the call and emitted effects, not the human authorization or purpose. Both blocking calls were sent by `0x1e1d8d811c11d33b75ab66573282fba28904bc66`.

At the retained block, parent `blocked(address)` returns true for both addresses. The recorded legacy balance of `0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e` remains exactly 859506.1537 units; the other address's balance is zero. The historical blocking calls and later flags are observations at separate times; uninterrupted blocking throughout the interval has not been proven.

On 2024-02-04, the same raw amount was minted in the newer token to `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84`, as documented in the preceding report. **Hypothesis:** that issuance could be related to the earlier legacy balance and blocking sequence. Exact quantity and chronology make it a specific lead, but do not prove a conversion, compensation, common beneficial owner or source-chain deposit. Do not add the two representations together as independently backed collateral or infer misconduct from the sequence.

## Legacy administration changed on 29 January 2024

Three successful transactions in block 11,044,868, timestamp 2024-01-29 12:42:27 UTC from the explorer, were sent by `0x1e1d8d811c11d33b75ab66573282fba28904bc66` to the legacy parent:

| Transaction index | Interpreted call | Argument |
| ---: | --- | --- |
| 235 | `removeController(address)` | `0x456325f2ac7067234dd71e01bebe032b0255e039` |
| 236 | `addController(address)` | `0x026151a366235fb32b6bedc9f45f154f17846986` |
| 237 | `transferOwnership(address)` | `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875` |

Hashes respectively: `0x390c0ce79056efaa7fa8e02b1f3ed36d523ed32aacc0abc41f716007bc4c8f6e`, `0xcbddee8a2a508bd71d580251a2d6054c924882647e009cf60278543b3f137204`, and `0x37ec4f61749a68356fc39ed85f55fb3cb43b64cba05ca9f7a2a11e0055e5e7fc`. Each receipt contains one administrative log. Selector names are based on the archived interface; add/remove selectors were independently derived with node `web3_sha3`.

The proposed new legacy owner is also the address in the newer factory's 27 January ownership-transfer call and the newer-BUSD mint. This provides a concrete administrative link between the representations. It does not identify a person, establish complete controller lists or replace historical storage checks.

## Bridge-reference search and source preservation

Two explorer windows returned fewer than the requested 1,000 top-level transactions: blocks 6,450,428–6,600,000 returned 198, and 10,900,000–11,050,000 returned 20. These are separated windows, not a continuous history. The early window contains a call matching `transferData(address,uint256,bytes)` whose payload is ordinary text; it was not treated as a bridge identifier. No explicit source-chain transaction reference was established from these inspected calls.

The preserved [OpenAPIExample](https://github.com/ChaingeFinance/OpenAPIExample/blob/f603f5f3ff36fbb9102d59c3adcfd43c7ed94807/src/openapi/index.js) passes `certHash`, chain, token and address fields to an off-chain order API after broadcasting a transaction. That example supports searching for preserved order records; it does not establish that all historical token operations used that protocol or that the metadata can be reconstructed from a token mint. No authenticated API call or transaction submission was performed.

Fusionite's preserved [chain-data page](https://www.fusionite.info/chaindata) links an unofficial [asset/chain spreadsheet](https://docs.google.com/spreadsheets/d/1neAzlrGX_60-EymjKP17rD9z1W7IZMlSY2VKNrb90xM/edit#gid=0). An unauthenticated XLSX export succeeded and was saved unchanged in the ignored archive as `archives/public/2026-10-11/bridge-review/cross-chain-pairings.xlsx`, SHA-256 `97343cda4310d343747b71eb7f9975ecee288d1e96e47a7fc31ef41e79ced84c`.

Read-only inspection found one tab, `Assets`, with 1,599 nonempty rows, no formulas and no cell consisting of a complete 20-byte hexadecimal address. BUSD appears on row 66. Columns Q, R, S and V contain a filled-circle marker under the row-3 headers FSN, ETH, BSC and AVAX. This is an unofficial chain-support lead, not a vault map. The workbook's data freshness, marker semantics and liquidity figures have not been independently verified. The workbook remains local pending publication review; only acquisition metadata is promoted.

## Next verification targets

Seek public records explaining the January legacy blocking sequence and February newer-token issuance, preserving attributed statements separately from chain facts. Source-chain correlation should begin with a specific independently verifiable transaction or historical order record, not the assumption that every mint was an external deposit. The collector must keep legacy parent/child roles, newer tokens, blocked balances and incomplete query coverage distinct.

Receipt checks cover the eight selected transactions saved in `verification.*`, not every transaction in the two windows. No pre/post historical state traces were obtained. Node and explorer may share the same upstream data. No source-chain vault is confirmed.
