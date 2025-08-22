using System;
using System.Drawing;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Tests.UnitTests.ValueObjects
{
    /// <summary>
    /// RecognitionResult值对象单元测试
    /// 测试识别结果的所有业务规则和不变性
    /// </summary>
    public class RecognitionResultTests : TestBase
    {
        #region 构造函数测试

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRecognitionResult()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var result = new RecognitionResult(isMatch, matchArea, confidence, templateName);

            // Assert
            result.IsMatch.Should().Be(isMatch);
            result.MatchArea.Should().Be(matchArea);
            result.Confidence.Should().Be(confidence);
            result.TemplateName.Should().Be(templateName);
            result.RecognizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_ShouldSetRecognizedAtToCurrentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var result = new RecognitionResult(isMatch, matchArea, confidence, templateName);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            result.RecognizedAt.Should().BeAfter(beforeCreation);
            result.RecognizedAt.Should().BeBefore(afterCreation);
        }

        #endregion

        #region NoMatch静态方法测试

        [Fact]
        public void NoMatch_ShouldCreateNoMatchResult()
        {
            // Arrange
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.NoMatch(templateName);

            // Assert
            result.IsMatch.Should().BeFalse();
            result.Confidence.Should().Be(0);
            result.TemplateName.Should().Be(templateName);
            result.MatchArea.Should().Be(Rectangle.Empty);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NoMatch_WithInvalidTemplateName_ShouldStillCreateResult(string templateName)
        {
            // Act
            var result = RecognitionResult.NoMatch(templateName);

            // Assert
            result.IsMatch.Should().BeFalse();
            result.Confidence.Should().Be(0);
            result.TemplateName.Should().Be(templateName);
            result.MatchArea.Should().Be(Rectangle.Empty);
        }

        #endregion

        #region Match静态方法测试

        [Fact]
        public void Match_WithValidParameters_ShouldCreateMatchResult()
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.Match(matchArea, confidence, templateName);

            // Assert
            result.IsMatch.Should().BeTrue();
            result.MatchArea.Should().Be(matchArea);
            result.Confidence.Should().Be(confidence);
            result.TemplateName.Should().Be(templateName);
            result.RecognizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(-1.0)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Match_WithInvalidConfidence_ShouldThrowArgumentException(double invalidConfidence)
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var templateName = "Test Template";

            // Act & Assert
            var action = () => RecognitionResult.Match(matchArea, invalidConfidence, templateName);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Confidence must be between 0 and 1.*")
                .WithParameterName("confidence");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void Match_WithValidConfidence_ShouldCreateResult(double validConfidence)
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.Match(matchArea, validConfidence, templateName);

            // Assert
            result.IsMatch.Should().BeTrue();
            result.Confidence.Should().Be(validConfidence);
            result.TemplateName.Should().Be(templateName);
        }

        #endregion

        #region 值对象相等性测试

        [Fact]
        public void RecognitionResult_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";
            var recognizedAt = DateTime.UtcNow;

            var result1 = new RecognitionResult(isMatch, matchArea, confidence, templateName);
            var result2 = new RecognitionResult(isMatch, matchArea, confidence, templateName);

            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(result1, recognizedAt);
            recognizedAtProperty.SetValue(result2, recognizedAt);

            // Act & Assert
            result1.Should().Be(result2);
            result1.GetHashCode().Should().Be(result2.GetHashCode());
            (result1 == result2).Should().BeTrue();
            (result1 != result2).Should().BeFalse();
        }

        [Fact]
        public void RecognitionResult_WithDifferentIsMatch_ShouldNotBeEqual()
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";
            var recognizedAt = DateTime.UtcNow;

            var result1 = new RecognitionResult(true, matchArea, confidence, templateName);
            var result2 = new RecognitionResult(false, matchArea, confidence, templateName);

            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(result1, recognizedAt);
            recognizedAtProperty.SetValue(result2, recognizedAt);

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
            (result1 == result2).Should().BeFalse();
            (result1 != result2).Should().BeTrue();
        }

        [Fact]
        public void RecognitionResult_WithDifferentMatchArea_ShouldNotBeEqual()
        {
            // Arrange
            var isMatch = true;
            var confidence = 0.85;
            var templateName = "Test Template";
            var recognizedAt = DateTime.UtcNow;

            var result1 = new RecognitionResult(isMatch, new Rectangle(50, 50, 100, 100), confidence, templateName);
            var result2 = new RecognitionResult(isMatch, new Rectangle(60, 60, 100, 100), confidence, templateName);

            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(result1, recognizedAt);
            recognizedAtProperty.SetValue(result2, recognizedAt);

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
        }

        [Fact]
        public void RecognitionResult_WithDifferentConfidence_ShouldNotBeEqual()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var templateName = "Test Template";
            var recognizedAt = DateTime.UtcNow;

            var result1 = new RecognitionResult(isMatch, matchArea, 0.85, templateName);
            var result2 = new RecognitionResult(isMatch, matchArea, 0.90, templateName);

            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(result1, recognizedAt);
            recognizedAtProperty.SetValue(result2, recognizedAt);

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
        }

        [Fact]
        public void RecognitionResult_WithDifferentTemplateName_ShouldNotBeEqual()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var recognizedAt = DateTime.UtcNow;

            var result1 = new RecognitionResult(isMatch, matchArea, confidence, "Template1");
            var result2 = new RecognitionResult(isMatch, matchArea, confidence, "Template2");

            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(result1, recognizedAt);
            recognizedAtProperty.SetValue(result2, recognizedAt);

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
        }

        [Fact]
        public void RecognitionResult_WithDifferentRecognizedAt_ShouldNotBeEqual()
        {
            // Arrange
            var isMatch = true;
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            var result1 = new RecognitionResult(isMatch, matchArea, confidence, templateName);
            var result2 = new RecognitionResult(isMatch, matchArea, confidence, templateName);

            // Act & Assert
            result1.Should().NotBe(result2);
            result1.GetHashCode().Should().NotBe(result2.GetHashCode());
        }

        #endregion

        #region 空值和类型比较测试

        [Fact]
        public void RecognitionResult_ShouldNotEqualNull()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test");

            // Act & Assert
            result.Equals(null).Should().BeFalse();
            (result == null).Should().BeFalse();
            (null == result).Should().BeFalse();
        }

        [Fact]
        public void RecognitionResult_ShouldNotEqualDifferentType()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test");
            var otherObject = new object();

            // Act & Assert
            result.Equals(otherObject).Should().BeFalse();
        }

        [Fact]
        public void RecognitionResult_ShouldEqualItself()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test");

            // Act & Assert
            result.Equals(result).Should().BeTrue();
            (result == result).Should().BeTrue();
            (result != result).Should().BeFalse();
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void RecognitionResult_ShouldHandleExtremeConfidenceValues()
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var templateName = "Test Template";

            // Act & Assert
            var result1 = RecognitionResult.Match(matchArea, 0.0, templateName);
            result1.Confidence.Should().Be(0.0);

            var result2 = RecognitionResult.Match(matchArea, 1.0, templateName);
            result2.Confidence.Should().Be(1.0);

            var result3 = RecognitionResult.Match(matchArea, 0.0001, templateName);
            result3.Confidence.Should().Be(0.0001);

            var result4 = RecognitionResult.Match(matchArea, 0.9999, templateName);
            result4.Confidence.Should().Be(0.9999);
        }

        [Fact]
        public void RecognitionResult_ShouldHandleEmptyRectangle()
        {
            // Arrange
            var templateName = "Test Template";

            // Act & Assert
            var result1 = RecognitionResult.NoMatch(templateName);
            result1.MatchArea.Should().Be(Rectangle.Empty);

            var result2 = RecognitionResult.Match(Rectangle.Empty, 0.85, templateName);
            result2.MatchArea.Should().Be(Rectangle.Empty);
        }

        [Fact]
        public void RecognitionResult_ShouldHandleLargeRectangle()
        {
            // Arrange
            var largeRectangle = new Rectangle(0, 0, 10000, 10000);
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.Match(largeRectangle, 0.85, templateName);

            // Assert
            result.MatchArea.Should().Be(largeRectangle);
            result.MatchArea.Width.Should().Be(10000);
            result.MatchArea.Height.Should().Be(10000);
        }

        [Fact]
        public void RecognitionResult_ShouldHandleNegativeCoordinates()
        {
            // Arrange
            var negativeRectangle = new Rectangle(-100, -100, 50, 50);
            var templateName = "Test Template";

            // Act
            var result = RecognitionResult.Match(negativeRectangle, 0.85, templateName);

            // Assert
            result.MatchArea.Should().Be(negativeRectangle);
            result.MatchArea.X.Should().Be(-100);
            result.MatchArea.Y.Should().Be(-100);
        }

        #endregion

        #region 字符串表示测试

        [Fact]
        public void ToString_ShouldReturnCorrectStringRepresentation()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test Template");

            // Act
            var stringRepresentation = result.ToString();

            // Assert
            stringRepresentation.Should().Be("RecognitionResult: Match=True, Confidence=0.85, Template=Test Template");
        }

        [Fact]
        public void ToString_ShouldHandleNoMatchCase()
        {
            // Arrange
            var result = RecognitionResult.NoMatch("Test Template");

            // Act
            var stringRepresentation = result.ToString();

            // Assert
            stringRepresentation.Should().Be("RecognitionResult: Match=False, Confidence=0.00, Template=Test Template");
        }

        [Fact]
        public void ToString_ShouldFormatConfidenceCorrectly()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.8567, "Test Template");

            // Act
            var stringRepresentation = result.ToString();

            // Assert
            stringRepresentation.Should().Be("RecognitionResult: Match=True, Confidence=0.86, Template=Test Template");
        }

        #endregion

        #region 不变性测试

        [Fact]
        public void RecognitionResult_ShouldBeImmutable()
        {
            // Arrange
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test Template");

            // Act & Assert
            // 验证所有属性都是只读的
            var properties = typeof(RecognitionResult).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name != "EqualityComparer") // 排除比较器属性
                {
                    property.CanWrite.Should().BeFalse($"Property {property.Name} should be read-only");
                }
            }
        }

        [Fact]
        public void RecognitionResult_ShouldMaintainValueSemantics()
        {
            // Arrange
            var originalResult = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test Template");
            
            // 创建具有相同值的新实例
            var equalResult = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, "Test Template");
            
            // 使用反射设置相同的RecognizedAt时间
            var recognizedAtProperty = typeof(RecognitionResult).GetProperty("RecognizedAt");
            recognizedAtProperty.SetValue(equalResult, originalResult.RecognizedAt);

            // Act & Assert
            // 它们应该相等，但不是同一个引用
            originalResult.Should().Be(equalResult);
            originalResult.Should().NotBeSameAs(equalResult);
            
            // 验证值对象语义：相等的对象应该产生相同的哈希码
            originalResult.GetHashCode().Should().Be(equalResult.GetHashCode());
        }

        #endregion

        #region 静态工厂方法验证

        [Fact]
        public void StaticFactoryMethods_ShouldCreateCorrectResults()
        {
            // Arrange
            var matchArea = new Rectangle(50, 50, 100, 100);
            var confidence = 0.85;
            var templateName = "Test Template";

            // Act
            var noMatchResult = RecognitionResult.NoMatch(templateName);
            var matchResult = RecognitionResult.Match(matchArea, confidence, templateName);

            // Assert
            // NoMatch验证
            noMatchResult.IsMatch.Should().BeFalse();
            noMatchResult.Confidence.Should().Be(0);
            noMatchResult.MatchArea.Should().Be(Rectangle.Empty);
            noMatchResult.TemplateName.Should().Be(templateName);

            // Match验证
            matchResult.IsMatch.Should().BeTrue();
            matchResult.Confidence.Should().Be(confidence);
            matchResult.MatchArea.Should().Be(matchArea);
            matchResult.TemplateName.Should().Be(templateName);
        }

        [Fact]
        public void StaticFactoryMethods_ShouldHandleEdgeCases()
        {
            // Arrange
            var emptyTemplateName = "";
            var nullTemplateName = null;

            // Act & Assert
            // NoMatch方法应该处理空和null模板名称
            var noMatchResult1 = RecognitionResult.NoMatch(emptyTemplateName);
            noMatchResult1.TemplateName.Should().Be(emptyTemplateName);

            var noMatchResult2 = RecognitionResult.NoMatch(nullTemplateName);
            noMatchResult2.TemplateName.Should().BeNull();

            // Match方法应该处理空和null模板名称
            var matchResult1 = RecognitionResult.Match(Rectangle.Empty, 0.5, emptyTemplateName);
            matchResult1.TemplateName.Should().Be(emptyTemplateName);

            var matchResult2 = RecognitionResult.Match(Rectangle.Empty, 0.5, nullTemplateName);
            matchResult2.TemplateName.Should().BeNull();
        }

        #endregion
    }
}