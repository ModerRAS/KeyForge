using KeyForge.Abstractions.Models.Input;

namespace KeyForge.Abstractions.Models.Input
{
    /// <summary>
    /// 输入事件参数
    /// 【优化实现】统一了输入事件的参数模型，支持跨平台事件处理
    /// 原实现：各平台使用不同的事件参数格式
    /// 优化：定义统一的输入事件参数模型，便于跨平台处理
    /// </summary>
    public record InputEventArgs
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public InputEventType EventType { get; init; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; init; }
        
        /// <summary>
        /// 键盘事件数据
        /// </summary>
        public KeyEventData? KeyEvent { get; init; }
        
        /// <summary>
        /// 鼠标事件数据
        /// </summary>
        public MouseEventData? MouseEvent { get; init; }
        
        /// <summary>
        /// 热键事件数据
        /// </summary>
        public HotkeyEventData? HotkeyEvent { get; init; }
        
        /// <summary>
        /// 事件来源
        /// </summary>
        public string Source { get; init; } = string.Empty;
        
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled { get; set; }
    }
    
    /// <summary>
    /// 键盘事件数据
    /// </summary>
    public record KeyEventData
    {
        /// <summary>
        /// 按键代码
        /// </summary>
        public KeyCode KeyCode { get; init; }
        
        /// <summary>
        /// 按键状态
        /// </summary>
        public KeyState KeyState { get; init; }
        
        /// <summary>
        /// 修饰键
        /// </summary>
        public KeyModifiers Modifiers { get; init; }
        
        /// <summary>
        /// 重复次数
        /// </summary>
        public int RepeatCount { get; init; }
        
        /// <summary>
        /// 扫描码
        /// </summary>
        public int ScanCode { get; init; }
        
        /// <summary>
        /// 是否是扩展键
        /// </summary>
        public bool IsExtendedKey { get; init; }
    }
    
    /// <summary>
    /// 鼠标事件数据
    /// </summary>
    public record MouseEventData
    {
        /// <summary>
        /// 鼠标按钮
        /// </summary>
        public MouseButton Button { get; init; }
        
        /// <summary>
        /// 鼠标状态
        /// </summary>
        public MouseState MouseState { get; init; }
        
        /// <summary>
        /// X坐标
        /// </summary>
        public int X { get; init; }
        
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y { get; init; }
        
        /// <summary>
        /// 滚轮增量
        /// </summary>
        public int WheelDelta { get; init; }
        
        /// <summary>
        /// 移动增量X
        /// </summary>
        public int DeltaX { get; init; }
        
        /// <summary>
        /// 移动增量Y
        /// </summary>
        public int DeltaY { get; init; }
    }
    
    /// <summary>
    /// 热键事件数据
    /// </summary>
    public record HotkeyEventData
    {
        /// <summary>
        /// 热键ID
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// 按键代码
        /// </summary>
        public KeyCode KeyCode { get; init; }
        
        /// <summary>
        /// 修饰键
        /// </summary>
        public KeyModifiers Modifiers { get; init; }
        
        /// <summary>
        /// 热键名称
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        /// 是否是全局热键
        /// </summary>
        public bool IsGlobal { get; init; }
    }
    
    /// <summary>
    /// 热键事件参数
    /// </summary>
    public record HotkeyEventArgs
    {
        /// <summary>
        /// 热键ID
        /// </summary>
        public int Id { get; init; }
        
        /// <summary>
        /// 按键代码
        /// </summary>
        public KeyCode KeyCode { get; init; }
        
        /// <summary>
        /// 修饰键
        /// </summary>
        public KeyModifiers Modifiers { get; init; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; init; }
        
        /// <summary>
        /// 热键名称
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled { get; set; }
    }
    
    /// <summary>
    /// 输入设备信息
    /// </summary>
    public record InputDeviceInfo
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string Id { get; init; } = string.Empty;
        
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        /// 设备类型
        /// </summary>
        public InputDeviceType DeviceType { get; init; }
        
        /// <summary>
        /// 设备状态
        /// </summary>
        public InputDeviceStatus Status { get; init; }
        
        /// <summary>
        /// 设备功能
        /// </summary>
        public List<string> Capabilities { get; init; } = new();
        
        /// <summary>
        /// 连接类型
        /// </summary>
        public string ConnectionType { get; init; } = string.Empty;
        
        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; init; } = string.Empty;
        
        /// <summary>
        /// 产品ID
        /// </summary>
        public string ProductId { get; init; } = string.Empty;
        
        /// <summary>
        /// 驱动版本
        /// </summary>
        public string DriverVersion { get; init; } = string.Empty;
    }
    
    /// <summary>
    /// 输入历史记录
    /// </summary>
    public record InputHistoryRecord
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public Guid Id { get; init; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; init; }
        
        /// <summary>
        /// 事件类型
        /// </summary>
        public InputEventType EventType { get; init; }
        
        /// <summary>
        /// 事件数据
        /// </summary>
        public InputEventArgs EventData { get; init; } = new();
        
        /// <summary>
        /// 应用程序
        /// </summary>
        public string Application { get; init; } = string.Empty;
        
        /// <summary>
        /// 窗口标题
        /// </summary>
        public string WindowTitle { get; init; } = string.Empty;
        
        /// <summary>
        /// 用户会话
        /// </summary>
        public string UserSession { get; init; } = string.Empty;
        
        /// <summary>
        /// 处理结果
        /// </summary>
        public bool Processed { get; init; }
        
        /// <summary>
        /// 处理时间
        /// </summary>
        public TimeSpan ProcessingTime { get; init; }
    }
}