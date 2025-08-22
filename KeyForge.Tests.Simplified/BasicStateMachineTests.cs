using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using DomainValidationException = KeyForge.Domain.Exceptions.ValidationException;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 最简化的状态机聚合根测试
    /// 只测试最基本的构造函数和属性
    /// </summary>
    public class BasicStateMachineTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateStateMachine()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test State Machine";
            var description = "Test state machine description";

            // Act
            var stateMachine = new StateMachine(id, name, description);

            // Assert
            stateMachine.Should().NotBeNull();
            stateMachine.Id.Should().Be(id);
            stateMachine.Name.Should().Be(name);
            stateMachine.Description.Should().Be(description);
            stateMachine.States.Should().HaveCount(1); // 初始状态
            stateMachine.CurrentState.Should().NotBeNull();
            stateMachine.CurrentState.Name.Should().Be("Initial");
            stateMachine.Status.Should().Be(KeyForge.Domain.Aggregates.StateMachineStatus.Draft);
            stateMachine.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldNotThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "";
            var description = "Test state machine description";

            // Act & Assert
            // 注意：实际的StateMachine构造函数没有对空名称进行验证
            // 这与Script类不同，Script的Update方法有验证但构造函数没有
            var action = () => new StateMachine(id, name, description);
            action.Should().NotThrow();
        }

        [Fact]
        public void AddState_WithValidState_ShouldAddToStateMachine()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test State Machine", "Test description");
            var newState = new KeyForge.Domain.Entities.State(Guid.NewGuid(), "New State", "New state description");

            // Act
            stateMachine.AddState(newState);

            // Assert
            stateMachine.States.Should().Contain(newState);
            stateMachine.States.Should().HaveCount(2);
        }

        [Fact]
        public void Activate_WithValidStateMachine_ShouldSetStatusToActive()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test State Machine", "Test description");
            
            // 根据实际业务规则，需要至少2个状态和1个转换才能激活
            var secondState = new KeyForge.Domain.Entities.State(Guid.NewGuid(), "Second State", "Second state description");
            stateMachine.AddState(secondState);
            
            // 添加一个转换
            var transition = new KeyForge.Domain.Aggregates.StateTransition(
                Guid.NewGuid(), 
                stateMachine.CurrentState.Id, 
                secondState.Id, 
                new KeyForge.Domain.ValueObjects.ConditionExpression("1", KeyForge.Domain.ValueObjects.ComparisonOperator.Equal, "1"), 
                "Test transition"
            );
            stateMachine.AddTransition(transition);

            // Act
            stateMachine.Activate();

            // Assert
            stateMachine.Status.Should().Be(KeyForge.Domain.Aggregates.StateMachineStatus.Active);
        }

        [Fact]
        public void Activate_WithInsufficientStates_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test State Machine", "Test description");

            // Act & Assert
            // 根据实际业务规则，需要至少2个状态才能激活
            var action = () => stateMachine.Activate();
            action.Should().Throw<KeyForge.Domain.Exceptions.BusinessRuleViolationException>()
                .WithMessage("State machine must have at least 2 states.");
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldSetStatusToInactive()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test State Machine", "Test description");
            
            // 先激活状态机
            var secondState = new KeyForge.Domain.Entities.State(Guid.NewGuid(), "Second State", "Second state description");
            stateMachine.AddState(secondState);
            
            var transition = new KeyForge.Domain.Aggregates.StateTransition(
                Guid.NewGuid(), 
                stateMachine.CurrentState.Id, 
                secondState.Id, 
                new KeyForge.Domain.ValueObjects.ConditionExpression("1", KeyForge.Domain.ValueObjects.ComparisonOperator.Equal, "1"), 
                "Test transition"
            );
            stateMachine.AddTransition(transition);
            
            stateMachine.Activate();

            // Act
            stateMachine.Deactivate();

            // Assert
            stateMachine.Status.Should().Be(KeyForge.Domain.Aggregates.StateMachineStatus.Inactive);
        }
    }
}