using System;
using System.Drawing;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Exceptions;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 简化的图像模板聚合根测试
    /// 专门为跨平台环境设计，避免Windows特定的依赖
    /// </summary>
    public class SimplifiedImageTemplateTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateTemplate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test template description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);
            var matchThreshold = 0.8;

            // Act
            var template = new ImageTemplate(id, name, description, templateData, templateArea, matchThreshold);

            // Assert
            template.Should().NotBeNull();
            template.Id.Should().Be(id);
            template.Name.Should().Be(name);
            template.Description.Should().Be(description);
            template.TemplateData.Should().BeEquivalentTo(templateData);
            template.TemplateArea.Should().Be(templateArea);
            template.MatchThreshold.Should().Be(matchThreshold);
            template.IsActive.Should().BeTrue();
            template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "";
            var description = "Test template description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(id, name, description, templateData, templateArea);
            action.Should().Throw<ValidationException>()
                .WithMessage("Template name cannot be empty.");
        }

        [Fact]
        public void Constructor_WithEmptyTemplateData_ShouldThrowValidationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test template description";
            var templateData = Array.Empty<byte>();
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(id, name, description, templateData, templateArea);
            action.Should().Throw<ValidationException>()
                .WithMessage("Template data cannot be empty.");
        }

        [Fact]
        public void Constructor_WithInvalidMatchThreshold_ShouldThrowValidationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test template description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);
            var invalidThreshold = 1.5; // 超出范围

            // Act & Assert
            var action = () => new ImageTemplate(id, name, description, templateData, templateArea, invalidThreshold);
            action.Should().Throw<ValidationException>()
                .WithMessage("Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Original Name", 
                "Original description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));
            
            var newName = "Updated Name";
            var newDescription = "Updated description";
            var newArea = new Rectangle(10, 10, 200, 200);
            var newThreshold = 0.9;

            // Act
            template.Update(newName, newDescription, newArea, newThreshold);

            // Assert
            template.Name.Should().Be(newName);
            template.Description.Should().Be(newDescription);
            template.TemplateArea.Should().Be(newArea);
            template.MatchThreshold.Should().Be(newThreshold);
            template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateTemplateData_WithValidData_ShouldUpdateData()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));
            
            var newData = new byte[] { 4, 5, 6, 7, 8 };

            // Act
            template.UpdateTemplateData(newData);

            // Assert
            template.TemplateData.Should().BeEquivalentTo(newData);
            template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateTemplateData_WithEmptyData_ShouldThrowValidationException()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));

            // Act & Assert
            var action = () => template.UpdateTemplateData(Array.Empty<byte>());
            action.Should().Throw<ValidationException>()
                .WithMessage("Template data cannot be empty.");
        }

        [Fact]
        public void Activate_WhenInactive_ShouldActivateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));
            
            template.Deactivate();

            // Act
            template.Activate();

            // Assert
            template.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));

            // Act
            template.Deactivate();

            // Assert
            template.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Delete_ShouldDeactivateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));

            // Act
            template.Delete();

            // Assert
            template.IsActive.Should().BeFalse();
        }

        [Fact]
        public void IsMatch_WithMatchingResult_ShouldReturnTrue()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100),
                0.8);
            
            var result = new RecognitionResult(true, 0.9, new Rectangle(0, 0, 100, 100));

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void IsMatch_WithNonMatchingResult_ShouldReturnFalse()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100),
                0.8);
            
            var result = new RecognitionResult(true, 0.7, new Rectangle(0, 0, 100, 100));

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsMatch_WithNullResult_ShouldReturnFalse()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));

            // Act
            var isMatch = template.IsMatch(null);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsAreaMatch_WithIntersectingArea_ShouldReturnTrue()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));
            
            var searchArea = new Rectangle(50, 50, 100, 100);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithNonIntersectingArea_ShouldReturnFalse()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(), 
                "Test Template", 
                "Test description", 
                new byte[] { 1, 2, 3 }, 
                new Rectangle(0, 0, 100, 100));
            
            var searchArea = new Rectangle(200, 200, 100, 100);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeFalse();
        }
    }

    /// <summary>
    /// 简化的识别结果类，用于测试
    /// </summary>
    public class RecognitionResult
    {
        public bool IsMatch { get; }
        public double Confidence { get; }
        public Rectangle MatchArea { get; }

        public RecognitionResult(bool isMatch, double confidence, Rectangle matchArea)
        {
            IsMatch = isMatch;
            Confidence = confidence;
            MatchArea = matchArea;
        }
    }
}