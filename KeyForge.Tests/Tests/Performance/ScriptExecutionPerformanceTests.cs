using Xunit;
using FluentAssertions;
using System.Diagnostics;
using KeyForge.Core;
using KeyForge.Domain;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Performance;

/// <summary>
/// 脚本执行性能测试
/// 原本实现：复杂的性能基准测试
/// 简化实现：基础的性能验证
/// </summary>
public class ScriptExecutionPerformanceTests : TestBase
{
    private readonly ScriptPlayer _scriptPlayer;

    public ScriptExecutionPerformanceTests(ITestOutputHelper output) : base(output)
    {
        var mockExecutor = MockHelpers.CreateMockExecutor();
        _scriptPlayer = new ScriptPlayer(mockExecutor.Object);
    }

    [Fact]
    public async Task ExecuteLargeScript_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var script = TestFixtures.CreateLargeScript(1000); // 1000个动作
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _scriptPlayer.PlayAsync(script);
        
        stopwatch.Stop();

        // Assert
        result.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5秒内完成
        Log($"大型脚本执行时间: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void MemoryUsage_DuringScriptExecution_ShouldBeReasonable()
    {
        // Arrange
        var script = TestFixtures.CreateLargeScript(500);
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        var result = _scriptPlayer.PlayAsync(script).GetAwaiter().GetResult();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        result.Should().BeTrue();
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // 小于50MB
        Log($"内存使用增加: {memoryIncrease / 1024 / 1024}MB");
    }

    [Fact]
    public async Task ExecuteMultipleScripts_WithParallelExecution_ShouldBeEfficient()
    {
        // Arrange
        var scripts = TestFixtures.CreateMultipleScripts(10);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = scripts.Select(script => _scriptPlayer.PlayAsync(script));
        var results = await Task.WhenAll(tasks);
        
        stopwatch.Stop();

        // Assert
        results.Should().AllBeEquivalentTo(true);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // 10秒内完成
        Log($"并行执行{scripts.Count}个脚本时间: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ScriptPlayer_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _scriptPlayer.PlayAsync(script);
        
        stopwatch.Stop();

        // Assert
        result.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // 1秒内完成
        Log($"脚本播放器响应时间: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConsecutiveExecution_WithoutMemoryLeak_ShouldBeStable()
    {
        // Arrange
        var script = TestFixtures.CreateScriptWithActions(100);
        var executionTimes = new List<long>();
        var memoryUsages = new List<long>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            var initialMemory = GC.GetTotalMemory(true);
            
            var stopwatch = Stopwatch.StartNew();
            var result = await _scriptPlayer.PlayAsync(script);
            stopwatch.Stop();
            
            var finalMemory = GC.GetTotalMemory(false);
            
            executionTimes.Add(stopwatch.ElapsedMilliseconds);
            memoryUsages.Add(finalMemory - initialMemory);
            
            Log($"第{i+1}次执行: {stopwatch.ElapsedMilliseconds}ms, 内存增加: {(finalMemory - initialMemory) / 1024}KB");
        }

        // Assert
        executionTimes.Average().Should().BeLessThan(1000); // 平均执行时间小于1秒
        memoryUsages.Average().Should().BeLessThan(1024 * 1024); // 平均内存增加小于1MB
        Log($"连续执行稳定性测试完成");
    }
}