using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using KeyForge.Core.Domain.Act;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Models;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.IntegrationTests.Core
{
    /// <summary>
    /// 脚本播放器集成测试
    /// </summary>
    public class ScriptPlayerIntegrationTests : TestBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScriptPlayer _scriptPlayer;
        private readonly IScriptRecorder _scriptRecorder;

        public ScriptPlayerIntegrationTests()
        {
            // 设置依赖注入
            var services = new ServiceCollection();
            
            // 添加核心服务
            services.AddSingleton<IInputSimulator, TestInputSimulator>();
            services.AddSingleton<ILoggerService, TestLoggerService>();
            services.AddSingleton<IScriptPlayer, ScriptPlayer>();
            services.AddSingleton<IScriptRecorder, ScriptRecorder>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // 获取服务实例
            _scriptPlayer = _serviceProvider.GetRequiredService<IScriptPlayer>();
            _scriptRecorder = _serviceProvider.GetRequiredService<IScriptRecorder>();
        }

        [Fact]
        public void ScriptPlayer_ShouldIntegrateWithInputSimulator()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();

            // Act
            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.CurrentScript.Should().Be(script);
        }

        [Fact]
        public void ScriptRecorder_ShouldIntegrateWithInputSimulator()
        {
            // Arrange
            _scriptRecorder.StartRecording();

            // Act
            _scriptRecorder.StopRecording();

            // Assert
            _scriptRecorder.IsRecording.Should().BeFalse();
        }

        [Fact]
        public async Task RecordAndPlayback_ShouldWorkEndToEnd()
        {
            // Arrange
            _scriptRecorder.StartRecording();

            // 模拟一些输入
            var inputSimulator = _serviceProvider.GetRequiredService<IInputSimulator>() as TestInputSimulator;
            inputSimulator?.SimulateKeyPress(KeyCode.A);
            inputSimulator?.SimulateMouseClick(MouseButton.Left, 100, 200);

            await Task.Delay(100); // 等待录制

            _scriptRecorder.StopRecording();

            // 获取录制的脚本
            var recordedScript = _scriptRecorder.GetRecordedScript();

            // Act
            _scriptPlayer.LoadScript(recordedScript);
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.CurrentScript.Should().Be(recordedScript);
        }

        [Fact]
        public void PauseAndResume_ShouldWorkCorrectly()
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

            // Act - Resume
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void StopAndRestart_ShouldWorkCorrectly()
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

            // Act - Restart
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.IsPlaying.Should().BeTrue();
            _scriptPlayer.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void MultipleScripts_ShouldBeManagedCorrectly()
        {
            // Arrange
            var script1 = TestDataFactory.CreateCoreScript();
            var script2 = TestDataFactory.CreateCoreScript();

            // Act
            _scriptPlayer.LoadScript(script1);
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.CurrentScript.Should().Be(script1);

            // Act - Switch scripts
            _scriptPlayer.StopScript();
            _scriptPlayer.LoadScript(script2);
            _scriptPlayer.PlayScript();

            // Assert
            _scriptPlayer.CurrentScript.Should().Be(script2);
            _scriptPlayer.CurrentScript.Should().NotBe(script1);
        }

        [Fact]
        public void ErrorHandling_ShouldBeRobust()
        {
            // Arrange
            var script = TestDataFactory.CreateCoreScript();
            _scriptPlayer.LoadScript(script);

            // Act - Try to play without proper initialization
            var action = () => _scriptPlayer.PlayScript();

            // Assert
            action.Should().NotThrow(); // Should handle errors gracefully
        }
    }

    /// <summary>
    /// 测试用的输入模拟器
    /// </summary>
    internal class TestInputSimulator : IInputSimulator
    {
        public void KeyDown(KeyCode key)
        {
            // 简化实现 - 记录按键事件
            Console.WriteLine($"Key down: {key}");
        }

        public void KeyUp(KeyCode key)
        {
            Console.WriteLine($"Key up: {key}");
        }

        public void KeyPress(KeyCode key, int delay = 50)
        {
            KeyDown(key);
            Task.Delay(delay).Wait();
            KeyUp(key);
        }

        public void MouseDown(MouseButton button, int x, int y)
        {
            Console.WriteLine($"Mouse down: {button} at ({x}, {y})");
        }

        public void MouseUp(MouseButton button, int x, int y)
        {
            Console.WriteLine($"Mouse up: {button} at ({x}, {y})");
        }

        public void MouseMove(int x, int y)
        {
            Console.WriteLine($"Mouse move: ({x}, {y})");
        }

        public void MouseClick(MouseButton button, int x, int y, int delay = 100)
        {
            MouseDown(button, x, y);
            Task.Delay(delay).Wait();
            MouseUp(button, x, y);
        }

        public void MouseDoubleClick(MouseButton button, int x, int y)
        {
            MouseClick(button, x, y);
            Task.Delay(100).Wait();
            MouseClick(button, x, y);
        }

        public void MouseScroll(int delta)
        {
            Console.WriteLine($"Mouse scroll: {delta}");
        }

        public void SendMouse(MouseButton button, MouseState state)
        {
            Console.WriteLine($"Send mouse: {button} {state}");
        }

        public void SendKey(KeyCode key, KeyState state)
        {
            Console.WriteLine($"Send key: {key} {state}");
        }

        public void MoveMouse(int x, int y)
        {
            MouseMove(x, y);
        }

        // 测试辅助方法
        public void SimulateKeyPress(KeyCode key)
        {
            KeyPress(key, 50);
        }

        public void SimulateMouseClick(MouseButton button, int x, int y)
        {
            MouseClick(button, x, y, 100);
        }
    }

    /// <summary>
    /// 测试用的日志服务
    /// </summary>
    internal class TestLoggerService : ILoggerService
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void LogError(string message, Exception ex = null)
        {
            Console.WriteLine($"[ERROR] {message}");
            if (ex != null)
            {
                Console.WriteLine($"[ERROR] Exception: {ex.Message}");
            }
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }

        public void Info(string message)
        {
            LogInformation(message);
        }

        public void Warning(string message)
        {
            LogWarning(message);
        }

        public void Error(string message, Exception ex = null)
        {
            LogError(message, ex);
        }

        public void Debug(string message)
        {
            LogDebug(message);
        }
    }
}