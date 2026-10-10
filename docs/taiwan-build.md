# Dalamud ACT Compat 台服独立构建

台服在独立仓库 [DalamudActCompat-TW](https://github.com/JackyWilliam/DalamudActCompat-TW) 的 `main` 维护，使用台服 API 13 / .NET 9 依赖链。当前产物为待游戏内验收的候选包，不向现有国服订阅源发布。

## 范围

保留战斗解析与统计、历史、Cactbot、HTML 悬浮窗与快捷键、PictoACT 绘图、Host、Triggernometry、PostNamazu、FoxTTS、SilverDasher、自行导入兼容扩展、账号与云同步、好友与聊天、皮肤及六种界面语言。按用户要求，未找到台服适配的 Matcha 不随包提供，也不会从旧配置自动启用。

默认界面为简体中文，首次启动和恢复默认设置均使用简体；已保存的语言选择仍保留。解析区固定台服，导入旧配置不能改变底层区域。FFLogs 采用台服当前中量级 M5–M8 地图表及 TW 查询范围；缺少台服样本时不借用 CN/Global 缓存。在线样本是否可用仍取决于 FFLogs 服务。

## 来源与构建

- DACT 修改后的 IINACT 独立保存在 [IINACT-TW](https://github.com/JackyWilliam/IINACT-TW)，子模块按提交固定，不依赖原 DACT/IINACT 仓库的功能分支。
- 台服 IINACT：PlusoneChiang/IINACT `tc/api13_net9`，起点 `0788c177c5df622c9e20c4f00027b873732b5606`。
- Machina：`3cc6e1c50c28ea81d95a05e2b4bf4329ffac0b44`。
- Dalamud：yanmucorp 13.0.0.16，游戏版本 `2026.09.14.0000.0000`。
- FFXIV_ACT_Plugin 3.0.3.1、Unscrambler 7.41.1；完整来源与 SHA-256 见 `tools/taiwan-dependencies.lock.json`。
- DACT 界面、嵌入式 ACT 与 Host 功能在台服上游分支中重新合入，原工作目录保持不变。

安装 .NET SDK 10（用于 slnx）、.NET 9 Windows Desktop 运行库与 7-Zip，并递归检出子模块：

```powershell
git clone --recurse-submodules https://github.com/JackyWilliam/DalamudActCompat-TW.git
cd DalamudActCompat-TW
./tools/build-taiwan.ps1
# 使用已下载的同版本引用：
./tools/build-taiwan.ps1 -ReferenceDirectory 'X:/tw-dalamud'
```

脚本验证引用哈希、构建完整解决方案、运行离线 smoke，并生成 `artifacts/taiwan/DalamudActCompat-TW-candidate.zip` 与 SHA-256。API 13 模块和 Host 均面向 .NET 9；打包工具版本不代表游戏运行时 API。

`repo/pluginmaster.json` 暂为空，避免候选版误导向国服安装包。正式台服订阅需在游戏内验收后单独发布。历史国服版本修复工具不会从台服仓库分发，它的内置 API 15 修复包不适用于台服。

## 验证边界

离线覆盖真实 ACT/Triggernometry/SilverDasher 加载、台服报文布局与操作码、Host 传递、Cactbot 配置、快捷键、绘图回放与加速循环，以及皮肤/语言原生布局矩阵。Host 测试每次复制独立扩展目录，避免 Triggernometry 独占语言 XML 导致并行测试弹窗。

没有把离线成功当作游戏内验收。仍需台服实际客户端验证封包 hook、原生 VFX、24 人队伍布局与 SilverDasher FateInfo 动态识别。FateInfo 读取以三个已发布台服操作码交叉校验，失败时保留未知值并记录日志，不猜用其他区操作码。FFLogs 真实 TW 在线数据也未做账户授权查询验证。

版权与许可见 `LICENSE.md`、`THIRD_PARTY_NOTICES.md` 及包内 `LICENSES/`。
