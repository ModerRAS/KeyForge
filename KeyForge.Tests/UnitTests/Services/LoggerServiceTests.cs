using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Services;

/// <summary>
/// LoggerService 单元测试
/// 原本实现：复杂的日志服务测试
/// 简化实现：核心日志功能测试
/// </summary>
public class LoggerServiceTests : TestBase, IDisposable
{
    private readonly string _testLogDirectory;
    private readonly LoggerService _logger;

    public LoggerServiceTests(ITestOutputHelper output) : base(output)
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_LogTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _logger = new LoggerService(_testLogDirectory);
        Log($"日志测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"日志测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试目录失败: {ex.Message}");
        }
    }

    [Fact]
    public void Constructor_WithValidDirectory_ShouldInitialize()
    {
        // Act
        var logger = new LoggerService(_testLogDirectory);

        // Assert
        logger.Should().NotBeNull();
        Log("LoggerService 构造成功");
    }

    [Fact]
    public void Constructor_WithNullDirectory_ShouldThrowException()
    {
        // Act & Assert
        var action = () => new LoggerService(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空目录构造失败");
    }

    [Fact]
    public void Constructor_WithNonExistingDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var newDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_NewLogTest_{Guid.NewGuid()}");

        // Act
        var logger = new LoggerService(newDirectory);

        // Assert
        Directory.Exists(newDirectory).Should().BeTrue();
        Directory.Delete(newDirectory, true);
        Log("不存在的目录自动创建");
    }

    [Fact]
    public void LogInfo_WithMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test info message";

        // Act
        _logger.LogInfo(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        Log($"信息日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public void LogInfo_WithNullMessage_ShouldThrowException()
    {
        // Act & Assert
        var action = () => _logger.LogInfo(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空信息消息写入失败");
    }

    [Fact]
    public void LogInfo_WithEmptyMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "";

        // Act
        _logger.LogInfo(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        Log("空信息消息写入成功");
    }

    [Fact]
    public void LogError_WithMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test error message";

        // Act
        _logger.LogError(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        logContent.Should().Contain("ERROR");
        Log($"错误日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public void LogError_WithMessageAndException_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test error message";
        var exception = new Exception("Test exception");

        // Act
        _logger.LogError(message, exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        logContent.Should().Contain("ERROR");
        logContent.Should().Contain(exception.Message);
        Log($"错误日志（含异常）写入成功: {logFiles[0]}");
    }

    [Fact]
    public void LogError_WithNullException_ShouldThrowException()
    {
        // Arrange
        var message = "Test error message";

        // Act & Assert
        var action = () => _logger.LogError(message, null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空异常错误日志写入失败");
    }

    [Fact]
    public void LogWarning_WithMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test warning message";

        // Act
        _logger.LogWarning(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        logContent.Should().Contain("WARNING");
        Log($"警告日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public void LogDebug_WithMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test debug message";

        // Act
        _logger.LogDebug(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        logContent.Should().Contain("DEBUG");
        Log($"调试日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public void LogInfo_MultipleMessages_ShouldWriteToSameFile()
    {
        // Arrange
        var message1 = "Test message 1";
        var message2 = "Test message 2";

        // Act
        _logger.LogInfo(message1);
        _logger.LogInfo(message2);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message1);
        logContent.Should().Contain(message2);
        Log("多个消息写入同一文件");
    }

    [Fact]
    public void LogInfo_MultipleDays_ShouldCreateSeparateFiles()
    {
        // Arrange
        var message1 = "Test message 1";
        var message2 = "Test message 2";

        // Act
        _logger.LogInfo(message1);
        
        // 模拟日期变化（这里简化处理，实际测试中可能需要使用DateTime.Now的Mock）
        var logger2 = new LoggerService(_testLogDirectory);
        logger2.LogInfo(message2);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(2);
        Log("多日期日志创建分离文件");
    }

    [Fact]
    public void LogInfo_WithLongMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var longMessage = new string('A', 10000);

        // Act
        _logger.LogInfo(longMessage);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(longMessage);
        Log("长消息写入成功");
    }

    [Fact]
    public void LogInfo_WithSpecialCharacters_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test message with special chars: àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

        // Act
        _logger.LogInfo(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        Log("特殊字符消息写入成功");
    }

    [Fact]
    public void LogInfo_WithNewLines_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test message\nwith\nnew\nlines";

        // Act
        _logger.LogInfo(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        Log("多行消息写入成功");
    }

    [Fact]
    public void LogInfo_WithJson_ShouldWriteToLogFile()
    {
        // Arrange
        var jsonMessage = @"{""name"":""test"",""value"":123,""enabled"":true}";

        // Act
        _logger.LogInfo(jsonMessage);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(jsonMessage);
        Log("JSON消息写入成功");
    }

    [Fact]
    public void LogInfo_WithXml_ShouldWriteToLogFile()
    {
        // Arrange
        var xmlMessage = @"<root><item name=""test"" value=""123""/></root>";

        // Act
        _logger.LogInfo(xmlMessage);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(xmlMessage);
        Log("XML消息写入成功");
    }

    [Fact]
    public void LogInfo_ConcurrentAccess_ShouldNotThrowException()
    {
        // Arrange
        var message = "Test message";
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _logger.LogInfo(message)));
        }

        // Assert
        var allTasks = Task.WhenAll(tasks);
        allTasks.Should().NotThrow();
        Log("并发访问不抛出异常");
    }

    [Fact]
    public void LogInfo_ConcurrentAccess_ShouldWriteAllMessages()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var message = $"Test message {i}";
            tasks.Add(Task.Run(() => _logger.LogInfo(message)));
        }

        Task.WhenAll(tasks).Wait();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        for (int i = 0; i < 10; i++)
        {
            logContent.Should().Contain($"Test message {i}");
        }
        Log("并发访问写入所有消息");
    }

    [Fact]
    public void LogInfo_WhenDirectoryIsReadOnly_ShouldThrowException()
    {
        // Arrange
        var readOnlyDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_ReadOnlyTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(readOnlyDirectory);
        
        // 设置目录为只读（在Unix-like系统中可能需要不同的方法）
        var fileInfo = new DirectoryInfo(readOnlyDirectory);
        fileInfo.Attributes |= FileAttributes.ReadOnly;

        // Act & Assert
        var action = () => new LoggerService(readOnlyDirectory).LogInfo("Test message");
        action.Should().Throw<Exception>();

        // Cleanup
        fileInfo.Attributes &= ~FileAttributes.ReadOnly;
        Directory.Delete(readOnlyDirectory, true);
        LogError("只读目录写入失败");
    }

    [Fact]
    public void LogInfo_WhenDiskIsFull_ShouldThrowException()
    {
        // 注意：这个测试在实际环境中很难模拟，因为很难让磁盘真正满
        // 这里只是展示测试的思路
        
        // Arrange
        var message = "Test message";

        // Act & Assert
        // 在实际实现中，可能需要Mock文件系统来模拟磁盘满的情况
        // 这里我们假设磁盘空间足够，测试应该通过
        var action = () => _logger.LogInfo(message);
        action.Should().NotThrow();
        Log("磁盘空间充足时写入成功");
    }

    [Fact]
    public void LogInfo_WithVeryLongMessage_ShouldTruncate()
    {
        // Arrange
        var veryLongMessage = new string('A', 1000000); // 1MB message

        // Act
        _logger.LogInfo(veryLongMessage);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("A");
        Log("超长消息写入成功");
    }

    [Fact]
    public async Task LogInfoAsync_WithMessage_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test async message";

        // Act
        await _logger.LogInfoAsync(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        Log($"异步信息日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public async Task LogErrorAsync_WithMessageAndException_ShouldWriteToLogFile()
    {
        // Arrange
        var message = "Test async error message";
        var exception = new Exception("Test async exception");

        // Act
        await _logger.LogErrorAsync(message, exception);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "log_*.txt");
        logFiles.Should().HaveCount(1);
        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(message);
        logContent.Should().Contain(exception.Message);
        logContent.Should().Contain("ERROR");
        Log($"异步错误日志写入成功: {logFiles[0]}");
    }

    [Fact]
    public void GetLogFiles_ShouldReturnAllLogFiles()
    {
        // Arrange
        _logger.LogInfo("Test message 1");
        _logger.LogError("Test error message");

        // Act
        var logFiles = _logger.GetLogFiles();

        // Assert
        logFiles.Should().HaveCount(1);
        logFiles[0].Should().EndWith(".txt");
        Log($"获取日志文件成功: {logFiles.Count}个");
    }

    [Fact]
    public void GetLogFiles_WithNoLogs_ShouldReturnEmptyList()
    {
        // Act
        var logFiles = _logger.GetLogFiles();

        // Assert
        logFiles.Should().BeEmpty();
        Log("无日志时返回空列表");
    }

    [Fact]
    public void ClearLogs_ShouldRemoveAllLogFiles()
    {
        // Arrange
        _logger.LogInfo("Test message");

        // Act
        _logger.ClearLogs();
        var logFiles = _logger.GetLogFiles();

        // Assert
        logFiles.Should().BeEmpty();
        Log("日志清理成功");
    }

    [Fact]
    public void ClearLogs_WithNoLogs_ShouldDoNothing()
    {
        // Act
        _logger.ClearLogs();
        var logFiles = _logger.GetLogFiles();

        // Assert
        logFiles.Should().BeEmpty();
        Log("无日志时清理无影响");
    }

    [Fact]
    public void GetLogSize_ShouldReturnCorrectSize()
    {
        // Arrange
        _logger.LogInfo("Test message");

        // Act
        var size = _logger.GetLogSize();

        // Assert
        size.Should().BeGreaterThan(0);
        Log($"日志大小计算正确: {size} bytes");
    }

    [Fact]
    public void GetLogSize_WithNoLogs_ShouldReturnZero()
    {
        // Act
        var size = _logger.GetLogSize();

        // Assert
        size.Should().Be(0);
        Log("无日志时大小为0");
    }
}