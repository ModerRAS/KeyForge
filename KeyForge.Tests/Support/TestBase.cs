using System.Text.Json;
using KeyForge.Domain;
using KeyForge.Core;

namespace KeyForge.Tests.Support;

/// <summary>
/// 测试基类
/// 原本实现：复杂的测试基类
/// 简化实现：基础的测试支持功能
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly CancellationTokenSource CancellationTokenSource;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
        CancellationTokenSource = new CancellationTokenSource();
    }

    protected void Log(string message)
    {
        Output.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }

    protected void LogError(string message)
    {
        Output.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}");
    }

    protected void LogWarning(string message)
    {
        Output.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] WARNING: {message}");
    }

    protected void AssertScriptIsValid(Script script)
    {
        script.Should().NotBeNull();
        script.Id.Should().NotBeNullOrEmpty();
        script.Name.Should().NotBeNullOrEmpty();
        script.Status.Should().Be(ScriptStatus.Created);
    }

    protected void AssertExecutionResult(ExecutionResult result, bool expectedSuccess)
    {
        result.Should().NotBeNull();
        result.Success.Should().Be(expectedSuccess);
        
        if (expectedSuccess)
        {
            result.ExecutionTime.Should().BeGreaterThan(0);
            result.ExecutedActions.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    protected void AssertRecognitionResult(RecognitionResult result, bool expectedFound)
    {
        result.Should().NotBeNull();
        result.Found.Should().Be(expectedFound);
        result.ProcessingTime.Should().BeGreaterThan(0);
        
        if (expectedFound)
        {
            result.Confidence.Should().BeGreaterThan(0);
            result.Location.Should().NotBeNull();
        }
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 测试报告生成器
/// 原本实现：复杂的报告生成
/// 简化实现：基础的JSON报告生成
/// </summary>
public static class TestReportGenerator
{
    public static void GenerateReport(TestResults results)
    {
        var report = new TestReport
        {
            Timestamp = DateTime.Now,
            TotalTests = results.Total,
            PassedTests = results.Passed,
            FailedTests = results.Failed,
            SkippedTests = results.Skipped,
            ExecutionTime = results.Duration,
            Coverage = results.Coverage
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText("TestResults/test-report.json", json);
        
        // 生成简单的HTML报告
        GenerateHtmlReport(report);
    }

    private static void GenerateHtmlReport(TestReport report)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>KeyForge 测试报告</title>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .section {{ margin: 20px 0; }}
        .success {{ color: green; }}
        .error {{ color: red; }}
        .warning {{ color: orange; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>KeyForge 测试报告</h1>
        <p>生成时间: {report.Timestamp}</p>
    </div>
    
    <div class=""section"">
        <h2>测试摘要</h2>
        <table>
            <tr><th>总测试数</th><td>{report.TotalTests}</td></tr>
            <tr><th>通过</th><td class=""success"">{report.PassedTests}</td></tr>
            <tr><th>失败</th><td class=""error"">{report.FailedTests}</td></tr>
            <tr><th>跳过</th><td class=""warning"">{report.SkippedTests}</td></tr>
            <tr><th>执行时间</th><td>{report.ExecutionTime}ms</td></tr>
            <tr><th>覆盖率</th><td>{report.Coverage}%</td></tr>
        </table>
    </div>
</body>
</html>";

        File.WriteAllText("TestResults/test-report.html", html);
    }
}

/// <summary>
/// 测试结果数据结构
/// </summary>
public class TestResults
{
    public int Total { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public long Duration { get; set; }
    public double Coverage { get; set; }
}

/// <summary>
/// 测试报告数据结构
/// </summary>
public class TestReport
{
    public DateTime Timestamp { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public long ExecutionTime { get; set; }
    public double Coverage { get; set; }
}