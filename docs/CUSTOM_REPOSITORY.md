# 台服插件訂閱與獨立倉庫

- 原始碼：[DalamudActCompat-TW](https://github.com/JackyWilliam/DalamudActCompat-TW)，預設分支 `main`。
- 修改後的解析依賴：[IINACT-TW](https://github.com/JackyWilliam/IINACT-TW)，透過子模組固定提交。
- 構建：[Taiwan Build](https://github.com/JackyWilliam/DalamudActCompat-TW/actions/workflows/build.yml)，遞迴克隆、校驗台服 API 13 引用並執行完整離線迴歸。

## 安裝

在 Dalamud 的 `/xlsettings` → 實驗性功能 → 自訂插件倉庫中加入以下網址，儲存後開啟 `/xlplugins` 重新整理並安裝 **Dalamud ACT Compat (TW)**：

```text
https://raw.githubusercontent.com/JackyWilliam/DalamudActCompat-TW/main/repo/pluginmaster.json
```

目前訂閱提供 **0.4.5.15 測試候選版**，預設繁體中文，對應台服 API 13 與遊戲版本 `2026.09.14.0000.0000`。遊戲內相容性仍待驗收；不包含 Matcha。安裝前請先備份原有設定，勿同時載入國服與台服版本。

插件版本與遊戲版本分開處理：登入服務端時，台服用戶端會回報 `TW`、插件版本及正在執行的遊戲目錄內 `ffxivgame.ver` 的版本。讀取不到遊戲版本時不會猜測，也不會阻止登入。管理後台顯示最近一次有效回報；舊用戶端未提供的區域或遊戲版本顯示為「未上報」。

## 維護訂閱

1. 等待 Taiwan Build 完整驗證成功，下載該提交的候選產物，核對 SHA-256 與套件清單。
2. 以套件實際版本建立 GitHub 預先發行版本（目前為 `v0.4.5.15`），上傳 `DalamudActCompat-TW-candidate.zip` 及校驗檔。
3. 確認未登入使用者也能下載並通過校驗，再產生及提交訂閱清單：

```powershell
./tools/update-taiwan-pluginmaster.ps1 -Package ./DalamudActCompat-TW-candidate.zip -ReleaseTag v0.4.5.15 -Changelog "本版更新說明"
```

工具會拒絕非台服套件、不同 API／遊戲相容版本、含 Matcha 的套件，以及與套件版本不一致的標籤。更新時須先提高實際插件版本，不能以虛構的訂閱版本代替。訂閱格式參見 [Dalamud 自訂倉庫文件](https://dalamud.dev/plugin-publishing/custom-repositories/)。

手動建置與獨立測試目錄載入方式請參閱 [台服建置說明](taiwan-build.md)。

國服訂閱、舊版修復工具和歷史釋出流程不適用於台服。`tools/collect-release.ps1`、`tools/update-pluginmaster.ps1` 與舊修復工具構建入口在台服倉庫中禁用；手動 Taiwan Candidate 工作流只生成候選產物。
