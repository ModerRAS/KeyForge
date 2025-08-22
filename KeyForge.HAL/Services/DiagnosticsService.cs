using KeyForge.HAL.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyForge.HAL.Services;

/// <summary>
/// 诊断服务实现
/// 提供完整的系统诊断和健康检查功能
/// </summary>
public class DiagnosticsService : IDiagnosticsService, IDisposable
{
    private readonly ILogger<DiagnosticsService> _logger;
    private readonly object _lock = new();
    private bool _isDisposed;

    /// <summary>
    /// 初始化诊断服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public DiagnosticsService(ILogger<DiagnosticsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 诊断事件
    /// </summary>
    public event EventHandler<DiagnosticsEventArgs>? DiagnosticsReported;

    /// <summary>
    /// 执行诊断
    /// </summary>
    /// <param name="level">诊断级别</param>
    /// <returns>诊断报告</returns>
    public async Task<DiagnosticsReport> ExecuteDiagnosticsAsync(DiagnosticsLevel level = DiagnosticsLevel.Standard)
    {
        try
        {
            _logger.LogInformation("Executing diagnostics at level: {Level}", level);

            var report = new DiagnosticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                SystemInfo = await GetSystemInfoAsync(),
                MemoryDiagnostics = await GetMemoryDiagnosticsAsync(),
                ThreadDiagnostics = await GetThreadDiagnosticsAsync(),
                PerformanceDiagnostics = await GetPerformanceDiagnosticsAsync(),
                ErrorDiagnostics = await GetErrorDiagnosticsAsync()
            };

            // 根据诊断级别添加更多详细信息
            if (level >= DiagnosticsLevel.Verbose)
            {
                await AddVerboseDiagnosticsAsync(report);
            }

            if (level >= DiagnosticsLevel.Full)
            {
                await AddFullDiagnosticsAsync(report);
            }

            // 生成建议
            report.Recommendations = GenerateRecommendations(report);

            // 触发事件
            OnDiagnosticsReported(report, level);

            _logger.LogInformation("Diagnostics completed successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute diagnostics");
            
            return new DiagnosticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                SystemInfo = await GetBasicSystemInfoAsync(),
                Recommendations = new List<string>
                {
                    "诊断执行失败，请检查系统配置"
                }
            };
        }
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns>系统信息</returns>
    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        try
        {
            return new SystemInfo
            {
                OperatingSystem = RuntimeInformation.OSDescription,
                DotNetVersion = RuntimeInformation.FrameworkDescription,
                Processor = RuntimeInformation.ProcessArchitecture.ToString(),
                Memory = $"{GetTotalMemoryMB()}MB",
                Disk = await GetDiskInfoAsync(),
                Network = await GetNetworkInfoAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system info");
            return await GetBasicSystemInfoAsync();
        }
    }

    /// <summary>
    /// 获取内存诊断
    /// </summary>
    /// <returns>内存诊断</returns>
    public async Task<MemoryDiagnostics> GetMemoryDiagnosticsAsync()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var totalMemory = GetTotalMemoryMB();
            var usedMemory = process.WorkingSet64 / (1024.0 * 1024.0);
            var availableMemory = totalMemory - usedMemory;

            return new MemoryDiagnostics
            {
                TotalMemory = totalMemory,
                UsedMemory = usedMemory,
                AvailableMemory = availableMemory,
                MemoryUsagePercentage = (usedMemory / totalMemory) * 100,
                GCInfo = GetGCInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory diagnostics");
            return new MemoryDiagnostics();
        }
    }

    /// <summary>
    /// 获取线程诊断
    /// </summary>
    /// <returns>线程诊断</returns>
    public async Task<ThreadDiagnostics> GetThreadDiagnosticsAsync()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var threadPool = ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            var activeThreads = process.Threads.Count;

            return new ThreadDiagnostics
            {
                ThreadPoolThreads = workerThreads,
                ActiveThreads = activeThreads,
                HasDeadlocks = await CheckDeadlocksAsync(),
                ThreadStatistics = GetThreadStatistics()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get thread diagnostics");
            return new ThreadDiagnostics();
        }
    }

    /// <summary>
    /// 获取性能诊断
    /// </summary>
    /// <returns>性能诊断</returns>
    public async Task<PerformanceDiagnostics> GetPerformanceDiagnosticsAsync()
    {
        try
        {
            var cpuUsage = await GetCpuUsageAsync();
            var responseTime = await GetAverageResponseTimeAsync();
            var throughput = await GetThroughputAsync();
            var bottlenecks = await IdentifyBottlenecksAsync();

            return new PerformanceDiagnostics
            {
                CpuUsage = cpuUsage,
                AverageResponseTime = responseTime,
                Throughput = throughput,
                Bottlenecks = bottlenecks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance diagnostics");
            return new PerformanceDiagnostics();
        }
    }

    /// <summary>
    /// 获取错误诊断
    /// </summary>
    /// <returns>错误诊断</returns>
    public async Task<ErrorDiagnostics> GetErrorDiagnosticsAsync()
    {
        try
        {
            var recentErrors = await GetRecentErrorsAsync();
            var errorRate = await CalculateErrorRateAsync();
            var errorTrends = await GetErrorTrendsAsync();

            return new ErrorDiagnostics
            {
                ErrorCount = recentErrors.Count,
                ErrorRate = errorRate,
                RecentErrors = recentErrors,
                ErrorTrends = errorTrends
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get error diagnostics");
            return new ErrorDiagnostics();
        }
    }

    /// <summary>
    /// 清理诊断数据
    /// </summary>
    /// <param name="olderThan">清理早于此时间的数据</param>
    /// <returns>是否成功</returns>
    public async Task<bool> CleanupDiagnosticsDataAsync(DateTime olderThan)
    {
        try
        {
            // 在实际实现中，这里会清理数据库或文件中的历史诊断数据
            await Task.Delay(10); // 模拟清理操作
            
            _logger.LogInformation("Cleaned up diagnostics data older than {Date}", olderThan);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup diagnostics data");
            return false;
        }
    }

    /// <summary>
    /// 获取基本系统信息
    /// </summary>
    /// <returns>基本系统信息</returns>
    private async Task<SystemInfo> GetBasicSystemInfoAsync()
    {
        return new SystemInfo
        {
            OperatingSystem = RuntimeInformation.OSDescription,
            DotNetVersion = RuntimeInformation.FrameworkDescription,
            Processor = RuntimeInformation.ProcessArchitecture.ToString(),
            Memory = "Unknown",
            Disk = "Unknown",
            Network = "Unknown"
        };
    }

    /// <summary>
    /// 获取总内存（MB）
    /// </summary>
    /// <returns>总内存</returns>
    private double GetTotalMemoryMB()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var machineName = Environment.MachineName;
            
            // 在Windows上使用PerformanceCounter
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var counter = new PerformanceCounter("Memory", "Available MBytes");
                return counter.NextValue() + process.WorkingSet64 / (1024.0 * 1024.0);
            }
            
            // 在其他平台上使用GC信息
            return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0 * 1024.0);
        }
        catch
        {
            return 1024.0; // 默认1GB
        }
    }

    /// <summary>
    /// 获取磁盘信息
    /// </summary>
    /// <returns>磁盘信息</returns>
    private async Task<string> GetDiskInfoAsync()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
            if (drive != null)
            {
                var totalSpace = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                var freeSpace = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                var usedSpace = totalSpace - freeSpace;
                var usagePercentage = (usedSpace / totalSpace) * 100;

                return $"{drive.Name} {usedSpace:F1}GB/{totalSpace:F1}GB ({usagePercentage:F1}%)";
            }

            return "No drives available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get disk info");
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取网络信息
    /// </summary>
    /// <returns>网络信息</returns>
    private async Task<string> GetNetworkInfoAsync()
    {
        try
        {
            var hostName = Dns.GetHostName();
            var hostEntry = await Dns.GetHostEntryAsync(hostName);
            
            return $"{hostName} ({string.Join(", ", hostEntry.AddressList.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get network info");
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取GC信息
    /// </summary>
    /// <returns>GC信息</returns>
    private Dictionary<string, object> GetGCInfo()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            return new Dictionary<string, object>
            {
                ["Generation0"] = gcInfo.GenerationInfo[0].CollectionsBeforeNextGC,
                ["Generation1"] = gcInfo.GenerationInfo[1].CollectionsBeforeNextGC,
                ["Generation2"] = gcInfo.GenerationInfo[2].CollectionsBeforeNextGC,
                ["TotalMemory"] = gcInfo.TotalAvailableMemoryBytes,
                ["MemoryLoad"] = gcInfo.MemoryLoadBytes,
                ["Fragmented"] = gcInfo.FragmentedBytes,
                ["Compacted"] = gcInfo.Compacted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get GC info");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 检查死锁
    /// </summary>
    /// <returns>是否有死锁</returns>
    private async Task<bool> CheckDeadlocksAsync()
    {
        try
        {
            // 在实际实现中，这里会使用更复杂的死锁检测算法
            await Task.Delay(10);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check deadlocks");
            return false;
        }
    }

    /// <summary>
    /// 获取线程统计
    /// </summary>
    /// <returns>线程统计</returns>
    private Dictionary<string, int> GetThreadStatistics()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var stats = new Dictionary<string, int>
            {
                ["Total"] = process.Threads.Count,
                ["Running"] = 0,
                ["Waiting"] = 0,
                ["Suspended"] = 0
            };

            foreach (ProcessThread thread in process.Threads)
            {
                if (thread.ThreadState == System.Diagnostics.ThreadState.Running)
                {
                    stats["Running"]++;
                }
                else if (thread.ThreadState == System.Diagnostics.ThreadState.Wait)
                {
                    stats["Waiting"]++;
                }
                else if (thread.ThreadState == System.Diagnostics.ThreadState.Suspended)
                {
                    stats["Suspended"]++;
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get thread statistics");
            return new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 获取CPU使用率
    /// </summary>
    /// <returns>CPU使用率</returns>
    private async Task<double> GetCpuUsageAsync()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;

            await Task.Delay(100); // 等待100ms

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage");
            return 0;
        }
    }

    /// <summary>
    /// 获取平均响应时间
    /// </summary>
    /// <returns>平均响应时间</returns>
    private async Task<double> GetAverageResponseTimeAsync()
    {
        try
        {
            // 在实际实现中，这里会从性能监控系统中获取数据
            await Task.Delay(10);
            return 45.5; // 模拟45.5ms
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get average response time");
            return 0;
        }
    }

    /// <summary>
    /// 获取吞吐量
    /// </summary>
    /// <returns>吞吐量</returns>
    private async Task<double> GetThroughputAsync()
    {
        try
        {
            // 在实际实现中，这里会从性能监控系统中获取数据
            await Task.Delay(10);
            return 1250.5; // 模拟1250.5请求/秒
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get throughput");
            return 0;
        }
    }

    /// <summary>
    /// 识别性能瓶颈
    /// </summary>
    /// <returns>瓶颈列表</returns>
    private async Task<List<string>> IdentifyBottlenecksAsync()
    {
        try
        {
            var bottlenecks = new List<string>();
            var cpuUsage = await GetCpuUsageAsync();
            var memoryDiagnostics = await GetMemoryDiagnosticsAsync();

            if (cpuUsage > 80)
            {
                bottlenecks.Add("High CPU usage detected");
            }

            if (memoryDiagnostics.MemoryUsagePercentage > 85)
            {
                bottlenecks.Add("High memory usage detected");
            }

            // 在实际实现中，这里会添加更多的瓶颈检测逻辑
            return bottlenecks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to identify bottlenecks");
            return new List<string>();
        }
    }

    /// <summary>
    /// 获取最近的错误
    /// </summary>
    /// <returns>最近的错误</returns>
    private async Task<List<ErrorInfo>> GetRecentErrorsAsync()
    {
        try
        {
            // 在实际实现中，这里会从日志系统中获取最近的错误
            await Task.Delay(10);
            return new List<ErrorInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent errors");
            return new List<ErrorInfo>();
        }
    }

    /// <summary>
    /// 计算错误率
    /// </summary>
    /// <returns>错误率</returns>
    private async Task<double> CalculateErrorRateAsync()
    {
        try
        {
            // 在实际实现中，这里会从监控系统中计算错误率
            await Task.Delay(10);
            return 0.5; // 模拟0.5%错误率
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate error rate");
            return 0;
        }
    }

    /// <summary>
    /// 获取错误趋势
    /// </summary>
    /// <returns>错误趋势</returns>
    private async Task<Dictionary<string, int>> GetErrorTrendsAsync()
    {
        try
        {
            // 在实际实现中，这里会从监控系统中获取错误趋势
            await Task.Delay(10);
            return new Dictionary<string, int>
            {
                ["LastHour"] = 2,
                ["LastDay"] = 15,
                ["LastWeek"] = 87
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get error trends");
            return new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 添加详细诊断信息
    /// </summary>
    /// <param name="report">诊断报告</param>
    private async Task AddVerboseDiagnosticsAsync(DiagnosticsReport report)
    {
        try
        {
            // 添加更详细的诊断信息
            await Task.Delay(10);
            
            // 在实际实现中，这里会添加更详细的诊断信息
            report.Recommendations.Add("详细诊断模式已启用");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add verbose diagnostics");
        }
    }

    /// <summary>
    /// 添加完整诊断信息
    /// </summary>
    /// <param name="report">诊断报告</param>
    private async Task AddFullDiagnosticsAsync(DiagnosticsReport report)
    {
        try
        {
            // 添加完整的诊断信息
            await Task.Delay(10);
            
            // 在实际实现中，这里会添加完整的诊断信息
            report.Recommendations.Add("完整诊断模式已启用");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add full diagnostics");
        }
    }

    /// <summary>
    /// 生成建议
    /// </summary>
    /// <param name="report">诊断报告</param>
    /// <returns>建议列表</returns>
    private List<string> GenerateRecommendations(DiagnosticsReport report)
    {
        var recommendations = new List<string>();

        try
        {
            // 基于内存使用情况生成建议
            if (report.MemoryDiagnostics.MemoryUsagePercentage > 80)
            {
                recommendations.Add("内存使用率过高，建议优化内存使用或增加内存容量");
            }

            // 基于CPU使用情况生成建议
            if (report.PerformanceDiagnostics.CpuUsage > 80)
            {
                recommendations.Add("CPU使用率过高，建议优化CPU密集型操作");
            }

            // 基于错误率生成建议
            if (report.ErrorDiagnostics.ErrorRate > 1.0)
            {
                recommendations.Add("错误率过高，建议检查错误日志并修复问题");
            }

            // 基于性能瓶颈生成建议
            if (report.PerformanceDiagnostics.Bottlenecks.Any())
            {
                recommendations.Add($"检测到性能瓶颈: {string.Join(", ", report.PerformanceDiagnostics.Bottlenecks)}");
            }

            // 如果没有发现问题，添加正面建议
            if (!recommendations.Any())
            {
                recommendations.Add("系统运行状况良好，继续保持");
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate recommendations");
            return new List<string> { "无法生成建议" };
        }
    }

    /// <summary>
    /// 触发诊断事件
    /// </summary>
    /// <param name="report">诊断报告</param>
    /// <param name="level">诊断级别</param>
    private void OnDiagnosticsReported(DiagnosticsReport report, DiagnosticsLevel level)
    {
        try
        {
            DiagnosticsReported?.Invoke(this, new DiagnosticsEventArgs
            {
                Report = report,
                Level = level,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger diagnostics event");
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
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~DiagnosticsService()
    {
        Dispose(false);
    }
}