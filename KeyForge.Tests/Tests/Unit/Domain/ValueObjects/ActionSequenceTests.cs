using Xunit;
using FluentAssertions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Entities;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// 动作序列值对象单元测试
/// 原本实现：复杂的序列操作测试
/// 简化实现：核心功能测试
/// </summary>
public class ActionSequenceTests : TestBase
{
    public ActionSequenceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateActionSequence_WithEmptyActions_ShouldCreateSequence()
    {
        // Arrange
        var actions = new List<GameAction>();
        
        // Act
        var sequence = new ActionSequence(actions);
        
        // Assert
        sequence.Should().NotBeNull();
        sequence.Actions.Should().BeEmpty();
        sequence.Count.Should().Be(0);
        sequence.TotalDuration.Should().Be(TimeSpan.Zero);
        Log($"创建空动作序列成功");
    }

    [Fact]
    public void CreateActionSequence_WithValidActions_ShouldCreateSequence()
    {
        // Arrange
        var actions = CreateTestActions();
        
        // Act
        var sequence = new ActionSequence(actions);
        
        // Assert
        sequence.Should().NotBeNull();
        sequence.Actions.Should().HaveCount(3);
        sequence.Count.Should().Be(3);
        sequence.TotalDuration.Should().Be(TimeSpan.FromMilliseconds(450));
        Log($"创建动作序列成功: {sequence.Count} 个动作, 总时长 {sequence.TotalDuration.TotalMilliseconds}ms");
    }

    [Fact]
    public void CreateActionSequence_WithNullActions_ShouldThrowException()
    {
        // Arrange
        List<GameAction> actions = null;
        
        // Act & Assert
        var action = () => new ActionSequence(actions);
        action.Should().Throw<ArgumentNullException>();
        Log($"验证空动作列表创建序列失败");
    }

    [Fact]
    public void GetActionAt_WithValidIndex_ShouldReturnAction()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var action = sequence.GetActionAt(1);
        
        // Assert
        action.Should().NotBeNull();
        action.Should().Be(actions[1]);
        Log($"获取指定索引动作: {action.Type}");
    }

    [Fact]
    public void GetActionAt_WithInvalidIndex_ShouldThrowException()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act & Assert
        var action = () => sequence.GetActionAt(5);
        action.Should().Throw<ArgumentOutOfRangeException>();
        Log($"验证无效索引获取动作失败");
    }

    [Fact]
    public void GetActionsByType_WithValidType_ShouldReturnFilteredActions()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var keyboardActions = sequence.GetActionsByType(ActionType.KeyDown);
        
        // Assert
        keyboardActions.Should().HaveCount(2);
        keyboardActions.All(a => a.Type == ActionType.KeyDown).Should().BeTrue();
        Log($"按类型过滤动作: {keyboardActions.Count} 个键盘动作");
    }

    [Fact]
    public void GetActionsByType_WithNoMatchingActions_ShouldReturnEmptyList()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var mouseActions = sequence.GetActionsByType(ActionType.MouseMove);
        
        // Assert
        mouseActions.Should().BeEmpty();
        Log($"按类型过滤动作: 0 个鼠标动作");
    }

    [Fact]
    public void GetDurationRange_WithValidRange_ShouldReturnSubSequence()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        var startIndex = 1;
        var endIndex = 2;
        
        // Act
        var subSequence = sequence.GetDurationRange(startIndex, endIndex);
        
        // Assert
        subSequence.Should().NotBeNull();
        subSequence.Count.Should().Be(2);
        subSequence.Actions[0].Should().Be(actions[1]);
        subSequence.Actions[1].Should().Be(actions[2]);
        Log($"获取时间范围动作: {subSequence.Count} 个动作");
    }

    [Fact]
    public void GetDurationRange_WithInvalidRange_ShouldThrowException()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act & Assert
        var action = () => sequence.GetDurationRange(2, 1);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效时间范围失败");
    }

    [Fact]
    public void GetKeyboardActions_ShouldReturnOnlyKeyboardActions()
    {
        // Arrange
        var actions = CreateMixedActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var keyboardActions = sequence.GetKeyboardActions();
        
        // Assert
        keyboardActions.Should().HaveCount(2);
        keyboardActions.All(a => a.IsKeyboardAction).Should().BeTrue();
        Log($"获取键盘动作: {keyboardActions.Count} 个");
    }

    [Fact]
    public void GetMouseActions_ShouldReturnOnlyMouseActions()
    {
        // Arrange
        var actions = CreateMixedActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var mouseActions = sequence.GetMouseActions();
        
        // Assert
        mouseActions.Should().HaveCount(1);
        mouseActions.All(a => a.IsMouseAction).Should().BeTrue();
        Log($"获取鼠标动作: {mouseActions.Count} 个");
    }

    [Fact]
    public void GetDelayActions_ShouldReturnOnlyDelayActions()
    {
        // Arrange
        var actions = CreateMixedActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var delayActions = sequence.GetDelayActions();
        
        // Assert
        delayActions.Should().HaveCount(1);
        delayActions.All(a => a.IsDelayAction).Should().BeTrue();
        Log($"获取延迟动作: {delayActions.Count} 个");
    }

    [Fact]
    public void HasActionsOfType_WithExistingType_ShouldReturnTrue()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var hasKeyboardActions = sequence.HasActionsOfType(ActionType.KeyDown);
        
        // Assert
        hasKeyboardActions.Should().BeTrue();
        Log($"检查存在键盘动作: {hasKeyboardActions}");
    }

    [Fact]
    public void HasActionsOfType_WithNonExistingType_ShouldReturnFalse()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var hasMouseActions = sequence.HasActionsOfType(ActionType.MouseMove);
        
        // Assert
        hasMouseActions.Should().BeFalse();
        Log($"检查存在鼠标动作: {hasMouseActions}");
    }

    [Fact]
    public void GetActionStatistics_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var actions = CreateMixedActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var stats = sequence.GetActionStatistics();
        
        // Assert
        stats.TotalActions.Should().Be(4);
        stats.KeyboardActions.Should().Be(2);
        stats.MouseActions.Should().Be(1);
        stats.DelayActions.Should().Be(1);
        stats.AverageDelay.Should().Be(137.5); // (100 + 150 + 200 + 100) / 4
        Log($"获取动作统计: 总数={stats.TotalActions}, 键盘={stats.KeyboardActions}, 鼠标={stats.MouseActions}, 延迟={stats.DelayActions}");
    }

    [Fact]
    public void GetEnumerator_ShouldAllowEnumeration()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act
        var enumeratedActions = new List<GameAction>();
        foreach (var action in sequence)
        {
            enumeratedActions.Add(action);
        }
        
        // Assert
        enumeratedActions.Should().HaveCount(3);
        enumeratedActions.Should().BeEquivalentTo(actions);
        Log($"枚举动作序列: {enumeratedActions.Count} 个动作");
    }

    [Fact]
    public void Indexer_ShouldAllowAccessByIndex()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act & Assert
        sequence[0].Should().Be(actions[0]);
        sequence[1].Should().Be(actions[1]);
        sequence[2].Should().Be(actions[2]);
        Log($"索引访问动作: {sequence[0].Type}, {sequence[1].Type}, {sequence[2].Type}");
    }

    [Fact]
    public void Indexer_WithInvalidIndex_ShouldThrowException()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence = new ActionSequence(actions);
        
        // Act & Assert
        var action = () => { var _ = sequence[5]; };
        action.Should().Throw<ArgumentOutOfRangeException>();
        Log($"验证无效索引访问失败");
    }

    [Fact]
    public void ActionSequence_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var actions = CreateTestActions();
        var sequence1 = new ActionSequence(actions);
        var sequence2 = new ActionSequence(actions);
        var differentActions = CreateTestActions().ToList();
        differentActions.Add(new GameAction(Guid.NewGuid(), ActionType.Delay, 100));
        var sequence3 = new ActionSequence(differentActions);
        
        // Act & Assert
        sequence1.Should().Be(sequence2); // Same actions
        sequence1.Should().NotBe(sequence3); // Different actions
        (sequence1 == sequence2).Should().BeTrue();
        (sequence1 != sequence3).Should().BeTrue();
        Log($"验证动作序列相等性: 相同动作={sequence1.Count == sequence2.Count}, 不同动作={sequence1.Count != sequence3.Count}");
    }

    private List<GameAction> CreateTestActions()
    {
        return new List<GameAction>
        {
            new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100),
            new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.B, 150),
            new GameAction(Guid.NewGuid(), ActionType.Delay, 200)
        };
    }

    private List<GameAction> CreateMixedActions()
    {
        return new List<GameAction>
        {
            new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100),
            new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 150),
            new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200, 200),
            new GameAction(Guid.NewGuid(), ActionType.Delay, 100)
        };
    }
}