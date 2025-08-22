using KeyForge.HAL.Abstractions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyForge.HAL.Services.Windows;

/// <summary>
/// Windows键盘服务实现
/// 增强实现版本，包含完整的性能监控、错误处理和优化功能
/// </summary>
public class WindowsKeyboardService : IKeyboardService, IDisposable
{
    private readonly ILogger<WindowsKeyboardService> _logger;
    private readonly object _lock = new();
    private readonly PerformanceTracker _performanceTracker;
    private readonly ErrorTracker _errorTracker;
    private bool _isDisposed;

    // Windows API导入
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, 
        [Out] StringBuilder pwszBuff, int cchBuff, uint wFlags);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    private const int KEYEVENTF_KEYDOWN = 0x0000;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const int KEYEVENTF_UNICODE = 0x0004;
    private const uint MAPVK_VK_TO_CHAR = 2;
    private const int INPUT_KEYBOARD = 1;

    /// <summary>
    /// 初始化Windows键盘服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public WindowsKeyboardService(ILogger<WindowsKeyboardService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceTracker = new PerformanceTracker();
        _errorTracker = new ErrorTracker();
        
        InitializeKeyboardLayout();
    }

    /// <summary>
    /// 键盘事件
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyEvent;

    /// <summary>
    /// 键盘布局信息
    /// </summary>
    public string KeyboardLayout { get; private set; } = string.Empty;

    /// <summary>
    /// 服务是否已初始化
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 获取键盘服务统计信息
    /// </summary>
    public KeyboardServiceStats Stats => _performanceTracker.GetStats();

    /// <summary>
    /// 模拟按键按下
    /// </summary>
    /// <param name="key">按键代码</param>
    /// <returns>是否成功</returns>
    public async Task<bool> KeyDownAsync(KeyCode key)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            if (!IsInitialized)
            {
                _logger.LogWarning("Keyboard service not initialized");
                return false;
            }

            lock (_lock)
            {
                var virtualKey = (byte)key;
                var scanCode = MapVirtualKey(virtualKey, 0);
                
                // 使用SendInput替代keybd_event以获得更好的性能和兼容性
                SendKeyInput(virtualKey, scanCode, KEYEVENTF_KEYDOWN);
                
                _logger.LogDebug("Key down: {Key}, ScanCode: {ScanCode}", key, scanCode);
                OnKeyEvent(key, KeyState.Down);
                
                _performanceTracker.RecordSuccess(startTime);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"KeyDownAsync({key})");
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            if (!IsInitialized)
            {
                _logger.LogWarning("Keyboard service not initialized");
                return false;
            }

            lock (_lock)
            {
                var virtualKey = (byte)key;
                var scanCode = MapVirtualKey(virtualKey, 0);
                
                SendKeyInput(virtualKey, scanCode, KEYEVENTF_KEYUP);
                
                _logger.LogDebug("Key up: {Key}, ScanCode: {ScanCode}", key, scanCode);
                OnKeyEvent(key, KeyState.Up);
                
                _performanceTracker.RecordSuccess(startTime);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"KeyUpAsync({key})");
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            var downResult = await KeyDownAsync(key);
            await Task.Delay(10); // 短暂延迟以确保按键被正确识别
            var upResult = await KeyUpAsync(key);
            
            if (downResult && upResult)
            {
                _performanceTracker.RecordSuccess(startTime);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"KeyPressAsync({key})");
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (!IsInitialized)
            {
                _logger.LogWarning("Keyboard service not initialized");
                return false;
            }

            _logger.LogDebug("Typing text: {Text} with delay: {Delay}ms", text, delay);

            var successCount = 0;
            var totalCount = 0;

            foreach (var character in text)
            {
                totalCount++;
                var keyCode = GetKeyCodeFromChar(character);
                if (keyCode != KeyCode.Unknown)
                {
                    var result = await KeyPressAsync(keyCode);
                    if (result)
                    {
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to type character: {Character}", character);
                    }
                    
                    await Task.Delay(delay);
                }
                else
                {
                    // 尝试使用Unicode输入
                    if (await TypeUnicodeCharAsync(character))
                    {
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Cannot find key code for character: {Character}", character);
                    }
                }
            }

            var successRate = totalCount > 0 ? (double)successCount / totalCount : 0;
            _performanceTracker.RecordTextTyping(startTime, text.Length, successRate);

            return successRate >= 0.9; // 90%成功率
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"TypeTextAsync({text})");
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            if (!IsInitialized)
            {
                _logger.LogWarning("Keyboard service not initialized");
                return false;
            }

            _logger.LogDebug("Sending hotkey: {Modifiers} + {Key}", 
                string.Join("+", modifiers.Select(m => m.ToString())), key);

            // 按下所有修饰键
            foreach (var modifier in modifiers)
            {
                if (!await KeyDownAsync(modifier))
                {
                    _logger.LogWarning("Failed to press modifier: {Modifier}", modifier);
                    return false;
                }
            }

            await Task.Delay(10); // 短暂延迟

            // 按下主键
            if (!await KeyDownAsync(key))
            {
                _logger.LogWarning("Failed to press main key: {Key}", key);
                return false;
            }

            await Task.Delay(10);
            
            if (!await KeyUpAsync(key))
            {
                _logger.LogWarning("Failed to release main key: {Key}", key);
                return false;
            }

            // 释放所有修饰键
            foreach (var modifier in modifiers.Reverse())
            {
                if (!await KeyUpAsync(modifier))
                {
                    _logger.LogWarning("Failed to release modifier: {Modifier}", modifier);
                    return false;
                }
            }

            _performanceTracker.RecordSuccess(startTime);
            return true;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"SendHotkeyAsync({string.Join("+", modifiers)}+{key})");
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
            var state = GetAsyncKeyState((int)key);
            return (state & 0x8000) != 0 ? KeyState.Down : KeyState.Up;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"GetKeyState({key})");
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
            // 检查按键是否在支持的范围内
            if (key < KeyCode.A || key > KeyCode.NumPadDivide)
            {
                return false;
            }

            // 检查按键是否可以被映射
            var virtualKey = (byte)key;
            var scanCode = MapVirtualKey(virtualKey, 0);
            return scanCode != 0;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"IsKeyAvailable({key})");
            _logger.LogError(ex, "Failed to check key availability: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 获取当前键盘状态
    /// </summary>
    /// <returns>键盘状态数组</returns>
    public async Task<byte[]> GetKeyboardStateAsync()
    {
        try
        {
            var keyState = new byte[256];
            if (GetKeyboardState(keyState))
            {
                return keyState;
            }
            
            _logger.LogWarning("Failed to get keyboard state");
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, "GetKeyboardStateAsync");
            _logger.LogError(ex, "Failed to get keyboard state");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 从字符获取按键代码
    /// </summary>
    /// <param name="character">字符</param>
    /// <returns>按键代码</returns>
    private KeyCode GetKeyCodeFromChar(char character)
    {
        // 支持更多字符映射
        return character switch
        {
            >= 'A' and <= 'Z' => KeyCode.A + (character - 'A'),
            >= 'a' and <= 'z' => KeyCode.A + (character - 'a'),
            >= '0' and <= '9' => KeyCode.D0 + (character - '0'),
            ' ' => KeyCode.Space,
            '\n' or '\r' => KeyCode.Enter,
            '\t' => KeyCode.Tab,
            '\b' => KeyCode.Backspace,
            '!' => KeyCode.D1,
            '@' => KeyCode.D2,
            '#' => KeyCode.D3,
            '$' => KeyCode.D4,
            '%' => KeyCode.D5,
            '^' => KeyCode.D6,
            '&' => KeyCode.D7,
            '*' => KeyCode.D8,
            '(' => KeyCode.D9,
            ')' => KeyCode.D0,
            '-' => KeyCode.OemMinus,
            '=' => KeyCode.Oemplus,
            '[' => KeyCode.Oem4,
            ']' => KeyCode.Oem6,
            '\\' => KeyCode.Oem5,
            ';' => KeyCode.Oem1,
            '\'' => KeyCode.Oem7,
            ',' => KeyCode.Oemcomma,
            '.' => KeyCode.OemPeriod,
            '/' => KeyCode.Oem2,
            '`' => KeyCode.Oem3,
            _ => KeyCode.Unknown
        };
    }

    /// <summary>
    /// 输入Unicode字符
    /// </summary>
    /// <param name="character">字符</param>
    /// <returns>是否成功</returns>
    private async Task<bool> TypeUnicodeCharAsync(char character)
    {
        try
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)character,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));
            
            // 释放按键
            input.u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
            SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));

            return true;
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, $"TypeUnicodeCharAsync({character})");
            _logger.LogError(ex, "Failed to type unicode character: {Character}", character);
            return false;
        }
    }

    /// <summary>
    /// 发送按键输入
    /// </summary>
    /// <param name="virtualKey">虚拟键码</param>
    /// <param name="scanCode">扫描码</param>
    /// <param name="flags">标志</param>
    private void SendKeyInput(byte virtualKey, uint scanCode, uint flags)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = (ushort)scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));
    }

    /// <summary>
    /// 初始化键盘布局
    /// </summary>
    private void InitializeKeyboardLayout()
    {
        try
        {
            var layoutName = new StringBuilder(8);
            if (GetKeyboardLayoutName(layoutName))
            {
                KeyboardLayout = layoutName.ToString();
                _logger.LogDebug("Keyboard layout: {Layout}", KeyboardLayout);
            }
            
            IsInitialized = true;
            _logger.LogInformation("Windows keyboard service initialized successfully");
        }
        catch (Exception ex)
        {
            _errorTracker.RecordError(ex, "InitializeKeyboardLayout");
            _logger.LogError(ex, "Failed to initialize keyboard layout");
            IsInitialized = false;
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
            _errorTracker.RecordError(ex, "OnKeyEvent");
            _logger.LogError(ex, "Failed to raise keyboard event");
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
                _performanceTracker.Dispose();
                _errorTracker.Dispose();
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~WindowsKeyboardService()
    {
        Dispose(false);
    }
}

/// <summary>
/// 键盘服务统计信息
/// </summary>
public record KeyboardServiceStats
{
    /// <summary>
    /// 总操作次数
    /// </summary>
    public long TotalOperations { get; init; }

    /// <summary>
    /// 成功操作次数
    /// </summary>
    public long SuccessCount { get; init; }

    /// <summary>
    /// 失败操作次数
    /// </summary>
    public long FailureCount { get; init; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; init; }

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; init; }

    /// <summary>
    /// 文本输入统计
    /// </summary>
    public TextTypingStats TextTyping { get; init; } = new();

    /// <summary>
    /// 错误统计
    /// </summary>
    public ErrorStats Errors { get; init; } = new();
}

/// <summary>
/// 文本输入统计
/// </summary>
public record TextTypingStats
{
    /// <summary>
    /// 总字符数
    /// </summary>
    public long TotalCharacters { get; init; }

    /// <summary>
    /// 成功字符数
    /// </summary>
    public long SuccessCharacters { get; init; }

    /// <summary>
    /// 平均输入速度（字符/秒）
    /// </summary>
    public double AverageSpeed { get; init; }
}

/// <summary>
/// 错误统计
/// </summary>
public record ErrorStats
{
    /// <summary>
    /// 总错误数
    /// </summary>
    public long TotalErrors { get; init; }

    /// <summary>
    /// 最近错误
    /// </summary>
    public List<ErrorInfo> RecentErrors { get; init; } = new();
}

/// <summary>
/// 性能跟踪器
/// </summary>
internal class PerformanceTracker : IDisposable
{
    private long _totalOperations;
    private long _successCount;
    private long _failureCount;
    private long _totalResponseTime;
    private long _totalCharacters;
    private long _successCharacters;
    private readonly object _lock = new();

    public void RecordSuccess(DateTime startTime)
    {
        lock (_lock)
        {
            _totalOperations++;
            _successCount++;
            _totalResponseTime += (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }
    }

    public void RecordTextTyping(DateTime startTime, int characterCount, double successRate)
    {
        lock (_lock)
        {
            _totalOperations++;
            if (successRate >= 0.9)
            {
                _successCount++;
            }
            else
            {
                _failureCount++;
            }
            
            _totalCharacters += characterCount;
            _successCharacters += (int)(characterCount * successRate);
            _totalResponseTime += (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }
    }

    public void RecordFailure()
    {
        lock (_lock)
        {
            _totalOperations++;
            _failureCount++;
        }
    }

    public KeyboardServiceStats GetStats()
    {
        lock (_lock)
        {
            var successRate = _totalOperations > 0 ? (double)_successCount / _totalOperations : 0;
            var avgResponseTime = _successCount > 0 ? (double)_totalResponseTime / _successCount : 0;
            var textSuccessRate = _totalCharacters > 0 ? (double)_successCharacters / _totalCharacters : 0;
            var avgSpeed = avgResponseTime > 0 ? _totalCharacters / (avgResponseTime / 1000) : 0;

            return new KeyboardServiceStats
            {
                TotalOperations = _totalOperations,
                SuccessCount = _successCount,
                FailureCount = _failureCount,
                SuccessRate = successRate,
                AverageResponseTime = avgResponseTime,
                TextTyping = new TextTypingStats
                {
                    TotalCharacters = _totalCharacters,
                    SuccessCharacters = _successCharacters,
                    AverageSpeed = avgSpeed
                }
            };
        }
    }

    public void Dispose()
    {
        // 清理资源
    }
}

/// <summary>
/// 错误跟踪器
/// </summary>
internal class ErrorTracker : IDisposable
{
    private readonly List<ErrorInfo> _recentErrors = new();
    private readonly object _lock = new();
    private const int MaxErrorCount = 100;

    public void RecordError(Exception ex, string operation)
    {
        lock (_lock)
        {
            var errorInfo = new ErrorInfo
            {
                Message = ex.Message,
                ErrorType = ex.GetType().Name,
                ErrorTime = DateTime.UtcNow,
                StackTrace = ex.StackTrace ?? string.Empty
            };

            _recentErrors.Add(errorInfo);

            // 保持错误列表在限制范围内
            while (_recentErrors.Count > MaxErrorCount)
            {
                _recentErrors.RemoveAt(0);
            }
        }
    }

    public List<ErrorInfo> GetRecentErrors()
    {
        lock (_lock)
        {
            return _recentErrors.ToList();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _recentErrors.Clear();
        }
    }
}