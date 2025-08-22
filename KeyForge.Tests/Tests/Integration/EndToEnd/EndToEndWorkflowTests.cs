using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.IO;
using KeyForge.Tests.Support;
using KeyForge.Domain;
using KeyForge.Application;
using KeyForge.Infrastructure;
using KeyForge.Core;

namespace KeyForge.Tests.Integration.EndToEnd
{
    /// <summary>
    /// 端到端工作流测试 - 验证完整的业务流程
    /// 原本实现：简单的工作流测试
    /// 简化实现：完整的端到端业务流程验证
    /// </summary>
    public class EndToEndWorkflowTests : TestBase
    {
        private readonly IScriptService _scriptService;
        private readonly IExecutionEngine _executionEngine;
        private readonly IFileStorage _fileStorage;
        private readonly IValidationService _validationService;
        private readonly string _testDirectory;

        public EndToEndWorkflowTests(ITestOutputHelper output) : base(output)
        {
            _scriptService = ServiceProvider.GetRequiredService<IScriptService>();
            _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
            _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
            _validationService = ServiceProvider.GetRequiredService<IValidationService>();

            _testDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_EndToEnd_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            Log($"创建测试目录: {_testDirectory}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (Directory.Exists(_testDirectory))
                    {
                        Directory.Delete(_testDirectory, true);
                        Log($"清理测试目录: {_testDirectory}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"清理测试目录失败: {ex.Message}");
                }
            }
            base.Dispose(disposing);
        }

        [Fact]
        public async Task CompleteScriptLifecycle_ShouldWork()
        {
            try
            {
                // 1. 创建脚本
                var script = TestFixtures.CreateScriptWithActions(5);
                script.Name = "EndToEndTestScript";
                script.Description = "完整的端到端测试脚本";
                Log($"步骤1: 创建脚本 - {script.Name}");

                // 2. 验证脚本
                var validationResult = await _validationService.ValidateScriptAsync(script);
                validationResult.IsValid.Should().BeTrue();
                Log($"步骤2: 脚本验证通过");

                // 3. 保存脚本
                await _scriptService.SaveAsync(script);
                Log($"步骤3: 脚本保存成功 - {script.Id}");

                // 4. 加载脚本
                var loadedScript = await _scriptService.GetByIdAsync(script.Id);
                loadedScript.Should().NotBeNull();
                loadedScript.Name.Should().Be(script.Name);
                loadedScript.Actions.Should().HaveCount(5);
                Log($"步骤4: 脚本加载成功 - {loadedScript.Actions.Count}个动作");

                // 5. 激活脚本
                loadedScript.Activate();
                await _scriptService.UpdateAsync(loadedScript);
                Log($"步骤5: 脚本激活成功 - {loadedScript.Status}");

                // 6. 执行脚本
                var executionResult = await _executionEngine.ExecuteAsync(loadedScript);
                executionResult.Success.Should().BeTrue();
                executionResult.ExecutedActions.Should().Be(5);
                executionResult.ExecutionTime.Should().BeGreaterThan(0);
                Log($"步骤6: 脚本执行成功 - {executionResult.ExecutedActions}个动作, {executionResult.ExecutionTime}ms");

                // 7. 导出脚本
                var exportPath = Path.Combine(_testDirectory, $"{script.Id}.json");
                await _fileStorage.SaveAsync(script, exportPath);
                File.Exists(exportPath).Should().BeTrue();
                Log($"步骤7: 脚本导出成功 - {exportPath}");

                // 8. 导入脚本
                var importedScript = await _fileStorage.LoadAsync<Script>(exportPath);
                importedScript.Should().NotBeNull();
                importedScript.Id.Should().Be(script.Id);
                importedScript.Name.Should().Be(script.Name);
                Log($"步骤8: 脚本导入成功 - {importedScript.Name}");

                // 9. 删除脚本
                await _scriptService.DeleteAsync(script.Id);
                var deletedScript = await _scriptService.GetByIdAsync(script.Id);
                deletedScript.Should().BeNull();
                Log($"步骤9: 脚本删除成功");

                Log("完整脚本生命周期测试完成");
            }
            catch (Exception ex)
            {
                LogError($"完整脚本生命周期测试失败: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task ScriptManagementWorkflow_ShouldWork()
        {
            try
            {
                // 1. 批量创建脚本
                var scripts = TestFixtures.CreateMultipleScripts(3);
                Log($"步骤1: 批量创建{scripts.Count}个脚本");

                // 2. 批量保存脚本
                foreach (var script in scripts)
                {
                    await _scriptService.SaveAsync(script);
                }
                Log($"步骤2: 批量保存脚本完成");

                // 3. 获取所有脚本
                var allScripts = await _scriptService.GetAllAsync();
                allScripts.Should().HaveCount(3);
                Log($"步骤3: 获取所有脚本 - {allScripts.Count}个");

                // 4. 批量激活脚本
                foreach (var script in allScripts)
                {
                    script.Activate();
                    await _scriptService.UpdateAsync(script);
                }
                Log($"步骤4: 批量激活脚本完成");

                // 5. 批量执行脚本
                var executionTasks = allScripts.Select(script => _executionEngine.ExecuteAsync(script));
                var executionResults = await Task.WhenAll(executionTasks);
                executionResults.Should().AllSatisfy(result => result.Success.Should().BeTrue());
                Log($"步骤5: 批量执行脚本完成 - {executionResults.Length}个");

                // 6. 批量删除脚本
                foreach (var script in allScripts)
                {
                    await _scriptService.DeleteAsync(script.Id);
                }
                var remainingScripts = await _scriptService.GetAllAsync();
                remainingScripts.Should().BeEmpty();
                Log($"步骤6: 批量删除脚本完成");

                Log("脚本管理工作流测试完成");
            }
            catch (Exception ex)
            {
                LogError($"脚本管理工作流测试失败: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task ErrorRecoveryWorkflow_ShouldWork()
        {
            try
            {
                // 1. 创建包含错误动作的脚本
                var script = TestFixtures.CreateValidScript();
                script.Actions.Add(new GameAction
                {
                    Type = ActionType.Keyboard,
                    Key = "InvalidKey", // 无效按键
                    Timestamp = DateTime.UtcNow
                });
                Log($"步骤1: 创建包含错误动作的脚本");

                // 2. 验证脚本应该失败
                var validationResult = await _validationService.ValidateScriptAsync(script);
                validationResult.IsValid.Should().BeFalse();
                validationResult.Errors.Should().NotBeEmpty();
                Log($"步骤2: 脚本验证失败 - {validationResult.Errors.Count}个错误");

                // 3. 修复脚本错误
                script.Actions.Clear();
                script.Actions.AddRange(TestFixtures.CreateValidActions(3));
                Log($"步骤3: 修复脚本错误");

                // 4. 重新验证脚本
                var newValidationResult = await _validationService.ValidateScriptAsync(script);
                newValidationResult.IsValid.Should().BeTrue();
                Log($"步骤4: 脚本验证通过");

                // 5. 保存和执行脚本
                await _scriptService.SaveAsync(script);
                var executionResult = await _executionEngine.ExecuteAsync(script);
                executionResult.Success.Should().BeTrue();
                Log($"步骤5: 脚本保存和执行成功");

                // 6. 清理
                await _scriptService.DeleteAsync(script.Id);
                Log($"步骤6: 脚本清理完成");

                Log("错误恢复工作流测试完成");
            }
            catch (Exception ex)
            {
                LogError($"错误恢复工作流测试失败: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task LargeScriptProcessing_ShouldHandleVolume()
        {
            try
            {
                // 1. 创建大脚本
                var largeScript = TestFixtures.CreateLargeScript(1000); // 1000个动作
                Log($"步骤1: 创建大脚本 - {largeScript.Actions.Count}个动作");

                var startTime = DateTime.UtcNow;

                // 2. 保存大脚本
                await _scriptService.SaveAsync(largeScript);
                var saveTime = DateTime.UtcNow;
                Log($"步骤2: 大脚本保存完成");

                // 3. 执行大脚本
                var result = await _executionEngine.ExecuteAsync(largeScript);
                var executionTime = DateTime.UtcNow;
                Log($"步骤3: 大脚本执行完成");

                var endTime = DateTime.UtcNow;
                var totalDuration = (endTime - startTime).TotalMilliseconds;
                var saveDuration = (saveTime - startTime).TotalMilliseconds;
                var executionDuration = (executionTime - saveTime).TotalMilliseconds;

                // 4. 验证性能
                totalDuration.Should().BeLessThan(10000); // 10秒内完成
                saveDuration.Should().BeLessThan(5000); // 5秒内保存
                executionDuration.Should().BeLessThan(5000); // 5秒内执行
                result.Success.Should().BeTrue();
                result.ExecutedActions.Should().Be(1000);

                Log($"大脚本处理性能: 总耗时={totalDuration:F2}ms, 保存={saveDuration:F2}ms, 执行={executionDuration:F2}ms");

                // 5. 清理
                await _scriptService.DeleteAsync(largeScript.Id);
                Log($"步骤4: 大脚本清理完成");

                Log("大脚本处理测试完成");
            }
            catch (Exception ex)
            {
                LogError($"大脚本处理测试失败: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task ConcurrentWorkflow_ShouldHandleMultipleUsers()
        {
            try
            {
                // 1. 模拟多个用户并发操作
                var userCount = 5;
                var scriptsPerUser = 2;
                var allTasks = new System.Collections.Generic.List<Task>();

                Log($"步骤1: 模拟{userCount}个用户并发操作");

                for (int i = 0; i < userCount; i++)
                {
                    var userId = i;
                    var userTask = Task.Run(async () =>
                    {
                        try
                        {
                            // 每个用户创建脚本
                            var userScripts = TestFixtures.CreateMultipleScripts(scriptsPerUser);
                            foreach (var script in userScripts)
                            {
                                script.Name = $"User_{userId}_Script_{script.Name}";
                                await _scriptService.SaveAsync(script);
                                Log($"用户{userId}保存脚本: {script.Name}");

                                // 执行脚本
                                var result = await _executionEngine.ExecuteAsync(script);
                                result.Success.Should().BeTrue();
                                Log($"用户{userId}执行脚本: {script.Name}");
                            }

                            // 获取用户脚本
                            var userScriptList = await _scriptService.GetAllAsync();
                            var userScriptsCount = userScriptList.Count(s => s.Name.StartsWith($"User_{userId}_"));
                            Log($"用户{userId}拥有{userScriptsCount}个脚本");

                            // 清理用户脚本
                            foreach (var script in userScripts)
                            {
                                await _scriptService.DeleteAsync(script.Id);
                                Log($"用户{userId}删除脚本: {script.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"用户{userId}操作失败: {ex.Message}");
                            throw;
                        }
                    });

                    allTasks.Add(userTask);
                }

                // 2. 等待所有用户操作完成
                await Task.WhenAll(allTasks);
                Log($"步骤2: 所有用户操作完成");

                // 3. 验证系统状态
                var finalScripts = await _scriptService.GetAllAsync();
                finalScripts.Should().BeEmpty();
                Log($"步骤3: 系统状态验证完成 - 剩余{finalScripts.Count}个脚本");

                Log("并发工作流测试完成");
            }
            catch (Exception ex)
            {
                LogError($"并发工作流测试失败: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task ScriptExecutionWorkflow_ShouldHandleInterruptions()
        {
            try
            {
                // 1. 创建长时间运行的脚本
                var script = TestFixtures.CreateScriptWithActions(10);
                foreach (var action in script.Actions)
                {
                    if (action.Type == ActionType.Delay)
                    {
                        action.Delay = TimeSpan.FromMilliseconds(1000); // 增加延迟
                    }
                }
                Log($"步骤1: 创建长时间运行脚本 - {script.Actions.Count}个动作");

                // 2. 开始执行脚本
                var executionTask = _executionEngine.ExecuteAsync(script);
                Log($"步骤2: 开始执行脚本");

                // 3. 等待一段时间后中断
                await Task.Delay(2000);
                await _executionEngine.StopExecutionAsync(script.Id);
                Log($"步骤3: 中断脚本执行");

                // 4. 等待执行完成
                var result = await executionTask;
                Log($"步骤4: 脚本执行完成 - 成功: {result.Success}");

                // 5. 验证中断状态
                var executionHistory = await _scriptRepository.GetExecutionHistoryAsync(script.Id);
                executionHistory.Should().NotBeEmpty();
                var lastExecution = executionHistory.Last();
                lastExecution.Status.Should().Be("Interrupted");
                Log($"步骤5: 验证中断状态 - {lastExecution.Status}");

                // 6. 清理
                await _scriptService.DeleteAsync(script.Id);
                Log($"步骤6: 脚本清理完成");

                Log("脚本执行中断处理测试完成");
            }
            catch (Exception ex)
            {
                LogError($"脚本执行中断处理测试失败: {ex.Message}");
                throw;
            }
        }
    }
}