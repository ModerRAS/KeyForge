using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Models;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.PerformanceTests
{
    /// <summary>
    /// 脚本播放器性能测试
    /// </summary>
    public class ScriptPlayerPerformanceTests : TestBase
    {
        private readonly IScriptPlayer _scriptPlayer;
        private readonly IInputSimulator _inputSimulator;
        private readonly ILoggerService _loggerService;

        public ScriptPlayerPerformanceTests()
        {
            _inputSimulator = new PerformanceTestInputSimulator();
            _loggerService = new PerformanceTestLoggerService();
            _scriptPlayer = new ScriptPlayer(_inputSimulator, _loggerService);
        }

        [Fact]
        public void PlayScript_WithSmallScript_ShouldBeFast()
        {
            // Arrange
            var script = CreateScriptWithActions(10);
            const int maxExecutionTimeMs = 100;

            // Act
            var executionTime = MeasureExecutionTime(() =>
            {
                _scriptPlayer.LoadScript(script);
                _scriptPlayer.PlayScript();
            });

            // Assert
            executionTime.Should().BeLessThan(maxExecutionTimeMs);
        }

        [Fact]
        public void PlayScript_WithLargeScript_ShouldBeEfficient()
        {
            // Arrange
            var script = CreateScriptWithActions(1000);
            const int maxExecutionTimeMs = 5000;

            // Act
            var executionTime = MeasureExecutionTime(() =>
            {
                _scriptPlayer.LoadScript(script);
                _scriptPlayer.PlayScript();
            });

            // Assert
            executionTime.Should().BeLessThan(maxExecutionTimeMs);
        }

        [Fact]
        public void MemoryUsage_ShouldBeReasonable()
        {
            // Arrange
            const int scriptCount = 100;
            const int actionsPerScript = 100;
            const int maxMemoryMB = 50;

            var initialMemory = GC.GetTotalMemory(false);

            // Act
            for (int i = 0; i < scriptCount; i++)
            {
                var script = CreateScriptWithActions(actionsPerScript);
                _scriptPlayer.LoadScript(script);
                _scriptPlayer.PlayScript();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            var finalMemory = GC.GetTotalMemory(false);

            // Assert
            var memoryIncreaseMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);
            memoryIncreaseMB.Should().BeLessThan(maxMemoryMB);
        }

        [Fact]
        public void ConcurrentScriptExecution_ShouldScale()
        {
            // Arrange
            const int concurrentScripts = 10;
            const int actionsPerScript = 100;
            const int maxExecutionTimeMs = 10000;

            var scripts = new List<Script>();
            for (int i = 0; i < concurrentScripts; i++)
            {
                scripts.Add(CreateScriptWithActions(actionsPerScript));
            }

            // Act
            var executionTime = MeasureExecutionTime(() =>
            {
                Parallel.ForEach(scripts, script =>
                {
                    var player = new ScriptPlayer(_inputSimulator, _loggerService);
                    player.LoadScript(script);
                    player.PlayScript();
                });
            });

            // Assert
            executionTime.Should().BeLessThan(maxExecutionTimeMs);
        }

        [Fact]
        public void PauseAndResume_ShouldBeResponsive()
        {
            // Arrange
            var script = CreateScriptWithActions(100);
            const int maxResponseTimeMs = 50;

            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            var pauseTime = MeasureExecutionTime(() => _scriptPlayer.PauseScript());
            var resumeTime = MeasureExecutionTime(() => _scriptPlayer.PlayScript());

            // Assert
            pauseTime.Should().BeLessThan(maxResponseTimeMs);
            resumeTime.Should().BeLessThan(maxResponseTimeMs);
        }

        [Fact]
        public void StopExecution_ShouldBeImmediate()
        {
            // Arrange
            var script = CreateScriptWithActions(1000);
            const int maxStopTimeMs = 100;

            _scriptPlayer.LoadScript(script);
            _scriptPlayer.PlayScript();

            // Act
            var stopTime = MeasureExecutionTime(() => _scriptPlayer.StopScript());

            // Assert
            stopTime.Should().BeLessThan(maxStopTimeMs);
            _scriptPlayer.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public void ScriptLoading_ShouldBeFast()
        {
            // Arrange
            const int scriptCount = 100;
            const int actionsPerScript = 50;
            const int maxLoadingTimeMs = 1000;

            var scripts = new List<Script>();
            for (int i = 0; i < scriptCount; i++)
            {
                scripts.Add(CreateScriptWithActions(actionsPerScript));
            }

            // Act
            var loadingTime = MeasureExecutionTime(() =>
            {
                foreach (var script in scripts)
                {
                    _scriptPlayer.LoadScript(script);
                }
            });

            // Assert
            loadingTime.Should().BeLessThan(maxLoadingTimeMs);
        }

        [Fact]
        public void ActionExecutionRate_ShouldBeHigh()
        {
            // Arrange
            var script = CreateScriptWithActions(1000);
            const int minActionsPerSecond = 100;

            _scriptPlayer.LoadScript(script);

            // Act
            var stopwatch = Stopwatch.StartNew();
            _scriptPlayer.PlayScript();
            stopwatch.Stop();

            var executionTimeSeconds = stopwatch.Elapsed.TotalSeconds;
            var actionsPerSecond = script.Actions.Count / executionTimeSeconds;

            // Assert
            actionsPerSecond.Should().BeGreaterThan(minActionsPerSecond);
        }

        [Fact]
        public void StressTest_ShouldHandleHighLoad()
        {
            // Arrange
            const int durationSeconds = 30;
            const int minOperationsPerSecond = 10;

            var stopwatch = Stopwatch.StartNew();
            var operationCount = 0;
            var cts = new System.Threading.CancellationTokenSource();

            // Act
            var loadTask = System.Threading.Tasks.Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var script = CreateScriptWithActions(10);
                    _scriptPlayer.LoadScript(script);
                    _scriptPlayer.PlayScript();
                    _scriptPlayer.StopScript();
                    operationCount++;
                }
            }, cts.Token);

            System.Threading.Tasks.Task.Delay(durationSeconds * 1000).Wait();
            cts.Cancel();
            loadTask.Wait();
            stopwatch.Stop();

            var operationsPerSecond = operationCount / stopwatch.Elapsed.TotalSeconds;

            // Assert
            operationsPerSecond.Should().BeGreaterThan(minOperationsPerSecond);
        }

        private static long MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static Script CreateScriptWithActions(int actionCount)
        {
            var script = new Script(Guid.NewGuid(), "Performance Test Script", "Script for performance testing");

            for (int i = 0; i < actionCount; i++)
            {
                var action = i % 3 switch
                {
                    0 => new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 10, "Key action"),
                    1 => new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200, 10, "Mouse action"),
                    _ => new GameAction(Guid.NewGuid(), ActionType.Delay, 10, "Delay action")
                };
                script.AddAction(action);
            }

            return script;
        }

        private class PerformanceTestInputSimulator : IInputSimulator
        {
            public void KeyDown(KeyCode key) { }
            public void KeyUp(KeyCode key) { }
            public void KeyPress(KeyCode key, int delay = 50) { }
            public void MouseDown(MouseButton button, int x, int y) { }
            public void MouseUp(MouseButton button, int x, int y) { }
            public void MouseMove(int x, int y) { }
            public void MouseClick(MouseButton button, int x, int y, int delay = 100) { }
            public void MouseDoubleClick(MouseButton button, int x, int y) { }
            public void MouseScroll(int delta) { }
            public void SendMouse(MouseButton button, MouseState state) { }
            public void SendKey(KeyCode key, KeyState state) { }
            public void MoveMouse(int x, int y) { }
        }

        private class PerformanceTestLoggerService : ILoggerService
        {
            public void LogInformation(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message, Exception ex = null) { }
            public void LogDebug(string message) { }
            public void Info(string message) { }
            public void Warning(string message) { }
            public void Error(string message, Exception ex = null) { }
            public void Debug(string message) { }
        }
    }
}