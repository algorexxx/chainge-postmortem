# Standard asset page format

Every investigated asset uses the same Overview, Vaults and Transfers renderers in the single generated `dist/index.html`. Do not add asset names, chain exceptions or custom asset layouts to the UI code. Supply reviewed data in `studies.json`; `verify.ps1` checks it against `studies.schema.json` before building or publishing.

## Add an asset

1. Copy the entry in `study-template.json` into `studies.json`, replacing its example key with the lower-case Fusion contract address from the catalogue. Replace all example text and URLs. Keep `schemaVersion: 1`.
2. Set the title, review date, status, scope and assessment URL. Use `overviewLinks` for related research. These fields contain plain text or public HTTPS links, never HTML.
3. Add reviewed `vaults` and original vault-outflow `flows` using the existing study structure. Keep downstream steps and branches inside their originating case. Connect the reviewed generated ledger with `movementsFile`; do not hand-edit its classifications.
4. Add the optional Fusion accounting and aligned vault comparison described below. Leave absent evidence absent: the shared layout shows “Not established”.
5. Run `./verify.ps1` and `node registry/check-ui.cjs`, preview every tab, review the evidence and staged diff, then submit your research PR under `CONTRIBUTING.md`.

## Fusion accounting

`fusionStatistics` contains `referenceBlock`, UTC `referenceTime`, a public `source` URL, `snapshotNote`, `tokens`, `checks` and `notes`. The optional `configurationSha256` refers to the exact LF configuration bytes used by reconciliation.

Each token row has `address`, `decimals`, `fullTime`, `mintedRaw`, `burnedRaw` and `eventCount`. Raw amounts are unsigned integer **strings**, never JSON numbers. The renderer derives remaining supply as created minus burned. Include only reviewed ledgers; explain partial inventories and nonstandard supply semantics in `notes`. Selecting a contract absent from these rows must not produce an assumed zero supply.

Each `checks` entry has plain-text `text` and a public evidence `source`. Do not claim every token or holder was checked unless the evidence establishes that. Optional `addressOne` holds `token`, `address`, `rawAmount` and `decimals` for a separately qualified position; it is not treated as a burn.

## Aligned vault comparison

`vaultComparison` contains UTC `referenceTime`, public `source`, `snapshotNote` and `groups`. Its reference time must match Fusion accounting before a comparison gap can be displayed. Pin source-chain blocks using adjacent headers around that instant, and preserve those headers and chain identity checks.

Each group represents one chain, token and block:

| Field | Meaning |
| --- | --- |
| `chain`, `token`, optional `tokenLabel` | Chain-qualified contract and readable representation name |
| `block`, `decimals` | Pinned historical block and token precision |
| `method` | How balances were obtained, such as historical calls or bounded history reconstruction |
| `treatment` | `included` or `separate`; a deliberate reviewed choice |
| `reason` | Required for `separate`, for example unverified backing |
| `positions` | One record per tested vault address: `holder`, `rawAmount`, optional evidence request `source` path |

Use `rawAmount: null` for unavailable state. An observed zero is the string `"0"`. An empty group is invalid. A missing included balance makes the included total and gap unavailable. Separate groups retain their own balances and explanations but never enter the main total or gap. This rule is generic; Rollux is one application, not a special branch in the code.

Do not duplicate the same chain/token/holder position, mix observation dates within a group, infer unqueried zero balances, or treat reconstructed history as a successful state call. A complete set of tested positions does not prove a complete vault inventory. Do not sum unrelated token units just because they share a symbol. The displayed gap is arithmetic, not a certified loss or redeemable-dollar amount.

## Provenance and publication

The compact reviewed snapshots serve the static UI without embedding the large generated accounting reports and their input-hash inventories. Record this purpose and refresh procedure in `snapshotNote`. Store raw positions rather than separately persisted totals; totals are derived during rendering. Keep original captures and manifests in `evidence/`, not just UI snapshots.

Existing `vaults` rows can describe later observations, but their dates remain explicit and they do not feed the aligned comparison. Historical/new observations are distinct evidence. Adding a token does not require another HTML file or JavaScript branch. Schema validation checks structure; it cannot certify evidence, chronology, completeness or custody.
