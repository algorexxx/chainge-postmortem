# Routing interface checks — 2026-10-13

FSNEX reports no verified source/ABI for the Fusion routing target [3668…9152](https://fsnex.net/address/0x3668c219b1fa8fe8175158f6ce91ded36fde9152). Retained official Chainge repository metadata and tree captures show no contract interface in the inspected SDK, OpenAPI example and swap repositories. This is a bounded search, not proof no published interface exists.

The decoder therefore describes observed calldata layouts, not authenticated function names. Raw requests/receipts in the [Fusion package](../2026-10-13-usdc-fusion-receipts/README.md) support the layout. Selector `0x3f163c47` uses an address, amount, observed mode 1 and a canonical dynamic string at offset 128. The selected full-time burn, sender, token and amount must agree with the receipt before decoding. ASCII colon memos yield destination chain/token/recipient and numeric `1_` memo amount; the economic meaning of that amount is unverified. JSON memos are explicitly unsupported in this first decoder.

Other selectors observed alongside mints are not treated as burn requests. No verified ABI or complete source audit is claimed. Tests use the retained [416,302-unit burn](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802) as a literal vector and mutate bindings, offsets, lengths and padding to check rejection.
