# Initial contract and node findings

Observed on 2026-10-11. Status: preliminary, reproducible technical observations. No source-chain custody address or person controlling assets has been established.

## Main result

Six addresses selected from the spreadsheet have identical deployed runtime bytecode and return the same address from a call using the selector for `factory()`:

`0xda30862467559b8b6e46d587aa9af5f48088c98b`

This is a useful shared contract relationship. It does not establish that this address is a vault, identify source-chain reserves, prove deployment by that address, or identify a human operator.

The saved FRC758/FRC759 source code explains historical designs, but it is not a verified source match for these six deployments. In particular, the sampled addresses revert on the tested `parent()`, `fullTimeToken()`, `owner()` and `paused()` calls. The earlier parent/slice model must not be assigned to them merely because they appear in a Chainge-related spreadsheet.

## Chain and time boundary

The owner-authorized Fusion gateway answered:

| Field | Observed value |
| --- | --- |
| Chain ID | 32659 (`0x7f93`) |
| Genesis hash | `0xc2422b1d9d16331be2a5b207c0783027d4419498003f729f4b9e9c5c1838623a` |
| Latest retained block | 15,130,080 (`0xe6dde0`) |
| Block hash | `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f` |
| Block timestamp | 2025-10-07 08:43:50 UTC |

All sampled contract-state reads below use the explicit block number, not a moving `latest` tag. This is the gateway's retained view. It does not prove a network-wide final block or that no activity exists after this date on another chain or node. Acquisition in October 2026 must not be confused with state as of October 2026.

Evidence: [baseline requests](../evidence/2026-10-11-contract-review/node-baseline.request.json), [responses](../evidence/2026-10-11-contract-review/node-baseline.response.json), and [capture metadata](../evidence/2026-10-11-contract-review/node-baseline.capture.json). Private connection details are omitted.

## Six-token sample

Selection: familiar asset labels from the supplied public spreadsheet, suitable for checking whether its addresses share a technical implementation. Symbols are labels, not proof of backing or equivalence to a source-chain asset.

| Spreadsheet label | Fusion address | Returned symbol | Returned decimals |
| --- | --- | --- | --- |
| XCHNG | `0xab1f7e5bf2587543fe41f268c59d35da95f046e0` | XCHNG | 18 |
| USDT | `0x8a20c13b42d7fe418f10f922f2cee06246c24269` | USDT | 6 |
| ETH | `0xd82fe57a2643fbc692585bcb88df1d5d9c636cc8` | ETH | 18 |
| BTC | `0x585f70e031c9c10e99b7904a741dc54fe1fed197` | BTC | 8 |
| USDC | `0xfc9e57d7543e0591824121fda6461fe2148a5966` | USDC | 6 |
| BUSD | `0x9d7005f9175856bda5deb153f86a11153d9d0fa7` | BUSD | 18 |

Each returned 4,134 bytes of runtime code. Comparing the complete hex payloads, not just their sizes, found them identical. Each `factory()` call returned the shared address above. The shared address itself has 8,979 bytes of runtime code, and its `owner()` call returned `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1` at the pinned block. This is a getter observation, not independent proof of the returned address's powers or identity.

The FSNEX source-verification endpoint returned empty source code and `Contract source code not verified` for both the sampled USDT token and the shared factory target. Its default `Proxy` field must not be treated as independently verified proxy analysis.

Evidence: [token requests](../evidence/2026-10-11-contract-review/token-probe.request.json), [token responses](../evidence/2026-10-11-contract-review/token-probe.response.json), [parent probe](../evidence/2026-10-11-contract-review/parent-probe.response.json), [factory requests](../evidence/2026-10-11-contract-review/factory-probe.request.json), [factory responses](../evidence/2026-10-11-contract-review/factory-probe.response.json), [factory code/owner response](../evidence/2026-10-11-contract-review/busd-verification.response.json), [USDT source lookup](../evidence/2026-10-11-contract-review/usdt-explorer-source.json), [factory source lookup](../evidence/2026-10-11-contract-review/factory-explorer-source.json).

## Historical source review

The following observations apply to pinned source revisions, not automatically to deployed contracts:

| Source | Observed design | Investigation consequence |
| --- | --- | --- |
| [ChaingeToken at 5489997](https://github.com/ChaingeFinance/ChaingeToken/blob/54899979e196778800980f02bbf48827ca8c88c7/contracts/ChaingeToken.sol) and its Controllable base | Owner or authorized controllers can invoke mint, burn and slice burn | Identify deployed versions and permission history before interpreting issuance |
| [FRC758 interface at 5489997](https://github.com/ChaingeFinance/ChaingeToken/blob/54899979e196778800980f02bbf48827ca8c88c7/contracts/interfaces/IFRC758.sol) | Transfer event includes start and end times | A standard three-argument ERC-20 Transfer filter will miss this event signature |
| [FRC759 at 6cfe366](https://github.com/ChaingeFinance/FRC759/blob/6cfe366b26de7b613a1bd5de306da7bab606de9e/frc759.sol) | Parent creates Slice contracts, including fullTimeToken; child contracts emit ERC-20 transfers | Discover parent/child relationships from deployment evidence, getters and SliceCreated logs; do not double-count representations |
| Same FRC759 source, Controller and FRC759Token | Owner manages controllers; controllers can mint, burn, pause and manage restrictions | These source permissions are hypotheses to test against matching deployments |
| Same source, _mint and _burn | _mint increases _totalSupply; _burn does not decrease it | Do not assume totalSupply is net outstanding liabilities or backing; verify implementation semantics |
| [OpenAPIExample at f603f5f](https://github.com/ChaingeFinance/OpenAPIExample/blob/f603f5f3ff36fbb9102d59c3adcfd43c7ed94807/src/openapi/index.js) | Client requests getTransferToMinterRaw, signs/broadcasts the returned transaction, then submits its hash as certHash | Historical transaction payloads and receipts can reveal actual destinations; the example does not supply a fixed vault map |
| [Raw decoder at f603f5f](https://github.com/ChaingeFinance/OpenAPIExample/blob/f603f5f3ff36fbb9102d59c3adcfd43c7ed94807/src/utils/utils.js) | Decoder extracts to, value and data from the serialized transaction | Decode calldata as well as the transaction's top-level destination; an ERC-20 transfer targets its token contract at the transaction level |
| [Chainge_Sign_Example at 4dffafc](https://github.com/ChaingeFinance/Chainge_Sign_Example/blob/4dffafc437c23e5576d261cc84d7c67f159d6ae2/src/utils/generateSign.js) | Example builds an HMAC request signature from API parameters and headers | API authentication signing is not evidence of custody-key signing or an MPC participant |
| [Chainge_SDK_V1 at b5745d2](https://github.com/ChaingeFinance/Chainge_SDK_V1/tree/b5745d2feb3f25fd0e57947fa3f1125c5a41526a) | Captured default tree contains only LICENSE | Repository name alone does not establish preserved SDK implementation |

Both legacy read-only support-list endpoints named in OpenAPIExample failed DNS resolution from this environment. No API asset mapping was recovered. Archived scripts were read, not executed; no transaction was signed, submitted or broadcast.

## First BUSD historical observation

The public explorer's ascending transaction query for the spreadsheet's BUSD address returned an approval transaction at block 11,071,984:

`0x35c7a08175e77c2f08c64aa5c2305f14ec41fe43ee0bbf97518cc9e7d831c089`

The gateway independently returned the transaction and a successful receipt for the same hash and block hash `0xaf499ffc629ebaf9730196ddafc398d10e5b0ba75df9724f2535672294bc099d`.

- Sender: `0x8a4aa176007196d48d39c89402d3753c39ae64c1` on Fusion.
- Token called: `0x9d7005f9175856bda5deb153f86a11153d9d0fa7`.
- Approved spender: `0x1e0cc9b982c7c3fb1ec704c28f48c35e7bed08a1`.
- Approved raw amount: `2^256 - 1`, supported by both calldata and the Approval log.

This records an allowance, not a token transfer, deposit, withdrawal or reserve balance. It does not establish the sender's role, a source-chain vault, or cross-chain common control. Explorer and gateway agreement checks consistency across retrieval methods; they are not independent infrastructure providers. The first two indexed transactions are not claimed to include deployment or complete contract history.

Evidence: [explorer response](../evidence/2026-10-11-contract-review/busd-first-transactions.json), [explorer query metadata](../evidence/2026-10-11-contract-review/busd-first-transactions.json.capture.json), [node requests](../evidence/2026-10-11-contract-review/busd-verification.request.json), [node responses](../evidence/2026-10-11-contract-review/busd-verification.response.json).

## Access capabilities tested

| Test | Result | Limit |
| --- | --- | --- |
| Fusion code and eth_call at retained head | Successful reads and explicit reverts preserved | State at one block only |
| Fusion eth_getCode at block 1,000,000 | Missing trie node error | Historical blocks/receipts do not imply arbitrary historical state availability |
| Fusion historical transaction and receipt at 11,071,984 | Returned successfully | One transaction, not a completeness test |
| Fusion native callTracer for the retained head's one transaction, timeout 3s, reexec 0 | Returned a call frame | Confirms this bounded trace only; historical replay and contract-call tracing remain unqualified |
| BSC documented public RPC, individual chain ID/head/block-1 queries | Returned chain ID 56, head number and historical block 1 | No claim of archive state or complete receipts/logs |
| BSC batch including a single-block eth_getLogs query | Entire batch rejected with code -32005 and a method/rate-limit message | No log data obtained; use an appropriate provider or explorer before planning full-history extraction |

BSC endpoint provenance: [BNB Chain wallet configuration](https://docs.bnbchain.org/bnb-smart-chain/developers/wallet-configuration/). Requests and responses are in the evidence directory below. No node configuration, service lifecycle or database was changed.

## Reproduction and remaining work

The reviewed [evidence directory](../evidence/2026-10-11-contract-review/) contains exact read-only JSON-RPC requests/responses and capture metadata. [manifest.json](../evidence/2026-10-11-contract-review/manifest.json) hashes all promoted files. Acquisition sidecars retain their original archive paths as historical locations; the manifest's filenames resolve in the evidence directory. Hashes cover the saved UTF-8 files, not HTTP wire bytes. The interface-hash helper capture has no separate acquisition timestamp and is not used as dated chain-state evidence.

To reproduce, use a Fusion node with the recorded chain/genesis and retained block, submit the saved requests, and compare block hashes before interpreting results. Validate expected response IDs and errors. Derive selectors with Ethereum Keccak-256, not standardized SHA3-256. Different historical-state retention may change availability. Preserve reverts and missing-state errors rather than translating them to zero.

Next work:

Follow-up: [factory and mint authority](factory-and-mint-authority.md) now documents the factory deployment, BUSD creation event and the tested mint-permission path. The remaining items below still require broader historical and source-chain work.

1. Select BUSD as the first tracing pilot. Establish its Fusion deployment transaction, factory relationship and mint/burn authority with code and events; the current approval is only an initial fixture.
2. Identify the actual implementation behind the common bytecode, including authority getters and factory behavior. Do not substitute the archived FRC759 code.
3. Compare a bounded broader sample of spreadsheet tokens against the discovered bytecode and factory, preserving exceptions. Six matching contracts do not establish a pattern for all 2,184 addresses.
4. Verify BNB Chain token identity and discover historical transfers with an accessible public source. Establish deposits/withdrawals with transaction-level evidence before attributing any custody address.
5. Qualify a bounded historical contract-call trace separately before relying on replay for internal movements. Do not launch chain-wide tracing against the retained node as a default next step.
6. Add announcement dates from reviewed exports, then define meaningful before/after periods. No service-disruption date has been inferred from the gateway's retained head.
