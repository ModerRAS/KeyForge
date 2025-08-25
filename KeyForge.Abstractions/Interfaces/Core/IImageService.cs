using KeyForge.Abstractions.Models.Image;

namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 图像服务基础接口
    /// 【优化实现】统一了图像识别系统的抽象接口，支持跨平台图像处理
    /// 原实现：图像识别功能分散在各个平台，缺乏统一抽象
    /// 优化：定义统一的图像服务接口，所有平台实现都遵循此接口
    /// </summary>
    public interface IImageService : IDisposable
    {
        /// <summary>
        /// 初始化图像服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 捕获屏幕截图
        /// </summary>
        Task<ImageData> CaptureScreenAsync();
        
        /// <summary>
        /// 捕获指定区域的屏幕截图
        /// </summary>
        Task<ImageData> CaptureRegionAsync(ScreenRegion region);
        
        /// <summary>
        /// 服务状态
        /// </summary>
        ServiceStatus Status { get; }
    }
    
    /// <summary>
    /// 图像识别服务接口
    /// </summary>
    public interface IImageRecognitionService : IImageService
    {
        /// <summary>
        /// 创建图像模板
        /// </summary>
        Task<ImageTemplate> CreateTemplateAsync(string name, ImageData imageData, ScreenRegion region);
        
        /// <summary>
        /// 在屏幕上查找模板
        /// </summary>
        Task<RecognitionResult> FindTemplateAsync(ImageTemplate template, double threshold = 0.8);
        
        /// <summary>
        /// 在指定区域查找模板
        /// </summary>
        Task<RecognitionResult> FindTemplateInRegionAsync(ImageTemplate template, ScreenRegion region, double threshold = 0.8);
        
        /// <summary>
        /// 查找所有匹配的模板
        /// </summary>
        Task<List<RecognitionResult>> FindAllTemplatesAsync(ImageTemplate template, double threshold = 0.8);
        
        /// <summary>
        /// 保存模板到文件
        /// </summary>
        Task<bool> SaveTemplateAsync(ImageTemplate template, string filePath);
        
        /// <summary>
        /// 从文件加载模板
        /// </summary>
        Task<ImageTemplate> LoadTemplateAsync(string filePath);
        
        /// <summary>
        /// 图像识别完成事件
        /// </summary>
        event EventHandler<RecognitionEventArgs> OnRecognition;
    }
    
    /// <summary>
    /// 图像处理服务接口
    /// </summary>
    public interface IImageProcessingService : IImageService
    {
        /// <summary>
        /// 调整图像大小
        /// </summary>
        Task<ImageData> ResizeAsync(ImageData image, int width, int height);
        
        /// <summary>
        /// 裁剪图像
        /// </summary>
        Task<ImageData> CropAsync(ImageData image, ScreenRegion region);
        
        /// <summary>
        /// 转换为灰度图像
        /// </summary>
        Task<ImageData> ToGrayscaleAsync(ImageData image);
        
        /// <summary>
        /// 应用高斯模糊
        /// </summary>
        Task<ImageData> ApplyGaussianBlurAsync(ImageData image, double radius);
        
        /// <summary>
        /// 检测图像边缘
        /// </summary>
        Task<ImageData> DetectEdgesAsync(ImageData image);
        
        /// <summary>
        /// 比较两个图像的相似度
        /// </summary>
        Task<double> CompareImagesAsync(ImageData image1, ImageData image2);
    }
    
    /// <summary>
    /// 屏幕分析服务接口
    /// </summary>
    public interface IScreenAnalysisService : IImageService
    {
        /// <summary>
        /// 获取屏幕分辨率
        /// </summary>
        Task<(int Width, int Height)> GetScreenResolutionAsync();
        
        /// <summary>
        /// 获取屏幕DPI
        /// </summary>
        Task<double> GetScreenDpiAsync();
        
        /// <summary>
        /// 检测屏幕上的颜色变化
        /// </summary>
        Task<List<ScreenRegion>> DetectColorChangesAsync(ScreenRegion region, Color color, double threshold = 0.1);
        
        /// <summary>
        /// 分析屏幕内容变化
        /// </summary>
        Task<bool> DetectScreenChangesAsync(ScreenRegion region, double threshold = 0.05);
        
        /// <summary>
        /// 获取屏幕颜色直方图
        /// </summary>
        Task<ColorHistogram> GetColorHistogramAsync(ScreenRegion region);
    }
}