# Split Wallpaper

[English README](README.en.md)

一个仅支持 Windows 的桌面壁纸拼接工具，用来把主显示器画面拆成左右两部分，并分别应用不同壁纸。

## 功能特性

- 主显示器分屏壁纸：将桌面按左右区域拆分并分别设置图片
- 实时预览：按照主显示器分辨率比例预览最终效果
- 多种填充模式：适配不同尺寸与比例的图片素材
- 自定义分割比例：支持拖动调整左右区域占比
- Windows 桌面应用：基于 `.NET 8` 与 `WPF`

## 环境要求

- Windows 10 或 Windows 11
- Git
- `.NET 8 SDK`
- 可选：`VS Code` 或任意支持 C# 的编辑器

## 安装 .NET 8 SDK

推荐方式：

```powershell
winget install Microsoft.DotNet.SDK.8
```

安装完成后验证：

```powershell
dotnet --version
```

如果命令能输出 `8.x` 版本号，说明 SDK 已安装成功。

也可以从微软官方页面下载安装：

- `.NET 8 下载页： https://dotnet.microsoft.com/download/dotnet/8.0`
- `Windows 安装说明： https://learn.microsoft.com/dotnet/core/install/windows`

说明：

- 只要安装了 `.NET 8 SDK`，就不需要额外安装开发时使用的运行时
- 如果你只是想运行发布后的 `framework-dependent` 版本，则目标机器需要 `.NET Desktop Runtime 8`

## 快速开始

克隆仓库：

```powershell
git clone <your-repo-url>
cd SplitWallpaper
```

还原依赖：

```powershell
dotnet restore
```

构建项目：

```powershell
dotnet build
```

运行测试：

```powershell
dotnet test
```

启动应用：

```powershell
dotnet run --project .\src\SplitWallpaper.App\SplitWallpaper.App.csproj
```

## 发布打包

生成 `framework-dependent` 发布包：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-framework-dependent.ps1
```

发布产物输出到：

```text
release\framework-dependent
```

如果你把发布产物分发给其他用户，而目标机器没有安装 SDK，请安装 `.NET Desktop Runtime 8`：

```powershell
winget install Microsoft.DotNet.DesktopRuntime.8
```

## 项目结构

```text
SplitWallpaper
├─ src/                         应用与核心库源码
│  ├─ SplitWallpaper.App/
│  └─ SplitWallpaper.Core/
├─ tests/                       单元测试与应用层测试
├─ scripts/                     发布脚本
├─ README.md                    中文首页
├─ README.en.md                 英文文档
└─ SplitWallpaper.sln           解决方案文件
```

## 常见问题

### 必须使用 Visual Studio 吗？

不需要。安装 `.NET 8 SDK` 后，使用 PowerShell、命令提示符或 `VS Code` 就可以完成构建、测试、运行与发布。

### 为什么这个项目只支持 Windows？

应用当前使用 `WPF` 构建，并且只处理 Windows 桌面壁纸设置流程，因此不支持 macOS 或 Linux。

### 为什么仓库里没有 `.dotnet`、`.tooling`、`release` 这些目录？

这些目录属于本地 SDK、工具缓存或构建产物，不适合提交到 GitHub 源码仓库，因此默认被 `.gitignore` 忽略。

### 发布脚本为什么还能用？

发布脚本会优先使用仓库内的本地 `.dotnet`，如果不存在，则自动回退到系统环境中的 `dotnet`。

## 英文文档

如果你希望查看英文说明，请点击：

- `README.en.md`

## 许可证

本项目基于 `MIT` 许可证发布，详情请参阅根目录中的 `LICENSE`。
