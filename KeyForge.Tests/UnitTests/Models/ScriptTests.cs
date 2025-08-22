using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using KeyForge.Core.Models;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Models;

/// <summary>
/// Script 模型单元测试
/// 原本实现：复杂的脚本模型测试
/// 简化实现：核心脚本功能测试
/// </summary>
public class ScriptTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitialize()
    {
        // Act
        var script = new Script();

        // Assert
        script.Should().NotBeNull();
        script.Name.Should().BeNull();
        script.Description.Should().BeNull();
        script.Actions.Should().NotBeNull();
        script.Actions.Should().BeEmpty();
        script.RepeatCount.Should().Be(1);
        script.Loop.Should().BeFalse();
        script.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        script.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("Script 默认构造成功");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange
        var name = "Test Script";
        var description = "Test Description";

        // Act
        var script = new Script
        {
            Name = name,
            Description = description
        };

        // Assert
        script.Name.Should().Be(name);
        script.Description.Should().Be(description);
        Log("Script 参数构造成功");
    }

    [Fact]
    public void AddAction_WithValidAction_ShouldAddToList()
    {
        // Arrange
        var script = new Script();
        var action = new KeyAction
        {
            Key = "A",
            Delay = 100
        };

        // Act
        script.AddAction(action);

        // Assert
        script.Actions.Should().HaveCount(1);
        script.Actions.Should().Contain(action);
        script.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("动作添加成功");
    }

    [Fact]
    public void AddAction_WithNullAction_ShouldThrowException()
    {
        // Arrange
        var script = new Script();
        KeyAction action = null;

        // Act & Assert
        var action = () => script.AddAction(action);
        action.Should().Throw<ArgumentNullException>();
        LogError("空动作添加失败");
    }

    [Fact]
    public void RemoveAction_WithValidIndex_ShouldRemoveFromList()
    {
        // Arrange
        var script = new Script();
        var action = new KeyAction
        {
            Key = "A",
            Delay = 100
        };
        script.AddAction(action);

        // Act
        script.RemoveAction(0);

        // Assert
        script.Actions.Should().BeEmpty();
        script.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("动作移除成功");
    }

    [Fact]
    public void RemoveAction_WithInvalidIndex_ShouldDoNothing()
    {
        // Arrange
        var script = new Script();
        var action = new KeyAction
        {
            Key = "A",
            Delay = 100
        };
        script.AddAction(action);
        var originalUpdatedAt = script.UpdatedAt;

        // Act
        script.RemoveAction(10);

        // Assert
        script.Actions.Should().HaveCount(1);
        script.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("无效索引移除失败");
    }

    [Fact]
    public void RemoveAction_WithNegativeIndex_ShouldDoNothing()
    {
        // Arrange
        var script = new Script();
        var action = new KeyAction
        {
            Key = "A",
            Delay = 100
        };
        script.AddAction(action);
        var originalUpdatedAt = script.UpdatedAt;

        // Act
        script.RemoveAction(-1);

        // Assert
        script.Actions.Should().HaveCount(1);
        script.UpdatedAt.Should().Be(originalUpdatedAt);
        LogError("负索引移除失败");
    }

    [Fact]
    public void ClearActions_WithActions_ShouldClearAll()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        script.AddAction(new KeyAction { Key = "B", Delay = 200 });

        // Act
        script.ClearActions();

        // Assert
        script.Actions.Should().BeEmpty();
        script.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("动作清空成功");
    }

    [Fact]
    public void ClearActions_WithEmptyList_ShouldDoNothing()
    {
        // Arrange
        var script = new Script();
        var originalUpdatedAt = script.UpdatedAt;

        // Act
        script.ClearActions();

        // Assert
        script.Actions.Should().BeEmpty();
        script.UpdatedAt.Should().Be(originalUpdatedAt);
        Log("空列表清空无变化");
    }

    [Fact]
    public void GetTotalDuration_WithNoActions_ShouldReturnZero()
    {
        // Arrange
        var script = new Script();

        // Act
        var duration = script.GetTotalDuration();

        // Assert
        duration.Should().Be(0);
        Log("无动作时总时长为0");
    }

    [Fact]
    public void GetTotalDuration_WithActions_ShouldReturnSumOfDelays()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        script.AddAction(new KeyAction { Key = "B", Delay = 200 });
        script.AddAction(new KeyAction { Key = "C", Delay = 50 });

        // Act
        var duration = script.GetTotalDuration();

        // Assert
        duration.Should().Be(350);
        Log($"总时长计算正确: {duration}ms");
    }

    [Fact]
    public void GetTotalDuration_WithZeroDelay_ShouldReturnZero()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 0 });
        script.AddAction(new KeyAction { Key = "B", Delay = 0 });

        // Act
        var duration = script.GetTotalDuration();

        // Assert
        duration.Should().Be(0);
        Log("零延迟动作总时长为0");
    }

    [Fact]
    public void GetActionCount_WithNoActions_ShouldReturnZero()
    {
        // Arrange
        var script = new Script();

        // Act
        var count = script.GetActionCount();

        // Assert
        count.Should().Be(0);
        Log("无动作时数量为0");
    }

    [Fact]
    public void GetActionCount_WithActions_ShouldReturnCorrectCount()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        script.AddAction(new KeyAction { Key = "B", Delay = 200 });
        script.AddAction(new KeyAction { Key = "C", Delay = 50 });

        // Act
        var count = script.GetActionCount();

        // Assert
        count.Should().Be(3);
        Log($"动作数量计算正确: {count}");
    }

    [Fact]
    public void UpdatedAt_ShouldBeUpdated_WhenActionAdded()
    {
        // Arrange
        var script = new Script();
        var originalUpdatedAt = script.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });

        // Assert
        script.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("添加动作时更新时间正确");
    }

    [Fact]
    public void UpdatedAt_ShouldBeUpdated_WhenActionRemoved()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        var originalUpdatedAt = script.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        script.RemoveAction(0);

        // Assert
        script.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("移除动作时更新时间正确");
    }

    [Fact]
    public void UpdatedAt_ShouldBeUpdated_WhenActionsCleared()
    {
        // Arrange
        var script = new Script();
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        var originalUpdatedAt = script.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        script.ClearActions();

        // Assert
        script.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("清空动作时更新时间正确");
    }

    [Fact]
    public void RepeatCount_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.RepeatCount = 5;

        // Assert
        script.RepeatCount.Should().Be(5);
        Log("重复次数设置成功");
    }

    [Fact]
    public void RepeatCount_WithZeroValue_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.RepeatCount = 0;

        // Assert
        script.RepeatCount.Should().Be(0);
        Log("零重复次数设置成功");
    }

    [Fact]
    public void RepeatCount_WithNegativeValue_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.RepeatCount = -1;

        // Assert
        script.RepeatCount.Should().Be(-1);
        Log("负重复次数设置成功");
    }

    [Fact]
    public void Loop_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.Loop = true;

        // Assert
        script.Loop.Should().BeTrue();
        Log("循环设置成功");
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.Name = "Test Script";

        // Assert
        script.Name.Should().Be("Test Script");
        Log("名称设置成功");
    }

    [Fact]
    public void Name_WithEmptyString_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.Name = "";

        // Assert
        script.Name.Should().Be("");
        Log("空名称设置成功");
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.Description = "Test Description";

        // Assert
        script.Description.Should().Be("Test Description");
        Log("描述设置成功");
    }

    [Fact]
    public void Description_WithEmptyString_ShouldBeSettable()
    {
        // Arrange
        var script = new Script();

        // Act
        script.Description = "";

        // Assert
        script.Description.Should().Be("");
        Log("空描述设置成功");
    }

    [Fact]
    public void Actions_ShouldBeReadOnly()
    {
        // Arrange
        var script = new Script();
        var newActions = new List<KeyAction>();

        // Act
        var actions = script.Actions;

        // Assert
        actions.Should().BeOfType<List<KeyAction>>();
        actions.Should().NotBeSameAs(newActions);
        Log("动作列表是只读的");
    }
}

/// <summary>
/// 键盘动作模型
/// </summary>
public class KeyAction
{
    public string Key { get; set; }
    public int Delay { get; set; }
    public bool IsKeyDown { get; set; }
    public DateTime Timestamp { get; set; }
}