using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Tests.UnitTests.Entities
{
    /// <summary>
    /// GameAction实体单元测试
    /// 测试游戏动作的所有业务规则和不变性
    /// </summary>
    public class GameActionTests : TestBase
    {
        #region Delay构造函数测试

        [Fact]
        public void DelayConstructor_WithValidParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.Delay;
            var delay = 1000;
            var description = "Test delay action";

            // Act
            var action = new GameAction(actionId, type, delay, description);

            // Assert
            ShouldBeValidGameAction(action);
            action.Id.Should().Be(actionId);
            action.Type.Should().Be(type);
            action.Delay.Should().Be(delay);
            action.Description.Should().Be(description);
            action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            action.IsDelayAction.Should().BeTrue();
            action.IsKeyboardAction.Should().BeFalse();
            action.IsMouseAction.Should().BeFalse();
        }

        [Fact]
        public void DelayConstructor_WithDefaultParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.Delay;

            // Act
            var action = new GameAction(actionId, type);

            // Assert
            action.Delay.Should().Be(0);
            action.Description.Should().BeEmpty();
        }

        #endregion

        #region Keyboard构造函数测试

        [Fact]
        public void KeyboardConstructor_WithValidParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.KeyDown;
            var key = KeyCode.A;
            var delay = 100;
            var description = "Test keyboard action";

            // Act
            var action = new GameAction(actionId, type, key, delay, description);

            // Assert
            ShouldBeValidGameAction(action);
            action.Id.Should().Be(actionId);
            action.Type.Should().Be(type);
            action.Key.Should().Be(key);
            action.Delay.Should().Be(delay);
            action.Description.Should().Be(description);
            action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            action.IsKeyboardAction.Should().BeTrue();
            action.IsMouseAction.Should().BeFalse();
            action.IsDelayAction.Should().BeFalse();
        }

        [Fact]
        public void KeyboardConstructor_WithDefaultParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.KeyDown;
            var key = KeyCode.A;

            // Act
            var action = new GameAction(actionId, type, key);

            // Assert
            action.Delay.Should().Be(0);
            action.Description.Should().BeEmpty();
        }

        [Theory]
        [InlineData(ActionType.KeyDown)]
        [InlineData(ActionType.KeyUp)]
        public void KeyboardConstructor_WithValidKeyActionTypes_ShouldCreateGameAction(ActionType validType)
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var key = KeyCode.A;

            // Act
            var action = new GameAction(actionId, validType, key);

            // Assert
            action.Type.Should().Be(validType);
            action.Key.Should().Be(key);
            action.IsKeyboardAction.Should().BeTrue();
        }

        #endregion

        #region Mouse构造函数测试

        [Fact]
        public void MouseConstructor_WithValidParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.MouseDown;
            var button = MouseButton.Left;
            var x = 100;
            var y = 200;
            var delay = 50;
            var description = "Test mouse action";

            // Act
            var action = new GameAction(actionId, type, button, x, y, delay, description);

            // Assert
            ShouldBeValidGameAction(action);
            action.Id.Should().Be(actionId);
            action.Type.Should().Be(type);
            action.Button.Should().Be(button);
            action.X.Should().Be(x);
            action.Y.Should().Be(y);
            action.Delay.Should().Be(delay);
            action.Description.Should().Be(description);
            action.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            action.IsMouseAction.Should().BeTrue();
            action.IsKeyboardAction.Should().BeFalse();
            action.IsDelayAction.Should().BeFalse();
        }

        [Fact]
        public void MouseConstructor_WithDefaultParameters_ShouldCreateGameAction()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.MouseDown;
            var button = MouseButton.Left;
            var x = 100;
            var y = 200;

            // Act
            var action = new GameAction(actionId, type, button, x, y);

            // Assert
            action.Delay.Should().Be(0);
            action.Description.Should().BeEmpty();
        }

        [Theory]
        [InlineData(ActionType.MouseDown)]
        [InlineData(ActionType.MouseUp)]
        [InlineData(ActionType.MouseMove)]
        public void MouseConstructor_WithValidMouseActionTypes_ShouldCreateGameAction(ActionType validType)
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var button = MouseButton.Left;
            var x = 100;
            var y = 200;

            // Act
            var action = new GameAction(actionId, validType, button, x, y);

            // Assert
            action.Type.Should().Be(validType);
            action.Button.Should().Be(button);
            action.IsMouseAction.Should().BeTrue();
        }

        #endregion

        #region UpdatePosition方法测试

        [Fact]
        public void UpdatePosition_WithValidMouseAction_ShouldUpdatePosition()
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 100, 200, 50, "Test");
            var originalTimestamp = action.Timestamp;
            var newX = 300;
            var newY = 400;

            // Act
            action.UpdatePosition(newX, newY);

            // Assert
            action.X.Should().Be(newX);
            action.Y.Should().Be(newY);
            action.Timestamp.Should().BeAfter(originalTimestamp);
        }

        [Theory]
        [InlineData(ActionType.KeyDown)]
        [InlineData(ActionType.KeyUp)]
        [InlineData(ActionType.Delay)]
        public void UpdatePosition_WithNonMouseAction_ShouldThrowBusinessRuleViolationException(ActionType actionType)
        {
            // Arrange
            GameAction action;
            if (actionType == ActionType.Delay)
            {
                action = new GameAction(Guid.NewGuid(), ActionType.Delay, 100, "Test");
            }
            else
            {
                action = new GameAction(Guid.NewGuid(), actionType, KeyCode.A, 100, "Test");
            }

            // Act & Assert
            var updateAction = () => action.UpdatePosition(100, 200);
            updateAction.Should().Throw<BusinessRuleViolationException>()
                .WithMessage("Cannot update position for non-mouse actions.");
        }

        [Theory]
        [InlineData(ActionType.MouseDown)]
        [InlineData(ActionType.MouseUp)]
        [InlineData(ActionType.MouseMove)]
        public void UpdatePosition_WithValidMouseActionTypes_ShouldUpdatePosition(ActionType actionType)
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), actionType, MouseButton.Left, 100, 200, 50, "Test");
            var newX = 300;
            var newY = 400;

            // Act
            action.UpdatePosition(newX, newY);

            // Assert
            action.X.Should().Be(newX);
            action.Y.Should().Be(newY);
        }

        #endregion

        #region UpdateDelay方法测试

        [Fact]
        public void UpdateDelay_WithValidDelay_ShouldUpdateDelay()
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test");
            var originalTimestamp = action.Timestamp;
            var newDelay = 200;

            // Act
            action.UpdateDelay(newDelay);

            // Assert
            action.Delay.Should().Be(newDelay);
            action.Timestamp.Should().BeAfter(originalTimestamp);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-1000)]
        public void UpdateDelay_WithNegativeDelay_ShouldThrowValidationException(int negativeDelay)
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test");

            // Act & Assert
            var updateAction = () => action.UpdateDelay(negativeDelay);
            updateAction.Should().Throw<ValidationException>()
                .WithMessage("Delay cannot be negative.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(60000)] // 1分钟
        public void UpdateDelay_WithValidDelays_ShouldUpdateDelay(int validDelay)
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test");

            // Act
            action.UpdateDelay(validDelay);

            // Assert
            action.Delay.Should().Be(validDelay);
        }

        #endregion

        #region UpdateDescription方法测试

        [Fact]
        public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Original description");
            var newDescription = "Updated description";

            // Act
            action.UpdateDescription(newDescription);

            // Assert
            action.Description.Should().Be(newDescription);
        }

        [Fact]
        public void UpdateDescription_WithNullDescription_ShouldSetToEmpty()
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Original description");

            // Act
            action.UpdateDescription(null);

            // Assert
            action.Description.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("New description")]
        [InlineData("Very long description with lots of text to test edge cases")]
        public void UpdateDescription_WithVariousDescriptions_ShouldUpdateDescription(string description)
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Original description");

            // Act
            action.UpdateDescription(description);

            // Assert
            action.Description.Should().Be(description ?? string.Empty);
        }

        #endregion

        #region 属性测试

        [Fact]
        public void IsKeyboardAction_ShouldReturnCorrectValue()
        {
            // Arrange & Act & Assert
            var keyDownAction = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A);
            keyDownAction.IsKeyboardAction.Should().BeTrue();

            var keyUpAction = new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A);
            keyUpAction.IsKeyboardAction.Should().BeTrue();

            var mouseAction = new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200);
            mouseAction.IsKeyboardAction.Should().BeFalse();

            var delayAction = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            delayAction.IsKeyboardAction.Should().BeFalse();
        }

        [Fact]
        public void IsMouseAction_ShouldReturnCorrectValue()
        {
            // Arrange & Act & Assert
            var mouseDownAction = new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200);
            mouseDownAction.IsMouseAction.Should().BeTrue();

            var mouseUpAction = new GameAction(Guid.NewGuid(), ActionType.MouseUp, MouseButton.Left, 100, 200);
            mouseUpAction.IsMouseAction.Should().BeTrue();

            var mouseMoveAction = new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 100, 200);
            mouseMoveAction.IsMouseAction.Should().BeTrue();

            var keyAction = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A);
            keyAction.IsMouseAction.Should().BeFalse();

            var delayAction = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            delayAction.IsMouseAction.Should().BeFalse();
        }

        [Fact]
        public void IsDelayAction_ShouldReturnCorrectValue()
        {
            // Arrange & Act & Assert
            var delayAction = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            delayAction.IsDelayAction.Should().BeTrue();

            var keyAction = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A);
            keyAction.IsDelayAction.Should().BeFalse();

            var mouseAction = new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200);
            mouseAction.IsDelayAction.Should().BeFalse();
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void GameAction_ShouldHandleExtremeCoordinateValues()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.MouseMove;
            var button = MouseButton.Left;

            // Act & Assert
            var action1 = new GameAction(actionId, type, button, int.MinValue, int.MinValue);
            action1.X.Should().Be(int.MinValue);
            action1.Y.Should().Be(int.MinValue);

            var action2 = new GameAction(actionId, type, button, int.MaxValue, int.MaxValue);
            action2.X.Should().Be(int.MaxValue);
            action2.Y.Should().Be(int.MaxValue);

            var action3 = new GameAction(actionId, type, button, 0, 0);
            action3.X.Should().Be(0);
            action3.Y.Should().Be(0);
        }

        [Fact]
        public void GameAction_ShouldHandleLargeDelayValues()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var type = ActionType.Delay;

            // Act & Assert
            var action1 = new GameAction(actionId, type, int.MaxValue);
            action1.Delay.Should().Be(int.MaxValue);

            var action2 = new GameAction(actionId, type, 0);
            action2.Delay.Should().Be(0);

            var action3 = new GameAction(actionId, type, 60000); // 1分钟
            action3.Delay.Should().Be(60000);
        }

        [Fact]
        public void GameAction_ShouldHandleAllKeyCodes()
        {
            // Arrange
            var actionId = Guid.NewGuid();

            // Act & Assert
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                var action = new GameAction(actionId, ActionType.KeyDown, keyCode);
                action.Key.Should().Be(keyCode);
            }
        }

        [Fact]
        public void GameAction_ShouldHandleAllMouseButtons()
        {
            // Arrange
            var actionId = Guid.NewGuid();

            // Act & Assert
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var action = new GameAction(actionId, ActionType.MouseDown, button, 100, 200);
                action.Button.Should().Be(button);
            }
        }

        [Fact]
        public void GameAction_ShouldHandleAllActionTypes()
        {
            // Arrange
            var actionId = Guid.NewGuid();

            // Act & Assert
            foreach (ActionType actionType in Enum.GetValues(typeof(ActionType)))
            {
                GameAction action;
                switch (actionType)
                {
                    case ActionType.KeyDown:
                    case ActionType.KeyUp:
                        action = new GameAction(actionId, actionType, KeyCode.A);
                        break;
                    case ActionType.MouseDown:
                    case ActionType.MouseUp:
                    case ActionType.MouseMove:
                        action = new GameAction(actionId, actionType, MouseButton.Left, 100, 200);
                        break;
                    case ActionType.Delay:
                        action = new GameAction(actionId, actionType, 100);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported action type: {actionType}");
                }
                
                action.Type.Should().Be(actionType);
            }
        }

        #endregion

        #region 实体行为测试

        [Fact]
        public void GameAction_ShouldUpdateTimestampOnModification()
        {
            // Arrange
            var action = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Test");
            var originalTimestamp = action.Timestamp;

            // 等待一小段时间确保时间戳不同
            System.Threading.Thread.Sleep(10);

            // Act
            action.UpdateDelay(200);

            // Assert
            action.Timestamp.Should().BeAfter(originalTimestamp);
        }

        [Fact]
        public void GameAction_ShouldMaintainIdImmutability()
        {
            // Arrange
            var actionId = Guid.NewGuid();
            var action = new GameAction(actionId, ActionType.KeyDown, KeyCode.A, 100, "Test");

            // Act & Assert
            action.Id.Should().Be(actionId);
            // ID不能被修改，这是实体不变性的要求
        }

        [Fact]
        public void GameAction_ShouldValidateTypeSpecificConstraints()
        {
            // Arrange & Act & Assert
            // 键盘动作应该有Key属性
            var keyboardAction = new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A);
            keyboardAction.Key.Should().Be(KeyCode.A);

            // 鼠标动作应该有Button、X、Y属性
            var mouseAction = new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 200);
            mouseAction.Button.Should().Be(MouseButton.Left);
            mouseAction.X.Should().Be(100);
            mouseAction.Y.Should().Be(200);

            // 延迟动作应该只有Delay属性
            var delayAction = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            delayAction.Delay.Should().Be(100);
        }

        #endregion

        #region 构造函数参数验证测试

        [Fact]
        public void Constructors_WithEmptyGuid_ShouldCreateGameAction()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            var action1 = new GameAction(emptyGuid, ActionType.Delay, 100);
            action1.Id.Should().Be(emptyGuid);

            var action2 = new GameAction(emptyGuid, ActionType.KeyDown, KeyCode.A, 100);
            action2.Id.Should().Be(emptyGuid);

            var action3 = new GameAction(emptyGuid, ActionType.MouseDown, MouseButton.Left, 100, 200, 100);
            action3.Id.Should().Be(emptyGuid);
        }

        [Fact]
        public void Constructors_ShouldSetTimestampToCurrentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
            var actionId = Guid.NewGuid();

            // Act
            var action = new GameAction(actionId, ActionType.KeyDown, KeyCode.A, 100, "Test");
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            action.Timestamp.Should().BeAfter(beforeCreation);
            action.Timestamp.Should().BeBefore(afterCreation);
        }

        #endregion
    }
}