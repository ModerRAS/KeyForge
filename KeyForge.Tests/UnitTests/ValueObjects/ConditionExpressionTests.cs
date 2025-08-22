using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using KeyForge.Domain.ValueObjects;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.ValueObjects;

/// <summary>
/// ConditionExpression 单元测试
/// 原本实现：复杂的条件表达式测试
/// 简化实现：核心条件表达式功能测试
/// </summary>
public class ConditionExpressionTests : TestBase
{
    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitialize()
    {
        // Act
        var condition = new ConditionExpression();

        // Assert
        condition.Should().NotBeNull();
        condition.Id.Should().NotBeEmpty();
        condition.Expression.Should().BeNull();
        condition.Description.Should().BeNull();
        condition.Variables.Should().NotBeNull();
        condition.Variables.Should().BeEmpty();
        condition.IsEnabled.Should().BeTrue();
        condition.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        condition.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("ConditionExpression 默认构造成功");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange
        var expression = "x > 10 && y < 20";
        var description = "Test condition";
        var variables = new Dictionary<string, object>
        {
            { "x", 15 },
            { "y", 15 }
        };

        // Act
        var condition = new ConditionExpression
        {
            Expression = expression,
            Description = description,
            Variables = variables
        };

        // Assert
        condition.Expression.Should().Be(expression);
        condition.Description.Should().Be(description);
        condition.Variables.Should().Equal(variables);
        Log("ConditionExpression 参数构造成功");
    }

    [Fact]
    public void Expression_ShouldBeSettable()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        condition.Expression = "x > 10";

        // Assert
        condition.Expression.Should().Be("x > 10");
        Log("表达式设置成功");
    }

    [Fact]
    public void Expression_WithEmptyString_ShouldBeSettable()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        condition.Expression = "";

        // Assert
        condition.Expression.Should().Be("");
        Log("空表达式设置成功");
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        condition.Description = "Test condition";

        // Assert
        condition.Description.Should().Be("Test condition");
        Log("描述设置成功");
    }

    [Fact]
    public void IsEnabled_ShouldBeSettable()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        condition.IsEnabled = false;

        // Assert
        condition.IsEnabled.Should().BeFalse();
        Log("启用状态设置成功");
    }

    [Fact]
    public void SetVariable_WithValidKeyAndValue_ShouldSetVariable()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "x";
        var value = 10;

        // Act
        condition.SetVariable(key, value);

        // Assert
        condition.Variables.Should().ContainKey(key);
        condition.Variables[key].Should().Be(value);
        condition.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        Log("变量设置成功");
    }

    [Fact]
    public void SetVariable_WithNullKey_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression();
        string key = null;
        var value = 10;

        // Act & Assert
        var action = () => condition.SetVariable(key, value);
        action.Should().Throw<ArgumentNullException>();
        LogError("空键变量设置失败");
    }

    [Fact]
    public void SetVariable_WithEmptyKey_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "";
        var value = 10;

        // Act & Assert
        var action = () => condition.SetVariable(key, value);
        action.Should().Throw<ArgumentException>();
        LogError("空键变量设置失败");
    }

    [Fact]
    public void SetVariable_WithNullValue_ShouldSetVariable()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "x";
        object value = null;

        // Act
        condition.SetVariable(key, value);

        // Assert
        condition.Variables.Should().ContainKey(key);
        condition.Variables[key].Should().BeNull();
        Log("空值变量设置成功");
    }

    [Fact]
    public void SetVariable_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "x";
        condition.SetVariable(key, 10);
        var originalUpdatedAt = condition.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        condition.SetVariable(key, 20);

        // Assert
        condition.Variables[key].Should().Be(20);
        condition.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("现有变量更新成功");
    }

    [Fact]
    public void GetVariable_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "x";
        var expectedValue = 10;
        condition.SetVariable(key, expectedValue);

        // Act
        var result = condition.GetVariable(key);

        // Assert
        result.Should().Be(expectedValue);
        Log("变量获取成功");
    }

    [Fact]
    public void GetVariable_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "nonExistingKey";

        // Act
        var result = condition.GetVariable(key);

        // Assert
        result.Should().BeNull();
        Log("不存在变量返回null");
    }

    [Fact]
    public void GetVariable_WithNullKey_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression();
        string key = null;

        // Act & Assert
        var action = () => condition.GetVariable(key);
        action.Should().Throw<ArgumentNullException>();
        LogError("空键变量获取失败");
    }

    [Fact]
    public void GetVariable_WithEmptyKey_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "";

        // Act & Assert
        var action = () => condition.GetVariable(key);
        action.Should().Throw<ArgumentException>();
        LogError("空键变量获取失败");
    }

    [Fact]
    public void RemoveVariable_WithExistingKey_ShouldRemoveVariable()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "x";
        condition.SetVariable(key, 10);
        var originalUpdatedAt = condition.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        var result = condition.RemoveVariable(key);

        // Assert
        result.Should().BeTrue();
        condition.Variables.Should().NotContainKey(key);
        condition.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("变量移除成功");
    }

    [Fact]
    public void RemoveVariable_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression();
        var key = "nonExistingKey";

        // Act
        var result = condition.RemoveVariable(key);

        // Assert
        result.Should().BeFalse();
        Log("不存在变量移除返回false");
    }

    [Fact]
    public void ClearVariables_ShouldRemoveAllVariables()
    {
        // Arrange
        var condition = new ConditionExpression();
        condition.SetVariable("x", 10);
        condition.SetVariable("y", 20);
        var originalUpdatedAt = condition.UpdatedAt;
        System.Threading.Thread.Sleep(10); // 确保时间差

        // Act
        condition.ClearVariables();

        // Assert
        condition.Variables.Should().BeEmpty();
        condition.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        Log("变量清空成功");
    }

    [Fact]
    public void ClearVariables_WithNoVariables_ShouldDoNothing()
    {
        // Arrange
        var condition = new ConditionExpression();
        var originalUpdatedAt = condition.UpdatedAt;

        // Act
        condition.ClearVariables();

        // Assert
        condition.Variables.Should().BeEmpty();
        condition.UpdatedAt.Should().Be(originalUpdatedAt);
        Log("无变量时清空无影响");
    }

    [Fact]
    public void Evaluate_WithValidExpressionAndVariables_ShouldReturnTrue()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10"
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeTrue();
        Log("条件评估成功：true");
    }

    [Fact]
    public void Evaluate_WithValidExpressionAndVariables_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10"
        };
        condition.SetVariable("x", 5);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeFalse();
        Log("条件评估成功：false");
    }

    [Fact]
    public void Evaluate_WithComplexExpression_ShouldReturnCorrectResult()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10 && y < 20 || z == 30"
        };
        condition.SetVariable("x", 15);
        condition.SetVariable("y", 15);
        condition.SetVariable("z", 25);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeTrue();
        Log("复杂条件评估成功");
    }

    [Fact]
    public void Evaluate_WithNullExpression_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = null
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeFalse();
        Log("空表达式评估返回false");
    }

    [Fact]
    public void Evaluate_WithEmptyExpression_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = ""
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeFalse();
        Log("空表达式评估返回false");
    }

    [Fact]
    public void Evaluate_WithInvalidExpression_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "invalid expression syntax"
        };
        condition.SetVariable("x", 15);

        // Act & Assert
        var action = () => condition.Evaluate();
        action.Should().Throw<InvalidOperationException>();
        LogError("无效表达式评估失败");
    }

    [Fact]
    public void Evaluate_WithMissingVariable_ShouldThrowException()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10"
        };

        // Act & Assert
        var action = () => condition.Evaluate();
        action.Should().Throw<InvalidOperationException>();
        LogError("缺失变量评估失败");
    }

    [Fact]
    public void Evaluate_WithDisabledCondition_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10",
            IsEnabled = false
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeFalse();
        Log("禁用条件评估返回false");
    }

    [Fact]
    public void Evaluate_WithStringVariables_ShouldReturnCorrectResult()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "name == \"John\" && age > 20"
        };
        condition.SetVariable("name", "John");
        condition.SetVariable("age", 25);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeTrue();
        Log("字符串变量评估成功");
    }

    [Fact]
    public void Evaluate_WithBooleanVariables_ShouldReturnCorrectResult()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "isEnabled && !isDisabled"
        };
        condition.SetVariable("isEnabled", true);
        condition.SetVariable("isDisabled", false);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeTrue();
        Log("布尔变量评估成功");
    }

    [Fact]
    public void Evaluate_WithMathematicalExpression_ShouldReturnCorrectResult()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "(x + y) * 2 > 100"
        };
        condition.SetVariable("x", 30);
        condition.SetVariable("y", 25);

        // Act
        var result = condition.Evaluate();

        // Assert
        result.Should().BeTrue();
        Log("数学表达式评估成功");
    }

    [Fact]
    public void TryEvaluate_WithValidExpression_ShouldReturnTrueAndResult()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10"
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.TryEvaluate(out var evaluationResult);

        // Assert
        result.Should().BeTrue();
        evaluationResult.Should().BeTrue();
        Log("条件尝试评估成功");
    }

    [Fact]
    public void TryEvaluate_WithInvalidExpression_ShouldReturnFalseAndFalse()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "invalid expression syntax"
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.TryEvaluate(out var evaluationResult);

        // Assert
        result.Should().BeFalse();
        evaluationResult.Should().BeFalse();
        Log("无效条件尝试评估返回false");
    }

    [Fact]
    public void GetVariableNames_ShouldReturnAllVariableNames()
    {
        // Arrange
        var condition = new ConditionExpression();
        condition.SetVariable("x", 10);
        condition.SetVariable("y", 20);
        condition.SetVariable("z", 30);

        // Act
        var names = condition.GetVariableNames();

        // Assert
        names.Should().BeEquivalentTo(new[] { "x", "y", "z" });
        Log("变量名获取成功");
    }

    [Fact]
    public void GetVariableNames_WithNoVariables_ShouldReturnEmptyList()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        var names = condition.GetVariableNames();

        // Assert
        names.Should().BeEmpty();
        Log("无变量时返回空列表");
    }

    [Fact]
    public void HasVariable_WithExistingVariable_ShouldReturnTrue()
    {
        // Arrange
        var condition = new ConditionExpression();
        condition.SetVariable("x", 10);

        // Act
        var result = condition.HasVariable("x");

        // Assert
        result.Should().BeTrue();
        Log("包含变量检查成功");
    }

    [Fact]
    public void HasVariable_WithNonExistingVariable_ShouldReturnFalse()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        var result = condition.HasVariable("x");

        // Assert
        result.Should().BeFalse();
        Log("不包含变量检查成功");
    }

    [Fact]
    public void GetVariableCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var condition = new ConditionExpression();
        condition.SetVariable("x", 10);
        condition.SetVariable("y", 20);
        condition.SetVariable("z", 30);

        // Act
        var count = condition.GetVariableCount();

        // Assert
        count.Should().Be(3);
        Log("变量计数正确");
    }

    [Fact]
    public void GetVariableCount_WithNoVariables_ShouldReturnZero()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        var count = condition.GetVariableCount();

        // Assert
        count.Should().Be(0);
        Log("无变量时计数为0");
    }

    [Fact]
    public void Clone_ShouldCreateCopy()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10",
            Description = "Test condition",
            IsEnabled = true
        };
        condition.SetVariable("x", 15);

        // Act
        var clonedCondition = condition.Clone();

        // Assert
        clonedCondition.Should().NotBeNull();
        clonedCondition.Should().NotBeSameAs(condition);
        clonedCondition.Id.Should().NotBe(condition.Id);
        clonedCondition.Expression.Should().Be(condition.Expression);
        clonedCondition.Description.Should().Be(condition.Description);
        clonedCondition.IsEnabled.Should().Be(condition.IsEnabled);
        clonedCondition.Variables.Should().Equal(condition.Variables);
        Log("条件克隆成功");
    }

    [Fact]
    public void ToString_ShouldReturnExpression()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10"
        };

        // Act
        var result = condition.ToString();

        // Assert
        result.Should().Be("x > 10");
        Log("条件字符串表示成功");
    }

    [Fact]
    public void ToString_WithNoExpression_ShouldReturnConditionId()
    {
        // Arrange
        var condition = new ConditionExpression();

        // Act
        var result = condition.ToString();

        // Assert
        result.Should().Be(condition.Id.ToString());
        Log("无表达式字符串表示成功");
    }

    [Fact]
    public void Validate_WithValidCondition_ShouldReturnSuccess()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10",
            IsEnabled = true
        };
        condition.SetVariable("x", 15);

        // Act
        var result = condition.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("有效条件验证成功");
    }

    [Fact]
    public void Validate_WithInvalidCondition_ShouldReturnFailure()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "",
            IsEnabled = true
        };

        // Act
        var result = condition.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("expression"));
        LogError("无效条件验证失败");
    }

    [Fact]
    public void Validate_WithDisabledCondition_ShouldReturnSuccess()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "",
            IsEnabled = false
        };

        // Act
        var result = condition.Validate();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        Log("禁用条件验证成功");
    }

    [Fact]
    public void GetRequiredVariables_ShouldReturnRequiredVariables()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "x > 10 && y < 20 || z == 30"
        };

        // Act
        var requiredVariables = condition.GetRequiredVariables();

        // Assert
        requiredVariables.Should().BeEquivalentTo(new[] { "x", "y", "z" });
        Log("必需变量获取成功");
    }

    [Fact]
    public void GetRequiredVariables_WithNoExpression_ShouldReturnEmptyList()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = null
        };

        // Act
        var requiredVariables = condition.GetRequiredVariables();

        // Assert
        requiredVariables.Should().BeEmpty();
        Log("无表达式时返回空列表");
    }

    [Fact]
    public void GetRequiredVariables_WithConstantExpression_ShouldReturnEmptyList()
    {
        // Arrange
        var condition = new ConditionExpression
        {
            Expression = "true"
        };

        // Act
        var requiredVariables = condition.GetRequiredVariables();

        // Assert
        requiredVariables.Should().BeEmpty();
        Log("常量表达式返回空列表");
    }
}