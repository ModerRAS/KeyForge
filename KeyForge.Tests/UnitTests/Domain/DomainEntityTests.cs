using System;
using System.Collections.Generic;
using System.Linq;
using KeyForge.Domain;
using KeyForge.Domain.Events;
using Xunit;

namespace KeyForge.Tests.UnitTests.Domain
{
    /// <summary>
    /// 领域实体和值对象的单元测试
    /// 测试KeyForge核心领域模型的行为
    /// </summary>
    public class DomainEntityTests
    {
        /// <summary>
        /// 测试用例：测试实体相等性比较
        /// 验证领域实体的相等性逻辑是否正确
        /// </summary>
        [Fact]
        public void Entity_Equals_SameId_ShouldReturnTrue()
        {
            // Arrange - 准备测试数据
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            // Act - 执行测试
            var result = entity1.Equals(entity2);

            // Assert - 验证结果
            Assert.True(result);
            Assert.True(entity1 == entity2);
            Assert.False(entity1 != entity2);
        }

        /// <summary>
        /// 测试用例：测试实体不等性比较
        /// 验证不同ID的实体是否正确返回不等
        /// </summary>
        [Fact]
        public void Entity_Equals_DifferentId_ShouldReturnFalse()
        {
            // Arrange
            var entity1 = new TestEntity(Guid.NewGuid());
            var entity2 = new TestEntity(Guid.NewGuid());

            // Act
            var result = entity1.Equals(entity2);

            // Assert
            Assert.False(result);
            Assert.False(entity1 == entity2);
            Assert.True(entity1 != entity2);
        }

        /// <summary>
        /// 测试用例：测试空实体比较
        /// 验证与null比较时的行为
        /// </summary>
        [Fact]
        public void Entity_Equals_Null_ShouldReturnFalse()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());

            // Act
            var result = entity.Equals(null);

            // Assert
            Assert.False(result);
            Assert.False(entity == null);
        }

        /// <summary>
        /// 测试用例：测试空Guid实体比较
        /// 验证空Guid的实体比较行为
        /// </summary>
        [Fact]
        public void Entity_Equals_EmptyGuid_ShouldReturnFalse()
        {
            // Arrange
            var entity1 = new TestEntity(Guid.Empty);
            var entity2 = new TestEntity(Guid.Empty);

            // Act
            var result = entity1.Equals(entity2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// 测试用例：测试值对象相等性
        /// 验证值对象的相等性比较逻辑
        /// </summary>
        [Fact]
        public void ValueObject_Equals_SameValues_ShouldReturnTrue()
        {
            // Arrange
            var value1 = new TestValueObject("test", 42);
            var value2 = new TestValueObject("test", 42);

            // Act
            var result = value1.Equals(value2);

            // Assert
            Assert.True(result);
            Assert.True(value1 == value2);
            Assert.False(value1 != value2);
        }

        /// <summary>
        /// 测试用例：测试值对象不等性
        /// 验证不同值的值对象比较
        /// </summary>
        [Fact]
        public void ValueObject_Equals_DifferentValues_ShouldReturnFalse()
        {
            // Arrange
            var value1 = new TestValueObject("test1", 42);
            var value2 = new TestValueObject("test2", 42);

            // Act
            var result = value1.Equals(value2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// 测试用例：测试聚合根领域事件
        /// 验证聚合根的领域事件管理功能
        /// </summary>
        [Fact]
        public void AggregateRoot_DomainEvents_ShouldWorkCorrectly()
        {
            // Arrange
            var aggregate = new TestAggregate(Guid.NewGuid());
            var domainEvent = new TestDomainEvent();

            // Act - 添加领域事件
            aggregate.AddTestDomainEvent(domainEvent);

            // Assert - 验证事件被添加
            Assert.Single(aggregate.DomainEvents);
            Assert.Same(domainEvent, aggregate.DomainEvents.First());

            // Act - 清除事件
            aggregate.ClearDomainEvents();

            // Assert - 验证事件被清除
            Assert.Empty(aggregate.DomainEvents);
        }

        /// <summary>
        /// 测试用例：测试实体哈希码
        /// 验证实体的哈希码生成是否正确
        /// </summary>
        [Fact]
        public void Entity_GetHashCode_SameId_ShouldReturnSameHashCode()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            // Act
            var hash1 = entity1.GetHashCode();
            var hash2 = entity2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        /// <summary>
        /// 测试用例：测试值对象哈希码
        /// 验证值对象的哈希码生成
        /// </summary>
        [Fact]
        public void ValueObject_GetHashCode_SameValues_ShouldReturnSameHashCode()
        {
            // Arrange
            var value1 = new TestValueObject("test", 42);
            var value2 = new TestValueObject("test", 42);

            // Act
            var hash1 = value1.GetHashCode();
            var hash2 = value2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        /// <summary>
        /// 测试用例：测试引用相等性
        /// 验证同一个引用的实体比较
        /// </summary>
        [Fact]
        public void Entity_Equals_SameReference_ShouldReturnTrue()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());

            // Act
            var result = entity.Equals(entity);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// 测试用例：测试类型不同比较
        /// 验证不同类型的实体比较
        /// </summary>
        [Fact]
        public void Entity_Equals_DifferentType_ShouldReturnFalse()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());
            var otherObject = new object();

            // Act
            var result = entity.Equals(otherObject);

            // Assert
            Assert.False(result);
        }
    }

    /// <summary>
    /// 测试用的实体类
    /// 用于测试Entity基类的功能
    /// </summary>
    internal class TestEntity : Entity
    {
        public TestEntity(Guid id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// 测试用的聚合根类
    /// 用于测试AggregateRoot基类的功能
    /// </summary>
    internal class TestAggregate : AggregateRoot
    {
        public TestAggregate(Guid id)
        {
            Id = id;
        }

        // 公开受保护的AddDomainEvent方法用于测试
        public void AddTestDomainEvent(IDomainEvent domainEvent)
        {
            // 使用反射调用受保护的方法
            var method = typeof(AggregateRoot).GetMethod("AddDomainEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(this, new object[] { domainEvent });
        }
    }

    /// <summary>
    /// 测试用的值对象类
    /// 用于测试ValueObject基类的功能
    /// </summary>
    internal class TestValueObject : ValueObject
    {
        public string Name { get; }
        public int Number { get; }

        public TestValueObject(string name, int number)
        {
            Name = name;
            Number = number;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Number;
        }
    }

    /// <summary>
    /// 测试用的领域事件
    /// 用于测试领域事件功能
    /// </summary>
    internal class TestDomainEvent : DomainEvent
    {
        public string EventType => "TestDomainEvent";
    }
}