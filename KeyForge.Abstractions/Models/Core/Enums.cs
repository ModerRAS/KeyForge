namespace KeyForge.Abstractions.Models.Core
{
    /// <summary>
    /// 服务状态枚举
    /// 【优化实现】定义统一的服务状态，便于状态管理
    /// 原实现：状态定义分散，缺乏统一规范
    /// 优化：通过枚举统一状态定义，提高代码规范性
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        NotInitialized = 0,
        
        /// <summary>
        /// 正在初始化
        /// </summary>
        Initializing = 1,
        
        /// <summary>
        /// 已就绪
        /// </summary>
        Ready = 2,
        
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 3,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 4,
        
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped = 5,
        
        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 6,
        
        /// <summary>
        /// 已销毁
        /// </summary>
        Disposed = 7
    }
    
    /// <summary>
    /// HAL类型枚举
    /// </summary>
    public enum HALType
    {
        /// <summary>
        /// 输入HAL
        /// </summary>
        Input = 0,
        
        /// <summary>
        /// 图形HAL
        /// </summary>
        Graphics = 1,
        
        /// <summary>
        /// 系统HAL
        /// </summary>
        System = 2,
        
        /// <summary>
        /// 音频HAL
        /// </summary>
        Audio = 3,
        
        /// <summary>
        /// 网络HAL
        /// </summary>
        Network = 4
    }
    
    /// <summary>
    /// HAL状态枚举
    /// </summary>
    public enum HALStatus
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        NotInitialized = 0,
        
        /// <summary>
        /// 正在初始化
        /// </summary>
        Initializing = 1,
        
        /// <summary>
        /// 已就绪
        /// </summary>
        Ready = 2,
        
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 3,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 4,
        
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped = 5,
        
        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 6
    }
    
    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NotStarted = 0,
        
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 1,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 2,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 3,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed = 4,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 5
    }
    
    /// <summary>
    /// 进程状态枚举
    /// </summary>
    public enum ProcessState
    {
        /// <summary>
        /// 未运行
        /// </summary>
        NotRunning = 0,
        
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 1,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Suspended = 2,
        
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped = 3,
        
        /// <summary>
        /// 已崩溃
        /// </summary>
        Crashed = 4
    }
    
    /// <summary>
    /// 系统事件类型枚举
    /// </summary>
    public enum SystemEventType
    {
        /// <summary>
        /// 进程启动
        /// </summary>
        ProcessStarted = 0,
        
        /// <summary>
        /// 进程结束
        /// </summary>
        ProcessExited = 1,
        
        /// <summary>
        /// 系统休眠
        /// </summary>
        SystemSuspend = 2,
        
        /// <summary>
        /// 系统唤醒
        /// </summary>
        SystemResume = 3,
        
        /// <summary>
        /// 显示器变化
        /// </summary>
        DisplayChanged = 4,
        
        /// <summary>
        /// 用户会话变化
        /// </summary>
        SessionChanged = 5
    }
}