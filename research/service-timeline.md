# Chainge service and restoration timeline

Reviewed 2026-10-13. Status: preliminary communications timeline, not a determination of cause or liability.

The chronological timeline and selected verbatim source excerpts are maintained in [registry/timeline.json](../registry/timeline.json). The static site renders that file in the project-wide **Timeline** view (`#timeline`). It is separate from individual asset movements because a service announcement does not automatically apply to every token, address or chain.

## What this first pass establishes

The retained founder and community-admin messages distinguish an earlier beginner-mode security interruption, a wKAS route/queue problem, the 2025 stuck-transaction backlog, a blacklisting explanation, repeated funding estimates, an explicit service-suspension statement and subsequent funding updates. Every milestone is an **attributed statement**. An announcement supports what the account said, not whether its explanation or promised outcome occurred.

The 14 March 2025 explanation attributes liquidity depletion to blacklisted fund-holding addresses. It does not identify an asset issuer, chain-qualified vault, restriction transaction or authority. The claim that an issuer blocked particular assets therefore remains an open verification question. Do not turn the word “blacklisted” into a verified on-chain freeze.

The 26 March service-recovery message concerns a separately described AWS/domain interruption. It is not a statement that the outstanding withdrawal backlog had been settled. The 26 April three-day funding expectation was conditional on withdrawal from the crypto fund succeeding; the further one-week settlement estimate was conditional on receipt. Preserve these conditions when comparing promises with later updates.

The BUSD movements already documented in [source-chain flows](busd-source-chain-flows.md) and [the January trace](busd-case-assessment.md) precede the 2025 founder blacklisting explanation. Their timing alone cannot connect them to it. No universal “after shutdown” filter should be imposed on asset ledgers from this preliminary chronology.

## Provenance and privacy

The source is a Telegram Desktop JSON export of the public Chainge group, retained privately outside the checkout. Its observed range, counts and SHA-256 are recorded in the timeline coverage object. The date of this review is not the export completion date, which has not been supplied. A hash establishes integrity of the inspected bytes, not independent authenticity or completeness. A second independent backup has not been confirmed.

The maintainer confirmed that the author of [message 864721](https://t.me/chaingefinance/864721) is @djqian. Selected founder messages share that sender identity in the export. This supports attribution for this pass; the export does not itself preserve the username or certify past account control. No numeric sender identifier or private confirmation transcript is published.

Only allowlisted founder and confirmed community-admin messages were used for the published timeline. Exact excerpts were checked against the private original. Ordinary-participant content, reply/forward metadata, ticket-to-person associations, attachments and unrelated personal information were excluded. Selected excerpts are labelled as excerpts rather than presented as complete messages. Additional excerpts from a long statement remain separate, so omissions are visible. Public message IDs and links are retained only for the confirmed founder and community-admin sources.

UTC is derived from `date_unixtime`, not from the export's offset-free display date. `edited_unixtime`, where present, is retained separately. Message text is the version present at export; it must not be described as a recovered original revision. Edited content cannot be assumed to have appeared unchanged at initial publication.

The public JSON is both the reviewed research record and the site's input. The site generates its presentation from that single source. The SHA-256 manifest beside it records the reviewed derivative. Raw messages are not build inputs and were never copied into the checkout.

The maintainer confirmed Tanvir (@Nabil437), KaBii (@Kabii70) and James (@James_man) as community admins. This is role confirmation, not evidence of custody control or historical permissions. Only Tanvir has been added as a timeline source in this pass: his selected messages share the exported sender identity of [message 823196](https://t.me/chaingefinance/823196), which identifies @Nabil437 as his username. Private screenshots and confirmation transcripts are not published. KaBii and James require further message-level attribution review before their statements are added. Oana and Vlad remain discovery leads; names alone do not establish an account, employment role or authority.

The January 2025 admin statements describe a KAS migration and planned vault-address updates. The March admin statement describes BTC being affected first. Neither establishes an incident start date or verifies a freeze; the January migration must not be assumed to explain the later blacklisting claim.

## Coverage and remaining work

1. Verify the original official source of the earlier circulating blacklisting explanation. A repost by an unverified participant is a discovery lead, not an official announcement; its identifying information and exact text are not published here.
2. Identify historical staff/admin accounts and their roles before attributing their messages. Do not infer authority from display names or confident wording.
3. Establish the earliest affected withdrawal dates by asset and route. The founder's references to prior work or legal involvement are retrospective claims, not exact incident onset dates. Ordinary-user complaints require separate privacy review and corroboration before use.
4. Obtain specific restricted addresses, token contracts, chains, restriction times and issuer/authority evidence. Distinguish an issuer's contract restriction, exchange compliance label and another kind of access failure.
5. Correlate funding and settlement claims with independently verified transaction receipts and the outstanding-token/vault ledger. A cheque, a stated fund release or a count of resolved tickets does not prove receipt by users.
6. Review original announcement channels, linked AMAs and relevant media. The first pass is text-focused; it does not transcribe attachments or exhaustively preserve linked websites.
7. Review Fusion's HTML export for infrastructure context, keeping it separate from Chainge redemption claims. It was inventoried but not analyzed in this pass.
8. Expand beyond the founder's latest retained message. No later message from that account was found in this Chainge export after 9 October 2025 through the export's last retained record on 11 October 2026. This says nothing conclusive about deleted messages, other groups, private messages or other platforms.

The web tool could not retrieve the selected Telegram public message/profile pages during this review. Message URLs are constructed from the public handle and exported IDs; their live availability and current text have not been independently reverified. Neither external commentary nor automated summaries of chat messages were used as evidence for the published milestones.

## Corrections

Append or correct milestones with their sources and explicit uncertainty. Preserve local Git history for changes in attribution, chronology and interpretation. Do not silently replace an attributed claim with a verified event or mark a target as achieved merely because its date passed.
