using System;
using System.Drawing;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.UnitTests.Domain
{
    /// <summary>
    /// 图像模板聚合根单元测试
    /// </summary>
    public class ImageTemplateTests : TestBase
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateImageTemplate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "A test template";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);
            var matchThreshold = 0.8;
            var templateType = TemplateType.Image;

            // Act
            var template = new ImageTemplate(id, name, description, templateData, templateArea, matchThreshold, templateType);

            // Assert
            ShouldBeValidImageTemplate(template);
            template.Name.Should().Be(name);
            template.Description.Should().Be(description);
            template.TemplateData.Should().BeEquivalentTo(templateData);
            template.TemplateArea.Should().Be(templateArea);
            template.MatchThreshold.Should().Be(matchThreshold);
            template.TemplateType.Should().Be(templateType);
            ShouldBeActive(template);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var id = Guid.NewGuid();
            var description = "A test template";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(id, invalidName, description, templateData, templateArea);
            ShouldThrowValidationException(action, "Template name cannot be empty.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public void Constructor_WithInvalidTemplateData_ShouldThrowValidationException(byte[] invalidData)
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "A test template";
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(id, name, description, invalidData, templateArea);
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Constructor_WithInvalidThreshold_ShouldThrowValidationException(double invalidThreshold)
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "A test template";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(id, name, description, templateData, templateArea, invalidThreshold);
            ShouldThrowValidationException(action, "Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;
            var newName = "Updated Template";
            var newDescription = "Updated description";
            var newArea = new Rectangle(10, 10, 150, 150);
            var newThreshold = 0.9;

            // Act
            template.Update(newName, newDescription, newArea, newThreshold);

            // Assert
            template.Name.Should().Be(newName);
            template.Description.Should().Be(newDescription);
            template.TemplateArea.Should().Be(newArea);
            template.MatchThreshold.Should().Be(newThreshold);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act & Assert
            var action = () => template.Update(invalidName, "Updated description", template.TemplateArea, template.MatchThreshold);
            ShouldThrowValidationException(action, "Template name cannot be empty.");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Update_WithInvalidThreshold_ShouldThrowValidationException(double invalidThreshold)
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act & Assert
            var action = () => template.Update("Updated name", "Updated description", template.TemplateArea, invalidThreshold);
            ShouldThrowValidationException(action, "Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void UpdateTemplateData_WithValidData_ShouldUpdateData()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;
            var newData = new byte[] { 5, 4, 3, 2, 1 };

            // Act
            template.UpdateTemplateData(newData);

            // Assert
            template.TemplateData.Should().BeEquivalentTo(newData);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public void UpdateTemplateData_WithInvalidData_ShouldThrowValidationException(byte[] invalidData)
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act & Assert
            var action = () => template.UpdateTemplateData(invalidData);
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Fact]
        public void Activate_WithInactiveTemplate_ShouldActivate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.Deactivate(); // 先停用
            var originalVersion = template.Version;

            // Act
            template.Activate();

            // Assert
            ShouldBeActive(template);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void Activate_WithActiveTemplate_ShouldDoNothing()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Activate();

            // Assert
            ShouldBeActive(template);
            template.Version.Should().Be(originalVersion); // 版本不应改变
        }

        [Fact]
        public void Deactivate_WithActiveTemplate_ShouldDeactivate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Deactivate();

            // Assert
            ShouldBeInactive(template);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void Deactivate_WithInactiveTemplate_ShouldDoNothing()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.Deactivate(); // 先停用
            var originalVersion = template.Version;

            // Act
            template.Deactivate();

            // Assert
            ShouldBeInactive(template);
            template.Version.Should().Be(originalVersion); // 版本不应改变
        }

        [Fact]
        public void Delete_WithValidTemplate_ShouldDelete()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Delete();

            // Assert
            ShouldBeInactive(template);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void IsMatch_WithMatchingResult_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
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
            var template = TestDataFactory.CreateValidImageTemplate();
            var result = new RecognitionResult(false, 0.5, new Rectangle(0, 0, 100, 100));

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsMatch_WithLowConfidenceResult_ShouldReturnFalse()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate(); // 默认阈值为0.8
            var result = new RecognitionResult(true, 0.7, new Rectangle(0, 0, 100, 100)); // 置信度低于阈值

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsMatch_WithNullResult_ShouldReturnFalse()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act
            var isMatch = template.IsMatch(null);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsAreaMatch_WithOverlappingArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var searchArea = new Rectangle(50, 50, 100, 100); // 与模板区域重叠

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithNonOverlappingArea_ShouldReturnFalse()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var searchArea = new Rectangle(200, 200, 100, 100); // 与模板区域不重叠

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsAreaMatch_WithContainingArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var searchArea = new Rectangle(0, 0, 200, 200); // 包含模板区域

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithContainedArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var searchArea = new Rectangle(25, 25, 50, 50); // 被模板区域包含

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void DomainEvents_ShouldBeRaisedCorrectly()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act
            template.Deactivate();

            // Assert
            var domainEvents = template.DomainEvents;
            domainEvents.Should().HaveCount(2); // Created, Deactivated
            domainEvents.Should().Contain(e => e.GetType().Name == "ImageTemplateCreatedEvent");
            domainEvents.Should().Contain(e => e.GetType().Name == "ImageTemplateDeactivatedEvent");
        }

        [Theory]
        [InlineData(TemplateType.Image)]
        [InlineData(TemplateType.Text)]
        [InlineData(TemplateType.Color)]
        [InlineData(TemplateType.Pattern)]
        public void Constructor_WithDifferentTemplateTypes_ShouldCreateCorrectly(TemplateType templateType)
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template";
            var description = "A test template";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act
            var template = new ImageTemplate(id, name, description, templateData, templateArea, 0.8, templateType);

            // Assert
            template.TemplateType.Should().Be(templateType);
        }
    }
}