# USDC: second-asset workflow trial

Status: reproducible partial investigation, transaction-selected Ethereum sample. Reviewed 2026-10-13. Configuration: [usdc.json](../../reconciliation/usdc.json). Not added to the public site's assessed studies. This validates reuse of the workflow, not completion of a second asset investigation.

## Identity and candidate scope

The catalogue's Fusion USDC address is [0xfc9e57d7543e0591824121fda6461fe2148a5966](https://fsnex.net/address/0xfc9e57d7543e0591824121fda6461fe2148a5966); the [retained initial probe](../initial-contract-findings.md) returned USDC and six decimals. No Fusion USDC event history is included in this run.

Ethereum token [0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48](https://etherscan.io/token/0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48) matches [Circle's issuer contract list](https://developers.circle.com/stablecoins/usdc-contract-addresses), consulted 2026-10-13. Its six-decimal precision is corroborated by the retained pinned `eth_call` below. The live issuer list is an attribution source, not a preserved historical copy.

Two Ethereum addresses are tested: [0x456325f2ac7067234dd71e01bebe032b0255e039](https://etherscan.io/address/0x456325f2ac7067234dd71e01bebe032b0255e039) and [0x874de5761900ee526950d707d8cbd4b93092e290](https://etherscan.io/address/0x874de5761900ee526950d707d8cbd4b93092e290). Their lead provenance is [BUSD operating-address research](../busd-source-chain-flows.md). Their USDC custody role and completeness are unverified; they are not asserted to be the only USDC vaults.

## Retained observations and coverage

The configuration filters USDC logs from already retained `ethereum-*.response.json` files in the BUSD source-review package. It is biased toward transactions selected for BUSD context. All 55 distinct USDC events have receipt corroboration and timestamps; only 12 involve the configured candidate addresses. There are six incoming and four outgoing possible asset exchanges, plus one incoming and one outgoing reported asset exchange. No possible Fusion redemption/deposit is inferred because no Fusion issuance history was supplied. Zero unmatched rows in this sample does not mean all USDC movements are explained.

The first candidate has observed sample inflows of 97518.294606 USDC and outflows of 23876.395290 USDC, a sample net increase of 73641.899316 USDC. These are exact six-decimal conversions of the generated integer totals. The second candidate has no transfers in this selected sample. Neither result is a closing balance or full history.

A [new public-node capture](../../evidence/2026-10-13-usdc-workflow-trial/) returned zero `balanceOf` for both addresses at Ethereum block **26182536**, hash `0xefd17b5282b448dc3d3b865285ed9fcac1dd2a5c2327c337edb4f9625d2ffd9f`, **2026-10-13 09:01:11 UTC**. The node returned chain ID 1, the block under its `finalized` tag, and the same hash on a second numbered-block read. These checks corroborate consistency of that provider's view, not independent consensus verification. Capture UTC is recorded separately in the sidecars.

The mismatch between a positive partial-history net flow and a zero later observed balance demonstrates why event sums cannot substitute for complete balance reconciliation. No missing-asset amount is inferred.

## Policy, reproduction and acceptance

`./reconcile.ps1 -Asset usdc` uses six decimals, an explicit five-minute equal-amount hypothesis, two-minute clock tolerance and 10000-USDC triage threshold. These are trial parameters; no authenticated fee schedule was obtained. BUSD fee, slice, migration, inventory and source-chain matching rules are disabled. Reuse required configuration only, without changing the matcher.

`./verify.ps1` runs this configuration alongside BUSD. The accounting summary and detailed outputs stay in ignored `archives/`; no generated USDC site report is committed. A matching run must not create Fusion zero-supply rows for absent data.

Before any completed USDC assessment: discover and verify other representations/chains and address roles, acquire full candidate transfer histories, collect Fusion issuance/holder history and implementation semantics, establish a common cutoff, and reconcile opening/closing balances. USDC was selected for available six-decimal evidence and narrow trial scope, not because it exists on only one chain.
