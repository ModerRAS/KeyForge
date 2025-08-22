using Xunit;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace KeyForge.Tests.PerformanceTests.Services;

/// <summary>
/// GlobalHotkeyManager 性能测试
/// 原本实现：完整的快捷键管理器性能测试
/// 简化实现：核心快捷键性能测试
/// </summary>
public class GlobalHotkeyManagerPerformanceTests : TestBase, IDisposable
{
    private readonly IntPtr _testWindowHandle;
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly string _testLogDirectory;

    public GlobalHotkeyManagerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _testWindowHandle = new IntPtr(12345);
        _hotkeyManager = new GlobalHotkeyManager(_testWindowHandle);
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_HotkeyPerfTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        Log($"快捷键性能测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            _hotkeyManager?.Dispose();
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"快捷键性能测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public void HotkeyRegistration_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 响应时间
        
        var hotkeyCount = 100;
        var hotkeys = new string[hotkeyCount];
        
        // 生成测试快捷键
        for (int i = 0; i < hotkeyCount; i++)
        {
            hotkeys[i] = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < hotkeyCount; i++)
        {
            _hotkeyManager.RegisterHotkey(hotkeys[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)hotkeyCount;
        
        Log($"注册 {hotkeyCount} 个快捷键总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均注册时间: {averageTime:F2}ms 每个快捷键");
        
        // 每个快捷键注册时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void HotkeyUnregistration_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 响应时间
        
        var hotkeyCount = 100;
        var hotkeys = new string[hotkeyCount];
        
        // 先注册快捷键
        for (int i = 0; i < hotkeyCount; i++)
        {
            hotkeys[i] = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeys[i]);
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < hotkeyCount; i++)
        {
            _hotkeyManager.UnregisterHotkey(hotkeys[i]);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)hotkeyCount;
        
        Log($"注销 {hotkeyCount} 个快捷键总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均注销时间: {averageTime:F2}ms 每个快捷键");
        
        // 每个快捷键注销时间应该小于1ms
        averageTime.Should().BeLessThan(1);
    }

    [Fact]
    public void HotkeyMessageProcessing_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 消息处理响应时间
        
        var hotkeyString = "Ctrl+A";
        var messageCount = 1000;
        var eventTriggered = 0;
        
        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered++;
        };
        
        _hotkeyManager.RegisterHotkey(hotkeyString);
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < messageCount; i++)
        {
            var message = new Message
            {
                Msg = 0x0312, // WM_HOTKEY
                WParam = new IntPtr(1),
                LParam = IntPtr.Zero
            };
            
            _hotkeyManager.ProcessMessage(ref message);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)messageCount;
        
        Log($"处理 {messageCount} 个快捷键消息总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均处理时间: {averageTime:F2}ms 每个消息");
        Log($"事件触发次数: {eventTriggered}");
        
        // 每个消息处理时间应该小于0.1ms
        averageTime.Should().BeLessThan(0.1);
        eventTriggered.Should().Be(messageCount);
    }

    [Fact]
    public void HotkeyLookup_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 查找响应时间
        
        var hotkeyCount = 1000;
        var hotkeys = new string[hotkeyCount];
        
        // 注册大量快捷键
        for (int i = 0; i < hotkeyCount; i++)
        {
            hotkeys[i] = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeys[i]);
        }
        
        var lookupCount = 10000;
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < lookupCount; i++)
        {
            var hotkeyToCheck = hotkeys[i % hotkeyCount];
            _hotkeyManager.IsHotkeyRegistered(hotkeyToCheck);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)lookupCount;
        
        Log($"查找 {lookupCount} 次快捷键状态总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均查找时间: {averageTime:F4}ms 每次查找");
        
        // 每次查找时间应该小于0.01ms
        averageTime.Should().BeLessThan(0.01);
    }

    [Fact]
    public void ConcurrentHotkeyOperations_Performance_ShouldBeGood()
    {
        // 基于验收标准 AC-NONFUNC-001: 并发性能
        
        var operationCount = 1000;
        var successCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, operationCount, i =>
        {
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            
            try
            {
                var result = _hotkeyManager.RegisterHotkey(hotkeyString);
                if (result)
                {
                    Interlocked.Increment(ref successCount);
                }
                
                // 立即注销
                _hotkeyManager.UnregisterHotkey(hotkeyString);
            }
            catch (Exception ex)
            {
                LogError($"并发操作失败: {ex.Message}");
            }
        });
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)operationCount;
        var successRate = successCount / (double)operationCount;
        
        Log($"并发操作 {operationCount} 个快捷键总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均并发操作时间: {averageTime:F2}ms 每个操作");
        Log($"成功率: {successRate:P2}");
        
        // 并发操作时间应该小于5ms每个操作
        averageTime.Should().BeLessThan(5);
        successRate.Should().BeGreaterThan(0.95);
    }

    [Fact]
    public void MemoryUsage_DuringHotkeyOperations_ShouldBeReasonable()
    {
        // 基于验收标准 AC-NONFUNC-001: 资源占用
        
        var initialMemory = GC.GetTotalMemory(true);
        var hotkeyCount = 1000;
        
        // 注册大量快捷键
        for (int i = 0; i < hotkeyCount; i++)
        {
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeyString);
        }
        
        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;
        
        Log($"初始内存: {initialMemory / 1024 / 1024:F2}MB");
        Log($"峰值内存: {peakMemory / 1024 / 1024:F2}MB");
        Log($"内存增长: {memoryIncrease / 1024 / 1024:F2}MB");
        
        // 内存增长应该小于10MB
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024);
        
        // 清理
        _hotkeyManager.UnregisterAllHotkeys();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;
        
        Log($"清理后内存: {finalMemory / 1024 / 1024:F2}MB");
        Log($"清理后内存增长: {memoryAfterCleanup / 1024 / 1024:F2}MB");
        
        // 清理后内存增长应该很小
        memoryAfterCleanup.Should().BeLessThan(1 * 1024 * 1024);
    }

    [Fact]
    public void StressTest_Limit_ShouldBeIdentified()
    {
        // 基于验收标准 AC-NONFUNC-001: 压力测试
        
        var maxConcurrentOperations = 100;
        var successCount = 0;
        var failureCount = 0;
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(0, maxConcurrentOperations, i =>
        {
            try
            {
                var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
                var result = _hotkeyManager.RegisterHotkey(hotkeyString);
                
                if (result)
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref failureCount);
                }
            }
            catch
            {
                Interlocked.Increment(ref failureCount);
            }
        });
        
        stopwatch.Stop();
        
        var successRate = successCount / (double)(successCount + failureCount);
        
        Log($"压力测试结果:");
        Log($"  并发操作数: {maxConcurrentOperations}");
        Log($"  成功数: {successCount}");
        Log($"  失败数: {failureCount}");
        Log($"  成功率: {successRate:P2}");
        Log($"  总时间: {stopwatch.ElapsedMilliseconds}ms");
        
        // 成功率应该大于80%
        successRate.Should().BeGreaterThan(0.80);
        
        // 总时间应该小于10秒
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    public void HotkeyEnumeration_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 枚举性能
        
        var hotkeyCount = 100;
        var enumerationCount = 1000;
        
        // 注册快捷键
        for (int i = 0; i < hotkeyCount; i++)
        {
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeyString);
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < enumerationCount; i++)
        {
            var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();
            registeredHotkeys.Should().HaveCount(hotkeyCount);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)enumerationCount;
        
        Log($"枚举 {enumerationCount} 次快捷键列表总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均枚举时间: {averageTime:F4}ms 每次");
        
        // 每次枚举时间应该小于0.1ms
        averageTime.Should().BeLessThan(0.1);
    }

    [Fact]
    public void HotkeyStringParsing_Performance_ShouldBeFast()
    {
        // 基于验收标准 AC-NONFUNC-001: 解析性能
        
        var parseCount = 10000;
        var testHotkeys = new[]
        {
            "Ctrl+A", "Alt+F4", "Shift+Esc", "Win+R", "Ctrl+Alt+Del",
            "Ctrl+Shift+Esc", "Ctrl+Alt+Shift+Win+A", "Ctrl+Shift+Alt+B"
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < parseCount; i++)
        {
            var hotkeyString = testHotkeys[i % testHotkeys.Length];
            var result = _hotkeyManager.RegisterHotkey(hotkeyString);
            
            // 立即注销以便下次测试
            _hotkeyManager.UnregisterHotkey(hotkeyString);
        }
        
        stopwatch.Stop();
        
        var averageTime = stopwatch.ElapsedMilliseconds / (double)parseCount;
        
        Log($"解析 {parseCount} 个快捷键字符串总时间: {stopwatch.ElapsedMilliseconds}ms");
        Log($"平均解析时间: {averageTime:F4}ms 每个字符串");
        
        // 每个字符串解析时间应该小于0.01ms
        averageTime.Should().BeLessThan(0.01);
    }

    [Fact]
    public void LongTermStability_ShouldBeMaintained()
    {
        // 基于验收标准 AC-NONFUNC-001: 长期稳定性
        
        var testDuration = TimeSpan.FromMinutes(1);
        var operations = new List<long>();
        var memorySamples = new List<long>();
        
        var endTime = DateTime.UtcNow + testDuration;
        var operationCount = 0L;
        
        while (DateTime.UtcNow < endTime)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 执行典型操作
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (operationCount % 26))}";
            var result = _hotkeyManager.RegisterHotkey(hotkeyString);
            
            if (result)
            {
                _hotkeyManager.UnregisterHotkey(hotkeyString);
            }
            
            stopwatch.Stop();
            
            operations.Add(stopwatch.ElapsedMilliseconds);
            operationCount++;
            
            // 定期采样内存使用
            if (operationCount % 100 == 0)
            {
                memorySamples.Add(GC.GetTotalMemory(false));
            }
            
            // 短暂延迟避免过度占用CPU
            System.Threading.Thread.Sleep(1);
        }
        
        var averageOperationTime = operations.Average();
        var maxOperationTime = operations.Max();
        var finalMemory = GC.GetTotalMemory(true);
        
        Log($"长期稳定性测试结果:");
        Log($"  测试持续时间: {testDuration.TotalMinutes:F1} 分钟");
        Log($"  操作总数: {operationCount}");
        Log($"  平均操作时间: {averageOperationTime:F4}ms");
        Log($"  最大操作时间: {maxOperationTime:F2}ms");
        Log($"  最终内存使用: {finalMemory / 1024 / 1024:F2}MB");
        Log($"  吞吐量: {operationCount / testDuration.TotalSeconds:F2} 操作/秒");
        
        // 长期稳定性指标
        averageOperationTime.Should().BeLessThan(1); // 平均操作时间小于1ms
        maxOperationTime.Should().BeLessThan(10); // 最大操作时间小于10ms
        (operationCount / testDuration.TotalSeconds).Should().BeGreaterThan(100); // 吞吐量大于100操作/秒
    }

    [Fact]
    public void PerformanceComparison_WindowsHookVsTimer_ShouldBeMeasured()
    {
        // 基于验收标准 AC-NONFUNC-001: 性能对比
        
        var iterationCount = 1000;
        
        // 测试Windows Hook性能（简化实现）
        var hookStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterationCount; i++)
        {
            // 模拟Windows Hook处理
            var message = new Message
            {
                Msg = 0x0312, // WM_HOTKEY
                WParam = new IntPtr(1),
                LParam = IntPtr.Zero
            };
            
            _hotkeyManager.ProcessMessage(ref message);
        }
        
        hookStopwatch.Stop();
        
        // 测试Timer轮询性能（简化实现）
        var timerStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterationCount; i++)
        {
            // 模拟Timer轮询
            var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();
            registeredHotkeys.Should().NotBeNull();
        }
        
        timerStopwatch.Stop();
        
        var hookAverageTime = hookStopwatch.ElapsedMilliseconds / (double)iterationCount;
        var timerAverageTime = timerStopwatch.ElapsedMilliseconds / (double)iterationCount;
        
        Log($"性能对比结果:");
        Log($"  Windows Hook 平均时间: {hookAverageTime:F4}ms");
        Log($"  Timer 轮询平均时间: {timerAverageTime:F4}ms");
        Log($"  性能差异: {timerAverageTime / hookAverageTime:F2}x");
        
        // Windows Hook应该比Timer轮询更快
        hookAverageTime.Should().BeLessThan(timerAverageTime);
    }
}

/// <summary>
/// GlobalHotkeyManager 基准测试
/// </summary>
[MemoryDiagnoser]
public class GlobalHotkeyManagerBenchmarks
{
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly IntPtr _testWindowHandle;

    public GlobalHotkeyManagerBenchmarks()
    {
        _testWindowHandle = new IntPtr(12345);
        _hotkeyManager = new GlobalHotkeyManager(_testWindowHandle);
    }

    [Benchmark]
    public void RegisterHotkey()
    {
        var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (DateTime.Now.Millisecond % 26))}";
        _hotkeyManager.RegisterHotkey(hotkeyString);
    }

    [Benchmark]
    public void UnregisterHotkey()
    {
        var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (DateTime.Now.Millisecond % 26))}";
        _hotkeyManager.RegisterHotkey(hotkeyString);
        _hotkeyManager.UnregisterHotkey(hotkeyString);
    }

    [Benchmark]
    public void ProcessHotkeyMessage()
    {
        var message = new Message
        {
            Msg = 0x0312, // WM_HOTKEY
            WParam = new IntPtr(1),
            LParam = IntPtr.Zero
        };
        
        _hotkeyManager.ProcessMessage(ref message);
    }

    [Benchmark]
    public void IsHotkeyRegistered()
    {
        var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (DateTime.Now.Millisecond % 26))}";
        _hotkeyManager.IsHotkeyRegistered(hotkeyString);
    }

    [Benchmark]
    public void GetRegisteredHotkeys()
    {
        _hotkeyManager.GetRegisteredHotkeys();
    }
}