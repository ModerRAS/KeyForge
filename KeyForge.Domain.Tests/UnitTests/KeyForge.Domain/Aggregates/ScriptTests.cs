using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Tests.Aggregates
{
    /// <summary>
    /// Script聚合根的单元测试
    /// 测试Script实体的核心业务逻辑
    /// 简化实现：只测试现有的功能
    /// </summary>
    public class ScriptTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var name = "Test Script";
            var description = "Test Description";

            // Act
            var script = new Script(scriptId, name, description);

            // Assert
            script.Should().NotBeNull();
            script.Id.Should().Be(scriptId);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Status.Should().Be(ScriptStatus.Draft);
            script.Actions.Should().BeEmpty();
            script.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            script.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowException(string invalidName)
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var description = "Test Description";

            // Act & Assert
            var action = () => new Script(scriptId, invalidName, description);
            action.Should().Throw<ValidationException>()
                .WithMessage("Script name cannot be empty.");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test Description");
            var newName = "Updated Script";
            var newDescription = "Updated Description";

            // Act
            script.Update(newName, newDescription);

            // Assert
            script.Name.Should().Be(newName);
            script.Description.Should().Be(newDescription);
            script.UpdatedAt.Should().BeAfter(script.CreatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithInvalidName_ShouldThrowException(string invalidName)
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test Description");

            // Act & Assert
            var action = () => script.Update(invalidName, "Updated Description");
            action.Should().Throw<ValidationException>()
                .WithMessage("Script name cannot be empty.");
        }
    }
}