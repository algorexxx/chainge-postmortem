# Repository publication audit

Reviewed 2026-10-13, against the 32 commits reachable from local refs before this audit change. The initial pass inspected 2,515 distinct Git blobs (116,667,189 bytes), including text extracted from the retained public PDF and XML members of any ZIP containers. This is a bounded content audit, not a guarantee that arbitrary secrets or personal information are absent.

## Findings and remediation

A downloaded public chain-directory response in `evidence/2026-10-11-busd-payout-discovery/chain-registry.response.json` contained unrelated RPC endpoint authentication and provider-key material. It entered history in pre-rewrite commit `b25160e`. The current file is now a filtered derivative retaining only the relevant BNB Smart Chain record; the package manifest and provenance note have been updated. No removed credential-bearing endpoint was used for this investigation. Credentials were not tested or reproduced in this report.

Local history cleanup completed on 2026-10-13 with maintainer authorization. The original capture was replaced with the sanitized derivative throughout local commit history, with corresponding historical manifest checksums updated. A retained Codex checkpoint tree was sanitized separately. Obsolete reflogs and objects were pruned, and the original capture blob is no longer present in the local Git object database. The rewrite preserved the current committed file tree. Git integrity verification passed.

The post-cleanup scan inspected all 2,805 stored Git objects, including unreachable objects (2,522 blobs, 250 trees and 33 commits; 115,949,519 uncompressed bytes). Content patterns were checked in blobs and commit/tag metadata; binary tree records were enumerated. It also checked 2,562 current tracked/generated files, including the generated site HTML. No remaining matches were found for the credential and privacy patterns described below. These counts precede the documentation commit recording completion.

No remote push was performed. The maintainer must replace remote history with the rewritten branch. Local cleanup cannot erase copies in existing clones, remote retention or external caches.

No additional hits were found in the scanned history for private-key blocks, common hosted-service token formats, credential assignments, raw Telegram record fields, known private community-lead identifiers, private IPv4 ranges or local user-directory paths. URL credentials and common provider-key paths were separately checked. Email-pattern matches in file contents were parts of the same endpoint authentication URLs. This does not cover arbitrary unlabeled credentials, steganography or unknown encodings. Git author/committer identity metadata is distinct from file contents and has not been anonymized.

The public joint contributor statement intentionally retains its authors’ names. Founder/admin attribution, public on-chain addresses and the requested donation address are also intentional public material. Raw chat exports, screenshots and the community source-selection index remain outside the repository. Ignore rules additionally exclude private export/review directories and common credential-container filenames; ignore rules do not protect already tracked history.

## Explorer links

Rendered contract, vault, controller, traced-address, recipient and transaction references use chain-specific explorers: FSNEX for Fusion, BscScan for BSC, Etherscan for Ethereum and Snowtrace for Avalanche. Composite event IDs retain their log index as the visible label and link to the transaction. Receipt and delayed-candidate JSON shown in the UI receives the same contextual linking. File checksums remain checksums, and evidence filenames retain repository links. Token and movement selection controls have adjacent explorer links so navigation controls are not nested inside links.

Links are constructed from validated address/hash shapes and known chains. Unknown chains are not guessed. This change covers the site’s rendered interface; downloadable raw datasets and externally rendered GitHub research documents remain evidence files, not automatically linkified web pages. Explorer route availability and index completeness are separate from correct URL construction.


## October 15 closing-pass additions

The new and modified public material was checked for private-key blocks, common hosted-service tokens, credential-bearing URLs, private IPv4 ranges, local user paths and raw Telegram identity fields. No matches were found in that bounded scan. Capture endpoints were anonymous public sources; staged paths exclude working archives, exports, screenshots and generated deployment output. Original webpage response bytes retain their whitespace and hashes. This is an additions review, not a repeat audit of every historical Git object or a guarantee against arbitrary unknown encodings.

The BAT additions received the same bounded pattern review with no matches. All 1,878 newly retained evidence files covered by manifests matched both SHA-256 manifests and exact staged Git blob bytes. Failed private Fusion connection probes omit their endpoint; working connection scripts remain ignored. BAT configuration/report/site hashes agree with LF configuration bytes. No community submissions or raw exports were added, and no remote push was performed.
