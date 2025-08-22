using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using KeyForge.Core.Domain.Act;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Models;
using KeyForge.Core.Enums;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.UnitTests.Core
{
    /// <summary>
    /// 脚本播放器单元测试
    /// </summary>
    public class ScriptPlayerTests : TestBase
    {
        private readonly Mock<IInputSimulator> _mockInputSimulator;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly ScriptPlayer _scriptPlayer;

        public ScriptPlayerTests()
        {
            _mockInputSimulator = MockFactory.CreateInputSimulatorMock();
            _mockLogger = MockFactory.CreateLoggerServiceMock();
            _scriptPlayer = new ScriptPlayer(_mockInputSimulator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreatePlayer()
        {
            // Arrange & Act
            var player = new ScriptPlayer(_mockInputSimulator.Object, _mockLogger.Object);

            // Assert
            player.Should().NotBeNull();
            player.IsPlaying.Should().BeFalse();
            player.IsPaused.Should().BeFalse();
            player.CurrentScript.Should().BeNull();
        }

        [Fact]
        public void LoadScript_WithValidScript_ShouldLoadScript()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();

            // Act
            _scriptPlayer.LoadScript(script);

            // Assert
            _scriptPlayer.CurrentScript.Should().Be(script);
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void LoadScript_WithNullScript_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            var action = () => _scriptPlayer.LoadScript(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PlayScript_WithLoadedScript_ShouldStartPlaying()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.IsPaused.Should().BeFalse();
            
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("开始执行脚本"))), Times.Once);
        }

        [Fact]
        public void PlayScript_WithoutLoadedScript_ShouldThrowInvalidOperationException()
        {
            // Arrange & Act & Assert
            var action = () => _scriptPlayer.PlayScript();
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void PlayScript_WhenAlreadyPlaying_ShouldDoNothing()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.IsPaused.Should().BeFalse();
            
            // 验证只调用了一次日志记录
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("开始执行脚本"))), Times.Once);
        }

        [Fact]
        public void PauseScript_WhenPlaying_ShouldPause()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            _scriptPlayer.PauseScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeTrue();
        }

        [Fact]
        public void PauseScript_WhenNotPlaying_ShouldDoNothing()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PauseScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void StopScript_WhenPlaying_ShouldStop()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            _scriptPlayer.StopScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void StopScript_WhenPaused_ShouldStop()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();
            _scriptPlayer.PauseScript();

            // Act
            _scriptPlayer.StopScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void StopScript_WhenNotPlaying_ShouldDoNothing()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.StopScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteScript_WithKeyActions_ShouldExecuteKeyActions()
        {
            // Arrange
            var keyAction = new KeyForge.Core.Models.Action(
                Guid.NewGuid(),
                KeyForge.Core.Enums.ActionType.KeyDown,
                key: KeyCode.A,
                delay: 100);

            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(MapCoreActionToDomainAction(keyAction));
            
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PlayScript();
            await Task.Delay(200); // 等待执行完成

            // Assert
            _mockInputSimulator.Verify(s => s.KeyDown(KeyCode.A), Times.Once);
            _mockInputSimulator.Verify(s => s.KeyUp(KeyCode.A), Times.Once);
        }

        [Fact]
        public async Task ExecuteScript_WithMouseActions_ShouldExecuteMouseActions()
        {
            // Arrange
            var mouseAction = new KeyForge.Core.Models.Action(
                Guid.NewGuid(),
                KeyForge.Core.Enums.ActionType.MouseDown,
                button: MouseButton.Left,
                x: 100,
                y: 200,
                delay: 100);

            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(MapCoreActionToDomainAction(mouseAction));
            
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PlayScript();
            await Task.Delay(200); // 等待执行完成

            // Assert
            _mockInputSimulator.Verify(s => s.MouseDown(MouseButton.Left, 100, 200), Times.Once);
            _mockInputSimulator.Verify(s => s.MouseUp(MouseButton.Left, 100, 200), Times.Once);
        }

        [Fact]
        public async Task ExecuteScript_WithDelayActions_ShouldWait()
        {
            // Arrange
            var delayAction = new KeyForge.Core.Models.Action(
                Guid.NewGuid(),
                KeyForge.Core.Enums.ActionType.Delay,
                delay: 500);

            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(MapCoreActionToDomainAction(delayAction));
            
            _scriptPlayer.LoadScript(script);
            
            var startTime = DateTime.UtcNow;

            // Act
            _scriptPlayer.PlayScript();
            await Task.Delay(600); // 等待执行完成

            // Assert
            var executionTime = DateTime.UtcNow - startTime;
            executionTime.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task ExecuteScript_WithMixedActions_ShouldExecuteInOrder()
        {
            // Arrange
            var actions = new[]
            {
                new KeyForge.Core.Models.Action(Guid.NewGuid(), KeyForge.Core.Enums.ActionType.KeyDown, key: KeyCode.A, delay: 50),
                new KeyForge.Core.Models.Action(Guid.NewGuid(), KeyForge.Core.Enums.ActionType.Delay, delay: 100),
                new KeyForge.Core.Models.Action(Guid.NewGuid(), KeyForge.Core.Enums.ActionType.MouseDown, button: MouseButton.Left, x: 100, y: 200, delay: 50),
                new KeyForge.Core.Models.Action(Guid.NewGuid(), KeyForge.Core.Enums.ActionType.Delay, delay: 100),
                new KeyForge.Core.Models.Action(Guid.NewGuid(), KeyForge.Core.Enums.ActionType.KeyUp, key: KeyCode.A, delay: 50)
            };

            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            foreach (var action in actions)
            {
                script.AddAction(MapCoreActionToDomainAction(action));
            }
            
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PlayScript();
            await Task.Delay(500); // 等待执行完成

            // Assert
            // 验证动作按顺序执行
            var invocationOrder = new List<string>();
            _mockInputSimulator.Invocations.ToList().ForEach(invocation =>
            {
                invocationOrder.Add(invocation.Method.Name);
            });

            invocationOrder.Should().ContainInOrder("KeyDown", "MouseDown", "KeyUp");
        }

        [Fact]
        public async Task ExecuteScript_WhenPaused_ShouldPauseExecution()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Long delay"));
            
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            await Task.Delay(100); // 让脚本开始执行
            _scriptPlayer.PauseScript();
            await Task.Delay(200); // 等待一段时间

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteScript_WhenStopped_ShouldStopExecution()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Long delay"));
            
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            await Task.Delay(100); // 让脚本开始执行
            _scriptPlayer.StopScript();
            await Task.Delay(200); // 等待一段时间

            // Assert
            _scriptPlayer.IsPlaying.Should().BeFalse();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void ExecuteScript_WithException_ShouldLogError()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test action"));
            
            _mockInputSimulator.Setup(s => s.KeyDown(KeyCode.A))
                .Throws(new Exception("Simulator error"));
            
            _scriptPlayer.LoadScript(script);

            // Act
            _scriptPlayer.PlayScript();

            // Assert
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("执行脚本时发生错误")), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void GetProgress_WithLoadedScript_ShouldReturnProgress()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Action 1"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 100, "Action 2"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Action 3"));
            
            _scriptPlayer.LoadScript(script);

            // Act
            var progress = _scriptPlayer.GetProgress();

            // Assert
            progress.Should().Be(0); // 初始进度为0
        }

        [Fact]
        public void GetProgress_WithoutLoadedScript_ShouldReturnZero()
        {
            // Act
            var progress = _scriptPlayer.GetProgress();

            // Assert
            progress.Should().Be(0);
        }

        private static GameAction MapCoreActionToDomainAction(KeyForge.Core.Models.Action coreAction)
        {
            var domainType = coreAction.Type switch
            {
                KeyForge.Core.Enums.ActionType.KeyDown => ActionType.KeyDown,
                KeyForge.Core.Enums.ActionType.KeyUp => ActionType.KeyUp,
                KeyForge.Core.Enums.ActionType.MouseDown => ActionType.MouseDown,
                KeyForge.Core.Enums.ActionType.MouseUp => ActionType.MouseUp,
                KeyForge.Core.Enums.ActionType.MouseMove => ActionType.MouseMove,
                KeyForge.Core.Enums.ActionType.Delay => ActionType.Delay,
                _ => throw new ArgumentException($"Unsupported action type: {coreAction.Type}")
            };

            return domainType switch
            {
                ActionType.KeyDown or ActionType.KeyUp => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Key.Value,
                    coreAction.Delay,
                    "Test action"),
                
                ActionType.MouseDown or ActionType.MouseUp or ActionType.MouseMove => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Button.Value,
                    coreAction.X,
                    coreAction.Y,
                    coreAction.Delay,
                    "Test action"),
                
                ActionType.Delay => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Delay,
                    "Test action"),
                
                _ => throw new ArgumentException($"Unsupported action type: {domainType}")
            };
        }
    }
}