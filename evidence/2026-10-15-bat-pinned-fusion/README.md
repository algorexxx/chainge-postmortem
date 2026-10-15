# BAT pinned Fusion state

Read-only calls at block 15130080 through existing authorized access to the restored Fusion node. Connection details are omitted. The public FSNEX API remains the default for logs, receipts, calldata and headers; it does not expose arbitrary pinned state.

Standard configured balances agree with event replay. Old FRC758 event sums do not agree with ordinary balances and must not be treated as a standard ledger. Restriction calls for standard legacy children target their parent; newer calls target the factory returned by the token. Calls made for early nonstandard tokens to that later parent do not establish their restrictions and are excluded from published flags. Child totalSupply calls revert; those failures are preserved, not interpreted as zero. Parent and child runtime checks normalize only known Solidity compiler metadata digests.
