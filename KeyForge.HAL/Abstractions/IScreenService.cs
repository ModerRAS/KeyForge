namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 屏幕服务接口 - 提供跨平台屏幕操作功能
/// </summary>
public interface IScreenService
{
    /// <summary>
    /// 获取屏幕截图
    /// </summary>
    /// <param name="x">起始X坐标</param>
    /// <param name="y">起始Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>截图图像</returns>
    Task<byte[]> CaptureScreenAsync(int x = 0, int y = 0, int width = 0, int height = 0);

    /// <summary>
    /// 获取全屏截图
    /// </summary>
    /// <returns>全屏截图</returns>
    Task<byte[]> CaptureFullScreenAsync();

    /// <summary>
    /// 获取指定窗口的截图
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口截图</returns>
    Task<byte[]> CaptureWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 获取屏幕分辨率
    /// </summary>
    /// <returns>屏幕分辨率</returns>
    Size GetScreenResolution();

    /// <summary>
    /// 获取屏幕数量
    /// </summary>
    /// <returns>屏幕数量</returns>
    int GetScreenCount();

    /// <summary>
    /// 获取指定屏幕的边界
    /// </summary>
    /// <param name="screenIndex">屏幕索引</param>
    /// <returns>屏幕边界</returns>
    Rectangle GetScreenBounds(int screenIndex = 0);

    /// <summary>
    /// 获取主屏幕索引
    /// </summary>
    /// <returns>主屏幕索引</returns>
    int GetPrimaryScreenIndex();

    /// <summary>
    /// 检查坐标是否在屏幕范围内
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>是否在范围内</returns>
    bool IsPointOnScreen(int x, int y);

    /// <summary>
    /// 获取屏幕颜色深度
    /// </summary>
    /// <returns>颜色深度</returns>
    int GetColorDepth();

    /// <summary>
    /// 获取屏幕刷新率
    /// </summary>
    /// <returns>刷新率</returns>
    int GetRefreshRate();
}