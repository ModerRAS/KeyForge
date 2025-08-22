using System;
using System.Collections.Generic;

namespace KeyForge.Tests.TestFramework
{
    /// <summary>
    /// 测试方法信息
    /// 简化实现：基本的测试方法信息
    /// 原本实现：包含完整的测试方法元数据和分析
    /// </summary>
    public class TestMethodInfo
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }
        
        /// <summary>
        /// 是否为Fact测试
        /// </summary>
        public bool IsFact { get; set; }
        
        /// <summary>
        /// 是否为Theory测试
        /// </summary>
        public bool IsTheory { get; set; }
        
        /// <summary>
        /// 测试类别
        /// </summary>
        public string Category { get; set; } = "Unit";
        
        /// <summary>
        /// 估算执行时间（毫秒）
        /// </summary>
        public double ExecutionTime { get; set; }
        
        /// <summary>
        /// 完整方法名
        /// </summary>
        public string FullName => $"{ClassName}.{MethodName}";
    }
    
    /// <summary>
    /// 被测试代码信息
    /// 简化实现：基本的代码信息
    /// 原本实现：包含完整的代码元数据和复杂度分析
    /// </summary>
    public class TestedCodeInfo
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName { get; set; }
        
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }
        
        /// <summary>
        /// 是否为公共方法
        /// </summary>
        public bool IsPublic { get; set; }
        
        /// <summary>
        /// 估算复杂度
        /// </summary>
        public int Complexity { get; set; }
        
        /// <summary>
        /// 估算代码行数
        /// </summary>
        public int LinesOfCode { get; set; }
        
        /// <summary>
        /// 完整方法名
        /// </summary>
        public string FullName => $"{ClassName}.{MethodName}";
    }
    
    /// <summary>
    /// 覆盖率指标
    /// 简化实现：基本的覆盖率指标
    /// 原本实现：包含完整的覆盖率计算和趋势分析
    /// </summary>
    public class CoverageMetrics
    {
        /// <summary>
        /// 测试方法数量
        /// </summary>
        public int TestMethodCount { get; set; }
        
        /// <summary>
        /// 被测试方法数量
        /// </summary>
        public int TestedMethodCount { get; set; }
        
        /// <summary>
        /// 单元测试数量
        /// </summary>
        public int UnitTestCount { get; set; }
        
        /// <summary>
        /// 集成测试数量
        /// </summary>
        public int IntegrationTestCount { get; set; }
        
        /// <summary>
        /// 端到端测试数量
        /// </summary>
        public int EndToEndTestCount { get; set; }
        
        /// <summary>
        /// 性能测试数量
        /// </summary>
        public int PerformanceTestCount { get; set; }
        
        /// <summary>
        /// 估计行覆盖率
        /// </summary>
        public double EstimatedLineCoverage { get; set; }
        
        /// <summary>
        /// 估计分支覆盖率
        /// </summary>
        public double EstimatedBranchCoverage { get; set; }
        
        /// <summary>
        /// 估计方法覆盖率
        /// </summary>
        public double EstimatedMethodCoverage { get; set; }
        
        /// <summary>
        /// 平均测试执行时间
        /// </summary>
        public double AverageTestExecutionTime { get; set; }
        
        /// <summary>
        /// 测试复杂度得分
        /// </summary>
        public double TestComplexityScore { get; set; }
        
        /// <summary>
        /// 代码复杂度得分
        /// </summary>
        public double CodeComplexityScore { get; set; }
    }
    
    /// <summary>
    /// 覆盖率分析结果
    /// 简化实现：基本的覆盖率分析结果
    /// 原本实现：包含完整的分析结果和历史趋势
    /// </summary>
    public class CoverageAnalysisResult
    {
        /// <summary>
        /// 分析时间
        /// </summary>
        public DateTime AnalysisTime { get; set; }
        
        /// <summary>
        /// 测试程序集
        /// </summary>
        public string TestAssembly { get; set; }
        
        /// <summary>
        /// 测试方法信息
        /// </summary>
        public List<TestMethodInfo> TestMethods { get; set; } = new List<TestMethodInfo>();
        
        /// <summary>
        /// 被测试代码信息
        /// </summary>
        public List<TestedCodeInfo> TestedCode { get; set; } = new List<TestedCodeInfo>();
        
        /// <summary>
        /// 覆盖率指标
        /// </summary>
        public CoverageMetrics CoverageMetrics { get; set; } = new CoverageMetrics();
        
        /// <summary>
        /// 改进建议
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}