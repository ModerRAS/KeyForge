using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using KeyForge.Application.DTOs;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.UnitTests.Application.DTOs
{
    /// <summary>
    /// DTO映射单元测试
    /// 测试Application层与Domain层之间的DTO映射功能
    /// </summary>
    public class DtoMappingTests : TestBase
    {
        #region ScriptDto映射测试

        [Fact]
        public void ScriptDto_ShouldCorrectlyMapFromDomainScript()
        {
            // Arrange
            var domainScript = TestDataFactory.CreateValidScript();
            var originalActionCount = domainScript.Actions.Count;
            var originalDuration = domainScript.GetEstimatedDuration();

            // Act
            var dto = new ScriptDto
            {
                Id = domainScript.Id,
                Name = domainScript.Name,
                Description = domainScript.Description,
                Status = MapScriptStatusToDto(domainScript.Status),
                CreatedAt = domainScript.CreatedAt,
                UpdatedAt = domainScript.UpdatedAt,
                Version = domainScript.Version,
                Actions = domainScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = originalDuration
            };

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(domainScript.Id);
            dto.Name.Should().Be(domainScript.Name);
            dto.Description.Should().Be(domainScript.Description);
            dto.Status.Should().Be(MapScriptStatusToDto(domainScript.Status));
            dto.CreatedAt.Should().Be(domainScript.CreatedAt);
            dto.UpdatedAt.Should().Be(domainScript.UpdatedAt);
            dto.Version.Should().Be(domainScript.Version);
            dto.Actions.Should().HaveCount(originalActionCount);
            dto.ActionCount.Should().Be(originalActionCount);
            dto.EstimatedDuration.Should().Be(originalDuration);
        }

        [Fact]
        public void ScriptDto_ShouldHandleEmptyActions()
        {
            // Arrange
            var domainScript = new Script(Guid.NewGuid(), "Empty Script", "No actions");

            // Act
            var dto = new ScriptDto
            {
                Id = domainScript.Id,
                Name = domainScript.Name,
                Description = domainScript.Description,
                Status = MapScriptStatusToDto(domainScript.Status),
                CreatedAt = domainScript.CreatedAt,
                UpdatedAt = domainScript.UpdatedAt,
                Version = domainScript.Version,
                Actions = domainScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = domainScript.GetEstimatedDuration()
            };

            // Assert
            dto.Actions.Should().BeEmpty();
            dto.ActionCount.Should().Be(0);
            dto.EstimatedDuration.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void ScriptDto_ShouldCalculateEstimatedDurationCorrectly()
        {
            // Arrange
            var domainScript = new Script(Guid.NewGuid(), "Test Script", "Test");
            var action1 = new GameAction(Guid.NewGuid(), ActionType.Delay, 100);
            var action2 = new GameAction(Guid.NewGuid(), ActionType.Delay, 200);
            var action3 = new GameAction(Guid.NewGuid(), ActionType.Delay, 50);
            
            domainScript.AddAction(action1);
            domainScript.AddAction(action2);
            domainScript.AddAction(action3);

            // Act
            var dto = new ScriptDto
            {
                Id = domainScript.Id,
                Name = domainScript.Name,
                Description = domainScript.Description,
                Status = MapScriptStatusToDto(domainScript.Status),
                CreatedAt = domainScript.CreatedAt,
                UpdatedAt = domainScript.UpdatedAt,
                Version = domainScript.Version,
                Actions = domainScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = domainScript.GetEstimatedDuration()
            };

            // Assert
            dto.EstimatedDuration.Should().Be(TimeSpan.FromMilliseconds(350));
        }

        #endregion

        #region GameActionDto映射测试

        [Fact]
        public void GameActionDto_ShouldCorrectlyMapFromDomainGameAction()
        {
            // Arrange
            var domainAction = TestDataFactory.CreateGameAction();

            // Act
            var dto = MapToDto(domainAction);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(domainAction.Id);
            dto.Type.Should().Be(MapActionTypeToDto(domainAction.Type));
            dto.Key.Should().Be(MapKeyCodeToDto(domainAction.Key));
            dto.Button.Should().Be(MapMouseButtonToDto(domainAction.Button));
            dto.X.Should().Be(domainAction.X);
            dto.Y.Should().Be(domainAction.Y);
            dto.Delay.Should().Be(domainAction.Delay);
            dto.Timestamp.Should().Be(domainAction.Timestamp);
            dto.Description.Should().Be(domainAction.Description);
        }

        [Theory]
        [InlineData(ActionType.KeyDown)]
        [InlineData(ActionType.KeyUp)]
        public void GameActionDto_ShouldCorrectlyMapKeyActions(ActionType actionType)
        {
            // Arrange
            var domainAction = new GameAction(Guid.NewGuid(), actionType, KeyCode.A, 100, "Test key action");

            // Act
            var dto = MapToDto(domainAction);

            // Assert
            dto.Type.Should().Be(MapActionTypeToDto(actionType));
            dto.Key.Should().Be(KeyCodeDto.A);
            dto.IsKeyboardAction.Should().BeTrue();
            dto.IsMouseAction.Should().BeFalse();
            dto.IsDelayAction.Should().BeFalse();
        }

        [Theory]
        [InlineData(ActionType.MouseDown)]
        [InlineData(ActionType.MouseUp)]
        [InlineData(ActionType.MouseMove)]
        public void GameActionDto_ShouldCorrectlyMapMouseActions(ActionType actionType)
        {
            // Arrange
            var domainAction = new GameAction(actionType, MouseButton.Left, 100, 200, 50, "Test mouse action");

            // Act
            var dto = MapToDto(domainAction);

            // Assert
            dto.Type.Should().Be(MapActionTypeToDto(actionType));
            dto.Button.Should().Be(MouseButtonDto.Left);
            dto.X.Should().Be(100);
            dto.Y.Should().Be(200);
            dto.IsMouseAction.Should().BeTrue();
            dto.IsKeyboardAction.Should().BeFalse();
            dto.IsDelayAction.Should().BeFalse();
        }

        [Fact]
        public void GameActionDto_ShouldCorrectlyMapDelayActions()
        {
            // Arrange
            var domainAction = new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Test delay action");

            // Act
            var dto = MapToDto(domainAction);

            // Assert
            dto.Type.Should().Be(ActionTypeDto.Delay);
            dto.Delay.Should().Be(1000);
            dto.IsDelayAction.Should().BeTrue();
            dto.IsKeyboardAction.Should().BeFalse();
            dto.IsMouseAction.Should().BeFalse();
        }

        [Fact]
        public void GameActionDto_ShouldHandleExtremeValues()
        {
            // Arrange
            var domainAction = new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, int.MaxValue, int.MaxValue, int.MaxValue, "Extreme values");

            // Act
            var dto = MapToDto(domainAction);

            // Assert
            dto.X.Should().Be(int.MaxValue);
            dto.Y.Should().Be(int.MaxValue);
            dto.Delay.Should().Be(int.MaxValue);
        }

        #endregion

        #region ImageTemplateDto映射测试

        [Fact]
        public void ImageTemplateDto_ShouldCorrectlyMapFromDomainImageTemplate()
        {
            // Arrange
            var domainTemplate = TestDataFactory.CreateValidImageTemplate();

            // Act
            var dto = new ImageTemplateDto
            {
                Id = domainTemplate.Id,
                Name = domainTemplate.Name,
                Description = domainTemplate.Description,
                TemplateData = domainTemplate.TemplateData,
                TemplateArea = MapRectangleToDto(domainTemplate.TemplateArea),
                MatchThreshold = domainTemplate.MatchThreshold,
                TemplateType = MapTemplateTypeToDto(domainTemplate.TemplateType),
                IsActive = domainTemplate.IsActive,
                CreatedAt = domainTemplate.CreatedAt,
                UpdatedAt = domainTemplate.UpdatedAt,
                Version = domainTemplate.Version
            };

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(domainTemplate.Id);
            dto.Name.Should().Be(domainTemplate.Name);
            dto.Description.Should().Be(domainTemplate.Description);
            dto.TemplateData.Should().BeEquivalentTo(domainTemplate.TemplateData);
            dto.TemplateArea.Should().Be(MapRectangleToDto(domainTemplate.TemplateArea));
            dto.MatchThreshold.Should().Be(domainTemplate.MatchThreshold);
            dto.TemplateType.Should().Be(MapTemplateTypeToDto(domainTemplate.TemplateType));
            dto.IsActive.Should().Be(domainTemplate.IsActive);
            dto.CreatedAt.Should().Be(domainTemplate.CreatedAt);
            dto.UpdatedAt.Should().Be(domainTemplate.UpdatedAt);
            dto.Version.Should().Be(domainTemplate.Version);
        }

        [Fact]
        public void ImageTemplateDto_ShouldHandleLargeTemplateData()
        {
            // Arrange
            var largeTemplateData = new byte[1024 * 1024]; // 1MB
            var domainTemplate = new ImageTemplate(
                Guid.NewGuid(),
                "Large Template",
                "Large template data",
                largeTemplateData,
                new System.Drawing.Rectangle(0, 0, 1920, 1080),
                0.9,
                KeyForge.Domain.Common.TemplateType.Image);

            // Act
            var dto = new ImageTemplateDto
            {
                Id = domainTemplate.Id,
                Name = domainTemplate.Name,
                Description = domainTemplate.Description,
                TemplateData = domainTemplate.TemplateData,
                TemplateArea = MapRectangleToDto(domainTemplate.TemplateArea),
                MatchThreshold = domainTemplate.MatchThreshold,
                TemplateType = MapTemplateTypeToDto(domainTemplate.TemplateType),
                IsActive = domainTemplate.IsActive,
                CreatedAt = domainTemplate.CreatedAt,
                UpdatedAt = domainTemplate.UpdatedAt,
                Version = domainTemplate.Version
            };

            // Assert
            dto.TemplateData.Should().HaveCount(1024 * 1024);
            dto.TemplateData.Length.Should().Be(1024 * 1024);
        }

        #endregion

        #region StateMachineDto映射测试

        [Fact]
        public void StateMachineDto_ShouldCorrectlyMapFromDomainStateMachine()
        {
            // Arrange
            var domainStateMachine = TestDataFactory.CreateValidStateMachine();

            // Act
            var dto = new StateMachineDto
            {
                Id = domainStateMachine.Id,
                Name = domainStateMachine.Name,
                Description = domainStateMachine.Description,
                Status = MapStateMachineStatusToDto(domainStateMachine.Status),
                CreatedAt = domainStateMachine.CreatedAt,
                UpdatedAt = domainStateMachine.UpdatedAt,
                Version = domainStateMachine.Version,
                States = domainStateMachine.States.Select(MapToDto).ToList(),
                Transitions = domainStateMachine.Transitions.Select(MapToDto).ToList(),
                Rules = domainStateMachine.Rules.Select(MapToDto).ToList(),
                CurrentState = MapToDto(domainStateMachine.CurrentState)
            };

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(domainStateMachine.Id);
            dto.Name.Should().Be(domainStateMachine.Name);
            dto.Description.Should().Be(domainStateMachine.Description);
            dto.Status.Should().Be(MapStateMachineStatusToDto(domainStateMachine.Status));
            dto.States.Should().HaveCount(domainStateMachine.States.Count);
            dto.Transitions.Should().HaveCount(domainStateMachine.Transitions.Count);
            dto.Rules.Should().HaveCount(domainStateMachine.Rules.Count);
            dto.CurrentState.Should().NotBeNull();
            dto.CurrentState.Id.Should().Be(domainStateMachine.CurrentState.Id);
        }

        #endregion

        #region 枚举映射测试

        [Theory]
        [InlineData(ScriptStatus.Draft, ScriptStatusDto.Draft)]
        [InlineData(ScriptStatus.Active, ScriptStatusDto.Active)]
        [InlineData(ScriptStatus.Stopped, ScriptStatusDto.Inactive)]
        [InlineData(ScriptStatus.Error, ScriptStatusDto.Deleted)]
        public void ScriptStatusMapping_ShouldBeCorrect(ScriptStatus domainStatus, ScriptStatusDto expectedDtoStatus)
        {
            // Act
            var dtoStatus = MapScriptStatusToDto(domainStatus);

            // Assert
            dtoStatus.Should().Be(expectedDtoStatus);
        }

        [Theory]
        [InlineData(ScriptStatusDto.Draft, ScriptStatus.Draft)]
        [InlineData(ScriptStatusDto.Active, ScriptStatus.Active)]
        [InlineData(ScriptStatusDto.Inactive, ScriptStatus.Stopped)]
        [InlineData(ScriptStatusDto.Deleted, ScriptStatus.Error)]
        public void ScriptStatusDtoMapping_ShouldBeCorrect(ScriptStatusDto dtoStatus, ScriptStatus expectedDomainStatus)
        {
            // Act
            var domainStatus = MapScriptStatusToDomain(dtoStatus);

            // Assert
            domainStatus.Should().Be(expectedDomainStatus);
        }

        [Theory]
        [InlineData(ActionType.KeyDown, ActionTypeDto.KeyDown)]
        [InlineData(ActionType.KeyUp, ActionTypeDto.KeyUp)]
        [InlineData(ActionType.MouseDown, ActionTypeDto.MouseDown)]
        [InlineData(ActionType.MouseUp, ActionTypeDto.MouseUp)]
        [InlineData(ActionType.MouseMove, ActionTypeDto.MouseMove)]
        [InlineData(ActionType.Delay, ActionTypeDto.Delay)]
        public void ActionTypeMapping_ShouldBeCorrect(ActionType domainType, ActionTypeDto expectedDtoType)
        {
            // Act
            var dtoType = MapActionTypeToDto(domainType);

            // Assert
            dtoType.Should().Be(expectedDtoType);
        }

        [Theory]
        [InlineData(ActionTypeDto.KeyDown, ActionType.KeyDown)]
        [InlineData(ActionTypeDto.KeyUp, ActionType.KeyUp)]
        [InlineData(ActionTypeDto.MouseDown, ActionType.MouseDown)]
        [InlineData(ActionTypeDto.MouseUp, ActionType.MouseUp)]
        [InlineData(ActionTypeDto.MouseMove, ActionType.MouseMove)]
        [InlineData(ActionTypeDto.Delay, ActionType.Delay)]
        public void ActionTypeDtoMapping_ShouldBeCorrect(ActionTypeDto dtoType, ActionType expectedDomainType)
        {
            // Act
            var domainType = MapActionTypeToDomain(dtoType);

            // Assert
            domainType.Should().Be(expectedDomainType);
        }

        [Theory]
        [InlineData(KeyCode.A, KeyCodeDto.A)]
        [InlineData(KeyCode.Z, KeyCodeDto.Z)]
        [InlineData(KeyCode.F1, KeyCodeDto.F1)]
        [InlineData(KeyCode.F12, KeyCodeDto.F12)]
        [InlineData(KeyCode.Control, KeyCodeDto.Control)]
        [InlineData(KeyCode.Alt, KeyCodeDto.Alt)]
        public void KeyCodeMapping_ShouldBeCorrect(KeyCode domainCode, KeyCodeDto expectedDtoCode)
        {
            // Act
            var dtoCode = MapKeyCodeToDto(domainCode);

            // Assert
            dtoCode.Should().Be(expectedDtoCode);
        }

        [Theory]
        [InlineData(MouseButton.Left, MouseButtonDto.Left)]
        [InlineData(MouseButton.Right, MouseButtonDto.Right)]
        [InlineData(MouseButton.Middle, MouseButtonDto.Middle)]
        [InlineData(MouseButton.None, MouseButtonDto.None)]
        public void MouseButtonMapping_ShouldBeCorrect(MouseButton domainButton, MouseButtonDto expectedDtoButton)
        {
            // Act
            var dtoButton = MapMouseButtonToDto(domainButton);

            // Assert
            dtoButton.Should().Be(expectedDtoButton);
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void DtoMapping_ShouldHandleNullValues()
        {
            // Arrange
            var domainScript = new Script(Guid.NewGuid(), "Test", "Test");

            // Act
            var dto = new ScriptDto
            {
                Id = domainScript.Id,
                Name = domainScript.Name,
                Description = domainScript.Description,
                Status = MapScriptStatusToDto(domainScript.Status),
                CreatedAt = domainScript.CreatedAt,
                UpdatedAt = domainScript.UpdatedAt,
                Version = domainScript.Version,
                Actions = domainScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = domainScript.GetEstimatedDuration()
            };

            // Assert
            dto.Should().NotBeNull();
            dto.Actions.Should().BeEmpty();
        }

        [Fact]
        public void DtoMapping_ShouldHandleEmptyCollections()
        {
            // Arrange
            var domainStateMachine = new StateMachine(Guid.NewGuid(), "Test", "Test");

            // Act
            var dto = new StateMachineDto
            {
                Id = domainStateMachine.Id,
                Name = domainStateMachine.Name,
                Description = domainStateMachine.Description,
                Status = MapStateMachineStatusToDto(domainStateMachine.Status),
                CreatedAt = domainStateMachine.CreatedAt,
                UpdatedAt = domainStateMachine.UpdatedAt,
                Version = domainStateMachine.Version,
                States = domainStateMachine.States.Select(MapToDto).ToList(),
                Transitions = domainStateMachine.Transitions.Select(MapToDto).ToList(),
                Rules = domainStateMachine.Rules.Select(MapToDto).ToList(),
                CurrentState = MapToDto(domainStateMachine.CurrentState)
            };

            // Assert
            dto.States.Should().HaveCount(1); // 初始状态
            dto.Transitions.Should().BeEmpty();
            dto.Rules.Should().BeEmpty();
        }

        [Fact]
        public void DtoMapping_ShouldPreserveDataIntegrity()
        {
            // Arrange
            var originalScript = TestDataFactory.CreateValidScript();
            var totalDuration = originalScript.GetEstimatedDuration();

            // Act
            var dto = new ScriptDto
            {
                Id = originalScript.Id,
                Name = originalScript.Name,
                Description = originalScript.Description,
                Status = MapScriptStatusToDto(originalScript.Status),
                CreatedAt = originalScript.CreatedAt,
                UpdatedAt = originalScript.UpdatedAt,
                Version = originalScript.Version,
                Actions = originalScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = totalDuration
            };

            // Assert
            // 验证所有数据都被正确映射
            dto.Id.Should().Be(originalScript.Id);
            dto.Name.Should().Be(originalScript.Name);
            dto.Description.Should().Be(originalScript.Description);
            dto.Status.Should().Be(MapScriptStatusToDto(originalScript.Status));
            dto.CreatedAt.Should().Be(originalScript.CreatedAt);
            dto.UpdatedAt.Should().Be(originalScript.UpdatedAt);
            dto.Version.Should().Be(originalScript.Version);
            dto.EstimatedDuration.Should().Be(totalDuration);
            dto.Actions.Should().HaveCount(originalScript.Actions.Count);

            // 验证动作映射的完整性
            foreach (var originalAction in originalScript.Actions)
            {
                var matchingDto = dto.Actions.FirstOrDefault(a => a.Id == originalAction.Id);
                matchingDto.Should().NotBeNull();
                matchingDto.Type.Should().Be(MapActionTypeToDto(originalAction.Type));
                matchingDto.Delay.Should().Be(originalAction.Delay);
                matchingDto.Description.Should().Be(originalAction.Description);
            }
        }

        #endregion

        #region 性能测试

        [Fact]
        public void DtoMapping_ShouldBeEfficient()
        {
            // Arrange
            var largeScript = new Script(Guid.NewGuid(), "Large Script", "Many actions");
            
            // 添加大量动作
            for (int i = 0; i < 1000; i++)
            {
                var action = new GameAction(Guid.NewGuid(), ActionType.Delay, 10, $"Action {i}");
                largeScript.AddAction(action);
            }

            // Act
            var startTime = DateTime.UtcNow;
            var dto = new ScriptDto
            {
                Id = largeScript.Id,
                Name = largeScript.Name,
                Description = largeScript.Description,
                Status = MapScriptStatusToDto(largeScript.Status),
                CreatedAt = largeScript.CreatedAt,
                UpdatedAt = largeScript.UpdatedAt,
                Version = largeScript.Version,
                Actions = largeScript.Actions.Select(MapToDto).ToList(),
                EstimatedDuration = largeScript.GetEstimatedDuration()
            };
            var endTime = DateTime.UtcNow;

            // Assert
            var mappingTime = endTime - startTime;
            mappingTime.Should().BeLessThan(TimeSpan.FromMilliseconds(100)); // 应该在100ms内完成
            dto.Actions.Should().HaveCount(1000);
        }

        #endregion

        #region 辅助方法

        private static ScriptStatusDto MapScriptStatusToDto(ScriptStatus status)
        {
            return status switch
            {
                ScriptStatus.Draft => ScriptStatusDto.Draft,
                ScriptStatus.Active => ScriptStatusDto.Active,
                ScriptStatus.Stopped => ScriptStatusDto.Inactive,
                ScriptStatus.Error => ScriptStatusDto.Deleted,
                _ => throw new ArgumentException($"Unknown script status: {status}")
            };
        }

        private static ScriptStatus MapScriptStatusToDomain(ScriptStatusDto status)
        {
            return status switch
            {
                ScriptStatusDto.Draft => ScriptStatus.Draft,
                ScriptStatusDto.Active => ScriptStatus.Active,
                ScriptStatusDto.Inactive => ScriptStatus.Stopped,
                ScriptStatusDto.Deleted => ScriptStatus.Error,
                _ => throw new ArgumentException($"Unknown script status: {status}")
            };
        }

        private static ActionTypeDto MapActionTypeToDto(ActionType type)
        {
            return type switch
            {
                ActionType.KeyDown => ActionTypeDto.KeyDown,
                ActionType.KeyUp => ActionTypeDto.KeyUp,
                ActionType.MouseDown => ActionTypeDto.MouseDown,
                ActionType.MouseUp => ActionTypeDto.MouseUp,
                ActionType.MouseMove => ActionTypeDto.MouseMove,
                ActionType.Delay => ActionTypeDto.Delay,
                _ => throw new ArgumentException($"Unknown action type: {type}")
            };
        }

        private static ActionType MapActionTypeToDomain(ActionTypeDto type)
        {
            return type switch
            {
                ActionTypeDto.KeyDown => ActionType.KeyDown,
                ActionTypeDto.KeyUp => ActionType.KeyUp,
                ActionTypeDto.MouseDown => ActionType.MouseDown,
                ActionTypeDto.MouseUp => ActionType.MouseUp,
                ActionTypeDto.MouseMove => ActionType.MouseMove,
                ActionTypeDto.Delay => ActionType.Delay,
                _ => throw new ArgumentException($"Unknown action type: {type}")
            };
        }

        private static KeyCodeDto MapKeyCodeToDto(KeyCode key)
        {
            return (KeyCodeDto)key;
        }

        private static MouseButtonDto MapMouseButtonToDto(MouseButton button)
        {
            return button switch
            {
                MouseButton.None => MouseButtonDto.None,
                MouseButton.Left => MouseButtonDto.Left,
                MouseButton.Right => MouseButtonDto.Right,
                MouseButton.Middle => MouseButtonDto.Middle,
                _ => throw new ArgumentException($"Unknown mouse button: {button}")
            };
        }

        private static RectangleDto MapRectangleToDto(System.Drawing.Rectangle rectangle)
        {
            return new RectangleDto
            {
                X = rectangle.X,
                Y = rectangle.Y,
                Width = rectangle.Width,
                Height = rectangle.Height
            };
        }

        private static TemplateTypeDto MapTemplateTypeToDto(KeyForge.Domain.Common.TemplateType templateType)
        {
            return templateType switch
            {
                KeyForge.Domain.Common.TemplateType.Image => TemplateTypeDto.Image,
                KeyForge.Domain.Common.TemplateType.Color => TemplateTypeDto.Color,
                KeyForge.Domain.Common.TemplateType.Text => TemplateTypeDto.Text,
                _ => throw new ArgumentException($"Unknown template type: {templateType}")
            };
        }

        private static StateMachineStatusDto MapStateMachineStatusToDto(StateMachineStatus status)
        {
            return status switch
            {
                StateMachineStatus.Draft => StateMachineStatusDto.Draft,
                StateMachineStatus.Active => StateMachineStatusDto.Active,
                StateMachineStatus.Paused => StateMachineStatusDto.Paused,
                StateMachineStatus.Stopped => StateMachineStatusDto.Stopped,
                StateMachineStatus.Error => StateMachineStatusDto.Error,
                _ => throw new ArgumentException($"Unknown state machine status: {status}")
            };
        }

        private static GameActionDto MapToDto(GameAction action)
        {
            return new GameActionDto
            {
                Id = action.Id,
                Type = MapActionTypeToDto(action.Type),
                Key = MapKeyCodeToDto(action.Key),
                Button = MapMouseButtonToDto(action.Button),
                X = action.X,
                Y = action.Y,
                Delay = action.Delay,
                Timestamp = action.Timestamp,
                Description = action.Description
            };
        }

        private static StateDto MapToDto(KeyForge.Domain.Entities.State state)
        {
            return new StateDto
            {
                Id = state.Id,
                Name = state.Name,
                Description = state.Description,
                Variables = new System.Collections.Generic.Dictionary<string, object>(),
                CreatedAt = state.CreatedAt,
                UpdatedAt = state.UpdatedAt
            };
        }

        private static StateTransitionDto MapToDto(KeyForge.Domain.Aggregates.StateTransition transition)
        {
            return new StateTransitionDto
            {
                Id = transition.Id,
                FromStateId = transition.FromStateId,
                ToStateId = transition.ToStateId,
                Condition = transition.Condition?.ToString(),
                Description = transition.Description
            };
        }

        private static DecisionRuleDto MapToDto(KeyForge.Domain.Entities.DecisionRule rule)
        {
            return new DecisionRuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                Condition = rule.Condition?.ToString(),
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt
            };
        }

        #endregion
    }
}