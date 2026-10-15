# Fusion holder review and announcement timing

The checked balances do not yet establish consolidated customer liabilities. This review separates contract balances, address-one holdings, dated rights and possible migration overlaps instead of adding them into one claim total.

## Reference and reproduction

Fusion block **15,130,080**, **2025-10-07 08:43:50 UTC**. Reconcile `reconciliation/busd.json` and `reconciliation/usdc.json` to regenerate `claim-positions.json` and `balance-checks.json`. `verify.ps1` checks every published holder position against both generated history and a retained balance observation at that block. The compact inventory in `registry/studies.json` drives the expandable holder table; totals are derived from raw amounts.

There are **210 positive BUSD positions** across six standard contracts and **529 positive USDC positions** across six contracts. These are address/token positions, not unique people. Agreement between logs and retained balance calls corroborates the arithmetic; shared infrastructure and incomplete history remain limitations. The earlier nonstandard BUSD contract is excluded from this standard-token inventory; see [time-slice coverage](busd-time-slice-coverage.md).

## Full-time balances

Amounts below are token units, with original precision preserved.

| Representation | At address one | At other addresses |
| --- | ---: | ---: |
| BUSD legacy full-time | 1007499579.438009673238337766 | 867196.865081511853500767 |
| BUSD newer | 0 | 1042080.420700366057106486 |
| USDC legacy full-time | 996416657.619439 | 2683105.898389 |
| USDC newer | 0 | 7061027.518489 |

[Address one](https://fsnex.net/address/0x0000000000000000000000000000000000000001) is retained separately. A transfer there is not automatically a burn, nor does the balance establish a customer claim. “Other addresses” also does not mean customer-owned. Expired dated balances remain in the inventory, separately labelled; they are not added to unexpired rights. Different representations may overlap.

The shared legacy holder [0x7bc4…695e](https://fsnex.net/address/0x7bc4db49e8bc78c20fbc9f76ec3eb2d552c6695e) holds **859506.1537 BUSD** and **2677846.972295 USDC**. Equal-sized newer mints occurred in separate transactions to a different holder. The retained administrative calls and balance matches support a migration hypothesis, but do not establish that the claims were legally or technically extinguished. See [legacy transitions](legacy-balance-match-and-admin-transition.md) and [authority and slice accounting](legacy-authority-and-slice-accounting.md).

Subtracting those positions from the respective non-address-one legacy balances would leave **7690.711381511853500767 BUSD** and **5258.926094 USDC**. These are conditional arithmetic remainders, **not verified customer liabilities**. No migration deduction has been applied to the published supply comparison.

Large newer balances also require ownership and redemption-rights review. For example, [0xf11b…c52c](https://fsnex.net/address/0xf11b4db13148d56e2a10ad701ad0ff338c62c52c) holds 859506.1536991404938463 newer BUSD and 2080912.112338 newer USDC. [0x66ff…ecf1](https://fsnex.net/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) holds 3446190.707623 newer USDC. Reuse of an address on a source chain does not establish beneficial ownership or justify excluding its Fusion balance.

## Announcement reference points

The UI resolves reviewed IDs from `registry/timeline.json`; it does not maintain a second set of announcement dates or excerpts.

- [DJ Qian's March 14 explanation](https://t.me/chaingefinance/864721), published **2025-03-14 04:02:07 UTC**, describes blacklisting and liquidity problems generally.
- [DJ Qian's May 27 suspension statement](https://t.me/chaingefinance/883566), published **2025-05-27 13:35:10 UTC**, describes service suspension pending liquidity injection.

Neither gives a verified BUSD/USDC incident onset or exact technical shutdown time. Text was edited after publication; the timeline retains edit details. Comparisons use publication time, with grouped case date ranges labelled as overlapping when precision is insufficient. Chronology alone cannot establish the purpose or legitimacy of a transfer.

Within the retained outgoing-event ledgers, all 2,665 BUSD events precede both statements. Of 16,864 USDC outgoing events, 105 follow the March statement and three follow the May statement. Coverage is limited to the retained vault inventory and histories; these counts do not prove that other vaults were inactive.

The three retained USDC outflows after the May statement are:

| UTC date | USDC units | Ethereum transaction |
| --- | ---: | --- |
| 2025-10-10 14:47:35 | 12.938852 | [Transaction](https://etherscan.io/tx/0x3823e138982b276b1046a6c552c7db7a2da083b5d63610751360164b8d1e1eda) |
| 2025-10-10 15:49:11 | 542.761951 | [Transaction](https://etherscan.io/tx/0xd15fdd43bbd24ed88ee64bf7cb92a671b7be9818f5f377cbc9f406fd5694ef50) |
| 2026-01-27 16:03:11 | 634.222907 | [Transaction](https://etherscan.io/tx/0x5e4a63c30ff5e019abb98ce4c72f58d59f30b7612a3683086ac008fc4cc6cf0c) |

All three are **after the Fusion reference block**. Their lack of automatic matches cannot establish an absence of corresponding burns beyond that cutoff. Event evidence and its verification status remain available in the complete transfer ledger. Use “All results”, outgoing direction, no minimum amount and the announcement timing filter to inspect them.

## Remaining work

Establish migration replacement semantics and the rights associated with large holder positions before publishing a consolidated claim figure. Extend Fusion history beyond the late source-chain transfers before assessing their settlement. Continue to distinguish unknown vault coverage, token backing and beneficiary ownership from observed balances. See the [BUSD](assets/busd.md) and [USDC](assets/usdc.md) investigations for the source-chain scope and unresolved cases.

## Post-snapshot follow-up

The [three-case review](usdc-post-snapshot-outflows.md) identifies new funding before each late USDC payment, two swaps returning ETH to the same vault, and the full January payment reaching an OKX-labelled endpoint. See [snapshot provenance](fusion-snapshot-provenance.md) for why later Fusion settlement remains unknown.

## Migration semantics follow-up

The [contract and state review](fusion-migration-semantics.md) confirms coordinated BUSD/USDC blocking, matches both legacy runtimes to source, and checks all positive legacy-holder flags. Blocked balances remain recorded and reversible; no additional overlap deduction is applied. The reviewed UI separates the blocked subset from other balances.
