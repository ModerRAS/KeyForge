using KeyForge.HAL.Abstractions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KeyForge.HAL.Services.Windows;

/// <summary>
/// Windows屏幕服务实现
/// 这是简化实现，专注于核心功能
/// </summary>
public class WindowsScreenService : IScreenService
{
    private readonly ILogger<WindowsScreenService> _logger;
    private readonly object _lock = new();
    private bool _isDisposed;

    // Windows API导入
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, 
        IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    private const int SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// 初始化Windows屏幕服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public WindowsScreenService(ILogger<WindowsScreenService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取屏幕截图
    /// </summary>
    /// <param name="x">起始X坐标</param>
    /// <param name="y">起始Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>截图图像</returns>
    public async Task<byte[]> CaptureScreenAsync(int x = 0, int y = 0, int width = 0, int height = 0)
    {
        try
        {
            lock (_lock)
            {
                // 如果未指定宽度和高度，则使用全屏
                if (width == 0 || height == 0)
                {
                    var screenSize = GetScreenResolution();
                    width = screenSize.Width;
                    height = screenSize.Height;
                }

                // 检查坐标是否在屏幕范围内
                var screenBounds = GetScreenBounds();
                if (x < 0 || y < 0 || x + width > screenBounds.Width || y + height > screenBounds.Height)
                {
                    _logger.LogWarning("Capture area is outside screen bounds");
                    return Array.Empty<byte>();
                }

                // 创建位图
                using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);

                // 复制屏幕内容
                graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                // 转换为字节数组
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                
                _logger.LogDebug("Screen captured: ({X}, {Y}, {Width}, {Height})", x, y, width, height);
                
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture screen: ({X}, {Y}, {Width}, {Height})", x, y, width, height);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 获取全屏截图
    /// </summary>
    /// <returns>全屏截图</returns>
    public async Task<byte[]> CaptureFullScreenAsync()
    {
        try
        {
            var resolution = GetScreenResolution();
            return await CaptureScreenAsync(0, 0, resolution.Width, resolution.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture full screen");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 获取指定窗口的截图
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口截图</returns>
    public async Task<byte[]> CaptureWindowAsync(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
            {
                return await CaptureFullScreenAsync();
            }

            lock (_lock)
            {
                // 获取窗口矩形
                if (!GetWindowRect(windowHandle, out var rect))
                {
                    _logger.LogError("Failed to get window rect");
                    return Array.Empty<byte>();
                }

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                if (width <= 0 || height <= 0)
                {
                    _logger.LogError("Invalid window size: {Width}x{Height}", width, height);
                    return Array.Empty<byte>();
                }

                // 创建位图
                using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);

                // 复制窗口内容
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                // 转换为字节数组
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                
                _logger.LogDebug("Window captured: {Handle}, size: {Width}x{Height}", windowHandle, width, height);
                
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture window: {Handle}", windowHandle);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 获取屏幕分辨率
    /// </summary>
    /// <returns>屏幕分辨率</returns>
    public Size GetScreenResolution()
    {
        try
        {
            return new Size(
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get screen resolution");
            return new Size(1920, 1080); // 默认值
        }
    }

    /// <summary>
    /// 获取屏幕数量
    /// </summary>
    /// <returns>屏幕数量</returns>
    public int GetScreenCount()
    {
        try
        {
            return System.Windows.Forms.Screen.AllScreens.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get screen count");
            return 1; // 默认值
        }
    }

    /// <summary>
    /// 获取指定屏幕的边界
    /// </summary>
    /// <param name="screenIndex">屏幕索引</param>
    /// <returns>屏幕边界</returns>
    public Rectangle GetScreenBounds(int screenIndex = 0)
    {
        try
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screenIndex < 0 || screenIndex >= screens.Length)
            {
                screenIndex = 0;
            }

            var screen = screens[screenIndex];
            return new Rectangle(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get screen bounds for index: {Index}", screenIndex);
            return new Rectangle(0, 0, 1920, 1080); // 默认值
        }
    }

    /// <summary>
    /// 获取主屏幕索引
    /// </summary>
    /// <returns>主屏幕索引</returns>
    public int GetPrimaryScreenIndex()
    {
        try
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].Primary)
                {
                    return i;
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary screen index");
            return 0; // 默认值
        }
    }

    /// <summary>
    /// 检查坐标是否在屏幕范围内
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>是否在范围内</returns>
    public bool IsPointOnScreen(int x, int y)
    {
        try
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            foreach (var screen in screens)
            {
                if (x >= screen.Bounds.Left && x <= screen.Bounds.Right &&
                    y >= screen.Bounds.Top && y <= screen.Bounds.Bottom)
                {
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if point is on screen: ({X}, {Y})", x, y);
            return false;
        }
    }

    /// <summary>
    /// 获取屏幕颜色深度
    /// </summary>
    /// <returns>颜色深度</returns>
    public int GetColorDepth()
    {
        try
        {
            using var graphics = Graphics.FromHwnd(GetDesktopWindow());
            return graphics.DpiX;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get color depth");
            return 32; // 默认值
        }
    }

    /// <summary>
    /// 获取屏幕刷新率
    /// </summary>
    /// <returns>刷新率</returns>
    public int GetRefreshRate()
    {
        try
        {
            // 简化实现 - 返回标准刷新率
            return 60;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refresh rate");
            return 60; // 默认值
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
    ~WindowsScreenService()
    {
        Dispose(false);
    }
}