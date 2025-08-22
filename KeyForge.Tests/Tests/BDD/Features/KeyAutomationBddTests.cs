using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using KeyForge.Tests.Support;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;

namespace KeyForge.Tests.BDD.Features;

/// <summary>
/// 按键自动化模块BDD测试
/// 基于验收标准 AC-FUNC-001
/// 原本实现：复杂的BDD场景
/// 简化实现：核心按键自动化功能测试
/// </summary>
public class KeyAutomationBddTests : BddTestBase
{
    public KeyAutomationBddTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void ScriptRecordingAndPlayback_ShouldWorkCorrectly()
    {
        // 基于验收标准 AC-FUNC-001: 按键自动化模块
        
        Given("I have a new script for recording", () =>
        {
            var script = CreateTestScript();
            script.Should().NotBeNull();
            script.Status.Should().Be(ScriptStatus.Draft);
        });

        When("I record keyboard actions", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateDelayAction());
            script.AddAction(CreateKeyboardAction());
        });

        Then("The script should contain all recorded actions", () =>
        {
            var script = CreateScriptWithActions(3);
            script.Actions.Should().HaveCount(3);
            script.Actions.Count(a => a.IsKeyboardAction).Should().Be(2);
            script.Actions.Count(a => a.IsDelayAction).Should().Be(1);
        });

        And("The script should have correct total duration", () =>
        {
            var script = CreateScriptWithActions(3);
            var duration = script.GetEstimatedDuration();
            duration.Should().BeGreaterThan(TimeSpan.Zero);
            duration.TotalMilliseconds.Should().BePositive();
        });

        When("I activate the script", () =>
        {
            var script = CreateScriptWithActions(3);
            script.Activate();
            script.Status.Should().Be(ScriptStatus.Active);
        });

        Then("The script should be ready for playback", () =>
        {
            var script = CreateScriptWithActions(3);
            script.Activate();
            script.Status.Should().Be(ScriptStatus.Active);
            script.Actions.Should().HaveCount(3);
        });

        When("I deactivate the script", () =>
        {
            var script = CreateScriptWithActions(3);
            script.Activate();
            script.Deactivate();
        });

        Then("The script should not be available for playback", () =>
        {
            var script = CreateScriptWithActions(3);
            script.Activate();
            script.Deactivate();
            script.Status.Should().Be(ScriptStatus.Inactive);
        });
    }

    [Fact]
    public void ScriptManagement_ShouldSupportCrudOperations()
    {
        // 基于验收标准 AC-FUNC-001: 脚本管理
        
        Given("I have multiple scripts", () =>
        {
            var scripts = new[]
            {
                CreateTestScript(),
                CreateTestScript(),
                CreateTestScript()
            };
            scripts.Length.Should().Be(3);
        });

        When("I save scripts to storage", () =>
        {
            var scripts = new[]
            {
                CreateTestScript(),
                CreateTestScript(),
                CreateTestScript()
            };
            
            foreach (var script in scripts)
            {
                var filePath = Path.Combine(_testDirectory, $"{script.Id}.json");
                FileStorage.SaveScript(script, filePath);
            }
        });

        Then("All scripts should be persisted", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            files.Length.Should().Be(3);
        });

        When("I load scripts from storage", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            files.Length.Should().Be(3);
        });

        Then("All scripts should be loaded correctly", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            var loadedScripts = files.Select(f => FileStorage.LoadScript<Script>(f)).Where(s => s != null).ToList();
            loadedScripts.Count.Should().Be(3);
        });

        When("I delete a script", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            if (files.Length > 0)
            {
                File.Delete(files[0]);
            }
        });

        Then("The script should be removed from storage", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            files.Length.Should().Be(2);
        });
    }

    [Fact]
    public void ScriptValidation_ShouldEnforceBusinessRules()
    {
        // 基于验收标准 AC-FUNC-001: 脚本验证和错误处理
        
        Given("I have a script with invalid name", () =>
        {
            Action action = () => new Script(Guid.NewGuid(), "", "Test Description");
            action.Should().Throw<ArgumentException>();
        });

        When("I try to create the script", () =>
        {
            Action action = () => new Script(Guid.NewGuid(), "", "Test Description");
        });

        Then("The system should reject the invalid script", () =>
        {
            Action action = () => new Script(Guid.NewGuid(), "", "Test Description");
            action.Should().Throw<ArgumentException>();
        });

        Given("I have an empty script", () =>
        {
            var script = CreateTestScript();
            script.Actions.Should().BeEmpty();
        });

        When("I try to activate it", () =>
        {
            var script = CreateTestScript();
            Action action = () => script.Activate();
        });

        Then("The system should prevent activation", () =>
        {
            var script = CreateTestScript();
            Action action = () => script.Activate();
            action.Should().Throw<BusinessRuleViolationException>();
        });

        Given("I have an active script", () =>
        {
            var script = CreateScriptWithActions(1);
            script.Activate();
            script.Status.Should().Be(ScriptStatus.Active);
        });

        When("I try to add actions to it", () =>
        {
            var script = CreateScriptWithActions(1);
            script.Activate();
            Action action = () => script.AddAction(CreateKeyboardAction());
        });

        Then("The system should prevent modification", () =>
        {
            var script = CreateScriptWithActions(1);
            script.Activate();
            Action action = () => script.AddAction(CreateKeyboardAction());
            action.Should().Throw<BusinessRuleViolationException>();
        });
    }

    [Fact]
    public void ScriptExecution_ShouldHandleVariousActionTypes()
    {
        // 基于验收标准 AC-FUNC-001: 执行准确率和错误处理
        
        Given("I have a script with mixed action types", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            
            script.Actions.Should().HaveCount(3);
            script.Actions.Count(a => a.IsKeyboardAction).Should().Be(1);
            script.Actions.Count(a => a.IsMouseAction).Should().Be(1);
            script.Actions.Count(a => a.IsDelayAction).Should().Be(1);
        });

        When("I prepare the script for execution", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            script.Activate();
        });

        Then("The script should be ready for execution", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            script.Activate();
            
            script.Status.Should().Be(ScriptStatus.Active);
            script.GetActionSequence().Should().NotBeNull();
        });

        And("The action sequence should be valid", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            
            var sequence = script.GetActionSequence();
            sequence.Should().NotBeNull();
            sequence.Count.Should().Be(3);
        });

        When("I execute the script", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            script.Activate();
            
            // 模拟执行 - 在实际系统中这里会调用执行引擎
            script.MarkAsExecuted();
        });

        Then("The execution should complete successfully", () =>
        {
            var script = CreateTestScript();
            script.AddAction(CreateKeyboardAction());
            script.AddAction(CreateMouseAction());
            script.AddAction(CreateDelayAction());
            script.Activate();
            script.MarkAsExecuted();
            
            script.Status.Should().Be(ScriptStatus.Executed);
        });
    }

    [Fact]
    public void ScriptPerformance_ShouldMeetRequirements()
    {
        // 基于验收标准 AC-FUNC-001: 性能要求
        
        Given("I have a large script with many actions", () =>
        {
            var script = CreateScriptWithActions(1000);
            script.Actions.Should().HaveCount(1000);
        });

        When("I process the script", () =>
        {
            var script = CreateScriptWithActions(1000);
            var startTime = DateTime.UtcNow;
            
            // 模拟处理 - 计算总时长
            var duration = script.GetEstimatedDuration();
            
            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;
            
            processingTime.Should().BeLessThan(100); // 处理时间应该小于100ms
        });

        Then("The processing should complete within time limits", () =>
        {
            var script = CreateScriptWithActions(1000);
            var startTime = DateTime.UtcNow;
            
            var duration = script.GetEstimatedDuration();
            
            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;
            
            processingTime.Should().BeLessThan(100);
        });

        And("The memory usage should be reasonable", () =>
        {
            var script = CreateScriptWithActions(1000);
            var initialMemory = GC.GetTotalMemory(false);
            
            var duration = script.GetEstimatedDuration();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;
            
            memoryIncrease.Should().BeLessThan(1024 * 1024); // 内存增加应该小于1MB
        });
    }
}