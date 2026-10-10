# 絕伊甸、絕妖星繪圖診斷

Host 對 Triggernometry 2.2 的原始執行流程增加被動觀察，不修改訂閱 XML、開關、
變數、條件或動作順序，也不增加等待或重試。原有 U7b 初始化時序補丁保留。

啟動時 `DACT_DRAW_DIAG_READY` 表示這些觀察器已裝入。詳細記錄以
`DACT_DRAW_DIAG ` 加單行 JSON 輸出，由現有 Host 日誌通道寫入 `dalamud.log`。
僅在地圖 1238 / 1363 且有 ReadCombatLogs 許可權時收集。節點按祖先名稱
U7a / U7b 識別；重新命名了整組字首的自定義版本不在本輪節點觀察範圍內。
舊 TN ABI 保持原流程，啟動記錄 `DACT_DRAW_DIAG_UNAVAILABLE`。

| stage | 可以確認的事情 |
|---|---|
| received / queued | Host 收到的原始日誌、進入 TN 佇列的日誌；透過時間與 actor Key 關聯轉換前後 |
| inventory / node-config | TN/Host 程式集版本、註冊回撥數、相關節點是否啟用、父級是否啟用、來源與訂閱許可權限制 |
| match | 原正則確實匹配，並附當時的重觸發冷卻狀態 |
| folder-filter | 原目錄過濾函式的結果，沒有額外重新計算條件 |
| condition-blocked / fire-return | 原節點條件是否阻止執行，以及動作是否接受排隊/延後執行 |
| action-enter / action-return | 原動作進入和返回、動作序號、原動作結果與前後關鍵變數 |
| callback-enter / callback-return / callback-throw | 實際回撥名、註冊數、解析後的數值引數、返回或異常 |
| entity-change | 最近相關 actor 從缺失到就緒、地址/BNpcID 變化或消失 |
| exception / observer-error | 原節點/動作異常；診斷觀察器自身異常（不會阻斷原動作） |

`event.Key` 用原日誌時間和 actor 關聯；`event.DigestValue` 區分同一時間的不同事件。
每次上下文有 `contextId`，每次呼叫有 `callId`，可串聯延遲動作。
狀態包括 Host 實體快照的存在性、地址、BNpcID、快照年齡，遊戲程序是否已登記，
原生/命令許可權，以及 U7a 光輪、U7b P1 冰火散攤、myIdx 等限定變數和光輪開關。
快照狀態屬於 Host 已收到的資料，不等於額外讀取了遊戲記憶體；不主動初始化鯰魚精，
也不額外掃描記憶體。連線或掃描失敗仍結合原鯰魚精啟動日誌及回撥異常判斷。

`registered=0` 表示未註冊的靜默呼叫；`-1` 表示暫時無法讀取登錄檔。
`callback-return` 只表示處理函式返回，不能證明遊戲已渲染；動作返回也不能覆蓋同一
上下文的異常記錄。`acceptedOrDeferred=true` 可包含等待互斥鎖，不表示繪圖已經完成。
變數 null 表示該項缺失；`unavailable-or-busy` 表示無法在不阻塞原執行緒的前提下采樣。

每分鐘分別限額：120 條接收/入隊事件、600 條節點/回撥/狀態記錄、60 條錯誤記錄。
原始事件洪流不會耗盡節點或錯誤預算。下一視窗首條記錄攜帶
`suppressedSincePreviousWindow`；預算耗盡期間不能用“沒有日誌”證明“沒有觸發”。
節點清單最多每分鐘一次、最多 48 個相關節點；實體跟蹤最多 64 個、保留 30 秒。
NPC 生成/讀條過濾玩家施法噪聲，仍保留玩家點名和狀態事件。無完整聊天、原始日誌文字
或任意繪圖引數轉儲；原生地址/數值 CSV 記錄原值，其他繪圖引數只記長度與摘要。

## 取證方式

1. 使用診斷版啟動遊戲，檢查 `DACT_DRAW_DIAG_READY`，保留同次啟動的初始化日誌。
2. 進入對應副本正常觸發機制。缺圈時記錄大概時間、階段和缺失效果。
3. 保留該次 `dalamud.log`（若輪轉則一併保留相關舊檔案），對照上述 stage 查斷點。

這份補丁增強取證，不宣稱修復絕伊甸缺圈。僅有輸出正常的正例不能反推實戰前置正常。

## 迴歸

- 基礎測試覆蓋事件摘要、隱去名字/任意文字、預算與抑制計數、日誌許可權、其他地圖無輸出、
  未註冊回撥、原異常傳播，以及觀察器失效時仍執行原回撥。
- 對真實重寫 DLL 的七處入口執行 JIT 檢查，並驗證原正則返回值不變。
- `--probe-drawing-diagnostics Host.exe <plugin-root> <独立配置目录>`：原 U7a 光輪讀條/輸出
  兩節點，回撥在隔離 Host 內替換為記錄器；驗證原讀條產生火安全變數、正常延遲輸出、
  功能關閉、缺實體/晚到和故意拋錯。沒有遊戲記憶體許可權，未連線或控制實際遊戲。
- 現有 `--probe-triggernometry-u7b-ordering` 驗證原分散 8 圈/分攤 2 圈加箭頭、半場刀和
  晚到實體時序保持，同時檢查實體到達和原回撥診斷，無 observer-error。

`Fixtures/U7aOrb.xml` 來自 U7a v0.11.1 本機訂閱快取的兩個原節點及祖先配置。
完整來源 SHA-256：`A60984AA5491D1ABB044A978C7CB30A0EB3A7E7DFEDD72F68A68274C1A54350B`。
來源：<https://1824544011.cdn.123clouddisk.com/1824544011/Remote_Triggers/U7a.xml>。
測試命令會寫入獨立配置目錄，不能指向正式使用者配置。
