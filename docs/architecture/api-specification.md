# KeyForge API 规范文档

## 1. API 设计原则

### 1.1 设计原则
- **RESTful风格**：遵循REST架构风格
- **资源导向**：以资源为中心的API设计
- **版本控制**：支持API版本管理
- **一致性**：保持API设计的一致性
- **可扩展性**：支持未来的功能扩展

### 1.2 约定规范
- **URL格式**：使用kebab-case命名
- **HTTP方法**：使用标准HTTP方法
- **状态码**：使用标准HTTP状态码
- **数据格式**：使用JSON格式
- **错误处理**：统一的错误响应格式

## 2. 内部服务接口设计

### 2.1 领域服务接口

#### 2.1.1 自动化执行服务接口

```csharp
// 脚本执行服务接口
public interface IScriptExecutionService
{
    Task<ScriptExecutionResult> ExecuteScriptAsync(ScriptExecutionRequest request);
    Task<ScriptValidationResult> ValidateScriptAsync(ScriptValidationRequest request);
    Task<ScriptRecordingResult> StartRecordingAsync(ScriptRecordingRequest request);
    Task StopRecordingAsync(ScriptRecordingStopRequest request);
    event EventHandler<ScriptExecutionEvent> OnScriptExecutionEvent;
}

// 输入录制服务接口
public interface IInputRecordingService
{
    Task<InputRecordingResult> StartRecordingAsync(InputRecordingRequest request);
    Task StopRecordingAsync();
    Task<InputRecordingSession> GetRecordingSessionAsync(Guid sessionId);
    event EventHandler<InputEvent> OnInputRecorded;
}

// 操作执行服务接口
public interface IActionExecutionService
{
    Task<ActionExecutionResult> ExecuteActionAsync(ActionExecutionRequest request);
    Task<ActionResult> ExecuteKeyboardActionAsync(KeyboardActionRequest request);
    Task<ActionResult> ExecuteMouseActionAsync(MouseActionRequest request);
    Task<SequenceExecutionResult> ExecuteSequenceAsync(SequenceExecutionRequest request);
}
```

#### 2.1.2 图像识别服务接口

```csharp
// 图像识别服务接口
public interface IImageRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(RecognitionRequest request);
    Task<BatchRecognitionResult> RecognizeBatchAsync(BatchRecognitionRequest request);
    Task<TemplateCreationResult> CreateTemplateAsync(TemplateCreationRequest request);
    Task<TemplateUpdateResult> UpdateTemplateAsync(TemplateUpdateRequest request);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    event EventHandler<RecognitionEvent> OnRecognitionCompleted;
}

// 屏幕捕获服务接口
public interface IScreenCaptureService
{
    Task<ScreenCaptureResult> CaptureScreenAsync(ScreenCaptureRequest request);
    Task<ScreenCaptureResult> CaptureRegionAsync(ScreenRegionCaptureRequest request);
    Task<ScreenCaptureResult> CaptureWindowAsync(WindowCaptureRequest request);
    Task<Stream> StartLiveCaptureAsync(LiveCaptureRequest request);
    Task StopLiveCaptureAsync();
}

// 模板管理服务接口
public interface ITemplateManagementService
{
    Task<Template> GetTemplateAsync(Guid templateId);
    Task<IEnumerable<Template>> GetAllTemplatesAsync();
    Task<Template> SaveTemplateAsync(Template template);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<TemplateValidationResult> ValidateTemplateAsync(TemplateValidationRequest request);
}
```

#### 2.1.3 决策引擎服务接口

```csharp
// 规则引擎服务接口
public interface IRuleEngineService
{
    Task<RuleEvaluationResult> EvaluateRuleAsync(RuleEvaluationRequest request);
    Task<BatchRuleEvaluationResult> EvaluateRulesAsync(BatchRuleEvaluationRequest request);
    Task<RuleCreationResult> CreateRuleAsync(RuleCreationRequest request);
    Task<RuleUpdateResult> UpdateRuleAsync(RuleUpdateRequest request);
    Task<bool> DeleteRuleAsync(Guid ruleId);
    event EventHandler<RuleEvaluationEvent> OnRuleEvaluated;
}

// 状态机服务接口
public interface IStateMachineService
{
    Task<StateTransitionResult> TransitionAsync(StateTransitionRequest request);
    Task<StateMachineState> GetCurrentStateAsync(Guid stateMachineId);
    Task<StateMachineCreationResult> CreateStateMachineAsync(StateMachineCreationRequest request);
    Task<StateMachineUpdateResult> UpdateStateMachineAsync(StateMachineUpdateRequest request);
    Task<bool> DeleteStateMachineAsync(Guid stateMachineId);
}

// 表达式求值服务接口
public interface IExpressionEvaluationService
{
    Task<ExpressionEvaluationResult> EvaluateAsync(ExpressionEvaluationRequest request);
    Task<ExpressionValidationResult> ValidateAsync(ExpressionValidationRequest request);
    Task<IEnumerable<VariableDefinition>> GetVariablesAsync(string expression);
    Task<ExpressionCompilationResult> CompileAsync(ExpressionCompilationRequest request);
}
```

#### 2.1.4 系统管理服务接口

```csharp
// 配置管理服务接口
public interface IConfigurationService
{
    Task<ConfigurationValue> GetConfigurationAsync(string key);
    Task<Dictionary<string, ConfigurationValue>> GetConfigurationsAsync(IEnumerable<string> keys);
    Task SetConfigurationAsync(string key, ConfigurationValue value);
    Task SetConfigurationsAsync(Dictionary<string, ConfigurationValue> configurations);
    Task<bool> DeleteConfigurationAsync(string key);
    event EventHandler<ConfigurationChangedEvent> OnConfigurationChanged;
}

// 日志服务接口
public interface ILoggingService
{
    Task LogAsync(LogEntry entry);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryRequest request);
    Task<LogStatistics> GetLogStatisticsAsync(LogStatisticsRequest request);
    Task ClearLogsAsync(LogClearRequest request);
    Task ExportLogsAsync(LogExportRequest request);
}

// 监控服务接口
public interface IMonitoringService
{
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    Task<SystemHealth> GetSystemHealthAsync();
    Task<IEnumerable<PerformanceMetric>> GetMetricsHistoryAsync(MetricsHistoryRequest request);
    Task<Alert> CreateAlertAsync(AlertCreationRequest request);
    Task<IEnumerable<Alert>> GetAlertsAsync(AlertQueryRequest request);
}
```

### 2.2 应用服务接口

#### 2.2.1 用例服务接口

```csharp
// 脚本录制用例服务
public interface IRecordScriptUseCase
{
    Task<ScriptId> Execute(RecordScriptCommand command);
    Task<ScriptRecordingSession> GetRecordingSessionAsync(Guid sessionId);
    Task<bool> IsRecordingAsync(Guid sessionId);
}

// 图像触发用例服务
public interface IImageTriggerUseCase
{
    Task Execute(ImageTriggerCommand command);
    Task<ImageTriggerSession> CreateTriggerSessionAsync(ImageTriggerSessionRequest request);
    Task<bool> IsTriggerActiveAsync(Guid sessionId);
}

// 智能决策用例服务
public interface IDecisionMakingUseCase
{
    Task<DecisionResult> Execute(DecisionCommand command);
    Task<DecisionSession> CreateDecisionSessionAsync(DecisionSessionRequest request);
    Task<DecisionHistory> GetDecisionHistoryAsync(DecisionHistoryRequest request);
}
```

## 3. 外部集成接口设计

### 3.1 Web API 接口

#### 3.1.1 脚本管理 API

```yaml
# 脚本管理 API 规范
openapi: 3.0.0
info:
  title: KeyForge Script Management API
  version: 1.0.0
  description: 脚本管理相关的 REST API

paths:
  /api/v1/scripts:
    get:
      summary: 获取所有脚本
      operationId: getScripts
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
        - name: limit
          in: query
          schema:
            type: integer
            default: 20
        - name: status
          in: query
          schema:
            type: string
            enum: [active, inactive, draft]
      responses:
        200:
          description: 成功响应
          content:
            application/json:
              schema:
                type: object
                properties:
                  scripts:
                    type: array
                    items:
                      $ref: '#/components/schemas/Script'
                  pagination:
                    $ref: '#/components/schemas/Pagination'
    
    post:
      summary: 创建新脚本
      operationId: createScript
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateScriptRequest'
      responses:
        201:
          description: 创建成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'

  /api/v1/scripts/{scriptId}:
    get:
      summary: 获取指定脚本
      operationId: getScript
      parameters:
        - name: scriptId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        200:
          description: 成功响应
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'
        404:
          description: 脚本不存在
    
    put:
      summary: 更新脚本
      operationId: updateScript
      parameters:
        - name: scriptId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdateScriptRequest'
      responses:
        200:
          description: 更新成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'
    
    delete:
      summary: 删除脚本
      operationId: deleteScript
      parameters:
        - name: scriptId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        204:
          description: 删除成功

  /api/v1/scripts/{scriptId}/execute:
    post:
      summary: 执行脚本
      operationId: executeScript
      parameters:
        - name: scriptId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ExecuteScriptRequest'
      responses:
        200:
          description: 执行成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ScriptExecutionResult'

components:
  schemas:
    Script:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
        description:
          type: string
        version:
          type: string
        status:
          type: string
          enum: [active, inactive, draft]
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
        actionSequences:
          type: array
          items:
            $ref: '#/components/schemas/ActionSequence'
    
    ActionSequence:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
        actions:
          type: array
          items:
            $ref: '#/components/schemas/GameAction'
        order:
          type: integer
        loopCount:
          type: integer
    
    GameAction:
      type: object
      properties:
        id:
          type: string
          format: uuid
        type:
          type: string
          enum: [keyboard, mouse, delay]
        parameters:
          type: object
        delay:
          type: integer
          description: 延迟时间（毫秒）
    
    CreateScriptRequest:
      type: object
      required:
        - name
      properties:
        name:
          type: string
        description:
          type: string
        actionSequences:
          type: array
          items:
            $ref: '#/components/schemas/ActionSequence'
    
    UpdateScriptRequest:
      type: object
      properties:
        name:
          type: string
        description:
          type: string
        status:
          type: string
          enum: [active, inactive, draft]
        actionSequences:
          type: array
          items:
            $ref: '#/components/schemas/ActionSequence'
    
    ExecuteScriptRequest:
      type: object
      properties:
        parameters:
          type: object
        timeout:
          type: integer
          description: 超时时间（毫秒）
    
    ScriptExecutionResult:
      type: object
      properties:
        executionId:
          type: string
          format: uuid
        status:
          type: string
          enum: [running, completed, failed, cancelled]
        startTime:
          type: string
          format: date-time
        endTime:
          type: string
          format: date-time
        duration:
          type: integer
          description: 执行时间（毫秒）
        error:
          type: string
    
    Pagination:
      type: object
      properties:
        page:
          type: integer
        limit:
          type: integer
        total:
          type: integer
        totalPages:
          type: integer
```

#### 3.1.2 图像识别 API

```yaml
# 图像识别 API 规范
openapi: 3.0.0
info:
  title: KeyForge Image Recognition API
  version: 1.0.0
  description: 图像识别相关的 REST API

paths:
  /api/v1/templates:
    get:
      summary: 获取所有图像模板
      operationId: getTemplates
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
        - name: limit
          in: query
          schema:
            type: integer
            default: 20
      responses:
        200:
          description: 成功响应
          content:
            application/json:
              schema:
                type: object
                properties:
                  templates:
                    type: array
                    items:
                      $ref: '#/components/schemas/ImageTemplate'
                  pagination:
                    $ref: '#/components/schemas/Pagination'
    
    post:
      summary: 创建图像模板
      operationId: createTemplate
      requestBody:
        required: true
        content:
          multipart/form-data:
            schema:
              type: object
              required:
                - name
                - image
              properties:
                name:
                  type: string
                description:
                  type: string
                image:
                  type: string
                  format: binary
                parameters:
                  $ref: '#/components/schemas/RecognitionParameters'
      responses:
        201:
          description: 创建成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ImageTemplate'

  /api/v1/templates/{templateId}/recognize:
    post:
      summary: 执行图像识别
      operationId: recognizeImage
      parameters:
        - name: templateId
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        content:
          multipart/form-data:
            schema:
              type: object
              properties:
                image:
                  type: string
                  format: binary
                region:
                  $ref: '#/components/schemas/ScreenRegion'
      responses:
        200:
          description: 识别成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/RecognitionResult'

components:
  schemas:
    ImageTemplate:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
        description:
          type: string
        version:
          type: string
        parameters:
          $ref: '#/components/schemas/RecognitionParameters'
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
    
    RecognitionParameters:
      type: object
      properties:
        method:
          type: string
          enum: [template-matching, feature-matching, ocr]
        threshold:
          type: number
          format: float
          minimum: 0
          maximum: 1
        region:
          $ref: '#/components/schemas/ScreenRegion'
        preprocessing:
          type: object
          properties:
            grayscale:
              type: boolean
            resize:
              type: object
              properties:
                width:
                  type: integer
                height:
                  type: integer
            blur:
              type: object
              properties:
                kernelSize:
                  type: integer
    
    RecognitionResult:
      type: object
      properties:
        status:
          type: string
          enum: [success, failed, partial]
        location:
          $ref: '#/components/schemas/ScreenLocation'
        confidence:
          type: number
          format: float
          minimum: 0
          maximum: 1
        processingTime:
          type: integer
          description: 处理时间（毫秒）
        method:
          type: string
        error:
          type: string
    
    ScreenRegion:
      type: object
      properties:
        x:
          type: integer
        y:
          type: integer
        width:
          type: integer
        height:
          type: integer
    
    ScreenLocation:
      type: object
      properties:
        x:
          type: integer
        y:
          type: integer
```

### 3.2 WebSocket 接口

#### 3.2.1 实时通信接口

```csharp
// WebSocket 消息定义
public class WebSocketMessage
{
    public string MessageType { get; set; }
    public string MessageId { get; set; }
    public object Payload { get; set; }
    public DateTime Timestamp { get; set; }
}

// 消息类型
public static class WebSocketMessageTypes
{
    public const string ScriptExecutionUpdate = "script_execution_update";
    public const string ImageRecognitionUpdate = "image_recognition_update";
    public const string SystemStatusUpdate = "system_status_update";
    public const string PerformanceMetricsUpdate = "performance_metrics_update";
    public const string AlertNotification = "alert_notification";
}

// WebSocket 处理器
public class KeyForgeWebSocketHandler
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly IEventBus _eventBus;
    
    public async Task HandleConnectionAsync(WebSocket socket)
    {
        var connectionId = await _connectionManager.AddConnectionAsync(socket);
        
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var message = await ReceiveMessageAsync(socket);
                await ProcessMessageAsync(connectionId, message);
            }
        }
        finally
        {
            await _connectionManager.RemoveConnectionAsync(connectionId);
        }
    }
    
    private async Task<WebSocketMessage> ReceiveMessageAsync(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return JsonConvert.DeserializeObject<WebSocketMessage>(messageJson);
    }
    
    private async Task ProcessMessageAsync(string connectionId, WebSocketMessage message)
    {
        switch (message.MessageType)
        {
            case WebSocketMessageTypes.Subscribe:
                await HandleSubscriptionAsync(connectionId, message);
                break;
            case WebSocketMessageTypes.Unsubscribe:
                await HandleUnsubscriptionAsync(connectionId, message);
                break;
            default:
                await HandleCustomMessageAsync(connectionId, message);
                break;
        }
    }
}
```

## 4. 领域事件和命令定义

### 4.1 领域事件定义

#### 4.1.1 核心领域事件

```csharp
// 脚本相关事件
public record ScriptCreatedEvent(
    ScriptId ScriptId,
    ScriptName Name,
    Timestamp CreatedAt
) : DomainEvent;

public record ScriptUpdatedEvent(
    ScriptId ScriptId,
    ScriptName Name,
    ScriptVersion Version,
    Timestamp UpdatedAt
) : DomainEvent;

public record ScriptDeletedEvent(
    ScriptId ScriptId,
    Timestamp DeletedAt
) : DomainEvent;

public record ScriptExecutionStartedEvent(
    ScriptId ScriptId,
    ExecutionId ExecutionId,
    ExecutionContext Context,
    Timestamp StartedAt
) : DomainEvent;

public record ScriptExecutionCompletedEvent(
    ScriptId ScriptId,
    ExecutionId ExecutionId,
    ExecutionResult Result,
    Duration Duration,
    Timestamp CompletedAt
) : DomainEvent;

public record ScriptExecutionFailedEvent(
    ScriptId ScriptId,
    ExecutionId ExecutionId,
    string ErrorMessage,
    Exception Exception,
    Timestamp FailedAt
) : DomainEvent;

// 图像识别事件
public record ImageTemplateCreatedEvent(
    ImageTemplateId TemplateId,
    TemplateName Name,
    ImageData TemplateImage,
    Timestamp CreatedAt
) : DomainEvent;

public record ImageTemplateUpdatedEvent(
    ImageTemplateId TemplateId,
    TemplateName Name,
    TemplateVersion Version,
    Timestamp UpdatedAt
) : DomainEvent;

public record ImageTemplateDeletedEvent(
    ImageTemplateId TemplateId,
    Timestamp DeletedAt
) : DomainEvent;

public record ImageRecognitionCompletedEvent(
    ImageTemplateId TemplateId,
    RecognitionResult Result,
    ScreenRegion Region,
    Duration ProcessingTime,
    Timestamp CompletedAt
) : DomainEvent;

public record ImageRecognitionFailedEvent(
    ImageTemplateId TemplateId,
    string ErrorMessage,
    Exception Exception,
    Timestamp FailedAt
) : DomainEvent;

// 决策相关事件
public record DecisionRuleCreatedEvent(
    RuleId RuleId,
    RuleName Name,
    ConditionExpression Condition,
    Timestamp CreatedAt
) : DomainEvent;

public record DecisionRuleEvaluatedEvent(
    RuleId RuleId,
    RuleEvaluationResult Result,
    ExecutionContext Context,
    Timestamp EvaluatedAt
) : DomainEvent;

public record StateMachineStateChangedEvent(
    StateMachineId StateMachineId,
    StateId PreviousState,
    StateId CurrentState,
    StateTransitionResult Result,
    Timestamp ChangedAt
) : DomainEvent;

// 系统管理事件
public record ConfigurationChangedEvent(
    string ConfigurationKey,
    ConfigurationValue OldValue,
    ConfigurationValue NewValue,
    Timestamp ChangedAt
) : DomainEvent;

public record SystemAlertCreatedEvent(
    AlertId AlertId,
    AlertLevel Level,
    string Message,
    Timestamp CreatedAt
) : DomainEvent;

public record PerformanceThresholdExceededEvent(
    string MetricName,
    double CurrentValue,
    double ThresholdValue,
    Timestamp ExceededAt
) : DomainEvent;
```

### 4.2 命令定义

#### 4.2.1 应用层命令

```csharp
// 脚本相关命令
public record RecordScriptCommand(
    ScriptName Name,
    ScriptDescription Description,
    RecordingParameters Parameters
) : ICommand<ScriptId>;

public record ExecuteScriptCommand(
    ScriptId ScriptId,
    ExecutionParameters Parameters
) : ICommand<ExecutionId>;

public record UpdateScriptCommand(
    ScriptId ScriptId,
    ScriptName Name,
    ScriptDescription Description,
    IReadOnlyList<ActionSequence> ActionSequences
) : ICommand;

public record DeleteScriptCommand(
    ScriptId ScriptId
) : ICommand;

// 图像识别相关命令
public record CreateImageTemplateCommand(
    TemplateName Name,
    TemplateDescription Description,
    ImageData TemplateImage,
    RecognitionParameters Parameters
) : ICommand<ImageTemplateId>;

public record RecognizeImageCommand(
    ImageTemplateId TemplateId,
    ImageData SourceImage,
    ScreenRegion Region
) : ICommand<RecognitionResult>;

public record UpdateImageTemplateCommand(
    ImageTemplateId TemplateId,
    TemplateName Name,
    TemplateDescription Description,
    ImageData TemplateImage,
    RecognitionParameters Parameters
) : ICommand;

// 决策相关命令
public record EvaluateRuleCommand(
    RuleId RuleId,
    ExecutionContext Context
) : ICommand<RuleEvaluationResult>;

public record TransitionStateCommand(
    StateMachineId StateMachineId,
    StateId TargetState,
    ExecutionContext Context
) : ICommand<StateTransitionResult>;

public record CreateRuleCommand(
    RuleName Name,
    ConditionExpression Condition,
    ActionSequence ThenAction,
    ActionSequence ElseAction
) : ICommand<RuleId>;

// 系统管理相关命令
public record UpdateConfigurationCommand(
    string ConfigurationKey,
    ConfigurationValue Value
) : ICommand;

public record CreateAlertCommand(
    AlertLevel Level,
    string Message,
    AlertParameters Parameters
) : ICommand<AlertId>;

public record ClearLogsCommand(
    LogClearParameters Parameters
) : ICommand;
```

### 4.3 查询定义

#### 4.3.1 查询接口

```csharp
// 查询接口定义
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}

// 脚本相关查询
public record GetScriptQuery(ScriptId ScriptId) : IQuery<Script>;
public record GetAllScriptsQuery(ScriptFilter Filter) : IQuery<IReadOnlyList<Script>>;
public record GetScriptExecutionsQuery(ScriptId ScriptId) : IQuery<IReadOnlyList<ScriptExecution>>;

// 图像识别相关查询
public record GetImageTemplateQuery(ImageTemplateId TemplateId) : IQuery<ImageTemplate>;
public record GetAllImageTemplatesQuery(TemplateFilter Filter) : IQuery<IReadOnlyList<ImageTemplate>>;
public record GetRecognitionHistoryQuery(ImageTemplateId TemplateId) : IQuery<IReadOnlyList<RecognitionResult>>;

// 决策相关查询
public record GetRuleQuery(RuleId RuleId) : IQuery<DecisionRule>;
public record GetAllRulesQuery(RuleFilter Filter) : IQuery<IReadOnlyList<DecisionRule>>;
public record GetStateMachineQuery(StateMachineId StateMachineId) : IQuery<StateMachine>;

// 系统管理相关查询
public record GetConfigurationQuery(string Key) : IQuery<ConfigurationValue>;
public record GetLogsQuery(LogFilter Filter) : IQuery<IReadOnlyList<LogEntry>>;
public record GetPerformanceMetricsQuery(MetricsFilter Filter) : IQuery<PerformanceMetrics>;
```

## 5. 数据传输对象 (DTO) 规范

### 5.1 请求 DTO

#### 5.1.1 脚本管理 DTO

```csharp
// 创建脚本请求 DTO
public class CreateScriptRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; } = new();
    
    public CreateScriptCommand ToCommand()
    {
        return new CreateScriptCommand(
            new ScriptName(Name),
            new ScriptDescription(Description),
            ActionSequences.Select(dto => dto.ToDomainObject()).ToList()
        );
    }
}

// 更新脚本请求 DTO
public class UpdateScriptRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; } = new();
    
    public UpdateScriptCommand ToCommand(ScriptId scriptId)
    {
        return new UpdateScriptCommand(
            scriptId,
            new ScriptName(Name),
            new ScriptDescription(Description),
            ActionSequences.Select(dto => dto.ToDomainObject()).ToList()
        );
    }
}

// 执行脚本请求 DTO
public class ExecuteScriptRequestDto
{
    public Dictionary<string, object> Parameters { get; set; } = new();
    public int Timeout { get; set; } = 30000; // 30秒
    
    public ExecuteScriptCommand ToCommand(ScriptId scriptId)
    {
        return new ExecuteScriptCommand(
            scriptId,
            new ExecutionParameters(Parameters, Timeout)
        );
    }
}
```

#### 5.1.2 图像识别 DTO

```csharp
// 创建图像模板请求 DTO
public class CreateImageTemplateRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IFormFile Image { get; set; }
    public RecognitionParametersDto Parameters { get; set; }
    
    public async Task<CreateImageTemplateCommand> ToCommandAsync()
    {
        using var memoryStream = new MemoryStream();
        await Image.CopyToAsync(memoryStream);
        
        return new CreateImageTemplateCommand(
            new TemplateName(Name),
            new TemplateDescription(Description),
            new ImageData(memoryStream.ToArray(), Image.FileName),
            Parameters.ToDomainObject()
        );
    }
}

// 识别图像请求 DTO
public class RecognizeImageRequestDto
{
    public IFormFile Image { get; set; }
    public ScreenRegionDto Region { get; set; }
    
    public async Task<RecognizeImageCommand> ToCommandAsync(ImageTemplateId templateId)
    {
        using var memoryStream = new MemoryStream();
        await Image.CopyToAsync(memoryStream);
        
        return new RecognizeImageCommand(
            templateId,
            new ImageData(memoryStream.ToArray(), Image.FileName),
            Region.ToDomainObject()
        );
    }
}
```

### 5.2 响应 DTO

#### 5.2.1 脚本管理响应 DTO

```csharp
// 脚本响应 DTO
public class ScriptResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ActionSequenceDto> ActionSequences { get; set; } = new();
    
    public static ScriptResponseDto FromDomain(Script script)
    {
        return new ScriptResponseDto
        {
            Id = script.Id.Value.ToString(),
            Name = script.Name.Value,
            Description = script.Description.Value,
            Version = script.Version.Value,
            Status = script.Status.ToString(),
            CreatedAt = script.CreatedAt.Value,
            UpdatedAt = script.UpdatedAt.Value,
            ActionSequences = script.ActionSequences.Select(ActionSequenceDto.FromDomain).ToList()
        };
    }
}

// 脚本执行响应 DTO
public class ScriptExecutionResponseDto
{
    public string ExecutionId { get; set; }
    public string Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long Duration { get; set; }
    public string Error { get; set; }
    
    public static ScriptExecutionResponseDto FromResult(ScriptExecutionResult result)
    {
        return new ScriptExecutionResponseDto
        {
            ExecutionId = result.ExecutionId.Value.ToString(),
            Status = result.Status.ToString(),
            StartTime = result.StartTime.Value,
            EndTime = result.EndTime?.Value,
            Duration = result.Duration.Value.TotalMilliseconds,
            Error = result.Error
        };
    }
}
```

#### 5.2.2 图像识别响应 DTO

```csharp
// 图像模板响应 DTO
public class ImageTemplateResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public RecognitionParametersDto Parameters { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public static ImageTemplateResponseDto FromDomain(ImageTemplate template)
    {
        return new ImageTemplateResponseDto
        {
            Id = template.Id.Value.ToString(),
            Name = template.Name.Value,
            Description = template.Description.Value,
            Version = template.Version.Value,
            Parameters = RecognitionParametersDto.FromDomain(template.Parameters),
            CreatedAt = template.CreatedAt.Value,
            UpdatedAt = template.UpdatedAt.Value
        };
    }
}

// 识别结果响应 DTO
public class RecognitionResultResponseDto
{
    public string Status { get; set; }
    public ScreenLocationDto Location { get; set; }
    public double Confidence { get; set; }
    public long ProcessingTime { get; set; }
    public string Method { get; set; }
    public string Error { get; set; }
    
    public static RecognitionResultResponseDto FromDomain(RecognitionResult result)
    {
        return new RecognitionResultResponseDto
        {
            Status = result.Status.ToString(),
            Location = ScreenLocationDto.FromDomain(result.Location),
            Confidence = result.Confidence.Value,
            ProcessingTime = result.ProcessingTime.Value.TotalMilliseconds,
            Method = result.Method.ToString(),
            Error = result.Error
        };
    }
}
```

### 5.3 分页 DTO

```csharp
// 分页请求 DTO
public class PaginationRequestDto
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
    public string SortBy { get; set; } = "createdAt";
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}

// 分页响应 DTO
public class PaginationResponseDto<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadataDto Metadata { get; set; }
    
    public static PaginationResponseDto<T> Create<TItem>(
        IEnumerable<TItem> items,
        int page,
        int limit,
        int total,
        Func<TItem, T> selector)
    {
        return new PaginationResponseDto<T>
        {
            Items = items.Select(selector).ToList(),
            Metadata = new PaginationMetadataDto
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling(total / (double)limit)
            }
        };
    }
}

// 分页元数据 DTO
public class PaginationMetadataDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

// 排序方向枚举
public enum SortDirection
{
    Ascending,
    Descending
}
```

## 6. 错误处理和验证

### 6.1 错误响应格式

```csharp
// 错误响应 DTO
public class ErrorResponseDto
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; } = new();
    public string TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    
    public static ErrorResponseDto FromException(Exception ex, string traceId)
    {
        return new ErrorResponseDto
        {
            Code = GetErrorCode(ex),
            Message = ex.Message,
            Details = ex.StackTrace,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }
    
    public static ErrorResponseDto FromValidationException(
        ValidationException ex,
        string traceId)
    {
        return new ErrorResponseDto
        {
            Code = "VALIDATION_ERROR",
            Message = "Validation failed",
            ValidationErrors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private static string GetErrorCode(Exception ex)
    {
        return ex switch
        {
            NotFoundException => "NOT_FOUND",
            ValidationException => "VALIDATION_ERROR",
            UnauthorizedException => "UNAUTHORIZED",
            ForbiddenException => "FORBIDDEN",
            BusinessException => "BUSINESS_ERROR",
            _ => "INTERNAL_ERROR"
        };
    }
}
```

### 6.2 验证规则

```csharp
// 验证属性
public class ScriptNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var name = value as string;
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationResult("Script name is required");
        }
        
        if (name.Length < 3 || name.Length > 100)
        {
            return new ValidationResult("Script name must be between 3 and 100 characters");
        }
        
        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-_]+$"))
        {
            return new ValidationResult("Script name can only contain letters, numbers, spaces, hyphens and underscores");
        }
        
        return ValidationResult.Success;
    }
}

// 验证过滤器
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            var response = new ErrorResponseDto
            {
                Code = "VALIDATION_ERROR",
                Message = "Validation failed",
                ValidationErrors = errors,
                TraceId = context.HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };
            
            context.Result = new BadRequestObjectResult(response);
            return;
        }
        
        await next();
    }
}
```

## 7. 安全性设计

### 7.1 认证和授权

```csharp
// JWT 认证服务
public interface IJwtTokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    ClaimsPrincipal GetPrincipalFromToken(string token);
}

// 授权策略
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ScriptManagement = "ScriptManagement";
    public const string ImageRecognition = "ImageRecognition";
    public const string SystemConfiguration = "SystemConfiguration";
}

// 授权过滤器
public class AuthorizeWithPolicyAttribute : AuthorizeAttribute
{
    public AuthorizeWithPolicyAttribute(string policy)
    {
        Policy = policy;
    }
}
```

### 7.2 速率限制

```csharp
// 速率限制服务
public interface IRateLimitService
{
    Task<bool> IsAllowedAsync(string key, int limit, TimeSpan period);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string key);
}

// 速率限制过滤器
public class RateLimitFilter : IAsyncActionFilter
{
    private readonly IRateLimitService _rateLimitService;
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        var endpoint = context.HttpContext.Request.Path;
        
        var key = $"{clientIp}:{endpoint}";
        
        if (!await _rateLimitService.IsAllowedAsync(key, 100, TimeSpan.FromMinutes(1)))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
            return;
        }
        
        await next();
    }
}
```

## 8. 总结

KeyForge系统的API设计遵循现代化的RESTful架构原则，提供了完整的内部服务接口、外部集成接口、领域事件和命令定义。通过清晰的分层设计和标准化的数据传输对象，确保系统的可维护性和可扩展性。

### 8.1 设计亮点
- **RESTful API**：遵循REST架构风格，提供标准化的接口
- **CQRS模式**：分离命令和查询，提高系统性能
- **事件驱动**：通过领域事件实现松耦合的组件通信
- **类型安全**：使用强类型DTO确保数据一致性
- **安全性**：完整的认证、授权和速率限制机制

### 8.2 技术优势
- **标准化**：遵循OpenAPI规范，便于API文档生成
- **可扩展性**：支持API版本控制和功能扩展
- **性能优化**：异步处理和缓存机制提高响应速度
- **监控友好**：完整的日志记录和性能监控
- **错误处理**：统一的错误处理和验证机制