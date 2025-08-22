using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain.Aggregates;

/// <summary>
/// 状态机聚合根单元测试
/// 原本实现：复杂的状态转换测试
/// 简化实现：核心功能测试
/// </summary>
public class StateMachineTests : TestBase
{
    public StateMachineTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateStateMachine_WithValidData_ShouldCreateStateMachine()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var machineName = "Test State Machine";
        var initialState = "Idle";
        
        // Act
        var stateMachine = new StateMachine(machineId, machineName, initialState);
        
        // Assert
        stateMachine.Should().NotBeNull();
        stateMachine.Id.Should().Be(machineId);
        stateMachine.Name.Should().Be(machineName);
        stateMachine.CurrentState.Should().Be(initialState);
        stateMachine.States.Should().ContainKey(initialState);
        stateMachine.IsActive.Should().BeTrue();
        stateMachine.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"创建状态机成功: {stateMachine.Id}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStateMachine_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var initialState = "Idle";
        
        // Act & Assert
        var action = () => new StateMachine(machineId, invalidName, initialState);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效状态机名称: {invalidName}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStateMachine_WithInvalidInitialState_ShouldThrowException(string invalidState)
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var machineName = "Test State Machine";
        
        // Act & Assert
        var action = () => new StateMachine(machineId, machineName, invalidState);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效初始状态: {invalidState}");
    }

    [Fact]
    public void AddState_WithValidState_ShouldAddState()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        var newState = "Running";
        
        // Act
        stateMachine.AddState(newState);
        
        // Assert
        stateMachine.States.Should().ContainKey(newState);
        stateMachine.States[newState].Should().NotBeNull();
        stateMachine.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"添加状态成功: {newState}");
    }

    [Fact]
    public void AddState_WithDuplicateState_ShouldThrowException()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        var duplicateState = "Idle"; // 已存在的状态
        
        // Act & Assert
        var action = () => stateMachine.AddState(duplicateState);
        action.Should().Throw<InvalidOperationException>();
        Log($"验证重复状态添加失败: {duplicateState}");
    }

    [Fact]
    public void AddTransition_WithValidTransition_ShouldAddTransition()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        var fromState = "Idle";
        var toState = "Running";
        var condition = "start_command";
        
        // Act
        stateMachine.AddTransition(fromState, toState, condition);
        
        // Assert
        var transitions = stateMachine.GetTransitions(fromState);
        transitions.Should().ContainSingle();
        transitions.First().ToState.Should().Be(toState);
        transitions.First().Condition.Should().Be(condition);
        Log($"添加状态转换成功: {fromState} -> {toState} ({condition})");
    }

    [Fact]
    public void AddTransition_WithNonExistentFromState_ShouldThrowException()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        var fromState = "NonExistent";
        var toState = "Running";
        var condition = "start_command";
        
        // Act & Assert
        var action = () => stateMachine.AddTransition(fromState, toState, condition);
        action.Should().Throw<KeyNotFoundException>();
        Log($"验证不存在起始状态的转换失败: {fromState}");
    }

    [Fact]
    public void AddTransition_WithNonExistentToState_ShouldThrowException()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        var fromState = "Idle";
        var toState = "NonExistent";
        var condition = "start_command";
        
        // Act & Assert
        var action = () => stateMachine.AddTransition(fromState, toState, condition);
        action.Should().Throw<KeyNotFoundException>();
        Log($"验证不存在目标状态的转换失败: {toState}");
    }

    [Fact]
    public void TransitionTo_WithValidTransition_ShouldChangeState()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.AddState("Running");
        stateMachine.AddTransition("Idle", "Running", "start_command");
        
        // Act
        var result = stateMachine.TransitionTo("Running", "start_command");
        
        // Assert
        result.Should().BeTrue();
        stateMachine.CurrentState.Should().Be("Running");
        stateMachine.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"状态转换成功: Idle -> Running");
    }

    [Fact]
    public void TransitionTo_WithInvalidTransition_ShouldReturnFalse()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        
        // Act
        var result = stateMachine.TransitionTo("Running", "invalid_command");
        
        // Assert
        result.Should().BeFalse();
        stateMachine.CurrentState.Should().Be("Idle");
        Log($"无效状态转换失败: Idle -> Running (invalid_command)");
    }

    [Fact]
    public void TransitionTo_WithNonExistentState_ShouldReturnFalse()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        
        // Act
        var result = stateMachine.TransitionTo("NonExistent", "any_command");
        
        // Assert
        result.Should().BeFalse();
        stateMachine.CurrentState.Should().Be("Idle");
        Log($"不存在状态转换失败: Idle -> NonExistent");
    }

    [Fact]
    public void CanTransitionTo_WithValidTransition_ShouldReturnTrue()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.AddState("Running");
        stateMachine.AddTransition("Idle", "Running", "start_command");
        
        // Act
        var result = stateMachine.CanTransitionTo("Running", "start_command");
        
        // Assert
        result.Should().BeTrue();
        Log($"验证可转换状态: Idle -> Running (start_command) = {result}");
    }

    [Fact]
    public void CanTransitionTo_WithInvalidTransition_ShouldReturnFalse()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        
        // Act
        var result = stateMachine.CanTransitionTo("Running", "invalid_command");
        
        // Assert
        result.Should().BeFalse();
        Log($"验证不可转换状态: Idle -> Running (invalid_command) = {result}");
    }

    [Fact]
    public void GetPossibleTransitions_WithCurrentState_ShouldReturnTransitions()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.AddState("Running");
        stateMachine.AddState("Stopped");
        stateMachine.AddTransition("Idle", "Running", "start_command");
        stateMachine.AddTransition("Idle", "Stopped", "stop_command");
        
        // Act
        var transitions = stateMachine.GetPossibleTransitions();
        
        // Assert
        transitions.Should().HaveCount(2);
        transitions.Should().Contain(t => t.ToState == "Running" && t.Condition == "start_command");
        transitions.Should().Contain(t => t.ToState == "Stopped" && t.Condition == "stop_command");
        Log($"获取可能转换: {transitions.Count} 个转换");
    }

    [Fact]
    public void GetStateHistory_ShouldReturnHistory()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.AddState("Running");
        stateMachine.AddTransition("Idle", "Running", "start_command");
        
        // Act
        stateMachine.TransitionTo("Running", "start_command");
        var history = stateMachine.GetStateHistory();
        
        // Assert
        history.Should().HaveCount(2);
        history[0].State.Should().Be("Idle");
        history[1].State.Should().Be("Running");
        Log($"获取状态历史: {history.Count} 个状态");
    }

    [Fact]
    public void Reset_ShouldReturnToInitialState()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.AddState("Running");
        stateMachine.AddTransition("Idle", "Running", "start_command");
        stateMachine.TransitionTo("Running", "start_command");
        
        // Act
        stateMachine.Reset();
        
        // Assert
        stateMachine.CurrentState.Should().Be("Idle");
        stateMachine.GetStateHistory().Should().HaveCount(1);
        stateMachine.GetStateHistory().Last().State.Should().Be("Idle");
        Log($"重置状态机成功: 返回到初始状态 Idle");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetActiveToTrue()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        stateMachine.Deactivate();
        
        // Act
        stateMachine.Activate();
        
        // Assert
        stateMachine.IsActive.Should().BeTrue();
        Log($"激活状态机成功: {stateMachine.Id}");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetActiveToFalse()
    {
        // Arrange
        var stateMachine = CreateValidStateMachine();
        
        // Act
        stateMachine.Deactivate();
        
        // Assert
        stateMachine.IsActive.Should().BeFalse();
        Log($"停用状态机成功: {stateMachine.Id}");
    }

    [Fact]
    public void StateMachine_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var machine1 = new StateMachine(machineId, "Machine 1", "Idle");
        var machine2 = new StateMachine(machineId, "Machine 2", "Running");
        var machine3 = new StateMachine(Guid.NewGuid(), "Machine 1", "Idle");
        
        // Act & Assert
        machine1.Should().Be(machine2); // Same ID
        machine1.Should().NotBe(machine3); // Different ID
        (machine1 == machine2).Should().BeTrue();
        (machine1 != machine3).Should().BeTrue();
        Log($"验证状态机相等性: 相同ID={machine1.Id == machine2.Id}, 不同ID={machine1.Id != machine3.Id}");
    }

    private StateMachine CreateValidStateMachine()
    {
        return new StateMachine(Guid.NewGuid(), "Test State Machine", "Idle");
    }
}