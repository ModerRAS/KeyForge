using KeyForge.Abstractions.Models.Application;

namespace KeyForge.Abstractions.Interfaces.Application
{
    /// <summary>
    /// 查询处理器基础接口
    /// 【优化实现】定义了CQRS架构中的查询处理器接口
    /// 原实现：查询逻辑直接写在控制器中，缺乏分层
    /// 优化：通过查询处理器接口，实现清晰的CQRS架构
    /// </summary>
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery
        where TResult : IQueryResult
    {
        /// <summary>
        /// 处理查询
        /// </summary>
        Task<TResult> HandleAsync(TQuery query);
        
        /// <summary>
        /// 验证查询
        /// </summary>
        Task<ValidationResult> ValidateAsync(TQuery query);
        
        /// <summary>
        /// 检查授权
        /// </summary>
        Task<bool> AuthorizeAsync(TQuery query);
    }
    
    /// <summary>
    /// 查询基础接口
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 查询ID
        /// </summary>
        Guid QueryId { get; }
        
        /// <summary>
        /// 查询类型
        /// </summary>
        string QueryType { get; }
        
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
    /// 查询结果基础接口
    /// </summary>
    public interface IQueryResult
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
        /// 总记录数
        /// </summary>
        int TotalCount { get; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        Dictionary<string, object> Metadata { get; }
    }
    
    /// <summary>
    /// 输入系统查询处理器接口
    /// </summary>
    public interface IInputQueryHandler :
        IQueryHandler<GetInputStatusQuery, InputQueryResult>,
        IQueryHandler<GetInputHistoryQuery, InputQueryResult>,
        IQueryHandler<GetHotkeyListQuery, InputQueryResult>,
        IQueryHandler<GetInputDevicesQuery, InputQueryResult>
    {
    }
    
    /// <summary>
    /// 图像系统查询处理器接口
    /// </summary>
    public interface IImageQueryHandler :
        IQueryHandler<GetImageTemplatesQuery, ImageQueryResult>,
        IQueryHandler<GetImageHistoryQuery, ImageQueryResult>,
        IQueryHandler<GetScreenInfoQuery, ImageQueryResult>,
        IQueryHandler<GetImageProcessingStatusQuery, ImageQueryResult>
    {
    }
    
    /// <summary>
    /// 脚本系统查询处理器接口
    /// </summary>
    public interface IScriptQueryHandler :
        IQueryHandler<GetScriptsQuery, ScriptQueryResult>,
        IQueryHandler<GetScriptDetailsQuery, ScriptQueryResult>,
        IQueryHandler<GetScriptHistoryQuery, ScriptQueryResult>,
        IQueryHandler<GetScriptExecutionStatusQuery, ScriptQueryResult>
    {
    }
    
    /// <summary>
    /// 配置系统查询处理器接口
    /// </summary>
    public interface IConfigurationQueryHandler :
        IQueryHandler<GetConfigurationQuery, ConfigurationQueryResult>,
        IQueryHandler<GetConfigurationHistoryQuery, ConfigurationQueryResult>,
        IQueryHandler<GetConfigurationSchemaQuery, ConfigurationQueryResult>,
        IQueryHandler<GetConfigurationValidationQuery, ConfigurationQueryResult>
    {
    }
    
    // 查询定义（简化示例）
    public record GetInputStatusQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetInputStatus";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetInputHistoryQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetInputHistory";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetHotkeyListQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetHotkeyList";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetInputDevicesQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetInputDevices";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetImageTemplatesQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetImageTemplates";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetImageHistoryQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetImageHistory";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetScreenInfoQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetScreenInfo";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetImageProcessingStatusQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetImageProcessingStatus";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetScriptsQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetScripts";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetScriptDetailsQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetScriptDetails";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetScriptHistoryQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetScriptHistory";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetScriptExecutionStatusQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetScriptExecutionStatus";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetConfigurationQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetConfiguration";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetConfigurationHistoryQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetConfigurationHistory";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetConfigurationSchemaQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetConfigurationSchema";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record GetConfigurationValidationQuery : IQuery
    {
        public Guid QueryId { get; init; } = Guid.NewGuid();
        public string QueryType { get; init; } = "GetConfigurationValidation";
        public string? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    // 查询结果定义（简化示例）
    public record InputQueryResult : IQueryResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public string ErrorCode { get; init; } = string.Empty;
        public object Data { get; init; }
        public int TotalCount { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ImageQueryResult : IQueryResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public string ErrorCode { get; init; } = string.Empty;
        public object Data { get; init; }
        public int TotalCount { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ScriptQueryResult : IQueryResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public string ErrorCode { get; init; } = string.Empty;
        public object Data { get; init; }
        public int TotalCount { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    public record ConfigurationQueryResult : IQueryResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public string ErrorCode { get; init; } = string.Empty;
        public object Data { get; init; }
        public int TotalCount { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
}