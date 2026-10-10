# 7.56 熱修依賴審計（0.4.4.0）

目標客戶端：`2026.09.15.0000.0000`。此前釋出物仍使用 9 月 1 日事件表；新客戶端 ActionEffect01 從 `0x2EC` 改為 `0x313`，舊解析器不能按原編號識別技能事件。四種坦克無敵的觸發規則已經存在，本次更新解析輸入所需的版本配置和事件表。

## 依賴來源

| 元件 | 本次使用 | 來源 |
| --- | --- | --- |
| FFXIV ACT Plugin | 3.0.3.1 | [正式釋出](https://github.com/ravahn/FFXIV_ACT_Plugin/releases/tag/3.0.3.1)；官方 ZIP SHA-256 `9703ccf9f2937fb4840c42b1b82e1d4834f5e81ba9ec80e3c234f54d251525f5` |
| IINACT | 2.10.3.8，保留 DACT 分支修改 | [上游版本](https://github.com/marzent/IINACT/releases/tag/v2.10.3.8) |
| OverlayPlugin Core | 0.19.108 | [上游釋出](https://github.com/OverlayPlugin/OverlayPlugin/releases/tag/v0.19.108)；事件表取 IINACT `v2.10.3.8` 的 `OverlayPlugin.Core/resources/opcodes.jsonc` |
| Unscrambler.XIV | 7.56.1，NuGet 鎖定 | [包](https://www.nuget.org/packages/Unscrambler.XIV/7.56.1)、[原始碼提交](https://github.com/perchbirdd/unscrambler/commit/c4abc533dda9ab29afe0883854e1d9e1c4a9d69a) |
| Machina | DACT 分支 2.4.5.5 + 7.56h 事件表 | [事件表來源提交](https://github.com/ravahn/machina/commit/6bb98826caf2b445dadb0cb8eb40c4bd55ecb373)；僅同步 Global/CN/KR 三份表，未將整個分支替換成 NuGet 2.4.7.9 |
| Cactbot | 0.37.5 | 保留已有鎖定資源 |

ACT DLL 經現有同步工具解包和相容處理；日誌頭仍保留真實解析器身份，不使用偽造版本字串。依賴 SDK 副本與執行副本一起更新。

## 地區和解混淆約束

- 7.56h 的混淆表長度為 165 個整數（660 位元組），不是原 7.56 的 772 位元組；19 個混淆事件均核對。
- 國際服使用官方配置。國服仍從當前客戶端掃描專用金鑰表地址，不復用國際服的固定 RVA `0x2312740`。僅在已核對版本、表長度及完整操作碼滿足約束時採用地區配置；未知版本或長度不匹配保持原有明確 fallback 提示。
- InitZone=`0x32B`、ActorControl=`0x25F`、ActorCast=`0x162`、ActionEffect01=`0x313`；Machina、Unscrambler 與 OP 的相關事件保持一致。
- 抹茶和銀山雀兒同步對應對映；別名 CompanyAirshipStatus 對應 AirshipTimers，ResumeEventScene32 對應 MiniCactpotInit。MarketBoardItemRequest 保留出站方向高位；未經確認的 FateInfo/WorldVisitQueue 不填入猜測值。

## 驗證與邊界

- 國服掃描地址選擇、拒絕國際服固定地址、舊錶長度和未來未知版本的迴歸。
- 國服／國際服合成 ActorCast、ActionEffect 包，覆蓋神聖領域（`1E`）、死鬥（`2B`）、行屍走肉（`E36`）、超火流星（`3F18`）及寬技能編號；比較完整解碼包和傷害欄位，檢查無額外寫入。
- 構建、完整 Package/Host 迴歸及實際 cimgui + 本機 SqPack 的皮膚和好友介面迴歸；釋出流水線再次執行掃描、Host、仿生石、U7b 初始化、繪圖診斷及資源檢查。
- 合成解碼不是遊戲內施放驗收。語音、字幕和真實副本繪圖仍需更新後驗證，不能據此宣稱所有絕伊甸／絕妖星缺圈均已修復。

本次公開版同時包含已完成的皮膚、統計背景、彈窗與底邊修正，以及團隊秒傷。未覆蓋本機安裝，保留使用者的稍後安裝安排。
