# Fusion factory and BUSD mint authority

Observed: 2026-10-11. Status: deployed behavior verified for the specified calls and retained block; source-chain custody attribution unresolved.

The six-token sample in [initial findings](initial-contract-findings.md) shares a factory. This follow-up identifies its deployment and a BUSD creation event, then tests BUSD's mint permission path. No transactions were signed or broadcast. Simulations do not represent historical mint transactions.

## Scope and evidence

Chain: Fusion, ID 32659. State calls are pinned to block **15,130,080** (`0xe6dde0`), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`, timestamp **2025-10-07 08:43:50 UTC**. This is the gateway's retained view, not a verified current network head or a service-disruption date. Explorer and gateway results may share an upstream node; agreement is not independent-provider corroboration.

Reviewed requests, responses and capture metadata are in [the evidence directory](../evidence/2026-10-11-factory-review/). Its [manifest](../evidence/2026-10-11-factory-review/manifest.json) provides SHA-256 hashes of saved files. Connection details are excluded. The previous report preserves the baseline block and runtime-code evidence.

## Deployment and token creation

| Observation | Recorded value |
| --- | --- |
| Factory | `0xda30862467559b8b6e46d587aa9af5f48088c98b` |
| Factory deployment transaction | `0x60c343e680b4a9a4ca3c85d70d141583b9a438640d0b797eba330ffaa3d4cc98` |
| Deployment sender | `0xda810a9d52a86ce51ee28defbd8f026d4e363541` |
| Deployment block | 11,025,020; `0x1b425fb1042ff98027d147b3fdee8f07ef2bca9f6a786af8547822807f5bcc1c` |
| Deployment time | 2024-01-26 12:56:38 UTC, from retrieved block header |
| BUSD token | `0x9d7005f9175856bda5deb153f86a11153d9d0fa7` |
| BUSD creation-event transaction | `0x2ca7c059b8e04cd3aa6675232d11ee4c757bb30947a990f8cbe352819c173f63` |
| BUSD event block | 11,025,309; `0x971728a1ec7a675869b91b5312b49004ea85a53420612b41c40ae4fbe4d56c97` |
| BUSD event time | 2024-01-26 13:59:21 UTC, from explorer log response; block header not separately retrieved |

The factory deployment receipt succeeds and names the factory as `contractAddress`. The BUSD transaction also succeeds, is sent by the deployment sender to the factory, and emits one receipt log with the BUSD address indexed. Its first topic matches `TokenCreated(address)` using node-computed Ethereum Keccak-256. Interpreting selector `0x210f5dda` as `createToken(string,string,uint8,uint256)`, its calldata decodes to `("Binance USD", "BUSD", 18, 0)`.

Evidence: `factory-deployment.*`, `busd-creation-logs.*`, `authority.*` and `signatures.*`. The transaction receipt, event, token's `factory()` result and shared runtime support the creation relationship. A historical internal CREATE trace has not been obtained; the fourth argument's operational meaning is not independently established by decoding alone.

The sampled token runtime is 4,134 bytes. Its complete bytes, including metadata, occur inside the 8,979-byte factory runtime starting at zero-based byte offset 4,792. This supports a common implementation; it does not identify a person or establish backing assets.

## Permissions at the retained block

The earlier factory probe returned `owner()` = `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1`. The following calls to the deployed factory returned:

| Address tested with `isController(address)` | Result |
| --- | --- |
| `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1` (recorded owner) | true |
| `0xda810a9d52a86ce51ee28defbd8f026d4e363541` (deployment sender) | false |
| `0x8a4aa176007196d48d39c89402d3753c39ae64c1` (public Fusion approval sender documented in initial findings) | false |
| `0x0000000000000000000000000000000000000001` (test control) | false |

These four queries are not a complete controller inventory. For BUSD, factory `isPaused(address)` returned false and token `callBack()` returned the zero address. These observations apply only to the pinned state and interpreted interfaces.

Two `eth_call` simulations invoked BUSD `mint(address,uint256)` with recipient `0x0000000000000000000000000000000000000001` and amount 1 raw unit:

- Caller set to the recorded owner/controller: completed successfully with empty return data.
- Caller set to the test control: reverted with `Token: Only Controller!`.

A `debug_traceCall` of the successful simulation shows BUSD issuing a `STATICCALL` to the factory with selector `0xb429afeb` (`isController(address)`) and the simulated caller as argument. The factory returns true and the outer call completes without an error. The trace used `callTracer`, timeout `3s` and `reexec: 0`. See `mint-simulation.*`.

This directly establishes the tested mint entry point's factory permission check. The simulated caller is a supplied RPC parameter; success does not prove possession of that address's key. No state persisted. No conclusion is drawn about historical mint amounts, burn permissions, reserve backing, redemption access or who operated any address.

## Source comparison and retrieval limits

A [Base explorer source listing](https://basescan.org/address/0x2adaf20fc35221bac93c9362499e47f3f3701982#code) supplied candidate interface names for comparison. Its source has **not** been compiled and matched against the Fusion deployment. It is a discovery aid, not verified Fusion source or evidence that the Base address has a custody role. The captured page and extracted source remain in the ignored public archive.

Bytecode metadata points to IPFS CIDs `QmURAthwex3wjApeEqFNcuE24ZiFE4gvfWTfBwqMAM5AQo` (token) and `QmZVe8u6nYQty4oeXquBeUBdqdCzZD3YqZWgSsqkpmC7ZL` (factory), with compiler-version bytes corresponding to 0.8.21. Retrieval attempts through ipfs.io, and dweb.link for the factory, returned HTTP 403. This is an access limitation from this environment, not proof that the content is unavailable elsewhere. Failure captures are preserved.

## Reproduction and next work

Follow-up: [factory history and BUSD supply reconciliation](factory-history-and-busd-supply.md) now records administrative transactions and observed controller states, plus historical BUSD mint/burn evidence. It retains the distinction between successful calls, inferred state transitions and unavailable historical traces.

Use the saved request/response IDs to pair each query and result. Confirm the chain identity and pinned block hash before replaying state calls. Use Ethereum Keccak-256 for selectors and event topics. Keep errors distinct from zero or false. Capture sidecars provide acquisition times; the signature-hash helper has no separate timestamp and is not dated state evidence. Hashes cover saved files rather than HTTP wire bytes.

1. Reconstruct successful factory administrative calls and controller changes, including internal calls if accessible. Do not infer ownership history solely from deployment sender and retained `owner()`.
2. Collect a bounded BUSD mint/burn event history, preserving transaction receipts and call context where available. Establish actual operations separately from simulated permissions.
3. Extend the bytecode/factory check to more spreadsheet entries, documenting exceptions.
4. Correlate verified Fusion operations with independently retrieved source-chain transactions. Timing or amount matches alone cannot establish a vault.

No source-chain vault is identified by this report. The incident timeline and present-day asset movements remain open work.
