using Xunit;
using FluentAssertions;
using System;

namespace KeyForge.Tests.UnitTests.Basic
{
    public class BasicTests
    {
        [Fact]
        public void Test_Addition_ShouldWorkCorrectly()
        {
            // Arrange
            int a = 2;
            int b = 3;
            
            // Act
            int result = a + b;
            
            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public void Test_StringConcatenation_ShouldWorkCorrectly()
        {
            // Arrange
            string hello = "Hello";
            string world = "World";
            
            // Act
            string result = hello + " " + world;
            
            // Assert
            result.Should().Be("Hello World");
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(5, 5, 10)]
        [InlineData(-1, 1, 0)]
        public void Test_Addition_WithVariousInputs_ShouldReturnExpectedResult(int a, int b, int expected)
        {
            // Act
            int result = a + b;
            
            // Assert
            result.Should().Be(expected);
        }
    }
}