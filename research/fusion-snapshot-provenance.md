# Fusion snapshot: scope and provenance

The investigation uses the highest preserved Fusion history currently available to this project, accessed through a node restored from a chain-data backup. This describes the project's data source, not an independently established final network state.

The retained accounting reference is [block 15,130,080](https://fsnex.net/block/15130080), timestamp **2025-10-07 08:43:50 UTC**, hash `0xe93ffded087a79097d4309c7831690161db6ed136f4b1a22c4c83a99db80f99f`. Existing [node-access corroboration](../evidence/2026-10-13-fusion-rpc-recovery/README.md) and [USDC receipt checks](../evidence/2026-10-13-usdc-fusion-receipts/README.md) describe the retained evidence. FSNEX and the node share infrastructure; they are not independent witnesses.

This project has not established that no valid Fusion blocks exist beyond the preserved history. Nor does a foundation infrastructure shutdown establish the exact last block produced by other validators. Comparing other node heights without preserving their headers and chain ancestry does not resolve this question.

Within the snapshot, completeness must be assessed for each acquired contract history. A preserved chain database does not by itself prove that an explorer export or token inventory includes every relevant event. After the snapshot, absence of a Fusion burn in this dataset means **not observed**, not **proved not to have occurred**. Earlier pending withdrawals also remain possible explanations for later payments unless evidence excludes them.

All balances and supply comparisons remain pinned to this reference. A later, demonstrably compatible chain backup would require extending the ledger and refreshing the comparisons, not silently treating unknown later activity as zero.
