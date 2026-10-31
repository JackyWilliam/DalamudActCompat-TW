# Dalamud ACT Compat 台服獨立構建

台服在獨立倉庫 [DalamudActCompat-TW](https://github.com/JackyWilliam/DalamudActCompat-TW) 的 `main` 維護，使用台服 API 13 / .NET 9 依賴鏈。當前產物為待遊戲內驗收的候選包，不向現有國服訂閱源釋出。

## 範圍

保留戰鬥解析與統計、歷史、Cactbot、HTML 懸浮窗與快捷鍵、PictoACT 繪圖、Host、Triggernometry、PostNamazu、FoxTTS、SilverDasher、自行匯入相容擴充套件、帳號與雲同步、好友與聊天、皮膚及六種介面語言。按使用者要求，未找到台服適配的 Matcha 不隨包提供，也不會從舊配置自動啟用。

預設介面為繁體中文，首次啟動和恢復預設設定均使用繁體；已儲存的語言選擇仍保留。解析區固定台服，匯入舊配置不能改變底層區域。FFLogs 採用台服當前中量級 M5–M8 地圖表及 TW 查詢範圍；缺少台服樣本時不借用 CN/Global 快取。線上樣本是否可用仍取決於 FFLogs 服務。

## 來源與構建

- DACT 修改後的 IINACT 獨立儲存在 [IINACT-TW](https://github.com/JackyWilliam/IINACT-TW)，子模組按提交固定，不依賴原 DACT/IINACT 倉庫的功能分支。
- 台服 IINACT：PlusoneChiang/IINACT `tc/api13_net9`，起點 `0788c177c5df622c9e20c4f00027b873732b5606`。
- Machina：`3cc6e1c50c28ea81d95a05e2b4bf4329ffac0b44`。
- Dalamud：yanmucorp 13.0.0.16，遊戲版本 `2026.09.14.0000.0000`。
- FFXIV_ACT_Plugin 3.0.3.1、Unscrambler 7.41.1；完整來源與 SHA-256 見 `tools/taiwan-dependencies.lock.json`。
- DACT 介面、嵌入式 ACT 與 Host 功能在台服上游分支中重新合入，原工作目錄保持不變。
- 功能與原專案 `main` 0.4.5.18 同步（合併原專案 0.4.5.8–0.4.5.18 的介面、帳號、Host、觸發器時間基準、木人統計、近期戰鬥頁、command 靜默語義與 Redraw/108 診斷修復），隨包 Triggernometry 為 2.3.0.0。原專案的國服／國際服 7.57 解析依賴、收包簽名與操作碼不適用台服，不隨同步帶入；台服仍使用上列依賴鏈與自己的操作碼表。

安裝 .NET SDK 10（用於 slnx）、.NET 9 Windows Desktop 執行庫與 7-Zip，並遞迴檢出子模組：

```powershell
git clone --recurse-submodules https://github.com/JackyWilliam/DalamudActCompat-TW.git
cd DalamudActCompat-TW
./tools/build-taiwan.ps1
# 使用已下载的同版本引用：
./tools/build-taiwan.ps1 -ReferenceDirectory 'X:/tw-dalamud'
```

指令碼驗證引用雜湊、構建完整解決方案、執行離線 smoke，並生成 `artifacts/taiwan/DalamudActCompat-TW-candidate.zip` 與 SHA-256，並將 Host、Cactbot、擴充套件逐檔與本次構建／來源比對，輸出 `resource-content-hashes.json`。API 13 模組和 Host 均面向 .NET 9；打包工具版本不代表遊戲執行時 API。

台服獨立訂閱已提供 **0.4.5.18 測試候選版**，安裝及更新均指向本倉庫的台服 Release 資產。庫鏈為：

```text
https://raw.githubusercontent.com/JackyWilliam/DalamudActCompat-TW/main/repo/pluginmaster.json
```

在 `/xlsettings` 的自訂插件儲存庫加入並儲存後，使用 `/xlplugins` 安裝 **Dalamud ACT Compat (TW)**。此訂閱可直接安裝候選版，並不表示遊戲內驗收已完成；正式穩定版仍待驗收。歷史國服版本修復工具不會從台服倉庫分發，它的內建 API 15 修復包不適用於台服。分發步驟見[台服訂閱說明](CUSTOM_REPOSITORY.md)。

## 驗證邊界

離線覆蓋真實 ACT/Triggernometry/SilverDasher 載入、台服報文佈局與操作碼、Host 傳遞、Cactbot 配置、快捷鍵、繪圖回放與加速迴圈，以及皮膚/語言原生布局矩陣。Host 測試每次複製獨立擴充套件目錄，避免 Triggernometry 獨佔語言 XML 導致並行測試彈窗。

沒有把離線成功當作遊戲內驗收。仍需台服實際客戶端驗證封包 hook、原生 VFX、24 人隊伍佈局與 SilverDasher FateInfo 動態識別。FateInfo 讀取以三個已釋出台服操作碼交叉校驗，失敗時保留未知值並記錄日誌，不猜用其他區操作碼。FFLogs 真實 TW 線上資料也未做帳戶授權查詢驗證。

版權與許可見 `LICENSE.md`、`THIRD_PARTY_NOTICES.md` 及包內 `LICENSES/`。
