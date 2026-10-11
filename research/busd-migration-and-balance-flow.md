# BUSD migration evidence and balance flow

Observed: 2026-10-11. Fusion chain ID 32659. State reads use retained block 15,130,080 (`0xe6dde0`), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`, dated 2025-10-07 08:43:50 UTC. Present-day balances and source-chain reserves are outside this report.

This follows [factory history and BUSD supply](factory-history-and-busd-supply.md). [Query evidence](../evidence/2026-10-11-flow-review/) and its [integrity manifest](../evidence/2026-10-11-flow-review/manifest.json) preserve this pass. The earliest mint and burn/mint receipts were preserved in the prior history review; they are referenced rather than copied.

## Earliest returned mint: liquidity migration evidence

Transaction `0x2a579e739a7cee8d3eaa5a6a6cd2a21cc80a375d8f012893b1b49c86e209635e` on 2024-02-02 calls `0x9851034eca59466ec346425a4674f93ef14d261f` with selector `0x85a6a020` and one encoded address, `0x277b7e062172c7dcad64746e43003f18d334d053`. The selector's function name remains unknown: tested `migrate(address)`, `migrateLP(address)` and `migratePair(address)` do not match.

The called contract has 3,537 bytes of runtime code at the retained block, including the text `Migrate: newToken address error!` and `Migrate: only owner!`. Its `owner()` returns `0xda810a9d52a86ce51ee28defbd8f026d4e363541`; `router()` returns `0x62bc5327868a998e1b3d9fe6037273e642a137ae`. A successful deployment receipt for transaction `0xfb9fe43f8e1b127ac6e368fd2bab584fb1671a618ee7ded0cbde417fb8e0f25d`, block 11,069,564, names this contract and that same deployment sender. The explorer supplies no verified source.

The argument address's `token0()` and `token1()` calls return 96 bytes each, not ordinary single-address returns. Their first words identify the two legacy tokens below; the remaining words are zero and `0xffffffffffffffff`. No full ABI or meaning of those trailing words is established. The proposed new pair returns ordinary 32-byte token addresses:

| Candidate pair | First returned token | Second returned token |
| --- | --- | --- |
| Old: `0x277b7e062172c7dcad64746e43003f18d334d053` | `0x9636d3294e45823ec924c8d89dd1f1dffcf044e6` | `0xd169bfa08ae2c9bea4123a8760ba058afd647ed2` |
| New: `0x471645ee19c7580e16b07956a0daa8de15438369` | `0x8a20c13b42d7fe418f10f922f2cee06246c24269` | `0x9d7005f9175856bda5deb153f86a11153d9d0fa7` |

The legacy token getters report `Tether` / `USDT` / 6 decimals and `BUSD Token` / `BUSD` / 18 decimals, respectively. These self-reported names do not establish issuer or source-chain backing. The newer tokens were identified in the initial spreadsheet sample.

The historical receipt contains, in order:

1. A log matching `PairCreated(address,address,address,uint256)` from `0x487dbcacc7e6c072af998bfda4ba80621b2dc031`, identifying the newer USDT/BUSD pair.
2. Zero-sender Transfer events minting 3.069775 newer USDT and 3.048703795875905039 newer BUSD to the called contract, followed by approvals to the recorded router.
3. Transfers of those exact token quantities from the called contract into the new pair.
4. Pair-token zero-sender Transfer events, followed by logs consistent with reserve synchronization and liquidity minting. The `Mint(address,uint256,uint256)` topic was independently derived with node `web3_sha3`.

**Inference:** this transaction is consistent with migrating or recreating liquidity from a legacy Fusion pair into a newer Fusion pair. Contract strings, argument context, token getters and receipt events support this interpretation. It does not prove old balances were destroyed, that all liquidity was migrated, or that a source-chain deposit occurred. Historical internal tracing failed in the previous pass. A BUSD mint event alone must therefore not be classified as a new external deposit.

## Equal-amount burn/mint: balance reconciliation

Only the newer Fusion BUSD token is considered here. Address-scoped Transfer queries cover its creation block 11,025,309 through the retained head. Each direction uses limit 1,000 and returns fewer records; the empty result is retained as the explorer returned it. This is endpoint-reported coverage, not an independent proof of index completeness.

| Event | Token units | Evidence |
| --- | ---: | --- |
| Incoming transfer to `0x35cbec6a804556b460cd6007b23608329ccf8c65` on 2024-02-06 06:47:28 UTC | 859506.153700000000000000 | `0xd2e58fc384577136c17d62a46d271a8b6c5d9f9cedca646a4c8de5a65411dccc` |
| First burn on 2024-05-13 | 0.000000859506153700 | `0xe26a133ca2b6b44e38b5fc3a2f660de8e1efe04757d051770664a6be9376ff9a` |
| Second burn on 2024-05-13 | 859506.153699140493846300 | `0x465081d8e38ac19522989181c963ba54bdb35438a79ff06f4d29ffebed7803ac` |
| Mint to `0xf11b4db13148d56e2a10ad701ad0ff338c62c52c` on 2024-05-17 | 859506.153699140493846300 | `0x57c44db8ecb46343edb6856c74b40f3212f025c07e3c0b2ae4fa1fb0edc6fa01` |

The incoming transaction is a direct BUSD `transfer(address,uint256)` from `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84`; its successful node receipt agrees with the explorer log. The original address has one returned incoming event and two outgoing burn events. Integer amounts reconcile to zero, matching `balanceOf()` at the retained block.

The later recipient has one returned incoming mint and no returned outgoing BUSD Transfer events. Its retained balance is exactly the minted amount. Both addresses return empty runtime code at the retained block; this alone does not prove their historical account type or common control. The migration contract's retained BUSD balance is also zero.

**Hypothesis:** the equal-amount burn/mint pair represents a balance reallocation. The evidence establishes the numerical relationship and retained balances, not intent, human ownership or any movement of collateral. The tiny first burn is preserved exactly; no explanation is assigned to it.

## Limits and next work

Follow-up: [legacy BUSD and funding origin](legacy-busd-and-funding-origin.md) traces the funding address's newer-token issuance and verifies the legacy parent/child event structure. It also records the explorer's historical scan limit.

The unfiltered `busd-transfers.json` capture stops at its 1,000-record limit and is incomplete. It is not used as a complete history or added to the address-filtered results. Likewise, `minter-first.json` contains only the first 20 top-level transactions. Capture sidecars retain request URLs and acquisition times; all state calls are read-only, and no transaction was signed or broadcast. Explorer and gateway may share an upstream node.

Next, follow the public funding address `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84`, inspect the legacy BUSD representation and seek preserved migration announcements. Distinguish legacy-to-new Fusion conversion from new source-chain deposits before attempting vault attribution. No source-chain vault is confirmed by this report.
