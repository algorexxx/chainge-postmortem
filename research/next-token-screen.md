# Next-token screening: 2026-10-15

Recommendation: BAT is the next bounded workflow trial. This is a screening decision, not a completed asset investigation or a market-adoption ranking. ENS was the initial candidate, but its listed Fusion contract has little activity. LQTY's listed address has no indexed metadata or transfers. Older representations may change either picture.

## Fusion activity

The public FSNEX API supplied uncapped counts for the exact contracts in the preserved token catalogue. Holder counts refer to indexed addresses, not people. Supply is reported at block 15,130,080; transfer counts are explorer-index results, not independently replayed histories.

| Asset | Indexed holders | Indexed transfer events |
| --- | ---: | ---: |
| ENS | 2 | 5 |
| LQTY | 0 | 0 |
| HOT | 19 | 76 |
| LRC | 14 | 22 |
| DENT | 0 | 0 |
| COMP | 18 | 189 |
| BAT | 30 | 77 |
| ZRX | 10 | 25 |

Sources: [initial screening](../evidence/2026-10-15-next-token-screen-public/) and [alternatives](../evidence/2026-10-15-next-token-alternatives/). A missing metadata record for LQTY or DENT is not proof that no contract exists. The COMP transfer response contains only the first 100 rows; 189 is its uncapped reported total. Other listed histories fit within the requested first page. No legacy-token inventory was completed in this screen.

BAT's listed Fusion contract (`0x8083d1465ea684ce18888bd58b182a33b52a6647`) reports 6,483.855833284028158463 tokens. Its 77 returned events include 35 transfers from the zero address, four to the zero address, and 38 other transfers. These are event shapes, not verified customer deposits or redemptions. They provide enough variety for a manageable workflow trial.

## Ethereum vault-candidate screen

[Preserved RPC requests and responses](../evidence/2026-10-15-next-token-vault-screen/) query each token in both directions across the five Ethereum addresses already used as candidates in the USDC study. That role does not automatically transfer to these assets. Queries cover block 0 through 23,524,761 inclusive, with Ethereum chain ID 1 and a captured cutoff header at 2025-10-07 08:43:47 UTC. This is close to the recovered Fusion snapshot time, not a current balance check.

| Asset | Incoming events | Outgoing events |
| --- | ---: | ---: |
| ENS | 5 | 8 |
| BAT | 9 | 5 |
| ZRX | 4 | 7 |
| COMP | 10 | 11 |
| LRC | 12 | 9 |

All returned amounts are nonzero. A transfer between two tested addresses appears in both directions; these counts must not be added as independent movements. Receipts have not yet been independently corroborated. Contract identities other than BAT/ENS remain screening inputs requiring verification before reconciliation.

BAT activity occurs at four tested addresses: `0x456325f2ac7067234dd71e01bebe032b0255e039`, `0x8a4aa176007196d48d39c89402d3753c39ae64c1`, `0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e`, and `0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1`. Multiple addresses mean that a simple chain footprint will not eliminate vault changes.

## Chain scope and decision

The [official BAT FAQ](https://basicattentiontoken.org/faq/), [preserved here](../evidence/2026-10-15-next-token-official/), identifies Ethereum and bridged Solana BAT. Ethereum contract: `0x0d8775f648430679a709e98d2b0cb6250d2887ef`. Solana mint: `EPeUFDgHRxs9xxEPVaL6kfGQvCon7jmAWKVUHuux1Tpz`.

This supports BAT as a candidate with a relatively narrow documented footprint. It does not establish that Chainge used only Ethereum, exclude third-party wrapped versions, or prove that any Solana vault existed. Solana is outside the current EVM reconciliation adapter. If evidence shows relevant Solana custody, explicitly extend support or retain that scope as an unresolved gap.

Next investigation steps:

1. Inventory BAT's older Fusion contracts, migrations and dated rights; corroborate the explorer counts and zero-address events with logs, receipts and state.
2. Establish each candidate vault's BAT role and period. Follow the small Ethereum set and inspect other representations indicated by Chainge evidence; do not infer complete chain coverage from the issuer FAQ.
3. Acquire aligned balances and complete histories, then configure BAT-specific matching hypotheses. Do not copy stablecoin fee assumptions.
4. Apply CONTRIBUTING.md through closing review. Do not publish a supply/backing conclusion from this screening data.

BAT is preferable to ENS for this trial because it has more holders and both mint- and burn-shaped events. COMP has more listed-contract activity, but its broader deployment inventory needs separate examination. ZRX remains a smaller alternative. None has been established as the statistical middle of Chainge adoption.
