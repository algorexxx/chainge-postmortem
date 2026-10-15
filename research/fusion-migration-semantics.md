# Fusion migration review: blocked balances are not cancelled claims

Reviewed 2026-10-15 at Fusion block **15,130,080**, **2025-10-07 08:43:50 UTC**. The newly captured head still matches this reference. [Backup provenance](fusion-snapshot-provenance.md) limits what can be said about later activity. All calls in this review were read-only; nothing was signed or broadcast.

## Result

The two large legacy balances are **blocked, still recorded, and not burned**. Blocking is reversible under the matched contract code. The legacy transfers, blocking calls and equal newer mints show closely coordinated timing and shared administrative addresses, strengthening a migration hypothesis. They do not establish that the newer issuance legally or technically extinguished the old claims.

**No migration deduction is justified yet.** A blocked balance is neither freely transferable at this snapshot nor automatically a cancelled liability. The supply comparison remains unchanged.

## Parallel BUSD and USDC sequence

Both assets use legacy holder [7bc4…695e](https://fsnex.net/address/0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e) and newer mint recipient [8a85…4e84](https://fsnex.net/address/0x8a85c29369ac6a12bfad9da45b9c9311d1e14e84).

| Operation | BUSD | USDC | Receipt/header result |
| --- | --- | --- | --- |
| Transfer into legacy holder | [859506.1537](https://fsnex.net/transaction/0x3646f153d092ca1c088f69fe5c705a87b9cc888049d19412c611f3dddf4309fd) | [2677846.972295](https://fsnex.net/transaction/0x349d41aeb3198d652cf7758d5012ab48c258eb14c4e67c6fde5ba649dcad1cdf) | Both successful in block 10,982,823, January 20, 2024 04:12:06 UTC |
| Block that holder | [Blocking call](https://fsnex.net/transaction/0xbd7f46a14b21437abd1a29e70bb14393c201ddcf705df7b71be2402a51474699) | [Blocking call](https://fsnex.net/transaction/0x5808e2c8853fc0e6495dcad5ee01eba7756984151c59816852f62fec1b3301ff) | Both successful in block 10,982,945, January 20, 2024 04:38:37 UTC |
| Equal newer issuance | [859506.1537](https://fsnex.net/transaction/0x8cabe4959081211d7a2d4aac1fd2e3c7777b3bb1188b85907b9a6d285278429f) | [2677846.972295](https://fsnex.net/transaction/0xa7c577ee69d7329a46796b4abac3d3f3b616c4da178f5c8eb6bbbb0b885d07a8) | February 4, 2024: USDC at 13:16:19 UTC, BUSD 39 seconds later |

The two legacy transfers share transaction sender [15a9…08e9](https://fsnex.net/address/0x15a9741998a96f3a9f9feb4c46634c4d546008e9). The blocking calls share [1e1d…bc66](https://fsnex.net/address/0x1e1d8d811c11d33b75ab66573282fba28904bc66). The newer mints share [cd66…f875](https://fsnex.net/address/0xcd66ae3529d74cd1d7185fe6882a3b80b791f875). These establish address-level coordination, not common beneficial ownership or a person's identity. Neither newer-mint receipt contains a matching legacy burn.

## Contract mechanics and state

The retained official FRC759 source compiled with Solidity 0.8.12, optimizer 200 runs, matches both legacy parents and both full-time child runtimes after normalizing only the compiler metadata IPFS digests. Parent runtime length is 15,419 bytes; child runtime is 5,513 bytes. BUSD is now covered by the same runtime comparison previously established for USDC. The source and compilation method are documented in [the earlier source-match package](../evidence/2026-10-13-usdc-fusion-source-match/README.md); this review preserves new node code and exact differing byte offsets.

- `blockUser` and `unblockUser` only change a Boolean mapping. They do not change token balances, burn supply, or mint a replacement.
- Child transfers check that sender and recipient are not blocked. The parent routes transfers through the child. Direct child transfers also require a separate global permission.
- Controller burning is a distinct operation. In the matched source, it can burn a blocked account's balance while the contract is unpaused.
- Both parents are unpaused at the reference; direct child transfers are disabled. The large legacy holder is blocked in both parents.

Read-only `eth_call` simulations from the blocked holder reject parent transfers with `Slice: sender blocked`. Direct child calls reject with `Slice: slice transfer not allowed`. Separate simulations using the recorded owner as hypothetical sender accept `unblockUser` and a one-raw-unit burn. These do not prove possession of the owner's key or that the controller ever performed those actions. Follow-up getters still return the original blocked flags and full balances, as expected for non-persistent simulations.

Historical balance/flag calls around the January transactions returned **missing trie node** errors. The backup retains usable headers and receipts but lacks the requested historical state. Errors were preserved and never treated as false flags or zero balances. Thus uninterrupted blocking between January 2024 and the snapshot remains unproved. The matched source does not emit an event for changing the block list.

## Reconciliation boundaries

Amounts are token units. Rows are categories, **not an additive liability calculation**.

| Category | BUSD | USDC |
| --- | ---: | ---: |
| Newer contract supply | 1042080.420700366057106486 | 7061027.518489 |
| Legacy full-time at address one | 1007499579.438009673238337766 | 996416657.619439 |
| Legacy full-time at blocked holder | 859506.1537 | 2677846.972295 |
| Legacy full-time at other, non-blocklisted holders | 7690.711381511853500767 | 5258.926094 |
| Additional deduction for these two possible overlaps | 0 | 0 |
| Consolidated customer claims | Not established | Not established |

The last legacy numeric row is now supported by checking every positive legacy holder's blocking flag, not merely by subtracting an assumed duplicate. It still does not identify customers or establish redemption rights. Address one is not blocklisted in either parent and has no special burn treatment in the matched source. Proven same-receipt conversions elsewhere already contribute burns and mints to the respective contract ledgers; they must not be deducted a second time.

Flags were checked for all **203 unique asset/legacy-holder pairs**, covering all **216 positive legacy positions** across the ten legacy child contracts. Additional blocked dated balances exist: BUSD **66192.1719379491064** in one remaining-time child; USDC **12.442006** and **455.626954** in its two remaining-time children. These remain separate dated rights, not additions to full-time claims. The UI derives the per-contract blocked subset from the checked positions. Newer-token blocking flags are not inferred from legacy flags.

For the three largest newer holders of each asset, one-raw-unit transfer simulations succeed for all three BUSD positions and two USDC positions. The USDC holder [e4a2…4c1b](https://fsnex.net/address/0xe4a2505146d097df6ff6e4c3e9d3d31eec7c4c1b), holding **622376.614316**, rejects the tested transfer with `Token: token transfer not allow!`. The cause and scope of that newer-contract restriction are not established here. A successful simulation is not evidence of key ownership or practical redemption; a failed simulation is not proof the balance is extinguished. None of these newer balances is deducted.

Identified vault balances and the separately displayed unverified Rollux balance remain in the aligned comparison in [the holder review](fusion-claims-review.md) and asset overviews. This contract review changes the interpretation of positions, not source-chain reserve measurements.

## Reproduce and remaining evidence

[New evidence package](../evidence/2026-10-15-fusion-migration-state/) preserves requests, responses, missing-state errors, successful receipts, headers, non-persistent simulations, runtime comparisons and a SHA-256 manifest. `research/check-migration-state.ps1` verifies published flags and key results against these captures; `verify.ps1` invokes it before publication.

A liability adjustment still needs an authenticated replacement rule, an actual extinguishing operation or equivalent evidence linking the two claims. Controller records or official migration terms may establish such a rule; equal amounts and administrative blocking alone do not. Beneficial ownership of large newer holders and the economic treatment of address one remain separate unresolved questions.

Follow-up: the [newer-holder review](newer-holder-funding-and-restrictions.md) establishes that the 622376.614316 USDC position has an account-specific factory block, checks all 523 positive newer positions and traces the largest holders' funding. This resolves the restriction-cause uncertainty above. A preserved migration guide supports general contract replacement but does not establish cancellation of the two large blocked legacy balances; no additional deduction is applied.
