# 第三方 ACT 擴充套件相容說明

> 台服目前的依賴版本、功能範圍與驗收狀態以[台服構建說明](taiwan-build.md)為準；Matcha 不隨台服候選包提供。以下保留原專案的相容性與版本歷史，不能視為台服遊戲內驗收結果。

2026-10-10 新增 [ACTLogsUploader 與絕神兵工具手動相容](manual-act-plugins.md)：兩者由第三個通用 Host 載入，不隨包分發。修復通用 Host 的 TTS 來源校驗、僅訂閱後置事件時的日誌遺失，以及上傳目錄和 .NET 執行階段依賴相容；已隨原專案 0.4.5.9 發佈。

2026-09-14 新增 [Simulant 手動匯入相容適配](simulant-compatibility.md)：DACT 僅適配使用者自行提供的原版 DLL；未安裝時隱藏，不提供下載或隨包分發。共享 Host 離線聯調與本機國服 7.56 的 64 組靜態簽名檢查透過，實際遊戲模擬待驗收。

核對日期：2026-09-17；0.4.4.0 已完成 7.56 熱修離線迴歸，實際遊戲語音、字幕仍需更新後驗收，正式釋出狀態以 GitHub Release 為準。詳細來源見
[7.56 熱修依賴審計](bundled-plugin-audit-2026-09-17.md)；原版 7.56 的歷史記錄見 [9 月 9 日審計](bundled-plugin-audit-2026-09-09.md)。

7.56 熱修的國服、國際服和韓服操作碼已按上游分別核對。解析鏈已更新到
IINACT 2.10.3.8、OverlayPlugin Core 0.19.108、Machina 7.56h 事件表、
Unscrambler.XIV 7.56.1 和 FFXIV_ACT_Plugin 3.0.3.1。
國服 `2026.09.15.0000.0000` 使用 Unscrambler 7.56.1 的完整操作碼定義，並從正在執行的
國服客戶端動態定位地區專用金鑰表，不復用國際服固定記憶體地址。未來遊戲版本未被正式支援時，
外掛會明確提示兜底解析不保證 FF Logs 排名有效，不再靜默生成看似安全的上傳日誌。

Triggernometry 中文維護版 2.2.0.1、ACT.FoxTTS 3.3.1.189、PostNamazu
1.3.6.6、SilverDasher 0.6.0.4 和 Cafe.Matcha 26.8.12.1622 隨本外掛釋出。
首次安裝，以及隨包或線上 DLL 的版本/雜湊實際變化後，
遊戲內會彈出告知視窗，展示 DLL 作者、當前維護者、版本、許可證、專案網址、
原始碼網址、DLL 實際下載網址和 SHA-256；視窗不會自動開啟網頁，未確認時不會載入
這些 DLL。Triggernometry 2.2 的區域事件介面已接入 Host，保留舊版方法介面；抹茶
和銀山雀兒在載入時使用 7.56h 的國服/國際服對映；2026-09-21 已按抹茶上游
`3e4ecb4` 補齊 FateInfo `0x0154` 與抹茶 WorldVisitQueue `0x01E8`。
抹茶 DACT4 包同步該提交的動態事件、FATE、副本和世界資料，相容 DLL 保持不變。
完整包按 ZIP 雜湊識別資料更新，舊版缺少包雜湊的安裝需要重新確認安裝一次；
更新時保留使用者禁用狀態。Cactbot 官方包仍為 0.37.5，7.56 新資料尚在
上游原始碼中；不將其描述為已經隨正式包分發。上述資訊僅說明元件來源，不代表原作者或維護者與本專案存在合作、
認可或聯絡。單純升級 DalamudActCompat 不再重複告知；許可權確認也只在尚無明確決定的
能力出現時顯示。外掛每次啟動還會檢查公開上游：Triggernometry 使用公開提供的
DLL/漢化檔案地址，FoxTTS 和 PostNamazu 使用 GitHub 最新 Release；發現檔案版本
或雜湊變化後先停止舊擴充套件的後續載入並再次彈窗，確認後才安裝下載快取中的版本。
單個來源離線或檢查失敗時保留經過鎖定校驗的隨包版本，不影響主體啟動。

| 元件 | 當前維護來源 | 釋出物/安裝模型 | 當前相容範圍 |
| --- | --- | --- | --- |
| Cactbot | [OverlayPlugin/cactbot](https://github.com/OverlayPlugin/cactbot) | 官方 Release ZIP；OverlayPlugin addon，包含 DLL、UI、資源與時間軸 | 可獨立安裝完整資源；內建 Cactbot 事件源；HTML Overlay 編輯模式禁用 WebView2 輸入，由宿主檢測全域性滑鼠並讓唯一目標視窗取得 Windows 滑鼠捕獲，同時只在可見編輯視窗下方啟用完全透明的 Dalamud 滑鼠輸入層，阻止遊戲重置游標且不切換外部視窗焦點；鎖定後移除輸入層並恢復網頁互動，等待遊戲內聯動驗收 |
| CactbotSelf / MoreLogLine | [tssailzz8/cacbotSelf](https://github.com/tssailzz8/cacbotSelf) | `CactbotSelf.dll` 或作者 ZIP；ACT + OverlayPlugin 擴充套件 | 可安裝和載入；額外日誌/攝像機/標點能力需遊戲內逐項實測 |
| PostNamazu | [Natsukage/PostNamazu](https://github.com/Natsukage/PostNamazu) | `PostNamazu.dll` 或作者 ZIP；普通 ACT 外掛，同時聯動 FFXIV_ACT_Plugin、OverlayPlugin、Triggernometry | 在原共享 Host 中執行；程式集、`InitPlugin`、ACT/FFXIV facade、日誌、Clipboard 和語義命令 Broker 分階段顯示狀態。“複製日誌”只在 UI 執行緒快照，拼接和 Clipboard 重試轉到有界 STA 佇列。任意地址讀寫/`calli` 已禁止；遊戲命令預設拒絕，等待遊戲內逐動作驗收 |
| ACT.FoxTTS | [Noisyfox/ACT.FoxTTS](https://github.com/Noisyfox/ACT.FoxTTS) | 當前官方 Release 為 7z，解壓後選擇 `ACT.FoxTTS.dll`；普通 ACT 外掛 | 在原共享 Host 中執行並可載入；音訊裝置和所選網路語音後端等待遊戲內實測 |
| Triggernometry 中文維護版 | [MnFeN/Triggernometry](https://github.com/MnFeN/Triggernometry) | `Triggernometry.dll` + `*.triglations.xml`；普通 ACT 外掛 | 在原共享 Host 中執行；離線真實 DLL 已驗證配置讀取、標準日誌、網路等價事件、區域限制、戰鬥開始/結束、正則、遊戲側 TTS 授權和 Host 內輸出排程閉環。管理員狀態保持真實，普通許可權能力改為說明；網路、外部程式和高風險指令碼預設拒絕；實際音訊裝置輸出仍待遊戲內手測 |
| SilverDasher | FFCafe / The Players | 經授權隨包的完整 `0.6.0.4` 包 | 預設關閉；使用者啟用後在原共享 Host 中最後載入。通知、日誌、TTS、訂閱和受控記憶體相容橋已接入 |
| Cafe.Matcha | [thewakingsands/matcha](https://github.com/thewakingsands/matcha) | 精確原始碼提交 `6cf242b` 加公開 DACT 相容補丁；完整包與入口 DLL 雙雜湊固定 | 預設啟用；嚴格在共享 Host 外掛之後啟動，單獨執行於第二個 Host，擁有獨立 IPC、收發包有界佇列、許可權快照、停止和重啟入口 |
| Triggernometry 原版 | [paissaheavyindustries/Triggernometry](https://github.com/paissaheavyindustries/Triggernometry) | Release ZIP；普通 ACT 外掛 | 僅保留相容基線，不作為中文使用者首選來源 |

`quisquous/cactbot` 是舊上游，不再作為安裝器的當前來源。CactbotSelf 也不是
Cactbot 本體，兩者在設定頁和安裝目錄中保持獨立。

傳統 ACT 外掛由 `DalamudActCompat.Host.exe` 載入，不再由 FFXIV/Dalamud 程序直接
載入。FoxTTS、PostNamazu、Triggernometry 和 SilverDasher 保持在原共享 Host；
Cafe.Matcha 獨佔第二個 Host。抹茶崩潰不會終止共享 Host 或遊戲核心心。Host 崩潰、
斷管和阻塞讀執行緒已有自動檢測/終止測試；這提供程序級故障隔離，但 Host 仍是普通
使用者許可權的完整信任程序，不能視為 OS 沙箱。

“DLL 成功載入”只代表基礎 ABI 透過，不代表外掛所有功能已經相容。完整聯動依賴
ACT 的日誌、戰鬥生命週期、外掛列表、TTS/聲音、WinForms 相容物件，以及
OverlayPlugin 的事件源、WebSocket 和基於 WebView2 的 Cactbot HTML Overlay 視窗。
