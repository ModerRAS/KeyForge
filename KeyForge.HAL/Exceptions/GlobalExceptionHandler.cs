using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Serilog;
using Serilog.Events;

namespace KeyForge.HAL.Exceptions;

/// <summary>
/// 全局异常处理器
/// 这是简化实现，专注于核心功能
/// </summary>
public class GlobalExceptionHandler : IDisposable
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly object _lock = new();
    private bool _isDisposed;

    // Serilog配置
    private ILogger? _serilogLogger;
    private readonly List<ExceptionEventHandler> _exceptionHandlers = new();

    /// <summary>
    /// 异常事件处理委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">异常事件参数</param>
    public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs e);

    /// <summary>
    /// 全局异常事件
    /// </summary>
    public event ExceptionEventHandler? OnException;

    /// <summary>
    /// 初始化全局异常处理器
    /// </summary>
    /// <param name="logger">日志服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        InitializeSerilog();
        SubscribeToGlobalExceptions();
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="exception">异常</param>
    /// <param name="context">上下文信息</param>
    /// <returns>是否处理成功</returns>
    public async Task<bool> HandleExceptionAsync(Exception exception, Dictionary<string, object>? context = null)
    {
        try
        {
            lock (_lock)
            {
                var halException = ConvertToHALException(exception, context);
                LogException(halException);
                NotifyExceptionHandlers(halException);
                
                _logger.LogError(halException, "Exception handled: {ExceptionType}", halException.ExceptionType);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle exception: {OriginalException}", exception.Message);
            return false;
        }
    }

    /// <summary>
    /// 处理HAL异常
    /// </summary>
    /// <param name="halException">HAL异常</param>
    /// <returns>是否处理成功</returns>
    public async Task<bool> HandleHALExceptionAsync(HALException halException)
    {
        try
        {
            lock (_lock)
            {
                LogException(halException);
                NotifyExceptionHandlers(halException);
                
                _logger.LogError(halException, "HAL exception handled: {ExceptionType}", halException.ExceptionType);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle HAL exception: {ExceptionType}", halException.ExceptionType);
            return false;
        }
    }

    /// <summary>
    /// 添加异常处理器
    /// </summary>
    /// <param name="handler">异常处理器</param>
    public void AddExceptionHandler(ExceptionEventHandler handler)
    {
        lock (_lock)
        {
            _exceptionHandlers.Add(handler);
            _logger.LogDebug("Exception handler added: {Handler}", handler.Target?.GetType().Name ?? "Unknown");
        }
    }

    /// <summary>
    /// 移除异常处理器
    /// </summary>
    /// <param name="handler">异常处理器</param>
    public void RemoveExceptionHandler(ExceptionEventHandler handler)
    {
        lock (_lock)
        {
            _exceptionHandlers.Remove(handler);
            _logger.LogDebug("Exception handler removed: {Handler}", handler.Target?.GetType().Name ?? "Unknown");
        }
    }

    /// <summary>
    /// 获取异常统计信息
    /// </summary>
    /// <returns>异常统计信息</returns>
    public Dictionary<string, object> GetExceptionStatistics()
    {
        // 简化实现 - 返回默认统计信息
        return new Dictionary<string, object>
        {
            ["TotalHandledExceptions"] = 0,
            ["ExceptionsByType"] = new Dictionary<string, int>(),
            ["ExceptionsByComponent"] = new Dictionary<string, int>(),
            ["ExceptionsBySeverity"] = new Dictionary<string, int>()
        };
    }

    /// <summary>
    /// 生成异常报告
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>异常报告</returns>
    public async Task<ExceptionReport> GenerateExceptionReportAsync(DateTimeRange timeRange)
    {
        try
        {
            _logger.LogInformation("Generating exception report for range: {Start} to {End}", timeRange.Start, timeRange.End);

            var report = new ExceptionReport
            {
                GeneratedAt = DateTime.UtcNow,
                TimeRange = timeRange,
                Statistics = GetExceptionStatistics(),
                Summary = new Dictionary<string, object>
                {
                    ["ReportPeriod"] = $"{timeRange.Start:yyyy-MM-dd HH:mm:ss} to {timeRange.End:yyyy-MM-dd HH:mm:ss}",
                    ["GeneratedBy"] = "GlobalExceptionHandler"
                },
                Recommendations = GenerateExceptionRecommendations()
            };

            _logger.LogInformation("Exception report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate exception report");
            return new ExceptionReport
            {
                GeneratedAt = DateTime.UtcNow,
                TimeRange = timeRange,
                Summary = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 释放托管资源
                _serilogLogger?.Dispose();
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~GlobalExceptionHandler()
    {
        Dispose(false);
    }

    /// <summary>
    /// 初始化Serilog
    /// </summary>
    private void InitializeSerilog()
    {
        try
        {
            _serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/hal-exceptions-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _logger.LogDebug("Serilog initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Serilog");
        }
    }

    /// <summary>
    /// 订阅全局异常
    /// </summary>
    private void SubscribeToGlobalExceptions()
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            _logger.LogDebug("Global exception handlers subscribed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to global exceptions");
        }
    }

    /// <summary>
    /// 处理未处理异常
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">异常事件参数</param>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                HandleExceptionAsync(exception, new Dictionary<string, object>
                {
                    ["IsTerminating"] = e.IsTerminating,
                    ["Sender"] = sender?.GetType().Name ?? "Unknown"
                }).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle unhandled exception");
        }
    }

    /// <summary>
    /// 处理未观察到的任务异常
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">异常事件参数</param>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            var exception = e.Exception;
            HandleExceptionAsync(exception, new Dictionary<string, object>
            {
                ["IsObserved"] = e.Observed,
                ["Sender"] = sender?.GetType().Name ?? "Unknown"
            }).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle unobserved task exception");
        }
    }

    /// <summary>
    /// 转换为HAL异常
    /// </summary>
    /// <param name="exception">原始异常</param>
    /// <param name="context">上下文信息</param>
    /// <returns>HAL异常</returns>
    private HALException ConvertToHALException(Exception exception, Dictionary<string, object>? context = null)
    {
        if (exception is HALException halException)
        {
            return halException;
        }

        var exceptionType = exception.GetType().Name;
        var component = context?["Component"]?.ToString() ?? "Unknown";
        var severity = DetermineSeverity(exception);

        return new HALException(
            exception.Message,
            $"HAL.{exceptionType}",
            severity,
            component,
            exception);
    }

    /// <summary>
    /// 确定异常严重程度
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>严重程度</returns>
    private static ExceptionSeverity DetermineSeverity(Exception exception)
    {
        return exception switch
        {
            OutOfMemoryException => ExceptionSeverity.Critical,
            StackOverflowException => ExceptionSeverity.Critical,
            AccessViolationException => ExceptionSeverity.Critical,
            InvalidOperationException => ExceptionSeverity.Error,
            ArgumentException => ExceptionSeverity.Warning,
            ArgumentNullException => ExceptionSeverity.Warning,
            ArgumentOutOfRangeException => ExceptionSeverity.Warning,
            _ => ExceptionSeverity.Error
        };
    }

    /// <summary>
    /// 记录异常
    /// </summary>
    /// <param name="halException">HAL异常</param>
    private void LogException(HALException halException)
    {
        try
        {
            // 使用Serilog记录结构化日志
            _serilogLogger?.Write(ConvertToSerilogLevel(halException.Severity), halException, 
                "HAL Exception: {ExceptionType} in {Component}", 
                halException.ExceptionType, halException.Component);
            
            // 使用标准日志记录
            _logger.LogError(halException, "HAL Exception: {ExceptionType} in {Component}", 
                halException.ExceptionType, halException.Component);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log exception: {ExceptionType}", halException.ExceptionType);
        }
    }

    /// <summary>
    /// 转换为Serilog级别
    /// </summary>
    /// <param name="severity">严重程度</param>
    /// <returns>Serilog级别</returns>
    private static LogEventLevel ConvertToSerilogLevel(ExceptionSeverity severity)
    {
        return severity switch
        {
            ExceptionSeverity.Critical => LogEventLevel.Fatal,
            ExceptionSeverity.Error => LogEventLevel.Error,
            ExceptionSeverity.Warning => LogEventLevel.Warning,
            ExceptionSeverity.Info => LogEventLevel.Information,
            _ => LogEventLevel.Error
        };
    }

    /// <summary>
    /// 通知异常处理器
    /// </summary>
    /// <param name="halException">HAL异常</param>
    private void NotifyExceptionHandlers(HALException halException)
    {
        try
        {
            var args = new ExceptionEventArgs
            {
                Exception = halException,
                Timestamp = DateTime.UtcNow,
                Handled = true
            };

            // 触发全局异常事件
            OnException?.Invoke(this, args);

            // 通知所有注册的处理器
            foreach (var handler in _exceptionHandlers)
            {
                try
                {
                    handler(this, args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception handler failed");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify exception handlers");
        }
    }

    /// <summary>
    /// 生成异常建议
    /// </summary>
    /// <returns>建议列表</returns>
    private static List<string> GenerateExceptionRecommendations()
    {
        return new List<string>
        {
            "Monitor exception frequency and patterns",
            "Implement proper error handling in all components",
            "Consider adding retry logic for transient errors",
            "Ensure proper logging and monitoring",
            "Review exception handling best practices"
        };
    }
}

/// <summary>
/// 异常事件参数
/// </summary>
public class ExceptionEventArgs : EventArgs
{
    /// <summary>
    /// 异常
    /// </summary>
    public HALException Exception { get; init; } = new();

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已处理
    /// </summary>
    public bool Handled { get; init; }
}

/// <summary>
/// 异常报告
/// </summary>
public class ExceptionReport
{
    /// <summary>
    /// 报告生成时间
    /// </summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>
    /// 时间范围
    /// </summary>
    public DateTimeRange TimeRange { get; init; } = new();

    /// <summary>
    /// 统计信息
    /// </summary>
    public Dictionary<string, object> Statistics { get; init; } = new();

    /// <summary>
    /// 摘要信息
    /// </summary>
    public Dictionary<string, object> Summary { get; init; } = new();

    /// <summary>
    /// 建议
    /// </summary>
    public List<string> Recommendations { get; init; } = new();
}