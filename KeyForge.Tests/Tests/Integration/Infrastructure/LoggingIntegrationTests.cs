using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using KeyForge.Tests.Support;
using Microsoft.Extensions.Logging;

namespace KeyForge.Tests.Integration.Infrastructure;

/// <summary>
/// 日志系统集成测试
/// 原本实现：复杂的日志系统测试
/// 简化实现：核心日志功能测试
/// </summary>
public class LoggingIntegrationTests : TestBase
{
    private readonly string _testDirectory;
    private readonly string _logDirectory;

    public LoggingIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Log_Test_{Guid.NewGuid()}");
        _logDirectory = Path.Combine(_testDirectory, "Logs");
        Directory.CreateDirectory(_logDirectory);
        Log($"创建日志测试目录: {_testDirectory}");
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
                    Log($"清理日志测试目录: {_testDirectory}");
                }
            }
            catch (Exception ex)
            {
                Log($"清理日志测试目录失败: {ex.Message}");
            }
        }
        base.Dispose(disposing);
    }

    [Fact]
    public void FileLogger_CreateLogger_ShouldCreateValidLogger()
    {
        // Arrange
        var categoryName = "TestLogger";
        
        // Act
        var logger = new FileLogger(categoryName, _logDirectory);
        
        // Assert
        logger.Should().NotBeNull();
        logger.CategoryName.Should().Be(categoryName);
        Log($"创建文件日志器成功: {categoryName}");
    }

    [Fact]
    public void FileLogger_LogInformation_ShouldWriteToFile()
    {
        // Arrange
        var categoryName = "InfoLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var message = "Test information message";
        
        // Act
        logger.LogInformation(message);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(message);
        logContent.Should().Contain("Information");
        Log($"信息日志写入成功: {message}");
    }

    [Fact]
    public void FileLogger_LogWarning_ShouldWriteToFile()
    {
        // Arrange
        var categoryName = "WarningLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var message = "Test warning message";
        
        // Act
        logger.LogWarning(message);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(message);
        logContent.Should().Contain("Warning");
        Log($"警告日志写入成功: {message}");
    }

    [Fact]
    public void FileLogger_LogError_ShouldWriteToFile()
    {
        // Arrange
        var categoryName = "ErrorLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var message = "Test error message";
        var exception = new InvalidOperationException("Test exception");
        
        // Act
        logger.LogError(exception, message);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(message);
        logContent.Should().Contain("Error");
        logContent.Should().Contain(exception.Message);
        Log($"错误日志写入成功: {message}");
    }

    [Fact]
    public void FileLogger_LogDebug_ShouldWriteToFile()
    {
        // Arrange
        var categoryName = "DebugLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var message = "Test debug message";
        
        // Act
        logger.LogDebug(message);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(message);
        logContent.Should().Contain("Debug");
        Log($"调试日志写入成功: {message}");
    }

    [Fact]
    public void FileLogger_LogScope_ShouldIncludeScopeInformation()
    {
        // Arrange
        var categoryName = "ScopeLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var message = "Test scoped message";
        
        // Act
        using (var scope = logger.BeginScope("TestScope"))
        {
            logger.LogInformation(message);
        }
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(message);
        logContent.Should().Contain("TestScope");
        Log($"作用域日志写入成功: {message}");
    }

    [Fact]
    public void FileLogger_MultipleLoggers_ShouldCreateSeparateFiles()
    {
        // Arrange
        var logger1 = new FileLogger("Logger1", _logDirectory);
        var logger2 = new FileLogger("Logger2", _logDirectory);
        
        // Act
        logger1.LogInformation("Message from logger 1");
        logger2.LogInformation("Message from logger 2");
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().HaveCount(2);
        
        var logContent1 = File.ReadAllText(logFiles[0]);
        var logContent2 = File.ReadAllText(logFiles[1]);
        
        (logContent1.Contains("Message from logger 1") || logContent2.Contains("Message from logger 1")).Should().BeTrue();
        (logContent1.Contains("Message from logger 2") || logContent2.Contains("Message from logger 2")).Should().BeTrue();
        Log($"多个日志器创建独立文件成功");
    }

    [Fact]
    public void FileLogger_LogRotation_ShouldCreateNewFiles()
    {
        // Arrange
        var categoryName = "RotationLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        
        // Act - 写入大量日志以触发轮转
        for (int i = 0; i < 1000; i++)
        {
            logger.LogInformation($"Test log message {i}");
        }
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().HaveCountGreaterThan(1);
        Log($"日志轮转创建多个文件成功: {logFiles.Length} 个文件");
    }

    [Fact]
    public void FileLogger_ConcurrentLogging_ShouldBeThreadSafe()
    {
        // Arrange
        var categoryName = "ConcurrentLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var messageCount = 100;
        
        // Act - 并行写入日志
        Parallel.For(0, messageCount, i =>
        {
            logger.LogInformation($"Concurrent log message {i}");
        });
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        for (int i = 0; i < messageCount; i++)
        {
            logContent.Should().Contain($"Concurrent log message {i}");
        }
        Log($"并发日志写入成功: {messageCount} 条消息");
    }

    [Fact]
    public void FileLogger_LogWithParameters_ShouldFormatCorrectly()
    {
        // Arrange
        var categoryName = "ParameterLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var name = "TestUser";
        var count = 42;
        
        // Act
        logger.LogInformation("User {Name} has {Count} items", name, count);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain("User TestUser has 42 items");
        Log($"参数化日志写入成功");
    }

    [Fact]
    public void FileLogger_LogLargeMessage_ShouldHandleGracefully()
    {
        // Arrange
        var categoryName = "LargeMessageLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var largeMessage = new string('a', 10000); // 10KB消息
        
        // Act
        logger.LogInformation(largeMessage);
        
        // Assert
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        var logContent = File.ReadAllText(logFiles.First());
        logContent.Should().Contain(largeMessage.Substring(0, 100)); // 检查前100个字符
        Log($"大消息日志写入成功: {largeMessage.Length} 字符");
    }

    [Fact]
    public void FileLogger_InvalidDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var invalidDirectory = Path.Combine(_testDirectory, "Invalid", "Directory", "Path");
        var categoryName = "InvalidDirLogger";
        
        // Act
        var logger = new FileLogger(categoryName, invalidDirectory);
        logger.LogInformation("Test message");
        
        // Assert
        Directory.Exists(invalidDirectory).Should().BeTrue();
        var logFiles = Directory.GetFiles(invalidDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        Log($"无效目录自动创建成功");
    }

    [Fact]
    public void FileLogger_Performance_ShouldBeFast()
    {
        // Arrange
        var categoryName = "PerformanceLogger";
        var logger = new FileLogger(categoryName, _logDirectory);
        var messageCount = 1000;
        
        // Act
        var startTime = DateTime.UtcNow;
        for (int i = 0; i < messageCount; i++)
        {
            logger.LogInformation($"Performance test message {i}");
        }
        var endTime = DateTime.UtcNow;
        
        // Assert
        var duration = (endTime - startTime).TotalMilliseconds;
        duration.Should().BeLessThan(5000); // 应该在5秒内完成1000条日志
        
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        logFiles.Should().NotBeEmpty();
        
        Log($"日志性能测试: {messageCount} 条消息用时 {duration:F2}ms");
    }
}

/// <summary>
/// 简化的文件日志器实现
/// 原本实现：复杂的日志系统
/// 简化实现：基础的文件日志功能
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly object _lock = new object();

    public FileLogger(string categoryName, string logDirectory)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public string CategoryName => _categoryName;

    public IDisposable BeginScope<TState>(TState state)
    {
        return new NoopDisposable();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";
        
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception;
        }

        lock (_lock)
        {
            var logFile = Path.Combine(_logDirectory, $"{_categoryName}_{DateTime.UtcNow:yyyyMMdd}.log");
            File.AppendAllText(logFile, logEntry + Environment.NewLine);
        }
    }

    private class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}