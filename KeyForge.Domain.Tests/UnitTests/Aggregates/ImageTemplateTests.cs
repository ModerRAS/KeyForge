using System;
using System.Drawing;
using Xunit;
using FluentAssertions;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using KeyForge.Domain.Events;

namespace KeyForge.Domain.Tests.UnitTests.Aggregates
{
    /// <summary>
    /// ImageTemplate聚合根单元测试
    /// 测试图像模板的所有业务规则和不变性
    /// </summary>
    public class ImageTemplateTests : TestBase
    {
        #region 构造函数测试

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateImageTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);
            var matchThreshold = 0.8;
            var templateType = TemplateType.Image;

            // Act
            var template = new ImageTemplate(templateId, name, description, templateData, 
                templateArea, matchThreshold, templateType);

            // Assert
            ShouldBeValidImageTemplate(template);
            template.Name.Should().Be(name);
            template.Description.Should().Be(description);
            template.TemplateData.Should().BeEquivalentTo(templateData);
            template.TemplateArea.Should().Be(templateArea);
            template.MatchThreshold.Should().Be(matchThreshold);
            template.TemplateType.Should().Be(templateType);
            template.IsActive.Should().BeTrue();
            template.Version.Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var description = "Test Description";
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(templateId, invalidName, description, templateData, templateArea);
            ShouldThrowValidationException(action, "Template name cannot be empty.");
        }

        [Fact]
        public void Constructor_WithNullTemplateData_ShouldThrowValidationException()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(templateId, name, description, null, templateArea);
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Fact]
        public void Constructor_WithEmptyTemplateData_ShouldThrowValidationException()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = Array.Empty<byte>();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(templateId, name, description, templateData, templateArea);
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Constructor_WithInvalidMatchThreshold_ShouldThrowValidationException(double invalidThreshold)
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            var action = () => new ImageTemplate(templateId, name, description, templateData, templateArea, invalidThreshold);
            ShouldThrowValidationException(action, "Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void Constructor_ShouldRaiseImageTemplateCreatedEvent()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Test Template";
            var description = "Test Description";
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act
            var template = new ImageTemplate(templateId, name, description, templateData, templateArea);

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateCreatedEvent>();
            var createdEvent = (ImageTemplateCreatedEvent)domainEvent;
            createdEvent.TemplateId.Should().Be(templateId);
            createdEvent.TemplateName.Should().Be(name);
        }

        #endregion

        #region Update方法测试

        [Fact]
        public void Update_WithValidParameters_ShouldUpdateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;
            var newName = "Updated Template";
            var newDescription = "Updated Description";
            var newTemplateArea = new Rectangle(20, 20, 150, 150);
            var newMatchThreshold = 0.9;

            // Act
            template.Update(newName, newDescription, newTemplateArea, newMatchThreshold);

            // Assert
            template.Name.Should().Be(newName);
            template.Description.Should().Be(newDescription);
            template.TemplateArea.Should().Be(newTemplateArea);
            template.MatchThreshold.Should().Be(newMatchThreshold);
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
            var newTemplateArea = new Rectangle(20, 20, 150, 150);
            var newMatchThreshold = 0.9;

            // Act & Assert
            var action = () => template.Update(invalidName, "Updated Description", newTemplateArea, newMatchThreshold);
            ShouldThrowValidationException(action, "Template name cannot be empty.");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public void Update_WithInvalidMatchThreshold_ShouldThrowValidationException(double invalidThreshold)
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var newTemplateArea = new Rectangle(20, 20, 150, 150);

            // Act & Assert
            var action = () => template.Update("Updated Name", "Updated Description", newTemplateArea, invalidThreshold);
            ShouldThrowValidationException(action, "Match threshold must be between 0 and 1.");
        }

        [Fact]
        public void Update_ShouldRaiseImageTemplateUpdatedEvent()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.ClearDomainEvents(); // 清除构造函数产生的事件
            var newTemplateArea = new Rectangle(20, 20, 150, 150);
            var newMatchThreshold = 0.9;

            // Act
            template.Update("Updated Name", "Updated Description", newTemplateArea, newMatchThreshold);

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateUpdatedEvent>();
            var updatedEvent = (ImageTemplateUpdatedEvent)domainEvent;
            updatedEvent.TemplateId.Should().Be(template.Id);
            updatedEvent.TemplateName.Should().Be("Updated Name");
        }

        #endregion

        #region UpdateTemplateData方法测试

        [Fact]
        public void UpdateTemplateData_WithValidData_ShouldUpdateTemplateData()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;
            var newTemplateData = TestDataFactory.CreateRandomImageData();

            // Act
            template.UpdateTemplateData(newTemplateData);

            // Assert
            template.TemplateData.Should().BeEquivalentTo(newTemplateData);
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void UpdateTemplateData_WithNullData_ShouldThrowValidationException()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act & Assert
            var action = () => template.UpdateTemplateData(null);
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Fact]
        public void UpdateTemplateData_WithEmptyData_ShouldThrowValidationException()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();

            // Act & Assert
            var action = () => template.UpdateTemplateData(Array.Empty<byte>());
            ShouldThrowValidationException(action, "Template data cannot be empty.");
        }

        [Fact]
        public void UpdateTemplateData_ShouldRaiseImageTemplateDataUpdatedEvent()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.ClearDomainEvents();
            var newTemplateData = TestDataFactory.CreateRandomImageData();

            // Act
            template.UpdateTemplateData(newTemplateData);

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateDataUpdatedEvent>();
            var updatedEvent = (ImageTemplateDataUpdatedEvent)domainEvent;
            updatedEvent.TemplateId.Should().Be(template.Id);
            updatedEvent.TemplateName.Should().Be(template.Name);
        }

        #endregion

        #region Activate方法测试

        [Fact]
        public void Activate_WhenInactive_ShouldActivateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.Deactivate(); // 先停用
            var originalVersion = template.Version;

            // Act
            template.Activate();

            // Assert
            template.IsActive.Should().BeTrue();
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldDoNothing()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Activate();

            // Assert
            template.IsActive.Should().BeTrue();
            template.Version.Should().Be(originalVersion); // 版本不应该改变
        }

        [Fact]
        public void Activate_ShouldRaiseImageTemplateActivatedEvent()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.Deactivate();
            template.ClearDomainEvents();

            // Act
            template.Activate();

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateActivatedEvent>();
            var activatedEvent = (ImageTemplateActivatedEvent)domainEvent;
            activatedEvent.TemplateId.Should().Be(template.Id);
            activatedEvent.TemplateName.Should().Be(template.Name);
        }

        #endregion

        #region Deactivate方法测试

        [Fact]
        public void Deactivate_WhenActive_ShouldDeactivateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Deactivate();

            // Assert
            template.IsActive.Should().BeFalse();
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldDoNothing()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.Deactivate(); // 先停用
            var originalVersion = template.Version;

            // Act
            template.Deactivate();

            // Assert
            template.IsActive.Should().BeFalse();
            template.Version.Should().Be(originalVersion); // 版本不应该改变
        }

        [Fact]
        public void Deactivate_ShouldRaiseImageTemplateDeactivatedEvent()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.ClearDomainEvents();

            // Act
            template.Deactivate();

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateDeactivatedEvent>();
            var deactivatedEvent = (ImageTemplateDeactivatedEvent)domainEvent;
            deactivatedEvent.TemplateId.Should().Be(template.Id);
            deactivatedEvent.TemplateName.Should().Be(template.Name);
        }

        #endregion

        #region Delete方法测试

        [Fact]
        public void Delete_ShouldDeactivateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalVersion = template.Version;

            // Act
            template.Delete();

            // Assert
            template.IsActive.Should().BeFalse();
            template.Version.Should().Be(originalVersion + 1);
            template.UpdatedAt.Should().BeAfter(template.CreatedAt);
        }

        [Fact]
        public void Delete_ShouldRaiseImageTemplateDeletedEvent()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.ClearDomainEvents();

            // Act
            template.Delete();

            // Assert
            template.DomainEvents.Should().HaveCount(1);
            var domainEvent = template.DomainEvents.First();
            domainEvent.Should().BeOfType<ImageTemplateDeletedEvent>();
            var deletedEvent = (ImageTemplateDeletedEvent)domainEvent;
            deletedEvent.TemplateId.Should().Be(template.Id);
            deletedEvent.TemplateName.Should().Be(template.Name);
        }

        #endregion

        #region IsMatch方法测试

        [Fact]
        public void IsMatch_WithMatchingResult_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.MatchThreshold = 0.8;
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.85, template.Name);

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
            template.MatchThreshold = 0.8;
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), 0.75, template.Name);

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void IsMatch_WithNoMatchResult_ShouldReturnFalse()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var result = new RecognitionResult(false, Rectangle.Empty, 0, template.Name);

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

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(0.79)]
        [InlineData(0.8)]
        [InlineData(0.81)]
        [InlineData(0.9)]
        [InlineData(1.0)]
        public void IsMatch_ShouldRespectMatchThreshold(double confidence)
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.MatchThreshold = 0.8;
            var result = new RecognitionResult(true, new Rectangle(50, 50, 100, 100), confidence, template.Name);

            // Act
            var isMatch = template.IsMatch(result);

            // Assert
            isMatch.Should().Be(confidence >= template.MatchThreshold);
        }

        #endregion

        #region IsAreaMatch方法测试

        [Fact]
        public void IsAreaMatch_WithIntersectingArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.TemplateArea = new Rectangle(50, 50, 100, 100);
            var searchArea = new Rectangle(75, 75, 100, 100);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithNonIntersectingArea_ShouldReturnFalse()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.TemplateArea = new Rectangle(50, 50, 100, 100);
            var searchArea = new Rectangle(200, 200, 100, 100);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeFalse();
        }

        [Fact]
        public void IsAreaMatch_WithContainedArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.TemplateArea = new Rectangle(50, 50, 100, 100);
            var searchArea = new Rectangle(75, 75, 50, 50);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithContainingArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            template.TemplateArea = new Rectangle(75, 75, 50, 50);
            var searchArea = new Rectangle(50, 50, 100, 100);

            // Act
            var isAreaMatch = template.IsAreaMatch(searchArea);

            // Assert
            isAreaMatch.Should().BeTrue();
        }

        [Fact]
        public void IsAreaMatch_WithSameArea_ShouldReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var templateArea = template.TemplateArea;

            // Act
            var isAreaMatch = template.IsAreaMatch(templateArea);

            // Assert
            isAreaMatch.Should().BeTrue();
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void ImageTemplate_ShouldHandleLargeTemplateData()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Large Template";
            var description = "Large template data";
            var largeTemplateData = new byte[1024 * 1024]; // 1MB
            var templateArea = new Rectangle(0, 0, 1920, 1080);
            var matchThreshold = 0.9;

            // Act
            var template = new ImageTemplate(templateId, name, description, largeTemplateData, 
                templateArea, matchThreshold, TemplateType.Image);

            // Assert
            template.TemplateData.Should().HaveCount(1024 * 1024);
            template.TemplateData.Length.Should().Be(1024 * 1024);
        }

        [Fact]
        public void ImageTemplate_ShouldHandleDifferentTemplateTypes()
        {
            // Arrange
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            foreach (TemplateType templateType in Enum.GetValues(typeof(TemplateType)))
            {
                var template = new ImageTemplate(Guid.NewGuid(), "Test", "Test", templateData, 
                    templateArea, 0.8, templateType);
                
                template.TemplateType.Should().Be(templateType);
            }
        }

        [Fact]
        public void ImageTemplate_ShouldMaintainVersionConsistency()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var newTemplateData = TestDataFactory.CreateRandomImageData();
            var initialVersion = template.Version;

            // Act
            template.Update("Updated Name", "Updated Description", new Rectangle(20, 20, 150, 150), 0.9);
            var versionAfterUpdate = template.Version;

            template.UpdateTemplateData(newTemplateData);
            var versionAfterDataUpdate = template.Version;

            template.Deactivate();
            var versionAfterDeactivate = template.Version;

            template.Activate();
            var versionAfterActivate = template.Version;

            // Assert
            versionAfterUpdate.Should().Be(initialVersion + 1);
            versionAfterDataUpdate.Should().Be(versionAfterUpdate + 1);
            versionAfterDeactivate.Should().Be(versionAfterDataUpdate + 1);
            versionAfterActivate.Should().Be(versionAfterDeactivate + 1);
        }

        [Fact]
        public void ImageTemplate_ShouldHandleExtremeMatchThresholds()
        {
            // Arrange
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            // 测试边界值
            var template1 = new ImageTemplate(Guid.NewGuid(), "Test1", "Test", templateData, templateArea, 0.0);
            template1.MatchThreshold.Should().Be(0.0);

            var template2 = new ImageTemplate(Guid.NewGuid(), "Test2", "Test", templateData, templateArea, 1.0);
            template2.MatchThreshold.Should().Be(1.0);

            var template3 = new ImageTemplate(Guid.NewGuid(), "Test3", "Test", templateData, templateArea, 0.5);
            template3.MatchThreshold.Should().Be(0.5);
        }

        #endregion

        #region 不变性测试

        [Fact]
        public void ImageTemplate_ShouldMaintainImmutabilityOfTemplateData()
        {
            // Arrange
            var template = TestDataFactory.CreateValidImageTemplate();
            var originalData = template.TemplateData;

            // Act
            // 尝试修改返回的数据数组
            var dataArray = template.TemplateData;
            dataArray[0] = 255; // 修改第一个字节

            // Assert
            template.TemplateData.Should().BeEquivalentTo(originalData);
            template.TemplateData[0].Should().NotBe(255); // 原始数据不应该被修改
        }

        [Fact]
        public void ImageTemplate_ShouldValidateAllBusinessRules()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var name = "Valid Template";
            var description = "Valid Description";
            var templateData = TestDataFactory.CreateRandomImageData();
            var templateArea = new Rectangle(10, 10, 100, 100);

            // Act & Assert
            // 所有参数都有效时应该成功创建
            var template = new ImageTemplate(templateId, name, description, templateData, templateArea);
            template.Should().NotBeNull();

            // 测试各种无效组合
            Assert.Throws<ValidationException>(() => new ImageTemplate(templateId, "", description, templateData, templateArea));
            Assert.Throws<ValidationException>(() => new ImageTemplate(templateId, name, description, null, templateArea));
            Assert.Throws<ValidationException>(() => new ImageTemplate(templateId, name, description, Array.Empty<byte>(), templateArea));
            Assert.Throws<ValidationException>(() => new ImageTemplate(templateId, name, description, templateData, templateArea, -0.1));
            Assert.Throws<ValidationException>(() => new ImageTemplate(templateId, name, description, templateData, templateArea, 1.1));
        }

        #endregion
    }
}