using KeyForge.Abstractions.Models.Core;
using KeyForge.Abstractions.Interfaces.HAL;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

namespace KeyForge.HAL.Windows.Input
{
    /// <summary>
    /// Windows键盘硬件抽象层实现
    /// 【优化实现】实现了跨平台键盘输入的Windows HAL
    /// 原实现：直接调用Windows API，缺乏抽象层
    /// 优化：通过HAL抽象，实现了统一的键盘输入接口
    /// </summary>
    public class WindowsKeyboardHAL : IKeyboardHAL
    {
        private readonly ILogger<WindowsKeyboardHAL> _logger;
        private readonly InputSimulator _inputSimulator;
        private KeyboardHookCallback? _hookCallback;
        private IntPtr _hookHandle = IntPtr.Zero;
        private bool _isDisposed = false;

        public HALType HALType => HALType.Windows;
        public Version Version => new Version(2, 0, 0);
        public HALStatus Status { get; private set; } = HALStatus.Uninitialized;

        public WindowsKeyboardHAL(ILogger<WindowsKeyboardHAL> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inputSimulator = new InputSimulator();
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                Status = HALStatus.Initializing;
                _logger.LogInformation("初始化Windows键盘HAL");

                // 检查权限
                if (!CheckPermissions())
                {
                    Status = HALStatus.Error;
                    _logger.LogError("权限检查失败");
                    return false;
                }

                // 初始化键盘状态
                await Task.Delay(10); // 模拟异步初始化

                Status = HALStatus.Ready;
                _logger.LogInformation("Windows键盘HAL初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Status = HALStatus.Error;
                _logger.LogError(ex, "Windows键盘HAL初始化失败");
                return false;
            }
        }

        public async Task<bool> SendKeyEventAsync(int keyCode, bool isKeyDown)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("键盘HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                var virtualKeyCode = (VirtualKeyCode)keyCode;
                
                if (isKeyDown)
                {
                    _inputSimulator.Keyboard.KeyDown(virtualKeyCode);
                }
                else
                {
                    _inputSimulator.Keyboard.KeyUp(virtualKeyCode);
                }

                _logger.LogDebug("发送键盘事件：KeyCode={KeyCode}, IsKeyDown={IsKeyDown}", keyCode, isKeyDown);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送键盘事件失败：KeyCode={KeyCode}, IsKeyDown={IsKeyDown}", keyCode, isKeyDown);
                return false;
            }
        }

        public async Task<bool> SendTextAsync(string text)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("键盘HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                if (string.IsNullOrEmpty(text))
                {
                    _logger.LogWarning("发送的文本为空");
                    return false;
                }

                _inputSimulator.Keyboard.TextEntry(text);
                _logger.LogDebug("发送文本：{Text}", text);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送文本失败：{Text}", text);
                return false;
            }
        }

        public async Task<bool> GetKeyStateAsync(int keyCode)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("键盘HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                // 使用Windows API获取按键状态
                var result = GetKeyState((short)keyCode);
                var isPressed = (result & 0x8000) != 0;
                
                _logger.LogDebug("获取按键状态：KeyCode={KeyCode}, IsPressed={IsPressed}", keyCode, isPressed);
                return await Task.FromResult(isPressed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取按键状态失败：KeyCode={KeyCode}", keyCode);
                return false;
            }
        }

        public async Task<bool> SetKeyboardHookAsync(KeyboardHookCallback callback)
        {
            try
            {
                if (_hookHandle != IntPtr.Zero)
                {
                    _logger.LogWarning("键盘钩子已存在，先移除现有钩子");
                    await RemoveKeyboardHookAsync();
                }

                _hookCallback = callback;
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProc, GetModuleHandle(null), 0);

                if (_hookHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    _logger.LogError("设置键盘钩子失败，错误代码：{ErrorCode}", errorCode);
                    return false;
                }

                _logger.LogInformation("键盘钩子设置成功");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置键盘钩子失败");
                return false;
            }
        }

        public async Task<bool> RemoveKeyboardHookAsync()
        {
            try
            {
                if (_hookHandle == IntPtr.Zero)
                {
                    _logger.LogWarning("没有活动的键盘钩子");
                    return true;
                }

                if (UnhookWindowsHookEx(_hookHandle))
                {
                    _hookHandle = IntPtr.Zero;
                    _hookCallback = null;
                    _logger.LogInformation("键盘钩子移除成功");
                    return await Task.FromResult(true);
                }
                else
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    _logger.LogError("移除键盘钩子失败，错误代码：{ErrorCode}", errorCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除键盘钩子失败");
                return false;
            }
        }

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _hookCallback != null)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var isKeyDown = (int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN;
                
                try
                {
                    _hookCallback(vkCode, isKeyDown);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "键盘钩子回调异常");
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private bool CheckPermissions()
        {
            // 检查是否具有足够的权限
            try
            {
                // 这里可以添加更详细的权限检查
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "权限检查失败");
                return false;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                RemoveKeyboardHookAsync().Wait();
                _inputSimulator?.Dispose();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        // Windows API 常量
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Windows API 函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern short GetKeyState(int nVirtKey);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}