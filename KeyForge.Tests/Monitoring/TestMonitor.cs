using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace KeyForge.Tests.Monitoring
{
    /// <summary>
    /// 测试监控器
    /// 简化实现：基本的测试监控功能
    /// 原本实现：包含完整的监控、告警和性能分析
    /// </summary>
    public class TestMonitor
    {
        private readonly ILogger<TestMonitor> _logger;
        private readonly IConfiguration _configuration;
        private readonly TestMetricsCollector _metricsCollector;
        
        public TestMonitor(
            ILogger<TestMonitor> logger,
            IConfiguration configuration,
            TestMetricsCollector metricsCollector)
        {
            _logger = logger;
            _configuration = configuration;
            _metricsCollector = metricsCollector;
        }
        
        /// <summary>
        /// 监控测试执行
        /// </summary>
        /// <param name="testSessionId">测试会话ID</param>
        /// <param name="testExecution">测试执行函数</param>
        /// <returns>监控结果</returns>
        public async Task<MonitoringResult> MonitorTestExecutionAsync(string testSessionId, Func<Task> testExecution)
        {
            _logger.LogInformation("开始监控测试执行: {TestSessionId}", testSessionId);
            
            var monitoringResult = new MonitoringResult
            {
                TestSessionId = testSessionId,
                StartTime = DateTime.UtcNow,
                Status = "Running"
            };
            
            try
            {
                // 开始监控
                var monitoringTask = MonitorTestMetricsAsync(testSessionId, monitoringResult);
                
                // 执行测试
                await testExecution();
                
                // 停止监控
                monitoringResult.Status = "Completed";
                monitoringResult.EndTime = DateTime.UtcNow;
                
                // 等待监控任务完成
                await monitoringTask;
                
                _logger.LogInformation("测试执行监控完成: {TestSessionId}", testSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试执行监控失败: {TestSessionId}", testSessionId);
                monitoringResult.Status = "Failed";
                monitoringResult.ErrorMessage = ex.Message;
                monitoringResult.EndTime = DateTime.UtcNow;
            }
            
            return monitoringResult;
        }
        
        /// <summary>
        /// 监控测试指标
        /// </summary>
        private async Task MonitorTestMetricsAsync(string testSessionId, MonitoringResult monitoringResult)
        {
            _logger.LogInformation("开始监控测试指标: {TestSessionId}", testSessionId);
            
            try
            {
                var metrics = new List<TestMetrics>();
                
                // 持续监控直到测试完成
                while (monitoringResult.Status == "Running")
                {
                    // 收集指标
                    var currentMetrics = await _metricsCollector.CollectMetricsAsync(testSessionId);
                    metrics.Add(currentMetrics);
                    
                    // 更新监控结果
                    UpdateMonitoringResult(monitoringResult, metrics);
                    
                    // 检查告警条件
                    CheckAlertConditions(monitoringResult, currentMetrics);
                    
                    // 等待下一次监控
                    await Task.Delay(1000); // 每秒监控一次
                }
                
                monitoringResult.Metrics = metrics;
                monitoringResult.Summary = GenerateMetricsSummary(metrics);
                
                _logger.LogInformation("测试指标监控完成: {TestSessionId}", testSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试指标监控失败: {TestSessionId}", testSessionId);
                monitoringResult.ErrorMessage += $"; Metrics monitoring failed: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 更新监控结果
        /// </summary>
        private void UpdateMonitoringResult(MonitoringResult monitoringResult, List<TestMetrics> metrics)
        {
            if (metrics.Count == 0)
                return;
            
            var latestMetrics = metrics.Last();
            
            // 更新内存使用
            monitoringResult.PeakMemoryUsage = Math.Max(monitoringResult.PeakMemoryUsage, latestMetrics.MemoryUsage);
            
            // 更新CPU使用
            monitoringResult.AverageCpuUsage = metrics.Average(m => m.CpuUsage);
            
            // 更新线程数
            monitoringResult.MaxThreadCount = Math.Max(monitoringResult.MaxThreadCount, latestMetrics.ThreadCount);
            
            // 更新执行时间
            monitoringResult.Duration = DateTime.UtcNow - monitoringResult.StartTime;
        }
        
        /// <summary>
        /// 检查告警条件
        /// </summary>
        private void CheckAlertConditions(MonitoringResult monitoringResult, TestMetrics metrics)
        {
            var alerts = new List<string>();
            
            // 检查内存使用
            var memoryThreshold = _configuration.GetValue<long>("Monitoring:MemoryThresholdMB", 512) * 1024 * 1024;
            if (metrics.MemoryUsage > memoryThreshold)
            {
                alerts.Add($"内存使用过高: {metrics.MemoryUsage / 1024 / 1024:F1}MB > {memoryThreshold / 1024 / 1024:F1}MB");
            }
            
            // 检查CPU使用
            var cpuThreshold = _configuration.GetValue<double>("Monitoring:CpuThresholdPercent", 80.0);
            if (metrics.CpuUsage > cpuThreshold)
            {
                alerts.Add($"CPU使用过高: {metrics.CpuUsage:F1}% > {cpuThreshold:F1}%");
            }
            
            // 检查线程数
            var threadThreshold = _configuration.GetValue<int>("Monitoring:ThreadThreshold", 50);
            if (metrics.ThreadCount > threadThreshold)
            {
                alerts.Add($"线程数过多: {metrics.ThreadCount} > {threadThreshold}");
            }
            
            // 检查执行时间
            var timeThreshold = _configuration.GetValue<int>("Monitoring:TimeThresholdSeconds", 300);
            var executionTime = (DateTime.UtcNow - monitoringResult.StartTime).TotalSeconds;
            if (executionTime > timeThreshold)
            {
                alerts.Add($"执行时间过长: {executionTime:F1}s > {timeThreshold}s");
            }
            
            // 添加告警
            foreach (var alert in alerts)
            {
                monitoringResult.Alerts.Add(new Alert
                {
                    Timestamp = DateTime.UtcNow,
                    Message = alert,
                    Severity = AlertSeverity.Warning,
                    Category = "Performance"
                });
                
                _logger.LogWarning("监控告警: {Alert}", alert);
            }
        }
        
        /// <summary>
        /// 生成指标汇总
        /// </summary>
        private TestMetricsSummary GenerateMetricsSummary(List<TestMetrics> metrics)
        {
            if (metrics.Count == 0)
                return new TestMetricsSummary();
            
            return new TestMetricsSummary
            {
                TotalMeasurements = metrics.Count,
                AverageMemoryUsage = metrics.Average(m => m.MemoryUsage),
                PeakMemoryUsage = metrics.Max(m => m.MemoryUsage),
                AverageCpuUsage = metrics.Average(m => m.CpuUsage),
                PeakCpuUsage = metrics.Max(m => m.CpuUsage),
                AverageThreadCount = metrics.Average(m => m.ThreadCount),
                MaxThreadCount = metrics.Max(m => m.ThreadCount),
                MeasurementDuration = metrics.Last().Timestamp - metrics.First().Timestamp
            };
        }
        
        /// <summary>
        /// 生成监控报告
        /// </summary>
        public string GenerateMonitoringReport(MonitoringResult result)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== 测试监控报告 ===");
            report.AppendLine($"测试会话ID: {result.TestSessionId}");
            report.AppendLine($"监控时间: {result.StartTime:yyyy-MM-dd HH:mm:ss} - {result.EndTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"监控时长: {result.Duration.TotalSeconds:F1}秒");
            report.AppendLine($"状态: {result.Status}");
            report.AppendLine();
            
            if (result.Summary != null)
            {
                report.AppendLine("【性能指标】");
                report.AppendLine($"平均内存使用: {result.Summary.AverageMemoryUsage / 1024 / 1024:F1}MB");
                report.AppendLine($"峰值内存使用: {result.Summary.PeakMemoryUsage / 1024 / 1024:F1}MB");
                report.AppendLine($"平均CPU使用: {result.Summary.AverageCpuUsage:F1}%");
                report.AppendLine($"峰值CPU使用: {result.Summary.PeakCpuUsage:F1}%");
                report.AppendLine($"平均线程数: {result.Summary.AverageThreadCount:F1}");
                report.AppendLine($"最大线程数: {result.Summary.MaxThreadCount}");
                report.AppendLine($"测量次数: {result.Summary.TotalMeasurements}");
                report.AppendLine($"测量时长: {result.Summary.MeasurementDuration.TotalSeconds:F1}秒");
                report.AppendLine();
            }
            
            if (result.Alerts.Count > 0)
            {
                report.AppendLine("【监控告警】");
                foreach (var alert in result.Alerts)
                {
                    report.AppendLine($"[{alert.Timestamp:HH:mm:ss}] {alert.Severity}: {alert.Message}");
                }
                report.AppendLine();
            }
            
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                report.AppendLine($"错误信息: {result.ErrorMessage}");
            }
            
            return report.ToString();
        }
    }
}