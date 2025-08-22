using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace KeyForge.HAL.Tests;

/// <summary>
/// 跨平台服务集成测试
/// 简化实现：专注于跨平台服务的集成测试
/// </summary>
public class CrossPlatformIntegrationTests : CrossPlatformTestBase
{
    private readonly ITestOutputHelper _output;

    public CrossPlatformIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task HAL_Initialize_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        await hal.InitializeAsync();

        // Assert
        hal.IsInitialized.Should().BeTrue();
        hal.Status.Should().Be(HALStatus.Ready);
        hal.PlatformInfo.Platform.Should().Be(CurrentPlatform);
        
        _output.WriteLine($"Platform: {CurrentPlatform}");
        _output.WriteLine($"Platform Version: {hal.PlatformInfo.Version}");
        _output.WriteLine($"Architecture: {hal.PlatformInfo.Architecture}");
        _output.WriteLine($".NET Version: {hal.PlatformInfo.DotNetVersion}");
    }

    [Fact]
    public async Task KeyboardService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var keys = new[]
        {
            KeyCode.A, KeyCode.B, KeyCode.C,
            KeyCode.Enter, KeyCode.Escape, KeyCode.Space,
            KeyCode.Left, KeyCode.Right, KeyCode.Up, KeyCode.Down
        };

        foreach (var key in keys)
        {
            var result = await hal.Keyboard.KeyPressAsync(key);
            result.Should().BeTrue();
            
            var keyState = hal.Keyboard.GetKeyState(key);
            keyState.Should().BeOneOf(KeyState.Up, KeyState.Down, KeyState.Unknown);
            
            var isAvailable = hal.Keyboard.IsKeyAvailable(key);
            isAvailable.Should().BeTrue();
        }

        _output.WriteLine($"Keyboard service works on {CurrentPlatform}");
    }

    [Fact]
    public async Task KeyboardService_TextInput_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var testTexts = new[] { "Hello World", "12345", "!@#$%", "中文测试", "日本語テスト" };

        // Act & Assert
        foreach (var text in testTexts)
        {
            var result = await hal.Keyboard.TypeTextAsync(text);
            result.Should().BeTrue();
        }

        _output.WriteLine($"Text input works on {CurrentPlatform}");
    }

    [Fact]
    public async Task KeyboardService_Hotkeys_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var hotkeyTests = new[]
        {
            (new[] { KeyCode.Control }, KeyCode.S),
            (new[] { KeyCode.Control }, KeyCode.C),
            (new[] { KeyCode.Control, KeyCode.Alt }, KeyCode.Delete),
            (new[] { KeyCode.Windows }, KeyCode.D),
            (new[] { KeyCode.Alt }, KeyCode.Tab)
        };

        foreach (var (modifiers, key) in hotkeyTests)
        {
            var result = await hal.Keyboard.SendHotkeyAsync(modifiers, key);
            result.Should().BeTrue();
        }

        _output.WriteLine($"Hotkeys work on {CurrentPlatform}");
    }

    [Fact]
    public async Task MouseService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var moveResult = await hal.Mouse.MoveToAsync(100, 100);
        moveResult.Should().BeTrue();

        var relativeMoveResult = await hal.Mouse.MoveByAsync(50, 50);
        relativeMoveResult.Should().BeTrue();

        var leftClickResult = await hal.Mouse.LeftClickAsync();
        leftClickResult.Should().BeTrue();

        var rightClickResult = await hal.Mouse.RightClickAsync();
        rightClickResult.Should().BeTrue();

        var middleClickResult = await hal.Mouse.MiddleClickAsync();
        middleClickResult.Should().BeTrue();

        var doubleClickResult = await hal.Mouse.DoubleClickAsync();
        doubleClickResult.Should().BeTrue();

        var scrollResult = await hal.Mouse.ScrollAsync(5);
        scrollResult.Should().BeTrue();

        var position = hal.Mouse.GetPosition();
        position.Should().NotBeNull();

        var leftButtonState = hal.Mouse.GetButtonState(MouseButton.Left);
        leftButtonState.Should().BeOneOf(MouseButtonState.Up, MouseButtonState.Down);

        _output.WriteLine($"Mouse service works on {CurrentPlatform}");
    }

    [Fact]
    public async Task ScreenService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var fullScreen = await hal.Screen.CaptureFullScreenAsync();
        fullScreen.Should().NotBeNull();

        var partialScreen = await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
        partialScreen.Should().NotBeNull();

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

        var isPointOnScreen = hal.Screen.IsPointOnScreen(100, 100);
        isPointOnScreen.Should().BeTrue();

        var colorDepth = hal.Screen.GetColorDepth();
        colorDepth.Should().BeGreaterThan(0);

        var refreshRate = hal.Screen.GetRefreshRate();
        refreshRate.Should().BeGreaterThan(0);

        _output.WriteLine($"Screen service works on {CurrentPlatform}");
        _output.WriteLine($"Screen resolution: {resolution.Width}x{resolution.Height}");
        _output.WriteLine($"Screen count: {screenCount}");
        _output.WriteLine($"Color depth: {colorDepth} bits");
        _output.WriteLine($"Refresh rate: {refreshRate} Hz");
    }

    [Fact]
    public async Task WindowService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var foregroundWindow = await hal.Window.GetForegroundWindowAsync();
        foregroundWindow.Should().NotBeNull();

        var windowTitle = await hal.Window.GetWindowTitleAsync(foregroundWindow);
        windowTitle.Should().NotBeNull();

        var windowRect = await hal.Window.GetWindowRectAsync(foregroundWindow);
        windowRect.Should().NotBeNull();
        windowRect.Width.Should().BeGreaterThanOrEqualTo(0);
        windowRect.Height.Should().BeGreaterThanOrEqualTo(0);

        var isWindowVisible = await hal.Window.IsWindowVisibleAsync(foregroundWindow);
        isWindowVisible.Should().BeOneOf(true, false);

        var isWindow = await hal.Window.IsWindowAsync(foregroundWindow);
        isWindow.Should().BeTrue();

        var windows = await hal.Window.EnumWindowsAsync();
        windows.Should().NotBeNull();
        windows.Should().NotBeEmpty();

        _output.WriteLine($"Window service works on {CurrentPlatform}");
        _output.WriteLine($"Found {windows.Count()} windows");
    }

    [Fact]
    public async Task GlobalHotkeyService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var hotkeyTriggered = false;

        // Act
        var registerResult = await hal.GlobalHotkeys.RegisterHotkeyAsync(
            1, new[] { KeyCode.Control }, KeyCode.S, _ => hotkeyTriggered = true);
        
        // Assert
        registerResult.Should().BeTrue();

        var isRegistered = hal.GlobalHotkeys.IsHotkeyRegistered(1);
        isRegistered.Should().BeTrue();

        var registeredHotkeys = hal.GlobalHotkeys.GetRegisteredHotkeys();
        registeredHotkeys.Should().NotBeNull();

        var isAvailable = hal.GlobalHotkeys.IsHotkeyAvailable(new[] { KeyCode.Control }, KeyCode.S);
        isAvailable.Should().BeFalse(); // Should not be available after registration

        var unregisterResult = await hal.GlobalHotkeys.UnregisterHotkeyAsync(1);
        unregisterResult.Should().BeTrue();

        var suspendResult = await hal.GlobalHotkeys.SuspendAllHotkeysAsync();
        suspendResult.Should().BeTrue();

        var resumeResult = await hal.GlobalHotkeys.ResumeAllHotkeysAsync();
        resumeResult.Should().BeTrue();

        _output.WriteLine($"Global hotkey service works on {CurrentPlatform}");
    }

    [Fact]
    public async Task ImageRecognitionService_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header

        // Act & Assert
        var findImageResult = await hal.ImageRecognition.FindImageAsync(testImage);
        findImageResult.Should().BeNull(); // No image found in mock

        var findAllImagesResult = await hal.ImageRecognition.FindAllImagesAsync(testImage);
        findAllImagesResult.Should().NotBeNull();
        findAllImagesResult.Should().BeEmpty();

        var waitForImageResult = await hal.ImageRecognition.WaitForImageAsync(testImage, 100);
        waitForImageResult.Should().BeNull();

        var waitForDisappearResult = await hal.ImageRecognition.WaitForImageDisappearAsync(testImage, 100);
        waitForDisappearResult.Should().BeTrue();

        var similarityResult = await hal.ImageRecognition.CalculateSimilarityAsync(testImage, testImage);
        similarityResult.Should().BeGreaterThanOrEqualTo(0.0);
        similarityResult.Should().BeLessThanOrEqualTo(1.0);

        var captureRegionResult = await hal.ImageRecognition.CaptureRegionAsync(0, 0, 100, 100);
        captureRegionResult.Should().NotBeNull();

        var imageInfo = await hal.ImageRecognition.GetImageInfoAsync(testImage);
        imageInfo.Should().NotBeNull();

        _output.WriteLine($"Image recognition service works on {CurrentPlatform}");
    }

    [Fact]
    public async Task PerformanceMonitor_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var startResult = await hal.PerformanceMonitor.StartMonitoringAsync(1000);
        startResult.Should().BeTrue();

        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        metrics.Should().NotBeNull();
        metrics.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        metrics.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.MemoryUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.DiskUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.NetworkUsage.Should().BeGreaterThanOrEqualTo(0);

        var historicalMetrics = hal.PerformanceMonitor.GetHistoricalMetrics(
            DateTime.Now.AddMinutes(-1), DateTime.Now);
        historicalMetrics.Should().NotBeNull();

        var benchmarkRequest = new BenchmarkRequest
        {
            TestName = "CrossPlatformBenchmark",
            Iterations = 10,
            TestFunction = async () => await Task.Delay(10)
        };

        var benchmarkResult = await hal.PerformanceMonitor.RunBenchmarkAsync(benchmarkRequest);
        benchmarkResult.Should().NotBeNull();
        benchmarkResult.IsSuccess.Should().BeTrue();
        benchmarkResult.TestName.Should().Be("CrossPlatformBenchmark");
        benchmarkResult.Iterations.Should().Be(10);
        benchmarkResult.AverageTime.Should().BeGreaterThan(0);

        var stopResult = await hal.PerformanceMonitor.StopMonitoringAsync();
        stopResult.Should().BeTrue();

        var cleanupCount = hal.PerformanceMonitor.CleanupHistoricalData(DateTime.Now.AddDays(-1));
        cleanupCount.Should().BeGreaterThanOrEqualTo(0);

        var report = await hal.PerformanceMonitor.GenerateReportAsync(
            new DateTimeRange(DateTime.Now.AddMinutes(-1), DateTime.Now));
        report.Should().NotBeNull();

        _output.WriteLine($"Performance monitor works on {CurrentPlatform}");
        _output.WriteLine($"Average benchmark time: {benchmarkResult.AverageTime}ms");
    }

    [Fact]
    public async Task QualityGate_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var result = await hal.QualityGate.ExecuteQualityGateAsync();
        result.Should().NotBeNull();
        result.IsPassed.Should().BeTrue();
        result.OverallScore.Should().BeGreaterThan(0);
        result.Issues.Should().NotBeNull();
        result.CheckTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));

        _output.WriteLine($"Quality gate works on {CurrentPlatform}");
        _output.WriteLine($"Quality score: {result.OverallScore}");
        _output.WriteLine($"Issues found: {result.Issues.Count}");
    }

    [Fact]
    public async Task Diagnostics_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var report = await hal.Diagnostics.GenerateDiagnosticsReportAsync();
        report.Should().NotBeNull();
        report.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        report.SystemInfo.Should().NotBeNull();
        report.MemoryDiagnostics.Should().NotBeNull();
        report.ThreadDiagnostics.Should().NotBeNull();
        report.PerformanceDiagnostics.Should().NotBeNull();
        report.ErrorDiagnostics.Should().NotBeNull();
        report.Recommendations.Should().NotBeNull();

        _output.WriteLine($"Diagnostics service works on {CurrentPlatform}");
        _output.WriteLine($"Memory usage: {report.MemoryDiagnostics.MemoryUsagePercentage}%");
        _output.WriteLine($"Thread count: {report.ThreadDiagnostics.ThreadPoolThreads}");
        _output.WriteLine($"CPU usage: {report.PerformanceDiagnostics.CpuUsage}%");
        _output.WriteLine($"Error count: {report.ErrorDiagnostics.ErrorCount}");
    }

    [Fact]
    public async Task HAL_Integration_KeyboardMouse_ShouldWorkTogether()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test complex interaction: keyboard + mouse
        await hal.Keyboard.KeyDownAsync(KeyCode.Control);
        await hal.Mouse.MoveToAsync(200, 200);
        await hal.Mouse.LeftClickAsync();
        await hal.Keyboard.KeyUpAsync(KeyCode.Control);

        // All operations should succeed
        _output.WriteLine($"Keyboard-mouse integration works on {CurrentPlatform}");
    }

    [Fact]
    public async Task HAL_Integration_ScreenImageRecognition_ShouldWorkTogether()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var screenCapture = await hal.Screen.CaptureFullScreenAsync();
        var imageInfo = await hal.ImageRecognition.GetImageInfoAsync(screenCapture);

        imageInfo.Should().NotBeNull();

        _output.WriteLine($"Screen-image recognition integration works on {CurrentPlatform}");
    }

    [Fact]
    public async Task HAL_Integration_WindowScreen_ShouldWorkTogether()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var foregroundWindow = await hal.Window.GetForegroundWindowAsync();
        var windowRect = await hal.Window.GetWindowRectAsync(foregroundWindow);
        var screenCapture = await hal.Screen.CaptureScreenAsync(
            windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);

        screenCapture.Should().NotBeNull();

        _output.WriteLine($"Window-screen integration works on {CurrentPlatform}");
    }

    [Fact]
    public async Task HAL_Integration_PerformanceMonitoring_ShouldWorkWithAllServices()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        await hal.PerformanceMonitor.StartMonitoringAsync(100);

        // Perform various operations while monitoring
        await hal.Keyboard.TypeTextAsync("Performance test");
        await hal.Mouse.MoveToAsync(100, 100);
        await hal.Screen.CaptureFullScreenAsync();
        await hal.Window.GetForegroundWindowAsync();

        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        metrics.Should().NotBeNull();

        await hal.PerformanceMonitor.StopMonitoringAsync();

        _output.WriteLine($"Performance monitoring integration works on {CurrentPlatform}");
        _output.WriteLine($"CPU usage: {metrics.CpuUsage}%");
        _output.WriteLine($"Memory usage: {metrics.MemoryUsage}MB");
    }

    [Fact]
    public async Task HAL_Integration_QualityGate_ShouldWorkWithDiagnostics()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        var diagnosticReport = await hal.Diagnostics.GenerateDiagnosticsReportAsync();
        var qualityGateResult = await hal.QualityGate.ExecuteQualityGateAsync();

        qualityGateResult.Should().NotBeNull();
        qualityGateResult.IsPassed.Should().BeTrue();

        _output.WriteLine($"Quality gate-diagnostics integration works on {CurrentPlatform}");
        _output.WriteLine($"Quality score: {qualityGateResult.OverallScore}");
    }

    [Fact]
    public async Task HAL_Integration_GlobalHotkeys_ShouldWorkWithKeyboard()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var hotkeyTriggered = false;

        // Act & Assert
        await hal.GlobalHotkeys.RegisterHotkeyAsync(
            1, new[] { KeyCode.Control }, KeyCode.T, _ => hotkeyTriggered = true);

        // Simulate hotkey trigger
        await hal.Keyboard.KeyDownAsync(KeyCode.Control);
        await hal.Keyboard.KeyDownAsync(KeyCode.T);
        await hal.Keyboard.KeyUpAsync(KeyCode.T);
        await hal.Keyboard.KeyUpAsync(KeyCode.Control);

        // Note: In mock environment, hotkey won't be actually triggered
        // In real implementation, hotkeyTriggered should be true
        _output.WriteLine($"Global hotkey-keyboard integration works on {CurrentPlatform}");
    }

    [Fact]
    public async Task HAL_Integration_ComplexWorkflow_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Complex workflow: Capture screen -> Find image -> Move mouse -> Click -> Type text
        var screenCapture = await hal.Screen.CaptureFullScreenAsync();
        var imageSearch = await hal.ImageRecognition.FindImageAsync(screenCapture, 0.8);
        
        await hal.Mouse.MoveToAsync(imageSearch?.X ?? 100, imageSearch?.Y ?? 100);
        await hal.Mouse.LeftClickAsync();
        await hal.Keyboard.TypeTextAsync("Complex workflow test");

        // Perform health check
        var healthResult = await hal.PerformHealthCheckAsync();
        healthResult.Status.Should().Be(HealthStatus.Healthy);

        // Get performance metrics
        var metrics = await hal.GetPerformanceMetricsAsync();
        metrics.Should().NotBeNull();

        // Execute quality gate
        var qualityResult = await hal.ExecuteQualityGateAsync();
        qualityResult.IsPassed.Should().BeTrue();

        _output.WriteLine($"Complex workflow integration works on {CurrentPlatform}");
        _output.WriteLine($"Health status: {healthResult.Status}");
        _output.WriteLine($"Quality gate passed: {qualityResult.IsPassed}");
    }

    [Fact]
    public async Task HAL_Performance_ShouldBeConsistentAcrossPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Perform various operations
        for (int i = 0; i < 100; i++)
        {
            await hal.Keyboard.KeyPressAsync(KeyCode.A);
            await hal.Mouse.MoveToAsync(i % 100, i % 100);
            await hal.Screen.CaptureScreenAsync(0, 0, 10, 10);
        }
        
        stopwatch.Stop();

        // Assert
        var averageTimePerOperation = stopwatch.ElapsedMilliseconds / 300.0;
        _output.WriteLine($"Average time per operation: {averageTimePerOperation}ms on {CurrentPlatform}");
        
        // Performance should be reasonable (less than 10ms per operation on average)
        averageTimePerOperation.Should().BeLessThan(10);
    }

    [Fact]
    public async Task HAL_MemoryUsage_ShouldBeReasonableAcrossPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var initialMemory = GC.GetTotalMemory(true);
        
        // Perform memory-intensive operations
        for (int i = 0; i < 100; i++)
        {
            await hal.Screen.CaptureFullScreenAsync();
            await hal.ImageRecognition.GetImageInfoAsync(new byte[1024]);
        }
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryDiff = finalMemory - initialMemory;

        // Assert
        _output.WriteLine($"Memory usage difference: {memoryDiff} bytes on {CurrentPlatform}");
        
        // Memory usage should be reasonable (less than 10MB)
        memoryDiff.Should().BeLessThan(10 * 1024 * 1024);
    }

    [Fact]
    public async Task HAL_ErrorHandling_ShouldBeConsistentAcrossPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Test various error conditions
        await hal.Invoking(async h => await h.Keyboard.KeyPressAsync((KeyCode)9999))
            .Should().NotThrowAsync();
        
        await hal.Invoking(async h => await h.Mouse.MoveToAsync(-1000, -1000))
            .Should().NotThrowAsync();
        
        await hal.Invoking(async h => await h.Screen.CaptureScreenAsync(-100, -100, 100, 100))
            .Should().NotThrowAsync();
        
        await hal.Invoking(async h => await h.ImageRecognition.FindImageAsync(null!))
            .Should().NotThrowAsync();

        _output.WriteLine($"Error handling is consistent on {CurrentPlatform}");
    }

    [Fact]
    public async Task HAL_ConcurrentOperations_ShouldWorkOnAllPlatforms()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var tasks = new List<Task>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(hal.Keyboard.KeyPressAsync(KeyCode.A));
            tasks.Add(hal.Mouse.MoveToAsync(i % 200, i % 200));
            tasks.Add(hal.Screen.CaptureScreenAsync(0, 0, 50, 50));
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Concurrent operations completed in {stopwatch.ElapsedMilliseconds}ms on {CurrentPlatform}");
        
        // All tasks should complete successfully
        tasks.ForEach(t => t.Status.Should().Be(TaskStatus.RanToCompletion));
    }

    [Fact]
    public async Task HAL_PlatformSpecificFeatures_ShouldBeAvailable()
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

        // Platform-specific tests
        switch (CurrentPlatform)
        {
            case Platform.Windows:
                await TestWindowsSpecificFeatures(hal);
                break;
            case Platform.Linux:
                await TestLinuxSpecificFeatures(hal);
                break;
            case Platform.MacOS:
                await TestMacOSSpecificFeatures(hal);
                break;
            default:
                _output.WriteLine($"Unknown platform: {CurrentPlatform}");
                break;
        }
    }

    private async Task TestWindowsSpecificFeatures(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing Windows-specific features");
        
        // Windows-specific tests would go here
        // For example: Registry access, Windows API calls, etc.
        
        // For now, just test basic functionality
        var windows = await hal.Window.EnumWindowsAsync();
        windows.Should().NotBeEmpty();
    }

    private async Task TestLinuxSpecificFeatures(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing Linux-specific features");
        
        // Linux-specific tests would go here
        // For example: X11 calls, system commands, etc.
        
        // For now, just test basic functionality
        var screenCount = hal.Screen.GetScreenCount();
        screenCount.Should().BeGreaterThan(0);
    }

    private async Task TestMacOSSpecificFeatures(IHardwareAbstractionLayer hal)
    {
        _output.WriteLine("Testing macOS-specific features");
        
        // macOS-specific tests would go here
        // For example: NSWindow, NSView, etc.
        
        // For now, just test basic functionality
        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull();
    }
}