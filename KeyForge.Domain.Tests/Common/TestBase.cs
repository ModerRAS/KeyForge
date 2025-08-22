using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Exceptions;

namespace KeyForge.Domain.Tests.Common
{
    /// <summary>
    /// 测试基类
    /// 提供常用的测试方法和断言
    /// 原本实现：复杂的测试基类
    /// 简化实现：基础测试基类，专注于Domain层测试
    /// </summary>
    public abstract class TestBase
    {
        protected static void ShouldThrowValidationException(Action action, string expectedMessage)
        {
            var exception = Assert.Throws<ValidationException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static void ShouldThrowArgumentNullException(Action action, string expectedParamName)
        {
            var exception = Assert.Throws<ArgumentNullException>(action);
            exception.ParamName.Should().Be(expectedParamName);
        }

        protected static void ShouldThrowArgumentOutOfRangeException(Action action, string expectedParamName)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);
            exception.ParamName.Should().Be(expectedParamName);
        }

        protected static void ShouldThrowKeyNotFoundException(Action action, string expectedMessage)
        {
            var exception = Assert.Throws<KeyNotFoundException>(action);
            exception.Message.Should().Be(expectedMessage);
        }

        protected static void ShouldBeCloseTo(DateTime actual, DateTime expected, TimeSpan tolerance)
        {
            var difference = (actual - expected).Duration();
            difference.Should().BeLessThanOrEqualTo(tolerance);
        }

        protected static void ShouldHaveDomainEvents(object aggregate, int expectedCount)
        {
            var domainEventsProperty = aggregate.GetType().GetProperty("DomainEvents");
            domainEventsProperty.Should().NotBeNull();
            
            var domainEvents = domainEventsProperty.GetValue(aggregate) as System.Collections.IEnumerable;
            domainEvents.Should().NotBeNull();
            
            var count = 0;
            foreach (var _ in domainEvents)
            {
                count++;
            }
            
            count.Should().Be(expectedCount);
        }
    }
}