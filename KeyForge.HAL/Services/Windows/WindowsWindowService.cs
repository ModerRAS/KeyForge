using KeyForge.HAL.Abstractions;
using System.Runtime.InteropServices;

namespace KeyForge.HAL.Services.Windows;

/// <summary>
/// Windows窗口服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class WindowsWindowService : IWindowService
{
    private readonly ILogger<WindowsWindowService> _logger;
    private readonly object _lock = new();
    private bool _isDisposed;

    // Windows API导入
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, 
        int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool CloseWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, 
        string lpszClass, string lpszWindow);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const int SW_MINIMIZE = 6;
    private const int SW_MAXIMIZE = 3;
    private const int SW_RESTORE = 9;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_NOACTIVATE = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// 初始化Windows窗口服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public WindowsWindowService(ILogger<WindowsWindowService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取当前活动窗口
    /// </summary>
    /// <returns>窗口句柄</returns>
    public async Task<IntPtr> GetForegroundWindowAsync()
    {
        try
        {
            var hWnd = GetForegroundWindow();
            _logger.LogDebug("Foreground window: {Handle}", hWnd);
            return await Task.FromResult(hWnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get foreground window");
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 设置活动窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SetForegroundWindowAsync(IntPtr windowHandle)
    {
        try
        {
            lock (_lock)
            {
                var result = SetForegroundWindow(windowHandle);
                _logger.LogDebug("Set foreground window: {Handle}, result: {Result}", windowHandle, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set foreground window: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 获取窗口标题
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口标题</returns>
    public async Task<string> GetWindowTitleAsync(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(256);
            var length = GetWindowText(windowHandle, builder, builder.Capacity);
            
            return length > 0 ? builder.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get window title: {Handle}", windowHandle);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取窗口类名
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口类名</returns>
    public async Task<string> GetWindowClassNameAsync(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(256);
            var length = GetClassName(windowHandle, builder, builder.Capacity);
            
            return length > 0 ? builder.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get window class name: {Handle}", windowHandle);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取窗口位置和大小
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口矩形</returns>
    public async Task<Rectangle> GetWindowRectAsync(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
            {
                return new Rectangle(0, 0, 0, 0);
            }

            if (GetWindowRect(windowHandle, out var rect))
            {
                return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            return new Rectangle(0, 0, 0, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get window rect: {Handle}", windowHandle);
            return new Rectangle(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// 设置窗口位置和大小
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SetWindowPosAsync(IntPtr windowHandle, int x, int y, int width, int height)
    {
        try
        {
            lock (_lock)
            {
                var result = SetWindowPos(windowHandle, IntPtr.Zero, x, y, width, height, 
                    SWP_NOZORDER | SWP_NOACTIVATE);
                
                _logger.LogDebug("Set window position: {Handle}, ({X}, {Y}, {Width}, {Height}), result: {Result}", 
                    windowHandle, x, y, width, height, result);
                
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set window position: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 最小化窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    public async Task<bool> MinimizeWindowAsync(IntPtr windowHandle)
    {
        try
        {
            lock (_lock)
            {
                var result = ShowWindow(windowHandle, SW_MINIMIZE);
                _logger.LogDebug("Minimize window: {Handle}, result: {Result}", windowHandle, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to minimize window: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 最大化窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    public async Task<bool> MaximizeWindowAsync(IntPtr windowHandle)
    {
        try
        {
            lock (_lock)
            {
                var result = ShowWindow(windowHandle, SW_MAXIMIZE);
                _logger.LogDebug("Maximize window: {Handle}, result: {Result}", windowHandle, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to maximize window: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 恢复窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    public async Task<bool> RestoreWindowAsync(IntPtr windowHandle)
    {
        try
        {
            lock (_lock)
            {
                var result = ShowWindow(windowHandle, SW_RESTORE);
                _logger.LogDebug("Restore window: {Handle}, result: {Result}", windowHandle, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore window: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    public async Task<bool> CloseWindowAsync(IntPtr windowHandle)
    {
        try
        {
            lock (_lock)
            {
                var result = CloseWindow(windowHandle);
                _logger.LogDebug("Close window: {Handle}, result: {Result}", windowHandle, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close window: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 枚举所有窗口
    /// </summary>
    /// <returns>窗口信息列表</returns>
    public async Task<IEnumerable<WindowInfo>> EnumWindowsAsync()
    {
        try
        {
            var windows = new List<WindowInfo>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var title = GetWindowTitleAsync(hWnd).GetAwaiter().GetResult();
                    var className = GetWindowClassNameAsync(hWnd).GetAwaiter().GetResult();
                    var rect = GetWindowRectAsync(hWnd).GetAwaiter().GetResult();
                    
                    windows.Add(new WindowInfo
                    {
                        Handle = hWnd,
                        Title = title,
                        ClassName = className,
                        Rectangle = rect,
                        IsVisible = true,
                        IsActive = hWnd == GetForegroundWindow()
                    });
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate windows");
            return Enumerable.Empty<WindowInfo>();
        }
    }

    /// <summary>
    /// 根据标题查找窗口
    /// </summary>
    /// <param name="title">窗口标题</param>
    /// <param name="exactMatch">是否精确匹配</param>
    /// <returns>窗口句柄</returns>
    public async Task<IntPtr> FindWindowByTitleAsync(string title, bool exactMatch = false)
    {
        try
        {
            var windows = await EnumWindowsAsync();
            
            foreach (var window in windows)
            {
                if (exactMatch)
                {
                    if (window.Title == title)
                    {
                        return window.Handle;
                    }
                }
                else
                {
                    if (window.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                    {
                        return window.Handle;
                    }
                }
            }

            return IntPtr.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find window by title: {Title}", title);
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 根据类名查找窗口
    /// </summary>
    /// <param name="className">窗口类名</param>
    /// <returns>窗口句柄</returns>
    public async Task<IntPtr> FindWindowByClassAsync(string className)
    {
        try
        {
            var hWnd = FindWindow(className, null);
            _logger.LogDebug("Find window by class: {ClassName}, result: {Handle}", className, hWnd);
            return hWnd;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find window by class: {ClassName}", className);
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 检查窗口是否可见
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否可见</returns>
    public async Task<bool> IsWindowVisibleAsync(IntPtr windowHandle)
    {
        try
        {
            return IsWindowVisible(windowHandle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if window is visible: {Handle}", windowHandle);
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否存在
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否存在</returns>
    public async Task<bool> IsWindowAsync(IntPtr windowHandle)
    {
        try
        {
            return IsWindow(windowHandle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if window exists: {Handle}", windowHandle);
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
    ~WindowsWindowService()
    {
        Dispose(false);
    }
}