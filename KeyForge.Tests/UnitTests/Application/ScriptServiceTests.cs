using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using MediatR;
using KeyForge.Application.Services;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using KeyForge.Application.DTOs;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Interfaces;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.UnitTests.Application
{
    /// <summary>
    /// 脚本应用服务单元测试
    /// </summary>
    public class ScriptServiceTests : TestBase
    {
        private readonly Mock<IScriptRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ScriptService _scriptService;

        public ScriptServiceTests()
        {
            _mockRepository = MockFactory.CreateScriptRepositoryMock();
            _mockUnitOfWork = MockFactory.CreateUnitOfWorkMock();
            _scriptService = new ScriptService(_mockRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_CreateScriptCommand_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var command = TestDataFactory.CreateCreateScriptCommand();
            var expectedScript = TestDataFactory.CreateValidScript();
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, expectedScript);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.Status.Should().Be(ScriptStatus.Draft);
            result.Actions.Should().HaveCount(command.Actions.Count);
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateScriptCommand_WithValidData_ShouldUpdateScript()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var command = TestDataFactory.CreateUpdateScriptCommand(existingScript.Id);
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(command.Id);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateScriptCommand_WithNonExistingScript_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var command = TestDataFactory.CreateUpdateScriptCommand(Guid.NewGuid());
            
            _mockRepository.Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync((Script)null);

            // Act & Assert
            var action = async () => await _scriptService.Handle(command, CancellationToken.None);
            await ShouldThrowEntityNotFoundExceptionAsync<Script>(action, nameof(Script), command.Id);
        }

        [Fact]
        public async Task Handle_DeleteScriptCommand_WithExistingScript_ShouldDeleteScript()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var command = new DeleteScriptCommand { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            existingScript.Status.Should().Be(ScriptStatus.Deleted);
            
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteScriptCommand_WithNonExistingScript_ShouldReturnFalse()
        {
            // Arrange
            var command = new DeleteScriptCommand { Id = Guid.NewGuid() };
            
            _mockRepository.Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync((Script)null);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Script>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ActivateScriptCommand_WithValidScript_ShouldActivateScript()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            existingScript.AddAction(action);
            
            var command = new ActivateScriptCommand { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            existingScript.Status.Should().Be(ScriptStatus.Active);
            
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ActivateScriptCommand_WithScriptWithNoActions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var command = new ActivateScriptCommand { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act & Assert
            var action = async () => await _scriptService.Handle(command, CancellationToken.None);
            await ShouldThrowBusinessRuleViolationExceptionAsync(action, "Cannot activate script with no actions.");
        }

        [Fact]
        public async Task Handle_DeactivateScriptCommand_WithActiveScript_ShouldDeactivateScript()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            existingScript.AddAction(action);
            existingScript.Activate();
            
            var command = new DeactivateScriptCommand { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            existingScript.Status.Should().Be(ScriptStatus.Inactive);
            
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_GetScriptQuery_WithExistingScript_ShouldReturnScript()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var query = new GetScriptQuery { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingScript.Id);
            result.Name.Should().Be(existingScript.Name);
            result.Description.Should().Be(existingScript.Description);
            result.Status.Should().Be(existingScript.Status);
        }

        [Fact]
        public async Task Handle_GetScriptQuery_WithNonExistingScript_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var query = new GetScriptQuery { Id = Guid.NewGuid() };
            
            _mockRepository.Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync((Script)null);

            // Act & Assert
            var action = async () => await _scriptService.Handle(query, CancellationToken.None);
            await ShouldThrowEntityNotFoundExceptionAsync<Script>(action, nameof(Script), query.Id);
        }

        [Fact]
        public async Task Handle_GetAllScriptsQuery_WithoutStatus_ShouldReturnAllScripts()
        {
            // Arrange
            var scripts = new[]
            {
                TestDataFactory.CreateValidScript(),
                TestDataFactory.CreateValidScript(),
                TestDataFactory.CreateValidScript()
            };
            
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(scripts);

            var query = new GetAllScriptsQuery();

            // Act
            var result = await _scriptService.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(scripts.Length);
            result.Should().Contain(r => r.Id == scripts[0].Id);
            result.Should().Contain(r => r.Id == scripts[1].Id);
            result.Should().Contain(r => r.Id == scripts[2].Id);
        }

        [Fact]
        public async Task Handle_GetAllScriptsQuery_WithStatus_ShouldReturnFilteredScripts()
        {
            // Arrange
            var activeScript = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            activeScript.AddAction(action);
            activeScript.Activate();
            
            var draftScript = TestDataFactory.CreateValidScript();
            
            _mockRepository.Setup(r => r.GetByStatusAsync(ScriptStatus.Active))
                .ReturnsAsync(new[] { activeScript });

            var query = new GetAllScriptsQuery { Status = ScriptStatus.Active };

            // Act
            var result = await _scriptService.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(activeScript.Id);
            result[0].Status.Should().Be(ScriptStatus.Active);
        }

        [Fact]
        public async Task Handle_GetScriptDetailsQuery_WithExistingScript_ShouldReturnScriptDetails()
        {
            // Arrange
            var existingScript = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            existingScript.AddAction(action);
            
            var query = new GetScriptDetailsQuery { Id = existingScript.Id };
            
            MockFactory.SetupScriptRepositoryWithScript(_mockRepository, existingScript);

            // Act
            var result = await _scriptService.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingScript.Id);
            result.Name.Should().Be(existingScript.Name);
            result.Description.Should().Be(existingScript.Description);
            result.Status.Should().Be(existingScript.Status);
            result.Actions.Should().HaveCount(1);
            result.Actions[0].Id.Should().Be(action.Id);
            result.EstimatedDuration.Should().Be(existingScript.GetEstimatedDuration());
        }

        [Fact]
        public async Task Handle_GetScriptDetailsQuery_WithNonExistingScript_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var query = new GetScriptDetailsQuery { Id = Guid.NewGuid() };
            
            _mockRepository.Setup(r => r.GetByIdAsync(query.Id))
                .ReturnsAsync((Script)null);

            // Act & Assert
            var action = async () => await _scriptService.Handle(query, CancellationToken.None);
            await ShouldThrowEntityNotFoundExceptionAsync<Script>(action, nameof(Script), query.Id);
        }

        [Fact]
        public async Task Handle_WhenUnitOfWorkFails_ShouldNotCommit()
        {
            // Arrange
            var command = TestDataFactory.CreateCreateScriptCommand();
            
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var action = async () => await _scriptService.Handle(command, CancellationToken.None);
            await action.Should().ThrowAsync<Exception>();
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Script>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public void MapToDto_WithValidScript_ShouldMapCorrectly()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var action = TestDataFactory.CreateGameAction();
            script.AddAction(action);

            // Act
            var dto = ScriptServiceTestsHelper.MapToDto(script);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(script.Id);
            dto.Name.Should().Be(script.Name);
            dto.Description.Should().Be(script.Description);
            dto.Status.Should().Be(script.Status);
            dto.CreatedAt.Should().Be(script.CreatedAt);
            dto.UpdatedAt.Should().Be(script.UpdatedAt);
            dto.Version.Should().Be(script.Version);
            dto.Actions.Should().HaveCount(1);
            dto.Actions[0].Id.Should().Be(action.Id);
            dto.EstimatedDuration.Should().Be(script.GetEstimatedDuration());
        }

        [Fact]
        public void MapActionToDto_WithValidAction_ShouldMapCorrectly()
        {
            // Arrange
            var action = TestDataFactory.CreateGameAction();

            // Act
            var dto = ScriptServiceTestsHelper.MapActionToDto(action);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(action.Id);
            dto.Type.Should().Be(action.Type);
            dto.Key.Should().Be(action.Key);
            dto.Button.Should().Be(action.Button);
            dto.X.Should().Be(action.X);
            dto.Y.Should().Be(action.Y);
            dto.Delay.Should().Be(action.Delay);
            dto.Timestamp.Should().Be(action.Timestamp);
            dto.Description.Should().Be(action.Description);
        }

        [Fact]
        public void CreateGameActionFromDto_WithKeyAction_ShouldCreateCorrectly()
        {
            // Arrange
            var dto = new GameActionDto
            {
                Id = Guid.NewGuid(),
                Type = ActionType.KeyDown,
                Key = KeyCode.A,
                Delay = 100,
                Description = "Test key action"
            };

            // Act
            var action = ScriptServiceTestsHelper.CreateGameActionFromDto(dto);

            // Assert
            action.Should().NotBeNull();
            action.Id.Should().Be(dto.Id);
            action.Type.Should().Be(dto.Type);
            action.Key.Should().Be(dto.Key);
            action.Delay.Should().Be(dto.Delay);
        }

        [Fact]
        public void CreateGameActionFromDto_WithMouseAction_ShouldCreateCorrectly()
        {
            // Arrange
            var dto = new GameActionDto
            {
                Id = Guid.NewGuid(),
                Type = ActionType.MouseDown,
                Button = MouseButton.Left,
                X = 100,
                Y = 200,
                Delay = 150,
                Description = "Test mouse action"
            };

            // Act
            var action = ScriptServiceTestsHelper.CreateGameActionFromDto(dto);

            // Assert
            action.Should().NotBeNull();
            action.Id.Should().Be(dto.Id);
            action.Type.Should().Be(dto.Type);
            action.Button.Should().Be(dto.Button);
            action.X.Should().Be(dto.X);
            action.Y.Should().Be(dto.Y);
            action.Delay.Should().Be(dto.Delay);
        }

        [Fact]
        public void CreateGameActionFromDto_WithDelayAction_ShouldCreateCorrectly()
        {
            // Arrange
            var dto = new GameActionDto
            {
                Id = Guid.NewGuid(),
                Type = ActionType.Delay,
                Delay = 1000,
                Description = "Test delay action"
            };

            // Act
            var action = ScriptServiceTestsHelper.CreateGameActionFromDto(dto);

            // Assert
            action.Should().NotBeNull();
            action.Id.Should().Be(dto.Id);
            action.Type.Should().Be(dto.Type);
            action.Delay.Should().Be(dto.Delay);
        }

        [Fact]
        public void CreateGameActionFromDto_WithUnsupportedActionType_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = new GameActionDto
            {
                Id = Guid.NewGuid(),
                Type = (ActionType)999, // 不支持的动作类型
                Description = "Test unsupported action"
            };

            // Act & Assert
            var action = () => ScriptServiceTestsHelper.CreateGameActionFromDto(dto);
            ShouldThrowArgumentException(action, "actionType");
        }
    }

    /// <summary>
    /// 测试辅助类 - 访问ScriptService的私有方法
    /// </summary>
    internal static class ScriptServiceTestsHelper
    {
        public static Application.DTOs.ScriptDto MapToDto(Script script)
        {
            return new Application.DTOs.ScriptDto
            {
                Id = script.Id,
                Name = script.Name,
                Description = script.Description,
                Status = script.Status,
                CreatedAt = script.CreatedAt,
                UpdatedAt = script.UpdatedAt,
                Version = script.Version,
                Actions = script.Actions.Select(MapActionToDto).ToList(),
                EstimatedDuration = script.GetEstimatedDuration()
            };
        }

        public static Application.DTOs.GameActionDto MapActionToDto(GameAction action)
        {
            return new Application.DTOs.GameActionDto
            {
                Id = action.Id,
                Type = action.Type,
                Key = action.Key,
                Button = action.Button,
                X = action.X,
                Y = action.Y,
                Delay = action.Delay,
                Timestamp = action.Timestamp,
                Description = action.Description
            };
        }

        public static GameAction CreateGameActionFromDto(Application.DTOs.GameActionDto dto)
        {
            return dto.Type switch
            {
                ActionType.KeyDown or ActionType.KeyUp => new GameAction(dto.Id, dto.Type, dto.Key, dto.Delay, dto.Description),
                ActionType.MouseDown or ActionType.MouseUp or ActionType.MouseMove => new GameAction(dto.Id, dto.Type, dto.Button, dto.X, dto.Y, dto.Delay, dto.Description),
                ActionType.Delay => new GameAction(dto.Id, dto.Type, dto.Delay, dto.Description),
                _ => throw new ArgumentException($"Unsupported action type: {dto.Type}")
            };
        }
    }
}