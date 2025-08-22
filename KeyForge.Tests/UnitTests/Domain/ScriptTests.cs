using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.UnitTests.Domain
{
    /// <summary>
    /// 脚本聚合根单元测试
    /// </summary>
    public class ScriptTests : TestBase
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Script";
            var description = "A test script";

            // Act
            var script = new Script(id, name, description);

            // Assert
            ShouldBeValidScript(script);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Status.Should().Be(ScriptStatus.Draft);
            script.Actions.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var id = Guid.NewGuid();
            var description = "A test script";

            // Act & Assert
            var action = () => new Script(id, invalidName, description);
            ShouldThrowValidationException(action, "Script name cannot be empty.");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var originalVersion = script.Version;
            var newName = "Updated Script";
            var newDescription = "Updated description";

            // Act
            script.Update(newName, newDescription);

            // Assert
            script.Name.Should().Be(newName);
            script.Description.Should().Be(newDescription);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act & Assert
            var action = () => script.Update(invalidName, "Updated description");
            ShouldThrowValidationException(action, "Script name cannot be empty.");
        }

        [Fact]
        public void AddAction_WithValidAction_ShouldAddAction()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            var originalVersion = script.Version;

            // Act
            script.AddAction(action);

            // Assert
            script.Actions.Should().Contain(action);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void AddAction_WithNullAction_ShouldThrowValidationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act & Assert
            var action = () => script.AddAction(null);
            ShouldThrowValidationException(action, "Action cannot be null.");
        }

        [Fact]
        public void AddAction_WhenScriptNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Activate();
            var action = TestDataFactory.CreateGameAction();

            // Act & Assert
            var act = () => script.AddAction(action);
            ShouldThrowBusinessRuleViolationException(act, "Can only add actions to draft scripts.");
        }

        [Fact]
        public void RemoveAction_WithValidActionId_ShouldRemoveAction()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            var originalVersion = script.Version;

            // Act
            script.RemoveAction(action.Id);

            // Assert
            script.Actions.Should().NotContain(action);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void RemoveAction_WithInvalidActionId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var invalidId = Guid.NewGuid();

            // Act & Assert
            var action = () => script.RemoveAction(invalidId);
            ShouldThrowEntityNotFoundException<GameAction>(action, nameof(GameAction), invalidId);
        }

        [Fact]
        public void RemoveAction_WhenScriptNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            script.Activate();

            // Act & Assert
            var act = () => script.RemoveAction(action.Id);
            ShouldThrowBusinessRuleViolationException(act, "Can only remove actions from draft scripts.");
        }

        [Fact]
        public void Activate_WithValidDraftScript_ShouldActivateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            var originalVersion = script.Version;

            // Act
            script.Activate();

            // Assert
            script.Status.Should().Be(ScriptStatus.Active);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Activate_WhenScriptNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Activate();

            // Act & Assert
            var action = () => script.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Only draft scripts can be activated.");
        }

        [Fact]
        public void Activate_WhenScriptHasNoActions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act & Assert
            var action = () => script.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Cannot activate script with no actions.");
        }

        [Fact]
        public void Deactivate_WithActiveScript_ShouldDeactivateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            script.Activate();
            var originalVersion = script.Version;

            // Act
            script.Deactivate();

            // Assert
            script.Status.Should().Be(ScriptStatus.Inactive);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Deactivate_WhenScriptNotActive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act & Assert
            var action = () => script.Deactivate();
            ShouldThrowBusinessRuleViolationException(action, "Only active scripts can be deactivated.");
        }

        [Fact]
        public void Delete_WithValidScript_ShouldDeleteScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var originalVersion = script.Version;

            // Act
            script.Delete();

            // Assert
            script.Status.Should().Be(ScriptStatus.Deleted);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Delete_WhenScriptAlreadyDeleted_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Delete();

            // Act & Assert
            var action = () => script.Delete();
            ShouldThrowBusinessRuleViolationException(action, "Script is already deleted.");
        }

        [Fact]
        public void GetActionSequence_ShouldReturnActionSequence()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action1 = TestDataFactory.CreateGameAction();
            var action2 = TestDataFactory.CreateGameAction();
            script.AddAction(action1);
            script.AddAction(action2);

            // Act
            var sequence = script.GetActionSequence();

            // Assert
            sequence.Should().NotBeNull();
            sequence.Actions.Should().HaveCount(2);
            sequence.Actions.Should().ContainInOrder(action1, action2);
        }

        [Fact]
        public void GetEstimatedDuration_WithActions_ShouldReturnCorrectDuration()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action1 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A");
            var action2 = new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Wait");
            var action3 = new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 200, "Release A");
            
            script.AddAction(action1);
            script.AddAction(action2);
            script.AddAction(action3);

            // Act
            var duration = script.GetEstimatedDuration();

            // Assert
            duration.Should().Be(TimeSpan.FromMilliseconds(1300));
        }

        [Fact]
        public void GetEstimatedDuration_WithNoActions_ShouldReturnZero()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act
            var duration = script.GetEstimatedDuration();

            // Assert
            duration.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void DomainEvents_ShouldBeRaisedCorrectly()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);

            // Act
            script.Activate();

            // Assert
            var domainEvents = script.DomainEvents;
            domainEvents.Should().HaveCount(3); // Created, ActionAdded, Activated
            domainEvents.Should().Contain(e => e.GetType().Name == "ScriptCreatedEvent");
            domainEvents.Should().Contain(e => e.GetType().Name == "ScriptActivatedEvent");
        }
    }
}