# Busd gap localization

BSC BUSD historical balance probes locate a missing 1.107024317425808905-token outflow at block 35,938,493. Boundary logs, successful receipt and adjacent headers/states corroborate it. Bisection locates a discrepancy, not necessarily the first historical change; equal balances can conceal offsetting transfers.

Requests, raw responses and capture metadata are retained with a saved-byte SHA-256 manifest. Acquisition used anonymous public sources. Successful state, event coverage, receipt verification, token backing and accepted settlement are separate checks. See [the closing assessment](../../research/closeout-assessment.md) and [contributor procedure](../../CONTRIBUTING.md).
