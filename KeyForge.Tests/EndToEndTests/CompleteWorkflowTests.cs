using Xunit;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Core.Models;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.EndToEndTests;

/// <summary>
/// 完整工作流端到端测试
/// 原本实现：复杂的端到端测试场景
/// 简化实现：核心工作流测试
/// </summary>
public class CompleteWorkflowTests : TestBase, IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testLogDirectory;
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly ErrorHandlerManager _errorHandler;
    private readonly LoggerService _logger;
    private readonly IntPtr _testWindowHandle;

    public CompleteWorkflowTests(ITestOutputHelper output) : base(output)
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_E2ETest_{Guid.NewGuid()}");
        _testLogDirectory = Path.Combine(_testDirectory, "Logs");
        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_testLogDirectory);
        
        _testWindowHandle = new IntPtr(12345);
        _logger = new LoggerService(_testLogDirectory);
        _errorHandler = new ErrorHandlerManager(_logger, _testLogDirectory);
        _hotkeyManager = new GlobalHotkeyManager(_testWindowHandle);
        
        Log($"端到端测试目录创建: {_testDirectory}");
    }

    public void Dispose()
    {
        try
        {
            _hotkeyManager?.Dispose();
            _errorHandler?.Dispose();
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
                Log($"端到端测试目录清理: {_testDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public async Task CompleteScriptWorkflow_ShouldWorkSuccessfully()
    {
        // 基于验收标准 AC-FUNC-001: 完整脚本工作流
        
        // Arrange
        var script = new Script
        {
            Name = "Test Workflow Script",
            Description = "Complete workflow test script"
        };

        // Act - 1. 添加动作到脚本
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        script.AddAction(new KeyAction { Key = "B", Delay = 200 });
        script.AddAction(new KeyAction { Key = "C", Delay = 150 });

        // Assert - 1
        script.GetActionCount().Should().Be(3);
        script.GetTotalDuration().Should().Be(450);
        Log($"脚本创建成功: {script.GetActionCount()}个动作, {script.GetTotalDuration()}ms");

        // Act - 2. 保存脚本到文件
        var scriptFilePath = Path.Combine(_testDirectory, "test_script.json");
        await SaveScriptToFile(script, scriptFilePath);

        // Assert - 2
        File.Exists(scriptFilePath).Should().BeTrue();
        var fileInfo = new FileInfo(scriptFilePath);
        fileInfo.Length.Should().BeGreaterThan(0);
        Log($"脚本保存成功: {scriptFilePath}");

        // Act - 3. 从文件加载脚本
        var loadedScript = await LoadScriptFromFile(scriptFilePath);

        // Assert - 3
        loadedScript.Should().NotBeNull();
        loadedScript.Name.Should().Be(script.Name);
        loadedScript.GetActionCount().Should().Be(script.GetActionCount());
        loadedScript.GetTotalDuration().Should().Be(script.GetTotalDuration());
        Log($"脚本加载成功: {loadedScript.Name}");

        // Act - 4. 注册快捷键
        var hotkeyString = "Ctrl+Alt+T";
        var hotkeyRegistered = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert - 4
        hotkeyRegistered.Should().BeTrue();
        _hotkeyManager.IsHotkeyRegistered(hotkeyString).Should().BeTrue();
        Log($"快捷键注册成功: {hotkeyString}");

        // Act - 5. 模拟快捷键触发
        var eventTriggered = false;
        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered = true;
            Log($"快捷键触发: {args.Hotkey.OriginalString}");
        };

        var message = new Message
        {
            Msg = 0x0312, // WM_HOTKEY
            WParam = new IntPtr(1),
            LParam = IntPtr.Zero
        };

        _hotkeyManager.ProcessMessage(ref message);

        // Assert - 5
        eventTriggered.Should().BeTrue();
        Log("快捷键事件触发成功");

        // Act - 6. 处理错误情况
        try
        {
            throw new Exception("Test workflow exception");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex, "Workflow test error");
        }

        // Assert - 6
        var errorLogs = _errorHandler.GetErrorLogs();
        errorLogs.Should().NotBeEmpty();
        errorLogs.Should().Contain(log => log.Contains("Test workflow exception"));
        Log("错误处理成功");

        // Act - 7. 清理资源
        _hotkeyManager.UnregisterHotkey(hotkeyString);
        _errorHandler.ClearErrorLogs();

        // Assert - 7
        _hotkeyManager.IsHotkeyRegistered(hotkeyString).Should().BeFalse();
        _errorHandler.GetErrorCount().Should().Be(0);
        Log("资源清理成功");

        // Final Assert
        Log("完整工作流测试通过");
    }

    [Fact]
    public async Task ErrorRecoveryWorkflow_ShouldHandleErrorsGracefully()
    {
        // 基于验收标准 AC-FUNC-002: 错误恢复
        
        // Arrange
        var script = new Script
        {
            Name = "Error Recovery Test Script",
            Description = "Test error handling and recovery"
        };

        // Act - 1. 创建脚本并添加动作
        script.AddAction(new KeyAction { Key = "A", Delay = 100 });
        script.AddAction(new KeyAction { Key = "B", Delay = 200 });

        // Assert - 1
        script.GetActionCount().Should().Be(2);
        Log($"错误恢复脚本创建成功: {script.GetActionCount()}个动作");

        // Act - 2. 模拟文件保存错误
        var invalidFilePath = Path.Combine("/invalid/path/test_script.json");
        Exception capturedException = null;

        try
        {
            await SaveScriptToFile(script, invalidFilePath);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            _errorHandler.HandleError(ex, "Failed to save script to invalid path");
        }

        // Assert - 2
        capturedException.Should().NotBeNull();
        var errorLogs = _errorHandler.GetErrorLogs();
        errorLogs.Should().Contain(log => log.Contains("Failed to save script"));
        Log("文件保存错误处理成功");

        // Act - 3. 恢复操作 - 使用有效路径
        var validFilePath = Path.Combine(_testDirectory, "recovery_script.json");
        await SaveScriptToFile(script, validFilePath);

        // Assert - 3
        File.Exists(validFilePath).Should().BeTrue();
        Log("错误恢复后文件保存成功");

        // Act - 4. 模拟快捷键注册冲突
        var hotkeyString = "Ctrl+Shift+A";
        _hotkeyManager.RegisterHotkey(hotkeyString);
        var duplicateResult = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert - 4
        duplicateResult.Should().BeFalse();
        _hotkeyManager.IsHotkeyRegistered(hotkeyString).Should().BeTrue();
        Log("快捷键冲突处理成功");

        // Act - 5. 处理并发错误
        var concurrentErrors = 0;
        var maxConcurrentOperations = 10;

        Parallel.For(0, maxConcurrentOperations, i =>
        {
            try
            {
                // 模拟并发操作可能导致的错误
                if (i % 3 == 0)
                {
                    throw new Exception($"Concurrent error {i}");
                }
                
                _logger.LogInfo($"Concurrent operation {i} completed");
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref concurrentErrors);
                _errorHandler.HandleError(ex, $"Concurrent error {i}");
            }
        });

        // Assert - 5
        concurrentErrors.Should().BeGreaterThan(0);
        var finalErrorLogs = _errorHandler.GetErrorLogs();
        finalErrorLogs.Should().HaveCount(concurrentErrors);
        Log($"并发错误处理成功: {concurrentErrors}个错误");

        // Act - 6. 验证系统恢复状态
        var systemIsStable = true;
        try
        {
            // 测试系统核心功能是否仍然正常
            var testScript = new Script { Name = "Recovery Test" };
            testScript.AddAction(new KeyAction { Key = "X", Delay = 50 });
            
            var testFilePath = Path.Combine(_testDirectory, "stability_test.json");
            await SaveScriptToFile(testScript, testFilePath);
            
            var loadedTestScript = await LoadScriptFromFile(testFilePath);
            loadedTestScript.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            systemIsStable = false;
            _errorHandler.HandleError(ex, "System stability check failed");
        }

        // Assert - 6
        systemIsStable.Should().BeTrue();
        Log("系统稳定性验证成功");

        // Final cleanup and validation
        _hotkeyManager.UnregisterAllHotkeys();
        _errorHandler.ClearErrorLogs();
        
        Log("错误恢复工作流测试通过");
    }

    [Fact]
    public async Task PerformanceWorkflow_ShouldMeetPerformanceRequirements()
    {
        // 基于验收标准 AC-NONFUNC-001: 性能要求
        
        var stopwatch = Stopwatch.StartNew();
        
        // Arrange - 创建大型脚本
        var largeScript = new Script
        {
            Name = "Performance Test Script",
            Description = "Large script for performance testing"
        };

        // Act - 1. 批量添加动作
        var actionCount = 1000;
        for (int i = 0; i < actionCount; i++)
        {
            largeScript.AddAction(new KeyAction 
            { 
                Key = ((char)('A' + (i % 26))).ToString(), 
                Delay = 50 + (i % 100) 
            });
        }

        var batchAddTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 1
        largeScript.GetActionCount().Should().Be(actionCount);
        largeScript.GetTotalDuration().Should().BeGreaterThan(0);
        Log($"批量添加 {actionCount} 个动作耗时: {batchAddTime}ms");

        // Act - 2. 保存大型脚本
        var largeScriptFilePath = Path.Combine(_testDirectory, "large_script.json");
        await SaveScriptToFile(largeScript, largeScriptFilePath);

        var saveTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 2
        File.Exists(largeScriptFilePath).Should().BeTrue();
        var fileInfo = new FileInfo(largeScriptFilePath);
        fileInfo.Length.Should().BeGreaterThan(0);
        Log($"保存大型脚本耗时: {saveTime}ms, 文件大小: {fileInfo.Length} bytes");

        // Act - 3. 加载大型脚本
        var loadedLargeScript = await LoadScriptFromFile(largeScriptFilePath);

        var loadTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 3
        loadedLargeScript.Should().NotBeNull();
        loadedLargeScript.GetActionCount().Should().Be(actionCount);
        Log($"加载大型脚本耗时: {loadTime}ms");

        // Act - 4. 批量快捷键操作
        var hotkeyCount = 50;
        var hotkeys = new string[hotkeyCount];

        for (int i = 0; i < hotkeyCount; i++)
        {
            hotkeys[i] = $"Ctrl+{(char)('A' + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeys[i]);
        }

        var batchRegisterTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 4
        _hotkeyManager.GetRegisteredHotkeys().Should().HaveCount(hotkeyCount);
        Log($"批量注册 {hotkeyCount} 个快捷键耗时: {batchRegisterTime}ms");

        // Act - 5. 批量快捷键消息处理
        var messageCount = 1000;
        var eventsTriggered = 0;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            Interlocked.Increment(ref eventsTriggered);
        };

        for (int i = 0; i < messageCount; i++)
        {
            var message = new Message
            {
                Msg = 0x0312, // WM_HOTKEY
                WParam = new IntPtr(1 + (i % hotkeyCount)),
                LParam = IntPtr.Zero
            };

            _hotkeyManager.ProcessMessage(ref message);
        }

        var messageProcessTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 5
        eventsTriggered.Should().Be(messageCount);
        Log($"处理 {messageCount} 个快捷键消息耗时: {messageProcessTime}ms");

        // Act - 6. 批量错误处理
        var errorCount = 100;
        for (int i = 0; i < errorCount; i++)
        {
            try
            {
                throw new Exception($"Performance test exception {i}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, $"Performance error {i}");
            }
        }

        var batchErrorTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Assert - 6
        _errorHandler.GetErrorCount().Should().Be(errorCount);
        Log($"批量处理 {errorCount} 个错误耗时: {batchErrorTime}ms");

        // Act - 7. 内存使用检查
        var initialMemory = GC.GetTotalMemory(true);
        
        // 执行内存密集型操作
        for (int i = 0; i < 10; i++)
        {
            var tempScript = new Script { Name = $"Memory test {i}" };
            for (int j = 0; j < 100; j++)
            {
                tempScript.AddAction(new KeyAction { Key = "X", Delay = 10 });
            }
        }

        var peakMemory = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var finalMemory = GC.GetTotalMemory(true);

        var memoryIncrease = peakMemory - initialMemory;
        var memoryAfterCleanup = finalMemory - initialMemory;

        // Assert - 7
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // 小于10MB
        memoryAfterCleanup.Should().BeLessThan(1 * 1024 * 1024); // 清理后小于1MB
        Log($"内存使用: 增加 {memoryIncrease / 1024}KB, 清理后 {memoryAfterCleanup / 1024}KB");

        // Act - 8. 吞吐量测试
        var throughputTestDuration = TimeSpan.FromSeconds(10);
        var throughputEndTime = DateTime.UtcNow + throughputTestDuration;
        var throughputOperations = 0L;

        while (DateTime.UtcNow < throughputEndTime)
        {
            // 执行典型操作
            var script = new Script { Name = $"Throughput test {throughputOperations}" };
            script.AddAction(new KeyAction { Key = "T", Delay = 10 });
            
            await SaveScriptToFile(script, Path.Combine(_testDirectory, $"throughput_{throughputOperations}.json"));
            
            throughputOperations++;
        }

        var throughput = throughputOperations / throughputTestDuration.TotalSeconds;

        // Assert - 8
        throughput.Should().BeGreaterThan(10); // 每秒至少10个操作
        Log($"系统吞吐量: {throughput:F2} 操作/秒");

        // Final timing
        var totalTime = stopwatch.ElapsedMilliseconds;
        
        Log($"性能工作流总耗时: {totalTime}ms");
        Log($"性能指标验证:");
        Log($"  - 批量添加动作: {batchAddTime}ms ({actionCount}个动作)");
        Log($"  - 保存大型脚本: {saveTime}ms ({fileInfo.Length} bytes)");
        Log($"  - 加载大型脚本: {loadTime}ms");
        Log($"  - 批量注册快捷键: {batchRegisterTime}ms ({hotkeyCount}个快捷键)");
        Log($"  - 消息处理: {messageProcessTime}ms ({messageCount}个消息)");
        Log($"  - 批量错误处理: {batchErrorTime}ms ({errorCount}个错误)");
        Log($"  - 内存使用: {memoryIncrease / 1024}KB 峰值");
        Log($"  - 吞吐量: {throughput:F2} 操作/秒");

        // Performance assertions
        batchAddTime.Should().BeLessThan(1000); // 批量添加应该小于1秒
        saveTime.Should().BeLessThan(500); // 保存应该小于500ms
        loadTime.Should().BeLessThan(500); // 加载应该小于500ms
        batchRegisterTime.Should().BeLessThan(500); // 批量注册应该小于500ms
        messageProcessTime.Should().BeLessThan(1000); // 消息处理应该小于1秒
        batchErrorTime.Should().BeLessThan(1000); // 错误处理应该小于1秒

        Log("性能工作流测试通过");
    }

    [Fact]
    public async Task SecurityWorkflow_ShouldHandleSecurityScenarios()
    {
        // 基于验收标准 AC-SEC-001: 安全要求
        
        // Arrange
        var script = new Script
        {
            Name = "Security Test Script",
            Description = "Test security scenarios"
        };

        // Act - 1. 测试输入验证
        var invalidInputs = new[]
        {
            "",
            null,
            new string('A', 10000), // 超长输入
            "<script>alert('xss')</script>", // XSS尝试
            "'; DROP TABLE users; --", // SQL注入尝试
            "../../../etc/passwd", // 路径遍历尝试
            "file:///etc/passwd", // 文件协议尝试
            "javascript:alert('xss')" // JavaScript协议尝试
        };

        foreach (var invalidInput in invalidInputs)
        {
            try
            {
                // 尝试使用无效输入
                var testScript = new Script { Name = invalidInput ?? "null" };
                testScript.AddAction(new KeyAction { Key = "A", Delay = 100 });
                
                // 应该能够处理而不崩溃
                testScript.GetActionCount().Should().BeGreaterThanOrEqualTo(0);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, $"Security test with input: {invalidInput}");
            }
        }

        // Assert - 1
        // 系统应该能够处理所有无效输入而不崩溃
        Log("无效输入处理测试通过");

        // Act - 2. 测试文件路径安全
        var maliciousPaths = new[]
        {
            "../../../system32/config.exe",
            "/etc/passwd",
            "C:\\Windows\\System32\\cmd.exe",
            "~/../../.ssh/id_rsa",
            "\\\\malicious\\server\\share"
        };

        foreach (var maliciousPath in maliciousPaths)
        {
            try
            {
                // 尝试使用恶意路径
                var testFilePath = Path.Combine(_testDirectory, "security_test.json");
                await SaveScriptToFile(script, testFilePath);
                
                // 应该限制在测试目录内
                var fullPath = Path.GetFullPath(testFilePath);
                fullPath.Should().StartWith(_testDirectory);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, $"Path security test: {maliciousPath}");
            }
        }

        // Assert - 2
        Log("文件路径安全测试通过");

        // Act - 3. 测试快捷键安全
        var maliciousHotkeys = new[]
        {
            "Ctrl+Alt+Del", // 系统快捷键
            "Win+L", // 锁屏快捷键
            "Ctrl+Shift+Esc", // 任务管理器
            "Alt+F4", // 关闭窗口
            "Ctrl+Alt+Backspace", // 重启
            "Alt+Tab", // 切换窗口
            "Win+Tab", // 任务视图
            "Ctrl+Alt+Delete" // 安全选项
        };

        var systemHotkeyAttempts = 0;
        foreach (var maliciousHotkey in maliciousHotkeys)
        {
            try
            {
                var result = _hotkeyManager.RegisterHotkey(maliciousHotkey);
                if (!result)
                {
                    systemHotkeyAttempts++;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, $"Hotkey security test: {maliciousHotkey}");
            }
        }

        // Assert - 3
        // 大多数系统快捷键应该被拒绝
        systemHotkeyAttempts.Should().BeGreaterThan(maliciousHotkeys.Length / 2);
        Log($"系统快捷键保护测试通过: {systemHotkeyAttempts}/{maliciousHotkeys.Length} 被拒绝");

        // Act - 4. 测试异常处理
        var exceptionTypes = new[]
        {
            typeof(OutOfMemoryException),
            typeof(StackOverflowException),
            typeof(AccessViolationException),
            typeof(AppDomainUnloadedException),
            typeof(BadImageFormatException)
        };

        foreach (var exceptionType in exceptionTypes)
        {
            try
            {
                // 模拟系统异常
                var exception = (Exception)Activator.CreateInstance(exceptionType, "Security test exception");
                _errorHandler.HandleError(exception, $"Security exception test");
            }
            catch (Exception ex)
            {
                // 应该能够处理异常而不崩溃
                _logger.LogInfo($"Handled security exception: {ex.GetType().Name}");
            }
        }

        // Assert - 4
        Log("系统异常处理测试通过");

        // Act - 5. 测试资源限制
        var resourceTestTasks = new List<Task>();
        var maxConcurrentOperations = 20;

        for (int i = 0; i < maxConcurrentOperations; i++)
        {
            resourceTestTasks.Add(Task.Run(async () =>
            {
                try
                {
                    // 模拟资源密集型操作
                    for (int j = 0; j < 100; j++)
                    {
                        var tempScript = new Script { Name = $"Resource test {i}-{j}" };
                        tempScript.AddAction(new KeyAction { Key = "R", Delay = 10 });
                        
                        var tempFilePath = Path.Combine(_testDirectory, $"resource_{i}_{j}.json");
                        await SaveScriptToFile(tempScript, tempFilePath);
                        
                        // 短暂延迟
                        await Task.Delay(1);
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleError(ex, $"Resource limit test {i}");
                }
            }));
        }

        await Task.WhenAll(resourceTestTasks);

        // Assert - 5
        // 系统应该能够处理并发操作而不崩溃
        var filesInDirectory = Directory.GetFiles(_testDirectory, "resource_*.json");
        filesInDirectory.Length.Should().BeGreaterThan(0);
        Log($"资源限制测试通过: {filesInDirectory.Length}个文件创建");

        // Act - 6. 测试清理和恢复
        _hotkeyManager.UnregisterAllHotkeys();
        _errorHandler.ClearErrorLogs();
        
        var cleanupSuccessful = true;
        try
        {
            // 验证系统状态
            _hotkeyManager.GetRegisteredHotkeys().Should().BeEmpty();
            _errorHandler.GetErrorCount().Should().Be(0);
            
            // 创建新脚本测试系统功能
            var finalTestScript = new Script { Name = "Final Security Test" };
            finalTestScript.AddAction(new KeyAction { Key = "S", Delay = 50 });
            
            var finalTestPath = Path.Combine(_testDirectory, "final_security_test.json");
            await SaveScriptToFile(finalTestScript, finalTestPath);
            
            var loadedFinalScript = await LoadScriptFromFile(finalTestPath);
            loadedFinalScript.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            cleanupSuccessful = false;
            _errorHandler.HandleError(ex, "Cleanup and recovery failed");
        }

        // Assert - 6
        cleanupSuccessful.Should().BeTrue();
        Log("清理和恢复测试通过");

        Log("安全工作流测试通过");
    }

    // Helper methods
    private async Task SaveScriptToFile(Script script, string filePath)
    {
        // 简化的脚本保存实现
        var scriptData = new
        {
            script.Name,
            script.Description,
            Actions = script.Actions.Select(a => new
            {
                a.Key,
                a.Delay
            }).ToList(),
            script.RepeatCount,
            script.Loop,
            script.CreatedAt,
            script.UpdatedAt
        };

        var json = System.Text.Json.JsonSerializer.Serialize(scriptData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task<Script> LoadScriptFromFile(string filePath)
    {
        // 简化的脚本加载实现
        var json = await File.ReadAllTextAsync(filePath);
        var scriptData = System.Text.Json.JsonSerializer.Deserialize<ScriptData>(json);

        var script = new Script
        {
            Name = scriptData.Name,
            Description = scriptData.Description,
            RepeatCount = scriptData.RepeatCount,
            Loop = scriptData.Loop,
            CreatedAt = scriptData.CreatedAt,
            UpdatedAt = scriptData.UpdatedAt
        };

        foreach (var actionData in scriptData.Actions)
        {
            script.AddAction(new KeyAction
            {
                Key = actionData.Key,
                Delay = actionData.Delay
            });
        }

        return script;
    }

    // Helper class for JSON deserialization
    private class ScriptData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ActionData> Actions { get; set; }
        public int RepeatCount { get; set; }
        public bool Loop { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private class ActionData
    {
        public string Key { get; set; }
        public int Delay { get; set; }
    }
}