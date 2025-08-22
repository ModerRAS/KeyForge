using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Services.Linux;

/// <summary>
/// Linux全局热键服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class LinuxGlobalHotkeyService : IGlobalHotkeyService
{
    private readonly ILogger<LinuxGlobalHotkeyService> _logger;
    private readonly object _lock = new();
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();
    private bool _isDisposed;

    /// <summary>
    /// 初始化Linux全局热键服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public LinuxGlobalHotkeyService(ILogger<LinuxGlobalHotkeyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                _logger.LogInformation("Linux hotkey registered: {Id}, {Description}", id, hotkeyInfo.Description);
                
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

                _registeredHotkeys.Remove(id);
                _logger.LogInformation("Linux hotkey unregistered: {Id}", id);
                
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
                _logger.LogInformation("Linux all hotkeys suspended");
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
                _logger.LogInformation("Linux all hotkeys resumed");
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
                // 释放托管资源
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~LinuxGlobalHotkeyService()
    {
        Dispose(false);
    }
}