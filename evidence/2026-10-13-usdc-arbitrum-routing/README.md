# Targeted Arbitrum routing check — 2026-10-13

A receipt-bound Fusion burn [9a10…d802](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802) burns 416,302 USDC at 2024-09-01 22:15:40 UTC. Its colon memo names Arbitrum [USDC](https://arbiscan.io/address/0xaf88d065e77c8cc2239327c5edb3a432268e5831), recipient [35cb…e3f](https://arbiscan.io/address/0x35cba6ab1f851cdbf9accf17aa7fd98679bf3e3f), and raw memo amount 416052118806.

The public Arbitrum RPC was queried by block timestamp to locate the first block at/after the burn. A Transfer query for that token and recipient covers blocks 249105505–252105505 inclusive. This yielded one log. Its successful receipt and block header show [payment 564a…447e](https://arbiscan.io/tx/0x564a63fa0898a580f15770865d17e7dedf75af222f93c9e71f31773658e2447e) of **416,052.118806 USDC**, **79 seconds later**, from [3f0e…aa8e](https://arbiscan.io/address/0x3f0e40bc7e9cb5f46e906daed18651fb6212aa8e).

Chain, token, recipient and memo amount match exactly. This is a strong payout candidate; no shared bridge identifier or authenticated fee rule has been established. The 249.881194-unit difference from burned units is an observation, not a universal fee rule. The distinct nearby Ethereum payment remains unexplained by this burn.

All RPC requests, raw responses and capture-time sidecars are retained. Binary-search headers establish the starting bound; this is one recipient/token query, not an exhaustive vault history. Reconciliation imports the observed Arbitrum events without promoting a complete operating-address history. The routing review is separate from normal ledger classifications.
