using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KeyForge.Infrastructure.Repositories;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using KeyForge.Domain.Interfaces;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.ComponentTests.Infrastructure.Repositories
{
    /// <summary>
    /// ScriptRepository组件测试
    /// 测试脚本仓储的完整功能，包括CRUD操作、错误处理和性能
    /// </summary>
    public class ScriptRepositoryComponentTests : TestBase
    {
        private readonly Mock<KeyForgeDbContext> _mockContext;
        private readonly Mock<DbSet<Infrastructure.Data.Script>> _mockDbSet;
        private readonly Mock<ILogger<ScriptRepository>> _mockLogger;
        private readonly ScriptRepository _repository;

        public ScriptRepositoryComponentTests()
        {
            // 创建模拟的DbContext
            _mockContext = new Mock<KeyForgeDbContext>();
            _mockDbSet = new Mock<DbSet<Infrastructure.Data.Script>>();
            _mockLogger = new Mock<ILogger<ScriptRepository>>();

            // 设置DbContext的Scripts属性
            _mockContext.Setup(ctx => ctx.Scripts).Returns(_mockDbSet.Object);

            _repository = new ScriptRepository(_mockContext.Object, _mockLogger.Object);
        }

        #region GetByIdAsync测试

        [Fact]
        public async Task GetByIdAsync_WithExistingScript_ShouldReturnScript()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var scriptEntity = new Infrastructure.Data.Script
            {
                Id = scriptId,
                Name = "Test Script",
                Description = "Test Description",
                Status = ScriptStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var scriptEntities = new[] { scriptEntity }.AsQueryable();
            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(scriptEntity);

            // Act
            var result = await _repository.GetByIdAsync(scriptId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(scriptId);
            result.Name.Should().Be("Test Script");
            result.Description.Should().Be("Test Description");
            result.Status.Should().Be(ScriptStatus.Draft);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingScript_ShouldReturnNull()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync((Infrastructure.Data.Script)null);

            // Act
            var result = await _repository.GetByIdAsync(scriptId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenDbContextThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var exception = new Exception("Database error");
            
            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var action = async () => await _repository.GetByIdAsync(scriptId);
            await action.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region GetAllAsync测试

        [Fact]
        public async Task GetAllAsync_WithMultipleScripts_ShouldReturnAllScripts()
        {
            // Arrange
            var scriptEntities = new[]
            {
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Script 1",
                    Description = "Description 1",
                    Status = ScriptStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Script 2",
                    Description = "Description 2",
                    Status = ScriptStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Script 3",
                    Description = "Description 3",
                    Status = ScriptStatus.Stopped,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }.AsQueryable();

            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ReturnsAsync(scriptEntities.ToList());

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(s => s.Name == "Script 1");
            result.Should().Contain(s => s.Name == "Script 2");
            result.Should().Contain(s => s.Name == "Script 3");
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ReturnsAsync(new List<Infrastructure.Data.Script>());

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WhenDbContextThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var exception = new Exception("Database error");
            
            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var action = async () => await _repository.GetAllAsync();
            await action.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region GetByStatusAsync测试

        [Fact]
        public async Task GetByStatusAsync_WithMatchingScripts_ShouldReturnFilteredScripts()
        {
            // Arrange
            var scriptEntities = new[]
            {
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Active Script 1",
                    Status = ScriptStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Active Script 2",
                    Status = ScriptStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Draft Script",
                    Status = ScriptStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }.AsQueryable();

            _mockDbSet.Setup(dbSet => dbSet.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .Returns(scriptEntities.Where(s => s.Status == ScriptStatus.Active));

            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ReturnsAsync(scriptEntities.Where(s => s.Status == ScriptStatus.Active).ToList());

            // Act
            var result = await _repository.GetByStatusAsync(ScriptStatus.Active);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Status == ScriptStatus.Active);
            result.Should().Contain(s => s.Name == "Active Script 1");
            result.Should().Contain(s => s.Name == "Active Script 2");
        }

        [Fact]
        public async Task GetByStatusAsync_WithNoMatchingScripts_ShouldReturnEmptyList()
        {
            // Arrange
            var scriptEntities = new[]
            {
                new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = "Draft Script",
                    Status = ScriptStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }.AsQueryable();

            _mockDbSet.Setup(dbSet => dbSet.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .Returns(scriptEntities.Where(s => s.Status == ScriptStatus.Active));

            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ReturnsAsync(new List<Infrastructure.Data.Script>());

            // Act
            var result = await _repository.GetByStatusAsync(ScriptStatus.Active);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region SaveAsync测试

        [Fact]
        public async Task SaveAsync_WithNewScript_ShouldAddScriptToDatabase()
        {
            // Arrange
            var domainScript = TestDataFactory.CreateValidScript();
            var scriptId = Guid.NewGuid();

            // 设置反射来设置ID
            var idProperty = typeof(Script).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(domainScript, scriptId);
            }

            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync((Infrastructure.Data.Script)null);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _repository.SaveAsync(domainScript);

            // Assert
            result.Should().Be(scriptId);
            
            _mockDbSet.Verify(dbSet => dbSet.Add(It.IsAny<Infrastructure.Data.Script>()), Times.Once);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WithExistingScript_ShouldUpdateScriptInDatabase()
        {
            // Arrange
            var domainScript = TestDataFactory.CreateValidScript();
            var scriptId = Guid.NewGuid();
            var existingEntity = new Infrastructure.Data.Script
            {
                Id = scriptId,
                Name = "Old Name",
                Description = "Old Description",
                Status = ScriptStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // 设置反射来设置ID和更新属性
            var idProperty = typeof(Script).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(domainScript, scriptId);
            }

            domainScript.Update("Updated Name", "Updated Description");

            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(existingEntity);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _repository.SaveAsync(domainScript);

            // Assert
            result.Should().Be(scriptId);
            
            _mockDbSet.Verify(dbSet => dbSet.Add(It.IsAny<Infrastructure.Data.Script>()), Times.Never);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(), Times.Once);

            // 验证实体属性被更新
            existingEntity.Name.Should().Be("Updated Name");
            existingEntity.Description.Should().Be("Updated Description");
            existingEntity.UpdatedAt.Should().BeAfter(existingEntity.CreatedAt);
        }

        [Fact]
        public async Task SaveAsync_WhenDbContextThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var domainScript = TestDataFactory.CreateValidScript();
            var scriptId = Guid.NewGuid();

            // 设置反射来设置ID
            var idProperty = typeof(Script).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(domainScript, scriptId);
            }

            var exception = new Exception("Database error");
            
            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync((Infrastructure.Data.Script)null);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var action = async () => await _repository.SaveAsync(domainScript);
            await action.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region DeleteAsync测试

        [Fact]
        public async Task DeleteAsync_WithExistingScript_ShouldRemoveScriptFromDatabase()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var existingEntity = new Infrastructure.Data.Script
            {
                Id = scriptId,
                Name = "Test Script",
                Status = ScriptStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(existingEntity);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _repository.DeleteAsync(scriptId);

            // Assert
            _mockDbSet.Verify(dbSet => dbSet.Remove(existingEntity), Times.Once);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingScript_ShouldDoNothing()
        {
            // Arrange
            var scriptId = Guid.NewGuid();

            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync((Infrastructure.Data.Script)null);

            // Act
            await _repository.DeleteAsync(scriptId);

            // Assert
            _mockDbSet.Verify(dbSet => dbSet.Remove(It.IsAny<Infrastructure.Data.Script>()), Times.Never);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenDbContextThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var existingEntity = new Infrastructure.Data.Script
            {
                Id = scriptId,
                Name = "Test Script",
                Status = ScriptStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var exception = new Exception("Database error");
            
            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(existingEntity);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var action = async () => await _repository.DeleteAsync(scriptId);
            await action.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region ExistsAsync测试

        [Fact]
        public async Task ExistsAsync_WithExistingScript_ShouldReturnTrue()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            _mockDbSet.Setup(dbSet => dbSet.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.ExistsAsync(scriptId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingScript_ShouldReturnFalse()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            _mockDbSet.Setup(dbSet => dbSet.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.ExistsAsync(scriptId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WhenDbContextThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var exception = new Exception("Database error");
            
            _mockDbSet.Setup(dbSet => dbSet.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var action = async () => await _repository.ExistsAsync(scriptId);
            await action.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region 并发操作测试

        [Fact]
        public async Task ConcurrentOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var script1 = TestDataFactory.CreateValidScript();
            var script2 = TestDataFactory.CreateValidScript();

            _mockDbSet.Setup(dbSet => dbSet.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Infrastructure.Data.Script, bool>>>()))
                .ReturnsAsync((Infrastructure.Data.Script)null);

            _mockContext.Setup(ctx => ctx.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var task1 = _repository.SaveAsync(script1);
            var task2 = _repository.SaveAsync(script2);

            // Assert
            await Task.WhenAll(task1, task2);

            var result1 = await task1;
            var result2 = await task2;

            result1.Should().Be(script1.Id);
            result2.Should().Be(script2.Id);

            _mockDbSet.Verify(dbSet => dbSet.Add(It.IsAny<Infrastructure.Data.Script>()), Times.Exactly(2));
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(), Times.Exactly(2));
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public async Task LargeDatasetOperations_ShouldBeEfficient()
        {
            // Arrange
            var largeScriptList = Enumerable.Range(0, 1000)
                .Select(i => new Infrastructure.Data.Script
                {
                    Id = Guid.NewGuid(),
                    Name = $"Script {i}",
                    Status = ScriptStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            _mockDbSet.Setup(dbSet => dbSet.ToListAsync())
                .ReturnsAsync(largeScriptList);

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _repository.GetAllAsync();
            var endTime = DateTime.UtcNow;

            // Assert
            var executionTime = endTime - startTime;
            executionTime.Should().BeLessThan(TimeSpan.FromMilliseconds(1000)); // 应该在1秒内完成
            result.Should().HaveCount(1000);
        }

        [Fact]
        public void Repository_ShouldHandleNullContext()
        {
            // Arrange & Act & Assert
            var action = () => new ScriptRepository(null, _mockLogger.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }

        [Fact]
        public void Repository_ShouldHandleNullLogger()
        {
            // Arrange & Act & Assert
            var action = () => new ScriptRepository(_mockContext.Object, null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region ReconstructScriptFromEntity测试

        [Fact]
        public void ReconstructScriptFromEntity_ShouldCorrectlyReconstructDomainScript()
        {
            // Arrange
            var entity = new Infrastructure.Data.Script
            {
                Id = Guid.NewGuid(),
                Name = "Test Script",
                Description = "Test Description",
                Status = ScriptStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = _repository.GetType()
                .GetMethod("ReconstructScriptFromEntity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_repository, new object[] { entity }) as Script;

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Script");
            result.Description.Should().Be("Test Description");
            result.Status.Should().Be(ScriptStatus.Draft);
        }

        #endregion
    }
}