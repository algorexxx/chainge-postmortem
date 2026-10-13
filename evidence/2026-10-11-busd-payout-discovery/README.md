# BUSD payout-address discovery

Read-only public lookups captured 2026-10-11. This directory contains 74 Ethereum and 74 Avalanche BUSD.e holder-history queries, using the holders of all 447 previously unpaired Fusion burns. Capture sidecars preserve URL, retrieval time and HTTP status. All 148 explorer responses reported status 1 and returned arrays; there are 115 Ethereum rows and 358 Avalanche rows, including duplicate/overlapping observations. The largest response has 347 rows, below the requested 1,000-row limit. None of this proves indexing completeness.

69 Avalanche and 63 Ethereum lookups returned empty arrays. Ethereum Routescan is already known to omit previously node-verified controller events; empty responses must not be interpreted as no activity. These rows are loaded as unverified discovery observations with synthetic IDs and no asserted log index. They are never inserted into the token ledger or counted as receipt-verified movements. Any positive new lead would require a successful node receipt and timestamp corroboration before promotion.

The BSC Routescan probe reports unsupported chain. Direct BscScan HTTP returned a 403 challenge. Three further free endpoints listed in the captured chain registry were tested against the already-known BUSD transfer at block 13,132,555: two returned HTTP 403 and one HTTP 429. No log result was returned. A public browser inspection did load one holder's BUSD-specific history; `browser-observation.json` records its narrow scope and the failed export attempt. No bulk BSC holder coverage is claimed.

The [BNB Chain endpoint documentation](https://docs.bnbchain.org/bnb-smart-chain/developers/json_rpc/json-rpc-endpoint/) separately states that its listed mainnet endpoints disable `eth_getLogs`. This is consistent with earlier retained failures; these probes did not purchase services, register accounts or use private API keys.

`manifest.json` records saved-byte SHA-256 hashes for the files in this package, excluding itself. Requests are represented by capture URLs for GET requests and explicit request JSON for RPC probes. The chain registry is discovery provenance, not a recommendation or endorsement of its listed endpoints.

Reproduce the offline analysis with `./reconcile.ps1 -Asset busd` from the repository root. See [the sixth checkpoint](../../reconciliation/README.md#sixth-pass-payout-discovery-and-controller-inventory) for findings and limits.

## Sanitized public derivative (2026-10-13)

The captured public chain directory contained unrelated RPC URLs with embedded authentication and provider-key information. The public response file has been minimized to the BNB Smart Chain (chain ID 56) record actually relevant to these probes; all other chain records have been removed. It is a filtered, sanitized derivative rather than an untouched response. The package manifest records the sanitized bytes. The removed credential-bearing endpoints were not used for this investigation. Local Git history and retained checkpoint trees were rewritten on 2026-10-13 to replace the original capture and update historical manifest checksums. The original blob was pruned from the local object database. Remote publication of the rewritten history remains the maintainer’s responsibility; see the [publication audit](../../research/repository-publication-audit.md).
