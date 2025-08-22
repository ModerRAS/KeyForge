using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.IntegrationTests.Services;

/// <summary>
/// ErrorHandlerManager 集成测试
/// 原本实现：完整的错误处理管理器集成测试
/// 简化实现：核心错误处理集成功能测试
/// </summary>
public class ErrorHandlerManagerIntegrationTests : TestBase, IDisposable
{
    private readonly string _testLogDirectory;
    private readonly ErrorHandlerManager _errorHandler;
    private readonly LoggerService _logger;

    public ErrorHandlerManagerIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_ErrorIntegrationTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _logger = new LoggerService(_testLogDirectory);
        _errorHandler = new ErrorHandlerManager(_logger, _testLogDirectory);
        
        Log($"错误处理集成测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            _errorHandler?.Dispose();
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"错误处理集成测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public void HandleError_ShouldCreateLogFile()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        File.Exists(logFiles[0]).Should().BeTrue();
        Log($"错误日志文件创建成功: {logFiles[0]}");
    }

    [Fact]
    public void HandleMultipleErrors_ShouldCreateMultipleLogFiles()
    {
        // Arrange
        var exceptions = new[]
        {
            new Exception("Test exception 1"),
            new Exception("Test exception 2"),
            new Exception("Test exception 3")
        };

        // Act
        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(3);
        Log($"多个错误日志文件创建成功: {logFiles.Length}个");
    }

    [Fact]
    public void HandleError_ShouldWriteCorrectInformationToLogFile()
    {
        // Arrange
        var exception = new InvalidOperationException("Test invalid operation");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("InvalidOperationException");
        logContent.Should().Contain("Test invalid operation");
        logContent.Should().Contain("Stack Trace:");
        Log($"错误日志内容正确: {logFiles[0]}");
    }

    [Fact]
    public void HandleErrorWithCustomMessage_ShouldWriteMessageToLogFile()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var customMessage = "Custom error message";

        // Act
        _errorHandler.HandleError(exception, customMessage);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(customMessage);
        logContent.Should().Contain("Test exception");
        Log($"自定义消息错误日志正确: {logFiles[0]}");
    }

    [Fact]
    public void HandleAggregateException_ShouldWriteAllInnerExceptions()
    {
        // Arrange
        var innerExceptions = new[]
        {
            new Exception("Inner exception 1"),
            new Exception("Inner exception 2"),
            new Exception("Inner exception 3")
        };
        var aggregateException = new AggregateException(innerExceptions);

        // Act
        _errorHandler.HandleError(aggregateException);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("AggregateException");
        foreach (var innerException in innerExceptions)
        {
            logContent.Should().Contain(innerException.Message);
        }
        Log($"聚合异常日志包含所有内部异常");
    }

    [Fact]
    public void ErrorEvent_ShouldBeTriggered_WhenErrorOccurs()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var eventTriggered = false;
        Exception capturedException = null;
        string capturedMessage = null;

        _errorHandler.ErrorOccurred += (sender, args) =>
        {
            eventTriggered = true;
            capturedException = args.Exception;
            capturedMessage = args.Message;
        };

        // Act
        _errorHandler.HandleError(exception, "Custom message");

        // Assert
        eventTriggered.Should().BeTrue();
        capturedException.Should().BeSameAs(exception);
        capturedMessage.Should().Be("Custom message");
        Log("错误事件触发成功");
    }

    [Fact]
    public void GetErrorLogs_ShouldReturnAllErrorLogs()
    {
        // Arrange
        var exceptions = new[]
        {
            new Exception("Test exception 1"),
            new Exception("Test exception 2")
        };

        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Act
        var errorLogs = _errorHandler.GetErrorLogs();

        // Assert
        errorLogs.Should().HaveCount(2);
        errorLogs.Should().Contain(log => log.Contains("Test exception 1"));
        errorLogs.Should().Contain(log => log.Contains("Test exception 2"));
        Log($"获取错误日志成功: {errorLogs.Count}个");
    }

    [Fact]
    public void GetErrorCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var exceptions = new[]
        {
            new Exception("Test exception 1"),
            new Exception("Test exception 2"),
            new Exception("Test exception 3")
        };

        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Act
        var errorCount = _errorHandler.GetErrorCount();

        // Assert
        errorCount.Should().Be(3);
        Log($"错误计数正确: {errorCount}");
    }

    [Fact]
    public void ClearErrorLogs_ShouldRemoveAllErrorLogs()
    {
        // Arrange
        var exceptions = new[]
        {
            new Exception("Test exception 1"),
            new Exception("Test exception 2")
        };

        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Act
        _errorHandler.ClearErrorLogs();
        var errorLogs = _errorHandler.GetErrorLogs();
        var errorCount = _errorHandler.GetErrorCount();

        // Assert
        errorLogs.Should().BeEmpty();
        errorCount.Should().Be(0);
        Log("错误日志清理成功");
    }

    [Fact]
    public void HandleError_WithVeryLargeException_ShouldHandleGracefully()
    {
        // Arrange
        var largeMessage = new string('A', 10000);
        var exception = new Exception(largeMessage);

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(largeMessage);
        Log("大异常处理成功");
    }

    [Fact]
    public void HandleError_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialChars = "测试中文字符 © ® ™ → ← ↑ ↓ ↔ ⌫ ⌦ ⌧ ⌫ ⌦";
        var exception = new Exception(specialChars);

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(specialChars);
        Log("特殊字符异常处理成功");
    }

    [Fact]
    public void ConcurrentErrorHandling_ShouldWorkCorrectly()
    {
        // Arrange
        var taskCount = 10;
        var tasks = new Task[taskCount];

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var exception = new Exception($"Concurrent exception {i}");
            tasks[i] = Task.Run(() => _errorHandler.HandleError(exception));
        }

        Task.WhenAll(tasks).Wait();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(taskCount);
        var errorCount = _errorHandler.GetErrorCount();
        errorCount.Should().Be(taskCount);
        Log($"并发错误处理成功: {taskCount}个错误");
    }

    [Fact]
    public void HandleError_WhenLoggerFails_ShouldNotThrowException()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var faultyLogger = new FaultyLogger();
        var faultyErrorHandler = new ErrorHandlerManager(faultyLogger, _testLogDirectory);

        // Act
        var action = () => faultyErrorHandler.HandleError(exception);

        // Assert
        action.Should().NotThrow();
        Log("日志服务失败时不抛出异常");
    }

    [Fact]
    public void HandleError_WithDifferentExceptionTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var exceptions = new Exception[]
        {
            new ArgumentException("Invalid argument"),
            new InvalidOperationException("Invalid operation"),
            new NullReferenceException("Null reference"),
            new TimeoutException("Operation timed out"),
            new IOException("IO error"),
            new UnauthorizedAccessException("Access denied"),
            new OutOfMemoryException("Out of memory"),
            new StackOverflowException("Stack overflow")
        };

        // Act
        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(exceptions.Length);
        var errorCount = _errorHandler.GetErrorCount();
        errorCount.Should().Be(exceptions.Length);
        Log($"不同类型异常处理成功: {exceptions.Length}种");
    }

    [Fact]
    public void HandleError_ShouldIncludeTimestamp()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var beforeHandle = DateTime.Now;

        // Act
        _errorHandler.HandleError(exception);
        var afterHandle = DateTime.Now;

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(beforeHandle.ToString("yyyy-MM-dd"));
        logContent.Should().Contain(afterHandle.ToString("yyyy-MM-dd"));
        Log("错误日志包含时间戳");
    }

    [Fact]
    public void HandleError_ShouldNotCreateLogFiles_WhenDisabled()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var disabledErrorHandler = new ErrorHandlerManager(_logger, _testLogDirectory, enableLogging: false);

        // Act
        disabledErrorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().BeEmpty();
        Log("禁用时不创建日志文件");
    }

    [Fact]
    public void HandleError_ShouldRespectMaxLogFileSize()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var maxSizeErrorHandler = new ErrorHandlerManager(_logger, _testLogDirectory, maxLogFileSize: 1024); // 1KB

        // Act
        for (int i = 0; i < 100; i++)
        {
            maxSizeErrorHandler.HandleError(exception);
        }

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().NotBeEmpty();
        
        // Check that log files don't exceed max size
        foreach (var logFile in logFiles)
        {
            var fileInfo = new FileInfo(logFile);
            fileInfo.Length.Should().BeLessThanOrEqualTo(1024);
        }
        Log("日志文件大小限制 respected");
    }

    [Fact]
    public void HandleError_ShouldCleanupOldLogs()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var cleanupErrorHandler = new ErrorHandlerManager(_logger, _testLogDirectory, maxLogFiles: 2);

        // Act
        for (int i = 0; i < 5; i++)
        {
            cleanupErrorHandler.HandleError(exception);
        }

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(2);
        Log("旧日志清理成功");
    }

    [Fact]
    public void GetErrorStatistics_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var exceptions = new[]
        {
            new ArgumentException("Argument error"),
            new ArgumentException("Another argument error"),
            new InvalidOperationException("Operation error"),
            new TimeoutException("Timeout error"),
            new TimeoutException("Another timeout error")
        };

        foreach (var exception in exceptions)
        {
            _errorHandler.HandleError(exception);
        }

        // Act
        var statistics = _errorHandler.GetErrorStatistics();

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalErrors.Should().Be(5);
        statistics.ErrorsByType.Should().ContainKey("ArgumentException");
        statistics.ErrorsByType["ArgumentException"].Should().Be(2);
        statistics.ErrorsByType.Should().ContainKey("InvalidOperationException");
        statistics.ErrorsByType["InvalidOperationException"].Should().Be(1);
        statistics.ErrorsByType.Should().ContainKey("TimeoutException");
        statistics.ErrorsByType["TimeoutException"].Should().Be(2);
        Log("错误统计正确");
    }

    [Fact]
    public void MemoryUsage_ShouldBeReasonable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var errorCount = 1000;

        // Act
        for (int i = 0; i < errorCount; i++)
        {
            var exception = new Exception($"Test exception {i}");
            _errorHandler.HandleError(exception);
        }

        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;

        // Cleanup
        _errorHandler.ClearErrorLogs();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // 小于50MB
        memoryAfterCleanup.Should().BeLessThan(5 * 1024 * 1024); // 清理后小于5MB
        Log($"内存使用合理: 增加 {memoryIncrease / 1024}KB, 清理后 {memoryAfterCleanup / 1024}KB");
    }
}

/// <summary>
/// 故障日志服务（用于测试）
/// </summary>
public class FaultyLogger : ILoggerService
{
    public void LogInfo(string message)
    {
        throw new Exception("Logger failed");
    }

    public void LogError(string message)
    {
        throw new Exception("Logger failed");
    }

    public void LogError(string message, Exception exception)
    {
        throw new Exception("Logger failed");
    }

    public void LogWarning(string message)
    {
        throw new Exception("Logger failed");
    }

    public void LogDebug(string message)
    {
        throw new Exception("Logger failed");
    }

    public Task LogInfoAsync(string message)
    {
        throw new Exception("Logger failed");
    }

    public Task LogErrorAsync(string message)
    {
        throw new Exception("Logger failed");
    }

    public Task LogErrorAsync(string message, Exception exception)
    {
        throw new Exception("Logger failed");
    }
}