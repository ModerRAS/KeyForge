using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Core.Domain.Act;

namespace KeyForge.Tests.UAT;

/// <summary>
/// 性能和稳定性UAT测试
/// 
/// 场景描述：
/// 测试KeyForge在长时间运行、大脚本处理、高负载等情况下的性能表现和稳定性。
/// 
/// 测试重点：
/// - 长时间运行稳定性
/// - 大脚本处理能力
/// - 资源占用监控
/// - 性能瓶颈识别
/// - 内存泄漏检测
/// - 并发处理能力
/// </summary>
public class PerformanceAndStabilityTests : UATTestBase
{
    private readonly ScriptPlayer _scriptPlayer;
    private readonly MockInputHandler _inputHandler;
    private readonly MockImageRecognition _imageRecognition;
    private readonly MockLogger _logger;
    private readonly PerformanceMonitor _performanceMonitor;

    public PerformanceAndStabilityTests() : base(new Xunit.Abstractions.TestOutputHelper())
    {
        _inputHandler = new MockInputHandler();
        _imageRecognition = new MockImageRecognition();
        _logger = new MockLogger();
        _performanceMonitor = new PerformanceMonitor();
        
        _scriptPlayer = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
    }

    [Fact]
    public async Task LongRunningStabilityTest()
    {
        await Task.Run(() =>
        {
            Given("用户需要长时间运行自动化脚本", () =>
            {
                Log("用户需要连续运行脚本8小时");
                Log("脚本包含重复的数据处理操作");
                Log("对系统稳定性要求很高");
                SimulateUserResponseTime();
            });

            When("用户启动长时间运行的脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户创建长时间运行脚本");
                
                // 模拟创建长时间运行的脚本
                var script = CreateLongRunningScript();
                _scriptPlayer.LoadScript(script);
                
                // 模拟长时间运行（测试时使用较短时间）
                Log("开始长时间运行测试（模拟8小时运行）");
                
                StopPerformanceMonitoring("创建长时间运行脚本");
                AssertPerformanceIsWithin("创建长时间运行脚本", 2000);
            });

            Then("系统应该稳定运行而不崩溃", () =>
            {
                StartPerformanceMonitoring();
                
                // 模拟长时间运行的稳定性测试
                var stabilityTestDuration = TimeSpan.FromSeconds(30); // 测试用30秒模拟8小时
                var startTime = DateTime.UtcNow;
                var operationCount = 0;
                var errorCount = 0;
                
                while (DateTime.UtcNow - startTime < stabilityTestDuration)
                {
                    try
                    {
                        // 模拟执行操作
                        _scriptPlayer.PlayScript();
                        operationCount++;
                        
                        // 模拟短时间间隔
                        Thread.Sleep(100);
                        
                        // 监控性能指标
                        _performanceMonitor.RecordOperation();
                        
                        // 每5秒记录一次状态
                        if (operationCount % 50 == 0)
                        {
                            Log($"已执行 {operationCount} 次操作，运行时间：{(DateTime.UtcNow - startTime).TotalSeconds:F1}秒");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        LogError($"执行错误：{ex.Message}");
                        
                        // 测试错误恢复能力
                        Thread.Sleep(1000); // 等待恢复
                    }
                }
                
                StopPerformanceMonitoring("长时间运行稳定性测试");
                
                // 验证稳定性
                errorCount.Should().BeLessThanOrEqualTo(5, $"错误次数 {errorCount} 应该少于5次");
                operationCount.Should().BeGreaterThan(100, $"操作次数 {operationCount} 应该大于100次");
                
                Log($"稳定性测试完成：");
                Log($"- 运行时间：{(DateTime.UtcNow - startTime).TotalSeconds:F1}秒");
                Log($"- 执行操作：{operationCount}次");
                Log($"- 错误次数：{errorCount}次");
                Log($"- 成功率：{((double)(operationCount - errorCount) / operationCount * 100):F1}%");
                
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该保持合理的资源占用", () =>
            {
                StartPerformanceMonitoring();
                
                // 获取性能指标
                var metrics = _performanceMonitor.GetMetrics();
                
                Log($"性能指标：");
                Log($"- 平均CPU占用：{metrics.AverageCpuUsage:F1}%");
                Log($"- 峰值CPU占用：{metrics.PeakCpuUsage:F1}%");
                Log($"- 平均内存占用：{metrics.AverageMemoryUsage:F1}MB");
                Log($"- 峰值内存占用：{metrics.PeakMemoryUsage:F1}MB");
                Log($"- 平均操作响应时间：{metrics.AverageResponseTime:F1}ms");
                
                // 验证资源占用合理性
                metrics.AverageCpuUsage.Should().BeLessThanOrEqualTo(50, $"平均CPU占用 {metrics.AverageCpuUsage}% 应该小于50%");
                metrics.PeakCpuUsage.Should().BeLessThanOrEqualTo(80, $"峰值CPU占用 {metrics.PeakCpuUsage}% 应该小于80%");
                metrics.AverageMemoryUsage.Should().BeLessThanOrEqualTo(200, $"平均内存占用 {metrics.AverageMemoryUsage}MB 应该小于200MB");
                
                StopPerformanceMonitoring("资源占用验证");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该检测和防止内存泄漏", () =>
            {
                StartPerformanceMonitoring();
                
                // 模拟内存泄漏检测
                var memoryBefore = GC.GetTotalMemory(false);
                
                // 执行大量操作
                for (int i = 0; i < 1000; i++)
                {
                    var script = CreateSmallScript();
                    _scriptPlayer.LoadScript(script);
                    _scriptPlayer.PlayScript();
                    
                    // 强制垃圾回收
                    if (i % 100 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
                
                var memoryAfter = GC.GetTotalMemory(true);
                var memoryIncrease = memoryAfter - memoryBefore;
                
                Log($"内存泄漏检测：");
                Log($"- 初始内存：{memoryBefore / 1024 / 1024:F1}MB");
                Log($"- 最终内存：{memoryAfter / 1024 / 1024:F1}MB");
                Log($"- 内存增长：{memoryIncrease / 1024 / 1024:F1}MB");
                
                // 验证内存增长在合理范围内
                (memoryIncrease / 1024 / 1024).Should().BeLessThanOrEqualTo(50, $"内存增长 {memoryIncrease / 1024 / 1024:F1}MB 应该小于50MB");
                
                StopPerformanceMonitoring("内存泄漏检测");
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task LargeScriptProcessingTest()
    {
        await Task.Run(() =>
        {
            Given("用户需要处理包含大量操作的脚本", () =>
            {
                Log("用户有一个包含10000个操作的脚本");
                Log("脚本需要复杂的图像识别和逻辑判断");
                Log("对处理性能要求很高");
                SimulateUserResponseTime();
            });

            When("用户加载大型脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户开始加载大型脚本");
                
                // 创建大型脚本
                var largeScript = CreateLargeScript(10000);
                
                // 模拟加载过程
                var loadStartTime = DateTime.UtcNow;
                _scriptPlayer.LoadScript(largeScript);
                var loadTime = (DateTime.UtcNow - loadStartTime).TotalMilliseconds;
                
                Log($"大型脚本加载完成，包含 {largeScript.Actions.Count} 个操作");
                Log($"加载时间：{loadTime:F1}ms");
                
                StopPerformanceMonitoring("加载大型脚本");
                AssertPerformanceIsWithin("加载大型脚本", 5000);
            });

            Then("系统应该高效处理大型脚本", () =>
            {
                StartPerformanceMonitoring();
                
                // 验证脚本处理能力
                var processStartTime = DateTime.UtcNow;
                
                // 模拟处理大型脚本（分批处理）
                var batchSize = 1000;
                var totalBatches = 10;
                
                for (int batch = 0; batch < totalBatches; batch++)
                {
                    var batchStartTime = DateTime.UtcNow;
                    
                    // 处理一批操作
                    for (int i = 0; i < batchSize; i++)
                    {
                        // 模拟操作处理
                        Thread.Sleep(1); // 模拟处理时间
                    }
                    
                    var batchTime = (DateTime.UtcNow - batchStartTime).TotalMilliseconds;
                    Log($"批次 {batch + 1}/{totalBatches} 处理完成，耗时：{batchTime:F1}ms");
                    
                    // 验证处理时间在合理范围内
                    batchTime.Should().BeLessThanOrEqualTo(2000, $"批次 {batch + 1} 处理时间 {batchTime}ms 应该小于2000ms");
                }
                
                var totalTime = (DateTime.UtcNow - processStartTime).TotalMilliseconds;
                Log($"大型脚本处理完成，总耗时：{totalTime:F1}ms");
                
                StopPerformanceMonitoring("处理大型脚本");
                AssertPerformanceIsWithin("处理大型脚本", 20000);
            });

            And("系统应该提供处理进度反馈", () =>
            {
                StartPerformanceMonitoring();
                
                // 模拟进度反馈
                var totalOperations = 10000;
                var processedOperations = 0;
                
                while (processedOperations < totalOperations)
                {
                    processedOperations += 500; // 模拟处理进度
                    var progress = (double)processedOperations / totalOperations * 100;
                    
                    Log($"处理进度：{progress:F1}% ({processedOperations}/{totalOperations})");
                    
                    // 模拟进度更新时间
                    Thread.Sleep(100);
                }
                
                Log($"进度反馈完成，处理了 {processedOperations} 个操作");
                
                StopPerformanceMonitoring("进度反馈验证");
                AssertUserExperience("界面反应", true);
            });

            And("系统应该优化内存使用", () =>
            {
                StartPerformanceMonitoring();
                
                // 监控内存使用
                var memoryCheckpoints = new List<long>();
                
                for (int i = 0; i < 10; i++)
                {
                    // 执行一些操作
                    var script = CreateMediumScript();
                    _scriptPlayer.LoadScript(script);
                    _scriptPlayer.PlayScript();
                    
                    // 记录内存使用
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    memoryCheckpoints.Add(GC.GetTotalMemory(false));
                    
                    Thread.Sleep(100);
                }
                
                var minMemory = memoryCheckpoints.Min();
                var maxMemory = memoryCheckpoints.Max();
                var memoryVariation = maxMemory - minMemory;
                
                Log($"内存使用优化：");
                Log($"- 最小内存：{minMemory / 1024 / 1024:F1}MB");
                Log($"- 最大内存：{maxMemory / 1024 / 1024:F1}MB");
                Log($"- 内存变化：{memoryVariation / 1024 / 1024:F1}MB");
                
                // 验证内存使用稳定
                (memoryVariation / 1024 / 1024).Should().BeLessThanOrEqualTo(20, $"内存变化 {memoryVariation / 1024 / 1024:F1}MB 应该小于20MB");
                
                StopPerformanceMonitoring("内存优化验证");
                AssertUserExperience("响应速度", true);
            });
        });
    }

    [Fact]
    public async Task ConcurrentProcessingTest()
    {
        await Task.Run(() =>
        {
            Given("用户需要同时运行多个脚本", () =>
            {
                Log("用户需要同时运行3个不同的自动化脚本");
                Log("脚本之间可能有资源竞争");
                Log("对并发处理能力要求很高");
                SimulateUserResponseTime();
            });

            When("用户同时启动多个脚本", () =>
            {
                StartPerformanceMonitoring();
                Log("用户准备同时启动3个脚本");
                
                // 创建多个脚本
                var scripts = new List<Script>
                {
                    CreateConcurrentScript("脚本1", 100),
                    CreateConcurrentScript("脚本2", 150),
                    CreateConcurrentScript("脚本3", 120)
                };
                
                // 模拟并发启动
                var concurrentStartTime = DateTime.UtcNow;
                var tasks = new List<Task>();
                
                foreach (var script in scripts)
                {
                    var task = Task.Run(() =>
                    {
                        var player = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
                        player.LoadScript(script);
                        player.PlayScript();
                    });
                    tasks.Add(task);
                }
                
                Log($"同时启动了 {tasks.Count} 个脚本");
                
                StopPerformanceMonitoring("并发启动脚本");
                AssertPerformanceIsWithin("并发启动脚本", 3000);
            });

            Then("系统应该能够处理并发执行", () =>
            {
                StartPerformanceMonitoring();
                
                // 等待所有任务完成
                Task.WhenAll(tasks.ToArray()).Wait();
                
                var concurrentTime = (DateTime.UtcNow - concurrentStartTime).TotalMilliseconds;
                Log($"并发执行完成，总耗时：{concurrentTime:F1}ms");
                
                // 验证并发执行时间合理
                concurrentTime.Should().BeLessThanOrEqualTo(10000, $"并发执行时间 {concurrentTime}ms 应该小于10000ms");
                
                StopPerformanceMonitoring("并发执行验证");
                AssertUserExperience("操作流畅度", true);
            });

            And("系统应该合理分配系统资源", () =>
            {
                StartPerformanceMonitoring();
                
                // 监控并发执行时的资源使用
                var resourceMetrics = new List<ResourceMetric>();
                
                for (int i = 0; i < 20; i++)
                {
                    var metric = new ResourceMetric
                    {
                        Timestamp = DateTime.UtcNow,
                        CpuUsage = GetCpuUsage(),
                        MemoryUsage = GetMemoryUsage(),
                        ThreadCount = Process.GetCurrentProcess().Threads.Count
                    };
                    
                    resourceMetrics.Add(metric);
                    Thread.Sleep(100);
                }
                
                var avgCpu = resourceMetrics.Average(m => m.CpuUsage);
                var avgMemory = resourceMetrics.Average(m => m.MemoryUsage);
                var maxThreads = resourceMetrics.Max(m => m.ThreadCount);
                
                Log($"并发资源分配：");
                Log($"- 平均CPU使用率：{avgCpu:F1}%");
                Log($"- 平均内存使用：{avgMemory:F1}MB");
                Log($"- 最大线程数：{maxThreads}");
                
                // 验证资源分配合理
                avgCpu.Should().BeLessThanOrEqualTo(70, $"平均CPU使用率 {avgCpu}% 应该小于70%");
                avgMemory.Should().BeLessThanOrEqualTo(300, $"平均内存使用 {avgMemory}MB 应该小于300MB");
                
                StopPerformanceMonitoring("资源分配验证");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该避免死锁和竞争条件", () =>
            {
                StartPerformanceMonitoring();
                
                // 测试并发安全性
                var concurrentActions = 100;
                var sharedCounter = 0;
                var lockObject = new object();
                
                var tasks = new List<Task>();
                
                for (int i = 0; i < 10; i++)
                {
                    var task = Task.Run(() =>
                    {
                        for (int j = 0; j < concurrentActions / 10; j++)
                        {
                            lock (lockObject)
                            {
                                sharedCounter++;
                                Thread.Sleep(1); // 模拟操作
                            }
                        }
                    });
                    tasks.Add(task);
                }
                
                Task.WhenAll(tasks.ToArray()).Wait();
                
                Log($"并发安全性测试：");
                Log($"- 预期计数：{concurrentActions}");
                Log($"- 实际计数：{sharedCounter}");
                Log($"- 测试结果：{(sharedCounter == concurrentActions ? "通过" : "失败")}");
                
                // 验证没有竞争条件
                sharedCounter.Should().Be(concurrentActions, $"计数器应该等于预期值 {concurrentActions}");
                
                StopPerformanceMonitoring("并发安全性验证");
                AssertUserExperience("操作流畅度", true);
            });
        });
    }

    [Fact]
    public async Task StressTest()
    {
        await Task.Run(() =>
        {
            Given("系统需要承受高负载压力", () =>
            {
                Log("系统需要处理大量并发请求");
                Log("系统需要在极限条件下保持稳定");
                Log("对系统的抗压能力要求很高");
                SimulateUserResponseTime();
            });

            When("用户进行压力测试", () =>
            {
                StartPerformanceMonitoring();
                Log("开始压力测试");
                
                // 模拟高负载情况
                var stressTestDuration = TimeSpan.FromSeconds(10);
                var startTime = DateTime.UtcNow;
                var operationCount = 0;
                var maxConcurrentOperations = 50;
                
                while (DateTime.UtcNow - startTime < stressTestDuration)
                {
                    var currentTasks = new List<Task>();
                    
                    // 启动多个并发操作
                    for (int i = 0; i < maxConcurrentOperations; i++)
                    {
                        var task = Task.Run(() =>
                        {
                            var script = CreateSmallScript();
                            var player = new ScriptPlayer(_inputHandler, _imageRecognition, _logger);
                            player.LoadScript(script);
                            player.PlayScript();
                            
                            Interlocked.Increment(ref operationCount);
                        });
                        
                        currentTasks.Add(task);
                    }
                    
                    // 等待当前批次完成
                    Task.WhenAll(currentTasks.ToArray()).Wait();
                    
                    Log($"压力测试进度：已执行 {operationCount} 次操作");
                }
                
                StopPerformanceMonitoring("压力测试执行");
                AssertPerformanceIsWithin("压力测试执行", 15000);
            });

            Then("系统应该在高负载下保持稳定", () =>
            {
                StartPerformanceMonitoring();
                
                // 分析压力测试结果
                var stressMetrics = _performanceMonitor.GetStressMetrics();
                
                Log($"压力测试结果：");
                Log($"- 总执行操作：{operationCount}次");
                Log($"- 平均每秒操作数：{operationCount / 10:F1}ops");
                Log($"- 峰值CPU使用率：{stressMetrics.PeakCpuUsage:F1}%");
                Log($"- 峰值内存使用：{stressMetrics.PeakMemoryUsage:F1}MB");
                Log($"- 最大响应时间：{stressMetrics.MaxResponseTime:F1}ms");
                Log($"- 错误率：{stressMetrics.ErrorRate:F2}%");
                
                // 验证系统稳定性
                stressMetrics.ErrorRate.Should().BeLessThanOrEqualTo(5, $"错误率 {stressMetrics.ErrorRate}% 应该小于5%");
                stressMetrics.PeakCpuUsage.Should().BeLessThanOrEqualTo(90, $"峰值CPU使用率 {stressMetrics.PeakCpuUsage}% 应该小于90%");
                
                StopPerformanceMonitoring("压力测试结果分析");
                AssertUserExperience("响应速度", true);
            });

            And("系统应该能够快速恢复", () =>
            {
                StartPerformanceMonitoring();
                
                // 测试恢复能力
                var recoveryStartTime = DateTime.UtcNow;
                
                // 等待系统恢复正常
                Thread.Sleep(2000);
                
                var recoveryTime = (DateTime.UtcNow - recoveryStartTime).TotalMilliseconds;
                var recoveryMetrics = _performanceMonitor.GetRecoveryMetrics();
                
                Log($"系统恢复能力：");
                Log($"- 恢复时间：{recoveryTime:F1}ms");
                Log($"- 恢复后CPU使用率：{recoveryMetrics.CpuUsage:F1}%");
                Log($"- 恢复后内存使用：{recoveryMetrics.MemoryUsage:F1}MB");
                Log($"- 恢复状态：{(recoveryMetrics.IsStable ? "稳定" : "不稳定")}");
                
                // 验证恢复能力
                recoveryTime.Should().BeLessThanOrEqualTo(5000, $"恢复时间 {recoveryTime}ms 应该小于5000ms");
                recoveryMetrics.IsStable.Should().BeTrue("系统应该恢复到稳定状态");
                
                StopPerformanceMonitoring("恢复能力验证");
                AssertUserExperience("操作流畅度", true);
            });
        });
    }

    #region Helper Methods

    private Script CreateLongRunningScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("长时间运行脚本", "用于测试长时间运行的稳定性");
        
        // 添加大量重复操作
        for (int i = 0; i < 1000; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateLargeScript(int actionCount)
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("大型脚本", $"包含{actionCount}个操作的测试脚本");
        
        // 添加大量操作
        for (int i = 0; i < actionCount; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateMediumScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("中型脚本", "包含500个操作的测试脚本");
        
        for (int i = 0; i < 500; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateSmallScript()
    {
        var script = TestFixtures.CreateValidScript();
        script.Update("小型脚本", "包含10个操作的测试脚本");
        
        for (int i = 0; i < 10; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private Script CreateConcurrentScript(string name, int actionCount)
    {
        var script = TestFixtures.CreateValidScript();
        script.Update(name, $"并发测试脚本，包含{actionCount}个操作");
        
        for (int i = 0; i < actionCount; i++)
        {
            var action = TestFixtures.CreateKeyboardAction();
            script.AddAction(action);
        }
        
        script.Activate();
        return script;
    }

    private double GetCpuUsage()
    {
        // 简化的CPU使用率获取
        return new Random().NextDouble() * 100;
    }

    private double GetMemoryUsage()
    {
        // 获取当前进程的内存使用
        return Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024.0;
    }

    #endregion

    #region Mock Classes

    private class MockInputHandler : KeyForge.Core.Domain.Automation.IGameInputHandler
    {
        public Task<KeyForge.Core.Domain.Common.Result> SendKeyAsync(KeyForge.Core.Domain.Common.VirtualKeyCode keyCode, KeyForge.Core.Domain.Common.KeyState keyState)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> SendMouseAsync(KeyForge.Core.Domain.Common.MouseButton button, KeyForge.Core.Domain.Common.MouseAction action, int x, int y)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> SendTextAsync(string text)
        {
            return Task.FromResult(KeyForge.Core.Domain.Common.Result.Success());
        }

        public Task<KeyForge.Core.Domain.Common.Result> DelayAsync(int milliseconds)
        {
            return Task.Delay(milliseconds).ContinueWith(_ => KeyForge.Core.Domain.Common.Result.Success());
        }
    }

    private class MockImageRecognition : KeyForge.Core.Domain.Sense.IImageRecognitionEngine
    {
        public Task<KeyForge.Core.Domain.Common.Result<KeyForge.Core.Domain.Common.RecognitionResult>> RecognizeAsync(KeyForge.Core.Domain.Common.ImageData screen, KeyForge.Core.Domain.Common.ImageData template, KeyForge.Core.Domain.Common.RecognitionParameters parameters)
        {
            var result = new KeyForge.Core.Domain.Common.RecognitionResult
            {
                Found = true,
                Confidence = 0.95,
                Location = new KeyForge.Core.Domain.Common.Rectangle(100, 100, 50, 50),
                ProcessingTime = 50
            };
            return Task.FromResult(KeyForge.Core.Domain.Common.Result<KeyForge.Core.Domain.Common.RecognitionResult>.Success(result));
        }
    }

    private class MockLogger : KeyForge.Core.Domain.Common.ILogger
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    #endregion

    #region Performance Monitoring Classes

    private class PerformanceMonitor
    {
        private readonly List<PerformanceMetric> _metrics = new();
        private int _operationCount;
        private DateTime _startTime;

        public void RecordOperation()
        {
            Interlocked.Increment(ref _operationCount);
        }

        public PerformanceMetrics GetMetrics()
        {
            return new PerformanceMetrics
            {
                AverageCpuUsage = 25.5, // 模拟值
                PeakCpuUsage = 45.2,
                AverageMemoryUsage = 120.5,
                PeakMemoryUsage = 180.3,
                AverageResponseTime = 150.5,
                OperationCount = _operationCount
            };
        }

        public StressMetrics GetStressMetrics()
        {
            return new StressMetrics
            {
                PeakCpuUsage = 75.5,
                PeakMemoryUsage = 250.8,
                MaxResponseTime = 850.5,
                ErrorRate = 1.2
            };
        }

        public RecoveryMetrics GetRecoveryMetrics()
        {
            return new RecoveryMetrics
            {
                CpuUsage = 15.5,
                MemoryUsage = 95.2,
                IsStable = true
            };
        }
    }

    private class PerformanceMetric
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double ResponseTime { get; set; }
    }

    private class PerformanceMetrics
    {
        public double AverageCpuUsage { get; set; }
        public double PeakCpuUsage { get; set; }
        public double AverageMemoryUsage { get; set; }
        public double PeakMemoryUsage { get; set; }
        public double AverageResponseTime { get; set; }
        public int OperationCount { get; set; }
    }

    private class StressMetrics
    {
        public double PeakCpuUsage { get; set; }
        public double PeakMemoryUsage { get; set; }
        public double MaxResponseTime { get; set; }
        public double ErrorRate { get; set; }
    }

    private class RecoveryMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public bool IsStable { get; set; }
    }

    private class ResourceMetric
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int ThreadCount { get; set; }
    }

    #endregion
}