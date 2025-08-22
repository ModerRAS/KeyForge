using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyForge.Tests.Support;
using KeyForge.Domain;
using KeyForge.Core;
using KeyForge.Infrastructure;

namespace KeyForge.Tests.Integration.SystemIntegration
{
    /// <summary>
    /// 系统集成测试 - 验证与外部系统的集成
    /// 原本实现：简单的系统集成测试
    /// 简化实现：完整的Windows API和第三方库集成验证
    /// </summary>
    public class SystemIntegrationTests : TestBase
    {
        private readonly IInputSimulator _inputSimulator;
        private readonly IWindowManager _windowManager;
        private readonly IProcessManager _processManager;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly IFileStorage _fileStorage;

        public SystemIntegrationTests(ITestOutputHelper output) : base(output)
        {
            _inputSimulator = ServiceProvider.GetRequiredService<IInputSimulator>();
            _windowManager = ServiceProvider.GetRequiredService<IWindowManager>();
            _processManager = ServiceProvider.GetRequiredService<IProcessManager>();
            _systemInfoProvider = ServiceProvider.GetRequiredService<ISystemInfoProvider>();
            _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        }

        [Fact]
        public void InputSimulation_ShouldWorkWithSystem()
        {
            // Arrange - 获取当前活动窗口
            var activeWindow = _windowManager.GetActiveWindow();
            activeWindow.Should().NotBeNull();
            Log($"当前活动窗口: {activeWindow.Title}");

            // Act & Assert - 模拟按键输入
            _inputSimulator.SendKeys("Hello World");
            Log("模拟按键输入完成");

            // Act & Assert - 模拟鼠标移动
            _inputSimulator.MoveMouse(100, 100);
            Log("模拟鼠标移动完成");

            // Act & Assert - 模拟鼠标点击
            _inputSimulator.ClickMouse(MouseButton.Left);
            Log("模拟鼠标点击完成");

            // Act & Assert - 模拟组合键
            _inputSimulator.SendKeys("^c"); // Ctrl+C
            Log("模拟组合键完成");

            Log("输入模拟系统测试完成");
        }

        [Fact]
        public void ProcessManagement_ShouldControlApplications()
        {
            // Arrange - 启动记事本
            var notepad = _processManager.StartProcess("notepad.exe");
            notepad.Should().NotBeNull();
            Log($"启动记事本进程: {notepad.Id}");

            // Act & Assert - 等待启动
            System.Threading.Thread.Sleep(1000);
            var isRunning = _processManager.IsProcessRunning(notepad.Id);
            isRunning.Should().BeTrue();
            Log($"记事本进程运行中: {isRunning}");

            // Act & Assert - 获取进程信息
            var processInfo = _processManager.GetProcessInfo(notepad.Id);
            processInfo.Should().NotBeNull();
            processInfo.ProcessName.Should().Be("notepad");
            Log($"进程信息: {processInfo.ProcessName}, 内存使用: {processInfo.MemoryUsage}MB");

            // Act & Assert - 终止进程
            _processManager.KillProcess(notepad.Id);
            System.Threading.Thread.Sleep(500);
            isRunning = _processManager.IsProcessRunning(notepad.Id);
            isRunning.Should().BeFalse();
            Log($"记事本进程已终止: {!isRunning}");

            Log("进程管理测试完成");
        }

        [Fact]
        public void WindowManagement_ShouldControlWindows()
        {
            // Arrange - 启动记事本
            var notepad = _processManager.StartProcess("notepad.exe");
            System.Threading.Thread.Sleep(1000);

            try
            {
                // Act & Assert - 获取窗口列表
                var windows = _windowManager.GetWindows();
                windows.Should().NotBeEmpty();
                var notepadWindow = windows.FirstOrDefault(w => w.Title.Contains("记事本") || w.Title.Contains("Notepad"));
                notepadWindow.Should().NotBeNull();
                Log($"找到记事本窗口: {notepadWindow.Title}");

                // Act & Assert - 激活窗口
                _windowManager.ActivateWindow(notepadWindow.Handle);
                System.Threading.Thread.Sleep(500);
                var activeWindow = _windowManager.GetActiveWindow();
                activeWindow.Handle.Should().Be(notepadWindow.Handle);
                Log($"激活窗口成功: {activeWindow.Title}");

                // Act & Assert - 移动窗口
                _windowManager.MoveWindow(notepadWindow.Handle, 100, 100, 800, 600);
                System.Threading.Thread.Sleep(500);
                var updatedWindow = _windowManager.GetWindowInfo(notepadWindow.Handle);
                updatedWindow.Should().NotBeNull();
                Log($"移动窗口成功: 位置({updatedWindow.X}, {updatedWindow.Y}), 大小({updatedWindow.Width}x{updatedWindow.Height})");

                // Act & Assert - 最小化窗口
                _windowManager.MinimizeWindow(notepadWindow.Handle);
                System.Threading.Thread.Sleep(500);
                var minimizedWindow = _windowManager.GetWindowInfo(notepadWindow.Handle);
                minimizedWindow.IsMinimized.Should().BeTrue();
                Log($"最小化窗口成功: {minimizedWindow.IsMinimized}");

                // Act & Assert - 最大化窗口
                _windowManager.MaximizeWindow(notepadWindow.Handle);
                System.Threading.Thread.Sleep(500);
                var maximizedWindow = _windowManager.GetWindowInfo(notepadWindow.Handle);
                maximizedWindow.IsMaximized.Should().BeTrue();
                Log($"最大化窗口成功: {maximizedWindow.IsMaximized}");

                // Act & Assert - 恢复窗口
                _windowManager.RestoreWindow(notepadWindow.Handle);
                System.Threading.Thread.Sleep(500);
                var restoredWindow = _windowManager.GetWindowInfo(notepadWindow.Handle);
                restoredWindow.IsMinimized.Should().BeFalse();
                restoredWindow.IsMaximized.Should().BeFalse();
                Log($"恢复窗口成功: {restoredWindow.IsMinimized}, {restoredWindow.IsMaximized}");
            }
            finally
            {
                // Cleanup
                _processManager.KillProcess(notepad.Id);
            }

            Log("窗口管理测试完成");
        }

        [Fact]
        public void SystemInformation_ShouldProvideCorrectInfo()
        {
            // Act & Assert - 获取系统信息
            var systemInfo = _systemInfoProvider.GetSystemInfo();
            systemInfo.Should().NotBeNull();
            Log($"系统信息: {systemInfo.OSVersion}, {systemInfo.ProcessorCount}核心, {systemInfo.TotalMemory}MB内存");

            // Act & Assert - 获取屏幕信息
            var screenInfo = _systemInfoProvider.GetScreenInfo();
            screenInfo.Should().NotBeNull();
            screenInfo.Screens.Should().NotBeEmpty();
            Log($"屏幕信息: {screenInfo.Screens.Count}个显示器, 主屏分辨率{screenInfo.PrimaryScreen.Width}x{screenInfo.PrimaryScreen.Height}");

            // Act & Assert - 获取进程信息
            var processes = _systemInfoProvider.GetRunningProcesses();
            processes.Should().NotBeEmpty();
            var keyForgeProcess = processes.FirstOrDefault(p => p.ProcessName.Contains("KeyForge"));
            Log($"运行进程: {processes.Count}个, KeyForge进程: {keyForgeProcess != null}");

            // Act & Assert - 获取性能信息
            var performanceInfo = _systemInfoProvider.GetPerformanceInfo();
            performanceInfo.Should().NotBeNull();
            performanceInfo.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
            performanceInfo.MemoryUsage.Should().BeGreaterThan(0);
            Log($"性能信息: CPU使用率{performanceInfo.CpuUsage:F1}%, 内存使用{performanceInfo.MemoryUsage}MB");

            Log("系统信息测试完成");
        }

        [Fact]
        public void FileIntegration_ShouldWorkWithSystem()
        {
            // Arrange - 创建测试文件
            var testFilePath = Path.Combine(Path.GetTempPath(), $"KeyForge_Test_{Guid.NewGuid()}.txt");
            var testContent = "KeyForge系统集成测试内容";

            try
            {
                // Act & Assert - 写入文件
                File.WriteAllText(testFilePath, testContent);
                File.Exists(testFilePath).Should().BeTrue();
                Log($"创建测试文件: {testFilePath}");

                // Act & Assert - 读取文件
                var readContent = File.ReadAllText(testFilePath);
                readContent.Should().Be(testContent);
                Log($"读取文件内容: {readContent.Length}字符");

                // Act & Assert - 获取文件信息
                var fileInfo = new FileInfo(testFilePath);
                fileInfo.Exists.Should().BeTrue();
                fileInfo.Length.Should().BeGreaterThan(0);
                Log($"文件信息: 大小{fileInfo.Length}字节, 创建时间{fileInfo.CreationTime}");

                // Act & Assert - 复制文件
                var copyPath = testFilePath.Replace(".txt", "_copy.txt");
                File.Copy(testFilePath, copyPath);
                File.Exists(copyPath).Should().BeTrue();
                Log($"复制文件: {copyPath}");

                // Act & Assert - 删除文件
                File.Delete(copyPath);
                File.Exists(copyPath).Should().BeFalse();
                Log($"删除复制文件: {copyPath}");
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }

            Log("文件系统集成测试完成");
        }

        [Fact]
        public async Task ConcurrentSystemOperations_ShouldBeThreadSafe()
        {
            // Arrange - 并发操作数量
            var operationCount = 10;
            var tasks = new System.Collections.Generic.List<Task>();

            Log($"开始{operationCount}个并发系统操作测试");

            // Act - 并发执行系统操作
            for (int i = 0; i < operationCount; i++)
            {
                var taskId = i;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        // 1. 启动进程
                        var process = _processManager.StartProcess("notepad.exe");
                        Log($"任务{taskId}: 启动进程 {process.Id}");

                        await Task.Delay(500);

                        // 2. 获取系统信息
                        var systemInfo = _systemInfoProvider.GetSystemInfo();
                        Log($"任务{taskId}: 获取系统信息 {systemInfo.OSVersion}");

                        // 3. 模拟输入
                        _inputSimulator.SendKeys($"Task_{taskId}_Test");
                        Log($"任务{taskId}: 模拟输入完成");

                        await Task.Delay(500);

                        // 4. 终止进程
                        _processManager.KillProcess(process.Id);
                        Log($"任务{taskId}: 终止进程完成");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogError($"任务{taskId}失败: {ex.Message}");
                        return false;
                    }
                });

                tasks.Add(task);
            }

            // Assert - 等待所有操作完成
            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            var failureCount = results.Count(r => !r);

            successCount.Should().Be(operationCount);
            failureCount.Should().Be(0);
            Log($"并发系统操作完成: 成功{successCount}个, 失败{failureCount}个");

            Log("并发系统操作测试完成");
        }

        [Fact]
        public void ClipboardIntegration_ShouldWork()
        {
            // Arrange - 测试文本
            var testText = "KeyForge剪贴板集成测试文本";

            // Act & Assert - 设置剪贴板内容
            _inputSimulator.SetClipboardText(testText);
            Log($"设置剪贴板内容: {testText}");

            // Act & Assert - 获取剪贴板内容
            var clipboardText = _inputSimulator.GetClipboardText();
            clipboardText.Should().Be(testText);
            Log($"获取剪贴板内容: {clipboardText}");

            // Act & Assert - 清空剪贴板
            _inputSimulator.ClearClipboard();
            var emptyClipboard = _inputSimulator.GetClipboardText();
            emptyClipboard.Should().BeNullOrEmpty();
            Log($"清空剪贴板: {string.IsNullOrEmpty(emptyClipboard)}");

            Log("剪贴板集成测试完成");
        }

        [Fact]
        public void ScreenCaptureIntegration_ShouldWork()
        {
            // Act & Assert - 获取屏幕截图
            var screenshot = _systemInfoProvider.CaptureScreen();
            screenshot.Should().NotBeNull();
            screenshot.Width.Should().BeGreaterThan(0);
            screenshot.Height.Should().BeGreaterThan(0);
            Log($"屏幕截图: {screenshot.Width}x{screenshot.Height}");

            // Act & Assert - 保存截图
            var screenshotPath = Path.Combine(Path.GetTempPath(), $"KeyForge_Screenshot_{Guid.NewGuid()}.png");
            _systemInfoProvider.SaveScreenshot(screenshot, screenshotPath);
            File.Exists(screenshotPath).Should().BeTrue();
            Log($"保存截图: {screenshotPath}");

            // Act & Assert - 获取截图文件信息
            var fileInfo = new FileInfo(screenshotPath);
            fileInfo.Length.Should().BeGreaterThan(0);
            Log($"截图文件大小: {fileInfo.Length}字节");

            // Cleanup
            File.Delete(screenshotPath);
            Log($"删除截图: {screenshotPath}");

            Log("屏幕截图集成测试完成");
        }

        [Fact]
        public void Performance_SystemIntegration_ShouldBeFast()
        {
            // Arrange - 测试次数
            var testCount = 100;
            var startTime = DateTime.UtcNow;

            // Act - 执行系统集成操作
            for (int i = 0; i < testCount; i++)
            {
                // 1. 获取系统信息
                var systemInfo = _systemInfoProvider.GetSystemInfo();

                // 2. 获取活动窗口
                var activeWindow = _windowManager.GetActiveWindow();

                // 3. 模拟简单输入
                _inputSimulator.SendKeys("Test");

                // 4. 获取进程列表
                var processes = _systemInfoProvider.GetRunningProcesses();
            }

            var endTime = DateTime.UtcNow;
            var duration = (endTime - startTime).TotalMilliseconds;
            var averageTime = duration / testCount;

            // Assert - 性能验证
            duration.Should().BeLessThan(10000); // 10秒内完成100次操作
            averageTime.Should().BeLessThan(100); // 平均每次操作小于100ms

            Log($"系统集成性能测试完成: 总耗时{duration:F2}ms, 平均{averageTime:F2}ms/次, 共{testCount}次");
        }

        [Fact]
        public void ErrorHandling_SystemIntegration_ShouldHandleErrors()
        {
            // Act & Assert - 尝试启动不存在的进程
            var action = () => _processManager.StartProcess("nonexistent_process.exe");
            action.Should().Throw<System.ComponentModel.Win32Exception>();
            Log("启动不存在进程的异常处理成功");

            // Act & Assert - 尝试操作不存在的窗口
            var invalidHandle = new IntPtr(-1);
            var windowAction = () => _windowManager.ActivateWindow(invalidHandle);
            windowAction.Should().Throw<ArgumentException>();
            Log("操作不存在窗口的异常处理成功");

            // Act & Assert - 尝试获取不存在文件的信息
            var fileInfoAction = () => new FileInfo("nonexistent_file.txt");
            var fileInfo = fileInfoAction();
            fileInfo.Exists.Should().BeFalse();
            Log("不存在文件的异常处理成功");

            Log("系统集成错误处理测试完成");
        }
    }
}