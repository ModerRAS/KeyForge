namespace KeyForge.Abstractions.Models.Input
{
    /// <summary>
    /// 键修饰符枚举
    /// 【优化实现】定义统一的键修饰符，支持跨平台输入处理
    /// 原实现：平台特定的修饰符定义，缺乏统一规范
    /// 优化：通过枚举统一修饰符定义，提高跨平台兼容性
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        /// <summary>
        /// 无修饰符
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Shift键
        /// </summary>
        Shift = 1,
        
        /// <summary>
        /// Control键
        /// </summary>
        Control = 2,
        
        /// <summary>
        /// Alt键
        /// </summary>
        Alt = 4,
        
        /// <summary>
        /// Windows键
        /// </summary>
        Windows = 8,
        
        /// <summary>
        /// Command键 (macOS)
        /// </summary>
        Command = 16,
        
        /// <summary>
        /// Ctrl+Shift组合
        /// </summary>
        CtrlShift = Shift | Control,
        
        /// <summary>
        /// Ctrl+Alt组合
        /// </summary>
        CtrlAlt = Control | Alt,
        
        /// <summary>
        /// Shift+Alt组合
        /// </summary>
        ShiftAlt = Shift | Alt,
        
        /// <summary>
        /// Ctrl+Shift+Alt组合
        /// </summary>
        CtrlShiftAlt = Shift | Control | Alt
    }
    
    /// <summary>
    /// 输入设备类型枚举
    /// </summary>
    public enum InputDeviceType
    {
        /// <summary>
        /// 键盘
        /// </summary>
        Keyboard = 0,
        
        /// <summary>
        /// 鼠标
        /// </summary>
        Mouse = 1,
        
        /// <summary>
        /// 触摸屏
        /// </summary>
        TouchScreen = 2,
        
        /// <summary>
        /// 手柄
        /// </summary>
        Gamepad = 3,
        
        /// <summary>
        /// 触摸板
        /// </summary>
        Touchpad = 4,
        
        /// <summary>
        /// 手写笔
        /// </summary>
        Stylus = 5,
        
        /// <summary>
        /// 自定义设备
        /// </summary>
        Custom = 6
    }
    
    /// <summary>
    /// 输入设备状态枚举
    /// </summary>
    public enum InputDeviceStatus
    {
        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 0,
        
        /// <summary>
        /// 已断开
        /// </summary>
        Disconnected = 1,
        
        /// <summary>
        /// 正在初始化
        /// </summary>
        Initializing = 2,
        
        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// 不支持
        /// </summary>
        NotSupported = 4
    }
    
    /// <summary>
    /// 输入事件类型枚举
    /// </summary>
    public enum InputEventType
    {
        /// <summary>
        /// 键按下
        /// </summary>
        KeyDown = 0,
        
        /// <summary>
        /// 键释放
        /// </summary>
        KeyUp = 1,
        
        /// <summary>
        /// 鼠标按下
        /// </summary>
        MouseDown = 2,
        
        /// <summary>
        /// 鼠标释放
        /// </summary>
        MouseUp = 3,
        
        /// <summary>
        /// 鼠标移动
        /// </summary>
        MouseMove = 4,
        
        /// <summary>
        /// 鼠标滚轮
        /// </summary>
        MouseWheel = 5,
        
        /// <summary>
        /// 触摸开始
        /// </summary>
        TouchStart = 6,
        
        /// <summary>
        /// 触摸移动
        /// </summary>
        TouchMove = 7,
        
        /// <summary>
        /// 触摸结束
        /// </summary>
        TouchEnd = 8,
        
        /// <summary>
        /// 热键触发
        /// </summary>
        HotkeyTriggered = 9
    }
}