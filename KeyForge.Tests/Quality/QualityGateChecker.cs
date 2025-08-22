using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using KeyForge.Core.Quality;

namespace KeyForge.Tests.Quality
{
    /// <summary>
    /// 质量门禁检查器
    /// 简化实现：基本的质量门禁检查功能
    /// 原本实现：包含完整的质量门禁、报告生成和通知机制
    /// </summary>
    public class QualityGateChecker
    {
        private readonly ILogger<QualityGateChecker> _logger;
        private readonly QualityGateConfiguration _config;
        private readonly QualityMetricsCollector _metricsCollector;
        
        public QualityGateChecker(
            ILogger<QualityGateChecker> logger,
            IConfiguration configuration,
            QualityMetricsCollector metricsCollector)
        {
            _logger = logger;
            _config = configuration.GetSection("QualityGate").Get<QualityGateConfiguration>() ?? new QualityGateConfiguration();
            _metricsCollector = metricsCollector;
        }
        
        /// <summary>
        /// 执行质量门禁检查
        /// </summary>
        /// <param name="projectPath">项目路径</param>
        /// <returns>质量门禁检查结果</returns>
        public async Task<QualityGateResult> ExecuteQualityGateAsync(string projectPath)
        {
            _logger.LogInformation("开始执行质量门禁检查...");
            
            var result = new QualityGateResult
            {
                Timestamp = DateTime.UtcNow,
                ProjectPath = projectPath
            };
            
            try
            {
                // 收集质量指标
                var metrics = await _metricsCollector.CollectMetricsAsync(projectPath);
                result.Metrics = metrics;
                
                // 执行各项检查
                result.CoverageCheck = CheckCoverage(metrics);
                result.PerformanceCheck = CheckPerformance(metrics);
                result.ReliabilityCheck = CheckReliability(metrics);
                result.SecurityCheck = CheckSecurity(metrics);
                result.MaintainabilityCheck = CheckMaintainability(metrics);
                
                // 确定整体结果
                result.Passed = IsQualityGatePassed(result);
                
                _logger.LogInformation("质量门禁检查完成: {Result}", result.Passed ? "通过" : "失败");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "质量门禁检查失败");
                result.Passed = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        /// <summary>
        /// 检查代码覆盖率
        /// </summary>
        private QualityCheckResult CheckCoverage(QualityMetrics metrics)
        {
            var check = new QualityCheckResult
            {
                Name = "代码覆盖率",
                Description = "检查代码覆盖率是否达到要求"
            };
            
            var lineCoverage = metrics.Coverage?.LineCoverage ?? 0;
            var branchCoverage = metrics.Coverage?.BranchCoverage ?? 0;
            var methodCoverage = metrics.Coverage?.MethodCoverage ?? 0;
            
            check.Details.Add($"行覆盖率: {lineCoverage:F1}% (要求: {_config.Coverage.Line}%)");
            check.Details.Add($"分支覆盖率: {branchCoverage:F1}% (要求: {_config.Coverage.Branch}%)");
            check.Details.Add($"方法覆盖率: {methodCoverage:F1}% (要求: {_config.Coverage.Method}%)");
            
            check.Passed = lineCoverage >= _config.Coverage.Line &&
                          branchCoverage >= _config.Coverage.Branch &&
                          methodCoverage >= _config.Coverage.Method;
            
            if (!check.Passed)
            {
                check.Details.Add("❌ 代码覆盖率未达标");
            }
            else
            {
                check.Details.Add("✅ 代码覆盖率达标");
            }
            
            return check;
        }
        
        /// <summary>
        /// 检查性能指标
        /// </summary>
        private QualityCheckResult CheckPerformance(QualityMetrics metrics)
        {
            var check = new QualityCheckResult
            {
                Name = "性能指标",
                Description = "检查性能指标是否在可接受范围内"
            };
            
            var executionTime = metrics.Performance?.AverageExecutionTime ?? 0;
            var memoryUsage = metrics.Performance?.AverageMemoryUsage ?? 0;
            var successRate = metrics.Performance?.SuccessRate ?? 0;
            
            check.Details.Add($"平均执行时间: {executionTime:F1}ms (要求: <{_config.Performance.MaxExecutionTime}ms)");
            check.Details.Add($"平均内存使用: {memoryUsage / 1024 / 1024:F1}MB (要求: <{_config.Performance.MaxMemoryUsage / 1024 / 1024:F1}MB)");
            check.Details.Add($"成功率: {successRate:P1} (要求: >{_config.Performance.MinSuccessRate:P1})");
            
            check.Passed = executionTime <= _config.Performance.MaxExecutionTime &&
                          memoryUsage <= _config.Performance.MaxMemoryUsage &&
                          successRate >= _config.Performance.MinSuccessRate;
            
            if (!check.Passed)
            {
                check.Details.Add("❌ 性能指标未达标");
            }
            else
            {
                check.Details.Add("✅ 性能指标达标");
            }
            
            return check;
        }
        
        /// <summary>
        /// 检查可靠性指标
        /// </summary>
        private QualityCheckResult CheckReliability(QualityMetrics metrics)
        {
            var check = new QualityCheckResult
            {
                Name = "可靠性指标",
                Description = "检查测试可靠性和稳定性"
            };
            
            var testPassRate = metrics.Reliability?.TestPassRate ?? 0;
            var flakyTests = metrics.Reliability?.FlakyTestCount ?? 0;
            
            check.Details.Add($"测试通过率: {testPassRate:P1} (要求: {_config.Reliability.TestPassRate:P1})");
            check.Details.Add($"不稳定测试数量: {flakyTests} (要求: <{_config.Reliability.FlakyTests})");
            
            check.Passed = testPassRate >= _config.Reliability.TestPassRate &&
                          flakyTests <= _config.Reliability.FlakyTests;
            
            if (!check.Passed)
            {
                check.Details.Add("❌ 可靠性指标未达标");
            }
            else
            {
                check.Details.Add("✅ 可靠性指标达标");
            }
            
            return check;
        }
        
        /// <summary>
        /// 检查安全性指标
        /// </summary>
        private QualityCheckResult CheckSecurity(QualityMetrics metrics)
        {
            var check = new QualityCheckResult
            {
                Name = "安全性指标",
                Description = "检查代码安全性"
            };
            
            var vulnerabilities = metrics.Security?.VulnerabilityCount ?? 0;
            var codeSmells = metrics.Security?.CodeSmellCount ?? 0;
            
            check.Details.Add($"安全漏洞数量: {vulnerabilities} (要求: 0)");
            check.Details.Add($"代码异味数量: {codeSmells} (要求: <{_config.Security.CodeSmells})");
            
            check.Passed = vulnerabilities == 0 && codeSmells <= _config.Security.CodeSmells;
            
            if (!check.Passed)
            {
                check.Details.Add("❌ 安全性指标未达标");
            }
            else
            {
                check.Details.Add("✅ 安全性指标达标");
            }
            
            return check;
        }
        
        /// <summary>
        /// 检查可维护性指标
        /// </summary>
        private QualityCheckResult CheckMaintainability(QualityMetrics metrics)
        {
            var check = new QualityCheckResult
            {
                Name = "可维护性指标",
                Description = "检查代码可维护性"
            };
            
            var complexity = metrics.Maintainability?.AverageComplexity ?? 0;
            var duplication = metrics.Maintainability?.DuplicationPercentage ?? 0;
            
            check.Details.Add($"平均复杂度: {complexity:F1} (要求: <{_config.Maintainability.MaxComplexity})");
            check.Details.Add($"代码重复率: {duplication:F1}% (要求: <{_config.Maintainability.MaxDuplication}%)");
            
            check.Passed = complexity <= _config.Maintainability.MaxComplexity &&
                          duplication <= _config.Maintainability.MaxDuplication;
            
            if (!check.Passed)
            {
                check.Details.Add("❌ 可维护性指标未达标");
            }
            else
            {
                check.Details.Add("✅ 可维护性指标达标");
            }
            
            return check;
        }
        
        /// <summary>
        /// 判断质量门禁是否通过
        /// </summary>
        private bool IsQualityGatePassed(QualityGateResult result)
        {
            return result.CoverageCheck?.Passed == true &&
                   result.PerformanceCheck?.Passed == true &&
                   result.ReliabilityCheck?.Passed == true &&
                   result.SecurityCheck?.Passed == true &&
                   result.MaintainabilityCheck?.Passed == true;
        }
        
        /// <summary>
        /// 生成质量门禁报告
        /// </summary>
        public string GenerateReport(QualityGateResult result)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== 质量门禁检查报告 ===");
            report.AppendLine($"项目路径: {result.ProjectPath}");
            report.AppendLine($"检查时间: {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"整体结果: {(result.Passed ? "✅ 通过" : "❌ 失败")}");
            report.AppendLine();
            
            if (result.CoverageCheck != null)
            {
                report.AppendLine($"【{result.CoverageCheck.Name}】");
                report.AppendLine($"状态: {(result.CoverageCheck.Passed ? "✅ 通过" : "❌ 失败")}");
                foreach (var detail in result.CoverageCheck.Details)
                {
                    report.AppendLine($"  {detail}");
                }
                report.AppendLine();
            }
            
            if (result.PerformanceCheck != null)
            {
                report.AppendLine($"【{result.PerformanceCheck.Name}】");
                report.AppendLine($"状态: {(result.PerformanceCheck.Passed ? "✅ 通过" : "❌ 失败")}");
                foreach (var detail in result.PerformanceCheck.Details)
                {
                    report.AppendLine($"  {detail}");
                }
                report.AppendLine();
            }
            
            if (result.ReliabilityCheck != null)
            {
                report.AppendLine($"【{result.ReliabilityCheck.Name}】");
                report.AppendLine($"状态: {(result.ReliabilityCheck.Passed ? "✅ 通过" : "❌ 失败")}");
                foreach (var detail in result.ReliabilityCheck.Details)
                {
                    report.AppendLine($"  {detail}");
                }
                report.AppendLine();
            }
            
            if (result.SecurityCheck != null)
            {
                report.AppendLine($"【{result.SecurityCheck.Name}】");
                report.AppendLine($"状态: {(result.SecurityCheck.Passed ? "✅ 通过" : "❌ 失败")}");
                foreach (var detail in result.SecurityCheck.Details)
                {
                    report.AppendLine($"  {detail}");
                }
                report.AppendLine();
            }
            
            if (result.MaintainabilityCheck != null)
            {
                report.AppendLine($"【{result.MaintainabilityCheck.Name}】");
                report.AppendLine($"状态: {(result.MaintainabilityCheck.Passed ? "✅ 通过" : "❌ 失败")}");
                foreach (var detail in result.MaintainabilityCheck.Details)
                {
                    report.AppendLine($"  {detail}");
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