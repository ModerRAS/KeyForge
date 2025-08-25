using KeyForge.Abstractions.Models.Core;

namespace KeyForge.Abstractions.Models.Script
{
    /// <summary>
    /// 脚本模型
    /// 【优化实现】定义统一的脚本模型，支持多种脚本语言
    /// 原实现：脚本模型分散定义，缺乏统一规范
    /// 优化：通过统一的脚本模型，提高代码规范性
    /// </summary>
    public class Script
    {
        /// <summary>
        /// 脚本ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 脚本名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 脚本内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// 脚本语言
        /// </summary>
        public string Language { get; set; } = "CSharp";
        
        /// <summary>
        /// 脚本版本
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// 脚本描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty;
        
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 脚本参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
    
    /// <summary>
    /// 执行结果
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 输出内容
        /// </summary>
        public string Output { get; set; } = string.Empty;
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; } = string.Empty;
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }
        
        /// <summary>
        /// 执行状态
        /// </summary>
        public ExecutionStatus Status { get; set; }
        
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ExecutionResult SuccessResult(string output = "", object returnValue = null)
        {
            return new ExecutionResult
            {
                Success = true,
                Output = output,
                ReturnValue = returnValue,
                Status = ExecutionStatus.Completed
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ExecutionResult FailureResult(string error, Exception exception = null)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = error,
                Exception = exception,
                Status = ExecutionStatus.Failed
            };
        }
    }
    
    /// <summary>
    /// 调用堆栈帧
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; } = string.Empty;
        
        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// 列号
        /// </summary>
        public int ColumnNumber { get; set; }
        
        /// <summary>
        /// 偏移量
        /// </summary>
        public int Offset { get; set; }
        
        /// <summary>
        /// 是否为本机代码
        /// </summary>
        public bool IsNative { get; set; }
    }
}