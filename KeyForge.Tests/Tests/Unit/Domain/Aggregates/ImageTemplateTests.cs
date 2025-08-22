using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.Unit.Domain.Aggregates;

/// <summary>
/// 图像模板聚合根单元测试
/// 原本实现：复杂的图像处理测试
/// 简化实现：核心功能测试
/// </summary>
public class ImageTemplateTests : TestBase
{
    public ImageTemplateTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreateImageTemplate_WithValidData_ShouldCreateTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var imagePath = "test_template.png";
        var region = new System.Drawing.Rectangle(0, 0, 100, 100);
        var threshold = 0.8;
        
        // Act
        var template = new ImageTemplate(templateId, imagePath, region, threshold);
        
        // Assert
        template.Should().NotBeNull();
        template.Id.Should().Be(templateId);
        template.ImagePath.Should().Be(imagePath);
        template.Region.Should().Be(region);
        template.Threshold.Should().Be(threshold);
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        template.IsActive.Should().BeTrue();
        Log($"创建图像模板成功: {template.Id}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateImageTemplate_WithInvalidImagePath_ShouldThrowException(string invalidPath)
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var region = new System.Drawing.Rectangle(0, 0, 100, 100);
        var threshold = 0.8;
        
        // Act & Assert
        var action = () => new ImageTemplate(templateId, invalidPath, region, threshold);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效图像路径: {invalidPath}");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void CreateImageTemplate_WithInvalidThreshold_ShouldThrowException(double invalidThreshold)
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var imagePath = "test_template.png";
        var region = new System.Drawing.Rectangle(0, 0, 100, 100);
        
        // Act & Assert
        var action = () => new ImageTemplate(templateId, imagePath, region, invalidThreshold);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效阈值: {invalidThreshold}");
    }

    [Fact]
    public void UpdateRegion_WithValidRegion_ShouldUpdateRegion()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var newRegion = new System.Drawing.Rectangle(50, 50, 200, 200);
        
        // Act
        template.UpdateRegion(newRegion);
        
        // Assert
        template.Region.Should().Be(newRegion);
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新模板区域成功: {newRegion}");
    }

    [Fact]
    public void UpdateThreshold_WithValidThreshold_ShouldUpdateThreshold()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var newThreshold = 0.9;
        
        // Act
        template.UpdateThreshold(newThreshold);
        
        // Assert
        template.Threshold.Should().Be(newThreshold);
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新模板阈值成功: {newThreshold}");
    }

    [Fact]
    public void UpdateThreshold_WithInvalidThreshold_ShouldThrowException()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        
        // Act & Assert
        var action = () => template.UpdateThreshold(1.5);
        action.Should().Throw<ArgumentException>();
        Log($"验证无效阈值更新失败");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        template.Deactivate(); // 先停用
        
        // Act
        template.Activate();
        
        // Assert
        template.IsActive.Should().BeTrue();
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"激活图像模板成功: {template.Id}");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        
        // Act
        template.Deactivate();
        
        // Assert
        template.IsActive.Should().BeFalse();
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"停用图像模板成功: {template.Id}");
    }

    [Fact]
    public void UpdateUsageCount_ShouldIncrementCount()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var initialCount = template.UsageCount;
        
        // Act
        template.UpdateUsageCount();
        
        // Assert
        template.UsageCount.Should().Be(initialCount + 1);
        template.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        Log($"更新使用次数成功: {template.UsageCount}");
    }

    [Fact]
    public void ResetUsageCount_ShouldSetCountToZero()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        template.UpdateUsageCount();
        template.UpdateUsageCount();
        
        // Act
        template.ResetUsageCount();
        
        // Assert
        template.UsageCount.Should().Be(0);
        template.LastUsedAt.Should().BeNull();
        Log($"重置使用次数成功");
    }

    [Fact]
    public void IsMatch_WithHighConfidence_ShouldReturnTrue()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var confidence = 0.85; // 高于阈值0.8
        
        // Act
        var result = template.IsMatch(confidence);
        
        // Assert
        result.Should().BeTrue();
        Log($"验证高置信度匹配: {confidence} = {result}");
    }

    [Fact]
    public void IsMatch_WithLowConfidence_ShouldReturnFalse()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var confidence = 0.75; // 低于阈值0.8
        
        // Act
        var result = template.IsMatch(confidence);
        
        // Assert
        result.Should().BeFalse();
        Log($"验证低置信度匹配: {confidence} = {result}");
    }

    [Fact]
    public void IsMatch_WithBoundaryConfidence_ShouldReturnTrue()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var confidence = 0.8; // 等于阈值0.8
        
        // Act
        var result = template.IsMatch(confidence);
        
        // Assert
        result.Should().BeTrue();
        Log($"验证边界置信度匹配: {confidence} = {result}");
    }

    [Fact]
    public void UpdateLastUsedAt_WithValidDateTime_ShouldUpdateTimestamp()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var newTimestamp = DateTime.UtcNow;
        
        // Act
        template.UpdateLastUsedAt(newTimestamp);
        
        // Assert
        template.LastUsedAt.Should().Be(newTimestamp);
        Log($"更新最后使用时间成功: {newTimestamp}");
    }

    [Fact]
    public void GetArea_ShouldReturnCorrectArea()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        template.UpdateRegion(new System.Drawing.Rectangle(10, 20, 100, 50));
        
        // Act
        var area = template.GetArea();
        
        // Assert
        area.Should().Be(5000); // 100 * 50
        Log($"计算模板面积成功: {area}");
    }

    [Fact]
    public void GetCenterPoint_ShouldReturnCorrectCenter()
    {
        // Arrange
        var template = TestFixtures.CreateValidImageTemplate();
        var region = new System.Drawing.Rectangle(10, 20, 100, 50);
        template.UpdateRegion(region);
        
        // Act
        var center = template.GetCenterPoint();
        
        // Assert
        center.X.Should().Be(60); // 10 + 100/2
        center.Y.Should().Be(45); // 20 + 50/2
        Log($"计算模板中心点成功: ({center.X}, {center.Y})");
    }

    [Fact]
    public void ImageTemplate_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template1 = new ImageTemplate(templateId, "test1.png", new System.Drawing.Rectangle(0, 0, 100, 100), 0.8);
        var template2 = new ImageTemplate(templateId, "test2.png", new System.Drawing.Rectangle(50, 50, 200, 200), 0.9);
        var template3 = new ImageTemplate(Guid.NewGuid(), "test1.png", new System.Drawing.Rectangle(0, 0, 100, 100), 0.8);
        
        // Act & Assert
        template1.Should().Be(template2); // Same ID
        template1.Should().NotBe(template3); // Different ID
        (template1 == template2).Should().BeTrue();
        (template1 != template3).Should().BeTrue();
        Log($"验证图像模板相等性: 相同ID={template1.Id == template2.Id}, 不同ID={template1.Id != template3.Id}");
    }
}