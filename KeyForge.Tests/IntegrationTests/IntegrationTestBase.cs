using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Moq;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using KeyForge.Core.Domain.Interfaces;
using KeyForge.Core.Domain.Automation;

namespace KeyForge.Tests.IntegrationTests
{
    /// <summary>
    /// 集成测试基类
    /// 提供集成测试的基础设施和通用方法
    /// 原本实现：复杂的集成测试框架
    /// 简化实现：轻量级的集成测试基类
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly string TestDirectory;
        protected readonly string TestDataDirectory;
        protected readonly Mock<ILoggerService> MockLogger;
        protected readonly List<IDisposable> Disposables;

        protected IntegrationTestBase(ITestOutputHelper output)
        {
            Output = output;
            Disposables = new List<IDisposable>();
            
            // 创建测试目录
            TestDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Integration_Test_{Guid.NewGuid()}");
            TestDataDirectory = Path.Combine(TestDirectory, "TestData");
            Directory.CreateDirectory(TestDirectory);
            Directory.CreateDirectory(TestDataDirectory);
            
            // 创建Mock对象
            MockLogger = new Mock<ILoggerService>();
            MockLogger.Setup(x => x.LogInformation(It.IsAny<string>()))
                .Callback<string>(msg => Output.WriteLine($"[INFO] {msg}"));
            MockLogger.Setup(x => x.LogWarning(It.IsAny<string>()))
                .Callback<string>(msg => Output.WriteLine($"[WARN] {msg}"));
            MockLogger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
                .Callback<string, Exception>((msg, ex) => Output.WriteLine($"[ERROR] {msg}: {ex?.Message}"));
            MockLogger.Setup(x => x.LogDebug(It.IsAny<string>()))
                .Callback<string>(msg => Output.WriteLine($"[DEBUG] {msg}"));
            
            Output.WriteLine($"集成测试目录创建: {TestDirectory}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // 清理所有Disposable对象
                    foreach (var disposable in Disposables)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Output.WriteLine($"清理Disposable对象失败: {ex.Message}");
                        }
                    }
                    
                    // 清理测试目录
                    if (Directory.Exists(TestDirectory))
                    {
                        Directory.Delete(TestDirectory, true);
                        Output.WriteLine($"集成测试目录清理: {TestDirectory}");
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"集成测试清理失败: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region 测试数据创建方法

        protected Script CreateTestScript(string name = "Test Script", string description = "Test Description")
        {
            var script = new Script(Guid.NewGuid(), name, description);
            
            // 添加一些测试动作
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 500, "Wait 500ms"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 0, "Release A"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 100, 0, "Left Click"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseUp, MouseButton.Left, 100, 100, 0, "Release Left Click"));
            
            return script;
        }

        protected GameAction CreateTestGameAction(ActionType type = ActionType.KeyDown, KeyCode key = KeyCode.A)
        {
            return new GameAction(Guid.NewGuid(), type, key, 100, $"Test {type} action");
        }

        protected GameAction CreateTestMouseAction(ActionType type = ActionType.MouseDown, MouseButton button = MouseButton.Left)
        {
            return new GameAction(Guid.NewGuid(), type, button, 100, 100, 0, $"Test {type} action");
        }

        protected ActionSequence CreateTestActionSequence()
        {
            var actions = new List<GameAction>
            {
                CreateTestGameAction(ActionType.KeyDown, KeyCode.W),
                CreateTestGameAction(ActionType.Delay),
                CreateTestGameAction(ActionType.KeyUp, KeyCode.W),
                CreateTestMouseAction(ActionType.MouseDown, MouseButton.Left),
                CreateTestMouseAction(ActionType.MouseUp, MouseButton.Left)
            };
            return new ActionSequence(actions);
        }

        #endregion

        #region 文件操作辅助方法

        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(TestDataDirectory, fileName);
        }

        protected void CreateTestFile(string fileName, string content)
        {
            var filePath = GetTestFilePath(fileName);
            File.WriteAllText(filePath, content);
        }

        protected string ReadTestFile(string fileName)
        {
            var filePath = GetTestFilePath(fileName);
            return File.ReadAllText(filePath);
        }

        protected bool TestFileExists(string fileName)
        {
            var filePath = GetTestFilePath(fileName);
            return File.Exists(filePath);
        }

        protected void DeleteTestFile(string fileName)
        {
            var filePath = GetTestFilePath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region 脚本序列化辅助方法

        protected string SerializeScript(Script script)
        {
            // 简化的脚本序列化
            var actions = script.Actions.Select(a => new
            {
                a.Id,
                Type = a.Type.ToString(),
                a.Key,
                a.Button,
                a.X,
                a.Y,
                a.Delay,
                a.Description
            }).ToList();

            var scriptData = new
            {
                script.Id,
                script.Name,
                script.Description,
                Status = script.Status.ToString(),
                script.CreatedAt,
                script.UpdatedAt,
                script.Version,
                Actions = actions
            };

            return System.Text.Json.JsonSerializer.Serialize(scriptData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        protected Script DeserializeScript(string json)
        {
            var scriptData = System.Text.Json.JsonSerializer.Deserialize<JsonScriptData>(json);
            if (scriptData == null)
                throw new InvalidOperationException("无法反序列化脚本数据");

            var script = new Script(scriptData.Id, scriptData.Name, scriptData.Description);
            
            foreach (var actionData in scriptData.Actions)
            {
                GameAction action;
                if (Enum.TryParse<ActionType>(actionData.Type, out var actionType))
                {
                    switch (actionType)
                    {
                        case ActionType.KeyDown:
                        case ActionType.KeyUp:
                            action = new GameAction(actionData.Id, actionType, actionData.Key, actionData.Delay, actionData.Description);
                            break;
                        case ActionType.MouseDown:
                        case ActionType.MouseUp:
                        case ActionType.MouseMove:
                            action = new GameAction(actionData.Id, actionType, actionData.Button, actionData.X, actionData.Y, actionData.Delay, actionData.Description);
                            break;
                        case ActionType.Delay:
                            action = new GameAction(actionData.Id, actionType, actionData.Delay, actionData.Description);
                            break;
                        default:
                            action = new GameAction(actionData.Id, actionType, actionData.Delay, actionData.Description);
                            break;
                    }
                    script.AddAction(action);
                }
            }

            return script;
        }

        private class JsonScriptData
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int Version { get; set; }
            public List<JsonActionData> Actions { get; set; }
        }

        private class JsonActionData
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public KeyCode Key { get; set; }
            public MouseButton Button { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Delay { get; set; }
            public string Description { get; set; }
        }

        #endregion

        #region 验证辅助方法

        protected void ValidateScript(Script script, string expectedName = null, int expectedActionCount = 0)
        {
            script.Should().NotBeNull();
            script.Id.Should().NotBeEmpty();
            script.Name.Should().NotBeNullOrEmpty();
            
            if (expectedName != null)
            {
                script.Name.Should().Be(expectedName);
            }
            
            script.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            script.Version.Should().BeGreaterThan(0);
            
            if (expectedActionCount > 0)
            {
                script.Actions.Should().HaveCount(expectedActionCount);
            }
        }

        protected void ValidateGameAction(GameAction action, ActionType expectedType, int expectedDelay = 0)
        {
            action.Should().NotBeNull();
            action.Id.Should().NotBeEmpty();
            action.Type.Should().Be(expectedType);
            action.Delay.Should().Be(expectedDelay);
            action.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        protected void ValidateActionSequence(ActionSequence sequence, int expectedActionCount)
        {
            sequence.Should().NotBeNull();
            sequence.Actions.Should().HaveCount(expectedActionCount);
            sequence.ActionCount.Should().Be(expectedActionCount);
            sequence.TotalDuration.Should().BeGreaterThanOrEqualTo(0);
        }

        protected void ValidateFileExists(string filePath, string description = "文件")
        {
            File.Exists(filePath).Should().BeTrue($"{description}应该存在: {filePath}");
        }

        protected void ValidateFileContent(string filePath, string expectedContent, string description = "文件内容")
        {
            var content = File.ReadAllText(filePath);
            content.Should().Be(expectedContent, $"{description}应该匹配预期内容");
        }

        #endregion

        #region 异步操作辅助方法

        protected async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName = "操作")
        {
            var timeoutTask = Task.Delay(timeout);
            var operationTask = operation();

            var completedTask = await Task.WhenAny(timeoutTask, operationTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"{operationName}超时: {timeout.TotalMilliseconds}ms");
            }

            return await operationTask;
        }

        protected async Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName = "操作")
        {
            var timeoutTask = Task.Delay(timeout);
            var operationTask = operation();

            var completedTask = await Task.WhenAny(timeoutTask, operationTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"{operationName}超时: {timeout.TotalMilliseconds}ms");
            }

            await operationTask;
        }

        #endregion

        #region 日志辅助方法

        protected void LogInfo(string message)
        {
            Output.WriteLine($"[INFO] {message}");
        }

        protected void LogWarning(string message)
        {
            Output.WriteLine($"[WARN] {message}");
        }

        protected void LogError(string message, Exception ex = null)
        {
            Output.WriteLine($"[ERROR] {message}: {ex?.Message}");
        }

        protected void LogDebug(string message)
        {
            Output.WriteLine($"[DEBUG] {message}");
        }

        #endregion

        #region 注册Disposable对象

        protected void RegisterDisposable(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        #endregion
    }
}