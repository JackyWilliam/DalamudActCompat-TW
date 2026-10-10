# 獨立 ACT Host：實現與安全邊界

更新日期：2026-07-31

## 結論

第二階段採用混合架構。Triggernometry、PostNamazu、FoxTTS 和基於 manifest
發現的傳統 ACT 外掛由 `DalamudActCompat.Host.exe` 載入；FFXIV 程序只保留
Dalamud 資料採集、官方解析/Overlay 核心、ImGui 狀態介面、IPC 客戶端和經過
驗證的語義命令 Broker。

這解決的是現實中的故障域問題：傳統外掛死迴圈、WinForms 死鎖、阻塞呼叫、
未處理託管異常、Host 原生崩潰和 Host 記憶體失控不再與 FFXIV 共用程序。單純把
外掛換到另一個執行緒不能提供這些保證，因為 .NET 不能安全終止任意失控執行緒，
`Thread.Abort` 也不能恢復被破壞的鎖/原生狀態。

## 程序與執行緒模型

```text
FFXIV / Dalamud
├─ 遊戲資料與官方解析/Overlay 核心
├─ ImGui 狀態/許可權介面
├─ ActHostSupervisor + HostIpcClient
└─ 白名單語義命令 Broker
          │ current-user-only named pipes
          ▼
DalamudActCompat.Host.exe
├─ ACT/NotACT STA 訊息迴圈
├─ 每個傳統外掛的 STA WinForms 訊息迴圈
├─ Triggernometry / PostNamazu / FoxTTS
├─ 有界 Clipboard STA 工作執行緒
└─ IPC reader / writer / heartbeat
```

遊戲 Framework Update 只發布非阻塞事件，不呼叫傳統外掛。日誌先進入有界
遊戲側佇列，再按最多 128 條或 64 KiB 批次傳送。Host reader 呼叫 ACT 回撥；
如果回撥卡住，獨立 heartbeat 仍會報告“已接收序列未前進”，遊戲側看門狗在
5 秒後判定 Host 處理停滯並終止/重啟 Host。

## IPC 協議 v2

傳輸使用兩條僅當前使用者可訪問的命名管道，分別承載 game-to-host 與
host-to-game。幀為 4 位元組長度字首加 JSON，單幀最大 1 MiB。公共信封包含：

- 協議版本、隨機 session ID、單調 sequence；
- 型別、優先順序、建立時間和可選 deadline；
- correlation ID 與型別化 payload。

遊戲到 Host 支援握手、許可權快照、日誌批、區域、戰鬥開始/結束、開啟外掛 UI、
關閉和僅測試構建啟用的故障注入。Host 到遊戲支援握手、心跳、健康/階段狀態、
結構化診斷、命令請求、命令結果和關閉確認。未知 session、倒退 sequence、過期
deadline、超長幀和未知命令均拒絕。

協議不傳遞函式地址、記憶體地址、任意程式集、任意命令列、PowerShell 或指令碼。

## 佇列與背壓

| 佇列 | 容量 | 滿載策略 | 保證 |
| --- | ---: | --- | --- |
| IPC 控制/關鍵 | 256 | 拒絕新項並記錄；關鍵區域/戰鬥狀態保留最新副本供重連重放 | 單連線內保序，不聲稱斷電式持久可靠 |
| IPC 資料 | 8192 | 丟棄最舊低優先順序項，累計 drop 計數 | 保留剩餘日誌順序 |
| 遊戲側待傳送日誌 | 8192 | 丟棄最舊日誌 | 每批 128 條/64 KiB，20 ms flush |
| NotACT 日誌 | 8192 | 丟棄最舊並按 2 的冪次限流告警 | 有界記憶體 |
| NotACT combat action | 4096 | 丟棄最舊並計數 | 有界記憶體 |
| Triggernometry event | 8192 | 丟棄最舊並限流告警 | 透過 Adapter 修補上游無界佇列 |
| Clipboard | 8 | 拒絕新請求 | 最多 200,000 項、8 MiB 文字 |
| 遊戲側命令 Broker | 64 | 拒絕並返回 `busy` | 單消費者，禁止按請求無限 `Task.Run` |

狀態型訊息只保留最新值。控制事件不與高頻日誌共用資料容量；斷線後只重放最新
區域和戰鬥狀態，舊日誌不無限快取。

## 看門狗與熔斷

- heartbeat 每秒一次；3 秒無心跳進入 suspect，5 秒無心跳判死；
- heartbeat 存活但 Host 的 last-received sequence 5 秒不前進，同樣判死；
- working set 超過 1.5 GiB 或執行緒超過 256，判為 Host 故障；
- 自動重啟使用 1、2、5 秒退避；10 分鐘內超過三次失敗開啟總熔斷，等待手動恢復；
- 手動停止會取消已計劃的自動重啟，手動重啟會清除失敗歷史；
- ACT 回撥記錄完成次數、異常、慢呼叫、當前執行項和持續時間；連續 5 次異常或
  連續 3 次超過 2 秒時開啟回撥熔斷；
- 診斷按錯誤指紋保留累計次數，只發前 3 次和後續 2 的冪次，防止刷屏；
- Host 終止隻影響相容 Host，FFXIV 與 Dalamud 不退出。

程序內回撥熔斷只能阻止“下一次”呼叫，不能搶佔已經死迴圈的執行緒；搶佔由遊戲側
終止整個 Host 程序完成。若將來要求某一個惡意外掛不能拖死同 Host 的其它外掛，
必須繼續拆為每外掛/信任域一個 Worker 程序。

## 許可權與命令邊界

許可權按外掛和能力儲存，可在設定中撤銷：日誌讀取、本地配置、TTS、Clipboard、
網路、啟動外部程式、檔案寫入、遊戲命令、原生遊戲記憶體和高風險指令碼。預設只允許
日誌、本地配置、TTS 與 Clipboard；網路、程序、寫入、遊戲命令、原生記憶體和指令碼
預設拒絕。授權判斷和允許/拒絕的 Broker 呼叫都會寫審計日誌。

Triggernometry 的 TTS 請求先由遊戲側按外掛身份、能力和 2 秒 deadline 授權，實際
FoxTTS 回撥只在獨立 Host 內執行。最多保留 64 個待授權請求；無輸出提供者、過期、
拒絕或佇列滿都會顯式失敗。FoxTTS 卡死會阻塞 Host 的處理進度並觸發程序級恢復，
不會把第三方音訊回撥帶回 FFXIV 程序。

當前遊戲側只接受：

- `triggernometry` 的有界文字 `tts`；
- `postnamazu` 的單條、長度受限、無換行、斜槓開頭的語義聊天命令。

PostNamazu 的任意地址 read/write/`calli` 和舊 GreyMagic 入口在外部 Host 中明確
丟擲“不支援”，不會降級為假成功。即便使用者授權遊戲命令，引數仍需再次驗證，並
有 2 秒 deadline/cancellation；超時後的 Framework 回撥會再次檢查取消狀態，避免
遲到副作用。

限制：同一 Host 內仍是完全信任的 .NET 程式碼。惡意外掛可以繞過託管 Broker 直接
訪問桌面 API，也可能偽造另一個外掛的邏輯身份。嚴格來源隔離需要獨立受限 Worker、
受限令牌或 AppContainer；現階段 UI 明確顯示這一點，不把能力開關宣傳為 OS 沙箱。

## Triggernometry Adapter

- 管理員檢測保留真實結果；普通許可權時改為說明可用的日誌、區域、戰鬥、正則、
  配置、Clipboard 和 Broker TTS 能力，不偽造管理員身份；
- 高風險指令碼、網路倉庫更新與外部程序啟動分別經過許可權檢查；
- `Thread.Abort` 被替換為結構化“執行緒未停止”報告，由 supervisor 結束 Host；
- 上游兩個無界事件 enqueue 改為 8192 項 drop-oldest；
- 原版 `DataSubscription.ZoneChanged` 在 event-only facade 中不存在，專屬 Adapter
  註冊/登出區域監聽，傳遞真實 territory ID/zone name 並更新區域限制；
- 內部 Error 日誌每秒取樣，按 timestamp/message 去重並輸出結構化診斷。上游只保留
  message 時，stack 明確標註“上游未儲存”，不偽造堆疊。

## PostNamazu Adapter 與階段狀態

| 階段 | 當前狀態 | 說明 |
| --- | --- | --- |
| 程式集載入 | 成功/失敗實時顯示 | 驗證真實 DLL 路徑 |
| `InitPlugin` | 成功 | 外部 STA WinForms Host 已載入真實外掛 |
| ACT Host / Host IPC | 成功 | NotACT 與 v2 命名管道均已初始化 |
| FFXIV_ACT_Plugin 發現 | 成功 | 提供官方型別身份的 event-only facade |
| Triggernometry 發現 | 成功或失敗實時顯示 | 在 PostNamazu 初始化前加入 ACT 外掛列表 |
| OverlayPlugin 發現 | 未實現 | Overlay 留在遊戲側，不把完整 DLL 暴露給不可信 Host |
| 遊戲程序識別 | 未實現（安全限制） | 不暴露 raw `Process`、記憶體或注入入口 |
| 命令橋 | 成功 | 只提供白名單語義 Broker；舊 native 呼叫明確失敗 |
| 日誌系統 | 成功 | 走有界 ACT 日誌批 |
| 命令傳送測試 | 未測試 | 必須在遊戲執行且使用者明確授權後逐動作驗證 |
| 解除安裝測試 | Host 退出時記錄 | `DeInitPlugin` 有界等待；卡住則終止 Host |

“複製日誌”的舊阻塞點是 PostNamazu UI 執行緒上的日誌遍歷、字串拼接和最多 10 次
Clipboard 重試。Adapter 現在只在 UI 執行緒快照最多 200,000 項；字串構建與最多
5 次、總等待有界的 Clipboard 操作在容量 8 的專用 STA 佇列執行。失敗不會永久鎖住
PostNamazu UI，更不會阻塞 FFXIV。

## 已自動驗證

- Host 握手、版本/session/sequence、任意命令拒絕和正常關閉；
- Host 直接 kill、客戶端斷管、Host reader 阻塞 30 秒時遊戲側測試繼續前進；
- 控制佇列溢位拒絕、資料佇列 drop-oldest、狀態壓縮；
- NotACT 日誌/combat action 佇列上限；
- 連續異常開啟單回撥熔斷；
- 真實 Triggernometry DLL 初始化與配置讀取；
- 標準日誌與 FFXIV 網路等價事件分別正則命中，完成遊戲側 TTS 授權並排程到
  Host 測試輸出提供者；
- 區域變化更新區域限制後命中；
- 戰鬥開始/結束事件分別命中；
- Triggernometry、PostNamazu、FoxTTS 外部載入和 Host 正常退出。

仍需遊戲執行後驗證：重複開啟/關閉真實 UI、實際 TTS 音訊、PostNamazu 每一條允許
命令、Host 被工作管理員結束後的 UI 恢復、退出遊戲後的孤兒程序清理，以及真實戰鬥
高峰下的積壓/記憶體曲線。
