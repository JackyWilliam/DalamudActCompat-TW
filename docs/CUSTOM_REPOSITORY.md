# 台服獨立倉庫與候選構建

- 原始碼：[DalamudActCompat-TW](https://github.com/JackyWilliam/DalamudActCompat-TW)，預設分支 `main`。
- 修改後的解析依賴：[IINACT-TW](https://github.com/JackyWilliam/IINACT-TW)，透過子模組固定提交。
- 構建：[Taiwan Build](https://github.com/JackyWilliam/DalamudActCompat-TW/actions/workflows/build.yml)，遞迴克隆、校驗台服 API 13 引用並執行完整離線迴歸。

當前只有候選包，正式台服訂閱尚未釋出，`repo/pluginmaster.json` 保持空陣列。請按 [台服構建說明](taiwan-build.md) 在獨立測試目錄載入候選。

國服訂閱、舊版修復工具和歷史釋出流程不適用於台服。`tools/collect-release.ps1`、`tools/update-pluginmaster.ps1` 與舊修復工具構建入口在台服倉庫中禁用；手動 Taiwan Candidate 工作流只生成候選產物。
