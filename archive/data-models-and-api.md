# KeyForge 数据模型和API接口设计

## 1. 数据模型设计

### 1.1 核心实体关系图

```mermaid
erDiagram
    Script ||--o{ ActionSequence : contains
    Script ||--o{ ScriptMetadata : has
    ActionSequence ||--o{ GameAction : contains
    ImageTemplate ||--o{ RecognitionParameters : has
    DecisionRule ||--o{ ConditionExpression : has
    DecisionRule ||--o{ ActionSequence : triggers
    StateMachine ||--o{ State : contains
    StateMachine ||--o{ Transition : contains
    SystemConfiguration ||--o{ ConfigurationItem : contains

    Script {
        Guid Id
        string Name
        string Description
        int Version
        ScriptStatus Status
        DateTime CreatedAt
        DateTime UpdatedAt
    }

    ActionSequence {
        Guid Id
        int Order
        int LoopCount
        TimeSpan EstimatedDuration
    }

    GameAction {
        Guid Id
        ActionType Type
        DateTime Timestamp
        int DelayMs
        string Parameters
    }

    ImageTemplate {
        Guid Id
        string Name
        byte[] ImageData
        int Width
        int Height
        DateTime CreatedAt
    }

    DecisionRule {
        Guid Id
        string Name
        string Condition
        int Priority
        RuleStatus Status
    }
}
```

### 1.2 数据库表设计

#### 1.2.1 Scripts表 - 脚本主表
```sql
CREATE TABLE Scripts (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT,
    Version INTEGER NOT NULL DEFAULT 1,
    Status INTEGER NOT NULL DEFAULT 0, -- 0: Draft, 1: Active, 2: Paused, 3: Stopped
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    Metadata TEXT -- JSON格式存储元数据
);

CREATE INDEX idx_scripts_name ON Scripts(Name);
CREATE INDEX idx_scripts_status ON Scripts(Status);
CREATE INDEX idx_scripts_created_at ON Scripts(CreatedAt);
```

#### 1.2.2 ActionSequences表 - 操作序列表
```sql
CREATE TABLE ActionSequences (
    Id TEXT PRIMARY KEY NOT NULL,
    ScriptId TEXT NOT NULL,
    OrderIndex INTEGER NOT NULL,
    LoopCount INTEGER NOT NULL DEFAULT 1,
    EstimatedDurationMs INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (ScriptId) REFERENCES Scripts(Id) ON DELETE CASCADE
);

CREATE INDEX idx_action_sequences_script_id ON ActionSequences(ScriptId);
CREATE INDEX idx_action_sequences_order ON ActionSequences(OrderIndex);
```

#### 1.2.3 GameActions表 - 游戏操作表
```sql
CREATE TABLE GameActions (
    Id TEXT PRIMARY KEY NOT NULL,
    SequenceId TEXT NOT NULL,
    ActionType INTEGER NOT NULL, -- 0: Keyboard, 1: Mouse, 2: Delay, 3: ImageCheck
    OrderIndex INTEGER NOT NULL,
    Parameters TEXT NOT NULL, -- JSON格式存储操作参数
    DelayMs INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (SequenceId) REFERENCES ActionSequences(Id) ON DELETE CASCADE
);

CREATE INDEX idx_game_actions_sequence_id ON GameActions(SequenceId);
CREATE INDEX idx_game_actions_order ON GameActions(OrderIndex);
```

#### 1.2.4 ImageTemplates表 - 图像模板表
```sql
CREATE TABLE ImageTemplates (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    ImageData BLOB NOT NULL,
    Width INTEGER NOT NULL,
    Height INTEGER NOT NULL,
    Parameters TEXT, -- JSON格式存储识别参数
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX idx_image_templates_name ON ImageTemplates(Name);
CREATE INDEX idx_image_templates_created_at ON ImageTemplates(CreatedAt);
```

#### 1.2.5 DecisionRules表 - 决策规则表
```sql
CREATE TABLE DecisionRules (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    ConditionExpression TEXT NOT NULL,
    ThenActionSequenceId TEXT,
    ElseActionSequenceId TEXT,
    Priority INTEGER NOT NULL DEFAULT 0,
    Status INTEGER NOT NULL DEFAULT 1, -- 0: Disabled, 1: Enabled
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (ThenActionSequenceId) REFERENCES ActionSequences(Id) ON DELETE SET NULL,
    FOREIGN KEY (ElseActionSequenceId) REFERENCES ActionSequences(Id) ON DELETE SET NULL
);

CREATE INDEX idx_decision_rules_name ON DecisionRules(Name);
CREATE INDEX idx_decision_rules_priority ON DecisionRules(Priority);
CREATE INDEX idx_decision_rules_status ON DecisionRules(Status);
```

#### 1.2.6 StateMachines表 - 状态机表
```sql
CREATE TABLE StateMachines (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    CurrentStateId TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX idx_state_machines_name ON StateMachines(Name);
```

#### 1.2.7 States表 - 状态表
```sql
CREATE TABLE States (
    Id TEXT PRIMARY KEY NOT NULL,
    StateMachineId TEXT NOT NULL,
    Name TEXT NOT NULL,
    EntryActionSequenceId TEXT,
    ExitActionSequenceId TEXT,
    TimeoutMs INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (StateMachineId) REFERENCES StateMachines(Id) ON DELETE CASCADE,
    FOREIGN KEY (EntryActionSequenceId) REFERENCES ActionSequences(Id) ON DELETE SET NULL,
    FOREIGN KEY (ExitActionSequenceId) REFERENCES ActionSequences(Id) ON DELETE SET NULL
);

CREATE INDEX idx_states_state_machine_id ON States(StateMachineId);
```

#### 1.2.8 Transitions表 - 状态转换表
```sql
CREATE TABLE Transitions (
    Id TEXT PRIMARY KEY NOT NULL,
    StateMachineId TEXT NOT NULL,
    FromStateId TEXT NOT NULL,
    ToStateId TEXT NOT NULL,
    ConditionExpression TEXT,
    ActionSequenceId TEXT,
    FOREIGN KEY (StateMachineId) REFERENCES StateMachines(Id) ON DELETE CASCADE,
    FOREIGN KEY (FromStateId) REFERENCES States(Id) ON DELETE CASCADE,
    FOREIGN KEY (ToStateId) REFERENCES States(Id) ON DELETE CASCADE,
    FOREIGN KEY (ActionSequenceId) REFERENCES ActionSequences(Id) ON DELETE SET NULL
);

CREATE INDEX idx_transitions_state_machine_id ON Transitions(StateMachineId);
CREATE INDEX idx_transitions_from_state_id ON Transitions(FromStateId);
```

#### 1.2.9 Configuration表 - 配置表
```sql
CREATE TABLE Configuration (
    Id TEXT PRIMARY KEY NOT NULL,
    Key TEXT NOT NULL UNIQUE,
    Value TEXT NOT NULL,
    ValueType TEXT NOT NULL, -- string, int, bool, double
    Category TEXT NOT NULL,
    Description TEXT,
    IsReadOnly INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX idx_configuration_key ON Configuration(Key);
CREATE INDEX idx_configuration_category ON Configuration(Category);
```

#### 1.2.10 Logs表 - 日志表
```sql
CREATE TABLE Logs (
    Id TEXT PRIMARY KEY NOT NULL,
    Timestamp TEXT NOT NULL,
    Level INTEGER NOT NULL, -- 0: Debug, 1: Info, 2: Warning, 3: Error, 4: Fatal
    Message TEXT NOT NULL,
    Exception TEXT,
    Category TEXT NOT NULL,
    Properties TEXT -- JSON格式存储额外属性
);

CREATE INDEX idx_logs_timestamp ON Logs(Timestamp);
CREATE INDEX idx_logs_level ON Logs(Level);
CREATE INDEX idx_logs_category ON Logs(Category);
```

### 1.3 Entity Framework Core 实体定义

#### 1.3.1 核心实体类
```csharp
// KeyForge.Domain/Entities/Script.cs
public class Script : Entity<ScriptId>
{
    public ScriptName Name { get; private set; }
    public ScriptDescription Description { get; private set; }
    public ScriptVersion Version { get; private set; }
    public ScriptStatus Status { get; private set; }
    public IReadOnlyList<ActionSequence> ActionSequences { get; private set; }
    public ScriptMetadata Metadata { get; private set; }
    
    // 原本实现：完整的业务逻辑封装
    // 简化实现：基本的属性和操作
    public Script(ScriptName name, ScriptDescription description, ScriptVersion version)
    {
        Id = ScriptId.New();
        Name = name;
        Description = description;
        Version = version;
        Status = ScriptStatus.Draft;
        ActionSequences = new List<ActionSequence>();
        Metadata = new ScriptMetadata();
        CreatedAt = Timestamp.Now;
        UpdatedAt = Timestamp.Now;
    }
    
    public void AddActionSequence(ActionSequence sequence)
    {
        // 简化实现：直接添加，缺少业务验证
        var sequences = new List<ActionSequence>(ActionSequences) { sequence };
        ActionSequences = sequences.AsReadOnly();
        UpdatedAt = Timestamp.Now;
    }
    
    public void UpdateStatus(ScriptStatus status)
    {
        Status = status;
        UpdatedAt = Timestamp.Now;
    }
    
    // 简化实现：缺少完整的业务规则验证
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (ActionSequences.Count == 0)
            errors.Add("脚本必须包含至少一个操作序列");
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}

// KeyForge.Domain/Entities/ActionSequence.cs
public class ActionSequence : Entity<SequenceId>
{
    public int Order { get; private set; }
    public int LoopCount { get; private set; }
    public IReadOnlyList<GameAction> Actions { get; private set; }
    
    public ActionSequence(int order, int loopCount = 1)
    {
        Id = SequenceId.New();
        Order = order;
        LoopCount = loopCount;
        Actions = new List<GameAction>();
    }
    
    public void AddAction(GameAction action)
    {
        // 简化实现：直接添加，缺少顺序验证
        var actions = new List<GameAction>(Actions) { action };
        Actions = actions.AsReadOnly();
    }
    
    // 简化实现：预估持续时间计算
    public Duration GetEstimatedDuration()
    {
        var totalDelay = Actions.Sum(a => a.Delay.Value);
        return Duration.FromMilliseconds(totalDelay);
    }
}

// KeyForge.Domain/Entities/GameAction.cs
public abstract class GameAction : Entity<ActionId>
{
    public ActionDelay Delay { get; protected set; }
    public Timestamp Timestamp { get; protected set; }
    
    protected GameAction(ActionDelay delay)
    {
        Id = ActionId.New();
        Delay = delay;
        Timestamp = Timestamp.Now;
    }
    
    public abstract ActionResult Execute(IGameInputHandler handler);
    
    // 简化实现：缺少完整的错误处理和重试机制
    public virtual async Task<ActionResult> ExecuteAsync(IGameInputHandler handler)
    {
        try
        {
            var result = Execute(handler);
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            // 简化实现：基本错误处理
            return ActionResult.Failure(ex.Message);
        }
    }
}

// KeyForge.Domain/Entities/KeyboardAction.cs
public class KeyboardAction : GameAction
{
    public VirtualKeyCode KeyCode { get; private set; }
    public KeyState State { get; private set; }
    public bool IsExtendedKey { get; private set; }
    
    public KeyboardAction(VirtualKeyCode keyCode, KeyState state, ActionDelay delay, bool isExtendedKey = false)
        : base(delay)
    {
        KeyCode = keyCode;
        State = state;
        IsExtendedKey = isExtendedKey;
    }
    
    public override ActionResult Execute(IGameInputHandler handler)
    {
        // 原本实现：完整的Windows API调用和错误处理
        // 简化实现：基本操作模拟
        try
        {
            handler.SendKeyboardInput(KeyCode, State, IsExtendedKey);
            return ActionResult.Success();
        }
        catch (Exception ex)
        {
            return ActionResult.Failure($"键盘操作失败: {ex.Message}");
        }
    }
}

// KeyForge.Domain/Entities/MouseAction.cs
public class MouseAction : GameAction
{
    public MousePosition Position { get; private set; }
    public MouseActionType Action { get; private set; }
    public MouseButton Button { get; private set; }
    public ScrollDelta Scroll { get; private set; }
    
    public MouseAction(MousePosition position, MouseActionType action, MouseButton button, ActionDelay delay, ScrollDelta scroll = null)
        : base(delay)
    {
        Position = position;
        Action = action;
        Button = button;
        Scroll = scroll;
    }
    
    public override ActionResult Execute(IGameInputHandler handler)
    {
        // 原本实现：完整的鼠标操作处理
        // 简化实现：基本鼠标操作
        try
        {
            switch (Action)
            {
                case MouseActionType.Move:
                    handler.MoveMouse(Position);
                    break;
                case MouseActionType.Click:
                    handler.ClickMouse(Button);
                    break;
                case MouseActionType.DoubleClick:
                    handler.DoubleClickMouse(Button);
                    break;
                case MouseActionType.Scroll:
                    handler.ScrollMouse(Scroll?.Delta ?? 0);
                    break;
            }
            return ActionResult.Success();
        }
        catch (Exception ex)
        {
            return ActionResult.Failure($"鼠标操作失败: {ex.Message}");
        }
    }
}
```

#### 1.3.2 图像识别相关实体
```csharp
// KeyForge.Domain/Entities/ImageTemplate.cs
public class ImageTemplate : Entity<ImageTemplateId>
{
    public TemplateName Name { get; private set; }
    public ImageData TemplateImage { get; private set; }
    public RecognitionParameters Parameters { get; private set; }
    public TemplateVersion Version { get; private set; }
    
    public ImageTemplate(TemplateName name, ImageData templateImage, RecognitionParameters parameters)
    {
        Id = ImageTemplateId.New();
        Name = name;
        TemplateImage = templateImage;
        Parameters = parameters;
        Version = TemplateVersion.Initial;
        CreatedAt = Timestamp.Now;
        UpdatedAt = Timestamp.Now;
    }
    
    // 原本实现：使用OpenCV进行图像识别
    // 简化实现：基本识别逻辑
    public RecognitionResult Recognize(ScreenCapture screen)
    {
        try
        {
            // 简化实现：返回固定结果
            // 实际应该使用OpenCV进行模板匹配
            return new RecognitionResult(
                RecognitionStatus.Success,
                new ScreenLocation(100, 100),
                new ConfidenceScore(0.85),
                RecognitionMethod.TemplateMatching,
                Duration.FromMilliseconds(50)
            );
        }
        catch (Exception ex)
        {
            return new RecognitionResult(
                RecognitionStatus.Failed,
                null,
                ConfidenceScore.Zero,
                RecognitionMethod.TemplateMatching,
                Duration.FromMilliseconds(0)
            );
        }
    }
    
    public void UpdateTemplate(ImageData newImage)
    {
        TemplateImage = newImage;
        Version = Version.Next();
        UpdatedAt = Timestamp.Now;
    }
}

// KeyForge.Domain/ValueObjects/RecognitionResult.cs
public class RecognitionResult : ValueObject
{
    public RecognitionStatus Status { get; private set; }
    public ScreenLocation Location { get; private set; }
    public ConfidenceScore Confidence { get; private set; }
    public RecognitionMethod Method { get; private set; }
    public Duration ProcessingTime { get; private set; }
    
    public RecognitionResult(RecognitionStatus status, ScreenLocation location, ConfidenceScore confidence, RecognitionMethod method, Duration processingTime)
    {
        Status = status;
        Location = location;
        Confidence = confidence;
        Method = method;
        ProcessingTime = processingTime;
    }
    
    public bool IsSuccessful() => Status == RecognitionStatus.Success;
    public bool IsReliable() => Confidence >= ConfidenceScore.Reliable;
}
```

## 2. API接口设计

### 2.1 应用服务接口

#### 2.1.1 脚本管理服务
```csharp
// KeyForge.Application/Interfaces/IScriptApplicationService.cs
public interface IScriptApplicationService
{
    Task<ScriptDto> CreateScriptAsync(CreateScriptRequest request);
    Task<ScriptDto> UpdateScriptAsync(UpdateScriptRequest request);
    Task<ScriptDto> GetScriptAsync(ScriptId id);
    Task<IEnumerable<ScriptDto>> GetAllScriptsAsync();
    Task<ScriptDto> DeleteScriptAsync(ScriptId id);
    Task<ScriptDto> ExecuteScriptAsync(ExecuteScriptRequest request);
    Task<ScriptDto> StopScriptAsync(ScriptId id);
    Task<ScriptDto> DuplicateScriptAsync(ScriptId id);
}

// KeyForge.Application/DTOs/ScriptDto.cs
public class ScriptDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Version { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; }
    public ScriptMetadataDto Metadata { get; set; }
}

// KeyForge.Application/Commands/CreateScriptRequest.cs
public class CreateScriptRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; }
}

// KeyForge.Application/Commands/ExecuteScriptRequest.cs
public class ExecuteScriptRequest
{
    public Guid ScriptId { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public bool Loop { get; set; }
    public int MaxLoops { get; set; }
}
```

#### 2.1.2 图像识别服务
```csharp
// KeyForge.Application/Interfaces/IImageRecognitionApplicationService.cs
public interface IImageRecognitionApplicationService
{
    Task<ImageTemplateDto> CreateTemplateAsync(CreateTemplateRequest request);
    Task<ImageTemplateDto> UpdateTemplateAsync(UpdateTemplateRequest request);
    Task<ImageTemplateDto> GetTemplateAsync(ImageTemplateId id);
    Task<IEnumerable<ImageTemplateDto>> GetAllTemplatesAsync();
    Task<RecognitionResultDto> RecognizeAsync(RecognizeRequest request);
    Task<bool> DeleteTemplateAsync(ImageTemplateId id);
    Task<ImageTemplateDto> CaptureTemplateAsync(CaptureTemplateRequest request);
}

// KeyForge.Application/DTOs/RecognitionResultDto.cs
public class RecognitionResultDto
{
    public bool Success { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public double Confidence { get; set; }
    public string Method { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string ErrorMessage { get; set; }
}
```

#### 2.1.3 决策引擎服务
```csharp
// KeyForge.Application/Interfaces/IDecisionMakingApplicationService.cs
public interface IDecisionMakingApplicationService
{
    Task<DecisionRuleDto> CreateRuleAsync(CreateRuleRequest request);
    Task<DecisionRuleDto> UpdateRuleAsync(UpdateRuleRequest request);
    Task<DecisionRuleDto> GetRuleAsync(RuleId id);
    Task<IEnumerable<DecisionRuleDto>> GetAllRulesAsync();
    Task<DecisionResultDto> EvaluateRuleAsync(EvaluateRuleRequest request);
    Task<bool> DeleteRuleAsync(RuleId id);
    Task<StateMachineDto> CreateStateMachineAsync(CreateStateMachineRequest request);
    Task<StateMachineDto> UpdateStateMachineAsync(UpdateStateMachineRequest request);
    Task<StateMachineDto> GetStateMachineAsync(StateMachineId id);
    Task<StateTransitionResultDto> TransitionStateAsync(TransitionStateRequest request);
}

// KeyForge.Application/DTOs/DecisionResultDto.cs
public class DecisionResultDto
{
    public bool Success { get; set; }
    public bool ConditionResult { get; set; }
    public ActionSequenceDto ExecutedAction { get; set; }
    public string ErrorMessage { get; set; }
}
```

### 2.2 领域服务接口

#### 2.2.1 输入处理服务
```csharp
// KeyForge.Domain/Interfaces/IGameInputHandler.cs
public interface IGameInputHandler
{
    void SendKeyboardInput(VirtualKeyCode keyCode, KeyState state, bool isExtendedKey = false);
    void MoveMouse(MousePosition position);
    void ClickMouse(MouseButton button);
    void DoubleClickMouse(MouseButton button);
    void ScrollMouse(int delta);
    MousePosition GetCurrentMousePosition();
    void SetMousePosition(MousePosition position);
}

// KeyForge.Domain/Interfaces/IInputRecorder.cs
public interface IInputRecorder
{
    Task<RecordingSession> StartRecordingAsync(RecordingParameters parameters);
    Task StopRecordingAsync();
    bool IsRecording { get; }
    event EventHandler<InputEvent> OnInputReceived;
}

// KeyForge.Domain/Interfaces/IActionExecutor.cs
public interface IActionExecutor
{
    Task<ActionResult> ExecuteActionAsync(GameAction action);
    Task<ActionResult> ExecuteSequenceAsync(ActionSequence sequence);
    Task<ActionResult> ExecuteScriptAsync(Script script, ExecutionContext context);
}
```

#### 2.2.2 图像识别服务
```csharp
// KeyForge.Domain/Interfaces/IImageRecognitionService.cs
public interface IImageRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(ScreenCapture screenCapture, ImageTemplate template, RecognitionParameters parameters);
    Task<ScreenCapture> CaptureScreenAsync(ScreenRegion region);
    Task<bool> ValidateTemplateAsync(ImageTemplate template);
    Task<List<RecognitionResult>> RecognizeMultipleAsync(ScreenCapture screenCapture, List<ImageTemplate> templates);
}

// KeyForge.Domain/Interfaces/IScreenCaptureService.cs
public interface IScreenCaptureService
{
    Task<ScreenCapture> CaptureScreenAsync(ScreenRegion region = null);
    Task<ScreenCapture> CaptureWindowAsync(IntPtr windowHandle);
    Task<byte[]> CaptureToByteArrayAsync(ScreenRegion region = null);
    Task SaveCaptureAsync(ScreenCapture capture, string filePath);
}
```

#### 2.2.3 决策引擎服务
```csharp
// KeyForge.Domain/Interfaces/IRuleEngine.cs
public interface IRuleEngine
{
    Task<RuleResult> EvaluateRuleAsync(RuleId ruleId, ExecutionContext context);
    Task<List<RuleResult>> EvaluateAllRulesAsync(ExecutionContext context);
    Task<RuleResult> AddRuleAsync(DecisionRule rule);
    Task<bool> RemoveRuleAsync(RuleId ruleId);
    Task<RuleResult> UpdateRuleAsync(DecisionRule rule);
}

// KeyForge.Domain/Interfaces/IStateMachineEngine.cs
public interface IStateMachineEngine
{
    Task<StateMachine> CreateStateMachineAsync(string name, List<State> states, List<Transition> transitions);
    Task<StateTransitionResult> TransitionAsync(StateMachineId machineId, StateId targetState, ExecutionContext context);
    Task<StateMachine> GetStateMachineAsync(StateMachineId machineId);
    Task<bool> CanTransitionAsync(StateMachineId machineId, StateId targetState);
}

// KeyForge.Domain/Interfaces/IContextProvider.cs
public interface IContextProvider
{
    Task<ExecutionContext> GetContextAsync(ContextId contextId);
    Task<ContextId> CreateContextAsync(Dictionary<string, object> initialData);
    Task<ContextId> UpdateContextAsync(ContextId contextId, Dictionary<string, object> updates);
    Task<bool> DeleteContextAsync(ContextId contextId);
}
```

### 2.3 基础设施服务接口

#### 2.3.1 数据访问接口
```csharp
// KeyForge.Domain/Interfaces/IScriptRepository.cs
public interface IScriptRepository
{
    Task<Script> GetByIdAsync(ScriptId id);
    Task<IEnumerable<Script>> GetAllAsync();
    Task<Script> AddAsync(Script script);
    Task<Script> UpdateAsync(Script script);
    Task<bool> DeleteAsync(ScriptId id);
    Task<bool> ExistsAsync(ScriptId id);
}

// KeyForge.Domain/Interfaces/IImageTemplateRepository.cs
public interface IImageTemplateRepository
{
    Task<ImageTemplate> GetByIdAsync(ImageTemplateId id);
    Task<IEnumerable<ImageTemplate>> GetAllAsync();
    Task<ImageTemplate> AddAsync(ImageTemplate template);
    Task<ImageTemplate> UpdateAsync(ImageTemplate template);
    Task<bool> DeleteAsync(ImageTemplateId id);
    Task<bool> ExistsAsync(ImageTemplateId id);
}

// KeyForge.Domain/Interfaces/IDecisionRuleRepository.cs
public interface IDecisionRuleRepository
{
    Task<DecisionRule> GetByIdAsync(RuleId id);
    Task<IEnumerable<DecisionRule>> GetAllAsync();
    Task<DecisionRule> AddAsync(DecisionRule rule);
    Task<DecisionRule> UpdateAsync(DecisionRule rule);
    Task<bool> DeleteAsync(RuleId id);
    Task<bool> ExistsAsync(RuleId id);
}
```

#### 2.3.2 配置和日志接口
```csharp
// KeyForge.Domain/Interfaces/IConfigurationService.cs
public interface IConfigurationService
{
    T GetValue<T>(string key);
    void SetValue<T>(string key, T value);
    bool HasValue(string key);
    void RemoveValue(string key);
    IEnumerable<string> GetKeys();
    void Save();
    void Reload();
}

// KeyForge.Domain/Interfaces/ILogService.cs
public interface ILogService
{
    void LogDebug(string message, Dictionary<string, object> properties = null);
    void LogInfo(string message, Dictionary<string, object> properties = null);
    void LogWarning(string message, Dictionary<string, object> properties = null);
    void LogError(string message, Exception exception = null, Dictionary<string, object> properties = null);
    void LogFatal(string message, Exception exception = null, Dictionary<string, object> properties = null);
    
    IEnumerable<LogEntry> GetLogs(DateTime? startTime = null, DateTime? endTime = null, LogLevel? level = null);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogFilter filter);
    void ClearLogs();
}

// KeyForge.Domain/Interfaces/IMetricsService.cs
public interface IMetricsService
{
    void IncrementCounter(string name, double value = 1);
    void RecordTiming(string name, TimeSpan duration);
    void RecordGauge(string name, double value);
    MetricsReport GetReport();
    Task<MetricsReport> GetReportAsync();
}
```

## 3. 数据传输对象 (DTO) 设计

### 3.1 请求/响应DTO

#### 3.1.1 脚本相关DTO
```csharp
// KeyForge.Application/DTOs/Requests/CreateScriptRequest.cs
public class CreateScriptRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("脚本名称不能为空");
        
        if (Name.Length > 100)
            errors.Add("脚本名称不能超过100个字符");
        
        if (ActionSequences == null || ActionSequences.Count == 0)
            errors.Add("脚本必须包含至少一个操作序列");
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}

// KeyForge.Application/DTOs/Requests/UpdateScriptRequest.cs
public class UpdateScriptRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

// KeyForge.Application/DTOs/Responses/ScriptResponse.cs
public class ScriptResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Version { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
```

#### 3.1.2 图像识别相关DTO
```csharp
// KeyForge.Application/DTOs/Requests/CreateTemplateRequest.cs
public class CreateTemplateRequest
{
    public string Name { get; set; }
    public byte[] ImageData { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public RecognitionParametersDto Parameters { get; set; }
    
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("模板名称不能为空");
        
        if (ImageData == null || ImageData.Length == 0)
            errors.Add("图像数据不能为空");
        
        if (Width <= 0 || Height <= 0)
            errors.Add("图像尺寸必须大于0");
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}

// KeyForge.Application/DTOs/Requests/RecognizeRequest.cs
public class RecognizeRequest
{
    public Guid TemplateId { get; set; }
    public ScreenRegionDto Region { get; set; }
    public RecognitionParametersDto Parameters { get; set; }
}

// KeyForge.Application/DTOs/Responses/RecognitionResponse.cs
public class RecognitionResponse
{
    public bool Success { get; set; }
    public RecognitionResultDto Result { get; set; }
    public string ErrorMessage { get; set; }
}
```

### 3.2 值对象DTO

#### 3.2.1 通用值对象DTO
```csharp
// KeyForge.Application/DTOs/ValueObjects/ActionSequenceDto.cs
public class ActionSequenceDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public int LoopCount { get; set; }
    public List<GameActionDto> Actions { get; set; }
    public long EstimatedDurationMs { get; set; }
}

// KeyForge.Application/DTOs/ValueObjects/GameActionDto.cs
public class GameActionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public int DelayMs { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

// KeyForge.Application/DTOs/ValueObjects/KeyboardActionDto.cs
public class KeyboardActionDto : GameActionDto
{
    public int KeyCode { get; set; }
    public string State { get; set; }
    public bool IsExtendedKey { get; set; }
}

// KeyForge.Application/DTOs/ValueObjects/MouseActionDto.cs
public class MouseActionDto : GameActionDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Action { get; set; }
    public string Button { get; set; }
    public int ScrollDelta { get; set; }
}
```

## 4. 事件模型设计

### 4.1 领域事件

#### 4.1.1 脚本相关事件
```csharp
// KeyForge.Domain/Events/ScriptCreatedEvent.cs
public class ScriptCreatedEvent : DomainEvent
{
    public ScriptId ScriptId { get; private set; }
    public ScriptName ScriptName { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    
    public ScriptCreatedEvent(ScriptId scriptId, ScriptName scriptName)
    {
        ScriptId = scriptId;
        ScriptName = scriptName;
        CreatedAt = Timestamp.Now;
    }
}

// KeyForge.Domain/Events/ScriptExecutedEvent.cs
public class ScriptExecutedEvent : DomainEvent
{
    public ScriptId ScriptId { get; private set; }
    public ExecutionResult Result { get; private set; }
    public Duration ExecutionDuration { get; private set; }
    public Timestamp ExecutedAt { get; private set; }
    
    public ScriptExecutedEvent(ScriptId scriptId, ExecutionResult result, Duration executionDuration)
    {
        ScriptId = scriptId;
        Result = result;
        ExecutionDuration = executionDuration;
        ExecutedAt = Timestamp.Now;
    }
}

// KeyForge.Domain/Events/ScriptExecutionFailedEvent.cs
public class ScriptExecutionFailedEvent : DomainEvent
{
    public ScriptId ScriptId { get; private set; }
    public string ErrorMessage { get; private set; }
    public Exception Exception { get; private set; }
    public Timestamp FailedAt { get; private set; }
    
    public ScriptExecutionFailedEvent(ScriptId scriptId, string errorMessage, Exception exception)
    {
        ScriptId = scriptId;
        ErrorMessage = errorMessage;
        Exception = exception;
        FailedAt = Timestamp.Now;
    }
}
```

#### 4.1.2 图像识别相关事件
```csharp
// KeyForge.Domain/Events/ImageRecognizedEvent.cs
public class ImageRecognizedEvent : DomainEvent
{
    public ImageTemplateId TemplateId { get; private set; }
    public RecognitionResult Result { get; private set; }
    public ScreenRegion Region { get; private set; }
    public Timestamp RecognizedAt { get; private set; }
    
    public ImageRecognizedEvent(ImageTemplateId templateId, RecognitionResult result, ScreenRegion region)
    {
        TemplateId = templateId;
        Result = result;
        Region = region;
        RecognizedAt = Timestamp.Now;
    }
}

// KeyForge.Domain/Events/ImageRecognitionFailedEvent.cs
public class ImageRecognitionFailedEvent : DomainEvent
{
    public ImageTemplateId TemplateId { get; private set; }
    public string ErrorMessage { get; private set; }
    public ScreenRegion Region { get; private set; }
    public Timestamp FailedAt { get; private set; }
    
    public ImageRecognitionFailedEvent(ImageTemplateId templateId, string errorMessage, ScreenRegion region)
    {
        TemplateId = templateId;
        ErrorMessage = errorMessage;
        Region = region;
        FailedAt = Timestamp.Now;
    }
}
```

### 4.2 集成事件

#### 4.2.1 系统集成事件
```csharp
// KeyForge.Application/Events/SystemMetricsEvent.cs
public class SystemMetricsEvent : IntegrationEvent
{
    public double CpuUsage { get; private set; }
    public double MemoryUsage { get; private set; }
    public int ActiveScripts { get; private set; }
    public int TotalExecutions { get; private set; }
    public Timestamp RecordedAt { get; private set; }
    
    public SystemMetricsEvent(double cpuUsage, double memoryUsage, int activeScripts, int totalExecutions)
    {
        CpuUsage = cpuUsage;
        MemoryUsage = memoryUsage;
        ActiveScripts = activeScripts;
        TotalExecutions = totalExecutions;
        RecordedAt = Timestamp.Now;
    }
}

// KeyForge.Application/Events/PerformanceWarningEvent.cs
public class PerformanceWarningEvent : IntegrationEvent
{
    public string MetricName { get; private set; }
    public double CurrentValue { get; private set; }
    public double Threshold { get; private set; }
    public string WarningMessage { get; private set; }
    public Timestamp WarningAt { get; private set; }
    
    public PerformanceWarningEvent(string metricName, double currentValue, double threshold, string warningMessage)
    {
        MetricName = metricName;
        CurrentValue = currentValue;
        Threshold = threshold;
        WarningMessage = warningMessage;
        WarningAt = Timestamp.Now;
    }
}
```

## 5. API版本控制

### 5.1 版本控制策略
```csharp
// KeyForge.Api/Controllers/ApiVersionController.cs
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ApiVersionController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1()
    {
        return Ok(new { Version = "1.0", Message = "API Version 1.0" });
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2()
    {
        return Ok(new { Version = "2.0", Message = "API Version 2.0" });
    }
}
```

### 5.2 API文档配置
```csharp
// KeyForge.Api/ConfigureServices.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "KeyForge API",
                Version = "v1.0",
                Description = "KeyForge 自动化按键脚本系统 API"
            });
            
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "KeyForge API",
                Version = "v2.0",
                Description = "KeyForge 自动化按键脚本系统 API v2.0"
            });
            
            // 包含XML注释
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            
            // 支持JWT认证
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });
        
        return services;
    }
}
```

## 6. 数据验证和错误处理

### 6.1 验证模型
```csharp
// KeyForge.Application/Validation/ValidationResult.cs
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public List<string> Errors { get; private set; }
    
    public ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }
    
    public static ValidationResult Success() => new ValidationResult(true, new List<string>());
    public static ValidationResult Failure(List<string> errors) => new ValidationResult(false, errors);
    public static ValidationResult Failure(string error) => new ValidationResult(false, new List<string> { error });
}

// KeyForge.Application/Validation/FluentValidation配置
public class CreateScriptRequestValidator : AbstractValidator<CreateScriptRequest>
{
    public CreateScriptRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("脚本名称不能为空")
            .MaximumLength(100).WithMessage("脚本名称不能超过100个字符");
        
        RuleFor(x => x.ActionSequences)
            .NotEmpty().WithMessage("脚本必须包含至少一个操作序列");
        
        RuleForEach(x => x.ActionSequences)
            .ChildRules(actions =>
            {
                actions.RuleFor(a => a.Actions)
                    .NotEmpty().WithMessage("操作序列必须包含至少一个操作");
            });
    }
}
```

### 6.2 错误处理中间件
```csharp
// KeyForge.Api/Middleware/ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ValidationException validationEx => new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList()
            },
            NotFoundException notFoundEx => new ApiResponse<object>
            {
                Success = false,
                Message = notFoundEx.Message,
                StatusCode = StatusCodes.Status404NotFound
            },
            BusinessException businessEx => new ApiResponse<object>
            {
                Success = false,
                Message = businessEx.Message,
                StatusCode = StatusCodes.Status400BadRequest
            },
            _ => new ApiResponse<object>
            {
                Success = false,
                Message = "An internal error occurred",
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
        
        context.Response.StatusCode = response.StatusCode ?? StatusCodes.Status500InternalServerError;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}
```

## 7. 安全和认证

### 7.1 JWT认证配置
```csharp
// KeyForge.Api/Authentication/JwtConfiguration.cs
public static class JwtConfiguration
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });
        
        return services;
    }
    
    public static string GenerateJwtToken(this IConfiguration configuration, string username, IEnumerable<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

这个数据模型和API接口设计提供了完整的系统数据结构定义，包括：

1. **完整的数据库表设计** - 支持所有核心功能
2. **实体框架模型** - 采用DDD模式的实体定义
3. **应用服务接口** - 清晰的层次分离
4. **数据传输对象** - 完整的DTO体系
5. **事件模型** - 领域事件和集成事件
6. **API版本控制** - 支持多版本API
7. **验证和错误处理** - 完整的验证体系
8. **安全认证** - JWT认证机制

所有的简化实现都有明确标注，便于后续优化时进行替换和完善。