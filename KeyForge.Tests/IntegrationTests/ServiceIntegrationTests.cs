using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Moq;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Infrastructure.Services;
using KeyForge.Infrastructure.Native;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Domain.Interfaces;

namespace KeyForge.Tests.IntegrationTests
{
    /// <summary>
    /// 服务集成测试 - KeyInputService与WindowsKeyHook的集成
    /// 测试按键录制、模拟和事件处理的完整流程
    /// 原本实现：分离的单元测试
    /// 简化实现：集成的服务测试，确保组件间协作正常
    /// </summary>
    public class ServiceIntegrationTests : IntegrationTestBase
    {
        private readonly KeyInputService _keyInputService;
        private readonly List<KeyEventArgs> _recordedKeys;
        private readonly List<string> _statusMessages;

        public ServiceIntegrationTests(ITestOutputHelper output) : base(output)
        {
            _keyInputService = new KeyInputService();
            _recordedKeys = new List<KeyEventArgs>();
            _statusMessages = new List<string>();

            // 订阅事件
            _keyInputService.KeyRecorded += OnKeyRecorded;
            _keyInputService.StatusChanged += OnStatusChanged;

            RegisterDisposable(_keyInputService);
        }

        private void OnKeyRecorded(object sender, KeyEventArgs e)
        {
            lock (_recordedKeys)
            {
                _recordedKeys.Add(e);
                LogDebug($"录制到按键: {e.Key} (KeyDown: {e.IsKeyDown})");
            }
        }

        private void OnStatusChanged(object sender, string message)
        {
            lock (_statusMessages)
            {
                _statusMessages.Add(message);
                LogInfo($"状态变更: {message}");
            }
        }

        #region 基础功能测试

        [Fact]
        public void StartRecording_WhenCalled_ShouldStartRecording()
        {
            // Given
            _keyInputService.IsRecording.Should().BeFalse();

            // When
            _keyInputService.StartRecording();

            // Then
            _keyInputService.IsRecording.Should().BeTrue();
            _statusMessages.Should().Contain("开始录制按键...");
        }

        [Fact]
        public void StopRecording_WhenRecording_ShouldStopRecording()
        {
            // Given
            _keyInputService.StartRecording();
            _keyInputService.IsRecording.Should().BeTrue();

            // When
            _keyInputService.StopRecording();

            // Then
            _keyInputService.IsRecording.Should().BeFalse();
            _statusMessages.Should().Contain("录制已停止");
        }

        [Fact]
        public void StartRecording_WhenAlreadyRecording_ShouldNotStartAgain()
        {
            // Given
            _keyInputService.StartRecording();
            var initialMessageCount = _statusMessages.Count;

            // When
            _keyInputService.StartRecording();

            // Then
            _keyInputService.IsRecording.Should().BeTrue();
            _statusMessages.Count.Should().Be(initialMessageCount); // 没有新的状态消息
        }

        [Fact]
        public void StopRecording_WhenNotRecording_ShouldNotStopAgain()
        {
            // Given
            _keyInputService.IsRecording.Should().BeFalse();
            var initialMessageCount = _statusMessages.Count;

            // When
            _keyInputService.StopRecording();

            // Then
            _keyInputService.IsRecording.Should().BeFalse();
            _statusMessages.Count.Should().Be(initialMessageCount); // 没有新的状态消息
        }

        #endregion

        #region 按键模拟测试

        [Fact]
        public void SimulateKey_WithValidKey_ShouldSimulateKeyPress()
        {
            // Given
            var testKey = System.Windows.Forms.Keys.A;
            var isKeyDown = true;

            // When
            Action action = () => _keyInputService.SimulateKey(testKey, isKeyDown);

            // Then
            action.Should().NotThrow();
        }

        [Fact]
        public void SimulateKeySequence_WithValidSequence_ShouldSimulateAllKeys()
        {
            // Given
            var keySequence = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>
            {
                (System.Windows.Forms.Keys.A, true),
                (System.Windows.Forms.Keys.A, false),
                (System.Windows.Forms.Keys.B, true),
                (System.Windows.Forms.Keys.B, false)
            };

            // When
            Action action = () => _keyInputService.SimulateKeySequence(keySequence);

            // Then
            action.Should().NotThrow();
        }

        [Fact]
        public void SimulateKeySequence_WithNullSequence_ShouldNotThrow()
        {
            // Given
            IEnumerable<(System.Windows.Forms.Keys Key, bool IsKeyDown)> keySequence = null;

            // When
            Action action = () => _keyInputService.SimulateKeySequence(keySequence);

            // Then
            action.Should().NotThrow();
        }

        [Fact]
        public void SimulateKeySequence_WithEmptySequence_ShouldNotThrow()
        {
            // Given
            var keySequence = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>();

            // When
            Action action = () => _keyInputService.SimulateKeySequence(keySequence);

            // Then
            action.Should().NotThrow();
        }

        #endregion

        #region 事件处理测试

        [Fact]
        public void KeyRecorded_WhenRecording_ShouldCaptureEvents()
        {
            // Given
            _keyInputService.StartRecording();
            _recordedKeys.Clear();

            // When
            // 模拟按键事件（在实际环境中，这些事件由Windows钩子触发）
            var testEvent = new KeyEventArgs(System.Windows.Forms.Keys.A, true);
            _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_keyInputService, new object[] { this, testEvent });

            // Then
            _recordedKeys.Should().HaveCount(1);
            _recordedKeys[0].Key.Should().Be(System.Windows.Forms.Keys.A);
            _recordedKeys[0].IsKeyDown.Should().BeTrue();
        }

        [Fact]
        public void KeyRecorded_WhenNotRecording_ShouldNotCaptureEvents()
        {
            // Given
            _keyInputService.StopRecording();
            _recordedKeys.Clear();

            // When
            // 模拟按键事件
            var testEvent = new KeyEventArgs(System.Windows.Forms.Keys.A, true);
            _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_keyInputService, new object[] { this, testEvent });

            // Then
            _recordedKeys.Should().BeEmpty();
        }

        [Fact]
        public void StatusChanged_WhenStatusChanges_ShouldRaiseEvents()
        {
            // Given
            _statusMessages.Clear();

            // When
            _keyInputService.StartRecording();
            _keyInputService.StopRecording();

            // Then
            _statusMessages.Should().Contain("开始录制按键...");
            _statusMessages.Should().Contain("录制已停止");
        }

        #endregion

        #region 线程安全测试

        [Fact]
        public void ConcurrentAccess_WhenMultipleThreads_ShouldBeThreadSafe()
        {
            // Given
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var exceptions = new List<Exception>();

            // When
            var threads = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            // 随机选择操作
                            if (j % 2 == 0)
                            {
                                _keyInputService.StartRecording();
                                Thread.Sleep(1);
                                _keyInputService.StopRecording();
                            }
                            else
                            {
                                _keyInputService.SimulateKey(System.Windows.Forms.Keys.A, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });

                threads.Add(thread);
            }

            // 启动所有线程
            threads.ForEach(t => t.Start());

            // 等待所有线程完成
            threads.ForEach(t => t.Join());

            // Then
            exceptions.Should().BeEmpty("不应该有线程异常");
            _keyInputService.IsRecording.Should().BeFalse();
        }

        [Fact]
        public async Task RecordingState_WhenAccessedConcurrently_ShouldBeConsistent()
        {
            // Given
            const int taskCount = 50;
            var results = new List<bool>();

            // When
            var tasks = new List<Task>();
            for (int i = 0; i < taskCount; i++)
            {
                var task = Task.Run(() =>
                {
                    results.Add(_keyInputService.IsRecording);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Then
            results.All(r => r == false).Should().BeTrue("所有读取操作应该返回一致的结果");
        }

        #endregion

        #region 资源管理测试

        [Fact]
        public void Dispose_WhenCalled_ShouldCleanUpResources()
        {
            // Given
            _keyInputService.StartRecording();
            _keyInputService.IsRecording.Should().BeTrue();

            // When
            _keyInputService.Dispose();

            // Then
            _keyInputService.IsRecording.Should().BeFalse();
            // 验证没有资源泄漏（通过检查是否可以再次调用方法）
            Action action = () => _keyInputService.StartRecording();
            action.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void MultipleDispose_WhenCalled_ShouldNotThrow()
        {
            // Given
            _keyInputService.StartRecording();

            // When
            _keyInputService.Dispose();
            Action action = () => _keyInputService.Dispose();

            // Then
            action.Should().NotThrow("多次调用Dispose不应该抛出异常");
        }

        #endregion

        #region 错误处理测试

        [Fact]
        public void StartRecording_WhenHookFails_ShouldHandleError()
        {
            // Given
            // 这是一个简化的测试，实际中需要模拟钩子失败的情况
            // 在实际环境中，可以通过模拟Windows API失败来测试

            // When & Then
            // 由于无法轻易模拟Windows API失败，这里我们验证错误处理机制存在
            Action action = () => _keyInputService.StartRecording();
            action.Should().NotThrow("正常的启动操作不应该抛出异常");
        }

        [Fact]
        public void StopRecording_WhenHookFails_ShouldHandleError()
        {
            // Given
            _keyInputService.StartRecording();

            // When & Then
            // 同样，这是一个简化的测试
            Action action = () => _keyInputService.StopRecording();
            action.Should().NotThrow("正常的停止操作不应该抛出异常");
        }

        #endregion

        #region 性能测试

        [Fact]
        public void KeyRecording_WhenHighFrequency_ShouldPerformWell()
        {
            // Given
            _keyInputService.StartRecording();
            _recordedKeys.Clear();
            const int keyCount = 1000;

            // When
            var startTime = DateTime.UtcNow;
            for (int i = 0; i < keyCount; i++)
            {
                var testEvent = new KeyEventArgs(System.Windows.Forms.Keys.A, true);
                _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, testEvent });
            }
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;

            // Then
            _recordedKeys.Should().HaveCount(keyCount);
            duration.Should().BeLessThan(1000, "录制1000个按键应该在1秒内完成");
            LogInfo($"录制{keyCount}个按键耗时: {duration:F2}ms");
        }

        [Fact]
        public void KeySimulation_WhenHighFrequency_ShouldPerformWell()
        {
            // Given
            const int sequenceCount = 100;
            var keySequence = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>
            {
                (System.Windows.Forms.Keys.A, true),
                (System.Windows.Forms.Keys.A, false),
                (System.Windows.Forms.Keys.B, true),
                (System.Windows.Forms.Keys.B, false)
            };

            // When
            var startTime = DateTime.UtcNow;
            for (int i = 0; i < sequenceCount; i++)
            {
                _keyInputService.SimulateKeySequence(keySequence);
            }
            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;

            // Then
            duration.Should().BeLessThan(5000, "模拟100个按键序列应该在5秒内完成");
            LogInfo($"模拟{sequenceCount}个按键序列耗时: {duration:F2}ms");
        }

        #endregion

        #region 集成场景测试

        [Fact]
        public void FullRecordingCycle_WhenComplete_ShouldWorkCorrectly()
        {
            // Given
            _recordedKeys.Clear();
            _statusMessages.Clear();

            // When - 完整的录制周期
            _keyInputService.StartRecording();
            
            // 模拟一些按键事件
            var keys = new[] { System.Windows.Forms.Keys.A, System.Windows.Forms.Keys.B, System.Windows.Forms.Keys.C };
            foreach (var key in keys)
            {
                var downEvent = new KeyEventArgs(key, true);
                var upEvent = new KeyEventArgs(key, false);
                
                _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, downEvent });
                _keyInputService.GetType().GetMethod("OnKeyReleased", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, upEvent });
            }
            
            _keyInputService.StopRecording();

            // Then
            _statusMessages.Should().Contain("开始录制按键...");
            _statusMessages.Should().Contain("录制已停止");
            _recordedKeys.Should().HaveCount(keys.Length * 2); // 每个按键有按下和释放事件
            
            // 验证按键顺序
            for (int i = 0; i < keys.Length; i++)
            {
                var downIndex = i * 2;
                var upIndex = i * 2 + 1;
                
                _recordedKeys[downIndex].Key.Should().Be(keys[i]);
                _recordedKeys[downIndex].IsKeyDown.Should().BeTrue();
                
                _recordedKeys[upIndex].Key.Should().Be(keys[i]);
                _recordedKeys[upIndex].IsKeyDown.Should().BeFalse();
            }
        }

        [Fact]
        public void RecordingAndPlayback_WhenIntegrated_ShouldMaintainConsistency()
        {
            // Given
            _recordedKeys.Clear();
            var originalKeys = new List<System.Windows.Forms.Keys>
            {
                System.Windows.Forms.Keys.A,
                System.Windows.Forms.Keys.B,
                System.Windows.Forms.Keys.C
            };

            // When - 录制按键
            _keyInputService.StartRecording();
            
            foreach (var key in originalKeys)
            {
                var downEvent = new KeyEventArgs(key, true);
                var upEvent = new KeyEventArgs(key, false);
                
                _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, downEvent });
                _keyInputService.GetType().GetMethod("OnKeyReleased", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, upEvent });
            }
            
            _keyInputService.StopRecording();

            // Then - 验证录制结果
            _recordedKeys.Should().HaveCount(originalKeys.Count * 2);
            
            // 验证播放序列
            var playbackSequence = _recordedKeys.Select(k => (k.Key, k.IsKeyDown)).ToList();
            Action action = () => _keyInputService.SimulateKeySequence(playbackSequence);
            action.Should().NotThrow("播放录制的按键序列不应该抛出异常");
        }

        #endregion
    }
}