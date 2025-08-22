# KeyForge项目 - CI/CD配置

本目录包含KeyForge项目的持续集成和持续部署配置文件。

## 配置文件说明

### GitHub Actions
- `.github/workflows/test.yml` - 主要的测试工作流
- `.github/workflows/build.yml` - 构建和部署工作流
- `.github/workflows/quality.yml` - 代码质量检查工作流

### Azure DevOps
- `azure-pipelines.yml` - Azure DevOps构建管道
- `azure-pipelines-test.yml` - Azure DevOps测试管道

### Docker
- `Dockerfile` - 容器化配置
- `docker-compose.yml` - 容器编排配置

## 工作流概述

### 测试工作流 (test.yml)
- 触发条件：推送到main/develop分支，Pull Request
- 执行环境：Windows Latest
- 测试类别：Unit, Integration, System, UAT
- 代码覆盖率：生成覆盖率报告
- 测试报告：上传测试结果

### 构建工作流 (build.yml)
- 触发条件：测试通过后
- 构建配置：Release模式
- 代码签名：自动签名
- 部署目标：GitHub Releases, NuGet, Docker Hub

### 质量检查工作流 (quality.yml)
- 触发条件：每次提交
- 检查项目：代码规范、安全漏洞、性能分析
- 报告生成：SonarQube集成
- 质量门禁：自动质量检查

## 使用说明

### GitHub Actions
1. 确保GitHub仓库已启用Actions
2. 配置必要的secrets（API密钥、证书等）
3. 根据需要修改工作流配置

### Azure DevOps
1. 创建新的Build Pipeline
2. 选择对应的yml文件
3. 配置构建代理和变量
4. 设置触发条件和部署目标

### Docker
1. 构建镜像：`docker build -t keyforge .`
2. 运行容器：`docker run -d keyforge`
3. 使用docker-compose：`docker-compose up -d`

## 环境要求

### 构建环境
- .NET 9.0 SDK
- Windows 10/11 或 Linux
- Node.js 16+ (用于前端)
- Docker (可选)

### 测试环境
- Windows 10/11 (用于系统集成测试)
- SQL Server/SQLite (用于数据库测试)
- 浏览器 (用于UI测试)

### 部署环境
- Windows Server 2019+
- IIS 或 Docker
- SQL Server 2019+
- SSL证书 (用于HTTPS)

## 监控和报告

### 测试报告
- 测试结果：JUnit格式
- 覆盖率报告：HTML格式
- 性能报告：JSON格式
- 质量报告：SonarQube格式

### 构建报告
- 构建日志：完整构建过程
- 构建产物：可执行文件、安装包
- 部署状态：部署结果和URL
- 版本信息：版本号和变更记录

### 质量报告
- 代码质量：SonarQube报告
- 安全扫描：依赖项安全报告
- 性能分析：性能基准报告
- 合规检查：许可证合规报告

## 故障排除

### 常见问题
1. **测试失败**：检查测试环境和依赖项
2. **构建失败**：检查构建配置和代码错误
3. **部署失败**：检查部署环境和权限
4. **权限问题**：检查secrets和访问权限

### 调试步骤
1. 查看详细的构建日志
2. 在本地重现问题
3. 检查依赖项版本
4. 验证环境配置
5. 联系相关责任人

## 最佳实践

### 工作流优化
- 使用缓存加速构建
- 并行执行独立任务
- 设置适当的超时时间
- 定期清理临时文件

### 质量保证
- 设置质量门禁
- 自动化代码审查
- 定期安全扫描
- 性能回归测试

### 部署策略
- 蓝绿部署
- 金丝雀发布
- 回滚机制
- 监控告警

## 扩展配置

### 自定义步骤
1. 在相应的工作流文件中添加步骤
2. 配置必要的输入和输出
3. 设置适当的条件和依赖
4. 测试新的工作流

### 集成外部服务
1. 添加相应的API密钥
2. 配置服务连接
3. 设置通知和报告
4. 监控集成状态

## 联系方式

如有问题或建议，请联系：
- 开发团队：dev@keyforge.com
- DevOps团队：devops@keyforge.com
- QA团队：qa@keyforge.com