using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Services.Windows
{
    /// <summary>
    /// Windows键盘服务实现（条件编译版本）
    /// 完整实现：使用Windows API实现真实的键盘操作、钩子、热键等功能
    /// 条件编译：Windows API调用只在Windows平台编译，其他平台使用替代实现
    /// </summary>
    public class WindowsKeyboardService : IKeyboardService, IDisposable
    {
        private bool _isInitialized = false;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private readonly object _lock = new();
        
        public event EventHandler<KeyboardEventArgs>? KeyEvent;
        
        // Windows API导入
#if WINDOWS
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const uint KEYEVENTF_KEYUP = 0x0002;
#endif
        
        public WindowsKeyboardService()
        {
#if WINDOWS
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardCallback, IntPtr.Zero, 0);
            if (_keyboardHook == IntPtr.Zero)
            {
                throw new HALException("Failed to set keyboard hook", HardwareOperation.KeyboardOperation, Platform.Windows);
            }
            _isInitialized = true;
#else
            // 非Windows平台抛出异常
            throw new PlatformNotSupportedException("Windows keyboard service is not supported on this platform");
#endif
        }
        
#if WINDOWS
        private IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyCode keyCode = (KeyCode)vkCode;
                
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    KeyEvent?.Invoke(this, new KeyboardEventArgs(keyCode, KeyState.Down));
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    KeyEvent?.Invoke(this, new KeyboardEventArgs(keyCode, KeyState.Up));
                }
            }
            
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }
#endif
        
        public async Task<bool> KeyDownAsync(KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
#if WINDOWS
            keybd_event((byte)key, 0, 0, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> KeyUpAsync(KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
#if WINDOWS
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> KeyPressAsync(KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
#if WINDOWS
            await KeyDownAsync(key);
            await Task.Delay(10);
            await KeyUpAsync(key);
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> TypeTextAsync(string text, int delay = 50)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
            if (string.IsNullOrEmpty(text))
                return true;
            
#if WINDOWS
            foreach (char c in text)
            {
                KeyCode keyCode = CharToKeyCode(c);
                if (keyCode != KeyCode.None)
                {
                    await KeyPressAsync(keyCode);
                    await Task.Delay(delay);
                }
            }
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> SendHotkeyAsync(KeyCode[] modifiers, KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
#if WINDOWS
            // 按下所有修饰键
            foreach (KeyCode modifier in modifiers)
            {
                await KeyDownAsync(modifier);
            }
            
            // 按下主键
            await KeyDownAsync(key);
            await Task.Delay(10);
            await KeyUpAsync(key);
            
            // 释放所有修饰键
            foreach (KeyCode modifier in modifiers.Reverse())
            {
                await KeyUpAsync(modifier);
            }
            
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public KeyState GetKeyState(KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
#if WINDOWS
            short state = GetAsyncKeyState((int)key);
            if (state < 0)
                return KeyState.Down;
            else if (state == 1)
                return KeyState.Toggled;
            else
                return KeyState.Up;
#else
            return KeyState.Unknown;
#endif
        }
        
        public bool IsKeyAvailable(KeyCode key)
        {
            if (!_isInitialized)
                throw new HALException("Keyboard service is not initialized", HardwareOperation.KeyboardOperation, Platform.Windows);
            
            return true;
        }
        
        public void Dispose()
        {
            lock (_lock)
            {
#if WINDOWS
                if (_keyboardHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHook);
                    _keyboardHook = IntPtr.Zero;
                }
#endif
                _isInitialized = false;
            }
        }
        
#if WINDOWS
        private KeyCode CharToKeyCode(char c)
        {
            // 简化的字符到KeyCode映射
            if (c >= 'A' && c <= 'Z')
                return (KeyCode)((int)KeyCode.A + (c - 'A'));
            if (c >= 'a' && c <= 'z')
                return (KeyCode)((int)KeyCode.A + (c - 'a'));
            if (c >= '0' && c <= '9')
                return (KeyCode)((int)KeyCode.D0 + (c - '0'));
            
            return KeyCode.None;
        }
#endif
    }
}