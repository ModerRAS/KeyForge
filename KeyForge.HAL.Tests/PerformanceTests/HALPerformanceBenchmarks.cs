using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using KeyForge.HAL.Abstractions;
using Microsoft.Extensions.Logging;

namespace KeyForge.HAL.Tests;

/// <summary>
/// HAL性能基准测试
/// 简化实现：专注于性能基准测试
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[HtmlExporter]
[CsvExporter]
[RPlotExporter]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class HALPerformanceBenchmarks
{
    private IHardwareAbstractionLayer _hal = null!;
    private readonly ILogger<HALPerformanceBenchmarks> _logger;

    public HALPerformanceBenchmarks()
    {
        // 使用Serilog创建日志记录器
        _logger = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger());
        }).CreateLogger<HALPerformanceBenchmarks>();
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        // 创建HAL实例
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSerilog());
        services.AddSingleton<IHardwareAbstractionLayer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<HardwareAbstractionLayer>>();
            return new HardwareAbstractionLayer(logger, sp,
                CreateMockKeyboardService(),
                CreateMockMouseService(),
                CreateMockScreenService(),
                CreateMockGlobalHotkeyService(),
                CreateMockWindowService(),
                CreateMockImageRecognitionService(),
                CreateMockPerformanceMonitor());
        });

        var serviceProvider = services.BuildServiceProvider();
        _hal = serviceProvider.GetRequiredService<IHardwareAbstractionLayer>();
        
        // 初始化HAL
        await _hal.InitializeAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        if (_hal != null)
        {
            await _hal.ShutdownAsync();
            _hal.Dispose();
        }
    }

    #region 键盘性能测试

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_KeyPress_Single()
    {
        await _hal.Keyboard.KeyPressAsync(KeyCode.A);
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_KeyPress_Sequence()
    {
        var keys = new[] { KeyCode.H, KeyCode.E, KeyCode.L, KeyCode.L, KeyCode.O };
        foreach (var key in keys)
        {
            await _hal.Keyboard.KeyPressAsync(key);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_TypeText_Short()
    {
        await _hal.Keyboard.TypeTextAsync("Hello");
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_TypeText_Medium()
    {
        await _hal.Keyboard.TypeTextAsync("This is a medium length text for testing keyboard performance.");
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_TypeText_Long()
    {
        await _hal.Keyboard.TypeTextAsync("This is a very long text for testing keyboard performance with multiple sentences and paragraphs. " +
            "It contains various characters including numbers 12345, symbols !@#$%, and special characters. " +
            "The purpose is to test how the keyboard service handles large amounts of text input efficiently.");
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_Hotkey_Simple()
    {
        await _hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.S);
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_Hotkey_Complex()
    {
        await _hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control, KeyCode.Alt }, KeyCode.Delete);
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_GetKeyState()
    {
        _hal.Keyboard.GetKeyState(KeyCode.Space);
    }

    [Benchmark]
    [BenchmarkCategory("Keyboard")]
    public async Task Keyboard_IsKeyAvailable()
    {
        _hal.Keyboard.IsKeyAvailable(KeyCode.Enter);
    }

    #endregion

    #region 鼠标性能测试

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_MoveTo_Single()
    {
        await _hal.Mouse.MoveToAsync(100, 100);
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_MoveTo_Multiple()
    {
        for (int i = 0; i < 10; i++)
        {
            await _hal.Mouse.MoveToAsync(i * 50, i * 50);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_MoveBy_Single()
    {
        await _hal.Mouse.MoveByAsync(50, 50);
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_MoveBy_Multiple()
    {
        for (int i = 0; i < 10; i++)
        {
            await _hal.Mouse.MoveByAsync(10, 10);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_LeftClick()
    {
        await _hal.Mouse.LeftClickAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_RightClick()
    {
        await _hal.Mouse.RightClickAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_DoubleClick()
    {
        await _hal.Mouse.DoubleClickAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_Scroll()
    {
        await _hal.Mouse.ScrollAsync(5);
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_GetPosition()
    {
        _hal.Mouse.GetPosition();
    }

    [Benchmark]
    [BenchmarkCategory("Mouse")]
    public async Task Mouse_GetButtonState()
    {
        _hal.Mouse.GetButtonState(MouseButton.Left);
    }

    #endregion

    #region 屏幕性能测试

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_CaptureFullScreen()
    {
        await _hal.Screen.CaptureFullScreenAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_CapturePartial()
    {
        await _hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_CaptureMultiple()
    {
        for (int i = 0; i < 5; i++)
        {
            await _hal.Screen.CaptureScreenAsync(i * 50, i * 50, 100, 100);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_GetScreenResolution()
    {
        _hal.Screen.GetScreenResolution();
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_GetScreenCount()
    {
        _hal.Screen.GetScreenCount();
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_GetScreenBounds()
    {
        _hal.Screen.GetScreenBounds(0);
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_IsPointOnScreen()
    {
        _hal.Screen.IsPointOnScreen(100, 100);
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_GetColorDepth()
    {
        _hal.Screen.GetColorDepth();
    }

    [Benchmark]
    [BenchmarkCategory("Screen")]
    public async Task Screen_GetRefreshRate()
    {
        _hal.Screen.GetRefreshRate();
    }

    #endregion

    #region 窗口性能测试

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_GetForegroundWindow()
    {
        await _hal.Window.GetForegroundWindowAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_GetWindowTitle()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        await _hal.Window.GetWindowTitleAsync(window);
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_GetWindowRect()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        await _hal.Window.GetWindowRectAsync(window);
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_SetForegroundWindow()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        await _hal.Window.SetForegroundWindowAsync(window);
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_EnumWindows()
    {
        await _hal.Window.EnumWindowsAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_FindWindowByTitle()
    {
        await _hal.Window.FindWindowByTitleAsync("Test", true);
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_IsWindowVisible()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        await _hal.Window.IsWindowVisibleAsync(window);
    }

    [Benchmark]
    [BenchmarkCategory("Window")]
    public async Task Window_IsWindow()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        await _hal.Window.IsWindowAsync(window);
    }

    #endregion

    #region 图像识别性能测试

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_FindImage()
    {
        await _hal.ImageRecognition.FindImageAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
    }

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_FindAllImages()
    {
        await _hal.ImageRecognition.FindAllImagesAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
    }

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_WaitForImage()
    {
        await _hal.ImageRecognition.WaitForImageAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, 100);
    }

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_CalculateSimilarity()
    {
        var image = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        await _hal.ImageRecognition.CalculateSimilarityAsync(image, image);
    }

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_CaptureRegion()
    {
        await _hal.ImageRecognition.CaptureRegionAsync(0, 0, 100, 100);
    }

    [Benchmark]
    [BenchmarkCategory("ImageRecognition")]
    public async Task ImageRecognition_GetImageInfo()
    {
        await _hal.ImageRecognition.GetImageInfoAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
    }

    #endregion

    #region 全局热键性能测试

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_Register()
    {
        await _hal.GlobalHotkeys.RegisterHotkeyAsync(1, new[] { KeyCode.Control }, KeyCode.S, _ => { });
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_Unregister()
    {
        await _hal.GlobalHotkeys.UnregisterHotkeyAsync(1);
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_IsRegistered()
    {
        _hal.GlobalHotkeys.IsHotkeyRegistered(1);
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_GetRegisteredHotkeys()
    {
        _hal.GlobalHotkeys.GetRegisteredHotkeys();
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_IsAvailable()
    {
        _hal.GlobalHotkeys.IsHotkeyAvailable(new[] { KeyCode.Control }, KeyCode.S);
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_SuspendAll()
    {
        await _hal.GlobalHotkeys.SuspendAllHotkeysAsync();
    }

    [Benchmark]
    [BenchmarkCategory("GlobalHotkey")]
    public async Task GlobalHotkey_ResumeAll()
    {
        await _hal.GlobalHotkeys.ResumeAllHotkeysAsync();
    }

    #endregion

    #region 性能监控测试

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_GetCurrentMetrics()
    {
        _hal.PerformanceMonitor.GetCurrentMetrics();
    }

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_GetHistoricalMetrics()
    {
        _hal.PerformanceMonitor.GetHistoricalMetrics(DateTime.Now.AddMinutes(-1), DateTime.Now);
    }

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_RunBenchmark()
    {
        var request = new BenchmarkRequest
        {
            TestName = "PerformanceTest",
            Iterations = 10,
            TestFunction = async () => await Task.Delay(1)
        };
        await _hal.PerformanceMonitor.RunBenchmarkAsync(request);
    }

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_GenerateReport()
    {
        var range = new DateTimeRange(DateTime.Now.AddMinutes(-1), DateTime.Now);
        await _hal.PerformanceMonitor.GenerateReportAsync(range);
    }

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_StartStopMonitoring()
    {
        await _hal.PerformanceMonitor.StartMonitoringAsync(100);
        await _hal.PerformanceMonitor.StopMonitoringAsync();
    }

    [Benchmark]
    [BenchmarkCategory("PerformanceMonitor")]
    public async Task PerformanceMonitor_CleanupHistoricalData()
    {
        _hal.PerformanceMonitor.CleanupHistoricalData(DateTime.Now.AddDays(-1));
    }

    #endregion

    #region HAL核心性能测试

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_GetPerformanceMetrics()
    {
        await _hal.GetPerformanceMetricsAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_PerformHealthCheck()
    {
        await _hal.PerformHealthCheckAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_ExecuteQualityGate()
    {
        await _hal.ExecuteQualityGateAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_GenerateDiagnosticsReport()
    {
        await _hal.GenerateDiagnosticsReportAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_GetSystemInfo()
    {
        await _hal.GetSystemInfoAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_CheckPermissions()
    {
        await _hal.CheckPermissionsAsync();
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_RequestPermissions()
    {
        var request = new PermissionRequest
        {
            PermissionType = "TestPermission",
            Description = "Test permission",
            IsRequired = false
        };
        await _hal.RequestPermissionsAsync(request);
    }

    [Benchmark]
    [BenchmarkCategory("HAL")]
    public async Task HAL_Reconfigure()
    {
        var config = new HALConfiguration();
        await _hal.ReconfigureAsync(config);
    }

    #endregion

    #region 综合性能测试

    [Benchmark]
    [BenchmarkCategory("Integration")]
    public async Task Integration_ComplexWorkflow()
    {
        // 复杂工作流程：键盘输入 + 鼠标操作 + 屏幕捕获
        await _hal.Keyboard.TypeTextAsync("Hello World");
        await _hal.Mouse.MoveToAsync(100, 100);
        await _hal.Mouse.LeftClickAsync();
        await _hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
    }

    [Benchmark]
    [BenchmarkCategory("Integration")]
    public async Task Integration_ConcurrentOperations()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_hal.Keyboard.KeyPressAsync(KeyCode.A));
            tasks.Add(_hal.Mouse.MoveToAsync(i, i));
            tasks.Add(_hal.Screen.CaptureScreenAsync(i, i, 10, 10));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [BenchmarkCategory("Integration")]
    public async Task Integration_PerformanceMonitoring()
    {
        await _hal.PerformanceMonitor.StartMonitoringAsync(10);
        
        for (int i = 0; i < 100; i++)
        {
            await _hal.Keyboard.KeyPressAsync(KeyCode.A);
            await _hal.Mouse.MoveToAsync(i % 100, i % 100);
        }
        
        await _hal.PerformanceMonitor.StopMonitoringAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Integration")]
    public async Task Integration_ImageProcessing()
    {
        var screen = await _hal.Screen.CaptureFullScreenAsync();
        var info = await _hal.ImageRecognition.GetImageInfoAsync(screen);
        var similarity = await _hal.ImageRecognition.CalculateSimilarityAsync(screen, screen);
    }

    [Benchmark]
    [BenchmarkCategory("Integration")]
    public async Task Integration_WindowManagement()
    {
        var window = await _hal.Window.GetForegroundWindowAsync();
        var rect = await _hal.Window.GetWindowRectAsync(window);
        var title = await _hal.Window.GetWindowTitleAsync(window);
        var isVisible = await _hal.Window.IsWindowVisibleAsync(window);
    }

    #endregion

    #region 内存和CPU密集型测试

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public async Task Memory_MultipleScreenCaptures()
    {
        var captures = new List<byte[]>();
        for (int i = 0; i < 10; i++)
        {
            var capture = await _hal.Screen.CaptureFullScreenAsync();
            captures.Add(capture);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public async Task Memory_ImageProcessing()
    {
        var largeImage = new byte[1024 * 1024]; // 1MB
        for (int i = 0; i < 5; i++)
        {
            await _hal.ImageRecognition.GetImageInfoAsync(largeImage);
            await _hal.ImageRecognition.CalculateSimilarityAsync(largeImage, largeImage);
        }
    }

    [Benchmark]
    [BenchmarkCategory("CPU")]
    public async Task CPU_ImageSearch()
    {
        var screen = await _hal.Screen.CaptureFullScreenAsync();
        for (int i = 0; i < 10; i++)
        {
            await _hal.ImageRecognition.FindImageAsync(screen, 0.8);
        }
    }

    [Benchmark]
    [BenchmarkCategory("CPU")]
    public async Task CPU_WindowEnumeration()
    {
        for (int i = 0; i < 10; i++)
        {
            await _hal.Window.EnumWindowsAsync();
        }
    }

    #endregion

    #region 边界条件测试

    [Benchmark]
    [BenchmarkCategory("Boundary")]
    public async Task Boundary_LargeTextInput()
    {
        var largeText = new string('A', 10000);
        await _hal.Keyboard.TypeTextAsync(largeText);
    }

    [Benchmark]
    [BenchmarkCategory("Boundary")]
    public async Task Boundary_ExtremeCoordinates()
    {
        await _hal.Mouse.MoveToAsync(int.MaxValue, int.MaxValue);
        await _hal.Mouse.MoveToAsync(int.MinValue, int.MinValue);
    }

    [Benchmark]
    [BenchmarkCategory("Boundary")]
    public async Task Boundary_MultipleHotkeys()
    {
        for (int i = 0; i < 100; i++)
        {
            await _hal.GlobalHotkeys.RegisterHotkeyAsync(i, new[] { KeyCode.Control }, (KeyCode)(65 + i % 26), _ => { });
            await _hal.GlobalHotkeys.UnregisterHotkeyAsync(i);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Boundary")]
    public async Task Boundary_RapidOperations()
    {
        for (int i = 0; i < 1000; i++)
        {
            await _hal.Keyboard.KeyPressAsync(KeyCode.A);
            await _hal.Mouse.MoveToAsync(i % 100, i % 100);
        }
    }

    #endregion

    #region 辅助方法

    private IKeyboardService CreateMockKeyboardService()
    {
        var mock = new Mock<IKeyboardService>();
        mock.Setup(x => x.KeyPressAsync(It.IsAny<KeyCode>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.KeyDownAsync(It.IsAny<KeyCode>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.KeyUpAsync(It.IsAny<KeyCode>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.TypeTextAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.SendHotkeyAsync(It.IsAny<KeyCode[]>(), It.IsAny<KeyCode>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.GetKeyState(It.IsAny<KeyCode>()))
            .Returns(KeyState.Up);
        mock.Setup(x => x.IsKeyAvailable(It.IsAny<KeyCode>()))
            .Returns(true);
        return mock.Object;
    }

    private IMouseService CreateMockMouseService()
    {
        var mock = new Mock<IMouseService>();
        mock.Setup(x => x.MoveToAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.MoveByAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.LeftClickAsync())
            .ReturnsAsync(true);
        mock.Setup(x => x.RightClickAsync())
            .ReturnsAsync(true);
        mock.Setup(x => x.DoubleClickAsync())
            .ReturnsAsync(true);
        mock.Setup(x => x.ScrollAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.GetPosition())
            .Returns(new Point(0, 0));
        mock.Setup(x => x.GetButtonState(It.IsAny<MouseButton>()))
            .Returns(MouseButtonState.Up);
        return mock.Object;
    }

    private IScreenService CreateMockScreenService()
    {
        var mock = new Mock<IScreenService>();
        mock.Setup(x => x.CaptureScreenAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<byte>());
        mock.Setup(x => x.CaptureFullScreenAsync())
            .ReturnsAsync(Array.Empty<byte>());
        mock.Setup(x => x.GetScreenResolution())
            .Returns(new Size(1920, 1080));
        mock.Setup(x => x.GetScreenCount())
            .Returns(1);
        mock.Setup(x => x.GetScreenBounds(It.IsAny<int>()))
            .Returns(new Rectangle(0, 0, 1920, 1080));
        mock.Setup(x => x.GetPrimaryScreenIndex())
            .Returns(0);
        mock.Setup(x => x.IsPointOnScreen(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);
        mock.Setup(x => x.GetColorDepth())
            .Returns(32);
        mock.Setup(x => x.GetRefreshRate())
            .Returns(60);
        return mock.Object;
    }

    private IGlobalHotkeyService CreateMockGlobalHotkeyService()
    {
        var mock = new Mock<IGlobalHotkeyService>();
        mock.Setup(x => x.RegisterHotkeyAsync(It.IsAny<int>(), It.IsAny<KeyCode[]>(), It.IsAny<KeyCode>(), It.IsAny<Action<HotkeyEventArgs>>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.UnregisterHotkeyAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.IsHotkeyRegistered(It.IsAny<int>()))
            .Returns(false);
        mock.Setup(x => x.GetRegisteredHotkeys())
            .Returns(Enumerable.Empty<HotkeyInfo>());
        mock.Setup(x => x.IsHotkeyAvailable(It.IsAny<KeyCode[]>(), It.IsAny<KeyCode>()))
            .Returns(true);
        mock.Setup(x => x.SuspendAllHotkeysAsync())
            .ReturnsAsync(true);
        mock.Setup(x => x.ResumeAllHotkeysAsync())
            .ReturnsAsync(true);
        return mock.Object;
    }

    private IWindowService CreateMockWindowService()
    {
        var mock = new Mock<IWindowService>();
        mock.Setup(x => x.GetForegroundWindowAsync())
            .ReturnsAsync(IntPtr.Zero);
        mock.Setup(x => x.SetForegroundWindowAsync(It.IsAny<IntPtr>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.GetWindowTitleAsync(It.IsAny<IntPtr>()))
            .ReturnsAsync("Test Window");
        mock.Setup(x => x.GetWindowRectAsync(It.IsAny<IntPtr>()))
            .ReturnsAsync(new Rectangle(0, 0, 800, 600));
        mock.Setup(x => x.EnumWindowsAsync())
            .ReturnsAsync(Enumerable.Empty<WindowInfo>());
        mock.Setup(x => x.FindWindowByTitleAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(IntPtr.Zero);
        mock.Setup(x => x.IsWindowVisibleAsync(It.IsAny<IntPtr>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.IsWindowAsync(It.IsAny<IntPtr>()))
            .ReturnsAsync(true);
        return mock.Object;
    }

    private IImageRecognitionService CreateMockImageRecognitionService()
    {
        var mock = new Mock<IImageRecognitionService>();
        mock.Setup(x => x.FindImageAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<Rectangle?>()))
            .ReturnsAsync((ImageMatchResult?)null);
        mock.Setup(x => x.FindAllImagesAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<Rectangle?>()))
            .ReturnsAsync(Enumerable.Empty<ImageMatchResult>());
        mock.Setup(x => x.WaitForImageAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync((ImageMatchResult?)null);
        mock.Setup(x => x.CalculateSimilarityAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .ReturnsAsync(0.0);
        mock.Setup(x => x.CaptureRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<byte>());
        mock.Setup(x => x.GetImageInfoAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(new ImageInfo());
        return mock.Object;
    }

    private IPerformanceMonitor CreateMockPerformanceMonitor()
    {
        var mock = new Mock<IPerformanceMonitor>();
        mock.Setup(x => x.GetCurrentMetrics())
            .Returns(new PerformanceMetrics());
        mock.Setup(x => x.GetHistoricalMetrics(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(Enumerable.Empty<PerformanceMetrics>());
        mock.Setup(x => x.RunBenchmarkAsync(It.IsAny<BenchmarkRequest>()))
            .ReturnsAsync(new BenchmarkResult { IsSuccess = true });
        mock.Setup(x => x.GenerateReportAsync(It.IsAny<DateTimeRange>()))
            .ReturnsAsync(new PerformanceReport());
        mock.Setup(x => x.StartMonitoringAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.StopMonitoringAsync())
            .ReturnsAsync(true);
        mock.Setup(x => x.CleanupHistoricalData(It.IsAny<DateTime>()))
            .Returns(0);
        return mock.Object;
    }

    #endregion
}

/// <summary>
/// 性能测试运行器
/// </summary>
public static class PerformanceTestRunner
{
    public static void Run()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.JoinSummary)
            .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(50));

        BenchmarkRunner.Run<HALPerformanceBenchmarks>(config);
    }
}