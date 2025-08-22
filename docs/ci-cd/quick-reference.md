# KeyForge GitHub Actions 快速参考卡

## 🚀 常用命令

### 创建发布版本
```bash
# 创建标签并推送
git tag v1.0.0
git push origin v1.0.0
```

### 验证工作流
```bash
# 触发所有工作流
git commit -m "test: 验证CI/CD" --allow-empty
git push origin master
```

### 查看状态
```bash
# 使用GitHub CLI
gh run list
gh run view <run-id>
gh workflow list
```

## 📋 工作流触发条件

| 工作流 | 触发条件 |
|--------|----------|
| build-and-test.yml | push to master/develop, PR |
| uat-testing.yml | push to master, PR, daily 10:00 |
| code-quality.yml | push to master/develop, PR, weekly |
| release.yml | tag v*, manual |
| merge-and-deploy.yml | PR, push to master/develop |
| test-github-actions.yml | push to master |

## 🏷️ PR标签

必需标签（至少一个）：
- `bug` - 错误修复
- `enhancement` - 功能增强
- `documentation` - 文档更新
- `testing` - 测试相关
- `refactoring` - 重构

## 📊 质量标准

- 代码覆盖率 > 80%
- 代码重复率 < 5%
- 技术债务 < 5%
- 构建时间 < 30秒
- 测试通过率 > 95%

## 🔧 配置文件位置

```
.github/workflows/
├── build-and-test.yml
├── uat-testing.yml
├── code-quality.yml
├── release.yml
├── merge-and-deploy.yml
└── test-github-actions.yml
```

## 🌐 重要链接

- **GitHub Actions**: `https://github.com/ModerRAS/KeyForge/actions`
- **GitHub Releases**: `https://github.com/ModerRAS/KeyForge/releases`
- **SonarQube**: `https://sonarcloud.io/project/overview?id=ModerRAS_KeyForge`

## 🚨 故障排除

### 常见问题
1. **构建失败**: 检查.NET版本和依赖
2. **测试失败**: 查看测试报告和日志
3. **发布失败**: 验证标签格式和权限

### 调试命令
```bash
# 本地测试
dotnet build
dotnet test

# 清理重建
dotnet clean
dotnet restore
dotnet build
```

---

*保存此卡片以便快速参考*