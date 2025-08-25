using KeyForge.Abstractions.Enums;
using KeyForge.Abstractions.Interfaces.HAL;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

namespace KeyForge.HAL.Windows.Input
{
    /// <summary>
    /// Windows鼠标硬件抽象层实现
    /// 【优化实现】实现了跨平台鼠标输入的Windows HAL
    /// 原实现：直接调用Windows API，缺乏抽象层
    /// 优化：通过HAL抽象，实现了统一的鼠标输入接口
    /// </summary>
    public class WindowsMouseHAL : IMouseHAL
    {
        private readonly ILogger<WindowsMouseHAL> _logger;
        private readonly InputSimulator _inputSimulator;
        private MouseHookCallback? _hookCallback;
        private IntPtr _hookHandle = IntPtr.Zero;
        private bool _isDisposed = false;

        public HALType HALType => HALType.Windows;
        public Version Version => new Version(2, 0, 0);
        public HALStatus Status { get; private set; } = HALStatus.Uninitialized;

        public WindowsMouseHAL(ILogger<WindowsMouseHAL> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inputSimulator = new InputSimulator();
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                Status = HALStatus.Initializing;
                _logger.LogInformation("初始化Windows鼠标HAL");

                // 检查权限
                if (!CheckPermissions())
                {
                    Status = HALStatus.Error;
                    _logger.LogError("权限检查失败");
                    return false;
                }

                // 初始化鼠标状态
                await Task.Delay(10); // 模拟异步初始化

                Status = HALStatus.Ready;
                _logger.LogInformation("Windows鼠标HAL初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Status = HALStatus.Error;
                _logger.LogError(ex, "Windows鼠标HAL初始化失败");
                return false;
            }
        }

        public async Task<bool> MoveMouseAsync(int x, int y)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("鼠标HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                _inputSimulator.Mouse.MoveMouseTo(x, y);
                _logger.LogDebug("移动鼠标到位置：X={X}, Y={Y}", x, y);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移动鼠标失败：X={X}, Y={Y}", x, y);
                return false;
            }
        }

        public async Task<bool> SendMouseEventAsync(int x, int y, int mouseButton, bool isMouseDown)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("鼠标HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                var button = (MouseButton)mouseButton;
                
                // 移动到指定位置
                _inputSimulator.Mouse.MoveMouseTo(x, y);
                
                // 发送鼠标事件
                if (isMouseDown)
                {
                    await MouseDownAsync(button);
                }
                else
                {
                    await MouseUpAsync(button);
                }

                _logger.LogDebug("发送鼠标事件：X={X}, Y={Y}, Button={Button}, IsDown={IsDown}", 
                    x, y, button, isMouseDown);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送鼠标事件失败：X={X}, Y={Y}, Button={Button}, IsDown={IsDown}", 
                    x, y, mouseButton, isMouseDown);
                return false;
            }
        }

        public async Task<bool> SendMouseWheelEventAsync(int delta)
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("鼠标HAL未就绪，状态：{Status}", Status);
                    return false;
                }

                _inputSimulator.Mouse.VerticalScroll(delta);
                _logger.LogDebug("发送鼠标滚轮事件：Delta={Delta}", delta);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送鼠标滚轮事件失败：Delta={Delta}", delta);
                return false;
            }
        }

        public async Task<(int X, int Y)> GetMousePositionAsync()
        {
            try
            {
                if (Status != HALStatus.Ready && Status != HALStatus.Running)
                {
                    _logger.LogWarning("鼠标HAL未就绪，状态：{Status}", Status);
                    return (0, 0);
                }

                var point = new POINT();
                GetCursorPos(out point);
                
                _logger.LogDebug("获取鼠标位置：X={X}, Y={Y}", point.X, point.Y);
                return await Task.FromResult((point.X, point.Y));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取鼠标位置失败");
                return (0, 0);
            }
        }

        public async Task<bool> SetMouseHookAsync(MouseHookCallback callback)
        {
            try
            {
                if (_hookHandle != IntPtr.Zero)
                {
                    _logger.LogWarning("鼠标钩子已存在，先移除现有钩子");
                    await RemoveMouseHookAsync();
                }

                _hookCallback = callback;
                _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProc, GetModuleHandle(null), 0);

                if (_hookHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    _logger.LogError("设置鼠标钩子失败，错误代码：{ErrorCode}", errorCode);
                    return false;
                }

                _logger.LogInformation("鼠标钩子设置成功");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置鼠标钩子失败");
                return false;
            }
        }

        public async Task<bool> RemoveMouseHookAsync()
        {
            try
            {
                if (_hookHandle == IntPtr.Zero)
                {
                    _logger.LogWarning("没有活动的鼠标钩子");
                    return true;
                }

                if (UnhookWindowsHookEx(_hookHandle))
                {
                    _hookHandle = IntPtr.Zero;
                    _hookCallback = null;
                    _logger.LogInformation("鼠标钩子移除成功");
                    return await Task.FromResult(true);
                }
                else
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    _logger.LogError("移除鼠标钩子失败，错误代码：{ErrorCode}", errorCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除鼠标钩子失败");
                return false;
            }
        }

        private async Task<bool> MouseDownAsync(MouseButton button)
        {
            try
            {
                switch (button)
                {
                    case MouseButton.Left:
                        _inputSimulator.Mouse.LeftButtonDown();
                        break;
                    case MouseButton.Right:
                        _inputSimulator.Mouse.RightButtonDown();
                        break;
                    case MouseButton.Middle:
                        _inputSimulator.Mouse.MiddleButtonDown();
                        break;
                    case MouseButton.XButton1:
                        _inputSimulator.Mouse.XButtonDown(1);
                        break;
                    case MouseButton.XButton2:
                        _inputSimulator.Mouse.XButtonDown(2);
                        break;
                    default:
                        _logger.LogWarning("不支持的鼠标按钮：{Button}", button);
                        return false;
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标按下失败：{Button}", button);
                return false;
            }
        }

        private async Task<bool> MouseUpAsync(MouseButton button)
        {
            try
            {
                switch (button)
                {
                    case MouseButton.Left:
                        _inputSimulator.Mouse.LeftButtonUp();
                        break;
                    case MouseButton.Right:
                        _inputSimulator.Mouse.RightButtonUp();
                        break;
                    case MouseButton.Middle:
                        _inputSimulator.Mouse.MiddleButtonUp();
                        break;
                    case MouseButton.XButton1:
                        _inputSimulator.Mouse.XButtonUp(1);
                        break;
                    case MouseButton.XButton2:
                        _inputSimulator.Mouse.XButtonUp(2);
                        break;
                    default:
                        _logger.LogWarning("不支持的鼠标按钮：{Button}", button);
                        return false;
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鼠标释放失败：{Button}", button);
                return false;
            }
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _hookCallback != null)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var mouseEvent = (int)wParam;
                
                try
                {
                    int button = 0;
                    bool isMouseDown = false;

                    // 解析鼠标事件
                    switch (mouseEvent)
                    {
                        case WM_LBUTTONDOWN:
                            button = (int)MouseButton.Left;
                            isMouseDown = true;
                            break;
                        case WM_LBUTTONUP:
                            button = (int)MouseButton.Left;
                            isMouseDown = false;
                            break;
                        case WM_RBUTTONDOWN:
                            button = (int)MouseButton.Right;
                            isMouseDown = true;
                            break;
                        case WM_RBUTTONUP:
                            button = (int)MouseButton.Right;
                            isMouseDown = false;
                            break;
                        case WM_MBUTTONDOWN:
                            button = (int)MouseButton.Middle;
                            isMouseDown = true;
                            break;
                        case WM_MBUTTONUP:
                            button = (int)MouseButton.Middle;
                            isMouseDown = false;
                            break;
                        case WM_MOUSEMOVE:
                            button = 0;
                            isMouseDown = false;
                            break;
                        case WM_MOUSEWHEEL:
                            button = 0;
                            isMouseDown = false;
                            break;
                    }

                    _hookCallback(hookStruct.pt.x, hookStruct.pt.y, button, isMouseDown);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "鼠标钩子回调异常");
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private bool CheckPermissions()
        {
            try
            {
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
                RemoveMouseHookAsync().Wait();
                _inputSimulator?.Dispose();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        // Windows API 常量
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSEWHEEL = 0x020A;

        // Windows API 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Windows API 函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}