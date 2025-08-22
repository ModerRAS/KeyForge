using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using KeyForge.Tests.Support;
using KeyForge.Domain;
using KeyForge.Core;

namespace KeyForge.Tests.UAT.UserScenarios
{
    /// <summary>
    /// UAT测试基类 - 提供用户场景测试的基础设施
    /// 原本实现：简单的UAT测试基类
    /// 简化实现：完整的用户场景测试支持
    /// </summary>
    public abstract class UATTestBase : TestBase
    {
        protected readonly IUserInterface _userInterface;
        protected readonly IScenarioRunner _scenarioRunner;
        protected readonly IPerformanceMonitor _performanceMonitor;
        protected readonly IUserExperienceEvaluator _uxEvaluator;
        protected readonly ITestDataManager _testDataManager;

        protected UATTestBase(ITestOutputHelper output) : base(output)
        {
            _userInterface = ServiceProvider.GetRequiredService<IUserInterface>();
            _scenarioRunner = ServiceProvider.GetRequiredService<IScenarioRunner>();
            _performanceMonitor = ServiceProvider.GetRequiredService<IPerformanceMonitor>();
            _uxEvaluator = ServiceProvider.GetRequiredService<IUserExperienceEvaluator>();
            _testDataManager = ServiceProvider.GetRequiredService<ITestDataManager>();
        }

        protected void SimulateUserAction(string actionName, Action action)
        {
            Log($"用户操作: {actionName}");
            var startTime = DateTime.UtcNow;
            
            try
            {
                action();
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                Log($"操作完成: {duration:F2}ms");
                
                // 记录性能数据
                _performanceMonitor.RecordUserAction(actionName, duration);
            }
            catch (Exception ex)
            {
                LogError($"用户操作失败: {actionName} - {ex.Message}");
                throw;
            }
        }

        protected async Task SimulateUserActionAsync(string actionName, Func<Task> action)
        {
            Log($"用户操作: {actionName}");
            var startTime = DateTime.UtcNow;
            
            try
            {
                await action();
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                Log($"操作完成: {duration:F2}ms");
                
                // 记录性能数据
                _performanceMonitor.RecordUserAction(actionName, duration);
            }
            catch (Exception ex)
            {
                LogError($"用户操作失败: {actionName} - {ex.Message}");
                throw;
            }
        }

        protected void ValidateUserExperience(string aspect, Func<bool> validation, string description = null)
        {
            description = description ?? aspect;
            Log($"验证用户体验: {aspect}");
            
            var startTime = DateTime.UtcNow;
            var result = validation();
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;
            
            result.Should().BeTrue($"用户体验验证失败: {aspect}");
            Log($"用户体验验证通过: {aspect} ({duration:F2}ms)");
            
            // 记录用户体验数据
            _uxEvaluator.RecordValidation(aspect, result, duration);
        }

        protected void ValidatePerformance(string aspect, double actualValue, double expectedThreshold, string unit = "ms")
        {
            Log($"验证性能: {aspect} = {actualValue:F2}{unit} (阈值: {expectedThreshold:F2}{unit})");
            
            actualValue.Should().BeLessThan(expectedThreshold, $"性能验证失败: {aspect}");
            Log($"性能验证通过: {aspect}");
            
            // 记录性能数据
            _performanceMonitor.RecordPerformanceMetric(aspect, actualValue, expectedThreshold);
        }

        protected void ValidateBusinessRule(string ruleName, Func<bool> validation)
        {
            Log($"验证业务规则: {ruleName}");
            
            var result = validation();
            result.Should().BeTrue($"业务规则验证失败: {ruleName}");
            Log($"业务规则验证通过: {ruleName}");
        }

        protected void MeasureUserSatisfaction(string feature, Action action)
        {
            Log($"测量用户满意度: {feature}");
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                action();
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                
                // 评估用户满意度
                var satisfaction = _uxEvaluator.EvaluateSatisfaction(feature, duration);
                Log($"用户满意度评估: {feature} - {satisfaction:F1}/10.0");
                
                satisfaction.Should().BeGreaterThanOrEqualTo(7.0, $"用户满意度过低: {feature}");
            }
            catch (Exception ex)
            {
                LogError($"用户满意度测量失败: {feature} - {ex.Message}");
                throw;
            }
        }

        protected async Task MeasureUserSatisfactionAsync(string feature, Func<Task> action)
        {
            Log($"测量用户满意度: {feature}");
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                await action();
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                
                // 评估用户满意度
                var satisfaction = _uxEvaluator.EvaluateSatisfaction(feature, duration);
                Log($"用户满意度评估: {feature} - {satisfaction:F1}/10.0");
                
                satisfaction.Should().BeGreaterThanOrEqualTo(7.0, $"用户满意度过低: {feature}");
            }
            catch (Exception ex)
            {
                LogError($"用户满意度测量失败: {feature} - {ex.Message}");
                throw;
            }
        }

        protected void SimulateUserError(string errorScenario, Action action)
        {
            Log($"模拟用户错误: {errorScenario}");
            
            try
            {
                action();
                LogError($"预期的用户错误未发生: {errorScenario}");
                Assert.True(false, $"预期的用户错误未发生: {errorScenario}");
            }
            catch (Exception ex)
            {
                Log($"用户错误处理: {errorScenario} - {ex.Message}");
                
                // 评估错误处理的用户友好性
                var friendliness = _uxEvaluator.EvaluateErrorHandling(ex.Message);
                friendliness.Should().BeGreaterThanOrEqualTo(0.7, $"错误处理不够友好: {errorScenario}");
                Log($"错误处理友好度: {friendliness:F1}");
            }
        }

        protected void ValidateLearningCurve(string feature, Action action, double expectedLearningTime = 30.0)
        {
            Log($"验证学习曲线: {feature}");
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                action();
                
                var endTime = DateTime.UtcNow;
                var learningTime = (endTime - startTime).TotalSeconds;
                
                Log($"学习时间: {learningTime:F1}秒 (期望: {expectedLearningTime:F1}秒)");
                
                learningTime.Should().BeLessThan(expectedLearningTime, $"学习成本过高: {feature}");
                
                // 记录学习曲线数据
                _uxEvaluator.RecordLearningCurve(feature, learningTime);
            }
            catch (Exception ex)
            {
                LogError($"学习曲线验证失败: {feature} - {ex.Message}");
                throw;
            }
        }

        protected void RunScenario(string scenarioName, Action scenario)
        {
            Log($"开始用户场景: {scenarioName}");
            
            var scenarioStartTime = DateTime.UtcNow;
            
            try
            {
                // 开始场景
                _scenarioRunner.StartScenario(scenarioName);
                
                // 执行场景
                scenario();
                
                // 完成场景
                var scenarioResult = _scenarioRunner.CompleteScenario(scenarioName);
                
                var scenarioEndTime = DateTime.UtcNow;
                var scenarioDuration = (scenarioEndTime - scenarioStartTime).TotalMilliseconds;
                
                Log($"用户场景完成: {scenarioName} - {scenarioDuration:F2}ms");
                
                // 验证场景结果
                scenarioResult.Success.Should().BeTrue($"用户场景失败: {scenarioName}");
                scenarioResult.UserSatisfaction.Should().BeGreaterThanOrEqualTo(7.0, $"用户满意度过低: {scenarioName}");
                
                // 记录场景数据
                _performanceMonitor.RecordScenario(scenarioName, scenarioDuration, scenarioResult.UserSatisfaction);
            }
            catch (Exception ex)
            {
                LogError($"用户场景失败: {scenarioName} - {ex.Message}");
                
                // 记录失败的场景
                _scenarioRunner.FailScenario(scenarioName, ex.Message);
                
                throw;
            }
        }

        protected async Task RunScenarioAsync(string scenarioName, Func<Task> scenario)
        {
            Log($"开始用户场景: {scenarioName}");
            
            var scenarioStartTime = DateTime.UtcNow;
            
            try
            {
                // 开始场景
                _scenarioRunner.StartScenario(scenarioName);
                
                // 执行场景
                await scenario();
                
                // 完成场景
                var scenarioResult = _scenarioRunner.CompleteScenario(scenarioName);
                
                var scenarioEndTime = DateTime.UtcNow;
                var scenarioDuration = (scenarioEndTime - scenarioStartTime).TotalMilliseconds;
                
                Log($"用户场景完成: {scenarioName} - {scenarioDuration:F2}ms");
                
                // 验证场景结果
                scenarioResult.Success.Should().BeTrue($"用户场景失败: {scenarioName}");
                scenarioResult.UserSatisfaction.Should().BeGreaterThanOrEqualTo(7.0, $"用户满意度过低: {scenarioName}");
                
                // 记录场景数据
                _performanceMonitor.RecordScenario(scenarioName, scenarioDuration, scenarioResult.UserSatisfaction);
            }
            catch (Exception ex)
            {
                LogError($"用户场景失败: {scenarioName} - {ex.Message}");
                
                // 记录失败的场景
                _scenarioRunner.FailScenario(scenarioName, ex.Message);
                
                throw;
            }
        }

        protected void SimulateRealWorldUsage(string usagePattern, Action action, int repeatCount = 1)
        {
            Log($"模拟真实使用: {usagePattern} (重复{repeatCount}次)");
            
            var totalStartTime = DateTime.UtcNow;
            
            for (int i = 0; i < repeatCount; i++)
            {
                var iterationStartTime = DateTime.UtcNow;
                
                try
                {
                    Log($"  第{i + 1}次执行");
                    action();
                    
                    var iterationEndTime = DateTime.UtcNow;
                    var iterationDuration = (iterationEndTime - iterationStartTime).TotalMilliseconds;
                    
                    Log($"  第{i + 1}次完成: {iterationDuration:F2}ms");
                }
                catch (Exception ex)
                {
                    LogError($"第{i + 1}次执行失败: {ex.Message}");
                    throw;
                }
            }
            
            var totalEndTime = DateTime.UtcNow;
            var totalDuration = (totalEndTime - totalStartTime).TotalMilliseconds;
            var averageDuration = totalDuration / repeatCount;
            
            Log($"真实使用模拟完成: 总耗时{totalDuration:F2}ms, 平均{averageDuration:F2}ms/次");
            
            // 记录真实使用数据
            _performanceMonitor.RecordRealWorldUsage(usagePattern, totalDuration, repeatCount);
        }

        protected void ValidateAccessibility(string accessibilityFeature, Func<bool> validation)
        {
            Log($"验证可访问性: {accessibilityFeature}");
            
            var result = validation();
            result.Should().BeTrue($"可访问性验证失败: {accessibilityFeature}");
            Log($"可访问性验证通过: {accessibilityFeature}");
            
            // 记录可访问性数据
            _uxEvaluator.RecordAccessibility(accessibilityFeature, result);
        }

        protected void GenerateUserExperienceReport()
        {
            Log("生成用户体验报告");
            
            var report = _uxEvaluator.GenerateReport();
            report.Should().NotBeNull();
            
            Log($"用户体验报告生成完成");
            Log($"  整体满意度: {report.OverallSatisfaction:F1}/10.0");
            Log($"  平均响应时间: {report.AverageResponseTime:F2}ms");
            Log($"  学习曲线评分: {report.LearningCurveScore:F1}/10.0");
            Log($"  错误处理评分: {report.ErrorHandlingScore:F1}/10.0");
            Log($"  可访问性评分: {report.AccessibilityScore:F1}/10.0");
        }
    }

    /// <summary>
    /// 用户场景测试数据结构
    /// </summary>
    public class UserScenario
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GameAction[] Actions { get; set; }
        public string ExpectedOutcome { get; set; }
        public double ExpectedSatisfaction { get; set; } = 8.0;
    }

    /// <summary>
    /// 游戏自动化场景
    /// </summary>
    public class GameAutomationScenario : UserScenario
    {
        public string GameName { get; set; }
        public string GameGenre { get; set; }
        public TimeSpan ExpectedExecutionTime { get; set; }
    }

    /// <summary>
    /// 办公自动化场景
    /// </summary>
    public class OfficeAutomationScenario : UserScenario
    {
        public string Application { get; set; }
        public string DocumentType { get; set; }
        public int ExpectedDocumentsProcessed { get; set; }
    }

    /// <summary>
    /// 系统操作场景
    /// </summary>
    public class SystemOperationScenario : UserScenario
    {
        public string OperationType { get; set; }
        public string TargetSystem { get; set; }
        public int ExpectedOperationsCompleted { get; set; }
    }
}