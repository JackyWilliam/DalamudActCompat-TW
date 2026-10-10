# Dalamud ACT Compat (TW)

> 本倉庫為台服 API 13 / .NET 9 獨立候選構建，未包含 Matcha，尚待台服遊戲內驗收。依賴鎖定及驗證範圍見[台服構建說明](docs/taiwan-build.md)。

[繁體中文](README.md) | [English](README_EN.md)

台服版預設使用**繁體中文**；首次啟動與恢復預設設定均採用繁體，已儲存的語言選擇會保留。

[官方網站](https://dact.localhost2019.com/) · [台服候選構建](https://github.com/JackyWilliam/DalamudActCompat-TW/actions/workflows/build.yml) · [第三方擴充套件相容說明](docs/third-party-compatibility.md) · [問題反饋](https://github.com/JackyWilliam/DalamudActCompat-TW/issues) · [愛發電支援](https://ifdian.net/a/raynor)

**在 FFXIV 的 Dalamud 中直接使用戰鬥統計、Cactbot 懸浮窗和常見 ACT 擴充套件，不需要另外啟動 ACT。**

Dalamud ACT Compat（簡稱 DACT）把 `FFXIV_ACT_Plugin`、`OverlayPlugin` 和遊戲內介面整合進一個 Dalamud 外掛。Triggernometry、鯰魚精、FoxTTS 等傳統 ACT 擴充套件執行在獨立 Host 程序中；某個擴充套件卡死時，可以只重啟對應 Host，不必立刻重啟遊戲。

> **DACT 是免費專案。** 外掛和一次性啟用碼均不收費，請勿購買或轉售；如果有人借 DACT 或啟用碼收費，請透過 [Issues](https://github.com/JackyWilliam/DalamudActCompat-TW/issues)、QQ 群或 Discord 聯絡維護者。

此獨立構建面向 **Windows、台服 Dalamud API 13、.NET 9**，尚未完成台服遊戲內驗收。它不是 Square Enix、Dalamud、ACT、FFLogs 或所捆綁第三方擴充套件的官方專案。

## 你可以用它做什麼

| 功能 | 實際用途 |
| --- | --- |
| 遊戲內戰鬥統計 | 檢視 4/8/24 人 DPS、rDPS、HPS、總傷害、最高傷害、命中率、死亡和本地 FFLogs 區間估算 |
| 統計樣式 | 經典榜、橫版和職能分欄三選一；8 人經典表格可自定義，24 人使用固定的職業/名字 + DPS/HPS 緊湊條 |
| 戰鬥歷史 | 按每次進本和每一把戰鬥儲存記錄，回看隊伍資料與原始日誌 |
| Cactbot / HTML 懸浮窗 | 安裝 Cactbot 資源，在遊戲內建立、縮放、鎖定和穿透懸浮窗 |
| 常見 ACT 擴充套件 | 使用 Triggernometry、PostNamazu、ACT.FoxTTS 和 SilverDasher |
| 自行匯入擴充套件 | 對 DLL/ZIP 做靜態預檢，確認許可權後在通用 Host 中載入相容的 ACT 外掛 |
| 帳號與雲同步 | 使用啟用碼註冊、可選自動登入、生成最多 3 個好友邀請碼，並端到端加密備份配置 |
| 好友與聊天 | 新增好友、檢視線上狀態、收發訊息及離線來信；支援未讀提醒、來信氣泡、快捷回覆和四種提示音 |
| RPets 共用登入 | 與獨立安裝的 RPets 同步登入、切換帳號和退出狀態 |
| 故障隔離與診斷 | 分開檢視解析器、共享 Host 和通用 Host，按元件停止或重啟 |

它不承諾相容所有 ACT 外掛。依賴舊版 .NET Framework、注入、任意記憶體讀寫、特殊 ACT 介面或未實現介面的擴充套件，可能無法執行。

## 安裝

### 1. 獲取台服候選包

台服訂閱尚未釋出。可執行 `tools/build-taiwan.ps1` 構建，或從本倉庫 `main` 的 Taiwan Build 工作流下載候選產物。將完整候選包解壓到獨立測試目錄，透過台服 Dalamud 的開發外掛設定登記 DLL。候選包僅適用於鎖定的 API 13 客戶端版本，不覆蓋原國服安裝。

測試前請閱讀[台服構建說明](docs/taiwan-build.md)。歷史國服版本修復工具不適用於此台服構建。

### 2. 獲取一次性啟用碼

註冊 DACT 帳號需要一次性啟用碼。請加入 QQ 群 **1098561701**，或加入 [Discord 伺服器](https://discord.gg/mZHFwXe7Gk)，聯絡群主或頻道主獲取。

> 啟用碼使用後即失效，請勿把收到的啟用碼釋出到 Issue 或其他公開位置。

### 3. 完成首次啟動

1. 閱讀首次彈出的第三方擴充套件宣告，核對作者、來源、版本、許可權和 SHA-256，再決定是否安裝及授權。
2. 輸入 `/actcompat` 開啟控制中心。
3. 在“概覽”確認“啟用解析”和“自動啟動解析器”已開啟，頂部狀態為“執行中”。
4. 輸入 `/actcompat meter` 開啟戰鬥統計，對木人或副本敵人造成有效傷害後即可看到資料。
5. 需要 Cactbot、Triggernometry、鯰魚精或其他擴充套件時，再到“懸浮窗”和“擴充套件”頁配置。

控制中心右上角有完整的遊戲內幫助，支援中英文關鍵詞搜尋和宏指令複製。

0.4.1.0 起，登入後點選主視窗雲狀態右側的好友圖示，即可查詢帳號、處理好友申請和開啟聊天。狀態彈窗可設定線上狀態及是否分享當前副本；來信提醒設定支援獨立開關氣泡和聲音、調整位置與背景透明度、選擇及試聽四種提示音。每對好友雙方合計保留最近 20 條歷史和最新 3 條離線待收訊息。這裡的好友申請與註冊用的邀請碼是兩個獨立功能。

> “開啟 FFLogs 上傳日誌”只會開啟本地 Network 日誌目錄並複製路徑，不會自動上傳。診斷日誌用於排錯，不能代替戰鬥日誌。

## 介面預覽

![繁體中文控制中心概覽](docs/images/readme/tw-home.png)

| Cactbot 時間軸 | 自訂 HTML 懸浮窗 |
| --- | --- |
| [![Cactbot 設定](docs/images/readme/tw-cactbot.png)](docs/images/readme/tw-cactbot.png) | [![懸浮窗一般設定](docs/images/readme/tw-general.png)](docs/images/readme/tw-general.png) |
| **顯示／隱藏快捷鍵** | **來源與網址** |
| [![快捷鍵設定](docs/images/readme/tw-hotkeys.png)](docs/images/readme/tw-hotkeys.png) | [![來源設定](docs/images/readme/tw-source.png)](docs/images/readme/tw-source.png) |

> 圖片由台服候選版的實際介面繪製程式以繁體中文產生，使用離線測試資料，並非遊戲內實測截圖。範例中的自訂名稱會原樣保留，不隨介面語言轉換。點選小圖可檢視原圖。

## 控制中心

| 頁面 | 這裡能做什麼 |
| --- | --- |
| 概覽 | 檢視解析器狀態；開關解析、自動啟動、失焦隱藏網頁懸浮窗和精簡模式；開啟統計、歷史、執行狀態與日誌目錄 |
| 戰鬥統計 | 在三個互斥模板間切換；設定 8/24 人模式、DPS/HPS 排序、獨立統計槽位、透明度和 FFLogs 估算 |
| 懸浮窗 | 安裝 Cactbot；管理文字提醒、時間軸和自定義 HTML 懸浮窗；設定遊戲失焦時隱藏網頁懸浮窗 |
| 擴充套件 | 啟停擴充套件；開啟擴充套件配置；檢查來源與更新；匯入 DLL/ZIP；分配許可權 |
| 雲同步 | 註冊或登入帳號；選擇是否自動登入；管理好友邀請碼；上傳、預覽、恢復或回滾端到端加密配置 |
| 設定 | 檢視固定的台服區域；重啟解析器；複製診斷；設定語言和快捷按鈕；執行需再次確認的恢復出廠設定 |

解析區域固定為台服，匯入舊配置也不會切換底層協議；客戶端語言仍獨立保留。FFLogs 區間估算使用 M5–M8 地圖表及 TW 查詢範圍，缺少台服資料時不會借用國服或全球快取分數。

## 常用命令

| 命令 | 作用 |
| --- | --- |
| `/actcompat` | 開啟或關閉控制中心 |
| `/actcompat on` / `/actcompat off` | 明確開啟 / 關閉控制中心 |
| `/actcompat meter` | 開啟並定位戰鬥統計 |
| `/actcompat simple on` / `/actcompat simple off` | 進入 / 退出精簡模式；精簡主頁只保留統計窗開關與退出入口 |
| `/actcompat history` | 開啟近期戰鬥 |
| `/actcompat logs` | 開啟已儲存的日誌檔案列表 |
| `/actcompat status` | 檢視解析器與各 Host 的執行狀態 |
| `/actcompat cactbot` | 開啟當前選擇的 Cactbot 懸浮窗 |
| `/actcompat overlay [模板名]` | 開啟當前或指定的 HTML 懸浮窗 |
| `/actcompat clear` | 清空當前戰鬥顯示，不刪除歷史或原始日誌 |

診斷與開發命令還包括 `/actcompat host`、`stop`、`sample`、`install "<DLL 或 ZIP 路径>"` 和 `factory-reset`。輸入未知子命令時，外掛會開啟內建的完整命令頁。

## 戰鬥統計怎麼理解

- **DPS**：按每名玩家自己的有效動作時長計算。
- **EncDPS**：按整場戰鬥經過時間計算，更接近傳統 ACT 的整場口徑。
- **rDPS**：根據本地戰鬥事件和團隊增益歸屬實時估算，不等於 FFLogs 伺服器端結果。
- **HPS**：按本把戰鬥從開怪到結束的完整時間計算，包括轉階段和目標不可選中的時間。
- **最高傷害**：記錄本場最高的一次命中；之後出現更高傷害時替換，相同傷害不抖動，下一場重新記錄。
- **FFLogs 區間估算**：把本場資料代入本地快取曲線，只用於即時參考，不是上傳後的正式排名。
- **`--`**：當前還沒有足夠的有效資料，不表示數值為零。

戰鬥結束後，實時統計會保留上一把結果；下一場出現有效資料後才從零替換。副本內團滅重開後也遵循這一規則；歷史頁仍把同一次進本放在一個資料夾裡，每一把作為獨立子記錄儲存。轉階段產生的 ACT 片段只在內部合併，不會被誤拆成多把。

“重置當前戰鬥”只結束並清空當前顯示，不會刪除已經儲存的歷史和原始 Network 日誌。玩家 ID 遮蓋也隻影響介面顯示，不會改寫日誌內容。

## 擴充套件、許可權與重啟

“啟用擴充套件”和“開放許可權”是兩個獨立條件。一個擴充套件顯示已啟用，不代表它已經獲得網路、檔案、遊戲指令、原生記憶體或高風險指令碼許可權。

| 元件 | 執行位置 | 更新或異常後優先操作 |
| --- | --- | --- |
| Triggernometry、PostNamazu、ACT.FoxTTS、SilverDasher | 共享 Host | `/actcompat status` →“重啟共享 Host” |
| 自行匯入的普通 ACT 擴充套件 | 通用 Host | `/actcompat status` →“重啟通用 Host” |
| FFXIV_ACT_Plugin、OverlayPlugin | 遊戲內解析器 | 控制中心“設定”→“重啟解析器” |

傳統 ACT 擴充套件以當前 Windows 使用者許可權執行。獨立 Host 能避免大多數擴充套件崩潰直接帶走 FFXIV，但它不是作業系統沙箱；同一 Host 內的惡意 DLL 仍可能呼叫桌面 API 或影響其他擴充套件。只安裝可信來源，並只開放實際需要的許可權。

第三方元件的維護來源、隨包版本、安裝模型和已驗證範圍見[第三方 ACT 擴充套件相容說明](docs/third-party-compatibility.md)。

## 常見問題

### `/actcompat` 沒反應或提示命令不存在

在 `/xlplugins` 中確認 Dalamud ACT Compat 已安裝、已啟用且沒有載入錯誤。此時外掛內的重啟和診斷按鈕也無法工作，先處理 Dalamud 顯示的載入錯誤。

### 戰鬥統計視窗不見了或一直為空

1. 輸入 `/actcompat meter`，並暫時關閉“脫戰自動隱藏”。
2. 確認“概覽”和 `/actcompat status` 中的解析器都是“執行中”。
3. 對木人或副本敵人實際造成幾次有效傷害；僅進本、選中目標或開啟視窗不會產生資料。
4. 仍為空時重啟解析器，並重新產生一場新樣本。

### 只有某個擴充套件、TTS、觸發器或標點不能用

先確認擴充套件本身已啟用且許可權足夠，再到 `/actcompat status` 只重啟它所在的 Host。不要在解析器未執行時反覆重灌下游擴充套件。

### 懸浮窗能顯示網頁，但沒有戰鬥資料

網頁開啟不等於資料協議已經連線。先確認解析器和 OverlayPlugin 正在執行，再檢查該懸浮窗的現代 OverlayPlugin / ACTWS 自動檢測狀態並執行“重新檢測”。

### 反饋問題需要什麼

請提供 DACT 版本、發生時間、復現步驟、預期結果、實際現象、`/actcompat status` 中第一個失敗的元件和“複製診斷日誌”的完整內容。統計缺失或數值異常還需保留對應的 `Network_*.log`。提交前請自行檢查角色名、伺服器名和本地路徑。

## 它是怎麼執行的

```text
FFXIV / Dalamud 程序
└─ Dalamud ACT Compat
   ├─ FFXIV_ACT_Plugin + OverlayPlugin
   ├─ 控制中心、戰鬥統計、歷史與懸浮窗
   ├─ 帳號、邀請和本機加密的雲配置同步
   └─ 有界 IPC
      ├─ 共享 Host：Triggernometry / PostNamazu / FoxTTS / SilverDasher
      └─ 通用 Host：使用者匯入的相容 ACT 擴充套件
```

解析器和 OverlayPlugin 留在遊戲程序中，以便直接接收 Dalamud 資料並繪製介面；傳統 ACT 擴充套件放到獨立程序中，便於限流、診斷、停止和重啟。IPC 只開放經過定義的訊息和遊戲指令入口，不向 Host 暴露任意函式指標。

## 開發與構建

需要 Windows、.NET SDK 10、.NET 9 Windows Desktop 執行庫和 7-Zip。指令碼校驗鎖定的台服 API 13 引用，生成獨立候選包，不更新國服訂閱。

```powershell
git clone --recurse-submodules https://github.com/JackyWilliam/DalamudActCompat-TW.git
cd DalamudActCompat-TW
./tools/build-taiwan.ps1
```

依賴來源、SHA-256、測試覆蓋及遊戲內驗收邊界見[台服構建說明](docs/taiwan-build.md)。

## 許可證

Copyright (C) 2026 DalamudActCompat contributors.

Dalamud ACT Compat 以 [GPL-3.0](LICENSE.md) 釋出。隨包第三方元件分別遵循各自許可證；詳情見 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) 和釋出包內的 `LICENSES/` 目錄。
