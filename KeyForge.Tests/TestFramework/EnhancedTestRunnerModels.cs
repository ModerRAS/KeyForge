using System;
using System.Collections.Generic;

namespace KeyForge.Tests.TestFramework
{
    /// <summary>
    /// 测试运行结果
    /// 简化实现：基本的测试运行结果
    /// 原本实现：包含完整的测试运行结果和历史记录
    /// </summary>
    public class TestRunResult
    {
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
        /// 测试程序集路径
        /// </summary>
        public string TestAssemblyPath { get; set; }
        
        /// <summary>
        /// 单元测试结果
        /// </summary>
        public TestCategoryResult UnitTestResults { get; set; }
        
        /// <summary>
        /// 集成测试结果
        /// </summary>
        public TestCategoryResult IntegrationTestResults { get; set; }
        
        /// <summary>
        /// 端到端测试结果
        /// </summary>
        public TestCategoryResult EndToEndTestResults { get; set; }
        
        /// <summary>
        /// 性能测试结果
        /// </summary>
        public TestCategoryResult PerformanceTestResults { get; set; }
        
        /// <summary>
        /// 质量门禁测试结果
        /// </summary>
        public TestCategoryResult QualityGateTestResults { get; set; }
        
        /// <summary>
        /// 测试汇总
        /// </summary>
        public TestSummary Summary { get; set; }
        
        /// <summary>
        /// 覆盖率报告
        /// </summary>
        public CoverageAnalysisResult CoverageReport { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// 测试类别结果
    /// 简化实现：基本的测试类别结果
    /// 原本实现：包含完整的测试类别结果和详细统计
    /// </summary>
    public class TestCategoryResult
    {
        /// <summary>
        /// 测试类别
        /// </summary>
        public string Category { get; set; }
        
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
        /// 总测试数
        /// </summary>
        public int TotalTests { get; set; }
        
        /// <summary>
        /// 通过测试数
        /// </summary>
        public int PassedTests { get; set; }
        
        /// <summary>
        /// 失败测试数
        /// </summary>
        public int FailedTests { get; set; }
        
        /// <summary>
        /// 跳过测试数
        /// </summary>
        public int SkippedTests { get; set; }
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 是否通过
        /// </summary>
        public bool Passed { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 通过率
        /// </summary>
        public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests : 0;
    }
    
    /// <summary>
    /// 测试汇总
    /// 简化实现：基本的测试汇总
    /// 原本实现：包含完整的测试汇总和趋势分析
    /// </summary>
    public class TestSummary
    {
        /// <summary>
        /// 总测试数
        /// </summary>
        public int TotalTests { get; set; }
        
        /// <summary>
        /// 通过测试数
        /// </summary>
        public int PassedTests { get; set; }
        
        /// <summary>
        /// 失败测试数
        /// </summary>
        public int FailedTests { get; set; }
        
        /// <summary>
        /// 跳过测试数
        /// </summary>
        public int SkippedTests { get; set; }
        
        /// <summary>
        /// 总执行时间
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }
        
        /// <summary>
        /// 通过率
        /// </summary>
        public double PassRate { get; set; }
        
        /// <summary>
        /// 整体是否通过
        /// </summary>
        public bool OverallPassed { get; set; }
    }
}