# FFLogs rDPS Parity Harness

該工具從 FFLogs v2 public API 抓取真實 DNC 擊殺樣本，並透過 production
`RaidDpsEstimator` 重放。它的職責是暴露差異，不會修改公式或用最終 rDPS
反推 adaptor 補償。

```text
FFLogs API events
        ↓
FflogsEventNormalizer
        ↓
RaidDpsEstimator (production)
        ↓
ParityReportWriter
```

## 資料口徑

- `characterRankings` 僅用於發現公開 report/fight，不作為 parity ground truth。
- FFLogs `DamageDone` table 的 `totalRDPS`、`totalRDPSTaken`、
  `totalRDPSGiven`、`totalADPS`、`totalNDPS` 是參考總量。
- FFLogs 與 DACT 都使用 `DamageDone.totalTime` 作為報告中的共同分母。
- downtime 分類使用同一張表的 `damageDowntime`，不從最終 rDPS 反推。
- `damage.amount` 提供有效傷害；相同 packet ID 的 `calculateddamage.timestamp`
  提供 DACT action packet 時間。DoT tick 沒有對應 calculateddamage 是預期行為。
- FFLogs status ID 的一百萬名稱空間只做機械還原；Technical Finish 的 legacy
  action ID 只做當前網路 action ID 對映。Normalization 不讀取或補償最終 rDPS。

FFLogs 官方數學參考：<https://www.fflogs.com/help/rdps>。

## 使用

憑據優先讀取 `FFLOGS_CLIENT_ID` / `FFLOGS_CLIENT_SECRET`，否則讀取 DACT
配置中的 FFLogs client。憑據和 token 不會寫入快取。

```powershell
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --samples 100
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --samples 100 --collect-only
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --samples 100 --replay-only
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --self-test
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --devilment-probe
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --guaranteed-hit-experiment
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --attribution-matrix
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --percentage-audit
dotnet run --project tools/DalamudActCompat.FflogsParityHarness -c Release -- --percentage-ordering-only
```

`--percentage-ordering-only` is cache-only. It reads production per-event component counters,
compares parameter-free percentage/rate ordering counterfactuals, and emits packet-sequence,
expiry, Technical eligibility, direct/periodic, and conservation reports. It never modifies
the production attribution equation.

The same mode also emits the strict direct-normal core, explicit interaction decomposition,
provider-level identifiability, cross-provider validation, and live-causal status fallback
comparison in `percentage-identification-*` and `percentage-status-*` reports.

`--percentage-audit` 只讀取現有 106 場 cache，逐 provider/recipient 對賬 fixed-magnitude
percentage contribution，並輸出 reference direction、single/overlap、event-state、DoT snapshot
和 provider 型別診斷。FFLogs public API 不提供 per-event/per-window contribution；報告不會從整場
`given[]` 反推不存在的逐事件 truth。

`--guaranteed-hit-experiment` 只讀取現有 100 場快取。它從 61 場 SAM partner 中選取
30 場 guaranteed damage 最高的高資訊量樣本，並把 production 實測的 regular-action
Devilment contribution 保持不變，只離線替換 guaranteed-event candidate contribution。
比較基準只能是 FFLogs 整場 `Devilment given.total`；工具不會偽造或反推 per-action truth，
也不會寫回 production `TransferGuaranteedCriticalDirectContribution`。

`--devilment-probe` 只讀取現有 manifest、原始 event cache 和第一輪
`parity-report.json`。它固定選擇 6 個 SAM 與 5 個 DRG 樣本，輸出逐 action、逐 window、
guaranteed coverage、component parity 和 Cu/Du sensitivity 檔案。FFLogs API 未提供的
per-action/per-window contribution 保持為 `unavailable_api`；diagnostic scenario 不會寫回
normalization 或 production `RaidDpsEstimator`。

原始 GraphQL JSON 預設快取在
`artifacts/fflogs-parity-harness/cache`。JSON、CSV 和 Markdown 報告預設寫到
`artifacts/fflogs-parity-harness/reports`。整個 `artifacts` 目錄已被 Git 忽略，
避擴音交大體積戰鬥資料和公開玩家名稱。

採集器會快取每個原始響應、分頁拉取 events、原子寫快取、保留 rate-limit
餘量並對瞬時 HTTP/GraphQL 失敗重試。`--refresh` 會忽略現有響應快取。

## 當前邊界

- 第一輪範圍固定為 CN、AAC Heavyweight、difficulty 101、partition 9，按四個
  encounter 分層抽樣。
- API events 沒有 DACT 的完整 NameToggle/targetability 原始包；目標可選時長
  parity 單獨記錄，不在 adaptor 中偽造。
- production estimator 目前按來源聚合 DNC percentage contribution，因此只能
  直接對比 Technical + Standard 合計；FFLogs 的兩個分項仍單獨保留在報告中。
- DNC 尚未達到 production parity，不能標記 frozen。

## 第一輪公式審計

與 FFLogs 公開說明逐項對照後：

- production 的 percentage multiplier 乘法疊加、lost damage 和 log-weighted
  分攤結構與公開公式一致。
- regular hit 與 simulated DoT 的 Crit/DH 分攤結構也與公開公式一致；self
  contribution 會排除，pet damage 會回到 owner。
- 尚未 parity 的首要數學輸入差異是 `C_u` / `D_u`：FFLogs 公式要求玩家真實的
  unbuffed Crit/DH chance，DACT 當前只能用 `25%`、40-hit prior 和觀測命中估計，
  再限制到 5%–50%。這不是 FFLogs 公開公式的一部分。
- guaranteed Crit/DH 的 production action 表和 rate-buff scaling 是額外實現層；
  第一輪樣本中 SAM partner 的 Devilment 負差顯著，必須優先逐 action 對照，不能
  用最終差值調整 multiplier。
- buff packet 時序、死亡清狀態、DoT application/refresh snapshot 和 pet/owner
  狀態繼承仍屬於 event/state parity，不能由最終 rDPS 接近來判定正確。

本輪只記錄這些差異，沒有修改 production `RaidDpsEstimator`。
