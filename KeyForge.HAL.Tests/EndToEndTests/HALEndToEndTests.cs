using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Xunit.Abstractions;

namespace KeyForge.HAL.Tests;

/// <summary>
/// HAL端到端测试场景
/// 简化实现：专注于完整的端到端测试场景
/// </summary>
public class HALEndToEndTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public HALEndToEndTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task E2E_ScriptRecordingAndPlayback_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        // Phase 1: Simulate script recording
        _output.WriteLine("Phase 1: Recording script actions");
        
        var recordingActions = new List<Func<Task>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Record typical user actions
        recordingActions.Add(() => hal.Keyboard.TypeTextAsync("Hello World"));
        recordingActions.Add(() => hal.Keyboard.KeyPressAsync(KeyCode.Enter));
        recordingActions.Add(() => hal.Mouse.MoveToAsync(100, 100));
        recordingActions.Add(() => hal.Mouse.LeftClickAsync());
        recordingActions.Add(() => hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.A));
        recordingActions.Add(() => hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.C));
        recordingActions.Add(() => hal.Mouse.MoveToAsync(200, 200));
        recordingActions.Add(() => hal.Mouse.LeftClickAsync());
        recordingActions.Add(() => hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.V));
        recordingActions.Add(() => hal.Keyboard.KeyPressAsync(KeyCode.Enter));

        // Execute recording actions
        foreach (var action in recordingActions)
        {
            await action();
        }

        stopwatch.Stop();
        _output.WriteLine($"Recording completed in {stopwatch.ElapsedMilliseconds}ms");

        // Phase 2: Simulate script playback
        _output.WriteLine("Phase 2: Playing back recorded actions");
        
        var playbackStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Playback the same actions
        foreach (var action in recordingActions)
        {
            await action();
        }

        playbackStopwatch.Stop();
        _output.WriteLine($"Playback completed in {playbackStopwatch.ElapsedMilliseconds}ms");

        // Phase 3: Validation
        _output.WriteLine("Phase 3: Validating end-to-end workflow");
        
        var healthCheck = await hal.PerformHealthCheckAsync();
        healthCheck.Status.Should().Be(HealthStatus.Healthy);
        
        var metrics = await hal.GetPerformanceMetricsAsync();
        metrics.Should().NotBeNull();
        
        _output.WriteLine($"Health status: {healthCheck.Status}");
        _output.WriteLine($"CPU usage: {metrics.CpuUsage}%");
        _output.WriteLine($"Memory usage: {metrics.MemoryUsage}MB");
        
        // Performance should be reasonable
        var totalTime = stopwatch.ElapsedMilliseconds + playbackStopwatch.ElapsedMilliseconds;
        totalTime.Should().BeLessThan(10000, "E2E script recording and playback should complete within 10 seconds");
    }

    [Fact]
    public async Task E2E_AutomationWorkflow_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting automation workflow");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Step 1: Initial setup
        await hal.Keyboard.TypeTextAsync("Starting automation workflow");
        await hal.Keyboard.KeyPressAsync(KeyCode.Enter);
        
        // Step 2: Navigate to target area
        await hal.Mouse.MoveToAsync(50, 50);
        await hal.Mouse.LeftClickAsync();
        await Task.Delay(100); // Simulate UI response time
        
        // Step 3: Perform data entry
        await hal.Keyboard.TypeTextAsync("Automated data entry");
        await hal.Keyboard.KeyPressAsync(KeyCode.Tab);
        await hal.Keyboard.TypeTextAsync("12345");
        await hal.Keyboard.KeyPressAsync(KeyCode.Tab);
        await hal.Keyboard.TypeTextAsync("test@example.com");
        
        // Step 4: Perform actions
        await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.S); // Save
        await Task.Delay(200);
        
        // Step 5: Verify results
        await hal.Mouse.MoveToAsync(100, 300);
        await hal.Mouse.LeftClickAsync();
        await Task.Delay(100);
        
        // Step 6: Capture evidence
        var screenCapture = await hal.Screen.CaptureScreenAsync(0, 0, 800, 600);
        screenCapture.Should().NotBeNull();
        
        // Step 7: Generate report
        var diagnostics = await hal.GenerateDiagnosticsReportAsync();
        diagnostics.Should().NotBeNull();
        
        workflowStopwatch.Stop();
        
        // Validation
        var healthCheck = await hal.PerformHealthCheckAsync();
        healthCheck.Status.Should().Be(HealthStatus.Healthy);
        
        var qualityGate = await hal.ExecuteQualityGateAsync();
        qualityGate.IsPassed.Should().BeTrue();
        
        _output.WriteLine($"Automation workflow completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Health status: {healthCheck.Status}");
        _output.WriteLine($"Quality gate passed: {qualityGate.IsPassed}");
        _output.WriteLine($"Quality score: {qualityGate.OverallScore}");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(15000, "Automation workflow should complete within 15 seconds");
    }

    [Fact]
    public async Task E2E_MultiWindowWorkflow_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting multi-window workflow");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Step 1: Get current windows
        var initialWindows = await hal.Window.EnumWindowsAsync();
        initialWindows.Should().NotBeEmpty();
        
        _output.WriteLine($"Found {initialWindows.Count()} initial windows");
        
        // Step 2: Focus on different windows
        var windows = initialWindows.Take(Math.Min(3, initialWindows.Count())).ToList();
        
        foreach (var window in windows)
        {
            var title = await hal.Window.GetWindowTitleAsync(window);
            var rect = await hal.Window.GetWindowRectAsync(window);
            
            _output.WriteLine($"Focusing window: {title} at {rect.X},{rect.Y}");
            
            await hal.Window.SetForegroundWindowAsync(window);
            await Task.Delay(200);
            
            // Perform actions in each window
            await hal.Keyboard.TypeTextAsync($"Action in {title}");
            await hal.Keyboard.KeyPressAsync(KeyCode.Enter);
        }
        
        // Step 3: Return to original window
        var foregroundWindow = await hal.Window.GetForegroundWindowAsync();
        await hal.Window.SetForegroundWindowAsync(foregroundWindow);
        
        // Step 4: Capture final state
        var finalScreen = await hal.Screen.CaptureFullScreenAsync();
        finalScreen.Should().NotBeNull();
        
        // Step 5: Performance monitoring
        await hal.PerformanceMonitor.StartMonitoringAsync(100);
        await Task.Delay(500);
        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        await hal.PerformanceMonitor.StopMonitoringAsync();
        
        workflowStopwatch.Stop();
        
        // Validation
        metrics.Should().NotBeNull();
        metrics.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.MemoryUsage.Should().BeGreaterThanOrEqualTo(0);
        
        _output.WriteLine($"Multi-window workflow completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Final CPU usage: {metrics.CpuUsage}%");
        _output.WriteLine($"Final memory usage: {metrics.MemoryUsage}MB");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(20000, "Multi-window workflow should complete within 20 seconds");
    }

    [Fact]
    public async Task E2E_ImageBasedAutomation_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting image-based automation workflow");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Step 1: Capture reference images
        var referenceScreen = await hal.Screen.CaptureFullScreenAsync();
        referenceScreen.Should().NotBeNull();
        
        var referenceInfo = await hal.ImageRecognition.GetImageInfoAsync(referenceScreen);
        referenceInfo.Should().NotBeNull();
        
        _output.WriteLine($"Reference image captured: {referenceInfo.Width}x{referenceInfo.Height}");
        
        // Step 2: Perform actions that change the screen
        await hal.Keyboard.TypeTextAsync("Image-based test");
        await hal.Keyboard.KeyPressAsync(KeyCode.Enter);
        await hal.Mouse.MoveToAsync(200, 200);
        await hal.Mouse.LeftClickAsync();
        
        // Step 3: Capture updated screen
        var updatedScreen = await hal.Screen.CaptureFullScreenAsync();
        updatedScreen.Should().NotBeNull();
        
        // Step 4: Compare images
        var similarity = await hal.ImageRecognition.CalculateSimilarityAsync(referenceScreen, updatedScreen);
        similarity.Should().BeGreaterThanOrEqualTo(0.0);
        similarity.Should().BeLessThanOrEqualTo(1.0);
        
        _output.WriteLine($"Image similarity: {similarity:P2}");
        
        // Step 5: Try to find specific elements (simulated)
        var searchRegion = await hal.ImageRecognition.CaptureRegionAsync(100, 100, 200, 200);
        var searchResult = await hal.ImageRecognition.FindImageAsync(searchRegion, 0.8);
        
        // Step 6: Wait for screen changes (simulated)
        var waitResult = await hal.ImageRecognition.WaitForImageDisappearAsync(referenceScreen, 1000);
        waitResult.Should().BeTrue();
        
        workflowStopwatch.Stop();
        
        // Validation
        var diagnostics = await hal.GenerateDiagnosticsReportAsync();
        diagnostics.Should().NotBeNull();
        
        _output.WriteLine($"Image-based automation completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Memory usage: {diagnostics.MemoryDiagnostics.MemoryUsagePercentage}%");
        _output.WriteLine($"Thread count: {diagnostics.ThreadDiagnostics.ThreadPoolThreads}");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Image-based automation should complete within 10 seconds");
    }

    [Fact]
    public async Task E2E_HotkeyDrivenWorkflow_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var hotkeyTriggered = false;
        var hotkeySequence = new List<string>();

        // Define hotkeys for workflow
        var hotkeys = new[]
        {
            (1, new[] { KeyCode.Control }, KeyCode.S, "Save"),
            (2, new[] { KeyCode.Control }, KeyCode.O, "Open"),
            (3, new[] { KeyCode.Control }, KeyCode.N, "New"),
            (4, new[] { KeyCode.Control, KeyCode.Alt }, KeyCode.Delete, "Task Manager"),
            (5, new[] { KeyCode.Windows }, KeyCode.D, "Show Desktop")
        };

        // Act & Assert
        _output.WriteLine("Starting hotkey-driven workflow");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Step 1: Register hotkeys
        foreach (var (id, modifiers, key, action) in hotkeys)
        {
            var result = await hal.GlobalHotkeys.RegisterHotkeyAsync(
                id, modifiers, key, _ => 
                {
                    hotkeyTriggered = true;
                    hotkeySequence.Add(action);
                });
            
            result.Should().BeTrue($"Failed to register hotkey {action}");
        }
        
        _output.WriteLine($"Registered {hotkeys.Length} hotkeys");
        
        // Step 2: Simulate hotkey triggers
        foreach (var (id, modifiers, key, action) in hotkeys)
        {
            await hal.Keyboard.KeyDownAsync(modifiers[0]);
            if (modifiers.Length > 1)
                await hal.Keyboard.KeyDownAsync(modifiers[1]);
            await hal.Keyboard.KeyDownAsync(key);
            await hal.Keyboard.KeyUpAsync(key);
            if (modifiers.Length > 1)
                await hal.Keyboard.KeyUpAsync(modifiers[1]);
            await hal.Keyboard.KeyUpAsync(modifiers[0]);
            
            await Task.Delay(100);
        }
        
        // Step 3: Perform workflow actions
        await hal.Keyboard.TypeTextAsync("Hotkey workflow test");
        await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.S); // Save
        
        // Step 4: Monitor performance
        await hal.PerformanceMonitor.StartMonitoringAsync(50);
        
        for (int i = 0; i < 10; i++)
        {
            await hal.Keyboard.KeyPressAsync(KeyCode.A + i);
            await hal.Mouse.MoveToAsync(i * 50, i * 50);
        }
        
        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        await hal.PerformanceMonitor.StopMonitoringAsync();
        
        // Step 5: Cleanup
        foreach (var (id, _, _, _) in hotkeys)
        {
            await hal.GlobalHotkeys.UnregisterHotkeyAsync(id);
        }
        
        workflowStopwatch.Stop();
        
        // Validation
        metrics.Should().NotBeNull();
        hotkeySequence.Should().NotBeEmpty();
        
        _output.WriteLine($"Hotkey-driven workflow completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Hotkeys triggered: {hotkeySequence.Count}");
        _output.WriteLine($"Actions: {string.Join(", ", hotkeySequence)}");
        _output.WriteLine($"Average CPU usage: {metrics.CpuUsage}%");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(15000, "Hotkey-driven workflow should complete within 15 seconds");
    }

    [Fact]
    public async Task E2E_ErrorRecoveryWorkflow_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting error recovery workflow");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var errorCount = 0;
        var recoveryCount = 0;
        
        // Step 1: Normal operations
        try
        {
            await hal.Keyboard.TypeTextAsync("Normal operation");
            await hal.Mouse.MoveToAsync(100, 100);
            await hal.Mouse.LeftClickAsync();
        }
        catch (Exception ex)
        {
            errorCount++;
            _output.WriteLine($"Error in normal operation: {ex.Message}");
        }
        
        // Step 2: Simulate error conditions
        var errorOperations = new List<Func<Task>>
        {
            () => hal.Keyboard.KeyPressAsync((KeyCode)9999), // Invalid key
            () => hal.Mouse.MoveToAsync(-1000, -1000), // Invalid coordinates
            () => hal.Screen.CaptureScreenAsync(-100, -100, 100, 100), // Invalid rectangle
            () => hal.ImageRecognition.FindImageAsync(null!), // Null image
            () => hal.GlobalHotkeys.RegisterHotkeyAsync(-1, new[] { KeyCode.Control }, KeyCode.S, _ => { }) // Invalid hotkey ID
        };
        
        foreach (var operation in errorOperations)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                errorCount++;
                _output.WriteLine($"Expected error caught: {ex.Message}");
            }
        }
        
        // Step 3: Recovery operations
        var recoveryOperations = new List<Func<Task>>
        {
            () => hal.Keyboard.KeyPressAsync(KeyCode.A), // Valid key
            () => hal.Mouse.MoveToAsync(50, 50), // Valid coordinates
            () => hal.Screen.CaptureScreenAsync(0, 0, 100, 100), // Valid rectangle
            () => hal.ImageRecognition.FindImageAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 }), // Valid image
            () => hal.GetPerformanceMetricsAsync() // Valid operation
        };
        
        foreach (var operation in recoveryOperations)
        {
            try
            {
                await operation();
                recoveryCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                _output.WriteLine($"Unexpected error in recovery: {ex.Message}");
            }
        }
        
        // Step 4: Final validation
        var healthCheck = await hal.PerformHealthCheckAsync();
        var qualityGate = await hal.ExecuteQualityGateAsync();
        
        workflowStopwatch.Stop();
        
        // Validation
        healthCheck.Status.Should().Be(HealthStatus.Healthy);
        qualityGate.IsPassed.Should().BeTrue();
        recoveryCount.Should().Be(recoveryOperations.Count);
        
        _output.WriteLine($"Error recovery workflow completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Errors encountered: {errorCount}");
        _output.WriteLine($"Successful recoveries: {recoveryCount}");
        _output.WriteLine($"Recovery rate: {(double)recoveryCount / recoveryOperations.Count * 100:F2}%");
        _output.WriteLine($"Health status: {healthCheck.Status}");
        _output.WriteLine($"Quality gate passed: {qualityGate.IsPassed}");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Error recovery workflow should complete within 10 seconds");
    }

    [Fact]
    public async Task E2E_ConcurrentWorkflows_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting concurrent workflows");
        
        var workflowStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Define concurrent workflows
        var workflows = new List<Func<Task>>
        {
            async () =>
            {
                // Workflow 1: Data entry
                for (int i = 0; i < 10; i++)
                {
                    await hal.Keyboard.TypeTextAsync($"Data entry {i}");
                    await hal.Keyboard.KeyPressAsync(KeyCode.Enter);
                }
            },
            async () =>
            {
                // Workflow 2: Mouse navigation
                for (int i = 0; i < 20; i++)
                {
                    await hal.Mouse.MoveToAsync(i * 50, i * 30);
                    await hal.Mouse.LeftClickAsync();
                }
            },
            async () =>
            {
                // Workflow 3: Screen operations
                for (int i = 0; i < 5; i++)
                {
                    await hal.Screen.CaptureScreenAsync(i * 100, i * 100, 200, 200);
                }
            },
            async () =>
            {
                // Workflow 4: Window management
                for (int i = 0; i < 5; i++)
                {
                    var window = await hal.Window.GetForegroundWindowAsync();
                    await hal.Window.SetForegroundWindowAsync(window);
                }
            },
            async () =>
            {
                // Workflow 5: Performance monitoring
                for (int i = 0; i < 10; i++)
                {
                    await hal.GetPerformanceMetricsAsync();
                    await Task.Delay(100);
                }
            }
        };
        
        // Execute workflows concurrently
        var tasks = workflows.Select(workflow => workflow()).ToList();
        await Task.WhenAll(tasks);
        
        workflowStopwatch.Stop();
        
        // Validation
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        var finalMetrics = await hal.GetPerformanceMetricsAsync();
        finalMetrics.Should().NotBeNull();
        
        _output.WriteLine($"Concurrent workflows completed in {workflowStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Workflows executed: {workflows.Count}");
        _output.WriteLine($"Final CPU usage: {finalMetrics.CpuUsage}%");
        _output.WriteLine($"Final memory usage: {finalMetrics.MemoryUsage}MB");
        
        // Performance validation
        workflowStopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Concurrent workflows should complete within 30 seconds");
    }

    [Fact]
    public async Task E2E_LongRunningStabilityTest_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var duration = TimeSpan.FromMinutes(1);
        var cancellationTokenSource = new CancellationTokenSource(duration);
        var operationCount = 0;
        var errorCount = 0;

        // Act & Assert
        _output.WriteLine($"Starting long-running stability test for {duration}");
        
        var testStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    // Perform various operations
                    var operationType = operationCount % 20;
                    
                    switch (operationType)
                    {
                        case 0:
                        case 1:
                        case 2:
                            await hal.Keyboard.KeyPressAsync(KeyCode.A + operationType);
                            break;
                        case 3:
                        case 4:
                            await hal.Keyboard.TypeTextAsync($"Stability test {operationCount}");
                            break;
                        case 5:
                        case 6:
                            await hal.Mouse.MoveToAsync(operationCount % 1000, operationCount % 1000);
                            break;
                        case 7:
                        case 8:
                            await hal.Mouse.LeftClickAsync();
                            break;
                        case 9:
                            await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
                            break;
                        case 10:
                            await hal.Window.GetForegroundWindowAsync();
                            break;
                        case 11:
                            await hal.GetPerformanceMetricsAsync();
                            break;
                        case 12:
                            await hal.PerformHealthCheckAsync();
                            break;
                        case 13:
                            await hal.ExecuteQualityGateAsync();
                            break;
                        case 14:
                            await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.S);
                            break;
                        case 15:
                            await hal.GlobalHotkeys.IsHotkeyAvailable(new[] { KeyCode.Control }, KeyCode.S);
                            break;
                        case 16:
                            await hal.ImageRecognition.GetImageInfoAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
                            break;
                        case 17:
                            await hal.Mouse.ScrollAsync(5);
                            break;
                        case 18:
                            await hal.Keyboard.KeyDownAsync(KeyCode.Shift);
                            await hal.Keyboard.KeyUpAsync(KeyCode.Shift);
                            break;
                        case 19:
                            await hal.Screen.GetScreenResolution();
                            break;
                    }

                    operationCount++;
                    
                    // Log progress every 100 operations
                    if (operationCount % 100 == 0)
                    {
                        var metrics = await hal.GetPerformanceMetricsAsync();
                        _output.WriteLine($"Operations: {operationCount}, CPU: {metrics.CpuUsage}%, Memory: {metrics.MemoryUsage}MB");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _output.WriteLine($"Error at operation {operationCount}: {ex.Message}");
                }
            }
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
        
        testStopwatch.Stop();
        
        // Final validation
        var finalHealthCheck = await hal.PerformHealthCheckAsync();
        var finalQualityGate = await hal.ExecuteQualityGateAsync();
        var finalMetrics = await hal.GetPerformanceMetricsAsync();
        
        // Validation
        operationCount.Should().BeGreaterThan(0);
        testStopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(duration);
        finalHealthCheck.Status.Should().Be(HealthStatus.Healthy);
        finalQualityGate.IsPassed.Should().BeTrue();
        
        var errorRate = (double)errorCount / operationCount * 100;
        errorRate.Should().BeLessThan(1.0, "Error rate should be less than 1%");
        
        _output.WriteLine($"Long-running stability test completed");
        _output.WriteLine($"Duration: {testStopwatch.Elapsed}");
        _output.WriteLine($"Total operations: {operationCount}");
        _output.WriteLine($"Error count: {errorCount}");
        _output.WriteLine($"Error rate: {errorRate:F2}%");
        _output.WriteLine($"Operations per second: {operationCount / testStopwatch.Elapsed.TotalSeconds:F2}");
        _output.WriteLine($"Final health status: {finalHealthCheck.Status}");
        _output.WriteLine($"Final quality gate passed: {finalQualityGate.IsPassed}");
        _output.WriteLine($"Final CPU usage: {finalMetrics.CpuUsage}%");
        _output.WriteLine($"Final memory usage: {finalMetrics.MemoryUsage}MB");
    }

    [Fact]
    public async Task E2E_CompleteSystemIntegration_ShouldWork()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();

        // Act & Assert
        _output.WriteLine("Starting complete system integration test");
        
        var integrationStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Phase 1: System initialization and validation
        _output.WriteLine("Phase 1: System initialization");
        var healthCheck = await hal.PerformHealthCheckAsync();
        healthCheck.Status.Should().Be(HealthStatus.Healthy);
        
        var systemInfo = await hal.GetSystemInfoAsync();
        systemInfo.Should().NotBeNull();
        
        _output.WriteLine($"System: {systemInfo.OperatingSystem}");
        _output.WriteLine($".NET: {systemInfo.DotNetVersion}");
        
        // Phase 2: Service validation
        _output.WriteLine("Phase 2: Service validation");
        
        // Keyboard service
        await hal.Keyboard.TypeTextAsync("System integration test");
        await hal.Keyboard.SendHotkeyAsync(new[] { KeyCode.Control }, KeyCode.S);
        
        // Mouse service
        await hal.Mouse.MoveToAsync(100, 100);
        await hal.Mouse.LeftClickAsync();
        await hal.Mouse.ScrollAsync(3);
        
        // Screen service
        var screen = await hal.Screen.CaptureFullScreenAsync();
        screen.Should().NotBeNull();
        var resolution = hal.Screen.GetScreenResolution();
        resolution.Should().NotBeNull();
        
        // Window service
        var window = await hal.Window.GetForegroundWindowAsync();
        var windowTitle = await hal.Window.GetWindowTitleAsync(window);
        windowTitle.Should().NotBeNull();
        
        // Global hotkey service
        await hal.GlobalHotkeys.RegisterHotkeyAsync(1, new[] { KeyCode.Control }, KeyCode.T, _ => { });
        await hal.GlobalHotkeys.UnregisterHotkeyAsync(1);
        
        // Image recognition service
        var imageInfo = await hal.ImageRecognition.GetImageInfoAsync(screen);
        imageInfo.Should().NotBeNull();
        
        // Phase 3: Performance and quality validation
        _output.WriteLine("Phase 3: Performance and quality validation");
        
        await hal.PerformanceMonitor.StartMonitoringAsync(100);
        
        // Perform mixed operations
        for (int i = 0; i < 50; i++)
        {
            await hal.Keyboard.KeyPressAsync(KeyCode.A + i % 26);
            await hal.Mouse.MoveToAsync(i * 20, i * 20);
            await hal.GetPerformanceMetricsAsync();
        }
        
        var metrics = hal.PerformanceMonitor.GetCurrentMetrics();
        await hal.PerformanceMonitor.StopMonitoringAsync();
        
        var qualityGate = await hal.ExecuteQualityGateAsync();
        qualityGate.IsPassed.Should().BeTrue();
        
        // Phase 4: Comprehensive diagnostics
        _output.WriteLine("Phase 4: Comprehensive diagnostics");
        
        var diagnostics = await hal.GenerateDiagnosticsReportAsync();
        diagnostics.Should().NotBeNull();
        
        // Phase 5: Cleanup and final validation
        _output.WriteLine("Phase 5: Cleanup and final validation");
        
        await hal.ShutdownAsync();
        
        integrationStopwatch.Stop();
        
        // Final validation
        var totalOperations = 50 + 20; // Mixed operations + service validation
        var operationsPerSecond = totalOperations / integrationStopwatch.Elapsed.TotalSeconds;
        
        _output.WriteLine($"Complete system integration test completed in {integrationStopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Operations per second: {operationsPerSecond:F2}");
        _output.WriteLine($"Quality gate score: {qualityGate.OverallScore}");
        _output.WriteLine($"Issues found: {qualityGate.Issues.Count}");
        _output.WriteLine($"Memory usage: {diagnostics.MemoryDiagnostics.MemoryUsagePercentage}%");
        _output.WriteLine($"Thread count: {diagnostics.ThreadDiagnostics.ThreadPoolThreads}");
        _output.WriteLine($"CPU usage: {diagnostics.PerformanceDiagnostics.CpuUsage}%");
        
        // Performance validation
        integrationStopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Complete system integration should complete within 30 seconds");
        operationsPerSecond.Should().BeGreaterThan(1.0, "Should handle at least 1 operation per second");
        qualityGate.OverallScore.Should().BeGreaterThan(50.0, "Quality gate score should be reasonable");
    }
}