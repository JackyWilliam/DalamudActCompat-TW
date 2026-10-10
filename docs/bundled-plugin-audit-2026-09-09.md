# FF14 7.56 與分發外掛審計

核對日期：2026-09-09；2026-09-10 補充本機國服驗收。基線為公開版 0.4.1.0，本次版本為 0.4.1.1。
0.4.1.1 已安裝，使用者確認遊戲內執行正常並授權釋出；正式釋出狀態以 GitHub Release 為準。

| 元件 | 原版本 | 本次結果 |
| --- | --- | --- |
| FFXIV_ACT_Plugin 與 SDK | 3.0.2.8 | 更新至 [3.0.3.0](https://github.com/ravahn/FFXIV_ACT_Plugin/releases/tag/3.0.3.0)，上游明確支援 7.56 |
| IINACT fork | 2.10.3.6 | 對齊 [2.10.3.7](https://github.com/marzent/IINACT/releases/tag/v2.10.3.7)，保留 DACT 的區域、統計、Host 與日誌補丁 |
| OverlayPlugin Core | 0.19.105 | 對齊 [0.19.107](https://github.com/OverlayPlugin/OverlayPlugin/releases/tag/v0.19.107) 的操作碼和 SDK 更新；DACT 內嵌原始碼埠，不替換成外接 ACT 的 DLL |
| Unscrambler.XIV | 7.55.2 | 更新至 [7.56.0](https://www.nuget.org/packages/Unscrambler.XIV/7.56.0)，包含完整 7.56 常量與嵌入資源 |
| Machina fork | 7.55h2 | 更新國服、國際服、韓服各 26 項操作碼；繁中表保持原區域資料 |
| Triggernometry CN | 2.1.2.2 | 實際分發 DLL 更新到 2.2.0.1；適配 2.2 區域事件介面及切區後的日誌實體快取清理 |
| Cactbot | 0.37.5 | [最新正式包](https://github.com/OverlayPlugin/cactbot/releases/tag/v0.37.5)不變；[7.56 資料更新](https://github.com/OverlayPlugin/cactbot/commit/8fd80bbeb6bbfbc17a341ddc8eddc4a0d04f8012)和[馴獸師資料](https://github.com/OverlayPlugin/cactbot/commit/b228a6fee96bdec37fcbf494c02a0c7f2f448157)僅在原始碼中，等待上游正式包後再同步 |
| PostNamazu | 1.3.6.6 | [正式 Release](https://github.com/Natsukage/PostNamazu/releases/tag/1.3.6.6)不變，繼續現有 Host 命令橋 |
| ACT.FoxTTS | 3.3.1.189 | [正式 Release](https://github.com/Noisyfox/ACT.FoxTTS/releases/tag/3.3.1)不變，下載雜湊一致 |
| SilverDasher | 0.6.0.4 | 保留經授權固定包；Host 更新 InitZone、ActorControlSelf，並防止舊線上資料覆蓋。公開入口不足以證明存在更新的完整分發包 |
| Cafe.Matcha | 26.8.12.1622 / DACT3 | [上游原始碼](https://github.com/thewakingsands/matcha/tree/6cf242b59475aa77e4c2deee61e1b9191be5ba13)未變；Host 同時更新 China/Global 的 21 項對映，保持包內相容構建與授權資訊 |
| CactbotSelf / MoreLogLine（僅支援使用者安裝） | 不隨包 | [上游最新公開 Release](https://github.com/tssailzz8/cacbotSelf/releases/tag/7.25)仍標為 7.25，未發現可宣稱支援 7.56 的新版；原生攝像機/標點等功能需單獨實機驗證 |

## 操作碼與解混淆

- Machina Global 表採用 [marzent/machina@3600c03](https://github.com/marzent/machina/commit/3600c03883f4aa6f76f057c47b97078c3ecec72e)。
- [FFXIVOpcodes@040fafa](https://github.com/karashiiro/FFXIVOpcodes/blob/040fafa385c991d5d1624097125f49bdd0d1781f/opcodes.json)標明 Global/CN/KR 均為 7.56，三個 ServerZoneIpcType 表逐項一致；OverlayPlugin 也分別釋出了三地服 7.56 表。
- 解混淆常量來自 NuGet 包所標註的 [Unscrambler 原始碼提交](https://github.com/perchbirdd/unscrambler/blob/7ddf69f7f04a68e96458689680018e7e4f637c38/Unscrambler/Constants/Versions/7.5/Constants756.cs)：客戶端版本 `2026.09.01.0000.0000`，19 項混淆操作碼，金鑰表大小 `193 × 4 = 772` 位元組。國服使用實際掃描出的表地址，不能複用國際服 `0x2312040` RVA。
- 抹茶別名沿用原協議語義：`ResumeEventScene32` 對應 `MiniCactpotInit`，不是 `EventPlay32`；客戶端市場板請求保留 `0x8000` 方向位。
- 上游未給出新的 `FateInfo`、`WorldVisitQueue` 值。抹茶兩地服均移除舊鍵；銀山雀兒兩地服使用原國際服的越界佔位 `0xF009`。它不是已經驗證的新網路操作碼。舊國服 `FateInfo=0x00E9` 在 7.56 已是 Waymark，繼續使用會錯誤處理場地標點。銀山雀兒的 ActorControlSelf FATE 開始/結束/進度路徑保留。

## Triggernometry 2.2 相容

[2.1.2.2 到 2.2.0.1 原始碼差異](https://github.com/MnFeN/Triggernometry/compare/b0f571a70e3593fc838d563e0af11fb02ea8ea6d...e030ec2fd872adb4cca72dac9aba8f59d6a65815)包括區域訂閱改為事件、日誌轉寫器、倉庫後設資料替換、職業資料和 7.56 VFX 簽名。DACT 的舊方法精確匹配在真實 DLL 上失敗，現保留舊入口並接受新版繫結委託的 owner，沿用弱引用訂閱和解除安裝清理。切區時顯式清理新版日誌實體快取，防止 Host 提前更新 currentZone 後跳過上游清理。

新版 DLL SHA-256：`ea764f6e2ba1a8955574efd9cbf744948e82f0986d15faa3768d07d97a85e5e8`。
原生地面 VFX 的更新函式由 Dalamud 提供的 FFXIVClientStructs 地址解析；本專案沒有照抄上游另一套 StaticVfxRun 呼叫點簽名。更新後的本機 SDK 的建立/更新簽名與 DACT 的移除、Actor VFX 建立/銷燬簽名均在新版國服客戶端唯一命中；實際繪圖仍須實機驗收。

## FFLogs 日誌版本

重打解析器補丁、同步執行時與 SDK 的 Logfile.dll，並實際呼叫最終執行時 DLL 的 `LogFormat.FormatVersion()`。期望輸出為：

```text
This is IINACT 2.10.3.7 (API 1.6.0) based on FFXIV_ACT_Plugin 3.0.3.0
```

這裡的 `API 1.6.0` 是 IINACT 解析介面版本，不能改成 Dalamud API 15。
不修改使用者已有 Network 日誌、不偽造版本頭或上傳結果；新日誌隨候選執行時生成。
迴歸同時驗證未知版本和不匹配金鑰表不能進入已驗證解混淆路徑。離線測試不等同於 FFLogs 服務端排名驗收。

## 驗證邊界

對相關原始碼與上游差異進行本地審查，並執行解析/包、真實 DLL Host、Legacy Resource 和 Scanner 迴歸。Host 檢查使用本輪實際載入的改寫 DLL 路徑，避免共享快取中較新的資源轉換 DLL 導致誤報。

國際服額外使用專案 CI 的[官方參考程式集](https://goatcorp.github.io/dalamud-distrib/stg/latest.zip)進行獨立構建：Dalamud `15.0.3.4+cc568d5616baeb3c32e13acecd9171881a122501`，下載 ZIP SHA-256 為 `2343ab848def4f749a8b7ecf9a139e82f38801a8f631f6b55caf748c768aab4c`。官方 stable/staging 版本後設資料當時均為 15.0.3.4。補充迴歸驗證四個國際服原生客戶端語言碼 × 五種顯示語言（包含中文）均選擇 Global 7.56 配置，並呼叫實際 Unscrambler DLL，將合成 ActorCast/ActionEffect 報文還原為已知技能 ID、傷害欄位且不改寫其他位元組。國際服使用官方金鑰表 RVA `0x2312040`；國服繼續動態掃描。本機未配置可用的國際服客戶端，未聲稱國際服遊戲內實測。

使用者更新後，本機國服遊戲檔案為 `2026.09.01.0000.0000`，客戶端 SHA-256 為 `64e07db25f0b72f9bc2dbd6ff7fb3725095ab251d928dd70bf98b0a8cebc6893`。只讀 PE 檢查確認 ZoneDown 簽名恰有 3 個匹配，PacketDispatcher 虛表簽名唯一匹配，順著 OnReceivePacket 找到國服金鑰表 RVA `0x230DD10`、大小 `772` 位元組，滿足本次配置條件。VFX 相關簽名唯一匹配，SDK 欄位佈局仍為 `0x38/0x50/0x60/0x70/0x260`。使用剛更新的衛月/FFXIVClientStructs 構建透過。

2026-09-10 本機國服啟動日誌確認解析器 3.0.3.0、國服動態金鑰表 `0x230DD10` / `772` 位元組及原生地面 VFX 後端成功初始化。實際新生成的 `Network_30300` 日誌首行與上述 IINACT/解析器版本頭一致，使用者確認 0.4.1.1 遊戲內執行正常。

該確認不等同於國際服遊戲內實測、所有副本機制逐項驗收或 FFLogs 服務端上傳/排名驗收；這些場景及缺少上游值的可選事件仍保留驗證邊界。
