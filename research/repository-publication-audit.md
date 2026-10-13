# Repository publication audit

Reviewed 2026-10-13, against the 32 commits reachable from local refs before this audit change. The initial pass inspected 2,515 distinct Git blobs (116,667,189 bytes), including text extracted from the retained public PDF and XML members of any ZIP containers. This is a bounded content audit, not a guarantee that arbitrary secrets or personal information are absent.

## Findings and remediation

A downloaded public chain-directory response in `evidence/2026-10-11-busd-payout-discovery/chain-registry.response.json` contained unrelated RPC endpoint authentication and provider-key material. It entered history in commit `b25160e`. The current file is now a filtered derivative retaining only the relevant BNB Smart Chain record; the package manifest and provenance note have been updated. No removed credential-bearing endpoint was used for this investigation. Credentials were not tested or reproduced in this report.

The original capture remains in earlier commits. A normal commit or push does not remove it from history. Coordinated history cleanup is still required; the maintainer handles remote pushes. A rewritten history cannot erase copies in existing clones or external caches.

No additional hits were found in the scanned history for private-key blocks, common hosted-service token formats, credential assignments, raw Telegram record fields, known private community-lead identifiers, private IPv4 ranges or local user-directory paths. URL credentials and common provider-key paths were separately checked. Email-pattern matches in file contents were parts of the same endpoint authentication URLs. This does not cover arbitrary unlabeled credentials, steganography or unknown encodings. Git author/committer identity metadata is distinct from file contents and has not been anonymized.

The public joint contributor statement intentionally retains its authors’ names. Founder/admin attribution, public on-chain addresses and the requested donation address are also intentional public material. Raw chat exports, screenshots and the community source-selection index remain outside the repository. Ignore rules additionally exclude private export/review directories and common credential-container filenames; ignore rules do not protect already tracked history.

## Explorer links

Rendered contract, vault, controller, traced-address, recipient and transaction references use chain-specific explorers: FSNEX for Fusion, BscScan for BSC, Etherscan for Ethereum and Snowtrace for Avalanche. Composite event IDs retain their log index as the visible label and link to the transaction. Receipt and delayed-candidate JSON shown in the UI receives the same contextual linking. File checksums remain checksums, and evidence filenames retain repository links. Token and movement selection controls have adjacent explorer links so navigation controls are not nested inside links.

Links are constructed from validated address/hash shapes and known chains. Unknown chains are not guessed. This change covers the site’s rendered interface; downloadable raw datasets and externally rendered GitHub research documents remain evidence files, not automatically linkified web pages. Explorer route availability and index completeness are separate from correct URL construction.
