using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using DomainValidationException = KeyForge.Domain.Exceptions.ValidationException;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 最简化的脚本聚合根测试
    /// 只测试最基本的构造函数和属性
    /// </summary>
    public class BasicScriptTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateScript()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Script";
            var description = "Test script description";

            // Act
            var script = new Script(id, name, description);

            // Assert
            script.Should().NotBeNull();
            script.Id.Should().Be(id);
            script.Name.Should().Be(name);
            script.Description.Should().Be(description);
            script.Actions.Should().BeEmpty();
            script.Status.Should().Be(KeyForge.Domain.Aggregates.ScriptStatus.Draft);
            script.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldNotThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "";
            var description = "Test script description";

            // Act & Assert
            // 注意：实际的Script构造函数没有对空名称进行验证
            // 验证只在Update方法中进行
            var action = () => new Script(id, name, description);
            action.Should().NotThrow();
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateScript()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Original Name", "Original description");
            var newName = "Updated Name";
            var newDescription = "Updated description";

            // Act
            script.Update(newName, newDescription);

            // Assert
            script.Name.Should().Be(newName);
            script.Description.Should().Be(newDescription);
            script.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_WithValidScript_ShouldSetStatusToActive()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            
            // 根据实际业务规则，需要至少有一个action才能激活
            var action = new KeyForge.Domain.Entities.GameAction(
                Guid.NewGuid(),
                KeyForge.Domain.Entities.ActionType.KeyDown,
                KeyForge.Domain.Entities.KeyCode.A,
                100,
                "Test action"
            );
            script.AddAction(action);

            // Act
            script.Activate();

            // Assert
            script.Status.Should().Be(KeyForge.Domain.Aggregates.ScriptStatus.Active);
        }

        [Fact]
        public void Activate_WithNoActions_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");

            // Act & Assert
            // 根据实际业务规则，没有actions的脚本不能激活
            var action = () => script.Activate();
            action.Should().Throw<KeyForge.Domain.Exceptions.BusinessRuleViolationException>()
                .WithMessage("Cannot activate script with no actions.");
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldSetStatusToInactive()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test Script", "Test description");
            
            // 先激活脚本
            var action = new KeyForge.Domain.Entities.GameAction(
                Guid.NewGuid(),
                KeyForge.Domain.Entities.ActionType.KeyDown,
                KeyForge.Domain.Entities.KeyCode.A,
                100,
                "Test action"
            );
            script.AddAction(action);
            script.Activate();

            // Act
            script.Deactivate();

            // Assert
            script.Status.Should().Be(KeyForge.Domain.Aggregates.ScriptStatus.Inactive);
        }
    }
}