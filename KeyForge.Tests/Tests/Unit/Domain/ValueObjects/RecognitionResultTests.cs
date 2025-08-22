using Xunit;
using FluentAssertions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// 识别结果值对象单元测试
/// 原本实现：复杂的识别算法测试
/// 简化实现：核心功能测试
/// </summary>
public class RecognitionResultTests : TestBase
{
    public RecognitionResultTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateRecognitionResult_WithSuccess_ShouldCreateResult()
    {
        // Arrange
        var isSuccessful = true;
        var confidence = 0.85;
        var matchedRegion = new System.Drawing.Rectangle(100, 200, 50, 30);
        var processingTime = 50;
        
        // Act
        var result = new RecognitionResult(isSuccessful, confidence, matchedRegion, processingTime);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().Be(isSuccessful);
        result.Confidence.Should().Be(confidence);
        result.MatchedRegion.Should().Be(matchedRegion);
        result.ProcessingTimeMs.Should().Be(processingTime);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"创建成功识别结果: {confidence} 置信度");
    }

    [Fact]
    public void CreateRecognitionResult_WithFailure_ShouldCreateResult()
    {
        // Arrange
        var isSuccessful = false;
        var confidence = 0.0;
        var processingTime = 30;
        
        // Act
        var result = new RecognitionResult(isSuccessful, confidence, null, processingTime);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().Be(isSuccessful);
        result.Confidence.Should().Be(confidence);
        result.MatchedRegion.Should().BeNull();
        result.ProcessingTimeMs.Should().Be(processingTime);
        Log($"创建失败识别结果: {processingTime}ms 处理时间");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void CreateRecognitionResult_WithInvalidConfidence_ShouldThrowException(double invalidConfidence)
    {
        // Arrange
        var isSuccessful = true;
        var matchedRegion = new System.Drawing.Rectangle(100, 200, 50, 30);
        var processingTime = 50;
        
        // Act & Assert
        var action = () => new RecognitionResult(isSuccessful, invalidConfidence, matchedRegion, processingTime);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效置信度: {invalidConfidence}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CreateRecognitionResult_WithInvalidProcessingTime_ShouldThrowException(int invalidTime)
    {
        // Arrange
        var isSuccessful = true;
        var confidence = 0.85;
        var matchedRegion = new System.Drawing.Rectangle(100, 200, 50, 30);
        
        // Act & Assert
        var action = () => new RecognitionResult(isSuccessful, confidence, matchedRegion, invalidTime);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效处理时间: {invalidTime}");
    }

    [Fact]
    public void CreateSuccess_WithValidParameters_ShouldReturnSuccessResult()
    {
        // Arrange
        var confidence = 0.9;
        var matchedRegion = new System.Drawing.Rectangle(100, 200, 50, 30);
        var processingTime = 45;
        
        // Act
        var result = RecognitionResult.CreateSuccess(confidence, matchedRegion, processingTime);
        
        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Confidence.Should().Be(confidence);
        result.MatchedRegion.Should().Be(matchedRegion);
        result.ProcessingTimeMs.Should().Be(processingTime);
        Log($"创建成功识别结果 (工厂方法): {confidence} 置信度");
    }

    [Fact]
    public void CreateFailure_WithValidParameters_ShouldReturnFailureResult()
    {
        // Arrange
        var processingTime = 35;
        
        // Act
        var result = RecognitionResult.CreateFailure(processingTime);
        
        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Confidence.Should().Be(0.0);
        result.MatchedRegion.Should().BeNull();
        result.ProcessingTimeMs.Should().Be(processingTime);
        Log($"创建失败识别结果 (工厂方法): {processingTime}ms 处理时间");
    }

    [Fact]
    public void IsHighConfidence_WithConfidenceAboveThreshold_ShouldReturnTrue()
    {
        // Arrange
        var result = new RecognitionResult(true, 0.85, new System.Drawing.Rectangle(0, 0, 100, 100), 50);
        
        // Act
        var isHighConfidence = result.IsHighConfidence();
        
        // Assert
        isHighConfidence.Should().BeTrue();
        Log($"验证高置信度: 0.85 = {isHighConfidence}");
    }

    [Fact]
    public void IsHighConfidence_WithConfidenceBelowThreshold_ShouldReturnFalse()
    {
        // Arrange
        var result = new RecognitionResult(true, 0.75, new System.Drawing.Rectangle(0, 0, 100, 100), 50);
        
        // Act
        var isHighConfidence = result.IsHighConfidence();
        
        // Assert
        isHighConfidence.Should().BeFalse();
        Log($"验证低置信度: 0.75 = {isHighConfidence}");
    }

    [Fact]
    public void IsHighConfidence_WithCustomThreshold_ShouldUseCustomThreshold()
    {
        // Arrange
        var result = new RecognitionResult(true, 0.85, new System.Drawing.Rectangle(0, 0, 100, 100), 50);
        var customThreshold = 0.9;
        
        // Act
        var isHighConfidence = result.IsHighConfidence(customThreshold);
        
        // Assert
        isHighConfidence.Should().BeFalse();
        Log($"验证自定义阈值置信度: 0.85 (阈值: {customThreshold}) = {isHighConfidence}");
    }

    [Fact]
    public void GetMatchCenter_WithValidRegion_ShouldReturnCenterPoint()
    {
        // Arrange
        var region = new System.Drawing.Rectangle(100, 200, 50, 30);
        var result = new RecognitionResult(true, 0.85, region, 50);
        
        // Act
        var center = result.GetMatchCenter();
        
        // Assert
        center.X.Should().Be(125); // 100 + 50/2
        center.Y.Should().Be(215); // 200 + 30/2
        Log($"计算匹配中心点: ({center.X}, {center.Y})");
    }

    [Fact]
    public void GetMatchCenter_WithNullRegion_ShouldThrowException()
    {
        // Arrange
        var result = new RecognitionResult(false, 0.0, null, 50);
        
        // Act & Assert
        var action = () => result.GetMatchCenter();
        action.Should().Throw<InvalidOperationException>();
        Log($"验证空区域计算中心点失败");
    }

    [Fact]
    public void GetMatchArea_WithValidRegion_ShouldReturnArea()
    {
        // Arrange
        var region = new System.Drawing.Rectangle(100, 200, 50, 30);
        var result = new RecognitionResult(true, 0.85, region, 50);
        
        // Act
        var area = result.GetMatchArea();
        
        // Assert
        area.Should().Be(1500); // 50 * 30
        Log($"计算匹配区域面积: {area}");
    }

    [Fact]
    public void GetMatchArea_WithNullRegion_ShouldReturnZero()
    {
        // Arrange
        var result = new RecognitionResult(false, 0.0, null, 50);
        
        // Act
        var area = result.GetMatchArea();
        
        // Assert
        area.Should().Be(0);
        Log($"验证空区域面积为: {area}");
    }

    [Fact]
    public void ToString_WithSuccessResult_ShouldReturnSuccessString()
    {
        // Arrange
        var result = new RecognitionResult(true, 0.85, new System.Drawing.Rectangle(100, 200, 50, 30), 50);
        
        // Act
        var resultString = result.ToString();
        
        // Assert
        resultString.Should().Contain("Success");
        resultString.Should().Contain("0.85");
        resultString.Should().Contain("50ms");
        Log($"识别结果字符串: {resultString}");
    }

    [Fact]
    public void ToString_WithFailureResult_ShouldReturnFailureString()
    {
        // Arrange
        var result = new RecognitionResult(false, 0.0, null, 50);
        
        // Act
        var resultString = result.ToString();
        
        // Assert
        resultString.Should().Contain("Failed");
        resultString.Should().Contain("50ms");
        Log($"识别结果字符串: {resultString}");
    }

    [Fact]
    public void RecognitionResult_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var region = new System.Drawing.Rectangle(100, 200, 50, 30);
        var timestamp = DateTime.UtcNow;
        
        var result1 = new RecognitionResult(true, 0.85, region, 50);
        var result2 = new RecognitionResult(true, 0.85, region, 50);
        var result3 = new RecognitionResult(true, 0.9, region, 50);
        
        // Act & Assert
        result1.Should().Be(result2); // Same values
        result1.Should().NotBe(result3); // Different confidence
        (result1 == result2).Should().BeTrue();
        (result1 != result3).Should().BeTrue();
        Log($"验证识别结果相等性: 相同值={result1.IsSuccessful == result2.IsSuccessful}, 不同值={result1.Confidence != result3.Confidence}");
    }

    [Fact]
    public void RecognitionResult_GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var region = new System.Drawing.Rectangle(100, 200, 50, 30);
        var result1 = new RecognitionResult(true, 0.85, region, 50);
        var result2 = new RecognitionResult(true, 0.85, region, 50);
        
        // Act & Assert
        result1.GetHashCode().Should().Be(result2.GetHashCode());
        Log($"验证识别结果哈希码一致性: {result1.GetHashCode()}");
    }
}