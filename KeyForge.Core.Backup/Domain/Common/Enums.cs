namespace KeyForge.Core.Domain.Common
{
    /// <summary>
    /// Core层枚举定义 - 引用Domain层的统一定义
    /// 这是简化实现，避免重复定义和循环依赖
    /// 所有枚举定义都统一在Domain层，Core层直接使用Domain层的定义
    /// </summary>
    
    // Core层直接使用Domain层的枚举定义，避免重复
    // 使用using别名简化引用
    
    /// <summary>
    /// Core层脚本状态 - 直接使用Domain层的ScriptStatus
    /// </summary>
    // public enum ScriptStatus - 使用Domain.ScriptStatus替代
    
    /// <summary>
    /// Core层识别状态 - 直接使用Domain层的RecognitionStatus  
    /// </summary>
    // public enum RecognitionStatus - 使用Domain.RecognitionStatus替代
    
    /// <summary>
    /// Core层识别方法 - 直接使用Domain层的RecognitionMethod
    /// </summary>
    // public enum RecognitionMethod - 使用Domain.RecognitionMethod替代
    
    /// <summary>
    /// Core层鼠标动作类型 - 直接使用Domain层的MouseActionType
    /// </summary>
    // public enum MouseActionType - 使用Domain.MouseActionType替代
    
    /// <summary>
    /// Core层鼠标按钮 - 直接使用Domain层的MouseButton
    /// </summary>
    // public enum MouseButton - 使用Domain.MouseButton替代
    
    /// <summary>
    /// Core层键盘状态 - 直接使用Domain层的KeyState
    /// </summary>
    // public enum KeyState - 使用Domain.KeyState替代
    
    /// <summary>
    /// Core层虚拟键码 - 直接使用Domain层的VirtualKeyCode
    /// </summary>
    // public enum VirtualKeyCode - 使用Domain.VirtualKeyCode替代
    
    /// <summary>
    /// Core层特有的类型定义
    /// 这里只定义Core层特有的类型，其他都使用Domain层的定义
    /// </summary>
    
    /// <summary>
    /// 核心服务状态
    /// </summary>
    public enum CoreServiceStatus
    {
        Stopped = 0,
        Starting = 1,
        Running = 2,
        Stopping = 3,
        Error = 4
    }
    
    /// <summary>
    /// 输入处理优先级
    /// </summary>
    public enum InputPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// 执行模式
    /// </summary>
    public enum ExecutionMode
    {
        Sync = 0,
        Async = 1,
        Parallel = 2
    }
}