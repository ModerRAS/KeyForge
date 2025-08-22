using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Exceptions;
using Xunit.Abstractions;

namespace KeyForge.HAL.Tests;

/// <summary>
/// HAL负载和压力测试
/// 简化实现：专注于负载和压力测试
/// </summary>
public class HALStressTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public HALStressTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Stress_ConcurrentInitialization_ShouldNotFail()
    {
        // Arrange
        var iterations = 100;
        var tasks = new List<Task<IHardwareAbstractionLayer>>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var hal = GetHAL();
                await hal.InitializeAsync();
                return hal;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeAssignableTo<IHardwareAbstractionLayer>();
        results.Should().AllSatisfy(hal => hal.IsInitialized.Should().BeTrue());
        
        _output.WriteLine($"Successfully initialized {iterations} HAL instances concurrently");
    }

    [Fact]
    public async Task Stress_MassiveKeyboardOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 10000;
        var keys = Enum.GetValues<KeyCode>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            var key = keys[i % keys.Length];
            tasks.Add(hal.Keyboard.KeyPressAsync(key));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations} keyboard operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)iterations}ms");
    }

    [Fact]
    public async Task Stress_MassiveMouseOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 10000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.Mouse.MoveToAsync(i % 1000, i % 1000));
            tasks.Add(hal.Mouse.LeftClickAsync());
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 2} mouse operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 2)}ms");
    }

    [Fact]
    public async Task Stress_MassiveScreenOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000; // Screen operations are more expensive

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.Screen.CaptureScreenAsync(i % 100, i % 100, 100, 100));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // 60 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations} screen operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)iterations}ms");
    }

    [Fact]
    public async Task Stress_MassiveWindowOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.Window.GetForegroundWindowAsync());
            tasks.Add(hal.Window.EnumWindowsAsync());
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 2} window operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 2)}ms");
    }

    [Fact]
    public async Task Stress_MassiveHotkeyOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.GlobalHotkeys.RegisterHotkeyAsync(i, new[] { KeyCode.Control }, (KeyCode)(65 + i % 26), _ => { }));
            tasks.Add(hal.GlobalHotkeys.UnregisterHotkeyAsync(i));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 2} hotkey operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 2)}ms");
    }

    [Fact]
    public async Task Stress_MassiveImageRecognitionOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;
        var testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.ImageRecognition.FindImageAsync(testImage));
            tasks.Add(hal.ImageRecognition.CalculateSimilarityAsync(testImage, testImage));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // 60 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 2} image recognition operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 2)}ms");
    }

    [Fact]
    public async Task Stress_MassivePerformanceMonitoring_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.PerformanceMonitor.GetCurrentMetrics());
            tasks.Add(hal.GetPerformanceMetricsAsync());
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 2} performance monitoring operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 2)}ms");
    }

    [Fact]
    public async Task Stress_MixedOperations_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 5000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(hal.Keyboard.KeyPressAsync(KeyCode.A));
            tasks.Add(hal.Mouse.MoveToAsync(i % 1000, i % 1000));
            tasks.Add(hal.Screen.CaptureScreenAsync(i % 100, i % 100, 50, 50));
            tasks.Add(hal.Window.GetForegroundWindowAsync());
            tasks.Add(hal.GetPerformanceMetricsAsync());
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // 60 seconds max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {iterations * 5} mixed operations in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per operation: {stopwatch.ElapsedMilliseconds / (double)(iterations * 5)}ms");
    }

    [Fact]
    public async Task Stress_LongRunningOperations_ShouldNotLeakMemory()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var duration = TimeSpan.FromMinutes(1);
        var cancellationTokenSource = new CancellationTokenSource(duration);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(true);
        var operationCount = 0;

        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                // Perform various operations
                await hal.Keyboard.TypeTextAsync("Memory test");
                await hal.Mouse.MoveToAsync(operationCount % 1000, operationCount % 1000);
                await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
                await hal.GetPerformanceMetricsAsync();
                
                operationCount++;
                
                if (operationCount % 1000 == 0)
                {
                    GC.Collect();
                    var currentMemory = GC.GetTotalMemory(true);
                    var memoryDiff = currentMemory - initialMemory;
                    
                    _output.WriteLine($"Operation {operationCount}: Memory diff = {memoryDiff} bytes");
                    
                    // Memory should not grow unbounded
                    memoryDiff.Should().BeLessThan(50 * 1024 * 1024); // 50MB max
                }
            }
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        stopwatch.Stop();

        // Assert
        operationCount.Should().BeGreaterThan(0);
        stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(duration);

        // Final memory check
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        var totalMemoryDiff = finalMemory - initialMemory;
        
        _output.WriteLine($"Total operations: {operationCount}");
        _output.WriteLine($"Total time: {stopwatch.Elapsed}");
        _output.WriteLine($"Total memory diff: {totalMemoryDiff} bytes");
        _output.WriteLine($"Operations per second: {operationCount / stopwatch.Elapsed.TotalSeconds:F2}");
        
        totalMemoryDiff.Should().BeLessThan(100 * 1024 * 1024); // 100MB max for 1 minute
    }

    [Fact]
    public async Task Stress_RapidStartStop_ShouldNotFail()
    {
        // Arrange
        var iterations = 100;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            using var hal = GetHAL();
            await hal.InitializeAsync();
            await hal.ShutdownAsync();
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
        
        _output.WriteLine($"Completed {iterations} start/stop cycles in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average time per cycle: {stopwatch.ElapsedMilliseconds / (double)iterations}ms");
    }

    [Fact]
    public async Task Stress_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var threadCount = 10;
        var operationsPerThread = 1000;
        var threads = new List<Task>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            threads.Add(Task.Run(async () =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    await hal.Keyboard.KeyPressAsync(KeyCode.A);
                    await hal.Mouse.MoveToAsync(threadId * 100 + j % 100, threadId * 100 + j % 100);
                    await hal.Screen.CaptureScreenAsync(j % 50, j % 50, 50, 50);
                }
            }));
        }

        await Task.WhenAll(threads);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // 60 seconds max
        threads.Should().AllSatisfy(thread => thread.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed {threadCount * operationsPerThread * 3} operations across {threadCount} threads in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Operations per second: {(threadCount * operationsPerThread * 3) / stopwatch.Elapsed.TotalSeconds:F2}");
    }

    [Fact]
    public async Task Stress_ErrorRecovery_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var successCount = 0;
        var errorCount = 0;

        for (int i = 0; i < iterations; i++)
        {
            try
            {
                // Normal operations
                await hal.Keyboard.KeyPressAsync(KeyCode.A);
                await hal.Mouse.MoveToAsync(100, 100);
                
                // Potentially problematic operations
                await hal.Keyboard.KeyPressAsync((KeyCode)9999);
                await hal.Mouse.MoveToAsync(-1000, -1000);
                await hal.Screen.CaptureScreenAsync(-100, -100, 100, 100);
                
                // Recovery operations
                await hal.Keyboard.KeyPressAsync(KeyCode.B);
                await hal.Mouse.MoveToAsync(200, 200);
                
                successCount++;
            }
            catch (Exception)
            {
                errorCount++;
            }
        }

        stopwatch.Stop();

        // Assert
        successCount.Should().BeGreaterThan(0);
        errorCount.Should().BeGreaterThanOrEqualTo(0);
        
        _output.WriteLine($"Completed {iterations} operations with {successCount} successes and {errorCount} errors");
        _output.WriteLine($"Success rate: {(double)successCount / iterations * 100:F2}%");
        _output.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Stress_ResourceLimits_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var largeText = new string('A', 1000000); // 1MB string
        var largeImage = new byte[10 * 1024 * 1024]; // 10MB image

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Test large text input
        await hal.Keyboard.TypeTextAsync(largeText);

        // Test large image processing
        for (int i = 0; i < 10; i++)
        {
            await hal.ImageRecognition.GetImageInfoAsync(largeImage);
            await hal.ImageRecognition.CalculateSimilarityAsync(largeImage, largeImage);
        }

        // Test many concurrent operations
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(hal.Screen.CaptureFullScreenAsync());
            tasks.Add(hal.Keyboard.TypeTextAsync(largeText.Substring(0, 1000)));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(120000); // 2 minutes max
        tasks.Should().AllSatisfy(task => task.Status.Should().Be(TaskStatus.RanToCompletion));
        
        _output.WriteLine($"Completed resource limit tests in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Processed {largeText.Length} characters and {largeImage.Length * 10} bytes of image data");
    }

    [Fact]
    public async Task Stress_PerformanceUnderLoad_ShouldBeConsistent()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var iterations = 1000;
        var responseTimes = new List<long>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            await hal.Keyboard.KeyPressAsync(KeyCode.A);
            await hal.Mouse.MoveToAsync(100, 100);
            await hal.Screen.CaptureScreenAsync(0, 0, 50, 50);
            
            stopwatch.Stop();
            responseTimes.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var averageTime = responseTimes.Average();
        var maxTime = responseTimes.Max();
        var minTime = responseTimes.Min();
        var p95Time = responseTimes.OrderBy(t => t).Skip((int)(iterations * 0.95)).First();

        _output.WriteLine($"Average response time: {averageTime:F2}ms");
        _output.WriteLine($"Min response time: {minTime}ms");
        _output.WriteLine($"Max response time: {maxTime}ms");
        _output.WriteLine($"95th percentile: {p95Time}ms");

        // Performance should be consistent (not too much variation)
        averageTime.Should().BeLessThan(100); // 100ms average
        p95Time.Should().BeLessThan(500); // 500ms 95th percentile
        maxTime.Should().BeLessThan(1000); // 1000ms maximum
    }

    [Fact]
    public async Task Stress_SystemStability_ShouldNotFail()
    {
        // Arrange
        var hal = GetHAL();
        await hal.InitializeAsync();
        var duration = TimeSpan.FromMinutes(5);
        var cancellationTokenSource = new CancellationTokenSource(duration);
        var operationCount = 0;
        var errorCount = 0;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    // Perform random operations
                    var operationType = operationCount % 10;
                    
                    switch (operationType)
                    {
                        case 0:
                            await hal.Keyboard.KeyPressAsync(KeyCode.A);
                            break;
                        case 1:
                            await hal.Mouse.MoveToAsync(operationCount % 1000, operationCount % 1000);
                            break;
                        case 2:
                            await hal.Screen.CaptureScreenAsync(0, 0, 100, 100);
                            break;
                        case 3:
                            await hal.Window.GetForegroundWindowAsync();
                            break;
                        case 4:
                            await hal.GetPerformanceMetricsAsync();
                            break;
                        case 5:
                            await hal.Keyboard.TypeTextAsync($"Stability test {operationCount}");
                            break;
                        case 6:
                            await hal.Mouse.LeftClickAsync();
                            break;
                        case 7:
                            await hal.GlobalHotkeys.IsHotkeyAvailable(new[] { KeyCode.Control }, KeyCode.S);
                            break;
                        case 8:
                            await hal.ImageRecognition.GetImageInfoAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
                            break;
                        case 9:
                            await hal.PerformHealthCheckAsync();
                            break;
                    }

                    operationCount++;
                }
                catch (Exception)
                {
                    errorCount++;
                }
            }
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        stopwatch.Stop();

        // Assert
        operationCount.Should().BeGreaterThan(0);
        stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(duration);
        
        var errorRate = (double)errorCount / operationCount * 100;
        _output.WriteLine($"System stability test completed");
        _output.WriteLine($"Duration: {stopwatch.Elapsed}");
        _output.WriteLine($"Total operations: {operationCount}");
        _output.WriteLine($"Error count: {errorCount}");
        _output.WriteLine($"Error rate: {errorRate:F2}%");
        _output.WriteLine($"Operations per second: {operationCount / stopwatch.Elapsed.TotalSeconds:F2}");

        // Error rate should be very low
        errorRate.Should().BeLessThan(1.0); // Less than 1% error rate
    }
}