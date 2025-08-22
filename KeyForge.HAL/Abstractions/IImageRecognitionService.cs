namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 图像识别服务接口 - 提供跨平台图像识别功能
/// </summary>
public interface IImageRecognitionService
{
    /// <summary>
    /// 在屏幕上查找图像
    /// </summary>
    /// <param name="template">模板图像数据</param>
    /// <param name="threshold">匹配阈值（0-1）</param>
    /// <param name="searchArea">搜索区域</param>
    /// <returns>匹配结果</returns>
    Task<ImageMatchResult?> FindImageAsync(byte[] template, double threshold = 0.8, Rectangle? searchArea = null);

    /// <summary>
    /// 在屏幕上查找所有匹配的图像
    /// </summary>
    /// <param name="template">模板图像数据</param>
    /// <param name="threshold">匹配阈值（0-1）</param>
    /// <param name="searchArea">搜索区域</param>
    /// <returns>匹配结果列表</returns>
    Task<IEnumerable<ImageMatchResult>> FindAllImagesAsync(byte[] template, double threshold = 0.8, Rectangle? searchArea = null);

    /// <summary>
    /// 等待图像出现
    /// </summary>
    /// <param name="template">模板图像数据</param>
    /// <param name="timeout">超时时间（毫秒）</param>
    /// <param name="threshold">匹配阈值（0-1）</param>
    /// <param name="checkInterval">检查间隔（毫秒）</param>
    /// <returns>匹配结果</returns>
    Task<ImageMatchResult?> WaitForImageAsync(byte[] template, int timeout = 10000, double threshold = 0.8, int checkInterval = 500);

    /// <summary>
    /// 等待图像消失
    /// </summary>
    /// <param name="template">模板图像数据</param>
    /// <param name="timeout">超时时间（毫秒）</param>
    /// <param name="threshold">匹配阈值（0-1）</param>
    /// <param name="checkInterval">检查间隔（毫秒）</param>
    /// <returns>是否成功</returns>
    Task<bool> WaitForImageDisappearAsync(byte[] template, int timeout = 10000, double threshold = 0.8, int checkInterval = 500);

    /// <summary>
    /// 计算图像相似度
    /// </summary>
    /// <param name="image1">图像1数据</param>
    /// <param name="image2">图像2数据</param>
    /// <returns>相似度（0-1）</returns>
    Task<double> CalculateSimilarityAsync(byte[] image1, byte[] image2);

    /// <summary>
    /// 获取指定区域的图像
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>图像数据</returns>
    Task<byte[]> CaptureRegionAsync(int x, int y, int width, int height);

    /// <summary>
    /// 保存图像到文件
    /// </summary>
    /// <param name="imageData">图像数据</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> SaveImageAsync(byte[] imageData, string filePath);

    /// <summary>
    /// 从文件加载图像
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>图像数据</returns>
    Task<byte[]> LoadImageAsync(string filePath);

    /// <summary>
    /// 调整图像大小
    /// </summary>
    /// <param name="imageData">图像数据</param>
    /// <param name="width">新宽度</param>
    /// <param name="height">新高度</param>
    /// <returns>调整后的图像数据</returns>
    Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height);

    /// <summary>
    /// 转换图像格式
    /// </summary>
    /// <param name="imageData">图像数据</param>
    /// <param name="targetFormat">目标格式</param>
    /// <returns>转换后的图像数据</returns>
    Task<byte[]> ConvertImageFormatAsync(byte[] imageData, string targetFormat);

    /// <summary>
    /// 获取图像信息
    /// </summary>
    /// <param name="imageData">图像数据</param>
    /// <returns>图像信息</returns>
    Task<ImageInfo> GetImageInfoAsync(byte[] imageData);
}