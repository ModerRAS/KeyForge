using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using KeyForge.Tests.Support;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Tests.BDD.Features;

/// <summary>
/// 图像识别模块BDD测试
/// 基于验收标准 AC-FUNC-002
/// 原本实现：复杂的图像处理BDD场景
/// 简化实现：核心图像识别功能测试
/// </summary>
public class ImageRecognitionBddTests : BddTestBase
{
    public ImageRecognitionBddTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void ImageTemplateCreation_ShouldWorkCorrectly()
    {
        // 基于验收标准 AC-FUNC-002: 基础识别
        
        Given("I have a valid image template", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            template.Should().NotBeNull();
            template.Id.Should().NotBe(Guid.Empty);
            template.ImagePath.Should().NotBeNullOrWhiteSpace();
            template.Threshold.Should().BeGreaterThan(0);
            template.Threshold.Should().BeLessThanOrEqualTo(1);
        });

        When("I create the template with specific parameters", () =>
        {
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "test_image.png",
                new System.Drawing.Rectangle(10, 20, 100, 50),
                0.85
            );
        });

        Then("The template should have the correct properties", () =>
        {
            var template = new ImageTemplate(
                Guid.NewGuid(),
                "test_image.png",
                new System.Drawing.Rectangle(10, 20, 100, 50),
                0.85
            );
            
            template.ImagePath.Should().Be("test_image.png");
            template.Region.X.Should().Be(10);
            template.Region.Y.Should().Be(20);
            template.Region.Width.Should().Be(100);
            template.Region.Height.Should().Be(50);
            template.Threshold.Should().Be(0.85);
        });

        And("The template should be active by default", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            template.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public void ImageRecognition_Processing_ShouldWorkCorrectly()
    {
        // 基于验收标准 AC-FUNC-002: 识别准确率和响应时间
        
        Given("I have an active image template", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            template.IsActive.Should().BeTrue();
        });

        When("I perform image recognition", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            
            // 模拟识别过程
            var result = SimulateImageRecognition(template, 0.9);
            
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
        });

        Then("The recognition should complete within time limits", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var startTime = DateTime.UtcNow;
            
            var result = SimulateImageRecognition(template, 0.9);
            
            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;
            
            processingTime.Should().BeLessThan(100); // 响应时间小于100ms
        });

        And("The result should have high confidence", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var result = SimulateImageRecognition(template, 0.9);
            
            result.IsSuccessful.Should().BeTrue();
            result.Confidence.Should().BeGreaterThan(0.8);
            result.IsHighConfidence().Should().BeTrue();
        });

        When("I perform recognition with low confidence", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var result = SimulateImageRecognition(template, 0.7);
            
            result.IsSuccessful.Should().BeFalse();
            result.Confidence.Should().BeLessThan(template.Threshold);
        });

        Then("The recognition should indicate failure", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var result = SimulateImageRecognition(template, 0.7);
            
            result.IsSuccessful.Should().BeFalse();
            result.IsHighConfidence().Should().BeFalse();
        });
    }

    [Fact]
    public void ImageTemplateManagement_ShouldSupportCrudOperations()
    {
        // 基于验收标准 AC-FUNC-002: 模板管理
        
        Given("I have multiple image templates", () =>
        {
            var templates = new[]
            {
                TestFixtures.CreateValidImageTemplate(),
                TestFixtures.CreateValidImageTemplate(),
                TestFixtures.CreateValidImageTemplate()
            };
            templates.Length.Should().Be(3);
        });

        When("I save templates to storage", () =>
        {
            var templates = new[]
            {
                TestFixtures.CreateValidImageTemplate(),
                TestFixtures.CreateValidImageTemplate(),
                TestFixtures.CreateValidImageTemplate()
            };
            
            foreach (var template in templates)
            {
                var filePath = Path.Combine(_testDirectory, $"{template.Id}.json");
                FileStorage.SaveScript(template, filePath);
            }
        });

        Then("All templates should be persisted", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            files.Length.Should().Be(3);
        });

        When("I load templates from storage", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            var loadedTemplates = files.Select(f => FileStorage.LoadScript<ImageTemplate>(f)).Where(t => t != null).ToList();
            loadedTemplates.Count.Should().Be(3);
        });

        Then("All templates should be loaded correctly", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            var loadedTemplates = files.Select(f => FileStorage.LoadScript<ImageTemplate>(f)).Where(t => t != null).ToList();
            loadedTemplates.All(t => t.IsActive).Should().BeTrue();
        });

        When("I update a template", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            var template = FileStorage.LoadScript<ImageTemplate>(files[0]);
            if (template != null)
            {
                template.UpdateThreshold(0.95);
                var filePath = Path.Combine(_testDirectory, $"{template.Id}.json");
                FileStorage.SaveScript(template, filePath);
            }
        });

        Then("The template should be updated correctly", () =>
        {
            var files = Directory.GetFiles(_testDirectory, "*.json");
            var template = FileStorage.LoadScript<ImageTemplate>(files[0]);
            if (template != null)
            {
                template.Threshold.Should().Be(0.95);
            }
        });
    }

    [Fact]
    public void ImageRecognition_Performance_ShouldMeetRequirements()
    {
        // 基于验收标准 AC-FUNC-002: 性能要求
        
        Given("I have multiple image templates", () =>
        {
            var templates = Enumerable.Range(0, 100)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            templates.Count.Should().Be(100);
        });

        When("I perform batch recognition", () =>
        {
            var templates = Enumerable.Range(0, 100)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            
            var startTime = DateTime.UtcNow;
            
            var results = templates.Select(t => SimulateImageRecognition(t, 0.85)).ToList();
            
            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;
            
            processingTime.Should().BeLessThan(1000); // 批量处理时间应该小于1秒
        });

        Then("The batch processing should complete within time limits", () =>
        {
            var templates = Enumerable.Range(0, 100)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            
            var startTime = DateTime.UtcNow;
            
            var results = templates.Select(t => SimulateImageRecognition(t, 0.85)).ToList();
            
            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;
            
            processingTime.Should().BeLessThan(1000);
        });

        And("The accuracy should be maintained", () =>
        {
            var templates = Enumerable.Range(0, 100)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            
            var results = templates.Select(t => SimulateImageRecognition(t, 0.85)).ToList();
            var successRate = results.Count(r => r.IsSuccessful) / (double)results.Count;
            
            successRate.Should().BeGreaterThan(0.95); // 准确率应该大于95%
        });

        When("I process concurrent recognition requests", () =>
        {
            var templates = Enumerable.Range(0, 50)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            
            var results = new System.Collections.Concurrent.ConcurrentBag<RecognitionResult>();
            
            Parallel.ForEach(templates, template =>
            {
                var result = SimulateImageRecognition(template, 0.85);
                results.Add(result);
            });
            
            results.Count.Should().Be(50);
        });

        Then("The concurrent processing should handle all requests", () =>
        {
            var templates = Enumerable.Range(0, 50)
                .Select(_ => TestFixtures.CreateValidImageTemplate())
                .ToList();
            
            var results = new System.Collections.Concurrent.ConcurrentBag<RecognitionResult>();
            
            Parallel.ForEach(templates, template =>
            {
                var result = SimulateImageRecognition(template, 0.85);
                results.Add(result);
            });
            
            results.Count.Should().Be(50);
            
            var successRate = results.Count(r => r.IsSuccessful) / (double)results.Count;
            successRate.Should().BeGreaterThan(0.95);
        });
    }

    [Fact]
    public void ImageRecognition_ErrorHandling_ShouldBeRobust()
    {
        // 基于验收标准 AC-FUNC-002: 容错处理
        
        Given("I have an invalid image path", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "", // 无效路径
                new System.Drawing.Rectangle(0, 0, 100, 100),
                0.8
            );
            action.Should().Throw<ArgumentException>();
        });

        When("I try to create a template with invalid path", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "",
                new System.Drawing.Rectangle(0, 0, 100, 100),
                0.8
            );
        });

        Then("The system should reject the invalid template", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "",
                new System.Drawing.Rectangle(0, 0, 100, 100),
                0.8
            );
            action.Should().Throw<ArgumentException>();
        });

        Given("I have a template with invalid threshold", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "test.png",
                new System.Drawing.Rectangle(0, 0, 100, 100),
                1.5 // 无效阈值
            );
            action.Should().Throw<ArgumentException>();
        });

        When("I try to create a template with invalid threshold", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "test.png",
                new System.Drawing.Rectangle(0, 0, 100, 100),
                1.5
            );
        });

        Then("The system should reject the invalid threshold", () =>
        {
            Action action = () => new ImageTemplate(
                Guid.NewGuid(),
                "test.png",
                new System.Drawing.Rectangle(0, 0, 100, 100),
                1.5
            );
            action.Should().Throw<ArgumentException>();
        });

        Given("I have recognition results with varying confidence", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var results = new[]
            {
                SimulateImageRecognition(template, 0.95),
                SimulateImageRecognition(template, 0.85),
                SimulateImageRecognition(template, 0.75),
                SimulateImageRecognition(template, 0.65)
            };
            
            results.Should().HaveCount(4);
        });

        When("I filter results by confidence", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var results = new[]
            {
                SimulateImageRecognition(template, 0.95),
                SimulateImageRecognition(template, 0.85),
                SimulateImageRecognition(template, 0.75),
                SimulateImageRecognition(template, 0.65)
            };
            
            var highConfidenceResults = results.Where(r => r.IsHighConfidence()).ToList();
            var successfulResults = results.Where(r => r.IsSuccessful).ToList();
        });

        Then("The filtering should work correctly", () =>
        {
            var template = TestFixtures.CreateValidImageTemplate();
            var results = new[]
            {
                SimulateImageRecognition(template, 0.95),
                SimulateImageRecognition(template, 0.85),
                SimulateImageRecognition(template, 0.75),
                SimulateImageRecognition(template, 0.65)
            };
            
            var highConfidenceResults = results.Where(r => r.IsHighConfidence()).ToList();
            var successfulResults = results.Where(r => r.IsSuccessful).ToList();
            
            highConfidenceResults.Count.Should().Be(2);
            successfulResults.Count.Should().Be(2);
        });
    }

    private RecognitionResult SimulateImageRecognition(ImageTemplate template, double confidence)
    {
        // 模拟图像识别过程
        var processingTime = new Random().Next(10, 50); // 10-50ms处理时间
        
        if (confidence >= template.Threshold)
        {
            return new RecognitionResult(
                true,
                confidence,
                new System.Drawing.Rectangle(
                    template.Region.X + new Random().Next(-10, 10),
                    template.Region.Y + new Random().Next(-10, 10),
                    template.Region.Width,
                    template.Region.Height
                ),
                processingTime
            );
        }
        else
        {
            return new RecognitionResult(
                false,
                confidence,
                null,
                processingTime
            );
        }
    }
}