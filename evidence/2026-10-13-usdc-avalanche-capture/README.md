# USDC evidence: avalanche capture

Captured 2026-10-13. The reusable C# collector completed 124 planned data requests plus a fresh chain-ID check, all successful. These fill missing bounded log windows and enrich the earlier events with receipts and headers. Both USDC representations and all five candidate addresses now have both-direction coverage through block 97436077. The final regenerated Avalanche plan has no remaining log, receipt or header requests within this configured scope.

Request/response and capture sidecars preserve public sources, capture times and original response bytes. The SHA-256 manifest detects later changes, not source accuracy. Failed calls are unknown results, never zero. This evidence does not identify a beneficial owner or establish authority, intent, complete custody or consolidated claims. See the [USDC assessment](../../research/assets/usdc.md) and [configuration](../../reconciliation/usdc.json).
