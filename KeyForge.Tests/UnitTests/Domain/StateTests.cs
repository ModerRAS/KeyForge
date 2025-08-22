using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using KeyForge.Domain.Entities;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Domain;

/// <summary>
/// State 单元测试
/// 原本实现：复杂的状态管理测试
/// 简化实现：核心状态功能测试
/// </summary>
public class StateTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitialize()
    {
        // Act
        var state = new State();

        // Assert
        state.Should().NotBeNull();
        state.Id.Should().NotBeEmpty();
        state.Name.Should().BeNull();
        state.Description.Should().BeNull();
        state.Values.Should().NotBeNull();
        state.values.Should().BeEmpty();
        state.IsActive.Should().BeTrue();
        state.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        state.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("State 默认构造成功");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange
        var name = "Test State";
        var description = "Test Description";
        var initialValues = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };

        // Act
        var state = new State
        {
            Name = name,
            Description = description,
            values = initialValues
        };

        // Assert
        state.Name.Should().Be(name);
        state.Description.Should().Be(description);
        state.values.Should().Equal(initialValues);
        Log("State 参数构造成功");
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var state = new State();

        // Act
        state.Name = "Test State";

        // Assert
        state.Name.Should().Be("Test State");
        Log("状态名称设置成功");
    }

    [Fact]
    public void Name_WithEmptyString_ShouldBeSettable()
    {
        // Arrange
        var state = new State();

        // Act
        state.Name = "";

        // Assert
        state.Name.Should().Be("");
        Log("空状态名称设置成功");
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var state = new State();

        // Act
        state.Description = "Test Description";

        // Assert
        state.Description.Should().Be("Test Description");
        Log("状态描述设置成功");
    }

    [Fact]
    public void IsActive_ShouldBeSettable()
    {
        // Arrange
        var state = new State();

        // Act
        state.IsActive = false;

        // Assert
        state.IsActive.Should().BeFalse();
        Log("状态激活设置成功");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var state = new State { IsActive = false };

        // Act
        state.Activate();

        // Assert
        state.IsActive.Should().BeTrue();
        Log("状态激活成功");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var state = new State { IsActive = true };

        // Act
        state.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
        Log("状态停用成功");
    }

    [Fact]
    public void SetValue_WithValidKeyAndValue_ShouldSetValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        var value = "testValue";

        // Act
        state.SetValue(key, value);

        // Assert
        state.values.Should().ContainKey(key);
        state.values[key].Should().Be(value);
        state.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("状态值设置成功");
    }

    [Fact]
    public void SetValue_WithNullKey_ShouldThrowException()
    {
        // Arrange
        var state = new State();
        string key = null;
        var value = "testValue";

        // Act & Assert
        var action = () => state.SetValue(key, value);
        action.Should().Throw<ArgumentNullException>();
        LogError("空键设置失败");
    }

    [Fact]
    public void SetValue_WithEmptyKey_ShouldThrowException()
    {
        // Arrange
        var state = new State();
        var key = "";
        var value = "testValue";

        // Act & Assert
        var action = () => state.SetValue(key, value);
        action.Should().Throw<ArgumentException>();
        LogError("空键设置失败");
    }

    [Fact]
    public void SetValue_WithNullValue_ShouldSetValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        object value = null;

        // Act
        state.SetValue(key, value);

        // Assert
        state.values.Should().ContainKey(key);
        state.values[key].Should().BeNull();
        Log("空值设置成功");
    }

    [Fact]
    public void SetValue_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        state.SetValue(key, "originalValue");
        var originalUpdatedAt = state.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        state.SetValue(key, "updatedValue");

        // Assert
        state.values[key].Should().Be("updatedValue");
        state.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("现有键值更新成功");
    }

    [Fact]
    public void GetValue_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        var expectedValue = "testValue";
        state.SetValue(key, expectedValue);

        // Act
        var result = state.GetValue(key);

        // Assert
        result.Should().Be(expectedValue);
        Log("状态值获取成功");
    }

    [Fact]
    public void GetValue_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var state = new State();
        var key = "nonExistingKey";

        // Act
        var result = state.GetValue(key);

        // Assert
        result.Should().BeNull();
        Log("不存在键返回null");
    }

    [Fact]
    public void GetValue_WithNullKey_ShouldThrowException()
    {
        // Arrange
        var state = new State();
        string key = null;

        // Act & Assert
        var action = () => state.GetValue(key);
        action.Should().Throw<ArgumentNullException>();
        LogError("空键获取失败");
    }

    [Fact]
    public void GetValue_WithEmptyKey_ShouldThrowException()
    {
        // Arrange
        var state = new State();
        var key = "";

        // Act & Assert
        var action = () => state.GetValue(key);
        action.Should().Throw<ArgumentException>();
        LogError("空键获取失败");
    }

    [Fact]
    public void GetValue_WithDefault_ShouldReturnValueOrDefault()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        var defaultValue = "defaultValue";

        // Act
        var result1 = state.GetValue(key, defaultValue);
        state.SetValue(key, "actualValue");
        var result2 = state.GetValue(key, defaultValue);

        // Assert
        result1.Should().Be(defaultValue);
        result2.Should().Be("actualValue");
        Log("默认值获取成功");
    }

    [Fact]
    public void TryGetValue_WithExistingKey_ShouldReturnTrueAndValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        var expectedValue = "testValue";
        state.SetValue(key, expectedValue);

        // Act
        var result = state.TryGetValue(key, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(expectedValue);
        Log("状态值尝试获取成功");
    }

    [Fact]
    public void TryGetValue_WithNonExistingKey_ShouldReturnFalseAndNull()
    {
        // Arrange
        var state = new State();
        var key = "nonExistingKey";

        // Act
        var result = state.TryGetValue(key, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
        Log("不存在键尝试获取返回false");
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        state.SetValue(key, "testValue");

        // Act
        var result = state.ContainsKey(key);

        // Assert
        result.Should().BeTrue();
        Log("包含键检查成功");
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var state = new State();
        var key = "nonExistingKey";

        // Act
        var result = state.ContainsKey(key);

        // Assert
        result.Should().BeFalse();
        Log("不包含键检查成功");
    }

    [Fact]
    public void RemoveValue_WithExistingKey_ShouldRemoveValue()
    {
        // Arrange
        var state = new State();
        var key = "testKey";
        state.SetValue(key, "testValue");
        var originalUpdatedAt = state.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        var result = state.RemoveValue(key);

        // Assert
        result.Should().BeTrue();
        state.values.Should().NotContainKey(key);
        state.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("状态值移除成功");
    }

    [Fact]
    public void RemoveValue_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var state = new State();
        var key = "nonExistingKey";

        // Act
        var result = state.RemoveValue(key);

        // Assert
        result.Should().BeFalse();
        Log("不存在键移除返回false");
    }

    [Fact]
    public void RemoveValue_WithNullKey_ShouldThrowException()
    {
        // Arrange
        var state = new State();
        string key = null;

        // Act & Assert
        var action = () => state.RemoveValue(key);
        action.Should().Throw<ArgumentNullException>();
        LogError("空键移除失败");
    }

    [Fact]
    public void ClearValues_ShouldRemoveAllValues()
    {
        // Arrange
        var state = new State();
        state.SetValue("key1", "value1");
        state.SetValue("key2", "value2");
        var originalUpdatedAt = state.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        state.ClearValues();

        // Assert
        state.values.Should().BeEmpty();
        state.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("状态值清空成功");
    }

    [Fact]
    public void ClearValues_WithNoValues_ShouldDoNothing()
    {
        // Arrange
        var state = new State();
        var originalUpdatedAt = state.UpdatedAt;

        // Act
        state.ClearValues();

        // Assert
        state.values.Should().BeEmpty();
        state.UpdatedAt.Should().Be(originalUpdatedAt);
        Log("无值时清空无影响");
    }

    [Fact]
    public void GetAllValues_ShouldReturnAllValues()
    {
        // Arrange
        var state = new State();
        var expectedValues = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };

        foreach (var kvp in expectedValues)
        {
            state.SetValue(kvp.Key, kvp.Value);
        }

        // Act
        var result = state.GetAllValues();

        // Assert
        result.Should().Equal(expectedValues);
        Log("所有状态值获取成功");
    }

    [Fact]
    public void GetAllValues_WithNoValues_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var state = new State();

        // Act
        var result = state.GetAllValues();

        // Assert
        result.Should().BeEmpty();
        Log("无值时返回空字典");
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeys()
    {
        // Arrange
        var state = new State();
        var expectedKeys = new[] { "key1", "key2", "key3" };

        foreach (var key in expectedKeys)
        {
            state.SetValue(key, $"value{key}");
        }

        // Act
        var result = state.GetKeys();

        // Assert
        result.Should().BeEquivalentTo(expectedKeys);
        Log("所有键获取成功");
    }

    [Fact]
    public void GetKeys_WithNoValues_ShouldReturnEmptyList()
    {
        // Arrange
        var state = new State();

        // Act
        var result = state.GetKeys();

        // Assert
        result.Should().BeEmpty();
        Log("无值时返回空键列表");
    }

    [Fact]
    public void GetValueCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var state = new State();
        state.SetValue("key1", "value1");
        state.SetValue("key2", "value2");
        state.SetValue("key3", "value3");

        // Act
        var count = state.GetValueCount();

        // Assert
        count.Should().Be(3);
        Log("状态值计数正确");
    }

    [Fact]
    public void GetValueCount_WithNoValues_ShouldReturnZero()
    {
        // Arrange
        var state = new State();

        // Act
        var count = state.GetValueCount();

        // Assert
        count.Should().Be(0);
        Log("无值时计数为0");
    }

    [Fact]
    public void HasValues_ShouldReturnCorrectStatus()
    {
        // Arrange
        var state = new State();

        // Act & Assert
        state.HasValues().Should().BeFalse();
        
        state.SetValue("key1", "value1");
        state.HasValues().Should().BeTrue();
        
        state.ClearValues();
        state.HasValues().Should().BeFalse();
        Log("状态值存在检查成功");
    }

    [Fact]
    public void Clone_ShouldCreateCopy()
    {
        // Arrange
        var state = new State
        {
            Name = "Test State",
            Description = "Test Description",
            IsActive = true
        };
        state.SetValue("key1", "value1");
        state.SetValue("key2", 42);

        // Act
        var clonedState = state.Clone();

        // Assert
        clonedState.Should().NotBeNull();
        clonedState.Should().NotBeSameAs(state);
        clonedState.Id.Should().NotBe(state.Id);
        clonedState.Name.Should().Be(state.Name);
        clonedState.Description.Should().Be(state.Description);
        clonedState.IsActive.Should().Be(state.IsActive);
        clonedState.values.Should().Equal(state.values);
        Log("状态克隆成功");
    }

    [Fact]
    public void ToString_ShouldReturnStateName()
    {
        // Arrange
        var state = new State { Name = "Test State" };

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("Test State");
        Log("状态字符串表示成功");
    }

    [Fact]
    public void ToString_WithNoName_ShouldReturnStateId()
    {
        // Arrange
        var state = new State();

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be(state.Id.ToString());
        Log("无名称状态字符串表示成功");
    }

    [Fact]
    public void Equals_WithSameState_ShouldReturnTrue()
    {
        // Arrange
        var state = new State { Name = "Test State" };

        // Act
        var result = state.Equals(state);

        // Assert
        result.Should().BeTrue();
        Log("相同状态相等");
    }

    [Fact]
    public void Equals_WithDifferentState_ShouldReturnFalse()
    {
        // Arrange
        var state1 = new State { Name = "Test State 1" };
        var state2 = new State { Name = "Test State 2" };

        // Act
        var result = state1.Equals(state2);

        // Assert
        result.Should().BeFalse();
        Log("不同状态不相等");
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var state = new State { Name = "Test State" };

        // Act
        var result = state.Equals(null);

        // Assert
        result.Should().BeFalse();
        Log("与null比较不相等");
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var state = new State { Name = "Test State" };

        // Act
        var hash1 = state.GetHashCode();
        var hash2 = state.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        Log("状态哈希码一致");
    }

    [Fact]
    public void Validate_WithValidState_ShouldReturnSuccess()
    {
        // Arrange
        var state = new State
        {
            Name = "Test State",
            IsActive = true
        };

        // Act
        var result = state.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("有效状态验证成功");
    }

    [Fact]
    public void Validate_WithInvalidState_ShouldReturnFailure()
    {
        // Arrange
        var state = new State
        {
            Name = ""
        };

        // Act
        var result = state.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("name"));
        LogError("无效状态验证失败");
    }

    [Fact]
    public void MergeWith_WithOtherState_ShouldMergeValues()
    {
        // Arrange
        var state1 = new State { Name = "State 1" };
        var state2 = new State { Name = "State 2" };
        
        state1.SetValue("key1", "value1");
        state1.SetValue("key2", "value2");
        state2.SetValue("key2", "newValue2");
        state2.SetValue("key3", "value3");

        // Act
        state1.MergeWith(state2);

        // Assert
        state1.values.Should().ContainKey("key1");
        state1.values.Should().ContainKey("key2");
        state1.values.Should().ContainKey("key3");
        state1.values["key1"].Should().Be("value1");
        state1.values["key2"].Should().Be("newValue2");
        state1.values["key3"].Should().Be("value3");
        Log("状态合并成功");
    }

    [Fact]
    public void MergeWith_WithNullState_ShouldThrowException()
    {
        // Arrange
        var state = new State { Name = "Test State" };

        // Act & Assert
        var action = () => state.MergeWith(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空状态合并失败");
    }

    [Fact]
    public void MergeWith_WithInactiveState_ShouldNotMerge()
    {
        // Arrange
        var state1 = new State { Name = "State 1" };
        var state2 = new State { Name = "State 2", IsActive = false };
        
        state1.SetValue("key1", "value1");
        state2.SetValue("key2", "value2");

        // Act
        state1.MergeWith(state2);

        // Assert
        state1.values.Should().ContainKey("key1");
        state1.values.Should().NotContainKey("key2");
        Log("非活跃状态不合并");
    }
}