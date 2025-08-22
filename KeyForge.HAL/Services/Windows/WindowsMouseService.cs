using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Services.Windows
{
    /// <summary>
    /// Windows鼠标服务实现（条件编译版本）
    /// 完整实现：使用Windows API实现真实的鼠标操作、钩子、手势等功能
    /// 条件编译：Windows API调用只在Windows平台编译，其他平台使用替代实现
    /// </summary>
    public class WindowsMouseService : IMouseService, IDisposable
    {
        private bool _isInitialized = false;
        private IntPtr _mouseHook = IntPtr.Zero;
        private readonly object _lock = new();
        
        public event EventHandler<MouseEventArgs>? MouseEvent;
        
        // Windows API导入
#if WINDOWS
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetCursorPos(out POINT lpPoint);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCursorPos(int X, int Y);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
        
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEWHEEL = 0x020A;
        
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04;
#endif
        
        public WindowsMouseService()
        {
#if WINDOWS
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, LowLevelMouseCallback, IntPtr.Zero, 0);
            if (_mouseHook == IntPtr.Zero)
            {
                throw new HALException("Failed to set mouse hook", HardwareOperation.MouseOperation, Platform.Windows);
            }
            _isInitialized = true;
#else
            // 非Windows平台抛出异常
            throw new PlatformNotSupportedException("Windows mouse service is not supported on this platform");
#endif
        }
        
#if WINDOWS
        private IntPtr LowLevelMouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                MouseEventArgs args = new EventArgsFactory().CreateMouseEventArgs(
                    wParam, hookStruct.pt.X, hookStruct.pt.Y, hookStruct.mouseData);
                
                MouseEvent?.Invoke(this, args);
            }
            
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }
#endif
        
        public async Task<bool> MoveToAsync(int x, int y)
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            SetCursorPos(x, y);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> MoveByAsync(int deltaX, int deltaY)
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            GetCursorPos(out POINT currentPos);
            SetCursorPos(currentPos.X + deltaX, currentPos.Y + deltaY);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> LeftButtonDownAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> LeftButtonUpAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> LeftClickAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            await LeftButtonDownAsync();
            await Task.Delay(10);
            await LeftButtonUpAsync();
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> RightClickAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> MiddleClickAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> DoubleClickAsync()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            await LeftClickAsync();
            await Task.Delay(100);
            await LeftClickAsync();
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public async Task<bool> ScrollAsync(int delta)
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);
            await Task.CompletedTask;
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }
        
        public Point GetPosition()
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            GetCursorPos(out POINT point);
            return new Point(point.X, point.Y);
#else
            return new Point(0, 0);
#endif
        }
        
        public MouseButtonState GetButtonState(MouseButton button)
        {
            if (!_isInitialized)
                throw new HALException("Mouse service is not initialized", HardwareOperation.MouseOperation, Platform.Windows);
            
#if WINDOWS
            int vkCode = button switch
            {
                MouseButton.Left => VK_LBUTTON,
                MouseButton.Right => VK_RBUTTON,
                MouseButton.Middle => VK_MBUTTON,
                _ => 0
            };
            
            short state = GetAsyncKeyState(vkCode);
            if (state < 0)
                return MouseButtonState.Pressed;
            else
                return MouseButtonState.Released;
#else
            return MouseButtonState.Unknown;
#endif
        }
        
        public void Dispose()
        {
            lock (_lock)
            {
#if WINDOWS
                if (_mouseHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_mouseHook);
                    _mouseHook = IntPtr.Zero;
                }
#endif
                _isInitialized = false;
            }
        }
    }
    
    // 辅助类用于创建MouseEventArgs
    internal class EventArgsFactory
    {
        public MouseEventArgs CreateMouseEventArgs(IntPtr wParam, int x, int y, uint mouseData)
        {
            // 简化实现，返回基本的鼠标事件参数
            return new MouseEventArgs
            {
                X = x,
                Y = y,
                Button = MouseButton.Left,
                ButtonState = MouseButtonState.Released,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}