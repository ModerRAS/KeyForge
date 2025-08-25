namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 图像系统请求基类
    /// 【优化实现】定义图像系统请求的基类，统一请求处理
    /// 原实现：请求类型分散定义，缺乏统一规范
    /// 优化：通过基类统一请求处理，提高代码规范性
    /// </summary>
    public abstract class ImageRequestBase
    {
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 图像格式
        /// </summary>
        public string ImageFormat { get; set; } = "png";
        
        /// <summary>
        /// 图像质量
        /// </summary>
        public int Quality { get; set; } = 90;
    }
    
    /// <summary>
    /// 捕获屏幕请求
    /// </summary>
    public class CaptureScreenRequest : ImageRequestBase
    {
        /// <summary>
        /// 捕获区域
        /// </summary>
        public Rectangle CaptureArea { get; set; }
        
        /// <summary>
        /// 包含鼠标指针
        /// </summary>
        public bool IncludeCursor { get; set; } = false;
        
        /// <summary>
        /// 监视器索引
        /// </summary>
        public int MonitorIndex { get; set; } = 0;
    }
    
    /// <summary>
    /// 创建模板请求
    /// </summary>
    public class CreateTemplateRequest : ImageRequestBase
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// 模板图像
        /// </summary>
        public byte[] TemplateImage { get; set; }
        
        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new();
    }
    
    /// <summary>
    /// 识别图像请求
    /// </summary>
    public class RecognizeImageRequest : ImageRequestBase
    {
        /// <summary>
        /// 待识别图像
        /// </summary>
        public byte[] ImageData { get; set; }
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// 相似度阈值
        /// </summary>
        public double SimilarityThreshold { get; set; } = 0.8;
        
        /// <summary>
        /// 识别模式
        /// </summary>
        public string RecognitionMode { get; set; } = "TemplateMatching";
    }
    
    /// <summary>
    /// 处理图像请求
    /// </summary>
    public class ProcessImageRequest : ImageRequestBase
    {
        /// <summary>
        /// 输入图像
        /// </summary>
        public byte[] InputImage { get; set; }
        
        /// <summary>
        /// 处理操作
        /// </summary>
        public string ProcessOperation { get; set; }
        
        /// <summary>
        /// 处理参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
    
    /// <summary>
    /// 分析屏幕请求
    /// </summary>
    public class AnalyzeScreenRequest : ImageRequestBase
    {
        /// <summary>
        /// 分析区域
        /// </summary>
        public Rectangle AnalysisArea { get; set; }
        
        /// <summary>
        /// 分析类型
        /// </summary>
        public string AnalysisType { get; set; } = "ColorAnalysis";
        
        /// <summary>
        /// 分析参数
        /// </summary>
        public Dictionary<string, object> AnalysisParameters { get; set; } = new();
    }
    
    /// <summary>
    /// 矩形区域
    /// </summary>
    public class Rectangle
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
    }
}