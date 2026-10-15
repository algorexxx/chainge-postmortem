# BUSD and USDC closing assessment

Reviewed October 15, 2026. This is a bounded closing pass, not a certification that every vault or customer claim has been found. **Both assets remain reproducible partial investigations.** Their published comparisons show observed Fusion positions and tested vault balances at stated cutoffs, with unverified backing separated.

## What this pass changes

- BUSD's small BSC balance disagreement is explained by a missing swap outflow; the event is restored, not written off. Broad Ethereum logs also add two zero-value records. [Current BUSD assessment](busd-case-assessment.md).
- Anonymous archive queries recover the configured Ethereum USDC history and the native/bridged USDC histories of five Polygon candidate vaults. Receipt and header captures corroborate the recovered events. Rate-limit and pruned-history failures are preserved alongside successful retries. These results establish provider-returned coverage for the requested contracts/holders, not complete vault discovery.
- BSC balance/replay probes find additional omitted USDC transfers. The bounded search does not eliminate the remaining history disagreements. Broad BSC acquisition hit a public-provider rate limit; Base's broad query exceeded its range limit. Those gaps remain visible rather than being treated as zero activity.
- The May 2025 17,000-USDC case has a further mixed-funds branch whose 18,150-USDC balance is independently observed at a later recipient. It remains inside the original case; it is not another vault, outflow or loss amount. [Trace and balance](usdc-terminal-review.md#october-15-follow-up-retained-18150-usdc-branch).
- The closing selection now includes **every** unmatched outflow meeting the configured material threshold within 90 days before each retained chain/token/vault's last outflow, plus explicitly retained older and post-snapshot cases. Missing dates retain material cases. Written findings are tied to exact event IDs and evidence references in the [BUSD](assets/busd-closeout.json) and [USDC](assets/usdc-closeout.json) registers.

A regenerated shortlist can change as missing receipts add swap context or history changes the last activity date. The registers preserve earlier reviewed cases as well. They are an assessment layer, not a replacement for the generated ledger or an accepted-settlement table. Derive current counts with `research/review-closeout.ps1`; do not sum reviewed outflows into a loss figure.

## Material disposition boundaries

| Case group | Current conclusion | What remains unknown |
| --- | --- | --- |
| BUSD December 2021 large payments | Documented transfers to a labelled exchange destination; kept explicitly in scope despite their age. | Credited account, payment purpose and accepted Fusion settlement. |
| BUSD January 2024 204,567.658… | Verified intermediate route to a Binance-labelled endpoint. | Exchange-internal conversion, ownership and later disposition. The FDUSD search did not establish replacement reserves. |
| BUSD November 2023 balancing candidate | Exchange route followed by a near-matching Ethereum return, documented together. | A shared settlement identifier or exchange records proving the relationship. |
| BUSD 80,000 and 40,000 older payments | Same-holder burns shortly before payment, with a 50-token difference, are possible redemption counterparts. | Authenticated fee and settlement linkage. Thresholds remain unchanged. |
| USDC Ethereum 416,050.404450 | Existing Netcoins/Hyperliquid trace remains attached to the original vault outflow; nearby incoming funding has its own Fusion issuance context. | The incoming deposit does not establish the outgoing payout's purpose or claimant. |
| USDC same-holder / routing candidates | Retained exact or near-exact destination context supports possible redemptions for specified cases. | Candidate matching does not authenticate settlement, fees or ownership. |
| USDC Polygon newly recovered cases | Original receipts, call inputs and bounded first-hop observations support case-specific review. Shared-router transactions are assessed from their receipts rather than unrelated global router history. | Direct payments can remain unexplained; a router or swap does not prove reimbursement. Short recipient windows do not cover all later activity. |
| USDC post-snapshot payments | Two swaps return ETH to the same vault; the later January payment reaches the documented OKX-labelled endpoint. | Fusion settlement after the recovered reference block is unavailable. |
| Rollux-labelled USDC | Direct-mint and round-trip routes, and the downstream distribution, are documented. Its vault balance stays separate. | Independently redeemable backing and the rights associated with related Fusion issuance. |

The linked registers retain individual transaction references and limitations. A first observed onward payment can mix prior funds and is not necessarily a full trace of the original units. An exchange label identifies the public endpoint only. No beneficial owner is inferred from address reuse or fund flow.

## Accounting conclusion

Use the [explicit treatment table](fusion-claims-review.md#current-accounting-treatment-october-15-closing-pass): newer standard supply, legacy/address-one positions, blocking flags, dated rights and vault-held Fusion inventory remain separate. Observed migration operations do not establish extinguished rights. Included source balances are token units in the tested inventory, not a guarantee of redeemable dollars. Rollux remains outside included backing.

The snapshot is the highest known recovered Fusion backup and continued beyond foundation infrastructure shutdown, according to the documented provenance. That does not independently prove a final global ledger. The late source-chain transfers cannot establish an absence of later Fusion burns.

## Completion and reopening

The bounded pass can finish with explicit unresolved findings. A **completed reconciliation** still requires consolidated rights, justified full vault inventory, token equivalence/redeemable backing, reconciled opening/closing histories and evidence-backed material adjustments. Neither asset meets every condition yet.

Reopen a case when new vault evidence, historical logs/state, a later reliable Fusion snapshot, authenticated routing/settlement data, rights documentation, or reliable recipient/exchange-account attribution becomes available. Do not keep extending arbitrary amount/time tolerances to remove unmatched events. Failed anonymous providers are documented attempts, not proof that all public sources are exhausted.

## Reproduction

Follow [CONTRIBUTING.md](../CONTRIBUTING.md) for the complete ordered procedure, including anonymous capture/resume, second-plan acquisition, history/state checks, rights treatment, tracing, the common UI and PR review. For existing assets, reconcile, run `research/review-closeout.ps1 -Asset <asset> -RequireReviewed`, review any changed findings, explicitly promote the generated report, then run `verify.ps1`. Verification is offline and rejects a missing material assessment or stale published report; it cannot certify ownership or completeness.
