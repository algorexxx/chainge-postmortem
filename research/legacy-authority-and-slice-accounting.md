# Legacy authority and time-slice accounting

Observed: 2026-10-11. Fusion chain ID 32659. State calls use retained block 15,130,080 (`0xe6dde0`), hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`, dated 2025-10-07 08:43:50 UTC. This report does not establish present-day permissions, balances or source-chain reserves.

This follows [legacy balance match and administrative transition](legacy-balance-match-and-admin-transition.md). [Raw evidence](../evidence/2026-10-11-legacy-authority-review/) and the [integrity manifest](../evidence/2026-10-11-legacy-authority-review/manifest.json) preserve the requests and responses. Retrievals and traces were read-only; nothing was signed or broadcast.

## Legacy ownership events

The parent `0xd169bfa08ae2c9bea4123a8760ba058afd647ed2` returned four `OwnershipTransferred` events for deployment through the retained head. Each was matched to a successful node receipt; block hashes and timestamps also agree with retrieved node headers.

| UTC | Block | New owner |
| --- | ---: | --- |
| 2022-03-03 19:51:17 | 6,450,428 | `0x15526b2e10ef81a2ee4a810fd49ca50458639aca` (initial assignment from zero) |
| 2022-03-03 19:51:30 | 6,450,429 | `0x1e1d8d811c11d33b75ab66573282fba28904bc66` |
| 2024-01-29 12:42:27 | 11,044,868 | `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875` |
| 2024-09-27 10:00:52 | 12,648,496 | `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1` |

Each event's previous owner agrees with the preceding event's new owner; the last agrees with the retained `owner()` getter documented earlier. The last transaction is `0x86ee226ff0b7cc2b254ef790638387ad1273c17adaf5eb7edd80459ac59c9054`. This is an event-supported sequence, not a proof that no unlogged storage changes occurred. It differs from the newer factory's ownership sequence, which includes another intermediate address and reaches the same final owner earlier in September.

## Controller permissions differ between generations

The parent event queries returned four `ControllerAdded` events and one `ControllerRemoved` event. Their addresses are in event data, not indexed topics. Each matches a successful receipt and a block header.

| Address | Observed legacy event history | Legacy `isController` at retained block |
| --- | --- | --- |
| `0x874de5761900ee526950d707d8cbd4b93092e290` | Added 2022-03-04 | true |
| `0x456325f2ac7067234dd71e01bebe032b0255e039` | Added 2022-07-11; removed 2024-01-29 | false |
| `0x026151a366235fb32b6bedc9f45f154f17846986` | Added 2024-01-29 | true |
| `0x61b54d2263b9249ae5aa4acd5acac880ed815efc` | Added 2024-01-30 | true |
| `0xcd66ae3529d74cd1d7185fe6882a3b80b791f875` | Former owner; no addition in returned controller events | false |
| `0x1d9a17f3097cee804b3c3717222a22b8eb55bcb1` | Retained owner; no addition in returned controller events | true |

The earlier newer-factory state checks returned false for both `0x026151a366235fb32b6bedc9f45f154f17846986` and `0x61b54d2263b9249ae5aa4acd5acac880ed815efc` at the same block. These addresses therefore have different observed controller status across the two contracts. The old contract's owner-inclusive authorization is consistent with the archived candidate source, but no full compiled-source match is claimed. This queried subset is not proof that no other controllers exist.

## The original balance accumulated over time

The address `0xda832772b9fa5610223d41b7c0f09eff9d21ca2c`, which sent 859506.1537 legacy BUSD on 2024-01-19, has 45 returned full-time-child Transfer events: 25 incoming and 20 outgoing. Five contiguous, nonoverlapping block windows cover child deployment through the retained head:

`6,450,428–7,649,999`; `7,650,000–8,849,999`; `8,850,000–9,999,999`; `10,000,000–11,050,000`; `11,050,001–15,130,080`.

Both directions were queried separately with limit 1,000. All responses are arrays below the limit, with either successful records or an explicit empty result; none reports the earlier scan-cap error. After deduplication by transaction hash and log index, both incoming and outgoing totals are exactly **1875649794185792013753854 raw units**, or **1875649.794185792013753854 token units**. The difference is zero, matching the retained parent `balanceOf()` result. This supports balance consistency for the returned full-time events; it does not independently prove explorer index completeness or account for every separate time slice.

Important points in the returned history:

- The first incoming event, 2022-03-05, credits 899999.5 units from `0x716daec7a67036a509d8786a7c4115f80b5cc92e`. Its successful transaction `0xc6772a86d41dac1a99a985b83ae9e53cc51525093a583b092ed9d83dc825ae35` is a call to `0x87dc8992d5370b0633afd201cd46ac4c81ad9b42`, rather than a direct call to BUSD. Its internal path and business purpose remain unverified.
- A direct controller mint on 2024-01-01 adds 2989.24318158 units. It is only one part of the accumulated balance, not the origin of the later 859506.1537 transfer.
- After the 2024-01-19 outgoing transfer, event arithmetic leaves **0.030048708641095100** units.
- On 2024-02-02 15:53:38 UTC (explorer timestamp), a child burn event removes exactly that remainder. Successful transaction `0x2134366509de7e8adc1e8999fb30adfda19794303d71f52be8de243385708fa9` calls `0x61b54d2263b9249ae5aa4acd5acac880ed815efc` from `0x07281e30b8ef4985e75233aafdf5d3f27a09ceb2`. The receipt confirms the amount; the top-level selector `0xba009887` and internal path have not been decoded. No later full-time events were returned for this address.

## A zero-sender event that represents slice conversion

Transaction `0x4a46f2f5cbb23a69c6f86981038113247e339e3ee0ec7f0809acf9ef2e06ef8e` on 2023-01-01 is sent by the same original address to the legacy parent. Its selector matches node-derived `mergeSlices(uint256,address[])`; its calldata names one slice, `0xcb637ddc0731e9a6b721715c7c6fe058695aa08d`, and amount `154485229260173438889446` raw units.

The successful receipt contains exactly two Transfer logs:

1. That dated slice burns 154485.229260173438889446 units from the address.
2. The full-time child `0x8f8c247a300312b805692a3bc49414e35dec64ce` emits a zero-sender Transfer crediting the same amount to the same address.

At the retained block, the dated slice's `parent()` points to legacy BUSD; its start is `0x63b0ccff` and its end is `0xffffffffffffffff`. These observations support a conversion between representations of the same parent asset. Counting the second event as a new external deposit would omit the simultaneous slice burn. Other zero-sender events require individual classification; this one example does not establish all of their meanings.

## Verification limits and next steps

Administrative-event times were checked against headers in this pass. Other dates are explorer observations unless explicitly stated otherwise. Receipts were checked for the nine administrative events, the January 2024 mint, first incoming transfer, one direct burn, the slice conversion and the residual burn. Remaining address-history logs were checked for shape, ordering, uniqueness and balance arithmetic, not individually against receipts. Node and explorer may share upstream data.

The next contract leads are the first incoming transfer's top-level target and the residual-burn target. Their roles must be established before using their counterparties for source-chain correlations. A collector needs separate classifications for direct mint/burn, slice conversion, migration, ordinary transfer and unresolved contract interactions. No source-chain vault or collateral total is established here.
