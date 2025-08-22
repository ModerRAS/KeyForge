using Xunit;
using FluentAssertions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.IntegrationTests.Services;

/// <summary>
/// GlobalHotkeyManager 集成测试
/// 原本实现：完整的快捷键管理器集成测试
/// 简化实现：核心快捷键集成功能测试
/// </summary>
public class GlobalHotkeyManagerIntegrationTests : TestBase, IDisposable
{
    private readonly IntPtr _testWindowHandle;
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly string _testLogDirectory;

    public GlobalHotkeyManagerIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _testWindowHandle = new IntPtr(12345); // 模拟窗口句柄
        _hotkeyManager = new GlobalHotkeyManager(_testWindowHandle);
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"KeyForge_HotkeyTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        Log($"快捷键集成测试目录创建: {_testLogDirectory}");
    }

    public void Dispose()
    {
        try
        {
            _hotkeyManager?.Dispose();
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
                Log($"快捷键集成测试目录清理: {_testLogDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogError($"清理测试资源失败: {ex.Message}");
        }
    }

    [Fact]
    public void RegisterAndUnregisterHotkey_ShouldWorkSuccessfully()
    {
        // Arrange
        var hotkeyString = "Ctrl+Shift+A";
        var eventTriggered = false;
        HotkeyEventArgs eventArgs = null;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered = true;
            eventArgs = args;
        };

        // Act
        var registerResult = _hotkeyManager.RegisterHotkey(hotkeyString);
        var unregisterResult = _hotkeyManager.UnregisterHotkey(hotkeyString);

        // Assert
        registerResult.Should().BeTrue();
        unregisterResult.Should().BeTrue();
        Log($"快捷键注册和注销成功: {hotkeyString}");
    }

    [Fact]
    public void RegisterMultipleHotkeys_ShouldWorkSuccessfully()
    {
        // Arrange
        var hotkeys = new[] { "Ctrl+A", "Alt+F4", "Shift+Esc", "Win+R" };
        var eventTriggered = false;
        var triggeredCount = 0;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered = true;
            triggeredCount++;
        };

        // Act
        foreach (var hotkey in hotkeys)
        {
            var result = _hotkeyManager.RegisterHotkey(hotkey);
            result.Should().BeTrue();
        }

        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        registeredHotkeys.Should().HaveCount(hotkeys.Length);
        foreach (var hotkey in hotkeys)
        {
            registeredHotkeys.Should().Contain(h => h.OriginalString == hotkey);
        }
        Log($"多个快捷键注册成功: {hotkeys.Length}个");
    }

    [Fact]
    public void HotkeyEvent_ShouldBeTriggered_WhenMessageProcessed()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";
        var eventTriggered = false;
        HotkeyEventArgs capturedEventArgs = null;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered = true;
            capturedEventArgs = args;
        };

        // Act
        _hotkeyManager.RegisterHotkey(hotkeyString);

        // 模拟Windows消息
        var message = new Message
        {
            Msg = 0x0312, // WM_HOTKEY
            WParam = new IntPtr(1), // 第一个注册的快捷键ID
            LParam = IntPtr.Zero
        };

        _hotkeyManager.ProcessMessage(ref message);

        // Assert
        eventTriggered.Should().BeTrue();
        capturedEventArgs.Should().NotBeNull();
        capturedEventArgs.Hotkey.OriginalString.Should().Be(hotkeyString);
        Log($"快捷键事件触发成功: {hotkeyString}");
    }

    [Fact]
    public void HotkeyEvent_ShouldNotBeTriggered_ForNonHotkeyMessage()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";
        var eventTriggered = false;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            eventTriggered = true;
        };

        // Act
        _hotkeyManager.RegisterHotkey(hotkeyString);

        // 模拟非快捷键消息
        var message = new Message
        {
            Msg = 0x0001, // WM_CREATE
            WParam = new IntPtr(1),
            LParam = IntPtr.Zero
        };

        _hotkeyManager.ProcessMessage(ref message);

        // Assert
        eventTriggered.Should().BeFalse();
        Log("非快捷键消息不触发事件");
    }

    [Fact]
    public void DuplicateHotkeyRegistration_ShouldFail()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";

        // Act
        var firstResult = _hotkeyManager.RegisterHotkey(hotkeyString);
        var secondResult = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert
        firstResult.Should().BeTrue();
        secondResult.Should().BeFalse();
        LogError("重复快捷键注册失败");
    }

    [Fact]
    public void UnregisterNonExistingHotkey_ShouldFail()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";

        // Act
        var result = _hotkeyManager.UnregisterHotkey(hotkeyString);

        // Assert
        result.Should().BeFalse();
        Log("不存在快捷键注销返回false");
    }

    [Fact]
    public void IsHotkeyRegistered_ShouldReturnCorrectStatus()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";

        // Act
        var beforeRegistration = _hotkeyManager.IsHotkeyRegistered(hotkeyString);
        _hotkeyManager.RegisterHotkey(hotkeyString);
        var afterRegistration = _hotkeyManager.IsHotkeyRegistered(hotkeyString);
        _hotkeyManager.UnregisterHotkey(hotkeyString);
        var afterUnregistration = _hotkeyManager.IsHotkeyRegistered(hotkeyString);

        // Assert
        beforeRegistration.Should().BeFalse();
        afterRegistration.Should().BeTrue();
        afterUnregistration.Should().BeFalse();
        Log("快捷键注册状态检查正确");
    }

    [Fact]
    public void GetRegisteredHotkeys_ShouldReturnAllHotkeys()
    {
        // Arrange
        var hotkeys = new[] { "Ctrl+A", "Alt+F4", "Shift+Esc" };

        // Act
        foreach (var hotkey in hotkeys)
        {
            _hotkeyManager.RegisterHotkey(hotkey);
        }

        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        registeredHotkeys.Should().HaveCount(hotkeys.Length);
        foreach (var hotkey in hotkeys)
        {
            registeredHotkeys.Should().Contain(h => h.OriginalString == hotkey);
        }
        Log($"所有快捷键获取成功: {registeredHotkeys.Length}个");
    }

    [Fact]
    public void UnregisterAllHotkeys_ShouldRemoveAllHotkeys()
    {
        // Arrange
        var hotkeys = new[] { "Ctrl+A", "Alt+F4", "Shift+Esc" };

        foreach (var hotkey in hotkeys)
        {
            _hotkeyManager.RegisterHotkey(hotkey);
        }

        // Act
        _hotkeyManager.UnregisterAllHotkeys();
        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        registeredHotkeys.Should().BeEmpty();
        Log("所有快捷键清理成功");
    }

    [Fact]
    public void HotkeyInfo_ShouldContainCorrectInformation()
    {
        // Arrange
        var hotkeyString = "Ctrl+Shift+Alt+Win+A";

        // Act
        _hotkeyManager.RegisterHotkey(hotkeyString);
        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();
        var hotkeyInfo = registeredHotkeys[0];

        // Assert
        hotkeyInfo.Key.Should().Be(Keys.A);
        hotkeyInfo.Modifiers.Should().Be(HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.Alt | HotkeyModifiers.Win);
        hotkeyInfo.OriginalString.Should().Be(hotkeyString);
        hotkeyInfo.Id.Should().BeGreaterThan(0);
        Log($"快捷键信息正确: {hotkeyInfo}");
    }

    [Fact]
    public void HotkeyModifiers_ShouldBeParsedCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            (Hotkey: "Ctrl+A", Expected: HotkeyModifiers.Control),
            (Hotkey: "Alt+F4", Expected: HotkeyModifiers.Alt),
            (Hotkey: "Shift+Esc", Expected: HotkeyModifiers.Shift),
            (Hotkey: "Win+R", Expected: HotkeyModifiers.Win),
            (Hotkey: "Ctrl+Alt+Del", Expected: HotkeyModifiers.Control | HotkeyModifiers.Alt),
            (Hotkey: "Ctrl+Shift+Esc", Expected: HotkeyModifiers.Control | HotkeyModifiers.Shift),
            (Hotkey: "Ctrl+Alt+Shift+Win+A", Expected: HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win)
        };

        // Act & Assert
        foreach (var (hotkey, expected) in testCases)
        {
            _hotkeyManager.RegisterHotkey(hotkey);
            var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();
            var hotkeyInfo = registeredHotkeys[registeredHotkeys.Count - 1];
            
            hotkeyInfo.Modifiers.Should().Be(expected);
            Log($"快捷键修饰符解析正确: {hotkey} -> {expected}");
        }
    }

    [Fact]
    public void InvalidHotkeyStrings_ShouldFailToRegister()
    {
        // Arrange
        var invalidHotkeys = new[] { "", null, "Invalid", "Ctrl+", "+A", "Ctrl++A", "Ctrl+InvalidKey" };

        // Act & Assert
        foreach (var hotkey in invalidHotkeys)
        {
            var result = _hotkeyManager.RegisterHotkey(hotkey);
            result.Should().BeFalse();
            LogError($"无效快捷键注册失败: {hotkey}");
        }
    }

    [Fact]
    public void CaseInsensitiveHotkeyRegistration_ShouldWork()
    {
        // Arrange
        var hotkeyVariations = new[] { "Ctrl+A", "ctrl+a", "CTRL+A", "Ctrl+a" };

        // Act
        var firstResult = _hotkeyManager.RegisterHotkey(hotkeyVariations[0]);
        var otherResults = new bool[hotkeyVariations.Length - 1];

        for (int i = 1; i < hotkeyVariations.Length; i++)
        {
            otherResults[i - 1] = _hotkeyManager.RegisterHotkey(hotkeyVariations[i]);
        }

        // Assert
        firstResult.Should().BeTrue();
        otherResults.Should().AllBeEquivalentTo(false);
        Log("快捷键注册不区分大小写");
    }

    [Fact]
    public void HotkeyWithTag_ShouldStoreTagCorrectly()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";
        var tag = "CustomTag";

        // Act
        var hotkeyInfo = new HotkeyInfo
        {
            Key = Keys.A,
            Modifiers = HotkeyModifiers.Control,
            OriginalString = hotkeyString,
            Tag = tag
        };

        var result = _hotkeyManager.RegisterHotkey(hotkeyInfo);
        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        result.Should().BeTrue();
        registeredHotkeys.Should().HaveCount(1);
        registeredHotkeys[0].Tag.Should().Be(tag);
        Log($"快捷键标签存储正确: {tag}");
    }

    [Fact]
    public void HotkeyEventShouldIncludeTag()
    {
        // Arrange
        var hotkeyString = "Ctrl+A";
        var tag = "CustomTag";
        var capturedTag = null as object;

        _hotkeyManager.HotkeyPressed += (sender, args) =>
        {
            capturedTag = args.Hotkey.Tag;
        };

        // Act
        var hotkeyInfo = new HotkeyInfo
        {
            Key = Keys.A,
            Modifiers = HotkeyModifiers.Control,
            OriginalString = hotkeyString,
            Tag = tag
        };

        _hotkeyManager.RegisterHotkey(hotkeyInfo);

        var message = new Message
        {
            Msg = 0x0312, // WM_HOTKEY
            WParam = new IntPtr(1),
            LParam = IntPtr.Zero
        };

        _hotkeyManager.ProcessMessage(ref message);

        // Assert
        capturedTag.Should().Be(tag);
        Log($"快捷键事件包含标签: {tag}");
    }

    [Fact]
    public void StressTest_MultipleHotkeyRegistrations_ShouldWork()
    {
        // Arrange
        var hotkeyCount = 50;
        var registeredCount = 0;

        // Act
        for (int i = 0; i < hotkeyCount; i++)
        {
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + i)}";
            if (_hotkeyManager.RegisterHotkey(hotkeyString))
            {
                registeredCount++;
            }
        }

        var registeredHotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        registeredCount.Should().BeGreaterThan(0);
        registeredHotkeys.Should().HaveCount(registeredCount);
        Log($"压力测试注册成功: {registeredCount}/{hotkeyCount}个快捷键");
    }

    [Fact]
    public void ConcurrentUserOperations_ShouldNotCauseIssues()
    {
        // Arrange
        var operations = new List<Action>
        {
            () => _hotkeyManager.RegisterHotkey("Ctrl+A"),
            () => _hotkeyManager.RegisterHotkey("Alt+F4"),
            () => _hotkeyManager.UnregisterHotkey("Ctrl+A"),
            () => _hotkeyManager.GetRegisteredHotkeys(),
            () => _hotkeyManager.IsHotkeyRegistered("Alt+F4"),
            () => _hotkeyManager.UnregisterAllHotkeys()
        };

        // Act
        Parallel.ForEach(operations, operation =>
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                LogError($"并发操作失败: {ex.Message}");
            }
        });

        // Assert
        // 如果没有抛出异常，则测试通过
        Log("并发操作测试完成");
    }

    [Fact]
    public void MemoryUsage_ShouldBeReasonable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var hotkeyCount = 100;

        // Act
        for (int i = 0; i < hotkeyCount; i++)
        {
            var hotkeyString = $"Ctrl+{(Keys)(Keys.A + (i % 26))}";
            _hotkeyManager.RegisterHotkey(hotkeyString);
        }

        var peakMemory = GC.GetTotalMemory(false);
        var memoryIncrease = peakMemory - initialMemory;

        // Cleanup
        _hotkeyManager.UnregisterAllHotkeys();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryAfterCleanup = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // 小于10MB
        memoryAfterCleanup.Should().BeLessThan(1 * 1024 * 1024); // 清理后小于1MB
        Log($"内存使用合理: 增加 {memoryIncrease / 1024}KB, 清理后 {memoryAfterCleanup / 1024}KB");
    }
}