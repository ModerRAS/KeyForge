namespace KeyForge.Abstractions.Models.Image
{
    /// <summary>
    /// 图像信息
    /// 【优化实现】定义统一的图像信息模型，支持跨平台图像处理
    /// 原实现：图像模型分散定义，缺乏统一规范
    /// 优化：通过统一的图像模型，提高代码规范性
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// 图像高度
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// 图像格式
        /// </summary>
        public string Format { get; set; } = "png";
        
        /// <summary>
        /// 位深度
        /// </summary>
        public int BitDepth { get; set; } = 32;
        
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 哈希值
        /// </summary>
        public string Hash { get; set; } = string.Empty;
        
        /// <summary>
        /// 图像数据
        /// </summary>
        public byte[] Data { get; set; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
    /// <summary>
    /// 图像模板
    /// </summary>
    public class ImageTemplate
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 模板图像
        /// </summary>
        public byte[] TemplateImage { get; set; }
        
        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 相似度阈值
        /// </summary>
        public double SimilarityThreshold { get; set; } = 0.8;
        
        /// <summary>
        /// 识别区域
        /// </summary>
        public Rectangle Region { get; set; }
        
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
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
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;
        
        /// <summary>
        /// 创建矩形
        /// </summary>
        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            return new Rectangle
            {
                X = left,
                Y = top,
                Width = right - left,
                Height = bottom - top
            };
        }
    }
    
    /// <summary>
    /// 图像识别结果
    /// </summary>
    public class ImageRecognitionResult
    {
        /// <summary>
        /// 是否找到匹配
        /// </summary>
        public bool IsMatch { get; set; }
        
        /// <summary>
        /// 相似度
        /// </summary>
        public double Similarity { get; set; }
        
        /// <summary>
        /// 匹配位置
        /// </summary>
        public Rectangle MatchLocation { get; set; }
        
        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// 处理时间
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;
        
        /// <summary>
        /// 额外数据
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}