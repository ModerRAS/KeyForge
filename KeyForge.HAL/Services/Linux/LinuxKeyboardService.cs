using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Services.Linux;

/// <summary>
/// Linux键盘服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class LinuxKeyboardService : IKeyboardService
{
    private readonly ILogger<LinuxKeyboardService> _logger;
    private readonly object _lock = new();
    private bool _isDisposed;

    /// <summary>
    /// 初始化Linux键盘服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public LinuxKeyboardService(ILogger<LinuxKeyboardService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 键盘事件
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyEvent;

    /// <summary>
    /// 模拟按键按下
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    public async Task<bool> KeyDownAsync(KeyCode key)
    {
        try
        {
            lock (_lock)
            {
                // 简化实现 - 记录日志
                _logger.LogDebug("Linux key down: {Key}", key);
                OnKeyEvent(key, KeyState.Down);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to press key down: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 模拟按键释放
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    public async Task<bool> KeyUpAsync(KeyCode key)
    {
        try
        {
            lock (_lock)
            {
                // 简化实现 - 记录日志
                _logger.LogDebug("Linux key up: {Key}", key);
                OnKeyEvent(key, KeyState.Up);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 模拟按键点击（按下+释放）
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    public async Task<bool> KeyPressAsync(KeyCode key)
    {
        try
        {
            var downResult = await KeyDownAsync(key);
            await Task.Delay(10); // 短暂延迟
            var upResult = await KeyUpAsync(key);
            
            return downResult && upResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to press key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 模拟文本输入
    /// </summary>
    /// <param name="text">要输入的文本</param>
    /// <param name="delay">字符间延迟（毫秒）</param>
    /// <returns>是否成功</returns>
    public async Task<bool> TypeTextAsync(string text, int delay = 50)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            _logger.LogDebug("Linux typing text: {Text} with delay: {Delay}ms", text, delay);

            foreach (var character in text)
            {
                var keyCode = GetKeyCodeFromChar(character);
                if (keyCode != KeyCode.A) // 默认值，表示未找到
                {
                    await KeyPressAsync(keyCode);
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogWarning("Cannot find key code for character: {Character}", character);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to type text: {Text}", text);
            return false;
        }
    }

    /// <summary>
    /// 模拟组合键
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SendHotkeyAsync(KeyCode[] modifiers, KeyCode key)
    {
        try
        {
            _logger.LogDebug("Linux sending hotkey: {Modifiers} + {Key}", 
                string.Join("+", modifiers.Select(m => m.ToString())), key);

            // 按下所有修饰键
            foreach (var modifier in modifiers)
            {
                await KeyDownAsync(modifier);
            }

            await Task.Delay(10); // 短暂延迟

            // 按下主键
            await KeyDownAsync(key);
            await Task.Delay(10);
            await KeyUpAsync(key);

            // 释放所有修饰键
            foreach (var modifier in modifiers.Reverse())
            {
                await KeyUpAsync(modifier);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send hotkey: {Modifiers} + {Key}", 
                string.Join("+", modifiers.Select(m => m.ToString())), key);
            return false;
        }
    }

    /// <summary>
    /// 获取当前按键状态
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>按键状态</returns>
    public KeyState GetKeyState(KeyCode key)
    {
        try
        {
            // 简化实现 - 返回默认状态
            _logger.LogDebug("Linux get key state: {Key}", key);
            return KeyState.Up;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get key state: {Key}", key);
            return KeyState.Unknown;
        }
    }

    /// <summary>
    /// 检查按键是否可用
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否可用</returns>
    public bool IsKeyAvailable(KeyCode key)
    {
        try
        {
            // 简化实现 - 检查是否在支持的范围内
            return key >= KeyCode.A && key <= KeyCode.NumPadDivide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check key availability: {Key}", key);
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
    ~LinuxKeyboardService()
    {
        Dispose(false);
    }

    /// <summary>
    /// 从字符获取按键代码
    /// </summary>
    /// <param name="character">字符</param>
    /// <returns>按键代码</returns>
    private KeyCode GetKeyCodeFromChar(char character)
    {
        // 简化实现 - 只支持基本字符
        if (character >= 'A' && character <= 'Z')
        {
            return KeyCode.A + (character - 'A');
        }
        else if (character >= 'a' && character <= 'z')
        {
            return KeyCode.A + (character - 'a');
        }
        else if (character >= '0' && character <= '9')
        {
            return KeyCode.D0 + (character - '0');
        }
        else if (character == ' ')
        {
            return KeyCode.Space;
        }
        else if (character == '\n' || character == '\r')
        {
            return KeyCode.Enter;
        }
        else if (character == '\t')
        {
            return KeyCode.Tab;
        }
        else if (character == '\b')
        {
            return KeyCode.Backspace;
        }
        else
        {
            return KeyCode.A; // 默认值
        }
    }

    /// <summary>
    /// 触发键盘事件
    /// </summary>
    /// <param name="keyCode">按键代码</param>
    /// <param name="keyState">按键状态</param>
    private void OnKeyEvent(KeyCode keyCode, KeyState keyState)
    {
        try
        {
            KeyEvent?.Invoke(this, new KeyboardEventArgs
            {
                KeyCode = keyCode,
                KeyState = keyState,
                Timestamp = DateTime.UtcNow,
                IsRepeat = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to raise keyboard event");
        }
    }
}