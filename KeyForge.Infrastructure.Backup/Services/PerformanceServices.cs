using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using KeyForge.Infrastructure.Extensions;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 性能监控器接口
    /// 
    /// 原本实现：复杂的性能指标收集和分析
    /// 简化实现：基本的性能计数器和统计
    /// </summary>
    public interface IPerformanceMonitor
    {
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        Task<Dictionary<string, double>> GetCurrentMetricsAsync();
        Task RecordMetricAsync(string name, double value);
        Task ResetMetricsAsync();
    }

    /// <summary>
    /// 性能监控器实现
    /// 
    /// 原本实现：使用专业的APM工具集成
    /// 简化实现：内存中的性能计数器
    /// 
    /// 优化建议：
    /// 1. 集成Application Insights或Prometheus
    /// 2. 添加分布式追踪
    /// 3. 实现性能告警
    /// 4. 支持自定义指标
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly Dictionary<string, MetricData> _metrics = new();
        private readonly object _lock = new();
        private bool _isMonitoring = false;
        private CancellationTokenSource _cancellationTokenSource;

        private class MetricData
        {
            public double Value { get; set; }
            public DateTime LastUpdated { get; set; }
            public int Count { get; set; }
            public double Sum { get; set; }
            public double Min { get; set; } = double.MaxValue;
            public double Max { get; set; } = double.MinValue;
        }

        public async Task StartMonitoringAsync()
        {
            lock (_lock)
            {
                if (_isMonitoring)
                    return;

                _isMonitoring = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            // 启动后台任务进行定期指标收集
            _ = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await CollectSystemMetricsAsync();
                        await Task.Delay(5000, _cancellationTokenSource.Token); // 每5秒收集一次
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 记录错误但不停止监控
                        Console.WriteLine($"性能监控错误: {ex.Message}");
                    }
                }
            });
        }

        public async Task StopMonitoringAsync()
        {
            lock (_lock)
            {
                if (!_isMonitoring)
                    return;

                _isMonitoring = false;
                _cancellationTokenSource?.Cancel();
            }

            if (_cancellationTokenSource != null)
            {
                try
                {
                    await _cancellationTokenSource.Token.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"停止性能监控时发生错误: {ex.Message}");
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        public async Task<Dictionary<string, double>> GetCurrentMetricsAsync()
        {
            lock (_lock)
            {
                return _metrics.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Value
                );
            }
        }

        public async Task RecordMetricAsync(string name, double value)
        {
            lock (_lock)
            {
                if (!_metrics.TryGetValue(name, out var metric))
                {
                    metric = new MetricData();
                    _metrics[name] = metric;
                }

                metric.Value = value;
                metric.LastUpdated = DateTime.UtcNow;
                metric.Count++;
                metric.Sum += value;
                metric.Min = Math.Min(metric.Min, value);
                metric.Max = Math.Max(metric.Max, value);
            }
        }

        public async Task ResetMetricsAsync()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }
        }

        private async Task CollectSystemMetricsAsync()
        {
            try
            {
                // 收集CPU使用率
                var cpuUsage = GetCpuUsage();
                await RecordMetricAsync("System.CPU.Usage", cpuUsage);

                // 收集内存使用情况
                var memoryUsage = GetMemoryUsage();
                await RecordMetricAsync("System.Memory.Usage", memoryUsage);

                // 收集GC信息
                var gcMemory = GC.GetTotalMemory(false);
                await RecordMetricAsync("System.GC.Memory", gcMemory);

                // 收集线程数
                var threadCount = Process.GetCurrentProcess().Threads.Count;
                await RecordMetricAsync("System.Threads.Count", threadCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"收集系统指标时发生错误: {ex.Message}");
            }
        }

        private double GetCpuUsage()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = currentProcess.TotalProcessorTime;

                Thread.Sleep(100); // 等待100ms测量CPU使用率

                var endTime = DateTime.UtcNow;
                var endCpuUsage = currentProcess.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;

                var cpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                return cpuUsage * 100; // 转换为百分比
            }
            catch
            {
                return 0;
            }
        }

        private double GetMemoryUsage()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var memoryUsed = currentProcess.WorkingSet64;
                var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                
                return (double)memoryUsed / totalMemory * 100; // 转换为百分比
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// 指标收集器接口
    /// 
    /// 原本实现：支持多种数据源和格式的指标收集
    /// 简化实现：基本的内存指标收集
    /// </summary>
    public interface IMetricsCollector
    {
        Task CollectAsync(string metricName, double value);
        Task<Dictionary<string, double>> GetMetricsAsync();
        Task ClearMetricsAsync();
    }

    /// <summary>
    /// 指标收集器实现
    /// 
    /// 原本实现：支持时间序列数据库和分布式收集
    /// 简化实现：内存中的指标存储
    /// </summary>
    public class MetricsCollector : IMetricsCollector
    {
        private readonly Dictionary<string, List<MetricPoint>> _metrics = new();
        private readonly object _lock = new();
        private const int MaxMetricsPerKey = 1000;

        private class MetricPoint
        {
            public double Value { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public async Task CollectAsync(string metricName, double value)
        {
            lock (_lock)
            {
                if (!_metrics.TryGetValue(metricName, out var metricPoints))
                {
                    metricPoints = new List<MetricPoint>();
                    _metrics[metricName] = metricPoints;
                }

                metricPoints.Add(new MetricPoint
                {
                    Value = value,
                    Timestamp = DateTime.UtcNow
                });

                // 限制保留的指标点数量
                if (metricPoints.Count > MaxMetricsPerKey)
                {
                    metricPoints.RemoveAt(0);
                }
            }
        }

        public async Task<Dictionary<string, double>> GetMetricsAsync()
        {
            lock (_lock)
            {
                return _metrics.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Count > 0 ? kvp.Value.Last().Value : 0
                );
            }
        }

        public async Task ClearMetricsAsync()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }
        }
    }

    /// <summary>
    /// 性能监控服务
    /// 
    /// 原本实现：后台服务，定期收集和报告性能指标
    /// 简化实现：基本的托管服务
    /// </summary>
    public class PerformanceMonitoringService : BackgroundService
    {
        private readonly IPerformanceMonitor _performanceMonitor;
        private readonly IMetricsCollector _metricsCollector;
        private readonly ILogger<PerformanceMonitoringService> _logger;

        public PerformanceMonitoringService(
            IPerformanceMonitor performanceMonitor,
            IMetricsCollector metricsCollector,
            ILogger<PerformanceMonitoringService> logger)
        {
            _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("性能监控服务启动");

            try
            {
                await _performanceMonitor.StartMonitoringAsync();

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // 收集当前指标
                        var metrics = await _performanceMonitor.GetCurrentMetricsAsync();
                        
                        foreach (var metric in metrics)
                        {
                            await _metricsCollector.CollectAsync(metric.Key, metric.Value);
                        }

                        // 记录一些关键指标到日志
                        if (metrics.TryGetValue("System.CPU.Usage", out var cpuUsage))
                        {
                            _logger.LogDebug("CPU使用率: {CpuUsage}%", cpuUsage);
                        }

                        if (metrics.TryGetValue("System.Memory.Usage", out var memoryUsage))
                        {
                            _logger.LogDebug("内存使用率: {MemoryUsage}%", memoryUsage);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "性能监控服务执行过程中发生错误");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            finally
            {
                await _performanceMonitor.StopMonitoringAsync();
                _logger.LogInformation("性能监控服务停止");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止性能监控服务...");
            await base.StopAsync(cancellationToken);
        }
    }
}