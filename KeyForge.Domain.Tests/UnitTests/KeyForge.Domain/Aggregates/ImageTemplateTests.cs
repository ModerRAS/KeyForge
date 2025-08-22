using System;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using Rectangle = KeyForge.Domain.Common.Rectangle;

namespace KeyForge.Domain.Tests.Aggregates
{
    /// <summary>
    /// ImageTemplate聚合根的单元测试
    /// 测试ImageTemplate实体的核心业务逻辑
    /// </summary>
    public class ImageTemplateTests
    {
        [Fact]
        public void Constructor_WithValidData_ShouldCreateImageTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);
            var matchThreshold = 0.8;

            // Act
            var template = new ImageTemplate(
                templateId,
                name,
                description,
                templateData,
                templateArea,
                matchThreshold
            );

            // Assert
            template.Should().NotBeNull();
            template.Id.Should().Be(templateId);
            template.Name.Should().Be(name);
            template.Description.Should().Be(description);
            template.TemplateData.Should().BeEquivalentTo(templateData);
            template.TemplateArea.Should().Be(templateArea);
            template.MatchThreshold.Should().Be(matchThreshold);
            template.IsActive.Should().BeTrue();
            template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            template.Version.Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowException(string invalidName)
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var description = "Test Description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(
                templateId,
                invalidName,
                description,
                templateData,
                templateArea
            );
            action.Should().Throw<ValidationException>()
                .WithMessage("Template name cannot be empty.");
        }

        [Fact]
        public void Constructor_WithEmptyTemplateData_ShouldThrowException()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = Array.Empty<byte>();
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(
                templateId,
                name,
                description,
                templateData,
                templateArea
            );
            action.Should().Throw<ValidationException>()
                .WithMessage("Template data cannot be empty.");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public void Constructor_WithInvalidMatchThreshold_ShouldThrowException(double invalidThreshold)
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = new byte[] { 1, 2, 3, 4, 5 };
            var templateArea = new Rectangle(0, 0, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(
                templateId,
                name,
                description,
                templateData,
                templateArea,
                invalidThreshold
            );
            action.Should().Throw<ValidationException>()
                .WithMessage("Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            var newName = "Updated Template";
            var newDescription = "Updated Description";
            var newArea = new Rectangle(10, 10, 200, 200);
            var newThreshold = 0.9;

            // Act
            template.Update(newName, newDescription, newArea, newThreshold);

            // Assert
            template.Name.Should().Be(newName);
            template.Description.Should().Be(newDescription);
            template.TemplateArea.Should().Be(newArea);
            template.MatchThreshold.Should().Be(newThreshold);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
            template.Version.Should().Be(2);
        }

        [Fact]
        public void UpdateTemplateData_WithValidData_ShouldUpdateTemplateData()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            var newTemplateData = new byte[] { 6, 7, 8, 9, 10 };

            // Act
            template.UpdateTemplateData(newTemplateData);

            // Assert
            template.TemplateData.Should().BeEquivalentTo(newTemplateData);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
            template.Version.Should().Be(2);
        }

        [Fact]
        public void UpdateTemplateData_WithEmptyData_ShouldThrowException()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );

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
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            template.Deactivate();

            // Act
            template.Activate();

            // Assert
            template.IsActive.Should().BeTrue();
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
            template.Version.Should().Be(3);
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldDoNothing()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            var originalUpdatedAt = template.UpdatedAt;

            // Act
            template.Activate();

            // Assert
            template.IsActive.Should().BeTrue();
            template.UpdatedAt.Should().Be(originalUpdatedAt);
            template.Version.Should().Be(1);
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateTemplate()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );

            // Act
            template.Deactivate();

            // Assert
            template.IsActive.Should().BeFalse();
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
            template.Version.Should().Be(2);
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldDoNothing()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            template.Deactivate();
            var originalUpdatedAt = template.UpdatedAt;

            // Act
            template.Deactivate();

            // Assert
            template.IsActive.Should().BeFalse();
            template.UpdatedAt.Should().Be(originalUpdatedAt);
            template.Version.Should().Be(2);
        }

        [Fact]
        public void IsMatch_WithMatchingResult_ShouldReturnTrue()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100),
                0.8
            );
            var result = new RecognitionResult(true, new Rectangle(0, 0, 100, 100), 0.9, "Test Template");

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
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100),
                0.8
            );
            var result = new RecognitionResult(true, new Rectangle(0, 0, 100, 100), 0.7, "Test Template");

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
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100),
                0.8
            );

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
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            var searchArea = new Rectangle(50, 50, 100, 100);

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithNonIntersectingArea_ShouldReturnFalse()
        {
            // Arrange
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "Test Template",
                "Test Description",
                new byte[] { 1, 2, 3, 4, 5 },
                new Rectangle(0, 0, 100, 100)
            );
            var searchArea = new Rectangle(150, 150, 100, 100);

            // Act
            var isMatch = template.IsAreaMatch(searchArea);

            // Assert
            isMatch.Should().BeFalse();
        }
    }
}