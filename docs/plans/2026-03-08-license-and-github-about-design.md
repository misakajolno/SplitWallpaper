# SplitWallpaper 许可证与 GitHub 仓库简介设计

日期：2026-03-08

## 目标

为 `SplitWallpaper` 补齐公开仓库常见的许可证文件，并整理一套适合直接填写到 GitHub 仓库设置页的简介文案。

## 已确认决策

- 使用 `MIT` 许可证
- GitHub 仓库简介采用直白的功能型表述
- 项目仍保持 Windows-only 定位
- README 首页继续以中文为主，并保留英文说明页

## 设计内容

### 许可证

- 在仓库根目录新增 `LICENSE`
- 内容使用标准 `MIT License`
- 许可证覆盖仓库源码与文档，便于公开 fork、学习和二次使用

### README 调整

- 在 `README.md` 新增“许可证”小节
- 在 `README.en.md` 新增 `License` 小节
- 小节内容简洁说明项目采用 `MIT` 许可证，并指向根目录 `LICENSE`

### GitHub 仓库简介

推荐使用英文一句话，适合显示在 GitHub About 区域和搜索结果中：

`Windows app for splitting the primary desktop into left and right wallpapers.`

备选：

- `A Windows split-wallpaper tool with live preview for the primary display.`
- `Split Wallpaper is a Windows desktop app for composing two wallpapers on one screen.`

### GitHub Topics 建议

- `windows`
- `wpf`
- `dotnet`
- `wallpaper`
- `desktop-app`

## 验证方式

完成后检查以下内容：

- `LICENSE` 存在且为标准 `MIT` 文本
- `README.md` 包含“许可证”小节
- `README.en.md` 包含 `License` 小节
- 最终交付中提供一条适合直接粘贴到 GitHub 的仓库简介文案
