# KeyForge按键脚本工具 - 测试实施建议

## 执行摘要

基于之前的测试需求分析和差距分析，本文档提供了具体的测试实施建议，包括技术选型、架构设计、实施步骤和最佳实践。

## 技术选型建议

### 测试框架选型

#### 1. 集成测试框架
**推荐选择**：xUnit + FluentAssertions

**理由**：
- 与现有项目兼容
- 丰富的断言库
- 良好的并行测试支持
- 活跃的社区支持

**配置示例**：
```xml
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

#### 2. BDD测试框架
**推荐选择**：SpecFlow

**理由**：
- 支持Gherkin语法
- 与xUnit集成良好
- 提供完整的BDD测试支持
- 支持测试报告生成

**配置示例**：
```xml
<PackageReference Include="SpecFlow" Version="3.9.74" />
<PackageReference Include="SpecFlow.xUnit" Version="3.9.74" />
<PackageReference Include="SpecFlow.Tools.MsBuild.Generation" Version="3.9.74" />
```

#### 3. 性能测试框架
**推荐选择**：BenchmarkDotNet

**理由**：
- 专业的性能测试框架
- 详细的性能报告
- 支持多种性能指标
- 与.NET生态系统集成良好

**配置示例**：
```xml
<PackageReference Include="BenchmarkDotNet" Version="0.13.12" />
```

#### 4. UI测试框架
**推荐选择**：FlaUI

**理由**：
- 专为Windows UI测试设计
- 支持WinForms和WPF
- 轻量级且高效
- 良好的API设计

**配置示例**：
```xml
<PackageReference Include="FlaUI.UIA3" Version="4.0.0" />
<PackageReference Include="FlaUI.Core" Version="4.0.0" />
```

### 测试工具选型

#### 1. 测试数据管理
**推荐选择**：AutoFixture + Factory模式

**理由**：
- 自动生成测试数据
- 支持复杂对象创建
- 减少测试代码重复
- 提高测试可维护性

#### 2. 测试报告工具
**推荐选择**：Allure + 自定义HTML报告

**理由**：
- 丰富的报告功能
- 支持多种图表和统计
- 良好的可视化效果
- 支持历史数据对比

#### 3. CI/CD集成
**推荐选择**：GitHub Actions

**理由**：
- 与GitHub集成良好
- 免费且功能强大
- 支持多种操作系统
- 丰富的action生态系统

#### 4. 性能监控
**推荐选择**：Application Insights

**理由**：
- 与Azure集成良好
- 详细的性能指标
- 实时监控和报警
- 支持分布式追踪

## 测试架构设计

### 测试分层架构

```
测试架构
├── 单元测试 (Unit Tests)
│   ├── Domain层测试
│   ├── Application层测试
│   └── Infrastructure层测试
├── 集成测试 (Integration Tests)
│   ├── 模块间交互测试
│   ├── 端到端工作流测试
│   ├── 系统集成测试
│   └── 性能集成测试
├── 系统测试 (System Tests)
│   ├── UI测试
│   ├── API测试
│   └── 数据库测试
└── UAT测试 (User Acceptance Tests)
    ├── 用户场景测试
    ├── 业务流程测试
    ├── 用户体验测试
    └── 实际使用场景验证
```

### 测试项目结构

```
KeyForge.Tests/
├── KeyForge.Tests.Unit/
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
├── KeyForge.Tests.Integration/
│   ├── LayerInteraction/
│   ├── EndToEnd/
│   ├── SystemIntegration/
│   └── Performance/
├── KeyForge.Tests.System/
│   ├── UI/
│   ├── API/
│   └── Database/
├── KeyForge.Tests.UAT/
│   ├── UserScenarios/
│   ├── BusinessFlows/
│   ├── UserExperience/
│   └── RealWorldScenarios/
└── KeyForge.Tests.Common/
    ├── TestFixtures/
    ├── TestData/
    ├── TestUtilities/
    └── TestReports/
```

### 测试数据管理架构

```
测试数据管理
├── 数据工厂 (Data Factories)
│   ├── ScriptFactory
│   ├── UserFactory
│   └── ActionFactory
├── 测试数据存储 (Test Data Storage)
│   ├── JSON文件存储
│   ├── 数据库存储
│   └── 内存数据存储
├── 数据清理机制 (Data Cleanup)
│   ├── 自动清理
│   ├── 手动清理
│   └── 条件清理
└── 数据验证机制 (Data Validation)
    ├── 数据完整性验证
    ├── 数据一致性验证
    └── 数据格式验证
```

## 实施步骤

### 阶段1：基础设施搭建 (1周)

#### 1.1 测试框架配置
```csharp
// 测试基类示例
public abstract class TestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly CancellationTokenSource CancellationTokenSource;
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
        CancellationTokenSource = new CancellationTokenSource();
        ServiceProvider = CreateServiceProvider();
    }

    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // 注册测试服务
        services.AddLogging();
        services.AddTestServices();
        
        return services.BuildServiceProvider();
    }

    protected void Log(string message)
    {
        Output.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
    }
}
```

#### 1.2 测试数据工厂
```csharp
// 脚本数据工厂
public static class ScriptFactory
{
    public static Script CreateValidScript()
    {
        return new Script(
            id: Guid.NewGuid(),
            name: $"TestScript_{DateTime.Now:yyyyMMdd_HHmmss}",
            description: "Auto-generated test script"
        );
    }

    public static Script CreateScriptWithActions(int actionCount = 5)
    {
        var script = CreateValidScript();
        
        for (int i = 0; i < actionCount; i++)
        {
            script.AddAction(CreateRandomAction());
        }
        
        return script;
    }

    private static GameAction CreateRandomAction()
    {
        var random = new Random();
        var actionType = random.Next(0, 3);
        
        return actionType switch
        {
            0 => CreateKeyboardAction(),
            1 => CreateMouseAction(),
            2 => CreateDelayAction(),
            _ => CreateKeyboardAction()
        };
    }

    private static GameAction CreateKeyboardAction()
    {
        return new GameAction
        {
            Type = ActionType.Keyboard,
            Key = "A",
            Timestamp = DateTime.UtcNow,
            Duration = TimeSpan.FromMilliseconds(100)
        };
    }

    private static GameAction CreateMouseAction()
    {
        return new GameAction
        {
            Type = ActionType.Mouse,
            X = 100,
            Y = 100,
            Button = MouseButton.Left,
            Timestamp = DateTime.UtcNow,
            Duration = TimeSpan.FromMilliseconds(50)
        };
    }

    private static GameAction CreateDelayAction()
    {
        return new GameAction
        {
            Type = ActionType.Delay,
            Delay = TimeSpan.FromMilliseconds(500),
            Timestamp = DateTime.UtcNow
        };
    }
}
```

#### 1.3 BDD测试框架配置
```csharp
// BDD测试基类
public abstract class BddTestBase : TestBase
{
    protected BddTestBase(ITestOutputHelper output) : base(output)
    {
    }

    protected void Given(string description, Action action)
    {
        Log($"GIVEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void When(string description, Action action)
    {
        Log($"WHEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void Then(string description, Action action)
    {
        Log($"THEN: {description}");
        action();
        Log($"✓ {description}");
    }

    protected void And(string description, Action action)
    {
        Log($"AND: {description}");
        action();
        Log($"✓ {description}");
    }
}
```

### 阶段2：集成测试实现 (2周)

#### 2.1 模块间交互测试
```csharp
// 模块间交互测试示例
public class LayerInteractionTests : TestBase
{
    private readonly IScriptService _scriptService;
    private readonly IScriptRepository _scriptRepository;
    private readonly IExecutionEngine _executionEngine;

    public LayerInteractionTests(ITestOutputHelper output) : base(output)
    {
        _scriptRepository = ServiceProvider.GetRequiredService<IScriptRepository>();
        _scriptService = ServiceProvider.GetRequiredService<IScriptService>();
        _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
    }

    [Fact]
    public async Task ScriptCreation_ShouldIntegrateAllLayers()
    {
        // Arrange
        var script = ScriptFactory.CreateScriptWithActions(3);

        // Act - Application Layer
        await _scriptService.SaveAsync(script);

        // Assert - Infrastructure Layer
        var savedScript = await _scriptRepository.GetByIdAsync(script.Id);
        savedScript.Should().NotBeNull();
        savedScript.Name.Should().Be(script.Name);

        // Act - Execution Engine
        var result = await _executionEngine.ExecuteAsync(savedScript);

        // Assert - Execution Result
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ExecutedActions.Should().Be(3);
    }

    [Fact]
    public async Task LayerCommunication_ShouldHandleExceptions()
    {
        // Arrange
        var invalidScript = ScriptFactory.CreateValidScript();
        invalidScript.Name = ""; // 无效的脚本名称

        // Act & Assert
        var action = async () => await _scriptService.SaveAsync(invalidScript);
        await action.Should().ThrowAsync<BusinessRuleViolationException>();
    }
}
```

#### 2.2 端到端工作流测试
```csharp
// 端到端工作流测试示例
public class EndToEndWorkflowTests : TestBase
{
    private readonly IScriptService _scriptService;
    private readonly IExecutionEngine _executionEngine;
    private readonly IFileStorage _fileStorage;

    public EndToEndWorkflowTests(ITestOutputHelper output) : base(output)
    {
        _scriptService = ServiceProvider.GetRequiredService<IScriptService>();
        _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
    }

    [Fact]
    public async Task CompleteScriptLifecycle_ShouldWork()
    {
        // 1. 创建脚本
        var script = ScriptFactory.CreateScriptWithActions(5);
        script.Name = "EndToEndTestScript";

        // 2. 保存脚本
        await _scriptService.SaveAsync(script);
        Log($"脚本已保存: {script.Id}");

        // 3. 加载脚本
        var loadedScript = await _scriptService.GetByIdAsync(script.Id);
        loadedScript.Should().NotBeNull();

        // 4. 激活脚本
        loadedScript.Activate();
        await _scriptService.UpdateAsync(loadedScript);

        // 5. 执行脚本
        var executionResult = await _executionEngine.ExecuteAsync(loadedScript);
        executionResult.Success.Should().BeTrue();

        // 6. 导出脚本
        var filePath = Path.Combine(Path.GetTempPath(), $"{script.Id}.json");
        await _fileStorage.SaveAsync(script, filePath);
        File.Exists(filePath).Should().BeTrue();

        // 7. 删除脚本
        await _scriptService.DeleteAsync(script.Id);
        var deletedScript = await _scriptService.GetByIdAsync(script.Id);
        deletedScript.Should().BeNull();

        Log("完整脚本生命周期测试完成");
    }

    [Fact]
    public async Task LargeScriptProcessing_ShouldHandleVolume()
    {
        // 创建大脚本
        var largeScript = ScriptFactory.CreateScriptWithActions(1000);
        
        var startTime = DateTime.UtcNow;
        
        // 保存大脚本
        await _scriptService.SaveAsync(largeScript);
        
        // 执行大脚本
        var result = await _executionEngine.ExecuteAsync(largeScript);
        
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalMilliseconds;
        
        // 验证性能
        duration.Should().BeLessThan(5000); // 5秒内完成
        result.Success.Should().BeTrue();
        result.ExecutedActions.Should().Be(1000);
        
        Log($"大脚本处理完成: {duration:F2}ms");
    }
}
```

#### 2.3 系统集成测试
```csharp
// 系统集成测试示例
public class SystemIntegrationTests : TestBase
{
    private readonly IInputSimulator _inputSimulator;
    private readonly IWindowManager _windowManager;
    private readonly IProcessManager _processManager;

    public SystemIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _inputSimulator = ServiceProvider.GetRequiredService<IInputSimulator>();
        _windowManager = ServiceProvider.GetRequiredService<IWindowManager>();
        _processManager = ServiceProvider.GetRequiredService<IProcessManager>();
    }

    [Fact]
    public void InputSimulation_ShouldWorkWithSystem()
    {
        // 获取当前活动窗口
        var activeWindow = _windowManager.GetActiveWindow();
        activeWindow.Should().NotBeNull();

        // 模拟按键输入
        _inputSimulator.SendKeys("Hello World");
        
        // 模拟鼠标移动
        _inputSimulator.MoveMouse(100, 100);
        
        // 模拟鼠标点击
        _inputSimulator.ClickMouse(MouseButton.Left);
        
        Log("输入模拟测试完成");
    }

    [Fact]
    public void ProcessManagement_ShouldControlApplications()
    {
        // 启动记事本
        var notepad = _processManager.StartProcess("notepad.exe");
        notepad.Should().NotBeNull();

        // 等待启动
        Thread.Sleep(1000);

        // 检查进程是否运行
        var isRunning = _processManager.IsProcessRunning(notepad.Id);
        isRunning.Should().BeTrue();

        // 终止进程
        _processManager.KillProcess(notepad.Id);

        // 验证进程已终止
        isRunning = _processManager.IsProcessRunning(notepad.Id);
        isRunning.Should().BeFalse();

        Log("进程管理测试完成");
    }
}
```

### 阶段3：UAT测试实现 (2周)

#### 3.1 用户场景测试
```csharp
// 用户场景测试示例
public class UserScenarioTests : TestBase
{
    private readonly IUserInterface _userInterface;
    private readonly IScenarioRunner _scenarioRunner;

    public UserScenarioTests(ITestOutputHelper output) : base(output)
    {
        _userInterface = ServiceProvider.GetRequiredService<IUserInterface>();
        _scenarioRunner = ServiceProvider.GetRequiredService<IScenarioRunner>();
    }

    [Fact]
    public async Task GameAutomationScenario_ShouldWork()
    {
        // 游戏自动化场景
        var scenario = new GameAutomationScenario
        {
            GameName = "TestGame",
            Actions = new[]
            {
                new GameAction { Type = ActionType.Keyboard, Key = "Space" },
                new GameAction { Type = ActionType.Delay, Delay = TimeSpan.FromMilliseconds(500) },
                new GameAction { Type = ActionType.Keyboard, Key = "Ctrl+C" }
            }
        };

        var result = await _scenarioRunner.RunScenarioAsync(scenario);
        result.Success.Should().BeTrue();
        result.ExecutionTime.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task OfficeAutomationScenario_ShouldWork()
    {
        // 办公自动化场景
        var scenario = new OfficeAutomationScenario
        {
            Application = "Word",
            Actions = new[]
            {
                new GameAction { Type = ActionType.Keyboard, Key = "Ctrl+N" }, // 新建文档
                new GameAction { Type = ActionType.Delay, Delay = TimeSpan.FromMilliseconds(1000) },
                new GameAction { Type = ActionType.Keyboard, Key = "Hello World" }, // 输入文本
                new GameAction { Type = ActionType.Keyboard, Key = "Ctrl+S" } // 保存文档
            }
        };

        var result = await _scenarioRunner.RunScenarioAsync(scenario);
        result.Success.Should().BeTrue();
    }
}
```

#### 3.2 用户体验测试
```csharp
// 用户体验测试示例
public class UserExperienceTests : TestBase
{
    private readonly IUserInterface _userInterface;
    private readonly IPerformanceMonitor _performanceMonitor;

    public UserExperienceTests(ITestOutputHelper output) : base(output)
    {
        _userInterface = ServiceProvider.GetRequiredService<IUserInterface>();
        _performanceMonitor = ServiceProvider.GetRequiredService<IPerformanceMonitor>();
    }

    [Fact]
    public void InterfaceResponseTime_ShouldMeetRequirements()
    {
        // 测试界面响应时间
        var responseTime = _performanceMonitor.MeasureResponseTime(() =>
        {
            _userInterface.ClickButton("RecordButton");
        });

        responseTime.Should().BeLessThan(100); // 响应时间小于100ms
        Log($"界面响应时间: {responseTime}ms");
    }

    [Fact]
    public void ErrorHandling_ShouldBeUserFriendly()
    {
        // 测试错误处理
        var errorMessage = _userInterface.SimulateError("InvalidScriptError");
        
        errorMessage.Should().NotBeNullOrEmpty();
        errorMessage.Should().Contain("script");
        errorMessage.Should().Contain("invalid");
        
        Log($"错误消息: {errorMessage}");
    }

    [Fact]
    public void LearningCurve_ShouldBeReasonable()
    {
        // 测试学习成本
        var learningTime = _userInterface.MeasureLearningTime();
        
        learningTime.Should().BeLessThan(TimeSpan.FromMinutes(30)); // 学习时间小于30分钟
        Log($"学习时间: {learningTime.TotalMinutes}分钟");
    }
}
```

### 阶段4：自动化和CI/CD (1周)

#### 4.1 GitHub Actions配置
```yaml
# .github/workflows/test.yml
name: KeyForge Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: windows-latest
    
    strategy:
      matrix:
        test-category: [Unit, Integration, System, UAT]
        
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run ${{ matrix.test-category }} Tests
      run: dotnet test --filter "TestCategory=${{ matrix.test-category }}" --verbosity normal
    
    - name: Generate Test Report
      run: dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
    
    - name: Upload Test Results
      uses: actions/upload-artifact@v3
      with:
        name: test-results-${{ matrix.test-category }}
        path: TestResults/
```

#### 4.2 测试报告生成
```csharp
// 测试报告生成器
public static class TestReportGenerator
{
    public static void GenerateComprehensiveReport(TestResults results)
    {
        var report = new ComprehensiveTestReport
        {
            Timestamp = DateTime.Now,
            Summary = CreateSummary(results),
            Coverage = CreateCoverage(results),
            Performance = CreatePerformance(results),
            Recommendations = CreateRecommendations(results)
        };

        // 生成JSON报告
        GenerateJsonReport(report);
        
        // 生成HTML报告
        GenerateHtmlReport(report);
        
        // 生成Markdown报告
        GenerateMarkdownReport(report);
    }

    private static TestSummary CreateSummary(TestResults results)
    {
        return new TestSummary
        {
            TotalTests = results.Total,
            PassedTests = results.Passed,
            FailedTests = results.Failed,
            SkippedTests = results.Skipped,
            SuccessRate = (double)results.Passed / results.Total * 100,
            ExecutionTime = results.Duration
        };
    }

    private static void GenerateHtmlReport(ComprehensiveTestReport report)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>KeyForge 测试报告</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .summary {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .success {{ color: green; }}
        .error {{ color: red; }}
        .warning {{ color: orange; }}
        .chart {{ width: 100%; height: 300px; }}
    </style>
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
</head>
<body>
    <h1>KeyForge 测试报告</h1>
    <div class=""summary"">
        <h2>测试摘要</h2>
        <p>总测试数: {report.Summary.TotalTests}</p>
        <p>通过: <span class=""success"">{report.Summary.PassedTests}</span></p>
        <p>失败: <span class=""error"">{report.Summary.FailedTests}</span></p>
        <p>跳过: <span class=""warning"">{report.Summary.SkippedTests}</span></p>
        <p>成功率: {report.Summary.SuccessRate:F1}%</p>
        <p>执行时间: {report.Summary.ExecutionTime}ms</p>
    </div>
    
    <div class=""chart"">
        <canvas id=""testChart""></canvas>
    </div>
    
    <script>
        const ctx = document.getElementById('testChart').getContext('2d');
        new Chart(ctx, {{
            type: 'pie',
            data: {{
                labels: ['通过', '失败', '跳过'],
                datasets: [{{
                    data: [
                        {report.Summary.PassedTests},
                        {report.Summary.FailedTests},
                        {report.Summary.SkippedTests}
                    ],
                    backgroundColor: ['#28a745', '#dc3545', '#ffc107']
                }}]
            }}
        }});
    </script>
</body>
</html>";

        File.WriteAllText("TestResults/comprehensive-report.html", html);
    }
}
```

## 最佳实践建议

### 1. 测试命名约定
```csharp
// 好的测试命名
public class ScriptServiceTests
{
    [Fact]
    public async Task SaveScript_WithValidScript_ShouldPersistToDatabase()
    [Fact]
    public async Task SaveScript_WithDuplicateId_ShouldThrowException()
    [Fact]
    public async Task GetScriptById_WithNonExistingId_ShouldReturnNull()
}

// BDD测试命名
public class ScriptRecordingTests
{
    [Fact]
    public void ScriptRecordingAndPlayback_ShouldWorkCorrectly()
    [Fact]
    public void ScriptManagement_ShouldSupportCrudOperations()
    [Fact]
    public void ScriptValidation_ShouldEnforceBusinessRules()
}
```

### 2. 测试数据管理
```csharp
// 使用测试数据工厂
public class ScriptServiceTests
{
    [Fact]
    public async Task SaveScript_WithValidScript_ShouldPersistToDatabase()
    {
        // Arrange
        var script = ScriptFactory.CreateValidScript();
        
        // Act
        await _scriptService.SaveAsync(script);
        
        // Assert
        var savedScript = await _scriptService.GetByIdAsync(script.Id);
        savedScript.Should().NotBeNull();
    }
}
```

### 3. 测试隔离
```csharp
// 每个测试使用独立的数据
public class ScriptRepositoryTests : TestBase
{
    [Fact]
    public async Task SaveScript_ShouldCreateNewRecord()
    {
        // Arrange
        var script = ScriptFactory.CreateValidScript();
        
        // Act
        await _repository.SaveAsync(script);
        
        // Assert
        var savedScript = await _repository.GetByIdAsync(script.Id);
        savedScript.Should().NotBeNull();
        savedScript.Name.Should().Be(script.Name);
    }
    
    [Fact]
    public async Task SaveScript_WithDuplicateId_ShouldThrowException()
    {
        // Arrange
        var script = ScriptFactory.CreateValidScript();
        await _repository.SaveAsync(script);
        
        // Act & Assert
        var action = async () => await _repository.SaveAsync(script);
        await action.Should().ThrowAsync<DuplicateKeyException>();
    }
}
```

### 4. 性能测试
```csharp
[MemoryDiagnoser]
public class ScriptPerformanceTests
{
    [Benchmark]
    public void CreateScript()
    {
        var script = ScriptFactory.CreateValidScript();
    }
    
    [Benchmark]
    public void CreateScriptWithActions()
    {
        var script = ScriptFactory.CreateScriptWithActions(100);
    }
    
    [Benchmark]
    public async Task SaveAndLoadScript()
    {
        var script = ScriptFactory.CreateScriptWithActions(50);
        await _repository.SaveAsync(script);
        var loadedScript = await _repository.GetByIdAsync(script.Id);
    }
}
```

### 5. 测试覆盖率
```xml
<!-- 覆盖率配置 -->
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="ReportGenerator" Version="5.1.26" />
```

```bash
# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
reportgenerator -reports:TestResults/coverage.xml -targetdir:TestResults/Report
```

## 监控和维护

### 1. 测试执行监控
```csharp
// 测试执行监控器
public class TestExecutionMonitor
{
    private readonly ILogger<TestExecutionMonitor> _logger;
    private readonly ITestMetricsCollector _metricsCollector;

    public TestExecutionMonitor(
        ILogger<TestExecutionMonitor> logger,
        ITestMetricsCollector metricsCollector)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    public async Task MonitorTestExecutionAsync(Func<Task> testAction)
    {
        var startTime = DateTime.UtcNow;
        var startMemory = GC.GetTotalMemory(false);
        
        try
        {
            await testAction();
            
            var endTime = DateTime.UtcNow;
            var endMemory = GC.GetTotalMemory(false);
            
            var duration = (endTime - startTime).TotalMilliseconds;
            var memoryUsed = endMemory - startMemory;
            
            _metricsCollector.RecordTestExecution(duration, memoryUsed, true);
            
            _logger.LogInformation(
                "测试执行成功 - 耗时: {Duration}ms, 内存使用: {Memory}bytes",
                duration, memoryUsed);
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;
            
            _metricsCollector.RecordTestExecution(duration, 0, false);
            
            _logger.LogError(ex, "测试执行失败 - 耗时: {Duration}ms", duration);
            throw;
        }
    }
}
```

### 2. 测试健康检查
```csharp
// 测试健康检查
public class TestHealthChecker
{
    private readonly ITestRepository _testRepository;
    private readonly ITestRunner _testRunner;

    public TestHealthChecker(
        ITestRepository testRepository,
        ITestRunner testRunner)
    {
        _testRepository = testRepository;
        _testRunner = testRunner;
    }

    public async Task<TestHealthStatus> CheckHealthAsync()
    {
        var status = new TestHealthStatus();
        
        // 检查测试数量
        var testCount = await _testRepository.GetTestCountAsync();
        status.TestCount = testCount;
        
        // 检查最近测试结果
        var recentResults = await _testRepository.GetRecentTestResultsAsync(24);
        status.RecentSuccessRate = CalculateSuccessRate(recentResults);
        
        // 检查测试执行时间
        var avgExecutionTime = await _testRepository.GetAverageExecutionTimeAsync();
        status.AverageExecutionTime = avgExecutionTime;
        
        // 运行健康检查测试
        var healthCheckResult = await _testRunner.RunHealthCheckTestsAsync();
        status.HealthCheckPassed = healthCheckResult.Success;
        
        return status;
    }

    private double CalculateSuccessRate(IEnumerable<TestResult> results)
    {
        if (!results.Any()) return 0;
        
        var successCount = results.Count(r => r.Success);
        return (double)successCount / results.Count() * 100;
    }
}
```

## 总结

通过以上实施建议，KeyForge项目可以建立一个完整的测试体系，包括：

1. **完整的测试覆盖**：从单元测试到UAT测试的完整覆盖
2. **高质量的测试代码**：遵循最佳实践的测试实现
3. **自动化测试流程**：CI/CD集成的自动化测试
4. **详细的测试报告**：可视化的测试结果展示
5. **持续的监控和维护**：测试执行的健康检查

这些改进将显著提升项目的测试质量和覆盖率，确保系统的稳定性和可靠性。