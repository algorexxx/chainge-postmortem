# USDC downstream: Arbitrum and Hyperliquid

Reviewed 2026-10-13. This follows the separate [416,050.404450-USDC Ethereum payment](https://etherscan.io/tx/0x626d2b4a805005e7556d0180233f3b2913c4d6f185a5fd7a91b6557c17e0392e). The [USDC assessment](assets/usdc.md) records its split, the rejected direct-redemption match and the CCTP source/destination message link. No human owner or authorized purpose is established here.

## The bridged amount entered Hyperliquid

The [CCTP mint](https://arbiscan.io/tx/0xc7661ff75f5b8c9db1209aabd4957441674688128ee8e7042ee60cefec588e6a) delivered **316,050.304450 native USDC** to [584d…fea6](https://arbiscan.io/address/0x584de4fe0aea85fa45f5c4d7f50e09deeeecfea6) on September 4, 2024 at 02:57:02 UTC. At 03:09:19 it [sent 316,050.300000 USDC](https://arbiscan.io/tx/0xea07d3f2140a9452e2e4af93ca1413d18212cd6461c6cffaaf91fe582bf9e697) to [2df1…3df7](https://arbiscan.io/address/0x2df1c51e09aecf9cacb7bc98cb1742757f163df7), leaving **0.004450 USDC**. Both receipts succeed. These are the first two native-USDC events returned by both-direction queries from genesis for this address.

[Hyperliquid's official bridge documentation](https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/usdc) identifies that exact contract and describes crediting deposits to the sending account. Its public account API independently records the same deposit transaction at 03:09:22.942 UTC. The current documentation calls this the legacy bridge; that present-day terminology is not evidence of its status in September 2024. This is a Hyperliquid deposit, not a centralized-exchange attribution.

## Returned venue ledger

| UTC timestamp | API entry | USDC amount | Arbitrum transaction |
| --- | --- | ---: | --- |
| 2024-09-04T03:09:22Z | deposit | 316050.3 | [Transaction](https://arbiscan.io/tx/0xea07d3f2140a9452e2e4af93ca1413d18212cd6461c6cffaaf91fe582bf9e697) |
| 2024-09-29T18:09:25Z | withdraw | 274999.0 | [Transaction](https://arbiscan.io/tx/0x0da81d28d88cd6d8d3224b1932cdcdbef5e240e77c586b80b1133a70e9c4dce3) |
| 2024-10-12T04:34:46Z | withdraw | 45000.0 | [Transaction](https://arbiscan.io/tx/0xc51a1b3129c78c5e0893b864502feead7d5c6b2831525342eaa7c6d9638ba529) |
| 2024-11-14T15:51:50Z | withdraw | 361367.05 | [Transaction](https://arbiscan.io/tx/0x29ab9cdb12fd6579b4ceaa3e2afcfbc1608e49457dc69e1d232579a4a233b876) |
| 2024-12-23T06:26:29Z | deposit | 44258.91 | [Transaction](https://arbiscan.io/tx/0xa2253a09ea0b32314a6245b101759e5f8cb2d73fc3ab059ee91dd948d205564f) |
| 2025-01-18T01:08:13Z | withdraw | 102842.644973 | [Transaction](https://arbiscan.io/tx/0x55c39ad13872267fbe41d8bea74215c3ecfa8ec8046297dc7e328aadc4d5eda4) |
| 2025-01-20T00:28:28Z | deposit | 102842.64 | [Transaction](https://arbiscan.io/tx/0xd9b575b0eed793462a665564c75e92ea88aae357d7996c3574ec8d0b0a53e9cc) |
| 2025-03-17T21:02:43Z | withdraw | 102841.64 | [Transaction](https://arbiscan.io/tx/0xd8b1b14a29f119de498afc545717f7a30fa8d37a6a6538b5a983f21f42eb6f12) |

Amounts are the API's `delta.usdc` fields. Withdrawal entries also report a 1-USDC fee; these fee fields are not silently subtracted a second time from the on-chain transfer amounts. Each listed deposit/withdrawal hash occurs in the retained Arbitrum history. Deposits total 463,151.850000 USDC and withdrawals total 887,050.334973 USDC in the returned ledger. Their difference is **not** a claimed trading profit: a full economic reconciliation must also account for funding, fees, positions, transfers and any API retention limits.

The available fills pages contain **6,707 distinct BTC fill records** from September 6, 2024 through January 13, 2025, deduplicated by `(tid, hash)`. The endpoint returns at most 2,000 per page and exposes only the latest 10,000 fills. Inclusive timestamp pagination returned a final shorter page. These observations show recorded trading activity in this account; they do not prove who traded or whether every historical fill remains available.

At capture, the main perpetual account reports zero account value, zero withdrawable funds and no positions. The spot balance list and vault-equity list are empty; subaccounts returns null. The Arbitrum native-USDC `balanceOf` is zero at block **514479102**. These are separate snapshots and are not a claim about every asset or other account controlled by the same person.

## On-chain withdrawals did not remain at the recipient

The Arbitrum history contains 33 distinct native-USDC transfers. All 33 have retained successful receipts and block headers; replay ends at zero and matches the pinned state read. In particular:

- The 274,999-USDC Hyperliquid withdrawal was followed by a [274,999-USDC payment](https://arbiscan.io/tx/0xe2b7fabad546e18a79b44a1f62f72d94d1953fe884978edab8b0e0ada2a20e16) to [5fcb…b03b](https://arbiscan.io/address/0x5fcb0eb5958383136768675f9ac593289de9b03b). This is the same address string as the Ethereum USDT recipient in the assessment; a common string alone does not settle the relationship between those flows.
- The 45,000-USDC withdrawal was followed by a [45,000-USDC payment](https://arbiscan.io/tx/0x98befd910929b045f8252871fe0ca0fe53ca45fd47ab8e9323124671d3011827) to [27bc…3b22](https://arbiscan.io/address/0x27bcf108994ef67404ca74ebb51ae802f33c3b22).
- Later CCTP transfers of [230,730.929738](https://arbiscan.io/tx/0x5cbf24c1bb0171bfaf1f3668206398bfbcdcbfa92ff9c5638193bf08489fa25f), [102,842.646711](https://arbiscan.io/tx/0x150a85b85887d74fe5ca75a810da2d8cedbdf76e8f124ddb0b4733bf4516a338) and [102,841.646711 USDC](https://arbiscan.io/tx/0xf596ddf65de5b8165ce5806062e8b803cb61a8d33def363c7fd75cbc30ea05cd) went through [Circle TokenMinter e7ed…ba48](https://arbiscan.io/address/0xe7ed1fa7f45d05c508232aa32649d89b73b8ba48).

Small incoming transfers and zero-value logs also remain in the ledger. Do not merge similar-looking addresses by prefix/suffix; the full addresses differ. Later CCTP funding and venue activity prevent treating every subsequent payment as uniquely belonging to the original 316,050-USDC branch.

## Return to the candidate and later CCTP routes

At **2024-09-30 03:01:18 UTC**, [5fcb…b03b returned 274,999 USDC](https://arbiscan.io/tx/0x97e81005a3d3aa342656e5426c4621d976d3563ef54c52812940f3fddee9d570) to [66ff…ecf1](https://arbiscan.io/address/0x66ff2f0ac3214758d1e61b16b41e3d5e62caecf1) on Arbitrum. Its successful call to [f422…2d9f](https://arbiscan.io/address/0xf422ec7591e83612a39dbdf94563f20133ef2d9f) carries a readable `chainge:ETH` memo naming Ethereum USDT and the same recipient. The retained full native-USDC query history for 5fcb contains three rows: this return, the earlier 274,999-USDC receipt, and a 0.002749-USDC transfer from a different similar-looking address. Replay matches the pinned **0.002749 USDC** state.

At **04:21:11 UTC**, the [previously reviewed Ethereum exchange](https://etherscan.io/tx/0x3e0a6a69ce57f3706916f04a050f2a1d53c13353d397d3c62f28e5639905217e) spent 274,999 USDC from the candidate and delivered 272,074.135714 USDT to the same recipient. Together, timing, source amount, named destination chain/token/recipient and receipts support a cross-chain exchange interpretation. The memo's output-related value 272,074.607395 differs from the actual USDT payment by 0.471681; its precise quote/minimum/fee semantics are unverified. This is not an authenticated settlement identifier, but the Arbitrum receipt supplies funding context that the Ethereum receipt alone did not show. No Fusion burn is needed to explain an ordinary source-chain-to-source-chain exchange; that is an interpretation of this route, not proof about every candidate outflow.

The later e7ed recipient is **Circle's TokenMinter contract**, not an identified end-user wallet. The relevant receipts transfer USDC into it and burn that same amount, emitting CCTP messages. Matching source domain, destination domain, nonce, sender and the full 132-byte message body connect these routes to successful destination receipts:

| Direction | Nonce | USDC minted at destination | Source receipt | Destination receipt |
| --- | ---: | ---: | --- | --- |
| Arbitrum → Ethereum | 375508 | 1.000000 | [Source](https://arbiscan.io/tx/0x79e326cdab34ae7165e712a0777dee69c61fe5ac74c6d17185057d3cef11804c) | [Destination](https://etherscan.io/tx/0xb06056403ef6565b70691625dc2a998f462260ad5d01a34fe1b406edb6536321) |
| Arbitrum → Ethereum | 375610 | 230,730.929738 | [Source](https://arbiscan.io/tx/0x5cbf24c1bb0171bfaf1f3668206398bfbcdcbfa92ff9c5638193bf08489fa25f) | [Destination](https://etherscan.io/tx/0xb47bf1661543e05d18b8630fc777ffae3ee7a9455b20dead67fa54b424d67d93) |
| Arbitrum → Ethereum | 417708 | 102,842.646711 | [Source](https://arbiscan.io/tx/0x150a85b85887d74fe5ca75a810da2d8cedbdf76e8f124ddb0b4733bf4516a338) | [Destination](https://etherscan.io/tx/0x0138132eb02e122a52c6108ef7e4b16b8d44015c161cb0aa6e95500f6b9884e7) |
| Arbitrum → Ethereum | 471858 | 102,841.646711 | [Source](https://arbiscan.io/tx/0xf596ddf65de5b8165ce5806062e8b803cb61a8d33def363c7fd75cbc30ea05cd) | [Destination](https://etherscan.io/tx/0x7dbc326018e5c63dd0ab86066a9c97843c8e045c9a35b453059dcbafc8521750) |
| Ethereum → Arbitrum | 104217 | 316,050.304450 | [Source](https://etherscan.io/tx/0xd292b5ec3edc809ae826ad69db887862b9a6459fd64750c03e6c00480a7f48ac) | [Destination](https://arbiscan.io/tx/0xc7661ff75f5b8c9db1209aabd4957441674688128ee8e7042ee60cefec588e6a) |
| Ethereum → Arbitrum | 160814 | 44,258.911738 | [Source](https://etherscan.io/tx/0xa0573bd775732c5d4c691c13b6b3adcdc8880b11151a2001e3d0ac0416a789b4) | [Destination](https://arbiscan.io/tx/0x6774d3fefb48604a1eec37dd2c4e561e43f9885c5af7a7b3841665e9fb27eed9) |
| Ethereum → Arbitrum | 182598 | 102,842.646711 | [Source](https://etherscan.io/tx/0xf7db6ba0e11dec6869aa202054c639301c20d6d60a0da59a8968c281bd64cde0) | [Destination](https://arbiscan.io/tx/0x3f36c04d846bf349af3d9120648fd1975731de4555a326726552bb787d561b76) |

The Ethereum recipient history returned 59 explorer rows, all corroborated against retained successful receipts. Replay and the pinned state are zero. This is paginated explorer discovery, not independently complete bounded RPC history. Later Ethereum payments include further transfers to the previously examined [a4b6 recipient](https://etherscan.io/address/0xa4b6aa5f955f3a944cca20e65b22280a47cc8673), other recipients and CCTP redeposits. These later payments must not be added to the original 416,050-USDC outflow as new vault losses.

The attempted full history of Circle's shared minter timed out. Its early returned rows include unrelated users and are excluded from this token's selected address ledger. The relevant source receipts already identify the burns and destination messages, so a global history of that shared protocol contract is unnecessary for these seven links.

## Evidence and limits

[Bounded logs and pinned balance](../evidence/2026-10-13-usdc-cctp-destination/), [receipts and headers](../evidence/2026-10-13-usdc-cctp-context/) and [official documentation and venue API captures](../evidence/2026-10-13-usdc-hyperliquid/) retain raw bytes, request parameters, capture times and SHA-256 manifests. API documentation explains the observation; it does not authenticate beneficial ownership or provide a full independent venue-state replay. No custody address or settled redemption is automatically inferred from this downstream trace.

Additional evidence: [Arbitrum return context](../evidence/2026-10-13-usdc-return-context/), [bounded downstream queries](../evidence/2026-10-13-usdc-downstream-single/), [Ethereum discovery pages](../evidence/2026-10-13-usdc-return-ethereum/) and [Ethereum receipts, headers and state](../evidence/2026-10-13-usdc-return-ethereum-context/).
