# Legacy BUSD structure and funding origin

Observed: 2026-10-11. Chain: Fusion, ID 32659. State reads are pinned to retained block 15,130,080 (`0xe6dde0`), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`, dated 2025-10-07 08:43:50 UTC. No present-day state or source-chain reserve claim is made.

This follows [BUSD migration and balance flow](busd-migration-and-balance-flow.md). [Raw evidence](../evidence/2026-10-11-legacy-review/) includes requests, responses, acquisition metadata and a [SHA-256 manifest](../evidence/2026-10-11-legacy-review/manifest.json). All operations were read-only.

## The funding address received a direct mint

The newer BUSD token is `0x9d7005f9175856bda5deb153f86a11153d9d0fa7`. The funding address identified in the preceding report, `0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84`, has one incoming and one outgoing Transfer event returned for blocks 11,025,309–15,130,080, using a limit of 1,000 per direction.

| UTC, from explorer | Operation | Exact newer-BUSD units | Transaction |
| --- | --- | ---: | --- |
| 2024-02-04 13:16:58 | Mint to funding address | 859506.153700000000000000 | `0x8cabe4959081211d7a2d4aac1fd2e3c7777b3bb1188b85907b9a6d285278429f` |
| 2024-02-06 06:47:28 | Transfer to `0x35cbec6a804556b460cd6007b23608329ccf8c65` | 859506.153700000000000000 | `0xd2e58fc384577136c17d62a46d271a8b6c5d9f9cedca646a4c8de5a65411dccc` |

The mint transaction directly calls newer BUSD with selector `0x40c10f19`, matching `mint(address,uint256)`, and encodes the funding address and amount. Its sender is `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875`, previously observed making factory controller-setting calls and the later burn/mint operations. A successful node receipt contains the matching zero-sender Transfer event. The outgoing transaction was verified in the previous report.

The funding address's newer-BUSD balance is zero at the retained block, matching the event difference. It has no runtime code at that block. Getter calls made directly to this code-empty address return empty `0x`; those are not valid owner, controller or supply values.

**Observation:** this extends the Fusion path backward to a direct issuance by an already identified controller address. **Unresolved:** whether that issuance represents migration, a source-chain deposit or another operation. No external deposit reference was established from the transaction calldata, and no human ownership is assigned.

## Legacy BUSD uses a parent and a full-time child

| Role supported by queried behavior | Address |
| --- | --- |
| Legacy parent | `0xd169bfa08ae2c9bea4123a8760ba058afd647ed2` |
| Full-time child returned by the parent | `0x8f8c247a300312b805692a3bc49414e35dec64ce` |

The parent's successful deployment receipt is transaction `0x735c91acd8e881768a895a67547a6c07be133fe666078d39fce1952fdd152192`, block 6,450,428. The explorer dates it to 2022-03-03 19:51:17 UTC. The deployment sender is `0x15526b2e10ef81a2ee4a810fd49ca50458639aca`; the receipt names the parent contract and includes a log indexing the child with start/end words zero and `0xffffffffffffffff`.

At the retained block:

- Parent `fullTimeToken()` returns the child above.
- Child `parent()` returns the legacy parent, `startTime()` returns zero and `endTime()` returns 18,446,744,073,709,551,615.
- Parent `owner()` returns `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1`, also observed as the newer factory's owner at this block. This establishes matching address values, not human identity or complete shared administration history.
- Parent `paused()` returns false. A singular `controller()` getter reverts; this does not mean no controllers exist.
- Both parent and child report a zero balance for the funding address. A `debug_traceCall` of parent `balanceOf(fundingAddress)` shows a `STATICCALL` to the child with identical calldata and matching zero return data. The trace used `callTracer`, timeout 3 seconds and `reexec: 0`.

The parent's 15,419-byte runtime contains FRC759 and Slice error strings. The archived [FRC759 source at the preserved commit](https://github.com/ChaingeFinance/FRC759/blob/6cfe366b26de7b613a1bd5de306da7bab606de9e/frc759.sol) describes a parent/full-time-slice structure compatible with these observations. No compiled bytecode match has been performed, and the explorer returns no verified source; the repository code remains a comparison source.

## Historical event-emitter check

Transaction `0x707bab52b01535ef50ce4150154808312aac40e7f419aa560fec842eaa9503ce` calls the legacy parent with the mint selector. Its successful receipt contains a zero-sender Transfer log emitted by the **child**, `0x8f8c247a300312b805692a3bc49414e35dec64ce`, to `0x829a3efcd90d237bbcd9a021d748705ae05b188e`. Saved calldata and log amounts agree.

This directly demonstrates that a parent-address-only Transfer query would miss this mint event. Future collection must distinguish the address called from the address emitting events. The full-time child is only one component; other time-slice contracts need separate discovery and must not be added together as if each represented distinct collateral.

## Coverage limitations and corrected query

Both broad child-history queries for the funding address, blocks 6,450,428–15,130,080, returned `NOTOK`: the explorer scanned 25,000 address logs without completing the page. These are **incomplete scans**, not empty histories. Narrowed queries for blocks 11,000,000–11,150,000 returned empty arrays in both directions. They cannot establish the absence of older or later activity, other time slices, or source-chain activity.

An initial balance-query construction split the selector and address into two malformed calls. Both failures are retained in `verification.*` and are excluded from conclusions. The corrected complete ABI calldata and successful results are in `child.*`. Errors and empty return data are never interpreted as zero balances.

The legacy explorer transaction capture contains only the first ten top-level transactions. Explorer and gateway may share an upstream node; their agreement is not independent-provider corroboration. Dates from the explorer were not separately verified against block headers in this pass.

## Implications for continued research

Follow-up: [legacy balance match and administrative transition](legacy-balance-match-and-admin-transition.md) records the same amount in earlier legacy transfers, subsequent blocking calls, and a change of legacy administration. Any connection to newer issuance remains a hypothesis.

The BUSD pilot now distinguishes a newer token contract, a legacy parent, its full-time child, and liquidity-migration activity. These roles should guide the eventual collector rather than treating every spreadsheet entry or token mint alike.

Next, preserve and investigate legacy administrative and conversion records in bounded block windows, locate any explicit source-chain references in documented bridge operations, and compare those with independently retrieved source-chain evidence. The direct mint found here is an issuance origin on Fusion, not a confirmed vault or proof of a deposit elsewhere.
