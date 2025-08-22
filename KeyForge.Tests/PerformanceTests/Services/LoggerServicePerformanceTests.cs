using Xunit;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;
using BenchmarkDotNet.Attributes;

namespace KeyForge.Tests.PerformanceTests.Services;

/// <summary>
/// LoggerService 性能测试
/// 原本实现：完整的日志服务性能测试
/// 简化实现：核心日志性能测试
/// </summary>
public class LoggerServicePerformanceTests : TestBase, IDisposable
{
    private readonly string _testLogDirectory;
    private readonly LoggerService _logger;

    public LoggerServicePerformanceTests(ITestOutputHelper output) : base(output)
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_LogPerfTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _logger = new LoggerService(_testLogDirectory);
        
        Log($"日志性能测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"日志性能测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public void LogInfo_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志记录响应时间
        
        var logCount = 1000;
        var messages = new string[logCount];
        
        // 生成测试消息
        for (int i = 0; i < logCount; i++)
        {
            messages[i] = $"Test info message {i}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo(messages[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条信息日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条日志记录时间应该小于0.5ms
        averageTime.Should().BeLessThan(0.5);
    }

    [Fact]
    public void LogError_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 错误日志记录响应时间
        
        var logCount = 1000;
        var exceptions = new Exception[logCount];
        
        // 生成测试异常
        for (int i = 0; i < logCount; i++)
        {
            exceptions[i] = new Exception($"Test exception {i}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogError($"Error message {i}", exceptions[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条错误日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条错误日志记录时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void LogWarning_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 警告日志记录响应时间
        
        var logCount = 1000;
        var messages = new string[logCount];
        
        // 生成测试消息
        for (int i = 0; i < logCount; i++)
        {
            messages[i] = $"Test warning message {i}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogWarning(messages[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条警告日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条警告日志记录时间应该小于0.5ms
        averageTime.Should().BeLessThan(0.5);
    }

    [Fact]
    public void LogDebug_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 调试日志记录响应时间
        
        var logCount = 1000;
        var messages = new string[logCount];
        
        // 生成测试消息
        for (int i = 0; i < logCount; i++)
        {
            messages[i] = $"Test debug message {i}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogDebug(messages[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条调试日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条调试日志记录时间应该小于0.5ms
        averageTime.Should().BeLessThan(0.5);
    }

    [Fact]
    public async Task AsyncLogging_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 异步日志记录性能
        
        var logCount = 1000;
        var messages = new string[logCount];
        
        // 生成测试消息
        for (int i = 0; i < logCount; i++)
        {
            messages[i] = $"Test async message {i}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = new Task[logCount];
        for (int i = 0; i < logCount; i++)
        {
            tasks[i] = _logger.LogInfoAsync(messages[i]);
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"异步记录 {logCount} 条日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条异步日志记录时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void LargeMessageLogging_Performance_ShouldBeAcceptable()
    {
        // 基于验收标准 AC-NONFUNC-001: 大消息日志记录性能
        
        var largeMessage = new string('A', 10000); // 10KB message
        var logCount = 100;
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"{largeMessage} - Message {i}");
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条大消息日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 每条大消息日志记录时间应该小于10ms
        averageTime.Should().BeLessThan(10);
    }

    [Fact]
    public void ConcurrentLogging_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 并发日志记录性能
        
        var operationCount = 1000;
        var successCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, operationCount, i =>
        {
            try
            {
                _logger.LogInfo($"Concurrent log message {i}");
                Interlocked.Increment(ref successCount);
            }
            catch (Exception ex)
            {
                LogError($"并发日志记录失败: {ex.Message}");
            }
        });
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)operationCount;
        var successRate = successCount / (double)operationCount;
        
        Log($"并发记录 {operationCount} 条日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        Log($"成功率: {successRate:P2}");
        
        // 并发记录时间应该小于1ms每条日志
        averageTime.Should().BeLessThan(1);
        successRate.Should().BeGreaterThan(0.95);
    }

    [Fact]
    public void LogFileRetrieval_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志文件检索性能
        
        var logCount = 100;
        
        // 先记录一些日志
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"Test message {i}");
        }
        
        var retrievalCount = 1000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < retrievalCount; i++)
        {
            var logFiles = _logger.GetLogFiles();
            logFiles.Should().NotBeNull();
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)retrievalCount;
        
        Log($"检索 {retrievalCount} 次日志文件总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均检索时间: {averageTime:F6}ms 每次检索");
        
        // 每次检索时间应该小于0.01ms
        averageTime.Should().BeLessThan(0.01);
    }

    [Fact]
    public void LogSizeCalculation_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志大小计算性能
        
        var logCount = 100;
        
        // 先记录一些日志
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"Test message {i}");
        }
        
        var calculationCount = 1000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < calculationCount; i++)
        {
            var size = _logger.GetLogSize();
            size.Should().BeGreaterThanOrEqualTo(0);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)calculationCount;
        
        Log($"计算 {calculationCount} 次日志大小总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均计算时间: {averageTime:F6}ms 每次");
        
        // 每次计算时间应该小于0.01ms
        averageTime.Should().BeLessThan(0.01);
    }

    [Fact]
    public void LogCleanup_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志清理性能
        
        var logCount = 1000;
        
        // 先记录一些日志
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"Test message {i}");
        }
        
        var cleanupCount = 100;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < cleanupCount; i++)
        {
            _logger.ClearLogs();
            
            // 重新记录日志以便下次清理
            for (int j = 0; j < 10; j++)
            {
                _logger.LogInfo($"Test message {i}-{j}");
            }
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)cleanupCount;
        
        Log($"清理 {cleanupCount} 次日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均清理时间: {averageTime:F4}ms 每次");
        
        // 每次清理时间应该小于5ms
        averageTime.Should().BeLessThan(5);
    }

    [Fact]
    public void MemoryUsage_DuringLogging_ShouldBeReasonable()
    {
        // 基于验收标准 AC-NONFUNC-001: 内存使用
        
        var initialMemory = GC.GetTotalMemory(true);
        var logCount = 10000;
        
        // 记录大量日志
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"Memory test message {i}");
        }
        
        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;
        
        Log($"初始内存: {initialMemory / 1024 / 1024:F2}MB");
        Log($"峰值内存: {peakMemory / 1024 / 1024:F2}MB");
        Log($"内存增长: {memoryIncrease / 1024 / 1024:F2}MB");
        
        // 内存增长应该小于20MB
        memoryIncrease.Should().BeLessThan(20 * 1024 * 1024);
        
        // 清理
        _logger.ClearLogs();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;
        
        Log($"清理后内存: {finalMemory / 1024 / 1024:F2}MB");
        Log($"清理后内存增长: {memoryAfterCleanup / 1024 / 1024:F2}MB");
        
        // 清理后内存增长应该很小
        memoryAfterCleanup.Should().BeLessThan(1 * 1024 * 1024);
    }

    [Fact]
    public void StressTest_Limit_ShouldBeIdentified()
    {
        // 基于验收标准 AC-NONFUNC-001: 压力测试
        
        var maxConcurrentLogs = 100;
        var successCount = 0;
        var failureCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, maxConcurrentLogs, i =>
        {
            try
            {
                _logger.LogInfo($"Stress test log message {i}");
                Interlocked.Increment(ref successCount);
            }
            catch
            {
                Interlocked.Increment(ref failureCount);
            }
        });
        
        stopwatch.Stop();
        
        var successRate = successCount / (double)(successCount + failureCount);
        
        Log($"压力测试结果:");
        Log($"  并发日志数: {maxConcurrentLogs}");
        Log($"  成功数: {successCount}");
        Log($"  失败数: {failureCount}");
        Log($"  成功率: {successRate:P2}");
        Log($"  总时间: {stopwatch.ElapsedMilliseconds}ms");
        
        // 成功率应该大于95%
        successRate.Should().BeGreaterThan(0.95);
        
        // 总时间应该小于5秒
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public void LongTermStability_ShouldBeMaintained()
    {
        // 基于验收标准 AC-NONFUNC-001: 长期稳定性
        
        var testDuration = TimeSpan.FromMinutes(2);
        var operations = new List<long>();
        var memorySamples = new List<long>();
        
        var endTime = DateTime.UtcNow + testDuration;
        var operationCount = 0L;
        
        while (DateTime.UtcNow < endTime)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 执行典型操作
            _logger.LogInfo($"Long term test message {operationCount}");
            
            stopwatch.Stop();
            
            operations.Add(stopwatch.ElapsedMilliseconds);
            operationCount++;
            
            // 定期采样内存使用
            if (operationCount % 100 == 0)
            {
                memorySamples.Add(GC.GetTotalMemory(false));
            }
            
            // 定期清理
            if (operationCount % 1000 == 0)
            {
                _logger.ClearLogs();
            }
            
            // 短暂延迟避免过度占用CPU
            System.Threading.Thread.Sleep(1);
        }
        
        var averageOperationTime = operations.Average();
        var maxOperationTime = operations.Max();
        var finalMemory = GC.GetTotalMemory(true);
        
        Log($"长期稳定性测试结果:");
        Log($"  测试持续时间: {testDuration.TotalMinutes:F1} 分钟");
        Log($"  操作总数: {operationCount}");
        Log($"  平均操作时间: {averageOperationTime:F4}ms");
        Log($"  最大操作时间: {maxOperationTime:F2}ms");
        Log($"  最终内存使用: {finalMemory / 1024 / 1024:F2}MB");
        Log($"  吞吐量: {operationCount / testDuration.TotalSeconds:F2} 操作/秒");
        
        // 长期稳定性指标
        averageOperationTime.Should().BeLessThan(0.5); // 平均操作时间小于0.5ms
        maxOperationTime.Should().BeLessThan(5); // 最大操作时间小于5ms
        (operationCount / testDuration.TotalSeconds).Should().BeGreaterThan(100); // 吞吐量大于100操作/秒
    }

    [Fact]
    public void DifferentLogLevels_Performance_ShouldBeConsistent()
    {
        // 基于验收标准 AC-NONFUNC-001: 不同日志级别性能
        
        var logActions = new[]
        {
            (Action<string>)((msg) => _logger.LogInfo(msg)),
            (Action<string>)((msg) => _logger.LogWarning(msg)),
            (Action<string>)((msg) => _logger.LogDebug(msg))
        };
        
        var logLevels = new[] { "Info", "Warning", "Debug" };
        var iterationsPerLevel = 100;
        var levelPerformance = new Dictionary<string, double>();
        
        for (int i = 0; i < logActions.Length; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            
            for (int j = 0; j < iterationsPerLevel; j++)
            {
                logActions[i]($"Test message {j}");
            }
            
            stopwatch.Stop();
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterationsPerLevel;
            levelPerformance[logLevels[i]] = averageTime;
            
            Log($"{logLevels[i]} 平均记录时间: {averageTime:F4}ms");
        }
        
        // 所有日志级别的处理时间应该一致（差异小于50%）
        var minTime = levelPerformance.Values.Min();
        var maxTime = levelPerformance.Values.Max();
        var variation = (maxTime - minTime) / minTime;
        
        variation.Should().BeLessThan(0.5);
        Log($"日志级别处理时间 variation: {variation:P2}");
    }

    [Fact]
    public void FileRotation_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 文件轮换性能
        
        var logCount = 1000;
        
        var stopwatch = Stopwatch.StartNew();
        
        // 记录大量日志以触发文件轮换
        for (int i = 0; i < logCount; i++)
        {
            _logger.LogInfo($"Rotation test message {i}");
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)logCount;
        
        Log($"记录 {logCount} 条日志（含轮换）总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均记录时间: {averageTime:F4}ms 每条日志");
        
        // 包含轮换的每条日志记录时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }
}

/// <summary>
/// LoggerService 基准测试
/// </summary>
[MemoryDiagnoser]
public class LoggerServiceBenchmarks
{
    private readonly LoggerService _logger;
    private readonly string _testLogDirectory;

    public LoggerServiceBenchmarks()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Benchmark_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _logger = new LoggerService(_testLogDirectory);
    }

    [Benchmark]
    public void LogInfo()
    {
        _logger.LogInfo($"Benchmark info message {DateTime.Now.Ticks}");
    }

    [Benchmark]
    public void LogWarning()
    {
        _logger.LogWarning($"Benchmark warning message {DateTime.Now.Ticks}");
    }

    [Benchmark]
    public void LogDebug()
    {
        _logger.LogDebug($"Benchmark debug message {DateTime.Now.Ticks}");
    }

    [Benchmark]
    public void LogError()
    {
        _logger.LogError($"Benchmark error message {DateTime.Now.Ticks}", new Exception("Benchmark exception"));
    }

    [Benchmark]
    public async Task LogInfoAsync()
    {
        await _logger.LogInfoAsync($"Benchmark async message {DateTime.Now.Ticks}");
    }

    [Benchmark]
    public void GetLogFiles()
    {
        _logger.GetLogFiles();
    }

    [Benchmark]
    public void GetLogSize()
    {
        _logger.GetLogSize();
    }

    [Benchmark]
    public void ClearLogs()
    {
        _logger.ClearLogs();
    }
}