# KeyForge HAL 完整测试套件文档

## 📋 测试套件概述

本测试套件为 KeyForge 硬件抽象层（HAL）提供全面的测试覆盖，确保跨平台兼容性、性能和质量标准。

### 🎯 测试目标

- **代码覆盖率**: > 80%
- **性能基准**: 响应时间 < 100ms
- **质量门禁**: 综合评分 > 80分
- **跨平台兼容性**: Windows、Linux、macOS
- **内存使用**: < 50MB 空闲时

## 🏗️ 测试架构

### 测试分类

```
KeyForge HAL Tests/
├── UnitTests/                 # 单元测试
│   ├── HALAbstractionUnitTests.cs
│   └── HALExceptionHandlingTests.cs
├── IntegrationTests/         # 集成测试
│   └── CrossPlatformIntegrationTests.cs
├── PerformanceTests/         # 性能测试
│   ├── HALPerformanceBenchmarks.cs
│   └── HALStressTests.cs
├── CompatibilityTests/       # 兼容性测试
│   └── HALCompatibilityTests.cs
├── EndToEndTests/           # 端到端测试
│   └── HALEndToEndTests.cs
├── QualityGateTests/         # 质量门禁测试
│   └── HALQualityGateTests.cs
└── TestCoverage/            # 测试覆盖率
    └── KeyForge.Tests.Coverage.csproj
```

### 测试框架依赖

- **xUnit**: 2.9.2 - 测试框架
- **FluentAssertions**: 6.12.1 - 断言库
- **Moq**: 4.20.70 - Mock框架
- **BenchmarkDotNet**: 0.13.12 - 性能测试
- **Coverlet**: 6.0.2 - 代码覆盖率
- **ReportGenerator**: 5.2.0 - 报告生成

## 🚀 运行测试

### 前置条件

1. **.NET 9.0 SDK** 或更高版本
2. **Visual Studio 2022** 或 **Rider** 或 **VS Code**
3. **Git** 用于版本控制

### 运行所有测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test KeyForge.HAL.Tests/KeyForge.HAL.Tests.csproj

# 运行覆盖率测试
dotnet test KeyForge.Tests.Coverage/KeyForge.Tests.Coverage.csproj
```

### 运行特定测试类别

```bash
# 单元测试
dotnet test --filter "FullyQualifiedName~UnitTests"

# 集成测试
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# 性能测试
dotnet test --filter "FullyQualifiedName~PerformanceTests"

# 兼容性测试
dotnet test --filter "FullyQualifiedName~CompatibilityTests"

# 端到端测试
dotnet test --filter "FullyQualifiedName~EndToEndTests"

# 质量门禁测试
dotnet test --filter "FullyQualifiedName~QualityGateTests"
```

### 运行性能基准测试

```bash
# 运行性能基准测试
dotnet run --project KeyForge.HAL.Tests --configuration Release -- --benchmark

# 运行特定性能测试
dotnet run --project KeyForge.HAL.Tests --configuration Release -- --benchmark --filter *Keyboard*
```

### 生成测试报告

```bash
# 生成覆盖率报告
dotnet test KeyForge.Tests.Coverage/KeyForge.Tests.Coverage.csproj --collect:"XPlat Code Coverage"

# 生成HTML报告
reportgenerator -reports:coverage/coverage.json -targetdir:coverage/report -reporttypes:Html

# 查看报告
open coverage/report/index.html
```

## 📊 测试覆盖率

### 覆盖率目标

- **行覆盖率**: > 80%
- **分支覆盖率**: > 75%
- **方法覆盖率**: > 85%

### 排除文件

```xml
<Exclude>
  [*]*.Designer.cs
  [*]*.AssemblyInfo.cs
  [*]*.g.cs
  [*]*.xaml.cs
  [KeyForge.Tests.*]*
  [KeyForge.Presentation]*
  [Program]*
</Exclude>
```

### 覆盖率报告位置

- **JSON报告**: `coverage/coverage.json`
- **HTML报告**: `coverage/report/index.html`
- **Cobertura报告**: `coverage/report/cobertura.xml`

## 🔧 质量门禁

### 质量标准

- **测试覆盖率**: > 80%
- **代码复杂度**: < 10
- **代码重复率**: < 5%
- **性能基准**: 响应时间 < 100ms
- **内存使用**: < 50MB

### 质量检查项

1. **代码质量**
   - 静态分析
   - 代码风格
   - 命名规范

2. **测试质量**
   - 测试覆盖率
   - 测试有效性
   - Mock使用

3. **性能质量**
   - 响应时间
   - 内存使用
   - CPU使用率

4. **安全质量**
   - 安全漏洞
   - 权限检查
   - 输入验证

## 🎯 测试场景

### 1. 单元测试场景

**HAL抽象层测试**
- 初始化和关闭
- 性能指标收集
- 健康检查
- 质量门禁执行

**键盘服务测试**
- 按键按下/释放
- 文本输入
- 热键组合
- 按键状态查询

**鼠标服务测试**
- 鼠标移动
- 点击操作
- 滚轮操作
- 按键状态查询

### 2. 集成测试场景

**跨平台集成**
- 平台检测
- 服务兼容性
- 性能一致性
- 错误处理

**服务间集成**
- 键盘+鼠标协作
- 屏幕+图像识别
- 窗口+全局热键
- 性能监控+质量门禁

### 3. 性能测试场景

**基准测试**
- 键盘操作性能
- 鼠标操作性能
- 屏幕捕获性能
- 图像识别性能

**压力测试**
- 并发操作
- 长时间运行
- 内存使用
- 错误恢复

### 4. 兼容性测试场景

**平台兼容性**
- Windows 特性测试
- Linux 特性测试
- macOS 特性测试
- 跨平台一致性

**版本兼容性**
- .NET 版本兼容性
- 依赖版本兼容性
- API 兼容性

### 5. 端到端测试场景

**完整工作流**
- 脚本录制和回放
- 自动化工作流
- 多窗口操作
- 图像识别自动化

**长时间稳定性**
- 长时间运行测试
- 内存泄漏检测
- 性能退化检测

### 6. 质量门禁测试场景

**代码质量检查**
- 覆盖率检查
- 复杂度检查
- 重复率检查

**运行时质量**
- 性能监控
- 错误处理
- 资源管理

## 📈 性能基准

### 响应时间基准

| 操作类型 | 平均响应时间 | 95百分位 | 最大响应时间 |
|---------|-------------|----------|-------------|
| 键盘操作 | < 10ms | < 50ms | < 100ms |
| 鼠标操作 | < 15ms | < 75ms | < 150ms |
| 屏幕捕获 | < 50ms | < 200ms | < 500ms |
| 图像识别 | < 100ms | < 500ms | < 1000ms |

### 资源使用基准

| 资源类型 | 空闲时使用 | 正常使用 | 峰值使用 |
|---------|-----------|----------|----------|
| 内存 | < 50MB | < 100MB | < 200MB |
| CPU | < 5% | < 20% | < 50% |
| 磁盘 | < 1MB/s | < 10MB/s | < 50MB/s |

### 并发性能基准

| 并发级别 | 吞吐量 | 平均延迟 | 错误率 |
|---------|--------|----------|--------|
| 10 并发 | > 100 ops/s | < 50ms | < 1% |
| 50 并发 | > 500 ops/s | < 100ms | < 2% |
| 100 并发 | > 1000 ops/s | < 200ms | < 5% |

## 🐛 故障排除

### 常见问题

1. **测试失败**
   ```bash
   # 清理和重建
   dotnet clean
   dotnet restore
   dotnet build
   
   # 运行特定测试
   dotnet test --filter "TestName"
   ```

2. **覆盖率报告生成失败**
   ```bash
   # 检查 coverlet 版本
   dotnet list package --include-transitive
   
   # 重新安装包
   dotnet add package coverlet.msbuild
   dotnet add package coverlet.collector
   ```

3. **性能测试异常**
   ```bash
   # 确保在 Release 模式下运行
   dotnet test --configuration Release
   
   # 检查系统资源
   dotnet run --project KeyForge.HAL.Tests -- --benchmark --diagnosers
   ```

### 调试技巧

1. **启用详细日志**
   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

2. **运行单个测试**
   ```bash
   dotnet test --filter "TestName"
   ```

3. **生成测试结果文件**
   ```bash
   dotnet test --logger "trx;LogFileName=test_results.trx"
   ```

## 📝 测试最佳实践

### 1. 测试命名规范

```csharp
// 使用描述性的测试名称
[Fact]
public async Task Keyboard_KeyPressAsync_WithValidKey_ShouldSucceed()

// 使用 Theory 进行参数化测试
[Theory]
[InlineData(KeyCode.A)]
[InlineData(KeyCode.B)]
[InlineData(KeyCode.C)]
public async Task Keyboard_KeyPressAsync_WithCommonKeys_ShouldSucceed(KeyCode key)
```

### 2. 测试结构

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - 准备测试数据
    var hal = GetHAL();
    await hal.InitializeAsync();
    
    // Act - 执行测试操作
    var result = await hal.Keyboard.KeyPressAsync(KeyCode.A);
    
    // Assert - 验证结果
    result.Should().BeTrue();
}
```

### 3. Mock 使用

```csharp
// 使用 Mock 模拟依赖
var mockKeyboardService = new Mock<IKeyboardService>();
mockKeyboardService.Setup(x => x.KeyPressAsync(It.IsAny<KeyCode>()))
    .ReturnsAsync(true);
```

### 4. 异步测试

```csharp
[Fact]
public async Task AsyncTest()
{
    // 使用 async/await
    var result = await SomeAsyncOperation();
    result.Should().Be(expectedValue);
}
```

## 🔄 CI/CD 集成

### Azure Pipelines 配置

```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '9.0.x'

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: '$(solution)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: '$(solution)'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests/*.csproj'
    arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'

- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'

- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(System.DefaultWorkingDirectory)/**/coverage.cobertura.xml'
```

### GitHub Actions 配置

```yaml
# .github/workflows/test.yml
name: Run Tests

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest, macos-latest]
        
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

## 📊 测试报告

### 报告类型

1. **测试结果报告**
   - 通过/失败状态
   - 执行时间
   - 错误信息

2. **覆盖率报告**
   - 行覆盖率
   - 分支覆盖率
   - 方法覆盖率

3. **性能报告**
   - 响应时间
   - 吞吐量
   - 资源使用

4. **质量报告**
   - 质量评分
   - 问题列表
   - 改进建议

### 报告查看

```bash
# 查看测试结果
dotnet test --logger "console;verbosity=detailed"

# 查看覆盖率报告
open coverage/report/index.html

# 查看性能报告
open BenchmarkDotNet.Artifacts/results/report.html
```

## 🎯 测试目标达成

### 成功标准

- ✅ **代码覆盖率**: > 80%
- ✅ **性能基准**: 响应时间 < 100ms
- ✅ **质量门禁**: 综合评分 > 80分
- ✅ **跨平台兼容性**: Windows、Linux、macOS
- ✅ **内存使用**: < 50MB 空闲时

### 持续改进

- 定期更新测试用例
- 监控性能趋势
- 改进测试覆盖率
- 优化测试执行时间

---

## 📞 支持

如果遇到问题或需要帮助，请：

1. 查看本文档的故障排除部分
2. 检查 GitHub Issues
3. 联系开发团队

---

*最后更新: 2024-01-15*