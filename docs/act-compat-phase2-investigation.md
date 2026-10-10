# ACT Compatibility Layer 第二階段調查報告

調查日期：2026-07-31

調查基線：

- DalamudActCompat：`main` 分支，開始調查時工作樹幹淨，線上基線 0.2.33。
- Triggernometry 中文維護版：2.1.1.2；上游原始碼 `MnFeN/Triggernometry`，調查提交 `2ada750fb78dbce27cb54e9a0b60e89c8aabf33c`。
- PostNamazu：1.3.6.6；上游原始碼 `Natsukage/PostNamazu`。
- IINACTEx：`Latihas/IINACTEx` 的 `cn` 分支，調查提交 `b0985055bfa1cc58f1e0d4d6d1eabd2d1c8e35b5`；Triggernometry 子模組 `ca19636f125de4a8141ad18f5fd295b1c37ef7c9`，PostNamazu 子模組 `4eedf8e512bf70cec65eb98d9275be8548991d05`。
- 執行證據：`2026-07-30 23:37:56.335`，Dalamud 記錄 `Long UiBuilder(Dalamud ACT Compat) detected, 11269.0566ms > 100ms`。

## 1. Triggernometry 管理員提示

管理員檢測位於：

`Source/Triggernometry/Core/RealPlugin/RealPlugin.Helper.cs` 的
`RealPlugin.CheckIfAdministrator(bool warnIfNotAdmin)`。

實現呼叫 `WindowsIdentity.GetCurrent()` 與
`WindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator)`。檢測結果還儲存在
`isRunningAsAdmin`，並被 `Core/Scripting/ScriptSecurity.cs` 使用：程序已提升時，
高風險指令碼 API 需要額外的 `AllowAdmin` 授權。把結果偽造成 `true` 不僅不真實，
還會破壞 Triggernometry 自己的指令碼安全模型。

提示源自傳統 ACT 部署假設，而不是 Triggernometry 基礎觸發功能的硬依賴。
傳統 ACT/FFXIV_ACT_Plugin 可能因網路抓包驅動、跨程序讀取遊戲、控制更高完整性
外部程序等原因遇到許可權限制。DalamudActCompat 已處於遊戲程序內，並透過
Dalamud/FFXIV_ACT_Plugin 橋提供日誌、網路等價事件、區域、戰鬥與實體資訊，
這些能力不需要提升 FFXIV。

普通許可權下可以支援：

- ACT 標準日誌和 FFXIV 網路等價日誌；
- 區域與戰鬥生命週期；
- 配置、正則、變數和本地觸發器；
- 透過宿主能力提供的 TTS、聲音與受控 Clipboard；
- 經使用者授權、帶超時的普通網路訪問和普通使用者目錄檔案訪問。

普通許可權下不能保證：

- 控制管理員身份執行的外部程式；
- 訪問受 ACL 或完整性級別保護的檔案、登錄檔和程序；
- 依賴管理員驅動或提升許可權抓包器的功能。

推薦的 Triggernometry Adapter 保留真實的非管理員結果，但把舊的籠統 Toast
替換為宿主能力說明：

> 當前以普通許可權執行。Dalamud 日誌、網路事件、區域、戰鬥和 TTS 橋可用；
> 需要跨完整性訪問受保護外部程序的動作不可用。請勿提升 FFXIV、Dalamud
> 或全部外掛的許可權。

## 2. Triggernometry 的 13 條 Error

實際 `Triggernometry.config.xml` 中恰好有 13 個啟用、啟動時更新的遠端倉庫。
它們都指向 `1824544011.v.123pan.cn`，所有 `UpdateLastChecked` 值集中在
`2026-07-30 23:37:58.075` 至 `.086`，而
`Config/TriggernometryRepoBackups` 不存在。調查時對 13 個地址併發執行 HEAD，
全部未在限定時間內返回。

`RealPlugin.UpdateRepositoriesAsync` 會併發呼叫所有倉庫的
`Repository.CheckAndUpdateAsync`。遠端後設資料失敗且本地備份不存在時，
`Repository.UpdateAsync` 再下載倉庫；每個下載超時分別寫入一條 Error。

| # | 倉庫 | 地址尾部 | 型別 | 執行緒 | 重複性 | 分類 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 問題自檢工具箱 | `SelfTest.xml` | `TimeoutException` | 執行緒池 | 每次啟動/更新週期 | 遠端配置源 |
| 2 | 執行支援庫 | `Utils.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 3 | 7.0 M1-4 | `S7a.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 4 | 7.2 M5-8 | `S7b.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 5 | 7.4 M9-12 | `S7c.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 6 | 7.X 極神 | `Ex7.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 7 | 臨時推送 | `temp.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 8 | 6.3 絕歐米茄 | `U6b.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 9 | 7.1 絕伊甸 | `U7a.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 10 | 7.5 絕妖星 | `U7b.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 11 | 特殊場景探索 | `field.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 12 | 深宮 | `dungeon.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |
| 13 | 異聞迷宮 | `vc.xml` | `TimeoutException` | 執行緒池 | 同上 | 遠端配置源 |

共同異常鏈：

`HttpClient.SendAsync` 的 `TaskCanceledException`
→ `HttpHelper` 轉換為 `TimeoutException`
→ `Repository.UpdateAsync`
→ `RealPlugin.FilteredAddToLog(Error, ...)`。

13 個任務併發，因此具體哪個倉庫先完成並不穩定。第一根異常是共同遠端請求
超時，13 條記錄只有一個獨立根因，不是 13 個 ACT API 缺口。

當前 Triggernometry 的 `Repository.UpdateAsync` catch 只把 `ex.Message` 交給
內部日誌，異常型別和完整堆疊已經在外掛內部丟失；DalamudActCompat 也沒有讀取
Triggernometry 內部日誌。因此舊執行無法恢復完整堆疊。相容層必須先增加結構化
診斷採集，並明確標註“外掛只提供訊息”的記錄，不能偽造堆疊。

## 3. Triggernometry 最小閉環差異

原外掛預期：

- `ActGlobals.oFormActMain` 是完整 ACT 主窗體和排程物件；
- `ActPlugins` 可發現 FFXIV_ACT_Plugin、OverlayPlugin 和其它外掛；
- 標準日誌、網路日誌、區域、戰鬥事件按 ACT 順序到達；
- WinForms 有真實 STA 訊息迴圈和 `ISynchronizeInvoke` 語義；
- 解除安裝時事件、動作、光環、端點和指令碼執行緒可結束。

當前實現：

- 已提供外掛發現、標準日誌、戰鬥事件和 FFXIV DataRepository/DataSubscription；
- 每個傳統外掛有獨立 STA WinForms 執行緒；
- `FormActMain.Invoke/BeginInvoke` 卻直接在呼叫者執行緒執行；
- Triggernometry 的事件和動作佇列無界；
- Triggernometry 解除安裝失敗後呼叫現代 .NET 不支援的 `Thread.Abort`；
- 尚無自動化的配置讀取、日誌、網路、區域、戰鬥、正則、TTS、重複開關 UI、
  資源釋放端到端驗證。

管理員提示和 13 個遠端倉庫 Error 不等於閉環失敗。閉環必須由可觀測探針逐項
驗證。

## 4. PostNamazu 分階段狀態

| 階段 | 當前狀態 | 證據與原因 |
| --- | --- | --- |
| 程式集載入 | 成功 | UI 已能開啟 |
| `InitPlugin` 呼叫 | 部分成功 | UI、程序管理器、動作模組已建立；現代 .NET/強名稱 GreyMagic 載入點可能中斷尾部生命週期 |
| ACT Host | 成功 | `ActGlobals.oFormActMain` 存在 |
| FFXIV_ACT_Plugin 發現 | 成功 | Native Bridge 能透過 ProcessManager 取得 FFXIV 程序 |
| OverlayPlugin 發現 | 成功 | 執行日誌顯示共享事件處理器已註冊 |
| Triggernometry 發現 | 未驗證 | 型別和外掛列表入口存在，但沒有成功回撥計數 |
| 遊戲程序識別 | 成功 | Native Bridge 啟動依賴此步驟，執行日誌已越過 |
| 全域性簽名掃描 | 成功 | `GetOffsets()` 返回 true |
| 模組簽名掃描 | 表面成功 | 狀態文案沒有 unavailable module；未做真實動作測試 |
| 命令橋初始化 | 表面成功 | IL 已重寫到 Native Bridge；橋仍允許任意函式地址/記憶體訪問 |
| 日誌系統 | 部分成功 | ListBox 和 Dalamud 日誌入口存在；沒有結構化、限流和階段狀態 |
| 文字命令測試 | 未實現 | “native bridge active” 不包含端到端命令確認 |
| 標點/場地標點/預設 | 未驗證 | 仍依賴通用 `calli/read/write` 等價假設 |
| 解除安裝測試 | 未實現 | WinForms Join 超時後可能遺留執行緒 |

`FFXIV entrance function found(7.3_raw)` 只表示舊 GreyMagic 入口簽名被找到，
不表示命令傳送、模組呼叫或 IPC 已連通。

## 5. PostNamazu 複製日誌卡死

準確呼叫鏈：

`CmdCopyProblematic_Click`
→ `CopyLog(true)`
→ 列舉 `lstMessages.Items`
→ `StringBuilder.AppendLine`
→ 被 IL 改寫的 `LegacyResourceCompatibility.SetClipboardText`
→ `RetryClipboard`
→ `Clipboard.SetText`。

呼叫發生在 PostNamazu 的 STA UI 執行緒，執行緒 apartment 本身正確。不存在非同步
任務同步等待、Dispatcher、IPC 或外掛呼叫。發生 `ExternalException` 時，
當前相容補丁在 UI 執行緒 `Thread.Sleep(20 * (attempt + 1))`，前九次累計阻塞
900ms，第十次仍失敗則重拋。日誌遍歷和字串拼接也在 UI 執行緒，日誌量大時會
進一步阻塞。

修復應快速快照 UI 專案，把字串整理放到後臺，再交給專用 STA Clipboard
服務；佇列有界、請求可取消、整體有 deadline，失敗後返回結構化狀態。不能在
UI 執行緒重試或僅在外圍加 try/catch。

## 6. 當前執行緒與死鎖面

- Dalamud Framework Update：玩家/隊伍/戰鬥狀態採集、遭遇快照、當前所有
  PostNamazu 原生呼叫。
- PluginLifecycle 執行緒池任務：同步執行 `StartParser`、Overlay 和傳統外掛載入。
- 每外掛 STA 執行緒：WinForms 配置 UI、`InitPlugin`、訊息迴圈、`DeInitPlugin`。
- NotACT：日誌讀、日誌寫、AfterCombatAction 等執行緒，部分使用無界
  `ConcurrentQueue`。
- Triggernometry：無界 `Queue<LogEvent>`、動作執行緒、端點、指令碼、光環和繪製
  執行緒。
- PostNamazu：原版程序監控、HTTP 與 GreyMagic 排程執行緒；部分已被補丁跳過。

高風險同步點：

- `NativePostNamazuBridge.RunOnFrameworkThread(...).GetAwaiter().GetResult()`；
- Framework Update 中執行任意地址 `calli/read/write`；
- `LoadedActPlugin.Load` 等待外掛初始化最多 30 秒；
- 解除安裝等待外掛 STA 執行緒 10 秒，超時後繼續並留下執行緒；
- Triggernometry 執行緒 Join 失敗後呼叫 `Thread.Abort`；
- NotACT 的假 `Invoke/BeginInvoke` 不執行真實執行緒切換；
- 無界事件/日誌佇列；
- 外掛構造和 Dispose 中的同步非同步等待。

## 7. 隔離方案評估

### 程序內執行緒隔離

能解決：

- 正常慢回撥不再直接佔用 Framework Update；
- 透過有界佇列限制積壓和記憶體；
- 對仍能返回的超時、異常做計數、暫停和熔斷；
- 保持 UI 和事件入口可操作。

不能解決：

- 無法安全強制停止失控託管執行緒；
- 死迴圈、全域性鎖、棧溢位、執行緒池耗盡仍共享程序；
- 原生訪問衝突和 DLL 崩潰直接終止 FFXIV；
- 嚴重記憶體增長仍影響遊戲。

### 獨立輔助程序

Host 程序負責 ACT API 模擬、WinForms、Triggernometry、PostNamazu 非遊戲內
部分、日誌/觸發計算和外掛生命週期。FFXIV 程序只負責採集、狀態 UI、監督、
IPC 和嚴格驗證的最小命令 Broker。

Host 死迴圈、死鎖、原生崩潰或記憶體失控時，遊戲側可結束並重啟 Host。單個
外掛卡死無法在同一 Host 內安全解除安裝；看門狗要記錄最後執行外掛，重啟 Host 後
隔離該外掛。未來可按相容組拆分 Worker 程序。

### 推薦

採用混合架構，並分階段遷移。執行緒隔離是遷移期減災，不作為最終安全邊界。

## 8. IPC 協議草案

傳輸：當前使用者 ACL 限制的雙向命名管道；長度字首幀，不使用無限 JSON 行流。

公共幀頭：

- `ProtocolVersion`
- `SessionId`
- `Sequence`
- `MessageType`
- `Priority`
- `CorrelationId`
- `TimestampUtc`
- `DeadlineUtc`
- `PayloadLength`

遊戲到 Host：

- `Hello`
- `LogLine`
- `NetworkLogLine`
- `ZoneChanged`
- `CombatStarted`
- `CombatEnded`
- `StateSnapshot`
- `Shutdown`

Host 到遊戲：

- `HelloAck`
- `Heartbeat`
- `Ack`
- `TtsRequest`
- `PostNamazuCommand`
- `PermissionRequest`
- `HealthReport`

未知版本、未知訊息、過期 deadline、越界長度和未授權命令直接拒絕。禁止傳輸
函式地址、記憶體地址、任意程式碼或 PowerShell。

## 9. 佇列與背壓

- 控制佇列容量 256：區域、戰鬥開始/結束、關閉、許可權響應；必須保序、ACK、
  有限重試。
- 資料佇列容量 8192：標準日誌和網路日誌；保序，滿時丟棄最舊低優先順序項，
  記錄連續區間和丟棄計數，不允許無限積壓。
- 狀態槽：玩家、隊伍、戰鬥狀態按 key 只保留最新值。
- 每批最多 128 條或 64 KiB，最長聚合 20ms。
- 關鍵控制事件使用獨立容量，不能被戰鬥日誌擠佔。
- 斷線期間只保留有限關鍵重放視窗，不能宣稱絕對可靠。

## 10. 看門狗與熔斷

- Host 心跳：1 秒；
- 3 秒無心跳：Suspect，停止傳送低優先順序資料；
- 5 秒無心跳：結束 Host，遊戲保持執行；
- 重啟退避：1、2、5、15、60 秒；
- 10 分鐘內三次失敗：進入手動恢復；
- 單外掛回撥超過 100ms：警告；
- 連續三次超過 2 秒：暫停新事件並標記熔斷候選；
- 佇列 75%：警告；90%：丟棄低優先順序舊事件；
- 監測異常/超時次數、IPC 延遲、記憶體及增長率、執行緒數量、UI 無響應和日誌速率；
- 日誌按錯誤指紋限流並保留累計次數。

分級處置：

1. 記錄一次結構化警告；
2. 暫停對應外掛的新事件；
3. 丟棄其低優先順序積壓；
4. 熔斷並持久化隔離原因；
5. 嘗試正常解除安裝；
6. 無響應時重啟 Host；
7. FFXIV 和 Dalamud 不退出。

## 11. 許可權邊界

許可權類別：

- 只讀日誌；
- 本地配置讀取；
- TTS；
- Clipboard；
- 網路請求；
- 啟動外部程式；
- 檔案寫入；
- 遊戲內語義命令；
- 高風險指令碼。

只讀日誌和外掛自身配置目錄讀取可預設允許。TTS/Clipboard 需可見狀態。
網路、外部程式、任意路徑寫入、遊戲命令和指令碼預設拒絕或首次明確授權，按外掛
和動作類別持久化，可撤銷並寫審計日誌。

同一完全信任 .NET Host 內的外掛仍能繞過宿主 API 直接呼叫檔案、網路和
`Process.Start`。Broker 是必要邊界但不是完整 OS 沙箱；強制執行需要受限令牌、
AppContainer 或獨立 Worker。任何外掛都不得自動提升許可權。

特別風險：當前 Triggernometry 的 13 個遠端倉庫全部具有
`AllowScriptExecution=true` 和 `AllowProcessLaunch=true`。遷移到新 Host 時
不得靜默繼承為宿主授權。

## 12. IINACTEx 對照結論

IINACTEx 透過專案引用直接編譯大幅修改的 Triggernometry/PostNamazu fork，
把它們深度繫結到 Dalamud 服務和 ImGui；它還在多處使用
`RunOnTick(...).Wait()`/`.Result`，並直接關閉管理員 Toast。該實現說明了外掛
需要哪些資料和發現入口，但不滿足本專案的獨立程序、安全命令、上游相容和
故障隔離目標。

可借鑑的是：

- 明確維護 FFXIV_ACT_Plugin、OverlayPlugin、Triggernometry、PostNamazu 的
  外掛發現順序；
- 為 Triggernometry 提供遊戲實體、區域、指令碼環境等明確能力；
- 為 PostNamazu 提供語義化的宿主入口。

不可照搬的是：

- 把第三方原始碼整體編入 Dalamud 外掛；
- 註釋掉管理員提示；
- 在 Framework Update 同步執行動作；
- 繼續維護大規模外掛 fork；
- 用任意記憶體/原生函式作為橋接協議。

## 13. 遷移與測試順序

1. 落地結構化診斷和階段狀態；
2. 修復 Clipboard 非阻塞路徑；
3. 建立有界佇列、統計與遷移期熔斷；
4. 定義共享 IPC 協議和 HostSupervisor；
5. 把 Triggernometry 移入 Host，驗證初始化、重複 UI、配置、標準/網路日誌、
   區域、戰鬥、正則、文字/TTS 和解除安裝；
6. 把 PostNamazu UI/計算移入 Host，遊戲側只保留逐動作白名單 Broker；
7. 實現命令測試和解除安裝測試後才報告 Ready；
8. 預設關閉舊程序內傳統外掛路徑；
9. 執行死迴圈、30 秒阻塞、IPC 永不返回、十萬行復制、Host 崩潰/強殺、
   斷管、佇列滿、持續異常、記憶體增長、退出清理、重連和熔斷測試。

驗收硬條件：任何 Host 或傳統 ACT 外掛故障都不能讓 Framework Update、
Dalamud UI 或 FFXIV 發生永久同步等待。

## 14. 實施結果（2026-07-31）

調查後的實現採用上述混合架構，詳見
[`act-host-isolation.md`](act-host-isolation.md)。傳統 ACT 外掛已從 FFXIV 程序移入
獨立 Host；IPC v2、有界佇列、心跳/處理進度看門狗、自動重啟/熔斷、許可權快照、
語義命令 Broker、PostNamazu 階段狀態和結構化異常記錄已經落地。

真實 Triggernometry 離線閉環首次執行時沒有觸發任何規則。第一根相容層異常不是
正則或 TTS，而是 `FormActMain.CurrentZone` 初始為 `null`：Triggernometry 收到第一條
日誌後執行 `ZoneChanged(null)`，而 `PassesZoneRestriction(null)` 對所有觸發器返回
`false`，導致所有規則被標記為 `ZoneBlocked`。將 ACT Host 的初始區域改為真實的
“未知但非 null”空字串後，標準日誌和網路等價事件閉環透過。

繼續驗證戰鬥事件時，`OnCombatStart` 已到達但 `OnCombatEnd` 未到達。根因是 NotACT
`SetEncounter` 建立 encounter 後沒有設定 `Active`/`StartTimes`，`EndCombat` 因此
提前返回。這是通用 ACT 生命週期缺口；修復狀態機後，戰鬥開始和結束均透過真實
Triggernometry ACT-source 觸發器驗證。

區域事件另有一個明確差異：Triggernometry 原版透過
`FFXIV_ACT_Plugin.DataSubscription.ZoneChanged` 反射訂閱，而外部 Host 僅提供 event-only
facade，不初始化原版抓包/記憶體 IOC。專屬 Adapter 現在註冊/登出區域監聽，傳遞真實
territory ID 與 zone name；帶區域限制的真實觸發器已驗證透過。該 Adapter 不提供
程序、記憶體或抓包假物件。

離線自動化當前已透過配置讀取、標準日誌、網路等價事件、區域限制、戰鬥開始、
戰鬥結束、正則、TTS command request、正常解除安裝，以及 Host kill/斷管/30 秒阻塞、
有界佇列和回撥熔斷測試。遊戲未啟動不影響這些合成事件測試；實際 TTS 音訊、
PostNamazu 遊戲命令和 Dalamud UI/退出行為仍明確標為待遊戲內驗收，不能報告為成功。
