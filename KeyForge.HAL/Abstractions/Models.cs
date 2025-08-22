namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 表示一个二维点
/// </summary>
public record Point(int X, int Y)
{
    /// <summary>
    /// 原点
    /// </summary>
    public static readonly Point Origin = new(0, 0);

    /// <summary>
    /// 计算与另一个点的距离
    /// </summary>
    /// <param name="other">另一个点</param>
    /// <returns>距离</returns>
    public double DistanceTo(Point other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 偏移点
    /// </summary>
    /// <param name="dx">X偏移量</param>
    /// <param name="dy">Y偏移量</param>
    /// <returns>偏移后的点</returns>
    public Point Offset(int dx, int dy) => new(X + dx, Y + dy);
}

/// <summary>
/// 表示一个二维尺寸
/// </summary>
public record Size(int Width, int Height);

/// <summary>
/// 表示一个矩形区域
/// </summary>
public record Rectangle(int X, int Y, int Width, int Height)
{
    /// <summary>
    /// 获取矩形的左边界
    /// </summary>
    public int Left => X;

    /// <summary>
    /// 获取矩形的右边界
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    /// 获取矩形的上边界
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// 获取矩形的下边界
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    /// 获取矩形的中心点
    /// </summary>
    public Point Center => new Point(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// 检查点是否在矩形内
    /// </summary>
    /// <param name="point">点坐标</param>
    /// <returns>是否在矩形内</returns>
    public bool Contains(Point point)
    {
        return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
    }

    /// <summary>
    /// 检查矩形是否相交
    /// </summary>
    /// <param name="other">另一个矩形</param>
    /// <returns>是否相交</returns>
    public bool Intersects(Rectangle other)
    {
        return !(Right < other.Left || Left > other.Right || Bottom < other.Top || Top > other.Bottom);
    }
}

/// <summary>
/// 平台信息
/// </summary>
public record PlatformInfo
{
    /// <summary>
    /// 平台类型
    /// </summary>
    public Platform Platform { get; init; }

    /// <summary>
    /// 平台版本
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// 架构信息
    /// </summary>
    public string Architecture { get; init; } = string.Empty;

    /// <summary>
    /// .NET版本
    /// </summary>
    public string DotNetVersion { get; init; } = string.Empty;

    /// <summary>
    /// 是否64位系统
    /// </summary>
    public bool Is64Bit { get; init; }

    /// <summary>
    /// 系统名称
    /// </summary>
    public string SystemName { get; init; } = string.Empty;

    /// <summary>
    /// 主机名
    /// </summary>
    public string HostName { get; init; } = string.Empty;
}

/// <summary>
/// 权限请求
/// </summary>
public record PermissionRequest
{
    /// <summary>
    /// 请求的权限类型
    /// </summary>
    public string PermissionType { get; init; } = string.Empty;

    /// <summary>
    /// 权限描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 是否必需权限
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// 请求原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// 健康检查结果
/// </summary>
public record HealthCheckResult
{
    /// <summary>
    /// 健康状态
    /// </summary>
    public HealthStatus Status { get; init; }

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime CheckTime { get; init; }

    /// <summary>
    /// 响应时间（毫秒）
    /// </summary>
    public long ResponseTime { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 详细信息
    /// </summary>
    public Dictionary<string, object> Details { get; init; } = new();

    /// <summary>
    /// 是否健康
    /// </summary>
    public bool IsHealthy => Status == HealthStatus.Healthy;
}

/// <summary>
/// 性能指标
/// </summary>
public record PerformanceMetrics
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// CPU使用率（百分比）
    /// </summary>
    public double CpuUsage { get; init; }

    /// <summary>
    /// 内存使用量（MB）
    /// </summary>
    public double MemoryUsage { get; init; }

    /// <summary>
    /// 磁盘使用率（百分比）
    /// </summary>
    public double DiskUsage { get; init; }

    /// <summary>
    /// 网络使用率（百分比）
    /// </summary>
    public double NetworkUsage { get; init; }

    /// <summary>
    /// 自定义指标
    /// </summary>
    public Dictionary<string, double> CustomMetrics { get; init; } = new();

    /// <summary>
    /// 标签
    /// </summary>
    public Dictionary<string, string> Tags { get; init; } = new();
}

/// <summary>
/// 基准测试请求
/// </summary>
public record BenchmarkRequest
{
    /// <summary>
    /// 测试名称
    /// </summary>
    public string TestName { get; init; } = string.Empty;

    /// <summary>
    /// 测试描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 迭代次数
    /// </summary>
    public int Iterations { get; init; } = 100;

    /// <summary>
    /// 预热迭代次数
    /// </summary>
    public int WarmupIterations { get; init; } = 10;

    /// <summary>
    /// 测试函数
    /// </summary>
    public Func<Task>? TestFunction { get; init; }

    /// <summary>
    /// 测试参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}

/// <summary>
/// 基准测试结果
/// </summary>
public record BenchmarkResult
{
    /// <summary>
    /// 测试名称
    /// </summary>
    public string TestName { get; init; } = string.Empty;

    /// <summary>
    /// 平均执行时间（毫秒）
    /// </summary>
    public double AverageTime { get; init; }

    /// <summary>
    /// 最小执行时间（毫秒）
    /// </summary>
    public double MinTime { get; init; }

    /// <summary>
    /// 最大执行时间（毫秒）
    /// </summary>
    public double MaxTime { get; init; }

    /// <summary>
    /// 中位数执行时间（毫秒）
    /// </summary>
    public double MedianTime { get; init; }

    /// <summary>
    /// 标准差（毫秒）
    /// </summary>
    public double StandardDeviation { get; init; }

    /// <summary>
    /// 95百分位执行时间（毫秒）
    /// </summary>
    public double P95Time { get; init; }

    /// <summary>
    /// 99百分位执行时间（毫秒）
    /// </summary>
    public double P99Time { get; init; }

    /// <summary>
    /// 迭代次数
    /// </summary>
    public int Iterations { get; init; }

    /// <summary>
    /// 测试时间
    /// </summary>
    public DateTime TestTime { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 时间范围
/// </summary>
public record DateTimeRange
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime Start { get; init; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime End { get; init; }

    /// <summary>
    /// 获取时间跨度
    /// </summary>
    public TimeSpan Duration => End - Start;
}

/// <summary>
/// 性能报告
/// </summary>
public record PerformanceReport
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
    /// 性能指标列表
    /// </summary>
    public List<PerformanceMetrics> Metrics { get; init; } = new();

    /// <summary>
    /// 基准测试结果
    /// </summary>
    public List<BenchmarkResult> BenchmarkResults { get; init; } = new();

    /// <summary>
    /// 摘要信息
    /// </summary>
    public Dictionary<string, object> Summary { get; init; } = new();

    /// <summary>
    /// 建议
    /// </summary>
    public List<string> Recommendations { get; init; } = new();
}

/// <summary>
/// HAL初始化选项
/// </summary>
public record HALInitializationOptions
{
    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; init; } = true;

    /// <summary>
    /// 是否启用质量门禁
    /// </summary>
    public bool EnableQualityGate { get; init; } = true;

    /// <summary>
    /// 是否启用诊断
    /// </summary>
    public bool EnableDiagnostics { get; init; } = true;

    /// <summary>
    /// 性能监控间隔（毫秒）
    /// </summary>
    public int PerformanceMonitoringInterval { get; init; } = 1000;

    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel LogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    /// 自定义配置
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; init; } = new();
}

/// <summary>
/// HAL配置
/// </summary>
public record HALConfiguration
{
    /// <summary>
    /// 平台特定配置
    /// </summary>
    public Dictionary<string, object> PlatformSettings { get; init; } = new();

    /// <summary>
    /// 服务配置
    /// </summary>
    public Dictionary<string, object> ServiceSettings { get; init; } = new();

    /// <summary>
    /// 性能配置
    /// </summary>
    public PerformanceConfiguration Performance { get; init; } = new();

    /// <summary>
    /// 质量门禁配置
    /// </summary>
    public QualityGateConfiguration QualityGate { get; init; } = new();

    /// <summary>
    /// 诊断配置
    /// </summary>
    public DiagnosticsConfiguration Diagnostics { get; init; } = new();
}

/// <summary>
/// 性能配置
/// </summary>
public record PerformanceConfiguration
{
    /// <summary>
    /// 是否启用基准测试
    /// </summary>
    public bool EnableBenchmarking { get; init; } = true;

    /// <summary>
    /// 是否启用实时监控
    /// </summary>
    public bool EnableRealTimeMonitoring { get; init; } = true;

    /// <summary>
    /// 监控间隔（毫秒）
    /// </summary>
    public int MonitoringInterval { get; init; } = 1000;

    /// <summary>
    /// 性能阈值
    /// </summary>
    public PerformanceThresholds Thresholds { get; init; } = new();
}

/// <summary>
/// 性能阈值
/// </summary>
public record PerformanceThresholds
{
    /// <summary>
    /// CPU使用率阈值（百分比）
    /// </summary>
    public double CpuUsageThreshold { get; init; } = 80.0;

    /// <summary>
    /// 内存使用阈值（MB）
    /// </summary>
    public double MemoryUsageThreshold { get; init; } = 512.0;

    /// <summary>
    /// 响应时间阈值（毫秒）
    /// </summary>
    public double ResponseTimeThreshold { get; init; } = 100.0;

    /// <summary>
    /// 错误率阈值（百分比）
    /// </summary>
    public double ErrorRateThreshold { get; init; } = 1.0;
}

/// <summary>
/// 质量门禁配置
/// </summary>
public record QualityGateConfiguration
{
    /// <summary>
    /// 测试覆盖率阈值（百分比）
    /// </summary>
    public double TestCoverageThreshold { get; init; } = 80.0;

    /// <summary>
    /// 代码复杂度阈值
    /// </summary>
    public int CodeComplexityThreshold { get; init; } = 10;

    /// <summary>
    /// 代码重复率阈值（百分比）
    /// </summary>
    public double CodeDuplicationThreshold { get; init; } = 5.0;

    /// <summary>
    /// 是否启用静态分析
    /// </summary>
    public bool EnableStaticAnalysis { get; init; } = true;

    /// <summary>
    /// 是否启用动态分析
    /// </summary>
    public bool EnableDynamicAnalysis { get; init; } = true;
}

/// <summary>
/// 诊断配置
/// </summary>
public record DiagnosticsConfiguration
{
    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    public bool EnableVerboseLogging { get; init; } = false;

    /// <summary>
    /// 是否启用内存诊断
    /// </summary>
    public bool EnableMemoryDiagnostics { get; init; } = true;

    /// <summary>
    /// 是否启用线程诊断
    /// </summary>
    public bool EnableThreadDiagnostics { get; init; } = true;

    /// <summary>
    /// 诊断级别
    /// </summary>
    public DiagnosticsLevel Level { get; init; } = DiagnosticsLevel.Standard;
}

/// <summary>
/// 权限状态结果
/// </summary>
public record PermissionStatusResult
{
    /// <summary>
    /// 总体权限状态
    /// </summary>
    public PermissionStatus OverallStatus { get; init; }

    /// <summary>
    /// 各权限类型的详细状态
    /// </summary>
    public Dictionary<string, PermissionStatus> PermissionStatuses { get; init; } = new();

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime CheckTime { get; init; }

    /// <summary>
    /// 是否所有权限都已授予
    /// </summary>
    public bool AllGranted => OverallStatus == PermissionStatus.Granted;
}

/// <summary>
/// 权限请求结果
/// </summary>
public record PermissionRequestResult
{
    /// <summary>
    /// 请求是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 请求的权限类型
    /// </summary>
    public string PermissionType { get; init; } = string.Empty;

    /// <summary>
    /// 最终权限状态
    /// </summary>
    public PermissionStatus Status { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestTime { get; init; }
}

/// <summary>
/// 健康检查选项
/// </summary>
public record HealthCheckOptions
{
    /// <summary>
    /// 是否检查服务健康
    /// </summary>
    public bool CheckServices { get; init; } = true;

    /// <summary>
    /// 是否检查性能
    /// </summary>
    public bool CheckPerformance { get; init; } = true;

    /// <summary>
    /// 是否检查权限
    /// </summary>
    public bool CheckPermissions { get; init; } = true;

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout { get; init; } = 5000;

    /// <summary>
    /// 自定义检查项
    /// </summary>
    public List<string> CustomChecks { get; init; } = new();
}

/// <summary>
/// 质量门禁结果
/// </summary>
public record QualityGateResult
{
    /// <summary>
    /// 质量门禁是否通过
    /// </summary>
    public bool IsPassed { get; init; }

    /// <summary>
    /// 总体分数
    /// </summary>
    public double OverallScore { get; init; }

    /// <summary>
    /// 质量问题列表
    /// </summary>
    public List<QualityIssue> Issues { get; init; } = new();

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime CheckTime { get; init; }

    /// <summary>
    /// 质量门禁类型
    /// </summary>
    public QualityGateType GateType { get; init; }
}

/// <summary>
/// 质量问题
/// </summary>
public record QualityIssue
{
    /// <summary>
    /// 问题类型
    /// </summary>
    public QualityIssueType Type { get; init; }

    /// <summary>
    /// 严重程度
    /// </summary>
    public QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// 问题消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 问题位置
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// 建议解决方案
    /// </summary>
    public string SuggestedFix { get; init; } = string.Empty;
}

/// <summary>
/// 诊断报告
/// </summary>
public record DiagnosticsReport
{
    /// <summary>
    /// 报告生成时间
    /// </summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>
    /// 系统信息
    /// </summary>
    public SystemInfo SystemInfo { get; init; } = new();

    /// <summary>
    /// 内存诊断
    /// </summary>
    public MemoryDiagnostics MemoryDiagnostics { get; init; } = new();

    /// <summary>
    /// 线程诊断
    /// </summary>
    public ThreadDiagnostics ThreadDiagnostics { get; init; } = new();

    /// <summary>
    /// 性能诊断
    /// </summary>
    public PerformanceDiagnostics PerformanceDiagnostics { get; init; } = new();

    /// <summary>
    /// 错误诊断
    /// </summary>
    public ErrorDiagnostics ErrorDiagnostics { get; init; } = new();

    /// <summary>
    /// 建议
    /// </summary>
    public List<string> Recommendations { get; init; } = new();
}

/// <summary>
/// 系统信息
/// </summary>
public record SystemInfo
{
    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string OperatingSystem { get; init; } = string.Empty;

    /// <summary>
    /// .NET版本
    /// </summary>
    public string DotNetVersion { get; init; } = string.Empty;

    /// <summary>
    /// 处理器信息
    /// </summary>
    public string Processor { get; init; } = string.Empty;

    /// <summary>
    /// 内存信息
    /// </summary>
    public string Memory { get; init; } = string.Empty;

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public string Disk { get; init; } = string.Empty;

    /// <summary>
    /// 网络信息
    /// </summary>
    public string Network { get; init; } = string.Empty;
}

/// <summary>
/// 内存诊断
/// </summary>
public record MemoryDiagnostics
{
    /// <summary>
    /// 总内存（MB）
    /// </summary>
    public double TotalMemory { get; init; }

    /// <summary>
    /// 已用内存（MB）
    /// </summary>
    public double UsedMemory { get; init; }

    /// <summary>
    /// 可用内存（MB）
    /// </summary>
    public double AvailableMemory { get; init; }

    /// <summary>
    /// 内存使用率（百分比）
    /// </summary>
    public double MemoryUsagePercentage { get; init; }

    /// <summary>
    /// GC信息
    /// </summary>
    public Dictionary<string, object> GCInfo { get; init; } = new();
}

/// <summary>
/// 线程诊断
/// </summary>
public record ThreadDiagnostics
{
    /// <summary>
    /// 线程池线程数
    /// </summary>
    public int ThreadPoolThreads { get; init; }

    /// <summary>
    /// 活动线程数
    /// </summary>
    public int ActiveThreads { get; init; }

    /// <summary>
    /// 死锁检测
    /// </summary>
    public bool HasDeadlocks { get; init; }

    /// <summary>
    /// 线程统计
    /// </summary>
    public Dictionary<string, int> ThreadStatistics { get; init; } = new();
}

/// <summary>
/// 性能诊断
/// </summary>
public record PerformanceDiagnostics
{
    /// <summary>
    /// CPU使用率（百分比）
    /// </summary>
    public double CpuUsage { get; init; }

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; init; }

    /// <summary>
    /// 吞吐量（请求/秒）
    /// </summary>
    public double Throughput { get; init; }

    /// <summary>
    /// 性能瓶颈
    /// </summary>
    public List<string> Bottlenecks { get; init; } = new();
}

/// <summary>
/// 错误诊断
/// </summary>
public record ErrorDiagnostics
{
    /// <summary>
    /// 错误计数
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// 错误率（百分比）
    /// </summary>
    public double ErrorRate { get; init; }

    /// <summary>
    /// 最近错误
    /// </summary>
    public List<ErrorInfo> RecentErrors { get; init; } = new();

    /// <summary>
    /// 错误趋势
    /// </summary>
    public Dictionary<string, int> ErrorTrends { get; init; } = new();
}

/// <summary>
/// 错误信息
/// </summary>
public record ErrorInfo
{
    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 错误类型
    /// </summary>
    public string ErrorType { get; init; } = string.Empty;

    /// <summary>
    /// 错误时间
    /// </summary>
    public DateTime ErrorTime { get; init; }

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string StackTrace { get; init; } = string.Empty;
}

/// <summary>
/// 配置结果
/// </summary>
public record ConfigurationResult
{
    /// <summary>
    /// 配置是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 配置时间
    /// </summary>
    public DateTime ConfigurationTime { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 配置变更
    /// </summary>
    public List<string> Changes { get; init; } = new();
}