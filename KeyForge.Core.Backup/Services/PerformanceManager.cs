using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Core.Interfaces;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 性能监控和资源管理器 - 简化实现
    /// </summary>
    public class PerformanceManager : IDisposable
    {
        private readonly ILoggerService _logger;
        private readonly Dictionary<string, PerformanceCounter> _counters;
        private readonly Dictionary<string, PerformanceMetrics> _metrics;
        private readonly System.Threading.Timer _monitoringTimer;
        private readonly object _lock = new object();
        private bool _disposed;
        private DateTime _startTime;

        public event EventHandler<PerformanceMetrics> MetricsUpdated;
        public event EventHandler<PerformanceAlert> PerformanceAlert;

        public PerformanceManager(ILoggerService logger)
        {
            _logger = logger;
            _counters = new Dictionary<string, PerformanceCounter>();
            _metrics = new Dictionary<string, PerformanceMetrics>();
            _startTime = DateTime.Now;
            
            InitializeCounters();
            
            // 每5秒更新一次性能数据
            _monitoringTimer = new System.Threading.Timer(UpdateMetrics, null, 5000, 5000);
            
            _logger.Info("性能管理器初始化完成");
        }

        /// <summary>
        /// 初始化性能计数器
        /// </summary>
        private void InitializeCounters()
        {
            try
            {
                // CPU使用率计数器
                _counters["CPU"] = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                
                // 内存使用率计数器
                _counters["Memory"] = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                
                // 进程内存计数器
                _counters["ProcessMemory"] = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);
                
                // 磁盘I/O计数器
                _counters["DiskRead"] = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                _counters["DiskWrite"] = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                
                _logger.Info("性能计数器初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Warning($"性能计数器初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新性能指标
        /// </summary>
        private void UpdateMetrics(object state)
        {
            try
            {
                var metrics = CollectMetrics();
                
                lock (_lock)
                {
                    _metrics["Current"] = metrics;
                }
                
                // 触发事件
                MetricsUpdated?.Invoke(this, metrics);
                
                // 检查性能警告
                CheckPerformanceAlerts(metrics);
            }
            catch (Exception ex)
            {
                _logger.Error($"更新性能指标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 收集性能指标
        /// </summary>
        private PerformanceMetrics CollectMetrics()
        {
            var metrics = new PerformanceMetrics
            {
                Timestamp = DateTime.Now,
                Uptime = DateTime.Now - _startTime
            };

            try
            {
                // CPU使用率
                if (_counters.ContainsKey("CPU"))
                {
                    metrics.CpuUsage = _counters["CPU"].NextValue();
                }

                // 内存使用率
                if (_counters.ContainsKey("Memory"))
                {
                    metrics.MemoryUsage = _counters["Memory"].NextValue();
                }

                // 进程内存
                if (_counters.ContainsKey("ProcessMemory"))
                {
                    metrics.ProcessMemoryBytes = (long)_counters["ProcessMemory"].NextValue();
                }

                // 磁盘I/O
                if (_counters.ContainsKey("DiskRead"))
                {
                    metrics.DiskReadBytesPerSec = (long)_counters["DiskRead"].NextValue();
                }

                if (_counters.ContainsKey("DiskWrite"))
                {
                    metrics.DiskWriteBytesPerSec = (long)_counters["DiskWrite"].NextValue();
                }

                // GC信息
                metrics.GcCollectionCount0 = GC.CollectionCount(0);
                metrics.GcCollectionCount1 = GC.CollectionCount(1);
                metrics.GcCollectionCount2 = GC.CollectionCount(2);
                metrics.TotalMemoryManaged = GC.GetTotalMemory(false);

                // 线程信息
                var process = Process.GetCurrentProcess();
                metrics.ThreadCount = process.Threads.Count;
                metrics.HandleCount = process.HandleCount;

            }
            catch (Exception ex)
            {
                _logger.Error($"收集性能指标失败: {ex.Message}");
            }

            return metrics;
        }

        /// <summary>
        /// 检查性能警告
        /// </summary>
        private void CheckPerformanceAlerts(PerformanceMetrics metrics)
        {
            // CPU使用率过高
            if (metrics.CpuUsage > 80)
            {
                RaiseAlert("CPU", $"CPU使用率过高: {metrics.CpuUsage:F1}%", AlertLevel.Warning);
            }

            // 内存使用率过高
            if (metrics.MemoryUsage > 85)
            {
                RaiseAlert("Memory", $"内存使用率过高: {metrics.MemoryUsage:F1}%", AlertLevel.Warning);
            }

            // 进程内存过大
            if (metrics.ProcessMemoryBytes > 500 * 1024 * 1024) // 500MB
            {
                RaiseAlert("ProcessMemory", $"进程内存使用过大: {metrics.ProcessMemoryBytes / 1024 / 1024:F1}MB", AlertLevel.Warning);
            }

            // 线程数过多
            if (metrics.ThreadCount > 100)
            {
                RaiseAlert("Threads", $"线程数过多: {metrics.ThreadCount}", AlertLevel.Info);
            }
        }

        /// <summary>
        /// 触发性能警告
        /// </summary>
        private void RaiseAlert(string source, string message, AlertLevel level)
        {
            var alert = new PerformanceAlert
            {
                Source = source,
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            };

            PerformanceAlert?.Invoke(this, alert);

            if (level == AlertLevel.Warning || level == AlertLevel.Error)
            {
                _logger.Warning($"性能警告 [{level}]: {message}");
            }
        }

        /// <summary>
        /// 获取当前性能指标
        /// </summary>
        public PerformanceMetrics GetCurrentMetrics()
        {
            lock (_lock)
            {
                return _metrics.TryGetValue("Current", out var metrics) ? metrics : new PerformanceMetrics();
            }
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        public PerformanceReport GetPerformanceReport()
        {
            var currentMetrics = GetCurrentMetrics();
            var allMetrics = _metrics.Values.ToList();

            return new PerformanceReport
            {
                CurrentMetrics = currentMetrics,
                AverageMetrics = CalculateAverageMetrics(allMetrics),
                PeakMetrics = CalculatePeakMetrics(allMetrics),
                GeneratedAt = DateTime.Now,
                Uptime = DateTime.Now - _startTime
            };
        }

        /// <summary>
        /// 计算平均性能指标
        /// </summary>
        private PerformanceMetrics CalculateAverageMetrics(List<PerformanceMetrics> metricsList)
        {
            if (metricsList.Count == 0)
            {
                return new PerformanceMetrics();
            }

            return new PerformanceMetrics
            {
                CpuUsage = metricsList.Average(m => m.CpuUsage),
                MemoryUsage = metricsList.Average(m => m.MemoryUsage),
                ProcessMemoryBytes = (long)metricsList.Average(m => m.ProcessMemoryBytes),
                DiskReadBytesPerSec = (long)metricsList.Average(m => m.DiskReadBytesPerSec),
                DiskWriteBytesPerSec = (long)metricsList.Average(m => m.DiskWriteBytesPerSec),
                ThreadCount = (int)metricsList.Average(m => m.ThreadCount),
                TotalMemoryManaged = (long)metricsList.Average(m => m.TotalMemoryManaged)
            };
        }

        /// <summary>
        /// 计算峰值性能指标
        /// </summary>
        private PerformanceMetrics CalculatePeakMetrics(List<PerformanceMetrics> metricsList)
        {
            if (metricsList.Count == 0)
            {
                return new PerformanceMetrics();
            }

            return new PerformanceMetrics
            {
                CpuUsage = metricsList.Max(m => m.CpuUsage),
                MemoryUsage = metricsList.Max(m => m.MemoryUsage),
                ProcessMemoryBytes = metricsList.Max(m => m.ProcessMemoryBytes),
                DiskReadBytesPerSec = metricsList.Max(m => m.DiskReadBytesPerSec),
                DiskWriteBytesPerSec = metricsList.Max(m => m.DiskWriteBytesPerSec),
                ThreadCount = metricsList.Max(m => m.ThreadCount),
                TotalMemoryManaged = metricsList.Max(m => m.TotalMemoryManaged)
            };
        }

        /// <summary>
        /// 优化系统资源
        /// </summary>
        public void OptimizeResources()
        {
            try
            {
                _logger.Info("开始优化系统资源");

                // 强制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // 清理未使用的对象
                CleanUpUnusedResources();

                // 检查内存使用情况
                var currentMemory = GC.GetTotalMemory(false);
                _logger.Info($"资源优化完成，当前内存使用: {currentMemory / 1024 / 1024:F1}MB");
            }
            catch (Exception ex)
            {
                _logger.Error($"资源优化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理未使用的资源
        /// </summary>
        private void CleanUpUnusedResources()
        {
            // 清理旧的性能指标
            lock (_lock)
            {
                var keysToRemove = _metrics.Keys.Where(k => k != "Current").ToList();
                foreach (var key in keysToRemove)
                {
                    if (DateTime.Now - _metrics[key].Timestamp > TimeSpan.FromHours(1))
                    {
                        _metrics.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// 开始性能监控
        /// </summary>
        public void StartMonitoring(string operationName)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.Debug($"开始性能监控: {operationName}");
        }

        /// <summary>
        /// 结束性能监控
        /// </summary>
        public void EndMonitoring(string operationName)
        {
            _logger.Debug($"结束性能监控: {operationName}");
        }

        /// <summary>
        /// 记录操作性能
        /// </summary>
        public void RecordOperationPerformance(string operationName, TimeSpan duration)
        {
            if (duration.TotalMilliseconds > 1000) // 超过1秒的操作
            {
                _logger.Warning($"操作耗时过长: {operationName} - {duration.TotalMilliseconds:F1}ms");
            }
            else if (duration.TotalMilliseconds > 100) // 超过100ms的操作
            {
                _logger.Debug($"操作耗时: {operationName} - {duration.TotalMilliseconds:F1}ms");
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
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _monitoringTimer?.Dispose();
                    
                    foreach (var counter in _counters.Values)
                    {
                        counter?.Dispose();
                    }
                    _counters.Clear();
                    
                    _metrics.Clear();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~PerformanceManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// 性能指标
    /// </summary>
    public class PerformanceMetrics
    {
        public DateTime Timestamp { get; set; }
        public TimeSpan Uptime { get; set; }
        public float CpuUsage { get; set; }
        public float MemoryUsage { get; set; }
        public long ProcessMemoryBytes { get; set; }
        public long DiskReadBytesPerSec { get; set; }
        public long DiskWriteBytesPerSec { get; set; }
        public long TotalMemoryManaged { get; set; }
        public int GcCollectionCount0 { get; set; }
        public int GcCollectionCount1 { get; set; }
        public int GcCollectionCount2 { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public override string ToString()
        {
            return $"CPU: {CpuUsage:F1}% | Memory: {MemoryUsage:F1}% | ProcessMemory: {ProcessMemoryBytes / 1024 / 1024:F1}MB | Threads: {ThreadCount}";
        }
    }

    /// <summary>
    /// 性能报告
    /// </summary>
    public class PerformanceReport
    {
        public PerformanceMetrics CurrentMetrics { get; set; }
        public PerformanceMetrics AverageMetrics { get; set; }
        public PerformanceMetrics PeakMetrics { get; set; }
        public DateTime GeneratedAt { get; set; }
        public TimeSpan Uptime { get; set; }
    }

    /// <summary>
    /// 性能警告
    /// </summary>
    public class PerformanceAlert
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public AlertLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 警告级别
    /// </summary>
    public enum AlertLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// 性能监控助手类
    /// </summary>
    public static class PerformanceHelper
    {
        /// <summary>
        /// 执行操作并监控性能
        /// </summary>
        public static T ExecuteWithMonitoring<T>(Func<T> action, string operationName, ILoggerService logger = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = action();
                stopwatch.Stop();
                
                logger?.Debug($"操作完成: {operationName} - {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error($"操作失败: {operationName} - {stopwatch.ElapsedMilliseconds}ms - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步执行操作并监控性能
        /// </summary>
        public static async Task<T> ExecuteWithMonitoringAsync<T>(Func<Task<T>> action, string operationName, ILoggerService logger = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await action();
                stopwatch.Stop();
                
                logger?.Debug($"操作完成: {operationName} - {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error($"操作失败: {operationName} - {stopwatch.ElapsedMilliseconds}ms - {ex.Message}");
                throw;
            }
        }
    }
}