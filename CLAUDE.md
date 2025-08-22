# KeyForge 项目文件存放规范

## 📁 项目目录结构说明

本文档规定了KeyForge项目中各种文件类型的标准存放位置，确保项目结构清晰、易于维护。

## 🏗️ 标准目录结构

### 根目录文件
```
KeyForge/
├── README.md                          # 项目主文档
├── LICENSE                            # 开源许可证
├── CLAUDE.md                          # 文件存放规范(本文件)
├── KeyForge.sln                       # Visual Studio 解决方案
├── KeyForge-Simplified.sln            # 简化版解决方案
├── global.json                        # .NET SDK 版本配置
├── .gitignore                         # Git 忽略文件
├── azure-pipelines.yml                # Azure Pipelines 配置
└── docs/                              # 文档目录
```

### 源代码目录
```
├── KeyForge.Core/                     # 核心业务逻辑
│   ├── Domain/                        # 领域模型
│   │   ├── Aggregates/                # 聚合根
│   │   ├── Entities/                  # 实体
│   │   ├── ValueObjects/              # 值对象
│   │   ├── Events/                    # 领域事件
│   │   ├── Exceptions/                # 领域异常
│   │   ├── Interfaces/                # 领域接口
│   │   ├── Services/                  # 领域服务
│   │   └── Common/                    # 通用类型
│   ├── Application/                   # 应用层
│   │   ├── Commands/                  # 命令
│   │   ├── Queries/                   # 查询
│   │   ├── DTOs/                      # 数据传输对象
│   │   ├── Interfaces/                # 应用接口
│   │   ├── Services/                  # 应用服务
│   │   └── Exceptions/                # 应用异常
│   └── Interfaces/                    # 核心接口
├── KeyForge.Infrastructure/           # 基础设施层
│   ├── Persistence/                    # 数据持久化
│   │   ├── Repositories/              # 仓储实现
│   │   ├── Contexts/                  # 数据上下文
│   │   └── Migrations/                # 数据迁移
│   ├── External/                      # 外部服务
│   │   ├── Imaging/                   # 图像处理
│   │   ├── Input/                     # 输入处理
│   │   └── Logging/                   # 日志服务
│   ├── Configuration/                 # 配置管理
│   ├── Caching/                       # 缓存服务
│   └── Messaging/                     # 消息服务
├── KeyForge.Application/              # 应用服务层
│   ├── Services/                      # 应用服务
│   ├── Commands/                      # 命令处理器
│   ├── Queries/                       # 查询处理器
│   ├── DTOs/                          # 数据传输对象
│   ├── Interfaces/                    # 服务接口
│   └── Exceptions/                    # 异常处理
├── KeyForge.Presentation/             # 表现层
│   ├── Controllers/                   # 控制器
│   ├── Views/                         # 视图
│   ├── Models/                        # 视图模型
│   ├── Services/                      # 表现服务
│   └── Interfaces/                    # 表现接口
├── KeyForge.UI/                       # 用户界面
│   ├── Forms/                         # 窗体
│   ├── Controls/                      # 控件
│   ├── Resources/                     # 资源文件
│   ├── Themes/                        # 主题文件
│   └── appsettings.json               # 应用配置
├── KeyForge.HAL/                      # 硬件抽象层
│   ├── Abstractions/                  # 抽象接口
│   ├── Implementation/                # 具体实现
│   │   ├── Windows/                   # Windows 实现
│   │   ├── Linux/                     # Linux 实现
│   │   └── macOS/                     # macOS 实现
│   ├── Services/                      # HAL 服务
│   ├── Models/                        # HAL 模型
│   └── Exceptions/                    # HAL 异常
└── KeyForge.Tests/                    # 测试项目
    ├── UnitTests/                     # 单元测试
    ├── IntegrationTests/              # 集成测试
    ├── EndToEndTests/                 # 端到端测试
    ├── PerformanceTests/              # 性能测试
    ├── SecurityTests/                 # 安全测试
    ├── UAT/                           # 用户验收测试
    ├── TestFramework/                 # 测试框架
    ├── TestData/                      # 测试数据
    └── TestResults/                   # 测试结果
```

### 配置和脚本文件
```
├── .github/                           # GitHub 配置
│   ├── workflows/                     # GitHub Actions 工作流
│   │   ├── build-and-test.yml        # 构建和测试
│   │   ├── uat-testing.yml           # UAT 测试
│   │   ├── code-quality.yml          # 代码质量
│   │   ├── release.yml               # 发布和部署
│   │   ├── merge-and-deploy.yml      # 合并和部署
│   │   └── test-github-actions.yml   # 基础测试
│   ├── ISSUE_TEMPLATE/               # Issue 模板
│   └── PULL_REQUEST_TEMPLATE/        # PR 模板
├── scripts/                           # 脚本文件
│   ├── build.sh                       # 构建脚本
│   ├── test.sh                        # 测试脚本
│   ├── deploy.sh                      # 部署脚本
│   └── validate-github-actions.sh     # 验证脚本
├── config/                            # 配置文件
│   ├── appsettings.json               # 应用配置
│   ├── appsettings.Development.json   # 开发配置
│   ├── appsettings.Production.json    # 生产配置
│   └── logging.json                   # 日志配置
└── tools/                             # 工具配置
    ├── dotnet-tools.json              # .NET 工具配置
    └── code-analysis.json             # 代码分析配置
```

### 文档目录
```
├── docs/                              # 文档根目录
│   ├── README.md                      # 文档导航
│   ├── CLAUDE.md                      # 文档管理规范
│   ├── architecture/                  # 架构文档
│   ├── development/                   # 开发文档
│   ├── deployment/                    # 部署文档
│   ├── user-guide/                    # 用户指南
│   ├── reference/                     # 参考资料
│   └── images/                        # 文档图片
└── reports/                           # 报告目录
    ├── test-reports/                  # 测试报告
    ├── coverage-reports/              # 覆盖率报告
    ├── quality-reports/               # 质量报告
    └── performance-reports/          # 性能报告
```

## 📋 文件类型存放规则

### 1. 源代码文件
- **C# 文件**: `.cs` 文件放在对应的项目目录中
- **配置文件**: `.json`, `.xml`, `.config` 放在 `config/` 目录
- **资源文件**: `.resx`, `.png`, `.ico` 放在项目 `Resources/` 目录

### 2. 文档文件
- **Markdown 文档**: `.md` 文件放在 `docs/` 对应子目录
- **架构图**: `.png`, `.svg`, `.drawio` 放在 `docs/images/` 目录
- **PDF 文档**: `.pdf` 文件放在 `docs/reference/` 目录

### 3. 测试文件
- **测试代码**: 放在对应测试项目的子目录
- **测试数据**: 放在 `TestData/` 目录
- **测试结果**: 放在 `TestResults/` 目录

### 4. 构建和部署文件
- **CI/CD 配置**: 放在 `.github/workflows/` 目录
- **构建脚本**: 放在 `scripts/` 目录
- **部署配置**: 放在 `config/deployment/` 目录

### 5. 项目管理文件
- **README 文件**: `README.md` 放在根目录和各子项目目录
- **许可证文件**: `LICENSE` 放在根目录
- **变更日志**: `CHANGELOG.md` 放在根目录

## 🏷️ 命名规范

### 目录命名
- 使用kebab-case: `user-guide`, `code-quality`
- 复数形式: `tests`, `images`, `documents`
- 缩写全大写: `HAL`, `UI`, `DTO`

### 文件命名
- C# 类文件: `PascalCase.cs` - `UserService.cs`
- 配置文件: `kebab-case.json` - `appsettings.json`
- 文档文件: `kebab-case.md` - `user-guide.md`
- 脚本文件: `kebab-case.sh` - `build-script.sh`

### 项目命名
- 主项目: `KeyForge.[Layer].csproj`
- 测试项目: `KeyForge.[Layer].Tests.csproj`
- 示例项目: `KeyForge.Example.[Name].csproj`

## 📦 依赖关系管理

### 项目引用
- **正向依赖**: Core → Application → Infrastructure → Presentation
- **测试依赖**: 测试项目引用被测试项目
- **接口分离**: 通过接口层实现解耦

### 包引用
- **版本管理**: 使用 `Directory.Build.props` 统一版本
- **包源**: 配置 NuGet 包源
- **私有包**: 使用本地或私有 NuGet 源

## 🔧 开发工具配置

### Visual Studio 配置
- `.vs/` 目录 (gitignore)
- `*.suo` 文件 (gitignore)
- `*.user` 文件 (gitignore)

### VS Code 配置
- `.vscode/` 目录
- `settings.json`
- `launch.json`
- `tasks.json`

### Rider 配置
- `.idea/` 目录 (gitignore)
- `*.iml` 文件 (gitignore)

## 🚀 构建和部署

### 构建配置
- 输出目录: `bin/`, `obj/`
- 发布目录: `publish/`
- 包目录: `packages/`

### 部署配置
- 环境配置: `config/environments/`
- 部署脚本: `scripts/deploy/`
- 监控配置: `config/monitoring/`

## 📊 质量保证

### 代码分析
- 规则集: `tools/code-analysis/`
- 风格配置: `.editorconfig`
- SonarQube: `sonar-project.properties`

### 测试配置
- 覆盖率: `config/coverage/`
- 性能测试: `config/performance/`
- 安全测试: `config/security/`

## 🔄 版本控制

### Git 配置
- 分支策略: `master`, `develop`, `feature/*`
- 标签规范: `v1.0.0`, `v1.0.0-beta`
- 提交信息: `type: description`

### 发布流程
- 版本管理: `global.json`
- 发布说明: `docs/reference/changelog.md`
- 发布包: `releases/`

## 📋 检查清单

### 新文件添加
- [ ] 确定文件类型和存放位置
- [ ] 遵循命名规范
- [ ] 添加到相应项目
- [ ] 更新文档和引用
- [ ] 提交代码审查

### 项目结构检查
- [ ] 目录结构符合规范
- [ ] 文件命名统一
- [ ] 依赖关系清晰
- [ ] 配置文件完整
- [ ] 文档更新同步

---

## 📝 附录

### A. 常见文件类型对照表
| 文件类型 | 扩展名 | 存放位置 |
|----------|--------|----------|
| C# 源代码 | .cs | 各项目目录 |
| C# 项目 | .csproj | 各项目根目录 |
| 解决方案 | .sln | 根目录 |
| 配置文件 | .json | config/ 目录 |
| Markdown | .md | docs/ 目录 |
| 脚本文件 | .sh, .bat | scripts/ 目录 |
| 工作流文件 | .yml | .github/workflows/ |

### B. 推荐的目录结构工具
- **Visual Studio**: 内置解决方案管理
- **Rider**: 项目结构视图
- **VS Code**: 文件资源管理器
- **命令行**: `tree`, `ls`, `find`

### C. 相关文档
- [docs/CLAUDE.md](docs/CLAUDE.md): 文档管理规范
- [docs/architecture/system-architecture.md](docs/architecture/system-architecture.md): 系统架构
- [docs/development/coding-standards.md](docs/development/coding-standards.md): 编码规范

---

*最后更新: 2025-08-22*  
*维护者: Claude Code*