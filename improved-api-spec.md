# KeyForge 改进API设计

## API概览

KeyForge提供了一套完整的按键录制和回放API，采用简洁的接口设计，支持同步和异步操作。

### 基础信息
- **版本**: 1.0.0
- **协议**: 内部API (非REST)
- **数据格式**: JSON
- **线程安全**: 是

## 核心接口

### IKeyForgeService
```csharp
// 简化实现：核心服务接口
// 原本实现：分散在多个类中的功能
// 简化实现：统一的服务接口，管理所有核心功能
public interface IKeyForgeService : IDisposable
{
    // 录制控制
    Task StartRecordingAsync();
    Task StopRecordingAsync();
    bool IsRecording { get; }
    
    // 播放控制
    Task PlayScriptAsync(PlaybackOptions? options = null);
    Task StopPlaybackAsync();
    bool IsPlaying { get; }
    
    // 脚本管理
    Task SaveScriptAsync(string filePath);
    Task LoadScriptAsync(string filePath);
    Task ClearScriptAsync();
    
    // 状态查询
    ScriptStats GetScriptStats();
    IReadOnlyList<KeyAction> GetActions();
    
    // 事件
    event EventHandler<KeyAction>? KeyRecorded;
    event EventHandler<KeyAction>? KeyPlayed;
    event EventHandler<string>? StatusChanged;
    event EventHandler<ErrorEventArgs>? ErrorOccurred;
}
```

### IWindowsHook
```csharp
// 简化实现：Windows Hook接口
// 原本实现：Timer轮询所有按键状态
// 简化实现：事件驱动的按键监听
public interface IWindowsHook : IDisposable
{
    // Hook控制
    void Start();
    void Stop();
    bool IsActive { get; }
    
    // 事件
    event EventHandler<KeyEventArgs>? KeyDown;
    event EventHandler<KeyEventArgs>? KeyUp;
    
    // 过滤器
    void AddKeyFilter(Keys key);
    void RemoveKeyFilter(Keys key);
    void ClearKeyFilters();
}
```

### IScriptManager
```csharp
// 简化实现：脚本管理接口
// 原本实现：复杂的仓储模式
// 简化实现：直接的脚本操作接口
public interface IScriptManager
{
    // 脚本操作
    void AddAction(KeyAction action);
    void RemoveAction(int index);
    void ClearActions();
    
    // 脚本信息
    IReadOnlyList<KeyAction> Actions { get; }
    ScriptStats GetStats();
    
    // 序列化
    string Serialize();
    void Deserialize(string json);
    
    // 验证
    ValidationResult Validate();
}
```

## 数据模型

### KeyAction
```csharp
// 简化实现：按键动作模型
// 原本实现：复杂的领域模型和值对象
// 简化实现：简单的POCO类，包含必要属性
public class KeyAction
{
    // 基本属性
    public int KeyCode { get; set; }
    public string KeyName { get; set; }
    public bool IsKeyDown { get; set; }
    
    // 时间信息
    public int Delay { get; set; } // 相对延迟（毫秒）
    public DateTime Timestamp { get; set; }
    
    // 扩展属性
    public KeyType KeyType { get; set; }
    public int RepeatCount { get; set; }
    
    // 构造函数
    public KeyAction(int keyCode, string keyName, bool isKeyDown)
    {
        KeyCode = keyCode;
        KeyName = keyName;
        IsKeyDown = isKeyDown;
        Timestamp = DateTime.Now;
        Delay = 0;
        KeyType = KeyType.Keyboard;
        RepeatCount = 1;
    }
    
    // 序列化支持
    public string ToJson() => JsonSerializer.Serialize(this);
    public static KeyAction? FromJson(string json) => 
        JsonSerializer.Deserialize<KeyAction>(json);
}
```

### ScriptStats
```csharp
// 简化实现：脚本统计信息
// 原本实现：复杂的统计对象
// 简化实现：简单的统计信息类
public class ScriptStats
{
    // 基本统计
    public int TotalActions { get; set; }
    public int KeyDownActions { get; set; }
    public int KeyUpActions { get; set; }
    
    // 时间统计
    public int Duration { get; set; } // 总时长（毫秒）
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    
    // 性能统计
    public double AverageDelay { get; set; }
    public int MaxDelay { get; set; }
    public int MinDelay { get; set; }
    
    // 计算属性
    public int UniqueKeys => TotalActions > 0 ? 
        Actions.GroupBy(a => a.KeyCode).Count() : 0;
}
```

### PlaybackOptions
```csharp
// 简化实现：播放选项
// 原本实现：硬编码的播放参数
// 简化实现：可配置的播放选项
public class PlaybackOptions
{
    // 速度控制
    public double SpeedMultiplier { get; set; } = 1.0;
    public bool PreserveTiming { get; set; } = true;
    
    // 循环控制
    public int RepeatCount { get; set; } = 1;
    public bool LoopForever { get; set; } = false;
    
    // 错误处理
    public bool ContinueOnError { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    
    // 高级选项
    public bool RandomizeTiming { get; set; } = false;
    public double RandomizationFactor { get; set; } = 0.1;
}
```

## 事件系统

### KeyEventArgs
```csharp
// 简化实现：按键事件参数
// 原本实现：分散的事件参数
// 简化实现：统一的按键事件参数
public class KeyEventArgs : EventArgs
{
    public int KeyCode { get; set; }
    public string KeyName { get; set; }
    public bool IsKeyDown { get; set; }
    public DateTime Timestamp { get; set; }
    public KeyType KeyType { get; set; }
    
    public KeyEventArgs(int keyCode, string keyName, bool isKeyDown)
    {
        KeyCode = keyCode;
        KeyName = keyName;
        IsKeyDown = isKeyDown;
        Timestamp = DateTime.Now;
        KeyType = KeyType.Keyboard;
    }
}
```

### ErrorEventArgs
```csharp
// 简化实现：错误事件参数
// 原本实现：直接抛出异常
// 简化实现：结构化的错误信息
public class ErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; set; }
    public string ErrorDetails { get; set; }
    public Exception? Exception { get; set; }
    public ErrorSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    
    public ErrorEventArgs(string errorMessage, ErrorSeverity severity = ErrorSeverity.Error)
    {
        ErrorMessage = errorMessage;
        Severity = severity;
        Timestamp = DateTime.Now;
    }
}
```

### ErrorSeverity
```csharp
// 简化实现：错误严重程度
public enum ErrorSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}
```

## 配置API

### KeyForgeConfig
```csharp
// 简化实现：应用配置
// 原本实现：硬编码的配置
// 简化实现：结构化的配置管理
public class KeyForgeConfig
{
    // 录制设置
    public RecordingConfig Recording { get; set; } = new();
    
    // 播放设置
    public PlaybackConfig Playback { get; set; } = new();
    
    // 界面设置
    public UIConfig UI { get; set; } = new();
    
    // 高级设置
    public AdvancedConfig Advanced { get; set; } = new();
    
    // 序列化
    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    });
    
    public static KeyForgeConfig FromJson(string json) => 
        JsonSerializer.Deserialize<KeyForgeConfig>(json) ?? new();
}

public class RecordingConfig
{
    public bool RecordMouseEvents { get; set; } = false;
    public bool RecordSystemKeys { get; set; } = true;
    public int MaxRecordingTime { get; set; } = 300; // 秒
    public bool AutoSave { get; set; } = false;
    public string AutoSavePath { get; set; } = "./scripts";
}

public class PlaybackConfig
{
    public double DefaultSpeed { get; set; } = 1.0;
    public bool ShowPlaybackProgress { get; set; } = true;
    public bool PlaySoundOnComplete { get; set; } = false;
    public int DefaultRepeatCount { get; set; } = 1;
}

public class UIConfig
{
    public string Theme { get; set; } = "Default";
    public bool ShowStats { get; set; } = true;
    public bool ShowTimestamps { get; set; } = true;
    public int MaxLogEntries { get; set; } = 1000;
}

public class AdvancedConfig
{
    public bool EnableDebugMode { get; set; } = false;
    public string LogPath { get; set; } = "./logs";
    public LogLevel LogLevel { get; set; } = LogLevel.Info;
    public bool EnableTelemetry { get; set; } = false;
}
```

## 验证API

### ValidationResult
```csharp
// 简化实现：验证结果
// 原本实现：没有验证
// 简化实现：结构化的验证结果
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
    
    public string GetFormattedMessage()
    {
        var sb = new StringBuilder();
        
        if (Errors.Count > 0)
        {
            sb.AppendLine("错误:");
            foreach (var error in Errors)
            {
                sb.AppendLine($"  - {error}");
            }
        }
        
        if (Warnings.Count > 0)
        {
            sb.AppendLine("警告:");
            foreach (var warning in Warnings)
            {
                sb.AppendLine($"  - {warning}");
            }
        }
        
        return sb.ToString();
    }
}
```

## 插件API

### IKeyForgePlugin
```csharp
// 简化实现：插件接口
// 原本实现：没有插件系统
// 简化实现：简单的插件扩展机制
public interface IKeyForgePlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    
    // 生命周期
    Task InitializeAsync(IKeyForgeService service);
    Task ShutdownAsync();
    
    // 事件处理
    Task OnKeyRecordedAsync(KeyAction action);
    Task OnKeyPlayedAsync(KeyAction action);
    Task OnErrorAsync(ErrorEventArgs errorArgs);
    
    // 自定义功能
    Task<object?> ExecuteCommandAsync(string command, object? parameters);
}
```

## 异步操作模式

### 异步API设计原则
1. **一致性**：所有可能阻塞的操作都提供异步版本
2. **取消支持**：所有异步操作都支持CancellationToken
3. **进度报告**：长时间操作支持进度报告
4. **错误处理**：统一的错误处理机制

### 示例：异步录制
```csharp
// 简化实现：异步录制API
// 原本实现：同步操作，可能阻塞UI
// 简化实现：完全异步，支持取消和进度
public async Task StartRecordingAsync(
    CancellationToken cancellationToken = default,
    IProgress<RecordingProgress>? progress = null)
{
    try
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // 报告开始
        progress?.Report(new RecordingProgress
        {
            Status = RecordingStatus.Starting,
            Message = "开始录制..."
        });
        
        // 启动Hook
        _windowsHook.Start();
        
        // 报告进行中
        progress?.Report(new RecordingProgress
        {
            Status = RecordingStatus.Recording,
            Message = "录制中...",
            StartTime = DateTime.Now
        });
        
        // 等待取消信号
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            await Task.Delay(100, _cancellationTokenSource.Token);
            
            // 报告进度
            progress?.Report(new RecordingProgress
        {
            Status = RecordingStatus.Recording,
            Message = $"录制中... ({_scriptManager.Actions.Count} 个动作)",
            ActionCount = _scriptManager.Actions.Count,
            Duration = (int)(DateTime.Now - _recordingStartTime).TotalMilliseconds
        });
        }
    }
    catch (OperationCanceledException)
    {
        progress?.Report(new RecordingProgress
        {
            Status = RecordingStatus.Cancelled,
            Message = "录制已取消"
        });
        throw;
    }
    catch (Exception ex)
    {
        progress?.Report(new RecordingProgress
        {
            Status = RecordingStatus.Error,
            Message = $"录制错误: {ex.Message}"
        });
        throw;
    }
}
```

## 总结

改进后的API设计具有以下特点：

1. **简洁性**：接口设计简洁明了，易于理解和使用
2. **一致性**：所有API遵循统一的设计模式
3. **可扩展性**：支持插件和自定义扩展
4. **异步支持**：完整的异步操作支持
5. **错误处理**：统一的错误处理和报告机制
6. **配置管理**：灵活的配置系统
7. **验证机制**：内置的输入验证

这个API设计为KeyForge提供了坚实的基础，既保持了简单性，又具备了企业级应用的可扩展性。