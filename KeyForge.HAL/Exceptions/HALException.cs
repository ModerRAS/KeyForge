using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Exceptions;

/// <summary>
/// HAL异常基类
/// </summary>
public class HALException : Exception
{
    /// <summary>
    /// 异常类型
    /// </summary>
    public string ExceptionType { get; init; } = string.Empty;

    /// <summary>
    /// 错误代码
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// 严重程度
    /// </summary>
    public ExceptionSeverity Severity { get; init; } = ExceptionSeverity.Error;

    /// <summary>
    /// 组件名称
    /// </summary>
    public string Component { get; init; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 初始化HAL异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="exceptionType">异常类型</param>
    /// <param name="severity">严重程度</param>
    /// <param name="component">组件名称</param>
    /// <param name="innerException">内部异常</param>
    public HALException(
        string message,
        string exceptionType = "HAL.General",
        ExceptionSeverity severity = ExceptionSeverity.Error,
        string component = "HAL",
        Exception? innerException = null) 
        : base(message, innerException)
    {
        ExceptionType = exceptionType;
        Severity = severity;
        Component = component;
        ErrorCode = GenerateErrorCode(exceptionType, severity);
    }

    /// <summary>
    /// 生成错误代码
    /// </summary>
    /// <param name="exceptionType">异常类型</param>
    /// <param name="severity">严重程度</param>
    /// <returns>错误代码</returns>
    private static string GenerateErrorCode(string exceptionType, ExceptionSeverity severity)
    {
        var severityCode = severity switch
        {
            ExceptionSeverity.Critical => "CRT",
            ExceptionSeverity.Error => "ERR",
            ExceptionSeverity.Warning => "WRN",
            ExceptionSeverity.Info => "INF",
            _ => "UNK"
        };

        return $"{severityCode}_{exceptionType.Replace(".", "_")}";
    }
}

/// <summary>
/// 初始化异常
/// </summary>
public class HALInitializationException : HALException
{
    /// <summary>
    /// 初始化初始化异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="component">组件名称</param>
    /// <param name="innerException">内部异常</param>
    public HALInitializationException(
        string message,
        string component = "HAL",
        Exception? innerException = null) 
        : base(message, "HAL.Initialization", ExceptionSeverity.Error, component, innerException)
    {
    }
}

/// <summary>
/// 平台不支持异常
/// </summary>
public class HALPlatformNotSupportedException : HALException
{
    /// <summary>
    /// 不支持的平台
    /// </summary>
    public Platform UnsupportedPlatform { get; init; }

    /// <summary>
    /// 初始化平台不支持异常
    /// </summary>
    /// <param name="platform">不支持的平台</param>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public HALPlatformNotSupportedException(
        Platform platform,
        string message,
        Exception? innerException = null) 
        : base(message, "HAL.PlatformNotSupported", ExceptionSeverity.Error, "HAL", innerException)
    {
        UnsupportedPlatform = platform;
    }
}

/// <summary>
/// 权限异常
/// </summary>
public class HALPermissionException : HALException
{
    /// <summary>
    /// 所需权限
    /// </summary>
    public string RequiredPermission { get; init; } = string.Empty;

    /// <summary>
    /// 初始化权限异常
    /// </summary>
    /// <param name="requiredPermission">所需权限</param>
    /// <param name="message">异常消息</param>
    /// <param name="component">组件名称</param>
    /// <param name="innerException">内部异常</param>
    public HALPermissionException(
        string requiredPermission,
        string message,
        string component = "HAL",
        Exception? innerException = null) 
        : base(message, "HAL.Permission", ExceptionSeverity.Error, component, innerException)
    {
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// 性能异常
/// </summary>
public class HALPerformanceException : HALException
{
    /// <summary>
    /// 性能指标
    /// </summary>
    public Dictionary<string, object> PerformanceMetrics { get; init; } = new();

    /// <summary>
    /// 初始化性能异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="performanceMetrics">性能指标</param>
    /// <param name="component">组件名称</param>
    /// <param name="innerException">内部异常</param>
    public HALPerformanceException(
        string message,
        Dictionary<string, object>? performanceMetrics = null,
        string component = "HAL",
        Exception? innerException = null) 
        : base(message, "HAL.Performance", ExceptionSeverity.Warning, component, innerException)
    {
        PerformanceMetrics = performanceMetrics ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// 严重程度枚举
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    /// 关键
    /// </summary>
    Critical,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// 信息
    /// </summary>
    Info
}