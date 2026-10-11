# FDUSD follow-up to the January BUSD exchange path

Assessment date: **2026-10-11**. **Result: no FDUSD return identified into the 20 tested candidate addresses.** This is a bounded search for replacement reserves, not a completed FDUSD or BUSD liability/vault reconciliation. No FDUSD holding is added to verified BUSD backing.

## Why FDUSD was tested

The [verified BUSD path](busd-204k-exchange-path.md) ends with **204660.303858061764406054 BUSD** reaching the Ethereum address labelled Binance 14 on **2024-01-01 03:00:11 UTC**. This includes the January controller payment of **204567.658073647823395109 BUSD** and an earlier **92.645784413941010945 BUSD** payment.

Binance's updated [conversion notice](https://www.binance.com/en/support/announcement/detail/1c98ce7bb464422dbbaeda7066ae445b) schedules withdrawal suspension and automatic conversion of remaining eligible BUSD balances from **2024-01-02 03:00 UTC**. The on-chain receipt precedes that time by **23 hours, 59 minutes, 49 seconds**. This was an exchange-service cutoff, not the date BUSD became invalid. The notice says late or unprocessed deposits would still be credited as BUSD, with conversion available afterward.

The [January 5 completion notice](https://www.binance.com/en/support/announcement/detail/b7d1cd119211458eb676a9a324edf314) reports completion of eligible Spot and Funding balances' conversion at 1:1. These announcements make FDUSD a reasonable hypothesis. They do not prove the identity of the credited account, when this deposit was credited, whether its balance remained eligible, whether it converted, or where it subsequently went. The pages were read in October 2026; they are live versions, not contemporaneous archived copies.

## Token and address scope

The [July 2023 listing announcement](https://www.binance.com/en/support/announcement/detail/065a0d561cc147ffaa74554360b9a042) identifies Ethereum and BNB Chain availability and links the original token contracts. The January completion article also links an opBNB contract in its currently served version. That link alone does not establish when opBNB withdrawals became available.

| Chain | Contract tested | Decimals returned |
| --- | --- | ---: |
| Ethereum | `0xc5f0f7b66764f6ec8c8dff7ba683102295e16409` | 18 |
| BSC | `0xc5f0f7b66764f6ec8c8dff7ba683102295e16409` | 18 |
| opBNB | `0x50c5725949a6f0c72e6c4a641f24049a917db0cb` | 18 |

The exact [candidate list](../evidence/2026-10-11-fdusd-review/candidate-addresses.json) contains the BUSD controller and predecessor; three liquidity-address candidates; January and November intermediaries; three administrative candidates; the Fusion controller; two routers; and five significant Fusion holders. These leads come from the prior [source-chain](busd-source-chain-flows.md), [migration](busd-migration-and-balance-flow.md), and [administrative](legacy-balance-match-and-admin-transition.md) research. Identical address strings on another chain are search leads, not proof of custody or common control. Holders and counterparties are not automatically vaults.

## Results

| Check | Result | Qualification |
| --- | --- | --- |
| Ethereum token-holder transfer history, 20 candidates | Etherscan displayed no matching entries for each | Explorer observation; no full-chain log replay |
| BSC token-holder transfer history, same 20 candidates | BscScan displayed no matching entries for each | Explorer observation; no full-chain log replay |
| Ethereum Routescan discovery, same 20 candidates | Empty result arrays | Previously demonstrated incomplete index; supplementary only |
| Ethereum `balanceOf`, same 20 candidates | All zero | Pinned block **26170686**, **2026-10-11 17:24:47 UTC** |
| BSC `balanceOf`, same 20 candidates | All zero | Unpinned `latest` calls; surrounding head observations **127059545–127059660**, **17:27:10–17:28:02 UTC** |
| opBNB `balanceOf`, same 20 candidates | All zero | Unpinned `latest` calls; surrounding head observations **194490801–194491006**, **17:27:11–17:28:02 UTC** |

All times in the balance rows are on October 11, 2026. The surrounding heads give observation context, not an exact block guarantee for each `latest` response. Each contract returned deployed code and 18 decimals; the zero balances are successfully decoded 32-byte results, not missing results or errors.

The explorer searches used the token page's address filter with no date restriction. BscScan returned transfers for Binance 51 and Etherscan returned transfers for Binance 14 as positive controls. This confirms the filters were capable of returning rows, without establishing historical completeness. Observations are preserved as a transcribed record with URLs, timing and control transaction hashes, not as raw HTML.

Public RPC attempts to obtain broad historical logs on Ethereum and BSC were rejected by archive-access restrictions. Failed queries are not treated as empty histories. No comprehensive opBNB transfer history was obtained; its zero current balances cannot exclude past receipts and subsequent spending. A small empty opBNB log probe in the evidence package does not cover the January event window.

## What remains unresolved

There is **no identified FDUSD replacement reserve** in this candidate set. There is also **no evidence here disproving conversion inside Binance**. A pooled exchange wallet's FDUSD holdings or unrelated withdrawals cannot be assigned to this deposit. Matching an amount alone would not establish that link.

The search does not cover unknown withdrawal addresses, every later FDUSD deployment or chain, other replacement assets, exchange-internal records, or a Fusion BUSD-to-FDUSD liability migration. It does not establish that FDUSD was issued on Fusion. A different stablecoin would require evidence of both its custody and its relationship to outstanding Fusion claims before it could enter a backing calculation.

The next evidence capable of advancing this lead would be an independently attributed replacement-custody address and receipt, a documented liability migration, or authenticated exchange records for the relevant account. Until then, the economic destination after the exchange receipt remains unresolved and [BUSD remains incomplete](busd-case-assessment.md).

The [reviewed evidence package](../evidence/2026-10-11-fdusd-review/README.md) preserves requests, responses, metadata, explorer observations and an integrity manifest. No private community submission is included.
