# KeyForge 测试套件

## 概述

KeyForge 测试套件是一个全面的测试框架，为 KeyForge 项目提供完整的测试覆盖。本测试套件包含单元测试、集成测试、性能测试和端到端测试，确保代码质量和系统稳定性。

## 测试架构

### 测试层次结构

```
KeyForge.Tests/
├── UnitTests/              # 单元测试
│   ├── Services/          # 服务层测试
│   ├── Models/           # 模型测试
│   ├── Domain/           # 领域层测试
│   └── ValueObjects/     # 值对象测试
├── IntegrationTests/      # 集成测试
│   ├── Services/         # 服务集成测试
│   └── Infrastructure/   # 基础设施集成测试
├── PerformanceTests/     # 性能测试
│   └── Services/         # 服务性能测试
├── EndToEndTests/        # 端到端测试
└── Support/              # 测试支持类
    ├── TestBase.cs       # 测试基类
    ├── TestFixtures.cs   # 测试数据工厂
    ├── MockHelpers.cs    # Mock对象辅助类
    └── TestConfig.cs     # 测试配置
```

### 测试分类

#### 1. 单元测试 (UnitTests)
- **目的**: 测试各个组件的独立功能
- **覆盖范围**: 
  - GlobalHotkeyManager - 全局快捷键管理
  - ErrorHandlerManager - 错误处理管理
  - LoggerService - 日志服务
  - PerformanceManager - 性能管理
  - Script - 脚本模型
  - DecisionRule - 决策规则
  - State - 状态管理
  - ActionSequence - 动作序列
  - ConditionExpression - 条件表达式

#### 2. 集成测试 (IntegrationTests)
- **目的**: 测试组件间的交互
- **覆盖范围**:
  - GlobalHotkeyManager 集成测试
  - ErrorHandlerManager 集成测试
  - 数据库集成测试
  - 服务间通信测试

#### 3. 性能测试 (PerformanceTests)
- **目的**: 测试系统性能和资源使用
- **覆盖范围**:
  - 响应时间测试
  - 内存使用测试
  - 并发性能测试
  - 压力测试
  - 长期稳定性测试

#### 4. 端到端测试 (EndToEndTests)
- **目的**: 测试完整的用户工作流
- **覆盖范围**:
  - 完整脚本工作流
  - 错误恢复工作流
  - 性能工作流
  - 安全工作流

## 测试运行

### 前置条件

- .NET 9.0 SDK 或更高版本
- PowerShell 5.1 或更高版本 (Windows)
- Bash 4.0 或更高版本 (Linux/macOS)
- 必要的 NuGet 包:
  - xunit
  - FluentAssertions
  - Moq
  - Microsoft.NET.Test.Sdk
  - coverlet.collector

### 运行测试

#### Windows (PowerShell)

```powershell
# 运行所有测试
.\run-tests.ps1

# 运行特定类别测试
.\run-tests.ps1 -Category Unit

# 运行调试配置测试
.\run-tests.ps1 -Configuration Debug

# 运行带详细输出的测试
.\run-tests.ps1 -Verbose

# 运行并生成报告
.\run-tests.ps1 -NoCoverage:$false
```

#### Linux/macOS (Bash)

```bash
# 设置执行权限
chmod +x run-tests.sh

# 运行所有测试
./run-tests.sh

# 运行特定类别测试
./run-tests.sh -t Unit

# 运行调试配置测试
./run-tests.sh -c Debug

# 运行带详细输出的测试
./run-tests.sh -v

# 运行并生成报告
./run-tests.sh -n=false
```

### 测试配置

测试运行配置文件 `test-run-configuration.json` 包含以下设置：

```json
{
  "TestCategories": [
    {
      "Name": "UnitTests",
      "Enabled": true,
      "Timeout": 30000,
      "Parallel": true,
      "MaxConcurrency": 4
    }
  ],
  "TestExecution": {
    "StopOnFailure": false,
    "CollectCoverage": true,
    "CoverageThreshold": 80
  }
}
```

## 测试报告

### 报告类型

1. **HTML 报告**: 详细的可视化报告
2. **JSON 报告**: 机器可读的结构化报告
3. **TRX 报告**: Visual Studio 兼容的测试结果文件
4. **覆盖率报告**: 代码覆盖率分析报告

### 报告内容

- 测试结果摘要
- 失败测试详情
- 代码覆盖率统计
- 性能指标
- 错误日志

### 查看报告

报告生成在 `TestResults/Reports/` 目录下：

```bash
# 打开HTML报告
open TestResults/Reports/test-report.html

# 查看JSON报告
cat TestResults/Reports/test-report.json
```

## 测试覆盖率

### 覆盖率目标

- **整体覆盖率**: ≥ 80%
- **单元测试覆盖率**: ≥ 90%
- **集成测试覆盖率**: ≥ 70%
- **关键路径覆盖率**: ≥ 95%

### 覆盖率工具

- **coverlet**: 代码覆盖率收集
- **ReportGenerator**: 报告生成
- **SonarQube**: 持续集成覆盖率分析

## 性能基准

### 响应时间要求

- **单元测试**: < 1ms 每个测试
- **集成测试**: < 10ms 每个测试
- **端到端测试**: < 100ms 每个测试

### 资源使用要求

- **内存使用**: < 100MB 峰值
- **CPU使用率**: < 80% 持续
- **磁盘I/O**: < 50MB/s 持续

## 持续集成

### GitHub Actions

项目包含 GitHub Actions 工作流文件，支持：

- 自动化测试运行
- 代码覆盖率检查
- 报告生成和发布
- 质量门禁检查

### 质量门禁

- 测试通过率: 100%
- 代码覆盖率: ≥ 80%
- 性能基准: 达到要求
- 安全检查: 通过

## 测试最佳实践

### 编写测试

1. **命名规范**: 使用描述性的测试名称
2. **AAA 模式**: Arrange-Act-Assert 结构
3. **测试隔离**: 每个测试独立运行
4. **断言明确**: 使用清晰的断言消息

### 维护测试

1. **定期更新**: 随代码变更更新测试
2. **性能监控**: 监控测试执行时间
3. **覆盖率检查**: 确保覆盖率不下降
4. **文档更新**: 保持测试文档同步

### 调试测试

1. **详细日志**: 使用 `-Verbose` 参数
2. **单独运行**: 使用 `-Filter` 参数运行特定测试
3. **断点调试**: 在 IDE 中设置断点
4. **日志分析**: 检查 `TestResults/Logs/` 目录

## 常见问题

### 测试失败

1. **依赖问题**: 检查 NuGet 包版本
2. **环境问题**: 确保 .NET SDK 版本正确
3. **权限问题**: 检查文件系统权限
4. **并发问题**: 减少并行测试数量

### 性能问题

1. **执行缓慢**: 检查测试配置
2. **内存不足**: 减少测试数据量
3. **I/O 瓶颈**: 优化文件操作
4. **CPU 占用高**: 减少计算密集型测试

### 覆盖率问题

1. **覆盖率低**: 添加更多测试用例
2. **分支覆盖**: 增加条件分支测试
3. **异常覆盖**: 添加异常处理测试
4. **边界覆盖**: 添加边界值测试

## 扩展测试

### 添加新测试

1. 在相应目录创建测试文件
2. 继承 `TestBase` 基类
3. 使用 `TestFixtures` 创建测试数据
4. 遵循现有测试模式

### 添加新类别

1. 更新 `test-run-configuration.json`
2. 添加相应的目录结构
3. 更新运行脚本
4. 更新文档

### 自定义报告

1. 修改 `generate-report.ps1` 脚本
2. 添加新的报告格式
3. 更新报告模板
4. 测试报告生成

## 贡献指南

### 提交测试

1. 确保所有测试通过
2. 保持或提高覆盖率
3. 遵循代码规范
4. 更新相关文档

### 代码审查

1. 测试逻辑正确性
2. 断言完整性
3. 性能影响评估
4. 维护性评估

## 版本历史

### v1.0.0 (2024-01-01)
- 初始版本
- 完整的测试套件
- 自动化测试运行
- 报告生成功能

## 许可证

本项目采用 MIT 许可证。详见 LICENSE 文件。

## 支持

如有问题或建议，请：

1. 查看 README 和文档
2. 搜索现有 issues
3. 创建新的 issue
4. 联系维护团队

---

**KeyForge 测试套件** - 确保代码质量的全面测试框架

## 测试用例示例

### 单元测试示例
```csharp
[Fact]
public void GlobalHotkeyManager_RegisterHotkey_WithValidString_ShouldReturnTrue()
{
    // Arrange
    var hotkeyString = "Ctrl+Shift+A";
    
    // Act
    var result = _hotkeyManager.RegisterHotkey(hotkeyString);
    
    // Assert
    result.Should().BeTrue();
}
```

### 集成测试示例
```csharp
[Fact]
public async Task DatabaseIntegration_InsertAndRetrieveScript_ShouldWorkCorrectly()
{
    // Arrange
    var script = new TestScript { Name = "Test Script" };
    
    // Act
    await _dbContext.Scripts.AddAsync(script);
    await _dbContext.SaveChangesAsync();
    
    var retrievedScript = await _dbContext.Scripts.FindAsync(script.Id);
    
    // Assert
    retrievedScript.Should().NotBeNull();
    retrievedScript.Name.Should().Be(script.Name);
}
```

### 性能测试示例
```csharp
[Fact]
public void HotkeyRegistration_Performance_ShouldBeFast()
{
    // Arrange
    var hotkeyCount = 1000;
    var hotkeys = new string[hotkeyCount];
    
    // Act
    var stopwatch = Stopwatch.StartNew();
    for (int i = 0; i < hotkeyCount; i++)
    {
        hotkeys[i] = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
        _hotkeyManager.RegisterHotkey(hotkeys[i]);
    }
    stopwatch.Stop();
    
    // Assert
    var averageTime = stopwatch.ElapsedMilliseconds / (double)hotkeyCount;
    averageTime.Should().BeLessThan(1);
}
```

### 端到端测试示例
```csharp
[Fact]
public async Task CompleteScriptWorkflow_ShouldWorkSuccessfully()
{
    // Arrange
    var script = new Script { Name = "Test Workflow Script" };
    script.AddAction(new KeyAction { Key = "A", Delay = 100 });
    
    // Act
    var scriptFilePath = Path.Combine(_testDirectory, "test_script.json");
    await SaveScriptToFile(script, scriptFilePath);
    var loadedScript = await LoadScriptFromFile(scriptFilePath);
    
    // Assert
    loadedScript.Should().NotBeNull();
    loadedScript.Name.Should().Be(script.Name);
}
```

## 测试数据管理

### 使用 TestFixtures
```csharp
// 创建测试数据
var script = TestFixtures.CreateValidScript();
var action = TestFixtures.CreateGameAction();
var template = TestFixtures.CreateValidImageTemplate();
```

### 使用 MockHelpers
```csharp
// 创建模拟对象
var mockRepository = MockHelpers.CreateMockRepository();
var mockService = MockHelpers.CreateMockExecutor();
```

## 测试配置管理

### 环境配置
```json
{
  "TestSettings": {
    "DefaultTimeout": 30000,
    "ParallelTestExecution": true,
    "MaxConcurrency": 4,
    "EnableTracing": false
  }
}
```

### 性能配置
```json
{
  "PerformanceSettings": {
    "WarmupIterations": 3,
    "MinIterationTime": 100,
    "MaxIterationTime": 5000,
    "MemoryLimitMB": 512
  }
}
```

## 测试运行脚本

### PowerShell 脚本功能
- 支持多种测试类别
- 自动生成报告
- 代码覆盖率收集
- 详细的日志输出

### Bash 脚本功能
- 跨平台支持
- 颜色输出
- 进度显示
- 错误处理

## 测试报告生成

### HTML 报告特性
- 响应式设计
- 交互式图表
- 详细的测试结果
- 性能指标展示

### JSON 报告特性
- 机器可读格式
- 完整的测试数据
- 便于CI/CD集成
- 支持数据分析

## 测试质量保证

### 代码质量检查
- 代码覆盖率 ≥ 80%
- 测试通过率 100%
- 无测试警告
- 性能基准达标

### 持续集成
- 自动化测试运行
- 质量门禁检查
- 报告自动生成
- 结果通知

## 总结

KeyForge 测试套件提供了完整的测试解决方案，确保项目的代码质量、功能正确性和性能稳定性。通过多层次的测试覆盖和自动化的测试运行，可以有效地发现和预防问题，提高项目的整体质量。