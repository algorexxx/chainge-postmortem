# Fusion USDC representation bytecode

Captured 2026-10-13 through the owner-authorized Fusion gateway, at block 15130080. Connection details are omitted. The request and original response retain the newer USDC contract and all five legacy child representations.

The legacy full-time child and four dated children have identical deployed runtime bytes: **5,513 bytes**, SHA-256 `04bc7a9824208fc551c005e1db7ccb475c0ea754d599bc304b2a7ad34e5ee354`. All five match the official source's `Slice` runtime with Solidity 0.8.12, optimizer enabled, 200 runs, after normalizing only its 32-byte IPFS metadata digest. `comparison.json` preserves the individual results and hashes; `compiled-slice-runtime.hex` preserves the compiler output. Reproduce using the source unit and settings described in the [parent comparison](../2026-10-13-usdc-fusion-source-match/README.md), selecting `Slice` instead of `FRC759Token`.

The newer token is a different 4,134-byte runtime with Solidity 0.8.21 metadata. It does not match `Slice`, and no source match is claimed for it here. Identical child code does not mean identical interval state, ownership or economic claims. The separate pinned holder checks establish balances; this package checks executable-code correspondence only. No transaction was sent.

The SHA-256 manifest detects later byte changes, not independent source truth. See the [USDC assessment](../../research/assets/usdc.md) for contract links and remaining claim questions.
