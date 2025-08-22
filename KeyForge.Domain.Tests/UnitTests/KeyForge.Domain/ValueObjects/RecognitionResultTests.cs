using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using Rectangle = KeyForge.Domain.Common.Rectangle;

namespace KeyForge.Domain.Tests.ValueObjects
{
    /// <summary>
    /// RecognitionResult值对象的单元测试
    /// 测试RecognitionResult的业务逻辑
    /// 简化实现：只测试现有的功能
    /// </summary>
    public class RecognitionResultTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateRecognitionResult()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(10, 10, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var result = new RecognitionResult(isMatch, matchArea, confidence, templateName);

            // Assert
            result.Should().NotBeNull();
            result.IsMatch.Should().Be(isMatch);
            result.MatchArea.Should().Be(matchArea);
            result.Confidence.Should().Be(confidence);
            result.TemplateName.Should().Be(templateName);
            result.RecognizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public void Constructor_WithInvalidConfidence_ShouldThrowException(double invalidConfidence)
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(10, 10, 100, 100);
            var templateName = "Test Template";

            // Act & Assert
            var action = () => RecognitionResult.Match(matchArea, invalidConfidence, templateName);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Confidence must be between 0 and 1.*");
        }

        [Fact]
        public void StaticFactory_NoMatch_ShouldCreateNoMatchResult()
        {
            // Arrange
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.NoMatch(templateName);

            // Assert
            result.Should().NotBeNull();
            result.IsMatch.Should().BeFalse();
            result.MatchArea.Should().Be(Rectangle.Empty);
            result.Confidence.Should().Be(0);
            result.TemplateName.Should().Be(templateName);
        }

        [Fact]
        public void StaticFactory_Match_ShouldCreateMatchResult()
        {
            // Arrange
            var matchArea = new Rectangle(10, 10, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.Match(matchArea, confidence, templateName);

            // Assert
            result.Should().NotBeNull();
            result.IsMatch.Should().BeTrue();
            result.MatchArea.Should().Be(matchArea);
            result.Confidence.Should().Be(confidence);
            result.TemplateName.Should().Be(templateName);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(10, 10, 100, 100), 0.85, "Test Template");

            // Act
            var resultString = result.ToString();

            // Assert
            resultString.Should().Contain("RecognitionResult");
            resultString.Should().Contain("Match=True");
            resultString.Should().Contain("Confidence=0.85");
        }
    }
}