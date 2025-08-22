# KeyForge 测试策略和质量保证

## 测试策略概述

### 测试目标
- 确保核心功能稳定可靠
- 验证系统性能指标达标
- 保证用户体验质量
- 降低维护成本和风险

### 测试原则
- **测试驱动开发**：先写测试再实现功能
- **自动化优先**：尽量自动化测试流程
- **持续集成**：频繁运行测试确保质量
- **用户导向**：测试真实使用场景

## 测试覆盖要求

### 1. 单元测试（覆盖率 60%）

#### 1.1 核心模块测试
```csharp
// 图像识别服务测试
[TestClass]
public class ImageRecognitionServiceTests
{
    private ImageRecognitionService _service;
    private TestImageHelper _imageHelper;
    
    [TestInitialize]
    public void Setup()
    {
        _service = new ImageRecognitionService(0.8);
        _imageHelper = new TestImageHelper();
    }
    
    [TestMethod]
    public void FindImage_WithExactMatch_ReturnsSuccess()
    {
        // Arrange
        var templatePath = _imageHelper.CreateTestTemplate("test_template.png");
        var screenPath = _imageHelper.CreateTestScreen("test_screen.png", templatePath);
        
        // Act
        var result = _service.FindImageOnScreen(templatePath);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Confidence > 0.9);
    }
    
    [TestMethod]
    public void FindImage_WithNoMatch_ReturnsFailure()
    {
        // Arrange
        var templatePath = _imageHelper.CreateTestTemplate("nonexistent.png");
        
        // Act
        var result = _service.FindImageOnScreen(templatePath);
        
        // Assert
        Assert.IsFalse(result.Success);
    }
    
    [TestMethod]
    public void FindImage_WithPartialMatch_ReturnsCorrectConfidence()
    {
        // Arrange
        var templatePath = _imageHelper.CreatePartialTemplate("partial_template.png");
        
        // Act
        var result = _service.FindImageOnScreen(templatePath);
        
        // Assert
        Assert.IsTrue(result.Confidence > 0.5 && result.Confidence < 0.8);
    }
}

// 条件判断测试
[TestClass]
public class ConditionEvaluatorTests
{
    private ConditionEvaluator _evaluator;
    
    [TestInitialize]
    public void Setup()
    {
        _evaluator = new ConditionEvaluator();
    }
    
    [TestMethod]
    public void Evaluate_EqualsCondition_ReturnsTrue()
    {
        var condition = new Condition
        {
            Operator = ConditionOperator.Equals,
            LeftValue = "test",
            RightValue = "test"
        };
        
        var result = _evaluator.Evaluate(condition, new Dictionary<string, object>());
        
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public void Evaluate_GreaterThanCondition_ReturnsCorrectResult()
    {
        var condition = new Condition
        {
            Operator = ConditionOperator.GreaterThan,
            LeftValue = "10",
            RightValue = "5"
        };
        
        var result = _evaluator.Evaluate(condition, new Dictionary<string, object>());
        
        Assert.IsTrue(result);
    }
}
```

#### 1.2 输入模拟测试
```csharp
[TestClass]
public class InputSimulatorTests
{
    private InputSimulatorService _simulator;
    private InputTestRecorder _recorder;
    
    [TestInitialize]
    public void Setup()
    {
        _simulator = new InputSimulatorService();
        _recorder = new InputTestRecorder();
    }
    
    [TestMethod]
    public void SendKey_KeyDownAndUp_TriggersCorrectEvents()
    {
        // Arrange
        _recorder.StartRecording();
        
        // Act
        _simulator.SendKey(KeyCode.A, KeyState.Down);
        _simulator.SendKey(KeyCode.A, KeyState.Up);
        
        // Assert
        var events = _recorder.StopRecording();
        Assert.AreEqual(2, events.Count);
        Assert.AreEqual(KeyCode.A, events[0].KeyCode);
        Assert.AreEqual(KeyState.Down, events[0].KeyState);
    }
    
    [TestMethod]
    public void SendMouse_MouseMove_SetsCorrectPosition()
    {
        // Arrange
        var testPosition = new Point(100, 200);
        
        // Act
        _simulator.SendMouse(testPosition.X, testPosition.Y, MouseButton.Left, MouseAction.Move);
        
        // Assert
        Assert.AreEqual(testPosition, Cursor.Position);
    }
}
```

### 2. 集成测试（覆盖率 30%）

#### 2.1 模块集成测试
```csharp
[TestClass]
public class ModuleIntegrationTests
{
    private SenseModule _senseModule;
    private JudgeModule _judgeModule;
    private ActModule _actModule;
    
    [TestInitialize]
    public void Setup()
    {
        _senseModule = new SenseModule(
            new ImageRecognitionService(),
            new InputCaptureService()
        );
        
        _judgeModule = new JudgeModule(
            new ConditionEvaluator(),
            new ScriptExecutor()
        );
        
        _actModule = new ActModule(
            new InputSimulatorService(),
            new FileLoggerService("test.log")
        );
    }
    
    [TestMethod]
    public async Task SenseJudgeAct_WithImageTrigger_CompletesSuccessfully()
    {
        // Arrange
        var script = new Script
        {
            Name = "Image Trigger Test",
            Actions = new List<ScriptAction>
            {
                new ScriptAction
                {
                    Type = ActionType.ImageRecognition,
                    TemplatePath = "test_trigger.png",
                    ThenActions = new List<ScriptAction>
                    {
                        new ScriptAction
                        {
                            Type = ActionType.KeyDown,
                            KeyCode = KeyCode.Space
                        }
                    }
                }
            }
        };
        
        // Act
        var result = await _judgeModule.ExecuteScript(script);
        
        // Assert
        Assert.AreEqual(ExecutionStatus.Completed, result.Status);
    }
}
```

#### 2.2 端到端测试
```csharp
[TestClass]
public class EndToEndTests
{
    private KeyForgeApplication _app;
    private TestWindowManager _windowManager;
    
    [TestInitialize]
    public void Setup()
    {
        _app = new KeyForgeApplication();
        _windowManager = new TestWindowManager();
    }
    
    [TestMethod]
    public void RecordAndPlaybackScript_CompletesSuccessfully()
    {
        // Arrange
        _app.Start();
        var mainWindow = _windowManager.GetMainWindow();
        
        // Act
        mainWindow.ClickRecordButton();
        SimulateUserInput();
        mainWindow.ClickStopButton();
        mainWindow.ClickPlayButton();
        
        // Assert
        var script = _app.GetCurrentScript();
        Assert.IsNotNull(script);
        Assert.IsTrue(script.Actions.Count > 0);
        Assert.AreEqual(ExecutionStatus.Completed, script.Status);
    }
    
    private void SimulateUserInput()
    {
        // 模拟用户输入
        var simulator = new InputSimulatorService();
        simulator.SendKey(KeyCode.A, KeyState.Down);
        simulator.SendKey(KeyCode.A, KeyState.Up);
        Thread.Sleep(100);
        simulator.SendMouse(100, 200, MouseButton.Left, MouseAction.Down);
        simulator.SendMouse(100, 200, MouseButton.Left, MouseAction.Up);
    }
}
```

### 3. 性能测试

#### 3.1 性能基准测试
```csharp
[TestClass]
public class PerformanceTests
{
    private ImageRecognitionService _imageService;
    private ScriptExecutor _scriptExecutor;
    
    [TestInitialize]
    public void Setup()
    {
        _imageService = new ImageRecognitionService();
        _scriptExecutor = new ScriptExecutor();
    }
    
    [TestMethod]
    public void ImageRecognition_Performance_MeetsRequirements()
    {
        // Arrange
        var templatePath = "performance_test.png";
        var iterations = 100;
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var result = _imageService.FindImageOnScreen(templatePath);
        }
        stopwatch.Stop();
        
        // Assert
        var averageTime = stopwatch.ElapsedMilliseconds / iterations;
        Assert.IsTrue(averageTime < 200, $"Average recognition time {averageTime}ms exceeds 200ms limit");
    }
    
    [TestMethod]
    public void ScriptExecution_Performance_MeetsRequirements()
    {
        // Arrange
        var script = CreatePerformanceTestScript();
        var iterations = 50;
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var result = _scriptExecutor.Execute(script);
        }
        stopwatch.Stop();
        
        // Assert
        var averageTime = stopwatch.ElapsedMilliseconds / iterations;
        Assert.IsTrue(averageTime < 100, $"Average execution time {averageTime}ms exceeds 100ms limit");
    }
    
    private Script CreatePerformanceTestScript()
    {
        return new Script
        {
            Name = "Performance Test",
            Actions = new List<ScriptAction>
            {
                new ScriptAction { Type = ActionType.KeyDown, KeyCode = KeyCode.A },
                new ScriptAction { Type = ActionType.KeyUp, KeyCode = KeyCode.A, Delay = 50 },
                new ScriptAction { Type = ActionType.MouseDown, X = 100, Y = 200 },
                new ScriptAction { Type = ActionType.MouseUp, X = 100, Y = 200, Delay = 50 }
            }
        };
    }
}
```

#### 3.2 内存泄漏测试
```csharp
[TestClass]
public class MemoryLeakTests
{
    [TestMethod]
    public void LongRunningOperation_NoMemoryLeak()
    {
        // Arrange
        var service = new ImageRecognitionService();
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act
        for (int i = 0; i < 1000; i++)
        {
            var result = service.FindImageOnScreen("test.png");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        // Assert
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        Assert.IsTrue(memoryIncrease < 10 * 1024 * 1024, $"Memory increase {memoryIncrease} bytes exceeds 10MB limit");
    }
}
```

### 4. 用户验收测试

#### 4.1 用户场景测试
```csharp
[TestClass]
public class UserAcceptanceTests
{
    private TestApplicationRunner _appRunner;
    
    [TestInitialize]
    public void Setup()
    {
        _appRunner = new TestApplicationRunner();
    }
    
    [TestMethod]
    public void UserScenario_RecordAndPlayback_Success()
    {
        // Arrange
        _appRunner.StartApplication();
        var mainWindow = _appRunner.GetMainWindow();
        
        // Act
        // 用户录制脚本
        mainWindow.ClickButton("RecordButton");
        _appRunner.SimulateUserInput(KeyCode.A, KeyState.Down);
        _appRunner.SimulateUserInput(KeyCode.A, KeyState.Up);
        mainWindow.ClickButton("StopButton");
        
        // 用户播放脚本
        mainWindow.ClickButton("PlayButton");
        
        // Assert
        var result = _appRunner.WaitForScriptCompletion();
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ExecutedActions.Count > 0);
    }
    
    [TestMethod]
    public void UserScenario_ImageTrigger_Success()
    {
        // Arrange
        _appRunner.StartApplication();
        var mainWindow = _appRunner.GetMainWindow();
        
        // Act
        // 用户配置图像触发
        mainWindow.ClickButton("AddTriggerButton");
        mainWindow.SelectTemplate("test_trigger.png");
        mainWindow.SetTriggerAction(KeyCode.Space);
        mainWindow.ClickButton("SaveButton");
        
        // 用户启动监控
        mainWindow.ClickButton("StartMonitoringButton");
        _appRunner.ShowTestImage("test_trigger.png");
        
        // Assert
        var result = _appRunner.WaitForTriggerActivation();
        Assert.IsTrue(result.Triggered);
        Assert.AreEqual(KeyCode.Space, result.TriggerKey);
    }
}
```

## 测试自动化策略

### 1. 持续集成配置
```yaml
# .github/workflows/ci.yml
name: KeyForge CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run unit tests
      run: dotnet test KeyForge.Tests.csproj --logger "console;verbosity=minimal"
    
    - name: Run integration tests
      run: dotnet test KeyForge.IntegrationTests.csproj --logger "console;verbosity=minimal"
    
    - name: Run performance tests
      run: dotnet test KeyForge.PerformanceTests.csproj --logger "console;verbosity=minimal"
    
    - name: Generate coverage report
      run: dotnet test --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v1
```

### 2. 测试数据管理
```csharp
// 测试数据工厂
public class TestDataFactory
{
    public static Script CreateTestScript()
    {
        return new Script
        {
            Name = "Test Script",
            Description = "Generated test script",
            Actions = new List<ScriptAction>
            {
                new ScriptAction
                {
                    Type = ActionType.KeyDown,
                    KeyCode = KeyCode.A,
                    Delay = 100
                },
                new ScriptAction
                {
                    Type = ActionType.KeyUp,
                    KeyCode = KeyCode.A,
                    Delay = 100
                }
            }
        };
    }
    
    public static Condition CreateTestCondition()
    {
        return new Condition
        {
            Operator = ConditionOperator.Equals,
            LeftValue = "test",
            RightValue = "test"
        };
    }
    
    public static void CreateTestImage(string path, string content)
    {
        using (var bitmap = new Bitmap(100, 100))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);
            graphics.DrawString(content, new Font("Arial", 12), Brushes.Black, 10, 10);
            bitmap.Save(path, ImageFormat.Png);
        }
    }
}
```

### 3. 测试环境配置
```json
// test-settings.json
{
  "TestSettings": {
    "ImageTestDirectory": "test-images",
    "LogTestDirectory": "test-logs",
    "PerformanceThresholds": {
      "ImageRecognitionMaxTime": 200,
      "ScriptExecutionMaxTime": 100,
      "MemoryLeakThreshold": 10485760
    },
    "TestTimeouts": {
      "UnitTestTimeout": 5000,
      "IntegrationTestTimeout": 30000,
      "PerformanceTestTimeout": 60000
    }
  }
}
```

## 质量保证措施

### 1. 代码质量检查

#### 1.1 静态代码分析
```bash
# 使用SonarQube进行代码质量分析
dotnet sonarscanner begin /k:"KeyForge" /n:"KeyForge" /v:"1.0"
dotnet build
dotnet sonarscanner end
```

#### 1.2 代码规范检查
```csharp
// 代码规范检查示例
public class CodeQualityAnalyzer
{
    public CodeQualityReport AnalyzeProject(string projectPath)
    {
        var report = new CodeQualityReport();
        
        // 检查方法长度
        var longMethods = FindLongMethods(projectPath);
        report.LongMethods = longMethods;
        
        // 检查复杂度
        var complexMethods = FindComplexMethods(projectPath);
        report.ComplexMethods = complexMethods;
        
        // 检查命名规范
        var namingIssues = FindNamingIssues(projectPath);
        report.NamingIssues = namingIssues;
        
        return report;
    }
    
    private List<MethodInfo> FindLongMethods(string projectPath)
    {
        // 实现方法长度检查逻辑
        return new List<MethodInfo>();
    }
}
```

### 2. 性能监控

#### 2.1 性能指标收集
```csharp
// 性能监控服务
public class PerformanceMonitor
{
    private readonly ILogger _logger;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    
    public PerformanceMonitor(ILogger logger)
    {
        _logger = logger;
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
    }
    
    public void StartMonitoring()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var cpuUsage = _cpuCounter.NextValue();
                var availableMemory = _memoryCounter.NextValue();
                
                _logger.Info($"CPU Usage: {cpuUsage}%, Available Memory: {availableMemory}MB");
                
                if (cpuUsage > 80)
                {
                    _logger.Warning($"High CPU usage detected: {cpuUsage}%");
                }
                
                if (availableMemory < 100)
                {
                    _logger.Warning($"Low memory available: {availableMemory}MB");
                }
                
                await Task.Delay(5000);
            }
        });
    }
}
```

### 3. 错误监控

#### 3.1 错误报告系统
```csharp
// 错误报告服务
public class ErrorReportService
{
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;
    
    public ErrorReportService(ILogger logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
    }
    
    private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = (Exception)e.ExceptionObject;
        var report = CreateErrorReport(ex);
        
        _logger.Error($"Unhandled exception: {ex.Message}", ex);
        _emailService.SendErrorReport(report);
    }
    
    private ErrorReport CreateErrorReport(Exception ex)
    {
        return new ErrorReport
        {
            Exception = ex,
            Timestamp = DateTime.Now,
            Environment = Environment.GetEnvironmentVariables(),
            UserAction = GetLastUserAction(),
            SystemState = GetSystemState()
        };
    }
}
```

### 4. 用户体验测试

#### 4.1 可用性测试
```csharp
// 可用性测试工具
public class UsabilityTester
{
    public UsabilityTestResult RunTest(TestScenario scenario)
    {
        var result = new UsabilityTestResult();
        var stopwatch = Stopwatch.StartNew();
        
        // 执行测试场景
        result.Success = ExecuteScenario(scenario);
        result.TimeToComplete = stopwatch.Elapsed;
        
        // 收集用户反馈
        result.UserFeedback = CollectUserFeedback();
        
        // 计算可用性分数
        result.UsabilityScore = CalculateUsabilityScore(result);
        
        return result;
    }
    
    private bool ExecuteScenario(TestScenario scenario)
    {
        // 实现场景执行逻辑
        return true;
    }
    
    private UserFeedback CollectUserFeedback()
    {
        // 实现用户反馈收集逻辑
        return new UserFeedback();
    }
    
    private double CalculateUsabilityScore(UsabilityTestResult result)
    {
        // 实现可用性分数计算逻辑
        return 0.0;
    }
}
```

## 测试文档和报告

### 1. 测试计划模板
```markdown
# KeyForge 测试计划

## 测试范围
- 单元测试：核心业务逻辑
- 集成测试：模块间交互
- 性能测试：关键指标验证
- 用户测试：实际使用场景

## 测试环境
- 开发环境：Windows 10/11, .NET 6.0
- 测试数据：标准测试图像和脚本
- 测试工具：xUnit, Moq, BenchmarkDotNet

## 测试进度
- 第1周：单元测试框架搭建
- 第2周：核心功能单元测试
- 第3周：集成测试开发
- 第4周：性能测试和用户测试

## 测试标准
- 代码覆盖率：≥60%
- 性能指标：达到设计要求
- 用户体验：满意度≥4.0/5.0
- 错误率：≤1%
```

### 2. 测试报告模板
```markdown
# KeyForge 测试报告

## 测试总结
- 测试通过率：95%
- 代码覆盖率：65%
- 发现缺陷：12个
- 修复缺陷：10个

## 功能测试结果
- 按键录制回放：通过
- 图像识别功能：通过
- 条件判断功能：通过
- 用户界面功能：通过

## 性能测试结果
- 系统启动时间：3.2秒
- 图像识别时间：180ms
- 内存占用：150MB
- CPU使用率：15%

## 建议和改进
- 优化图像识别算法
- 改进用户界面响应速度
- 增加错误处理机制
- 完善用户文档
```

## 持续改进

### 1. 测试反馈循环
```csharp
// 测试反馈服务
public class TestFeedbackService
{
    private readonly ITestResultStorage _storage;
    private readonly IAnalysisService _analysis;
    
    public TestFeedbackService(ITestResultStorage storage, IAnalysisService analysis)
    {
        _storage = storage;
        _analysis = analysis;
    }
    
    public void ProcessTestResults(IEnumerable<TestResult> results)
    {
        // 存储测试结果
        _storage.StoreResults(results);
        
        // 分析测试结果
        var analysis = _analysis.AnalyzeResults(results);
        
        // 生成改进建议
        var recommendations = GenerateRecommendations(analysis);
        
        // 应用改进措施
        ApplyImprovements(recommendations);
    }
    
    private void ApplyImprovements(IEnumerable<ImprovementRecommendation> recommendations)
    {
        foreach (var recommendation in recommendations)
        {
            switch (recommendation.Type)
            {
                case ImprovementType.Performance:
                    ApplyPerformanceImprovement(recommendation);
                    break;
                case ImprovementType.Reliability:
                    ApplyReliabilityImprovement(recommendation);
                    break;
                case ImprovementType.Usability:
                    ApplyUsabilityImprovement(recommendation);
                    break;
            }
        }
    }
}
```

这个测试策略文档提供了一个全面的测试方案，包括单元测试、集成测试、性能测试和用户验收测试，确保系统的质量和稳定性。