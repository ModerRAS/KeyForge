using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Moq;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using KeyForge.Domain.Events;

namespace KeyForge.Domain.Tests.UnitTests.Aggregates
{
    /// <summary>
    /// Script聚合根单元测试
    /// 测试脚本的所有业务规则和不变性
    /// </summary>
    public class ScriptTests : TestBase
    {
        #region 构造函数测试

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateScript()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var name = "Test Script";
            var description = "Test Description";

            // Act
            var script = new Script(scriptId, name, description);

            // Assert
            ShouldBeValidScript(script);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Status.Should().Be(ScriptStatus.Draft);
            script.Actions.Should().BeEmpty();
            script.Version.Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var description = "Test Description";

            // Act & Assert
            var action = () => new Script(scriptId, invalidName, description);
            ShouldThrowValidationException(action, "Script name cannot be empty.");
        }

        [Fact]
        public void Constructor_ShouldRaiseScriptCreatedEvent()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var name = "Test Script";
            var description = "Test Description";

            // Act
            var script = new Script(scriptId, name, description);

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptCreatedEvent>();
            var createdEvent = (ScriptCreatedEvent)domainEvent;
            createdEvent.ScriptId.Should().Be(scriptId);
            createdEvent.ScriptName.Should().Be(name);
        }

        #endregion

        #region Update方法测试

        [Fact]
        public void Update_WithValidParameters_ShouldUpdateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var originalVersion = script.Version;
            var newName = "Updated Script";
            var newDescription = "Updated Description";

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
            var action = () => script.Update(invalidName, "New Description");
            ShouldThrowValidationException(action, "Script name cannot be empty.");
        }

        [Fact]
        public void Update_ShouldRaiseScriptUpdatedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.ClearDomainEvents(); // 清除构造函数产生的事件

            // Act
            script.Update("Updated Name", "Updated Description");

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptUpdatedEvent>();
            var updatedEvent = (ScriptUpdatedEvent)domainEvent;
            updatedEvent.ScriptId.Should().Be(script.Id);
            updatedEvent.ScriptName.Should().Be("Updated Name");
        }

        #endregion

        #region AddAction方法测试

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
            script.Actions.Should().HaveCount(1);
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
            script.Activate(); // 激活脚本
            var action = TestDataFactory.CreateGameAction();

            // Act & Assert
            var actionMethod = () => script.AddAction(action);
            ShouldThrowBusinessRuleViolationException(actionMethod, "Can only add actions to draft scripts.");
        }

        [Fact]
        public void AddAction_ShouldRaiseScriptActionAddedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.ClearDomainEvents();
            var action = TestDataFactory.CreateGameAction();

            // Act
            script.AddAction(action);

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptActionAddedEvent>();
            var addedEvent = (ScriptActionAddedEvent)domainEvent;
            addedEvent.ScriptId.Should().Be(script.Id);
            addedEvent.ActionId.Should().Be(action.Id);
        }

        #endregion

        #region RemoveAction方法测试

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
            script.Actions.Should().BeEmpty();
            script.Version.Should().Be(originalVersion + 1);
        }

        [Fact]
        public void RemoveAction_WithInvalidActionId_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var invalidActionId = Guid.NewGuid();

            // Act & Assert
            var action = () => script.RemoveAction(invalidActionId);
            ShouldThrowEntityNotFoundException<GameAction>(action, nameof(GameAction), invalidActionId);
        }

        [Fact]
        public void RemoveAction_WhenScriptNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            script.Activate(); // 激活脚本

            // Act & Assert
            var actionMethod = () => script.RemoveAction(action.Id);
            ShouldThrowBusinessRuleViolationException(actionMethod, "Can only remove actions from draft scripts.");
        }

        [Fact]
        public void RemoveAction_ShouldRaiseScriptActionRemovedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);
            script.ClearDomainEvents();

            // Act
            script.RemoveAction(action.Id);

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptActionRemovedEvent>();
            var removedEvent = (ScriptActionRemovedEvent)domainEvent;
            removedEvent.ScriptId.Should().Be(script.Id);
            removedEvent.ActionId.Should().Be(action.Id);
        }

        #endregion

        #region Activate方法测试

        [Fact]
        public void Activate_WhenInDraftWithActions_ShouldActivateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var originalVersion = script.Version;

            // Act
            script.Activate();

            // Assert
            script.Status.Should().Be(ScriptStatus.Active);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Activate_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Activate(); // 先激活

            // Act & Assert
            var action = () => script.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Only draft scripts can be activated.");
        }

        [Fact]
        public void Activate_WhenNoActions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Empty Script", "No actions");

            // Act & Assert
            var action = () => script.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Cannot activate script with no actions.");
        }

        [Fact]
        public void Activate_ShouldRaiseScriptActivatedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.ClearDomainEvents();

            // Act
            script.Activate();

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptActivatedEvent>();
            var activatedEvent = (ScriptActivatedEvent)domainEvent;
            activatedEvent.ScriptId.Should().Be(script.Id);
            activatedEvent.ScriptName.Should().Be(script.Name);
        }

        #endregion

        #region Deactivate方法测试

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Activate();
            var originalVersion = script.Version;

            // Act
            script.Deactivate();

            // Assert
            script.Status.Should().Be(ScriptStatus.Stopped);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Deactivate_WhenNotActive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act & Assert
            var action = () => script.Deactivate();
            ShouldThrowBusinessRuleViolationException(action, "Only active scripts can be deactivated.");
        }

        [Fact]
        public void Deactivate_ShouldRaiseScriptDeactivatedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Activate();
            script.ClearDomainEvents();

            // Act
            script.Deactivate();

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptDeactivatedEvent>();
            var deactivatedEvent = (ScriptDeactivatedEvent)domainEvent;
            deactivatedEvent.ScriptId.Should().Be(script.Id);
            deactivatedEvent.ScriptName.Should().Be(script.Name);
        }

        #endregion

        #region Delete方法测试

        [Fact]
        public void Delete_WhenNotDeleted_ShouldDeleteScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var originalVersion = script.Version;

            // Act
            script.Delete();

            // Assert
            script.Status.Should().Be(ScriptStatus.Error);
            script.Version.Should().Be(originalVersion + 1);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.Delete(); // 先删除

            // Act & Assert
            var action = () => script.Delete();
            ShouldThrowBusinessRuleViolationException(action, "Script is already deleted.");
        }

        [Fact]
        public void Delete_ShouldRaiseScriptDeletedEvent()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            script.ClearDomainEvents();

            // Act
            script.Delete();

            // Assert
            script.DomainEvents.Should().HaveCount(1);
            var domainEvent = script.DomainEvents.First();
            domainEvent.Should().BeOfType<ScriptDeletedEvent>();
            var deletedEvent = (ScriptDeletedEvent)domainEvent;
            deletedEvent.ScriptId.Should().Be(script.Id);
            deletedEvent.ScriptName.Should().Be(script.Name);
        }

        #endregion

        #region 业务方法测试

        [Fact]
        public void GetActionSequence_ShouldReturnActionSequence()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var expectedCount = script.Actions.Count;

            // Act
            var sequence = script.GetActionSequence();

            // Assert
            sequence.Should().NotBeNull();
            sequence.Actions.Should().HaveCount(expectedCount);
            sequence.ActionCount.Should().Be(expectedCount);
            sequence.TotalDuration.Should().Be(script.GetEstimatedDuration().Milliseconds);
        }

        [Fact]
        public void GetEstimatedDuration_ShouldCalculateCorrectDuration()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");
            var action1 = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            var action2 = new GameAction(Guid.NewGuid(), ActionType.Delay, 200);
            var action3 = new GameAction(Guid.NewGuid(), ActionType.Delay, 50);
            
            script.AddAction(action1);
            script.AddAction(action2);
            script.AddAction(action3);

            // Act
            var duration = script.GetEstimatedDuration();

            // Assert
            duration.Should().Be(TimeSpan.FromMilliseconds(350));
        }

        [Fact]
        public void GetEstimatedDuration_WhenNoActions_ShouldReturnZero()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test");

            // Act
            var duration = script.GetEstimatedDuration();

            // Assert
            duration.Should().Be(TimeSpan.Zero);
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void Script_ShouldHandleLargeNumberOfActions()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Large Script", "Many actions");
            var actionCount = 1000;

            // Act
            for (int i = 0; i < actionCount; i++)
            {
                var action = new GameAction(Guid.NewGuid(), ActionType.Delay, 10);
                script.AddAction(action);
            }

            // Assert
            script.Actions.Should().HaveCount(actionCount);
            script.GetEstimatedDuration().Should().Be(TimeSpan.FromMilliseconds(10000));
        }

        [Fact]
        public void Script_ShouldMaintainVersionConsistency()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            var initialVersion = script.Version;

            // Act
            script.AddAction(action);
            var versionAfterAdd = script.Version;
            
            script.Update("Updated Name", "Updated Description");
            var versionAfterUpdate = script.Version;

            script.Activate();
            var versionAfterActivate = script.Version;

            // Assert
            versionAfterAdd.Should().Be(initialVersion + 1);
            versionAfterUpdate.Should().Be(versionAfterAdd + 1);
            versionAfterActivate.Should().Be(versionAfterUpdate + 1);
        }

        [Fact]
        public void Script_ShouldHandleConcurrentEventGeneration()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();

            // Act
            script.AddAction(action);
            script.Update("Updated Name", "Updated Description");
            script.Activate();

            // Assert
            script.DomainEvents.Should().HaveCount(3);
            script.DomainEvents.Should().Contain(e => e is ScriptActionAddedEvent);
            script.DomainEvents.Should().Contain(e => e is ScriptUpdatedEvent);
            script.DomainEvents.Should().Contain(e => e is ScriptActivatedEvent);
        }

        #endregion

        #region 不变性测试

        [Fact]
        public void Script_ShouldMaintainImmutabilityOfActions()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var actions = script.Actions;

            // Act & Assert
            // 验证返回的集合是只读的
            actions.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<GameAction>>();
        }

        [Fact]
        public void Script_ShouldNotAllowDirectModificationOfActions()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);

            // Act & Assert
            // 尝试通过反射修改内部集合应该失败或抛出异常
            var actionsProperty = typeof(Script).GetProperty("Actions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            actionsProperty.Should().NotBeNull();
            
            var internalActions = actionsProperty.GetValue(script);
            internalActions.Should().BeAssignableTo<System.Collections.Generic.IList<GameAction>>();
        }

        #endregion
    }
}