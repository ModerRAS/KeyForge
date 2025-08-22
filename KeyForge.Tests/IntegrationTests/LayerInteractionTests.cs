using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Moq;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Presentation.Interfaces;
using KeyForge.Presentation.Services;
using KeyForge.Presentation.Controllers;
using KeyForge.Presentation.Views;
using KeyForge.Core.Domain.Interfaces;

namespace KeyForge.Tests.IntegrationTests
{
    /// <summary>
    /// 层间交互测试 - UI层与服务层的交互
    /// 测试UI控制器、应用服务和视图之间的完整交互流程
    /// 原本实现：分离的UI测试和服务测试
    /// 简化实现：完整的层间交互测试，确保各层协作正常
    /// </summary>
    public class LayerInteractionTests : IntegrationTestBase
    {
        private readonly ApplicationService _applicationService;
        private readonly MockUIView _mockView;
        private readonly UIController _uiController;
        private readonly List<string> _uiEvents;

        public LayerInteractionTests(ITestOutputHelper output) : base(output)
        {
            _applicationService = new ApplicationService(MockLogger.Object);
            _mockView = new MockUIView();
            _uiController = new UIController(_applicationService, _mockView);
            _uiEvents = new List<string>();

            // 订阅UI控制器事件
            _uiController.StatusChanged += (sender, message) =>
            {
                _uiEvents.Add($"Status: {message}");
                LogDebug($"UI事件 - 状态变更: {message}");
            };

            _uiController.ScriptCreated += (sender, script) =>
            {
                _uiEvents.Add($"ScriptCreated: {script.Name}");
                LogDebug($"UI事件 - 脚本创建: {script.Name}");
            };

            _uiController.ScriptUpdated += (sender, script) =>
            {
                _uiEvents.Add($"ScriptUpdated: {script.Name}");
                LogDebug($"UI事件 - 脚本更新: {script.Name}");
            };

            _uiController.ScriptDeleted += (sender, scriptId) =>
            {
                _uiEvents.Add($"ScriptDeleted: {scriptId}");
                LogDebug($"UI事件 - 脚本删除: {scriptId}");
            };

            _uiController.ActionRecorded += (sender, action) =>
            {
                _uiEvents.Add($"ActionRecorded: {action.Type}");
                LogDebug($"UI事件 - 动作录制: {action.Type}");
            };

            _uiController.RecordingStateChanged += (sender, isRecording) =>
            {
                _uiEvents.Add($"RecordingStateChanged: {isRecording}");
                LogDebug($"UI事件 - 录制状态变更: {isRecording}");
            };

            _uiController.PlaybackStateChanged += (sender, isPlaying) =>
            {
                _uiEvents.Add($"PlaybackStateChanged: {isPlaying}");
                LogDebug($"UI事件 - 播放状态变更: {isPlaying}");
            };

            RegisterDisposable(_applicationService);
            RegisterDisposable(_uiController);
        }

        #region 基础层间交互测试

        [Fact]
        public async Task CreateScript_ThroughUI_ShouldCreateScriptAndUpdateView()
        {
            // Given
            var scriptName = "Test Script";
            var scriptDescription = "Test Description";

            // When
            var result = await _uiController.CreateScriptAsync(scriptName, scriptDescription);

            // Then
            result.Should().BeTrue("创建脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("创建脚本: Test Script").Should().BeTrue();
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be(scriptName);
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ScriptCreated") && e.Contains(scriptName));
            _uiEvents.Should().Contain(e => e.Contains("Status") && e.Contains(scriptName));
        }

        [Fact]
        public async Task GetAllScripts_ThroughUI_ShouldDisplayScriptList()
        {
            // Given
            await _uiController.CreateScriptAsync("Script 1", "Description 1");
            await _uiController.CreateScriptAsync("Script 2", "Description 2");
            _mockView.Reset();

            // When
            var scripts = await _uiController.GetAllScriptsAsync();

            // Then
            scripts.Should().HaveCount(2);
            _mockView.DisplayedScripts.Should().HaveCount(2);
            _mockView.HasScriptWithName("Script 1").Should().BeTrue();
            _mockView.HasScriptWithName("Script 2").Should().BeTrue();
        }

        [Fact]
        public async Task GetScript_ThroughUI_ShouldDisplayScriptDetails()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When
            var script = await _uiController.GetScriptAsync(scriptId);

            // Then
            script.Should().NotBeNull();
            script.Name.Should().Be("Test Script");
            _mockView.CurrentScript.Should().NotBeNull();
            _mockView.CurrentScript.Name.Should().Be("Test Script");
        }

        [Fact]
        public async Task UpdateScript_ThroughUI_ShouldUpdateScriptAndRefreshView()
        {
            // Given
            await _uiController.CreateScriptAsync("Original Name", "Original Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When
            var result = await _uiController.UpdateScriptAsync(scriptId, "Updated Name", "Updated Description");

            // Then
            result.Should().BeTrue("更新脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("更新脚本: Updated Name").Should().BeTrue();
            _mockView.CurrentScript.Name.Should().Be("Updated Name");
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ScriptUpdated") && e.Contains("Updated Name"));
        }

        [Fact]
        public async Task DeleteScript_ThroughUI_ShouldDeleteScriptAndUpdateView()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When
            var result = await _uiController.DeleteScriptAsync(scriptId);

            // Then
            result.Should().BeTrue("删除脚本应该成功");
            
            // 验证UI状态
            _mockView.DisplayedScripts.Should().BeEmpty();
            _mockView.CurrentScript.Should().BeNull();
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ScriptDeleted") && e.Contains(scriptId.ToString()));
        }

        #endregion

        #region 录制控制交互测试

        [Fact]
        public async Task StartRecording_ThroughUI_ShouldStartRecordingAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When
            var result = await _uiController.StartRecordingAsync(scriptId);

            // Then
            result.Should().BeTrue("开始录制应该成功");
            
            // 验证UI状态
            _mockView.IsRecording.Should().BeTrue();
            _mockView.HasStatusMessage("开始录制脚本: Test Script").Should().BeTrue();
            
            // 验证录制状态
            var isRecording = await _uiController.IsRecordingAsync();
            isRecording.Should().BeTrue();
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("RecordingStateChanged: True"));
        }

        [Fact]
        public async Task StopRecording_ThroughUI_ShouldStopRecordingAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            await _uiController.StartRecordingAsync(scriptId);
            _mockView.Reset();

            // When
            var result = await _uiController.StopRecordingAsync();

            // Then
            result.Should().BeTrue("停止录制应该成功");
            
            // 验证UI状态
            _mockView.IsRecording.Should().BeFalse();
            _mockView.HasStatusMessage("停止录制脚本: Test Script").Should().BeTrue();
            
            // 验证录制状态
            var isRecording = await _uiController.IsRecordingAsync();
            isRecording.Should().BeFalse();
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("RecordingStateChanged: False"));
        }

        [Fact]
        public async Task RecordActions_ThroughUI_ShouldCaptureActionsAndUpdateProgress()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            await _uiController.StartRecordingAsync(scriptId);
            _mockView.Reset();

            // When
            var action1 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A");
            var action2 = new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 0, "Release A");

            await _applicationService.AddActionAsync(scriptId, action1);
            await _applicationService.AddActionAsync(scriptId, action2);

            // Then
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ActionRecorded: KeyDown"));
            _uiEvents.Should().Contain(e => e.Contains("ActionRecorded: KeyUp"));
            
            // 验证进度更新
            _mockView.CurrentProgress.Should().BeGreaterThan(0);
        }

        #endregion

        #region 播放控制交互测试

        [Fact]
        public async Task StartPlayback_ThroughUI_ShouldStartPlayingAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            
            // 添加一些动作到脚本
            var script = await _uiController.GetScriptAsync(scriptId);
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 0, "Release A"));
            
            _mockView.Reset();

            // When
            var result = await _uiController.StartPlaybackAsync(scriptId, speedMultiplier: 10.0f);

            // Then
            result.Should().BeTrue("开始播放应该成功");
            
            // 验证UI状态
            _mockView.IsPlaying.Should().BeTrue();
            _mockView.HasStatusMessage("开始播放脚本: Test Script").Should().BeTrue();
            
            // 验证播放状态
            var isPlaying = await _uiController.IsPlayingAsync();
            isPlaying.Should().BeTrue();
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("PlaybackStateChanged: True"));
        }

        [Fact]
        public async Task StopPlayback_ThroughUI_ShouldStopPlayingAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            
            // 添加一些动作到脚本
            var script = await _uiController.GetScriptAsync(scriptId);
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 1000, "Press A"));
            
            await _uiController.StartPlaybackAsync(scriptId, speedMultiplier: 0.1f);
            _mockView.Reset();

            // When
            var result = await _uiController.StopPlaybackAsync();

            // Then
            result.Should().BeTrue("停止播放应该成功");
            
            // 验证UI状态
            _mockView.IsPlaying.Should().BeFalse();
            _mockView.HasStatusMessage("停止播放脚本").Should().BeTrue();
            
            // 验证播放状态
            var isPlaying = await _uiController.IsPlayingAsync();
            isPlaying.Should().BeFalse();
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("PlaybackStateChanged: False"));
        }

        #endregion

        #region 文件操作交互测试

        [Fact]
        public async Task SaveScript_ThroughUI_ShouldSaveScriptAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            var filePath = GetTestFilePath("ui_save_test.json");
            _mockView.Reset();

            // When
            var result = await _uiController.SaveScriptAsync(scriptId, filePath);

            // Then
            result.Should().BeTrue("保存脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("脚本已保存: Test Script").Should().BeTrue();
            
            // 验证文件存在
            ValidateFileExists(filePath, "UI保存的脚本文件");
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("Status") && e.Contains("脚本已保存"));
        }

        [Fact]
        public async Task LoadScript_ThroughUI_ShouldLoadScriptAndUpdateUI()
        {
            // Given
            var filePath = GetTestFilePath("ui_load_test.json");
            
            // 首先创建一个脚本文件
            await _uiController.CreateScriptAsync("Original Script", "Original Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            await _uiController.SaveScriptAsync(scriptId, filePath);
            
            _mockView.Reset();
            _uiEvents.Clear();

            // When
            var result = await _uiController.LoadScriptAsync(filePath);

            // Then
            result.Should().BeTrue("加载脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("脚本已加载: Original Script").Should().BeTrue();
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be("Original Script");
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ScriptCreated") && e.Contains("Original Script"));
        }

        [Fact]
        public async Task ExportScript_ThroughUI_ShouldExportScriptAndUpdateUI()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When
            var result = await _uiController.ExportScriptAsync(scriptId, "json");

            // Then
            result.Should().BeTrue("导出脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("脚本已保存: Test Script").Should().BeTrue();
            
            // 验证文件存在
            var expectedFilePath = GetTestFilePath("Test Script.json");
            ValidateFileExists(expectedFilePath, "UI导出的脚本文件");
        }

        [Fact]
        public async Task ImportScript_ThroughUI_ShouldImportScriptAndUpdateUI()
        {
            // Given
            var filePath = GetTestFilePath("ui_import_test.json");
            
            // 首先创建一个脚本文件
            await _uiController.CreateScriptAsync("Import Script", "Import Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            await _uiController.SaveScriptAsync(scriptId, filePath);
            
            _mockView.Reset();
            _uiEvents.Clear();

            // When
            var result = await _uiController.ImportScriptAsync(filePath, "json");

            // Then
            result.Should().BeTrue("导入脚本应该成功");
            
            // 验证UI状态
            _mockView.HasStatusMessage("脚本已加载: Import Script").Should().BeTrue();
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be("Import Script");
            
            // 验证UI事件
            _uiEvents.Should().Contain(e => e.Contains("ScriptCreated") && e.Contains("Import Script"));
        }

        #endregion

        #region 错误处理交互测试

        [Fact]
        public async Task CreateScript_WithInvalidName_ShouldShowError()
        {
            // Given
            var invalidName = "";

            // When
            var result = await _uiController.CreateScriptAsync(invalidName, "Description");

            // Then
            result.Should().BeFalse("创建脚本应该失败");
            
            // 验证错误处理
            _mockView.HasErrorMessage("脚本名称不能为空").Should().BeTrue();
        }

        [Fact]
        public async Task GetScript_WithInvalidId_ShouldShowError()
        {
            // Given
            var invalidScriptId = Guid.NewGuid();

            // When
            var script = await _uiController.GetScriptAsync(invalidScriptId);

            // Then
            script.Should().BeNull("获取不存在的脚本应该返回null");
            
            // 验证错误处理
            _mockView.HasErrorMessage("获取脚本失败").Should().BeTrue();
        }

        [Fact]
        public async Task SaveScript_WithInvalidPath_ShouldShowError()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            var invalidPath = "Z:\\invalid\\path\\script.json";
            _mockView.Reset();

            // When
            var result = await _uiController.SaveScriptAsync(scriptId, invalidPath);

            // Then
            result.Should().BeFalse("保存脚本应该失败");
            
            // 验证错误处理
            _mockView.HasErrorMessage("保存脚本失败").Should().BeTrue();
        }

        [Fact]
        public async Task LoadScript_WithNonExistentFile_ShouldShowError()
        {
            // Given
            var nonExistentPath = GetTestFilePath("nonexistent.json");
            _mockView.Reset();

            // When
            var result = await _uiController.LoadScriptAsync(nonExistentPath);

            // Then
            result.Should().BeFalse("加载脚本应该失败");
            
            // 验证错误处理
            _mockView.HasErrorMessage("加载脚本失败").Should().BeTrue();
        }

        #endregion

        #region 统计信息交互测试

        [Fact]
        public async Task ShowStatistics_ThroughUI_ShouldDisplayStatistics()
        {
            // Given
            await _uiController.CreateScriptAsync("Test Script", "Test Description");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            
            // 添加一些动作到脚本
            var script = await _uiController.GetScriptAsync(scriptId);
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 0, "Release A"));
            script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 500, "Wait 500ms"));
            
            _mockView.Reset();

            // When
            await (_uiController as UIController)?.ShowStatisticsAsync(scriptId);

            // Then
            _mockView.CurrentStatistics.Should().NotBeNull();
            _mockView.CurrentStatistics["ScriptName"].Should().Be("Test Script");
            _mockView.CurrentStatistics["TotalActions"].Should().Be(3);
            
            // 验证动作类型统计
            var actionTypes = _mockView.CurrentStatistics["ActionTypes"] as Dictionary<string, int>;
            actionTypes.Should().NotBeNull();
            actionTypes["KeyDown"].Should().Be(1);
            actionTypes["KeyUp"].Should().Be(1);
            actionTypes["Delay"].Should().Be(1);
        }

        #endregion

        #region 完整工作流程交互测试

        [Fact]
        public async Task CompleteWorkflow_ThroughUI_ShouldWorkSeamlessly()
        {
            // Given
            var scriptName = "Complete Workflow Test";
            var scriptDescription = "Complete workflow test script";
            var filePath = GetTestFilePath("complete_workflow.json");
            _mockView.Reset();
            _uiEvents.Clear();

            // When - 完整的UI工作流程
            // 1. 创建脚本
            var createResult = await _uiController.CreateScriptAsync(scriptName, scriptDescription);
            createResult.Should().BeTrue();

            // 2. 获取脚本ID
            var scriptId = _mockView.DisplayedScripts[0].Id;

            // 3. 开始录制
            var startRecordingResult = await _uiController.StartRecordingAsync(scriptId);
            startRecordingResult.Should().BeTrue();

            // 4. 添加动作
            var action1 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A");
            var action2 = new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 0, "Release A");
            await _applicationService.AddActionAsync(scriptId, action1);
            await _applicationService.AddActionAsync(scriptId, action2);

            // 5. 停止录制
            var stopRecordingResult = await _uiController.StopRecordingAsync();
            stopRecordingResult.Should().BeTrue();

            // 6. 保存脚本
            var saveResult = await _uiController.SaveScriptAsync(scriptId, filePath);
            saveResult.Should().BeTrue();

            // 7. 清空当前脚本列表
            _mockView.Reset();
            _uiEvents.Clear();

            // 8. 加载脚本
            var loadResult = await _uiController.LoadScriptAsync(filePath);
            loadResult.Should().BeTrue();

            // 9. 开始播放
            var startPlaybackResult = await _uiController.StartPlaybackAsync(scriptId, speedMultiplier: 10.0f);
            startPlaybackResult.Should().BeTrue();

            // 10. 停止播放
            await Task.Delay(100); // 等待播放开始
            var stopPlaybackResult = await _uiController.StopPlaybackAsync();
            stopPlaybackResult.Should().BeTrue();

            // 11. 显示统计信息
            await (_uiController as UIController)?.ShowStatisticsAsync(scriptId);

            // Then - 验证完整工作流程
            // 验证脚本创建
            _uiEvents.Should().Contain(e => e.Contains("ScriptCreated") && e.Contains(scriptName));
            
            // 验证录制流程
            _uiEvents.Should().Contain(e => e.Contains("RecordingStateChanged: True"));
            _uiEvents.Should().Contain(e => e.Contains("RecordingStateChanged: False"));
            _uiEvents.Should().Contain(e => e.Contains("ActionRecorded: KeyDown"));
            _uiEvents.Should().Contain(e => e.Contains("ActionRecorded: KeyUp"));
            
            // 验证文件操作
            _uiEvents.Should().Contain(e => e.Contains("Status") && e.Contains("脚本已保存"));
            _uiEvents.Should().Contain(e => e.Contains("ScriptCreated") && e.Contains(scriptName));
            
            // 验证播放流程
            _uiEvents.Should().Contain(e => e.Contains("PlaybackStateChanged: True"));
            _uiEvents.Should().Contain(e => e.Contains("PlaybackStateChanged: False"));
            
            // 验证最终状态
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be(scriptName);
            _mockView.CurrentStatistics.Should().NotBeNull();
            _mockView.CurrentStatistics["TotalActions"].Should().Be(2);
            
            // 验证文件存在
            ValidateFileExists(filePath, "完整工作流程脚本文件");
        }

        #endregion

        #region 并发交互测试

        [Fact]
        public async Task ConcurrentOperations_ThroughUI_ShouldHandleConcurrency()
        {
            // Given
            await _uiController.CreateScriptAsync("Concurrent Test", "Concurrent test script");
            var scriptId = _mockView.DisplayedScripts[0].Id;
            _mockView.Reset();

            // When - 并发操作
            var tasks = new List<Task>
            {
                _uiController.GetScriptAsync(scriptId),
                _uiController.IsRecordingAsync(),
                _uiController.IsPlayingAsync(),
                _uiController.GetAllScriptsAsync()
            };

            await Task.WhenAll(tasks);

            // Then - 验证并发操作不会导致错误
            _mockView.ErrorMessages.Should().BeEmpty("并发操作不应该产生错误");
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.CurrentScript.Should().NotBeNull();
        }

        #endregion

        #region 资源管理测试

        [Fact]
        public void Dispose_WhenCalled_ShouldCleanUpResources()
        {
            // Given
            _uiController.CreateScriptAsync("Dispose Test", "Dispose test script").Wait();

            // When
            _uiController.Dispose();

            // Then - 验证资源清理
            Action action = () => _uiController.CreateScriptAsync("After Dispose", "Should fail").Wait();
            action.Should().Throw<ObjectDisposedException>();
        }

        #endregion
    }
}