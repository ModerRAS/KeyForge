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
using KeyForge.Infrastructure.Services;
using KeyForge.Core.Models;
using KeyForge.Core.Domain.Interfaces;
using KeyForge.Core.Domain.Automation;

namespace KeyForge.Tests.IntegrationTests
{
    /// <summary>
    /// 数据集成测试 - 脚本保存和加载的完整流程
    /// 测试脚本序列化、文件存储、数据一致性等
    /// 原本实现：分散的文件操作测试
    /// 简化实现：完整的数据流程测试，确保数据完整性
    /// </summary>
    public class DataIntegrationTests : IntegrationTestBase
    {
        private readonly ScriptService _scriptService;
        private readonly List<KeyAction> _recordedActions;
        private readonly List<string> _statusMessages;

        public DataIntegrationTests(ITestOutputHelper output) : base(output)
        {
            _scriptService = new ScriptService();
            _recordedActions = new List<KeyAction>();
            _statusMessages = new List<string>();

            // 订阅事件
            _scriptService.ActionRecorded += OnActionRecorded;
            _scriptService.StatusChanged += OnStatusChanged;

            RegisterDisposable(_scriptService);
        }

        private void OnActionRecorded(object sender, KeyAction e)
        {
            lock (_recordedActions)
            {
                _recordedActions.Add(e);
                LogDebug($"录制到动作: {e}");
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

        #region 脚本录制测试

        [Fact]
        public void StartRecording_WhenCalled_ShouldClearActions()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));

            // When
            _scriptService.StartRecording();

            // Then
            _scriptService.Actions.Should().BeEmpty();
            _statusMessages.Should().Contain("开始录制脚本...");
        }

        [Fact]
        public void StopRecording_WhenCalled_ShouldRecordActionCount()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(65, "A", false, 200));

            // When
            _scriptService.StopRecording();

            // Then
            _statusMessages.Should().Contain("录制完成，共 2 个动作");
        }

        [Fact]
        public void AddAction_WhenRecording_ShouldCaptureAction()
        {
            // Given
            _scriptService.StartRecording();
            var action = new KeyAction(65, "A", true, 100);

            // When
            _scriptService.AddAction(action);

            // Then
            _scriptService.Actions.Should().HaveCount(1);
            _scriptService.Actions[0].Should().BeEquivalentTo(action);
            _recordedActions.Should().ContainEquivalentOf(action);
        }

        #endregion

        #region 文件保存测试

        [Fact]
        public void SaveToFile_WithValidData_ShouldCreateFile()
        {
            // Given
            var filePath = GetTestFilePath("test_script.json");
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(65, "A", false, 200));

            // When
            _scriptService.SaveToFile(filePath);

            // Then
            ValidateFileExists(filePath, "脚本文件");
            _statusMessages.Should().Contain($"脚本已保存到: {filePath}");
        }

        [Fact]
        public void SaveToFile_WithEmptyActions_ShouldCreateEmptyFile()
        {
            // Given
            var filePath = GetTestFilePath("empty_script.json");
            _scriptService.StartRecording();
            _scriptService.StopRecording();

            // When
            _scriptService.SaveToFile(filePath);

            // Then
            ValidateFileExists(filePath, "空脚本文件");
            var content = File.ReadAllText(filePath);
            content.Should().Be("[]", "空动作列表应该序列化为空数组");
        }

        [Fact]
        public void SaveToFile_WithInvalidPath_ShouldThrowException()
        {
            // Given
            var invalidPath = "Z:\\invalid\\path\\script.json";
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));

            // When & Then
            Action action = () => _scriptService.SaveToFile(invalidPath);
            action.Should().Throw<InvalidOperationException>().WithMessage("保存脚本失败*");
        }

        [Fact]
        public void SaveToFile_WithNestedDirectory_ShouldCreateDirectory()
        {
            // Given
            var nestedPath = GetTestFilePath("nested\\deep\\path\\script.json");
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));

            // When
            _scriptService.SaveToFile(nestedPath);

            // Then
            ValidateFileExists(nestedPath, "嵌套目录脚本文件");
        }

        #endregion

        #region 文件加载测试

        [Fact]
        public void LoadFromFile_WithValidFile_ShouldLoadActions()
        {
            // Given
            var filePath = GetTestFilePath("load_test.json");
            var expectedActions = new List<KeyAction>
            {
                new KeyAction(65, "A", true, 100),
                new KeyAction(65, "A", false, 200),
                new KeyAction(66, "B", true, 300)
            };

            // 创建测试文件
            var json = System.Text.Json.JsonSerializer.Serialize(expectedActions, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);

            // When
            _scriptService.LoadFromFile(filePath);

            // Then
            _scriptService.Actions.Should().HaveCount(3);
            _scriptService.Actions.Should().BeEquivalentTo(expectedActions, options => 
                options.Excluding(a => a.Timestamp));
            _statusMessages.Should().Contain("脚本已加载: 3 个动作");
        }

        [Fact]
        public void LoadFromFile_WithNonExistentFile_ShouldThrowException()
        {
            // Given
            var nonExistentPath = GetTestFilePath("nonexistent.json");

            // When & Then
            Action action = () => _scriptService.LoadFromFile(nonExistentPath);
            action.Should().Throw<InvalidOperationException>().WithMessage("加载脚本失败*");
        }

        [Fact]
        public void LoadFromFile_WithEmptyFile_ShouldLoadEmptyActions()
        {
            // Given
            var filePath = GetTestFilePath("empty.json");
            File.WriteAllText(filePath, "[]");

            // When
            _scriptService.LoadFromFile(filePath);

            // Then
            _scriptService.Actions.Should().BeEmpty();
            _statusMessages.Should().Contain("脚本已加载: 0 个动作");
        }

        [Fact]
        public void LoadFromFile_WithCorruptedFile_ShouldThrowException()
        {
            // Given
            var filePath = GetTestFilePath("corrupted.json");
            File.WriteAllText(filePath, "invalid json content");

            // When & Then
            Action action = () => _scriptService.LoadFromFile(filePath);
            action.Should().Throw<InvalidOperationException>().WithMessage("加载脚本失败*");
        }

        #endregion

        #region 数据一致性测试

        [Fact]
        public void SaveAndLoad_WithComplexData_ShouldMaintainDataIntegrity()
        {
            // Given
            var filePath = GetTestFilePath("integrity_test.json");
            var originalActions = new List<KeyAction>
            {
                new KeyAction(65, "A", true, 100),
                new KeyAction(65, "A", false, 200),
                new KeyAction(66, "B", true, 300),
                new KeyAction(66, "B", false, 400),
                new KeyAction(67, "C", true, 500),
                new KeyAction(67, "C", false, 600)
            };

            // 设置原始数据
            _scriptService.StartRecording();
            foreach (var action in originalActions)
            {
                _scriptService.AddAction(action);
            }

            // When
            _scriptService.SaveToFile(filePath);
            _scriptService.Clear();
            _scriptService.LoadFromFile(filePath);

            // Then
            _scriptService.Actions.Should().HaveCount(originalActions.Count);
            
            // 验证每个动作的数据完整性
            for (int i = 0; i < originalActions.Count; i++)
            {
                var original = originalActions[i];
                var loaded = _scriptService.Actions[i];
                
                loaded.KeyCode.Should().Be(original.KeyCode);
                loaded.KeyName.Should().Be(original.KeyName);
                loaded.IsKeyDown.Should().Be(original.IsKeyDown);
                loaded.Delay.Should().Be(original.Delay);
            }
        }

        [Fact]
        public void SaveAndLoad_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Given
            var filePath = GetTestFilePath("special_chars.json");
            var actionsWithSpecialChars = new List<KeyAction>
            {
                new KeyAction(65, "A", true, 100),
                new KeyAction(66, "B", false, 200),
                new KeyAction(13, "Enter", true, 300),
                new KeyAction(27, "Escape", false, 400)
            };

            _scriptService.StartRecording();
            foreach (var action in actionsWithSpecialChars)
            {
                _scriptService.AddAction(action);
            }

            // When
            _scriptService.SaveToFile(filePath);
            _scriptService.Clear();
            _scriptService.LoadFromFile(filePath);

            // Then
            _scriptService.Actions.Should().HaveCount(actionsWithSpecialChars.Count);
            _scriptService.Actions.Should().BeEquivalentTo(actionsWithSpecialChars, options => 
                options.Excluding(a => a.Timestamp));
        }

        [Fact]
        public void SaveAndLoad_WithLargeData_ShouldPerformWell()
        {
            // Given
            var filePath = GetTestFilePath("large_data.json");
            var largeActionCount = 1000;
            
            _scriptService.StartRecording();
            for (int i = 0; i < largeActionCount; i++)
            {
                var action = new KeyAction(65 + (i % 26), ((char)(65 + (i % 26))).ToString(), i % 2 == 0, i * 10);
                _scriptService.AddAction(action);
            }

            // When
            var saveStartTime = DateTime.UtcNow;
            _scriptService.SaveToFile(filePath);
            var saveEndTime = DateTime.UtcNow;
            var saveDuration = (saveEndTime - saveStartTime).TotalMilliseconds;

            _scriptService.Clear();
            var loadStartTime = DateTime.UtcNow;
            _scriptService.LoadFromFile(filePath);
            var loadEndTime = DateTime.UtcNow;
            var loadDuration = (loadEndTime - loadStartTime).TotalMilliseconds;

            // Then
            _scriptService.Actions.Should().HaveCount(largeActionCount);
            saveDuration.Should().BeLessThan(5000, "保存1000个动作应该在5秒内完成");
            loadDuration.Should().BeLessThan(5000, "加载1000个动作应该在5秒内完成");
            
            LogInfo($"保存{largeActionCount}个动作耗时: {saveDuration:F2}ms");
            LogInfo($"加载{largeActionCount}个动作耗时: {loadDuration:F2}ms");
        }

        #endregion

        #region 脚本统计测试

        [Fact]
        public void GetStats_WhenCalled_ShouldReturnCorrectStatistics()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(65, "A", false, 200));
            _scriptService.AddAction(new KeyAction(66, "B", true, 300));
            _scriptService.AddAction(new KeyAction(66, "B", false, 400));

            // When
            var stats = _scriptService.GetStats();

            // Then
            stats.TotalActions.Should().Be(4);
            stats.KeyDownActions.Should().Be(2);
            stats.KeyUpActions.Should().Be(2);
            stats.Duration.Should().Be(400);
            stats.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GetStats_WhenEmptyActions_ShouldReturnEmptyStatistics()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.StopRecording();

            // When
            var stats = _scriptService.GetStats();

            // Then
            stats.TotalActions.Should().Be(0);
            stats.KeyDownActions.Should().Be(0);
            stats.KeyUpActions.Should().Be(0);
            stats.Duration.Should().Be(0);
        }

        [Fact]
        public void GetStats_WhenActionsModified_ShouldUpdateStatistics()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            var initialStats = _scriptService.GetStats();

            // When
            _scriptService.AddAction(new KeyAction(66, "B", false, 200));
            var updatedStats = _scriptService.GetStats();

            // Then
            initialStats.TotalActions.Should().Be(1);
            updatedStats.TotalActions.Should().Be(2);
            updatedStats.Duration.Should().Be(200);
        }

        #endregion

        #region 脚本播放测试

        [Fact]
        public async Task PlayAsync_WithValidActions_ShouldPlayAllActions()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(65, "A", false, 200));
            _scriptService.AddAction(new KeyAction(66, "B", true, 300));

            var playedActions = new List<KeyAction>();
            _scriptService.ActionPlayed += (sender, action) => playedActions.Add(action);

            // When
            var playTask = _scriptService.PlayAsync(speedMultiplier: 10.0f); // 加速播放
            await playTask;

            // Then
            playedActions.Should().HaveCount(3);
            playedActions.Should().BeEquivalentTo(_scriptService.Actions);
            _statusMessages.Should().Contain("脚本播放完成");
        }

        [Fact]
        public async Task PlayAsync_WithRepeat_ShouldRepeatPlayback()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(65, "A", false, 200));

            var playCount = 0;
            _scriptService.PlaybackCompleted += (sender, args) => playCount++;

            // When
            var playTask = _scriptService.PlayAsync(speedMultiplier: 10.0f, repeat: true);
            await Task.Delay(3000); // 等待几次重复
            _scriptService.StopPlayback();
            await playTask;

            // Then
            playCount.Should().BeGreaterThan(0, "应该至少完成一次播放");
        }

        [Fact]
        public void StopPlayback_WhenPlaying_ShouldStopPlayback()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 1000)); // 长延迟以便测试停止

            // When
            var playTask = _scriptService.PlayAsync(speedMultiplier: 0.1f); // 减速播放
            _scriptService.IsPlaying.Should().BeTrue();
            
            _scriptService.StopPlayback();

            // Then
            _scriptService.IsPlaying.Should().BeFalse();
            _statusMessages.Should().Contain("脚本播放已停止");
        }

        #endregion

        #region 错误处理测试

        [Fact]
        public void PlayAsync_WithEmptyActions_ShouldThrowException()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.StopRecording();

            // When & Then
            Func<Task> action = () => _scriptService.PlayAsync();
            action.Should().ThrowAsync<InvalidOperationException>().WithMessage("没有可播放的动作");
        }

        [Fact]
        public void PlayAsync_WhenAlreadyPlaying_ShouldNotStartAgain()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));

            // When
            var playTask = _scriptService.PlayAsync(speedMultiplier: 10.0f);
            _scriptService.IsPlaying.Should().BeTrue();

            // 尝试再次播放
            Func<Task> secondPlay = () => _scriptService.PlayAsync();

            // Then
            secondPlay.Should().NotThrowAsync("第二次播放调用不应该抛出异常");
            _scriptService.StopPlayback();
        }

        #endregion

        #region 资源管理测试

        [Fact]
        public void Clear_WhenCalled_ShouldClearAllActions()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));
            _scriptService.AddAction(new KeyAction(66, "B", false, 200));

            // When
            _scriptService.Clear();

            // Then
            _scriptService.Actions.Should().BeEmpty();
            _statusMessages.Should().Contain("脚本已清空");
        }

        [Fact]
        public void Dispose_WhenCalled_ShouldCleanUpResources()
        {
            // Given
            _scriptService.StartRecording();
            _scriptService.AddAction(new KeyAction(65, "A", true, 100));

            // When
            _scriptService.Dispose();

            // Then
            Action action = () => _scriptService.StartRecording();
            action.Should().Throw<ObjectDisposedException>();
        }

        #endregion

        #region 完整流程测试

        [Fact]
        public void FullWorkflow_RecordingSavingLoadingPlaying_ShouldWorkCorrectly()
        {
            // Given
            var filePath = GetTestFilePath("full_workflow_test.json");
            var originalActions = new List<KeyAction>
            {
                new KeyAction(65, "A", true, 100),
                new KeyAction(65, "A", false, 200),
                new KeyAction(66, "B", true, 300),
                new KeyAction(66, "B", false, 400),
                new KeyAction(13, "Enter", true, 500),
                new KeyAction(13, "Enter", false, 600)
            };

            // When - 完整的工作流程
            // 1. 录制
            _scriptService.StartRecording();
            foreach (var action in originalActions)
            {
                _scriptService.AddAction(action);
            }
            _scriptService.StopRecording();

            // 2. 保存
            _scriptService.SaveToFile(filePath);

            // 3. 清空
            _scriptService.Clear();

            // 4. 加载
            _scriptService.LoadFromFile(filePath);

            // 5. 验证统计数据
            var stats = _scriptService.GetStats();

            // Then
            _scriptService.Actions.Should().HaveCount(originalActions.Count);
            stats.TotalActions.Should().Be(originalActions.Count);
            stats.KeyDownActions.Should().Be(3);
            stats.KeyUpActions.Should().Be(3);
            stats.Duration.Should().Be(600);

            // 验证文件内容
            ValidateFileExists(filePath, "完整工作流程脚本文件");
            var fileContent = File.ReadAllText(filePath);
            fileContent.Should().Contain("A");
            fileContent.Should().Contain("B");
            fileContent.Should().Contain("Enter");
        }

        [Fact]
        public async Task CompleteDataIntegrity_WithMultipleSaveLoadCycles_ShouldMaintainConsistency()
        {
            // Given
            var filePath1 = GetTestFilePath("cycle1.json");
            var filePath2 = GetTestFilePath("cycle2.json");
            var originalActions = new List<KeyAction>
            {
                new KeyAction(65, "A", true, 100),
                new KeyAction(65, "A", false, 200),
                new KeyAction(66, "B", true, 300),
                new KeyAction(66, "B", false, 400)
            };

            // When - 多次保存加载循环
            // 第一轮
            _scriptService.StartRecording();
            foreach (var action in originalActions)
            {
                _scriptService.AddAction(action);
            }
            _scriptService.SaveToFile(filePath1);
            _scriptService.Clear();
            _scriptService.LoadFromFile(filePath1);

            // 第二轮
            _scriptService.SaveToFile(filePath2);
            _scriptService.Clear();
            _scriptService.LoadFromFile(filePath2);

            // 第三轮
            _scriptService.SaveToFile(filePath1);
            _scriptService.Clear();
            _scriptService.LoadFromFile(filePath1);

            // Then
            _scriptService.Actions.Should().HaveCount(originalActions.Count);
            _scriptService.Actions.Should().BeEquivalentTo(originalActions, options => 
                options.Excluding(a => a.Timestamp));

            // 验证所有文件都存在且内容一致
            ValidateFileExists(filePath1, "第一轮保存文件");
            ValidateFileExists(filePath2, "第二轮保存文件");

            var content1 = File.ReadAllText(filePath1);
            var content2 = File.ReadAllText(filePath2);
            content1.Should().Be(content2, "多次保存加载后内容应该保持一致");
        }

        #endregion
    }
}