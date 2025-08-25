# KeyForge 接口设计规范

## 📋 概述

本文档定义了KeyForge项目的完整接口设计规范，包括所有核心接口、数据模型、枚举定义和交互模式。这些接口设计确保了系统的跨平台兼容性、可扩展性和可测试性。

## 🏗️ 接口分层设计

### 1.1 接口层次结构

```
KeyForge.Abstractions/
├── Interfaces/
│   ├── Core/
│   │   ├── IInputService.cs
│   │   ├── IImageService.cs
│   │   ├── IScriptService.cs
│   │   └── IConfigService.cs
│   ├── HAL/
│   │   ├── IInputHAL.cs
│   │   ├── IGraphicsHAL.cs
│   │   └── ISystemHAL.cs
│   └── Application/
│       ├── IUseCase.cs
│       ├── ICommandHandler.cs
│       └── IQueryHandler.cs
├── Models/
│   ├── Input/
│   │   ├── KeyInput.cs
│   │   ├── MouseInput.cs
│   │   └── InputEventArgs.cs
│   ├── Image/
│   │   ├── ImageTemplate.cs
│   │   ├── RecognitionResult.cs
│   │   └── ScreenRegion.cs
│   ├── Script/
│   │   ├── Script.cs
│   │   ├── ScriptContext.cs
│   │   └── ExecutionResult.cs
│   └── Configuration/
│       ├── Configuration.cs
│       ├── ConfigSection.cs
│       └── ConfigKey.cs
└── Enums/
    ├── KeyCode.cs
    ├── ActionType.cs
    ├── ScriptStatus.cs
    └── RecognitionStatus.cs
```

## 🔧 核心接口定义

### 2.1 输入系统接口

#### 2.1.1 基础输入接口
```csharp
namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 输入服务基础接口
    /// </summary>
    public interface IInputService : IDisposable
    {
        /// <summary>
        /// 初始化输入服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 启动输入监听
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// 停止输入监听
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// 输入事件触发
        /// </summary>
        event EventHandler<InputEventArgs> OnInput;
        
        /// <summary>
        /// 服务状态
        /// </summary>
        ServiceStatus Status { get; }
    }
    
    /// <summary>
    /// 键盘输入服务接口
    /// </summary>
    public interface IKeyboardService : IInputService
    {
        /// <summary>
        /// 发送键盘按键
        /// </summary>
        Task<bool> SendKeyAsync(KeyCode keyCode, KeyState state);
        
        /// <summary>
        /// 发送文本输入
        /// </summary>
        Task<bool> SendTextAsync(string text);
        
        /// <summary>
        /// 模拟按键组合
        /// </summary>
        Task<bool> SendKeyCombinationAsync(KeyCode[] keyCodes);
        
        /// <summary>
        /// 获取按键状态
        /// </summary>
        KeyState GetKeyState(KeyCode keyCode);
        
        /// <summary>
        /// 键盘事件
        /// </summary>
        event EventHandler<KeyInputEventArgs> OnKeyEvent;
    }
    
    /// <summary>
    /// 鼠标输入服务接口
    /// </summary>
    public interface IMouseService : IInputService
    {
        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        Task<bool> MoveMouseAsync(int x, int y);
        
        /// <summary>
        /// 相对移动鼠标
        /// </summary>
        Task<bool> MoveMouseRelativeAsync(int deltaX, int deltaY);
        
        /// <summary>
        /// 发送鼠标按键
        /// </summary>
        Task<bool> SendMouseButtonAsync(MouseButton button, MouseState state);
        
        /// <summary>
        /// 发送鼠标点击
        /// </summary>
        Task<bool> SendMouseClickAsync(MouseButton button);
        
        /// <summary>
        /// 发送鼠标双击
        /// </summary>
        Task<bool> SendMouseDoubleClickAsync(MouseButton button);
        
        /// <summary>
        /// 发送鼠标右键点击
        /// </summary>
        Task<bool> SendMouseRightClickAsync();
        
        /// <summary>
        /// 发送鼠标滚轮
        /// </summary>
        Task<bool> SendMouseWheelAsync(int delta);
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        Point GetMousePosition();
        
        /// <summary>
        /// 鼠标事件
        /// </summary>
        event EventHandler<MouseInputEventArgs> OnMouseEvent;
    }
    
    /// <summary>
    /// 输入钩子服务接口
    /// </summary>
    public interface IInputHookService : IDisposable
    {
        /// <summary>
        /// 设置键盘钩子
        /// </summary>
        Task<bool> SetKeyboardHookAsync();
        
        /// <summary>
        /// 设置鼠标钩子
        /// </summary>
        Task<bool> SetMouseHookAsync();
        
        /// <summary>
        /// 移除钩子
        /// </summary>
        Task RemoveHooksAsync();
        
        /// <summary>
        /// 键盘钩子事件
        /// </summary>
        event EventHandler<KeyHookEventArgs> OnKeyboardHook;
        
        /// <summary>
        /// 鼠标钩子事件
        /// </summary>
        event EventHandler<MouseHookEventArgs> OnMouseHook;
        
        /// <summary>
        /// 钩子状态
        /// </summary>
        HookStatus Status { get; }
    }
}
```

#### 2.1.2 输入事件参数
```csharp
namespace KeyForge.Abstractions.Models.Input
{
    /// <summary>
    /// 输入事件基类
    /// </summary>
    public class InputEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }
        public InputType InputType { get; }
        
        protected InputEventArgs(InputType inputType)
        {
            Timestamp = DateTime.Now;
            InputType = inputType;
        }
    }
    
    /// <summary>
    /// 键盘输入事件参数
    /// </summary>
    public class KeyInputEventArgs : InputEventArgs
    {
        public KeyCode KeyCode { get; }
        public KeyState KeyState { get; }
        public bool IsExtended { get; }
        public uint ScanCode { get; }
        
        public KeyInputEventArgs(KeyCode keyCode, KeyState keyState, bool isExtended = false, uint scanCode = 0)
            : base(InputType.Keyboard)
        {
            KeyCode = keyCode;
            KeyState = keyState;
            IsExtended = isExtended;
            ScanCode = scanCode;
        }
    }
    
    /// <summary>
    /// 鼠标输入事件参数
    /// </summary>
    public class MouseInputEventArgs : InputEventArgs
    {
        public MouseButton Button { get; }
        public MouseState State { get; }
        public Point Position { get; }
        public int WheelDelta { get; }
        
        public MouseInputEventArgs(MouseButton button, MouseState state, Point position, int wheelDelta = 0)
            : base(InputType.Mouse)
        {
            Button = button;
            State = state;
            Position = position;
            WheelDelta = wheelDelta;
        }
    }
    
    /// <summary>
    /// 键盘钩子事件参数
    /// </summary>
    public class KeyHookEventArgs : KeyInputEventArgs
    {
        public bool IsHandled { get; set; }
        public IntPtr WindowHandle { get; }
        
        public KeyHookEventArgs(KeyCode keyCode, KeyState keyState, IntPtr windowHandle, bool isExtended = false, uint scanCode = 0)
            : base(keyCode, keyState, isExtended, scanCode)
        {
            WindowHandle = windowHandle;
            IsHandled = false;
        }
    }
    
    /// <summary>
    /// 鼠标钩子事件参数
    /// </summary>
    public class MouseHookEventArgs : MouseInputEventArgs
    {
        public bool IsHandled { get; set; }
        public IntPtr WindowHandle { get; }
        
        public MouseHookEventArgs(MouseButton button, MouseState state, Point position, IntPtr windowHandle, int wheelDelta = 0)
            : base(button, state, position, wheelDelta)
        {
            WindowHandle = windowHandle;
            IsHandled = false;
        }
    }
}
```

### 2.2 图像识别系统接口

#### 2.2.1 图像识别服务接口
```csharp
namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 图像识别服务接口
    /// </summary>
    public interface IImageRecognitionService : IDisposable
    {
        /// <summary>
        /// 初始化图像识别服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 识别图像模板
        /// </summary>
        Task<RecognitionResult> RecognizeAsync(ImageTemplate template, ScreenRegion region = null);
        
        /// <summary>
        /// 批量识别图像模板
        /// </summary>
        Task<List<RecognitionResult>> RecognizeBatchAsync(List<ImageTemplate> templates, ScreenRegion region = null);
        
        /// <summary>
        /// 查找所有匹配的图像
        /// </summary>
        Task<List<RecognitionResult>> FindAllAsync(ImageTemplate template, ScreenRegion region = null);
        
        /// <summary>
        /// 等待图像出现
        /// </summary>
        Task<RecognitionResult> WaitForImageAsync(ImageTemplate template, int timeoutMs = 10000, ScreenRegion region = null);
        
        /// <summary>
        /// 创建图像模板
        /// </summary>
        Task<ImageTemplate> CreateTemplateAsync(string name, byte[] imageData, Rectangle templateArea);
        
        /// <summary>
        /// 保存图像模板
        /// </summary>
        Task<bool> SaveTemplateAsync(ImageTemplate template);
        
        /// <summary>
        /// 加载图像模板
        /// </summary>
        Task<ImageTemplate> LoadTemplateAsync(string templateName);
        
        /// <summary>
        /// 删除图像模板
        /// </summary>
        Task<bool> DeleteTemplateAsync(string templateName);
        
        /// <summary>
        /// 获取所有模板
        /// </summary>
        Task<List<ImageTemplate>> GetAllTemplatesAsync();
        
        /// <summary>
        /// 设置识别参数
        /// </summary>
        void SetRecognitionParameters(RecognitionParameters parameters);
        
        /// <summary>
        /// 获取识别参数
        /// </summary>
        RecognitionParameters GetRecognitionParameters();
    }
    
    /// <summary>
    /// 屏幕捕获服务接口
    /// </summary>
    public interface IScreenCaptureService : IDisposable
    {
        /// <summary>
        /// 捕获整个屏幕
        /// </summary>
        Task<byte[]> CaptureScreenAsync();
        
        /// <summary>
        /// 捕获指定区域
        /// </summary>
        Task<byte[]> CaptureRegionAsync(ScreenRegion region);
        
        /// <summary>
        /// 捕获指定窗口
        /// </summary>
        Task<byte[]> CaptureWindowAsync(IntPtr windowHandle);
        
        /// <summary>
        /// 获取屏幕尺寸
        /// </summary>
        Size GetScreenSize();
        
        /// <summary>
        /// 获取屏幕DPI
        /// </summary>
        double GetScreenDpi();
        
        /// <summary>
        /// 获取主屏幕
        /// </summary>
        ScreenInfo GetPrimaryScreen();
        
        /// <summary>
        /// 获取所有屏幕
        /// </summary>
        List<ScreenInfo> GetAllScreens();
        
        /// <summary>
        /// 屏幕变化事件
        /// </summary>
        event EventHandler<ScreenChangedEventArgs> OnScreenChanged;
    }
    
    /// <summary>
    /// 模板匹配器接口
    /// </summary>
    public interface ITemplateMatcher
    {
        /// <summary>
        /// 模板匹配
        /// </summary>
        RecognitionResult Match(byte[] screenData, byte[] templateData, Rectangle searchArea);
        
        /// <summary>
        /// 多模板匹配
        /// </summary>
        List<RecognitionResult> MatchMultiple(byte[] screenData, List<byte[]> templateDataList, Rectangle searchArea);
        
        /// <summary>
        /// 设置匹配参数
        /// </summary>
        void SetMatchParameters(MatchParameters parameters);
        
        /// <summary>
        /// 获取匹配参数
        /// </summary>
        MatchParameters GetMatchParameters();
    }
}
```

#### 2.2.2 图像识别模型
```csharp
namespace KeyForge.Abstractions.Models.Image
{
    /// <summary>
    /// 图像模板模型
    /// </summary>
    public class ImageTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public Rectangle TemplateArea { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public Dictionary<string, string> Metadata { get; set; } = new();
        
        public ImageTemplate() { }
        
        public ImageTemplate(string name, byte[] imageData, Rectangle templateArea)
        {
            Name = name;
            ImageData = imageData;
            TemplateArea = templateArea;
        }
    }
    
    /// <summary>
    /// 识别结果模型
    /// </summary>
    public class RecognitionResult
    {
        public string TemplateId { get; set; } = string.Empty;
        public RecognitionStatus Status { get; set; }
        public double Confidence { get; set; }
        public Point Location { get; set; }
        public Rectangle MatchArea { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        
        public RecognitionResult()
        {
            Status = RecognitionStatus.Failed;
            Confidence = 0.0;
            Location = Point.Empty;
            MatchArea = Rectangle.Empty;
            ProcessingTime = TimeSpan.Zero;
        }
        
        public static RecognitionResult Success(string templateId, double confidence, Point location, Rectangle matchArea)
        {
            return new RecognitionResult
            {
                TemplateId = templateId,
                Status = RecognitionStatus.Success,
                Confidence = confidence,
                Location = location,
                MatchArea = matchArea
            };
        }
        
        public static RecognitionResult Failed(string errorMessage)
        {
            return new RecognitionResult
            {
                Status = RecognitionStatus.Failed,
                ErrorMessage = errorMessage
            };
        }
    }
    
    /// <summary>
    /// 屏幕区域模型
    /// </summary>
    public class ScreenRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public ScreenRegion() { }
        
        public ScreenRegion(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
        
        public static ScreenRegion FromRectangle(Rectangle rectangle)
        {
            return new ScreenRegion(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
        
        public static ScreenRegion FullScreen => new ScreenRegion(0, 0, int.MaxValue, int.MaxValue);
    }
    
    /// <summary>
    /// 识别参数模型
    /// </summary>
    public class RecognitionParameters
    {
        public double ConfidenceThreshold { get; set; } = 0.8;
        public RecognitionMethod Method { get; set; } = RecognitionMethod.TemplateMatching;
        public int MaxResults { get; set; } = 1;
        public bool MultiScale { get; set; } = false;
        public double ScaleFactor { get; set; } = 1.1;
        public int MaxIterations { get; set; } = 10;
        public bool EnableCaching { get; set; } = true;
        public int CacheTimeoutMs { get; set; } = 1000;
    }
    
    /// <summary>
    /// 匹配参数模型
    /// </summary>
    public class MatchParameters
    {
        public double MinConfidence { get; set; } = 0.8;
        public bool NormalizeColors { get; set; } = true;
        public bool EnableRotation { get; set; } = false;
        public double MaxRotationAngle { get; set; } = 15.0;
        public bool EnableScaling { get; set; } = true;
        public double MinScale { get; set; } = 0.5;
        public double MaxScale { get; set; } = 2.0;
    }
    
    /// <summary>
    /// 屏幕信息模型
    /// </summary>
    public class ScreenInfo
    {
        public int Index { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public Rectangle WorkingArea { get; set; }
        public double Dpi { get; set; } = 96.0;
        public bool IsPrimary { get; set; }
    }
    
    /// <summary>
    /// 屏幕变化事件参数
    /// </summary>
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangeType ChangeType { get; set; }
        public ScreenInfo? ScreenInfo { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public ScreenChangedEventArgs(ScreenChangeType changeType, ScreenInfo? screenInfo = null)
        {
            ChangeType = changeType;
            ScreenInfo = screenInfo;
        }
    }
}
```

### 2.3 脚本执行引擎接口

#### 2.3.1 脚本引擎接口
```csharp
namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 脚本引擎接口
    /// </summary>
    public interface IScriptEngine : IDisposable
    {
        /// <summary>
        /// 初始化脚本引擎
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 编译脚本
        /// </summary>
        Task<CompilationResult> CompileAsync(Script script);
        
        /// <summary>
        /// 执行脚本
        /// </summary>
        Task<ExecutionResult> ExecuteAsync(Script script, ScriptContext context = null);
        
        /// <summary>
        /// 调试脚本
        /// </summary>
        Task<DebugResult> DebugAsync(Script script, ScriptContext context = null);
        
        /// <summary>
        /// 停止脚本执行
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// 暂停脚本执行
        /// </summary>
        Task PauseAsync();
        
        /// <summary>
        /// 恢复脚本执行
        /// </summary>
        Task ResumeAsync();
        
        /// <summary>
        /// 获取脚本状态
        /// </summary>
        ScriptStatus GetScriptStatus(string scriptId);
        
        /// <summary>
        /// 获取执行统计
        /// </summary>
        ExecutionStatistics GetExecutionStatistics(string scriptId);
        
        /// <summary>
        /// 设置断点
        /// </summary>
        bool SetBreakpoint(string scriptId, int lineNumber);
        
        /// <summary>
        /// 清除断点
        /// </summary>
        bool ClearBreakpoint(string scriptId, int lineNumber);
        
        /// <summary>
        /// 获取变量值
        /// </summary>
        object GetVariableValue(string scriptId, string variableName);
        
        /// <summary>
        /// 设置变量值
        /// </summary>
        bool SetVariableValue(string scriptId, string variableName, object value);
        
        /// <summary>
        /// 脚本执行事件
        /// </summary>
        event EventHandler<ScriptExecutionEventArgs> OnScriptExecution;
        
        /// <summary>
        /// 脚本状态变化事件
        /// </summary>
        event EventHandler<ScriptStatusEventArgs> OnScriptStatusChanged;
        
        /// <summary>
        /// 引擎状态
        /// </summary>
        EngineStatus Status { get; }
    }
    
    /// <summary>
    /// 脚本执行器接口
    /// </summary>
    public interface IScriptExecutor : IDisposable
    {
        /// <summary>
        /// 执行脚本
        /// </summary>
        Task<ExecutionResult> ExecuteAsync(Script script, ScriptContext context = null);
        
        /// <summary>
        /// 执行脚本动作序列
        /// </summary>
        Task<ExecutionResult> ExecuteActionsAsync(List<ScriptAction> actions, ScriptContext context = null);
        
        /// <summary>
        /// 执行单个动作
        /// </summary>
        Task<ActionResult> ExecuteActionAsync(ScriptAction action, ScriptContext context = null);
        
        /// <summary>
        /// 验证脚本
        /// </summary>
        Task<ValidationResult> ValidateAsync(Script script);
        
        /// <summary>
        /// 优化脚本
        /// </summary>
        Task<Script> OptimizeAsync(Script script);
        
        /// <summary>
        /// 获取执行计划
        /// </summary>
        ExecutionPlan GetExecutionPlan(Script script);
        
        /// <summary>
        /// 执行器状态
        /// </summary>
        ExecutorStatus Status { get; }
    }
    
    /// <summary>
    /// 脚本编译器接口
    /// </summary>
    public interface IScriptCompiler : IDisposable
    {
        /// <summary>
        /// 编译脚本
        /// </summary>
        Task<CompilationResult> CompileAsync(Script script);
        
        /// <summary>
        /// 验证脚本语法
        /// </summary>
        Task<ValidationResult> ValidateSyntaxAsync(string scriptCode);
        
        /// <summary>
        /// 预处理脚本
        /// </summary>
        Task<string> PreprocessAsync(string scriptCode);
        
        /// <summary>
        /// 生成中间代码
        /// </summary>
        Task<string> GenerateIntermediateCodeAsync(Script script);
        
        /// <summary>
        /// 优化中间代码
        /// </summary>
        Task<string> OptimizeIntermediateCodeAsync(string intermediateCode);
        
        /// <summary>
        /// 编译器状态
        /// </summary>
        CompilerStatus Status { get; }
    }
    
    /// <summary>
    /// 脚本调试器接口
    /// </summary>
    public interface IScriptDebugger : IDisposable
    {
        /// <summary>
        /// 开始调试
        /// </summary>
        Task<DebugSession> StartDebuggingAsync(Script script);
        
        /// <summary>
        /// 单步执行
        /// </summary>
        Task<DebugStepResult> StepOverAsync(DebugSession session);
        
        /// <summary>
        /// 单步进入
        /// </summary>
        Task<DebugStepResult> StepIntoAsync(DebugSession session);
        
        /// <summary>
        /// 单步跳出
        /// </summary>
        Task<DebugStepResult> StepOutAsync(DebugSession session);
        
        /// <summary>
        /// 继续执行
        /// </summary>
        Task ContinueAsync(DebugSession session);
        
        /// <summary>
        /// 设置断点
        /// </summary>
        Task<bool> SetBreakpointAsync(DebugSession session, int lineNumber);
        
        /// <summary>
        /// 清除断点
        /// </summary>
        Task<bool> ClearBreakpointAsync(DebugSession session, int lineNumber);
        
        /// <summary>
        /// 获取调用堆栈
        /// </summary>
        Task<List<StackFrame>> GetCallStackAsync(DebugSession session);
        
        /// <summary>
        /// 获取变量值
        /// </summary>
        Task<object> GetVariableValueAsync(DebugSession session, string variableName);
        
        /// <summary>
        /// 设置变量值
        /// </summary>
        Task<bool> SetVariableValueAsync(DebugSession session, string variableName, object value);
        
        /// <summary>
        /// 获取局部变量
        /// </summary>
        Task<List<VariableInfo>> GetLocalVariablesAsync(DebugSession session);
        
        /// <summary>
        /// 计算表达式
        /// </summary>
        Task<object> EvaluateExpressionAsync(DebugSession session, string expression);
        
        /// <summary>
        /// 停止调试
        /// </summary>
        Task StopDebuggingAsync(DebugSession session);
        
        /// <summary>
        /// 调试事件
        /// </summary>
        event EventHandler<DebugEventArgs> OnDebugEvent;
    }
}
```

#### 2.3.2 脚本模型
```csharp
namespace KeyForge.Abstractions.Models.Script
{
    /// <summary>
    /// 脚本模型
    /// </summary>
    public class Script
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public ScriptLanguage Language { get; set; } = ScriptLanguage.CSharp;
        public ScriptType Type { get; set; } = ScriptType.Sequence;
        public List<ScriptAction> Actions { get; set; } = new();
        public ScriptMetadata Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0.0";
        public ScriptStatus Status { get; set; } = ScriptStatus.Draft;
        public Dictionary<string, object> Variables { get; set; } = new();
        public List<ScriptParameter> Parameters { get; set; } = new();
        
        public Script() { }
        
        public Script(string name, string code, ScriptLanguage language = ScriptLanguage.CSharp)
        {
            Name = name;
            Code = code;
            Language = language;
        }
    }
    
    /// <summary>
    /// 脚本动作模型
    /// </summary>
    public class ScriptAction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ActionType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int Delay { get; set; } = 0;
        public int Repeat { get; set; } = 1;
        public ActionCondition Condition { get; set; } = new();
        public ActionErrorHandling ErrorHandling { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        
        public ScriptAction() { }
        
        public ScriptAction(ActionType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
    
    /// <summary>
    /// 脚本上下文模型
    /// </summary>
    public class ScriptContext
    {
        public string ScriptId { get; set; } = string.Empty;
        public Dictionary<string, object> Variables { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
        public CancellationToken CancellationToken { get; set; }
        public IProgress<ExecutionProgress> Progress { get; set; }
        public ILogger Logger { get; set; }
        public ExecutionOptions Options { get; set; } = new();
        public DateTime StartTime { get; set; } = DateTime.Now;
        public IntPtr WindowHandle { get; set; } = IntPtr.Zero;
        
        public ScriptContext() { }
        
        public ScriptContext(string scriptId)
        {
            ScriptId = scriptId;
        }
    }
    
    /// <summary>
    /// 执行结果模型
    /// </summary>
    public class ExecutionResult
    {
        public string ScriptId { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public int ActionsExecuted { get; set; }
        public int ActionsSucceeded { get; set; }
        public int ActionsFailed { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, object> Results { get; set; } = new();
        public List<ActionResult> ActionResults { get; set; } = new();
        
        public ExecutionResult()
        {
            Status = ExecutionStatus.Pending;
            Duration = TimeSpan.Zero;
        }
        
        public static ExecutionResult Success(string scriptId, string message = "")
        {
            return new ExecutionResult
            {
                ScriptId = scriptId,
                Status = ExecutionStatus.Completed,
                Message = message
            };
        }
        
        public static ExecutionResult Failed(string scriptId, string message, Exception exception = null)
        {
            return new ExecutionResult
            {
                ScriptId = scriptId,
                Status = ExecutionStatus.Failed,
                Message = message,
                Exception = exception
            };
        }
    }
    
    /// <summary>
    /// 动作结果模型
    /// </summary>
    public class ActionResult
    {
        public string ActionId { get; set; } = string.Empty;
        public ActionStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, object> Results { get; set; } = new();
        
        public ActionResult()
        {
            Status = ActionStatus.Pending;
            Duration = TimeSpan.Zero;
        }
        
        public static ActionResult Success(string actionId, string message = "")
        {
            return new ActionResult
            {
                ActionId = actionId,
                Status = ActionStatus.Success,
                Message = message
            };
        }
        
        public static ActionResult Failed(string actionId, string message, Exception exception = null)
        {
            return new ActionResult
            {
                ActionId = actionId,
                Status = ActionStatus.Failed,
                Message = message,
                Exception = exception
            };
        }
    }
    
    /// <summary>
    /// 编译结果模型
    /// </summary>
    public class CompilationResult
    {
        public string ScriptId { get; set; } = string.Empty;
        public CompilationStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[] CompiledCode { get; set; } = Array.Empty<byte>();
        public List<CompilationError> Errors { get; set; } = new();
        public List<CompilationWarning> Warnings { get; set; } = new();
        public TimeSpan CompilationTime { get; set; }
        
        public CompilationResult()
        {
            Status = CompilationStatus.Pending;
            CompilationTime = TimeSpan.Zero;
        }
        
        public static CompilationResult Success(string scriptId, byte[] compiledCode)
        {
            return new CompilationResult
            {
                ScriptId = scriptId,
                Status = CompilationStatus.Success,
                CompiledCode = compiledCode
            };
        }
        
        public static CompilationResult Failed(string scriptId, string message, List<CompilationError> errors = null)
        {
            return new CompilationResult
            {
                ScriptId = scriptId,
                Status = CompilationStatus.Failed,
                Message = message,
                Errors = errors ?? new List<CompilationError>()
            };
        }
    }
    
    /// <summary>
    /// 脚本元数据模型
    /// </summary>
    public class ScriptMetadata
    {
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int Priority { get; set; } = 0;
        public bool IsSystemScript { get; set; } = false;
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, string> CustomProperties { get; set; } = new();
    }
    
    /// <summary>
    /// 脚本参数模型
    /// </summary>
    public class ScriptParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ParameterType Type { get; set; } = ParameterType.String;
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; } = false;
        public List<ParameterValidation> Validations { get; set; } = new();
        
        public ScriptParameter() { }
        
        public ScriptParameter(string name, ParameterType type, object defaultValue = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }
    
    /// <summary>
    /// 执行选项模型
    /// </summary>
    public class ExecutionOptions
    {
        public bool EnableLogging { get; set; } = true;
        public bool EnableDebugging { get; set; } = false;
        public bool EnableProfiling { get; set; } = false;
        public int TimeoutMs { get; set; } = 30000;
        public int MaxRetries { get; set; } = 3;
        public bool ContinueOnError { get; set; } = false;
        public bool EnablePerformanceMonitoring { get; set; } = false;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
    
    /// <summary>
    /// 执行进度模型
    /// </summary>
    public class ExecutionProgress
    {
        public int CurrentAction { get; set; }
        public int TotalActions { get; set; }
        public double ProgressPercentage { get; set; }
        public string CurrentActionName { get; set; } = string.Empty;
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedRemainingTime { get; set; }
        public ExecutionStatus Status { get; set; }
        
        public ExecutionProgress(int currentAction, int totalActions)
        {
            CurrentAction = currentAction;
            TotalActions = totalActions;
            ProgressPercentage = totalActions > 0 ? (double)currentAction / totalActions * 100 : 0;
        }
    }
    
    /// <summary>
    /// 执行统计模型
    /// </summary>
    public class ExecutionStatistics
    {
        public string ScriptId { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public TimeSpan MinExecutionTime { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
        public DateTime LastExecutionTime { get; set; }
        public Dictionary<string, int> ActionStatistics { get; set; } = new();
        public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
        
        public ExecutionStatistics() { }
        
        public ExecutionStatistics(string scriptId)
        {
            ScriptId = scriptId;
        }
    }
}
```

### 2.4 配置管理接口

#### 2.4.1 配置服务接口
```csharp
namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 配置服务接口
    /// </summary>
    public interface IConfigurationService : IDisposable
    {
        /// <summary>
        /// 初始化配置服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 获取配置值
        /// </summary>
        T Get<T>(string key, T defaultValue = default);
        
        /// <summary>
        /// 设置配置值
        /// </summary>
        Task<bool> SetAsync<T>(string key, T value);
        
        /// <summary>
        /// 删除配置值
        /// </summary>
        Task<bool> DeleteAsync(string key);
        
        /// <summary>
        /// 检查配置键是否存在
        /// </summary>
        bool ContainsKey(string key);
        
        /// <summary>
        /// 获取所有配置键
        /// </summary>
        List<string> GetKeys();
        
        /// <summary>
        /// 获取配置节
        /// </summary>
        IConfigurationSection GetSection(string sectionPath);
        
        /// <summary>
        /// 保存配置
        /// </summary>
        Task<bool> SaveAsync();
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        Task<bool> ReloadAsync();
        
        /// <summary>
        /// 验证配置
        /// </summary>
        Task<ValidationResult> ValidateAsync();
        
        /// <summary>
        /// 获取配置状态
        /// </summary>
        ConfigurationStatus Status { get; }
        
        /// <summary>
        /// 配置变化事件
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> OnConfigurationChanged;
    }
    
    /// <summary>
    /// 配置提供者接口
    /// </summary>
    public interface IConfigurationProvider : IDisposable
    {
        /// <summary>
        /// 加载配置
        /// </summary>
        Task<Dictionary<string, object>> LoadAsync();
        
        /// <summary>
        /// 保存配置
        /// </summary>
        Task<bool> SaveAsync(Dictionary<string, object> configuration);
        
        /// <summary>
        /// 获取提供者名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 获取提供者优先级
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 是否支持热重载
        /// </summary>
        bool SupportsHotReload { get; }
        
        /// <summary>
        /// 配置变化事件
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> OnConfigurationChanged;
    }
    
    /// <summary>
    /// 配置验证器接口
    /// </summary>
    public interface IConfigurationValidator
    {
        /// <summary>
        /// 验证配置
        /// </summary>
        Task<ValidationResult> ValidateAsync(Dictionary<string, object> configuration);
        
        /// <summary>
        /// 验证配置值
        /// </summary>
        Task<ValidationResult> ValidateValueAsync(string key, object value);
        
        /// <summary>
        /// 获取验证规则
        /// </summary>
        List<ValidationRule> GetValidationRules();
        
        /// <summary>
        /// 添加验证规则
        /// </summary>
        void AddValidationRule(ValidationRule rule);
        
        /// <summary>
        /// 移除验证规则
        /// </summary>
        void RemoveValidationRule(string ruleName);
    }
    
    /// <summary>
    /// 配置节接口
    /// </summary>
    public interface IConfigurationSection
    {
        /// <summary>
        /// 获取节路径
        /// </summary>
        string Path { get; }
        
        /// <summary>
        /// 获取配置值
        /// </summary>
        T Get<T>(string key, T defaultValue = default);
        
        /// <summary>
        /// 设置配置值
        /// </summary>
        Task<bool> SetAsync<T>(string key, T value);
        
        /// <summary>
        /// 获取子节
        /// </summary>
        IConfigurationSection GetSection(string subPath);
        
        /// <summary>
        /// 获取所有子节
        /// </summary>
        List<IConfigurationSection> GetChildren();
        
        /// <summary>
        /// 获取所有键
        /// </summary>
        List<string> GetKeys();
        
        /// <summary>
        /// 检查键是否存在
        /// </summary>
        bool ContainsKey(string key);
        
        /// <summary>
        /// 获取父节
        /// </summary>
        IConfigurationSection Parent { get; }
        
        /// <summary>
        /// 获取配置值
        /// </summary>
        object this[string key] { get; set; }
    }
}
```

#### 2.4.2 配置模型
```csharp
namespace KeyForge.Abstractions.Models.Configuration
{
    /// <summary>
    /// 配置模型
    /// </summary>
    public class Configuration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Values { get; set; } = new();
        public Dictionary<string, ConfigurationSection> Sections { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0.0";
        public ConfigurationScope Scope { get; set; } = ConfigurationScope.User;
        public Dictionary<string, string> Metadata { get; set; } = new();
        
        public Configuration() { }
        
        public Configuration(string name)
        {
            Name = name;
        }
    }
    
    /// <summary>
    /// 配置节模型
    /// </summary>
    public class ConfigurationSection
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Values { get; set; } = new();
        public Dictionary<string, ConfigurationSection> Sections { get; set; } = new();
        public ConfigurationSection Parent { get; set; }
        
        public ConfigurationSection() { }
        
        public ConfigurationSection(string path, string name)
        {
            Path = path;
            Name = name;
        }
        
        public T Get<T>(string key, T defaultValue = default)
        {
            if (Values.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
        
        public void Set<T>(string key, T value)
        {
            Values[key] = value;
        }
    }
    
    /// <summary>
    /// 配置键模型
    /// </summary>
    public class ConfigKey
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ConfigType Type { get; set; } = ConfigType.String;
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsSensitive { get; set; } = false;
        public List<string> AllowedValues { get; set; } = new();
        public List<ValidationRule> ValidationRules { get; set; } = new();
        
        public ConfigKey() { }
        
        public ConfigKey(string key, ConfigType type, object defaultValue = null)
        {
            Key = key;
            Type = type;
            DefaultValue = defaultValue;
        }
    }
    
    /// <summary>
    /// 配置变化事件参数
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string Key { get; set; } = string.Empty;
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public ConfigurationChangedEventArgs(string key, object oldValue, object newValue, ChangeType changeType)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            ChangeType = changeType;
        }
    }
    
    /// <summary>
    /// 验证结果模型
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
        public Dictionary<string, object> ValidatedValues { get; set; } = new();
        
        public ValidationResult()
        {
            IsValid = true;
        }
        
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }
        
        public static ValidationResult Failed(params ValidationError[] errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }
    }
    
    /// <summary>
    /// 验证错误模型
    /// </summary>
    public class ValidationError
    {
        public string Key { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public object AttemptedValue { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        
        public ValidationError() { }
        
        public ValidationError(string key, string message, string errorCode = "")
        {
            Key = key;
            Message = message;
            ErrorCode = errorCode;
        }
    }
    
    /// <summary>
    /// 验证警告模型
    /// </summary>
    public class ValidationWarning
    {
        public string Key { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string WarningCode { get; set; } = string.Empty;
        public object Value { get; set; }
        
        public ValidationWarning() { }
        
        public ValidationWarning(string key, string message, string warningCode = "")
        {
            Key = key;
            Message = message;
            WarningCode = warningCode;
        }
    }
    
    /// <summary>
    /// 验证规则模型
    /// </summary>
    public class ValidationRule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Func<object, bool> Validator { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        public ValidationRule() { }
        
        public ValidationRule(string name, Func<object, bool> validator, string errorMessage)
        {
            Name = name;
            Validator = validator;
            ErrorMessage = errorMessage;
        }
    }
}
```

## 🔧 硬件抽象层接口

### 3.1 HAL基础接口

#### 3.1.1 HAL接口定义
```csharp
namespace KeyForge.Abstractions.Interfaces.HAL
{
    /// <summary>
    /// HAL基础接口
    /// </summary>
    public interface IHAL : IDisposable
    {
        /// <summary>
        /// 初始化HAL
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 获取HAL信息
        /// </summary>
        HALInfo GetHALInfo();
        
        /// <summary>
        /// 获取HAL状态
        /// </summary>
        HALStatus Status { get; }
        
        /// <summary>
        /// 获取平台信息
        /// </summary>
        PlatformInfo GetPlatformInfo();
        
        /// <summary>
        /// HAL状态变化事件
        /// </summary>
        event EventHandler<HALStatusEventArgs> OnHALStatusChanged;
    }
    
    /// <summary>
    /// 输入HAL接口
    /// </summary>
    public interface IInputHAL : IHAL
    {
        /// <summary>
        /// 获取键盘服务
        /// </summary>
        IPlatformKeyboardService GetKeyboardService();
        
        /// <summary>
        /// 获取鼠标服务
        /// </summary>
        IPlatformMouseService GetMouseService();
        
        /// <summary>
        /// 获取输入钩子服务
        /// </summary>
        IPlatformHookService GetHookService();
        
        /// <summary>
        /// 获取输入设备列表
        /// </summary>
        Task<List<InputDeviceInfo>> GetInputDevicesAsync();
        
        /// <summary>
        /// 检查输入权限
        /// </summary>
        Task<bool> CheckInputPermissionsAsync();
        
        /// <summary>
        /// 请求输入权限
        /// </summary>
        Task<bool> RequestInputPermissionsAsync();
    }
    
    /// <summary>
    /// 图形HAL接口
    /// </summary>
    public interface IGraphicsHAL : IHAL
    {
        /// <summary>
        /// 获取屏幕捕获服务
        /// </summary>
        IPlatformScreenService GetScreenService();
        
        /// <summary>
        /// 获取图像处理服务
        /// </summary>
        IPlatformImageService GetImageService();
        
        /// <summary>
        /// 获取显示设备列表
        /// </summary>
        Task<List<DisplayDeviceInfo>> GetDisplayDevicesAsync();
        
        /// <summary>
        /// 检查屏幕捕获权限
        /// </summary>
        Task<bool> CheckScreenCapturePermissionsAsync();
        
        /// <summary>
        /// 请求屏幕捕获权限
        /// </summary>
        Task<bool> RequestScreenCapturePermissionsAsync();
    }
    
    /// <summary>
    /// 系统HAL接口
    /// </summary>
    public interface ISystemHAL : IHAL
    {
        /// <summary>
        /// 获取系统信息
        /// </summary>
        SystemInfo GetSystemInfo();
        
        /// <summary>
        /// 获取进程信息
        /// </summary>
        ProcessInfo GetProcessInfo();
        
        /// <summary>
        /// 获取内存信息
        /// </summary>
        MemoryInfo GetMemoryInfo();
        
        /// <summary>
        /// 获取CPU信息
        /// </summary>
        CPUInfo GetCPUInfo();
        
        /// <summary>
        /// 执行系统命令
        /// </summary>
        Task<CommandResult> ExecuteCommandAsync(string command, string arguments = "");
        
        /// <summary>
        /// 获取环境变量
        /// </summary>
        string GetEnvironmentVariable(string name);
        
        /// <summary>
        /// 设置环境变量
        /// </summary>
        bool SetEnvironmentVariable(string name, string value);
        
        /// <summary>
        /// 获取临时目录
        /// </summary>
        string GetTempDirectory();
        
        /// <summary>
        /// 获取应用程序数据目录
        /// </summary>
        string GetAppDataDirectory();
    }
    
    /// <summary>
    /// 平台键盘服务接口
    /// </summary>
    public interface IPlatformKeyboardService : IDisposable
    {
        /// <summary>
        /// 发送键盘事件
        /// </summary>
        Task<bool> SendKeyEventAsync(KeyCode keyCode, KeyState state);
        
        /// <summary>
        /// 获取按键状态
        /// </summary>
        KeyState GetKeyState(KeyCode keyCode);
        
        /// <summary>
        /// 获取键盘状态
        /// </summary>
        KeyboardState GetKeyboardState();
        
        /// <summary>
        /// 设置键盘状态
        /// </summary>
        Task<bool> SetKeyboardStateAsync(KeyboardState state);
        
        /// <summary>
        /// 键盘事件
        /// </summary>
        event EventHandler<PlatformKeyEventArgs> OnKeyEvent;
    }
    
    /// <summary>
    /// 平台鼠标服务接口
    /// </summary>
    public interface IPlatformMouseService : IDisposable
    {
        /// <summary>
        /// 移动鼠标
        /// </summary>
        Task<bool> MoveMouseAsync(int x, int y);
        
        /// <summary>
        /// 相对移动鼠标
        /// </summary>
        Task<bool> MoveMouseRelativeAsync(int deltaX, int deltaY);
        
        /// <summary>
        /// 发送鼠标事件
        /// </summary>
        Task<bool> SendMouseEventAsync(MouseButton button, MouseState state);
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        Point GetMousePosition();
        
        /// <summary>
        /// 获取鼠标状态
        /// </summary>
        MouseState GetMouseState();
        
        /// <summary>
        /// 鼠标事件
        /// </summary>
        event EventHandler<PlatformMouseEventArgs> OnMouseEvent;
    }
    
    /// <summary>
    /// 平台钩子服务接口
    /// </summary>
    public interface IPlatformHookService : IDisposable
    {
        /// <summary>
        /// 设置键盘钩子
        /// </summary>
        Task<bool> SetKeyboardHookAsync();
        
        /// <summary>
        /// 设置鼠标钩子
        /// </summary>
        Task<bool> SetMouseHookAsync();
        
        /// <summary>
        /// 移除钩子
        /// </summary>
        Task RemoveHooksAsync();
        
        /// <summary>
        /// 键盘钩子事件
        /// </summary>
        event EventHandler<PlatformKeyHookEventArgs> OnKeyboardHook;
        
        /// <summary>
        /// 鼠标钩子事件
        /// </summary>
        event EventHandler<PlatformMouseHookEventArgs> OnMouseHook;
        
        /// <summary>
        /// 钩子状态
        /// </summary>
        HookStatus Status { get; }
    }
    
    /// <summary>
    /// 平台屏幕服务接口
    /// </summary>
    public interface IPlatformScreenService : IDisposable
    {
        /// <summary>
        /// 捕获屏幕
        /// </summary>
        Task<byte[]> CaptureScreenAsync();
        
        /// <summary>
        /// 捕获屏幕区域
        /// </summary>
        Task<byte[]> CaptureRegionAsync(Rectangle region);
        
        /// <summary>
        /// 获取屏幕尺寸
        /// </summary>
        Size GetScreenSize();
        
        /// <summary>
        /// 获取屏幕DPI
        /// </summary>
        double GetScreenDpi();
        
        /// <summary>
        /// 获取屏幕信息
        /// </summary>
        ScreenInfo GetScreenInfo();
        
        /// <summary>
        /// 屏幕变化事件
        /// </summary>
        event EventHandler<PlatformScreenEventArgs> OnScreenChanged;
    }
    
    /// <summary>
    /// 平台图像服务接口
    /// </summary>
    public interface IPlatformImageService : IDisposable
    {
        /// <summary>
        /// 加载图像
        /// </summary>
        Task<byte[]> LoadImageAsync(string path);
        
        /// <summary>
        /// 保存图像
        /// </summary>
        Task<bool> SaveImageAsync(string path, byte[] imageData);
        
        /// <summary>
        /// 调整图像大小
        /// </summary>
        Task<byte[]> ResizeImageAsync(byte[] imageData, Size newSize);
        
        /// <summary>
        /// 裁剪图像
        /// </summary>
        Task<byte[]> CropImageAsync(byte[] imageData, Rectangle cropArea);
        
        /// <summary>
        /// 转换图像格式
        /// </summary>
        Task<byte[]> ConvertImageFormatAsync(byte[] imageData, string targetFormat);
        
        /// <summary>
        /// 获取图像信息
        /// </summary>
        ImageInfo GetImageInfo(byte[] imageData);
    }
}
```

## 🎯 应用层接口

### 4.1 用例和命令处理接口

#### 4.1.1 用例接口
```csharp
namespace KeyForge.Abstractions.Interfaces.Application
{
    /// <summary>
    /// 用例基础接口
    /// </summary>
    public interface IUseCase
    {
        /// <summary>
        /// 执行用例
        /// </summary>
        Task<TResult> ExecuteAsync<TRequest, TResult>(TRequest request);
        
        /// <summary>
        /// 验证请求
        /// </summary>
        Task<ValidationResult> ValidateAsync<TRequest>(TRequest request);
        
        /// <summary>
        /// 获取用例信息
        /// </summary>
        UseCaseInfo GetUseCaseInfo();
    }
    
    /// <summary>
    /// 命令处理器接口
    /// </summary>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        Task HandleAsync(TCommand command);
        
        /// <summary>
        /// 验证命令
        /// </summary>
        Task<ValidationResult> ValidateAsync(TCommand command);
    }
    
    /// <summary>
    /// 查询处理器接口
    /// </summary>
    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// 处理查询
        /// </summary>
        Task<TResult> HandleAsync(TQuery query);
        
        /// <summary>
        /// 验证查询
        /// </summary>
        Task<ValidationResult> ValidateAsync(TQuery query);
    }
    
    /// <summary>
    /// 命令基础接口
    /// </summary>
    public interface ICommand
    {
        string CommandId { get; }
        DateTime Timestamp { get; }
        string UserId { get; }
    }
    
    /// <summary>
    /// 查询基础接口
    /// </summary>
    public interface IQuery<out TResult>
    {
        string QueryId { get; }
        DateTime Timestamp { get; }
        string UserId { get; }
    }
}
```

## 📋 枚举定义

### 5.1 核心枚举

#### 5.1.1 输入相关枚举
```csharp
namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 按键代码枚举
    /// </summary>
    public enum KeyCode
    {
        // 基础按键
        None = 0x00,
        LButton = 0x01,
        RButton = 0x02,
        Cancel = 0x03,
        MButton = 0x04,
        
        // 字母键
        A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46,
        G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C,
        M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52,
        S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58,
        Y = 0x59, Z = 0x5A,
        
        // 数字键
        D0 = 0x30, D1 = 0x31, D2 = 0x32, D3 = 0x33, D4 = 0x34,
        D5 = 0x35, D6 = 0x36, D7 = 0x37, D8 = 0x38, D9 = 0x39,
        
        // 功能键
        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74,
        F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79,
        F11 = 0x7A, F12 = 0x7B,
        
        // 控制键
        Shift = 0x10, Control = 0x11, Alt = 0x12,
        Enter = 0x0D, Escape = 0x1B, Space = 0x20, Tab = 0x09,
        CapsLock = 0x14, NumLock = 0x90,
        
        // 方向键
        Up = 0x26, Down = 0x28, Left = 0x25, Right = 0x27,
        
        // 其他键
        Back = 0x08, Delete = 0x2E, Insert = 0x2D,
        Home = 0x24, End = 0x23, PageUp = 0x21, PageDown = 0x22,
        
        // 小键盘
        NumPad0 = 0x60, NumPad1 = 0x61, NumPad2 = 0x62, NumPad3 = 0x63,
        NumPad4 = 0x64, NumPad5 = 0x65, NumPad6 = 0x66, NumPad7 = 0x67,
        NumPad8 = 0x68, NumPad9 = 0x69, Multiply = 0x6A, Add = 0x6B,
        Subtract = 0x6D, Decimal = 0x6E, Divide = 0x6F
    }
    
    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        XButton1 = 4,
        XButton2 = 5
    }
    
    /// <summary>
    /// 动作类型枚举
    /// </summary>
    public enum ActionType
    {
        KeyDown = 0,
        KeyUp = 1,
        MouseMove = 2,
        MouseDown = 3,
        MouseUp = 4,
        Delay = 5,
        MouseClick = 6,
        MouseDoubleClick = 7,
        MouseRightClick = 8,
        MouseWheel = 9,
        TextInput = 10,
        ImageRecognition = 11,
        ScriptExecution = 12,
        Conditional = 13,
        Loop = 14
    }
    
    /// <summary>
    /// 按键状态枚举
    /// </summary>
    public enum KeyState
    {
        Press = 0,
        Release = 1
    }
    
    /// <summary>
    /// 鼠标状态枚举
    /// </summary>
    public enum MouseState
    {
        Down = 0,
        Up = 1
    }
    
    /// <summary>
    /// 输入类型枚举
    /// </summary>
    public enum InputType
    {
        Keyboard = 0,
        Mouse = 1,
        Touch = 2,
        Pen = 3
    }
    
    /// <summary>
    /// 钩子状态枚举
    /// </summary>
    public enum HookStatus
    {
        Inactive = 0,
        Active = 1,
        Error = 2,
        Paused = 3
    }
    
    /// <summary>
    /// 服务状态枚举
    /// </summary>
    public enum ServiceStatus
    {
        Stopped = 0,
        Running = 1,
        Paused = 2,
        Error = 3,
        Initializing = 4
    }
}
```

#### 5.1.2 图像识别相关枚举
```csharp
namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 识别状态枚举
    /// </summary>
    public enum RecognitionStatus
    {
        Success = 0,
        Failed = 1,
        Partial = 2,
        Timeout = 3,
        NotFound = 4,
        Error = 5
    }
    
    /// <summary>
    /// 识别方法枚举
    /// </summary>
    public enum RecognitionMethod
    {
        TemplateMatching = 0,
        FeatureMatching = 1,
        OCR = 2,
        ColorDetection = 3,
        ContourDetection = 4,
        EdgeDetection = 5
    }
    
    /// <summary>
    /// 模板类型枚举
    /// </summary>
    public enum TemplateType
    {
        Image = 0,
        Color = 1,
        Text = 2,
        Pattern = 3
    }
    
    /// <summary>
    /// 屏幕变化类型枚举
    /// </summary>
    public enum ScreenChangeType
    {
        ResolutionChanged = 0,
        OrientationChanged = 1,
        DisplayAdded = 2,
        DisplayRemoved = 3,
        PrimaryDisplayChanged = 4,
        DPIChanged = 5
    }
}
```

#### 5.1.3 脚本相关枚举
```csharp
namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 脚本状态枚举
    /// </summary>
    public enum ScriptStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Deleted = 3,
        Compiling = 4,
        Running = 5,
        Paused = 6,
        Debugging = 7,
        Error = 8
    }
    
    /// <summary>
    /// 脚本语言枚举
    /// </summary>
    public enum ScriptLanguage
    {
        CSharp = 0,
        JavaScript = 1,
        Python = 2,
        Lua = 3,
        VisualBasic = 4,
        Custom = 5
    }
    
    /// <summary>
    /// 脚本类型枚举
    /// </summary>
    public enum ScriptType
    {
        Sequence = 0,
        Conditional = 1,
        Loop = 2,
        Parallel = 3,
        EventDriven = 4,
        StateMachine = 5
    }
    
    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Paused = 5,
        Timeout = 6
    }
    
    /// <summary>
    /// 编译状态枚举
    /// </summary>
    public enum CompilationStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Cancelled = 3,
        Warning = 4
    }
    
    /// <summary>
    /// 动作状态枚举
    /// </summary>
    public enum ActionStatus
    {
        Pending = 0,
        Running = 1,
        Success = 2,
        Failed = 3,
        Skipped = 4,
        Cancelled = 5,
        Timeout = 6
    }
    
    /// <summary>
    /// 引擎状态枚举
    /// </summary>
    public enum EngineStatus
    {
        Stopped = 0,
        Running = 1,
        Paused = 2,
        Error = 3,
        Initializing = 4
    }
    
    /// <summary>
    /// 执行器状态枚举
    /// </summary>
    public enum ExecutorStatus
    {
        Idle = 0,
        Executing = 1,
        Paused = 2,
        Error = 3
    }
    
    /// <summary>
    /// 编译器状态枚举
    /// </summary>
    public enum CompilerStatus
    {
        Ready = 0,
        Compiling = 1,
        Error = 2,
        Busy = 3
    }
    
    /// <summary>
    /// 参数类型枚举
    /// </summary>
    public enum ParameterType
    {
        String = 0,
        Integer = 1,
        Boolean = 2,
        Double = 3,
        DateTime = 4,
        Array = 5,
        Object = 6,
        Custom = 7
    }
}
```

#### 5.1.4 配置相关枚举
```csharp
namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 配置类型枚举
    /// </summary>
    public enum ConfigType
    {
        String = 0,
        Integer = 1,
        Boolean = 2,
        Double = 3,
        DateTime = 4,
        Json = 5,
        Array = 6,
        Binary = 7
    }
    
    /// <summary>
    /// 配置范围枚举
    /// </summary>
    public enum ConfigurationScope
    {
        System = 0,
        User = 1,
        Application = 2,
        Session = 3,
        Temporary = 4
    }
    
    /// <summary>
    /// 配置状态枚举
    /// </summary>
    public enum ConfigurationStatus
    {
        NotLoaded = 0,
        Loaded = 1,
        Modified = 2,
        Saving = 3,
        Error = 4
    }
    
    /// <summary>
    /// 变化类型枚举
    /// </summary>
    public enum ChangeType
    {
        Added = 0,
        Modified = 1,
        Removed = 2,
        Reloaded = 3
    }
}
```

#### 5.1.5 系统相关枚举
```csharp
namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 平台类型枚举
    /// </summary>
    public enum PlatformType
    {
        Windows = 0,
        Linux = 1,
        macOS = 2,
        Android = 3,
        iOS = 4
    }
    
    /// <summary>
    /// HAL状态枚举
    /// </summary>
    public enum HALStatus
    {
        NotInitialized = 0,
        Initializing = 1,
        Ready = 2,
        Running = 3,
        Error = 4,
        Stopped = 5
    }
    
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
    
    /// <summary>
    /// 窗口状态枚举
    /// </summary>
    public enum WindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2,
        Hidden = 3,
        Fullscreen = 4
    }
}
```

## 🎯 接口使用示例

### 6.1 输入系统使用示例

#### 6.1.1 基本输入操作
```csharp
// 创建输入服务
var keyboardService = serviceProvider.GetRequiredService<IKeyboardService>();
var mouseService = serviceProvider.GetRequiredService<IMouseService>();

// 发送按键组合
await keyboardService.SendKeyCombinationAsync(new[] { KeyCode.Control, KeyCode.C });

// 发送文本
await keyboardService.SendTextAsync("Hello World");

// 移动鼠标并点击
await mouseService.MoveMouseAsync(100, 200);
await mouseService.SendMouseClickAsync(MouseButton.Left);

// 订阅输入事件
keyboardService.OnKeyEvent += (sender, e) =>
{
    Console.WriteLine($"Key {e.KeyCode} {e.KeyState}");
};
```

#### 6.1.2 输入钩子使用示例
```csharp
// 创建钩子服务
var hookService = serviceProvider.GetRequiredService<IInputHookService>();

// 设置钩子
await hookService.SetKeyboardHookAsync();
await hookService.SetMouseHookAsync();

// 订阅钩子事件
hookService.OnKeyboardHook += (sender, e) =>
{
    Console.WriteLine($"Hooked key: {e.KeyCode}");
    e.IsHandled = true; // 阻止事件传播
};

hookService.OnMouseHook += (sender, e) =>
{
    Console.WriteLine($"Hooked mouse: {e.Button} at {e.Position}");
};
```

### 6.2 图像识别使用示例

#### 6.2.1 基本图像识别
```csharp
// 创建图像识别服务
var imageService = serviceProvider.GetRequiredService<IImageRecognitionService>();
var screenService = serviceProvider.GetRequiredService<IScreenCaptureService>();

// 创建图像模板
var template = await imageService.CreateTemplateAsync("button", buttonImageData, new Rectangle(0, 0, 100, 50));

// 识别图像
var result = await imageService.RecognizeAsync(template);
if (result.Status == RecognitionStatus.Success)
{
    Console.WriteLine($"Found button at {result.Location} with confidence {result.Confidence}");
    
    // 点击识别到的位置
    await mouseService.MoveMouseAsync(result.Location.X, result.Location.Y);
    await mouseService.SendMouseClickAsync(MouseButton.Left);
}
```

#### 6.2.2 批量识别和等待
```csharp
// 批量识别多个模板
var templates = new List<ImageTemplate> { template1, template2, template3 };
var results = await imageService.RecognizeBatchAsync(templates);

foreach (var result in results)
{
    if (result.Status == RecognitionStatus.Success)
    {
        Console.WriteLine($"Found {result.TemplateId} at {result.Location}");
    }
}

// 等待图像出现
var waitResult = await imageService.WaitForImageAsync(template, timeoutMs: 5000);
if (waitResult.Status == RecognitionStatus.Success)
{
    Console.WriteLine("Button appeared!");
}
```

### 6.3 脚本执行使用示例

#### 6.3.1 基本脚本执行
```csharp
// 创建脚本引擎
var scriptEngine = serviceProvider.GetRequiredService<IScriptEngine>();

// 创建脚本
var script = new Script
{
    Name = "Hello World",
    Language = ScriptLanguage.CSharp,
    Code = @"
        // 延迟1秒
        await Task.Delay(1000);
        
        // 输出Hello World
        Console.WriteLine(""Hello World"");
        
        // 发送按键
        await keyboardService.SendTextAsync(""Hello World"");
    "
};

// 编译脚本
var compilationResult = await scriptEngine.CompileAsync(script);
if (compilationResult.Status == CompilationStatus.Success)
{
    // 执行脚本
    var executionResult = await scriptEngine.ExecuteAsync(script);
    Console.WriteLine($"Script executed with status: {executionResult.Status}");
}
else
{
    Console.WriteLine($"Compilation failed: {compilationResult.Message}");
}
```

#### 6.3.2 脚本调试
```csharp
// 创建调试器
var debugger = serviceProvider.GetRequiredService<IScriptDebugger>();

// 开始调试
var session = await debugger.StartDebuggingAsync(script);

// 设置断点
await debugger.SetBreakpointAsync(session, 10);

// 单步执行
var stepResult = await debugger.StepOverAsync(session);
Console.WriteLine($"Stepped to line {stepResult.CurrentLine}");

// 获取变量值
var variables = await debugger.GetLocalVariablesAsync(session);
foreach (var variable in variables)
{
    Console.WriteLine($"{variable.Name} = {variable.Value}");
}

// 继续执行
await debugger.ContinueAsync(session);
```

### 6.4 配置管理使用示例

#### 6.4.1 基本配置操作
```csharp
// 创建配置服务
var configService = serviceProvider.GetRequiredService<IConfigurationService>();

// 设置配置值
await configService.SetAsync("theme", "dark");
await configService.SetAsync("fontSize", 14);
await configService.SetAsync("autoSave", true);

// 获取配置值
var theme = configService.Get<string>("theme", "light");
var fontSize = configService.Get<int>("fontSize", 12);
var autoSave = configService.Get<bool>("autoSave", false);

// 获取配置节
var uiSection = configService.GetSection("ui");
var windowWidth = uiSection.Get<int>("width", 800);
var windowHeight = uiSection.Get<int>("height", 600);
```

#### 6.4.2 配置验证和监听
```csharp
// 订阅配置变化事件
configService.OnConfigurationChanged += (sender, e) =>
{
    Console.WriteLine($"Configuration changed: {e.Key} = {e.NewValue}");
};

// 验证配置
var validationResult = await configService.ValidateAsync();
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error.Key} - {error.Message}");
    }
}
```

## 📋 总结

本接口设计规范为KeyForge项目提供了完整的接口定义，包括：

1. **核心服务接口**：输入、图像识别、脚本执行、配置管理
2. **硬件抽象层接口**：跨平台的硬件操作抽象
3. **应用层接口**：用例、命令处理、查询处理
4. **数据模型**：完整的业务对象定义
5. **枚举定义**：统一的枚举和常量定义
6. **使用示例**：详细的接口使用示例

这些接口设计确保了系统的：
- **跨平台兼容性**：通过HAL抽象实现跨平台支持
- **可扩展性**：基于接口的设计便于功能扩展
- **可测试性**：接口抽象便于单元测试和模拟
- **一致性**：统一的接口规范确保代码一致性
- **可维护性**：清晰的层次结构便于维护和升级

通过实施本接口设计规范，KeyForge将成为一个真正跨平台、高质量、易维护的企业级应用。

---

**文档完成时间**：2025-08-25  
**接口版本**：v2.0  
**下次更新**：根据实施反馈进行调整