# Fusion access correction — 2026-10-13

The previous investigation treated a timed-out direct LAN request as an unavailable gateway. That diagnosis was incomplete. The fsnex-rebirth handoff documents a site-restricted LAN listener and a separate SSH-tunnel access path for this Windows workspace. Existing SSH authentication succeeded; a temporary loopback-only tunnel restored access without changing the gateway firewall or exposing RPC publicly.

The retained request/response confirms chain ID 32659 and head block 15130080, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`. Successful receipts and transaction inputs were obtained for the [first newer USDC migration](https://fsnex.net/transaction/0x1baa6e0909589cac84be88ac1ecef3e91ae1aadf6d6b4b2b8e90d01ea5774ed7) and the [416302-USDC burn](https://fsnex.net/transaction/0x9a10c66ba478caf32f956a69d076920494061427764dfc0612a7fb9c17eed802).

For both transactions, retained FSNEX native detail agrees with the RPC transaction hash, block hash, sender, target, input, success status, log count, and every log's index/address/topics/data. FSNEX does expose receipt-derived information. Its JSON shape differs from raw JSON-RPC; the current reconciliation importer does not ingest that native detail format. That is an importer limitation, not evidence that receipts are unavailable. Explorer and RPC share infrastructure and are not independent providers.

This package records the correction and two corroboration probes. It does not imply that the full Fusion receipt history has already been acquired or imported. The existing USDC configuration is unchanged; bulk acquisition and rerunning the classifications remain outstanding. No private endpoints, credentials or SSH configuration are preserved here.
