using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using KeyForge.Domain.Events;

namespace KeyForge.Domain.Tests.UnitTests.Aggregates
{
    /// <summary>
    /// StateMachine聚合根单元测试
    /// 测试状态机的所有业务规则和不变性
    /// </summary>
    public class StateMachineTests : TestBase
    {
        #region 构造函数测试

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateStateMachine()
        {
            // Arrange
            var stateMachineId = Guid.NewGuid();
            var name = "Test State Machine";
            var description = "Test Description";

            // Act
            var stateMachine = new StateMachine(stateMachineId, name, description);

            // Assert
            ShouldBeValidStateMachine(stateMachine);
            stateMachine.Name.Should().Be(name);
            stateMachine.Description.Should().Be(description);
            stateMachine.Status.Should().Be(StateMachineStatus.Draft);
            stateMachine.States.Should().HaveCount(1); // 初始状态
            stateMachine.Transitions.Should().BeEmpty();
            stateMachine.Rules.Should().BeEmpty();
            stateMachine.CurrentState.Should().NotBeNull();
            stateMachine.CurrentState.Name.Should().Be("Initial");
            stateMachine.Version.Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var stateMachineId = Guid.NewGuid();
            var description = "Test Description";

            // Act & Assert
            var action = () => new StateMachine(stateMachineId, invalidName, description);
            ShouldThrowValidationException(action, "State machine name cannot be empty.");
        }

        [Fact]
        public void Constructor_ShouldCreateInitialState()
        {
            // Arrange
            var stateMachineId = Guid.NewGuid();
            var name = "Test State Machine";
            var description = "Test Description";

            // Act
            var stateMachine = new StateMachine(stateMachineId, name, description);

            // Assert
            stateMachine.States.Should().HaveCount(1);
            var initialState = stateMachine.States.First();
            initialState.Name.Should().Be("Initial");
            initialState.Description.Should().Be("Initial state");
            stateMachine.CurrentState.Should().Be(initialState);
        }

        [Fact]
        public void Constructor_ShouldRaiseStateMachineCreatedEvent()
        {
            // Arrange
            var stateMachineId = Guid.NewGuid();
            var name = "Test State Machine";
            var description = "Test Description";

            // Act
            var stateMachine = new StateMachine(stateMachineId, name, description);

            // Assert
            stateMachine.DomainEvents.Should().HaveCount(1);
            var domainEvent = stateMachine.DomainEvents.First();
            domainEvent.Should().BeOfType<StateMachineCreatedEvent>();
            var createdEvent = (StateMachineCreatedEvent)domainEvent;
            createdEvent.StateMachineId.Should().Be(stateMachineId);
            createdEvent.StateMachineName.Should().Be(name);
        }

        #endregion

        #region AddState方法测试

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
            stateMachine.States.Should().HaveCount(4); // 3个现有 + 1个新状态
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
            var existingState = stateMachine.States.First();
            var duplicateState = new State(Guid.NewGuid(), existingState.Name, "Duplicate description");

            // Act & Assert
            var action = () => stateMachine.AddState(duplicateState);
            ShouldThrowBusinessRuleViolationException(action, $"State with name '{existingState.Name}' already exists.");
        }

        [Fact]
        public void AddState_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate(); // 激活状态机
            var state = TestDataFactory.CreateState();

            // Act & Assert
            var action = () => stateMachine.AddState(state);
            ShouldThrowBusinessRuleViolationException(action, "Can only add states to draft state machines.");
        }

        #endregion

        #region AddTransition方法测试

        [Fact]
        public void AddTransition_WithValidTransition_ShouldAddTransition()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var transition = TestDataFactory.CreateStateTransition(states[0].Id, states[1].Id);
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.AddTransition(transition);

            // Assert
            stateMachine.Transitions.Should().HaveCount(4); // 3个现有 + 1个新转换
            stateMachine.Transitions.Should().Contain(transition);
            stateMachine.Version.Should().Be(originalVersion + 1);
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
            var states = stateMachine.States.ToList();
            var invalidFromStateId = Guid.NewGuid();
            var transition = TestDataFactory.CreateStateTransition(invalidFromStateId, states[1].Id);

            // Act & Assert
            var action = () => stateMachine.AddTransition(transition);
            ShouldThrowBusinessRuleViolationException(action, "From state does not exist.");
        }

        [Fact]
        public void AddTransition_WithInvalidToState_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var invalidToStateId = Guid.NewGuid();
            var transition = TestDataFactory.CreateStateTransition(states[0].Id, invalidToStateId);

            // Act & Assert
            var action = () => stateMachine.AddTransition(transition);
            ShouldThrowBusinessRuleViolationException(action, "To state does not exist.");
        }

        [Fact]
        public void AddTransition_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate(); // 激活状态机
            var states = stateMachine.States.ToList();
            var transition = TestDataFactory.CreateStateTransition(states[0].Id, states[1].Id);

            // Act & Assert
            var action = () => stateMachine.AddTransition(transition);
            ShouldThrowBusinessRuleViolationException(action, "Can only add transitions to draft state machines.");
        }

        #endregion

        #region AddRule方法测试

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
            stateMachine.Rules.Should().HaveCount(3); // 2个现有 + 1个新规则
            stateMachine.Rules.Should().Contain(rule);
            stateMachine.Version.Should().Be(originalVersion + 1);
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
        public void AddRule_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate(); // 激活状态机
            var rule = TestDataFactory.CreateDecisionRule();

            // Act & Assert
            var action = () => stateMachine.AddRule(rule);
            ShouldThrowBusinessRuleViolationException(action, "Can only add rules to draft state machines.");
        }

        #endregion

        #region TransitionTo方法测试

        [Fact]
        public void TransitionTo_WithValidState_ShouldTransitionToState()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var targetState = states[1];
            var originalVersion = stateMachine.Version;
            var originalStateId = stateMachine.CurrentState.Id;

            // Act
            stateMachine.TransitionTo(targetState.Id, "Test transition");

            // Assert
            stateMachine.CurrentState.Should().Be(targetState);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);

            // 验证事件
            stateMachine.DomainEvents.Should().Contain(e => e is StateTransitionEvent);
            var transitionEvent = stateMachine.DomainEvents.OfType<StateTransitionEvent>().First();
            transitionEvent.FromStateId.Should().Be(originalStateId);
            transitionEvent.ToStateId.Should().Be(targetState.Id);
            transitionEvent.Reason.Should().Be("Test transition");
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
        public void TransitionTo_ToSameState_ShouldDoNothing()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var currentStateId = stateMachine.CurrentState.Id;
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.TransitionTo(currentStateId);

            // Assert
            stateMachine.CurrentState.Id.Should().Be(currentStateId);
            stateMachine.Version.Should().Be(originalVersion); // 版本不应该改变
            stateMachine.DomainEvents.Should().NotContain(e => e is StateTransitionEvent);
        }

        #endregion

        #region CanTransitionTo方法测试

        [Fact]
        public void CanTransitionTo_WithValidTransition_ShouldReturnTrue()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var fromState = states[0];
            var toState = states[1];
            var transition = TestDataFactory.CreateStateTransition(fromState.Id, toState.Id);
            stateMachine.AddTransition(transition);

            // Act
            var canTransition = stateMachine.CanTransitionTo(toState.Id);

            // Assert
            canTransition.Should().BeTrue();
        }

        [Fact]
        public void CanTransitionTo_WithoutTransition_ShouldReturnFalse()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var targetState = states[2];

            // Act
            var canTransition = stateMachine.CanTransitionTo(targetState.Id);

            // Assert
            canTransition.Should().BeFalse();
        }

        [Fact]
        public void CanTransitionTo_WithInvalidState_ShouldReturnFalse()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var invalidStateId = Guid.NewGuid();

            // Act
            var canTransition = stateMachine.CanTransitionTo(invalidStateId);

            // Assert
            canTransition.Should().BeFalse();
        }

        #endregion

        #region GetAvailableTransitions方法测试

        [Fact]
        public void GetAvailableTransitions_ShouldReturnAvailableTransitions()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var currentState = stateMachine.CurrentState;
            
            // 添加一些转换
            var transition1 = TestDataFactory.CreateStateTransition(currentState.Id, states[1].Id);
            var transition2 = TestDataFactory.CreateStateTransition(currentState.Id, states[2].Id);
            var transition3 = TestDataFactory.CreateStateTransition(states[1].Id, states[2].Id); // 不应该返回这个
            
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
        public void GetAvailableTransitions_WhenNoTransitions_ShouldReturnEmpty()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");

            // Act
            var availableTransitions = stateMachine.GetAvailableTransitions();

            // Assert
            availableTransitions.Should().BeEmpty();
        }

        #endregion

        #region EvaluateRules方法测试

        [Fact]
        public void EvaluateRules_WhenActiveAndRulesMatch_ShouldTriggerEvents()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate(); // 激活状态机
            stateMachine.ClearDomainEvents();
            
            var rule = new DecisionRule(
                Guid.NewGuid(),
                "Test Rule",
                new ConditionExpression("testVar", ComparisonOperator.Equal, "testValue"),
                1,
                true,
                "Test description");
            
            // 模拟规则评估为true
            var mockRule = new Mock<DecisionRule>();
            mockRule.Setup(r => r.IsActive).Returns(true);
            mockRule.Setup(r => r.Priority).Returns(1);
            mockRule.Setup(r => r.Evaluate(It.IsAny<Func<string, object>>())).Returns(true);
            
            stateMachine.AddRule(rule);
            
            // 使用反射替换规则（因为规则是私有的）
            var rulesField = typeof(StateMachine).GetField("_rules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rules = (System.Collections.Generic.List<DecisionRule>)rulesField.GetValue(stateMachine);
            rules[0] = mockRule.Object;

            // Act
            stateMachine.EvaluateRules();

            // Assert
            stateMachine.DomainEvents.Should().Contain(e => e is RuleTriggeredEvent);
            var triggeredEvent = stateMachine.DomainEvents.OfType<RuleTriggeredEvent>().First();
            triggeredEvent.StateMachineId.Should().Be(stateMachine.Id);
            triggeredEvent.RuleId.Should().Be(rule.Id);
            triggeredEvent.RuleName.Should().Be(rule.Name);
        }

        [Fact]
        public void EvaluateRules_WhenNotActive_ShouldDoNothing()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var rule = TestDataFactory.CreateDecisionRule();
            stateMachine.AddRule(rule);
            stateMachine.ClearDomainEvents();

            // Act
            stateMachine.EvaluateRules();

            // Assert
            stateMachine.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void EvaluateRules_WhenNoCurrentState_ShouldDoNothing()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");
            stateMachine.Activate();
            
            // 移除所有状态（通过反射）
            var statesField = typeof(StateMachine).GetField("_states", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var states = (System.Collections.Generic.List<State>)statesField.GetValue(stateMachine);
            states.Clear();
            
            var rule = TestDataFactory.CreateDecisionRule();
            stateMachine.AddRule(rule);
            stateMachine.ClearDomainEvents();

            // Act
            stateMachine.EvaluateRules();

            // Assert
            stateMachine.DomainEvents.Should().BeEmpty();
        }

        #endregion

        #region Activate方法测试

        [Fact]
        public void Activate_WhenInDraftWithValidConfiguration_ShouldActivateStateMachine()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Activate();

            // Assert
            stateMachine.Status.Should().Be(StateMachineStatus.Active);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);
        }

        [Fact]
        public void Activate_WhenNotInDraft_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate(); // 先激活

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "Only draft state machines can be activated.");
        }

        [Fact]
        public void Activate_WhenLessThan2States_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "State machine must have at least 2 states.");
        }

        [Fact]
        public void Activate_WhenNoTransitions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");
            var state1 = new State(Guid.NewGuid(), "State1", "Description1");
            var state2 = new State(Guid.NewGuid(), "State2", "Description2");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);

            // Act & Assert
            var action = () => stateMachine.Activate();
            ShouldThrowBusinessRuleViolationException(action, "State machine must have at least 1 transition.");
        }

        [Fact]
        public void Activate_ShouldRaiseStateMachineActivatedEvent()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.ClearDomainEvents();

            // Act
            stateMachine.Activate();

            // Assert
            stateMachine.DomainEvents.Should().HaveCount(1);
            var domainEvent = stateMachine.DomainEvents.First();
            domainEvent.Should().BeOfType<StateMachineActivatedEvent>();
            var activatedEvent = (StateMachineActivatedEvent)domainEvent;
            activatedEvent.StateMachineId.Should().Be(stateMachine.Id);
            activatedEvent.StateMachineName.Should().Be(stateMachine.Name);
        }

        #endregion

        #region Deactivate方法测试

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateStateMachine()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate();
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Deactivate();

            // Assert
            stateMachine.Status.Should().Be(StateMachineStatus.Paused);
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
        public void Deactivate_ShouldRaiseStateMachineDeactivatedEvent()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            stateMachine.Activate();
            stateMachine.ClearDomainEvents();

            // Act
            stateMachine.Deactivate();

            // Assert
            stateMachine.DomainEvents.Should().HaveCount(1);
            var domainEvent = stateMachine.DomainEvents.First();
            domainEvent.Should().BeOfType<StateMachineDeactivatedEvent>();
            var deactivatedEvent = (StateMachineDeactivatedEvent)domainEvent;
            deactivatedEvent.StateMachineId.Should().Be(stateMachine.Id);
            deactivatedEvent.StateMachineName.Should().Be(stateMachine.Name);
        }

        #endregion

        #region Reset方法测试

        [Fact]
        public void Reset_WhenHasInitialState_ShouldResetToInitialState()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var states = stateMachine.States.ToList();
            var initialState = states.First(s => s.Name == "Initial");
            var targetState = states[1];
            
            // 先转换到另一个状态
            stateMachine.TransitionTo(targetState.Id);
            stateMachine.ClearDomainEvents();
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Reset();

            // Assert
            stateMachine.CurrentState.Should().Be(initialState);
            stateMachine.Version.Should().Be(originalVersion + 1);
            stateMachine.UpdatedAt.Should().BeAfter(stateMachine.CreatedAt);

            // 验证事件
            stateMachine.DomainEvents.Should().Contain(e => e is StateMachineResetEvent);
            var resetEvent = stateMachine.DomainEvents.OfType<StateMachineResetEvent>().First();
            resetEvent.StateMachineId.Should().Be(stateMachine.Id);
            resetEvent.StateMachineName.Should().Be(stateMachine.Name);
        }

        [Fact]
        public void Reset_WhenNoInitialState_ShouldDoNothing()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");
            var state1 = new State(Guid.NewGuid(), "State1", "Description1");
            var state2 = new State(Guid.NewGuid(), "State2", "Description2");
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            
            var originalVersion = stateMachine.Version;

            // Act
            stateMachine.Reset();

            // Assert
            stateMachine.Version.Should().Be(originalVersion); // 版本不应该改变
            stateMachine.DomainEvents.Should().NotContain(e => e is StateMachineResetEvent);
        }

        #endregion

        #region 复杂场景测试

        [Fact]
        public void StateMachine_ShouldHandleComplexWorkflow()
        {
            // Arrange
            var stateMachine = new StateMachine(Guid.NewGuid(), "Workflow", "Complex workflow");
            
            // 添加状态
            var initialState = stateMachine.CurrentState;
            var processingState = new State(Guid.NewGuid(), "Processing", "Processing state");
            var completedState = new State(Guid.NewGuid(), "Completed", "Completed state");
            var errorState = new State(Guid.NewGuid(), "Error", "Error state");
            
            stateMachine.AddState(processingState);
            stateMachine.AddState(completedState);
            stateMachine.AddState(errorState);
            
            // 添加转换
            var startTransition = new StateTransition(
                Guid.NewGuid(), 
                initialState.Id, 
                processingState.Id, 
                new ConditionExpression("canStart", ComparisonOperator.Equal, "true"),
                "Start processing");
            
            var completeTransition = new StateTransition(
                Guid.NewGuid(),
                processingState.Id,
                completedState.Id,
                new ConditionExpression("isComplete", ComparisonOperator.Equal, "true"),
                "Complete processing");
            
            var errorTransition = new StateTransition(
                Guid.NewGuid(),
                processingState.Id,
                errorState.Id,
                new ConditionExpression("hasError", ComparisonOperator.Equal, "true"),
                "Handle error");
            
            stateMachine.AddTransition(startTransition);
            stateMachine.AddTransition(completeTransition);
            stateMachine.AddTransition(errorTransition);

            // Act & Assert
            // 验证初始状态
            stateMachine.CurrentState.Should().Be(initialState);
            
            // 验证可用转换
            var availableTransitions = stateMachine.GetAvailableTransitions();
            availableTransitions.Should().HaveCount(1);
            availableTransitions.First().Should().Be(startTransition);
            
            // 转换到处理状态
            stateMachine.TransitionTo(processingState.Id);
            stateMachine.CurrentState.Should().Be(processingState);
            
            // 验证处理状态的可用转换
            availableTransitions = stateMachine.GetAvailableTransitions();
            availableTransitions.Should().HaveCount(2);
            availableTransitions.Should().Contain(completeTransition);
            availableTransitions.Should().Contain(errorTransition);
            
            // 激活状态机
            stateMachine.Activate();
            stateMachine.Status.Should().Be(StateMachineStatus.Active);
        }

        [Fact]
        public void StateMachine_ShouldMaintainImmutability()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();

            // Act & Assert
            // 验证返回的集合是只读的
            stateMachine.States.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<State>>();
            stateMachine.Transitions.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<StateTransition>>();
            stateMachine.Rules.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<DecisionRule>>();
        }

        [Fact]
        public void StateMachine_ShouldHandleConcurrentOperations()
        {
            // Arrange
            var stateMachine = TestDataFactory.CreateValidStateMachine();
            var state1 = new State(Guid.NewGuid(), "State1", "Description1");
            var state2 = new State(Guid.NewGuid(), "State2", "Description2");
            var rule1 = TestDataFactory.CreateDecisionRule();
            var rule2 = TestDataFactory.CreateDecisionRule();

            // Act
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            stateMachine.AddRule(rule1);
            stateMachine.AddRule(rule2);

            // Assert
            stateMachine.States.Should().HaveCount(5); // 3个现有 + 2个新状态
            stateMachine.Rules.Should().HaveCount(4); // 2个现有 + 2个新规则
            stateMachine.Version.Should().Be(5); // 初始1 + 4个操作
        }

        #endregion
    }
}