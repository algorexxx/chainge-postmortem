# BUSD: reference investigation package

Status: reproducible partial investigation. Reviewed 2026-10-13. The accounting target remains incomplete; this is the reference for running and reviewing the pipeline, not a reference example of proven full backing or deficit.

Configuration: [busd.json](../../reconciliation/busd.json). Public assessment: [BUSD case assessment](../busd-case-assessment.md). Site publication: one detailed BUSD study, explicitly incomplete.

## Identity, custody and cutoff evidence

Use the [representation/ledger research](../busd-ledger-reconciliation.md), [time-slice coverage](../busd-time-slice-coverage.md) and [source-chain roles and flows](../busd-source-chain-flows.md) for the chain-qualified addresses and their evidence. The configuration is the executable inventory of selected contracts, precision and candidate operating addresses; the cited reports justify their roles. An operating address is not automatically a segregated reserve vault.

Fusion observations are bounded by retained block 15130080, 2025-10-07 08:43:50 UTC. Source-chain state snapshots were acquired in October 2026 at their individual cited blocks. There is no validated common-cutoff comparison. Historical event coverage, earlier tokens, migration duplication and time-slice claims require the report-specific qualifications. Latest contract `totalSupply` alone is not consolidated outstanding claims.

## Reproduction and policy

Run `./reconcile.ps1 -Asset busd`, then inspect `archives/reconciliation/busd/accounting-summary.json` and the full reconciliation/CSV files. `./verify.ps1` additionally checks that the committed BUSD movement report exactly reproduces. The run preserves 47,808 events and 5,710 operating-address movements. The 2026-10-13 workflow changes leave the committed movement report byte-identical: matching decisions were not widened to reduce unmatched counts.

[Pipeline checkpoints](../../reconciliation/README.md) explain the fixed/variable fee windows, delayed candidates, swap/relay evidence and inventory/conversion rules with supporting event references. These BUSD-calibrated policies must not be inherited by another asset without evidence. The unmatched outflow queue contains 224 movements; possible exchanges and inventory clearing still require backing/settlement review.

The [January exchange path](../busd-204k-exchange-path.md) and [FDUSD follow-up](../busd-fdusd-followup.md) illustrate why reaching an exchange or finding another asset does not resolve liabilities. The earlier December flows remain subject to their documented uncertainties.

## Integrity correction and remaining work

The new manifest gate found five previously selected response files absent from the source-review manifest: `fusion-deposit-fees.response.json` and the four `fusion-small-payment-*.response.json` batches. Their existing bytes and request/capture companions were added to the manifest on 2026-10-13 (15 new entries); no original entry or capture was replaced. No mismatched existing checksum was found.

Completion still requires a justified full custody inventory, validated consolidated Fusion claims, common-cutoff source-chain balances, opening/closing balance reconciliation and disposition of material exceptions. Passing verification does not satisfy those missing conditions. See [the workflow audit](../workflow-audit.md).
