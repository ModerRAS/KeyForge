# KeyForge 测试架构设计

## 📋 概述

本文档定义了KeyForge项目的完整测试架构，包括分层测试策略、跨平台测试框架、模拟和存根策略，以及持续集成测试流水线。该测试架构确保系统的质量、可靠性和跨平台兼容性。

## 🏗️ 测试架构概览

### 1.1 测试金字塔设计

```
                   ┌─────────────────┐
                   │   E2E Tests     │
                   │    (10%)        │
                   ├─────────────────┤
                   │ Integration     │
                   │   Tests (20%)   │
                   ├─────────────────┤
                   │   Unit Tests    │
                   │    (70%)        │
                   └─────────────────┘
```

### 1.2 测试层次结构

```
KeyForge.Tests/
├── Unit/
│   ├── Domain.Tests/           // 领域层单元测试
│   ├── Application.Tests/      // 应用层单元测试
│   ├── Abstractions.Tests/     // 抽象层单元测试
│   └── HAL.Tests/             // 硬件抽象层单元测试
├── Integration/
│   ├── ScriptExecution.Tests/ // 脚本执行集成测试
│   ├── ImageRecognition.Tests/ // 图像识别集成测试
│   ├── InputSystem.Tests/     // 输入系统集成测试
│   └── Configuration.Tests/   // 配置管理集成测试
├── EndToEnd/
│   ├── UI.Workflow.Tests/     // UI工作流测试
│   ├── CrossPlatform.Tests/   // 跨平台测试
│   └── Performance.Tests/     // 性能测试
├── Acceptance/
│   ├── BDD.Tests/             // BDD风格测试
│   └── UAT.Tests/             // 用户验收测试
└── Common/
    ├── TestBase.cs            // 测试基类
    ├── TestDataFactory.cs     // 测试数据工厂
    ├── MockServices.cs         // 模拟服务
    └── TestHelpers.cs         // 测试辅助工具
```

## 🧪 分层测试策略

### 2.1 单元测试策略

#### 2.1.1 领域层单元测试
```csharp
// KeyForge.Tests/Unit/Domain/
namespace KeyForge.Tests.Unit.Domain
{
    /// <summary>
    /// 领域模型测试基类
    /// </summary>
    public abstract class DomainTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly Fixture Fixture;
        
        protected DomainTestBase(ITestOutputHelper output)
        {
            Output = output;
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        }
    }
    
    /// <summary>
    /// Script领域模型测试
    /// </summary>
    public class ScriptTests : DomainTestBase
    {
        public ScriptTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateScript()
        {
            // Arrange
            var name = "Test Script";
            var code = "Console.WriteLine('Hello World');";
            
            // Act
            var script = new Script(name, code);
            
            // Assert
            script.Should().NotBeNull();
            script.Name.Should().Be(name);
            script.Code.Should().Be(code);
            script.Status.Should().Be(ScriptStatus.Draft);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowException(string invalidName)
        {
            // Arrange
            var code = "Console.WriteLine('Hello World');";
            
            // Act & Assert
            var action = () => new Script(invalidName, code);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*name*");
        }
        
        [Fact]
        public void AddAction_WithValidAction_ShouldAddToActions()
        {
            // Arrange
            var script = new Script("Test", "code");
            var action = new ScriptAction(ActionType.KeyDown, "Press A");
            
            // Act
            script.Actions.Add(action);
            
            // Assert
            script.Actions.Should().ContainSingle();
            script.Actions[0].Should().Be(action);
        }
        
        [Fact]
        public void SetVariable_WithValidParameters_ShouldSetVariable()
        {
            // Arrange
            var script = new Script("Test", "code");
            var key = "testVar";
            var value = "testValue";
            
            // Act
            script.Variables[key] = value;
            
            // Assert
            script.Variables.Should().ContainKey(key);
            script.Variables[key].Should().Be(value);
        }
    }
    
    /// <summary>
    /// KeyAction领域模型测试
    /// </summary>
    public class KeyActionTests : DomainTestBase
    {
        public KeyActionTests(ITestOutputHelper output) : base(output) { }
        
        [Theory]
        [InlineData(KeyCode.A, KeyState.Press)]
        [InlineData(KeyCode.B, KeyState.Release)]
        [InlineData(KeyCode.Enter, KeyState.Press)]
        public void Constructor_WithValidParameters_ShouldCreateKeyAction(KeyCode keyCode, KeyState state)
        {
            // Arrange & Act
            var action = new KeyAction(keyCode, state);
            
            // Assert
            action.KeyCode.Should().Be(keyCode);
            action.KeyState.Should().Be(state);
            action.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        }
        
        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var action = new KeyAction(KeyCode.A, KeyState.Press);
            
            // Act
            var result = action.ToString();
            
            // Assert
            result.Should().Contain("A");
            result.Should().Contain("Press");
        }
    }
}
```

#### 2.1.2 应用层单元测试
```csharp
// KeyForge.Tests/Unit/Application/
namespace KeyForge.Tests.Unit.Application
{
    /// <summary>
    /// 应用层测试基类
    /// </summary>
    public abstract class ApplicationTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly Mock<IServiceProvider> ServiceProvider;
        protected readonly Fixture Fixture;
        
        protected ApplicationTestBase(ITestOutputHelper output)
        {
            Output = output;
            ServiceProvider = new Mock<IServiceProvider>();
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        }
        
        protected TService GetMockService<TService>() where TService : class
        {
            var mock = new Mock<TService>();
            ServiceProvider.Setup(x => x.GetService(typeof(TService))).Returns(mock.Object);
            return mock.Object;
        }
    }
    
    /// <summary>
    /// ScriptCommandHandler测试
    /// </summary>
    public class ScriptCommandHandlerTests : ApplicationTestBase
    {
        public ScriptCommandHandlerTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task Handle_WithValidCommand_ShouldExecuteScript()
        {
            // Arrange
            var scriptService = GetMockService<IScriptService>();
            var handler = new ScriptCommandHandler(scriptService);
            var command = new ExecuteScriptCommand
            {
                ScriptId = "test-script-id",
                Parameters = new Dictionary<string, object>
                {
                    ["speed"] = 1.0,
                    ["repeat"] = 3
                }
            };
            
            scriptService.Setup(x => x.ExecuteScriptAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new ExecutionResult { Status = ExecutionStatus.Success });
            
            // Act
            await handler.Handle(command);
            
            // Assert
            scriptService.Verify(x => x.ExecuteScriptAsync(command.ScriptId, command.Parameters), Times.Once);
        }
        
        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
        {
            // Arrange
            var scriptService = GetMockService<IScriptService>();
            var handler = new ScriptCommandHandler(scriptService);
            var command = new ExecuteScriptCommand
            {
                ScriptId = "" // Invalid script ID
            };
            
            // Act & Assert
            var action = async () => await handler.Handle(command);
            await action.Should().ThrowAsync<ValidationException>();
        }
        
        [Fact]
        public async Task Validate_WithValidCommand_ShouldReturnValidResult()
        {
            // Arrange
            var scriptService = GetMockService<IScriptService>();
            var handler = new ScriptCommandHandler(scriptService);
            var command = new ExecuteScriptCommand
            {
                ScriptId = "test-script-id",
                Parameters = new Dictionary<string, object>
                {
                    ["speed"] = 1.0
                }
            };
            
            // Act
            var result = await handler.Validate(command);
            
            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }
    }
    
    /// <summary>
    /// ImageRecognitionQueryHandler测试
    /// </summary>
    public class ImageRecognitionQueryHandlerTests : ApplicationTestBase
    {
        public ImageRecognitionQueryHandlerTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task Handle_WithValidQuery_ShouldReturnRecognitionResult()
        {
            // Arrange
            var imageService = GetMockService<IImageRecognitionService>();
            var handler = new ImageRecognitionQueryHandler(imageService);
            var query = new RecognizeImageQuery
            {
                TemplateId = "test-template",
                Region = new ScreenRegion(0, 0, 100, 100)
            };
            
            var expectedResult = new RecognitionResult
            {
                Status = RecognitionStatus.Success,
                Confidence = 0.95,
                Location = new Point(50, 50)
            };
            
            imageService.Setup(x => x.RecognizeAsync(It.IsAny<ImageTemplate>(), It.IsAny<ScreenRegion>()))
                .ReturnsAsync(expectedResult);
            
            // Act
            var result = await handler.Handle(query);
            
            // Assert
            result.Should().Be(expectedResult);
            imageService.Verify(x => x.RecognizeAsync(It.IsAny<ImageTemplate>(), It.IsAny<ScreenRegion>()), Times.Once);
        }
    }
}
```

#### 2.1.3 抽象层单元测试
```csharp
// KeyForge.Tests/Unit/Abstractions/
namespace KeyForge.Tests.Unit.Abstractions
{
    /// <summary>
    /// 抽象层测试基类
    /// </summary>
    public abstract class AbstractionsTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly Fixture Fixture;
        
        protected AbstractionsTestBase(ITestOutputHelper output)
        {
            Output = output;
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        }
    }
    
    /// <summary>
    /// ConfigurationService测试
    /// </summary>
    public class ConfigurationServiceTests : AbstractionsTestBase
    {
        public ConfigurationServiceTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task Get_WithExistingKey_ShouldReturnValue()
        {
            // Arrange
            var provider = new Mock<IConfigurationProvider>();
            var service = new ConfigurationService(provider.Object);
            
            provider.Setup(x => x.LoadAsync())
                .ReturnsAsync(new Dictionary<string, object>
                {
                    ["testKey"] = "testValue"
                });
            
            await service.InitializeAsync();
            
            // Act
            var result = service.Get<string>("testKey");
            
            // Assert
            result.Should().Be("testValue");
        }
        
        [Fact]
        public async Task Get_WithNonExistingKey_ShouldReturnDefaultValue()
        {
            // Arrange
            var provider = new Mock<IConfigurationProvider>();
            var service = new ConfigurationService(provider.Object);
            
            provider.Setup(x => x.LoadAsync())
                .ReturnsAsync(new Dictionary<string, object>());
            
            await service.InitializeAsync();
            
            // Act
            var result = service.Get<string>("nonExistingKey", "defaultValue");
            
            // Assert
            result.Should().Be("defaultValue");
        }
        
        [Fact]
        public async Task Set_WithValidParameters_ShouldSetValue()
        {
            // Arrange
            var provider = new Mock<IConfigurationProvider>();
            var service = new ConfigurationService(provider.Object);
            
            provider.Setup(x => x.LoadAsync())
                .ReturnsAsync(new Dictionary<string, object>());
            provider.Setup(x => x.SaveAsync(It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(true);
            
            await service.InitializeAsync();
            
            // Act
            var result = await service.SetAsync("testKey", "testValue");
            
            // Assert
            result.Should().BeTrue();
            provider.Verify(x => x.SaveAsync(It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("testKey") && d["testKey"].ToString() == "testValue")), Times.Once);
        }
    }
}
```

### 2.2 集成测试策略

#### 2.2.1 脚本执行集成测试
```csharp
// KeyForge.Tests/Integration/ScriptExecution/
namespace KeyForge.Tests.Integration.ScriptExecution
{
    /// <summary>
    /// 脚本执行集成测试基类
    /// </summary>
    public abstract class ScriptExecutionIntegrationTestBase : IClassFixture<IntegrationTestFixture>
    {
        protected readonly IntegrationTestFixture Fixture;
        protected readonly ITestOutputHelper Output;
        
        protected ScriptExecutionIntegrationTestBase(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }
        
        protected async Task<Script> CreateTestScriptAsync(string code, ScriptLanguage language = ScriptLanguage.CSharp)
        {
            var script = new Script
            {
                Name = $"Test Script {Guid.NewGuid()}",
                Code = code,
                Language = language,
                Type = ScriptType.Sequence
            };
            
            return script;
        }
    }
    
    /// <summary>
    /// ScriptEngine集成测试
    /// </summary>
    public class ScriptEngineIntegrationTests : ScriptExecutionIntegrationTestBase
    {
        public ScriptEngineIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }
        
        [Fact]
        public async Task ExecuteSimpleScript_ShouldSucceed()
        {
            // Arrange
            var script = await CreateTestScriptAsync(@"
                // Simple delay and key press
                await Task.Delay(100);
                Console.WriteLine(""Script executed successfully"");
            ");
            
            var engine = Fixture.GetService<IScriptEngine>();
            
            // Act
            var result = await engine.ExecuteAsync(script);
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ExecutionStatus.Completed);
            result.Message.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public async Task ExecuteScriptWithInputActions_ShouldExecuteActions()
        {
            // Arrange
            var script = await CreateTestScriptAsync(@"
                // Add key actions
                var keyAction = new ScriptAction(ActionType.KeyDown, ""Press A"");
                keyAction.Parameters[""KeyCode""] = KeyCode.A;
                keyAction.Parameters[""KeyState""] = KeyState.Press;
                
                var mouseAction = new ScriptAction(ActionType.MouseMove, ""Move mouse"");
                mouseAction.Parameters[""X""] = 100;
                mouseAction.Parameters[""Y""] = 200;
                
                script.Actions.Add(keyAction);
                script.Actions.Add(mouseAction);
            ");
            
            var engine = Fixture.GetService<IScriptEngine>();
            
            // Act
            var result = await engine.ExecuteAsync(script);
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ExecutionStatus.Completed);
            result.ActionsExecuted.Should().Be(2);
            result.ActionsSucceeded.Should().Be(2);
        }
        
        [Fact]
        public async Task ExecuteScriptWithImageRecognition_ShouldRecognizeImage()
        {
            // Arrange
            var script = await CreateTestScriptAsync(@"
                // Create image template
                var template = new ImageTemplate(""test"", imageData, new Rectangle(0, 0, 50, 50));
                
                // Recognize image
                var result = await imageService.RecognizeAsync(template);
                
                if (result.Status == RecognitionStatus.Success)
                {
                    // Click on recognized image
                    await mouseService.MoveMouseAsync(result.Location.X, result.Location.Y);
                    await mouseService.SendMouseClickAsync(MouseButton.Left);
                }
            ");
            
            var engine = Fixture.GetService<IScriptEngine>();
            
            // Act
            var result = await engine.ExecuteAsync(script);
            
            // Assert
            result.Should().NotBeNull();
            // Note: This test requires actual image data to be fully functional
        }
        
        [Fact]
        public async Task ExecuteScriptWithParameters_ShouldUseParameters()
        {
            // Arrange
            var script = await CreateTestScriptAsync(@"
                var speed = context.Parameters[""speed""] as double? ?? 1.0;
                var repeat = context.Parameters[""repeat""] as int? ?? 1;
                
                for (int i = 0; i < repeat; i++)
                {
                    await Task.Delay((int)(1000 / speed));
                    Console.WriteLine($""Iteration {i + 1}"");
                }
            ");
            
            var engine = Fixture.GetService<IScriptEngine>();
            var context = new ScriptContext(script.Id)
            {
                Parameters = new Dictionary<string, object>
                {
                    ["speed"] = 2.0,
                    ["repeat"] = 3
                }
            };
            
            // Act
            var result = await engine.ExecuteAsync(script, context);
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
    
    /// <summary>
    /// 集成测试Fixture
    /// </summary>
    public class IntegrationTestFixture : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        
        public IntegrationTestFixture()
        {
            var services = new ServiceCollection();
            
            // Register services
            services.AddLogging();
            services.AddKeyForgeServices();
            
            // Replace with test implementations
            services.Replace(ServiceDescriptor.Singleton<IImageRecognitionService, MockImageRecognitionService>());
            services.Replace(ServiceDescriptor.Singleton<IKeyboardService, MockKeyboardService>());
            services.Replace(ServiceDescriptor.Singleton<IMouseService, MockMouseService>());
            
            _serviceProvider = services.BuildServiceProvider();
        }
        
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }
        
        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
```

#### 2.2.2 图像识别集成测试
```csharp
// KeyForge.Tests/Integration/ImageRecognition/
namespace KeyForge.Tests.Integration.ImageRecognition
{
    /// <summary>
    /// 图像识别集成测试基类
    /// </summary>
    public abstract class ImageRecognitionIntegrationTestBase : IClassFixture<ImageRecognitionTestFixture>
    {
        protected readonly ImageRecognitionTestFixture Fixture;
        protected readonly ITestOutputHelper Output;
        
        protected ImageRecognitionIntegrationTestBase(ImageRecognitionTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }
    }
    
    /// <summary>
    /// ImageRecognitionService集成测试
    /// </summary>
    public class ImageRecognitionServiceIntegrationTests : ImageRecognitionIntegrationTestBase
    {
        public ImageRecognitionServiceIntegrationTests(ImageRecognitionTestFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }
        
        [Fact]
        public async Task RecognizeTemplate_WithValidTemplate_ShouldReturnResult()
        {
            // Arrange
            var service = Fixture.GetService<IImageRecognitionService>();
            var template = await CreateTestTemplateAsync();
            
            // Act
            var result = await service.RecognizeAsync(template);
            
            // Assert
            result.Should().NotBeNull();
            result.TemplateId.Should().Be(template.Id);
            result.Status.Should().BeOneOf(RecognitionStatus.Success, RecognitionStatus.NotFound);
        }
        
        [Fact]
        public async Task RecognizeBatch_WithMultipleTemplates_ShouldReturnMultipleResults()
        {
            // Arrange
            var service = Fixture.GetService<IImageRecognitionService>();
            var templates = new List<ImageTemplate>
            {
                await CreateTestTemplateAsync("template1"),
                await CreateTestTemplateAsync("template2"),
                await CreateTestTemplateAsync("template3")
            };
            
            // Act
            var results = await service.RecognizeBatchAsync(templates);
            
            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(3);
            results.Should().OnlyContain(r => templates.Any(t => t.Id == r.TemplateId));
        }
        
        [Fact]
        public async Task WaitForImage_WithExistingImage_ShouldReturnResult()
        {
            // Arrange
            var service = Fixture.GetService<IImageRecognitionService>();
            var template = await CreateTestTemplateAsync();
            
            // Act
            var result = await service.WaitForImageAsync(template, timeoutMs: 2000);
            
            // Assert
            result.Should().NotBeNull();
            result.TemplateId.Should().Be(template.Id);
        }
        
        [Fact]
        public async Task CreateTemplate_WithValidParameters_ShouldCreateTemplate()
        {
            // Arrange
            var service = Fixture.GetService<IImageRecognitionService>();
            var name = "test-template";
            var imageData = CreateTestImageData();
            var area = new Rectangle(0, 0, 100, 100);
            
            // Act
            var template = await service.CreateTemplateAsync(name, imageData, area);
            
            // Assert
            template.Should().NotBeNull();
            template.Name.Should().Be(name);
            template.ImageData.Should().BeEquivalentTo(imageData);
            template.TemplateArea.Should().Be(area);
        }
        
        private async Task<ImageTemplate> CreateTestTemplateAsync(string name = "test-template")
        {
            var service = Fixture.GetService<IImageRecognitionService>();
            var imageData = CreateTestImageData();
            var area = new Rectangle(0, 0, 100, 100);
            
            return await service.CreateTemplateAsync(name, imageData, area);
        }
        
        private byte[] CreateTestImageData()
        {
            // Create a simple test image (100x100 red rectangle)
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Red);
            
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
    
    /// <summary>
    /// 图像识别测试Fixture
    /// </summary>
    public class ImageRecognitionTestFixture : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        
        public ImageRecognitionTestFixture()
        {
            var services = new ServiceCollection();
            
            // Register services
            services.AddLogging();
            services.AddKeyForgeServices();
            
            // Use real image recognition service for integration tests
            services.AddSixLaborsImageSharp();
            
            _serviceProvider = services.BuildServiceProvider();
        }
        
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }
        
        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
```

### 2.3 端到端测试策略

#### 2.3.1 UI工作流测试
```csharp
// KeyForge.Tests/EndToEnd/UI.Workflow/
namespace KeyForge.Tests.EndToEnd.UI.Workflow
{
    /// <summary>
    /// UI工作流测试基类
    /// </summary>
    public abstract class UIWorkflowTestBase : IClassFixture<UITestFixture>
    {
        protected readonly UITestFixture Fixture;
        protected readonly ITestOutputHelper Output;
        
        protected UIWorkflowTestBase(UITestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }
    }
    
    /// <summary>
    /// ScriptCreationWorkflow测试
    /// </summary>
    public class ScriptCreationWorkflowTests : UIWorkflowTestBase
    {
        public ScriptCreationWorkflowTests(UITestFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }
        
        [Fact]
        public async Task CreateAndExecuteScript_ShouldSucceed()
        {
            // Arrange
            var window = Fixture.GetMainWindow();
            var scriptName = $"Test Script {Guid.NewGuid()}";
            var scriptCode = @"
                await Task.Delay(1000);
                Console.WriteLine(""Hello from UI test!"");
            ";
            
            // Act
            // Navigate to script creation
            await window.ClickButtonAsync("New Script");
            
            // Enter script details
            await window.EnterTextAsync("scriptName", scriptName);
            await window.EnterTextAsync("scriptCode", scriptCode);
            
            // Save script
            await window.ClickButtonAsync("Save");
            
            // Execute script
            await window.ClickButtonAsync("Execute");
            
            // Wait for execution completion
            await window.WaitForConditionAsync(() => 
                window.GetTextAsync("executionStatus").Result == "Completed", 
                TimeSpan.FromSeconds(10));
            
            // Assert
            var status = await window.GetTextAsync("executionStatus");
            status.Should().Be("Completed");
            
            var logs = await window.GetTextAsync("executionLogs");
            logs.Should().Contain("Hello from UI test!");
        }
        
        [Fact]
        public async Task CreateScriptWithImageRecognition_ShouldWork()
        {
            // Arrange
            var window = Fixture.GetMainWindow();
            var scriptName = $"Image Recognition Test {Guid.NewGuid()}";
            
            // Act
            // Navigate to script creation
            await window.ClickButtonAsync("New Script");
            
            // Enter script details
            await window.EnterTextAsync("scriptName", scriptName);
            
            // Add image recognition action
            await window.ClickButtonAsync("Add Action");
            await window.SelectDropdownAsync("actionType", "Image Recognition");
            
            // Configure image template
            await window.ClickButtonAsync("Select Image");
            await window.UploadFileAsync("imageFile", "test-button.png");
            
            // Save script
            await window.ClickButtonAsync("Save");
            
            // Assert
            var savedScripts = await window.GetListAsync("savedScripts");
            savedScripts.Should().Contain(scriptName);
        }
    }
    
    /// <summary>
    /// 配置管理工作流测试
    /// </summary>
    public class ConfigurationWorkflowTests : UIWorkflowTestBase
    {
        public ConfigurationWorkflowTests(UITestFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }
        
        [Fact]
        public async Task ConfigureApplicationSettings_ShouldPersist()
        {
            // Arrange
            var window = Fixture.GetMainWindow();
            
            // Act
            // Navigate to settings
            await window.ClickButtonAsync("Settings");
            
            // Change theme
            await window.SelectDropdownAsync("theme", "Dark");
            
            // Change font size
            await window.EnterTextAsync("fontSize", "14");
            
            // Enable auto-save
            await window.SetCheckboxAsync("autoSave", true);
            
            // Save settings
            await window.ClickButtonAsync("Save Settings");
            
            // Restart application
            await window.RestartAsync();
            
            // Navigate to settings again
            await window.ClickButtonAsync("Settings");
            
            // Assert
            var theme = await window.GetDropdownValueAsync("theme");
            theme.Should().Be("Dark");
            
            var fontSize = await window.GetTextAsync("fontSize");
            fontSize.Should().Be("14");
            
            var autoSave = await window.GetCheckboxValueAsync("autoSave");
            autoSave.Should().BeTrue();
        }
    }
    
    /// <summary>
    /// UI测试Fixture
    /// </summary>
    public class UITestFixture : IDisposable
    {
        private readonly AvaloniaApp _app;
        private readonly Window _mainWindow;
        
        public UITestFixture()
        {
            // Start Avalonia application
            _app = AvaloniaApp.Start();
            _mainWindow = _app.GetMainWindow();
        }
        
        public Window GetMainWindow()
        {
            return _mainWindow;
        }
        
        public void Dispose()
        {
            _app?.Dispose();
        }
    }
    
    /// <summary>
    /// Avalonia测试应用程序
    /// </summary>
    public class AvaloniaApp : IDisposable
    {
        private readonly App _app;
        private readonly Window _mainWindow;
        
        private AvaloniaApp(App app, Window mainWindow)
        {
            _app = app;
            _mainWindow = mainWindow;
        }
        
        public static AvaloniaApp Start()
        {
            var app = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .StartWithClassicDesktopLifetime([]);
            
            var mainWindow = app.MainWindow;
            return new AvaloniaApp(app, mainWindow);
        }
        
        public Window GetMainWindow()
        {
            return _mainWindow;
        }
        
        public void Dispose()
        {
            _app?.Dispose();
        }
    }
}
```

#### 2.3.2 跨平台测试
```csharp
// KeyForge.Tests/EndToEnd/CrossPlatform/
namespace KeyForge.Tests.EndToEnd.CrossPlatform
{
    /// <summary>
    /// 跨平台测试基类
    /// </summary>
    public abstract class CrossPlatformTestBase : IClassFixture<CrossPlatformTestFixture>
    {
        protected readonly CrossPlatformTestFixture Fixture;
        protected readonly ITestOutputHelper Output;
        
        protected CrossPlatformTestBase(CrossPlatformTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }
    }
    
    /// <summary>
    /// 跨平台功能测试
    /// </summary>
    public class CrossPlatformFeatureTests : CrossPlatformTestBase
    {
        public CrossPlatformFeatureTests(CrossPlatformTestFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }
        
        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Linux)]
        [InlineData(PlatformType.macOS)]
        public async Task InputSystem_ShouldWorkOnAllPlatforms(PlatformType platform)
        {
            // Arrange
            var services = Fixture.GetServicesForPlatform(platform);
            var keyboardService = services.GetRequiredService<IKeyboardService>();
            var mouseService = services.GetRequiredService<IMouseService>();
            
            // Act
            // Test keyboard input
            await keyboardService.SendKeyAsync(KeyCode.A, KeyState.Press);
            await Task.Delay(100);
            await keyboardService.SendKeyAsync(KeyCode.A, KeyState.Release);
            
            // Test mouse input
            await mouseService.MoveMouseAsync(100, 100);
            await Task.Delay(100);
            await mouseService.SendMouseClickAsync(MouseButton.Left);
            
            // Assert
            // Verify that the operations completed without exceptions
            // Note: Actual verification depends on platform-specific testing capabilities
        }
        
        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Linux)]
        [InlineData(PlatformType.macOS)]
        public async Task ImageRecognition_ShouldWorkOnAllPlatforms(PlatformType platform)
        {
            // Arrange
            var services = Fixture.GetServicesForPlatform(platform);
            var imageService = services.GetRequiredService<IImageRecognitionService>();
            var screenService = services.GetRequiredService<IScreenCaptureService>();
            
            // Act
            // Capture screen
            var screenData = await screenService.CaptureScreenAsync();
            
            // Create template from screen data
            var template = await imageService.CreateTemplateAsync("test", screenData, new Rectangle(0, 0, 100, 100));
            
            // Recognize template
            var result = await imageService.RecognizeAsync(template);
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().BeOneOf(RecognitionStatus.Success, RecognitionStatus.NotFound);
        }
        
        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Linux)]
        [InlineData(PlatformType.macOS)]
        public async Task ScriptExecution_ShouldWorkOnAllPlatforms(PlatformType platform)
        {
            // Arrange
            var services = Fixture.GetServicesForPlatform(platform);
            var scriptEngine = services.GetRequiredService<IScriptEngine>();
            
            var script = new Script
            {
                Name = "Cross-platform Test Script",
                Code = @"
                    await Task.Delay(500);
                    Console.WriteLine(""Cross-platform test executed successfully"");
                ",
                Language = ScriptLanguage.CSharp
            };
            
            // Act
            var result = await scriptEngine.ExecuteAsync(script);
            
            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
    
    /// <summary>
    /// 跨平台测试Fixture
    /// </summary>
    public class CrossPlatformTestFixture
    {
        private readonly Dictionary<PlatformType, ServiceProvider> _serviceProviders = new();
        
        public CrossPlatformTestFixture()
        {
            // Initialize service providers for each platform
            InitializePlatformServices();
        }
        
        private void InitializePlatformServices()
        {
            var platforms = new[] { PlatformType.Windows, PlatformType.Linux, PlatformType.macOS };
            
            foreach (var platform in platforms)
            {
                try
                {
                    var services = new ServiceCollection();
                    services.AddLogging();
                    services.AddKeyForgeServices();
                    
                    // Configure platform-specific services
                    ConfigurePlatformServices(services, platform);
                    
                    _serviceProviders[platform] = services.BuildServiceProvider();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize services for {platform}: {ex.Message}");
                }
            }
        }
        
        private void ConfigurePlatformServices(IServiceCollection services, PlatformType platform)
        {
            switch (platform)
            {
                case PlatformType.Windows:
                    services.AddSingleton<IKeyboardService, WindowsKeyboardService>();
                    services.AddSingleton<IMouseService, WindowsMouseService>();
                    services.AddSingleton<IScreenCaptureService, WindowsScreenCaptureService>();
                    break;
                    
                case PlatformType.Linux:
                    services.AddSingleton<IKeyboardService, LinuxKeyboardService>();
                    services.AddSingleton<IMouseService, LinuxMouseService>();
                    services.AddSingleton<IScreenCaptureService, LinuxScreenCaptureService>();
                    break;
                    
                case PlatformType.macOS:
                    services.AddSingleton<IKeyboardService, MacOSKeyboardService>();
                    services.AddSingleton<IMouseService, MacOSMouseService>();
                    services.AddSingleton<IScreenCaptureService, MacOSScreenCaptureService>();
                    break;
            }
        }
        
        public IServiceProvider GetServicesForPlatform(PlatformType platform)
        {
            if (_serviceProviders.TryGetValue(platform, out var serviceProvider))
            {
                return serviceProvider;
            }
            
            throw new NotSupportedException($"Platform {platform} is not supported or failed to initialize");
        }
    }
}
```

## 🔧 跨平台测试框架

### 3.1 测试基础设施

#### 3.1.1 测试基类
```csharp
// KeyForge.Tests/Common/
namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 测试基类
    /// </summary>
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper Output;
        protected readonly Fixture Fixture;
        protected readonly MockRepository MockRepository;
        
        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            Fixture = new Fixture();
            MockRepository = new MockRepository(MockBehavior.Strict);
            
            // Configure AutoFixture
            Fixture.Customize(new AutoMoqCustomization());
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                MockRepository.VerifyAll();
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    
    /// <summary>
    /// 跨平台测试基类
    /// </summary>
    public abstract class CrossPlatformTestBase : TestBase
    {
        protected readonly PlatformType CurrentPlatform;
        
        protected CrossPlatformTestBase(ITestOutputHelper output) : base(output)
        {
            CurrentPlatform = GetCurrentPlatform();
        }
        
        private PlatformType GetCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return PlatformType.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return PlatformType.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return PlatformType.macOS;
            
            return PlatformType.Windows; // Default
        }
        
        protected bool IsPlatformSupported(PlatformType platform)
        {
            return CurrentPlatform == platform;
        }
        
        protected void SkipIfPlatformNotSupported(PlatformType platform)
        {
            if (!IsPlatformSupported(platform))
            {
                Output.WriteLine($"Skipping test on {CurrentPlatform} - requires {platform}");
                throw new SkipTestException($"Test requires {platform} platform");
            }
        }
    }
    
    /// <summary>
    /// 集成测试基类
    /// </summary>
    public abstract class IntegrationTestBase : TestBase, IAsyncLifetime
    {
        protected ServiceProvider ServiceProvider { get; private set; }
        
        protected IntegrationTestBase(ITestOutputHelper output) : base(output) { }
        
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddKeyForgeServices();
            
            // Add test-specific services
            services.AddSingleton<ITestOutputHelper>(Output);
        }
        
        public async Task InitializeAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            ServiceProvider = services.BuildServiceProvider();
            
            await InitializeAsync(ServiceProvider);
        }
        
        protected virtual Task InitializeAsync(ServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
        
        public async Task DisposeAsync()
        {
            await DisposeAsync(ServiceProvider);
            ServiceProvider?.Dispose();
        }
        
        protected virtual Task DisposeAsync(ServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
        
        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }
        
        protected Mock<T> GetMock<T>() where T : class
        {
            return MockRepository.Create<T>();
        }
    }
}
```

#### 3.1.2 测试数据工厂
```csharp
// KeyForge.Tests/Common/
namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 测试数据工厂
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Fixture Fixture = new Fixture();
        
        static TestDataFactory()
        {
            // Configure AutoFixture
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Customize specific types
            Fixture.Customize<Script>(c => c
                .With(s => s.Id, Guid.NewGuid().ToString())
                .With(s => s.Status, ScriptStatus.Draft)
                .With(s => s.CreatedAt, DateTime.Now)
                .With(s => s.UpdatedAt, DateTime.Now));
            
            Fixture.Customize<ScriptAction>(c => c
                .With(a => a.Id, Guid.NewGuid().ToString())
                .With(a => a.Delay, 0)
                .With(a => a.Repeat, 1));
            
            Fixture.Customize<ImageTemplate>(c => c
                .With(t => t.Id, Guid.NewGuid().ToString())
                .With(t => t.CreatedAt, DateTime.Now)
                .With(t => t.UpdatedAt, DateTime.Now));
            
            Fixture.Customize<RecognitionResult>(c => c
                .With(r => r.Status, RecognitionStatus.Success)
                .With(r => r.Confidence, 0.8)
                .With(r => r.Location, new Point(100, 100))
                .With(r => r.MatchArea, new Rectangle(100, 100, 50, 50)));
        }
        
        // Script-related test data
        public static Script CreateScript(string name = null, string code = null, ScriptLanguage language = ScriptLanguage.CSharp)
        {
            return Fixture.Build<Script>()
                .With(s => s.Name, name ?? $"Test Script {Guid.NewGuid()}")
                .With(s => s.Code, code ?? "Console.WriteLine('Hello World');")
                .With(s => s.Language, language)
                .Create();
        }
        
        public static ScriptAction CreateKeyAction(KeyCode keyCode, KeyState state)
        {
            return Fixture.Build<ScriptAction>()
                .With(a => a.Type, ActionType.KeyDown)
                .With(a => a.Name, $"Key {keyCode} {state}")
                .With(a => a.Parameters, new Dictionary<string, object>
                {
                    ["KeyCode"] = keyCode,
                    ["KeyState"] = state
                })
                .Create();
        }
        
        public static ScriptAction CreateMouseAction(MouseButton button, MouseState state, int x = 0, int y = 0)
        {
            return Fixture.Build<ScriptAction>()
                .With(a => a.Type, ActionType.MouseDown)
                .With(a => a.Name, $"Mouse {button} {state}")
                .With(a => a.Parameters, new Dictionary<string, object>
                {
                    ["Button"] = button,
                    ["State"] = state,
                    ["X"] = x,
                    ["Y"] = y
                })
                .Create();
        }
        
        public static ScriptAction CreateDelayAction(int delayMs)
        {
            return Fixture.Build<ScriptAction>()
                .With(a => a.Type, ActionType.Delay)
                .With(a => a.Name, $"Delay {delayMs}ms")
                .With(a => a.Parameters, new Dictionary<string, object>
                {
                    ["Delay"] = delayMs
                })
                .Create();
        }
        
        // Image recognition test data
        public static ImageTemplate CreateImageTemplate(string name = null, byte[] imageData = null)
        {
            return Fixture.Build<ImageTemplate>()
                .With(t => t.Name, name ?? $"Test Template {Guid.NewGuid()}")
                .With(t => t.ImageData, imageData ?? CreateTestImageData())
                .With(t => t.TemplateArea, new Rectangle(0, 0, 100, 100))
                .Create();
        }
        
        public static RecognitionResult CreateRecognitionResult(string templateId = null, RecognitionStatus status = RecognitionStatus.Success)
        {
            return Fixture.Build<RecognitionResult>()
                .With(r => r.TemplateId, templateId ?? Guid.NewGuid().ToString())
                .With(r => r.Status, status)
                .Create();
        }
        
        // Configuration test data
        public static Configuration CreateConfiguration(string name = null)
        {
            return Fixture.Build<Configuration>()
                .With(c => c.Name, name ?? $"Test Configuration {Guid.NewGuid()}")
                .With(c => c.Values, new Dictionary<string, object>
                {
                    ["theme"] = "dark",
                    ["fontSize"] = 14,
                    ["autoSave"] = true
                })
                .Create();
        }
        
        // Execution test data
        public static ExecutionResult CreateExecutionResult(string scriptId = null, ExecutionStatus status = ExecutionStatus.Completed)
        {
            return Fixture.Build<ExecutionResult>()
                .With(r => r.ScriptId, scriptId ?? Guid.NewGuid().ToString())
                .With(r => r.Status, status)
                .With(r => r.Duration, TimeSpan.FromMilliseconds(100))
                .With(r => r.ActionsExecuted, 1)
                .With(r => r.ActionsSucceeded, 1)
                .Create();
        }
        
        // Helper methods
        public static byte[] CreateTestImageData(int width = 100, int height = 100)
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Create a simple test pattern
            graphics.Clear(Color.White);
            graphics.FillRectangle(Brushes.Red, 0, 0, width / 2, height / 2);
            graphics.FillRectangle(Brushes.Blue, width / 2, 0, width / 2, height / 2);
            graphics.FillRectangle(Brushes.Green, 0, height / 2, width / 2, height / 2);
            graphics.FillRectangle(Brushes.Yellow, width / 2, height / 2, width / 2, height / 2);
            
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
        
        public static List<ScriptAction> CreateScriptActionSequence(int count = 5)
        {
            var actions = new List<ScriptAction>();
            var random = new Random();
            
            for (int i = 0; i < count; i++)
            {
                var actionType = (ActionType)random.Next(0, 5);
                
                switch (actionType)
                {
                    case ActionType.KeyDown:
                    case ActionType.KeyUp:
                        var keyCode = (KeyCode)random.Next(0, 255);
                        var state = random.Next(0, 2) == 0 ? KeyState.Press : KeyState.Release;
                        actions.Add(CreateKeyAction(keyCode, state));
                        break;
                        
                    case ActionType.MouseDown:
                    case ActionType.MouseUp:
                        var button = (MouseButton)random.Next(1, 4);
                        var mouseState = random.Next(0, 2) == 0 ? MouseState.Down : MouseState.Up;
                        var x = random.Next(0, 1920);
                        var y = random.Next(0, 1080);
                        actions.Add(CreateMouseAction(button, mouseState, x, y));
                        break;
                        
                    case ActionType.Delay:
                        var delay = random.Next(100, 1000);
                        actions.Add(CreateDelayAction(delay));
                        break;
                }
            }
            
            return actions;
        }
        
        public static ScriptContext CreateScriptContext(string scriptId = null)
        {
            return Fixture.Build<ScriptContext>()
                .With(c => c.ScriptId, scriptId ?? Guid.NewGuid().ToString())
                .With(c => c.Variables, new Dictionary<string, object>
                {
                    ["counter"] = 0,
                    ["isActive"] = true
                })
                .With(c => c.Parameters, new Dictionary<string, object>
                {
                    ["speed"] = 1.0,
                    ["repeat"] = 1
                })
                .Create();
        }
    }
}
```

#### 3.1.3 模拟服务
```csharp
// KeyForge.Tests/Common/Mocks/
namespace KeyForge.Tests.Common.Mocks
{
    /// <summary>
    /// 模拟键盘服务
    /// </summary>
    public class MockKeyboardService : IKeyboardService
    {
        public event EventHandler<KeyInputEventArgs> OnKeyEvent;
        
        public ServiceStatus Status { get; private set; } = ServiceStatus.Stopped;
        
        public KeyState GetKeyState(KeyCode keyCode)
        {
            return KeyState.Release;
        }
        
        public async Task<bool> InitializeAsync()
        {
            Status = ServiceStatus.Running;
            return true;
        }
        
        public async Task<bool> SendKeyAsync(KeyCode keyCode, KeyState state)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            // Simulate key event
            OnKeyEvent?.Invoke(this, new KeyInputEventArgs(keyCode, state));
            
            return true;
        }
        
        public async Task<bool> SendTextAsync(string text)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            foreach (var c in text)
            {
                // Simulate text input
                await Task.Delay(10);
            }
            
            return true;
        }
        
        public async Task<bool> SendKeyCombinationAsync(KeyCode[] keyCodes)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            // Press all keys
            foreach (var keyCode in keyCodes)
            {
                await SendKeyAsync(keyCode, KeyState.Press);
            }
            
            await Task.Delay(50);
            
            // Release all keys
            foreach (var keyCode in keyCodes.Reverse())
            {
                await SendKeyAsync(keyCode, KeyState.Release);
            }
            
            return true;
        }
        
        public async Task StartAsync()
        {
            Status = ServiceStatus.Running;
            await Task.CompletedTask;
        }
        
        public async Task StopAsync()
        {
            Status = ServiceStatus.Stopped;
            await Task.CompletedTask;
        }
        
        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
    
    /// <summary>
    /// 模拟鼠标服务
    /// </summary>
    public class MockMouseService : IMouseService
    {
        public event EventHandler<MouseInputEventArgs> OnMouseEvent;
        
        public ServiceStatus Status { get; private set; } = ServiceStatus.Stopped;
        
        private Point _currentPosition = Point.Empty;
        
        public Point GetMousePosition()
        {
            return _currentPosition;
        }
        
        public MouseState GetMouseState()
        {
            return MouseState.Up;
        }
        
        public async Task<bool> InitializeAsync()
        {
            Status = ServiceStatus.Running;
            return true;
        }
        
        public async Task<bool> MoveMouseAsync(int x, int y)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            _currentPosition = new Point(x, y);
            
            // Simulate mouse move event
            OnMouseEvent?.Invoke(this, new MouseInputEventArgs(MouseButton.None, MouseState.Up, _currentPosition));
            
            return true;
        }
        
        public async Task<bool> MoveMouseRelativeAsync(int deltaX, int deltaY)
        {
            var newX = _currentPosition.X + deltaX;
            var newY = _currentPosition.Y + deltaY;
            return await MoveMouseAsync(newX, newY);
        }
        
        public async Task<bool> SendMouseButtonAsync(MouseButton button, MouseState state)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            // Simulate mouse event
            OnMouseEvent?.Invoke(this, new MouseInputEventArgs(button, state, _currentPosition));
            
            return true;
        }
        
        public async Task<bool> SendMouseClickAsync(MouseButton button)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            await SendMouseButtonAsync(button, MouseState.Down);
            await Task.Delay(50);
            await SendMouseButtonAsync(button, MouseState.Up);
            
            return true;
        }
        
        public async Task<bool> SendMouseDoubleClickAsync(MouseButton button)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            await SendMouseClickAsync(button);
            await Task.Delay(100);
            await SendMouseClickAsync(button);
            
            return true;
        }
        
        public async Task<bool> SendMouseRightClickAsync()
        {
            return await SendMouseClickAsync(MouseButton.Right);
        }
        
        public async Task<bool> SendMouseWheelAsync(int delta)
        {
            if (Status != ServiceStatus.Running)
                return false;
            
            // Simulate mouse wheel event
            OnMouseEvent?.Invoke(this, new MouseInputEventArgs(MouseButton.None, MouseState.Up, _currentPosition, delta));
            
            return true;
        }
        
        public async Task StartAsync()
        {
            Status = ServiceStatus.Running;
            await Task.CompletedTask;
        }
        
        public async Task StopAsync()
        {
            Status = ServiceStatus.Stopped;
            await Task.CompletedTask;
        }
        
        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
    
    /// <summary>
    /// 模拟图像识别服务
    /// </summary>
    public class MockImageRecognitionService : IImageRecognitionService
    {
        private readonly Dictionary<string, ImageTemplate> _templates = new();
        
        public async Task<bool> InitializeAsync()
        {
            return true;
        }
        
        public async Task<RecognitionResult> RecognizeAsync(ImageTemplate template, ScreenRegion region = null)
        {
            await Task.Delay(50); // Simulate processing time
            
            // Simulate recognition with random confidence
            var random = new Random();
            var confidence = random.NextDouble();
            
            if (confidence > 0.7)
            {
                return RecognitionResult.Success(
                    template.Id,
                    confidence,
                    new Point(random.Next(0, 1920), random.Next(0, 1080)),
                    new Rectangle(random.Next(0, 1820), random.Next(0, 980), 100, 50)
                );
            }
            
            return RecognitionResult.Failed("Template not found");
        }
        
        public async Task<List<RecognitionResult>> RecognizeBatchAsync(List<ImageTemplate> templates, ScreenRegion region = null)
        {
            var results = new List<RecognitionResult>();
            
            foreach (var template in templates)
            {
                var result = await RecognizeAsync(template, region);
                results.Add(result);
            }
            
            return results;
        }
        
        public async Task<List<RecognitionResult>> FindAllAsync(ImageTemplate template, ScreenRegion region = null)
        {
            var results = new List<RecognitionResult>();
            var random = new Random();
            
            // Simulate finding multiple matches
            var matchCount = random.Next(1, 5);
            
            for (int i = 0; i < matchCount; i++)
            {
                var result = await RecognizeAsync(template, region);
                if (result.Status == RecognitionStatus.Success)
                {
                    results.Add(result);
                }
            }
            
            return results;
        }
        
        public async Task<RecognitionResult> WaitForImageAsync(ImageTemplate template, int timeoutMs = 10000, ScreenRegion region = null)
        {
            var startTime = DateTime.Now;
            
            while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(timeoutMs))
            {
                var result = await RecognizeAsync(template, region);
                if (result.Status == RecognitionStatus.Success)
                {
                    return result;
                }
                
                await Task.Delay(100);
            }
            
            return RecognitionResult.Failed("Timeout waiting for image");
        }
        
        public async Task<ImageTemplate> CreateTemplateAsync(string name, byte[] imageData, Rectangle templateArea)
        {
            var template = new ImageTemplate
            {
                Name = name,
                ImageData = imageData,
                TemplateArea = templateArea
            };
            
            _templates[template.Id] = template;
            
            return template;
        }
        
        public async Task<bool> SaveTemplateAsync(ImageTemplate template)
        {
            _templates[template.Id] = template;
            return true;
        }
        
        public async Task<ImageTemplate> LoadTemplateAsync(string templateName)
        {
            var template = _templates.Values.FirstOrDefault(t => t.Name == templateName);
            return await Task.FromResult(template);
        }
        
        public async Task<bool> DeleteTemplateAsync(string templateName)
        {
            var template = _templates.Values.FirstOrDefault(t => t.Name == templateName);
            if (template != null)
            {
                _templates.Remove(template.Id);
                return true;
            }
            
            return false;
        }
        
        public async Task<List<ImageTemplate>> GetAllTemplatesAsync()
        {
            return await Task.FromResult(_templates.Values.ToList());
        }
        
        public void SetRecognitionParameters(RecognitionParameters parameters)
        {
            // Store parameters if needed
        }
        
        public RecognitionParameters GetRecognitionParameters()
        {
            return new RecognitionParameters();
        }
        
        public void Dispose()
        {
            // Cleanup
        }
    }
    
    /// <summary>
    /// 模拟屏幕捕获服务
    /// </summary>
    public class MockScreenCaptureService : IScreenCaptureService
    {
        public event EventHandler<ScreenChangedEventArgs> OnScreenChanged;
        
        public async Task<byte[]> CaptureScreenAsync()
        {
            await Task.Delay(100); // Simulate capture time
            
            // Return a simple test image
            return TestDataFactory.CreateTestImageData(1920, 1080);
        }
        
        public async Task<byte[]> CaptureRegionAsync(ScreenRegion region)
        {
            await Task.Delay(50); // Simulate capture time
            
            // Return a smaller test image
            return TestDataFactory.CreateTestImageData(region.Width, region.Height);
        }
        
        public async Task<byte[]> CaptureWindowAsync(IntPtr windowHandle)
        {
            await Task.Delay(75); // Simulate capture time
            
            // Return a test image
            return TestDataFactory.CreateTestImageData(800, 600);
        }
        
        public Size GetScreenSize()
        {
            return new Size(1920, 1080);
        }
        
        public double GetScreenDpi()
        {
            return 96.0;
        }
        
        public ScreenInfo GetPrimaryScreen()
        {
            return new ScreenInfo
            {
                Index = 0,
                DeviceName = "Primary Display",
                Bounds = new Rectangle(0, 0, 1920, 1080),
                WorkingArea = new Rectangle(0, 0, 1920, 1080),
                Dpi = 96.0,
                IsPrimary = true
            };
        }
        
        public List<ScreenInfo> GetAllScreens()
        {
            return new List<ScreenInfo>
            {
                GetPrimaryScreen()
            };
        }
        
        public void Dispose()
        {
            // Cleanup
        }
    }
}
```

## 🚀 持续集成测试流水线

### 4.1 CI/CD 配置

#### 4.1.1 GitHub Actions 配置
```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Restore dependencies
        run: dotnet restore
        
      - name: Build
        run: dotnet build --configuration Release --no-restore
        
      - name: Run Unit Tests
        run: dotnet test KeyForge.Tests --configuration Release --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"
        
      - name: Run Integration Tests
        run: dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Integration" --logger "console;verbosity=detailed"
        
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          file: ./coverage.xml
          flags: unittests
          name: codecov-umbrella

  test-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Install Linux dependencies
        run: |
          sudo apt-get update
          sudo apt-get install -y libx11-dev libxtst-dev libxrandr-dev libxinerama-dev libxcursor-dev libxi-dev
          
      - name: Restore dependencies
        run: dotnet restore
        
      - name: Build
        run: dotnet build --configuration Release --no-restore
        
      - name: Run Unit Tests
        run: dotnet test KeyForge.Tests --configuration Release --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"
        
      - name: Run Integration Tests
        run: dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Integration" --logger "console;verbosity=detailed"

  test-macos:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Restore dependencies
        run: dotnet restore
        
      - name: Build
        run: dotnet build --configuration Release --no-restore
        
      - name: Run Unit Tests
        run: dotnet test KeyForge.Tests --configuration Release --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"
        
      - name: Run Integration Tests
        run: dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Integration" --logger "console;verbosity=detailed"

  coverage:
    needs: [test-windows, test-linux, test-macos]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Generate coverage report
        run: |
          dotnet tool install -g dotnet-reportgenerator-globaltool
          reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:HtmlInline_AzurePipelines;Cobertura
          
      - name: Upload coverage report
        uses: actions/upload-artifact@v3
        with:
          name: coverage-report
          path: coverage-report/
          
      - name: Comment coverage
        uses: romeovs/lcov-reporter-action@v0.2.16
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          lcov-file: coverage.info

  quality-gate:
    needs: [test-windows, test-linux, test-macos, coverage]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Install quality tools
        run: |
          dotnet tool install -g dotnet-sonarscanner
          dotnet tool install -g dotnet-format
          
      - name: Run code quality checks
        run: |
          dotnet format --verify-no-changes
          dotnet sonarscanner begin /k:"KeyForge" /o:"organization" /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
          dotnet build
          dotnet sonarscanner end /d:sonar.login="${{ secrets.SONAR_TOKEN }}"

  deploy:
    needs: [quality-gate]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Build
        run: dotnet build --configuration Release
        
      - name: Publish
        run: dotnet publish --configuration Release --output ./publish
        
      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: keyforge-release
          path: ./publish/
```

#### 4.1.2 测试运行脚本
```bash
#!/bin/bash
# run-tests.sh

set -e

echo "🧪 Running KeyForge Test Suite..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Detect platform
PLATFORM="unknown"
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="linux"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macos"
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
    PLATFORM="windows"
fi

print_status "Detected platform: $PLATFORM"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet is not installed. Please install .NET 8.0 or later."
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version | cut -d. -f1-2)
if [[ "$DOTNET_VERSION" < "8.0" ]]; then
    print_error "dotnet version 8.0 or later is required. Current version: $DOTNET_VERSION"
    exit 1
fi

print_status "Using .NET $DOTNET_VERSION"

# Install platform-specific dependencies if needed
if [[ "$PLATFORM" == "linux" ]]; then
    print_status "Installing Linux dependencies..."
    sudo apt-get update
    sudo apt-get install -y libx11-dev libxtst-dev libxrandr-dev libxinerama-dev libxcursor-dev libxi-dev
fi

# Restore dependencies
print_status "Restoring dependencies..."
dotnet restore

# Build solution
print_status "Building solution..."
dotnet build --configuration Release

# Run unit tests
print_status "Running unit tests..."
dotnet test KeyForge.Tests --configuration Release --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"

# Run integration tests
print_status "Running integration tests..."
dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Integration" --logger "console;verbosity=detailed"

# Run platform-specific tests
print_status "Running platform-specific tests..."
if [[ "$PLATFORM" == "linux" ]]; then
    dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Linux" --logger "console;verbosity=detailed"
elif [[ "$PLATFORM" == "macos" ]]; then
    dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=macOS" --logger "console;verbosity=detailed"
elif [[ "$PLATFORM" == "windows" ]]; then
    dotnet test KeyForge.Tests --configuration Release --filter "TestCategory=Windows" --logger "console;verbosity=detailed"
fi

# Generate coverage report
print_status "Generating coverage report..."
if command -v reportgenerator &> /dev/null; then
    reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:HtmlInline_AzurePipelines;Cobertura
    print_status "Coverage report generated in coverage-report/"
else
    print_warning "reportgenerator not found. Skipping coverage report generation."
fi

print_status "🎉 All tests completed successfully!"

# Display summary
echo ""
echo "📊 Test Summary:"
echo "   - Unit Tests: ✅"
echo "   - Integration Tests: ✅"
echo "   - Platform Tests: ✅"
echo "   - Coverage Report: $(test -f coverage-report/index.html && echo '✅' || echo '❌')"
echo ""
echo "🔗 Test Results:"
echo "   - Coverage Report: file://$(pwd)/coverage-report/index.html"
echo "   - Test Logs: Check console output above"
```

### 4.2 测试质量门禁

#### 4.2.1 质量门禁配置
```yaml
# quality-gate.yml
quality_gate:
  test_coverage:
    minimum: 80%
    critical: true
    message: "Test coverage must be at least 80%"
  
  code_coverage:
    minimum: 75%
    critical: true
    message: "Code coverage must be at least 75%"
  
  branch_coverage:
    minimum: 70%
    critical: false
    message: "Branch coverage should be at least 70%"
  
  line_coverage:
    minimum: 80%
    critical: true
    message: "Line coverage must be at least 80%"
  
  test_success_rate:
    minimum: 95%
    critical: true
    message: "Test success rate must be at least 95%"
  
  code_quality:
    maximum_issues: 0
    critical: true
    message: "No code quality issues allowed"
  
  security_issues:
    maximum: 0
    critical: true
    message: "No security issues allowed"
  
  performance_issues:
    maximum: 0
    critical: false
    message: "No performance issues allowed"
  
  documentation_coverage:
    minimum: 90%
    critical: false
    message: "Documentation coverage should be at least 90%"
```

#### 4.2.2 质量检查脚本
```powershell
# quality-check.ps1
param(
    [string]$Configuration = "Release",
    [string]$TestProject = "KeyForge.Tests"
)

Write-Host "🔍 Running Quality Checks..." -ForegroundColor Green

# Test coverage check
Write-Host "📊 Checking test coverage..." -ForegroundColor Yellow
$coverageResult = dotnet test $TestProject --configuration $Configuration --collect:"XPlat Code Coverage" --logger "trx"
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "*.coverage" -Recurse | Select-Object -First 1

if ($coverageFile) {
    Write-Host "Coverage file found: $($coverageFile.FullName)" -ForegroundColor Green
} else {
    Write-Host "No coverage file found" -ForegroundColor Red
    exit 1
}

# Code quality check
Write-Host "🔧 Checking code quality..." -ForegroundColor Yellow
$formatResult = dotnet format --verify-no-changes --dry-run
if ($formatResult -ne $null) {
    Write-Host "Code formatting issues found" -ForegroundColor Red
    Write-Host $formatResult
    exit 1
}

# Security analysis
Write-Host "🔒 Running security analysis..." -ForegroundColor Yellow
if (Get-Command "dotnet-sonarscanner" -ErrorAction SilentlyContinue) {
    dotnet sonarscanner begin /k:"KeyForge" /d:sonar.login="$env:SONAR_TOKEN"
    dotnet build
    dotnet sonarscanner end /d:sonar.login="$env:SONAR_TOKEN"
} else {
    Write-Host "SonarScanner not found, skipping security analysis" -ForegroundColor Yellow
}

# Performance tests
Write-Host "⚡ Running performance tests..." -ForegroundColor Yellow
$performanceResult = dotnet test $TestProject --configuration $Configuration --filter "TestCategory=Performance"

# Documentation check
Write-Host "📚 Checking documentation coverage..." -ForegroundColor Yellow
$docFiles = Get-ChildItem -Path "**/*.md" -Recurse | Where-Object { $_.FullName -notlike "*/obj/*" -and $_.FullName -notlike "*/bin/*" }
$csFiles = Get-ChildItem -Path "**/*.cs" -Recurse | Where-Object { $_.FullName -notlike "*/obj/*" -and $_.FullName -notlike "*/bin/*" }

if ($docFiles.Count -gt 0 -and $csFiles.Count -gt 0) {
    $docRatio = [math]::Round(($docFiles.Count / $csFiles.Count) * 100, 2)
    Write-Host "Documentation ratio: $docRatio%" -ForegroundColor Green
    
    if ($docRatio -lt 10) {
        Write-Host "Documentation ratio is low: $docRatio%" -ForegroundColor Yellow
    }
}

Write-Host "✅ Quality checks completed!" -ForegroundColor Green
```

## 📋 测试最佳实践

### 5.1 测试命名约定

#### 5.1.1 单元测试命名
```csharp
// Good naming conventions
public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var userService = new UserService();
        var user = new User("test@example.com", "password123");
        
        // Act
        var result = await userService.CreateUserAsync(user);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var userService = new UserService();
        var user = new User("existing@example.com", "password123");
        
        // Act & Assert
        var action = async () => await userService.CreateUserAsync(user);
        await action.Should().ThrowAsync<DuplicateEmailException>();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateUser_WithInvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Arrange
        var userService = new UserService();
        var user = new User(invalidEmail, "password123");
        
        // Act & Assert
        var action = async () => await userService.CreateUserAsync(user);
        await action.Should().ThrowAsync<ValidationException>();
    }
}
```

#### 5.1.2 集成测试命名
```csharp
public class UserIntegrationTests
{
    [Fact]
    public async Task GetUserProfile_WithValidUser_ShouldReturnProfile()
    {
        // Arrange
        var context = GetTestDbContext();
        var userService = new UserService(context);
        var user = await CreateTestUserAsync(context);
        
        // Act
        var result = await userService.GetUserProfileAsync(user.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
    }
    
    [Fact]
    public async Task UpdateUserProfile_WithValidData_ShouldUpdateProfile()
    {
        // Arrange
        var context = GetTestDbContext();
        var userService = new UserService(context);
        var user = await CreateTestUserAsync(context);
        var updateData = new UserProfileUpdate { Name = "New Name" };
        
        // Act
        var result = await userService.UpdateUserProfileAsync(user.Id, updateData);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
    }
}
```

### 5.2 测试数据管理

#### 5.2.1 测试数据工厂模式
```csharp
public static class UserTestDataFactory
{
    public static User CreateValidUser()
    {
        return new User
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "ValidPassword123!",
            Name = "Test User",
            CreatedAt = DateTime.Now
        };
    }
    
    public static User CreateInvalidUser()
    {
        return new User
        {
            Email = "invalid-email",
            Password = "123",
            Name = ""
        };
    }
    
    public static List<User> CreateUsers(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateValidUser());
        }
        return users;
    }
}
```

#### 5.2.2 测试数据库管理
```csharp
public class TestDatabaseManager
{
    private readonly string _connectionString;
    
    public TestDatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    public async Task ResetDatabaseAsync()
    {
        using var connection = await CreateConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = "EXEC sp_MSForEachTable 'DELETE FROM ?'";
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = "EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'";
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task SeedDatabaseAsync()
    {
        using var connection = await CreateConnectionAsync();
        using var command = connection.CreateCommand();
        
        // Seed test data
        command.CommandText = @"
            INSERT INTO Users (Email, Password, Name, CreatedAt)
            VALUES ('admin@example.com', 'hashed_password', 'Admin User', GETDATE())";
        await command.ExecuteNonQueryAsync();
    }
}
```

### 5.3 测试断言最佳实践

#### 5.3.1 使用 FluentAssertions
```csharp
// Good assertions with FluentAssertions
[Fact]
public async Task ProcessOrder_WithValidOrder_ShouldSucceed()
{
    // Arrange
    var orderService = new OrderService();
    var order = new Order("test@example.com", new List<OrderItem>
    {
        new OrderItem("Product1", 2, 10.00m),
        new OrderItem("Product2", 1, 20.00m)
    });
    
    // Act
    var result = await orderService.ProcessOrderAsync(order);
    
    // Assert
    result.Should().NotBeNull();
    result.Status.Should().Be(OrderStatus.Processed);
    result.TotalAmount.Should().Be(40.00m);
    result.Items.Should().HaveCount(2);
    result.ProcessedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    result.OrderId.Should().NotBeEmpty();
    result.CustomerEmail.Should().Be("test@example.com");
}

// Bad assertions (too many asserts)
[Fact]
public async Task ProcessOrder_WithValidOrder_ShouldSucceed_Bad()
{
    // Arrange
    var orderService = new OrderService();
    var order = new Order("test@example.com", new List<OrderItem>
    {
        new OrderItem("Product1", 2, 10.00m),
        new OrderItem("Product2", 1, 20.00m)
    });
    
    // Act
    var result = await orderService.ProcessOrderAsync(order);
    
    // Assert - Too many individual assertions
    Assert.NotNull(result);
    Assert.Equal(OrderStatus.Processed, result.Status);
    Assert.Equal(40.00m, result.TotalAmount);
    Assert.Equal(2, result.Items.Count);
    Assert.True((DateTime.Now - result.ProcessedAt).TotalSeconds < 1);
    Assert.NotEmpty(result.OrderId);
    Assert.Equal("test@example.com", result.CustomerEmail);
}
```

## 📊 测试报告和监控

### 6.1 测试报告生成

#### 6.1.1 覆盖率报告
```xml
<!-- coverage.xml -->
<coverage version="1.9.11">
  <sources>
    <source>.</source>
  </sources>
  <packages>
    <package name="KeyForge.Core" line-rate="0.85" branch-rate="0.80" complexity="45">
      <classes>
        <class name="KeyForge.Core.Services.ScriptService" line-rate="0.90" branch-rate="0.85">
          <methods>
            <method name="ExecuteScriptAsync" line-rate="1.00" branch-rate="0.75">
              <lines>
                <line number="10" hits="5" branch="False"/>
                <line number="11" hits="5" branch="False"/>
                <line number="12" hits="3" branch="True" condition-coverage="75%"/>
              </lines>
            </method>
          </methods>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
```

#### 6.1.2 测试结果报告
```json
// test-results.json
{
  "summary": {
    "total": 150,
    "passed": 145,
    "failed": 3,
    "skipped": 2,
    "duration": "00:02:30",
    "successRate": 96.7
  },
  "coverage": {
    "lineCoverage": 85.2,
    "branchCoverage": 78.9,
    "methodCoverage": 92.1
  },
  "performance": {
    "averageTestDuration": "00:00:01.2",
    "slowestTest": "00:00:05.6",
    "fastestTest": "00:00:00.1"
  },
  "categories": {
    "Unit": {
      "total": 100,
      "passed": 98,
      "failed": 1,
      "skipped": 1
    },
    "Integration": {
      "total": 30,
      "passed": 28,
      "failed": 2,
      "skipped": 0
    },
    "E2E": {
      "total": 20,
      "passed": 19,
      "failed": 0,
      "skipped": 1
    }
  }
}
```

### 6.2 测试监控和告警

#### 6.2.1 测试监控配置
```yaml
# monitoring-config.yml
monitoring:
  test_metrics:
    - name: test_execution_time
      type: histogram
      buckets: [0.1, 0.5, 1.0, 2.0, 5.0, 10.0]
      description: "Test execution time in seconds"
      
    - name: test_success_rate
      type: gauge
      description: "Test success rate percentage"
      
    - name: test_coverage
      type: gauge
      description: "Code coverage percentage"
      
  alerts:
    - name: test_execution_time_alert
      condition: "test_execution_time > 10.0"
      severity: warning
      message: "Test execution time is too high"
      
    - name: test_success_rate_alert
      condition: "test_success_rate < 95.0"
      severity: critical
      message: "Test success rate is below threshold"
      
    - name: test_coverage_alert
      condition: "test_coverage < 80.0"
      severity: critical
      message: "Test coverage is below threshold"
```

## 🎯 总结

本测试架构设计为KeyForge项目提供了完整的测试解决方案，包括：

1. **分层测试策略**：单元测试、集成测试、端到端测试的完整覆盖
2. **跨平台测试框架**：支持Windows、Linux、macOS的统一测试框架
3. **模拟和存根策略**：完整的模拟服务和测试数据工厂
4. **持续集成测试流水线**：自动化的CI/CD流程和质量门禁
5. **测试最佳实践**：命名约定、数据管理、断言模式

### 6.3 测试架构优势

- **全面覆盖**：覆盖所有层次的代码和功能
- **跨平台兼容**：确保在所有支持平台上正常工作
- **自动化程度高**：最小化手动测试的需求
- **质量保证**：严格的质量门禁和监控
- **可维护性**：清晰的测试结构和命名约定
- **可扩展性**：易于添加新的测试用例和测试类型

### 6.4 实施建议

1. **分阶段实施**：先实现单元测试，再逐步增加集成测试和E2E测试
2. **持续改进**：定期审查和改进测试用例
3. **团队培训**：确保团队成员了解测试最佳实践
4. **工具选择**：选择合适的测试框架和工具
5. **文档维护**：保持测试文档的更新和同步

通过实施本测试架构设计，KeyForge项目将具备高质量的测试覆盖，确保系统的稳定性、可靠性和跨平台兼容性。

---

**文档完成时间**：2025-08-25  
**测试架构版本**：v2.0  
**下次更新**：根据实施反馈进行调整