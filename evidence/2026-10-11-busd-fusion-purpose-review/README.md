# Fusion issuance receipt and call capture

These read-only captures cover the 5,249 unique transactions containing mint/burn events already selected by the BUSD reconciliation configuration. `batch-*` files request receipts; `transactions-*` request top-level transaction data. Both use batches of at most 40 calls. All 5,249 receipts returned successful status, and all transaction queries returned an object. The event set is unchanged; this is not a new claim of complete chain coverage.

Each request has its raw JSON response and capture metadata with saved-response SHA-256. `manifest.json` hashes all 792 request/response/capture JSON files. Capture times record retrieval, not block time. Existing retained block timestamps remain the matching inputs. Duplicate event fields, transaction block hashes, receipt targets and transaction metadata are checked for consistency by the offline reader. The RPC connection is private; connection details are deliberately omitted. Only public on-chain data are retained here.

Reproduce classification with the root `reconcile.ps1 -Asset busd` command. Results and limitations are documented in `reconciliation/README.md`; generated review files are ignored local outputs.
