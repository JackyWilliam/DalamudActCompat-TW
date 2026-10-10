# 2026-09-27 外掛核查、抹茶更新與 FoxTTS 佈局修正

## 上游核查

| 元件 | 已核對版本或提交 | 處理 |
| --- | --- | --- |
| 抹茶 | [d4fffc6，2026-09-26](https://github.com/thewakingsands/matcha/commit/d4fffc60399549b9e9b8af9d6ca9bfec7378c7ff) | 從此原始碼構建 26.9.26.1753 DACT5，補齊 Host 的五項釣魚操作碼 |
| Triggernometry 中文維護版 | [2.2.0.1 / b795485](https://github.com/MnFeN/Triggernometry/commit/b795485b280bc7acf5ece8139fad54870f7eea46) | 維護者 DLL、漢化與許可證重新下載後校驗一致 |
| 鯰魚精郵差 | [1.3.6.6](https://github.com/Natsukage/PostNamazu/releases/tag/1.3.6.6) | 最新正式包校驗一致，現有 1.3.6.6/1.3.6.7 介面相容保留 |
| 仿生石 | [0.0.4.2](https://github.com/MnFeN/Simulant/releases/tag/v0.0.4.2) | 最新 DLL SHA-256 與現有測試鎖定值一致，原版相容測試透過；仍為使用者安裝擴充套件 |
| FoxTTS | [3.3.1.189](https://github.com/Noisyfox/ACT.FoxTTS/releases/tag/3.3.1) | 最新 DLL 校驗一致，在 DACT Host 中修正視窗布局 |
| 銀山雀兒 | 0.6.0.4 | 已有二進位制和 Host 迴歸透過；公開入口仍不能證明最新版，不宣稱沒有新包 |
| Cactbot | [0.37.5](https://github.com/OverlayPlugin/cactbot/releases/tag/v0.37.5) | 最新正式包與鎖檔案一致；main 的八個未釋出提交仍作為可選後續項 |

IINACT 2.10.3.8、OverlayPlugin 0.19.108、FFXIV ACT Plugin 3.0.3.1 的
官方 latest API 均與當前依賴一致。本次沒有更換解析核心。

## 抹茶

上游新增 Fishing 事件涵蓋拋竿、咬鉤、提竿、收線、捕獲、結束、退出和
換區重置，並攜帶魚餌、釣場、鉤型、狀態和時間。現有網路橋已傳遞原始
epoch，本次保留這條路徑。Host 重新構造事件表時補入 EventPlay4、
SystemLogMessage、FishCaught、StatusEffectList 和 ClientTrigger，後者保留
客戶端方向位。舊版 DLL 缺少新增列舉時，仍可載入原有事件對映。

DACT5 只替換 DACT4 包中的入口 DLL。資料、manifest、上游伴隨 DLL 和
加密執行時常量逐位元組保持。相容補丁繼續限制配置檔案訪問並提供原有
非阻塞通知橋。原始碼、版本與 DLL/ZIP 雜湊在鎖檔案和
`vendor/BundledActPlugins/matcha/BUILD.md` 中同步記錄。

## FoxTTS 視窗

原版一般設定頁把引擎面板錨定在固定左欄右側。控制元件縮放後，窄視窗中
右側滑條會被裁切。DACT 預設 960 畫素寬的承載視窗及其 WinForms 環境
使該佈局限制更容易出現；原版控制元件的隔離測試能夠復現。

Host 初始化 FoxTTS 後，將同一批控制元件放進可重排的佈局：空間足夠時保留
雙欄，空間不足時先顯示引擎和滑條、再向下排列其他設定。窄視窗使用
縱向滾動。控制元件和繫結保留，隨包 FoxTTS DLL、TTS 引擎、聲音設定和播報
路徑均未修改。其他擴充套件的視窗不應用此佈局。

## 驗證範圍

- 真實 FoxTTS 控制元件：中英文，100%/125%/150%/200% 控制元件縮放，700/944/1400
  畫素客戶區，反覆縮放視窗和更換 CafePro/Cafe/Edge 設定面板。檢查滑條
  橫向完整可見、寬度可操作、數值不變和無橫向滾動，並檢查截圖。
- 上述縮放是離線控制元件縮放測試，不等同於各物理顯示器 DPI 的現場驗收。
- 真實抹茶 DLL：國服和國際服事件表、釣魚完整生命週期、重複包、錯誤
  方向、截斷包、未知魚餌與服務端確認以小釣大、原 FishBite 事件；另迴歸
  FATE、跨服排隊、Host 雙向網路、通知、許可權與解除安裝。
- 全套 Package/FFXIV、Host（含真實 Triggernometry/PostNamazu/FoxTTS/
  SilverDasher/Matcha）、仿生石原版、LegacyResource 及 U7b 時序測試。
- 按現有釋出引數構建和收集完整包，檢查程式集/依賴版本、資源身份、
  包雜湊和無本機路徑/配置/除錯符號。未啟動遊戲或觸發真實遊戲命令。

本次適配隨 DACT 0.4.5.2 釋出。遊戲內釣魚事件及 FoxTTS 在使用者實際
螢幕上的顯示效果仍需更新後驗收。
