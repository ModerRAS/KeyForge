using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using KeyForge.Tests.Support;
using KeyForge.Domain;
using KeyForge.Application;
using KeyForge.Infrastructure;
using KeyForge.Core;
using Microsoft.Extensions.DependencyInjection;

namespace KeyForge.Tests.Integration.LayerInteraction
{
    /// <summary>
    /// 模块间交互测试 - 验证Domain、Application、Infrastructure、Presentation各层之间的交互
    /// 原本实现：简单的层间测试
    /// 简化实现：完整的层间交互验证测试
    /// </summary>
    public class LayerInteractionTests : TestBase
    {
        private readonly IScriptService _scriptService;
        private readonly IScriptRepository _scriptRepository;
        private readonly IExecutionEngine _executionEngine;
        private readonly IValidationService _validationService;

        public LayerInteractionTests(ITestOutputHelper output) : base(output)
        {
            _scriptRepository = ServiceProvider.GetRequiredService<IScriptRepository>();
            _scriptService = ServiceProvider.GetRequiredService<IScriptService>();
            _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
            _validationService = ServiceProvider.GetRequiredService<IValidationService>();
        }

        [Fact]
        public async Task ScriptCreation_ShouldIntegrateAllLayers()
        {
            // Arrange - Domain层
            var script = TestFixtures.CreateScriptWithActions(3);
            Log($"创建脚本: {script.Name}, 动作数量: {script.Actions.Count}");

            // Act - Application层
            await _scriptService.SaveAsync(script);

            // Assert - Infrastructure层
            var savedScript = await _scriptRepository.GetByIdAsync(script.Id);
            savedScript.Should().NotBeNull();
            savedScript.Name.Should().Be(script.Name);
            savedScript.Actions.Should().HaveCount(3);
            Log($"脚本保存成功: {savedScript.Id}");

            // Act - Execution Engine (Core层)
            var result = await _executionEngine.ExecuteAsync(savedScript);

            // Assert - Execution Result
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ExecutedActions.Should().Be(3);
            result.ExecutionTime.Should().BeGreaterThan(0);
            Log($"脚本执行成功: 执行{result.ExecutedActions}个动作, 耗时{result.ExecutionTime}ms");
        }

        [Fact]
        public async Task LayerCommunication_ShouldHandleExceptions()
        {
            // Arrange - Domain层创建无效脚本
            var invalidScript = TestFixtures.CreateValidScript();
            invalidScript.Name = ""; // 无效的脚本名称

            // Act & Assert - Application层到Domain层的异常传播
            var action = async () => await _scriptService.SaveAsync(invalidScript);
            await action.Should().ThrowAsync<BusinessRuleViolationException>();
            LogError("无效脚本名称异常处理成功");
        }

        [Fact]
        public async Task ValidationService_ShouldIntegrateWithApplicationLayer()
        {
            // Arrange - Domain层
            var script = TestFixtures.CreateScriptWithActions(5);
            
            // Act - Application层验证
            var validationResult = await _validationService.ValidateScriptAsync(script);

            // Assert - 验证结果
            validationResult.Should().NotBeNull();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Errors.Should().BeEmpty();
            Log($"脚本验证成功: {validationResult.IsValid}");

            // Act - Application层保存
            await _scriptService.SaveAsync(script);

            // Assert - Infrastructure层持久化
            var savedScript = await _scriptRepository.GetByIdAsync(script.Id);
            savedScript.Should().NotBeNull();
            savedScript.Name.Should().Be(script.Name);
            Log($"验证后的脚本保存成功");
        }

        [Fact]
        public async Task ScriptExecution_ShouldIntegrateWithInfrastructure()
        {
            // Arrange - Domain层创建脚本
            var script = TestFixtures.CreateScriptWithActions(2);
            script.Activate(); // 激活脚本

            // Act - Application层保存
            await _scriptService.SaveAsync(script);

            // Act - Core层执行
            var result = await _executionEngine.ExecuteAsync(script);

            // Assert - 执行结果
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ExecutedActions.Should().Be(2);
            
            // Assert - Infrastructure层更新执行历史
            var executionHistory = await _scriptRepository.GetExecutionHistoryAsync(script.Id);
            executionHistory.Should().NotBeEmpty();
            executionHistory.Should().Contain(h => h.ScriptId == script.Id);
            Log($"执行历史记录: {executionHistory.Count}条");
        }

        [Fact]
        public async Task ScriptUpdate_ShouldPropagateThroughAllLayers()
        {
            // Arrange - Domain层创建脚本
            var script = TestFixtures.CreateValidScript();
            await _scriptService.SaveAsync(script);

            // Act - Application层更新
            script.Name = "UpdatedScriptName";
            script.Description = "Updated description";
            await _scriptService.UpdateAsync(script);

            // Assert - Infrastructure层更新
            var updatedScript = await _scriptRepository.GetByIdAsync(script.Id);
            updatedScript.Should().NotBeNull();
            updatedScript.Name.Should().Be("UpdatedScriptName");
            updatedScript.Description.Should().Be("Updated description");
            Log($"脚本更新成功: {updatedScript.Name}");

            // Act - Core层验证更新后的脚本
            var validationResult = await _validationService.ValidateScriptAsync(updatedScript);

            // Assert - 验证结果
            validationResult.Should().NotBeNull();
            validationResult.IsValid.Should().BeTrue();
            Log($"更新后的脚本验证通过");
        }

        [Fact]
        public async Task ConcurrentOperations_ShouldHandleLayerInteraction()
        {
            // Arrange - Domain层创建多个脚本
            var scripts = TestFixtures.CreateMultipleScripts(5);
            
            // Act - 并行保存到Application层
            var saveTasks = scripts.Select(script => _scriptService.SaveAsync(script));
            await Task.WhenAll(saveTasks);

            // Assert - Infrastructure层持久化
            var savedScripts = await _scriptService.GetAllAsync();
            savedScripts.Should().HaveCount(5);
            Log($"并行保存{savedScripts.Count}个脚本成功");

            // Act - 并行执行
            var executionTasks = savedScripts.Select(script => _executionEngine.ExecuteAsync(script));
            var executionResults = await Task.WhenAll(executionTasks);

            // Assert - 执行结果
            executionResults.Should().AllSatisfy(result => result.Success.Should().BeTrue());
            executionResults.Should().AllSatisfy(result => result.ExecutedActions.Should().BeGreaterThan(0));
            Log($"并行执行{executionResults.Length}个脚本成功");
        }

        [Fact]
        public async Task ScriptLifecycle_ShouldWorkAcrossAllLayers()
        {
            // Arrange - Domain层创建脚本
            var script = TestFixtures.CreateScriptWithActions(3);
            Log($"创建脚本: {script.Name}");

            // Act & Assert - 生命周期管理
            // 1. Created -> Active
            script.Activate();
            await _scriptService.SaveAsync(script);
            var activeScript = await _scriptRepository.GetByIdAsync(script.Id);
            activeScript.Status.Should().Be(ScriptStatus.Active);
            Log($"脚本激活: {activeScript.Status}");

            // 2. Active -> Paused
            script.Pause();
            await _scriptService.UpdateAsync(script);
            var pausedScript = await _scriptRepository.GetByIdAsync(script.Id);
            pausedScript.Status.Should().Be(ScriptStatus.Paused);
            Log($"脚本暂停: {pausedScript.Status}");

            // 3. Paused -> Active
            script.Activate();
            await _scriptService.UpdateAsync(script);
            var reactivatedScript = await _scriptRepository.GetByIdAsync(script.Id);
            reactivatedScript.Status.Should().Be(ScriptStatus.Active);
            Log($"脚本重新激活: {reactivatedScript.Status}");

            // 4. Active -> Completed
            var executionResult = await _executionEngine.ExecuteAsync(reactivatedScript);
            executionResult.Success.Should().BeTrue();
            Log($"脚本执行完成: {executionResult.Success}");

            // 5. 清理
            await _scriptService.DeleteAsync(script.Id);
            var deletedScript = await _scriptRepository.GetByIdAsync(script.Id);
            deletedScript.Should().BeNull();
            Log($"脚本删除成功");
        }

        [Fact]
        public async Task ErrorHandling_ShouldPropagateCorrectly()
        {
            // Arrange - Domain层创建无效脚本
            var invalidScript = TestFixtures.CreateValidScript();
            invalidScript.Actions.Add(new GameAction
            {
                Type = ActionType.Keyboard,
                Key = "", // 无效的按键
                Timestamp = DateTime.UtcNow
            });

            // Act & Assert - Application层验证
            var validationResult = await _validationService.ValidateScriptAsync(invalidScript);
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().NotBeEmpty();
            validationResult.Errors.Should().Contain(e => e.Contains("按键"));
            Log($"验证失败: {validationResult.Errors.Count}个错误");

            // Act & Assert - 尝试保存无效脚本
            var saveAction = async () => await _scriptService.SaveAsync(invalidScript);
            await saveAction.Should().ThrowAsync<BusinessRuleViolationException>();
            LogError("无效脚本保存异常处理成功");
        }

        [Fact]
        public async Task Performance_MultipleLayerInteractions_ShouldBeFast()
        {
            // Arrange - Domain层创建多个脚本
            var scriptCount = 10;
            var scripts = TestFixtures.CreateMultipleScripts(scriptCount);

            var startTime = DateTime.UtcNow;

            // Act - 多层交互操作
            // 1. 保存所有脚本
            foreach (var script in scripts)
            {
                await _scriptService.SaveAsync(script);
            }

            // 2. 执行所有脚本
            var executionTasks = scripts.Select(script => _executionEngine.ExecuteAsync(script));
            await Task.WhenAll(executionTasks);

            // 3. 更新所有脚本
            foreach (var script in scripts)
            {
                script.Name = $"Updated_{script.Name}";
                await _scriptService.UpdateAsync(script);
            }

            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;

            // Assert - 性能验证
            duration.Should().BeLessThan(5000); // 5秒内完成
            Log($"多层交互性能测试完成: {duration:F2}ms, 处理{scriptCount}个脚本");
        }
    }
}