# USDC outflows after the preserved Fusion snapshot

Reviewed 2026-10-15. Three retained USDC outflows occur after the May 27, 2025 suspension statement and after the Fusion reference block. All originate at [4563…e039](https://etherscan.io/address/0x456325f2ac7067234dd71e01bebe032b0255e039). These cases concern observed use of a likely vault address; they do not establish ownership or a customer deficit. See [Fusion snapshot provenance](fusion-snapshot-provenance.md).

## Findings

| Vault outflow | Immediate funding | Observed destination |
| --- | --- | --- |
| 12.938852 USDC, October 10, 2025 14:47:35 UTC | Exactly 12.938852 USDC arrived 13 minutes 36 seconds earlier, from a token-sale transaction initiated by the same vault | Swapped for 0.002989706321703827 ETH, returned to the same vault according to explorer call traces |
| 542.761951 USDC, October 10, 2025 15:49:11 UTC | Exactly 542.761951 USDC arrived 108 seconds earlier, alongside a burn of another token by the vault | Swapped through USDT into 0.131560072533994001 ETH, returned to the same vault according to explorer call traces |
| 634.222907 USDC, January 27, 2026 16:03:11 UTC | Exactly 634.222907 USDC arrived January 20 at 14:22:59 UTC, following another token sale | Forwarded in full after 108 seconds to an address labelled OKX 225 by Etherscan |

These amounts were newly received **after** the Fusion snapshot. The retained USDC history reconstructs a zero balance before each funding event and after each outflow. They are not evidence that these amounts were held as USDC at the October 7 reference and then removed. That does not establish the ownership or backing purpose of the assets sold to generate them.

## October 10: token conversions with ETH returning to the vault

The [first funding transaction](https://etherscan.io/tx/0xe36ea02898a36e0d3c5c627d3efc04864fc3835d5e356113204df3fe4e1f23cc) is initiated by the vault and transfers a different [token](https://etherscan.io/address/0xc50ef449171a51fbeafd7c562b064b6471c36caa) out of it. Its successful receipt includes the router paying **12.938852 USDC** back to the vault. The subsequent [outflow](https://etherscan.io/tx/0x3823e138982b276b1046a6c552c7db7a2da083b5d63610751360164b8d1e1eda) sends that USDC to [88e6…5640](https://etherscan.io/address/0x88e6a0c2ddd26feeb64f039a2c41296fcb3f5640). The receipt shows WETH moving from that pool through the router and an unwrap operation; the retained explorer call trace shows **0.002989706321703827 ETH** returning to the vault.

The [second funding transaction](https://etherscan.io/tx/0x0feaa3dd06e963afddb344ff04cca33048d0af39ec5da6a4259c54b384632985) burns raw `542761951467755553197` units of [another token](https://etherscan.io/address/0xd585aaafa2b58b1cd75092b51ade9fa4ce52f247) and pays **542.761951 USDC** to the same vault. A position withdrawal is a plausible interpretation; this review does not authenticate the token's redemption semantics. The [subsequent swap](https://etherscan.io/tx/0xd15fdd43bbd24ed88ee64bf7cb92a671b7be9818f5f377cbc9f406fd5694ef50) transfers USDC to [3416…27c6](https://etherscan.io/address/0x3416cf6c708da44db2624d63ea0aaef7113527c6), receives USDT, routes it into WETH, and unwraps. Explorer call traces show **0.131560072533994001 ETH** returning to the vault.

Both original transactions were signed by the vault address and call [2e1d…8764](https://etherscan.io/address/0x2e1dee213ba8d7af0934c49a23187babeaca8764). These are observed conversions, not direct USDC payments to customers. ERC-20 transfers and successful execution are receipt-corroborated; native ETH returns use explicitly separate explorer trace evidence. This review stops at the returned ETH and does not establish its later use.

## January 27: full onward transfer to an exchange-labelled address

On January 20 the vault initiates a [transaction](https://etherscan.io/tx/0x23f813b10b6024314064a9d230beda43df8a7c2456b2ffeb74e29d5d4533285c) transferring [another token](https://etherscan.io/address/0x23b99002a1749cc4fb81d8a4a2d56acf4a2b47ad) out. Its successful receipt contains a multi-pool conversion and **634.222907 USDC** paid back to the vault. No customer-deposit interpretation is established for this funding.

On January 27 a [direct transfer](https://etherscan.io/tx/0x5e4a63c30ff5e019abb98ce4c72f58d59f30b7612a3683086ac008fc4cc6cf0c) sends the complete amount to [186f…dfc8](https://etherscan.io/address/0x186fa8ffffb1b744f875e99239716b3f6acddfc8). At 16:04:59 UTC the intermediary [forwards exactly 634.222907 USDC](https://etherscan.io/tx/0x307b499de8fe35227fa974b683ef37d26e2b9caa2e09e235b2865b85aa011853) to [445f…bd97](https://etherscan.io/address/0x445f16314284b43dfa1fd3cd77b9dea4a1bebd97). A successful receipt confirms the exact token, sender, recipient and amount; a block-hash-matched header establishes the 108-second delay. A pinned balance call returns zero USDC at the intermediary after this transfer.

Etherscan labels the destination **OKX 225**, including on its retained [creation transaction page](https://etherscan.io/tx/0x96f29c5f8d5841800185209a985b6cb42a52cf4f8a2929d61b1db2bd2ddff77e). This is an explorer attribution, not independent proof of ownership or identification of an exchange customer. The first recipient did not retain the amount intact. The exchange-labelled endpoint is where this trace stops; neither exchange credit nor subsequent off-chain disposition is known.

## Earlier burns and pending withdrawals

The generated Fusion purpose inventory contains **8,250 burn events**. None equals any of these three USDC amounts exactly. Among **797 decoded routing observations**, none names the January recipient, the two October pool recipients or the vault itself as a destination, and none quotes one of these exact raw USDC amounts. These are bounded negative results from the retained USDC dataset, not proof that no withdrawal instruction existed. Fees, other assets, unsupported memos, off-chain queues and post-snapshot Fusion activity remain unobserved possibilities. No arbitrary fee or delay tolerance was introduced to manufacture a match.

The automatic ledger remains unchanged. The reviewed October cases are token conversions; the January case remains unmatched with an exchange-labelled endpoint. Funding provenance changes their interpretation, but does not establish benign purpose, wrongdoing or coverage of outstanding Fusion liabilities.

## Reproduction and evidence

Run reconciliation for USDC, then `./research/check-usdc-post-snapshot.ps1`. It checks the selected receipt logs, transaction senders, block-bound timestamps, equal funding, explorer-reported ETH returns, onward transfer, pinned zero balance and the bounded burn/memo search. It does not certify source completeness or native-call traces independently.

- [RPC captures](../evidence/2026-10-15-usdc-post-snapshot-rpc/): three original transaction inputs and headers, three funding receipts and inputs; anonymous Ethereum RPC with chain identity verification.
- Original outgoing receipts: retained `ethereum-corroboration-v3-005`, `049` and `016` responses in [the investigation package](../evidence/2026-10-13-usdc-investigation/).
- [Explorer captures](../evidence/2026-10-15-usdc-post-snapshot-explorer/): two complete returned internal-call pages and the intermediary's 16-row USDC history, with no further returned cursor. Pagination completion is not an independent guarantee of chain completeness.
- [Onward RPC captures](../evidence/2026-10-15-usdc-post-snapshot-onward/): receipt, timestamp header and balance call pinned to the onward block.
- [Label capture](../evidence/2026-10-15-usdc-post-snapshot-label/): Etherscan HTML and source metadata.

Each package has a SHA-256 manifest. No private messages or community identities were added.
