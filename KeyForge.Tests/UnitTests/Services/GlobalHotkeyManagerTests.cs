using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Services;

/// <summary>
/// GlobalHotkeyManager 单元测试
/// 原本实现：完整的快捷键管理器测试
/// 简化实现：核心快捷键功能测试
/// </summary>
public class GlobalHotkeyManagerTests : TestBase
{
    private readonly Mock<IWindowsApiService> _mockWindowsApi;
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly IntPtr _testWindowHandle = new IntPtr(12345);

    public GlobalHotkeyManagerTests(ITestOutputHelper output) : base(output)
    {
        _mockWindowsApi = new Mock<IWindowsApiService>();
        _hotkeyManager = new GlobalHotkeyManager(_testWindowHandle, _mockWindowsApi.Object);
    }

    [Fact]
    public void Constructor_WithValidWindowHandle_ShouldInitialize()
    {
        // Arrange & Act
        var manager = new GlobalHotkeyManager(_testWindowHandle);

        // Assert
        manager.Should().NotBeNull();
        Log("GlobalHotkeyManager 构造成功");
    }

    [Fact]
    public void RegisterHotkey_WithValidString_ShouldReturnTrue()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        var result = _hotkeyManager.RegisterHotkey("Ctrl+Shift+A");

        // Assert
        result.Should().BeTrue();
        _mockWindowsApi.Verify(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        Log("快捷键注册成功: Ctrl+Shift+A");
    }

    [Fact]
    public void RegisterHotkey_WithNullString_ShouldReturnFalse()
    {
        // Arrange
        string hotkeyString = null;

        // Act
        var result = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert
        result.Should().BeFalse();
        LogError("空快捷键字符串注册失败");
    }

    [Fact]
    public void RegisterHotkey_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        string hotkeyString = "";

        // Act
        var result = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert
        result.Should().BeFalse();
        LogError("空快捷键字符串注册失败");
    }

    [Fact]
    public void RegisterHotkey_WithInvalidString_ShouldReturnFalse()
    {
        // Arrange
        string hotkeyString = "InvalidHotkey";

        // Act
        var result = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert
        result.Should().BeFalse();
        LogError("无效快捷键字符串注册失败");
    }

    [Theory]
    [InlineData("Ctrl+A")]
    [InlineData("Alt+F4")]
    [InlineData("Shift+Esc")]
    [InlineData("Win+R")]
    [InlineData("Ctrl+Alt+Del")]
    public void RegisterHotkey_WithValidCombinations_ShouldReturnTrue(string hotkeyString)
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        var result = _hotkeyManager.RegisterHotkey(hotkeyString);

        // Assert
        result.Should().BeTrue();
        Log($"快捷键注册成功: {hotkeyString}");
    }

    [Fact]
    public void RegisterHotkey_WithDuplicateHotkey_ShouldReturnFalse()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        var firstResult = _hotkeyManager.RegisterHotkey("Ctrl+A");
        var secondResult = _hotkeyManager.RegisterHotkey("Ctrl+A");

        // Assert
        firstResult.Should().BeTrue();
        secondResult.Should().BeFalse();
        LogError("重复快捷键注册失败");
    }

    [Fact]
    public void RegisterHotkey_WithApiFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(false);

        // Act
        var result = _hotkeyManager.RegisterHotkey("Ctrl+A");

        // Assert
        result.Should().BeFalse();
        LogError("API调用失败时快捷键注册失败");
    }

    [Fact]
    public void UnregisterHotkey_WithExistingHotkey_ShouldReturnTrue()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);
        _mockWindowsApi.Setup(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()))
            .Returns(true);

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        var result = _hotkeyManager.UnregisterHotkey("Ctrl+A");

        // Assert
        result.Should().BeTrue();
        _mockWindowsApi.Verify(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()), Times.Once);
        Log("快捷键注销成功: Ctrl+A");
    }

    [Fact]
    public void UnregisterHotkey_WithNonExistingHotkey_ShouldReturnFalse()
    {
        // Act
        var result = _hotkeyManager.UnregisterHotkey("Ctrl+A");

        // Assert
        result.Should().BeFalse();
        LogError("不存在的快捷键注销失败");
    }

    [Fact]
    public void UnregisterAllHotkeys_WithRegisteredHotkeys_ShouldUnregisterAll()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);
        _mockWindowsApi.Setup(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()))
            .Returns(true);

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        _hotkeyManager.RegisterHotkey("Alt+F4");
        _hotkeyManager.UnregisterAllHotkeys();

        // Assert
        _mockWindowsApi.Verify(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()), Times.Exactly(2));
        Log("所有快捷键注销成功");
    }

    [Fact]
    public void GetRegisteredHotkeys_WithNoHotkeys_ShouldReturnEmptyArray()
    {
        // Act
        var hotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        hotkeys.Should().BeEmpty();
        Log("无注册快捷键时返回空数组");
    }

    [Fact]
    public void GetRegisteredHotkeys_WithRegisteredHotkeys_ShouldReturnAllHotkeys()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        _hotkeyManager.RegisterHotkey("Alt+F4");
        var hotkeys = _hotkeyManager.GetRegisteredHotkeys();

        // Assert
        hotkeys.Should().HaveCount(2);
        hotkeys.Should().Contain(h => h.OriginalString == "Ctrl+A");
        hotkeys.Should().Contain(h => h.OriginalString == "Alt+F4");
        Log($"获取到 {hotkeys.Length} 个注册快捷键");
    }

    [Fact]
    public void IsHotkeyRegistered_WithExistingHotkey_ShouldReturnTrue()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        var result = _hotkeyManager.IsHotkeyRegistered("Ctrl+A");

        // Assert
        result.Should().BeTrue();
        Log("快捷键已注册检查成功");
    }

    [Fact]
    public void IsHotkeyRegistered_WithNonExistingHotkey_ShouldReturnFalse()
    {
        // Act
        var result = _hotkeyManager.IsHotkeyRegistered("Ctrl+A");

        // Assert
        result.Should().BeFalse();
        Log("快捷键未注册检查成功");
    }

    [Fact]
    public void ProcessMessage_WithHotkeyMessage_ShouldTriggerEvent()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        var eventTriggered = false;
        _hotkeyManager.HotkeyPressed += (sender, args) => eventTriggered = true;

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        var message = new Message
        {
            Msg = 0x0312, // WM_HOTKEY
            WParam = new IntPtr(1)
        };
        _hotkeyManager.ProcessMessage(ref message);

        // Assert
        eventTriggered.Should().BeTrue();
        Log("快捷键消息处理成功");
    }

    [Fact]
    public void ProcessMessage_WithNonHotkeyMessage_ShouldNotTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _hotkeyManager.HotkeyPressed += (sender, args) => eventTriggered = true;

        // Act
        var message = new Message
        {
            Msg = 0x0001, // WM_CREATE
            WParam = new IntPtr(1)
        };
        _hotkeyManager.ProcessMessage(ref message);

        // Assert
        eventTriggered.Should().BeFalse();
        Log("非快捷键消息不触发事件");
    }

    [Fact]
    public void Dispose_WithRegisteredHotkeys_ShouldUnregisterAll()
    {
        // Arrange
        _mockWindowsApi.Setup(x => x.RegisterHotKey(_testWindowHandle, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);
        _mockWindowsApi.Setup(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()))
            .Returns(true);

        // Act
        _hotkeyManager.RegisterHotkey("Ctrl+A");
        _hotkeyManager.Dispose();

        // Assert
        _mockWindowsApi.Verify(x => x.UnregisterHotKey(_testWindowHandle, It.IsAny<int>()), Times.Once);
        Log("Dispose时自动注销所有快捷键");
    }

    [Fact]
    public void HotkeyInfo_ToString_ShouldReturnOriginalString()
    {
        // Arrange
        var hotkeyInfo = new HotkeyInfo
        {
            Key = Keys.A,
            Modifiers = HotkeyModifiers.Control,
            OriginalString = "Ctrl+A"
        };

        // Act
        var result = hotkeyInfo.ToString();

        // Assert
        result.Should().Be("Ctrl+A");
        Log("HotkeyInfo ToString 返回原始字符串");
    }

    [Fact]
    public void HotkeyInfo_ToString_WithNoOriginalString_ShouldReturnGeneratedString()
    {
        // Arrange
        var hotkeyInfo = new HotkeyInfo
        {
            Key = Keys.A,
            Modifiers = HotkeyModifiers.Control,
            OriginalString = null
        };

        // Act
        var result = hotkeyInfo.ToString();

        // Assert
        result.Should().Be("Ctrl+A");
        Log("HotkeyInfo ToString 生成显示字符串");
    }
}

/// <summary>
/// Windows API 服务接口（用于测试）
/// </summary>
public interface IWindowsApiService
{
    bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    bool UnregisterHotKey(IntPtr hWnd, int id);
}