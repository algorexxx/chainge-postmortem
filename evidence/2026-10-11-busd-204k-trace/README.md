# Evidence for the January 2024 BUSD exchange path

Captured 2026-10-11. See the [research finding](../../research/busd-204k-exchange-path.md).

- `99d1-discovery.*`: Routescan token-transfer discovery for the first intermediary.
- `7c32-discovery.*`: second-intermediary discovery beginning near the large transfer; `7c32-full-discovery.*` repeats the query from block zero and locates the earlier payment.
- `receipts.*`: five unique successful Ethereum receipts and the pinned current block, requested from `https://ethereum-rpc.publicnode.com`.
- `headers.*`: the five transfer block headers, matched to receipts and discovery timestamps.
- `balances.*`: two successful pinned balance reads and seven rejected historical balance queries. Rejections are not zero balances.
- `binance14-page.capture.json` and `exchange-label.observation.json`: retrieval metadata and a title/label extraction from Etherscan. The observation records the original HTML byte count and SHA-256. Full HTML remains in the local public archive and is not included in this package.

Routescan's Ethereum index is known to omit some historical events. Positive discoveries are receipt-verified; returned event counts and zero net flows do not certify complete history. The captured exchange label is a source attribution, not an on-chain field or proof of an individual customer's account.

`manifest.json` covers every other file in this package by saved-byte SHA-256 and byte length. No private community material is used.
