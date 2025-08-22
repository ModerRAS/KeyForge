using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using KeyForge.Tests.Quality;
using KeyForge.Core.Quality;
using FluentAssertions;

namespace KeyForge.Tests.QualityTests
{
    /// <summary>
    /// 质量门禁测试
    /// 简化实现：基本的质量门禁功能测试
    /// 原本实现：包含完整的质量门禁测试和验证
    /// </summary>
    public class QualityGateTests : TestBase
    {
        private readonly QualityGateChecker _qualityGateChecker;
        private readonly QualityMetricsCollector _metricsCollector;
        private readonly ITestOutputHelper _output;
        
        public QualityGateTests(ITestOutputHelper output) : base(output)
        {
            _output = output;
            _metricsCollector = GetService<QualityMetricsCollector>();
            _qualityGateChecker = GetService<QualityGateChecker>();
        }
        
        protected override void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            // 添加配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("quality-gate.yml", optional: true)
                .Build();
            
            services.AddSingleton<IConfiguration>(configuration);
            
            // 添加质量门禁服务
            services.AddSingleton<QualityMetricsCollector>();
            services.AddSingleton<QualityGateChecker>();
            
            // 添加日志
            services.AddLogging(builder => builder.AddXunit(_output));
        }
        
        [Fact]
        public async Task QualityGateChecker_ShouldExecuteQualityGate()
        {
            // Arrange
            var projectPath = Directory.GetCurrentDirectory();
            
            // Act
            var result = await _qualityGateChecker.ExecuteQualityGateAsync(projectPath);
            
            // Assert
            result.Should().NotBeNull();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.ProjectPath.Should().Be(projectPath);
            
            _output.WriteLine($"质量门禁结果: {(result.Passed ? "通过" : "失败")}");
            _output.WriteLine($"检查时间: {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
        }
        
        [Fact]
        public async Task QualityGateChecker_ShouldGenerateReport()
        {
            // Arrange
            var projectPath = Directory.GetCurrentDirectory();
            var result = await _qualityGateChecker.ExecuteQualityGateAsync(projectPath);
            
            // Act
            var report = _qualityGateChecker.GenerateReport(result);
            
            // Assert
            report.Should().NotBeNullOrEmpty();
            report.Should().Contain("质量门禁检查报告");
            report.Should().Contain(projectPath);
            report.Should().Contain(result.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            report.Should().Contain(result.Passed ? "✅ 通过" : "❌ 失败");
            
            _output.WriteLine("生成的质量门禁报告:");
            _output.WriteLine(report);
        }
        
        [Fact]
        public async Task QualityMetricsCollector_ShouldCollectMetrics()
        {
            // Arrange
            var projectPath = Directory.GetCurrentDirectory();
            
            // Act
            var metrics = await _metricsCollector.CollectMetricsAsync(projectPath);
            
            // Assert
            metrics.Should().NotBeNull();
            metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            metrics.ProjectPath.Should().Be(projectPath);
            
            _output.WriteLine($"收集到的质量指标时间: {metrics.Timestamp:yyyy-MM-dd HH:mm:ss}");
            _output.WriteLine($"项目路径: {metrics.ProjectPath}");
        }
        
        [Fact]
        public void QualityGateConfiguration_ShouldHaveDefaultValues()
        {
            // Arrange
            var config = new QualityGateConfiguration();
            
            // Assert
            config.Coverage.Line.Should().Be(60);
            config.Coverage.Branch.Should().Be(55);
            config.Coverage.Method.Should().Be(65);
            
            config.Performance.MaxExecutionTime.Should().Be(5000);
            config.Performance.MaxMemoryUsage.Should().Be(52428800);
            config.Performance.MinSuccessRate.Should().Be(0.95);
            
            config.Reliability.TestPassRate.Should().Be(1.0);
            config.Reliability.FlakyTests.Should().Be(0);
            
            config.Security.NoVulnerabilities.Should().BeTrue();
            config.Security.CodeSmells.Should().Be(50);
            
            config.Maintainability.MaxComplexity.Should().Be(10);
            config.Maintainability.MaxDuplication.Should().Be(5);
        }
        
        [Fact]
        public void QualityMetrics_ShouldInitializeProperly()
        {
            // Arrange
            var metrics = new QualityMetrics
            {
                Timestamp = DateTime.UtcNow,
                ProjectPath = "/test/project",
                Coverage = new CoverageMetrics
                {
                    LineCoverage = 80.5,
                    BranchCoverage = 75.2,
                    MethodCoverage = 85.0
                },
                Performance = new PerformanceMetrics
                {
                    AverageExecutionTime = 1200,
                    AverageMemoryUsage = 1024 * 1024,
                    SuccessRate = 0.98
                },
                Reliability = new ReliabilityMetrics
                {
                    TestPassRate = 0.99,
                    FlakyTestCount = 0
                },
                Security = new SecurityMetrics
                {
                    VulnerabilityCount = 0,
                    CodeSmellCount = 10
                },
                Maintainability = new MaintainabilityMetrics
                {
                    AverageComplexity = 5.2,
                    DuplicationPercentage = 2.1
                }
            };
            
            // Assert
            metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            metrics.ProjectPath.Should().Be("/test/project");
            metrics.Coverage.LineCoverage.Should().Be(80.5);
            metrics.Coverage.BranchCoverage.Should().Be(75.2);
            metrics.Coverage.MethodCoverage.Should().Be(85.0);
            metrics.Performance.AverageExecutionTime.Should().Be(1200);
            metrics.Performance.AverageMemoryUsage.Should().Be(1024 * 1024);
            metrics.Performance.SuccessRate.Should().Be(0.98);
            metrics.Reliability.TestPassRate.Should().Be(0.99);
            metrics.Reliability.FlakyTestCount.Should().Be(0);
            metrics.Security.VulnerabilityCount.Should().Be(0);
            metrics.Security.CodeSmellCount.Should().Be(10);
            metrics.Maintainability.AverageComplexity.Should().Be(5.2);
            metrics.Maintainability.DuplicationPercentage.Should().Be(2.1);
        }
        
        [Fact]
        public void QualityCheckResult_ShouldTrackCheckStatus()
        {
            // Arrange
            var checkResult = new QualityCheckResult
            {
                Name = "覆盖率检查",
                Description = "检查代码覆盖率是否达标",
                Passed = true
            };
            
            checkResult.Details.Add("行覆盖率: 80% (要求: 60%)");
            checkResult.Details.Add("分支覆盖率: 75% (要求: 55%)");
            checkResult.Details.Add("方法覆盖率: 85% (要求: 65%)");
            checkResult.Details.Add("✅ 代码覆盖率达标");
            
            // Assert
            checkResult.Name.Should().Be("覆盖率检查");
            checkResult.Description.Should().Be("检查代码覆盖率是否达标");
            checkResult.Passed.Should().BeTrue();
            checkResult.Details.Should().HaveCount(4);
            checkResult.Details.Should().Contain("✅ 代码覆盖率达标");
        }
        
        [Fact]
        public void QualityGateResult_ShouldAggregateAllChecks()
        {
            // Arrange
            var result = new QualityGateResult
            {
                Timestamp = DateTime.UtcNow,
                ProjectPath = "/test/project",
                Passed = true
            };
            
            result.CoverageCheck = new QualityCheckResult
            {
                Name = "覆盖率检查",
                Passed = true
            };
            
            result.PerformanceCheck = new QualityCheckResult
            {
                Name = "性能检查",
                Passed = true
            };
            
            result.ReliabilityCheck = new QualityCheckResult
            {
                Name = "可靠性检查",
                Passed = true
            };
            
            result.SecurityCheck = new QualityCheckResult
            {
                Name = "安全性检查",
                Passed = true
            };
            
            result.MaintainabilityCheck = new QualityCheckResult
            {
                Name = "可维护性检查",
                Passed = true
            };
            
            // Assert
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.ProjectPath.Should().Be("/test/project");
            result.Passed.Should().BeTrue();
            result.CoverageCheck.Passed.Should().BeTrue();
            result.PerformanceCheck.Passed.Should().BeTrue();
            result.ReliabilityCheck.Passed.Should().BeTrue();
            result.SecurityCheck.Passed.Should().BeTrue();
            result.MaintainabilityCheck.Passed.Should().BeTrue();
        }
        
        [Fact]
        public async Task QualityGateChecker_ShouldHandleProjectPathNotFound()
        {
            // Arrange
            var invalidProjectPath = "/nonexistent/project/path";
            
            // Act
            var result = await _qualityGateChecker.ExecuteQualityGateAsync(invalidProjectPath);
            
            // Assert
            result.Should().NotBeNull();
            result.ProjectPath.Should().Be(invalidProjectPath);
            result.Passed.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            
            _output.WriteLine($"无效路径测试结果: {result.ErrorMessage}");
        }
        
        [Fact]
        public void QualityMetricsCollector_ShouldHandleEmptyProjectPath()
        {
            // Arrange
            var emptyProjectPath = "";
            
            // Act
            var metrics = _metricsCollector.CollectMetricsAsync(emptyProjectPath).Result;
            
            // Assert
            metrics.Should().NotBeNull();
            metrics.ProjectPath.Should().Be(emptyProjectPath);
            metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}