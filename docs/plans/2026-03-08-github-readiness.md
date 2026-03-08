# SplitWallpaper GitHub 仓库整理实施计划

日期：2026-03-08

1. 新建 `docs/plans/2026-03-08-github-readiness-design.md`
2. 新建 `docs/plans/2026-03-08-github-readiness.md`
3. 盘点仓库中需要公开保留的目录与文件
4. 更新根目录 `.gitignore`
5. 为 `.gitignore` 加入本地 SDK 与工具缓存规则
6. 为 `.gitignore` 加入 `bin/` 与 `obj/` 规则
7. 为 `.gitignore` 加入 `release/` 与 `output/` 规则
8. 为 `.gitignore` 加入编辑器和用户文件规则
9. 新建中文首页 `README.md`
10. 在中文首页加入英文文档跳转
11. 在中文首页写入环境要求
12. 在中文首页写入 Git 安装步骤
13. 在中文首页写入 `.NET 8 SDK` 安装步骤
14. 在中文首页写入构建、测试、运行命令
15. 在中文首页写入发布打包说明
16. 在中文首页写入常见问题
17. 新建英文文档 `README.en.md`
18. 在英文文档写入 prerequisites 与 quick start
19. 在英文文档写入 publish 与 FAQ
20. 修改 `scripts/publish-framework-dependent.ps1`
21. 让发布脚本优先使用本地 `.dotnet`
22. 让发布脚本回退到系统 `dotnet`
23. 为缺失 `dotnet` 的情况补充清晰报错
24. 执行 `dotnet build` 验证仓库源码可构建
25. 执行 `dotnet test` 验证现有测试通过
26. 执行发布脚本验证公开搭建流程闭环
