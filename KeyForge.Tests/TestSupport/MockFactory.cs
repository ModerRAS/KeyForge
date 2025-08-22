using System;
using Moq;
using KeyForge.HAL.Abstractions;

namespace KeyForge.Tests.TestSupport
{
    /// <summary>
    /// Mock工厂类
    /// 简化实现：基本的Mock对象创建
    /// 原本实现：包含复杂的Mock配置和验证
    /// </summary>
    public interface IMockFactory
    {
        /// <summary>
        /// 创建Mock对象
        /// </summary>
        /// <typeparam name="T">Mock类型</typeparam>
        /// <returns>Mock对象</returns>
        Mock<T> Create<T>() where T : class;
        
        /// <summary>
        /// 创建HAL Mock
        /// </summary>
        /// <returns>HAL Mock对象</returns>
        Mock<IHardwareAbstractionLayer> CreateHALMock();
        
        /// <summary>
        /// 创建键盘服务Mock
        /// </summary>
        /// <returns>键盘服务Mock对象</returns>
        Mock<IKeyboardService> CreateKeyboardServiceMock();
        
        /// <summary>
        /// 创建鼠标服务Mock
        /// </summary>
        /// <returns>鼠标服务Mock对象</returns>
        Mock<IMouseService> CreateMouseServiceMock();
        
        /// <summary>
        /// 创建屏幕服务Mock
        /// </summary>
        /// <returns>屏幕服务Mock对象</returns>
        Mock<IScreenService> CreateScreenServiceMock();
        
        /// <summary>
        /// 创建全局热键服务Mock
        /// </summary>
        /// <returns>全局热键服务Mock对象</returns>
        Mock<IGlobalHotkeyService> CreateGlobalHotkeyServiceMock();
        
        /// <summary>
        /// 创建窗口服务Mock
        /// </summary>
        /// <returns>窗口服务Mock对象</returns>
        Mock<IWindowService> CreateWindowServiceMock();
        
        /// <summary>
        /// 创建图像识别服务Mock
        /// </summary>
        /// <returns>图像识别服务Mock对象</returns>
        Mock<IImageRecognitionService> CreateImageRecognitionServiceMock();
    }

    /// <summary>
    /// Mock工厂实现
    /// </summary>
    public class MockFactory : IMockFactory
    {
        private readonly MockRepository _mockRepository;
        
        public MockFactory(MockRepository mockRepository)
        {
            _mockRepository = mockRepository;
        }
        
        /// <summary>
        /// 创建Mock对象
        /// </summary>
        /// <typeparam name="T">Mock类型</typeparam>
        /// <returns>Mock对象</returns>
        public Mock<T> Create<T>() where T : class
        {
            return _mockRepository.Create<T>();
        }
        
        /// <summary>
        /// 创建HAL Mock
        /// </summary>
        /// <returns>HAL Mock对象</returns>
        public Mock<IHardwareAbstractionLayer> CreateHALMock()
        {
            var halMock = _mockRepository.Create<IHardwareAbstractionLayer>();
            
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
            
            // 设置基本方法
            halMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            halMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            halMock.Setup(x => x.CheckPermissionsAsync()).ReturnsAsync(PermissionStatus.Granted);
            halMock.Setup(x => x.PerformHealthCheckAsync()).ReturnsAsync(new HealthCheckResult(true, "All systems operational"));
            halMock.Setup(x => x.GetPerformanceMetricsAsync()).ReturnsAsync(new PerformanceMetrics());
            
            return halMock;
        }
        
        /// <summary>
        /// 创建键盘服务Mock
        /// </summary>
        /// <returns>键盘服务Mock对象</returns>
        public Mock<IKeyboardService> CreateKeyboardServiceMock()
        {
            var keyboardMock = _mockRepository.Create<IKeyboardService>();
            
            // 设置基本属性
            keyboardMock.Setup(x => x.Capabilities).Returns(new KeyboardCapabilities());
            
            // 设置基本方法
            keyboardMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            keyboardMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            keyboardMock.Setup(x => x.PressKeyAsync(It.IsAny<KeyCode>(), It.IsAny<KeyModifiers>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.ReleaseKeyAsync(It.IsAny<KeyCode>(), It.IsAny<KeyModifiers>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.TypeTextAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            keyboardMock.Setup(x => x.GetKeyStateAsync(It.IsAny<KeyCode>()))
                .ReturnsAsync(KeyState.Up);
            keyboardMock.Setup(x => x.GetPressedKeysAsync()).ReturnsAsync(Array.Empty<KeyCode>());
            keyboardMock.Setup(x => x.PressKeysAsync(It.IsAny<System.Collections.Generic.IEnumerable<KeyCombination>>()))
                .ReturnsAsync(true);
            
            return keyboardMock;
        }
        
        /// <summary>
        /// 创建鼠标服务Mock
        /// </summary>
        /// <returns>鼠标服务Mock对象</returns>
        public Mock<IMouseService> CreateMouseServiceMock()
        {
            var mouseMock = _mockRepository.Create<IMouseService>();
            
            // 设置基本方法
            mouseMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            mouseMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            mouseMock.Setup(x => x.MoveToAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.MoveRelativeAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.ClickAsync(It.IsAny<MouseButton>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.PressButtonAsync(It.IsAny<MouseButton>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.ReleaseButtonAsync(It.IsAny<MouseButton>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.ScrollAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            mouseMock.Setup(x => x.DragDropAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<MouseButton>()))
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
        public Mock<IScreenService> CreateScreenServiceMock()
        {
            var screenMock = _mockRepository.Create<IScreenService>();
            
            // 设置基本方法
            screenMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            screenMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            screenMock.Setup(x => x.CaptureScreenAsync(It.IsAny<Rectangle?>()))
                .ReturnsAsync(new System.Drawing.Bitmap(100, 100));
            screenMock.Setup(x => x.GetScreensAsync())
                .ReturnsAsync(new[] { new ScreenInfo { IsPrimary = true } });
            screenMock.Setup(x => x.GetPrimaryScreenAsync())
                .ReturnsAsync(new ScreenInfo { IsPrimary = true });
            screenMock.Setup(x => x.GetScreenBoundsAsync())
                .ReturnsAsync(new Rectangle(0, 0, 1920, 1080));
            screenMock.Setup(x => x.GetPixelColorAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new Color(255, 255, 255));
            screenMock.Setup(x => x.FindColorAsync(It.IsAny<Color>(), It.IsAny<Rectangle>(), It.IsAny<int>()))
                .ReturnsAsync((Point?)null);
            screenMock.Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(new System.Drawing.Bitmap(100, 100));
            
            return screenMock;
        }
        
        /// <summary>
        /// 创建全局热键服务Mock
        /// </summary>
        /// <returns>全局热键服务Mock对象</returns>
        public Mock<IGlobalHotkeyService> CreateGlobalHotkeyServiceMock()
        {
            var hotkeyMock = _mockRepository.Create<IGlobalHotkeyService>();
            
            // 设置基本方法
            hotkeyMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            hotkeyMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            hotkeyMock.Setup(x => x.RegisterHotkeyAsync(It.IsAny<string>(), It.IsAny<HotkeyCombination>(), It.IsAny<object>()))
                .ReturnsAsync(true);
            hotkeyMock.Setup(x => x.UnregisterHotkeyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            hotkeyMock.Setup(x => x.GetRegisteredHotkeysAsync())
                .ReturnsAsync(Array.Empty<HotkeyInfo>());
            hotkeyMock.Setup(x => x.IsHotkeyRegisteredAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            return hotkeyMock;
        }
        
        /// <summary>
        /// 创建窗口服务Mock
        /// </summary>
        /// <returns>窗口服务Mock对象</returns>
        public Mock<IWindowService> CreateWindowServiceMock()
        {
            var windowMock = _mockRepository.Create<IWindowService>();
            
            // 设置基本方法
            windowMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            windowMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            windowMock.Setup(x => x.FindWindowAsync(It.IsAny<string>()))
                .ReturnsAsync(IntPtr.Zero);
            windowMock.Setup(x => x.FindWindowByClassAsync(It.IsAny<string>()))
                .ReturnsAsync(IntPtr.Zero);
            windowMock.Setup(x => x.GetForegroundWindowAsync())
                .ReturnsAsync(IntPtr.Zero);
            windowMock.Setup(x => x.ActivateWindowAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(true);
            windowMock.Setup(x => x.MinimizeWindowAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(true);
            windowMock.Setup(x => x.MaximizeWindowAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(true);
            windowMock.Setup(x => x.RestoreWindowAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(true);
            windowMock.Setup(x => x.GetWindowInfoAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(new WindowInfo());
            windowMock.Setup(x => x.GetWindowTitleAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(string.Empty);
            windowMock.Setup(x => x.GetWindowBoundsAsync(It.IsAny<IntPtr>()))
                .ReturnsAsync(new Rectangle(0, 0, 800, 600));
            
            return windowMock;
        }
        
        /// <summary>
        /// 创建图像识别服务Mock
        /// </summary>
        /// <returns>图像识别服务Mock对象</returns>
        public Mock<IImageRecognitionService> CreateImageRecognitionServiceMock()
        {
            var imageMock = _mockRepository.Create<IImageRecognitionService>();
            
            // 设置基本方法
            imageMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            imageMock.Setup(x => x.ShutdownAsync()).Returns(Task.CompletedTask);
            imageMock.Setup(x => x.FindImageAsync(It.IsAny<System.Drawing.Bitmap>(), It.IsAny<Rectangle?>(), It.IsAny<double>()))
                .ReturnsAsync(new RecognitionResult());
            imageMock.Setup(x => x.SaveTemplateAsync(It.IsAny<string>(), It.IsAny<System.Drawing.Bitmap>()))
                .ReturnsAsync(true);
            imageMock.Setup(x => x.LoadTemplateAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Drawing.Bitmap(50, 50));
            imageMock.Setup(x => x.DeleteTemplateAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            imageMock.Setup(x => x.GetTemplateNamesAsync())
                .ReturnsAsync(Array.Empty<string>());
            imageMock.Setup(x => x.FindColorInImageAsync(It.IsAny<System.Drawing.Bitmap>(), It.IsAny<Color>(), It.IsAny<int>()))
                .ReturnsAsync(Array.Empty<Point>());
            
            return imageMock;
        }
    }
}