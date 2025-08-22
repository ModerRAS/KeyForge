using Xunit;
using FluentAssertions;
using KeyForge.Domain.Entities;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain.Entities;

/// <summary>
/// 游戏动作实体单元测试
/// 原本实现：复杂的动作行为测试
/// 简化实现：核心功能测试
/// </summary>
public class GameActionTests : TestBase
{
    public GameActionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateGameAction_WithBasicConstructor_ShouldCreateAction()
    {
        // Arrange
        var actionId = Guid.NewGuid();
        var actionType = ActionType.Delay;
        var delay = 100;
        
        // Act
        var action = new GameAction(actionId, actionType, delay);
        
        // Assert
        action.Should().NotBeNull();
        action.Id.Should().Be(actionId);
        action.Type.Should().Be(actionType);
        action.Delay.Should().Be(delay);
        action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        action.IsKeyboardAction.Should().BeFalse();
        action.IsMouseAction.Should().BeFalse();
        action.IsDelayAction.Should().BeTrue();
        Log($"创建基础动作成功: {action.Type}");
    }

    [Fact]
    public void CreateGameAction_WithKeyboardConstructor_ShouldCreateAction()
    {
        // Arrange
        var actionId = Guid.NewGuid();
        var actionType = ActionType.KeyDown;
        var keyCode = KeyCode.A;
        var delay = 50;
        
        // Act
        var action = new GameAction(actionId, actionType, keyCode, delay);
        
        // Assert
        action.Should().NotBeNull();
        action.Id.Should().Be(actionId);
        action.Type.Should().Be(actionType);
        action.Key.Should().Be(keyCode);
        action.Delay.Should().Be(delay);
        action.IsKeyboardAction.Should().BeTrue();
        action.IsMouseAction.Should().BeFalse();
        action.IsDelayAction.Should().BeFalse();
        Log($"创建键盘动作成功: {action.Type} - {action.Key}");
    }

    [Fact]
    public void CreateGameAction_WithMouseConstructor_ShouldCreateAction()
    {
        // Arrange
        var actionId = Guid.NewGuid();
        var actionType = ActionType.MouseDown;
        var mouseButton = MouseButton.Left;
        var x = 100;
        var y = 200;
        var delay = 75;
        
        // Act
        var action = new GameAction(actionId, actionType, mouseButton, x, y, delay);
        
        // Assert
        action.Should().NotBeNull();
        action.Id.Should().Be(actionId);
        action.Type.Should().Be(actionType);
        action.Button.Should().Be(mouseButton);
        action.X.Should().Be(x);
        action.Y.Should().Be(y);
        action.Delay.Should().Be(delay);
        action.IsKeyboardAction.Should().BeFalse();
        action.IsMouseAction.Should().BeTrue();
        action.IsDelayAction.Should().BeFalse();
        Log($"创建鼠标动作成功: {action.Type} - {action.Button} at ({action.X}, {action.Y})");
    }

    [Fact]
    public void UpdatePosition_WithValidMouseAction_ShouldUpdateCoordinates()
    {
        // Arrange
        var action = TestFixtures.CreateMouseAction();
        var newX = 500;
        var newY = 600;
        
        // Act
        action.UpdatePosition(newX, newY);
        
        // Assert
        action.X.Should().Be(newX);
        action.Y.Should().Be(newY);
        action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新鼠标位置成功: ({newX}, {newY})");
    }

    [Fact]
    public void UpdatePosition_WithNonMouseAction_ShouldThrowException()
    {
        // Arrange
        var action = TestFixtures.CreateValidGameAction();
        
        // Act & Assert
        var updateAction = () => action.UpdatePosition(100, 200);
        updateAction.Should().Throw<BusinessRuleViolationException>();
        Log($"验证非鼠标动作更新位置失败");
    }

    [Fact]
    public void UpdateDelay_WithValidDelay_ShouldUpdateDelay()
    {
        // Arrange
        var action = TestFixtures.CreateValidGameAction();
        var newDelay = 500;
        
        // Act
        action.UpdateDelay(newDelay);
        
        // Assert
        action.Delay.Should().Be(newDelay);
        action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新延迟时间成功: {newDelay}ms");
    }

    [Fact]
    public void UpdateDelay_WithNegativeDelay_ShouldThrowException()
    {
        // Arrange
        var action = TestFixtures.CreateValidGameAction();
        
        // Act & Assert
        var updateAction = () => action.UpdateDelay(-100);
        updateAction.Should().Throw<ValidationException>();
        Log($"验证负延迟时间失败");
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
    {
        // Arrange
        var action = TestFixtures.CreateValidGameAction();
        var newDescription = "Updated action description";
        
        // Act
        action.UpdateDescription(newDescription);
        
        // Assert
        action.Description.Should().Be(newDescription);
        action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新动作描述成功: {newDescription}");
    }

    [Theory]
    [InlineData(ActionType.KeyDown, true)]
    [InlineData(ActionType.KeyUp, true)]
    [InlineData(ActionType.MouseDown, false)]
    [InlineData(ActionType.MouseUp, false)]
    [InlineData(ActionType.MouseMove, false)]
    [InlineData(ActionType.Delay, false)]
    public void IsKeyboardAction_ShouldReturnCorrectValue(ActionType actionType, bool expected)
    {
        // Arrange
        var action = new GameAction(Guid.NewGuid(), actionType, KeyCode.A, 100);
        
        // Act & Assert
        action.IsKeyboardAction.Should().Be(expected);
        Log($"验证键盘动作类型: {actionType} = {expected}");
    }

    [Theory]
    [InlineData(ActionType.KeyDown, false)]
    [InlineData(ActionType.KeyUp, false)]
    [InlineData(ActionType.MouseDown, true)]
    [InlineData(ActionType.MouseUp, true)]
    [InlineData(ActionType.MouseMove, true)]
    [InlineData(ActionType.Delay, false)]
    public void IsMouseAction_ShouldReturnCorrectValue(ActionType actionType, bool expected)
    {
        // Arrange
        var action = new GameAction(Guid.NewGuid(), actionType, MouseButton.Left, 100, 200, 100);
        
        // Act & Assert
        action.IsMouseAction.Should().Be(expected);
        Log($"验证鼠标动作类型: {actionType} = {expected}");
    }

    [Theory]
    [InlineData(ActionType.KeyDown, false)]
    [InlineData(ActionType.KeyUp, false)]
    [InlineData(ActionType.MouseDown, false)]
    [InlineData(ActionType.MouseUp, false)]
    [InlineData(ActionType.MouseMove, false)]
    [InlineData(ActionType.Delay, true)]
    public void IsDelayAction_ShouldReturnCorrectValue(ActionType actionType, bool expected)
    {
        // Arrange
        var action = new GameAction(Guid.NewGuid(), actionType, 100);
        
        // Act & Assert
        action.IsDelayAction.Should().Be(expected);
        Log($"验证延迟动作类型: {actionType} = {expected}");
    }

    [Fact]
    public void GameAction_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var actionId = Guid.NewGuid();
        var action1 = new GameAction(actionId, ActionType.KeyDown, KeyCode.A, 100);
        var action2 = new GameAction(actionId, ActionType.KeyDown, KeyCode.B, 200);
        var action3 = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100);
        
        // Act & Assert
        action1.Should().Be(action2); // Same ID
        action1.Should().NotBe(action3); // Different ID
        (action1 == action2).Should().BeTrue();
        (action1 != action3).Should().BeTrue();
        Log($"验证动作相等性: 相同ID={action1.Id == action2.Id}, 不同ID={action1.Id != action3.Id}");
    }
}