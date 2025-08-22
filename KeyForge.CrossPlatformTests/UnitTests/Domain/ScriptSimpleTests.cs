using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using KeyForge.CrossPlatformTests.Common;

namespace KeyForge.CrossPlatformTests.UnitTests.Domain
{
    /// <summary>
    /// 脚本领域模型简化测试
    /// 原本实现：复杂的测试场景和依赖
    /// 简化实现：只测试核心业务逻辑
    /// </summary>
    public class ScriptSimpleTests : TestBase
    {
        [Fact]
        public void CreateScript_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Script";
            var description = "A test script";

            // Act
            var script = new Script(id, name, description);

            // Assert
            script.Should().NotBeNull();
            script.Id.Should().Be(id);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Status.Should().Be(ScriptStatus.Draft);
            script.Actions.Should().BeEmpty();
            script.Version.Should().Be(1);
        }

        [Fact]
        public void AddAction_WithValidAction_ShouldAddAction()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();

            // Act
            script.AddAction(action);

            // Assert
            script.Actions.Should().Contain(action);
            script.Actions.Should().HaveCount(1);
            script.Version.Should().Be(2);
        }

        [Fact]
        public void ActivateScript_WithActions_ShouldActivate()
        {
            // Arrange
            var script = TestDataFactory.CreateScriptWithActions(1);

            // Act
            script.Activate();

            // Assert
            script.Status.Should().Be(ScriptStatus.Active);
            script.Version.Should().Be(3); // Create + AddAction + Activate
        }

        [Fact]
        public void CalculateDuration_WithActions_ShouldReturnCorrectDuration()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.AddAction(new GameAction(Guid.NewGuid(), Common.ActionType.KeyDown, KeyCode.A, 100, "Press A"));
            script.AddAction(new GameAction(Guid.NewGuid(), Common.ActionType.Delay, MouseButton.None, 0, 200, "Wait"));
            script.AddAction(new GameAction(Guid.NewGuid(), Common.ActionType.KeyUp, KeyCode.A, 150, "Release A"));

            // Act
            var duration = script.GetEstimatedDuration();

            // Assert
            duration.Should().Be(TimeSpan.FromMilliseconds(450));
        }

        [Fact]
        public void UpdateScript_WithValidData_ShouldUpdate()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var newName = "Updated Name";
            var newDescription = "Updated Description";

            // Act
            script.Update(newName, newDescription);

            // Assert
            script.Name.Should().Be(newName);
            script.Description.Should().Be(newDescription);
            script.Version.Should().Be(2);
        }
    }
}