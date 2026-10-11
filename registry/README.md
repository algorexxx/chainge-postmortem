# Site data and local preview

The static registry is a presentation of reviewed research. It does not collect chain data, automatically match burns, or complete an asset reconciliation.

## Build

From the repository root, with .NET SDK 10.0.401 or a later compatible patch:

```powershell
dotnet build Chainge.Postmortem.slnx
dotnet test --solution Chainge.Postmortem.slnx --no-build --coverage --coverage-output-format cobertura
dotnet run --project Chainge.Postmortem.Site --no-build -- evidence/2026-10-11-token-catalogue/tokens.csv registry/studies.json dist
```

Open `dist/index.html` in a browser, or serve `dist/` using a local static HTTP server. The generated HTML embeds its styles, JavaScript and registry data and needs no companion files. If a file preview does not run scripts, open the file directly in your browser instead. All runtime assets are embedded; no RPC connection, API key, analytics or remote font is required. Evidence links open public source reports and explorers. Only the generated `dist/` directory is intended for static hosting. It contains no archive files.

The output uses embedded assets and hash-based token selection, so it can be served under a GitHub Pages repository subdirectory without a server-side route fallback. Publication and remote Git pushes remain with the maintainer. No deployment workflow or automatic refresh is enabled.

## Sources and interpretation

- The generator reads the independently downloaded [public CSV](../evidence/2026-10-11-token-catalogue/tokens.csv) directly. It validates its headers/address syntax and groups addresses case-insensitively. All source row numbers and distinct source labels are retained. This produces 2,184 contract entries from 2,185 rows. Four hash-only source labels are retained verbatim.
- `studies.json` is an editorial presentation of cited reports. It is deliberately limited to selected BUSD positions and four reviewed outflow cases. Amounts are decimal strings, never floating-point ledger values. Numeric summaries truncate to two decimals; exact values remain in the data, detailed ledger/path views and source reports. Tiny nonzero balances display their full exact value beside the abbreviated amount.
- The other 2,183 contracts are selectable but unassessed. They have no invented supply, vaults or zero-balance defaults.
- Gross inflows include routing and internal transfers. They are not unique deposits. Newer-token supply is not a consolidated outstanding-claims denominator. No backing ratio or deficit is calculated with incompatible cutoffs or incomplete custody coverage.
- Movement cases are ordered by their stated initial BUSD amount. The 1.2-million case aggregates two separate outflows, not a single transfer. Repeated hops are not additive losses. An unmatched case has no verified burn linkage; this is not an allegation about its purpose. Cross-chain and redemption relationships remain explicitly inferred.
- Exchange destinations use attributed explorer labels; tracing stops at the boundary where public evidence cannot identify account credit or further disposition.

`studies.json` is an authored display selection, not a second accounting ledger. Changes to the research should be reviewed into this selection explicitly. A future collector should replace the authored amount fields with generated views of normalized events before claiming automatic refresh or exhaustive ranking.

## Implementation and checks

The Domain project holds the source token record. Application groups catalogue entries and attaches studies by contract address. Persistence.Json reads the local CSV/JSON sources and maps source records to Application models. Site composes dependencies and emits static assets; browser JavaScript controls presentation, search and selection. JSON serialization escapes untrusted token labels, which are also HTML-escaped when rendered.

Application tests cover duplicate-address grouping, source-row/alias retention, reviewed-first ordering and rejection of studies pointing outside the catalogue. The builder reached 100% line coverage; DI registration is excluded. Build completed with zero warnings. Manual data validation checked exact mint-minus-burn arithmetic, case ordering, transaction hashes and amounts against the cited reports.

Browser checks covered symbol search, no-result state, unassessed-token selection, investigation-only filtering, tabs and keyboard navigation, vault dust precision, outflow filtering and path selection. Overview and movements were checked at a 390-pixel viewport with no page-level horizontal overflow. No browser warnings or errors were observed. The UI does not test completeness of the underlying investigation.

The standalone-output regression check served only the generated index.html from an otherwise empty directory. The full catalogue, BUSD overview, movements and token search loaded without companion files or browser errors. This verifies HTTP browser execution; the Codex file preview may impose separate script restrictions.
