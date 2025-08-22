using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Tests.UnitTests.ValueObjects
{
    /// <summary>
    /// ActionSequence值对象单元测试
    /// 测试动作序列的所有业务规则和不变性
    /// </summary>
    public class ActionSequenceTests : TestBase
    {
        #region 构造函数测试

        [Fact]
        public void Constructor_WithValidActions_ShouldCreateActionSequence()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.Actions.Should().HaveCount(3);
            sequence.ActionCount.Should().Be(3);
            sequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay));
            sequence.Actions.Should().BeEquivalentTo(actions);
        }

        [Fact]
        public void Constructor_WithEmptyActions_ShouldCreateEmptySequence()
        {
            // Arrange
            var actions = Enumerable.Empty<GameAction>();

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.Actions.Should().BeEmpty();
            sequence.ActionCount.Should().Be(0);
            sequence.TotalDuration.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithNullActions_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new ActionSequence(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("actions");
        }

        [Fact]
        public void Constructor_ShouldCreateReadOnlyCollection()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.Actions.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<GameAction>>();
        }

        #endregion

        #region AddAction方法测试

        [Fact]
        public void AddAction_WithValidAction_ShouldReturnNewSequenceWithAddedAction()
        {
            // Arrange
            var originalActions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(originalActions);
            var newAction = TestDataFactory.CreateGameAction();

            // Act
            var newSequence = sequence.AddAction(newAction);

            // Assert
            newSequence.Should().NotBeSameAs(sequence);
            newSequence.Actions.Should().HaveCount(3);
            newSequence.ActionCount.Should().Be(3);
            newSequence.Actions.Should().Contain(newAction);
            newSequence.Actions.Should().Contain(originalActions[0]);
            newSequence.Actions.Should().Contain(originalActions[1]);
            newSequence.TotalDuration.Should().Be(originalActions.Sum(a => a.Delay) + newAction.Delay);
            
            // 原始序列应该保持不变
            sequence.Actions.Should().HaveCount(2);
            sequence.ActionCount.Should().Be(2);
            sequence.Actions.Should().NotContain(newAction);
        }

        [Fact]
        public void AddAction_WithNullAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);

            // Act & Assert
            var action = () => sequence.AddAction(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("action");
        }

        [Fact]
        public void AddAction_ToEmptySequence_ShouldReturnSequenceWithOneAction()
        {
            // Arrange
            var sequence = new ActionSequence(Enumerable.Empty<GameAction>());
            var newAction = TestDataFactory.CreateGameAction();

            // Act
            var newSequence = sequence.AddAction(newAction);

            // Assert
            newSequence.Actions.Should().HaveCount(1);
            newSequence.ActionCount.Should().Be(1);
            newSequence.Actions.Should().Contain(newAction);
            newSequence.TotalDuration.Should().Be(newAction.Delay);
        }

        #endregion

        #region RemoveAction方法测试

        [Fact]
        public void RemoveAction_WithValidActionId_ShouldReturnNewSequenceWithoutAction()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);
            var sequence = new ActionSequence(actions);
            var actionToRemove = actions[1];

            // Act
            var newSequence = sequence.RemoveAction(actionToRemove.Id);

            // Assert
            newSequence.Should().NotBeSameAs(sequence);
            newSequence.Actions.Should().HaveCount(2);
            newSequence.ActionCount.Should().Be(2);
            newSequence.Actions.Should().NotContain(actionToRemove);
            newSequence.Actions.Should().Contain(actions[0]);
            newSequence.Actions.Should().Contain(actions[2]);
            newSequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay) - actionToRemove.Delay);
            
            // 原始序列应该保持不变
            sequence.Actions.Should().HaveCount(3);
            sequence.Actions.Should().Contain(actionToRemove);
        }

        [Fact]
        public void RemoveAction_WithInvalidActionId_ShouldThrowArgumentException()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var invalidActionId = Guid.NewGuid();

            // Act & Assert
            var action = () => sequence.RemoveAction(invalidActionId);
            action.Should().Throw<ArgumentException>()
                .WithMessage($"Action with id {invalidActionId} not found.");
        }

        [Fact]
        public void RemoveAction_FromSingleActionSequence_ShouldReturnEmptySequence()
        {
            // Arrange
            var action = TestDataFactory.CreateGameAction();
            var sequence = new ActionSequence(new[] { action });

            // Act
            var newSequence = sequence.RemoveAction(action.Id);

            // Assert
            newSequence.Actions.Should().BeEmpty();
            newSequence.ActionCount.Should().Be(0);
            newSequence.TotalDuration.Should().Be(0);
        }

        #endregion

        #region InsertAction方法测试

        [Fact]
        public void InsertAction_WithValidIndex_ShouldReturnNewSequenceWithInsertedAction()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);
            var sequence = new ActionSequence(actions);
            var newAction = TestDataFactory.CreateGameAction();
            var insertIndex = 1;

            // Act
            var newSequence = sequence.InsertAction(insertIndex, newAction);

            // Assert
            newSequence.Should().NotBeSameAs(sequence);
            newSequence.Actions.Should().HaveCount(4);
            newSequence.ActionCount.Should().Be(4);
            newSequence.Actions[insertIndex].Should().Be(newAction);
            newSequence.Actions.Should().Contain(actions[0]);
            newSequence.Actions.Should().Contain(actions[1]);
            newSequence.Actions.Should().Contain(actions[2]);
            newSequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay) + newAction.Delay);
            
            // 原始序列应该保持不变
            sequence.Actions.Should().HaveCount(3);
            sequence.Actions.Should().NotContain(newAction);
        }

        [Fact]
        public void InsertAction_AtBeginning_ShouldInsertAtFirstPosition()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var newAction = TestDataFactory.CreateGameAction();

            // Act
            var newSequence = sequence.InsertAction(0, newAction);

            // Assert
            newSequence.Actions.Should().HaveCount(3);
            newSequence.Actions[0].Should().Be(newAction);
            newSequence.Actions[1].Should().Be(actions[0]);
            newSequence.Actions[2].Should().Be(actions[1]);
        }

        [Fact]
        public void InsertAction_AtEnd_ShouldInsertAtLastPosition()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var newAction = TestDataFactory.CreateGameAction();

            // Act
            var newSequence = sequence.InsertAction(2, newAction);

            // Assert
            newSequence.Actions.Should().HaveCount(3);
            newSequence.Actions[0].Should().Be(actions[0]);
            newSequence.Actions[1].Should().Be(actions[1]);
            newSequence.Actions[2].Should().Be(newAction);
        }

        [Fact]
        public void InsertAction_WithNullAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);

            // Act & Assert
            var action = () => sequence.InsertAction(1, null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("action");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(10)]
        public void InsertAction_WithInvalidIndex_ShouldThrowArgumentOutOfRangeException(int invalidIndex)
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var newAction = TestDataFactory.CreateGameAction();

            // Act & Assert
            var action = () => sequence.InsertAction(invalidIndex, newAction);
            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("index");
        }

        #endregion

        #region ReorderActions方法测试

        [Fact]
        public void ReorderActions_WithValidOrder_ShouldReturnNewSequenceWithReorderedActions()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);
            var sequence = new ActionSequence(actions);
            var reorderedIds = new[] { actions[2].Id, actions[0].Id, actions[1].Id };

            // Act
            var newSequence = sequence.ReorderActions(reorderedIds);

            // Assert
            newSequence.Should().NotBeSameAs(sequence);
            newSequence.Actions.Should().HaveCount(3);
            newSequence.Actions[0].Id.Should().Be(actions[2].Id);
            newSequence.Actions[1].Id.Should().Be(actions[0].Id);
            newSequence.Actions[2].Id.Should().Be(actions[1].Id);
            newSequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay)); // 总延迟应该不变
            
            // 原始序列应该保持不变
            sequence.Actions[0].Id.Should().Be(actions[0].Id);
            sequence.Actions[1].Id.Should().Be(actions[1].Id);
            sequence.Actions[2].Id.Should().Be(actions[2].Id);
        }

        [Fact]
        public void ReorderActions_WithPartialOrder_ShouldIncludeOnlySpecifiedActions()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(4);
            var sequence = new ActionSequence(actions);
            var partialOrder = new[] { actions[2].Id, actions[0].Id };

            // Act
            var newSequence = sequence.ReorderActions(partialOrder);

            // Assert
            newSequence.Actions.Should().HaveCount(2);
            newSequence.Actions[0].Id.Should().Be(actions[2].Id);
            newSequence.Actions[1].Id.Should().Be(actions[0].Id);
            newSequence.TotalDuration.Should().Be(actions[2].Delay + actions[0].Delay);
        }

        [Fact]
        public void ReorderActions_WithUnknownIds_ShouldIgnoreUnknownIds()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var unknownIds = new[] { Guid.NewGuid(), actions[0].Id, Guid.NewGuid() };

            // Act
            var newSequence = sequence.ReorderActions(unknownIds);

            // Assert
            newSequence.Actions.Should().HaveCount(1);
            newSequence.Actions[0].Id.Should().Be(actions[0].Id);
        }

        [Fact]
        public void ReorderActions_WithEmptyOrder_ShouldReturnEmptySequence()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);
            var sequence = new ActionSequence(actions);
            var emptyOrder = Enumerable.Empty<Guid>();

            // Act
            var newSequence = sequence.ReorderActions(emptyOrder);

            // Assert
            newSequence.Actions.Should().BeEmpty();
            newSequence.ActionCount.Should().Be(0);
            newSequence.TotalDuration.Should().Be(0);
        }

        #endregion

        #region 值对象相等性测试

        [Fact]
        public void ActionSequence_WithSameActionsInSameOrder_ShouldBeEqual()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(3);
            var sequence1 = new ActionSequence(actions);
            var sequence2 = new ActionSequence(actions);

            // Act & Assert
            sequence1.Should().Be(sequence2);
            sequence1.GetHashCode().Should().Be(sequence2.GetHashCode());
            (sequence1 == sequence2).Should().BeTrue();
            (sequence1 != sequence2).Should().BeFalse();
        }

        [Fact]
        public void ActionSequence_WithSameActionsInDifferentOrder_ShouldNotBeEqual()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence1 = new ActionSequence(actions);
            var reversedActions = new[] { actions[1], actions[0] };
            var sequence2 = new ActionSequence(reversedActions);

            // Act & Assert
            sequence1.Should().NotBe(sequence2);
            sequence1.GetHashCode().Should().NotBe(sequence2.GetHashCode());
        }

        [Fact]
        public void ActionSequence_WithDifferentActionCount_ShouldNotBeEqual()
        {
            // Arrange
            var actions1 = TestDataFactory.CreateGameActions(2);
            var actions2 = TestDataFactory.CreateGameActions(3);
            var sequence1 = new ActionSequence(actions1);
            var sequence2 = new ActionSequence(actions2);

            // Act & Assert
            sequence1.Should().NotBe(sequence2);
            sequence1.GetHashCode().Should().NotBe(sequence2.GetHashCode());
        }

        [Fact]
        public void ActionSequence_WithDifferentActions_ShouldNotBeEqual()
        {
            // Arrange
            var actions1 = TestDataFactory.CreateGameActions(2);
            var actions2 = TestDataFactory.CreateGameActions(2);
            var sequence1 = new ActionSequence(actions1);
            var sequence2 = new ActionSequence(actions2);

            // Act & Assert
            sequence1.Should().NotBe(sequence2);
            sequence1.GetHashCode().Should().NotBe(sequence2.GetHashCode());
        }

        #endregion

        #region 空值和类型比较测试

        [Fact]
        public void ActionSequence_ShouldNotEqualNull()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);

            // Act & Assert
            sequence.Equals(null).Should().BeFalse();
            (sequence == null).Should().BeFalse();
            (null == sequence).Should().BeFalse();
        }

        [Fact]
        public void ActionSequence_ShouldNotEqualDifferentType()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);
            var otherObject = new object();

            // Act & Assert
            sequence.Equals(otherObject).Should().BeFalse();
        }

        [Fact]
        public void ActionSequence_ShouldEqualItself()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);

            // Act & Assert
            sequence.Equals(sequence).Should().BeTrue();
            (sequence == sequence).Should().BeTrue();
            (sequence != sequence).Should().BeFalse();
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void ActionSequence_ShouldHandleLargeNumberOfActions()
        {
            // Arrange
            var actionCount = 1000;
            var actions = TestDataFactory.CreateGameActions(actionCount);

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.Actions.Should().HaveCount(actionCount);
            sequence.ActionCount.Should().Be(actionCount);
            sequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay));
        }

        [Fact]
        public void ActionSequence_ShouldHandleActionsWithZeroDelay()
        {
            // Arrange
            var action1 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 0, "Test");
            var action2 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.B, 0, "Test");
            var actions = new[] { action1, action2 };

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.TotalDuration.Should().Be(0);
            sequence.Actions.Should().HaveCount(2);
        }

        [Fact]
        public void ActionSequence_ShouldHandleActionsWithLargeDelay()
        {
            // Arrange
            var action1 = new GameAction(Guid.NewGuid(), ActionType.Delay, 60000, "Long delay"); // 1分钟
            var action2 = new GameAction(Guid.NewGuid(), ActionType.Delay, 120000, "Very long delay"); // 2分钟
            var actions = new[] { action1, action2 };

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.TotalDuration.Should().Be(180000); // 3分钟
            sequence.Actions.Should().HaveCount(2);
        }

        [Fact]
        public void ActionSequence_ShouldHandleMixedActionTypes()
        {
            // Arrange
            var actions = new[]
            {
                new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test"),
                new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 100, 200, 50, "Test"),
                new GameAction(Guid.NewGuid(), ActionType.Delay, 200, "Test"),
                new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Right, 300, 400, 0, "Test"),
                new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 150, "Test")
            };

            // Act
            var sequence = new ActionSequence(actions);

            // Assert
            sequence.Actions.Should().HaveCount(5);
            sequence.TotalDuration.Should().Be(100 + 50 + 200 + 0 + 150);
            sequence.Actions.Should().Contain(a => a.IsKeyboardAction);
            sequence.Actions.Should().Contain(a => a.IsMouseAction);
            sequence.Actions.Should().Contain(a => a.IsDelayAction);
        }

        #endregion

        #region 不变性测试

        [Fact]
        public void ActionSequence_ShouldBeImmutable()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var sequence = new ActionSequence(actions);

            // Act & Assert
            // 验证所有属性都是只读的
            var properties = typeof(ActionSequence).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name != "EqualityComparer") // 排除比较器属性
                {
                    property.CanWrite.Should().BeFalse($"Property {property.Name} should be read-only");
                }
            }
        }

        [Fact]
        public void ActionSequence_ShouldMaintainValueSemantics()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var originalSequence = new ActionSequence(actions);
            var equalSequence = new ActionSequence(actions);

            // Act & Assert
            // 它们应该相等，但不是同一个引用
            originalSequence.Should().Be(equalSequence);
            originalSequence.Should().NotBeSameAs(equalSequence);
            
            // 验证值对象语义：相等的对象应该产生相同的哈希码
            originalSequence.GetHashCode().Should().Be(equalSequence.GetHashCode());
        }

        [Fact]
        public void ActionSequence_MethodsShouldNotModifyOriginal()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(2);
            var originalSequence = new ActionSequence(actions);
            var newAction = TestDataFactory.CreateGameAction();

            // Act
            var modifiedSequence = originalSequence.AddAction(newAction);

            // Assert
            // 原始序列应该保持不变
            originalSequence.Actions.Should().HaveCount(2);
            originalSequence.ActionCount.Should().Be(2);
            originalSequence.TotalDuration.Should().Be(actions.Sum(a => a.Delay));
            originalSequence.Actions.Should().BeEquivalentTo(actions);
            
            // 修改的序列应该包含新动作
            modifiedSequence.Actions.Should().HaveCount(3);
            modifiedSequence.Actions.Should().Contain(newAction);
        }

        #endregion

        #region 性能测试

        [Fact]
        public void ActionSequence_Constructor_ShouldBeEfficient()
        {
            // Arrange
            var actionCount = 1000;
            var actions = TestDataFactory.CreateGameActions(actionCount);

            // Act
            var startTime = DateTime.UtcNow;
            var sequence = new ActionSequence(actions);
            var endTime = DateTime.UtcNow;

            // Assert
            (endTime - startTime).Should().BeLessThan(TimeSpan.FromMilliseconds(100));
            sequence.Actions.Should().HaveCount(actionCount);
        }

        [Fact]
        public void ActionSequence_Equals_ShouldBeEfficient()
        {
            // Arrange
            var actions = TestDataFactory.CreateGameActions(1000);
            var sequence1 = new ActionSequence(actions);
            var sequence2 = new ActionSequence(actions);

            // Act
            var startTime = DateTime.UtcNow;
            var areEqual = sequence1.Equals(sequence2);
            var endTime = DateTime.UtcNow;

            // Assert
            (endTime - startTime).Should().BeLessThan(TimeSpan.FromMilliseconds(50));
            areEqual.Should().BeTrue();
        }

        #endregion
    }
}