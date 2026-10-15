# Usdc polygon terminal context

Original transaction inputs and recipient-only outgoing USDC queries from each material vault payment block through 50,000 blocks later. Shared routing contracts are excluded from recipient-history expansion. The window is a bounded first-hop search, not complete later activity; original case IDs bind queries to source outflows. One transaction input returned a provider error and was captured separately via fallback.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
