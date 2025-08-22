# KeyForge 测试套件维护和扩展指南

## 概述

本指南提供了KeyForge测试套件的维护和扩展最佳实践，确保测试的长期可维护性和有效性。

## 目录

1. [测试架构概述](#测试架构概述)
2. [测试分类策略](#测试分类策略)
3. [测试编写规范](#测试编写规范)
4. [测试数据管理](#测试数据管理)
5. [Mock对象使用](#Mock对象使用)
6. [断言最佳实践](#断言最佳实践)
7. [测试组织结构](#测试组织结构)
8. [性能测试优化](#性能测试优化)
9. [CI/CD集成](#cicd集成)
10. [故障排除指南](#故障排除指南)
11. [扩展指南](#扩展指南)

## 测试架构概述

### 分层测试架构

```
┌─────────────────────────────────────────────────────────────┐
│                     端到端测试 (E2E)                        │
│                 (完整业务流程验证)                          │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    集成测试 (Integration)                   │
│                (组件间交互验证)                             │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     单元测试 (Unit)                         │
│                 (单个组件验证)                              │
└─────────────────────────────────────────────────────────────┘
```

### 测试金字塔

```
         ┌─────────────┐
         │   E2E测试   │  ← 少量，关键业务流程
         │    (5%)    │
         └─────────────┘
        ┌───────────────┐
        │  集成测试    │  ← 适中，组件交互
        │   (20%)     │
        └───────────────┘
     ┌───────────────────┐
     │     单元测试      │  ← 大量，核心逻辑
     │     (75%)       │
     └───────────────────┘
```

## 测试分类策略

### 1. 单元测试 (Unit Tests)

**目的**: 验证单个类或方法的行为

**特征**:
- 快速执行 (< 1秒)
- 无外部依赖
- 使用Mock对象
- 高覆盖率目标

**适用场景**:
- 业务逻辑验证
- 算法测试
- 边界条件测试
- 异常处理测试

**示例位置**: `KeyForge.Tests/UnitTests/`

### 2. 集成测试 (Integration Tests)

**目的**: 验证多个组件的交互

**特征**:
- 中等执行时间 (1-5秒)
- 需要真实依赖
- 测试接口契约
- 验证数据流

**适用场景**:
- 数据库交互
- 外部API调用
- 消息队列
- 文件系统操作

**示例位置**: `KeyForge.Tests/IntegrationTests/`

### 3. 端到端测试 (End-to-End Tests)

**目的**: 验证完整的业务流程

**特征**:
- 较慢执行 (5-30秒)
- 完整环境依赖
- 用户视角测试
- 关键路径覆盖

**适用场景**:
- 核心业务流程
- 用户注册登录
- 数据处理流程
- 关键用户故事

**示例位置**: `KeyForge.Tests/EndToEndTests/`

### 4. 性能测试 (Performance Tests)

**目的**: 验证系统性能特性

**特征**:
- 长时间运行 (30秒+)
- 性能指标收集
- 负载测试
- 基准比较

**适用场景**:
- 响应时间测试
- 并发处理测试
- 内存泄漏检测
- 性能回归测试

**示例位置**: `KeyForge.Tests/PerformanceTests/`

## 测试编写规范

### 命名约定

#### 测试类命名
```
[被测试类]Tests.cs
例如:
- ScriptTests.cs
- ScriptServiceTests.cs
- ScriptPlayerTests.cs
```

#### 测试方法命名
```
[测试场景]_[预期行为]_[条件]
例如:
- Constructor_WithValidData_ShouldCreateScript()
- AddAction_WithNullAction_ShouldThrowValidationException()
- PlayScript_WhenScriptNotLoaded_ShouldThrowException()
```

#### 测试分类
```csharp
[Trait("Category", "Unit")]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Performance")]
```

### 测试结构 (AAA模式)

#### Arrange (准备)
```csharp
// 准备测试数据
var scriptId = Guid.NewGuid();
var scriptName = "Test Script";
var scriptDescription = "A test script";

// 准备依赖
var mockRepository = new Mock<IScriptRepository>();
var service = new ScriptService(mockRepository.Object);
```

#### Act (执行)
```csharp
// 执行被测试的方法
var result = service.CreateScript(scriptId, scriptName, scriptDescription);
```

#### Assert (验证)
```csharp
// 验证结果
result.Should().NotBeNull();
result.Id.Should().Be(scriptId);
result.Name.Should().Be(scriptName);
result.Description.Should().Be(scriptDescription);

// 验证依赖调用
mockRepository.Verify(r => r.Add(It.IsAny<Script>()), Times.Once);
```

### 完整测试示例

```csharp
[Trait("Category", "Unit")]
public class ScriptServiceTests : TestBase
{
    [Fact]
    public void CreateScript_WithValidData_ShouldCreateScript()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var scriptName = "Test Script";
        var scriptDescription = "A test script";
        
        var mockRepository = new Mock<IScriptRepository>();
        var service = new ScriptService(mockRepository.Object);
        
        // Act
        var result = service.CreateScript(scriptId, scriptName, scriptDescription);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(scriptId);
        result.Name.Should().Be(scriptName);
        result.Description.Should().Be(scriptDescription);
        result.Status.Should().Be(ScriptStatus.Draft);
        
        mockRepository.Verify(r => r.Add(It.IsAny<Script>()), Times.Once);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateScript_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var scriptDescription = "A test script";
        
        var mockRepository = new Mock<IScriptRepository>();
        var service = new ScriptService(mockRepository.Object);
        
        // Act & Assert
        var action = () => service.CreateScript(scriptId, invalidName, scriptDescription);
        action.Should().Throw<ValidationException>()
              .WithMessage("Script name cannot be empty.");
    }
}
```

## 测试数据管理

### TestDataFactory 使用

#### 基本数据生成
```csharp
public static class TestDataFactory
{
    private static readonly Faker Faker = new Faker();
    
    public static Script CreateValidScript()
    {
        var scriptId = Guid.NewGuid();
        var script = new Script(scriptId, 
            Faker.Lorem.Sentence(3), 
            Faker.Lorem.Paragraph());
        
        // 添加随机动作
        var actions = CreateGameActions(5);
        foreach (var action in actions)
        {
            script.AddAction(action);
        }
        
        return script;
    }
    
    public static GameAction CreateGameAction()
    {
        var actionType = Faker.PickRandom<ActionType>();
        
        return actionType switch
        {
            ActionType.KeyDown or ActionType.KeyUp => new GameAction(
                Guid.NewGuid(),
                actionType,
                Faker.PickRandom<KeyCode>(),
                Faker.Random.Int(0, 1000),
                Faker.Lorem.Sentence()),
            
            ActionType.Delay => new GameAction(
                Guid.NewGuid(),
                actionType,
                Faker.Random.Int(100, 5000),
                Faker.Lorem.Sentence()),
            
            _ => throw new ArgumentException($"Unsupported action type: {actionType}")
        };
    }
}
```

#### 测试数据构建器模式
```csharp
public class ScriptBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Default Script";
    private string _description = "Default description";
    private List<GameAction> _actions = new List<GameAction>();
    
    public ScriptBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }
    
    public ScriptBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ScriptBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public ScriptBuilder WithAction(GameAction action)
    {
        _actions.Add(action);
        return this;
    }
    
    public ScriptBuilder WithRandomActions(int count = 5)
    {
        _actions.AddRange(TestDataFactory.CreateGameActions(count));
        return this;
    }
    
    public Script Build()
    {
        var script = new Script(_id, _name, _description);
        foreach (var action in _actions)
        {
            script.AddAction(action);
        }
        return script;
    }
}
```

### 测试数据清理

#### 使用IDisposable清理
```csharp
public class DatabaseTestBase : TestBase, IDisposable
{
    protected readonly TestDbContext _context;
    
    public DatabaseTestBase()
    {
        _context = new TestDbContext();
        _context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

#### 使用TestInitialize和TestCleanup
```csharp
public class ScriptServiceTests : TestBase
{
    private IScriptRepository _repository;
    private ScriptService _service;
    
    public ScriptServiceTests()
    {
        // TestInitialize
        _repository = new Mock<IScriptRepository>().Object;
        _service = new ScriptService(_repository);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        // 清理测试数据
        // 重置Mock对象
    }
}
```

## Mock对象使用

### 基本Mock设置
```csharp
// 创建Mock对象
var mockRepository = new Mock<IScriptRepository>();

// 设置方法返回值
mockRepository.Setup(r => r.GetById(It.IsAny<Guid>()))
              .ReturnsAsync((Guid id) => TestDataFactory.CreateValidScript());

// 设置方法抛出异常
mockRepository.Setup(r => r.GetById(It.IsAny<Guid>()))
              .ThrowsAsync(new EntityNotFoundException("Script", Guid.NewGuid()));

// 验证方法调用
mockRepository.Verify(r => r.GetById(It.IsAny<Guid>()), Times.Once);
mockRepository.Verify(r => r.Add(It.IsAny<Script>()), Times.Once);

// 验证方法从未调用
mockRepository.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
```

### 复杂Mock设置
```csharp
// 设置回调
mockRepository.Setup(r => r.Add(It.IsAny<Script>()))
              .Callback<Script>(script => 
              {
                  script.Id = Guid.NewGuid();
                  script.CreatedAt = DateTime.UtcNow;
              });

// 设置顺序验证
var sequence = new MockSequence();
mockRepository.InSequence(sequence)
              .Setup(r => r.GetById(scriptId))
              .ReturnsAsync(script);
mockRepository.InSequence(sequence)
              .Setup(r => r.Update(It.IsAny<Script>()))
              .Returns(Task.CompletedTask);

// 设置属性行为
mockLogger.Setup(l => l.IsEnabledFor(It.IsAny<LogLevel>()))
          .Returns(true);
mockLogger.Setup(l => l.Log(It.IsAny<LogLevel>(), 
                            It.IsAny<EventId>(),
                            It.IsAny<It.IsAnyType>(),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
          .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
          {
              // 记录日志
          });
```

### 严格Mock
```csharp
// 严格Mock - 未设置的方法调用会抛出异常
var strictMock = new Mock<IScriptRepository>(MockBehavior.Strict);
strictMock.Setup(r => r.GetById(It.IsAny<Guid>()))
          .ReturnsAsync(TestDataFactory.CreateValidScript());

// 松散Mock - 未设置的方法返回默认值
var looseMock = new Mock<IScriptRepository>(MockBehavior.Loose);
```

## 断言最佳实践

### 使用FluentAssertions
```csharp
// 基本断言
result.Should().NotBeNull();
result.Id.Should().Be(expectedId);
result.Name.Should().Be("Expected Name");
result.Actions.Should().HaveCount(5);

// 集合断言
actions.Should().Contain(action => action.Type == ActionType.KeyDown);
actions.Should().NotContain(action => action.Type == ActionType.Delay);
actions.Should().BeInAscendingOrder(a => a.Timestamp);

// 异常断言
Action action = () => service.CreateScript("", "Description");
action.Should().Throw<ValidationException>()
      .WithMessage("Script name cannot be empty.");

// 异步断言
Func<Task> asyncAction = async () => await service.GetScriptAsync(Guid.NewGuid());
await asyncAction.Should().ThrowAsync<EntityNotFoundException>();

// 对象等价性
result.Should().BeEquivalentTo(expected, options => 
    options.Excluding(x => x.CreatedAt)
           .Excluding(x => x.UpdatedAt));

// 数值范围
result.Delay.Should().BeInRange(100, 5000);
result.Version.Should().BeGreaterThan(0);

// 日期时间
result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
result.UpdatedAt.Should().BeAfter(result.CreatedAt);
```

### 自定义断言
```csharp
public static class ScriptAssertionExtensions
{
    public static void ShouldBeValidScript(this Script script)
    {
        script.Should().NotBeNull();
        script.Id.Should().NotBeEmpty();
        script.Name.Should().NotBeNullOrEmpty();
        script.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        script.Version.Should().BeGreaterThan(0);
    }
    
    public static void ShouldBeInStatus(this Script script, ScriptStatus expectedStatus)
    {
        script.Status.Should().Be(expectedStatus);
    }
    
    public static void ShouldHaveEstimatedDuration(this Script script, TimeSpan expectedMinDuration)
    {
        script.GetEstimatedDuration().Should().BeGreaterThanOrEqualTo(expectedMinDuration);
    }
}
```

## 测试组织结构

### 目录结构
```
KeyForge.Tests/
├── Common/
│   ├── TestBase.cs                 # 测试基类
│   ├── TestDataFactory.cs          # 测试数据工厂
│   ├── TestExtensions.cs           # 测试扩展方法
│   └── MockHelpers.cs              # Mock对象助手
├── UnitTests/
│   ├── Domain/
│   │   ├── Scripts/
│   │   │   ├── ScriptTests.cs
│   │   │   ├── ScriptBuilder.cs
│   │   │   └── ScriptAssertions.cs
│   │   ├── StateMachines/
│   │   └── ImageTemplates/
│   ├── Application/
│   │   ├── Services/
│   │   └── Commands/
│   └── Core/
├── IntegrationTests/
│   ├── Application/
│   └── Infrastructure/
├── EndToEndTests/
│   ├── ScriptLifecycle/
│   └── UserWorkflows/
└── PerformanceTests/
    ├── Benchmarks/
    └── LoadTests/
```

### 测试基类设计
```csharp
public abstract class TestBase
{
    protected readonly Faker Faker = new Faker();
    protected readonly Mock<ILogger<ScriptService>> MockLogger = new Mock<ILogger<ScriptService>>();
    
    protected TestBase()
    {
        // 通用测试设置
    }
    
    // 通用断言方法
    protected static void ShouldThrowValidationException(Action action, string expectedMessage)
    {
        var exception = Assert.Throws<ValidationException>(action);
        exception.Message.Should().Be(expectedMessage);
    }
    
    protected static void ShouldThrowBusinessRuleViolationException(Action action, string expectedMessage)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(action);
        exception.Message.Should().Be(expectedMessage);
    }
    
    // 测试数据生成方法
    protected Script CreateValidScript()
    {
        return TestDataFactory.CreateValidScript();
    }
    
    protected GameAction CreateGameAction()
    {
        return TestDataFactory.CreateGameAction();
    }
}
```

### 专用测试基类
```csharp
public class DatabaseTestBase : TestBase, IDisposable
{
    protected readonly TestDbContext Context;
    
    public DatabaseTestBase()
    {
        Context = new TestDbContext();
        Context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}

public class ServiceTestBase : TestBase
{
    protected Mock<IScriptRepository> MockRepository { get; set; }
    protected Mock<IEventBus> MockEventBus { get; set; }
    protected ScriptService Service { get; set; }
    
    public ServiceTestBase()
    {
        MockRepository = new Mock<IScriptRepository>();
        MockEventBus = new Mock<IEventBus>();
        Service = new ScriptService(MockRepository.Object, MockEventBus.Object, MockLogger.Object);
    }
}
```

## 性能测试优化

### BenchmarkDotNet使用
```csharp
[MemoryDiagnoser]
public class ScriptServiceBenchmark
{
    private ScriptService _service;
    private List<Script> _scripts;
    
    [GlobalSetup]
    public void Setup()
    {
        _service = new ScriptService(/* dependencies */);
        _scripts = Enumerable.Range(0, 1000)
                           .Select(_ => TestDataFactory.CreateValidScript())
                           .ToList();
    }
    
    [Benchmark]
    public void CreateScript()
    {
        _service.CreateScript(Guid.NewGuid(), "Benchmark Script", "Description");
    }
    
    [Benchmark]
    public void GetScriptById()
    {
        _service.GetScriptById(_scripts.First().Id);
    }
    
    [Benchmark]
    public void GetScriptsPaged()
    {
        _service.GetScriptsPaged(1, 20);
    }
}
```

### 并发测试
```csharp
[Fact]
public async Task ConcurrentScriptExecution_ShouldHandleConcurrency()
{
    // Arrange
    var scriptId = Guid.NewGuid();
    var service = new ScriptService(/* dependencies */);
    
    var tasks = new List<Task>();
    var concurrentCount = 10;
    
    // Act
    for (int i = 0; i < concurrentCount; i++)
    {
        tasks.Add(Task.Run(() => service.PlayScript(scriptId)));
    }
    
    // Assert
    await Task.WhenAll(tasks);
    
    // 验证并发处理结果
    // ...
}
```

### 内存泄漏检测
```csharp
[Fact]
public void ScriptService_ShouldNotHaveMemoryLeaks()
{
    // Arrange
    var weakReference = new WeakReference(null);
    
    // Act
    new Action(() =>
    {
        var service = new ScriptService(/* dependencies */);
        weakReference.Target = service;
    })();
    
    // 强制垃圾回收
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    
    // Assert
    weakReference.Target.Should().BeNull();
}
```

## CI/CD集成

### GitHub Actions工作流
```yaml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore KeyForge.Tests/KeyForge.Tests.csproj
    
    - name: Build
      run: dotnet build KeyForge.Tests/KeyForge.Tests.csproj --no-restore
    
    - name: Test
      run: dotnet test KeyForge.Tests/KeyForge.Tests.csproj --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./TestResults/coverage.xml

  integration-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Run integration tests
      run: ./run-tests.sh -i -c

  performance-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Run performance tests
      run: ./run-tests.sh -p
```

### 质量门禁
```yaml
  quality-gate:
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests]
    
    steps:
    - name: Check coverage
      run: |
        if [ $(cat TestResults/coverage.xml | grep -o 'line-rate="[^"]*"' | cut -d'"' -f2 | awk '{print $1*100}') -lt 80 ]; then
          echo "Coverage below 80%"
          exit 1
        fi
    
    - name: Check test results
      run: |
        if [ $(find TestResults -name "*.trx" -exec grep -l "Failed" {} \; | wc -l) -gt 0 ]; then
          echo "Tests failed"
          exit 1
        fi
```

## 故障排除指南

### 常见问题

#### 1. 编译错误
```
错误: 未能找到类型或命名空间名"IScriptRepository"
```
**解决方案**:
- 检查using语句
- 确认项目引用
- 验证命名空间

#### 2. 测试超时
```
错误: 测试执行超时
```
**解决方案**:
- 增加测试超时时间
- 优化测试逻辑
- 减少外部依赖

#### 3. Mock对象问题
```
错误: Mock对象返回默认值
```
**解决方案**:
- 检查Mock设置
- 验证方法签名
- 使用严格Mock

#### 4. 数据库连接问题
```
错误: 无法连接到数据库
```
**解决方案**:
- 检查连接字符串
- 确认数据库服务
- 使用内存数据库

### 调试技巧

#### 1. 启用详细输出
```bash
dotnet test --verbosity diagnostic
```

#### 2. 运行单个测试
```bash
dotnet test --filter "TestName"
```

#### 3. 调试测试
```csharp
[Fact]
public void DebuggableTest()
{
    // 添加断点
    Debugger.Launch();
    
    // 测试代码
    var result = service.CreateScript(Guid.NewGuid(), "Debug", "Test");
    result.Should().NotBeNull();
}
```

#### 4. 日志记录
```csharp
[Fact]
public void TestWithLogging()
{
    // 配置日志
    var loggerFactory = LoggerFactory.Create(builder => 
        builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
    
    var logger = loggerFactory.CreateLogger<ScriptService>();
    var service = new ScriptService(logger);
    
    // 执行测试
    service.CreateScript(Guid.NewGuid(), "Logged", "Test");
}
```

## 扩展指南

### 添加新的测试类别

#### 1. 创建测试目录
```bash
mkdir -p KeyForge.Tests/NewCategory
```

#### 2. 创建测试基类
```csharp
public abstract class NewCategoryTestBase : TestBase
{
    protected NewCategoryTestBase()
    {
        // 专用设置
    }
}
```

#### 3. 实现测试类
```csharp
[Trait("Category", "NewCategory")]
public class NewFeatureTests : NewCategoryTestBase
{
    [Fact]
    public void NewFeature_ShouldWorkCorrectly()
    {
        // 实现测试
    }
}
```

### 添加新的Mock类型

#### 1. 创建Mock助手
```csharp
public static class MockRepositoryExtensions
{
    public static Mock<IScriptRepository> SetupValidScript(this Mock<IScriptRepository> mock, Script script)
    {
        mock.Setup(r => r.GetById(script.Id))
            .ReturnsAsync(script);
        return mock;
    }
    
    public static Mock<IScriptRepository> SetupScriptNotFound(this Mock<IScriptRepository> mock, Guid scriptId)
    {
        mock.Setup(r => r.GetById(scriptId))
            .ThrowsAsync(new EntityNotFoundException("Script", scriptId));
        return mock;
    }
}
```

#### 2. 使用扩展方法
```csharp
var mockRepository = new Mock<IScriptRepository>()
    .SetupValidScript(script)
    .SetupScriptNotFound(Guid.NewGuid());
```

### 添加新的测试数据

#### 1. 扩展TestDataFactory
```csharp
public static class TestDataFactory
{
    public static Script CreateComplexScript()
    {
        var script = CreateValidScript();
        // 添加复杂逻辑
        return script;
    }
    
    public static Script CreateScriptWithSpecificActions(List<ActionType> actionTypes)
    {
        var script = CreateValidScript();
        foreach (var actionType in actionTypes)
        {
            script.AddAction(CreateGameActionOfType(actionType));
        }
        return script;
    }
}
```

### 添加新的性能基准

#### 1. 创建基准测试类
```csharp
[MemoryDiagnoser]
public class NewFeatureBenchmark
{
    [Benchmark]
    public void NewFeaturePerformance()
    {
        // 实现性能测试
    }
}
```

#### 2. 运行基准测试
```bash
dotnet run -c Release --project KeyForge.Benchmarks
```

## 总结

KeyForge测试套件维护和扩展指南提供了完整的测试最佳实践，包括：

- **测试架构**: 分层测试策略和金字塔模型
- **编写规范**: 命名约定、结构模式、分类标准
- **数据管理**: 测试数据生成、清理、构建器模式
- **Mock使用**: 基本设置、复杂配置、严格模式
- **断言最佳实践**: FluentAssertions、自定义断言
- **组织结构**: 目录结构、基类设计、专用基类
- **性能优化**: BenchmarkDotNet、并发测试、内存泄漏检测
- **CI/CD集成**: GitHub Actions、质量门禁
- **故障排除**: 常见问题、调试技巧
- **扩展指南**: 新功能测试、Mock扩展、数据扩展

遵循这些指南可以确保测试套件的可维护性、可扩展性和长期有效性。