# 隨包外掛與解析依賴走查（2026-08-30）

本次以倉庫鎖定檔案、隨包二進位制版本/雜湊和維護方公開源為準。結論分為“需要更新”“需要適配”和“無需變更”，避免把遊戲內或遠端的版本提示直接當成釋出版本。

## 隨包 ACT 外掛

| 元件 | 隨包版本 | 上游核對 | 結論 |
| --- | --- | --- | --- |
| Triggernometry CN | 2.1.2.2 | [MnFeN/Triggernometry](https://github.com/MnFeN/Triggernometry) 不釋出 GitHub Release，維護版透過倉庫 README 中的直鏈分發。使用者新下載的 DLL 與隨包檔案版本、長度和 SHA-256 完全相同 | 無更新；遊戲內 `.3` 提示為誤報，不改包 |
| ACT.FoxTTS | 3.3.1.189 | [最新 Release 仍為 3.3.1](https://github.com/Noisyfox/ACT.FoxTTS/releases/tag/3.3.1) | 無更新，無新增適配 |
| PostNamazu | 1.3.6.6 | [最新 Release 仍為 1.3.6.6](https://github.com/Natsukage/PostNamazu/releases/tag/1.3.6.6) | 無更新；現有 Host 許可權/命令橋繼續適用 |
| SilverDasher | 0.6.0.4 | 完整包版本不變；公開資料端的國際服 `InitZone`、`ActorControlSelf` 仍落後於 7.55h2 | 需要適配：更新隨包 opcode 種子，並在讀取下載資料後歸一化，防止遠端舊值覆蓋 |
| Cafe.Matcha | 26.8.12.1622 | 隨包原始碼基線仍為 [thewakingsands/matcha@6cf242b](https://github.com/thewakingsands/matcha/tree/6cf242b59475aa77e4c2deee61e1b9191be5ba13)，其 Global 表仍是 7.55h | 需要適配：Host 載入時注入已驗證的 Global 7.55h2 對映；沒有權威值的 `FateInfo`、`WorldVisitQueue` 不保留衝突舊值 |

## 遊戲內解析鏈

| 元件 | 調整前 | 調整後 | 說明 |
| --- | --- | --- | --- |
| IINACT | 2.10.3.5 | 2.10.3.6 | 對齊 [官方 v2.10.3.6](https://github.com/marzent/IINACT/releases/tag/v2.10.3.6)，保留本倉庫區域服和 Host 相容補丁 |
| OverlayPlugin Core | 0.19.104 | 0.19.105 | 對齊 [官方 v0.19.105](https://github.com/OverlayPlugin/OverlayPlugin/releases/tag/v0.19.105)；7.55h2 opcode 已核對 |
| Unscrambler.XIV | 7.55.1 | 7.55.2 | 7.55.2 已包含 `2026.08.11.0000.0000` Global h2 資源 |
| Machina | 7.55h2 fork | 不變 | 當前鎖定提交已經包含 Global 7.55h2 與國服資料，不回退到僅國際服的上游提交 |
| FFXIV_ACT_Plugin | 3.0.2.8 | 不變 | 二進位制已經是 3.0.2.8；只修正文件中誤寫的 3.0.2.7 |

## 其他結論

- Cactbot 不作為固定 DLL 隨包，它由官方 Release ZIP 的獨立安裝/校驗流程維護，本次沒有發現需要改宿主介面的新問題。
- 雙開 ACT / 同時啟動兩套解析鏈不列為缺陷；該用法本身不受支援。
- 命中統計的 ACT 集合遍歷已經位於 `ActionDataLock` 內，複核後無需重複加鎖或額外快照。
