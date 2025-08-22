using KeyForge.HAL.Abstractions;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace KeyForge.HAL.Services.MacOS;

/// <summary>
/// macOS性能监控服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class MacOSPerformanceMonitor : IPerformanceMonitor
{
    private readonly ILogger<MacOSPerformanceMonitor> _logger;
    private readonly object _lock = new();
    private readonly Subject<PerformanceMetrics> _metricsSubject = new();
    private readonly List<PerformanceMetrics> _historicalMetrics = new();
    private readonly object _metricsLock = new();
    
    private bool _isDisposed;
    private bool _isMonitoring;
    private CancellationTokenSource? _monitoringCancellationTokenSource;
    private Task? _monitoringTask;

    /// <summary>
    /// 初始化macOS性能监控服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public MacOSPerformanceMonitor(ILogger<MacOSPerformanceMonitor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 性能告警事件
    /// </summary>
    public event EventHandler<PerformanceAlertEventArgs>? AlertTriggered;

    /// <summary>
    /// 收集性能指标
    /// </summary>
    /// <returns>收集任务</returns>
    public async Task CollectMetricsAsync()
    {
        try
        {
            var metrics = new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                DiskUsage = GetDiskUsage(),
                NetworkUsage = GetNetworkUsage(),
                CustomMetrics = new Dictionary<string, double>(),
                Tags = new Dictionary<string, string>
                {
                    ["Platform"] = "macOS",
                    ["Host"] = Environment.MachineName
                }
            };

            // 添加自定义指标
            metrics.CustomMetrics["ProcessCount"] = GetProcessCount();
            metrics.CustomMetrics["ThreadCount"] = GetThreadCount();
            metrics.CustomMetrics["HandleCount"] = GetHandleCount();

            // 存储历史数据
            lock (_metricsLock)
            {
                _historicalMetrics.Add(metrics);
                
                // 限制历史数据大小
                if (_historicalMetrics.Count > 1000)
                {
                    _historicalMetrics.RemoveAt(0);
                }
            }

            // 发布到流
            _metricsSubject.OnNext(metrics);

            // 检查告警条件
            await CheckAlertConditionsAsync(metrics);

            _logger.LogDebug("macOS performance metrics collected: CPU={Cpu}%, Memory={Memory}MB, Disk={Disk}%, Network={Network}%",
                metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage, metrics.NetworkUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect performance metrics");
        }
    }

    /// <summary>
    /// 记录自定义指标
    /// </summary>
    /// <param name="name">指标名称</param>
    /// <param name="value">指标值</param>
    /// <param name="tags">标签</param>
    public void RecordMetric(string name, double value, Dictionary<string, string>? tags = null)
    {
        try
        {
            var metrics = GetCurrentMetrics();
            metrics.CustomMetrics[name] = value;
            
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    metrics.Tags[tag.Key] = tag.Value;
                }
            }

            _logger.LogDebug("macOS custom metric recorded: {Name}={Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record metric: {Name}", name);
        }
    }

    /// <summary>
    /// 运行性能基准测试
    /// </summary>
    /// <param name="request">基准测试请求</param>
    /// <returns>基准测试结果</returns>
    public async Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest request)
    {
        try
        {
            _logger.LogInformation("macOS running benchmark: {TestName}", request.TestName);

            if (request.TestFunction == null)
            {
                return new BenchmarkResult
                {
                    TestName = request.TestName,
                    IsSuccess = false,
                    ErrorMessage = "Test function is null"
                };
            }

            var times = new List<double>();
            var stopwatch = new Stopwatch();

            // 预热
            for (int i = 0; i < request.WarmupIterations; i++)
            {
                await request.TestFunction();
            }

            // 正式测试
            for (int i = 0; i < request.Iterations; i++)
            {
                stopwatch.Restart();
                await request.TestFunction();
                stopwatch.Stop();
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            var result = new BenchmarkResult
            {
                TestName = request.TestName,
                AverageTime = times.Average(),
                MinTime = times.Min(),
                MaxTime = times.Max(),
                MedianTime = CalculateMedian(times),
                StandardDeviation = CalculateStandardDeviation(times),
                P95Time = CalculatePercentile(times, 95),
                P99Time = CalculatePercentile(times, 99),
                Iterations = request.Iterations,
                TestTime = DateTime.UtcNow,
                IsSuccess = true
            };

            _logger.LogInformation("macOS benchmark completed: {TestName}, Average: {Average}ms, Min: {Min}ms, Max: {Max}ms",
                result.TestName, result.AverageTime, result.MinTime, result.MaxTime);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run benchmark: {TestName}", request.TestName);
            return new BenchmarkResult
            {
                TestName = request.TestName,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 获取性能指标流
    /// </summary>
    /// <returns>性能指标流</returns>
    public IObservable<PerformanceMetrics> GetMetricsStream()
    {
        return _metricsSubject.AsObservable();
    }

    /// <summary>
    /// 生成性能报告
    /// </summary>
    /// <param name="range">时间范围</param>
    /// <returns>性能报告</returns>
    public async Task<PerformanceReport> GenerateReportAsync(DateTimeRange range)
    {
        try
        {
            _logger.LogInformation("macOS generating performance report for range: {Start} to {End}", range.Start, range.End);

            var metrics = GetHistoricalMetrics(range.Start, range.End).ToList();
            
            if (!metrics.Any())
            {
                return new PerformanceReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    TimeRange = range,
                    Summary = new Dictionary<string, object>
                    {
                        ["Message"] = "No metrics data available for the specified time range"
                    }
                };
            }

            var report = new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                TimeRange = range,
                Metrics = metrics,
                BenchmarkResults = new List<BenchmarkResult>(),
                Summary = new Dictionary<string, object>
                {
                    ["TotalMetrics"] = metrics.Count,
                    ["AverageCpuUsage"] = metrics.Average(m => m.CpuUsage),
                    ["AverageMemoryUsage"] = metrics.Average(m => m.MemoryUsage),
                    ["AverageDiskUsage"] = metrics.Average(m => m.DiskUsage),
                    ["AverageNetworkUsage"] = metrics.Average(m => m.NetworkUsage),
                    ["MaxCpuUsage"] = metrics.Max(m => m.CpuUsage),
                    ["MaxMemoryUsage"] = metrics.Max(m => m.MemoryUsage),
                    ["MinCpuUsage"] = metrics.Min(m => m.CpuUsage),
                    ["MinMemoryUsage"] = metrics.Min(m => m.MemoryUsage)
                },
                Recommendations = GenerateRecommendations(metrics)
            };

            _logger.LogInformation("macOS performance report generated with {Count} metrics", metrics.Count);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate performance report");
            return new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                TimeRange = range,
                Summary = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// 启动性能监控
    /// </summary>
    /// <param name="interval">监控间隔（毫秒）</param>
    /// <returns>是否成功</returns>
    public async Task<bool> StartMonitoringAsync(int interval = 1000)
    {
        try
        {
            if (_isMonitoring)
            {
                _logger.LogWarning("macOS performance monitoring is already running");
                return false;
            }

            _monitoringCancellationTokenSource = new CancellationTokenSource();
            _isMonitoring = true;

            _monitoringTask = Task.Run(async () =>
            {
                while (!_monitoringCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await CollectMetricsAsync();
                    await Task.Delay(interval, _monitoringCancellationTokenSource.Token);
                }
            }, _monitoringCancellationTokenSource.Token);

            _logger.LogInformation("macOS performance monitoring started with interval: {Interval}ms", interval);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start performance monitoring");
            return false;
        }
    }

    /// <summary>
    /// 停止性能监控
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> StopMonitoringAsync()
    {
        try
        {
            if (!_isMonitoring)
            {
                _logger.LogWarning("macOS performance monitoring is not running");
                return false;
            }

            if (_monitoringCancellationTokenSource != null)
            {
                _monitoringCancellationTokenSource.Cancel();
                _monitoringCancellationTokenSource.Dispose();
                _monitoringCancellationTokenSource = null;
            }

            if (_monitoringTask != null)
            {
                await _monitoringTask;
                _monitoringTask = null;
            }

            _isMonitoring = false;
            _logger.LogInformation("macOS performance monitoring stopped");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop performance monitoring");
            return false;
        }
    }

    /// <summary>
    /// 获取当前性能指标
    /// </summary>
    /// <returns>性能指标</returns>
    public PerformanceMetrics GetCurrentMetrics()
    {
        lock (_metricsLock)
        {
            return _historicalMetrics.LastOrDefault() ?? new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                CpuUsage = 0,
                MemoryUsage = 0,
                DiskUsage = 0,
                NetworkUsage = 0,
                CustomMetrics = new Dictionary<string, double>(),
                Tags = new Dictionary<string, string>()
            };
        }
    }

    /// <summary>
    /// 获取历史性能指标
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>历史指标</returns>
    public IEnumerable<PerformanceMetrics> GetHistoricalMetrics(DateTime startTime, DateTime endTime)
    {
        lock (_metricsLock)
        {
            return _historicalMetrics
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();
        }
    }

    /// <summary>
    /// 清理历史数据
    /// </summary>
    /// <param name="olderThan">清理早于此时间的数据</param>
    /// <returns>清理的记录数</returns>
    public int CleanupHistoricalData(DateTime olderThan)
    {
        try
        {
            lock (_metricsLock)
            {
                var count = _historicalMetrics.Count;
                _historicalMetrics.RemoveAll(m => m.Timestamp < olderThan);
                var cleanedCount = count - _historicalMetrics.Count;
                
                _logger.LogDebug("macOS cleaned {Count} historical metrics older than {Date}", cleanedCount, olderThan);
                return cleanedCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup historical data");
            return 0;
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
                // 停止监控
                StopMonitoringAsync().GetAwaiter().GetResult();
                
                // 释放托管资源
                _metricsSubject.Dispose();
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~MacOSPerformanceMonitor()
    {
        Dispose(false);
    }

    /// <summary>
    /// 获取CPU使用率
    /// </summary>
    /// <returns>CPU使用率</returns>
    private double GetCpuUsage()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// 获取内存使用量
    /// </summary>
    /// <returns>内存使用量（MB）</returns>
    private double GetMemoryUsage()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// 获取磁盘使用率
    /// </summary>
    /// <returns>磁盘使用率</returns>
    private double GetDiskUsage()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// 获取网络使用率
    /// </summary>
    /// <returns>网络使用率</returns>
    private double GetNetworkUsage()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// 获取进程数量
    /// </summary>
    /// <returns>进程数量</returns>
    private int GetProcessCount()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 获取线程数量
    /// </summary>
    /// <returns>线程数量</returns>
    private int GetThreadCount()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 获取句柄数量
    /// </summary>
    /// <returns>句柄数量</returns>
    private int GetHandleCount()
    {
        try
        {
            // 简化实现 - 返回默认值
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 检查告警条件
    /// </summary>
    /// <param name="metrics">性能指标</param>
    private async Task CheckAlertConditionsAsync(PerformanceMetrics metrics)
    {
        try
        {
            // CPU告警
            if (metrics.CpuUsage > 80)
            {
                await TriggerAlertAsync(AlertLevel.Warning, $"High CPU usage: {metrics.CpuUsage:F1}%", 
                    new Dictionary<string, object> { ["Metric"] = "CPU", ["Value"] = metrics.CpuUsage });
            }

            // 内存告警
            if (metrics.MemoryUsage > 500) // 500MB
            {
                await TriggerAlertAsync(AlertLevel.Warning, $"High memory usage: {metrics.MemoryUsage:F1}MB", 
                    new Dictionary<string, object> { ["Metric"] = "Memory", ["Value"] = metrics.MemoryUsage });
            }

            // 磁盘告警
            if (metrics.DiskUsage > 90)
            {
                await TriggerAlertAsync(AlertLevel.Error, $"High disk usage: {metrics.DiskUsage:F1}%", 
                    new Dictionary<string, object> { ["Metric"] = "Disk", ["Value"] = metrics.DiskUsage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check alert conditions");
        }
    }

    /// <summary>
    /// 触发告警
    /// </summary>
    /// <param name="level">告警级别</param>
    /// <param name="message">告警消息</param>
    /// <param name="details">告警详情</param>
    private async Task TriggerAlertAsync(AlertLevel level, string message, Dictionary<string, object> details)
    {
        try
        {
            var args = new PerformanceAlertEventArgs
            {
                Level = level,
                Message = message,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            AlertTriggered?.Invoke(this, args);
            _logger.LogWarning("macOS performance alert triggered: {Level} - {Message}", level, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger alert");
        }
    }

    /// <summary>
    /// 计算中位数
    /// </summary>
    /// <param name="values">值列表</param>
    /// <returns>中位数</returns>
    private static double CalculateMedian(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var count = sorted.Count;
        
        if (count == 0)
            return 0;
        
        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        else
            return sorted[count / 2];
    }

    /// <summary>
    /// 计算标准差
    /// </summary>
    /// <param name="values">值列表</param>
    /// <returns>标准差</returns>
    private static double CalculateStandardDeviation(List<double> values)
    {
        if (values.Count == 0)
            return 0;
        
        var average = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    /// <summary>
    /// 计算百分位数
    /// </summary>
    /// <param name="values">值列表</param>
    /// <param name="percentile">百分位</param>
    /// <returns>百分位数值</returns>
    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (values.Count == 0)
            return 0;
        
        var sorted = values.OrderBy(v => v).ToList();
        var index = (percentile / 100.0) * (sorted.Count - 1);
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = (int)Math.Ceiling(index);
        
        if (lowerIndex == upperIndex)
            return sorted[lowerIndex];
        
        var weight = index - lowerIndex;
        return sorted[lowerIndex] * (1 - weight) + sorted[upperIndex] * weight;
    }

    /// <summary>
    /// 生成建议
    /// </summary>
    /// <param name="metrics">性能指标列表</param>
    /// <returns>建议列表</returns>
    private static List<string> GenerateRecommendations(List<PerformanceMetrics> metrics)
    {
        var recommendations = new List<string>();
        
        if (!metrics.Any())
            return recommendations;

        var avgCpu = metrics.Average(m => m.CpuUsage);
        var avgMemory = metrics.Average(m => m.MemoryUsage);
        var avgDisk = metrics.Average(m => m.DiskUsage);

        if (avgCpu > 70)
            recommendations.Add("High CPU usage detected. Consider optimizing CPU-intensive operations.");
        
        if (avgMemory > 300)
            recommendations.Add("High memory usage detected. Consider optimizing memory usage and implementing proper cleanup.");
        
        if (avgDisk > 80)
            recommendations.Add("High disk usage detected. Consider optimizing disk I/O operations.");

        if (recommendations.Count == 0)
            recommendations.Add("System performance is within normal parameters.");

        return recommendations;
    }
}