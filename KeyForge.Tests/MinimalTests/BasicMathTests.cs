using Xunit;
using FluentAssertions;
using System;

namespace KeyForge.Tests.Minimal
{
    public class BasicMathTests
    {
        [Fact]
        public void Add_TwoNumbers_ShouldReturnCorrectSum()
        {
            // Arrange
            int a = 5;
            int b = 3;
            
            // Act
            int result = a + b;
            
            // Assert
            result.Should().Be(8);
        }

        [Fact]
        public void Subtract_TwoNumbers_ShouldReturnCorrectDifference()
        {
            // Arrange
            int a = 10;
            int b = 4;
            
            // Act
            int result = a - b;
            
            // Assert
            result.Should().Be(6);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 2)]
        [InlineData(-1, 1, 0)]
        [InlineData(100, 200, 300)]
        public void Add_WithVariousInputs_ShouldReturnExpectedResult(int a, int b, int expected)
        {
            // Act
            int result = a + b;
            
            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void StringOperations_Concatenate_ShouldWorkCorrectly()
        {
            // Arrange
            string firstName = "John";
            string lastName = "Doe";
            
            // Act
            string fullName = $"{firstName} {lastName}";
            
            // Assert
            fullName.Should().Be("John Doe");
            fullName.Should().Contain("John");
            fullName.Should().EndWith("Doe");
        }

        [Fact]
        public void BooleanOperations_LogicalAnd_ShouldWorkCorrectly()
        {
            // Arrange
            bool trueValue = true;
            bool falseValue = false;
            
            // Act & Assert
            (trueValue && trueValue).Should().BeTrue();
            (trueValue && falseValue).Should().BeFalse();
            (falseValue && falseValue).Should().BeFalse();
        }
    }
}