# Earlier Rollux vault input: direct Syscoin mint

Reviewed 2026-10-15. The **199,899.699967** input identified in the [earlier-vault review](usdc-rollux-distribution.md) is now traced to a direct mint of the Syscoin parent token. This differs from the later [September route](usdc-rollux-roundtrip.md), for which a Fusion source event and memo were found. No equivalent Fusion source linkage is established for this February route.

## Receipt-supported route

All times are UTC on **2024-02-16**. Amounts are token units, not verified dollar reserves.

| Time | Chain | Event | Amount | Evidence |
| --- | --- | --- | ---: | --- |
| 14:41:16 | Syscoin | Parent token minted to 8a4a | 199,899.700003 | [Transaction](https://explorer.syscoin.org/tx/0xbb956778aa0cdcbf4014c33b372bfdfedae00da6ca356f4aaa35d6f0b8f09884) |
| 14:50:11 | Syscoin | 8a4a transfers to 8a85 | 199,899.699967 | [Transaction](https://explorer.syscoin.org/tx/0xfad5f7fc062392f4d36a73096032bb6217b89df2d67b4d4ec5f01943939b9578) |
| 14:57:46 | Syscoin | 8a85 locks parent tokens in the standard bridge | 199,899.699967 | [Transaction](https://explorer.syscoin.org/tx/0x698265c8f572bab325f9105e4f226e55ade7adcba1c5a6d1112ab9b375387d75) |
| 15:01:41 | Rollux | Bridge mints to 8a85 | 199,899.699967 | [Transaction](https://explorer.rollux.com/tx/0x97694881ac32e6b0ba1e27033ccdc181abba40bc63456ab76d6b23e834a1cdc8) |
| 15:13:01 | Rollux | 8a85 transfers to 35cb | 199,899.699967 | [Transaction](https://explorer.rollux.com/tx/0x1f685ba848b09b251c0b98e999f5d3f4fbdf7e90cad5c205dcdc309083bcb073) |
| 16:02:33 | Rollux | 35cb transfers to vault 8a4a | 199,899.699967 | [Transaction](https://explorer.rollux.com/tx/0x690980602603ee9c0e3ce3beff03225efba18d2f22acb6e1650a4e68bc705e3f) |

The first transaction goes directly to the [Syscoin parent token](https://explorer.syscoin.org/address/0x3fca6743e2fb55759fee767f3a68b2c06d699dc4), with selector `0x40c10f19` and two argument words specifying recipient and amount. Its receipt has a Transfer from the zero address. The sender is [0261…6986](https://explorer.syscoin.org/address/0x026151a366235fb32b6bedc9f45f154f17846986); this identifies the transaction signer, not a person or an authenticated organizational role. The retained receipt contains no source-chain identifier or deposit memo that establishes funding for this mint.

The Syscoin portal event contains the complete Rollux relay calldata byte-for-byte. Transfer receipts agree on parent token, Rollux token, recipient and bridged amount. The **0.000036** difference between minted and forwarded amounts is recorded without assigning fee semantics.

The [intermediate Rollux address 35cb](https://explorer.rollux.com/address/0x35cbec6a804556b460cd6007b23608329ccf8c65) has exactly one incoming and one outgoing token event from block zero through the vault transfer. Both are for the same amount, and its historical `balanceOf` is zero at that block. This supports a direct pass-through at this address. It does not establish ownership across chains.

## Where attribution stops

The [receiving Rollux vault 8a4a](https://explorer.rollux.com/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1) subsequently mixes this input with other deposits and withdrawals. Its later 237,535.648990 transfer to the next vault is not assigned entirely to this mint. The [distribution review](usdc-rollux-distribution.md) retains those separate totals.

A bridge lock demonstrates backing by units of the Syscoin parent token. It does not establish independent USDC reserves behind that parent. Conversely, the absence of a source identifier in this direct mint is not proof that no funding existed elsewhere. No missing-dollar amount, new customer liability, or additional vault balance is inferred. The existing site comparison remains unchanged.

## Reproduction

Run `./research/check-rollux-earlier-route.ps1`, included in `./verify.ps1`. It checks the new capture hashes, selected receipts and headers, exact bridge relay payload and intermediate history. [New evidence](../evidence/2026-10-15-rollux-earlier-route/) preserves anonymous public RPC responses. The final vault-transfer receipt/header is already retained in [the distribution package](../evidence/2026-10-15-rollux-distribution/). The [parent-token history](../evidence/2026-10-15-rollux-origin/syscoin-state.response.json) preserves the selected Syscoin Transfer events in their wider history.
