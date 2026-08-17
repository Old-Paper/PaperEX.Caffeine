# PaperEX的咖啡因 (PaperEX.Caffeine)

极简 Windows 防休眠托盘工具：.NET 8 + WinForms，无主窗口、无任务栏图标，仅驻留系统托盘。

## 下载

免安装单文件版本（win-x64 自包含，普通用户权限即可运行）：

<https://github.com/Old-Paper/PaperEX.Caffeine/releases>

## 功能

- 启动即自动进入「不休眠 + 屏幕常亮」模式；
- 托盘右键菜单可切换「不休眠 + 允许屏幕关闭」；
- 单实例运行；退出时恢复 Windows 正常电源行为。

实现原理：P/Invoke 调用 `kernel32.dll` 的 `SetThreadExecutionState`。
不模拟键鼠、不修改电源计划与注册表、不需要管理员权限、不联网、无遥测。

## 构建与发布

```bash
dotnet restore
dotnet build -c Release
dotnet publish -c Release -o dist
```

产物：`dist/PaperEX.Caffeine.exe`（win-x64、自包含、单文件，可直接发给别人运行）。
exe 可安全重命名（例如 `PaperEX的咖啡因.exe`）。

## 替换图标

替换 `Resources/app.ico` 后重新构建发布即可（托盘图标与 exe 文件图标共用该文件）。
