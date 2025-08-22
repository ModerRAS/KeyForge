using System;
using System.Collections.Generic;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;

namespace KeyForge.Tests.Support
{
    /// <summary>
    /// 测试配置类 - 集成测试配置
    /// 原本实现：分散的配置文件
    /// 简化实现：统一的测试配置类
    /// </summary>
    public static class IntegrationTestConfig
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);
        public static readonly int DefaultActionCount = 10;
        public static readonly int LargeActionCount = 1000;
        public static readonly float DefaultSpeedMultiplier = 1.0f;
        public static readonly float FastSpeedMultiplier = 10.0f;
        public static readonly float SlowSpeedMultiplier = 0.1f;
        
        // 测试文件名
        public static readonly string TestScriptFileName = "test_script.json";
        public static readonly string LargeScriptFileName = "large_script.json";
        public static readonly string EmptyScriptFileName = "empty_script.json";
        public static readonly string CorruptedScriptFileName = "corrupted_script.json";
        
        // 测试脚本名称
        public static readonly string DefaultScriptName = "Test Script";
        public static readonly string DefaultScriptDescription = "Test script description";
        public static readonly string ComplexScriptName = "Complex Test Script";
        public static readonly string PerformanceScriptName = "Performance Test Script";
        public static readonly string ConcurrentScriptName = "Concurrent Test Script";
        
        // 错误消息
        public static readonly string ScriptNameEmptyError = "脚本名称不能为空";
        public static readonly string ScriptNotFoundErrorMessage = "获取脚本失败";
        public static readonly string SaveScriptErrorMessage = "保存脚本失败";
        public static readonly string LoadScriptErrorMessage = "加载脚本失败";
        public static readonly string NoActionsToPlayErrorMessage = "没有可播放的动作";
        
        // 状态消息
        public static readonly string StartRecordingMessage = "开始录制脚本";
        public static readonly string StopRecordingMessage = "停止录制";
        public static readonly string StartPlaybackMessage = "开始播放脚本";
        public static readonly string StopPlaybackMessage = "停止播放";
        public static readonly string ScriptCreatedMessage = "创建脚本";
        public static readonly string ScriptSavedMessage = "脚本已保存";
        public static readonly string ScriptLoadedMessage = "脚本已加载";
        public static readonly string ScriptDeletedMessage = "删除脚本";
    }

    /// <summary>
    /// 测试数据工厂 - 集成测试数据
    /// 原本实现：分散的测试数据创建
    /// 简化实现：统一的测试数据工厂
    /// </summary>
    public static class IntegrationTestDataFactory
    {
        public static Script CreateValidScript(string name = null, string description = null, int actionCount = 0)
        {
            var script = new Script(Guid.NewGuid(), 
                name ?? IntegrationTestConfig.DefaultScriptName, 
                description ?? IntegrationTestConfig.DefaultScriptDescription);
            
            if (actionCount > 0)
            {
                for (int i = 0; i < actionCount; i++)
                {
                    var action = CreateValidGameAction(i);
                    script.AddAction(action);
                }
            }
            
            return script;
        }

        public static GameAction CreateValidGameAction(int index = 0)
        {
            var actionTypes = new[] { ActionType.KeyDown, ActionType.KeyUp, ActionType.Delay };
            var actionType = actionTypes[index % actionTypes.Length];
            
            switch (actionType)
            {
                case ActionType.KeyDown:
                case ActionType.KeyUp:
                    return new GameAction(Guid.NewGuid(), actionType, (KeyCode)(65 + (index % 26)), index * 100, $"Test {actionType} {index}");
                case ActionType.Delay:
                    return new GameAction(Guid.NewGuid(), actionType, index * 100, $"Test delay {index}");
                default:
                    return new GameAction(Guid.NewGuid(), actionType, index * 100, $"Test action {index}");
            }
        }

        public static KeyAction CreateValidKeyAction(int index = 0)
        {
            return new KeyAction(
                65 + (index % 26), 
                ((char)(65 + (index % 26))).ToString(), 
                index % 2 == 0, 
                index * 100
            );
        }

        public static List<GameAction> CreateGameActionList(int count)
        {
            var actions = new List<GameAction>();
            for (int i = 0; i < count; i++)
            {
                actions.Add(CreateValidGameAction(i));
            }
            return actions;
        }

        public static List<KeyAction> CreateKeyActionList(int count)
        {
            var actions = new List<KeyAction>();
            for (int i = 0; i < count; i++)
            {
                actions.Add(CreateValidKeyAction(i));
            }
            return actions;
        }

        public static KeyForge.Core.Interfaces.KeyEventArgs CreateKeyEventArgs(System.Windows.Forms.Keys key, bool isKeyDown)
        {
            return new KeyForge.Core.Interfaces.KeyEventArgs(key, isKeyDown);
        }

        public static List<(System.Windows.Forms.Keys Key, bool IsKeyDown)> CreateKeySequence(int count)
        {
            var sequence = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>();
            for (int i = 0; i < count; i++)
            {
                var key = System.Windows.Forms.Keys.A + (i % 26);
                var isKeyDown = i % 2 == 0;
                sequence.Add((key, isKeyDown));
            }
            return sequence;
        }
    }

    /// <summary>
    /// 测试断言助手 - 集成测试断言
    /// 原本实现：重复的断言代码
    /// 简化实现：统一的断言助手
    /// </summary>
    public static class IntegrationTestAssertions
    {
        public static void ShouldBeValidScript(Script script, string expectedName = null, int expectedActionCount = 0)
        {
            script.Should().NotBeNull();
            script.Id.Should().NotBeEmpty();
            script.Name.Should().NotBeNullOrEmpty();
            script.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.Version.Should().BeGreaterThan(0);
            
            if (expectedName != null)
            {
                script.Name.Should().Be(expectedName);
            }
            
            if (expectedActionCount > 0)
            {
                script.Actions.Should().HaveCount(expectedActionCount);
            }
        }

        public static void ShouldBeValidGameAction(GameAction action, ActionType expectedType, int expectedDelay = 0)
        {
            action.Should().NotBeNull();
            action.Id.Should().NotBeEmpty();
            action.Type.Should().Be(expectedType);
            action.Delay.Should().Be(expectedDelay);
            action.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        public static void ShouldBeValidKeyAction(KeyAction action, int expectedKeyCode, bool expectedIsKeyDown)
        {
            action.Should().NotBeNull();
            action.KeyCode.Should().Be(expectedKeyCode);
            action.IsKeyDown.Should().Be(expectedIsKeyDown);
            action.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        public static void ShouldHaveStatusMessage(List<string> messages, string expectedMessage)
        {
            messages.Should().Contain(expectedMessage);
        }

        public static void ShouldHaveErrorMessage(List<string> messages, string expectedMessage)
        {
            messages.Should().Contain(expectedMessage);
        }

        public static void ShouldHaveWorkflowEvent(List<string> events, string expectedEvent)
        {
            events.Should().Contain(e => e.Contains(expectedEvent));
        }

        public static void ShouldHaveScriptWithName(List<Script> scripts, string expectedName)
        {
            scripts.Should().Contain(s => s.Name == expectedName);
        }

        public static void ShouldHavePerformance(TimeSpan duration, TimeSpan maxDuration, string operationName)
        {
            duration.Should().BeLessThan(maxDuration, $"{operationName}应该在{maxDuration.TotalMilliseconds}ms内完成");
        }

        public static void ShouldHaveNoErrors(List<string> errorMessages)
        {
            errorMessages.Should().BeEmpty("不应该有错误消息");
        }

        public static void ShouldHaveDataConsistency<T>(IEnumerable<T> original, IEnumerable<T> loaded, string description = "数据")
        {
            original.Should().BeEquivalentTo(loaded, $"{description}应该保持一致");
        }
    }

    /// <summary>
    /// 测试计时器 - 性能测试辅助
    /// 原本实现：手动计时代码
    /// 简化实现：封装的计时器类
    /// </summary>
    public class TestTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly DateTime _startTime;
        private readonly Action<string, TimeSpan> _logCallback;

        public TestTimer(string operationName, Action<string, TimeSpan> logCallback = null)
        {
            _operationName = operationName;
            _startTime = DateTime.UtcNow;
            _logCallback = logCallback;
        }

        public TimeSpan Elapsed => DateTime.UtcNow - _startTime;

        public void Dispose()
        {
            var elapsed = Elapsed;
            _logCallback?.Invoke(_operationName, elapsed);
        }
    }

    /// <summary>
    /// 测试文件清理器 - 自动清理测试文件
    /// 原本实现：手动文件清理
    /// 简化实现：自动文件清理器
    /// </summary>
    public class TestFileCleaner : IDisposable
    {
        private readonly List<string> _createdFiles;
        private readonly List<string> _createdDirectories;
        private readonly Action<string> _logCallback;

        public TestFileCleaner(Action<string> logCallback = null)
        {
            _createdFiles = new List<string>();
            _createdDirectories = new List<string>();
            _logCallback = logCallback;
        }

        public string CreateTestFile(string directory, string fileName, string content = null)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _createdDirectories.Add(directory);
            }

            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, content ?? $"Test content for {fileName}");
            _createdFiles.Add(filePath);

            _logCallback?.Invoke($"创建测试文件: {filePath}");

            return filePath;
        }

        public string CreateTestDirectory(string parentDirectory, string directoryName)
        {
            var directoryPath = Path.Combine(parentDirectory, directoryName);
            Directory.CreateDirectory(directoryPath);
            _createdDirectories.Add(directoryPath);

            _logCallback?.Invoke($"创建测试目录: {directoryPath}");

            return directoryPath;
        }

        public void Dispose()
        {
            try
            {
                // 清理文件
                foreach (var file in _createdFiles)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        _logCallback?.Invoke($"删除测试文件: {file}");
                    }
                }

                // 清理目录
                foreach (var directory in _createdDirectories.OrderByDescending(d => d.Length))
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                        _logCallback?.Invoke($"删除测试目录: {directory}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logCallback?.Invoke($"清理测试文件失败: {ex.Message}");
            }
        }
    }
}