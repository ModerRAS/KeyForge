namespace KeyForge.Abstractions.Interfaces.Application
{
    /// <summary>
    /// 命令处理器基础接口
    /// 【优化实现】定义了CQRS架构中的命令处理器接口
    /// 原实现：命令处理逻辑直接写在控制器中，缺乏分层
    /// 优化：通过命令处理器接口，实现清晰的CQRS架构
    /// </summary>
    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand
        where TResult : ICommandResult
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        Task<TResult> HandleAsync(TCommand command);
        
        /// <summary>
        /// 验证命令
        /// </summary>
        Task<ValidationResult> ValidateAsync(TCommand command);
        
        /// <summary>
        /// 检查授权
        /// </summary>
        Task<bool> AuthorizeAsync(TCommand command);
    }
    
    /// <summary>
    /// 命令基础接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 命令ID
        /// </summary>
        Guid CommandId { get; }
        
        /// <summary>
        /// 命令类型
        /// </summary>
        string CommandType { get; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        string? UserId { get; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        DateTime Timestamp { get; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        Dictionary<string, object> Metadata { get; }
    }
    
    /// <summary>
    /// 命令结果基础接口
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool Success { get; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        string? ErrorMessage { get; }
        
        /// <summary>
        /// 错误代码
        /// </summary>
        string? ErrorCode { get; }
        
        /// <summary>
        /// 数据
        /// </summary>
        object? Data { get; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        Dictionary<string, object> Metadata { get; }
    }
    
    /// <summary>
    /// 输入系统命令处理器接口
    /// </summary>
    public interface IInputCommandHandler :
        ICommandHandler<StartInputCommand, InputCommandResult>,
        ICommandHandler<StopInputCommand, InputCommandResult>,
        ICommandHandler<SendInputCommand, InputCommandResult>,
        ICommandHandler<RegisterHotkeyCommand, InputCommandResult>,
        ICommandHandler<UnregisterHotkeyCommand, InputCommandResult>
    {
    }
    
    /// <summary>
    /// 图像系统命令处理器接口
    /// </summary>
    public interface IImageCommandHandler :
        ICommandHandler<CaptureImageCommand, ImageCommandResult>,
        ICommandHandler<CreateTemplateCommand, ImageCommandResult>,
        ICommandHandler<RecognizeImageCommand, ImageCommandResult>,
        ICommandHandler<ProcessImageCommand, ImageCommandResult>,
        ICommandHandler<AnalyzeImageCommand, ImageCommandResult>
    {
    }
    
    /// <summary>
    /// 脚本系统命令处理器接口
    /// </summary>
    public interface IScriptCommandHandler :
        ICommandHandler<StartRecordingCommand, ScriptCommandResult>,
        ICommandHandler<StopRecordingCommand, ScriptCommandResult>,
        ICommandHandler<ExecuteScriptCommand, ScriptCommandResult>,
        ICommandHandler<DebugScriptCommand, ScriptCommandResult>,
        ICommandHandler<ManageScriptCommand, ScriptCommandResult>
    {
    }
    
    /// <summary>
    /// 配置系统命令处理器接口
    /// </summary>
    public interface IConfigurationCommandHandler :
        ICommandHandler<GetConfigurationCommand, ConfigurationCommandResult>,
        ICommandHandler<SetConfigurationCommand, ConfigurationCommandResult>,
        ICommandHandler<ValidateConfigurationCommand, ConfigurationCommandResult>,
        ICommandHandler<ResetConfigurationCommand, ConfigurationCommandResult>,
        ICommandHandler<ImportExportConfigurationCommand, ConfigurationCommandResult>
    {
    }
    
    // 命令定义（简化示例）
    public record StartInputCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "StartInput";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record StopInputCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "StopInput";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record SendInputCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "SendInput";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record RegisterHotkeyCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "RegisterHotkey";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record UnregisterHotkeyCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "UnregisterHotkey";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record CaptureImageCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "CaptureImage";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record CreateTemplateCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "CreateTemplate";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record RecognizeImageCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "RecognizeImage";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ProcessImageCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ProcessImage";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record AnalyzeImageCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "AnalyzeImage";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record StartRecordingCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "StartRecording";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record StopRecordingCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "StopRecording";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ExecuteScriptCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ExecuteScript";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record DebugScriptCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "DebugScript";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ManageScriptCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ManageScript";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetConfigurationCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "GetConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record SetConfigurationCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "SetConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ValidateConfigurationCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ValidateConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ResetConfigurationCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ResetConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ImportExportConfigurationCommand : ICommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public string CommandType { get; init; } = "ImportExportConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    // 命令结果定义（简化示例）
    public record InputCommandResult : ICommandResult;
    public record ImageCommandResult : ICommandResult;
    public record ScriptCommandResult : ICommandResult;
    public record ConfigurationCommandResult : ICommandResult;
}