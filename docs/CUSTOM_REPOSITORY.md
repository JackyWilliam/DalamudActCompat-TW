# 台服独立仓库与候选构建

- 源码：[DalamudActCompat-TW](https://github.com/JackyWilliam/DalamudActCompat-TW)，默认分支 `main`。
- 修改后的解析依赖：[IINACT-TW](https://github.com/JackyWilliam/IINACT-TW)，通过子模块固定提交。
- 构建：[Taiwan Build](https://github.com/JackyWilliam/DalamudActCompat-TW/actions/workflows/build.yml)，递归克隆、校验台服 API 13 引用并执行完整离线回归。

当前只有候选包，正式台服订阅尚未发布，`repo/pluginmaster.json` 保持空数组。请按 [台服构建说明](taiwan-build.md) 在独立测试目录加载候选。

国服订阅、旧版修复工具和历史发布流程不适用于台服。`tools/collect-release.ps1`、`tools/update-pluginmaster.ps1` 与旧修复工具构建入口在台服仓库中禁用；手动 Taiwan Candidate 工作流只生成候选产物。
