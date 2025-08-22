using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit;
using Moq;
using KeyForge.HAL.Abstractions;

namespace KeyForge.Tests
{
    /// <summary>
    /// 测试基类
    /// 简化实现：基本的测试基础设施
    /// 原本实现：包含完整的测试框架和模拟工具
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected ILogger<TestBase> Logger { get; private set; }
        protected ITestOutputHelper Output { get; }
        protected MockRepository MockRepository { get; }
        
        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            MockRepository = new MockRepository(MockBehavior.Strict);
            
            var services = new ServiceCollection();
            
            // 注册基础服务
            services.AddLogging(builder => 
            {
                builder.AddDebug();
                builder.AddXunit(Output);
            });
            
            services.AddSingleton(MockRepository);
            services.AddSingleton<IMockFactory, MockFactory>();
            services.AddSingleton<ITestDataFactory, TestDataFactory>();
            
            // 注册被测试的服务
            RegisterServices(services);
            
            ServiceProvider = services.BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
        }
        
        /// <summary>
        /// 注册测试服务
        /// </summary>
        /// <param name="services">服务集合</param>
        protected abstract void RegisterServices(IServiceCollection services);
        
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例</returns>
        protected T GetService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }
        
        /// <summary>
        /// 创建Mock对象
        /// </summary>
        /// <typeparam name="T">Mock类型</typeparam>
        /// <returns>Mock对象</returns>
        protected Mock<T> CreateMock<T>() where T : class
        {
            return MockRepository.Create<T>();
        }
        
        /// <summary>
        /// 输出测试信息
        /// </summary>
        /// <param name="message">消息</param>
        protected void WriteTestInfo(string message)
        {
            Output.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss.fff} - {message}");
        }
        
        /// <summary>
        /// 输出测试警告
        /// </summary>
        /// <param name="message">消息</param>
        protected void WriteTestWarning(string message)
        {
            Output.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss.fff} - {message}");
        }
        
        /// <summary>
        /// 输出测试错误
        /// </summary>
        /// <param name="message">消息</param>
        protected void WriteTestError(string message)
        {
            Output.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss.fff} - {message}");
        }
        
        /// <summary>
        /// 断言不为空
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="paramName">参数名</param>
        protected static void AssertNotNull(object value, string paramName)
        {
            Assert.NotNull(value);
        }
        
        /// <summary>
        /// 断言字符串不为空
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="paramName">参数名</param>
        protected static void AssertNotNullOrEmpty(string value, string paramName)
        {
            Assert.False(string.IsNullOrEmpty(value), $"Parameter '{paramName}' cannot be null or empty");
        }
        
        /// <summary>
        /// 异步测试助手
        /// </summary>
        /// <param name="testAction">测试动作</param>
        /// <returns>任务</returns>
        protected async Task RunTestAsync(Func<Task> testAction)
        {
            try
            {
                await testAction();
            }
            catch (Exception ex)
            {
                WriteTestError($"Test failed: {ex.Message}");
                WriteTestError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            MockRepository.VerifyAll();
        }
    }

    /// <summary>
    /// HAL测试基类
    /// </summary>
    public abstract class HALTestBase : TestBase
    {
        protected HALTestBase(ITestOutputHelper output) : base(output)
        {
        }
        
        /// <summary>
        /// 创建HAL Mock
        /// </summary>
        /// <returns>HAL Mock对象</returns>
        protected Mock<IHardwareAbstractionLayer> CreateHALMock()
        {
            var halMock = CreateMock<IHardwareAbstractionLayer>();
            
            // 设置基本属性
            halMock.Setup(x => x.PlatformInfo).Returns(new PlatformInfo
            {
                Platform = Platform.Windows,
                Name = "Windows",
                Version = "10.0",
                Architecture = "x64",
                Capabilities = new PlatformCapabilities()
            });
            
            halMock.Setup(x => x.Status).Returns(HALStatus.Running);
            halMock.Setup(x => x.IsInitialized).Returns(true);
            
            return halMock;
        }
        
        /// <summary>
        /// 创建键盘服务Mock
        /// </summary>
        /// <returns>键盘服务Mock对象</returns>
        protected Mock<IKeyboardService> CreateKeyboardServiceMock()
        {
            var keyboardMock = CreateMock<IKeyboardService>();
            
            // 设置基本属性
            keyboardMock.Setup(x => x.Capabilities).Returns(new KeyboardCapabilities());
            
            // 设置基本方法
            keyboardMock.Setup(x => x.PressKeyAsync(It.IsAny<KeyCode>(), It.IsAny<KeyModifiers>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.ReleaseKeyAsync(It.IsAny<KeyCode>(), It.IsAny<KeyModifiers>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.TypeTextAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.GetKeyStateAsync(It.IsAny<KeyCode>()))
                .ReturnsAsync(KeyState.Up);
            
            return keyboardMock;
        }
        
        /// <summary>
        /// 创建鼠标服务Mock
        /// </summary>
        /// <returns>鼠标服务Mock对象</returns>
        protected Mock<IMouseService> CreateMouseServiceMock()
        {
            var mouseMock = CreateMock<IMouseService>();
            
            // 设置基本方法
            mouseMock.Setup(x => x.MoveToAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.ClickAsync(It.IsAny<MouseButton>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.GetPositionAsync())
                .ReturnsAsync(new Point(0, 0));
            mouseMock.Setup(x => x.GetStateAsync())
                .ReturnsAsync(new MouseState());
            
            return mouseMock;
        }
        
        /// <summary>
        /// 创建屏幕服务Mock
        /// </summary>
        /// <returns>屏幕服务Mock对象</returns>
        protected Mock<IScreenService> CreateScreenServiceMock()
        {
            var screenMock = CreateMock<IScreenService>();
            
            // 设置基本方法
            screenMock.Setup(x => x.CaptureScreenAsync(It.IsAny<Rectangle?>()))
                .ReturnsAsync(new System.Drawing.Bitmap(100, 100));
            screenMock.Setup(x => x.GetScreenBoundsAsync())
                .ReturnsAsync(new Rectangle(0, 0, 1920, 1080));
            screenMock.Setup(x => x.GetPixelColorAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Color(255, 255, 255));
            
            return screenMock;
        }
    }

    /// <summary>
    /// 跨平台测试基类
    /// </summary>
    public abstract class CrossPlatformTestBase : TestBase
    {
        protected Platform CurrentPlatform { get; private set; }
        
        protected CrossPlatformTestBase(ITestOutputHelper output) : base(output)
        {
            CurrentPlatform = DetectCurrentPlatform();
            WriteTestInfo($"Running tests on {CurrentPlatform} platform");
        }
        
        /// <summary>
        /// 检测当前平台
        /// </summary>
        /// <returns>平台类型</returns>
        private Platform DetectCurrentPlatform()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return Platform.Windows;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                return Platform.MacOS;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                return Platform.Linux;
            else
                return Platform.Unknown;
        }
        
        /// <summary>
        /// 检查平台是否支持
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>是否支持</returns>
        protected bool IsPlatformSupported(Platform platform)
        {
            return platform != Platform.Unknown;
        }
        
        /// <summary>
        /// 跳过不支持的平台
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <param name="reason">跳过原因</param>
        protected void SkipIfPlatformNotSupported(Platform platform, string reason = "Platform not supported")
        {
            if (!IsPlatformSupported(platform))
            {
                throw new SkipTestException(reason);
            }
        }
        
        /// <summary>
        /// 仅在特定平台运行测试
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <param name="reason">跳过原因</param>
        protected void SkipIfNotPlatform(Platform platform, string reason = $"Test only runs on {platform}")
        {
            if (CurrentPlatform != platform)
            {
                throw new SkipTestException(reason);
            }
        }
    }
}