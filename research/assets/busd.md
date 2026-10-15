# BUSD: reference investigation package

Status: reproducible partial investigation. Reviewed 2026-10-15. The accounting target remains incomplete; this is the reference for running and reviewing the pipeline, not a reference example of proven full backing or deficit.

Configuration: [busd.json](../../reconciliation/busd.json). Public assessment: [BUSD case assessment](../busd-case-assessment.md). Site publication: one detailed BUSD study, explicitly incomplete.

## Identity, custody and cutoff evidence

Use the [representation/ledger research](../busd-ledger-reconciliation.md), [time-slice coverage](../busd-time-slice-coverage.md) and [source-chain roles and flows](../busd-source-chain-flows.md) for the chain-qualified addresses and their evidence. The configuration is the executable inventory of selected contracts, precision and candidate operating addresses; the cited reports justify their roles. An operating address is not automatically a segregated reserve vault.

Fusion observations are bounded by retained block 15130080, 2025-10-07 08:43:50 UTC. The earlier source-chain snapshots were acquired in October 2026. The additional historical calls below now align the 15 tested source-chain positions to the Fusion reference time. Historical event coverage, earlier tokens, migration duplication and time-slice claims require the report-specific qualifications. Latest contract `totalSupply` alone is not consolidated outstanding claims.

## Balances aligned to the Fusion reference time

Captured 2026-10-15. Anonymous historical RPC calls now test the five candidate operating addresses on each of Ethereum, BSC and Avalanche. Adjacent headers bracket **2025-10-07 08:43:50 UTC** and their parent hashes join correctly. Each endpoint returned the expected chain ID and 18 token decimals. All 15 balance calls succeeded; zero is an observed response, not an inferred missing value.

| Chain / representation | Cutoff block | Nonzero positions (token units) |
| --- | ---: | --- |
| Ethereum BUSD | 23524761 | [66ff…ecf1](https://etherscan.io/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1): 23.724135682808759163 |
| BSC Binance-Peg BUSD | 63769742 | [8a4a…64c1](https://bscscan.com/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1): 0.080222660182393847 |
| Avalanche BUSD.e | 69908002 | [8a4a…64c1](https://snowtrace.io/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1): 0.000000000000087769 |

Each chain also tests the 874d, 4563, 3f0e and other listed candidates; all other tested positions are zero. Full addresses, contracts, requests, unchanged responses, capture plans and SHA-256 manifests are retained in the [Ethereum](../../evidence/2026-10-15-busd-cutoff-ethereum/), [BSC](../../evidence/2026-10-15-busd-cutoff-bsc/) and [Avalanche](../../evidence/2026-10-15-busd-cutoff-avalanche/) packages. Candidate selection extends the known historical/successor operating-address set; testing an address does not independently establish a vault role on every chain.

The sum of these positions is **23.804358342991240779 BUSD-labelled token units**. Newer Fusion supply is **1,042,080.420700366057106486**; the arithmetic comparison gap is **1,042,056.616342023065865707**. This is not a confirmed loss, customer-liability figure or verified redeemable-dollar total. Bridged representations, unknown vaults, legacy balances, address-one holdings, migrations and dated rights remain qualifications. No alternative asset or exchange deposit is counted as replacement BUSD backing.

The shared site overview derives these totals from individual raw positions. The balance requests are also registered in the reconciliation configuration, so the existing balance reviewer checks them against retained histories. Historical balance observations do not close missing history or establish the purpose of the 1.2M December 2021 and 204,567.658… January 2024 outflows. Their material disposition questions remain open in the linked assessments.

## Reproduction and policy

The common-cutoff balance reviewer finds 14 net agreements with incomplete history coverage and one discrepancy: retained BSC history reconstructs 1.187246977608202752 at the 8a4a candidate, versus the historical state call of 0.080222660182393847. The direct state observation is used in the comparison; the discrepancy remains a history gap, not a corrected or silently discarded transaction.

Run `./reconcile.ps1 -Asset busd`, then inspect `archives/reconciliation/busd/accounting-summary.json` and the full reconciliation/CSV files. `./verify.ps1` additionally checks that the committed BUSD movement report exactly reproduces. The run preserves 47,808 events and 5,710 operating-address movements. The 2026-10-13 workflow changes leave the committed movement report byte-identical: matching decisions were not widened to reduce unmatched counts.

[Pipeline checkpoints](../../reconciliation/README.md) explain the fixed/variable fee windows, delayed candidates, swap/relay evidence and inventory/conversion rules with supporting event references. These BUSD-calibrated policies must not be inherited by another asset without evidence. The unmatched outflow queue contains 224 movements; possible exchanges and inventory clearing still require backing/settlement review.

The [January exchange path](../busd-204k-exchange-path.md) and [FDUSD follow-up](../busd-fdusd-followup.md) illustrate why reaching an exchange or finding another asset does not resolve liabilities. The earlier December flows remain subject to their documented uncertainties.

## Integrity correction and remaining work

The new manifest gate found five previously selected response files absent from the source-review manifest: `fusion-deposit-fees.response.json` and the four `fusion-small-payment-*.response.json` batches. Their existing bytes and request/capture companions were added to the manifest on 2026-10-13 (15 new entries); no original entry or capture was replaced. No mismatched existing checksum was found.

Completion still requires a justified full custody inventory, validated consolidated Fusion claims, extension beyond the tested common-cutoff positions, opening/closing balance reconciliation and disposition of material exceptions. Passing verification does not satisfy those missing conditions. See [the workflow audit](../workflow-audit.md).
