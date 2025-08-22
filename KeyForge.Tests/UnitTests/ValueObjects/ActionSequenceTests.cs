using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using KeyForge.Domain.ValueObjects;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.ValueObjects;

/// <summary>
/// ActionSequence 单元测试
/// 原本实现：复杂的动作序列测试
/// 简化实现：核心动作序列功能测试
/// </summary>
public class ActionSequenceTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitialize()
    {
        // Act
        var sequence = new ActionSequence();

        // Assert
        sequence.Should().NotBeNull();
        sequence.Id.Should().NotBeEmpty();
        sequence.Name.Should().BeNull();
        sequence.Description.Should().BeNull();
        sequence.Actions.Should().NotBeNull();
        sequence.Actions.Should().BeEmpty();
        sequence.RepeatCount.Should().Be(1);
        sequence.IsLooping.Should().BeFalse();
        sequence.DelayBetweenActions.Should().Be(0);
        sequence.IsEnabled.Should().BeTrue();
        sequence.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        sequence.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("ActionSequence 默认构造成功");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange
        var name = "Test Sequence";
        var description = "Test Description";
        var actions = new List<IAction>
        {
            new MockAction("Action1"),
            new MockAction("Action2")
        };

        // Act
        var sequence = new ActionSequence
        {
            Name = name,
            Description = description,
            Actions = actions,
            RepeatCount = 3,
            IsLooping = true,
            DelayBetweenActions = 100
        };

        // Assert
        sequence.Name.Should().Be(name);
        sequence.Description.Should().Be(description);
        sequence.Actions.Should().Equal(actions);
        sequence.RepeatCount.Should().Be(3);
        sequence.IsLooping.Should().BeTrue();
        sequence.DelayBetweenActions.Should().Be(100);
        Log("ActionSequence 参数构造成功");
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.Name = "Test Sequence";

        // Assert
        sequence.Name.Should().Be("Test Sequence");
        Log("序列名称设置成功");
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.Description = "Test Description";

        // Assert
        sequence.Description.Should().Be("Test Description");
        Log("序列描述设置成功");
    }

    [Fact]
    public void RepeatCount_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.RepeatCount = 5;

        // Assert
        sequence.RepeatCount.Should().Be(5);
        Log("重复次数设置成功");
    }

    [Fact]
    public void RepeatCount_WithZeroValue_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.RepeatCount = 0;

        // Assert
        sequence.RepeatCount.Should().Be(0);
        Log("零重复次数设置成功");
    }

    [Fact]
    public void RepeatCount_WithNegativeValue_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.RepeatCount = -1;

        // Assert
        sequence.RepeatCount.Should().Be(-1);
        Log("负重复次数设置成功");
    }

    [Fact]
    public void IsLooping_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.IsLooping = true;

        // Assert
        sequence.IsLooping.Should().BeTrue();
        Log("循环设置成功");
    }

    [Fact]
    public void DelayBetweenActions_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.DelayBetweenActions = 500;

        // Assert
        sequence.DelayBetweenActions.Should().Be(500);
        Log("动作间延迟设置成功");
    }

    [Fact]
    public void DelayBetweenActions_WithNegativeValue_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.DelayBetweenActions = -100;

        // Assert
        sequence.DelayBetweenActions.Should().Be(-100);
        Log("负延迟设置成功");
    }

    [Fact]
    public void IsEnabled_ShouldBeSettable()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        sequence.IsEnabled = false;

        // Assert
        sequence.IsEnabled.Should().BeFalse();
        Log("启用状态设置成功");
    }

    [Fact]
    public void AddAction_WithValidAction_ShouldAddAction()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");

        // Act
        sequence.AddAction(action);

        // Assert
        sequence.Actions.Should().HaveCount(1);
        sequence.Actions.Should().Contain(action);
        sequence.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("动作添加成功");
    }

    [Fact]
    public void AddAction_WithNullAction_ShouldThrowException()
    {
        // Arrange
        var sequence = new ActionSequence();
        IAction action = null;

        // Act & Assert
        var action2 = () => sequence.AddAction(action);
        action2.Should().Throw<ArgumentNullException>();
        LogError("空动作添加失败");
    }

    [Fact]
    public void RemoveAction_WithValidIndex_ShouldRemoveAction()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        sequence.AddAction(action);
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        var result = sequence.RemoveAction(0);

        // Assert
        result.Should().BeTrue();
        sequence.Actions.Should().BeEmpty();
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作移除成功");
    }

    [Fact]
    public void RemoveAction_WithInvalidIndex_ShouldReturnFalse()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        sequence.AddAction(action);
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        var result = sequence.RemoveAction(10);

        // Assert
        result.Should().BeFalse();
        sequence.Actions.Should().HaveCount(1);
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("无效索引移除失败");
    }

    [Fact]
    public void RemoveAction_WithNegativeIndex_ShouldReturnFalse()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        sequence.AddAction(action);
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        var result = sequence.RemoveAction(-1);

        // Assert
        result.Should().BeFalse();
        sequence.Actions.Should().HaveCount(1);
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("负索引移除失败");
    }

    [Fact]
    public void ClearActions_ShouldRemoveAllActions()
    {
        // Arrange
        var sequence = new ActionSequence();
        sequence.AddAction(new MockAction("Action1"));
        sequence.AddAction(new MockAction("Action2"));
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        sequence.ClearActions();

        // Assert
        sequence.Actions.Should().BeEmpty();
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作清空成功");
    }

    [Fact]
    public void ClearActions_WithNoActions_ShouldDoNothing()
    {
        // Arrange
        var sequence = new ActionSequence();
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        sequence.ClearActions();

        // Assert
        sequence.Actions.Should().BeEmpty();
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        Log("无动作时清空无影响");
    }

    [Fact]
    public void GetTotalDuration_WithActions_ShouldCalculateCorrectDuration()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            DelayBetweenActions = 100
        };
        sequence.AddAction(new MockAction("Action1", 200));
        sequence.AddAction(new MockAction("Action2", 300));
        sequence.AddAction(new MockAction("Action3", 150));

        // Act
        var duration = sequence.GetTotalDuration();

        // Assert
        // 200 + 100 + 300 + 100 + 150 = 850ms
        duration.Should().Be(850);
        Log($"总时长计算正确: {duration}ms");
    }

    [Fact]
    public void GetTotalDuration_WithNoActions_ShouldReturnZero()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        var duration = sequence.GetTotalDuration();

        // Assert
        duration.Should().Be(0);
        Log("无动作时总时长为0");
    }

    [Fact]
    public void GetTotalDuration_WithZeroDelay_ShouldCalculateCorrectDuration()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            DelayBetweenActions = 0
        };
        sequence.AddAction(new MockAction("Action1", 200));
        sequence.AddAction(new MockAction("Action2", 300));

        // Act
        var duration = sequence.GetTotalDuration();

        // Assert
        duration.Should().Be(500); // 200 + 300
        Log($"零延迟总时长计算正确: {duration}ms");
    }

    [Fact]
    public void GetActionCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var sequence = new ActionSequence();
        sequence.AddAction(new MockAction("Action1"));
        sequence.AddAction(new MockAction("Action2"));
        sequence.AddAction(new MockAction("Action3"));

        // Act
        var count = sequence.GetActionCount();

        // Assert
        count.Should().Be(3);
        Log("动作数量计算正确");
    }

    [Fact]
    public void GetActionCount_WithNoActions_ShouldReturnZero()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        var count = sequence.GetActionCount();

        // Assert
        count.Should().Be(0);
        Log("无动作时数量为0");
    }

    [Fact]
    public void GetActionAt_WithValidIndex_ShouldReturnAction()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        sequence.AddAction(action);

        // Act
        var result = sequence.GetActionAt(0);

        // Assert
        result.Should().BeSameAs(action);
        Log("指定索引动作获取成功");
    }

    [Fact]
    public void GetActionAt_WithInvalidIndex_ShouldReturnNull()
    {
        // Arrange
        var sequence = new ActionSequence();
        sequence.AddAction(new MockAction("Test Action"));

        // Act
        var result = sequence.GetActionAt(10);

        // Assert
        result.Should().BeNull();
        Log("无效索引返回null");
    }

    [Fact]
    public void GetActionAt_WithNegativeIndex_ShouldReturnNull()
    {
        // Arrange
        var sequence = new ActionSequence();
        sequence.AddAction(new MockAction("Test Action"));

        // Act
        var result = sequence.GetActionAt(-1);

        // Assert
        result.Should().BeNull();
        Log("负索引返回null");
    }

    [Fact]
    public void InsertActionAt_WithValidIndex_ShouldInsertAction()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action1 = new MockAction("Action1");
        var action2 = new MockAction("Action2");
        var action3 = new MockAction("Action3");
        
        sequence.AddAction(action1);
        sequence.AddAction(action3);
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        var result = sequence.InsertActionAt(1, action2);

        // Assert
        result.Should().BeTrue();
        sequence.Actions.Should().HaveCount(3);
        sequence.Actions[0].Should().BeSameAs(action1);
        sequence.Actions[1].Should().BeSameAs(action2);
        sequence.Actions[2].Should().BeSameAs(action3);
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作插入成功");
    }

    [Fact]
    public void InsertActionAt_WithInvalidIndex_ShouldReturnFalse()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        var result = sequence.InsertActionAt(10, action);

        // Assert
        result.Should().BeFalse();
        sequence.Actions.Should().BeEmpty();
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("无效索引插入失败");
    }

    [Fact]
    public void InsertActionAt_WithNullAction_ShouldThrowException()
    {
        // Arrange
        var sequence = new ActionSequence();
        IAction action = null;

        // Act & Assert
        var action2 = () => sequence.InsertActionAt(0, action);
        action2.Should().Throw<ArgumentNullException>();
        LogError("空动作插入失败");
    }

    [Fact]
    public void MoveAction_WithValidIndices_ShouldMoveAction()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action1 = new MockAction("Action1");
        var action2 = new MockAction("Action2");
        var action3 = new MockAction("Action3");
        
        sequence.AddAction(action1);
        sequence.AddAction(action2);
        sequence.AddAction(action3);
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        var result = sequence.MoveAction(0, 2);

        // Assert
        result.Should().BeTrue();
        sequence.Actions[0].Should().BeSameAs(action2);
        sequence.Actions[1].Should().BeSameAs(action3);
        sequence.Actions[2].Should().BeSameAs(action1);
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作移动成功");
    }

    [Fact]
    public void MoveAction_WithInvalidIndices_ShouldReturnFalse()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action = new MockAction("Test Action");
        sequence.AddAction(action);
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        var result = sequence.MoveAction(0, 10);

        // Assert
        result.Should().BeFalse();
        sequence.Actions.Should().HaveCount(1);
        sequence.Actions[0].Should().BeSameAs(action);
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("无效索引移动失败");
    }

    [Fact]
    public void ReverseActions_ShouldReverseActionOrder()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action1 = new MockAction("Action1");
        var action2 = new MockAction("Action2");
        var action3 = new MockAction("Action3");
        
        sequence.AddAction(action1);
        sequence.AddAction(action2);
        sequence.AddAction(action3);
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        sequence.ReverseActions();

        // Assert
        sequence.Actions[0].Should().BeSameAs(action3);
        sequence.Actions[1].Should().BeSameAs(action2);
        sequence.Actions[2].Should().BeSameAs(action1);
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作顺序反转成功");
    }

    [Fact]
    public void ReverseActions_WithNoActions_ShouldDoNothing()
    {
        // Arrange
        var sequence = new ActionSequence();
        var originalUpdatedAt = sequence.UpdatedAt;

        // Act
        sequence.ReverseActions();

        // Assert
        sequence.Actions.Should().BeEmpty();
        sequence.UpdatedAt.Should().Be(originalUpdatedAt);
        Log("无动作时反转无影响");
    }

    [Fact]
    public void ShuffleActions_ShouldRandomizeActionOrder()
    {
        // Arrange
        var sequence = new ActionSequence();
        var action1 = new MockAction("Action1");
        var action2 = new MockAction("Action2");
        var action3 = new MockAction("Action3");
        
        sequence.AddAction(action1);
        sequence.AddAction(action2);
        sequence.AddAction(action3);
        var originalUpdatedAt = sequence.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        sequence.ShuffleActions();

        // Assert
        sequence.Actions.Should().HaveCount(3);
        sequence.Actions.Should().Contain(action1);
        sequence.Actions.Should().Contain(action2);
        sequence.Actions.Should().Contain(action3);
        sequence.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("动作随机化成功");
    }

    [Fact]
    public void Clone_ShouldCreateCopy()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            Name = "Test Sequence",
            Description = "Test Description",
            RepeatCount = 3,
            IsLooping = true,
            DelayBetweenActions = 100,
            IsEnabled = true
        };
        sequence.AddAction(new MockAction("Action1"));
        sequence.AddAction(new MockAction("Action2"));

        // Act
        var clonedSequence = sequence.Clone();

        // Assert
        clonedSequence.Should().NotBeNull();
        clonedSequence.Should().NotBeSameAs(sequence);
        clonedSequence.Id.Should().NotBe(sequence.Id);
        clonedSequence.Name.Should().Be(sequence.Name);
        clonedSequence.Description.Should().Be(sequence.Description);
        clonedSequence.RepeatCount.Should().Be(sequence.RepeatCount);
        clonedSequence.IsLooping.Should().Be(sequence.IsLooping);
        clonedSequence.DelayBetweenActions.Should().Be(sequence.DelayBetweenActions);
        clonedSequence.IsEnabled.Should().Be(sequence.IsEnabled);
        clonedSequence.Actions.Should().HaveCount(2);
        Log("序列克隆成功");
    }

    [Fact]
    public void ToString_ShouldReturnSequenceName()
    {
        // Arrange
        var sequence = new ActionSequence { Name = "Test Sequence" };

        // Act
        var result = sequence.ToString();

        // Assert
        result.Should().Be("Test Sequence");
        Log("序列字符串表示成功");
    }

    [Fact]
    public void ToString_WithNoName_ShouldReturnSequenceId()
    {
        // Arrange
        var sequence = new ActionSequence();

        // Act
        var result = sequence.ToString();

        // Assert
        result.Should().Be(sequence.Id.ToString());
        Log("无名称序列字符串表示成功");
    }

    [Fact]
    public void Validate_WithValidSequence_ShouldReturnSuccess()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            Name = "Test Sequence",
            IsEnabled = true
        };
        sequence.AddAction(new MockAction("Action1"));

        // Act
        var result = sequence.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("有效序列验证成功");
    }

    [Fact]
    public void Validate_WithInvalidSequence_ShouldReturnFailure()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            Name = ""
        };

        // Act
        var result = sequence.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("name"));
        LogError("无效序列验证失败");
    }

    [Fact]
    public void Validate_WithDisabledSequence_ShouldReturnSuccess()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            Name = "Test Sequence",
            IsEnabled = false
        };

        // Act
        var result = sequence.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("禁用序列验证成功");
    }

    [Fact]
    public void GetExecutionPlan_WithValidSequence_ShouldReturnPlan()
    {
        // Arrange
        var sequence = new ActionSequence
        {
            RepeatCount = 2,
            DelayBetweenActions = 100
        };
        sequence.AddAction(new MockAction("Action1", 200));
        sequence.AddAction(new MockAction("Action2", 300));

        // Act
        var plan = sequence.GetExecutionPlan();

        // Assert
        plan.Should().NotBeNull();
        plan.TotalDuration.Should().Be(1200); // (200 + 100 + 300) * 2
        plan.TotalActions.Should().Be(4); // 2 actions * 2 repeats
        plan.RepeatCount.Should().Be(2);
        Log($"执行计划生成成功: {plan.TotalDuration}ms, {plan.TotalActions}个动作");
    }
}

/// <summary>
/// 模拟动作接口实现
/// </summary>
public class MockAction : IAction
{
    public string Name { get; }
    public int Duration { get; }

    public MockAction(string name, int duration = 100)
    {
        Name = name;
        Duration = duration;
    }

    public void Execute()
    {
        // 模拟执行
    }

    public bool CanExecute()
    {
        return true;
    }

    public override string ToString()
    {
        return Name;
    }
}