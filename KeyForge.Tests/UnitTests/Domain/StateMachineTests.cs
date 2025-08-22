using System;
using System.Linq;
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
    /// 状态机聚合根单元测试
    /// </summary>
    public class StateMachineTests : TestBase
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateStateMachine()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test State Machine";
            var description = "A test state machine";

            // Act
            var stateMachine = new StateMachine(id, name, description);

            // Assert
            ShouldBeValidStateMachine(stateMachine);
            stateMachine.Name.Should().Be(name);
            stateMachine.Description.Should().Be(description);
            stateMachine.Status.Should().Be(StateMachineStatus.Draft);
            ShouldBeInState(stateMachine, "Initial");
            stateMachine.States.Should().HaveCount(1);
            stateMachine.Transitions.Should().BeEmpty();
            stateMachine.Rules.Should().BeEmpty();
        }

        [Fact]
        public void AddState_WithValidState_ShouldAddState()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state = TestDataFactory.CreateState();
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.AddState(state);

            // Assert
            stateMachine.States.Should().Contain(state);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void AddState_WithNullState_ShouldThrowValidationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            var action = () => stateMachine.AddState(null);
            ShouldThrowValidationException(action, "State cannot be null.");
        }

        [Fact]
        public void AddState_WithDuplicateName_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state = new State(Guid.NewGuid(), "Initial", "Duplicate state");

            // Act & Assert
            var action = () => stateMachine.AddState(state);
            ShouldThrowBusinessRuleViolationException(action, "State with name 'Initial' already exists.");
        }

        [Fact]
        public void AddState_WhenStateMachineNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            // 添加足够的状态和转换以激活状态机
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = new StateTransition(Guid.NewGuid(), state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            stateMachine.Activate();

            var newState = new State(Guid.NewGuid(), "State3", "Third state");

            // Act & Assert
            var action = () => stateMachine.AddState(newState);
            ShouldThrowBusinessRuleViolationException(action, "Can only add states to draft state machines.");
        }

        [Fact]
        public void AddTransition_WithValidTransition_ShouldAddTransition()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.AddTransition(transition);

            // Assert
            stateMachine.Transitions.Should().Contain(transition);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void AddTransition_WithNullTransition_ShouldThrowValidationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            var action = () => stateMachine.AddTransition(null);
            ShouldThrowValidationException(action, "Transition cannot be null.");
        }

        [Fact]
        public void AddTransition_WithInvalidFromState_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state = new State(Guid.NewGuid(), "State1", "First state");
            stateMachine.AddState(state);
            
            var invalidFromStateId = Guid.NewGuid();
            var transition = TestDataFactory.CreateStateTransition(invalidFromStateId, state.Id);

            // Act & Assert
            var action = () => stateMachine.AddTransition(transition);
            ShouldThrowBusinessRuleViolationException(action, "From state does not exist.");
        }

        [Fact]
        public void AddTransition_WithInvalidToState_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state = new State(Guid.NewGuid(), "State1", "First state");
            stateMachine.AddState(state);
            
            var invalidToStateId = Guid.NewGuid();
            var transition = TestDataFactory.CreateStateTransition(state.Id, invalidToStateId);

            // Act & Assert
            var action = () => stateMachine.AddTransition(transition);
            ShouldThrowBusinessRuleViolationException(action, "To state does not exist.");
        }

        [Fact]
        public void AddRule_WithValidRule_ShouldAddRule()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var rule = TestDataFactory.CreateDecisionRule();
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.AddRule(rule);

            // Assert
            stateMachine.Rules.Should().Contain(rule);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void AddRule_WithNullRule_ShouldThrowValidationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            var action = () => stateMachine.AddRule(null);
            ShouldThrowValidationException(action, "Rule cannot be null.");
        }

        [Fact]
        public void TransitionTo_WithValidState_ShouldTransition()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.TransitionTo(state2.Id, "Test transition");

            // Assert
            ShouldBeInState(stateMachine, "State2");
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void TransitionTo_WithInvalidState_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var invalidStateId = Guid.NewGuid();

            // Act & Assert
            var action = () => stateMachine.TransitionTo(invalidStateId);
            ShouldThrowBusinessRuleViolationException(action, "Target state does not exist.");
        }

        [Fact]
        public void TransitionTo_WithSameState_ShouldDoNothing()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var currentStateId = stateMachine.CurrentState.Id;
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.TransitionTo(currentStateId);

            // Assert
            stateMachine.CurrentState.Id.Should().Be(currentStateId);
            stateMachine.Version.Should().Be(originalVersion);
        }

        [Fact]
        public void CanTransitionTo_WithValidTransition_ShouldReturnTrue()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);

            // Act
            var canTransition = stateMachine.CanTransitionTo(state2.Id);

            // Assert
            canTransition.Should().BeTrue();
        }

        [Fact]
        public void CanTransitionTo_WithInvalidTransition_ShouldReturnFalse()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);

            // Act
            var canTransition = stateMachine.CanTransitionTo(state2.Id);

            // Assert
            canTransition.Should().BeFalse();
        }

        [Fact]
        public void GetAvailableTransitions_ShouldReturnCorrectTransitions()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            var state3 = new State(Guid.NewGuid(), "State3", "Third state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            stateMachine.AddState(state3);
            
            // 添加从当前状态到state2和state3的转换
            var transition1 = TestDataFactory.CreateStateTransition(stateMachine.CurrentState.Id, state2.Id);
            var transition2 = TestDataFactory.CreateStateTransition(stateMachine.CurrentState.Id, state3.Id);
            var transition3 = TestDataFactory.CreateStateTransition(state1.Id, state2.Id); // 不可达的转换
            
            stateMachine.AddTransition(transition1);
            stateMachine.AddTransition(transition2);
            stateMachine.AddTransition(transition3);

            // Act
            var availableTransitions = stateMachine.GetAvailableTransitions();

            // Assert
            availableTransitions.Should().HaveCount(2);
            availableTransitions.Should().Contain(transition1);
            availableTransitions.Should().Contain(transition2);
            availableTransitions.Should().NotContain(transition3);
        }

        [Fact]
        public void EvaluateRules_WithActiveStateMachine_ShouldEvaluateRules()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var rule1 = new DecisionRule(Guid.NewGuid(), "Rule1", 
                new ConditionExpression("test", ComparisonOperator.Equals, "value"), 1, true, "Test rule 1");
            var rule2 = new DecisionRule(Guid.NewGuid(), "Rule2", 
                new ConditionExpression("test", ComparisonOperator.Equals, "value"), 2, false, "Test rule 2"); // 非活跃规则
            
            stateMachine.AddRule(rule1);
            stateMachine.AddRule(rule2);
            
            // 添加足够的状态和转换以激活状态机
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            stateMachine.Activate();

            // Act
            stateMachine.EvaluateRules();

            // Assert
            // 验证规则被评估（通过检查是否触发了相应的事件）
            var domainEvents = stateMachine.DomainEvents;
            domainEvents.Should().Contain(e => e.GetType().Name == "RuleTriggeredEvent");
        }

        [Fact]
        public void EvaluateRules_WithInactiveStateMachine_ShouldNotEvaluateRules()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var rule = new DecisionRule(Guid.NewGuid(), "Rule1", 
                new ConditionExpression("test", ComparisonOperator.Equals, "value"), 1, true, "Test rule 1");
            
            stateMachine.AddRule(rule);

            // Act
            stateMachine.EvaluateRules();

            // Assert
            // 状态机未激活，不应评估规则
            var domainEvents = stateMachine.DomainEvents;
            domainEvents.Should().NotContain(e => e.GetType().Name == "RuleTriggeredEvent");
        }

        [Fact]
        public void Activate_WithValidStateMachine_ShouldActivate()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Activate();

            // Assert
            ShouldBeInStatus(stateMachine, StateMachineStatus.Active);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void Activate_WithInsufficientStates_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "State machine must have at least 2 states.");
        }

        [Fact]
        public void Activate_WithNoTransitions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "State machine must have at least 1 transition.");
        }

        [Fact]
        public void Activate_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            stateMachine.Activate();

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Only draft state machines can be activated.");
        }

        [Fact]
        public void Deactivate_WithActiveStateMachine_ShouldDeactivate()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(state1.Id, state2.Id);
            stateMachine.AddTransition(transition);
            stateMachine.Activate();
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Deactivate();

            // Assert
            ShouldBeInStatus(stateMachine, StateMachineStatus.Inactive);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void Deactivate_WhenNotActive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            var action = () => stateMachine.Deactivate();
            ShouldThrowBusinessRuleViolationException(action, "Only active state machines can be deactivated.");
        }

        [Fact]
        public void Reset_ShouldResetToInitialState()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "First state");
            var state2 = new State(Guid.NewGuid(), "State2", "Second state");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var transition = TestDataFactory.CreateStateTransition(stateMachine.CurrentState.Id, state1.Id);
            stateMachine.AddTransition(transition);
            
            // 转换到另一个状态
            stateMachine.TransitionTo(state1.Id);
            ShouldBeInState(stateMachine, "State1");
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Reset();

            // Assert
            ShouldBeInState(stateMachine, "Initial");
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }
    }
}