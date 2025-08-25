namespace KeyForge.Abstractions.Models.Image
{
    /// <summary>
    /// 图像数据
    /// 【优化实现】定义统一的图像数据模型，支持跨平台图像处理
    /// 原实现：图像数据格式不统一，跨平台兼容性差
    /// 优化：通过统一的图像数据模型，提高跨平台兼容性
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// 图像字节数组
        /// </summary>
        public byte[] Data { get; set; }
        
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
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "RGBA32";
        
        /// <summary>
        /// 每像素位数
        /// </summary>
        public int BitsPerPixel { get; set; } = 32;
        
        /// <summary>
        /// 图像步长
        /// </summary>
        public int Stride { get; set; }
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Data == null || Data.Length == 0;
        
        /// <summary>
        /// 创建图像数据
        /// </summary>
        public static ImageData Create(byte[] data, int width, int height, string format = "png")
        {
            return new ImageData
            {
                Data = data,
                Width = width,
                Height = height,
                Format = format,
                Stride = width * 4 // 假设32位RGBA格式
            };
        }
        
        /// <summary>
        /// 创建空图像数据
        /// </summary>
        public static ImageData Empty()
        {
            return new ImageData
            {
                Data = Array.Empty<byte>(),
                Width = 0,
                Height = 0
            };
        }
    }
    
    /// <summary>
    /// 屏幕区域
    /// </summary>
    public class ScreenRegion
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
        /// 监视器索引
        /// </summary>
        public int MonitorIndex { get; set; } = 0;
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;
        
        /// <summary>
        /// 创建屏幕区域
        /// </summary>
        public static ScreenRegion FromLTRB(int left, int top, int right, int bottom, int monitorIndex = 0)
        {
            return new ScreenRegion
            {
                X = left,
                Y = top,
                Width = right - left,
                Height = bottom - top,
                MonitorIndex = monitorIndex
            };
        }
        
        /// <summary>
        /// 创建全屏区域
        /// </summary>
        public static ScreenRegion FullScreen(int monitorIndex = 0)
        {
            return new ScreenRegion
            {
                X = 0,
                Y = 0,
                Width = int.MaxValue,
                Height = int.MaxValue,
                MonitorIndex = monitorIndex
            };
        }
    }
    
    /// <summary>
    /// 识别事件参数
    /// </summary>
    public class RecognitionEventArgs : EventArgs
    {
        /// <summary>
        /// 是否成功识别
        /// </summary>
        public bool IsRecognized { get; set; }
        
        /// <summary>
        /// 相似度
        /// </summary>
        public double Similarity { get; set; }
        
        /// <summary>
        /// 识别位置
        /// </summary>
        public Rectangle Location { get; set; }
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;
        
        /// <summary>
        /// 识别时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 处理时间
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }
        
        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// 额外数据
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
    
    /// <summary>
    /// 识别结果
    /// </summary>
    public class RecognitionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 相似度
        /// </summary>
        public double Similarity { get; set; }
        
        /// <summary>
        /// 识别位置
        /// </summary>
        public Rectangle Location { get; set; }
        
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
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static RecognitionResult SuccessResult(double similarity, Rectangle location, string templateName = "")
        {
            return new RecognitionResult
            {
                Success = true,
                Similarity = similarity,
                Location = location,
                TemplateName = templateName,
                Confidence = similarity
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static RecognitionResult FailureResult(string errorMessage)
        {
            return new RecognitionResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}