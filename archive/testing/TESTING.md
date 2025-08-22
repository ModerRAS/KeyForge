# KeyForge 测试文档

## 概述

KeyForge项目采用了完整的测试策略，包括单元测试、集成测试、性能测试、安全测试和覆盖率分析。本文档提供了全面的测试指南和最佳实践。

## 测试架构

### 测试分层结构

```
KeyForge.Tests/
├── KeyForge.Tests.csproj              # 主测试项目
├── KeyForge.Domain.Tests.csproj        # 领域层测试
├── KeyForge.Tests.Performance.csproj   # 性能测试
├── KeyForge.Tests.Security.csproj      # 安全测试
└── KeyForge.Tests.Coverage.csproj      # 覆盖率分析
```

### 测试类型

1. **单元测试** - 测试单个组件的功能
2. **集成测试** - 测试组件间的交互
3. **性能测试** - 测试系统性能和负载能力
4. **安全测试** - 测试系统安全性
5. **覆盖率分析** - 分析测试覆盖率

## 快速开始

### 环境要求

- .NET 8.0 SDK
- Visual Studio 2022 或 .NET CLI
- 可选：Docker（用于集成测试）
- 可选：Playwright（用于E2E测试）

### 运行测试

#### 使用命令行

```bash
# 运行所有单元测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj

# 运行特定测试项目
dotnet test KeyForge.Domain.Tests/KeyForge.Domain.Tests.csproj

# 运行性能测试
cd KeyForge.Tests/Performance && dotnet run -- quick

# 运行安全测试
cd KeyForge.Tests/Security && ./run-security-tests.sh quick

# 运行覆盖率分析
cd KeyForge.Tests.Coverage && ./run-coverage-analysis.sh run
```

#### 使用Visual Studio

1. 打开解决方案
2. 选择"测试"菜单
3. 选择"运行所有测试"
4. 查看测试资源管理器中的结果

## 测试配置

### 单元测试配置

```json
{
  "TestSettings": {
    "ParallelizeTestCollections": true,
    "MaxParallelThreads": 4,
    "TestOutputPath": "TestResults",
    "EnableTracing": false,
    "CollectCoverage": true
  }
}
```

### 性能测试配置

```json
{
  "PerformanceSettings": {
    "BenchmarkSettings": {
      "WarmupIterations": 3,
      "TargetIterations": 10,
      "MinIterationTime": "00:00:01"
    },
    "LoadTestSettings": {
      "DefaultDurationSeconds": 30,
      "DefaultConcurrentUsers": 50,
      "SuccessRateThreshold": 95.0
    }
  }
}
```

### 安全测试配置

```json
{
  "SecurityTestSettings": {
    "AuthenticationTests": {
      "TestPasswordComplexity": true,
      "MaxLoginAttempts": 5,
      "LockoutDurationMinutes": 15
    },
    "InputValidationTests": {
      "TestXssPrevention": true,
      "TestSqlInjectionPrevention": true
    }
  }
}
```

## 测试最佳实践

### 1. 单元测试最佳实践

#### 命名约定

```csharp
// 类名：[ClassName]Tests
public class ScriptServiceTests
{
    // 方法名：[Scenario]_[ExpectedBehavior]
    [Fact]
    public void CreateScript_WithValidData_ShouldReturnScript()
    {
        // 测试逻辑
    }
}
```

#### 测试结构

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - 准备测试数据
    var service = new ScriptService(mockRepository.Object);
    var request = new CreateScriptRequest("Test Script", "Content");
    
    // Act - 执行测试操作
    var result = service.CreateScript(request);
    
    // Assert - 验证测试结果
    Assert.NotNull(result);
    Assert.Equal("Test Script", result.Name);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Script>()), Times.Once);
}
```

#### Mock使用

```csharp
[Fact]
public async Task GetScript_WithValidId_ShouldReturnScript()
{
    // Arrange
    var scriptId = Guid.NewGuid();
    var expectedScript = new Script(scriptId, "Test Script", "Content");
    
    var mockRepository = new Mock<IScriptRepository>();
    mockRepository.Setup(r => r.GetByIdAsync(scriptId))
        .ReturnsAsync(expectedScript);
    
    var service = new ScriptService(mockRepository.Object);
    
    // Act
    var result = await service.GetScriptByIdAsync(scriptId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(scriptId, result.Id);
    mockRepository.Verify(r => r.GetByIdAsync(scriptId), Times.Once);
}
```

### 2. 集成测试最佳实践

#### 数据库测试

```csharp
[Fact]
public async Task CreateScript_WithValidData_ShouldPersistToDatabase()
{
    // Arrange
    using var context = CreateDbContext();
    var service = new ScriptService(new ScriptRepository(context));
    var request = new CreateScriptRequest("Test Script", "Content");
    
    // Act
    var result = await service.CreateScript(request);
    
    // Assert
    Assert.NotNull(result);
    
    var savedScript = await context.Scripts.FindAsync(result.Id);
    Assert.NotNull(savedScript);
    Assert.Equal("Test Script", savedScript.Name);
}
```

#### API测试

```csharp
[Fact]
public async Task CreateScript_WithValidData_ShouldReturnCreated()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var request = new CreateScriptRequest("Test Script", "Content");
    
    // Act
    var response = await client.PostAsJsonAsync("/api/scripts", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    var result = await response.Content.ReadFromJsonAsync<ScriptDto>();
    Assert.NotNull(result);
    Assert.Equal("Test Script", result.Name);
}
```

### 3. 性能测试最佳实践

#### 基准测试

```csharp
[MemoryDiagnoser]
public class ScriptServiceBenchmarks
{
    private readonly ScriptService _service;
    private readonly CreateScriptRequest _request;
    
    public ScriptServiceBenchmarks()
    {
        var mockRepository = new Mock<IScriptRepository>();
        _service = new ScriptService(mockRepository.Object);
        _request = new CreateScriptRequest("Benchmark Script", "Content");
    }
    
    [Benchmark]
    public Script CreateScript()
    {
        return _service.CreateScript(_request);
    }
    
    [Benchmark]
    public async Task<Script> CreateScriptAsync()
    {
        return await _service.CreateScriptAsync(_request);
    }
}
```

#### 负载测试

```csharp
[Fact]
public async Task ScriptService_ShouldHandleConcurrentRequests()
{
    // Arrange
    var service = new ScriptService(mockRepository.Object);
    var request = new CreateScriptRequest("Concurrent Script", "Content");
    var concurrentUsers = 50;
    
    // Act
    var tasks = new List<Task<Script>>();
    for (int i = 0; i < concurrentUsers; i++)
    {
        tasks.Add(service.CreateScriptAsync(request));
    }
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    Assert.Equal(concurrentUsers, results.Length);
    Assert.All(results, r => Assert.NotNull(r));
}
```

### 4. 安全测试最佳实践

#### 认证测试

```csharp
[Fact]
public async Task Login_WithValidCredentials_ShouldReturnToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var loginRequest = new LoginRequest("test@example.com", "ValidPass123!");
    
    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
    
    // Assert
    response.EnsureSuccessStatusCode();
    
    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
    Assert.NotNull(result.Token);
}
```

#### 输入验证测试

```csharp
[Theory]
[InlineData("<script>alert('xss')</script>")]
[InlineData("'; DROP TABLE users; --")]
[InlineData("../../../etc/passwd")]
public async Task CreateScript_WithMaliciousInput_ShouldBlockRequest(string maliciousInput)
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new CreateScriptRequest(maliciousInput, "Content");
    
    // Act
    var response = await client.PostAsJsonAsync("/api/scripts", request);
    
    // Assert
    Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Forbidden);
}
```

## 测试数据管理

### 测试数据工厂

```csharp
public static class TestDataFactory
{
    public static Script CreateValidScript()
    {
        return new Script(
            Guid.NewGuid(),
            "Test Script",
            "Script content",
            ScriptType.JavaScript,
            ScriptStatus.Active
        );
    }
    
    public static User CreateValidUser()
    {
        return new User(
            Guid.NewGuid(),
            "test@example.com",
            "Test User",
            "User"
        );
    }
    
    public static CreateScriptRequest CreateValidCreateScriptRequest()
    {
        return new CreateScriptRequest(
            "Test Script",
            "Script content"
        );
    }
}
```

### 数据库初始化

```csharp
public class TestDatabaseInitializer
{
    public static void Initialize(KeyForgeDbContext context)
    {
        // 清理数据库
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // 添加基础测试数据
        context.Users.Add(TestDataFactory.CreateValidUser());
        context.Scripts.Add(TestDataFactory.CreateValidScript());
        
        context.SaveChanges();
    }
}
```

## 测试覆盖率

### 覆盖率目标

- **行覆盖率**: 80%
- **分支覆盖率**: 75%
- **方法覆盖率**: 85%
- **类覆盖率**: 90%

### 覆盖率报告生成

```bash
# 生成覆盖率报告
./run-coverage-analysis.sh full

# 查看覆盖率结果
open coverage/report/index.html
```

### 覆盖率阈值验证

```bash
# 验证覆盖率是否达到阈值
./run-coverage-analysis.sh validate
```

## 持续集成

### GitHub Actions

项目已配置GitHub Actions工作流，自动运行：

1. **构建测试** - 构建并运行所有测试
2. **代码质量检查** - 运行SonarQube分析
3. **安全扫描** - 运行安全测试和漏洞扫描
4. **性能测试** - 运行性能和负载测试
5. **覆盖率分析** - 生成覆盖率报告
6. **部署** - 自动部署到staging/production

### Azure DevOps

项目支持Azure DevOps管道，提供：

1. **多阶段部署** - Build → Test → Deploy
2. **环境管理** - Staging → Production
3. **质量门禁** - 覆盖率、安全、性能阈值
4. **通知系统** - Slack/Teams集成

## 测试故障排除

### 常见问题

1. **测试失败**
   - 检查测试环境配置
   - 查看详细错误日志
   - 验证测试数据

2. **性能不达标**
   - 优化测试环境
   - 调整测试参数
   - 检查系统资源

3. **安全测试失败**
   - 更新漏洞数据库
   - 检查安全配置
   - 验证输入验证

### 日志文件

- `logs/unit-tests.log` - 单元测试日志
- `logs/integration-tests.log` - 集成测试日志
- `logs/performance-tests.log` - 性能测试日志
- `logs/security-tests.log` - 安全测试日志
- `logs/coverage-analysis.log` - 覆盖率分析日志

### 调试技巧

1. **使用断点调试**
   ```csharp
   [Fact]
   public void DebuggableTest()
   {
       // 在这里设置断点
       Debugger.Break();
       
       // 测试逻辑
   }
   ```

2. **输出调试信息**
   ```csharp
   [Fact]
   public void VerboseTest()
   {
       // 输出调试信息
       Console.WriteLine($"Current value: {variable}");
       
       // 测试逻辑
   }
   ```

3. **条件测试执行**
   ```csharp
   [Fact]
   public void ConditionalTest()
   {
       #if DEBUG
       // 仅在Debug模式下运行
       DebugTestLogic();
       #endif
   }
   ```

## 测试扩展

### 添加新的测试类型

1. **创建测试项目**
   ```bash
   dotnet new xunit -n KeyForge.Tests.NewType
   ```

2. **添加项目引用**
   ```xml
   <ItemGroup>
       <ProjectReference Include="..\KeyForge.Domain\KeyForge.Domain.csproj" />
   </ItemGroup>
   ```

3. **实现测试用例**
   ```csharp
   public class NewTypeTests
   {
       [Fact]
       public void NewFeature_ShouldWorkCorrectly()
       {
           // 测试实现
       }
   }
   ```

### 自定义测试运行器

```csharp
public class CustomTestRunner
{
    public async Task RunCustomTestsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var testCases = assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<FactAttribute>().Any());
        
        foreach (var test in testCases)
        {
            await RunTestAsync(test);
        }
    }
}
```

## 测试自动化

### 自动化测试脚本

项目提供了多个自动化测试脚本：

- `run-all-tests.sh` - 运行所有测试
- `run-performance-tests.sh` - 运行性能测试
- `run-security-tests.sh` - 运行安全测试
- `run-coverage-analysis.sh` - 运行覆盖率分析

### 定期测试执行

建议配置定期测试执行：

1. **每次代码提交** - 运行单元测试和集成测试
2. **每日构建** - 运行所有测试包括性能测试
3. **每周扫描** - 运行安全测试和漏洞扫描
4. **每月报告** - 生成综合测试报告

## 测试报告

### 测试报告类型

1. **单元测试报告** - xUnit XML格式
2. **性能测试报告** - BenchmarkDotNet HTML格式
3. **安全测试报告** - 自定义JSON/HTML格式
4. **覆盖率报告** - HTML/XML/Markdown格式

### 报告生成

```bash
# 生成所有报告
./run-all-tests.sh report

# 查看报告
open TestResults/
open PerformanceReports/
open SecurityReports/
open coverage/report/
```

### 报告分析

1. **测试通过率** - 应该 > 95%
2. **性能指标** - 响应时间 < 100ms
3. **安全评分** - 应该 > 80分
4. **覆盖率** - 应该 > 80%

## 最佳实践总结

### 1. 测试设计原则

- **单一职责** - 每个测试只验证一个功能点
- **可重复性** - 测试应该可以重复执行
- **独立性** - 测试之间不应该有依赖关系
- **可读性** - 测试代码应该清晰易懂

### 2. 测试数据管理

- **使用工厂模式** - 统一管理测试数据
- **数据隔离** - 每个测试使用独立的数据
- **数据清理** - 测试完成后清理数据

### 3. 测试执行优化

- **并行执行** - 提高测试执行速度
- **选择性执行** - 只运行相关的测试
- **缓存机制** - 缓存测试结果

### 4. 测试维护

- **定期更新** - 随着代码更新测试
- **重构优化** - 保持测试代码质量
- **文档同步** - 更新测试文档

## 参考资源

### 官方文档

- [xUnit Documentation](https://xunit.net/)
- [Microsoft Testing Documentation](https://docs.microsoft.com/en-us/dotnet/testing/)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)

### 工具和框架

- **测试框架**: xUnit, NUnit, MSTest
- **Mock框架**: Moq, NSubstitute, FakeItEasy
- **性能测试**: BenchmarkDotNet, NBomber
- **安全测试**: OWASP ZAP, SecurityCodeScan
- **覆盖率工具**: Coverlet, dotCover

### 社区资源

- [Stack Overflow](https://stackoverflow.com/)
- [GitHub Discussions](https://github.com/discussions)
- [Reddit r/csharp](https://www.reddit.com/r/csharp/)
- [Microsoft Q&A](https://docs.microsoft.com/en-us/answers/topics/dotnet)

---

*文档版本: 1.0*
*最后更新: 2024-01-01*