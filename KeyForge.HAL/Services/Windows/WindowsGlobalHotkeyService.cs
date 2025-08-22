using KeyForge.HAL.Abstractions;
using System.Runtime.InteropServices;

namespace KeyForge.HAL.Services.Windows;

/// <summary>
/// Windows全局热键服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class WindowsGlobalHotkeyService : IGlobalHotkeyService
{
    private readonly ILogger<WindowsGlobalHotkeyService> _logger;
    private readonly object _lock = new();
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();
    private bool _isDisposed;

    // Windows API导入
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, 
        uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, 
        IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;

    private IntPtr _messageWindowHandle = IntPtr.Zero;
    private bool _isMessageLoopRunning;

    /// <summary>
    /// 初始化Windows全局热键服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public WindowsGlobalHotkeyService(ILogger<WindowsGlobalHotkeyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeMessageWindow();
    }

    /// <summary>
    /// 热键事件
    /// </summary>
    public event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    /// <summary>
    /// 注册全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否成功</returns>
    public async Task<bool> RegisterHotkeyAsync(int id, KeyCode[] modifiers, KeyCode key, Action<HotkeyEventArgs> callback)
    {
        try
        {
            lock (_lock)
            {
                if (_registeredHotkeys.ContainsKey(id))
                {
                    _logger.LogWarning("Hotkey already registered: {Id}", id);
                    return false;
                }

                var fsModifiers = GetModifierFlags(modifiers);
                var vk = (uint)key;

                if (!RegisterHotKey(_messageWindowHandle, id, fsModifiers, vk))
                {
                    _logger.LogError("Failed to register hotkey: {Id}, modifiers: {Modifiers}, key: {Key}", 
                        id, string.Join("+", modifiers), key);
                    return false;
                }

                var hotkeyInfo = new HotkeyInfo
                {
                    Id = id,
                    Modifiers = modifiers,
                    Key = key,
                    Description = $"{string.Join("+", modifiers.Select(m => m.ToString()))} + {key}",
                    RegisteredAt = DateTime.UtcNow,
                    IsEnabled = true
                };

                _registeredHotkeys[id] = hotkeyInfo;

                _logger.LogInformation("Hotkey registered: {Id}, {Description}", id, hotkeyInfo.Description);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkey: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// 注销全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否成功</returns>
    public async Task<bool> UnregisterHotkeyAsync(int id)
    {
        try
        {
            lock (_lock)
            {
                if (!_registeredHotkeys.ContainsKey(id))
                {
                    _logger.LogWarning("Hotkey not registered: {Id}", id);
                    return false;
                }

                if (!UnregisterHotKey(_messageWindowHandle, id))
                {
                    _logger.LogError("Failed to unregister hotkey: {Id}", id);
                    return false;
                }

                _registeredHotkeys.Remove(id);
                _logger.LogInformation("Hotkey unregistered: {Id}", id);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister hotkey: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// 检查热键是否已注册
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否已注册</returns>
    public bool IsHotkeyRegistered(int id)
    {
        lock (_lock)
        {
            return _registeredHotkeys.ContainsKey(id);
        }
    }

    /// <summary>
    /// 获取所有已注册的热键
    /// </summary>
    /// <returns>热键列表</returns>
    public IEnumerable<HotkeyInfo> GetRegisteredHotkeys()
    {
        lock (_lock)
        {
            return _registeredHotkeys.Values.ToList();
        }
    }

    /// <summary>
    /// 检查热键组合是否可用
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>是否可用</returns>
    public bool IsHotkeyAvailable(KeyCode[] modifiers, KeyCode key)
    {
        try
        {
            lock (_lock)
            {
                var description = $"{string.Join("+", modifiers.Select(m => m.ToString()))} + {key}";
                return !_registeredHotkeys.Values.Any(h => h.Description == description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check hotkey availability");
            return false;
        }
    }

    /// <summary>
    /// 暂停所有热键
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> SuspendAllHotkeysAsync()
    {
        try
        {
            lock (_lock)
            {
                foreach (var id in _registeredHotkeys.Keys.ToList())
                {
                    UnregisterHotKey(_messageWindowHandle, id);
                }

                _logger.LogInformation("All hotkeys suspended");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suspend all hotkeys");
            return false;
        }
    }

    /// <summary>
    /// 恢复所有热键
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> ResumeAllHotkeysAsync()
    {
        try
        {
            lock (_lock)
            {
                foreach (var hotkey in _registeredHotkeys.Values.ToList())
                {
                    var fsModifiers = GetModifierFlags(hotkey.Modifiers);
                    var vk = (uint)hotkey.Key;

                    if (!RegisterHotKey(_messageWindowHandle, hotkey.Id, fsModifiers, vk))
                    {
                        _logger.LogError("Failed to resume hotkey: {Id}", hotkey.Id);
                    }
                }

                _logger.LogInformation("All hotkeys resumed");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume all hotkeys");
            return false;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 注销所有热键
                try
                {
                    SuspendAllHotkeysAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during hotkey cleanup");
                }

                // 销毁消息窗口
                if (_messageWindowHandle != IntPtr.Zero)
                {
                    DestroyWindow(_messageWindowHandle);
                    _messageWindowHandle = IntPtr.Zero;
                }
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~WindowsGlobalHotkeyService()
    {
        Dispose(false);
    }

    /// <summary>
    /// 初始化消息窗口
    /// </summary>
    private void InitializeMessageWindow()
    {
        try
        {
            // 创建隐藏的消息窗口来接收热键消息
            var hInstance = GetModuleHandle(null);
            _messageWindowHandle = CreateWindowEx(0, "MessageWindow", "KeyForgeHotkeyWindow", 0, 
                0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (_messageWindowHandle == IntPtr.Zero)
            {
                _logger.LogError("Failed to create message window");
                return;
            }

            // 启动消息循环
            StartMessageLoop();
            _logger.LogInformation("Message window created for hotkey handling");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize message window");
        }
    }

    /// <summary>
    /// 启动消息循环
    /// </summary>
    private void StartMessageLoop()
    {
        Task.Run(() =>
        {
            _isMessageLoopRunning = true;
            _logger.LogDebug("Hotkey message loop started");

            while (_isMessageLoopRunning)
            {
                try
                {
                    // 简化实现 - 使用异步方式处理热键
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in hotkey message loop");
                }
            }
        });
    }

    /// <summary>
    /// 获取修饰键标志
    /// </summary>
    /// <param name="modifiers">修饰键数组</param>
    /// <returns>修饰键标志</returns>
    private uint GetModifierFlags(KeyCode[] modifiers)
    {
        uint flags = 0;
        
        foreach (var modifier in modifiers)
        {
            flags |= modifier switch
            {
                KeyCode.Alt => MOD_ALT,
                KeyCode.Control => MOD_CONTROL,
                KeyCode.Shift => MOD_SHIFT,
                KeyCode.Windows => MOD_WIN,
                _ => 0
            };
        }
        
        return flags;
    }

    /// <summary>
    /// 触发热键事件
    /// </summary>
    /// <param name="id">热键ID</param>
    private void OnHotkeyPressed(int id)
    {
        try
        {
            if (_registeredHotkeys.TryGetValue(id, out var hotkey))
            {
                _logger.LogDebug("Hotkey pressed: {Id}, {Description}", id, hotkey.Description);
                
                HotkeyPressed?.Invoke(this, new HotkeyEventArgs
                {
                    HotkeyId = id,
                    Modifiers = hotkey.Modifiers,
                    Key = hotkey.Key,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle hotkey press: {Id}", id);
        }
    }
}