# Phase 1A Event Reconciliation — Red Hot and Deep Blue

本報告對應 encounter `f099db96-be70-4ac2-99a7-8b0ca1ea170f`、territory `1322`。Phase 1A 只增強開發者診斷；沒有修改 `RaidDpsEstimator`、正式 DamageMetricDuration、使用者可見 DPS/rDPS、Overlay 或 Host。

FFLogs 參考仍來自使用者截圖中的 report `nLM8q6TFA2kfVyc3` / `fight=last`。該截圖只顯示 `78.93M`，沒有 exact event export，因此本報告把“本地觀察”“官方語義”“推斷候選”和“未知”嚴格分開。

## 結論摘要

- DACT `80,461,274` 可以精確守恆為 `76,449,628` direct ActionEffect + `4,011,646` normalized periodic damage。
- Phase 0 的 `friendly-fire candidate = 165,198` 應改名為 `party-target periodic candidate`。它是 8 個正值 raw `24` tick；歷史檔案沒有 normalized target rows，不能把它直接認定為最終 DACT outgoing 或 FFLogs exclusion。
- `Parser normalization = +251,705` 已按 raw target bucket 拆為：normalized periodic 相對 party-source raw Boss periodic 增加 `416,903`；provisional raw total 還包含 `165,198` party-target periodic，所以 layer-total 淨值嚴格為 `416,903 - 165,198 = 251,705`。這是守恆分解，不宣稱兩項就是 parser 的逐事件加/減規則。
- 兩個 SMN 的 47 個 summon entity instance 被合併到 owner，但 raw direct source damage 與 owner 聚合後 DACT direct damage逐 actor完全相等；沒有 owner + pet 雙計。
- fight 範圍內 2,053 個 party-owned direct events 只命中 `炽红`、`深蓝`。沒有 trash/add/non-fight/environment/friendly direct target 被加入最終 outgoing total。
- 唯一 packet-HP overkill candidate 是 `21:31:42.388`、packet `00007CA8`、`笙夜之歌` 的 `团契` 對 `炽红`：raw `120,259`，packet 前 HP `91,744`，candidate `28,515`。FFLogs 官方 calculated-damage schema 明確 `amount` 不包含 overkill；是否該 event 在此 fight 的 exact amount 為 `91,744` 仍需 exact export 驗證。
- 兩 Boss 的 max HP 都是 `39,480,861`，合計 `78,961,722`。`78,961,722 - 28,515 = 78,933,207`，落在截圖 `78.93M` 的顯示區間內。這個一致性很強，但“兄弟同心/shared HP 如何被 FFLogs 處理”沒有公開的 encounter-specific event rule，因此只能標為 `inferred`。

## Total Damage 與候選差異

| 專案 | Amount | Evidence status | 說明 |
|---|---:|---|---|
| DACT saved total | 80,461,274 | observed | saved encounter exact total |
| FFLogs screenshot interval | 78,925,000 .. 78,935,000 | observed | `78.93M` 按 0.01M 顯示精度形成的區間 |
| DACT - screenshot delta | 1,526,274 .. 1,536,274 | observed/bounded | exact reference total 尚缺失 |
| Combined boss max HP | 78,961,722 | observed | `39,480,861 × 2` |
| Final packet overkill candidate | 28,515 | observed local + official semantic | packet HP 可計算；FFLogs amount 排除 overkill 是公開 schema 語義 |
| Combined-HP consistency reference | 78,933,207 | inferred | `combined max HP - local overkill candidate`；不是已確認 FFLogs 公式 |
| Candidate total delta | 1,528,067 | inferred | `DACT - 78,933,207` |
| Numerical residual vs screenshot | -1,793 .. +8,207 | bounded/inferred | 前提是 combined-HP candidate 正確；絕對上限約 raid total 的 `0.0104%` |

數值殘差已經低於 `0.2%` 目標，但證據殘差並沒有消失：其中 `1,499,552`（DACT total 相對 combined max HP 的超額）仍無法逐條指認為 FFLogs exclusion。它可能涉及 shared-HP/equalization、target state 或 encounter-specific effective damage semantics。沒有 exact FFLogs events 時，不能把數值吻合升級為正式排除規則。

### Delta breakdown（不可混加）

| Category | Amount | Evidence status | 是否可作為 FFLogs delta 加項 |
|---|---:|---|---|
| Shared-HP / equalization consistency candidate | 1,499,552 | inferred | 是候選；缺 exact events，不能寫正式 rule |
| Final packet overkill candidate | 28,515 | observed local / official general semantic | 是候選；缺 exact reference event amount |
| Candidate explained total | 1,528,067 | inferred | 上兩項之和 |
| Unknown numerical residual | -1,793 .. +8,207 | bounded/inferred | screenshot rounding 下的剩餘區間 |
| Parser normalization | +251,705 | observed layer relationship | 否；它解釋 DACT total 如何形成，已經包含在 80,461,274 內 |
| Party-target raw periodic | 165,198 | observed raw / normalized unknown | 否；它是 parser-normalization 分解中的 raw bucket，不是已證明的 FFLogs exclusion |

如果只計算“已由 exact FFLogs events 證明”的 delta，目前仍不能消除 `1,526,274 .. 1,536,274`：截圖沒有逐事件證據。`<0.011%` 是強一致性假設下的 numerical residual，不是已確認 parity residual。

## Raw → normalized → ledger 守恆

| Layer / category | Damage | Events | Evidence status |
|---|---:|---:|---|
| Party-owned direct `21/22` to bosses | 76,449,628 | 2,053 | observed |
| Provisional party-source raw `24` to bosses | 3,594,743 | 243 positive | observed |
| Provisional party-source raw `24` to party targets | 165,198 | 8 positive / 13 rows | observed raw; normalized treatment unknown |
| Provisional party-owned raw total | 80,209,569 | 2,304 positive | observed |
| Normalized direct component | 76,449,628 | 2,053 | observed; saved total minus normalized periodic also matches every actor |
| Normalized periodic component | 4,011,646 | 627 hit rows | observed aggregate; historical MasterSwing rows were not persisted |
| Normalized / DACT total | 80,461,274 | 2,680 hits | observed |
| Parser normalization net | +251,705 | +376 hit rows | observed layer-total relationship |

Parser normalization 的可驗證拆分：

```text
normalized party-attributed periodic                 4,011,646
- provisional party-source raw boss periodic        3,594,743
= actor/target reassignment or simulation delta       416,903

actor/target reassignment or simulation delta          416,903
- party-target rows present in provisional raw total   165,198
= parser normalization net                             251,705
```

raw 中還有 22 個命中 Boss、source 為 `E0000000` 或另一個 Boss 的 periodic ticks，共 `420,381`。它們與 `+416,903` 高度一致，是 source reassignment/simulation candidate；但 compiled FFXIV_ACT_Plugin 沒有公開中間對映，歷史 encounter 又沒有儲存 normalized MasterSwing，所以不能用相差僅 `3,478` 的數值吻合強行建立一對一配對。

### Normalized periodic actor aggregates

這些值由 saved actor total 減去 owner-resolved raw direct total得到，actor aggregate 是 exact local；不是偽造的 normalized event rows。

| Actor | Raw direct | Normalized periodic | Periodic hit rows | Saved total |
|---|---:|---:|---:|---:|
| 西卡利亞 | 14,685,334 | 174,431 | 54 | 14,859,765 |
| 笙夜之歌 | 11,648,123 | 0 | 28 | 11,648,123 |
| 唐酥餅 | 11,373,833 | 0 | 0 | 11,373,833 |
| 伊斑斑 | 9,637,966 | 113,275 | 43 | 9,751,241 |
| 黛朵洛·彼繆 | 9,456,980 | 216,684 | 82 | 9,673,664 |
| 埃斯蒂尼安丿 | 8,107,014 | 172,138 | 73 | 8,279,152 |
| Migi | 6,106,139 | 1,700,465 | 163 | 7,806,604 |
| 闋寒貓 | 5,434,239 | 1,634,653 | 184 | 7,068,892 |
| Total | 76,449,628 | 4,011,646 | 627 | 80,461,274 |

raw Boss periodic source 與 normalized actor periodic 的 aggregate 差如下。這裡能確定 actor bucket 移動了多少，但不能確定是哪一條 raw tick 對映到哪一條 MasterSwing。

| Actor | Raw party-source Boss periodic | Normalized actor periodic | Delta |
|---|---:|---:|---:|
| 埃斯蒂尼安丿 | 1,041,826 | 172,138 | -869,688 |
| 伊斑斑 | 113,275 | 113,275 | 0 |
| 笙夜之歌 | 533,481 | 0 | -533,481 |
| 唐酥餅 | 0 | 0 | 0 |
| Migi | 594,856 | 1,700,465 | +1,105,609 |
| 黛朵洛·彼繆 | 548,318 | 216,684 | -331,634 |
| 西卡利亞 | 174,431 | 174,431 | 0 |
| 闋寒貓 | 588,556 | 1,634,653 | +1,046,097 |
| Total | 3,594,743 | 4,011,646 | +416,903 |

## Top parser-normalization raw evidence

下面是 22 個 raw-source 非 party、target 為 Boss 的 periodic events；總和 `420,381`。這些是 parser reassignment candidate，不是已確認 FFLogs exclusion。

| Timestamp | Raw source | Target | Status ID | Amount | Raw checksum |
|---|---|---|---|---:|---|
| 21:27:18.500 | E0000000 | 深藍 | 0 | 22,629 | `594a1515247d6426` |
| 21:27:21.483 | E0000000 | 深藍 | 0 | 13,226 | `b19fcbb92aa490b0` |
| 21:27:30.482 | E0000000 | 深藍 | 0 | 17,581 | `1d91042023319571` |
| 21:27:33.468 | E0000000 | 深藍 | 0 | 23,067 | `eaf45114d3eec3bc` |
| 21:29:45.464 | E0000000 | 深藍 | 0 | 23,505 | `7ad9f6f0706007f8` |
| 21:29:48.495 | E0000000 | 深藍 | 0 | 24,668 | `8112fb6382020ab3` |
| 21:29:51.479 | E0000000 | 深藍 | 0 | 18,061 | `da392b6b84be24ac` |
| 21:29:54.466 | E0000000 | 深藍 | 0 | 19,125 | `3bda45181815027e` |
| 21:29:57.493 | E0000000 | 深藍 | 0 | 23,912 | `ff2369b321318a32` |
| 21:30:00.478 | E0000000 | 深藍 | 0 | 18,289 | `123432fec679bc12` |
| 21:30:03.462 | E0000000 | 深藍 | 0 | 23,804 | `33f0de347e11fb02` |
| 21:30:06.492 | E0000000 | 深藍 | 0 | 8,274 | `a4bf7208ae6e580f` |
| 21:30:09.481 | E0000000 | 深藍 | 0 | 8,172 | `cd4997b76e042bb8` |
| 21:30:12.462 | E0000000 | 深藍 | 0 | 8,089 | `fd24b2da946ffc17` |
| 21:30:15.491 | E0000000 | 深藍 | 0 | 8,906 | `12586452514a03a7` |
| 21:30:18.473 | E0000000 | 深藍 | 0 | 8,173 | `950258c928a895fb` |
| 21:30:39.469 | E0000000 | 深藍 | 0 | 27,838 | `5fb4063c0ff2abfb` |
| 21:30:42.503 | E0000000 | 深藍 | 0 | 21,569 | `07102d56e6a9831b` |
| 21:31:39.489 | E0000000 | 熾紅 | 0 | 28,313 | `113dadde2d9bf8c0` |
| 21:31:39.489 | 熾紅 (`40018738`) | 深藍 | 0 | 26,902 | `b79759f667d631ce` |
| 21:31:42.476 | 深藍 (`40018739`) | 熾紅 | 0 | 23,907 | `ee4e1c3417d8dc53` |
| 21:31:42.476 | E0000000 | 深藍 | 0 | 22,371 | `8bcdeeac9fff2056` |

下面 8 個正值 party-target periodic events 總和 `165,198`。raw source 在不同隊員之間跳變，status ID 均為 `0`，所以 Phase 0 的 “friendly fire” 命名過強；Phase 1A 將其改為 `party-target periodic candidate`。沒有歷史 normalized target rows，最終 treatment 標為 unknown。

| Timestamp | Raw source | Target | Amount | Raw checksum |
|---|---|---|---:|---|
| 21:26:04.927 | 笙夜之歌 | 笙夜之歌 | 23,096 | `164c9f9cce9bbf22` |
| 21:26:07.914 | Migi | 笙夜之歌 | 22,250 | `60b9ea0ff7144bb2` |
| 21:26:16.911 | 笙夜之歌 | 笙夜之歌 | 21,934 | `5b80899046e2a6fb` |
| 21:28:41.473 | 唐酥餅 | 西卡利亞 | 5,754 | `1e209f233de2af3e` |
| 21:28:44.458 | 唐酥餅 | 西卡利亞 | 23,285 | `e65859c791a34819` |
| 21:28:50.477 | 唐酥餅 | 西卡利亞 | 21,821 | `64aac76c106ce75b` |
| 21:31:34.942 | 西卡利亞 | 伊斑斑 | 24,282 | `a8cf9431ac8974c7` |
| 21:31:37.929 | 唐酥餅 | 伊斑斑 | 22,776 | `a2469407df027ea4` |

## Pet / owner reconciliation

| Owner | Pet direct damage | Pet events | Summoned entity instances | Owner-resolved direct | Saved direct component |
|---|---:|---:|---:|---:|---:|
| 西卡利亞 | 3,575,994 | 59 | 23 | 14,685,334 | 14,685,334 |
| 伊斑斑 | 2,254,193 | 53 | 24 | 9,637,966 | 9,637,966 |

檢查結果：

- `raw checksum + effect index` duplicate groups: `0`
- `packet + target index + effect index + source + target + action` duplicate groups: `0`
- `timestamp + source + target + action + amount` duplicate groups: `0`
- owner attribution 前後 damage conservation: exact

結論是 observed：pet respawn/instance merge 存在，但沒有證據顯示 owner + pet 雙計。新 paired replay fixture 還會在 one raw → two identical normalized rows時保留 `ambiguous` 和 `duplicate normalization candidate`，不會靜默合併。

## Target scope

| Target | Party-owned direct | Provisional party-source periodic | Non-party raw periodic | Scope result |
|---|---:|---:|---:|---|
| 熾紅 (`40018738`) | 45,035,304 | 2,064,825 | 52,220 | encounter boss |
| 深藍 (`40018739`) | 31,414,324 | 1,529,918 | 368,161 | encounter boss |
| 笙夜之歌 | 0 | 67,280 | 0 | party-target periodic candidate; normalized treatment unknown |
| 西卡利亞 | 0 | 50,860 | 0 | party-target periodic candidate; normalized treatment unknown |
| 伊斑斑 | 0 | 47,058 | 0 | party-target periodic candidate; normalized treatment unknown |

沒有 party-owned direct event 命中 adds、trash、permanently retired target 或 non-fight target。歷史檔案沒有 normalized target split，因此 `4,011,646` periodic 在兩個 Boss 間的最終分配仍需 MasterSwing 或 FFLogs exact events。

## Top actor delta evidence

下面仍是截圖欄位推導的 actor delta interval；是 inferred，不是 exact event attribution。

| Actor | DACT total | DACT - FFLogs interval |
|---|---:|---:|
| 唐酥餅 | 11,373,833 | +473,153 .. +474,574 |
| 笙夜之歌 | 11,648,123 | +309,668 .. +311,145 |
| 闋寒貓 | 7,068,892 | +282,837 .. +283,735 |
| 埃斯蒂尼安丿 | 8,279,152 | +153,077 .. +154,145 |
| 西卡利亞 | 14,859,765 | +108,745 .. +110,656 |
| 黛朵洛·彼繆 | 9,673,664 | +106,243 .. +107,494 |
| 伊斑斑 | 9,751,241 | +64,743 .. +66,010 |
| Migi | 7,806,604 | +27,684 .. +28,709 |

Top 50 positive DACT-vs-reference events、Top abilities delta 和 exact target delta 無法對這場歷史 fight 生成：缺失 FFLogs exact event export，且 Phase 0 recorder 部署前的 normalized MasterSwing 沒有持久化。新 Phase 1A report 對未來/重新錄製的 paired fixture 已支援這些檢視；本報告沒有用弱時間/金額匹配偽造歷史 events。

## Phase 1A infrastructure

- canonical event 增加真實 `packetId`、`parserEventId`、raw checksum、target/effect index/type、overkill/absorbed 欄位。
- ACT `TimeSorter` 不再冒充 packet ID；只有 parser tag 真正提供 packet ID 時才寫入該欄位。
- correlation 狀態：`matched`、`ambiguous`、`intentionally ignored`、`unmatched raw`、`unmatched normalized`、`unmatched reference`。
- evidence 狀態：`official`、`observed`、`inferred`、`unknown`。
- 每條 reconciliation row 包含 timestamp、source/owner/target/ability ID 與名稱、raw/normalized/included/reference amount、category、reason 和三層 ID。
- report 輸出 Top 50 positive reference delta、unmatched normalized/raw，以及 actor/ability/target delta aggregates。
- `ParityReferenceFight` 支援 JSON/CSV 開發者匯入；正常執行不依賴 FFLogs API，也不會覆蓋本地結果。
- paired fixture 覆蓋 strict packet correlation、pet respawn、duplicate owner attribution candidate、friendly target、overkill、environment、multiple targets、trash+boss、DoT one-to-many、large packet、unmatched raw/normalized。
- replay 斷言 raw、normalized ledger 和 owner attribution 三組守恆。

## 仍缺失的證據

1. 同 fight 的 FFLogs exact damage event JSON/CSV；這是把 `1,499,552` shared-HP candidate 分配到具體 event/target/ability 的必要條件。
2. 歷史 normalized MasterSwing 明細；closed-source compiled parser 的 DoT source reassignment 無法從 raw `24` 唯一逆推。
3. FFLogs 對該 encounter 的公開 shared-HP/equalization rule。公開 schema 解釋 effective damage/overkill，但不證明此 encounter 的 target-state 篩選。

因此 Phase 1A 不新增任何正式 exclusion rule。Phase 1B 應先用 exact reference events 驗證 shared-HP/target-state 假設，再提出 EncounterTimeline/downtime/fight-boundary 的正式變更；本階段不進入 Phase 1B。

公開資料：[FFLogs CalculatedDamageEvent schema](https://www.fflogs.com/scripting-api-docs/ff/interfaces/RpgLogs.CalculatedDamageEvent.html)、[FFLogs expression language damage definitions](https://www.fflogs.com/help/pins)、[FFXIV_ACT_Plugin repository notice](https://github.com/ravahn/FFXIV_ACT_Plugin)。
