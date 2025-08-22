using KeyForge.HAL.Abstractions;
using System.Runtime.InteropServices;

namespace KeyForge.HAL.Services.Windows;

/// <summary>
/// Windows鼠标服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class WindowsMouseService : IMouseService
{
    private readonly ILogger<WindowsMouseService> _logger;
    private readonly object _lock = new();
    private bool _isDisposed;

    // Windows API导入
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// 初始化Windows鼠标服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public WindowsMouseService(ILogger<WindowsMouseService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 鼠标事件
    /// </summary>
    public event EventHandler<MouseEventArgs>? MouseEvent;

    /// <summary>
    /// 移动鼠标到指定位置
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>是否成功</returns>
    public async Task<bool> MoveToAsync(int x, int y)
    {
        try
        {
            lock (_lock)
            {
                var result = SetCursorPos(x, y);
                _logger.LogDebug("Mouse moved to: ({X}, {Y})", x, y);
                
                OnMouseEvent(new Point(x, y), MouseButton.Unknown, MouseButtonState.Up, 0);
                
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move mouse to: ({X}, {Y})", x, y);
            return false;
        }
    }

    /// <summary>
    /// 相对移动鼠标
    /// </summary>
    /// <param name="deltaX">X轴偏移</param>
    /// <param name="deltaY">Y轴偏移</param>
    /// <returns>是否成功</returns>
    public async Task<bool> MoveByAsync(int deltaX, int deltaY)
    {
        try
        {
            var currentPos = GetPosition();
            var newX = currentPos.X + deltaX;
            var newY = currentPos.Y + deltaY;
            
            return await MoveToAsync(newX, newY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move mouse by: ({DeltaX}, {DeltaY})", deltaX, deltaY);
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标左键按下
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> LeftButtonDownAsync()
    {
        try
        {
            lock (_lock)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                _logger.LogDebug("Left mouse button down");
                
                var pos = GetPosition();
                OnMouseEvent(pos, MouseButton.Left, MouseButtonState.Down, 0);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to press left mouse button");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标左键释放
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> LeftButtonUpAsync()
    {
        try
        {
            lock (_lock)
            {
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                _logger.LogDebug("Left mouse button up");
                
                var pos = GetPosition();
                OnMouseEvent(pos, MouseButton.Left, MouseButtonState.Up, 0);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release left mouse button");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标左键点击
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> LeftClickAsync()
    {
        try
        {
            var downResult = await LeftButtonDownAsync();
            await Task.Delay(10); // 短暂延迟
            var upResult = await LeftButtonUpAsync();
            
            return downResult && upResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to left click");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标右键点击
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> RightClickAsync()
    {
        try
        {
            lock (_lock)
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                await Task.Delay(10);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                
                _logger.LogDebug("Right mouse button click");
                
                var pos = GetPosition();
                OnMouseEvent(pos, MouseButton.Right, MouseButtonState.Up, 0);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to right click");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标中键点击
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> MiddleClickAsync()
    {
        try
        {
            lock (_lock)
            {
                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                await Task.Delay(10);
                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                
                _logger.LogDebug("Middle mouse button click");
                
                var pos = GetPosition();
                OnMouseEvent(pos, MouseButton.Middle, MouseButtonState.Up, 0);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to middle click");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标双击
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> DoubleClickAsync()
    {
        try
        {
            var firstClick = await LeftClickAsync();
            await Task.Delay(100); // 双击间隔
            var secondClick = await LeftClickAsync();
            
            return firstClick && secondClick;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to double click");
            return false;
        }
    }

    /// <summary>
    /// 模拟鼠标滚轮滚动
    /// </summary>
    /// <param name="delta">滚动量</param>
    /// <returns>是否成功</returns>
    public async Task<bool> ScrollAsync(int delta)
    {
        try
        {
            lock (_lock)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, 0);
                _logger.LogDebug("Mouse scrolled: {Delta}", delta);
                
                var pos = GetPosition();
                OnMouseEvent(pos, MouseButton.Unknown, MouseButtonState.Up, delta);
            }
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scroll mouse: {Delta}", delta);
            return false;
        }
    }

    /// <summary>
    /// 获取当前鼠标位置
    /// </summary>
    /// <returns>鼠标位置</returns>
    public Point GetPosition()
    {
        try
        {
            GetCursorPos(out var point);
            return new Point(point.X, point.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get mouse position");
            return new Point(0, 0);
        }
    }

    /// <summary>
    /// 获取鼠标按钮状态
    /// </summary>
    /// <param name="button">鼠标按钮</param>
    /// <returns>按钮状态</returns>
    public MouseButtonState GetButtonState(MouseButton button)
    {
        try
        {
            var virtualKey = button switch
            {
                MouseButton.Left => 0x01,    // VK_LBUTTON
                MouseButton.Right => 0x02,   // VK_RBUTTON
                MouseButton.Middle => 0x04,  // VK_MBUTTON
                _ => 0
            };

            var state = GetAsyncKeyState(virtualKey);
            return (state & 0x8000) != 0 ? MouseButtonState.Down : MouseButtonState.Up;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get mouse button state: {Button}", button);
            return MouseButtonState.Unknown;
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
    ~WindowsMouseService()
    {
        Dispose(false);
    }

    /// <summary>
    /// 触发鼠标事件
    /// </summary>
    /// <param name="position">鼠标位置</param>
    /// <param name="button">鼠标按钮</param>
    /// <param name="buttonState">按钮状态</param>
    /// <param name="wheelDelta">滚轮增量</param>
    private void OnMouseEvent(Point position, MouseButton button, MouseButtonState buttonState, int wheelDelta)
    {
        try
        {
            MouseEvent?.Invoke(this, new MouseEventArgs
            {
                Position = position,
                Button = button,
                ButtonState = buttonState,
                WheelDelta = wheelDelta,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to raise mouse event");
        }
    }
}