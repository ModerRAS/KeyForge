using Xunit;
using FluentAssertions;
using System;
using KeyForge.Domain.Entities;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Domain;

/// <summary>
/// DecisionRule 单元测试
/// 原本实现：复杂的决策规则测试
/// 简化实现：核心决策规则功能测试
/// </summary>
public class DecisionRuleTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitialize()
    {
        // Act
        var rule = new DecisionRule();

        // Assert
        rule.Should().NotBeNull();
        rule.Id.Should().NotBeEmpty();
        rule.Name.Should().BeNull();
        rule.Description.Should().BeNull();
        rule.Condition.Should().BeNull();
        rule.Action.Should().BeNull();
        rule.Priority.Should().Be(0);
        rule.IsEnabled.Should().BeTrue();
        rule.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        rule.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("DecisionRule 默认构造成功");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange
        var name = "Test Rule";
        var description = "Test Description";
        var condition = "condition == true";
        var action = "ExecuteAction()";

        // Act
        var rule = new DecisionRule
        {
            Name = name,
            Description = description,
            Condition = condition,
            Action = action
        };

        // Assert
        rule.Name.Should().Be(name);
        rule.Description.Should().Be(description);
        rule.Condition.Should().Be(condition);
        rule.Action.Should().Be(action);
        Log("DecisionRule 参数构造成功");
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Name = "Test Rule";

        // Assert
        rule.Name.Should().Be("Test Rule");
        Log("规则名称设置成功");
    }

    [Fact]
    public void Name_WithEmptyString_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Name = "";

        // Assert
        rule.Name.Should().Be("");
        Log("空规则名称设置成功");
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Description = "Test Description";

        // Assert
        rule.Description.Should().Be("Test Description");
        Log("规则描述设置成功");
    }

    [Fact]
    public void Condition_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Condition = "condition == true";

        // Assert
        rule.Condition.Should().Be("condition == true");
        Log("规则条件设置成功");
    }

    [Fact]
    public void Action_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Action = "ExecuteAction()";

        // Assert
        rule.Action.Should().Be("ExecuteAction()");
        Log("规则动作设置成功");
    }

    [Fact]
    public void Priority_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Priority = 5;

        // Assert
        rule.Priority.Should().Be(5);
        Log("规则优先级设置成功");
    }

    [Fact]
    public void Priority_WithNegativeValue_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Priority = -1;

        // Assert
        rule.Priority.Should().Be(-1);
        Log("负优先级设置成功");
    }

    [Fact]
    public void Priority_WithZeroValue_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.Priority = 0;

        // Assert
        rule.Priority.Should().Be(0);
        Log("零优先级设置成功");
    }

    [Fact]
    public void IsEnabled_ShouldBeSettable()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        rule.IsEnabled = false;

        // Assert
        rule.IsEnabled.Should().BeFalse();
        Log("规则启用状态设置成功");
    }

    [Fact]
    public void Enable_ShouldSetIsEnabledToTrue()
    {
        // Arrange
        var rule = new DecisionRule { IsEnabled = false };

        // Act
        rule.Enable();

        // Assert
        rule.IsEnabled.Should().BeTrue();
        Log("规则启用成功");
    }

    [Fact]
    public void Disable_ShouldSetIsEnabledToFalse()
    {
        // Arrange
        var rule = new DecisionRule { IsEnabled = true };

        // Act
        rule.Disable();

        // Assert
        rule.IsEnabled.Should().BeFalse();
        Log("规则禁用成功");
    }

    [Fact]
    public void Update_ShouldUpdateProperties()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Original Name",
            Description = "Original Description",
            Condition = "original_condition",
            Action = "original_action",
            Priority = 1
        };

        var originalUpdatedAt = rule.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        rule.Update(
            name: "Updated Name",
            description: "Updated Description",
            condition: "updated_condition",
            action: "updated_action",
            priority: 5
        );

        // Assert
        rule.Name.Should().Be("Updated Name");
        rule.Description.Should().Be("Updated Description");
        rule.Condition.Should().Be("updated_condition");
        rule.Action.Should().Be("updated_action");
        rule.Priority.Should().Be(5);
        rule.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("规则更新成功");
    }

    [Fact]
    public void Update_WithPartialParameters_ShouldUpdateOnlyProvidedProperties()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Original Name",
            Description = "Original Description",
            Condition = "original_condition",
            Action = "original_action",
            Priority = 1
        };

        var originalUpdatedAt = rule.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        rule.Update(name: "Updated Name");

        // Assert
        rule.Name.Should().Be("Updated Name");
        rule.Description.Should().Be("Original Description");
        rule.Condition.Should().Be("original_condition");
        rule.Action.Should().Be("original_action");
        rule.Priority.Should().Be(1);
        rule.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("规则部分更新成功");
    }

    [Fact]
    public void Update_WithNullName_ShouldThrowException()
    {
        // Arrange
        var rule = new DecisionRule { Name = "Original Name" };

        // Act & Assert
        var action = () => rule.Update(name: null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空名称更新失败");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var rule = new DecisionRule { Name = "Original Name" };

        // Act & Assert
        var action = () => rule.Update(name: "");
        action.Should().Throw<ArgumentException>();
        LogError("空名称更新失败");
    }

    [Fact]
    public void Evaluate_WithValidCondition_ShouldReturnResult()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return true;"
        };

        // Act
        var result = rule.Evaluate();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().BeTrue();
        Log("规则评估成功");
    }

    [Fact]
    public void Evaluate_WithInvalidCondition_ShouldReturnFailure()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "invalid_condition_syntax",
            Action = "return true;"
        };

        // Act
        var result = rule.Evaluate();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        LogError("无效条件评估失败");
    }

    [Fact]
    public void Evaluate_WithDisabledRule_ShouldReturnNotEvaluated()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return true;",
            IsEnabled = false
        };

        // Act
        var result = rule.Evaluate();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disabled");
        Log("禁用规则评估返回未评估");
    }

    [Fact]
    public void Execute_WithValidAction_ShouldReturnResult()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return \"success\";"
        };

        // Act
        var result = rule.Execute();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("success");
        Log("规则执行成功");
    }

    [Fact]
    public void Execute_WithInvalidAction_ShouldReturnFailure()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "invalid_action_syntax"
        };

        // Act
        var result = rule.Execute();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        LogError("无效动作执行失败");
    }

    [Fact]
    public void Execute_WithDisabledRule_ShouldReturnNotExecuted()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return \"success\";",
            IsEnabled = false
        };

        // Act
        var result = rule.Execute();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disabled");
        Log("禁用规则执行返回未执行");
    }

    [Fact]
    public void EvaluateAndExecute_WithValidRule_ShouldReturnSuccess()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return \"success\";"
        };

        // Act
        var result = rule.EvaluateAndExecute();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("success");
        Log("规则评估并执行成功");
    }

    [Fact]
    public void EvaluateAndExecute_WithConditionFalse_ShouldNotExecute()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "false",
            Action = "return \"success\";"
        };

        // Act
        var result = rule.EvaluateAndExecute();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("condition");
        Log("条件为假时不执行");
    }

    [Fact]
    public void Clone_ShouldCreateCopy()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Description = "Test Description",
            Condition = "true",
            Action = "return \"success\";",
            Priority = 5,
            IsEnabled = true
        };

        // Act
        var clonedRule = rule.Clone();

        // Assert
        clonedRule.Should().NotBeNull();
        clonedRule.Should().NotBeSameAs(rule);
        clonedRule.Id.Should().NotBe(rule.Id);
        clonedRule.Name.Should().Be(rule.Name);
        clonedRule.Description.Should().Be(rule.Description);
        clonedRule.Condition.Should().Be(rule.Condition);
        clonedRule.Action.Should().Be(rule.Action);
        clonedRule.Priority.Should().Be(rule.Priority);
        clonedRule.IsEnabled.Should().Be(rule.IsEnabled);
        Log("规则克隆成功");
    }

    [Fact]
    public void ToString_ShouldReturnRuleName()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule"
        };

        // Act
        var result = rule.ToString();

        // Assert
        result.Should().Be("Test Rule");
        Log("规则字符串表示成功");
    }

    [Fact]
    public void ToString_WithNoName_ShouldReturnRuleId()
    {
        // Arrange
        var rule = new DecisionRule();

        // Act
        var result = rule.ToString();

        // Assert
        result.Should().Be(rule.Id.ToString());
        Log("无名称规则字符串表示成功");
    }

    [Fact]
    public void Equals_WithSameRule_ShouldReturnTrue()
    {
        // Arrange
        var rule = new DecisionRule { Name = "Test Rule" };

        // Act
        var result = rule.Equals(rule);

        // Assert
        result.Should().BeTrue();
        Log("相同规则相等");
    }

    [Fact]
    public void Equals_WithDifferentRule_ShouldReturnFalse()
    {
        // Arrange
        var rule1 = new DecisionRule { Name = "Test Rule 1" };
        var rule2 = new DecisionRule { Name = "Test Rule 2" };

        // Act
        var result = rule1.Equals(rule2);

        // Assert
        result.Should().BeFalse();
        Log("不同规则不相等");
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var rule = new DecisionRule { Name = "Test Rule" };

        // Act
        var result = rule.Equals(null);

        // Assert
        result.Should().BeFalse();
        Log("与null比较不相等");
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var rule = new DecisionRule { Name = "Test Rule" };

        // Act
        var hash1 = rule.GetHashCode();
        var hash2 = rule.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        Log("规则哈希码一致");
    }

    [Fact]
    public void Validate_WithValidRule_ShouldReturnSuccess()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return true;"
        };

        // Act
        var result = rule.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("有效规则验证成功");
    }

    [Fact]
    public void Validate_WithInvalidRule_ShouldReturnFailure()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "",
            Condition = null,
            Action = null
        };

        // Act
        var result = rule.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("name"));
        result.Errors.Should().Contain(e => e.Contains("condition"));
        result.Errors.Should().Contain(e => e.Contains("action"));
        LogError("无效规则验证失败");
    }

    [Fact]
    public void Validate_WithDisabledRule_ShouldReturnSuccess()
    {
        // Arrange
        var rule = new DecisionRule
        {
            Name = "Test Rule",
            Condition = "true",
            Action = "return true;",
            IsEnabled = false
        };

        // Act
        var result = rule.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("禁用规则验证成功");
    }
}