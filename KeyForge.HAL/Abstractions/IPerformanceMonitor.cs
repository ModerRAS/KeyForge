namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 性能监控接口 - 提供跨平台性能监控功能
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// 收集性能指标
    /// </summary>
    /// <returns>收集任务</returns>
    Task CollectMetricsAsync();

    /// <summary>
    /// 记录自定义指标
    /// </summary>
    /// <param name="name">指标名称</param>
    /// <param name="value">指标值</param>
    /// <param name="tags">标签</param>
    void RecordMetric(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// 运行性能基准测试
    /// </summary>
    /// <param name="request">基准测试请求</param>
    /// <returns>基准测试结果</returns>
    Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest request);

    /// <summary>
    /// 获取性能指标流
    /// </summary>
    /// <returns>性能指标流</returns>
    IObservable<PerformanceMetrics> GetMetricsStream();

    /// <summary>
    /// 生成性能报告
    /// </summary>
    /// <param name="range">时间范围</param>
    /// <returns>性能报告</returns>
    Task<PerformanceReport> GenerateReportAsync(DateTimeRange range);

    /// <summary>
    /// 启动性能监控
    /// </summary>
    /// <param name="interval">监控间隔（毫秒）</param>
    /// <returns>是否成功</returns>
    Task<bool> StartMonitoringAsync(int interval = 1000);

    /// <summary>
    /// 停止性能监控
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> StopMonitoringAsync();

    /// <summary>
    /// 获取当前性能指标
    /// </summary>
    /// <returns>性能指标</returns>
    PerformanceMetrics GetCurrentMetrics();

    /// <summary>
    /// 获取历史性能指标
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>历史指标</returns>
    IEnumerable<PerformanceMetrics> GetHistoricalMetrics(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 清理历史数据
    /// </summary>
    /// <param name="olderThan">清理早于此时间的数据</param>
    /// <returns>清理的记录数</returns>
    int CleanupHistoricalData(DateTime olderThan);

    /// <summary>
    /// 性能告警事件
    /// </summary>
    event EventHandler<PerformanceAlertEventArgs>? AlertTriggered;
}