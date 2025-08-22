namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 窗口服务接口 - 提供跨平台窗口操作功能
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// 获取当前活动窗口
    /// </summary>
    /// <returns>窗口句柄</returns>
    Task<IntPtr> GetForegroundWindowAsync();

    /// <summary>
    /// 设置活动窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    Task<bool> SetForegroundWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 获取窗口标题
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口标题</returns>
    Task<string> GetWindowTitleAsync(IntPtr windowHandle);

    /// <summary>
    /// 获取窗口类名
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口类名</returns>
    Task<string> GetWindowClassNameAsync(IntPtr windowHandle);

    /// <summary>
    /// 获取窗口位置和大小
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口矩形</returns>
    Task<Rectangle> GetWindowRectAsync(IntPtr windowHandle);

    /// <summary>
    /// 设置窗口位置和大小
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>是否成功</returns>
    Task<bool> SetWindowPosAsync(IntPtr windowHandle, int x, int y, int width, int height);

    /// <summary>
    /// 最小化窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    Task<bool> MinimizeWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 最大化窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    Task<bool> MaximizeWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 恢复窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    Task<bool> RestoreWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否成功</returns>
    Task<bool> CloseWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 枚举所有窗口
    /// </summary>
    /// <returns>窗口信息列表</returns>
    Task<IEnumerable<WindowInfo>> EnumWindowsAsync();

    /// <summary>
    /// 根据标题查找窗口
    /// </summary>
    /// <param name="title">窗口标题</param>
    /// <param name="exactMatch">是否精确匹配</param>
    /// <returns>窗口句柄</returns>
    Task<IntPtr> FindWindowByTitleAsync(string title, bool exactMatch = false);

    /// <summary>
    /// 根据类名查找窗口
    /// </summary>
    /// <param name="className">窗口类名</param>
    /// <returns>窗口句柄</returns>
    Task<IntPtr> FindWindowByClassAsync(string className);

    /// <summary>
    /// 检查窗口是否可见
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否可见</returns>
    Task<bool> IsWindowVisibleAsync(IntPtr windowHandle);

    /// <summary>
    /// 检查窗口是否存在
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否存在</returns>
    Task<bool> IsWindowAsync(IntPtr windowHandle);
}