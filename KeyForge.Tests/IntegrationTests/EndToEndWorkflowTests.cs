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
using KeyForge.Domain.Common;
using KeyForge.Infrastructure.Services;
using KeyForge.Infrastructure.Native;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Domain.Interfaces;
using KeyForge.Presentation.Interfaces;
using KeyForge.Presentation.Services;
using KeyForge.Presentation.Controllers;
using KeyForge.Presentation.Views;

namespace KeyForge.Tests.IntegrationTests
{
    /// <summary>
    /// 端到端工作流测试 - 录制→保存→加载→播放的完整流程
    /// 测试整个应用程序的完整工作流程，包括所有组件的协作
    /// 原本实现：分离的组件测试
    /// 简化实现：完整的端到端测试，确保整个系统工作正常
    /// </summary>
    public class EndToEndWorkflowTests : IntegrationTestBase
    {
        private readonly KeyInputService _keyInputService;
        private readonly ScriptService _scriptService;
        private readonly ApplicationService _applicationService;
        private readonly MockUIView _mockView;
        private readonly UIController _uiController;
        private readonly List<string> _workflowEvents;
        private readonly List<GameAction> _recordedActions;

        public EndToEndWorkflowTests(ITestOutputHelper output) : base(output)
        {
            // 初始化所有服务
            _keyInputService = new KeyInputService();
            _scriptService = new ScriptService();
            _applicationService = new ApplicationService(MockLogger.Object);
            _mockView = new MockUIView();
            _uiController = new UIController(_applicationService, _mockView);
            
            _workflowEvents = new List<string>();
            _recordedActions = new List<GameAction>();

            // 订阅所有服务的事件
            SubscribeToEvents();

            RegisterDisposable(_keyInputService);
            RegisterDisposable(_scriptService);
            RegisterDisposable(_applicationService);
            RegisterDisposable(_uiController);
        }

        private void SubscribeToEvents()
        {
            // KeyInputService 事件
            _keyInputService.KeyRecorded += (sender, e) =>
            {
                _workflowEvents.Add($"KeyInputService: {e.Key} {(e.IsKeyDown ? "按下" : "释放")}");
                LogDebug($"工作流事件 - 按键录制: {e.Key}");
            };

            _keyInputService.StatusChanged += (sender, message) =>
            {
                _workflowEvents.Add($"KeyInputService: {message}");
                LogDebug($"工作流事件 - 按键服务状态: {message}");
            };

            // ScriptService 事件
            _scriptService.ActionRecorded += (sender, action) =>
            {
                _recordedActions.Add(action);
                _workflowEvents.Add($"ScriptService: 录制动作 {action.KeyName}");
                LogDebug($"工作流事件 - 脚本服务录制: {action.KeyName}");
            };

            _scriptService.StatusChanged += (sender, message) =>
            {
                _workflowEvents.Add($"ScriptService: {message}");
                LogDebug($"工作流事件 - 脚本服务状态: {message}");
            };

            // ApplicationService 事件
            _applicationService.StatusChanged += (sender, message) =>
            {
                _workflowEvents.Add($"ApplicationService: {message}");
                LogDebug($"工作流事件 - 应用服务状态: {message}");
            };

            _applicationService.ScriptCreated += (sender, script) =>
            {
                _workflowEvents.Add($"ApplicationService: 创建脚本 {script.Name}");
                LogDebug($"工作流事件 - 应用服务创建脚本: {script.Name}");
            };

            _applicationService.ActionRecorded += (sender, action) =>
            {
                _workflowEvents.Add($"ApplicationService: 动作 {action.Type}");
                LogDebug($"工作流事件 - 应用服务动作: {action.Type}");
            };

            // UIController 事件
            _uiController.StatusChanged += (sender, message) =>
            {
                _workflowEvents.Add($"UIController: {message}");
                LogDebug($"工作流事件 - UI控制器状态: {message}");
            };

            _uiController.ScriptCreated += (sender, script) =>
            {
                _workflowEvents.Add($"UIController: 脚本创建 {script.Name}");
                LogDebug($"工作流事件 - UI控制器创建脚本: {script.Name}");
            };
        }

        #region 基础端到端工作流测试

        [Fact]
        public async Task CompleteWorkflow_RecordingToPlayback_ShouldWorkSeamlessly()
        {
            // Given
            var workflowName = "Complete E2E Workflow";
            var filePath = GetTestFilePath("complete_e2e_workflow.json");
            _mockView.Reset();
            _workflowEvents.Clear();
            _recordedActions.Clear();

            LogInfo($"开始端到端工作流测试: {workflowName}");

            // When - 完整的端到端工作流
            // 1. 创建脚本
            var createResult = await _uiController.CreateScriptAsync(workflowName, "完整的端到端测试脚本");
            createResult.Should().BeTrue("创建脚本应该成功");
            var scriptId = _mockView.DisplayedScripts[0].Id;

            LogInfo($"步骤1: 创建脚本完成 - {workflowName}");

            // 2. 开始录制（按键录制）
            var startRecordingResult = await _uiController.StartRecordingAsync(scriptId);
            startRecordingResult.Should().BeTrue("开始录制应该成功");

            LogInfo($"步骤2: 开始录制完成");

            // 3. 模拟按键录制
            var testKeys = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>
            {
                (System.Windows.Forms.Keys.A, true),
                (System.Windows.Forms.Keys.A, false),
                (System.Windows.Forms.Keys.B, true),
                (System.Windows.Forms.Keys.B, false),
                (System.Windows.Forms.Keys.Enter, true),
                (System.Windows.Forms.Keys.Enter, false)
            };

            foreach (var (key, isKeyDown) in testKeys)
            {
                var keyEvent = new KeyForge.Core.Interfaces.KeyEventArgs(key, isKeyDown);
                _keyInputService.GetType().GetMethod("OnKeyPressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_keyInputService, new object[] { this, keyEvent });
                
                // 转换为GameAction并添加到应用服务
                var gameAction = ConvertToGameAction(key, isKeyDown);
                await _applicationService.AddActionAsync(scriptId, gameAction);
                
                await Task.Delay(10); // 模拟按键间隔
            }

            LogInfo($"步骤3: 按键录制完成 - 录制了{testKeys.Count}个按键事件");

            // 4. 停止录制
            var stopRecordingResult = await _uiController.StopRecordingAsync();
            stopRecordingResult.Should().BeTrue("停止录制应该成功");

            LogInfo($"步骤4: 停止录制完成");

            // 5. 保存脚本到文件
            var saveResult = await _uiController.SaveScriptAsync(scriptId, filePath);
            saveResult.Should().BeTrue("保存脚本应该成功");

            LogInfo($"步骤5: 保存脚本完成 - {filePath}");

            // 6. 验证文件存在并读取内容
            ValidateFileExists(filePath, "端到端工作流脚本文件");
            var fileContent = File.ReadAllText(filePath);
            fileContent.Should().Contain("A");
            fileContent.Should().Contain("B");
            fileContent.Should().Contain("Enter");

            LogInfo($"步骤6: 文件验证完成");

            // 7. 清空当前状态
            _mockView.Reset();
            _workflowEvents.Clear();
            _recordedActions.Clear();

            LogInfo($"步骤7: 状态清空完成");

            // 8. 加载脚本
            var loadResult = await _uiController.LoadScriptAsync(filePath);
            loadResult.Should().BeTrue("加载脚本应该成功");
            var loadedScriptId = _mockView.DisplayedScripts[0].Id;

            LogInfo($"步骤8: 脚本加载完成");

            // 9. 验证脚本内容
            var loadedScript = await _uiController.GetScriptAsync(loadedScriptId);
            loadedScript.Should().NotBeNull();
            loadedScript.Name.Should().Be(workflowName);
            loadedScript.Actions.Should().HaveCount(testKeys.Count);

            LogInfo($"步骤9: 脚本内容验证完成");

            // 10. 开始播放
            var startPlaybackResult = await _uiController.StartPlaybackAsync(loadedScriptId, speedMultiplier: 5.0f);
            startPlaybackResult.Should().BeTrue("开始播放应该成功");

            LogInfo($"步骤10: 开始播放完成");

            // 11. 等待播放完成
            await Task.Delay(2000); // 等待播放完成
            var stopPlaybackResult = await _uiController.StopPlaybackAsync();
            stopPlaybackResult.Should().BeTrue("停止播放应该成功");

            LogInfo($"步骤11: 播放完成");

            // 12. 显示统计信息
            await (_uiController as UIController)?.ShowStatisticsAsync(loadedScriptId);

            LogInfo($"步骤12: 统计信息显示完成");

            // Then - 验证整个工作流程
            // 验证UI状态
            _mockView.ErrorMessages.Should().BeEmpty("工作流中不应该有错误");
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be(workflowName);

            // 验证工作流事件
            _workflowEvents.Should().Contain(e => e.Contains("创建脚本") && e.Contains(workflowName));
            _workflowEvents.Should().Contain(e => e.Contains("开始录制"));
            _workflowEvents.Should().Contain(e => e.Contains("按键录制"));
            _workflowEvents.Should().Contain(e => e.Contains("停止录制"));
            _workflowEvents.Should().Contain(e => e.Contains("脚本已保存"));
            _workflowEvents.Should().Contain(e => e.Contains("脚本已加载"));
            _workflowEvents.Should().Contain(e => e.Contains("开始播放"));
            _workflowEvents.Should().Contain(e => e.Contains("停止播放"));

            // 验证最终统计
            _mockView.CurrentStatistics.Should().NotBeNull();
            _mockView.CurrentStatistics["ScriptName"].Should().Be(workflowName);
            _mockView.CurrentStatistics["TotalActions"].Should().Be(testKeys.Count);

            LogInfo($"端到端工作流测试完成 - 成功");
        }

        [Fact]
        public async Task ComplexWorkflow_WithMultipleScripts_ShouldManageCorrectly()
        {
            // Given
            var scriptNames = new[] { "Script A", "Script B", "Script C" };
            var filePaths = scriptNames.Select(name => GetTestFilePath($"{name.Replace(" ", "_")}.json")).ToArray();
            _mockView.Reset();
            _workflowEvents.Clear();

            LogInfo($"开始复杂工作流测试: {scriptNames.Length} 个脚本");

            // When - 多脚本的复杂工作流
            for (int i = 0; i < scriptNames.Length; i++)
            {
                var scriptName = scriptNames[i];
                var filePath = filePaths[i];

                LogInfo($"处理脚本 {i + 1}: {scriptName}");

                // 1. 创建脚本
                await _uiController.CreateScriptAsync(scriptName, $"复杂工作流脚本 {i + 1}");
                var scriptId = _mockView.DisplayedScripts.First(s => s.Name == scriptName).Id;

                // 2. 录制不同的按键序列
                await _uiController.StartRecordingAsync(scriptId);
                
                var keys = new List<(System.Windows.Forms.Keys Key, bool IsKeyDown)>
                {
                    (System.Windows.Forms.Keys.A + i, true),
                    (System.Windows.Forms.Keys.A + i, false),
                    (System.Windows.Forms.Keys.D1 + i, true),
                    (System.Windows.Forms.Keys.D1 + i, false)
                };

                foreach (var (key, isKeyDown) in keys)
                {
                    var gameAction = ConvertToGameAction(key, isKeyDown);
                    await _applicationService.AddActionAsync(scriptId, gameAction);
                }

                await _uiController.StopRecordingAsync();

                // 3. 保存脚本
                await _uiController.SaveScriptAsync(scriptId, filePath);

                LogInfo($"脚本 {scriptName} 处理完成");
            }

            // 4. 验证所有脚本都已创建
            var allScripts = await _uiController.GetAllScriptsAsync();
            allScripts.Should().HaveCount(scriptNames.Length);

            // 5. 清空并重新加载所有脚本
            _mockView.Reset();
            foreach (var filePath in filePaths)
            {
                await _uiController.LoadScriptAsync(filePath);
            }

            // 6. 验证所有脚本都已加载
            var loadedScripts = await _uiController.GetAllScriptsAsync();
            loadedScripts.Should().HaveCount(scriptNames.Length);

            // 7. 逐个播放所有脚本
            foreach (var script in loadedScripts)
            {
                LogInfo($"播放脚本: {script.Name}");
                await _uiController.StartPlaybackAsync(script.Id, speedMultiplier: 10.0f);
                await Task.Delay(1000);
                await _uiController.StopPlaybackAsync();
            }

            // Then - 验证复杂工作流
            _mockView.ErrorMessages.Should().BeEmpty("复杂工作流中不应该有错误");
            _workflowEvents.Should().Contain(e => e.Contains("创建脚本") && e.Contains("Script A"));
            _workflowEvents.Should().Contain(e => e.Contains("创建脚本") && e.Contains("Script B"));
            _workflowEvents.Should().Contain(e => e.Contains("创建脚本") && e.Contains("Script C"));

            // 验证所有文件都存在
            foreach (var filePath in filePaths)
            {
                ValidateFileExists(filePath, $"复杂工作流脚本文件: {Path.GetFileName(filePath)}");
            }

            LogInfo($"复杂工作流测试完成 - 成功处理 {scriptNames.Length} 个脚本");
        }

        #endregion

        #region 错误恢复工作流测试

        [Fact]
        public async Task ErrorRecoveryWorkflow_WhenFailuresOccur_ShouldHandleGracefully()
        {
            // Given
            var workflowName = "Error Recovery Workflow";
            var invalidFilePath = "Z:\\invalid\\path\\script.json";
            var validFilePath = GetTestFilePath("error_recovery.json");
            _mockView.Reset();
            _workflowEvents.Clear();

            LogInfo($"开始错误恢复工作流测试: {workflowName}");

            // When - 包含错误的工作流
            // 1. 正常创建脚本
            await _uiController.CreateScriptAsync(workflowName, "错误恢复测试脚本");
            var scriptId = _mockView.DisplayedScripts[0].Id;

            LogInfo($"步骤1: 脚本创建完成");

            // 2. 尝试保存到无效路径（应该失败）
            var invalidSaveResult = await _uiController.SaveScriptAsync(scriptId, invalidFilePath);
            invalidSaveResult.Should().BeFalse("保存到无效路径应该失败");

            LogInfo($"步骤2: 无效路径保存失败（预期）");

            // 3. 验证错误处理
            _mockView.HasErrorMessage("保存脚本失败").Should().BeTrue();
            _workflowEvents.Should().Contain(e => e.Contains("保存失败"));

            LogInfo($"步骤3: 错误处理验证完成");

            // 4. 恢复：保存到有效路径
            var validSaveResult = await _uiController.SaveScriptAsync(scriptId, validFilePath);
            validSaveResult.Should().BeTrue("保存到有效路径应该成功");

            LogInfo($"步骤4: 恢复保存完成");

            // 5. 清空并尝试加载不存在的文件（应该失败）
            _mockView.Reset();
            var invalidLoadResult = await _uiController.LoadScriptAsync(GetTestFilePath("nonexistent.json"));
            invalidLoadResult.Should().BeFalse("加载不存在的文件应该失败");

            LogInfo($"步骤5: 无效文件加载失败（预期）");

            // 6. 验证错误处理
            _mockView.HasErrorMessage("加载脚本失败").Should().BeTrue();

            LogInfo($"步骤6: 错误处理验证完成");

            // 7. 恢复：加载有效文件
            var validLoadResult = await _uiController.LoadScriptAsync(validFilePath);
            validLoadResult.Should().BeTrue("加载有效文件应该成功");

            LogInfo($"步骤7: 恢复加载完成");

            // 8. 验证最终状态
            var loadedScript = _mockView.DisplayedScripts.FirstOrDefault();
            loadedScript.Should().NotBeNull();
            loadedScript.Name.Should().Be(workflowName);

            LogInfo($"步骤8: 最终状态验证完成");

            // Then - 验证错误恢复工作流
            _mockView.ErrorMessages.Should().HaveCount(2, "应该有2个预期的错误");
            _mockView.ErrorMessages.Should().Contain(e => e.Contains("保存脚本失败"));
            _mockView.ErrorMessages.Should().Contain(e => e.Contains("加载脚本失败"));

            // 验证最终成功状态
            _mockView.DisplayedScripts.Should().HaveCount(1);
            _mockView.DisplayedScripts[0].Name.Should().Be(workflowName);

            LogInfo($"错误恢复工作流测试完成 - 成功处理错误");
        }

        #endregion

        #region 性能工作流测试

        [Fact]
        public async Task PerformanceWorkflow_WithLargeDataset_ShouldPerformWell()
        {
            // Given
            var workflowName = "Performance Workflow";
            var filePath = GetTestFilePath("performance_workflow.json");
            var largeActionCount = 500;
            _mockView.Reset();
            _workflowEvents.Clear();

            LogInfo($"开始性能工作流测试: {largeActionCount} 个动作");

            // When - 大数据集的性能工作流
            var startTime = DateTime.UtcNow;

            // 1. 创建脚本
            await _uiController.CreateScriptAsync(workflowName, "性能测试脚本");
            var scriptId = _mockView.DisplayedScripts[0].Id;

            LogInfo($"步骤1: 脚本创建完成");

            // 2. 批量录制大量动作
            await _uiController.StartRecordingAsync(scriptId);
            
            for (int i = 0; i < largeActionCount; i++)
            {
                var key = System.Windows.Forms.Keys.A + (i % 26);
                var isKeyDown = i % 2 == 0;
                var gameAction = ConvertToGameAction(key, isKeyDown, i * 10);
                await _applicationService.AddActionAsync(scriptId, gameAction);
            }

            await _uiController.StopRecordingAsync();

            var recordingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            LogInfo($"步骤2: 批量录制完成 - {largeActionCount} 个动作，耗时 {recordingTime:F2}ms");

            // 3. 保存大文件
            var saveStartTime = DateTime.UtcNow;
            await _uiController.SaveScriptAsync(scriptId, filePath);
            var saveTime = (DateTime.UtcNow - saveStartTime).TotalMilliseconds;
            LogInfo($"步骤3: 大文件保存完成 - 耗时 {saveTime:F2}ms");

            // 4. 清空并加载大文件
            _mockView.Reset();
            var loadStartTime = DateTime.UtcNow;
            await _uiController.LoadScriptAsync(filePath);
            var loadTime = (DateTime.UtcNow - loadStartTime).TotalMilliseconds;
            LogInfo($"步骤4: 大文件加载完成 - 耗时 {loadTime:F2}ms");

            // 5. 验证加载的脚本
            var loadedScript = _mockView.DisplayedScripts.FirstOrDefault();
            loadedScript.Should().NotBeNull();
            loadedScript.Actions.Should().HaveCount(largeActionCount);

            // 6. 快速播放
            var playbackStartTime = DateTime.UtcNow;
            await _uiController.StartPlaybackAsync(loadedScript.Id, speedMultiplier: 50.0f);
            await Task.Delay(2000); // 等待播放开始
            await _uiController.StopPlaybackAsync();
            var playbackTime = (DateTime.UtcNow - playbackStartTime).TotalMilliseconds;
            LogInfo($"步骤6: 快速播放完成 - 耗时 {playbackTime:F2}ms");

            var totalTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            LogInfo($"性能工作流测试完成 - 总耗时 {totalTime:F2}ms");

            // Then - 验证性能要求
            recordingTime.Should().BeLessThan(5000, "录制500个动作应该在5秒内完成");
            saveTime.Should().BeLessThan(2000, "保存500个动作应该在2秒内完成");
            loadTime.Should().BeLessThan(2000, "加载500个动作应该在2秒内完成");
            totalTime.Should().BeLessThan(15000, "整个性能工作流应该在15秒内完成");

            // 验证数据完整性
            loadedScript.Actions.Should().HaveCount(largeActionCount);
            ValidateFileExists(filePath, "性能工作流脚本文件");
        }

        #endregion

        #region 并发工作流测试

        [Fact]
        public async Task ConcurrentWorkflow_WithMultipleOperations_ShouldHandleConcurrency()
        {
            // Given
            var workflowName = "Concurrent Workflow";
            var concurrentScriptCount = 5;
            var filePaths = new List<string>();
            _mockView.Reset();
            _workflowEvents.Clear();

            LogInfo($"开始并发工作流测试: {concurrentScriptCount} 个并发脚本");

            // When - 并发工作流
            var tasks = new List<Task>();

            for (int i = 0; i < concurrentScriptCount; i++)
            {
                var scriptName = $"{workflowName} {i + 1}";
                var filePath = GetTestFilePath($"concurrent_script_{i + 1}.json");
                filePaths.Add(filePath);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        LogInfo($"并发任务 {i + 1} 开始");

                        // 创建脚本
                        await _uiController.CreateScriptAsync(scriptName, $"并发测试脚本 {i + 1}");
                        var scripts = await _uiController.GetAllScriptsAsync();
                        var scriptId = scripts.First(s => s.Name == scriptName).Id;

                        // 录制少量动作
                        await _uiController.StartRecordingAsync(scriptId);
                        for (int j = 0; j < 10; j++)
                        {
                            var key = System.Windows.Forms.Keys.A + (i + j) % 26;
                            var gameAction = ConvertToGameAction(key, j % 2 == 0, j * 50);
                            await _applicationService.AddActionAsync(scriptId, gameAction);
                        }
                        await _uiController.StopRecordingAsync();

                        // 保存脚本
                        await _uiController.SaveScriptAsync(scriptId, filePath);

                        LogInfo($"并发任务 {i + 1} 完成");
                    }
                    catch (Exception ex)
                    {
                        LogError($"并发任务 {i + 1} 失败: {ex.Message}");
                        throw;
                    }
                });

                tasks.Add(task);
            }

            // 等待所有并发任务完成
            await Task.WhenAll(tasks);

            LogInfo($"所有并发任务完成");

            // Then - 验证并发工作流
            _mockView.ErrorMessages.Should().BeEmpty("并发工作流中不应该有错误");

            // 验证所有脚本都已创建
            var allScripts = await _uiController.GetAllScriptsAsync();
            allScripts.Should().HaveCount(concurrentScriptCount);

            // 验证所有文件都存在
            foreach (var filePath in filePaths)
            {
                ValidateFileExists(filePath, $"并发工作流脚本文件: {Path.GetFileName(filePath)}");
            }

            // 验证每个脚本都有正确的动作数量
            foreach (var script in allScripts)
            {
                script.Actions.Should().HaveCount(10, $"脚本 {script.Name} 应该有10个动作");
            }

            LogInfo($"并发工作流测试完成 - 成功处理 {concurrentScriptCount} 个并发脚本");
        }

        #endregion

        #region 数据完整性工作流测试

        [Fact]
        public async Task DataIntegrityWorkflow_WithMultipleSaveLoadCycles_ShouldMaintainConsistency()
        {
            // Given
            var workflowName = "Data Integrity Workflow";
            var cycleCount = 5;
            var filePaths = new List<string>();
            var originalActions = new List<GameAction>();
            
            // 创建原始动作数据
            for (int i = 0; i < 20; i++)
            {
                var action = new GameAction(Guid.NewGuid(), 
                    i % 2 == 0 ? ActionType.KeyDown : ActionType.KeyUp,
                    (KeyCode)(65 + (i % 26)), 
                    i * 100, 
                    $"Action {i + 1}");
                originalActions.Add(action);
            }

            _mockView.Reset();
            _workflowEvents.Clear();

            LogInfo($"开始数据完整性工作流测试: {cycleCount} 次保存加载循环");

            // When - 多次保存加载循环
            var currentScriptId = Guid.Empty;

            for (int cycle = 1; cycle <= cycleCount; cycle++)
            {
                LogInfo($"开始第 {cycle} 次循环");

                var filePath = GetTestFilePath($"integrity_cycle_{cycle}.json");
                filePaths.Add(filePath);

                // 1. 创建或更新脚本
                if (cycle == 1)
                {
                    await _uiController.CreateScriptAsync(workflowName, $"数据完整性测试第 {cycle} 轮");
                    var scripts = await _uiController.GetAllScriptsAsync();
                    currentScriptId = scripts.First().Id;
                }
                else
                {
                    await _uiController.UpdateScriptAsync(currentScriptId, workflowName, $"数据完整性测试第 {cycle} 轮");
                }

                // 2. 添加动作
                await _uiController.StartRecordingAsync(currentScriptId);
                foreach (var action in originalActions)
                {
                    await _applicationService.AddActionAsync(currentScriptId, action);
                }
                await _uiController.StopRecordingAsync();

                // 3. 保存脚本
                await _uiController.SaveScriptAsync(currentScriptId, filePath);

                // 4. 清空并重新加载
                _mockView.Reset();
                await _uiController.LoadScriptAsync(filePath);

                // 5. 获取加载的脚本
                var loadedScripts = await _uiController.GetAllScriptsAsync();
                currentScriptId = loadedScripts.First().Id;
                var loadedScript = await _uiController.GetScriptAsync(currentScriptId);

                LogInfo($"第 {cycle} 次循环完成");

                // 验证数据完整性
                loadedScript.Actions.Should().HaveCount(originalActions.Count, $"第 {cycle} 轮后动作数量应该保持一致");
            }

            LogInfo($"所有循环完成");

            // Then - 验证数据完整性
            var finalScript = await _uiController.GetScriptAsync(currentScriptId);
            finalScript.Should().NotBeNull();
            finalScript.Actions.Should().HaveCount(originalActions.Count);

            // 验证所有文件都存在
            foreach (var filePath in filePaths)
            {
                ValidateFileExists(filePath, $"数据完整性文件: {Path.GetFileName(filePath)}");
            }

            // 验证所有文件内容一致
            var firstFileContent = File.ReadAllText(filePaths[0]);
            for (int i = 1; i < filePaths.Count; i++)
            {
                var currentFileContent = File.ReadAllText(filePaths[i]);
                currentFileContent.Should().Be(firstFileContent, $"第 {i + 1} 轮文件内容应该与第一轮一致");
            }

            // 验证动作数据一致性
            for (int i = 0; i < originalActions.Count; i++)
            {
                var original = originalActions[i];
                var loaded = finalScript.Actions.ElementAt(i);

                loaded.Type.Should().Be(original.Type);
                loaded.Key.Should().Be(original.Key);
                loaded.Delay.Should().Be(original.Delay);
                loaded.Description.Should().Be(original.Description);
            }

            LogInfo($"数据完整性工作流测试完成 - {cycleCount} 次循环后数据完全一致");
        }

        #endregion

        #region 辅助方法

        private GameAction ConvertToGameAction(System.Windows.Forms.Keys key, bool isKeyDown, int delay = 100)
        {
            var keyCode = (KeyCode)key;
            return new GameAction(Guid.NewGuid(), 
                isKeyDown ? ActionType.KeyDown : ActionType.KeyUp, 
                keyCode, 
                delay, 
                $"Convert {key} {(isKeyDown ? "按下" : "释放")}");
        }

        #endregion
    }
}