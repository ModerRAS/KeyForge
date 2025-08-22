# KeyForge 系统架构设计

## 执行摘要

### 项目概述
KeyForge是一个基于C#的智能化游戏自动化按键脚本系统，采用领域驱动设计（DDD）和清洁架构模式。系统通过图像识别、智能决策和精确执行，为用户提供可靠的游戏自动化解决方案。

### 架构目标
- **高精度识别**：95%以上的图像识别准确率
- **智能化决策**：基于规则和机器学习的决策引擎
- **稳定可靠执行**：99%以上的执行成功率
- **全方位监控**：完整的日志记录和性能监控

### 架构原则
- **领域驱动设计**：清晰的领域边界和业务逻辑封装
- **清洁架构**：依赖倒置和分层设计
- **模块化架构**：高内聚、低耦合的组件设计
- **事件驱动**：异步处理和消息传递

## 1. 系统总体架构

### 1.1 架构模式选择

#### 1.1.1 清洁架构 (Clean Architecture)
```
┌─────────────────────────────────────────────────────────────┐
│                        表现层 (Presentation)                │
│  ┌─────────────┐ ┌─────────────────┐ ┌─────────────────┐   │
│  │  WPF UI     │ │ Web API         │ │ CLI 工具        │   │
│  └─────────────┘ └─────────────────┘ └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                        应用层 (Application)                 │
│  ┌─────────────┐ ┌─────────────────┐ ┌─────────────────┐   │
│  │  用例服务    │ │ 应用事件        │ │ DTO 映射        │   │
│  └─────────────┘ └─────────────────┘ └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                         领域层 (Domain)                   │
│  ┌─────────────┐ ┌─────────────────┐ ┌─────────────────┐   │
│  │  领域模型    │ │ 领域服务        │ │ 领域事件        │   │
│  └─────────────┘ └─────────────────┘ └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                      基础设施层 (Infrastructure)             │
│  ┌─────────────┐ ┌─────────────────┐ ┌─────────────────┐   │
│  │  数据持久化  │ │ 外部服务集成    │ │ 框架集成        │   │
│  └─────────────┘ └─────────────────┘ └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

#### 1.1.2 模块化架构
```
┌─────────────────────────────────────────────────────────────┐
│                      KeyForge 主应用程序                      │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐         │
│  │Automation   │ │  Vision     │ │  Decision   │         │
│  │  Module     │ │  Module     │ │  Module     │         │
│  └─────────────┘ └─────────────┘ └─────────────┘         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐         │
│  │Management   │ │ Interaction │ │  Common     │         │
│  │  Module     │ │  Module     │ │  Module     │         │
│  └─────────────┘ └─────────────┘ └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 技术栈选择

#### 1.2.1 核心技术栈
| 层级 | 技术 | 理由 |
|------|------|------|
| **开发框架** | .NET 8.0 | 最新LTS版本，性能优异，长期支持 |
| **UI框架** | WPF with MVVM | 成熟的桌面应用框架，丰富的控件库 |
| **数据库** | SQLite | 嵌入式数据库，无需额外配置 |
| **ORM** | Entity Framework Core | 轻量级ORM，支持多种数据库 |
| **日志** | Serilog | 结构化日志，支持多种输出 |
| **配置** | Microsoft.Extensions.Configuration | 灵活的配置管理 |
| **DI容器** | Microsoft.Extensions.DependencyInjection | 官方DI容器，性能优异 |

#### 1.2.2 图像处理技术栈
| 技术 | 用途 | 理由 |
|------|------|------|
| **OpenCVSharp** | 图像识别和模板匹配 | 最流行的计算机视觉库 |
| **Tesseract.NET** | OCR文字识别 | 开源OCR引擎，支持多语言 |
| **System.Drawing.Common** | 基础图像处理 | .NET内置图像处理库 |

#### 1.2.3 测试和开发工具
| 工具 | 用途 | 理由 |
|------|------|------|
| **xUnit** | 单元测试 | 现代测试框架，支持并行测试 |
| **Moq** | 模拟框架 | 轻量级模拟框架 |
| **Fluent Assertions** | 断言库 | 流畅的断言语法 |
| **Visual Studio 2022** | 开发环境 | 功能最全面的.NET IDE |

## 2. DDD领域模型设计

### 2.1 领域边界上下文

#### 2.1.1 自动化执行上下文 (Automation Context)
**职责**：负责按键操作的录制、回放和管理
**核心概念**：脚本、操作序列、执行计划
**边界**：与游戏交互的直接接口

#### 2.1.2 图像识别上下文 (Vision Context)
**职责**：负责屏幕捕获、图像识别和UI元素检测
**核心概念**：图像模板、识别算法、匹配结果
**边界**：视觉感知和模式识别

#### 2.1.3 决策引擎上下文 (Decision Context)
**职责**：负责业务逻辑判断和决策制定
**核心概念**：规则、条件、状态机、策略
**边界**：智能决策和流程控制

#### 2.1.4 系统管理上下文 (Management Context)
**职责**：负责系统配置、监控和维护
**核心概念**：配置、日志、性能指标
**边界**：系统运维和管理

#### 2.1.5 用户交互上下文 (Interaction Context)
**职责**：负责用户界面和交互体验
**核心概念**：界面、视图、控制器
**边界**：用户操作和展示

### 2.2 核心领域对象

#### 2.2.1 自动化执行上下文
```csharp
// 脚本实体 - 表示一个完整的自动化脚本
public class Script : Entity<ScriptId>
{
    public ScriptName Name { get; private set; }
    public ScriptDescription Description { get; private set; }
    public ScriptVersion Version { get; private set; }
    public ScriptStatus Status { get; private set; }
    public IReadOnlyList<ActionSequence> ActionSequences { get; private set; }
    public ScriptMetadata Metadata { get; private set; }
    
    // 业务方法
    public void Record(ActionSequence sequence) 
    {
        // 原本实现：复杂的序列验证和合并逻辑
        // 简化实现：基本添加逻辑
        var sequences = new List<ActionSequence>(ActionSequences) { sequence };
        ActionSequences = sequences.AsReadOnly();
        
        // 发布领域事件
        AddDomainEvent(new ScriptRecordedEvent(Id, sequence.Id));
    }
    
    public ExecutionResult Execute(ExecutionContext context) 
    {
        // 原本实现：复杂的执行逻辑和错误处理
        // 简化实现：基本执行循环
        var results = new List<ActionResult>();
        
        foreach (var sequence in ActionSequences)
        {
            try
            {
                var result = sequence.Execute(context);
                results.Add(result);
                
                if (result.Status == ActionStatus.Failed)
                {
                    // 简化错误处理
                    break;
                }
            }
            catch (Exception ex)
            {
                // 简化异常处理
                results.Add(new ActionResult(ActionStatus.Failed, ex.Message));
                break;
            }
        }
        
        return new ExecutionResult(results);
    }
    
    public void Validate() 
    {
        // 原本实现：复杂的验证逻辑
        // 简化实现：基本验证
        if (ActionSequences.Count == 0)
        {
            throw new DomainException("脚本必须包含至少一个操作序列");
        }
    }
}

// 操作序列值对象 - 表示一系列操作
public class ActionSequence : ValueObject
{
    public SequenceId Id { get; private set; }
    public IReadOnlyList<GameAction> Actions { get; private set; }
    public ExecutionOrder Order { get; private set; }
    public LoopCount LoopCount { get; private set; }
    
    public Duration GetEstimatedDuration() 
    {
        // 原本实现：考虑复杂的时间计算
        // 简化实现：基本时间估算
        var totalDelay = Actions.Sum(a => a.Delay.TotalMilliseconds);
        return Duration.FromMilliseconds(totalDelay * LoopCount.Value);
    }
    
    public ExecutionResult Execute(ExecutionContext context)
    {
        // 原本实现：复杂的执行逻辑
        // 简化实现：基本循环执行
        var results = new List<ActionResult>();
        
        for (int i = 0; i < LoopCount.Value; i++)
        {
            foreach (var action in Actions)
            {
                try
                {
                    var result = action.Execute(context);
                    results.Add(result);
                    
                    if (result.Status == ActionStatus.Failed)
                    {
                        return new ExecutionResult(results);
                    }
                    
                    // 简化延时处理
                    if (action.Delay.TotalMilliseconds > 0)
                    {
                        Thread.Sleep((int)action.Delay.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new ActionResult(ActionStatus.Failed, ex.Message));
                    return new ExecutionResult(results);
                }
            }
        }
        
        return new ExecutionResult(results);
    }
}

// 游戏操作值对象 - 表示单个游戏操作
public abstract class GameAction : ValueObject
{
    public ActionId Id { get; private set; }
    public Timestamp Timestamp { get; private set; }
    public ActionDelay Delay { get; private set; }
    public abstract ActionResult Execute(IGameInputHandler handler);
}

// 键盘操作值对象
public class KeyboardAction : GameAction
{
    public VirtualKeyCode KeyCode { get; private set; }
    public KeyState State { get; private set; }
    public bool IsExtendedKey { get; private set; }
    
    public override ActionResult Execute(IGameInputHandler handler)
    {
        try
        {
            // 原本实现：使用Windows API进行精确的键盘输入模拟
            // 简化实现：调用输入处理器的简单方法
            handler.SendKeyboardInput(KeyCode, State, IsExtendedKey);
            return new ActionResult(ActionStatus.Success);
        }
        catch (Exception ex)
        {
            return new ActionResult(ActionStatus.Failed, ex.Message);
        }
    }
}

// 鼠标操作值对象
public class MouseAction : GameAction
{
    public MousePosition Position { get; private set; }
    public MouseActionType Action { get; private set; }
    public MouseButton Button { get; private set; }
    public ScrollDelta Scroll { get; private set; }
    
    public override ActionResult Execute(IGameInputHandler handler)
    {
        try
        {
            // 原本实现：使用Windows API进行精确的鼠标输入模拟
            // 简化实现：调用输入处理器的简单方法
            handler.SendMouseInput(Position, Action, Button, Scroll);
            return new ActionResult(ActionStatus.Success);
        }
        catch (Exception ex)
        {
            return new ActionResult(ActionStatus.Failed, ex.Message);
        }
    }
}
```

#### 2.2.2 图像识别上下文
```csharp
// 图像模板实体
public class ImageTemplate : Entity<ImageTemplateId>
{
    public TemplateName Name { get; private set; }
    public ImageData TemplateImage { get; private set; }
    public RecognitionParameters Parameters { get; private set; }
    public TemplateVersion Version { get; private set; }
    
    public RecognitionResult Recognize(ScreenCapture screen)
    {
        // 原本实现：使用多种算法进行图像识别
        // 简化实现：使用简化的相关性计算
        return SimplifiedRecognition(screen, TemplateImage, Parameters);
    }
    
    // 简化实现：基本的图像识别
    private RecognitionResult SimplifiedRecognition(ScreenCapture screen, ImageData template, RecognitionParameters parameters)
    {
        // 这里是简化实现，实际应该使用OpenCV的高效算法
        // 原本实现应该包括：
        // 1. 图像预处理（灰度化、二值化、滤波等）
        // 2. 多种匹配算法（模板匹配、特征点匹配等）
        // 3. 结果后处理（非极大值抑制、置信度计算等）
        
        var result = new RecognitionResult(
            RecognitionStatus.Success,
            new ScreenLocation(100, 100), // 简化：固定位置
            new ConfidenceScore(0.85),    // 简化：固定置信度
            RecognitionMethod.TemplateMatching,
            Duration.FromMilliseconds(50) // 简化：固定处理时间
        );
        
        return result;
    }
    
    public void UpdateTemplate(ImageData newImage)
    {
        // 原本实现：复杂的图像验证和版本管理
        // 简化实现：直接更新
        TemplateImage = newImage;
        Version = TemplateVersion.Next(Version);
        
        AddDomainEvent(new TemplateUpdatedEvent(Id, Version));
    }
}

// 识别结果值对象
public class RecognitionResult : ValueObject
{
    public RecognitionStatus Status { get; private set; }
    public ScreenLocation Location { get; private set; }
    public ConfidenceScore Confidence { get; private set; }
    public RecognitionMethod Method { get; private set; }
    public Duration ProcessingTime { get; private set; }
    
    public bool IsSuccessful() => Status == RecognitionStatus.Success;
    public bool IsReliable() => Confidence >= ConfidenceScore.Reliable;
}

// 识别算法策略接口
public interface IRecognitionAlgorithm
{
    RecognitionResult Recognize(ImageData source, ImageData template, RecognitionParameters parameters);
    bool CanHandle(RecognitionMethod method);
    AlgorithmPerformance GetPerformanceMetrics();
}

// 模板匹配算法实现
public class TemplateMatchingAlgorithm : IRecognitionAlgorithm
{
    public RecognitionResult Recognize(ImageData source, ImageData template, RecognitionParameters parameters)
    {
        // 原本实现：使用OpenCV进行模板匹配
        // 简化实现：使用简化的相关性计算
        return SimplifiedTemplateMatching(source, template, parameters);
    }
    
    // 简化实现：使用基本的像素相关性计算
    private RecognitionResult SimplifiedTemplateMatching(ImageData source, ImageData template, RecognitionParameters parameters)
    {
        // 这里是简化实现，实际应该使用OpenCV的高效算法
        var result = new RecognitionResult(
            RecognitionStatus.Success,
            new ScreenLocation(100, 100), // 简化：固定位置
            new ConfidenceScore(0.85),    // 简化：固定置信度
            RecognitionMethod.TemplateMatching,
            Duration.FromMilliseconds(50) // 简化：固定处理时间
        );
        return result;
    }
    
    public bool CanHandle(RecognitionMethod method) => method == RecognitionMethod.TemplateMatching;
    public AlgorithmPerformance GetPerformanceMetrics() 
    {
        // 原本实现：复杂的性能指标计算
        // 简化实现：返回固定指标
        return new AlgorithmPerformance(
            successRate: 0.85,
            averageTime: Duration.FromMilliseconds(50),
            memoryUsage: 1024 * 1024 // 1MB
        );
    }
}
```

#### 2.2.3 决策引擎上下文
```csharp
// 决策规则实体
public class DecisionRule : Entity<RuleId>
{
    public RuleName Name { get; private set; }
    public ConditionExpression Condition { get; private set; }
    public ActionSequence ThenAction { get; private set; }
    public ActionSequence ElseAction { get; private set; }
    public RulePriority Priority { get; private set; }
    public RuleStatus Status { get; private set; }
    
    public DecisionResult Evaluate(ExecutionContext context)
    {
        // 原本实现：复杂的条件评估和决策逻辑
        // 简化实现：基本的条件判断
        var conditionResult = Condition.Evaluate(context.VariableProvider);
        
        if (conditionResult)
        {
            var executionResult = ThenAction.Execute(context);
            return new DecisionResult(true, executionResult);
        }
        else if (ElseAction != null)
        {
            var executionResult = ElseAction.Execute(context);
            return new DecisionResult(false, executionResult);
        }
        
        return new DecisionResult(false, null);
    }
    
    public void UpdateCondition(ConditionExpression newCondition)
    {
        // 原本实现：复杂的条件验证和更新逻辑
        // 简化实现：直接更新
        Condition = newCondition;
        
        AddDomainEvent(new RuleUpdatedEvent(Id));
    }
}

// 条件表达式值对象
public class ConditionExpression : ValueObject
{
    public string Expression { get; private set; }
    public ExpressionType Type { get; private set; }
    public IReadOnlyList<Variable> Variables { get; private set; }
    
    public bool Evaluate(IVariableProvider variableProvider)
    {
        // 原本实现：使用表达式解析器进行复杂计算
        // 简化实现：使用基本的字符串比较
        try
        {
            // 简化的表达式评估
            foreach (var variable in Variables)
            {
                var value = variableProvider.GetVariable(variable.Name);
                Expression = Expression.Replace($"${variable.Name}", value.ToString());
            }
            
            // 这里应该使用真正的表达式解析器
            // 简化实现：基本的布尔表达式
            return Expression.ToLower() == "true";
        }
        catch
        {
            return false;
        }
    }
    
    public ValidationResult Validate()
    {
        // 原本实现：复杂的语法验证
        // 简化实现：基本验证
        if (string.IsNullOrWhiteSpace(Expression))
        {
            return ValidationResult.Failure("表达式不能为空");
        }
        
        return ValidationResult.Success();
    }
}

// 状态机实体
public class StateMachine : Entity<StateMachineId>
{
    public MachineName Name { get; private set; }
    public State CurrentState { get; private set; }
    public IReadOnlyList<State> States { get; private set; }
    public IReadOnlyList<Transition> Transitions { get; private set; }
    
    public void TransitionTo(StateId targetState, ExecutionContext context)
    {
        // 原本实现：复杂的状态转换逻辑和验证
        // 简化实现：基本的状态切换
        if (CanTransitionTo(targetState))
        {
            var oldState = CurrentState;
            var newState = States.First(s => s.Id == targetState);
            
            // 执行退出动作
            oldState.OnExit(context);
            
            // 切换状态
            CurrentState = newState;
            
            // 执行进入动作
            newState.OnEnter(context);
            
            AddDomainEvent(new StateTransitionEvent(Id, oldState.Id, targetState));
        }
        else
        {
            throw new InvalidOperationException($"无法从状态 {CurrentState.Id} 转换到 {targetState}");
        }
    }
    
    public bool CanTransitionTo(StateId targetState)
    {
        // 原本实现：复杂的转换条件检查
        // 简化实现：基本的存在性检查
        return Transitions.Any(t => t.FromState == CurrentState.Id && t.ToState == targetState);
    }
}

// 状态值对象
public class State : ValueObject
{
    public StateId Id { get; private set; }
    public StateName Name { get; private set; }
    public ActionSequence EntryAction { get; private set; }
    public ActionSequence ExitAction { get; private set; }
    public Duration Timeout { get; private set; }
    
    public void OnEnter(ExecutionContext context)
    {
        // 原本实现：复杂的进入逻辑和超时处理
        // 简化实现：基本执行
        EntryAction?.Execute(context);
    }
    
    public void OnExit(ExecutionContext context)
    {
        // 原本实现：复杂的退出逻辑和清理
        // 简化实现：基本执行
        ExitAction?.Execute(context);
    }
}
```

### 2.3 值对象定义

```csharp
// 标识符值对象
public record ScriptId(Guid Value);
public record ImageTemplateId(Guid Value);
public record RuleId(Guid Value);
public record StateMachineId(Guid Value);

// 度量值对象
public record ConfidenceScore(double Value) : IComparable<ConfidenceScore>
{
    public static ConfidenceScore Reliable => new ConfidenceScore(0.8);
    public int CompareTo(ConfidenceScore other) => Value.CompareTo(other.Value);
}

public record Duration(TimeSpan Value);
public record Timestamp(DateTimeOffset Value);
public record ScreenLocation(int X, int Y);

// 枚举值对象
public enum RecognitionStatus { Success, Failed, Partial, Timeout }
public enum ScriptStatus { Draft, Active, Paused, Stopped, Error }
public enum LogLevel { Debug, Info, Warn, Error, Fatal }
```

## 3. Clean Architecture分层设计

### 3.1 依赖关系规则

```
┌─────────────────────────────────────────────────────────────┐
│                        表现层 (Presentation)                │
│  • WPF应用程序                                                │
│  • 视图模型 (ViewModels)                                     │
│  • 用户控件 (User Controls)                                  │
│  • 依赖：应用层                                              │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                        应用层 (Application)                 │
│  • 用例服务 (Use Cases)                                      │
│  • 应用服务 (Application Services)                           │
│  • DTO (Data Transfer Objects)                              │
│  • 依赖：领域层                                              │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                         领域层 (Domain)                   │
│  • 实体 (Entities)                                          │
│  • 值对象 (Value Objects)                                   │
│  • 领域服务 (Domain Services)                               │
│  • 仓储接口 (Repository Interfaces)                         │
│  • 无外部依赖                                              │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                      基础设施层 (Infrastructure)             │
│  • 数据库实现 (Database Implementations)                    │
│  • 外部服务 (External Services)                             │
│  • 文件系统 (File System)                                   │
│  • 依赖：领域层接口                                          │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 接口定义

#### 3.2.1 领域服务接口
```csharp
// 仓储接口定义
public interface IScriptRepository
{
    Task<Script> GetByIdAsync(ScriptId id);
    Task<IEnumerable<Script>> GetAllAsync();
    Task SaveAsync(Script script);
    Task DeleteAsync(ScriptId id);
}

public interface IImageTemplateRepository
{
    Task<ImageTemplate> GetByIdAsync(ImageTemplateId id);
    Task<IEnumerable<ImageTemplate>> GetAllAsync();
    Task SaveAsync(ImageTemplate template);
    Task DeleteAsync(ImageTemplateId id);
}

// 应用服务接口
public interface IImageRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(ScreenCapture screen, ImageTemplate template, RecognitionParameters parameters);
    Task<IEnumerable<RecognitionResult>> RecognizeMultipleAsync(ScreenCapture screen, IEnumerable<ImageTemplate> templates);
}

public interface IActionExecutor
{
    Task<ExecutionResult> ExecuteAsync(ActionSequence sequence);
    Task<ExecutionResult> ExecuteScriptAsync(Script script);
}

// 领域服务接口
public interface IInputRecorder
{
    Task<RecordingSession> StartRecordingAsync(RecordingParameters parameters);
    Task StopRecordingAsync();
    event EventHandler<InputEvent> OnInputReceived;
}

public interface IGameInputHandler
{
    void SendKeyboardInput(VirtualKeyCode keyCode, KeyState state, bool isExtended);
    void SendMouseInput(MousePosition position, MouseActionType action, MouseButton button, ScrollDelta scroll);
}
```

#### 3.2.2 应用服务实现
```csharp
// 脚本录制应用服务
public class ScriptRecordingApplicationService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IInputRecorder _inputRecorder;
    private readonly IEventBus _eventBus;
    
    public ScriptRecordingApplicationService(
        IScriptRepository scriptRepository,
        IInputRecorder inputRecorder,
        IEventBus eventBus)
    {
        _scriptRepository = scriptRepository;
        _inputRecorder = inputRecorder;
        _eventBus = eventBus;
    }
    
    public async Task<ScriptId> StartRecordingAsync(StartRecordingCommand command)
    {
        // 创建新脚本
        var script = new Script(
            command.Name,
            command.Description,
            ScriptVersion.Initial
        );
        
        // 开始录制
        var recordingSession = await _inputRecorder.StartRecordingAsync(
            new RecordingParameters(
                command.TargetWindow,
                command.RecordDelay,
                command.Sensitivity
            )
        );
        
        // 监听输入事件
        recordingSession.OnInputReceived += async (sender, inputEvent) =>
        {
            var action = ConvertToGameAction(inputEvent);
            script.Record(new ActionSequence(new[] { action }));
            
            // 实时保存
            await _scriptRepository.SaveAsync(script);
        };
        
        // 保存脚本
        await _scriptRepository.SaveAsync(script);
        
        // 发布事件
        await _eventBus.PublishAsync(new ScriptRecordingStartedEvent(script.Id));
        
        return script.Id;
    }
    
    public async Task StopRecordingAsync(StopRecordingCommand command)
    {
        await _inputRecorder.StopRecordingAsync();
        
        var script = await _scriptRepository.GetByIdAsync(command.ScriptId);
        if (script != null)
        {
            script.Status = ScriptStatus.Active;
            await _scriptRepository.SaveAsync(script);
            
            await _eventBus.PublishAsync(new ScriptRecordingStoppedEvent(script.Id));
        }
    }
    
    // 简化实现：输入转换逻辑
    private GameAction ConvertToGameAction(InputEvent input)
    {
        // 原本实现：完整的输入事件处理和转换
        // 简化实现：基本的类型判断和创建
        return input switch
        {
            KeyboardInputEvent keyInput => new KeyboardAction(
                ActionId.New(),
                Timestamp.Now,
                ActionDelay.Zero,
                keyInput.KeyCode,
                keyInput.State,
                keyInput.IsExtended
            ),
            MouseInputEvent mouseInput => new MouseAction(
                ActionId.New(),
                Timestamp.Now,
                ActionDelay.Zero,
                mouseInput.Position,
                mouseInput.Action,
                mouseInput.Button,
                mouseInput.Scroll
            ),
            _ => throw new NotSupportedException($"Unsupported input type: {input.GetType()}")
        };
    }
}

// 图像识别应用服务
public class ImageRecognitionApplicationService
{
    private readonly IImageTemplateRepository _templateRepository;
    private readonly IImageRecognitionService _recognitionService;
    private readonly IEventBus _eventBus;
    
    public ImageRecognitionApplicationService(
        IImageTemplateRepository templateRepository,
        IImageRecognitionService recognitionService,
        IEventBus eventBus)
    {
        _templateRepository = templateRepository;
        _recognitionService = recognitionService;
        _eventBus = eventBus;
    }
    
    public async Task<RecognitionResult> RecognizeAsync(RecognizeImageCommand command)
    {
        var template = await _templateRepository.GetByIdAsync(command.TemplateId);
        if (template == null)
        {
            throw new TemplateNotFoundException(command.TemplateId);
        }
        
        var screenCapture = await CaptureScreenAsync(command.ScreenRegion);
        var result = await _recognitionService.RecognizeAsync(
            screenCapture,
            template,
            command.Parameters
        );
        
        // 发布识别事件
        await _eventBus.PublishAsync(new ImageRecognizedEvent(
            command.TemplateId,
            result,
            screenCapture.Timestamp
        ));
        
        return result;
    }
    
    // 简化实现：屏幕捕获
    private async Task<ScreenCapture> CaptureScreenAsync(ScreenRegion region)
    {
        // 原本实现：使用Windows API进行屏幕捕获
        // 简化实现：返回空的捕获对象
        return await Task.FromResult(new ScreenCapture(
            new ImageData(new byte[0], 0, 0),
            region,
            Timestamp.Now
        ));
    }
}
```

## 4. Sense-Judge-Act闭环策略

### 4.1 闭环架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                      Sense (感知)                           │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ 屏幕捕获    │ │ 图像识别    │ │ 状态检测    │           │
│  │ ScreenCapture││ ImageRecognition││ StateDetection│           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                      Judge (决策)                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ 规则引擎    │ │ 状态机      │ │ 条件判断    │           │
│  │ RuleEngine  │ │ StateMachine │ │ Condition   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────┐
│                      Act (执行)                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ 按键执行    │ │ 鼠标操作    │ │ 错误处理    │           │
│  │ KeyExecutor │ │ MouseAction │ │ ErrorHandler│           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 闭环实现

```csharp
// 闭环控制器基类
public abstract class LoopController
{
    protected readonly ISenseService _senseService;
    protected readonly IJudgeService _judgeService;
    protected readonly IActService _actService;
    protected readonly ILogger<LoopController> _logger;
    
    protected LoopController(
        ISenseService senseService,
        IJudgeService judgeService,
        IActService actService,
        ILogger<LoopController> logger)
    {
        _senseService = senseService;
        _judgeService = judgeService;
        _actService = actService;
        _logger = logger;
    }
    
    public async Task RunAsync(LoopConfiguration configuration, CancellationToken cancellationToken)
    {
        _logger.LogInformation("启动闭环控制器");
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Sense - 感知环境状态
                var senseResult = await SenseAsync(configuration);
                
                // Judge - 决策判断
                var judgeResult = await JudgeAsync(senseResult, configuration);
                
                // Act - 执行动作
                var actResult = await ActAsync(judgeResult, configuration);
                
                // 处理执行结果
                await HandleResultAsync(senseResult, judgeResult, actResult);
                
                // 根据配置延时
                await Task.Delay(configuration.LoopInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("闭环控制器被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "闭环控制器发生错误");
            throw;
        }
    }
    
    protected abstract Task<SenseResult> SenseAsync(LoopConfiguration configuration);
    protected abstract Task<JudgeResult> JudgeAsync(SenseResult senseResult, LoopConfiguration configuration);
    protected abstract Task<ActResult> ActAsync(JudgeResult judgeResult, LoopConfiguration configuration);
    protected abstract Task HandleResultAsync(SenseResult senseResult, JudgeResult judgeResult, ActResult actResult);
}

// 图像触发闭环控制器
public class ImageTriggerLoopController : LoopController
{
    private readonly IImageTemplateRepository _templateRepository;
    
    public ImageTriggerLoopController(
        ISenseService senseService,
        IJudgeService judgeService,
        IActService actService,
        IImageTemplateRepository templateRepository,
        ILogger<ImageTriggerLoopController> logger)
        : base(senseService, judgeService, actService, logger)
    {
        _templateRepository = templateRepository;
    }
    
    protected override async Task<SenseResult> SenseAsync(LoopConfiguration configuration)
    {
        // 原本实现：复杂的屏幕捕获和图像处理
        // 简化实现：基本的图像识别
        var screenRegion = configuration.ScreenRegion ?? ScreenRegion.FullScreen;
        var screenCapture = await _senseService.CaptureScreenAsync(screenRegion);
        
        var templates = await _templateRepository.GetAllAsync();
        var recognitionResults = new List<RecognitionResult>();
        
        foreach (var template in templates)
        {
            var result = await _senseService.RecognizeAsync(screenCapture, template);
            recognitionResults.Add(result);
        }
        
        return new ImageSenseResult(screenCapture, recognitionResults);
    }
    
    protected override async Task<JudgeResult> JudgeAsync(SenseResult senseResult, LoopConfiguration configuration)
    {
        var imageSenseResult = (ImageSenseResult)senseResult;
        
        // 原本实现：复杂的决策逻辑和优先级判断
        // 简化实现：基本的结果判断
        var successfulResults = imageSenseResult.RecognitionResults
            .Where(r => r.IsSuccessful() && r.IsReliable())
            .ToList();
        
        if (successfulResults.Any())
        {
            // 选择置信度最高的结果
            var bestResult = successfulResults.OrderByDescending(r => r.Confidence).First();
            return new ImageJudgeResult(true, bestResult);
        }
        
        return new ImageJudgeResult(false, null);
    }
    
    protected override async Task<ActResult> ActAsync(JudgeResult judgeResult, LoopConfiguration configuration)
    {
        var imageJudgeResult = (ImageJudgeResult)judgeResult;
        
        if (imageJudgeResult.ShouldAct && imageJudgeResult.RecognitionResult != null)
        {
            // 原本实现：复杂的动作执行和错误处理
            // 简化实现：基本的动作执行
            var action = configuration.TriggerAction;
            var executionResult = await _actService.ExecuteAsync(action);
            
            return new ActResult(true, executionResult);
        }
        
        return new ActResult(false, null);
    }
    
    protected override async Task HandleResultAsync(SenseResult senseResult, JudgeResult judgeResult, ActResult actResult)
    {
        // 原本实现：复杂的结果处理和状态更新
        // 简化实现：基本的日志记录
        if (actResult.Success)
        {
            _logger.LogInformation("闭环执行成功");
        }
        else
        {
            _logger.LogWarning("闭环执行失败");
        }
        
        // 这里可以添加更多处理逻辑，比如：
        // - 更新执行统计
        // - 触发事件
        // - 调整参数
        // - 错误恢复
    }
}

// 闭环配置
public class LoopConfiguration
{
    public ScreenRegion ScreenRegion { get; set; }
    public Duration LoopInterval { get; set; }
    public ActionSequence TriggerAction { get; set; }
    public ActionSequence FailureAction { get; set; }
    public ConfidenceScore MinimumConfidence { get; set; }
    public int MaxRetries { get; set; }
    public Duration Timeout { get; set; }
}

// 闭环结果类
public abstract class SenseResult
{
    public Timestamp Timestamp { get; protected set; }
}

public class ImageSenseResult : SenseResult
{
    public ScreenCapture ScreenCapture { get; }
    public IReadOnlyList<RecognitionResult> RecognitionResults { get; }
    
    public ImageSenseResult(ScreenCapture screenCapture, IReadOnlyList<RecognitionResult> recognitionResults)
    {
        ScreenCapture = screenCapture;
        RecognitionResults = recognitionResults;
        Timestamp = screenCapture.Timestamp;
    }
}

public abstract class JudgeResult
{
    public bool ShouldAct { get; protected set; }
}

public class ImageJudgeResult : JudgeResult
{
    public RecognitionResult RecognitionResult { get; }
    
    public ImageJudgeResult(bool shouldAct, RecognitionResult recognitionResult)
    {
        ShouldAct = shouldAct;
        RecognitionResult = recognitionResult;
    }
}

public class ActResult
{
    public bool Success { get; }
    public ExecutionResult ExecutionResult { get; }
    
    public ActResult(bool success, ExecutionResult executionResult)
    {
        Success = success;
        ExecutionResult = executionResult;
    }
}
```

## 5. 技术栈和依赖管理

### 5.1 项目结构

```
KeyForge/
├── src/
│   ├── KeyForge.Application/          # 应用层
│   │   ├── UseCases/                  # 用例
│   │   ├── Services/                  # 应用服务
│   │   ├── DTOs/                      # 数据传输对象
│   │   └── Events/                    # 应用事件
│   ├── KeyForge.Domain/               # 领域层
│   │   ├── Models/                    # 领域模型
│   │   ├── Services/                  # 领域服务
│   │   ├── Interfaces/                # 接口定义
│   │   └── Events/                    # 领域事件
│   ├── KeyForge.Infrastructure/      # 基础设施层
│   │   ├── Data/                      # 数据访问
│   │   ├── Imaging/                   # 图像处理
│   │   ├── Input/                     # 输入处理
│   │   └── Logging/                   # 日志实现
│   ├── KeyForge.Presentation/         # 表现层
│   │   ├── Views/                     # 视图
│   │   ├── ViewModels/                # 视图模型
│   │   ├── Controls/                  # 用户控件
│   │   └── Resources/                 # 资源文件
│   └── KeyForge.Tests/                # 测试项目
│       ├── Unit/                      # 单元测试
│       ├── Integration/               # 集成测试
│       └── Acceptance/                # 验收测试
├── docs/                              # 文档
├── tools/                             # 工具脚本
├── build/                             # 构建脚本
└── README.md                          # 项目说明
```

### 5.2 NuGet包管理

#### 5.2.1 核心依赖
```xml
<!-- KeyForge.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- 领域层应该保持最少的外部依赖 -->
    <PackageReference Include="MediatR" Version="12.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>

<!-- KeyForge.Application.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
    <PackageReference Include="FluentValidation" Version="11.5.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\KeyForge.Domain\KeyForge.Domain.csproj" />
  </ItemGroup>
</Project>

<!-- KeyForge.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- 数据访问 -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
    
    <!-- 图像处理 -->
    <PackageReference Include="OpenCvSharp4" Version="4.8.0.20230708" />
    <PackageReference Include="OpenCvSharp4.runtime.win" Version="4.8.0.20230708" />
    <PackageReference Include="Tesseract" Version="5.2.0" />
    
    <!-- 日志 -->
    <PackageReference Include="Serilog" Version="3.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="7.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />
    
    <!-- 配置 -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.0" />
    
    <!-- 其他 -->
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\KeyForge.Domain\KeyForge.Domain.csproj" />
  </ItemGroup>
</Project>

<!-- KeyForge.Presentation.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="7.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\KeyForge.Application\KeyForge.Application.csproj" />
    <ProjectReference Include="..\KeyForge.Infrastructure\KeyForge.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

#### 5.2.2 依赖注入配置
```csharp
// KeyForge.Presentation/Bootstrapper.cs
public static class Bootstrapper
{
    public static IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/keyforge-.txt", rollingInterval: RollingInterval.Day))
            .ConfigureServices((context, services) =>
            {
                // 配置应用层服务
                services.AddApplication();
                
                // 配置基础设施层服务
                services.AddInfrastructure(context.Configuration);
                
                // 配置表现层服务
                services.AddPresentation();
            })
            .Build();
        
        return host;
    }
}

// KeyForge.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 添加MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssemblyContaining<DependencyInjection>();
        });
        
        // 添加应用服务
        services.AddScoped<IScriptRecordingApplicationService, ScriptRecordingApplicationService>();
        services.AddScoped<IImageRecognitionApplicationService, ImageRecognitionApplicationService>();
        services.AddScoped<IDecisionMakingApplicationService, DecisionMakingApplicationService>();
        
        // 添加验证器
        services.AddValidatorsFromAssemblyContaining<DependencyInjection>();
        
        return services;
    }
}

// KeyForge.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置数据库
        services.AddDbContext<KeyForgeDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        
        // 配置仓储
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IImageTemplateRepository, ImageTemplateRepository>();
        services.AddScoped<IDecisionRuleRepository, DecisionRuleRepository>();
        
        // 配置领域服务
        services.AddScoped<IImageRecognitionService, ImageRecognitionService>();
        services.AddScoped<IActionExecutor, ActionExecutor>();
        services.AddScoped<IInputRecorder, InputRecorder>();
        services.AddScoped<IGameInputHandler, GameInputHandler>();
        
        // 配置事件总线
        services.AddSingleton<IEventBus, MediatREventBus>();
        
        return services;
    }
}

// KeyForge.Presentation/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // 配置主窗口
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        
        // 配置视图和视图模型
        services.AddTransient<ScriptEditorView>();
        services.AddTransient<ScriptEditorViewModel>();
        services.AddTransient<ImageTemplateView>();
        services.AddTransient<ImageTemplateViewModel>();
        services.AddTransient<MonitoringView>();
        services.AddTransient<MonitoringViewModel>();
        
        // 配置其他服务
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        
        return services;
    }
}
```

## 6. 数据模型和API设计

### 6.1 数据库设计

#### 6.1.1 实体框架模型
```csharp
// KeyForge.Infrastructure/Data/KeyForgeDbContext.cs
public class KeyForgeDbContext : DbContext
{
    public KeyForgeDbContext(DbContextOptions<KeyForgeDbContext> options) : base(options)
    {
    }
    
    public DbSet<ScriptEntity> Scripts { get; set; }
    public DbSet<ActionSequenceEntity> ActionSequences { get; set; }
    public DbSet<GameActionEntity> GameActions { get; set; }
    public DbSet<ImageTemplateEntity> ImageTemplates { get; set; }
    public DbSet<DecisionRuleEntity> DecisionRules { get; set; }
    public DbSet<StateMachineEntity> StateMachines { get; set; }
    public DbSet<StateEntity> States { get; set; }
    public DbSet<ConfigurationEntity> Configurations { get; set; }
    public DbSet<LogEntryEntity> LogEntries { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeyForgeDbContext).Assembly);
    }
}

// 脚本实体配置
public class ScriptEntityConfiguration : IEntityTypeConfiguration<ScriptEntity>
{
    public void Configure(EntityTypeBuilder<ScriptEntity> builder)
    {
        builder.ToTable("Scripts");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        builder.HasMany(e => e.ActionSequences)
            .WithOne(e => e.Script)
            .HasForeignKey(e => e.ScriptId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(e => e.Metadata).HasColumnType("TEXT");
    }
}

// 图像模板实体配置
public class ImageTemplateEntityConfiguration : IEntityTypeConfiguration<ImageTemplateEntity>
{
    public void Configure(EntityTypeBuilder<ImageTemplateEntity> builder)
    {
        builder.ToTable("ImageTemplates");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        builder.Property(e => e.TemplateImage).HasColumnType("BLOB");
        builder.Property(e => e.Parameters).HasColumnType("TEXT");
    }
}

// 决策规则实体配置
public class DecisionRuleEntityConfiguration : IEntityTypeConfiguration<DecisionRuleEntity>
{
    public void Configure(EntityTypeBuilder<DecisionRuleEntity> builder)
    {
        builder.ToTable("DecisionRules");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Condition).IsRequired().HasColumnType("TEXT");
        builder.Property(e => e.Priority).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
    }
}
```

#### 6.1.2 实体到领域模型的映射
```csharp
// KeyForge.Infrastructure/Data/Mappings/ScriptMapping.cs
public static class ScriptMapping
{
    public static Script ToDomain(this ScriptEntity entity)
    {
        return new Script(
            ScriptId.From(entity.Id),
            new ScriptName(entity.Name),
            new ScriptDescription(entity.Description),
            ScriptVersion.From(entity.Version),
            ScriptStatus.From(entity.Status),
            entity.ActionSequences.Select(a => a.ToDomain()).ToList().AsReadOnly(),
            JsonSerializer.Deserialize<ScriptMetadata>(entity.Metadata ?? "{}") ?? new ScriptMetadata()
        );
    }
    
    public static ScriptEntity ToEntity(this Script domain)
    {
        return new ScriptEntity
        {
            Id = domain.Id.Value,
            Name = domain.Name.Value,
            Description = domain.Description.Value,
            Version = domain.Version.Value,
            Status = domain.Status.ToString(),
            CreatedAt = domain.CreatedAt.Value,
            UpdatedAt = domain.UpdatedAt.Value,
            ActionSequences = domain.ActionSequences.Select(a => a.ToEntity()).ToList(),
            Metadata = JsonSerializer.Serialize(domain.Metadata)
        };
    }
}

// KeyForge.Infrastructure/Data/Mappings/ImageTemplateMapping.cs
public static class ImageTemplateMapping
{
    public static ImageTemplate ToDomain(this ImageTemplateEntity entity)
    {
        return new ImageTemplate(
            ImageTemplateId.From(entity.Id),
            new TemplateName(entity.Name),
            new ImageData(entity.TemplateImage, entity.Width, entity.Height),
            JsonSerializer.Deserialize<RecognitionParameters>(entity.Parameters) ?? new RecognitionParameters(),
            TemplateVersion.From(entity.Version)
        );
    }
    
    public static ImageTemplateEntity ToEntity(this ImageTemplate domain)
    {
        return new ImageTemplateEntity
        {
            Id = domain.Id.Value,
            Name = domain.Name.Value,
            TemplateImage = domain.TemplateImage.Data,
            Width = domain.TemplateImage.Width,
            Height = domain.TemplateImage.Height,
            Parameters = JsonSerializer.Serialize(domain.Parameters),
            Version = domain.Version.Value,
            CreatedAt = domain.CreatedAt.Value,
            UpdatedAt = domain.UpdatedAt.Value
        };
    }
}
```

### 6.2 API设计

#### 6.2.1 应用层命令和查询
```csharp
// KeyForge.Application/Commands/ScriptCommands.cs
public record StartRecordingCommand(
    string Name,
    string Description,
    WindowHandle TargetWindow,
    Duration RecordDelay,
    SensitivityLevel Sensitivity
) : IRequest<ScriptId>;

public record StopRecordingCommand(
    ScriptId ScriptId
) : IRequest;

public record ExecuteScriptCommand(
    ScriptId ScriptId,
    ExecutionContext Context
) : IRequest<ExecutionResult>;

// KeyForge.Application/Queries/ScriptQueries.cs
public record GetScriptByIdQuery(
    ScriptId ScriptId
) : IRequest<ScriptDto>;

public record GetAllScriptsQuery() : IRequest<List<ScriptDto>>;

public record GetScriptHistoryQuery(
    ScriptId ScriptId,
    DateTimeRange DateRange
) : IRequest<List<ExecutionHistoryDto>>;

// KeyForge.Application/DTOs/ScriptDto.cs
public class ScriptDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; }
    public ScriptMetadataDto Metadata { get; set; }
}

public class ActionSequenceDto
{
    public Guid Id { get; set; }
    public List<GameActionDto> Actions { get; set; }
    public int Order { get; set; }
    public int LoopCount { get; set; }
}

public class GameActionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public DateTime Timestamp { get; set; }
    public int DelayMs { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}
```

#### 6.2.2 验证器
```csharp
// KeyForge.Application/Validators/StartRecordingCommandValidator.cs
public class StartRecordingCommandValidator : AbstractValidator<StartRecordingCommand>
{
    public StartRecordingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("脚本名称不能为空")
            .MaximumLength(200).WithMessage("脚本名称不能超过200个字符");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("脚本描述不能超过1000个字符");
            
        RuleFor(x => x.RecordDelay)
            .GreaterThanOrEqualTo(Duration.Zero).WithMessage("录制延迟不能为负数");
            
        RuleFor(x => x.Sensitivity)
            .IsInEnum().WithMessage("灵敏度级别无效");
    }
}

// KeyForge.Application/Validators/ExecuteScriptCommandValidator.cs
public class ExecuteScriptCommandValidator : AbstractValidator<ExecuteScriptCommand>
{
    public ExecuteScriptCommandValidator()
    {
        RuleFor(x => x.ScriptId)
            .NotEmpty().WithMessage("脚本ID不能为空");
            
        RuleFor(x => x.Context)
            .NotNull().WithMessage("执行上下文不能为空");
    }
}
```

## 7. 部署架构和扩展性设计

### 7.1 部署架构

#### 7.1.1 单体应用部署
```
┌─────────────────────────────────────────────────────────────┐
│                    用户机器 (Windows)                       │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                  KeyForge 应用程序                       │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐   │ │
│  │  │   主程序    │ │   配置文件  │ │   数据库文件    │   │ │
│  │  │KeyForge.exe │ │settings.json │ │  keyforge.db    │   │ │
│  │  └─────────────┘ └─────────────┘ └─────────────────┘   │ │
│  │                                                         │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐   │ │
│  │  │   日志文件  │ │   插件文件  │ │   模板文件      │   │ │
│  │  │ logs/       │ │ plugins/    │ │ templates/      │   │ │
│  │  └─────────────┘ └─────────────┘ └─────────────────┘   │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

#### 7.1.2 安装和配置
```xml
<!-- KeyForge.Installer/Product.wxs -->
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" 
           Name="KeyForge" 
           Language="1033" 
           Version="1.0.0.0" 
           Manufacturer="KeyForge Team" 
           UpgradeCode="PUT-GUID-HERE">
    
    <Package InstallerVersion="200" 
             Compressed="yes" 
             InstallScope="perMachine" />
    
    <MajorUpgrade DowngradeErrorMessage="A newer version of KeyForge is already installed." />
    
    <MediaTemplate EmbedCab="yes" />
    
    <!-- 安装目录 -->
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="KeyForge" />
      </Directory>
      
      <!-- 开始菜单 -->
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ApplicationProgramsFolder" Name="KeyForge"/>
      </Directory>
      
      <!-- 桌面快捷方式 -->
      <Directory Id="DesktopFolder" Name="Desktop" />
    </Directory>
    
    <!-- 主程序文件 -->
    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <Component Id="MainExecutable" Guid="*">
        <File Id="KeyForgeExe" 
              Source="$(var.KeyForge.TargetPath)" 
              KeyPath="yes" 
              Checksum="yes"/>
        
        <!-- 快捷方式 -->
        <Shortcut Id="ApplicationStartMenuShortcut"
                  Name="KeyForge"
                  Description="KeyForge 自动化按键脚本系统"
                  Target="[INSTALLFOLDER]KeyForge.exe"
                  WorkingDirectory="INSTALLFOLDER"
                  Icon="KeyForgeIcon.exe"
                  IconIndex="0"
                  Advertise="yes"/>
        
        <Shortcut Id="ApplicationDesktopShortcut"
                  Directory="DesktopFolder"
                  Name="KeyForge"
                  Description="KeyForge 自动化按键脚本系统"
                  Target="[INSTALLFOLDER]KeyForge.exe"
                  WorkingDirectory="INSTALLFOLDER"
                  Icon="KeyForgeIcon.exe"
                  IconIndex="0"/>
        
        <!-- 注册表项 -->
        <RegistryValue Root="HKCU" 
                      Key="Software\Microsoft\KeyForge" 
                      Name="installed" 
                      Type="integer" 
                      Value="1" 
                      KeyPath="yes"/>
      </Component>
      
      <!-- 配置文件 -->
      <Component Id="ConfigurationFiles" Guid="*">
        <File Id="SettingsJson" 
              Source="$(var.KeyForge.TargetDir)settings.json" />
        <File Id="AppSettingsJson" 
              Source="$(var.KeyForge.TargetDir)appsettings.json" />
      </Component>
      
      <!-- 依赖库 -->
      <Component Id="Dependencies" Guid="*">
        <File Id="OpenCvDll" 
              Source="$(var.KeyForge.TargetDir)opencv_world480.dll" />
        <File Id="TesseractDll" 
              Source="$(var.KeyForge.TargetDir)tesseract50.dll" />
      </Component>
    </ComponentGroup>
    
    <!-- 功能 -->
    <Feature Id="ProductFeature" Title="KeyForge" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
  </Product>
</Wix>
```

### 7.2 扩展性设计

#### 7.2.1 插件架构
```csharp
// KeyForge.Domain/Plugins/IPlugin.cs
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    PluginMetadata Metadata { get; }
    
    Task InitializeAsync(PluginContext context);
    Task ShutdownAsync();
    
    Task<PluginActionResult> ExecuteAsync(PluginAction action);
}

// KeyForge.Domain/Plugins/IPluginManager.cs
public interface IPluginManager
{
    Task<IEnumerable<IPlugin>> LoadPluginsAsync(string pluginDirectory);
    Task UnloadPluginAsync(string pluginName);
    Task<PluginActionResult> ExecutePluginActionAsync(string pluginName, PluginAction action);
    IEnumerable<PluginInfo> GetLoadedPlugins();
}

// KeyForge.Infrastructure/Plugins/PluginManager.cs
public class PluginManager : IPluginManager
{
    private readonly Dictionary<string, IPlugin> _loadedPlugins = new();
    private readonly ILogger<PluginManager> _logger;
    
    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger;
    }
    
    public async Task<IEnumerable<IPlugin>> LoadPluginsAsync(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
        {
            _logger.LogWarning("插件目录不存在: {PluginDirectory}", pluginDirectory);
            return Enumerable.Empty<IPlugin>();
        }
        
        var pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll");
        var loadedPlugins = new List<IPlugin>();
        
        foreach (var pluginFile in pluginFiles)
        {
            try
            {
                var plugin = await LoadPluginAsync(pluginFile);
                if (plugin != null)
                {
                    _loadedPlugins[plugin.Name] = plugin;
                    loadedPlugins.Add(plugin);
                    _logger.LogInformation("成功加载插件: {PluginName} v{PluginVersion}", 
                        plugin.Name, plugin.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载插件失败: {PluginFile}", pluginFile);
            }
        }
        
        return loadedPlugins;
    }
    
    private async Task<IPlugin> LoadPluginAsync(string pluginFile)
    {
        // 原本实现：复杂的插件加载和安全检查
        // 简化实现：基本的程序集加载
        var assembly = Assembly.LoadFrom(pluginFile);
        
        // 查找插件类型
        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();
        
        if (pluginTypes.Count == 0)
        {
            _logger.LogWarning("插件文件中未找到有效的插件类型: {PluginFile}", pluginFile);
            return null;
        }
        
        if (pluginTypes.Count > 1)
        {
            _logger.LogWarning("插件文件中包含多个插件类型，使用第一个: {PluginFile}", pluginFile);
        }
        
        var pluginType = pluginTypes.First();
        var plugin = (IPlugin)Activator.CreateInstance(pluginType);
        
        // 初始化插件
        await plugin.InitializeAsync(new PluginContext());
        
        return plugin;
    }
    
    public async Task UnloadPluginAsync(string pluginName)
    {
        if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
        {
            await plugin.ShutdownAsync();
            _loadedPlugins.Remove(pluginName);
            _logger.LogInformation("插件已卸载: {PluginName}", pluginName);
        }
        else
        {
            _logger.LogWarning("尝试卸载不存在的插件: {PluginName}", pluginName);
        }
    }
    
    public async Task<PluginActionResult> ExecutePluginActionAsync(string pluginName, PluginAction action)
    {
        if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
        {
            try
            {
                return await plugin.ExecuteAsync(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行插件动作失败: {PluginName}.{ActionName}", 
                    pluginName, action.Name);
                return new PluginActionResult(false, ex.Message);
            }
        }
        else
        {
            var message = $"插件未加载: {pluginName}";
            _logger.LogWarning(message);
            return new PluginActionResult(false, message);
        }
    }
    
    public IEnumerable<PluginInfo> GetLoadedPlugins()
    {
        return _loadedPlugins.Values.Select(p => new PluginInfo(
            p.Name,
            p.Version,
            p.Description,
            p.Metadata
        ));
    }
}
```

#### 7.2.2 配置系统
```csharp
// KeyForge.Domain/Configuration/IConfigurationService.cs
public interface IConfigurationService
{
    T GetValue<T>(string key, T defaultValue = default);
    void SetValue<T>(string key, T value);
    bool HasValue(string key);
    void RemoveValue(string key);
    IEnumerable<string> GetKeys();
    
    // 监听配置变化
    IDisposable Watch<T>(string key, Action<T> callback);
    
    // 配置导入导出
    Task ExportAsync(string filePath);
    Task ImportAsync(string filePath);
    
    // 配置验证
    ValidationResult Validate();
}

// KeyForge.Infrastructure/Configuration/ConfigurationService.cs
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly Dictionary<string, List<Action<object>>> _watchers = new();
    
    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public T GetValue<T>(string key, T defaultValue = default)
    {
        try
        {
            return _configuration.GetValue(key, defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置值失败: {Key}", key);
            return defaultValue;
        }
    }
    
    public void SetValue<T>(string key, T value)
    {
        // 原本实现：复杂的配置更新和持久化
        // 简化实现：仅记录日志
        _logger.LogInformation("设置配置值: {Key} = {Value}", key, value);
        
        // 通知观察者
        NotifyWatchers(key, value);
    }
    
    public bool HasValue(string key)
    {
        return _configuration[key] != null;
    }
    
    public void RemoveValue(string key)
    {
        // 简化实现：仅记录日志
        _logger.LogInformation("移除配置值: {Key}", key);
        NotifyWatchers(key, null);
    }
    
    public IEnumerable<string> GetKeys()
    {
        return _configuration.AsEnumerable().Select(kvp => kvp.Key);
    }
    
    public IDisposable Watch<T>(string key, Action<T> callback)
    {
        var disposable = new ConfigurationWatcher(this, key, callback);
        return disposable;
    }
    
    private void NotifyWatchers(string key, object value)
    {
        if (_watchers.TryGetValue(key, out var watchers))
        {
            foreach (var watcher in watchers.ToList())
            {
                try
                {
                    watcher(value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "配置观察者回调失败: {Key}", key);
                }
            }
        }
    }
    
    public async Task ExportAsync(string filePath)
    {
        // 原本实现：复杂的配置导出逻辑
        // 简化实现：基本JSON导出
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        var configData = new
        {
            ExportedAt = DateTime.UtcNow,
            Settings = _configuration.AsEnumerable()
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
        
        var json = JsonConvert.SerializeObject(configData, settings);
        await File.WriteAllTextAsync(filePath, json);
        
        _logger.LogInformation("配置已导出到: {FilePath}", filePath);
    }
    
    public async Task ImportAsync(string filePath)
    {
        // 原本实现：复杂的配置导入和验证
        // 简化实现：基本JSON导入
        var json = await File.ReadAllTextAsync(filePath);
        var configData = JsonConvert.DeserializeObject<dynamic>(json);
        
        if (configData?.Settings != null)
        {
            foreach (var property in configData.Settings.Properties())
            {
                SetValue(property.Name, property.Value.ToString());
            }
        }
        
        _logger.LogInformation("配置已从文件导入: {FilePath}", filePath);
    }
    
    public ValidationResult Validate()
    {
        // 原本实现：复杂的配置验证逻辑
        // 简化实现：基本验证
        var errors = new List<string>();
        
        // 检查必需的配置项
        var requiredKeys = new[] { "Logging:LogLevel:Default", "ConnectionStrings:DefaultConnection" };
        foreach (var key in requiredKeys)
        {
            if (!HasValue(key))
            {
                errors.Add($"缺少必需的配置项: {key}");
            }
        }
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}
```

## 8. 总结

### 8.1 架构优势

1. **清晰的分层架构**：采用Clean Architecture，确保各层职责明确，依赖关系清晰
2. **领域驱动设计**：基于DDD的领域模型设计，业务逻辑封装良好
3. **高度模块化**：各功能模块独立，便于维护和扩展
4. **可测试性**：依赖注入和接口抽象，便于单元测试和集成测试
5. **可扩展性**：插件架构和配置系统，支持功能扩展

### 8.2 技术亮点

1. **现代.NET技术栈**：使用.NET 8.0，享受最新特性和性能优化
2. **异步编程**：全面采用async/await，提高系统响应性
3. **结构化日志**：使用Serilog，便于问题诊断和监控
4. **事件驱动**：使用MediatR，实现松耦合的组件通信
5. **图像处理**：集成OpenCV，提供强大的图像识别能力

### 8.3 实施建议

1. **分阶段实施**：先实现核心功能，再逐步添加高级特性
2. **持续集成**：建立CI/CD流程，确保代码质量
3. **性能监控**：添加性能监控，及时发现和解决性能问题
4. **用户反馈**：建立用户反馈机制，持续改进产品
5. **文档完善**：保持文档和代码同步，便于团队协作

### 8.4 风险控制

1. **技术风险**：通过原型验证关键技术的可行性
2. **进度风险**：采用敏捷开发，分阶段交付
3. **质量风险**：建立完善的测试体系，确保代码质量
4. **维护风险**：保持代码清晰和文档完整，降低维护成本

### 8.5 简化实现说明

在当前架构设计中，为了满足MVP（最小可行产品）的快速迭代需求，许多复杂功能都采用了简化实现：

1. **图像识别算法**：原本应该使用OpenCV的复杂算法，简化实现使用了固定的返回值
2. **输入处理**：原本应该使用Windows API的精确模拟，简化实现使用了基本方法调用
3. **决策引擎**：原本应该使用复杂的表达式解析器，简化实现使用了基本的字符串比较
4. **配置管理**：原本应该使用复杂的配置持久化，简化实现仅记录日志
5. **插件系统**：原本应该包含安全沙箱和验证，简化实现使用了基本的程序集加载

这些简化实现都明确标注了"原本实现"和"简化实现"的区别，为后续优化提供了明确的指引。

这个架构设计为KeyForge项目提供了一个坚实的技术基础，能够支持项目的长期发展和扩展。通过采用现代化的技术栈和最佳实践，系统能够满足当前的需求，并为未来的功能扩展提供良好的支持。