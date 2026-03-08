# SplitWallpaper GitHub 仓库整理设计

日期：2026-03-08

## 目标

将 `SplitWallpaper` 整理成适合公开提交到 GitHub 的源码仓库，同时保证普通访客在 Windows 环境下可以按照 README 完成环境安装、构建、测试、运行与发布。

## 已确认决策

- 仓库保持轻量，只提交源码、测试、脚本与必要文档
- 不提交本地 SDK、工具缓存、构建输出与发布产物
- 首页 README 使用中文，并提供英文文档跳转
- 英文文档单独维护为 `README.en.md`
- 搭建流程面向第一次访问项目的 GitHub 访客
- 文档中必须包含 `.NET 8 SDK` 的安装方式
- 项目继续定位为 Windows-only 桌面应用，仅支持主显示器

## 仓库范围

### 保留内容

- `src/`
- `tests/`
- `scripts/`
- `SplitWallpaper.sln`
- `README.md`
- `README.en.md`
- `.gitignore`
- 必要的项目资源文件，例如应用图标

### 忽略内容

- `.dotnet/`
- `.tooling/`
- `release/`
- `output/`
- `**/bin/`
- `**/obj/`
- `.vs/`
- `.vscode/`
- 测试输出与常见本地用户文件

## README 设计

### 中文首页

`README.md` 作为 GitHub 首页，覆盖以下内容：

- 项目简介
- 功能特性
- 环境要求
- Git 与 `.NET 8 SDK` 安装方式
- 快速开始
- 运行测试
- 启动应用
- 发布打包
- 项目结构
- 常见问题
- 英文文档跳转

### 英文文档

`README.en.md` 提供与中文文档一致的核心信息，但表达更紧凑，重点保证海外访客能完成：

- prerequisites
- clone
- restore
- build
- test
- run
- publish

## 脚本调整

现有发布脚本依赖仓库内的 `.dotnet/dotnet.exe`，不适合公开源码仓库。脚本需要调整为：

- 优先使用本地 `.dotnet/dotnet.exe`（兼容当前本地工作流）
- 若不存在，则回退使用系统 `dotnet`
- 若系统 `dotnet` 也不存在，给出明确安装提示

## 验证方式

变更完成后，在仓库根目录执行以下命令验证：

- `dotnet restore`
- `dotnet build`
- `dotnet test`
- `powershell -ExecutionPolicy Bypass -File .\scripts\publish-framework-dependent.ps1`

验证重点：

- 新访客不依赖 `.dotnet/` 也能构建项目
- README 中的命令可直接执行
- 发布脚本在系统安装 `.NET 8 SDK` 后可正常工作
