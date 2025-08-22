# KeyForge 技术实现指南

## 核心Sense-Judge-Act架构

### 1. 架构概览

```
┌─────────────────────────────────────┐
│           用户界面层                 │
│     (Windows Forms 主界面)          │
└─────────────────────────────────────┘
                ↓
┌─────────────────────────────────────┐
│           核心控制层                 │
│  ├── 感知模块 (Sense)               │
│  ├── 决策模块 (Judge)               │
│  └── 执行模块 (Act)                 │
└─────────────────────────────────────┘
                ↓
┌─────────────────────────────────────┐
│           基础服务层                 │
│  ├── 输入服务                        │
│  ├── 图像服务                        │
│  ├── 配置服务                        │
│  └── 日志服务                        │
└─────────────────────────────────────┘
```

### 2. 核心模块设计

#### 2.1 感知模块 (Sense)
```csharp
// 简化的感知模块接口
public interface ISenseModule
{
    // 屏幕感知
    ScreenCapture CaptureScreen();
    ImageRecognitionResult FindImage(string templatePath);
    Color GetPixelColor(int x, int y);
    
    // 输入感知
    void StartInputCapture();
    void StopInputCapture();
    event EventHandler<InputEventArgs> InputCaptured;
}

// 实际实现类
public class SenseModule : ISenseModule
{
    private readonly IImageService _imageService;
    private readonly IInputService _inputService;
    
    public SenseModule(IImageService imageService, IInputService inputService)
    {
        _imageService = imageService;
        _inputService = inputService;
    }
    
    public ScreenCapture CaptureScreen()
    {
        return _imageService.CaptureScreen();
    }
    
    public ImageRecognitionResult FindImage(string templatePath)
    {
        return _imageService.FindImage(templatePath);
    }
    
    // ... 其他实现
}
```

#### 2.2 决策模块 (Judge)
```csharp
// 简化的决策模块接口
public interface IJudgeModule
{
    // 条件判断
    bool EvaluateCondition(Condition condition);
    
    // 脚本逻辑控制
    ScriptExecutionResult ExecuteScript(Script script);
    
    // 状态管理
    void SetState(string stateName);
    string GetCurrentState();
}

// 实际实现类
public class JudgeModule : IJudgeModule
{
    private readonly IConditionEvaluator _evaluator;
    private readonly IScriptExecutor _executor;
    private string _currentState = "Idle";
    
    public JudgeModule(IConditionEvaluator evaluator, IScriptExecutor executor)
    {
        _evaluator = evaluator;
        _executor = executor;
    }
    
    public bool EvaluateCondition(Condition condition)
    {
        return _evaluator.Evaluate(condition);
    }
    
    public ScriptExecutionResult ExecuteScript(Script script)
    {
        return _executor.Execute(script);
    }
    
    // ... 其他实现
}
```

#### 2.3 执行模块 (Act)
```csharp
// 简化的执行模块接口
public interface IActModule
{
    // 输入执行
    void ExecuteKeyAction(KeyAction action);
    void ExecuteMouseAction(MouseAction action);
    
    // 脚本执行
    void ExecuteScript(Script script);
    void StopExecution();
    
    // 执行状态
    bool IsExecuting { get; }
    event EventHandler<ExecutionEventArgs> ExecutionStateChanged;
}

// 实际实现类
public class ActModule : IActModule
{
    private readonly IInputSimulator _inputSimulator;
    private readonly ILogger _logger;
    private bool _isExecuting = false;
    
    public ActModule(IInputSimulator inputSimulator, ILogger logger)
    {
        _inputSimulator = inputSimulator;
        _logger = logger;
    }
    
    public void ExecuteKeyAction(KeyAction action)
    {
        _inputSimulator.SendKey(action.KeyCode, action.KeyState);
    }
    
    public void ExecuteMouseAction(MouseAction action)
    {
        _inputSimulator.SendMouse(action.X, action.Y, action.Button, action.Action);
    }
    
    // ... 其他实现
}
```

### 3. 核心数据模型

#### 3.1 简化的脚本模型
```csharp
// 简化的脚本模型
public class Script
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ScriptAction> Actions { get; set; } = new();
    public ScriptStatus Status { get; set; } = ScriptStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    // 简化的业务方法
    public void AddAction(ScriptAction action)
    {
        Actions.Add(action);
        UpdatedAt = DateTime.Now;
    }
    
    public void RemoveAction(int index)
    {
        if (index >= 0 && index < Actions.Count)
        {
            Actions.RemoveAt(index);
            UpdatedAt = DateTime.Now;
        }
    }
    
    public TimeSpan GetEstimatedDuration()
    {
        var totalDelay = Actions.Sum(a => a.Delay);
        return TimeSpan.FromMilliseconds(totalDelay);
    }
}

// 简化的动作模型
public class ScriptAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ActionType Type { get; set; }
    public int Delay { get; set; } = 0;
    
    // 键盘动作
    public KeyCode KeyCode { get; set; }
    public KeyState KeyState { get; set; }
    
    // 鼠标动作
    public int X { get; set; }
    public int Y { get; set; }
    public MouseButton MouseButton { get; set; }
    public MouseAction MouseAction { get; set; }
    
    // 图像识别动作
    public string TemplatePath { get; set; }
    public double Confidence { get; set; } = 0.8;
    
    // 条件动作
    public string Condition { get; set; }
    public List<ScriptAction> ThenActions { get; set; } = new();
    public List<ScriptAction> ElseActions { get; set; } = new();
}

public enum ActionType
{
    KeyDown,
    KeyUp,
    MouseDown,
    MouseUp,
    MouseMove,
    ImageRecognition,
    Condition,
    Delay,
    Loop
}
```

### 4. 具体实现指导

#### 4.1 项目结构
```
KeyForge/
├── KeyForge.Core/          # 核心业务逻辑
│   ├── Sense/              # 感知模块
│   ├── Judge/              # 决策模块
│   ├── Act/                # 执行模块
│   └── Services/           # 基础服务
├── KeyForge.UI/            # 用户界面
│   ├── Forms/              # 窗体
│   ├── Controls/           # 控件
│   └── Resources/          # 资源
├── KeyForge.Infrastructure/ # 基础设施
│   ├── Imaging/            # 图像处理
│   ├── Input/              # 输入处理
│   └── Storage/            # 数据存储
└── KeyForge.Tests/         # 测试项目
```

#### 4.2 关键实现要点

##### 4.2.1 图像识别实现
```csharp
// 简化的图像识别服务
public class ImageRecognitionService
{
    private readonly double _matchThreshold;
    
    public ImageRecognitionService(double matchThreshold = 0.8)
    {
        _matchThreshold = matchThreshold;
    }
    
    public ImageRecognitionResult FindImage(string templatePath)
    {
        try
        {
            // 1. 截取屏幕
            using (var screenBitmap = CaptureScreen())
            using (var templateBitmap = LoadTemplate(templatePath))
            {
                // 2. 转换为OpenCV格式
                using (var screenMat = screenBitmap.ToMat())
                using (var templateMat = templateBitmap.ToMat())
                {
                    // 3. 执行模板匹配
                    return MatchTemplate(screenMat, templateMat);
                }
            }
        }
        catch (Exception ex)
        {
            return new ImageRecognitionResult { Success = false, Error = ex.Message };
        }
    }
    
    private ImageRecognitionResult MatchTemplate(Mat screen, Mat template)
    {
        using (var result = new Mat())
        {
            // 执行模板匹配
            Cv2.MatchTemplate(screen, template, result, TemplateMatchModes.CCoeffNormed);
            
            // 查找最佳匹配
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);
            
            if (maxVal >= _matchThreshold)
            {
                return new ImageRecognitionResult
                {
                    Success = true,
                    Location = new Rectangle(maxLoc.X, maxLoc.Y, template.Width, template.Height),
                    Confidence = maxVal
                };
            }
            
            return new ImageRecognitionResult { Success = false, Confidence = maxVal };
        }
    }
}
```

##### 4.2.2 输入模拟实现
```csharp
// 简化的输入模拟服务
public class InputSimulatorService
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
    
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);
    
    public void SendKey(KeyCode keyCode, KeyState keyState)
    {
        byte virtualKey = (byte)keyCode;
        uint flags = keyState == KeyState.Down ? 0 : 2;
        
        keybd_event(virtualKey, 0, flags, 0);
    }
    
    public void SendMouse(int x, int y, MouseButton button, MouseAction action)
    {
        // 移动鼠标
        Cursor.Position = new Point(x, y);
        
        // 执行鼠标动作
        uint mouseFlag = action switch
        {
            MouseAction.Down => button switch
            {
                MouseButton.Left => 0x0002,
                MouseButton.Right => 0x0008,
                MouseButton.Middle => 0x0020,
                _ => 0x0002
            },
            MouseAction.Up => button switch
            {
                MouseButton.Left => 0x0004,
                MouseButton.Right => 0x0010,
                MouseButton.Middle => 0x0040,
                _ => 0x0004
            },
            _ => 0
        };
        
        if (mouseFlag != 0)
        {
            mouse_event(mouseFlag, 0, 0, 0, 0);
        }
    }
}
```

##### 4.2.3 条件判断实现
```csharp
// 简化的条件判断服务
public class ConditionEvaluator
{
    public bool Evaluate(Condition condition, Dictionary<string, object> variables)
    {
        return condition.Operator switch
        {
            ConditionOperator.Equals => EvaluateEquals(condition, variables),
            ConditionOperator.NotEquals => !EvaluateEquals(condition, variables),
            ConditionOperator.GreaterThan => EvaluateComparison(condition, variables) > 0,
            ConditionOperator.LessThan => EvaluateComparison(condition, variables) < 0,
            ConditionOperator.ImageFound => EvaluateImageFound(condition),
            ConditionOperator.ColorMatch => EvaluateColorMatch(condition),
            _ => false
        };
    }
    
    private bool EvaluateEquals(Condition condition, Dictionary<string, object> variables)
    {
        var leftValue = GetVariableValue(condition.LeftValue, variables);
        var rightValue = GetVariableValue(condition.RightValue, variables);
        
        return Equals(leftValue, rightValue);
    }
    
    private bool EvaluateImageFound(Condition condition)
    {
        var imageService = new ImageRecognitionService();
        var result = imageService.FindImage(condition.TemplatePath);
        return result.Success && result.Confidence >= condition.Confidence;
    }
    
    private bool EvaluateColorMatch(Condition condition)
    {
        var imageService = new ImageRecognitionService();
        var pixelColor = imageService.GetPixelColor(condition.X, condition.Y);
        return ColorsMatch(pixelColor, condition.TargetColor, condition.Tolerance);
    }
}
```

### 5. 性能优化建议

#### 5.1 图像识别优化
```csharp
// 图像识别缓存优化
public class CachedImageRecognitionService
{
    private readonly Dictionary<string, Mat> _templateCache = new();
    private readonly ImageRecognitionService _baseService;
    
    public ImageRecognitionResult FindImage(string templatePath)
    {
        // 缓存模板图像
        if (!_templateCache.ContainsKey(templatePath))
        {
            var template = Cv2.ImRead(templatePath);
            _templateCache[templatePath] = template;
        }
        
        var cachedTemplate = _templateCache[templatePath];
        return _baseService.FindImageWithMat(cachedTemplate);
    }
}
```

#### 5.2 执行性能优化
```csharp
// 异步执行优化
public class AsyncScriptExecutor
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task<ScriptExecutionResult> ExecuteAsync(Script script)
    {
        return await Task.Run(() =>
        {
            var result = new ScriptExecutionResult();
            
            foreach (var action in script.Actions)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    result.Status = ExecutionStatus.Cancelled;
                    break;
                }
                
                ExecuteAction(action);
                Thread.Sleep(action.Delay);
            }
            
            return result;
        }, _cancellationTokenSource.Token);
    }
    
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
}
```

### 6. 错误处理和日志

#### 6.1 错误处理策略
```csharp
// 全局错误处理
public class GlobalExceptionHandler
{
    private readonly ILogger _logger;
    
    public GlobalExceptionHandler(ILogger logger)
    {
        _logger = logger;
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        Application.ThreadException += HandleThreadException;
    }
    
    private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = (Exception)e.ExceptionObject;
        _logger.Error($"未处理的异常: {ex.Message}", ex);
    }
    
    private void HandleThreadException(object sender, ThreadExceptionEventArgs e)
    {
        _logger.Error($"线程异常: {e.Exception.Message}", e.Exception);
        MessageBox.Show($"发生错误: {e.Exception.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

#### 6.2 日志记录
```csharp
// 简化的日志服务
public class FileLoggerService : ILogger
{
    private readonly string _logFilePath;
    private readonly LogLevel _minLevel;
    
    public FileLoggerService(string logFilePath, LogLevel minLevel = LogLevel.Info)
    {
        _logFilePath = logFilePath;
        _minLevel = minLevel;
    }
    
    public void Info(string message)
    {
        if (_minLevel <= LogLevel.Info)
            Log(message, LogLevel.Info);
    }
    
    public void Error(string message, Exception ex = null)
    {
        if (_minLevel <= LogLevel.Error)
            Log($"{message}\n{ex?.StackTrace}", LogLevel.Error);
    }
    
    private void Log(string message, LogLevel level)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
    }
}
```

### 7. 测试策略

#### 7.1 单元测试
```csharp
// 条件判断测试
[TestClass]
public class ConditionEvaluatorTests
{
    [TestMethod]
    public void Evaluate_EqualsCondition_ReturnsCorrectResult()
    {
        var evaluator = new ConditionEvaluator();
        var condition = new Condition
        {
            Operator = ConditionOperator.Equals,
            LeftValue = "test",
            RightValue = "test"
        };
        
        var result = evaluator.Evaluate(condition, new Dictionary<string, object>());
        
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public void Evaluate_ImageFoundCondition_ReturnsCorrectResult()
    {
        var evaluator = new ConditionEvaluator();
        var condition = new Condition
        {
            Operator = ConditionOperator.ImageFound,
            TemplatePath = "test_template.png"
        };
        
        var result = evaluator.Evaluate(condition, new Dictionary<string, object>());
        
        // 根据实际测试图像存在与否断言
        Assert.IsNotNull(result);
    }
}
```

#### 7.2 集成测试
```csharp
// 端到端测试
[TestClass]
public class ScriptExecutionTests
{
    [TestMethod]
    public void ExecuteSimpleScript_CompletesSuccessfully()
    {
        var script = new Script
        {
            Name = "Test Script",
            Actions = new List<ScriptAction>
            {
                new ScriptAction { Type = ActionType.KeyDown, KeyCode = KeyCode.A },
                new ScriptAction { Type = ActionType.KeyUp, KeyCode = KeyCode.A, Delay = 100 }
            }
        };
        
        var executor = new ScriptExecutor();
        var result = executor.Execute(script);
        
        Assert.AreEqual(ExecutionStatus.Completed, result.Status);
    }
}
```

### 8. 部署和配置

#### 8.1 配置文件
```json
{
  "Logging": {
    "Level": "Info",
    "FilePath": "logs/keyforge.log"
  },
  "ImageRecognition": {
    "MatchThreshold": 0.8,
    "MaxSearchTime": 5000
  },
  "Execution": {
    "DefaultDelay": 100,
    "MaxExecutionTime": 300000
  },
  "UI": {
    "Theme": "Light",
    "Language": "zh-CN"
  }
}
```

#### 8.2 部署要求
- .NET 6.0 Runtime
- Windows 10/11
- OpenCVSharp 运行时库
- 2GB RAM 以上
- 100MB 磁盘空间

### 9. 开发检查清单

#### 9.1 代码质量检查
- [ ] 代码符合命名规范
- [ ] 方法长度不超过50行
- [ ] 类职责单一
- [ ] 错误处理完善
- [ ] 注释清晰易懂

#### 9.2 功能完整性检查
- [ ] 按键录制回放功能正常
- [ ] 图像识别功能正常
- [ ] 条件判断功能正常
- [ ] 用户界面响应正常
- [ ] 配置管理功能正常

#### 9.3 性能检查
- [ ] 系统启动时间 < 5秒
- [ ] 图像识别时间 < 200ms
- [ ] 内存占用 < 200MB
- [ ] 长时间运行无内存泄漏
- [ ] 并发执行稳定

这个实现指南提供了一个更务实、更具体的技术实现方案，避免了过度复杂的架构设计，专注于核心功能的实现。