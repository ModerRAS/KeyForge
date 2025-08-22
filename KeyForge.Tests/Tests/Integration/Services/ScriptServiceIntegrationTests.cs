using Xunit;
using FluentAssertions;
using Moq;
using KeyForge.Application.Services;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using KeyForge.Application.DTOs;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Interfaces;
using KeyForge.Tests.Support;
using System.Threading.Tasks;
using System.Threading;

namespace KeyForge.Tests.Integration.Services;

/// <summary>
/// 脚本服务集成测试
/// 原本实现：复杂的集成测试场景
/// 简化实现：核心集成功能测试
/// </summary>
public class ScriptServiceIntegrationTests : TestBase
{
    private readonly Mock<IScriptRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ScriptService _scriptService;

    public ScriptServiceIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _mockRepository = new Mock<IScriptRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _scriptService = new ScriptService(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_CreateScriptCommand_WithValidData_ShouldCreateScript()
    {
        // Arrange
        var command = new CreateScriptCommand
        {
            Name = "Test Script",
            Description = "Test Description",
            Actions = new List<GameActionDto>
            {
                new GameActionDto
                {
                    Id = Guid.NewGuid(),
                    Type = ActionType.KeyDown,
                    Key = KeyCode.A,
                    Delay = 100
                }
            }
        };

        Script capturedScript = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Script>()))
            .Callback<Script>(s => capturedScript = s)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Actions.Should().HaveCount(1);
        result.Actions[0].Type.Should().Be(ActionType.KeyDown);
        
        capturedScript.Should().NotBeNull();
        capturedScript.Name.Should().Be(command.Name);
        capturedScript.Actions.Should().HaveCount(1);
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Script>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Log($"创建脚本成功: {result.Id}");
    }

    [Fact]
    public async Task Handle_CreateScriptCommand_WithInvalidData_ShouldThrowException()
    {
        // Arrange
        var command = new CreateScriptCommand
        {
            Name = "", // 无效名称
            Description = "Test Description",
            Actions = new List<GameActionDto>()
        };

        // Act & Assert
        var action = () => _scriptService.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ValidationException>();
        Log($"验证无效脚本创建失败");
    }

    [Fact]
    public async Task Handle_UpdateScriptCommand_WithValidData_ShouldUpdateScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateValidScript();
        var command = new UpdateScriptCommand
        {
            Id = existingScript.Id,
            Name = "Updated Script",
            Description = "Updated Description"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Version.Should().Be(2); // 版本应该增加
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Log($"更新脚本成功: {result.Id}");
    }

    [Fact]
    public async Task Handle_UpdateScriptCommand_WithNonExistentScript_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateScriptCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Script",
            Description = "Updated Description"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Script)null);

        // Act & Assert
        var action = () => _scriptService.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
        Log($"验证更新不存在脚本失败");
    }

    [Fact]
    public async Task Handle_DeleteScriptCommand_WithExistingScript_ShouldDeleteScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateValidScript();
        var command = new DeleteScriptCommand { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingScript.Status.Should().Be(ScriptStatus.Deleted);
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Log($"删除脚本成功: {existingScript.Id}");
    }

    [Fact]
    public async Task Handle_DeleteScriptCommand_WithNonExistentScript_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeleteScriptCommand { Id = Guid.NewGuid() };

        _mockRepository.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Script)null);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        Log($"验证删除不存在脚本返回false");
    }

    [Fact]
    public async Task Handle_ActivateScriptCommand_WithValidScript_ShouldActivateScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateScriptWithActions(1);
        var command = new ActivateScriptCommand { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingScript.Status.Should().Be(ScriptStatus.Active);
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Log($"激活脚本成功: {existingScript.Id}");
    }

    [Fact]
    public async Task Handle_ActivateScriptCommand_WithEmptyScript_ShouldThrowException()
    {
        // Arrange
        var existingScript = TestFixtures.CreateValidScript();
        var command = new ActivateScriptCommand { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);

        // Act & Assert
        var action = () => _scriptService.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessRuleViolationException>();
        Log($"验证激活空脚本失败");
    }

    [Fact]
    public async Task Handle_DeactivateScriptCommand_WithActiveScript_ShouldDeactivateScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateScriptWithActions(1);
        existingScript.Activate();
        var command = new DeactivateScriptCommand { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingScript.Status.Should().Be(ScriptStatus.Inactive);
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Log($"停用脚本成功: {existingScript.Id}");
    }

    [Fact]
    public async Task Handle_GetScriptQuery_WithExistingScript_ShouldReturnScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateValidScript();
        var query = new GetScriptQuery { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);

        // Act
        var result = await _scriptService.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingScript.Id);
        result.Name.Should().Be(existingScript.Name);
        result.Description.Should().Be(existingScript.Description);
        result.Status.Should().Be(existingScript.Status);
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        Log($"获取脚本成功: {result.Id}");
    }

    [Fact]
    public async Task Handle_GetScriptQuery_WithNonExistentScript_ShouldThrowException()
    {
        // Arrange
        var query = new GetScriptQuery { Id = Guid.NewGuid() };

        _mockRepository.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync((Script)null);

        // Act & Assert
        var action = () => _scriptService.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
        Log($"验证获取不存在脚本失败");
    }

    [Fact]
    public async Task Handle_GetAllScriptsQuery_WithoutStatus_ShouldReturnAllScripts()
    {
        // Arrange
        var scripts = new List<Script>
        {
            TestFixtures.CreateValidScript(),
            TestFixtures.CreateValidScript(),
            TestFixtures.CreateValidScript()
        };
        var query = new GetAllScriptsQuery();

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(scripts);

        // Act
        var result = await _scriptService.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Id == scripts[0].Id);
        result.Should().Contain(r => r.Id == scripts[1].Id);
        result.Should().Contain(r => r.Id == scripts[2].Id);
        
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        Log($"获取所有脚本成功: {result.Length} 个脚本");
    }

    [Fact]
    public async Task Handle_GetAllScriptsQuery_WithStatus_ShouldReturnFilteredScripts()
    {
        // Arrange
        var scripts = new List<Script>
        {
            TestFixtures.CreateValidScript(),
            TestFixtures.CreateValidScript()
        };
        scripts[0].Activate();
        scripts[1].Activate();
        scripts[1].Deactivate();
        
        var query = new GetAllScriptsQuery { Status = ScriptStatus.Active };

        _mockRepository.Setup(r => r.GetByStatusAsync(ScriptStatus.Active))
            .ReturnsAsync(scripts.Where(s => s.Status == ScriptStatus.Active).ToList());

        // Act
        var result = await _scriptService.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(ScriptStatus.Active);
        
        _mockRepository.Verify(r => r.GetByStatusAsync(ScriptStatus.Active), Times.Once);
        Log($"获取按状态过滤脚本成功: {result.Length} 个活跃脚本");
    }

    [Fact]
    public async Task Handle_GetScriptDetailsQuery_WithExistingScript_ShouldReturnDetailedScript()
    {
        // Arrange
        var existingScript = TestFixtures.CreateScriptWithActions(2);
        var query = new GetScriptDetailsQuery { Id = existingScript.Id };

        _mockRepository.Setup(r => r.GetByIdAsync(existingScript.Id))
            .ReturnsAsync(existingScript);

        // Act
        var result = await _scriptService.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingScript.Id);
        result.Actions.Should().HaveCount(2);
        result.EstimatedDuration.Should().Be(existingScript.GetEstimatedDuration());
        
        _mockRepository.Verify(r => r.GetByIdAsync(existingScript.Id), Times.Once);
        Log($"获取脚本详情成功: {result.Id}");
    }

    [Fact]
    public async Task CreateGameActionFromDto_WithKeyboardAction_ShouldCreateKeyboardAction()
    {
        // Arrange
        var actionDto = new GameActionDto
        {
            Id = Guid.NewGuid(),
            Type = ActionType.KeyDown,
            Key = KeyCode.A,
            Delay = 100,
            Description = "Test keyboard action"
        };

        // Act
        var command = new CreateScriptCommand
        {
            Name = "Test Script",
            Description = "Test Description",
            Actions = new List<GameActionDto> { actionDto }
        };

        Script capturedScript = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Script>()))
            .Callback<Script>(s => capturedScript = s)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        capturedScript.Should().NotBeNull();
        capturedScript.Actions.Should().HaveCount(1);
        var action = capturedScript.Actions.First();
        action.Type.Should().Be(ActionType.KeyDown);
        action.Key.Should().Be(KeyCode.A);
        action.Delay.Should().Be(100);
        action.Description.Should().Be("Test keyboard action");
        Log($"创建键盘动作DTO转换成功");
    }

    [Fact]
    public async Task CreateGameActionFromDto_WithMouseAction_ShouldCreateMouseAction()
    {
        // Arrange
        var actionDto = new GameActionDto
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
        var command = new CreateScriptCommand
        {
            Name = "Test Script",
            Description = "Test Description",
            Actions = new List<GameActionDto> { actionDto }
        };

        Script capturedScript = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Script>()))
            .Callback<Script>(s => capturedScript = s)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        capturedScript.Should().NotBeNull();
        capturedScript.Actions.Should().HaveCount(1);
        var action = capturedScript.Actions.First();
        action.Type.Should().Be(ActionType.MouseDown);
        action.Button.Should().Be(MouseButton.Left);
        action.X.Should().Be(100);
        action.Y.Should().Be(200);
        action.Delay.Should().Be(150);
        action.Description.Should().Be("Test mouse action");
        Log($"创建鼠标动作DTO转换成功");
    }

    [Fact]
    public async Task CreateGameActionFromDto_WithDelayAction_ShouldCreateDelayAction()
    {
        // Arrange
        var actionDto = new GameActionDto
        {
            Id = Guid.NewGuid(),
            Type = ActionType.Delay,
            Delay = 200,
            Description = "Test delay action"
        };

        // Act
        var command = new CreateScriptCommand
        {
            Name = "Test Script",
            Description = "Test Description",
            Actions = new List<GameActionDto> { actionDto }
        };

        Script capturedScript = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Script>()))
            .Callback<Script>(s => capturedScript = s)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        await _scriptService.Handle(command, CancellationToken.None);

        // Assert
        capturedScript.Should().NotBeNull();
        capturedScript.Actions.Should().HaveCount(1);
        var action = capturedScript.Actions.First();
        action.Type.Should().Be(ActionType.Delay);
        action.Delay.Should().Be(200);
        action.Description.Should().Be("Test delay action");
        Log($"创建延迟动作DTO转换成功");
    }
}