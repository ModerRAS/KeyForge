namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 脚本系统请求基类
    /// 【优化实现】定义脚本系统请求的基类，统一请求处理
    /// 原实现：请求类型分散定义，缺乏统一规范
    /// 优化：通过基类统一请求处理，提高代码规范性
    /// </summary>
    public abstract class ScriptRequestBase
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
        /// 脚本语言
        /// </summary>
        public string ScriptLanguage { get; set; } = "CSharp";
        
        /// <summary>
        /// 执行超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    }
    
    /// <summary>
    /// 开始录制请求
    /// </summary>
    public class StartRecordingRequest : ScriptRequestBase
    {
        /// <summary>
        /// 录制名称
        /// </summary>
        public string RecordingName { get; set; }
        
        /// <summary>
        /// 录制类型
        /// </summary>
        public string RecordingType { get; set; } = "Input";
        
        /// <summary>
        /// 录制配置
        /// </summary>
        public Dictionary<string, object> RecordingConfig { get; set; } = new();
    }
    
    /// <summary>
    /// 停止录制请求
    /// </summary>
    public class StopRecordingRequest : ScriptRequestBase
    {
        /// <summary>
        /// 录制标识
        /// </summary>
        public string RecordingId { get; set; }
        
        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePath { get; set; }
        
        /// <summary>
        /// 是否保存
        /// </summary>
        public bool Save { get; set; } = true;
    }
    
    /// <summary>
    /// 执行脚本请求
    /// </summary>
    public class ExecuteScriptRequest : ScriptRequestBase
    {
        /// <summary>
        /// 脚本内容
        /// </summary>
        public string ScriptContent { get; set; }
        
        /// <summary>
        /// 脚本参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// 执行模式
        /// </summary>
        public string ExecutionMode { get; set; } = "Synchronous";
        
        /// <summary>
        /// 安全上下文
        /// </summary>
        public string SecurityContext { get; set; } = "Default";
    }
    
    /// <summary>
    /// 调试脚本请求
    /// </summary>
    public class DebugScriptRequest : ScriptRequestBase
    {
        /// <summary>
        /// 脚本内容
        /// </summary>
        public string ScriptContent { get; set; }
        
        /// <summary>
        /// 断点设置
        /// </summary>
        public List<int> Breakpoints { get; set; } = new();
        
        /// <summary>
        /// 调试级别
        /// </summary>
        public string DebugLevel { get; set; } = "Normal";
        
        /// <summary>
        /// 调试参数
        /// </summary>
        public Dictionary<string, object> DebugParameters { get; set; } = new();
    }
    
    /// <summary>
    /// 管理脚本请求
    /// </summary>
    public class ManageScriptRequest : ScriptRequestBase
    {
        /// <summary>
        /// 管理操作
        /// </summary>
        public string ManagementOperation { get; set; }
        
        /// <summary>
        /// 脚本标识
        /// </summary>
        public string ScriptId { get; set; }
        
        /// <summary>
        /// 管理参数
        /// </summary>
        public Dictionary<string, object> ManagementParameters { get; set; } = new();
    }
}