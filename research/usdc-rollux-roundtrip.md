# USDC route through Fusion, Syscoin and Rollux

Reviewed 2026-10-15. The previously linked Rollux deposit can now be traced back to a Fusion token transfer. This establishes a route through token representations; it does not establish independent USDC reserves or ownership of the addresses.

## Observed route

All times below are UTC on **2024-09-11**, from retained block headers. Amounts are units of each chain's token, not independently verified dollar values.

| Time | Chain | Observed event | Amount | Evidence |
| --- | --- | --- | ---: | --- |
| 04:41:42 | Fusion | Holder transfers newer USDC into the Fusion vault; no burn in this transaction | 2,059,247.080950 | [Transaction](https://fsnex.net/transaction/0x16ec6fd9136b92937086e3727dd5d1ca049ea3db48db238ca330b0cfaa2a9318) |
| 04:44:22 | Syscoin | Controller mints parent tokens to the same address | 2,058,011.232659 | [Transaction](https://explorer.syscoin.org/tx/0x48826ef744c815480b960c1a4067ce61cc664d15068c907e18d8ca24ef7db157) |
| 06:01:48 | Syscoin | Holder transfers parent tokens into the standard bridge | 2,058,011.232659 | [Transaction](https://explorer.syscoin.org/tx/0xf80a706e84b85902a5b7956180935ae74a03ec4103ca3fa89c8e85c82af00d54) |
| 06:03:25 | Rollux | Bridge mints the Rollux representation to the same address | 2,058,011.232659 | [Transaction](https://explorer.rollux.com/tx/0xcfc074fd463be3deeb6ee9b29fa13660fd81efc25ef2387c84667445e8ce2767) |
| 06:07:53 | Rollux | Holder sends a portion to another address | 50,000.000000 | [Transaction](https://explorer.rollux.com/tx/0x5a096a26d4f53f45d4ef2efadd4f1cf6f964477d1dad27553a96800a3243efdd) |
| 06:10:13 | Rollux | Holder deposits into the Rollux vault | 2,008,011.232650 | [Transaction](https://explorer.rollux.com/tx/0x010d8150a55a50a1bcd72900e0f4c83f03d7992047866ca6129df8dc9d9427fc) |
| 06:11:03 | Fusion | Controller mints newer USDC to a different recipient | 2,006,806.415910 | [Transaction](https://fsnex.net/transaction/0x5f838c567c6942f143b46ff76bfeebe205ce92f53e73d5dcf618f582adcacbc9) |

The initial holder is [8a85…4e84 on Fusion](https://fsnex.net/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84), with the same address on Syscoin and Rollux. The first recipient is the [Fusion vault 66ff…ecf1](https://fsnex.net/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1). The final mint recipient is [f11b…c52c](https://fsnex.net/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c). Address continuity does not identify a person or beneficial owner.

## Why these transactions are linked

The Fusion router event and Syscoin controller call carry the same identifier. The Fusion memo explicitly names SYSEVM, the [Syscoin parent token](https://explorer.syscoin.org/address/0x3fca6743e2fb55759fee767f3a68b2c06d699dc4), recipient and exact minted amount. The Fusion receipt contains one token Transfer to a nonzero address, not a burn.

The Syscoin bridge receipt transfers that parent token into the [standard bridge](https://explorer.syscoin.org/address/0x9cc66f9b7b07f72a487ff751a7cbe281976fce7c). Its portal event contains the complete subsequent Rollux relay calldata byte-for-byte. The Rollux receipt mints the [Rollux token](https://explorer.rollux.com/address/0x368433cac2a0b8d76e64681a9835502a1f2a8a30) to the same holder for the exact locked amount. This is stronger evidence than amount and timing alone.

The holder's bounded Rollux Transfer history has four incoming mints and five outgoing transfers through the vault deposit. Earlier incoming amounts had already been transferred out. The final mint splits into the 50,000 transfer, the 2,008,011.232650 vault deposit and a 0.000009 remainder. The 50,000 recipient is [127e…8e9d](https://explorer.rollux.com/address/0x127e7766f65c7a35e73177aa104999a189048e9d); its ownership and purpose remain unresolved.

The last Rollux-to-Fusion leg has a matching identifier and memo naming the Fusion token, recipient and exact output. Its detailed verification is in the [controller funding review](usdc-controller-funding-links.md).

## Accounting implications and limits

This route is **not evidence of a fresh independent USDC deposit**. Its traced input was an existing Fusion token position. The standard bridge locks Syscoin parent units for the Rollux representation, consistent with [Rollux's standard bridge documentation](https://docs.rollux.com/docs/developers/bridge/standard-bridge/); this does not establish the parent token's independent USDC backing.

The starting Fusion tokens were transferred, not burned in that transaction. That does not prove they remained untouched afterward, or that vault inventory represents a separate customer claim. Later movements and fungible mixing prevent assigning particular surviving units to this deposit. No supply deduction, additional customer liability, or missing-dollar total is inferred from this route.

The finding concerns this selected route, not every Rollux token or all possible reserves. The Rollux balance remains separate in the overview because its backing is unverified. Differences between transfer and mint amounts are observed differences; fee semantics are not authenticated here.

## Reproduction and preserved sources

Run `./research/check-rollux-roundtrip.ps1` and `./research/check-controller-links.ps1`, both included in `./verify.ps1`. The checks validate identifiers, memo, receipts, bridge payload, amounts and selected headers. They do not certify reserve quality.

Public anonymous RPC and explorer responses, requests and hashes are retained in [the origin evidence package](../evidence/2026-10-15-rollux-origin/). The initial Fusion receipt is in [the holder evidence package](../evidence/2026-10-15-newer-holder-review/); the final mint linkage is covered by [the controller funding package](../evidence/2026-10-15-controller-funding-links/). The Syscoin explorer token-history capture is only its first page; the separately retained RPC Transfer query covers blocks zero through the pinned Syscoin head. Neither a recent parent-token supply nor a present owner call proves historical dollar reserves.
