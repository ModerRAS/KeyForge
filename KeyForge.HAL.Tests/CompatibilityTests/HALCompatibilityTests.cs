using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Xunit.Abstractions;

namespace KeyForge.HAL.Tests;

/// <summary>
/// HAL兼容性测试套件
/// 简化实现：专注于三个平台的兼容性测试
/// </summary>
public class HALCompatibilityTests : CrossPlatformTestBase
{
    private readonly ITestOutputHelper _output;

    public HALCompatibilityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task PlatformDetection_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var platformInfo = PlatformDetector.DetectPlatform();

        // Assert
        platformInfo.Platform.Should().BeOneOf(Platform.Windows, Platform.Linux, Platform.MacOS);
        platformInfo.Version.Should().NotBeEmpty();
        platformInfo.Architecture.Should().NotBeEmpty();
        platformInfo.Is64Bit.Should().BeTrue();

        _output.WriteLine($"Detected platform: {platformInfo.Platform}");
        _output.WriteLine($"Version: {platformInfo.Version}");
        _output.WriteLine($"Architecture: {platformInfo.Architecture}");
        _output.WriteLine($"Is 64-bit: {platformInfo.Is64Bit}");
    }

    [Fact]
    public async Task PlatformSupport_ShouldBeCorrectlyReported()
    {
        // Arrange & Act
        var windowsSupported = PlatformDetector.IsPlatformSupported(Platform.Windows);
        var linuxSupported = PlatformDetector.IsPlatformSupported(Platform.Linux);
        var macOSupported = PlatformDetector.IsPlatformSupported(Platform.MacOS);

        // Assert
        // All platforms should be supported in .NET 9.0
        windowsSupported.Should().BeTrue();
        linuxSupported.Should().BeTrue();
        macOSupported.Should().BeTrue();

        _output.WriteLine($"Windows supported: {windowsSupported}");
        _output.WriteLine($"Linux supported: {linuxSupported}");
        _output.WriteLine($"macOS supported: {macOSupported}");
    }

    [Fact]
    public async Task HALInitialization_ShouldWorkOnCurrentPlatform()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        await hal.InitializeAsync();

        // Assert
        hal.IsInitialized.Should().BeTrue();
        hal.Status.Should().Be(HALStatus.Ready);
        hal.PlatformInfo.Platform.Should().Be(CurrentPlatform);

        _output.WriteLine($"HAL initialized successfully on {CurrentPlatform}");
    }

    [Fact]
    public async Task KeyboardCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var testKeys = new[]
        {
            // Basic keys
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.Z,
            KeyCode.D0, KeyCode.D1, KeyCode.D9,
            KeyCode.Space, KeyCode.Enter, KeyCode.Escape, KeyCode.Tab,
            
            // Modifier keys
            KeyCode.Shift, KeyCode.Control, KeyCode.Alt, KeyCode.Windows,
            
            // Function keys
            KeyCode.F1, KeyCode.F5, KeyCode.F12,
            
            // Navigation keys
            KeyCode.Left, KeyCode.Right, KeyCode.Up, KeyCode.Down,
            KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown,
            
            // Special keys
            KeyCode.Insert, KeyCode.Delete, KeyCode.Backspace,
            
            // Numpad keys
            KeyCode.NumPad0, KeyCode.NumPad5, KeyCode.NumPad9,
            KeyCode.NumPadAdd, KeyCode.NumPadSubtract, KeyCode.NumPadMultiply, KeyCode.NumPadDivide
        };

        foreach (var key in testKeys)
        {
            var pressResult = await hal.Keyboard.KeyPressAsync(key);
            pressResult.Should().BeTrue($"Key press failed for {key} on {CurrentPlatform}");

            var keyState = hal.Keyboard.GetKeyState(key);
            keyState.Should().BeOneOf(KeyState.Up, KeyState.Down, KeyState.Unknown);

            var isAvailable = hal.Keyboard.IsKeyAvailable(key);
            isAvailable.Should().BeTrue($"Key {key} not available on {CurrentPlatform}");
        }

        _output.WriteLine($"All keyboard keys work correctly on {CurrentPlatform}");
    }

    [Fact]
    public async Task MouseCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test movement
        var moveResult = await hal.Mouse.MoveToAsync(100, 100);
        moveResult.Should().BeTrue($"Mouse move failed on {CurrentPlatform}");

        var relativeMoveResult = await hal.Mouse.MoveByAsync(50, 50);
        relativeMoveResult.Should().BeTrue($"Mouse relative move failed on {CurrentPlatform}");

        // Test clicks
        var leftClickResult = await hal.Mouse.LeftClickAsync();
        leftClickResult.Should().BeTrue($"Left click failed on {CurrentPlatform}");

        var rightClickResult = await hal.Mouse.RightClickAsync();
        rightClickResult.Should().BeTrue($"Right click failed on {CurrentPlatform}");

        var middleClickResult = await hal.Mouse.MiddleClickAsync();
        middleClickResult.Should().BeTrue($"Middle click failed on {CurrentPlatform}");

        var doubleClickResult = await hal.Mouse.DoubleClickAsync();
        doubleClickResult.Should().BeTrue($"Double click failed on {CurrentPlatform}");

        // Test scroll
        var scrollResult = await hal.Mouse.ScrollAsync(5);
        scrollResult.Should().BeTrue($"Mouse scroll failed on {CurrentPlatform}");

        // Test position and state
        var position = hal.Mouse.GetPosition();
        position.Should().NotBeNull($"Mouse position failed on {CurrentPlatform}");

        var leftButtonState = hal.Mouse.GetButtonState(MouseButton.Left);
        leftButtonState.Should().BeOneOf(MouseButtonState.Up, MouseButtonState.Down);

        var rightButtonState = hal.Mouse.GetButtonState(MouseButton.Right);
        rightButtonState.Should().BeOneOf(MouseButtonState.Up, MouseButtonState.Down);

        var middleButtonState = hal.Mouse.GetButtonState(MouseButton.Middle);
        middleButtonState.Should().BeOneOf(MouseButtonState.Up, MouseButtonState.Down);

        _output.WriteLine($"All mouse operations work correctly on {CurrentPlatform}");
    }

    [Fact]
    public async Task ScreenCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test screen capture
        var fullScreen = await hal.Screen.CaptureFullScreenAsync();
        fullScreen.Should().NotBeNull($"Full screen capture failed on {CurrentPlatform}");

        var partialScreen = await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
        partialScreen.Should().NotBeNull($"Partial screen capture failed on {CurrentPlatform}");

        // Test screen info
        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull();
        resolution.Width.Should().BeGreaterThan(0);
        resolution.Height.Should().BeGreaterThan(0);

        var screenCount = hal.Screen.GetScreenCount();
        screenCount.Should().BeGreaterThan(0);

        var screenBounds = hal.Screen.GetScreenBounds(0);
        screenBounds.Should().NotBeNull();
        screenBounds.Width.Should().BeGreaterThan(0);
        screenBounds.Height.Should().BeGreaterThan(0);

        var primaryScreen = hal.Screen.GetPrimaryScreenIndex();
        primaryScreen.Should().BeGreaterThanOrEqualTo(0);
        primaryScreen.Should().BeLessThan(screenCount);

        // Test point checking
        var isPointOnScreen = hal.Screen.IsPointOnScreen(100, 100);
        isPointOnScreen.Should().BeTrue();

        var isPointOffScreen = hal.Screen.IsPointOnScreen(-100, -100);
        isPointOffScreen.Should().BeFalse();

        // Test screen properties
        var colorDepth = hal.Screen.GetColorDepth();
        colorDepth.Should().BeGreaterThan(0);

        var refreshRate = hal.Screen.GetRefreshRate();
        refreshRate.Should().BeGreaterThan(0);

        _output.WriteLine($"Screen resolution: {resolution.Width}x{resolution.Height} on {CurrentPlatform}");
        _output.WriteLine($"Screen count: {screenCount} on {CurrentPlatform}");
        _output.WriteLine($"Color depth: {colorDepth} bits on {CurrentPlatform}");
        _output.WriteLine($"Refresh rate: {refreshRate} Hz on {CurrentPlatform}");
    }

    [Fact]
    public async Task WindowCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test window operations
        var foregroundWindow = await hal.Window.GetForegroundWindowAsync();
        foregroundWindow.Should().NotBeNull();

        var windowTitle = await hal.Window.GetWindowTitleAsync(foregroundWindow);
        windowTitle.Should().NotBeNull();

        var windowRect = await hal.Window.GetWindowRectAsync(foregroundWindow);
        windowRect.Should().NotBeNull();
        windowRect.Width.Should().BeGreaterThanOrEqualTo(0);
        windowRect.Height.Should().BeGreaterThanOrEqualTo(0);

        // Test window properties
        var isWindowVisible = await hal.Window.IsWindowVisibleAsync(foregroundWindow);
        isWindowVisible.Should().BeOneOf(true, false);

        var isWindow = await hal.Window.IsWindowAsync(foregroundWindow);
        isWindow.Should().BeTrue();

        // Test window enumeration
        var windows = await hal.Window.EnumWindowsAsync();
        windows.Should().NotBeNull();
        windows.Should().NotBeEmpty();

        // Test window finding
        var foundWindow = await hal.Window.FindWindowByTitleAsync(windowTitle, true);
        foundWindow.Should().NotBeNull();

        _output.WriteLine($"Found {windows.Count()} windows on {CurrentPlatform}");
        _output.WriteLine($"Foreground window: {windowTitle} on {CurrentPlatform}");
    }

    [Fact]
    public async Task GlobalHotkeyCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var hotkeyTriggered = false;

        // Act & Assert
        // Test hotkey registration
        var registerResult = await hal.GlobalHotkeys.RegisterHotkeyAsync(
            1, new[] { KeyCode.Control }, KeyCode.S, _ => hotkeyTriggered = true);
        registerResult.Should().BeTrue($"Hotkey registration failed on {CurrentPlatform}");

        // Test hotkey status
        var isRegistered = hal.GlobalHotkeys.IsHotkeyRegistered(1);
        isRegistered.Should().BeTrue($"Hotkey status check failed on {CurrentPlatform}");

        // Test hotkey availability
        var isAvailable = hal.GlobalHotkeys.IsHotkeyAvailable(new[] { KeyCode.Control }, KeyCode.S);
        isAvailable.Should().BeFalse($"Hotkey availability check failed on {CurrentPlatform}");

        // Test hotkey listing
        var registeredHotkeys = hal.GlobalHotkeys.GetRegisteredHotkeys();
        registeredHotkeys.Should().NotBeNull();

        // Test hotkey unregistration
        var unregisterResult = await hal.GlobalHotkeys.UnregisterHotkeyAsync(1);
        unregisterResult.Should().BeTrue($"Hotkey unregistration failed on {CurrentPlatform}");

        // Test hotkey suspension/resumption
        var suspendResult = await hal.GlobalHotkeys.SuspendAllHotkeysAsync();
        suspendResult.Should().BeTrue($"Hotkey suspension failed on {CurrentPlatform}");

        var resumeResult = await hal.GlobalHotkeys.ResumeAllHotkeysAsync();
        resumeResult.Should().BeTrue($"Hotkey resumption failed on {CurrentPlatform}");

        _output.WriteLine($"Global hotkey operations work correctly on {CurrentPlatform}");
    }

    [Fact]
    public async Task ImageRecognitionCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header

        // Act & Assert
        // Test image search
        var findImageResult = await hal.ImageRecognition.FindImageAsync(testImage);
        findImageResult.Should().BeNull($"Image search failed on {CurrentPlatform}");

        var findAllImagesResult = await hal.ImageRecognition.FindAllImagesAsync(testImage);
        findAllImagesResult.Should().NotBeNull();
        findAllImagesResult.Should().BeEmpty();

        // Test image waiting
        var waitForImageResult = await hal.ImageRecognition.WaitForImageAsync(testImage, 100);
        waitForImageResult.Should().BeNull($"Image wait failed on {CurrentPlatform}");

        var waitForDisappearResult = await hal.ImageRecognition.WaitForImageDisappearAsync(testImage, 100);
        waitForDisappearResult.Should().BeTrue($"Image disappear wait failed on {CurrentPlatform}");

        // Test similarity calculation
        var similarityResult = await hal.ImageRecognition.CalculateSimilarityAsync(testImage, testImage);
        similarityResult.Should().BeGreaterThanOrEqualTo(0.0);
        similarityResult.Should().BeLessThanOrEqualTo(1.0);

        // Test region capture
        var captureRegionResult = await hal.ImageRecognition.CaptureRegionAsync(0, 0, 100, 100);
        captureRegionResult.Should().NotBeNull($"Region capture failed on {CurrentPlatform}");

        // Test image info
        var imageInfo = await hal.ImageRecognition.GetImageInfoAsync(testImage);
        imageInfo.Should().NotBeNull($"Image info failed on {CurrentPlatform}");

        _output.WriteLine($"Image recognition operations work correctly on {CurrentPlatform}");
    }

    [Fact]
    public async Task PerformanceMonitorCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test monitoring
        var startResult = await hal.PerformanceMonitor.StartMonitoringAsync(1000);
        startResult.Should().BeTrue($"Performance monitoring start failed on {CurrentPlatform}");

        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        metrics.Should().NotBeNull($"Current metrics failed on {CurrentPlatform}");
        metrics.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        metrics.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.MemoryUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.DiskUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.NetworkUsage.Should().BeGreaterThanOrEqualTo(0);

        // Test historical data
        var historicalMetrics = hal.PerformanceMonitor.GetHistoricalMetrics(
            DateTime.Now.AddMinutes(-1), DateTime.Now);
        historicalMetrics.Should().NotBeNull($"Historical metrics failed on {CurrentPlatform}");

        // Test benchmarking
        var benchmarkRequest = new BenchmarkRequest
        {
            TestName = "CompatibilityTest",
            Iterations = 10,
            TestFunction = async () => await Task.Delay(10)
        };

        var benchmarkResult = await hal.PerformanceMonitor.RunBenchmarkAsync(benchmarkRequest);
        benchmarkResult.Should().NotBeNull($"Benchmark failed on {CurrentPlatform}");
        benchmarkResult.IsSuccess.Should().BeTrue();
        benchmarkResult.TestName.Should().Be("CompatibilityTest");
        benchmarkResult.Iterations.Should().Be(10);
        benchmarkResult.AverageTime.Should().BeGreaterThan(0);

        // Test report generation
        var report = await hal.PerformanceMonitor.GenerateReportAsync(
            new DateTimeRange(DateTime.Now.AddMinutes(-1), DateTime.Now));
        report.Should().NotBeNull($"Report generation failed on {CurrentPlatform}");

        // Test monitoring stop
        var stopResult = await hal.PerformanceMonitor.StopMonitoringAsync();
        stopResult.Should().BeTrue($"Performance monitoring stop failed on {CurrentPlatform}");

        // Test cleanup
        var cleanupCount = hal.PerformanceMonitor.CleanupHistoricalData(DateTime.Now.AddDays(-1));
        cleanupCount.Should().BeGreaterThanOrEqualTo(0);

        _output.WriteLine($"Performance monitoring operations work correctly on {CurrentPlatform}");
        _output.WriteLine($"Average benchmark time: {benchmarkResult.AverageTime}ms on {CurrentPlatform}");
    }

    [Fact]
    public async Task QualityGateCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var result = await hal.QualityGate.ExecuteQualityGateAsync();
        result.Should().NotBeNull($"Quality gate failed on {CurrentPlatform}");
        result.IsPassed.Should().BeTrue();
        result.OverallScore.Should().BeGreaterThan(0);
        result.Issues.Should().NotBeNull();
        result.CheckTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));

        _output.WriteLine($"Quality gate works correctly on {CurrentPlatform}");
        _output.WriteLine($"Quality score: {result.OverallScore} on {CurrentPlatform}");
        _output.WriteLine($"Issues found: {result.Issues.Count} on {CurrentPlatform}");
    }

    [Fact]
    public async Task DiagnosticsCompatibility_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var report = await hal.Diagnostics.GenerateDiagnosticsReportAsync();
        report.Should().NotBeNull($"Diagnostics failed on {CurrentPlatform}");
        report.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        report.SystemInfo.Should().NotBeNull();
        report.MemoryDiagnostics.Should().NotBeNull();
        report.ThreadDiagnostics.Should().NotBeNull();
        report.PerformanceDiagnostics.Should().NotBeNull();
        report.ErrorDiagnostics.Should().NotBeNull();
        report.Recommendations.Should().NotBeNull();

        _output.WriteLine($"Diagnostics work correctly on {CurrentPlatform}");
        _output.WriteLine($"Memory usage: {report.MemoryDiagnostics.MemoryUsagePercentage}% on {CurrentPlatform}");
        _output.WriteLine($"Thread count: {report.ThreadDiagnostics.ThreadPoolThreads} on {CurrentPlatform}");
        _output.WriteLine($"CPU usage: {report.PerformanceDiagnostics.CpuUsage}% on {CurrentPlatform}");
    }

    [Fact]
    public async Task PlatformSpecificFeatures_ShouldBeAvailable()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var systemInfo = await hal.GetSystemInfoAsync();
        systemInfo.Should().NotBeNull();

        _output.WriteLine($"Platform: {CurrentPlatform}");
        _output.WriteLine($"OS: {systemInfo.OperatingSystem}");
        _output.WriteLine($"Processor: {systemInfo.Processor}");
        _output.WriteLine($"Memory: {systemInfo.Memory}");
        _output.WriteLine($".NET: {systemInfo.DotNetVersion}");

        // Platform-specific compatibility tests
        switch (CurrentPlatform)
        {
            case Platform.Windows:
                await TestWindowsCompatibility(hal);
                break;
            case Platform.Linux:
                await TestLinuxCompatibility(hal);
                break;
            case Platform.MacOS:
                await TestMacOSCompatibility(hal);
                break;
            default:
                _output.WriteLine($"Unknown platform: {CurrentPlatform}");
                break;
        }
    }

    private async Task TestWindowsCompatibility(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing Windows-specific compatibility");

        // Windows-specific compatibility tests
        var windows = await hal.Window.EnumWindowsAsync();
        windows.Should().NotBeEmpty("Windows enumeration should work on Windows");

        var screenCount = hal.Screen.GetScreenCount();
        screenCount.Should().BeGreaterThan(0, "Screen detection should work on Windows");

        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull("Screen resolution should be available on Windows");

        // Test Windows-specific keyboard behavior
        var hotkeyResult = await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Windows }, KeyCode.D);
        hotkeyResult.Should().BeTrue("Windows key should work on Windows");

        _output.WriteLine("Windows compatibility tests passed");
    }

    private async Task TestLinuxCompatibility(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing Linux-specific compatibility");

        // Linux-specific compatibility tests
        var screenCount = hal.Screen.GetScreenCount();
        screenCount.Should().BeGreaterThan(0, "Screen detection should work on Linux");

        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull("Screen resolution should be available on Linux");

        // Test Linux-specific keyboard behavior
        var hotkeyResult = await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.Alt, KeyCode.T);
        hotkeyResult.Should().BeTrue("Ctrl+Alt+T should work on Linux");

        _output.WriteLine("Linux compatibility tests passed");
    }

    private async Task TestMacOSCompatibility(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing macOS-specific compatibility");

        // macOS-specific compatibility tests
        var screenCount = hal.Screen.GetScreenCount();
        screenCount.Should().BeGreaterThan(0, "Screen detection should work on macOS");

        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull("Screen resolution should be available on macOS");

        // Test macOS-specific keyboard behavior
        var hotkeyResult = await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.Space);
        hotkeyResult.Should().BeTrue("Ctrl+Space should work on macOS");

        _output.WriteLine("macOS compatibility tests passed");
    }

    [Fact]
    public async Task CrossPlatformPerformance_ShouldBeConsistent()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Perform cross-platform operations
        await hal.Keyboard.TypeTextAsync("Cross-platform test");
        await hal.Mouse.MoveToAsync(100, 100);
        await hal.Mouse.LeftClickAsync();
        await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
        await hal.Window.GetForegroundWindowAsync();
        await hal.GetPerformanceMetricsAsync();
        
        stopwatch.Stop();

        // Assert
        var totalTime = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Cross-platform operations completed in {totalTime}ms on {CurrentPlatform}");

        // Performance should be reasonable across platforms
        totalTime.Should().BeLessThan(5000, "Cross-platform operations should complete within 5 seconds");
    }

    [Fact]
    public async Task CrossPlatformMemoryUsage_ShouldBeReasonable()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var initialMemory = GC.GetTotalMemory(true);
        
        // Perform memory-intensive operations
        for (int i = 0; i < 50; i++)
        {
            await hal.Screen.CaptureFullScreenAsync();
            await hal.ImageRecognition.GetImageInfoAsync(new byte[1024]);
            await hal.GetPerformanceMetricsAsync();
        }
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryDiff = finalMemory - initialMemory;

        // Assert
        _output.WriteLine($"Memory usage difference: {memoryDiff} bytes on {CurrentPlatform}");
        
        // Memory usage should be reasonable across platforms
        memoryDiff.Should().BeLessThan(10 * 1024 * 1024, "Memory usage should be less than 10MB");
    }

    [Fact]
    public async Task CrossPlatformErrorHandling_ShouldBeConsistent()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test various error conditions across platforms
        await hal.Invoking(async h => await h.Keyboard.KeyPressAsync((KeyCode)9999))
            .Should().NotThrowAsync("Invalid key codes should be handled gracefully");
        
        await hal.Invoking(async h => await h.Mouse.MoveToAsync(-1000, -1000))
            .Should().NotThrowAsync("Invalid coordinates should be handled gracefully");
        
        await hal.Invoking(async h => await h.Screen.CaptureScreenAsync(-100, -100, 100, 100))
            .Should().NotThrowAsync("Invalid screen coordinates should be handled gracefully");
        
        await hal.Invoking(async h => await h.ImageRecognition.FindImageAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>("Null parameters should throw ArgumentNullException");

        _output.WriteLine($"Error handling is consistent on {CurrentPlatform}");
    }

    [Fact]
    public async Task CrossPlatformConcurrency_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var tasks = new List<Task>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(hal.Keyboard.KeyPressAsync(KeyCode.A));
            tasks.Add(hal.Mouse.MoveToAsync(i % 200, i % 200));
            tasks.Add(hal.Screen.CaptureScreenAsync(i % 50, i % 50, 50, 50));
            tasks.Add(hal.Window.GetForegroundWindowAsync());
            tasks.Add(hal.GetPerformanceMetricsAsync());
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Concurrent operations completed in {stopwatch.ElapsedMilliseconds}ms on {CurrentPlatform}");
        
        // All tasks should complete successfully
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        // Performance should be reasonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Concurrent operations should complete within 30 seconds");
    }

    [Fact]
    public async Task CrossPlatformIntegration_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Complex integration test across all services
        await hal.Keyboard.TypeTextAsync("Integration test");
        await hal.Mouse.MoveToAsync(100, 100);
        await hal.Mouse.LeftClickAsync();
        
        var screen = await hal.Screen.CaptureFullScreenAsync();
        var imageInfo = await hal.ImageRecognition.GetImageInfoAsync(screen);
        
        var window = await hal.Window.GetForegroundWindowAsync();
        var windowTitle = await hal.Window.GetWindowTitleAsync(window);
        
        var metrics = await hal.GetPerformanceMetricsAsync();
        var health = await hal.PerformHealthCheckAsync();
        var quality = await hal.ExecuteQualityGateAsync();
        
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Integration test completed in {stopwatch.ElapsedMilliseconds}ms on {CurrentPlatform}");
        _output.WriteLine($"Window title: {windowTitle}");
        _output.WriteLine($"Health status: {health.Status}");
        _output.WriteLine($"Quality gate passed: {quality.IsPassed}");
        
        // All operations should succeed
        imageInfo.Should().NotBeNull();
        metrics.Should().NotBeNull();
        health.Status.Should().Be(HealthStatus.Healthy);
        quality.IsPassed.Should().BeTrue();
        
        // Performance should be reasonable
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Integration test should complete within 10 seconds");
    }

    [Fact]
    public async Task PlatformSpecificAPIs_ShouldBeAvailable()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test that all platform-specific APIs are available
        hal.Keyboard.Should().NotBeNull();
        hal.Mouse.Should().NotBeNull();
        hal.Screen.Should().NotBeNull();
        hal.GlobalHotkeys.Should().NotBeNull();
        hal.Window.Should().NotBeNull();
        hal.ImageRecognition.Should().NotBeNull();
        hal.PerformanceMonitor.Should().NotBeNull();
        hal.QualityGate.Should().NotBeNull();
        hal.Diagnostics.Should().NotBeNull();

        // Test that all methods are available
        var keyboardMethods = typeof(IKeyboardService).GetMethods();
        var mouseMethods = typeof(IMouseService).GetMethods();
        var screenMethods = typeof(IScreenService).GetMethods();
        var windowMethods = typeof(IWindowService).GetMethods();

        keyboardMethods.Should().NotBeEmpty();
        mouseMethods.Should().NotBeEmpty();
        screenMethods.Should().NotBeEmpty();
        windowMethods.Should().NotBeEmpty();

        _output.WriteLine($"All platform-specific APIs are available on {CurrentPlatform}");
        _output.WriteLine($"Keyboard methods: {keyboardMethods.Length}");
        _output.WriteLine($"Mouse methods: {mouseMethods.Length}");
        _output.WriteLine($"Screen methods: {screenMethods.Length}");
        _output.WriteLine($"Window methods: {windowMethods.Length}");
    }
}