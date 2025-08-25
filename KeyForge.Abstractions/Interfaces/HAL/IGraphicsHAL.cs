namespace KeyForge.Abstractions.Interfaces.HAL
{
    /// <summary>
    /// 图形硬件抽象层基础接口
    /// 【优化实现】定义了跨平台图形系统的硬件抽象层接口
    /// 原实现：图形功能直接调用平台API，缺乏统一抽象
    /// 优化：通过HAL抽象，实现跨平台图形系统的统一接口
    /// </summary>
    public interface IGraphicsHAL : IDisposable
    {
        /// <summary>
        /// 初始化图形HAL
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 获取HAL类型
        /// </summary>
        HALType HALType { get; }
        
        /// <summary>
        /// 获取HAL版本
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// HAL状态
        /// </summary>
        HALStatus Status { get; }
    }
    
    /// <summary>
    /// 屏幕捕获硬件抽象层接口
    /// </summary>
    public interface IScreenCaptureHAL : IGraphicsHAL
    {
        /// <summary>
        /// 捕获整个屏幕
        /// </summary>
        Task<byte[]> CaptureScreenAsync();
        
        /// <summary>
        /// 捕获指定区域
        /// </summary>
        Task<byte[]> CaptureRegionAsync(int x, int y, int width, int height);
        
        /// <summary>
        /// 获取屏幕分辨率
        /// </summary>
        Task<(int Width, int Height)> GetScreenResolutionAsync();
        
        /// <summary>
        /// 获取屏幕颜色深度
        /// </summary>
        Task<int> GetScreenColorDepthAsync();
        
        /// <summary>
        /// 获取屏幕刷新率
        /// </summary>
        Task<int> GetScreenRefreshRateAsync();
    }
    
    /// <summary>
    /// 图像处理硬件抽象层接口
    /// </summary>
    public interface IImageProcessingHAL : IGraphicsHAL
    {
        /// <summary>
        /// 调整图像大小
        /// </summary>
        Task<byte[]> ResizeImageAsync(byte[] imageData, int newWidth, int newHeight);
        
        /// <summary>
        /// 裁剪图像
        /// </summary>
        Task<byte[]> CropImageAsync(byte[] imageData, int x, int y, int width, int height);
        
        /// <summary>
        /// 转换为灰度图像
        /// </summary>
        Task<byte[]> ConvertToGrayscaleAsync(byte[] imageData);
        
        /// <summary>
        /// 应用高斯模糊
        /// </summary>
        Task<byte[]> ApplyGaussianBlurAsync(byte[] imageData, double radius);
        
        /// <summary>
        /// 检测图像边缘
        /// </summary>
        Task<byte[]> DetectEdgesAsync(byte[] imageData);
        
        /// <summary>
        /// 比较两个图像
        /// </summary>
        Task<double> CompareImagesAsync(byte[] image1, byte[] image2);
        
        /// <summary>
        /// 查找图像模板
        /// </summary>
        Task<TemplateMatchResult> FindTemplateAsync(byte[] sourceImage, byte[] templateImage, double threshold);
    }
    
    /// <summary>
    /// 显示控制硬件抽象层接口
    /// </summary>
    public interface IDisplayControlHAL : IGraphicsHAL
    {
        /// <summary>
        /// 获取显示器信息
        /// </summary>
        Task<List<DisplayInfo>> GetDisplayInfosAsync();
        
        /// <summary>
        /// 获取主显示器
        /// </summary>
        Task<DisplayInfo> GetPrimaryDisplayAsync();
        
        /// <summary>
        /// 设置显示器分辨率
        /// </summary>
        Task<bool> SetDisplayResolutionAsync(int displayIndex, int width, int height, int refreshRate);
        
        /// <summary>
        /// 获取显示器DPI
        /// </summary>
        Task<double> GetDisplayDpiAsync(int displayIndex);
        
        /// <summary>
        /// 获取显示器颜色配置
        /// </summary>
        Task<ColorProfile> GetColorProfileAsync(int displayIndex);
    }
    
    /// <summary>
    /// 模板匹配结果
    /// </summary>
    public record TemplateMatchResult
    {
        public bool Found { get; init; }
        public double Confidence { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
    }
    
    /// <summary>
    /// 显示器信息
    /// </summary>
    public record DisplayInfo
    {
        public int Index { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Width { get; init; }
        public int Height { get; init; }
        public int RefreshRate { get; init; }
        public bool IsPrimary { get; init; }
        public double Dpi { get; init; }
        public string DeviceName { get; init; } = string.Empty;
    }
    
    /// <summary>
    /// 颜色配置
    /// </summary>
    public record ColorProfile
    {
        public string Name { get; init; } = string.Empty;
        public int BitDepth { get; init; }
        public string ColorSpace { get; init; } = string.Empty;
        public string Gamma { get; init; } = string.Empty;
    }
}