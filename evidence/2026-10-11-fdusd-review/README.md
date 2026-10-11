# FDUSD candidate-address review

This package supports the [FDUSD follow-up](../../research/busd-fdusd-followup.md). It records a bounded negative search, not proof that replacement reserves do not exist.

- `candidate-addresses.json`: 20 address strings without the `0x` prefix, keyed by research labels. Labels select leads; they are not chain-independent ownership or vault findings.
- `eth-state-*`: pinned Ethereum balance calls, decimals and deployed code.
- `bsc-state-latest-*` and `opbnb-state-latest-*`: successful unpinned current-state calls. Corresponding `fresh-head` and `state-after-head` observations provide timing context, not exact per-call block attribution.
- `eth-discovery-*`: Routescan requests described in capture metadata and returned empty arrays. The index is known to be incomplete from prior research.
- `eth-probe`, `bsc-publicnode-probe`, `opbnb-probe`: original requests and responses, including headers and historical-log access failures. The small successful empty opBNB probe is not an exhaustive history or a January-window search.
- `explorer-observations.json`: manually transcribed browser observations from Etherscan/BscScan, exact repeatable filter URLs and positive controls. The controls have not been receipt-verified in this package and establish only that the UI returned transfer rows.
- `public-source-notes.json`: attributed paraphrases and contract-link observations from public Binance pages read on the assessment date. These are research notes, not original-page archives.
- `manifest.json`: saved-byte SHA-256 values for every distributed file except the manifest itself.

The remaining unsuccessful provider probes and raw working files stay in the ignored local archive. No raw Telegram data or direct community submission is distributed. This package does not preserve a full copy of the web pages or claim independent completeness of explorer indexes.

Validation matched every balance request's ABI address argument to the candidate list, checked response IDs, confirmed all 60 results are valid zero uint256 values, and checked decimals and nonempty deployed code on each chain. Historical-query errors remain errors. Ethereum's pinned block is 26170686; BSC and opBNB observations use `latest`. The report states exact coverage and limitations.
