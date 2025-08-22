using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using KeyForge.Application.Services;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using KeyForge.Application.DTOs;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Interfaces;
using KeyForge.Infrastructure.Persistence;
using KeyForge.Infrastructure.Dependencyinjection;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.IntegrationTests.Application
{
    /// <summary>
    /// 脚本服务集成测试
    /// </summary>
    public class ScriptServiceIntegrationTests : TestBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScriptService _scriptService;
        private readonly IScriptRepository _scriptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ScriptServiceIntegrationTests()
        {
            // 设置依赖注入
            var services = new ServiceCollection();
            
            // 添加基础设施服务
            services.AddInfrastructure();
            
            // 添加应用服务
            services.AddScoped<IScriptService, ScriptService>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // 获取服务实例
            _scriptService = _serviceProvider.GetRequiredService<IScriptService>();
            _scriptRepository = _serviceProvider.GetRequiredService<IScriptRepository>();
            _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        }

        [Fact]
        public async Task CreateScript_ShouldPersistToDatabase()
        {
            // Arrange
            var command = TestDataFactory.CreateCreateScriptCommand();

            // Act
            var result = await _scriptService.Handle(command, default);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.Status.Should().Be(ScriptStatus.Draft);

            // 验证数据库中存在该脚本
            var scriptFromDb = await _scriptRepository.GetByIdAsync(result.Id);
            scriptFromDb.Should().NotBeNull();
            scriptFromDb.Name.Should().Be(command.Name);
            scriptFromDb.Description.Should().Be(command.Description);
        }

        [Fact]
        public async Task UpdateScript_ShouldUpdateInDatabase()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(createCommand, default);

            var updateCommand = new UpdateScriptCommand
            {
                Id = createdScript.Id,
                Name = "Updated Script Name",
                Description = "Updated Description"
            };

            // Act
            var result = await _scriptService.Handle(updateCommand, default);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(createdScript.Id);
            result.Name.Should().Be(updateCommand.Name);
            result.Description.Should().Be(updateCommand.Description);

            // 验证数据库中的更新
            var scriptFromDb = await _scriptRepository.GetByIdAsync(result.Id);
            scriptFromDb.Name.Should().Be(updateCommand.Name);
            scriptFromDb.Description.Should().Be(updateCommand.Description);
        }

        [Fact]
        public async Task ActivateScript_ShouldUpdateStatusInDatabase()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(createCommand, default);

            var activateCommand = new ActivateScriptCommand { Id = createdScript.Id };

            // Act
            var result = await _scriptService.Handle(activateCommand, default);

            // Assert
            result.Should().BeTrue();

            // 验证数据库中的状态更新
            var scriptFromDb = await _scriptRepository.GetByIdAsync(createdScript.Id);
            scriptFromDb.Status.Should().Be(ScriptStatus.Active);
        }

        [Fact]
        public async Task DeactivateScript_ShouldUpdateStatusInDatabase()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(createCommand, default);

            // 先激活脚本
            var activateCommand = new ActivateScriptCommand { Id = createdScript.Id };
            await _scriptService.Handle(activateCommand, default);

            var deactivateCommand = new DeactivateScriptCommand { Id = createdScript.Id };

            // Act
            var result = await _scriptService.Handle(deactivateCommand, default);

            // Assert
            result.Should().BeTrue();

            // 验证数据库中的状态更新
            var scriptFromDb = await _scriptRepository.GetByIdAsync(createdScript.Id);
            scriptFromDb.Status.Should().Be(ScriptStatus.Inactive);
        }

        [Fact]
        public async Task GetAllScripts_ShouldReturnAllScriptsFromDatabase()
        {
            // Arrange
            var command1 = TestDataFactory.CreateCreateScriptCommand();
            var command2 = TestDataFactory.CreateCreateScriptCommand();
            var command3 = TestDataFactory.CreateCreateScriptCommand();

            await _scriptService.Handle(command1, default);
            await _scriptService.Handle(command2, default);
            await _scriptService.Handle(command3, default);

            var query = new GetAllScriptsQuery();

            // Act
            var result = await _scriptService.Handle(query, default);

            // Assert
            result.Should().HaveCountGreaterThanOrEqualTo(3);
        }

        [Fact]
        public async Task GetScriptDetails_ShouldReturnCompleteScriptDetails()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(createCommand, default);

            var query = new GetScriptDetailsQuery { Id = createdScript.Id };

            // Act
            var result = await _scriptService.Handle(query, default);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(createdScript.Id);
            result.Name.Should().Be(createdScript.Name);
            result.Description.Should().Be(createdScript.Description);
            result.Actions.Should().HaveCount(createCommand.Actions.Count);
            result.EstimatedDuration.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task DeleteScript_ShouldMarkScriptAsDeletedInDatabase()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(createCommand, default);

            var deleteCommand = new DeleteScriptCommand { Id = createdScript.Id };

            // Act
            var result = await _scriptService.Handle(deleteCommand, default);

            // Assert
            result.Should().BeTrue();

            // 验证数据库中的状态更新
            var scriptFromDb = await _scriptRepository.GetByIdAsync(createdScript.Id);
            scriptFromDb.Status.Should().Be(ScriptStatus.Deleted);
        }

        [Fact]
        public async Task ScriptLifecycle_ShouldWorkEndToEnd()
        {
            // Arrange
            var createCommand = TestDataFactory.CreateCreateScriptCommand();

            // 创建脚本
            var createdScript = await _scriptService.Handle(createCommand, default);
            createdScript.Should().NotBeNull();
            createdScript.Status.Should().Be(ScriptStatus.Draft);

            // 更新脚本
            var updateCommand = new UpdateScriptCommand
            {
                Id = createdScript.Id,
                Name = "Updated Name",
                Description = "Updated Description"
            };
            var updatedScript = await _scriptService.Handle(updateCommand, default);
            updatedScript.Name.Should().Be(updateCommand.Name);
            updatedScript.Description.Should().Be(updateCommand.Description);

            // 激活脚本
            var activateCommand = new ActivateScriptCommand { Id = createdScript.Id };
            var activateResult = await _scriptService.Handle(activateCommand, default);
            activateResult.Should().BeTrue();

            // 验证激活状态
            var activatedScript = await _scriptService.Handle(new GetScriptQuery { Id = createdScript.Id }, default);
            activatedScript.Status.Should().Be(ScriptStatus.Active);

            // 停用脚本
            var deactivateCommand = new DeactivateScriptCommand { Id = createdScript.Id };
            var deactivateResult = await _scriptService.Handle(deactivateCommand, default);
            deactivateResult.Should().BeTrue();

            // 验证停用状态
            var deactivatedScript = await _scriptService.Handle(new GetScriptQuery { Id = createdScript.Id }, default);
            deactivatedScript.Status.Should().Be(ScriptStatus.Inactive);

            // 删除脚本
            var deleteCommand = new DeleteScriptCommand { Id = createdScript.Id };
            var deleteResult = await _scriptService.Handle(deleteCommand, default);
            deleteResult.Should().BeTrue();

            // 验证删除状态
            var deletedScript = await _scriptService.Handle(new GetScriptQuery { Id = createdScript.Id }, default);
            deletedScript.Status.Should().Be(ScriptStatus.Deleted);
        }

        [Fact]
        public async Task ConcurrentScriptOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var command1 = TestDataFactory.CreateCreateScriptCommand();
            var command2 = TestDataFactory.CreateCreateScriptCommand();

            // 并发创建脚本
            var task1 = _scriptService.Handle(command1, default);
            var task2 = _scriptService.Handle(command2, default);

            // Act
            await Task.WhenAll(task1, task2);

            // Assert
            var result1 = await task1;
            var result2 = await task2;

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().NotBe(result2.Id);

            // 验证两个脚本都存在于数据库中
            var script1FromDb = await _scriptRepository.GetByIdAsync(result1.Id);
            var script2FromDb = await _scriptRepository.GetByIdAsync(result2.Id);

            script1FromDb.Should().NotBeNull();
            script2FromDb.Should().NotBeNull();
        }

        [Fact]
        public async Task LargeScriptOperations_ShouldHandleEfficiently()
        {
            // Arrange
            var command = TestDataFactory.CreateCreateScriptCommand();
            
            // 添加大量动作
            for (int i = 0; i < 100; i++)
            {
                command.Actions.Add(TestDataFactory.CreateGameActionDto());
            }

            var startTime = DateTime.UtcNow;

            // Act
            var result = await _scriptService.Handle(command, default);

            // Assert
            var executionTime = DateTime.UtcNow - startTime;
            executionTime.Should().BeLessThan(TimeSpan.FromSeconds(5)); // 应该在5秒内完成

            result.Should().NotBeNull();
            result.Actions.Should().HaveCount(100 + command.Actions.Count - 100); // 减去原始动作数量

            // 验证数据库查询性能
            var query = new GetScriptDetailsQuery { Id = result.Id };
            var details = await _scriptService.Handle(query, default);
            details.Actions.Should().HaveCount(100 + command.Actions.Count - 100);
        }

        [Fact]
        public async Task ErrorHandling_ShouldRollbackTransactions()
        {
            // Arrange
            var validCommand = TestDataFactory.CreateCreateScriptCommand();
            var createdScript = await _scriptService.Handle(validCommand, default);

            // 尝试更新不存在的脚本
            var invalidUpdateCommand = new UpdateScriptCommand
            {
                Id = Guid.NewGuid(),
                Name = "Invalid Update",
                Description = "This should fail"
            };

            // Act & Assert
            var action = async () => await _scriptService.Handle(invalidUpdateCommand, default);
            await action.Should().ThrowAsync<EntityNotFoundException>();

            // 验证原始脚本仍然存在且未被修改
            var originalScript = await _scriptService.Handle(new GetScriptQuery { Id = createdScript.Id }, default);
            originalScript.Should().NotBeNull();
            originalScript.Name.Should().Be(validCommand.Name);
        }
    }
}