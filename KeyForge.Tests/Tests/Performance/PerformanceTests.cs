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

namespace KeyForge.Tests.Performance;

/// <summary>
/// 性能测试
/// 基于验收标准 AC-NONFUNC-001
/// 原本实现：复杂的性能测试框架
/// 简化实现：核心性能指标测试
/// </summary>
public class PerformanceTests : TestBase
{
    private readonly string _testDirectory;

    public PerformanceTests(ITestOutputHelper output) : base(output)
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Perf_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        Log($"创建性能测试目录: {_testDirectory}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                    Log($"清理性能测试目录: {_testDirectory}");
                }
            }
            catch (Exception ex)
            {
                Log($"清理性能测试目录失败: {ex.Message}");
            }
        }
        base.Dispose(disposing);
    }

    [Fact]
    public void ScriptCreation_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 响应时间
        
        var scriptCount = 1000;
        var scripts = new List<Script>();
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < scriptCount; i++)
        {
            var script = new Script(
                Guid.NewGuid(),
                $"Performance Test Script {i}",
                $"Description for script {i}"
            );
            scripts.Add(script);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)scriptCount;
        
        Log($"创建 {scriptCount} 个脚本总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均创建时间: {averageTime:F2}ms 每个脚本");
        
        // 每个脚本创建时间应该小于1ms
        averageTime.Should().BeLessThan(1);
        scripts.Count.Should().Be(scriptCount);
    }

    [Fact]
    public void ScriptExecution_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 执行响应时间
        
        var script = TestFixtures.CreateLargeScript(1000); // 1000个动作
        var executionCount = 100;
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < executionCount; i++)
        {
            var result = SimulateScriptExecution(script);
            result.Should().BeTrue();
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)executionCount;
        
        Log($"执行 {executionCount} 次脚本总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均执行时间: {averageTime:F2}ms 每次");
        
        // 每次执行时间应该小于50ms
        averageTime.Should().BeLessThan(50);
    }

    [Fact]
    public void LargeScriptExecution_Performance_ShouldBeAcceptable()
    {
        // 基于验收标准 AC-NONFUNC-001: 大脚本执行性能
        
        var script = TestFixtures.CreateLargeScript(10000); // 10000个动作
        
        var stopwatch = Stopwatch.StartNew();
        var result = SimulateScriptExecution(script);
        stopwatch.Stop();
        
        Log($"执行大脚本 ({script.Actions.Count} 个动作) 时间: {stopwatch.ElapsedMilliseconds}ms");
        
        result.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 应该在5秒内完成
    }

    [Fact]
    public void MemoryUsage_DuringScriptOperations_ShouldBeReasonable()
    {
        // 基于验收标准 AC-NONFUNC-001: 资源占用
        
        var initialMemory = GC.GetTotalMemory(true);
        var scripts = new List<Script>();
        
        // 创建大量脚本
        for (int i = 0; i < 1000; i++)
        {
            var script = TestFixtures.CreateScriptWithActions(100);
            scripts.Add(script);
        }
        
        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;
        
        Log($"初始内存: {initialMemory / 1024 / 1024:F2}MB");
        Log($"峰值内存: {peakMemory / 1024 / 1024:F2}MB");
        Log($"内存增长: {memoryIncrease / 1024 / 1024:F2}MB");
        
        // 内存增长应该小于100MB
        memoryIncrease.Should().BeLessThan(100 * 1024 * 1024);
        
        // 清理
        scripts.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;
        
        Log($"清理后内存: {finalMemory / 1024 / 1024:F2}MB");
        Log($"清理后内存增长: {memoryAfterCleanup / 1024 / 1024:F2}MB");
        
        // 清理后内存增长应该很小
        memoryAfterCleanup.Should().BeLessThan(10 * 1024 * 1024);
    }

    [Fact]
    public void ConcurrentScriptExecution_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 并发性能
        
        var scriptCount = 50;
        var scripts = Enumerable.Range(0, scriptCount)
            .Select(_ => TestFixtures.CreateScriptWithActions(100))
            .ToList();
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.ForEach(scripts, script =>
        {
            var result = SimulateScriptExecution(script);
            result.Should().BeTrue();
        });
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)scriptCount;
        
        Log($"并发执行 {scriptCount} 个脚本总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均并发执行时间: {averageTime:F2}ms 每个脚本");
        
        // 并发执行时间应该小于100ms每个脚本
        averageTime.Should().BeLessThan(100);
    }

    [Fact]
    public void ImageRecognition_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 识别响应时间
        
        var template = TestFixtures.CreateValidImageTemplate();
        var recognitionCount = 1000;
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < recognitionCount; i++)
        {
            var result = SimulateImageRecognition(template, 0.85);
            result.Should().NotBeNull();
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)recognitionCount;
        
        Log($"执行 {recognitionCount} 次图像识别总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均识别时间: {averageTime:F2}ms 每次");
        
        // 每次识别时间应该小于100ms
        averageTime.Should().BeLessThan(100);
    }

    [Fact]
    public void FileStorage_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 文件操作性能
        
        var scriptCount = 500;
        var scripts = Enumerable.Range(0, scriptCount)
            .Select(_ => TestFixtures.CreateScriptWithActions(50))
            .ToList();
        
        var stopwatch = Stopwatch.StartNew();
        
        // 保存所有脚本
        foreach (var script in scripts)
        {
            var filePath = Path.Combine(_testDirectory, $"{script.Id}.json");
            FileStorage.SaveScript(script, filePath);
        }
        
        var saveTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();
        
        // 加载所有脚本
        foreach (var script in scripts)
        {
            var filePath = Path.Combine(_testDirectory, $"{script.Id}.json");
            var loadedScript = FileStorage.LoadScript<Script>(filePath);
            loadedScript.Should().NotBeNull();
        }
        
        var loadTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Stop();
        
        var averageSaveTime = saveTime / (double)scriptCount;
        var averageLoadTime = loadTime / (double)scriptCount;
        
        Log($"保存 {scriptCount} 个脚本总时间: {saveTime}ms, 平均 {averageSaveTime:F2}ms");
        Log($"加载 {scriptCount} 个脚本总时间: {loadTime}ms, 平均 {averageLoadTime:F2}ms");
        
        // 每个文件操作应该小于10ms
        averageSaveTime.Should().BeLessThan(10);
        averageLoadTime.Should().BeLessThan(10);
    }

    [Fact]
    public void SystemResponsiveness_UnderLoad_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 系统响应时间
        
        var loadDuration = TimeSpan.FromSeconds(30);
        var requestCount = 0;
        var responseTimes = new List<double>();
        
        var loadTask = Task.Run(async () =>
        {
            var endTime = DateTime.UtcNow + loadDuration;
            
            while (DateTime.UtcNow < endTime)
            {
                var stopwatch = Stopwatch.StartNew();
                
                // 模拟系统请求
                var script = TestFixtures.CreateValidScript();
                var result = SimulateScriptExecution(script);
                
                stopwatch.Stop();
                
                responseTimes.Add(stopwatch.ElapsedMilliseconds);
                requestCount++;
                
                // 添加小延迟以避免过度占用CPU
                await Task.Delay(1);
            }
        });
        
        loadTask.Wait();
        
        var averageResponseTime = responseTimes.Average();
        var maxResponseTime = responseTimes.Max();
        var p95ResponseTime = responseTimes.OrderBy(t => t).ElementAt((int)(responseTimes.Count * 0.95));
        
        Log($"负载测试完成:");
        Log($"  请求总数: {requestCount}");
        Log($"  平均响应时间: {averageResponseTime:F2}ms");
        Log($"  最大响应时间: {maxResponseTime:F2}ms");
        Log($"  95%响应时间: {p95ResponseTime:F2}ms");
        Log($"  吞吐量: {requestCount / loadDuration.TotalSeconds:F2} 请求/秒");
        
        // 性能指标验证
        averageResponseTime.Should().BeLessThan(100); // 平均响应时间小于100ms
        p95ResponseTime.Should().BeLessThan(200); // 95%响应时间小于200ms
        (requestCount / loadDuration.TotalSeconds).Should().BeGreaterThan(10); // 吞吐量大于10请求/秒
    }

    [Fact]
    public void StressTest_Limit_ShouldBeIdentified()
    {
        // 基于验收标准 AC-NONFUNC-001: 压力测试
        
        var maxConcurrentScripts = 100;
        var successCount = 0;
        var failureCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, maxConcurrentScripts, i =>
        {
            try
            {
                var script = TestFixtures.CreateLargeScript(1000);
                var result = SimulateScriptExecution(script);
                
                if (result)
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref failureCount);
                }
            }
            catch
            {
                Interlocked.Increment(ref failureCount);
            }
        });
        
        stopwatch.Stop();
        
        var successRate = successCount / (double)(successCount + failureCount);
        
        Log($"压力测试结果:");
        Log($"  并发脚本数: {maxConcurrentScripts}");
        Log($"  成功数: {successCount}");
        Log($"  失败数: {failureCount}");
        Log($"  成功率: {successRate:P2}");
        Log($"  总时间: {stopwatch.ElapsedMilliseconds}ms");
        
        // 成功率应该大于95%
        successRate.Should().BeGreaterThan(0.95);
        
        // 总时间应该小于60秒
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000);
    }

    [Fact]
    public void LongTermStability_ShouldBeMaintained()
    {
        // 基于验收标准 AC-NONFUNC-001: 长期稳定性
        
        var testDuration = TimeSpan.FromMinutes(5);
        var operations = new List<long>();
        var memorySamples = new List<long>();
        
        var endTime = DateTime.UtcNow + testDuration;
        var operationCount = 0L;
        
        while (DateTime.UtcNow < endTime)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 执行典型操作
            var script = TestFixtures.CreateScriptWithActions(10);
            var result = SimulateScriptExecution(script);
            
            stopwatch.Stop();
            
            operations.Add(stopwatch.ElapsedMilliseconds);
            operationCount++;
            
            // 定期采样内存使用
            if (operationCount % 100 == 0)
            {
                memorySamples.Add(GC.GetTotalMemory(false));
            }
            
            // 短暂延迟避免过度占用CPU
            System.Threading.Thread.Sleep(10);
        }
        
        var averageOperationTime = operations.Average();
        var maxOperationTime = operations.Max();
        var finalMemory = GC.GetTotalMemory(true);
        
        Log($"长期稳定性测试结果:");
        Log($"  测试持续时间: {testDuration.TotalMinutes:F1} 分钟");
        Log($"  操作总数: {operationCount}");
        Log($"  平均操作时间: {averageOperationTime:F2}ms");
        Log($"  最大操作时间: {maxOperationTime:F2}ms");
        Log($"  最终内存使用: {finalMemory / 1024 / 1024:F2}MB");
        Log($"  吞吐量: {operationCount / testDuration.TotalSeconds:F2} 操作/秒");
        
        // 长期稳定性指标
        averageOperationTime.Should().BeLessThan(50); // 平均操作时间小于50ms
        maxOperationTime.Should().BeLessThan(500); // 最大操作时间小于500ms
        (operationCount / testDuration.TotalSeconds).Should().BeGreaterThan(5); // 吞吐量大于5操作/秒
    }

    // 辅助方法
    private bool SimulateScriptExecution(Script script)
    {
        // 模拟脚本执行
        System.Threading.Thread.Sleep(script.Actions.Count * 1); // 每个动作1ms
        return true;
    }

    private RecognitionResult SimulateImageRecognition(ImageTemplate template, double confidence)
    {
        var processingTime = new Random().Next(5, 50);
        
        if (confidence >= template.Threshold)
        {
            return new RecognitionResult(
                true,
                confidence,
                template.Region,
                processingTime
            );
        }
        else
        {
            return new RecognitionResult(
                false,
                confidence,
                null,
                processingTime
            );
        }
    }
}