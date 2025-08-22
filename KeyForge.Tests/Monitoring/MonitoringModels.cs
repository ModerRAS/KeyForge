using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeyForge.Tests.Monitoring
{
    /// <summary>
    /// 测试指标
    /// 简化实现：基本的测试指标
    /// 原本实现：包含完整的指标收集和历史数据
    /// </summary>
    public class TestMetrics
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 内存使用（字节）
        /// </summary>
        public long MemoryUsage { get; set; }
        
        /// <summary>
        /// CPU使用率（百分比）
        /// </summary>
        public double CpuUsage { get; set; }
        
        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount { get; set; }
        
        /// <summary>
        /// GC收集次数（第0代）
        /// </summary>
        public int Gen0CollectionCount { get; set; }
        
        /// <summary>
        /// GC收集次数（第1代）
        /// </summary>
        public int Gen1CollectionCount { get; set; }
        
        /// <summary>
        /// GC收集次数（第2代）
        /// </summary>
        public int Gen2CollectionCount { get; set; }
        
        /// <summary>
        /// 句柄数
        /// </summary>
        public long HandleCount { get; set; }
    }
    
    /// <summary>
    /// 测试指标汇总
    /// 简化实现：基本的指标汇总
    /// 原本实现：包含完整的指标汇总和趋势分析
    /// </summary>
    public class TestMetricsSummary
    {
        /// <summary>
        /// 总测量次数
        /// </summary>
        public int TotalMeasurements { get; set; }
        
        /// <summary>
        /// 平均内存使用
        /// </summary>
        public double AverageMemoryUsage { get; set; }
        
        /// <summary>
        /// 峰值内存使用
        /// </summary>
        public long PeakMemoryUsage { get; set; }
        
        /// <summary>
        /// 平均CPU使用
        /// </summary>
        public double AverageCpuUsage { get; set; }
        
        /// <summary>
        /// 峰值CPU使用
        /// </summary>
        public double PeakCpuUsage { get; set; }
        
        /// <summary>
        /// 平均线程数
        /// </summary>
        public double AverageThreadCount { get; set; }
        
        /// <summary>
        /// 最大线程数
        /// </summary>
        public int MaxThreadCount { get; set; }
        
        /// <summary>
        /// 测量时长
        /// </summary>
        public TimeSpan MeasurementDuration { get; set; }
    }
    
    /// <summary>
    /// 监控结果
    /// 简化实现：基本的监控结果
    /// 原本实现：包含完整的监控结果和历史记录
    /// </summary>
    public class MonitoringResult
    {
        /// <summary>
        /// 测试会话ID
        /// </summary>
        public string TestSessionId { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// 执行时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// 峰值内存使用
        /// </summary>
        public long PeakMemoryUsage { get; set; }
        
        /// <summary>
        /// 平均CPU使用
        /// </summary>
        public double AverageCpuUsage { get; set; }
        
        /// <summary>
        /// 最大线程数
        /// </summary>
        public int MaxThreadCount { get; set; }
        
        /// <summary>
        /// 指标数据
        /// </summary>
        public List<TestMetrics> Metrics { get; set; } = new List<TestMetrics>();
        
        /// <summary>
        /// 指标汇总
        /// </summary>
        public TestMetricsSummary Summary { get; set; }
        
        /// <summary>
        /// 告警列表
        /// </summary>
        public List<Alert> Alerts { get; set; } = new List<Alert>();
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// 告警
    /// 简化实现：基本的告警信息
    /// 原本实现：包含完整的告警管理和通知机制
    /// </summary>
    public class Alert
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 告警消息
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 告警级别
        /// </summary>
        public AlertSeverity Severity { get; set; }
        
        /// <summary>
        /// 告警类别
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// 是否已解决
        /// </summary>
        public bool IsResolved { get; set; }
    }
    
    /// <summary>
    /// 告警级别
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        
        /// <summary>
        /// 严重
        /// </summary>
        Critical
    }
}