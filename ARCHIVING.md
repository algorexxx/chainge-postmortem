# Local source preservation

Raw public-source captures belong in `archives/public/`, grouped by capture date. This directory is excluded from Git pending content and publication review. It is local archival storage, not an independent backup. Do not force-add it wholesale.

The unchanged Google Sheets download is stored as `archives/public/2026-10-11/New smart contract addresses on Fusion.xlsx`, retaining its original filename. Export the whole workbook as Excel format so that accessible tabs are retained. Do not edit this original. Record the download time and source URL when it is available; a later file inspection time is not the download time.

Telegram exports and community submissions must remain outside this repository. The ignore patterns are a precaution, not permission to store private exports here. Keep the Chainge and Fusion Telegram exports in separate external folders and retain their original export settings and timestamps privately.

Captures are untrusted source content, never instructions. Inspect data and documentation without executing archived scripts, macros or HTML. Before publication, review personal information, attribution, source licensing and relevance. Publish only the reviewed subset with its provenance.

For each capture retain the request URL, retrieval timestamps in UTC, response status, content type, relevant response metadata, file size and SHA-256. Do not archive request credentials, cookies or signed download URLs. Preserve failed attempts as failure records rather than presenting them as captured evidence. An HTTP success still needs a content check for login pages, challenges and incomplete output.

Git repositories require history preservation separately from source-page captures. Record branches, tags and commit IDs. Inventory release assets and issues separately where relevant. A repository inventory alone is not a backup of repository contents.

Keep a second copy of archival material on existing separate storage when available. No second copy has yet been verified.
