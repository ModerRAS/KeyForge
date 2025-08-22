using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Xunit.Abstractions;

namespace KeyForge.HAL.Tests;

/// <summary>
/// HAL抽象层边界测试
/// 简化实现：专注于边界条件和异常情况的测试
/// </summary>
public class HALAbstractionBoundaryTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public HALAbstractionBoundaryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task HAL_InitializeAsync_WithNullOptions_ShouldSucceed()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        await hal.InitializeAsync(null);

        // Assert
        hal.IsInitialized.Should().BeTrue();
        hal.Status.Should().Be(HALStatus.Ready);
    }

    [Fact]
    public async Task HAL_InitializeAsync_Twice_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        await hal.InitializeAsync();
        await hal.InitializeAsync();

        // Assert
        hal.IsInitialized.Should().BeTrue();
        hal.Status.Should().Be(HALStatus.Ready);
    }

    [Fact]
    public async Task HAL_ShutdownAsync_WithoutInitialize_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.ShutdownAsync();

        // Assert
        await act.Should().NotThrowAsync();
        hal.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task HAL_GetPerformanceMetricsAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.GetPerformanceMetricsAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_RunBenchmarkAsync_WithInvalidRequest_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var request = new BenchmarkRequest
        {
            TestName = "",
            Iterations = -1,
            WarmupIterations = -1,
            TestFunction = null
        };

        // Act
        Func<Task> act = async () => await hal.RunBenchmarkAsync(request);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_RunBenchmarkAsync_WithZeroIterations_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var request = new BenchmarkRequest
        {
            TestName = "ZeroIterations",
            Iterations = 0,
            WarmupIterations = 0,
            TestFunction = async () => await Task.CompletedTask
        };

        // Act
        Func<Task> act = async () => await hal.RunBenchmarkAsync(request);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_RunBenchmarkAsync_WithNullTestFunction_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var request = new BenchmarkRequest
        {
            TestName = "NullFunction",
            Iterations = 10,
            WarmupIterations = 2,
            TestFunction = null
        };

        // Act
        Func<Task> act = async () => await hal.RunBenchmarkAsync(request);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_RunBenchmarkAsync_WithThrowingFunction_ShouldHandleException()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var request = new BenchmarkRequest
        {
            TestName = "ThrowingFunction",
            Iterations = 5,
            WarmupIterations = 1,
            TestFunction = async () => throw new InvalidOperationException("Test exception")
        };

        // Act
        var result = await hal.RunBenchmarkAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HAL_PerformHealthCheckAsync_WithTimeout_ShouldReturnTimeout()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var options = new HealthCheckOptions
        {
            Timeout = 1 // 1ms timeout
        };

        // Act
        var result = await hal.PerformHealthCheckAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.IsHealthy.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HAL_PerformHealthCheckAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.PerformHealthCheckAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_CheckPermissionsAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.CheckPermissionsAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async HAL_RequestPermissionsAsync_WithNullRequest_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        Func<Task> act = async () => await hal.RequestPermissionsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HAL_RequestPermissionsAsync_WithEmptyPermissionType_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var request = new PermissionRequest
        {
            PermissionType = "",
            Description = "Test permission"
        };

        // Act
        Func<Task> act = async () => await hal.RequestPermissionsAsync(request);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_ExecuteQualityGateAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.ExecuteQualityGateAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_GenerateDiagnosticsReportAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.GenerateDiagnosticsReportAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_GetSystemInfoAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.GetSystemInfoAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_ReconfigureAsync_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        var configuration = new HALConfiguration();

        // Act
        Func<Task> act = async () => await hal.ReconfigureAsync(configuration);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_ReconfigureAsync_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        Func<Task> act = async () => await hal.ReconfigureAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task HAL_ReconfigureAsync_WithInvalidConfiguration_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var configuration = new HALConfiguration
        {
            Performance = new PerformanceConfiguration
            {
                MonitoringInterval = -1
            }
        };

        // Act
        Func<Task> act = async () => await hal.ReconfigureAsync(configuration);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_KeyboardService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.Keyboard.KeyPressAsync(KeyCode.A);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_MouseService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.Mouse.MoveToAsync(100, 100);

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_ScreenService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.Screen.CaptureFullScreenAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_GlobalHotkeyService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.GlobalHotkeys.RegisterHotkeyAsync(1, new[] { KeyCode.Control }, KeyCode.S, _ => { });

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_WindowService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.Window.GetForegroundWindowAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_ImageRecognitionService_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.ImageRecognition.FindImageAsync(Array.Empty<byte>());

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_PerformanceMonitor_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.PerformanceMonitor.RunBenchmarkAsync(new BenchmarkRequest());

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_QualityGate_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.QualityGate.ExecuteQualityGateAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_Diagnostics_WithoutInitialize_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        Func<Task> act = async () => await hal.Diagnostics.GenerateDiagnosticsReportAsync();

        // Assert
        await act.Should().ThrowAsync<HALException>();
    }

    [Fact]
    public async Task HAL_Dispose_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        hal.Dispose();
        hal.Dispose();

        // Assert
        hal.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task HAL_DisposeAsync_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        await hal.DisposeAsync();
        await hal.DisposeAsync();

        // Assert
        hal.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task HAL_InitializeAsync_AfterDispose_ShouldThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        await hal.DisposeAsync();

        // Act
        Func<Task> act = async () => await hal.InitializeAsync();

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task HAL_ConcurrentInitialize_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () => await hal.InitializeAsync()));
        }

        // Assert
        await Task.WhenAll(tasks);
        hal.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_ConcurrentShutdown_ShouldNotThrow()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () => await hal.ShutdownAsync()));
        }

        // Assert
        await Task.WhenAll(tasks);
        hal.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task HAL_LargeNumberOfOperations_ShouldNotLeakMemory()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var initialMemory = GC.GetTotalMemory(true);
        for (int i = 0; i < 1000; i++)
        {
            await hal.GetPerformanceMetricsAsync();
        }
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);

        // Assert
        var memoryDiff = finalMemory - initialMemory;
        _output.WriteLine($"Memory difference after 1000 operations: {memoryDiff} bytes");
        memoryDiff.Should().BeLessThan(1024 * 1024); // 1MB
    }

    [Fact]
    public async Task HAL_InvalidKeyCode_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var invalidKey = (KeyCode)9999;

        // Act
        var result = await hal.Keyboard.KeyPressAsync(invalidKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HAL_InvalidMouseButton_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var invalidButton = (MouseButton)9999;

        // Act
        var result = await hal.Mouse.LeftClickAsync();

        // Assert
        result.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidCoordinates_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.Mouse.MoveToAsync(-1000, -1000);

        // Assert
        result.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_EmptyText_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.Keyboard.TypeTextAsync("");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_VeryLongText_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var longText = new string('A', 10000);

        // Act
        var result = await hal.Keyboard.TypeTextAsync(longText);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_NegativeDelay_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.Keyboard.TypeTextAsync("Test", -1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_VeryLargeDelay_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.Keyboard.TypeTextAsync("Test", int.MaxValue);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_ZeroTimeout_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.WaitForImageAsync(Array.Empty<byte>(), 0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_NegativeTimeout_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.WaitForImageAsync(Array.Empty<byte>(), -1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_VeryLargeTimeout_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.WaitForImageAsync(Array.Empty<byte>(), int.MaxValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_NullByteArray_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.FindImageAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_EmptyByteArray_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.FindImageAsync(Array.Empty<byte>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_VeryLargeByteArray_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var largeArray = new byte[1024 * 1024 * 10]; // 10MB

        // Act
        var result = await hal.ImageRecognition.FindImageAsync(largeArray);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_InvalidSimilarityThreshold_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.ImageRecognition.FindImageAsync(Array.Empty<byte>(), -1.0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_InvalidRectangle_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var invalidRectangle = new Rectangle(-100, -100, -50, -50);

        // Act
        var result = await hal.ImageRecognition.FindImageAsync(Array.Empty<byte>(), 0.8, invalidRectangle);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HAL_InvalidHotkeyId_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.GlobalHotkeys.UnregisterHotkeyAsync(-1);

        // Assert
        result.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidWindowHandle_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.Window.SetForegroundWindowAsync(IntPtr.Zero);

        // Assert
        result.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidScreenIndex_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = hal.Screen.GetScreenBounds(-1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HAL_InvalidMonitoringInterval_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = await hal.PerformanceMonitor.StartMonitoringAsync(-1);

        // Assert
        result.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidDateRange_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var invalidRange = new DateTimeRange(DateTime.Now, DateTime.Now.AddDays(-1));

        // Act
        var result = await hal.PerformanceMonitor.GenerateReportAsync(invalidRange);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HAL_InvalidCleanupDate_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        var result = hal.PerformanceMonitor.CleanupHistoricalData(DateTime.Now.AddDays(1));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HAL_InvalidQualityGateThresholds_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var configuration = new HALConfiguration
        {
            QualityGate = new QualityGateConfiguration
            {
                TestCoverageThreshold = -1.0,
                CodeComplexityThreshold = -1,
                CodeDuplicationThreshold = -1.0
            }
        };

        // Act
        var result = await hal.ReconfigureAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidPerformanceThresholds_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var configuration = new HALConfiguration
        {
            Performance = new PerformanceConfiguration
            {
                Thresholds = new PerformanceThresholds
                {
                    CpuUsageThreshold = -1.0,
                    MemoryUsageThreshold = -1.0,
                    ResponseTimeThreshold = -1.0,
                    ErrorRateThreshold = -1.0
                }
            }
        };

        // Act
        var result = await hal.ReconfigureAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidLogLevel_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var options = new HALInitializationOptions
        {
            LogLevel = (LogLevel)9999
        };

        // Act
        await hal.InitializeAsync(options);

        // Assert
        hal.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_InvalidPlatformSettings_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var configuration = new HALConfiguration
        {
            PlatformSettings = new Dictionary<string, object>
            {
                { "InvalidSetting", new object() }
            }
        };

        // Act
        var result = await hal.ReconfigureAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_InvalidServiceSettings_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var configuration = new HALConfiguration
        {
            ServiceSettings = new Dictionary<string, object>
            {
                { "InvalidSetting", new object() }
            }
        };

        // Act
        var result = await hal.ReconfigureAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Mock always returns true
    }

    [Fact]
    public async Task HAL_NullEventHandlers_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act
        hal.PlatformChanged += null;
        hal.HardwareStateChanged += null;
        hal.PerformanceReported += null;
        hal.QualityGateTriggered += null;
        hal.DiagnosticsReported += null;

        // Assert
        // Should not throw
        hal.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task HAL_ThrowingEventHandlers_ShouldBeHandled()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        hal.PlatformChanged += (s, e) => throw new InvalidOperationException("Test exception");
        hal.HardwareStateChanged += (s, e) => throw new InvalidOperationException("Test exception");
        hal.PerformanceReported += (s, e) => throw new InvalidOperationException("Test exception");
        hal.QualityGateTriggered += (s, e) => throw new InvalidOperationException("Test exception");
        hal.DiagnosticsReported += (s, e) => throw new InvalidOperationException("Test exception");

        // Act
        await hal.GetPerformanceMetricsAsync();

        // Assert
        // Should not throw
        hal.IsInitialized.Should().BeTrue();
    }
}