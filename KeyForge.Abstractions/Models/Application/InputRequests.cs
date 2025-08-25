namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 输入系统请求基类
    /// 【优化实现】定义输入系统请求的基类，统一请求处理
    /// 原实现：请求类型分散定义，缺乏统一规范
    /// 优化：通过基类统一请求处理，提高代码规范性
    /// </summary>
    public abstract class InputRequestBase
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
        /// 请求来源
        /// </summary>
        public string Source { get; set; } = "Unknown";
    }
    
    /// <summary>
    /// 开始监听请求
    /// </summary>
    public class StartListeningRequest : InputRequestBase
    {
        /// <summary>
        /// 监听设备类型
        /// </summary>
        public string DeviceType { get; set; } = "Keyboard";
        
        /// <summary>
        /// 监听配置
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();
    }
    
    /// <summary>
    /// 停止监听请求
    /// </summary>
    public class StopListeningRequest : InputRequestBase
    {
        /// <summary>
        /// 停止原因
        /// </summary>
        public string Reason { get; set; } = "UserRequest";
    }
    
    /// <summary>
    /// 发送输入请求
    /// </summary>
    public class SendInputRequest : InputRequestBase
    {
        /// <summary>
        /// 输入类型
        /// </summary>
        public string InputType { get; set; }
        
        /// <summary>
        /// 输入数据
        /// </summary>
        public object InputData { get; set; }
        
        /// <summary>
        /// 目标窗口
        /// </summary>
        public string TargetWindow { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 注册热键请求
    /// </summary>
    public class RegisterHotkeyRequest : InputRequestBase
    {
        /// <summary>
        /// 热键组合
        /// </summary>
        public string HotkeyCombination { get; set; }
        
        /// <summary>
        /// 热键名称
        /// </summary>
        public string HotkeyName { get; set; }
        
        /// <summary>
        /// 回调动作
        /// </summary>
        public string CallbackAction { get; set; }
    }
    
    /// <summary>
    /// 注销热键请求
    /// </summary>
    public class UnregisterHotkeyRequest : InputRequestBase
    {
        /// <summary>
        /// 热键名称
        /// </summary>
        public string HotkeyName { get; set; }
    }
}