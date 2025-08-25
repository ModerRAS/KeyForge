namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 输入系统结果基类
    /// 【优化实现】定义输入系统结果的基类，统一结果处理
    /// 原实现：结果类型分散定义，缺乏统一规范
    /// 优化：通过基类统一结果处理，提高代码规范性
    /// </summary>
    public class InputResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 结果数据
        /// </summary>
        public object Data { get; set; }
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static InputResult SuccessResult(object data = null, string requestId = "")
        {
            return new InputResult
            {
                Success = true,
                Data = data,
                RequestId = requestId
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static InputResult FailureResult(string errorMessage, object data = null, string requestId = "")
        {
            return new InputResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Data = data,
                RequestId = requestId
            };
        }
    }
    
    /// <summary>
    /// 图像系统结果
    /// </summary>
    public class ImageResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 图像数据
        /// </summary>
        public byte[] ImageData { get; set; }
        
        /// <summary>
        /// 图像格式
        /// </summary>
        public string ImageFormat { get; set; } = "png";
        
        /// <summary>
        /// 识别结果
        /// </summary>
        public object RecognitionResult { get; set; }
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ImageResult SuccessResult(byte[] imageData = null, object recognitionResult = null, string requestId = "")
        {
            return new ImageResult
            {
                Success = true,
                ImageData = imageData,
                RecognitionResult = recognitionResult,
                RequestId = requestId
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ImageResult FailureResult(string errorMessage, string requestId = "")
        {
            return new ImageResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                RequestId = requestId
            };
        }
    }
    
    /// <summary>
    /// 脚本系统结果
    /// </summary>
    public class ScriptResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 脚本输出
        /// </summary>
        public string Output { get; set; } = string.Empty;
        
        /// <summary>
        /// 执行状态
        /// </summary>
        public string ExecutionStatus { get; set; } = "Completed";
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ScriptResult SuccessResult(string output = "", string requestId = "")
        {
            return new ScriptResult
            {
                Success = true,
                Output = output,
                RequestId = requestId
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ScriptResult FailureResult(string errorMessage, string requestId = "")
        {
            return new ScriptResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                RequestId = requestId
            };
        }
    }
    
    /// <summary>
    /// 配置系统结果
    /// </summary>
    public class ConfigurationResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 配置数据
        /// </summary>
        public Dictionary<string, object> ConfigurationData { get; set; } = new();
        
        /// <summary>
        /// 验证结果
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ConfigurationResult SuccessResult(Dictionary<string, object> configData = null, string requestId = "")
        {
            return new ConfigurationResult
            {
                Success = true,
                ConfigurationData = configData ?? new Dictionary<string, object>(),
                IsValid = true,
                RequestId = requestId
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ConfigurationResult FailureResult(string errorMessage, string requestId = "")
        {
            return new ConfigurationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                RequestId = requestId
            };
        }
    }
}