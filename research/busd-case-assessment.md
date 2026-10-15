# BUSD case assessment

Reviewed October 15, 2026. **Status: reproducible partial investigation.** The bounded closing review is described in [the shared assessment](closeout-assessment.md). The full accounting comparison is available in the common site and [asset package](assets/busd.md); it is not a consolidated customer-liability or confirmed-loss statement.

## Established observations

- The standard Fusion representations, positive holder state, newer issuance/destruction, legacy blocking and controller history have been checked. [Migration semantics](fusion-migration-semantics.md) and [newer holder funding](newer-holder-funding-and-restrictions.md) explain the observed mechanisms. They do not prove which legacy rights were extinguished or who beneficially owns large positions.
- Fifteen tested source-chain token balances are aligned to the Fusion reference time. The [asset package](assets/busd.md#balances-aligned-to-the-fusion-reference-time) lists them and the exact comparison. The inventory remains limited to tested candidate vaults and token representations.
- Historical deposit/mint and burn/payment candidates, verified liquidity-address configuration and successor vault transfers support the documented vault roles. See [source-chain flows](busd-source-chain-flows.md), [ledger reconciliation](busd-ledger-reconciliation.md) and [time slices](busd-time-slice-coverage.md).
- The January 2024 outflow is traced to an explorer-labelled Binance endpoint. [Its exchange path](busd-204k-exchange-path.md) establishes public transfers, not the credited account or subsequent internal disposition. The [FDUSD search](busd-fdusd-followup.md) did not establish replacement backing.
- Major earlier flows include observed swaps, a possible BSC/Ethereum balancing relationship and an Avalanche bridge return. These explanations must remain attached to their original cases; an asset conversion or exchange deposit does not by itself discharge Fusion BUSD claims.

## Closing corrections and remaining material cases

The old BSC replay discrepancy is explained by a previously missing **1.107024317425808905 BUSD** outflow at block 35,938,493, February 7, 2024 22:04:00 UTC: [transaction](https://bscscan.com/tx/0x9f61b9cce465032ae57b4e9dda7bc857cbe5b04fa977461011a7e17210bd99f4). It sends BUSD from the 8a4a vault through a router into a swap. Adjacent state falls from 1.187246977608202752 to 0.080222660182393847 BUSD, matching the pinned comparison balance. [Captured logs, receipt and state](../evidence/2026-10-15-busd-gap-localization/) repair the ledger; this is not an accounting write-off. Endpoint agreement does not prove every historical transfer has been found.

Two older Ethereum payments in the material terminal review have same-holder Fusion burns shortly before them:

| Original vault payment | Possible Fusion counterpart | Interpretation limit |
| --- | --- | --- |
| [80,000 BUSD, March 18, 2022](https://etherscan.io/tx/0x342b13a83b2e5f0e8f7dddffabdfbbb6cb63e11ebee827b8e195032ac2bdf864) | [80,050 burned 308 seconds earlier](https://fsnex.net/transaction/0x83a5702b2dbff529bc0f649790fb13fe060d95a6ca5995644fa3e0ef65a52ca9) | Same holder and plausible fee difference; no authenticated 50-unit fee or shared settlement identifier. |
| [40,000 BUSD, May 12, 2022](https://etherscan.io/tx/0x16e3825aec1703a48bf4b64eb70c3032f2dd562da20d73acca7ed66236596212) | [40,050 burned 895 seconds earlier](https://fsnex.net/transaction/0xcdf53407183f2a9bab570ab993b00b95ad5df007d1e10e23bb82b289c4d2e9c0) | Same limit; a possible redemption, not accepted settlement. |

[Recipient discovery](../evidence/2026-10-15-terminal-recipient-discovery/) and [receipt checks](../evidence/2026-10-15-terminal-recipient-receipts/) retain the first observed onward payments. They do not identify beneficial owners. The older December 2021 large outflows and January 2024 exchange case remain separately documented and are not erased by a moving terminal-window cutoff.

## What would justify a stronger conclusion

A completed reconciliation still needs a justified complete vault inventory, consolidated rights across Fusion representations, and evidence establishing redeemable backing and material settlement adjustments. The [accounting treatment table](fusion-claims-review.md#current-accounting-treatment-october-15-closing-pass) states exactly what is included, separated and left unknown. A later Fusion snapshot could change conclusions about later source-chain settlement; reliable exchange/account records could resolve the exchange boundary. Until such evidence exists, use the observed supply-versus-tested-balances comparison with its qualifications.

Reproduce using `./reconcile.ps1 -Asset busd`, `./research/review-closeout.ps1 -Asset busd -RequireReviewed` and `./verify.ps1`. Matching thresholds were not relaxed to manufacture closure.
