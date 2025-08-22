using System;
using System.Collections.Generic;

namespace KeyForge.Core.Quality
{
    /// <summary>
    /// 质量门禁配置
    /// 简化实现：基本的质量门禁配置
    /// 原本实现：包含完整的配置管理和验证
    /// </summary>
    public class QualityGateConfiguration
    {
        /// <summary>
        /// 覆盖率配置
        /// </summary>
        public CoverageConfiguration Coverage { get; set; } = new CoverageConfiguration();
        
        /// <summary>
        /// 性能配置
        /// </summary>
        public PerformanceConfiguration Performance { get; set; } = new PerformanceConfiguration();
        
        /// <summary>
        /// 可靠性配置
        /// </summary>
        public ReliabilityConfiguration Reliability { get; set; } = new ReliabilityConfiguration();
        
        /// <summary>
        /// 安全性配置
        /// </summary>
        public SecurityConfiguration Security { get; set; } = new SecurityConfiguration();
        
        /// <summary>
        /// 可维护性配置
        /// </summary>
        public MaintainabilityConfiguration Maintainability { get; set; } = new MaintainabilityConfiguration();
    }
    
    /// <summary>
    /// 覆盖率配置
    /// </summary>
    public class CoverageConfiguration
    {
        /// <summary>
        /// 行覆盖率要求（百分比）
        /// </summary>
        public double Line { get; set; } = 60;
        
        /// <summary>
        /// 分支覆盖率要求（百分比）
        /// </summary>
        public double Branch { get; set; } = 55;
        
        /// <summary>
        /// 方法覆盖率要求（百分比）
        /// </summary>
        public double Method { get; set; } = 65;
    }
    
    /// <summary>
    /// 性能配置
    /// </summary>
    public class PerformanceConfiguration
    {
        /// <summary>
        /// 最大执行时间（毫秒）
        /// </summary>
        public double MaxExecutionTime { get; set; } = 5000;
        
        /// <summary>
        /// 最大内存使用（字节）
        /// </summary>
        public long MaxMemoryUsage { get; set; } = 52428800; // 50MB
        
        /// <summary>
        /// 最小成功率
        /// </summary>
        public double MinSuccessRate { get; set; } = 0.95;
    }
    
    /// <summary>
    /// 可靠性配置
    /// </summary>
    public class ReliabilityConfiguration
    {
        /// <summary>
        /// 测试通过率要求（百分比）
        /// </summary>
        public double TestPassRate { get; set; } = 1.0;
        
        /// <summary>
        /// 不稳定测试数量限制
        /// </summary>
        public int FlakyTests { get; set; } = 0;
    }
    
    /// <summary>
    /// 安全性配置
    /// </summary>
    public class SecurityConfiguration
    {
        /// <summary>
        /// 是否允许安全漏洞
        /// </summary>
        public bool NoVulnerabilities { get; set; } = true;
        
        /// <summary>
        /// 代码异味数量限制
        /// </summary>
        public int CodeSmells { get; set; } = 50;
    }
    
    /// <summary>
    /// 可维护性配置
    /// </summary>
    public class MaintainabilityConfiguration
    {
        /// <summary>
        /// 最大复杂度
        /// </summary>
        public int MaxComplexity { get; set; } = 10;
        
        /// <summary>
        /// 最大重复率（百分比）
        /// </summary>
        public double MaxDuplication { get; set; } = 5;
    }
    
    /// <summary>
    /// 质量指标
    /// </summary>
    public class QualityMetrics
    {
        /// <summary>
        /// 收集时间
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 项目路径
        /// </summary>
        public string ProjectPath { get; set; }
        
        /// <summary>
        /// 覆盖率指标
        /// </summary>
        public CoverageMetrics Coverage { get; set; }
        
        /// <summary>
        /// 性能指标
        /// </summary>
        public PerformanceMetrics Performance { get; set; }
        
        /// <summary>
        /// 可靠性指标
        /// </summary>
        public ReliabilityMetrics Reliability { get; set; }
        
        /// <summary>
        /// 安全性指标
        /// </summary>
        public SecurityMetrics Security { get; set; }
        
        /// <summary>
        /// 可维护性指标
        /// </summary>
        public MaintainabilityMetrics Maintainability { get; set; }
    }
    
    /// <summary>
    /// 覆盖率指标
    /// </summary>
    public class CoverageMetrics
    {
        /// <summary>
        /// 行覆盖率（百分比）
        /// </summary>
        public double LineCoverage { get; set; }
        
        /// <summary>
        /// 分支覆盖率（百分比）
        /// </summary>
        public double BranchCoverage { get; set; }
        
        /// <summary>
        /// 方法覆盖率（百分比）
        /// </summary>
        public double MethodCoverage { get; set; }
    }
    
    /// <summary>
    /// 性能指标
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// 平均执行时间（毫秒）
        /// </summary>
        public double AverageExecutionTime { get; set; }
        
        /// <summary>
        /// 平均内存使用（字节）
        /// </summary>
        public long AverageMemoryUsage { get; set; }
        
        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate { get; set; }
    }
    
    /// <summary>
    /// 可靠性指标
    /// </summary>
    public class ReliabilityMetrics
    {
        /// <summary>
        /// 测试通过率
        /// </summary>
        public double TestPassRate { get; set; }
        
        /// <summary>
        /// 不稳定测试数量
        /// </summary>
        public int FlakyTestCount { get; set; }
    }
    
    /// <summary>
    /// 安全性指标
    /// </summary>
    public class SecurityMetrics
    {
        /// <summary>
        /// 漏洞数量
        /// </summary>
        public int VulnerabilityCount { get; set; }
        
        /// <summary>
        /// 代码异味数量
        /// </summary>
        public int CodeSmellCount { get; set; }
    }
    
    /// <summary>
    /// 可维护性指标
    /// </summary>
    public class MaintainabilityMetrics
    {
        /// <summary>
        /// 平均复杂度
        /// </summary>
        public double AverageComplexity { get; set; }
        
        /// <summary>
        /// 重复率（百分比）
        /// </summary>
        public double DuplicationPercentage { get; set; }
    }
    
    /// <summary>
    /// 质量检查结果
    /// </summary>
    public class QualityCheckResult
    {
        /// <summary>
        /// 检查名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 检查描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 是否通过
        /// </summary>
        public bool Passed { get; set; }
        
        /// <summary>
        /// 详细信息
        /// </summary>
        public List<string> Details { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// 质量门禁检查结果
    /// </summary>
    public class QualityGateResult
    {
        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 项目路径
        /// </summary>
        public string ProjectPath { get; set; }
        
        /// <summary>
        /// 质量指标
        /// </summary>
        public QualityMetrics Metrics { get; set; }
        
        /// <summary>
        /// 覆盖率检查结果
        /// </summary>
        public QualityCheckResult CoverageCheck { get; set; }
        
        /// <summary>
        /// 性能检查结果
        /// </summary>
        public QualityCheckResult PerformanceCheck { get; set; }
        
        /// <summary>
        /// 可靠性检查结果
        /// </summary>
        public QualityCheckResult ReliabilityCheck { get; set; }
        
        /// <summary>
        /// 安全性检查结果
        /// </summary>
        public QualityCheckResult SecurityCheck { get; set; }
        
        /// <summary>
        /// 可维护性检查结果
        /// </summary>
        public QualityCheckResult MaintainabilityCheck { get; set; }
        
        /// <summary>
        /// 是否通过质量门禁
        /// </summary>
        public bool Passed { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}