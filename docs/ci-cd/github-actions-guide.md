# GitHub Actions 工作流

这个项目使用GitHub Actions进行自动化构建、测试和部署。

## 工作流概述

### 1. 构建和测试 (`build-and-test.yml`)

**触发条件：**
- 推送到 `master` 或 `develop` 分支
- 创建 Pull Request

**功能：**
- 跨平台构建 (Windows, Linux, macOS)
- 单元测试和集成测试
- 代码覆盖率报告
- 安全扫描 (CodeQL)
- UI测试 (仅Windows)

### 2. UAT测试 (`uat-testing.yml`)

**触发条件：**
- 推送到 `master` 分支
- Pull Request
- 定时任务 (每天北京时间10:00)

**功能：**
- 用户验收测试
- BDD风格测试
- 真实场景测试
- 边界条件测试
- 性能基准测试
- 安全测试

### 3. 代码质量检查 (`code-quality.yml`)

**触发条件：**
- 推送到 `master` 或 `develop` 分支
- Pull Request
- 定时任务 (每周一)

**功能：**
- SonarQube代码分析
- 依赖安全检查
- 性能分析
- 文档检查

### 4. 发布和部署 (`release.yml`)

**触发条件：**
- 创建标签 (v*)
- 手动触发

**功能：**
- 跨平台构建和打包
- 创建GitHub Release
- 自动生成发布说明
- 部署文档到GitHub Pages

### 5. 合并和部署 (`merge-and-deploy.yml`)

**触发条件：**
- Pull Request操作
- 推送到 `master` 或 `develop` 分支

**功能：**
- PR验证和自动合并
- 分阶段部署 (staging/production)
- 清理旧版本

## 使用方法

### 创建新版本

1. **方法一：创建标签**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **方法二：手动触发**
   - 访问GitHub Actions页面
   - 选择 "Release and Deploy" 工作流
   - 点击 "Run workflow"
   - 选择发布类型和是否为预发布版本

### 创建Pull Request

1. 确保PR标题符合语义化格式
2. 添加适当的标签 (bug, enhancement, documentation, testing, refactoring)
3. 工作流会自动验证和合并

### 查看测试结果

1. **GitHub Actions页面**
   - 查看所有工作流的运行状态
   - 下载测试报告和构建产物

2. **GitHub Releases**
   - 下载各平台的二进制文件
   - 查看发布说明和文档

## 支持的平台

- **Windows**: x64
- **Linux**: x64, ARM64
- **macOS**: x64, ARM64

## 环境变量

| 变量名 | 描述 | 默认值 |
|--------|------|--------|
| `DOTNET_VERSION` | .NET SDK版本 | `8.0.x` |
| `SOLUTION_FILE` | 解决方案文件 | `KeyForge-Simplified.sln` |

## 密钥配置

项目需要以下GitHub密钥：

- `SONAR_TOKEN`: SonarQube Cloud访问令牌
- `GITHUB_TOKEN`: GitHub API访问令牌 (自动提供)

## 工作流状态

| 工作流 | 状态 | 描述 |
|--------|------|------|
| Build and Test | ✅ | 构建和测试 |
| UAT Testing | ✅ | 用户验收测试 |
| Code Quality | ✅ | 代码质量检查 |
| Release and Deploy | ✅ | 发布和部署 |
| Merge and Deploy | ✅ | 合并和部署 |

## 故障排除

### 常见问题

1. **构建失败**
   - 检查.NET SDK版本是否正确
   - 确认所有依赖项都已恢复

2. **测试失败**
   - 查看测试报告了解详细信息
   - 检查测试环境配置

3. **发布失败**
   - 确认标签格式正确 (v*)
   - 检查GitHub权限设置

### 日志和报告

所有工作流都会生成详细的报告和日志：

- **测试结果**: 在Artifacts中下载
- **代码质量报告**: SonarQube Cloud
- **构建日志**: GitHub Actions页面
- **发布包**: GitHub Releases

## 最佳实践

1. **分支策略**
   - `master`: 生产就绪代码
   - `develop`: 开发分支
   - `feature/*`: 功能分支

2. **提交规范**
   - 使用语义化提交信息
   - 确保所有测试通过
   - 遵循代码质量标准

3. **发布流程**
   - 版本号使用语义化版本控制
   - 创建发布标签时附带发布说明
   - 确保所有平台构建成功

## 监控和维护

- 定期检查工作流状态
- 更新依赖项和工具版本
- 清理旧的构建产物和版本
- 监控安全漏洞和性能指标