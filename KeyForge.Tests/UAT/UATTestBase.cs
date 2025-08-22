using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using KeyForge.Tests.Support;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Core.Domain.Act;

namespace KeyForge.Tests.UAT;

/// <summary>
/// UAT测试基类 - 用户验收测试
/// 
/// 原本实现：
/// - 复杂的UI自动化框架
/// - 真实的用户界面操作
/// - 复杂的测试数据准备
/// 
/// 简化实现：
/// - 模拟用户操作的BDD测试
/// - 简化的用户体验验证
/// - 基础的性能监控
/// </summary>
public abstract class UATTestBase : TestBase
{
    protected readonly string _uatTestDirectory;
    protected readonly Stopwatch _performanceStopwatch;
    protected readonly List<UserActionRecord> _userActions;
    protected UserExperienceMetrics _experienceMetrics;

    protected UATTestBase(ITestOutputHelper output) : base(output)
    {
        _uatTestDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_UAT_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_uatTestDirectory);
        
        _performanceStopwatch = new Stopwatch();
        _userActions = new List<UserActionRecord>();
        _experienceMetrics = new UserExperienceMetrics();
        
        Log($"创建UAT测试目录: {_uatTestDirectory}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Directory.Exists(_uatTestDirectory))
                {
                    Directory.Delete(_uatTestDirectory, true);
                    Log($"清理UAT测试目录: {_uatTestDirectory}");
                }
                
                _performanceStopwatch.Stop();
                GenerateUATReport();
            }
            catch (Exception ex)
            {
                Log($"清理UAT测试目录失败: {ex.Message}");
            }
        }
        base.Dispose(disposing);
    }

    #region BDD Given-When-Then 结构

    protected void Given(string description, Action action)
    {
        Log($"GIVEN: {description}");
        RecordUserAction("Given", description);
        
        var start = Stopwatch.StartNew();
        action();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("Given", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void Given(string description, Func<Task> action)
    {
        Log($"GIVEN: {description}");
        RecordUserAction("Given", description);
        
        var start = Stopwatch.StartNew();
        action().GetAwaiter().GetResult();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("Given", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void When(string description, Action action)
    {
        Log($"WHEN: {description}");
        RecordUserAction("When", description);
        
        var start = Stopwatch.StartNew();
        action();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("When", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void When(string description, Func<Task> action)
    {
        Log($"WHEN: {description}");
        RecordUserAction("When", description);
        
        var start = Stopwatch.StartNew();
        action().GetAwaiter().GetResult();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("When", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void Then(string description, Action action)
    {
        Log($"THEN: {description}");
        RecordUserAction("Then", description);
        
        var start = Stopwatch.StartNew();
        action();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("Then", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void Then(string description, Func<Task> action)
    {
        Log($"THEN: {description}");
        RecordUserAction("Then", description);
        
        var start = Stopwatch.StartNew();
        action().GetAwaiter().GetResult();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("Then", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void And(string description, Action action)
    {
        Log($"AND: {description}");
        RecordUserAction("And", description);
        
        var start = Stopwatch.StartNew();
        action();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("And", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    protected void And(string description, Func<Task> action)
    {
        Log($"AND: {description}");
        RecordUserAction("And", description);
        
        var start = Stopwatch.StartNew();
        action().GetAwaiter().GetResult();
        start.Stop();
        
        _experienceMetrics.RecordStepTiming("And", start.ElapsedMilliseconds);
        Log($"✓ {description} (耗时: {start.ElapsedMilliseconds}ms)");
    }

    #endregion

    #region 用户体验相关方法

    protected void SimulateUserResponseTime()
    {
        // 模拟真实用户的响应时间 (0.5-2秒)
        var delay = new Random().Next(500, 2000);
        Task.Delay(delay).Wait();
        _experienceMetrics.RecordUserResponseTime(delay);
    }

    protected void AssertUserExperience(string aspect, bool expectedGoodExperience)
    {
        var actualExperience = _experienceMetrics.EvaluateExperience(aspect);
        actualExperience.Should().Be(expectedGoodExperience, 
            $"用户体验 {aspect} 不符合预期");
    }

    protected void AssertResponseTimeIsWithin(string operation, int maxMilliseconds)
    {
        var actualTime = _experienceMetrics.GetAverageResponseTime(operation);
        actualTime.Should().BeLessThanOrEqualTo(maxMilliseconds,
            $"操作 {operation} 的响应时间 {actualTime}ms 超过了最大允许时间 {maxMilliseconds}ms");
    }

    protected void RecordUserAction(string actionType, string description)
    {
        var record = new UserActionRecord
        {
            ActionId = Guid.NewGuid(),
            ActionTime = DateTime.UtcNow,
            ActionType = actionType,
            Description = description,
            TestContext = GetType().Name
        };
        
        _userActions.Add(record);
    }

    #endregion

    #region 性能监控方法

    protected void StartPerformanceMonitoring()
    {
        _performanceStopwatch.Restart();
        Log("开始性能监控");
    }

    protected void StopPerformanceMonitoring(string operationName)
    {
        _performanceStopwatch.Stop();
        var elapsed = _performanceStopwatch.ElapsedMilliseconds;
        _experienceMetrics.RecordPerformanceMetric(operationName, elapsed);
        Log($"性能监控结束 - {operationName}: {elapsed}ms");
    }

    protected void AssertPerformanceIsWithin(string operation, int maxMilliseconds)
    {
        var actualTime = _experienceMetrics.GetPerformanceMetric(operation);
        actualTime.Should().BeLessThanOrEqualTo(maxMilliseconds,
            $"操作 {operation} 的性能 {actualTime}ms 超过了最大允许时间 {maxMilliseconds}ms");
    }

    #endregion

    #region 测试数据创建

    protected Script CreateGameAutomationScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("游戏自动化脚本", "用于自动化游戏操作的脚本");
        
        // 添加游戏相关的动作
        var keyboardAction = TestFixtures.CreateKeyboardAction();
        var mouseAction = TestFixtures.CreateMouseAction();
        var delayAction = TestFixtures.CreateDelayAction();
        
        script.AddAction(keyboardAction);
        script.AddAction(mouseAction);
        script.AddAction(delayAction);
        
        script.Activate();
        return script;
    }

    protected Script CreateOfficeAutomationScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("办公自动化脚本", "用于自动化办公操作的脚本");
        
        // 添加办公相关的动作
        for (int i = 0; i < 10; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    protected Script CreateSystemMaintenanceScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("系统维护脚本", "用于自动化系统维护任务");
        
        // 添加系统维护相关的动作
        for (int i = 0; i < 20; i++)
        {
            var action = TestFixtures.CreateDelayAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    #endregion

    #region UAT报告生成

    private void GenerateUATReport()
    {
        var report = new UATReport
        {
            TestName = GetType().Name,
            ExecutionTime = DateTime.UtcNow,
            TotalSteps = _userActions.Count,
            PerformanceMetrics = _experienceMetrics.GetPerformanceMetrics(),
            UserExperienceScore = _experienceMetrics.CalculateOverallScore(),
            UserActions = _userActions.ToList()
        };

        var reportPath = Path.Combine(_uatTestDirectory, "UAT-Report.json");
        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(reportPath, json);
        Log($"UAT报告已生成: {reportPath}");
        
        GenerateHtmlReport(report);
    }

    private void GenerateHtmlReport(UATReport report)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>KeyForge UAT测试报告</title>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px; }}
        .section {{ margin: 20px 0; }}
        .success {{ color: #27ae60; }}
        .warning {{ color: #f39c12; }}
        .error {{ color: #e74c3c; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
        .score {{ font-size: 24px; font-weight: bold; }}
        .excellent {{ color: #27ae60; }}
        .good {{ color: #f39c12; }}
        .poor {{ color: #e74c3c; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>KeyForge UAT测试报告</h1>
        <p>测试名称: {report.TestName}</p>
        <p>执行时间: {report.ExecutionTime}</p>
    </div>
    
    <div class=""section"">
        <h2>用户体验评分</h2>
        <div class=""score {GetScoreClass(report.UserExperienceScore)}"">
            {report.UserExperienceScore:F1}/100
        </div>
    </div>
    
    <div class=""section"">
        <h2>性能指标</h2>
        <table>
            <tr><th>操作</th><th>平均响应时间(ms)</th><th>状态</th></tr>
            {GeneratePerformanceRows(report.PerformanceMetrics)}
        </table>
    </div>
    
    <div class=""section"">
        <h2>用户操作记录</h2>
        <table>
            <tr><th>时间</th><th>类型</th><th>描述</th></tr>
            {GenerateActionRows(report.UserActions)}
        </table>
    </div>
</body>
</html>";

        var htmlPath = Path.Combine(_uatTestDirectory, "UAT-Report.html");
        File.WriteAllText(htmlPath, html);
        Log($"UAT HTML报告已生成: {htmlPath}");
    }

    private string GetScoreClass(double score)
    {
        if (score >= 80) return "excellent";
        if (score >= 60) return "good";
        return "poor";
    }

    private string GeneratePerformanceRows(Dictionary<string, double> metrics)
    {
        var rows = new List<string>();
        foreach (var metric in metrics)
        {
            var status = metric.Value < 1000 ? "success" : metric.Value < 3000 ? "warning" : "error";
            rows.Add($"<tr><td>{metric.Key}</td><td>{metric.Value:F1}</td><td class=\"{status}\">{GetStatusText(metric.Value)}</td></tr>");
        }
        return string.Join("", rows);
    }

    private string GenerateActionRows(List<UserActionRecord> actions)
    {
        var rows = new List<string>();
        foreach (var action in actions)
        {
            rows.Add($"<tr><td>{action.ActionTime:HH:mm:ss}</td><td>{action.ActionType}</td><td>{action.Description}</td></tr>");
        }
        return string.Join("", rows);
    }

    private string GetStatusText(double value)
    {
        if (value < 1000) return "优秀";
        if (value < 3000) return "良好";
        return "需要优化";
    }

    #endregion
}

/// <summary>
/// 用户体验指标
/// </summary>
public class UserExperienceMetrics
{
    private readonly Dictionary<string, List<double>> _stepTimings = new();
    private readonly List<double> _userResponseTimes = new();
    private readonly Dictionary<string, double> _performanceMetrics = new();

    public void RecordStepTiming(string stepType, double milliseconds)
    {
        if (!_stepTimings.ContainsKey(stepType))
        {
            _stepTimings[stepType] = new List<double>();
        }
        _stepTimings[stepType].Add(milliseconds);
    }

    public void RecordUserResponseTime(double milliseconds)
    {
        _userResponseTimes.Add(milliseconds);
    }

    public void RecordPerformanceMetric(string operation, double milliseconds)
    {
        _performanceMetrics[operation] = milliseconds;
    }

    public double GetAverageResponseTime(string operation)
    {
        return _stepTimings.TryGetValue(operation, out var timings) 
            ? timings.Average() 
            : 0;
    }

    public double GetPerformanceMetric(string operation)
    {
        return _performanceMetrics.TryGetValue(operation, out var metric) ? metric : 0;
    }

    public bool EvaluateExperience(string aspect)
    {
        return aspect switch
        {
            "响应速度" => GetAverageResponseTime("When") < 2000,
            "操作流畅度" => _userResponseTimes.Average() < 1500,
            "界面反应" => GetAverageResponseTime("Then") < 1000,
            _ => true
        };
    }

    public double CalculateOverallScore()
    {
        var responseSpeedScore = Math.Max(0, 100 - GetAverageResponseTime("When") / 50);
        var operationSmoothnessScore = Math.Max(0, 100 - _userResponseTimes.Average() / 30);
        var interfaceResponseScore = Math.Max(0, 100 - GetAverageResponseTime("Then") / 20);
        
        return (responseSpeedScore + operationSmoothnessScore + interfaceResponseScore) / 3;
    }

    public Dictionary<string, double> GetPerformanceMetrics()
    {
        return new Dictionary<string, double>(_performanceMetrics);
    }
}

/// <summary>
/// 用户操作记录
/// </summary>
public class UserActionRecord
{
    public Guid ActionId { get; set; }
    public DateTime ActionTime { get; set; }
    public string ActionType { get; set; }
    public string Description { get; set; }
    public string TestContext { get; set; }
}

/// <summary>
/// UAT测试报告
/// </summary>
public class UATReport
{
    public string TestName { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int TotalSteps { get; set; }
    public Dictionary<string, double> PerformanceMetrics { get; set; }
    public double UserExperienceScore { get; set; }
    public List<UserActionRecord> UserActions { get; set; }
}