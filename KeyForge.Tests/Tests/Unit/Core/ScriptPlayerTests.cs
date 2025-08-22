using Xunit;
using FluentAssertions;
using Moq;
using KeyForge.Core;
using KeyForge.Domain;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Core;

/// <summary>
/// 脚本播放器核心功能单元测试
/// 原本实现：复杂的播放器测试
/// 简化实现：核心播放功能测试
/// </summary>
public class ScriptPlayerTests : TestBase
{
    private readonly Mock<IScriptExecutor> _mockExecutor;
    private readonly ScriptPlayer _scriptPlayer;

    public ScriptPlayerTests(ITestOutputHelper output) : base(output)
    {
        _mockExecutor = MockHelpers.CreateMockExecutor();
        _scriptPlayer = new ScriptPlayer(_mockExecutor.Object);
    }

    [Fact]
    public async Task PlayScript_WithValidScript_ShouldExecuteSuccessfully()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ReturnsAsync(true);

        // Act
        var result = await _scriptPlayer.PlayAsync(script);

        // Assert
        result.Should().BeTrue();
        _mockExecutor.Verify(x => x.ExecuteAsync(script), Times.Once);
        Log($"脚本执行成功: {script.Id}");
    }

    [Fact]
    public async Task PlayScript_WithNullScript_ShouldThrowException()
    {
        // Arrange
        Script script = null;

        // Act & Assert
        var action = async () => await _scriptPlayer.PlayAsync(script);
        await action.Should().ThrowAsync<ArgumentNullException>();
        LogError("空脚本参数验证成功");
    }

    [Fact]
    public async Task PlayScript_WhenExecutorFails_ShouldReturnFalse()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ReturnsAsync(false);

        // Act
        var result = await _scriptPlayer.PlayAsync(script);

        // Assert
        result.Should().BeFalse();
        Log($"脚本执行失败: {script.Id}");
    }

    [Fact]
    public async Task PlayScript_WhenExecutorThrowsException_ShouldThrowException()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ThrowsAsync(new Exception("Executor error"));

        // Act & Assert
        var action = async () => await _scriptPlayer.PlayAsync(script);
        await action.Should().ThrowAsync<Exception>();
        LogError("执行器异常处理成功");
    }

    [Fact]
    public async Task StopScript_WhenPlaying_ShouldStopExecution()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ReturnsAsync(true);

        // Act
        await _scriptPlayer.PlayAsync(script);
        await _scriptPlayer.StopAsync();

        // Assert
        _mockExecutor.Verify(x => x.StopAsync(), Times.Once);
        Log($"脚本停止成功: {script.Id}");
    }

    [Fact]
    public async Task PauseScript_WhenPlaying_ShouldPauseExecution()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ReturnsAsync(true);

        // Act
        await _scriptPlayer.PlayAsync(script);
        await _scriptPlayer.PauseAsync();

        // Assert
        _mockExecutor.Verify(x => x.PauseAsync(), Times.Once);
        Log($"脚本暂停成功: {script.Id}");
    }

    [Fact]
    public async Task ResumeScript_WhenPaused_ShouldResumeExecution()
    {
        // Arrange
        var script = TestFixtures.CreateValidScript();
        _mockExecutor.Setup(x => x.ExecuteAsync(script))
            .ReturnsAsync(true);

        // Act
        await _scriptPlayer.PlayAsync(script);
        await _scriptPlayer.PauseAsync();
        await _scriptPlayer.ResumeAsync();

        // Assert
        _mockExecutor.Verify(x => x.ResumeAsync(), Times.Once);
        Log($"脚本恢复成功: {script.Id}");
    }

    [Fact]
    public void GetStatus_WhenNotPlaying_ShouldReturnIdle()
    {
        // Act
        var status = _scriptPlayer.GetStatus();

        // Assert
        status.Should().Be(ScriptPlayerStatus.Idle);
        Log($"播放器状态: {status}");
    }
}