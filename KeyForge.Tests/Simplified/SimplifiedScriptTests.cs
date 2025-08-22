using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 简化的脚本聚合根测试
    /// 专门为跨平台环境设计，避免Windows特定的依赖
    /// </summary>
    public class SimplifiedScriptTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Script";
            var description = "Test script description";

            // Act
            var script = new Script(id, name, description);

            // Assert
            script.Should().NotBeNull();
            script.Id.Should().Be(id);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Actions.Should().BeEmpty();
            script.IsActive.Should().BeTrue();
            script.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "";
            var description = "Test script description";

            // Act & Assert
            var action = () => new Script(id, name, description);
            action.Should().Throw<ValidationException>()
                .WithMessage("Script name cannot be empty.");
        }

        [Fact]
        public void AddAction_WithValidAction_ShouldAddToScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            var action = CreateTestAction();

            // Act
            script.AddAction(action);

            // Assert
            script.Actions.Should().Contain(action);
            script.Actions.Should().HaveCount(1);
        }

        [Fact]
        public void RemoveAction_WithExistingAction_ShouldRemoveFromScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            var action = CreateTestAction();
            script.AddAction(action);

            // Act
            script.RemoveAction(action.Id);

            // Assert
            script.Actions.Should().NotContain(action);
            script.Actions.Should().BeEmpty();
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Original Name", "Original description");
            var newName = "Updated Name";
            var newDescription = "Updated description";

            // Act
            script.Update(newName, newDescription);

            // Assert
            script.Name.Should().Be(newName);
            script.Description.Should().Be(newDescription);
            script.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_WhenInactive_ShouldActivateScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            script.Deactivate();

            // Act
            script.Activate();

            // Assert
            script.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");

            // Act
            script.Deactivate();

            // Assert
            script.IsActive.Should().BeFalse();
        }

        [Fact]
        public void GetActionById_WithExistingAction_ShouldReturnAction()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            var action = CreateTestAction();
            script.AddAction(action);

            // Act
            var result = script.GetActionById(action.Id);

            // Assert
            result.Should().Be(action);
        }

        [Fact]
        public void GetActionById_WithNonExistingAction_ShouldReturnNull()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");

            // Act
            var result = script.GetActionById(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetActionsByType_WithExistingActions_ShouldReturnFilteredActions()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            var keyAction = CreateTestAction(ActionType.KeyDown);
            var mouseAction = CreateTestAction(ActionType.MouseDown);
            script.AddAction(keyAction);
            script.AddAction(mouseAction);

            // Act
            var keyActions = script.GetActionsByType(ActionType.KeyDown);
            var mouseActions = script.GetActionsByType(ActionType.MouseDown);

            // Assert
            keyActions.Should().Contain(keyAction);
            keyActions.Should().NotContain(mouseAction);
            mouseActions.Should().Contain(mouseAction);
            mouseActions.Should().NotContain(keyAction);
        }

        [Fact]
        public void ClearActions_WhenScriptHasActions_ShouldRemoveAllActions()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            script.AddAction(CreateTestAction());
            script.AddAction(CreateTestAction());

            // Act
            script.ClearActions();

            // Assert
            script.Actions.Should().BeEmpty();
        }

        [Fact]
        public void GetActionCount_WhenScriptHasActions_ShouldReturnCorrectCount()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            script.AddAction(CreateTestAction());
            script.AddAction(CreateTestAction());

            // Act
            var count = script.GetActionCount();

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void HasActions_WhenScriptHasActions_ShouldReturnTrue()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            script.AddAction(CreateTestAction());

            // Act
            var hasActions = script.HasActions();

            // Assert
            hasActions.Should().BeTrue();
        }

        [Fact]
        public void HasActions_WhenScriptHasNoActions_ShouldReturnFalse()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");

            // Act
            var hasActions = script.HasActions();

            // Assert
            hasActions.Should().BeFalse();
        }

        private static GameAction CreateTestAction(ActionType type = ActionType.KeyDown)
        {
            return type switch
            {
                ActionType.KeyDown => new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 10, "Key action"),
                ActionType.MouseDown => new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200, 10, "Mouse action"),
                ActionType.Delay => new GameAction(Guid.NewGuid(), ActionType.Delay, 100, "Delay action"),
                _ => new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 10, "Default action")
            };
        }
    }
}