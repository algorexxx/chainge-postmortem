# Public token catalogue export

`tokens.csv` is an independently retrieved public Google Sheets CSV export on 2026-10-11. `capture.json` records its URL, saved-byte hash, size and coverage. The exact retrieval time was not recorded. This adds an independent export to the previously supplied workbook; the workbook itself remains unchanged in the local archive.

The export has the expected two columns, 2,185 data rows and 2,184 case-insensitive distinct contract addresses. All address strings pass the 40-hex-character syntax check. Duplicate GLCH rows 700 and 1760 and the four literal hash-only labels are retained. A listing is not proof of deployment, metadata, issuer or source-chain custody.

The static-site generator consumes this original CSV directly. Source row numbers are preserved in the generated catalogue, and all rows stay available here even when duplicate addresses are grouped for navigation.
