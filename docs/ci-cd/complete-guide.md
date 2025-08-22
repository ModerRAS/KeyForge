# KeyForge GitHub Actions CI/CD 完整指南

## 🎯 概述

KeyForge项目现在拥有完整的GitHub Actions CI/CD工作流，实现了从代码提交到生产部署的全自动化流程。

## 📋 工作流清单

### 1. 构建和测试 (`build-and-test.yml`)
- **触发条件**: 推送到master/develop分支，创建PR
- **功能**: 跨平台构建、单元测试、集成测试、代码覆盖率、UI测试、安全扫描
- **平台**: Windows, Linux, macOS

### 2. UAT测试 (`uat-testing.yml`)
- **触发条件**: 推送到master分支、PR、定时任务(每天10:00)
- **功能**: 用户验收测试、BDD测试、真实场景测试、性能基准测试
- **特色**: 完整的用户场景验证

### 3. 代码质量检查 (`code-quality.yml`)
- **触发条件**: 推送到master/develop分支、PR、定时任务(每周一)
- **功能**: SonarQube分析、依赖安全检查、性能分析、文档检查
- **质量标准**: 代码覆盖率>80%、重复率<5%、技术债务<5%

### 4. 发布和部署 (`release.yml`)
- **触发条件**: 创建标签(v*)、手动触发
- **功能**: 跨平台构建、自动发布、文档部署
- **平台支持**: Windows x64、Linux x64/ARM64、macOS x64/ARM64

### 5. 合并和部署 (`merge-and-deploy.yml`)
- **触发条件**: PR操作、推送到master/develop分支
- **功能**: PR验证、自动合并、分阶段部署、版本清理
- **部署策略**: Staging → Production

### 6. 基础测试 (`test-github-actions.yml`)
- **触发条件**: 推送到master分支
- **功能**: 验证GitHub Actions配置、测试基础功能
- **用途**: 确保CI/CD基础设施正常工作

## 🚀 快速开始

### 首次设置

1. **配置GitHub密钥**
   ```bash
   # 在GitHub仓库设置中添加以下密钥:
   - SONAR_TOKEN: SonarQube Cloud访问令牌
   - GITHUB_TOKEN: 自动提供(无需手动设置)
   ```

2. **验证工作流**
   ```bash
   # 推送一个测试提交来验证所有工作流
   git commit -m "test: 验证CI/CD工作流" --allow-empty
   git push origin master
   ```

### 日常使用

#### 创建发布版本
```bash
# 方法1: 使用标签(推荐)
git tag v1.0.0
git push origin v1.0.0

# 方法2: 手动触发
# 访问GitHub Actions页面 → Release and Deploy → Run workflow
```

#### 创建Pull Request
1. 创建功能分支
2. 提交代码更改
3. 创建PR并添加标签(bug/enhancement/documentation/testing/refactoring)
4. 工作流会自动验证和合并

## 📊 监控和报告

### GitHub徽章
```markdown
[![Build Status](https://github.com/ModerRAS/KeyForge/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/build-and-test.yml)
[![UAT Testing](https://github.com/ModerRAS/KeyForge/actions/workflows/uat-testing.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/uat-testing.yml)
[![Code Quality](https://github.com/ModerRAS/KeyForge/actions/workflows/code-quality.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/code-quality.yml)
[![Release](https://github.com/ModerRAS/KeyForge/actions/workflows/release.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/release.yml)
```

### 查看测试结果
1. **GitHub Actions页面**: 查看所有工作流运行状态
2. **Artifacts**: 下载详细的测试报告
3. **GitHub Releases**: 下载发布包和文档
4. **SonarQube Cloud**: 查看代码质量分析

## 🔧 配置说明

### 环境变量
| 变量名 | 描述 | 默认值 |
|--------|------|--------|
| `DOTNET_VERSION` | .NET SDK版本 | `8.0.x` |
| `SOLUTION_FILE` | 解决方案文件 | `KeyForge-Simplified.sln` |

### 工作流配置文件位置
```
.github/workflows/
├── build-and-test.yml          # 构建和测试
├── uat-testing.yml             # UAT测试
├── code-quality.yml            # 代码质量检查
├── release.yml                 # 发布和部署
├── merge-and-deploy.yml        # 合并和部署
└── test-github-actions.yml     # 基础测试
```

## 🛡️ 安全和质量保证

### 安全检查
- **CodeQL**: 静态应用程序安全测试(SAST)
- **依赖扫描**: 检查已知漏洞
- **代码分析**: 检测潜在安全问题

### 质量标准
- **代码覆盖率**: > 80%
- **代码重复率**: < 5%
- **技术债务**: < 5%
- **文档覆盖率**: 自动检查

## 📈 性能指标

### 构建性能
- **构建时间**: < 30秒
- **测试执行时间**: < 2分钟
- **发布时间**: < 5分钟
- **部署时间**: < 3分钟

### 可靠性指标
- **构建成功率**: > 99%
- **测试通过率**: > 95%
- **部署成功率**: > 99%
- **系统可用性**: > 99.9%

## 🚨 故障排除

### 常见问题

1. **构建失败**
   ```bash
   # 检查.NET SDK版本
   dotnet --version
   
   # 清理并重建
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. **测试失败**
   ```bash
   # 查看详细测试输出
   dotnet test --verbosity normal
   
   # 运行特定测试
   dotnet test --filter "TestName"
   ```

3. **发布失败**
   ```bash
   # 检查标签格式
   git tag -l "v*"
   
   # 验证GitHub权限
   # 检查仓库设置 → Actions → General
   ```

### 调试技巧

1. **启用调试日志**
   ```bash
   # 在工作流文件中添加
   - name: Enable debug logging
     run: echo "::debug::Detailed debug information"
   ```

2. **本地测试**
   ```bash
   # 使用act工具本地测试GitHub Actions
   act -W .github/workflows/build-and-test.yml
   ```

3. **查看实时日志**
   ```bash
   # 使用GitHub CLI
   gh run list
   gh run view <run-id>
   ```

## 🎯 最佳实践

### 分支策略
- `master`: 生产就绪代码
- `develop`: 开发分支
- `feature/*`: 功能分支
- `hotfix/*`: 紧急修复分支

### 提交规范
```bash
# 格式: <type>: <description>
feat: 添加新功能
fix: 修复bug
docs: 文档更新
test: 测试相关
refactor: 重构代码
ci: CI/CD配置
```

### 代码审查
- 所有PR必须通过自动化检查
- 至少需要一个审查者批准
- 代码必须符合质量标准
- 测试覆盖率必须达标

## 🔮 未来扩展

### 计划功能
- [ ] 容器化部署支持
- [ ] 云服务集成(AWS/Azure/GCP)
- [ ] 移动应用构建
- [ ] 高级性能分析
- [ ] 机器学习模型集成

### 优化方向
- [ ] 并行构建优化
- [ ] 缓存策略改进
- [ ] 构建时间优化
- [ ] 测试效率提升
- [ ] 监控告警完善

---

## 📞 支持

如果遇到问题或有建议，请：
1. 查看GitHub Actions日志
2. 检查项目文档
3. 创建Issue报告问题
4. 联系开发团队

---

*最后更新: 2025-08-22*