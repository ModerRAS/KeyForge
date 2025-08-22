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
/// ErrorHandlerManager 性能测试
/// 原本实现：完整的错误处理管理器性能测试
/// 简化实现：核心错误处理性能测试
/// </summary>
public class ErrorHandlerManagerPerformanceTests : TestBase, IDisposable
{
    private readonly string _testLogDirectory;
    private readonly ErrorHandlerManager _errorHandler;
    private readonly LoggerService _logger;

    public ErrorHandlerManagerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_ErrorPerfTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _logger = new LoggerService(_testLogDirectory);
        _errorHandler = new ErrorHandlerManager(_logger, _testLogDirectory);
        
        Log($"错误处理性能测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            _errorHandler?.Dispose();
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"错误处理性能测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public void ErrorHandling_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 错误处理响应时间
        
        var errorCount = 1000;
        var exceptions = new Exception[errorCount];
        
        // 生成测试异常
        for (int i = 0; i < errorCount; i++)
        {
            exceptions[i] = new Exception($"Test exception {i}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(exceptions[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)errorCount;
        
        Log($"处理 {errorCount} 个错误总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F4}ms 每个错误");
        
        // 每个错误处理时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void ErrorHandlingWithCustomMessage_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 自定义消息错误处理响应时间
        
        var errorCount = 1000;
        var exceptions = new Exception[errorCount];
        var messages = new string[errorCount];
        
        // 生成测试异常和消息
        for (int i = 0; i < errorCount; i++)
        {
            exceptions[i] = new Exception($"Test exception {i}");
            messages[i] = $"Custom message {i}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(exceptions[i], messages[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)errorCount;
        
        Log($"处理 {errorCount} 个带自定义消息错误总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F4}ms 每个错误");
        
        // 每个错误处理时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void AggregateErrorHandling_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 聚合错误处理响应时间
        
        var errorCount = 100;
        var aggregateExceptions = new AggregateException[errorCount];
        
        // 生成测试聚合异常
        for (int i = 0; i < errorCount; i++)
        {
            var innerExceptions = new Exception[5];
            for (int j = 0; j < 5; j++)
            {
                innerExceptions[j] = new Exception($"Inner exception {i}-{j}");
            }
            aggregateExceptions[i] = new AggregateException(innerExceptions);
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(aggregateExceptions[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)errorCount;
        
        Log($"处理 {errorCount} 个聚合错误总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F4}ms 每个聚合错误");
        
        // 每个聚合错误处理时间应该小于5ms
        averageTime.Should().BeLessThan(5);
    }

    [Fact]
    public void ErrorLogRetrieval_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志检索响应时间
        
        var errorCount = 100;
        
        // 先添加一些错误
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(new Exception($"Test exception {i}"));
        }
        
        var retrievalCount = 1000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < retrievalCount; i++)
        {
            var errorLogs = _errorHandler.GetErrorLogs();
            errorLogs.Should().NotBeNull();
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)retrievalCount;
        
        Log($"检索 {retrievalCount} 次错误日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均检索时间: {averageTime:F4}ms 每次检索");
        
        // 每次检索时间应该小于0.1ms
        averageTime.Should().BeLessThan(0.1);
    }

    [Fact]
    public void ErrorCountRetrieval_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 计数检索响应时间
        
        var errorCount = 100;
        
        // 先添加一些错误
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(new Exception($"Test exception {i}"));
        }
        
        var retrievalCount = 10000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < retrievalCount; i++)
        {
            var count = _errorHandler.GetErrorCount();
            count.Should().BeGreaterThanOrEqualTo(0);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)retrievalCount;
        
        Log($"检索 {retrievalCount} 次错误计数总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均检索时间: {averageTime:F6}ms 每次检索");
        
        // 每次检索时间应该小于0.01ms
        averageTime.Should().BeLessThan(0.01);
    }

    [Fact]
    public void ConcurrentErrorHandling_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 并发错误处理性能
        
        var operationCount = 1000;
        var successCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, operationCount, i =>
        {
            try
            {
                var exception = new Exception($"Concurrent exception {i}");
                _errorHandler.HandleError(exception);
                Interlocked.Increment(ref successCount);
            }
            catch (Exception ex)
            {
                LogError($"并发错误处理失败: {ex.Message}");
            }
        });
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)operationCount;
        var successRate = successCount / (double)operationCount;
        
        Log($"并发处理 {operationCount} 个错误总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F4}ms 每个错误");
        Log($"成功率: {successRate:P2}");
        
        // 并发处理时间应该小于2ms每个错误
        averageTime.Should().BeLessThan(2);
        successRate.Should().BeGreaterThan(0.95);
    }

    [Fact]
    public void MemoryUsage_DuringErrorHandling_ShouldBeReasonable()
    {
        // 基于验收标准 AC-NONFUNC-001: 内存使用
        
        var initialMemory = GC.GetTotalMemory(true);
        var errorCount = 1000;
        
        // 处理大量错误
        for (int i = 0; i < errorCount; i++)
        {
            var exception = new Exception($"Memory test exception {i}");
            _errorHandler.HandleError(exception);
        }
        
        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;
        
        Log($"初始内存: {initialMemory / 1024 / 1024:F2}MB");
        Log($"峰值内存: {peakMemory / 1024 / 1024:F2}MB");
        Log($"内存增长: {memoryIncrease / 1024 / 1024:F2}MB");
        
        // 内存增长应该小于50MB
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024);
        
        // 清理
        _errorHandler.ClearErrorLogs();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;
        
        Log($"清理后内存: {finalMemory / 1024 / 1024:F2}MB");
        Log($"清理后内存增长: {memoryAfterCleanup / 1024 / 1024:F2}MB");
        
        // 清理后内存增长应该很小
        memoryAfterCleanup.Should().BeLessThan(5 * 1024 * 1024);
    }

    [Fact]
    public void LargeErrorHandling_Performance_ShouldBeAcceptable()
    {
        // 基于验收标准 AC-NONFUNC-001: 大错误处理性能
        
        var largeMessage = new string('A', 10000); // 10KB message
        var errorCount = 100;
        var exceptions = new Exception[errorCount];
        
        // 生成大消息异常
        for (int i = 0; i < errorCount; i++)
        {
            exceptions[i] = new Exception(largeMessage + $" Error {i}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(exceptions[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)errorCount;
        
        Log($"处理 {errorCount} 个大错误总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F4}ms 每个大错误");
        
        // 每个大错误处理时间应该小于10ms
        averageTime.Should().BeLessThan(10);
    }

    [Fact]
    public void ErrorLogCleanup_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 日志清理性能
        
        var errorCount = 1000;
        
        // 先添加一些错误
        for (int i = 0; i < errorCount; i++)
        {
            _errorHandler.HandleError(new Exception($"Test exception {i}"));
        }
        
        var cleanupCount = 100;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < cleanupCount; i++)
        {
            _errorHandler.ClearErrorLogs();
            
            // 重新添加错误以便下次清理
            for (int j = 0; j < 10; j++)
            {
                _errorHandler.HandleError(new Exception($"Test exception {i}-{j}"));
            }
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)cleanupCount;
        
        Log($"清理 {cleanupCount} 次错误日志总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均清理时间: {averageTime:F4}ms 每次");
        
        // 每次清理时间应该小于5ms
        averageTime.Should().BeLessThan(5);
    }

    [Fact]
    public void ErrorStatistics_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 统计性能
        
        var errorCount = 100;
        var exceptionTypes = new[]
        {
            typeof(ArgumentException),
            typeof(InvalidOperationException),
            typeof(NullReferenceException),
            typeof(TimeoutException),
            typeof(IOException)
        };
        
        // 添加不同类型的错误
        for (int i = 0; i < errorCount; i++)
        {
            var exceptionType = exceptionTypes[i % exceptionTypes.Length];
            var exception = (Exception)Activator.CreateInstance(exceptionType, $"Test exception {i}");
            _errorHandler.HandleError(exception);
        }
        
        var statisticsCount = 1000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < statisticsCount; i++)
        {
            var statistics = _errorHandler.GetErrorStatistics();
            statistics.Should().NotBeNull();
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)statisticsCount;
        
        Log($"计算 {statisticsCount} 次错误统计总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均计算时间: {averageTime:F4}ms 每次");
        
        // 每次统计计算时间应该小于0.1ms
        averageTime.Should().BeLessThan(0.1);
    }

    [Fact]
    public void StressTest_Limit_ShouldBeIdentified()
    {
        // 基于验收标准 AC-NONFUNC-001: 压力测试
        
        var maxConcurrentErrors = 100;
        var successCount = 0;
        var failureCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, maxConcurrentErrors, i =>
        {
            try
            {
                var exception = new Exception($"Stress test exception {i}");
                _errorHandler.HandleError(exception);
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
        Log($"  并发错误数: {maxConcurrentErrors}");
        Log($"  成功数: {successCount}");
        Log($"  失败数: {failureCount}");
        Log($"  成功率: {successRate:P2}");
        Log($"  总时间: {stopwatch.ElapsedMilliseconds}ms");
        
        // 成功率应该大于90%
        successRate.Should().BeGreaterThan(0.90);
        
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
            var exception = new Exception($"Long term test exception {operationCount}");
            _errorHandler.HandleError(exception);
            
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
                _errorHandler.ClearErrorLogs();
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
        averageOperationTime.Should().BeLessThan(1); // 平均操作时间小于1ms
        maxOperationTime.Should().BeLessThan(10); // 最大操作时间小于10ms
        (operationCount / testDuration.TotalSeconds).Should().BeGreaterThan(50); // 吞吐量大于50操作/秒
    }

    [Fact]
    public void DifferentExceptionTypes_Performance_ShouldBeConsistent()
    {
        // 基于验收标准 AC-NONFUNC-001: 不同异常类型性能
        
        var exceptionTypes = new[]
        {
            typeof(ArgumentException),
            typeof(InvalidOperationException),
            typeof(NullReferenceException),
            typeof(TimeoutException),
            typeof(IOException),
            typeof(UnauthorizedAccessException),
            typeof(OutOfMemoryException),
            typeof(StackOverflowException)
        };
        
        var iterationsPerType = 100;
        var typePerformance = new Dictionary<Type, double>();
        
        foreach (var exceptionType in exceptionTypes)
        {
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterationsPerType; i++)
            {
                var exception = (Exception)Activator.CreateInstance(exceptionType, $"Test exception {i}");
                _errorHandler.HandleError(exception);
            }
            
            stopwatch.Stop();
            var averageTime = stopwatch.ElapsedMilliseconds / (double)iterationsPerType;
            typePerformance[exceptionType] = averageTime;
            
            Log($"{exceptionType.Name} 平均处理时间: {averageTime:F4}ms");
        }
        
        // 所有异常类型的处理时间应该一致（差异小于50%）
        var minTime = typePerformance.Values.Min();
        var maxTime = typePerformance.Values.Max();
        var variation = (maxTime - minTime) / minTime;
        
        variation.Should().BeLessThan(0.5);
        Log($"异常类型处理时间 variation: {variation:P2}");
    }
}

/// <summary>
/// ErrorHandlerManager 基准测试
/// </summary>
[MemoryDiagnoser]
public class ErrorHandlerManagerBenchmarks
{
    private readonly ErrorHandlerManager _errorHandler;
    private readonly string _testLogDirectory;

    public ErrorHandlerManagerBenchmarks()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Benchmark_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        var logger = new LoggerService(_testLogDirectory);
        _errorHandler = new ErrorHandlerManager(logger, _testLogDirectory);
    }

    [Benchmark]
    public void HandleError()
    {
        var exception = new Exception($"Benchmark exception {DateTime.Now.Ticks}");
        _errorHandler.HandleError(exception);
    }

    [Benchmark]
    public void HandleErrorWithCustomMessage()
    {
        var exception = new Exception($"Benchmark exception {DateTime.Now.Ticks}");
        _errorHandler.HandleError(exception, "Custom benchmark message");
    }

    [Benchmark]
    public void GetErrorLogs()
    {
        _errorHandler.GetErrorLogs();
    }

    [Benchmark]
    public void GetErrorCount()
    {
        _errorHandler.GetErrorCount();
    }

    [Benchmark]
    public void ClearErrorLogs()
    {
        _errorHandler.ClearErrorLogs();
    }

    [Benchmark]
    public void GetErrorStatistics()
    {
        _errorHandler.GetErrorStatistics();
    }
}