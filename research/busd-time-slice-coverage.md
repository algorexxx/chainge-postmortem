# BUSD time-slice coverage and earlier representation

Observed 2026-10-11, using retained Fusion block **15,130,080**, dated **2025-10-07 08:43:50 UTC**. These are historical state observations, not present-day balances or a reserve audit. [Reviewed requests, responses and integrity manifest](../evidence/2026-10-11-busd-slices-review/) preserve the checks, including the unsuccessful ordinary-Transfer query.

## All five returned legacy slice contracts

The legacy parent `0xd169bfa08ae2c9bea4123a8760ba058afd647ed2` returns five `SliceCreated(address,uint256,uint256)` events in the requested genesis-to-retained-head range. The full-time child was already reconciled in the [ledger report](busd-ledger-reconciliation.md). The four additional children's Transfer histories and all observed nonzero-address balances have now been checked.

| Child | Start / end, Unix seconds | Transfer events | Observed nonzero addresses | Positive balances |
| --- | --- | ---: | ---: | ---: |
| `0x8f8c247a300312b805692a3bc49414e35dec64ce` | 0 / 18446744073709551615 | 35,552 | 776 | 23 |
| `0xba212492dd4b1d89e86fd97faadca3b519629618` | 0 / 1672531199 | 31 | 3 | 3 |
| `0xcb637ddc0731e9a6b721715c7c6fe058695aa08d` | 1672531199 / 18446744073709551615 | 1,596 | 120 | 35 |
| `0xd25377c665b37f2098e89ba39bf1bb8e5fe5e3b3` | 1704067199 / 18446744073709551615 | 603 | 74 | 34 |
| `0xabd59013a7e4e54e8346e0be9b2b1f3fd250e725` | 0 / 1704067199 | 57 | 3 | 3 |

Each additional ledger finishes on a page shorter than 1,000 records. Integer event accounting agrees exactly with every queried `balanceOf` result: **200 address/child pairs**, no mismatches and no negative nonzero-address balances. Each child's `parent()`, `startTime()` and `endTime()` also agrees with the creation event. These checks establish consistency for returned indexed history; they cannot rule out offsetting missing events.

The explorer's public network-status endpoint reports `indexedFromBlock: 0`, `indexedToBlock: 15130080`, and `historyState: HaltedComplete`. This is the index operator's reported coverage, not an independent verification of every block or log.

## Interval balances must not be added together as liabilities

The two finite intervals ended before the retained block, yet their child contracts still report positive raw balances. Expiration of a time interval and destruction of a token balance are different operations. Summing full-time, expired and later-starting child balances would double-count representations and ignore the time dimension.

The `0xcb637...aa08d` child has a large address-one position: **958841470.311324212309837958 units** at `0x0000000000000000000000000000000000000001`. As with the full-time child's address-one balance, this is not a zero-address burn or evidence of customer claims or recoverable reserves.

The later-starting `0xd253...e5b3` child has **68976.505917013336106034** units across 34 positive addresses. The largest is **66192.1719379491064** at `0xb114f2f2ea31ba83881e059d10a0329ff7d63be9`. This is a retained representation balance, not a conclusion about ownership, backing, transferability or entitlement. No recovery transaction was attempted.

## Earlier time-aware BUSD

For `0xcb02f42d8790738a78d47b4f67d242cce48f1de6`, the unfiltered explorer query returns **1,693 events**: 1,385 events with the five-argument Transfer topic, 307 with the ApprovalForAll topic, and one OwnershipTransferred event. The separate ordinary ERC-20 Transfer-topic query returns an empty array. Returned five-argument transfers span **2021-10-21 through 2022-03-15** and involve **237 nonzero addresses**.

The event history must not be processed as if it were a standard ERC-20 ledger. A trial reconstruction that adds/subtracts every interval containing the retained timestamp disagrees with 193 queried `balanceOf` values and produces 96 negative nonzero-address values. That trial model is rejected; its disagreement is not evidence of theft, negative on-chain balances or a specific index defect.

The preserved [ChaingeToken FRC758 source](https://github.com/ChaingeFinance/ChaingeToken/blob/54899979e196778800980f02bbf48827ca8c88c7/contracts/FRC758.sol) illustrates why contract-specific treatment matters: ordinary balances and linked time slices are distinct, slice operations alter their representation, and `_mintSlice` emits a full-time interval even though it stores a requested slice. **This source has not been compiled and matched to the deployed runtime.** It is an explanatory lead, not verified deployed behavior or a complete explanation of the failed trial model.

Both `balanceOf(address)` and `sliceOf(address)` were queried for all 237 observed addresses at the retained block. All calls succeeded. Four addresses return a positive ordinary balance:

| Address | Ordinary balance, 18 decimals |
| --- | ---: |
| `0x279094606327442b558afcd9601900fac9ea7bde` | 0.914233351472874092 |
| `0x2c38134a65eae40eafcc29fbe6bdb6ad6792ea63` | 0.000000000000000001 |
| `0xd85bb3644a94ee547c0858e2b76881f22e52452a` | 50.053363604226073458 |
| `0xffffffffffffffffffffffffffffffffffffffff` | 100043.968108552477284540 |

Thirty-five addresses have nonzero entries in the returned slice arrays. Only the four ordinary entries above cover the retained timestamp; 233 other nonzero returned entries have expired intervals. Returned arrays may include historical storage entries, so their amounts are not independently summed as live entitlements. The all-`f` address is reported as an address, without an unsupported claim about its controller or recoverability.

## Remaining limit

The newer token and all five returned legacy child ledgers reconcile to retained state. The earlier representation has an archived event history and an all-observed-address state snapshot, but **does not have a validated historical replay model**. Exact source verification and replay of its contract-specific operations would be needed to close that distinction. Representation balances from different generations must not be summed as independently backed BUSD.
