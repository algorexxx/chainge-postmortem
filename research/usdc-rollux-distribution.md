# Rollux: the 50,000 transfer and earlier vault balances

Reviewed 2026-10-15 using [preserved anonymous RPC evidence](../evidence/2026-10-15-rollux-distribution/). This follows the [Fusion–Syscoin–Rollux route](usdc-rollux-roundtrip.md). All amounts are six-decimal Rollux token units; independent USDC backing remains unverified.

## The 50,000 recipient retained most of its balance

At Rollux block **52,350,517**, timestamp **2026-10-15 12:11:15 UTC**, [127e…8e9d](https://explorer.rollux.com/address/0x127e7766f65c7a35e73177aa104999a189048e9d) holds **44,460.077515**. Its two incoming transfers total **50,001.000000**: an earlier one-unit transfer and the previously traced [50,000 transfer](https://explorer.rollux.com/tx/0x5a096a26d4f53f45d4ef2efadd4f1cf6f964477d1dad27553a96800a3243efdd).

Its **716 outgoing transfers total 5,540.922485**. The largest single outgoing transfer is **351.824090**. The largest recipient by aggregate amount is [50b5…3ab3](https://explorer.rollux.com/address/0x50b5be2f946a5128dd75fd330da9d1a331d53ab3), receiving **3,584.377805**. The [last outgoing transfer](https://explorer.rollux.com/tx/0x0896e5baaf5653ca6ca637bc1bae0041590581406949c864ea2251eb8c517779) occurred on **2025-05-15 at 12:36:31 UTC**.

The observed pattern is many small transfers with most units still at the first recipient. It does not establish an exchange destination, payment purpose or owner. The address has no runtime code at the pinned block; this does not identify who controls it. These downstream transfers do not receive separate vault-outflow cards.

## The second large vault deposit came from a mixed earlier balance

On **2024-09-05 at 12:13:53 UTC**, [3f0e…aa8e](https://explorer.rollux.com/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e) [transferred 259,666.261480](https://explorer.rollux.com/tx/0x388656f945001302451f68570e57d7016287d62b737887607cf28bdae312bf17) to [vault 66ff…ecf1](https://explorer.rollux.com/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1). It had ten incoming transfers totaling **264,712.433540** and three other outgoing transfers totaling **5,046.172060**. The large transfer emptied its observed token balance, which remains zero at the pinned block.

The dominant input was [237,535.648990 from vault 8a4a…64c1](https://explorer.rollux.com/tx/0xc02c9b164c14dbcd47879ed3bc2bbcd30d617666f8f079782c5df8619dcd1889) on **2024-08-23 at 23:15:03 UTC**. The remainder came from smaller deposits. That earlier vault also has a zero closing balance.

One large earlier input into [8a4a…64c1](https://explorer.rollux.com/address/0x8a4aa176007196d48d39c89402d3753c39ae64c1) was [199,899.699967 from 35cb…8c65](https://explorer.rollux.com/tx/0x690980602603ee9c0e3ce3beff03225efba18d2f22acb6e1650a4e68bc705e3f) on **2024-02-16 at 16:02:33 UTC**. This identifies an input to the mixed balance, not a one-to-one allocation of the later vault transfer. The earlier vault has 200 incoming and 108 outgoing events, so attributing its final balance entirely to that one input would be unsupported.

## Current comparison and remaining scope

Follow-up: the [199,899.699967 input is traced to a direct Syscoin mint and bridge lock](usdc-rollux-earlier-route.md). Its intermediate address has a verified one-in/one-out history, but no Fusion funding source is established for that mint.

Vault 66ff has **2,284,499.383095** at the new block, equal to the previously published snapshot amount. Its retained history has 285 incoming and 183 outgoing events; net transfers agree exactly with `balanceOf`. This later check does not replace the site's aligned historical comparison or establish dollar backing.

The 50,000 recipient is not promoted to a vault: no evidence here establishes that role. Its remaining balance is not added to the vault total. Likewise, transfers between the three vault addresses are not new external reserves. Independent backing of the Syscoin parent and beneficial ownership remain unresolved.

The reusable address-history checker reproduces counts, exact raw totals and recipient aggregates from the captures. It is included in repository verification through `check-rollux-distribution.ps1`. More detailed attribution of the mixed earlier vault balance remains separate work.
