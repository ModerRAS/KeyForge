using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;

namespace KeyForge.CrossPlatformTests.Common
{
    /// <summary>
    /// 跨平台测试基类
    /// 原本实现：复杂的测试基础设施
    /// 简化实现：基础测试功能
    /// </summary>
    public class TestBase
    {
        protected static void ShouldThrowValidationException(Action action, string expectedMessage)
        {
            action.Should().Throw<KeyForge.Domain.Exceptions.ValidationException>()
                .WithMessage(expectedMessage);
        }

        protected static void ShouldThrowBusinessRuleViolationException(Action action, string expectedMessage)
        {
            action.Should().Throw<BusinessRuleViolationException>()
                .WithMessage(expectedMessage);
        }

        protected static void ShouldThrowEntityNotFoundException(Action action, string entityName, Guid id)
        {
            action.Should().Throw<EntityNotFoundException>()
                .Where(e => e.Message.Contains(entityName) && e.Message.Contains(id.ToString()));
        }

        protected static void ShouldBeValidScript(Script script)
        {
            script.Should().NotBeNull();
            script.Id.Should().NotBe(Guid.Empty);
            script.Name.Should().NotBeNullOrWhiteSpace();
            script.Status.Should().Be(ScriptStatus.Draft);
            script.Version.Should().BeGreaterThan(0);
            script.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        protected static void ShouldBeValidGameAction(GameAction action)
        {
            action.Should().NotBeNull();
            action.Id.Should().NotBe(Guid.Empty);
            action.Delay.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}