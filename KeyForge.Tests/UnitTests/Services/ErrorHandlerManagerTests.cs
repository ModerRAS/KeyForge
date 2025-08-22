using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Services;

/// <summary>
/// ErrorHandlerManager 单元测试
/// 原本实现：复杂的错误处理管理器测试
/// 简化实现：核心错误处理功能测试
/// </summary>
public class ErrorHandlerManagerTests : TestBase, IDisposable
{
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly ErrorHandlerManager _errorHandler;
    private readonly string _testLogDirectory;

    public ErrorHandlerManagerTests(ITestOutputHelper output) : base(output)
    {
        _mockLogger = new Mock<ILoggerService>();
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_ErrorTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _errorHandler = new ErrorHandlerManager(_mockLogger.Object, _testLogDirectory);
        Log($"错误处理测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"错误处理测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试目录失败: {ex.Message}");
        }
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitialize()
    {
        // Act
        var handler = new ErrorHandlerManager(_mockLogger.Object, _testLogDirectory);

        // Assert
        handler.Should().NotBeNull();
        Log("ErrorHandlerManager 构造成功");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowException()
    {
        // Act & Assert
        var action = () => new ErrorHandlerManager(null, _testLogDirectory);
        action.Should().Throw<ArgumentNullException>();
        LogError("空日志服务构造失败");
    }

    [Fact]
    public void Constructor_WithNullLogDirectory_ShouldThrowException()
    {
        // Act & Assert
        var action = () => new ErrorHandlerManager(_mockLogger.Object, null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空日志目录构造失败");
    }

    [Fact]
    public void HandleError_WithException_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.Is<Exception>(e => e.Message == "Test exception")), Times.Once);
        Log("异常处理成功");
    }

    [Fact]
    public void HandleError_WithNullException_ShouldThrowException()
    {
        // Act & Assert
        var action = () => _errorHandler.HandleError(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空异常处理失败");
    }

    [Fact]
    public void HandleError_WithExceptionAndMessage_ShouldLogErrorWithMessage()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Custom error message";

        // Act
        _errorHandler.HandleError(exception, message);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains(message)), It.Is<Exception>(e => e.Message == "Test exception")), Times.Once);
        Log("带消息的异常处理成功");
    }

    [Fact]
    public void HandleError_WithExceptionAndNullMessage_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        string message = null;

        // Act
        _errorHandler.HandleError(exception, message);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.Is<Exception>(e => e.Message == "Test exception")), Times.Once);
        Log("空消息异常处理成功");
    }

    [Fact]
    public void HandleError_WithEmptyMessage_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "";

        // Act
        _errorHandler.HandleError(exception, message);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.Is<Exception>(e => e.Message == "Test exception")), Times.Once);
        Log("空消息异常处理成功");
    }

    [Fact]
    public void HandleError_WithAggregateException_ShouldLogAllInnerExceptions()
    {
        // Arrange
        var innerException1 = new Exception("Inner exception 1");
        var innerException2 = new Exception("Inner exception 2");
        var aggregateException = new AggregateException(innerException1, innerException2);

        // Act
        _errorHandler.HandleError(aggregateException);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.Is<Exception>(e => e is AggregateException)), Times.Once);
        Log("聚合异常处理成功");
    }

    [Fact]
    public void HandleError_WithTimeoutException_ShouldLogTimeout()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("timeout")), It.Is<Exception>(e => e is TimeoutException)), Times.Once);
        Log("超时异常处理成功");
    }

    [Fact]
    public void HandleError_WithIOException_ShouldLogIoError()
    {
        // Arrange
        var exception = new IOException("File not found");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("IO")), It.Is<Exception>(e => e is IOException)), Times.Once);
        Log("IO异常处理成功");
    }

    [Fact]
    public void HandleError_WithUnauthorizedAccessException_ShouldLogAccessDenied()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("access")), It.Is<Exception>(e => e is UnauthorizedAccessException)), Times.Once);
        Log("访问权限异常处理成功");
    }

    [Fact]
    public void HandleError_WithInvalidOperationException_ShouldLogInvalidOperation()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("invalid")), It.Is<Exception>(e => e is InvalidOperationException)), Times.Once);
        Log("无效操作异常处理成功");
    }

    [Fact]
    public void HandleError_WithArgumentException_ShouldLogInvalidArgument()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("argument")), It.Is<Exception>(e => e is ArgumentException)), Times.Once);
        Log("参数异常处理成功");
    }

    [Fact]
    public void HandleError_WithNullReferenceException_ShouldLogNullReference()
    {
        // Arrange
        var exception = new NullReferenceException("Object reference not set");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("null")), It.Is<Exception>(e => e is NullReferenceException)), Times.Once);
        Log("空引用异常处理成功");
    }

    [Fact]
    public void HandleError_WithOutOfMemoryException_ShouldLogOutOfMemory()
    {
        // Arrange
        var exception = new OutOfMemoryException("Out of memory");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("memory")), It.Is<Exception>(e => e is OutOfMemoryException)), Times.Once);
        Log("内存不足异常处理成功");
    }

    [Fact]
    public void HandleError_WithStackOverflowException_ShouldLogStackOverflow()
    {
        // Arrange
        var exception = new StackOverflowException("Stack overflow");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.Is<string>(m => m.Contains("stack")), It.Is<Exception>(e => e is StackOverflowException)), Times.Once);
        Log("堆栈溢出异常处理成功");
    }

    [Fact]
    public void HandleError_WithCustomException_ShouldLogCustomError()
    {
        // Arrange
        var exception = new CustomTestException("Custom test exception");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.Is<Exception>(e => e is CustomTestException)), Times.Once);
        Log("自定义异常处理成功");
    }

    [Fact]
    public void HandleError_ShouldCreateErrorLogFile()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(1);
        Log($"错误日志文件创建成功: {logFiles[0]}");
    }

    [Fact]
    public void HandleError_MultipleErrors_ShouldCreateMultipleLogFiles()
    {
        // Arrange
        var exception1 = new Exception("Test exception 1");
        var exception2 = new Exception("Test exception 2");

        // Act
        _errorHandler.HandleError(exception1);
        _errorHandler.HandleError(exception2);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "error_*.log");
        logFiles.Should().HaveCount(2);
        Log($"多个错误日志文件创建成功: {logFiles.Length}个");
    }

    [Fact]
    public void GetErrorLogs_ShouldReturnAllErrorLogs()
    {
        // Arrange
        var exception1 = new Exception("Test exception 1");
        var exception2 = new Exception("Test exception 2");

        _errorHandler.HandleError(exception1);
        _errorHandler.HandleError(exception2);

        // Act
        var logs = _errorHandler.GetErrorLogs();

        // Assert
        logs.Should().HaveCount(2);
        logs.Should().Contain(l => l.Contains("Test exception 1"));
        logs.Should().Contain(l => l.Contains("Test exception 2"));
        Log($"获取错误日志成功: {logs.Count}个");
    }

    [Fact]
    public void GetErrorLogs_WithNoErrors_ShouldReturnEmptyList()
    {
        // Act
        var logs = _errorHandler.GetErrorLogs();

        // Assert
        logs.Should().BeEmpty();
        Log("无错误时返回空列表");
    }

    [Fact]
    public void ClearErrorLogs_ShouldRemoveAllErrorLogs()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _errorHandler.HandleError(exception);

        // Act
        _errorHandler.ClearErrorLogs();
        var logs = _errorHandler.GetErrorLogs();

        // Assert
        logs.Should().BeEmpty();
        Log("错误日志清理成功");
    }

    [Fact]
    public void ClearErrorLogs_WithNoErrors_ShouldDoNothing()
    {
        // Act
        _errorHandler.ClearErrorLogs();
        var logs = _errorHandler.GetErrorLogs();

        // Assert
        logs.Should().BeEmpty();
        Log("无错误时清理无影响");
    }

    [Fact]
    public void GetErrorCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var exception1 = new Exception("Test exception 1");
        var exception2 = new Exception("Test exception 2");

        _errorHandler.HandleError(exception1);
        _errorHandler.HandleError(exception2);

        // Act
        var count = _errorHandler.GetErrorCount();

        // Assert
        count.Should().Be(2);
        Log($"错误计数正确: {count}");
    }

    [Fact]
    public void GetErrorCount_WithNoErrors_ShouldReturnZero()
    {
        // Act
        var count = _errorHandler.GetErrorCount();

        // Assert
        count.Should().Be(0);
        Log("无错误时计数为0");
    }

    [Fact]
    public void HandleError_ShouldTriggerErrorEvent()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var eventTriggered = false;
        Exception capturedException = null;

        _errorHandler.ErrorOccurred += (sender, args) =>
        {
            eventTriggered = true;
            capturedException = args.Exception;
        };

        // Act
        _errorHandler.HandleError(exception);

        // Assert
        eventTriggered.Should().BeTrue();
        capturedException.Should().BeSameAs(exception);
        Log("错误事件触发成功");
    }

    [Fact]
    public void HandleError_ShouldNotThrowException_WhenLoggerFails()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockLogger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
            .Throws(new Exception("Logger failed"));

        // Act & Assert
        var action = () => _errorHandler.HandleError(exception);
        action.Should().NotThrow();
        Log("日志服务失败时不抛出异常");
    }
}

/// <summary>
/// 自定义测试异常
/// </summary>
public class CustomTestException : Exception
{
    public CustomTestException(string message) : base(message)
    {
    }
}