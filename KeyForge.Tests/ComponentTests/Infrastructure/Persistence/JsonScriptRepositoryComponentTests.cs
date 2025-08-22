using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using KeyForge.Infrastructure.Persistence;
using KeyForge.Domain.Aggregates;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.ComponentTests.Infrastructure.Persistence
{
    /// <summary>
    /// JsonScriptRepository组件测试
    /// 测试JSON脚本仓储的完整功能，包括文件操作、序列化和错误处理
    /// </summary>
    public class JsonScriptRepositoryComponentTests : TestBase
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly string _testScriptsDirectory;
        private readonly JsonScriptRepository _repository;

        public JsonScriptRepositoryComponentTests()
        {
            _mockLogger = new Mock<ILogger>();
            
            // 创建测试目录
            _testScriptsDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Test_Scripts_{Guid.NewGuid()}");
            if (!Directory.Exists(_testScriptsDirectory))
            {
                Directory.CreateDirectory(_testScriptsDirectory);
            }

            // 使用反射设置私有目录字段
            _repository = new JsonScriptRepository(_mockLogger.Object);
            var scriptsDirectoryField = typeof(JsonScriptRepository).GetField("_scriptsDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scriptsDirectoryField.SetValue(_repository, _testScriptsDirectory);
        }

        public override void Dispose()
        {
            // 清理测试目录
            if (Directory.Exists(_testScriptsDirectory))
            {
                Directory.Delete(_testScriptsDirectory, true);
            }
            base.Dispose();
        }

        #region GetAllScripts测试

        [Fact]
        public void GetAllScripts_WithMultipleScripts_ShouldReturnAllScripts()
        {
            // Arrange
            var script1 = TestDataFactory.CreateValidScript();
            var script2 = TestDataFactory.CreateValidScript();
            var script3 = TestDataFactory.CreateValidScript();

            SaveScriptToJsonFile(script1);
            SaveScriptToJsonFile(script2);
            SaveScriptToJsonFile(script3);

            // Act
            var result = _repository.GetAllScripts();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(s => s.Id == script1.Id);
            result.Should().Contain(s => s.Id == script2.Id);
            result.Should().Contain(s => s.Id == script3.Id);
        }

        [Fact]
        public void GetAllScripts_WithEmptyDirectory_ShouldReturnEmptyList()
        {
            // Act
            var result = _repository.GetAllScripts();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAllScripts_WithInvalidJsonFiles_ShouldSkipInvalidFiles()
        {
            // Arrange
            var validScript = TestDataFactory.CreateValidScript();
            SaveScriptToJsonFile(validScript);

            // 创建无效的JSON文件
            var invalidJsonPath = Path.Combine(_testScriptsDirectory, $"{Guid.NewGuid()}.json");
            File.WriteAllText(invalidJsonPath, "invalid json content");

            // Act
            var result = _repository.GetAllScripts();

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(s => s.Id == validScript.Id);

            // 验证错误日志被记录
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetAllScripts_WithNonJsonFiles_ShouldIgnoreNonJsonFiles()
        {
            // Arrange
            var validScript = TestDataFactory.CreateValidScript();
            SaveScriptToJsonFile(validScript);

            // 创建非JSON文件
            var nonJsonPath = Path.Combine(_testScriptsDirectory, "readme.txt");
            File.WriteAllText(nonJsonPath, "This is not a JSON file");

            // Act
            var result = _repository.GetAllScripts();

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(s => s.Id == validScript.Id);
        }

        #endregion

        #region GetScriptById测试

        [Fact]
        public void GetScriptById_WithExistingScript_ShouldReturnScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            SaveScriptToJsonFile(script);

            // Act
            var result = _repository.GetScriptById(script.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(script.Id);
            result.Name.Should().Be(script.Name);
            result.Description.Should().Be(script.Description);
        }

        [Fact]
        public void GetScriptById_WithNonExistingScript_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = _repository.GetScriptById(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetScriptById_WithInvalidJsonFile_ShouldReturnNull()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var invalidJsonPath = Path.Combine(_testScriptsDirectory, $"{scriptId}.json");
            File.WriteAllText(invalidJsonPath, "invalid json content");

            // Act
            var result = _repository.GetScriptById(scriptId);

            // Assert
            result.Should().BeNull();

            // 验证错误日志被记录
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region SaveScript测试

        [Fact]
        public void SaveScript_WithValidScript_ShouldSaveScriptToFile()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();

            // Act
            _repository.SaveScript(script);

            // Assert
            var filePath = Path.Combine(_testScriptsDirectory, $"{script.Id}.json");
            File.Exists(filePath).Should().BeTrue();

            var jsonContent = File.ReadAllText(filePath);
            var deserializedScript = JsonSerializer.Deserialize<Script>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            deserializedScript.Should().NotBeNull();
            deserializedScript.Id.Should().Be(script.Id);
            deserializedScript.Name.Should().Be(script.Name);
        }

        [Fact]
        public void SaveScript_WithNullScript_ShouldThrowArgumentNullException()
        {
            // Arrange
            Script nullScript = null;

            // Act & Assert
            var action = () => _repository.SaveScript(nullScript);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("script");
        }

        [Fact]
        public void SaveScript_WhenFileAlreadyExists_ShouldOverwriteFile()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var filePath = Path.Combine(_testScriptsDirectory, $"{script.Id}.json");
            
            // 先创建一个文件
            File.WriteAllText(filePath, "old content");

            // Act
            _repository.SaveScript(script);

            // Assert
            var jsonContent = File.ReadAllText(filePath);
            jsonContent.Should().NotBe("old content");
            jsonContent.Should().Contain(script.Name);
        }

        [Fact]
        public void SaveScript_WhenDirectoryDoesNotExist_ShouldCreateDirectory()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var newDirectory = Path.Combine(_testScriptsDirectory, "NewDirectory");
            var newRepository = new JsonScriptRepository(_mockLogger.Object);
            
            // 使用反射设置新的目录
            var scriptsDirectoryField = typeof(JsonScriptRepository).GetField("_scriptsDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scriptsDirectoryField.SetValue(newRepository, newDirectory);

            // Act
            newRepository.SaveScript(script);

            // Assert
            var filePath = Path.Combine(newDirectory, $"{script.Id}.json");
            File.Exists(filePath).Should().BeTrue();
            Directory.Exists(newDirectory).Should().BeTrue();

            // 清理
            if (Directory.Exists(newDirectory))
            {
                Directory.Delete(newDirectory, true);
            }
        }

        #endregion

        #region SaveScript到指定路径测试

        [Fact]
        public void SaveScriptToFilePath_WithValidScript_ShouldSaveScriptToSpecifiedPath()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var customFilePath = Path.Combine(_testScriptsDirectory, "custom_script.json");

            // Act
            _repository.SaveScript(script, customFilePath);

            // Assert
            File.Exists(customFilePath).Should().BeTrue();

            var jsonContent = File.ReadAllText(customFilePath);
            jsonContent.Should().Contain(script.Name);
        }

        [Fact]
        public void SaveScriptToFilePath_WithRelativePath_ShouldSaveRelativeToCurrentDirectory()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var relativePath = "relative_script.json";

            // Act
            _repository.SaveScript(script, relativePath);

            // Assert
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            File.Exists(absolutePath).Should().BeTrue();

            // 清理
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        [Fact]
        public void SaveScriptToFilePath_WhenDirectoryDoesNotExist_ShouldCreateDirectory()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var newDirectory = Path.Combine(_testScriptsDirectory, "NewSaveDirectory");
            var customFilePath = Path.Combine(newDirectory, "custom_script.json");

            // Act
            _repository.SaveScript(script, customFilePath);

            // Assert
            File.Exists(customFilePath).Should().BeTrue();
            Directory.Exists(newDirectory).Should().BeTrue();
        }

        #endregion

        #region LoadScript测试

        [Fact]
        public void LoadScript_WithValidScriptFile_ShouldReturnScript()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            var filePath = Path.Combine(_testScriptsDirectory, "load_test_script.json");
            SaveScriptToJsonFile(script, filePath);

            // Act
            var result = _repository.LoadScript(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(script.Id);
            result.Name.Should().Be(script.Name);
            result.Description.Should().Be(script.Description);
        }

        [Fact]
        public void LoadScript_WithNonExistingFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistingFilePath = Path.Combine(_testScriptsDirectory, "non_existing_script.json");

            // Act & Assert
            var action = () => _repository.LoadScript(nonExistingFilePath);
            action.Should().Throw<FileNotFoundException>()
                .WithMessage($"Script file not found: {nonExistingFilePath}");
        }

        [Fact]
        public void LoadScript_WithInvalidJsonFile_ShouldThrowException()
        {
            // Arrange
            var invalidJsonPath = Path.Combine(_testScriptsDirectory, "invalid_script.json");
            File.WriteAllText(invalidJsonPath, "invalid json content");

            // Act & Assert
            var action = () => _repository.LoadScript(invalidJsonPath);
            action.Should().Throw<JsonException>();

            // 验证错误日志被记录
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoadScript_WithEmptyFilePath_ShouldThrowArgumentException()
        {
            // Arrange
            string emptyFilePath = "";

            // Act & Assert
            var action = () => _repository.LoadScript(emptyFilePath);
            action.Should().Throw<ArgumentException>()
                .WithMessage("File path cannot be empty*")
                .WithParameterName("filePath");
        }

        [Fact]
        public void LoadScript_WithNullFilePath_ShouldThrowArgumentException()
        {
            // Arrange
            string nullFilePath = null;

            // Act & Assert
            var action = () => _repository.LoadScript(nullFilePath);
            action.Should().Throw<ArgumentException>()
                .WithMessage("File path cannot be empty*")
                .WithParameterName("filePath");
        }

        #endregion

        #region DeleteScript测试

        [Fact]
        public void DeleteScript_WithExistingScript_ShouldDeleteScriptFile()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            SaveScriptToJsonFile(script);

            var filePath = Path.Combine(_testScriptsDirectory, $"{script.Id}.json");
            File.Exists(filePath).Should().BeTrue();

            // Act
            _repository.DeleteScript(script.Id);

            // Assert
            File.Exists(filePath).Should().BeFalse();
        }

        [Fact]
        public void DeleteScript_WithNonExistingScript_ShouldDoNothing()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var filePath = Path.Combine(_testScriptsDirectory, $"{nonExistingId}.json");
            File.Exists(filePath).Should().BeFalse();

            // Act
            _repository.DeleteScript(nonExistingId);

            // Assert
            File.Exists(filePath).Should().BeFalse();
        }

        #endregion

        #region ScriptExists测试

        [Fact]
        public void ScriptExists_WithExistingScript_ShouldReturnTrue()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            SaveScriptToJsonFile(script);

            // Act
            var result = _repository.ScriptExists(script.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ScriptExists_WithNonExistingScript_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = _repository.ScriptExists(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region JSON序列化测试

        [Fact]
        public void JsonSerialization_ShouldPreserveScriptData()
        {
            // Arrange
            var originalScript = TestDataFactory.CreateValidScript();
            var filePath = Path.Combine(_testScriptsDirectory, "serialization_test_script.json");

            // Act
            _repository.SaveScript(originalScript, filePath);
            var loadedScript = _repository.LoadScript(filePath);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Id.Should().Be(originalScript.Id);
            loadedScript.Name.Should().Be(originalScript.Name);
            loadedScript.Description.Should().Be(originalScript.Description);
            loadedScript.Status.Should().Be(originalScript.Status);
        }

        [Fact]
        public void JsonSerialization_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Script with 中文 and 🎉", "Description with \"quotes\" and \n newlines");
            var filePath = Path.Combine(_testScriptsDirectory, "special_chars_script.json");

            // Act
            _repository.SaveScript(script, filePath);
            var loadedScript = _repository.LoadScript(filePath);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Name.Should().Be("Script with 中文 and 🎉");
            loadedScript.Description.Should().Be("Description with \"quotes\" and \n newlines");
        }

        [Fact]
        public void JsonSerialization_WithLargeScriptData_ShouldBeEfficient()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            
            // 添加大量动作
            for (int i = 0; i < 1000; i++)
            {
                var action = TestDataFactory.CreateGameAction();
                script.AddAction(action);
            }

            var filePath = Path.Combine(_testScriptsDirectory, "large_script.json");

            var startTime = DateTime.UtcNow;

            // Act
            _repository.SaveScript(script, filePath);
            var loadedScript = _repository.LoadScript(filePath);
            var endTime = DateTime.UtcNow;

            // Assert
            var serializationTime = endTime - startTime;
            serializationTime.Should().BeLessThan(TimeSpan.FromSeconds(5)); // 应该在5秒内完成

            loadedScript.Should().NotBeNull();
            loadedScript.Actions.Should().HaveCount(1000);
        }

        #endregion

        #region 并发操作测试

        [Fact]
        public void ConcurrentOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var script1 = TestDataFactory.CreateValidScript();
            var script2 = TestDataFactory.CreateValidScript();
            var script3 = TestDataFactory.CreateValidScript();

            // Act
            Parallel.Invoke(
                () => _repository.SaveScript(script1),
                () => _repository.SaveScript(script2),
                () => _repository.SaveScript(script3)
            );

            // Assert
            var allScripts = _repository.GetAllScripts();
            allScripts.Should().HaveCount(3);
            allScripts.Should().Contain(s => s.Id == script1.Id);
            allScripts.Should().Contain(s => s.Id == script2.Id);
            allScripts.Should().Contain(s => s.Id == script3.Id);
        }

        [Fact]
        public void ConcurrentReads_ShouldNotCauseIssues()
        {
            // Arrange
            var script = TestDataFactory.CreateValidScript();
            _repository.SaveScript(script);

            // Act
            var results = new System.Collections.Concurrent.ConcurrentBag<Script>();
            Parallel.For(0, 10, i =>
            {
                var loadedScript = _repository.GetScriptById(script.Id);
                if (loadedScript != null)
                {
                    results.Add(loadedScript);
                }
            });

            // Assert
            results.Should().HaveCount(10);
            results.All(s => s.Id == script.Id).Should().BeTrue();
        }

        #endregion

        #region 错误处理测试

        [Fact]
        public void Repository_ShouldHandleNullLogger()
        {
            // Arrange & Act & Assert
            var action = () => new JsonScriptRepository(null);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public void Repository_ShouldCreateScriptsDirectoryAutomatically()
        {
            // Arrange
            var newDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_Auto_Create_{Guid.NewGuid()}");
            var newRepository = new JsonScriptRepository(_mockLogger.Object);
            
            // 使用反射设置新的目录
            var scriptsDirectoryField = typeof(JsonScriptRepository).GetField("_scriptsDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scriptsDirectoryField.SetValue(newRepository, newDirectory);

            // Act
            var result = newRepository.GetAllScripts();

            // Assert
            Directory.Exists(newDirectory).Should().BeTrue();
            result.Should().BeEmpty();

            // 清理
            if (Directory.Exists(newDirectory))
            {
                Directory.Delete(newDirectory, true);
            }
        }

        [Fact]
        public void Repository_ShouldHandlePermissionIssues()
        {
            // Arrange
            var readOnlyDirectory = Path.Combine(_testScriptsDirectory, "ReadOnly");
            Directory.CreateDirectory(readOnlyDirectory);

            // 设置目录为只读（在支持的系统上）
            try
            {
                var directoryInfo = new DirectoryInfo(readOnlyDirectory);
                directoryInfo.Attributes |= FileAttributes.ReadOnly;

                var script = TestDataFactory.CreateValidScript();
                var readOnlyRepository = new JsonScriptRepository(_mockLogger.Object);
                
                // 使用反射设置只读目录
                var scriptsDirectoryField = typeof(JsonScriptRepository).GetField("_scriptsDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                scriptsDirectoryField.SetValue(readOnlyRepository, readOnlyDirectory);

                // Act & Assert
                var action = () => readOnlyRepository.SaveScript(script);
                action.Should().Throw<Exception>();

                // 验证错误日志被记录
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                // 清理
                if (Directory.Exists(readOnlyDirectory))
                {
                    var directoryInfo = new DirectoryInfo(readOnlyDirectory);
                    directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
                    Directory.Delete(readOnlyDirectory, true);
                }
            }
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void Repository_ShouldHandleVeryLongScriptNames()
        {
            // Arrange
            var longName = new string('A', 1000); // 1000字符的名称
            var script = new Script(Guid.NewGuid(), longName, "Test description");

            // Act
            _repository.SaveScript(script);
            var loadedScript = _repository.GetScriptById(script.Id);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Name.Should().Be(longName);
        }

        [Fact]
        public void Repository_ShouldHandleVeryLongDescriptions()
        {
            // Arrange
            var longDescription = new string('B', 5000); // 5000字符的描述
            var script = new Script(Guid.NewGuid(), "Test script", longDescription);

            // Act
            _repository.SaveScript(script);
            var loadedScript = _repository.GetScriptById(script.Id);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Description.Should().Be(longDescription);
        }

        [Fact]
        public void Repository_ShouldHandleEmptyScriptName()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "", "Test description");

            // Act
            _repository.SaveScript(script);
            var loadedScript = _repository.GetScriptById(script.Id);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Name.Should().BeEmpty();
        }

        [Fact]
        public void Repository_ShouldHandleEmptyScriptDescription()
        {
            // Arrange
            var script = new Script(Guid.NewGuid(), "Test script", "");

            // Act
            _repository.SaveScript(script);
            var loadedScript = _repository.GetScriptById(script.Id);

            // Assert
            loadedScript.Should().NotBeNull();
            loadedScript.Description.Should().BeEmpty();
        }

        #endregion

        #region 辅助方法

        private void SaveScriptToJsonFile(Script script)
        {
            var filePath = Path.Combine(_testScriptsDirectory, $"{script.Id}.json");
            SaveScriptToJsonFile(script, filePath);
        }

        private void SaveScriptToJsonFile(Script script, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(script, options);
            File.WriteAllText(filePath, json);
        }

        #endregion
    }
}