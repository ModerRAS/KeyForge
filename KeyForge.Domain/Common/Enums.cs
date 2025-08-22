using System;

namespace KeyForge.Domain.Common
{
    /// <summary>
    /// 按键代码枚举 - 统一定义
    /// 这是简化实现，原本有多个文件中重复定义KeyCode
    /// 现在统一在Domain层定义，其他层引用此定义
    /// </summary>
    public enum KeyCode
    {
        // 无按键
        None = 0x00,
        
        // 鼠标按钮
        LButton = 0x01,
        RButton = 0x02,
        Cancel = 0x03,
        MButton = 0x04,
        
        // 字母键
        A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46,
        G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C,
        M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52,
        S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58,
        Y = 0x59, Z = 0x5A,
        
        // 数字键
        D0 = 0x30, D1 = 0x31, D2 = 0x32, D3 = 0x33, D4 = 0x34,
        D5 = 0x35, D6 = 0x36, D7 = 0x37, D8 = 0x38, D9 = 0x39,
        
        // 功能键
        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74,
        F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79,
        F11 = 0x7A, F12 = 0x7B,
        
        // 控制键
        Shift = 0x10, Control = 0x11, Alt = 0x12,
        Enter = 0x0D, Escape = 0x1B, Space = 0x20, Tab = 0x09,
        CapsLock = 0x14, NumLock = 0x90,
        
        // 方向键
        Up = 0x26, Down = 0x28, Left = 0x25, Right = 0x27,
        
        // 其他键
        Back = 0x08, Delete = 0x2E, Insert = 0x2D,
        Home = 0x24, End = 0x23, PageUp = 0x21, PageDown = 0x22,
        
        // 小键盘
        NumPad0 = 0x60, NumPad1 = 0x61, NumPad2 = 0x62, NumPad3 = 0x63,
        NumPad4 = 0x64, NumPad5 = 0x65, NumPad6 = 0x66, NumPad7 = 0x67,
        NumPad8 = 0x68, NumPad9 = 0x69, Multiply = 0x6A, Add = 0x6B,
        Separator = 0x6C, Subtract = 0x6D, Decimal = 0x6E, Divide = 0x6F
    }

    /// <summary>
    /// 鼠标按钮枚举 - 统一定义
    /// 这是简化实现，原本有多个文件中重复定义MouseButton
    /// 现在统一在Domain层定义，其他层引用此定义
    /// </summary>
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3
    }

    /// <summary>
    /// 动作类型枚举 - 统一定义
    /// 这是简化实现，原本有多个文件中重复定义ActionType
    /// 现在统一在Domain层定义，其他层引用此定义
    /// </summary>
    public enum ActionType
    {
        KeyDown = 0,
        KeyUp = 1,
        MouseMove = 2,
        MouseDown = 3,
        MouseUp = 4,
        Delay = 5,
        MouseClick = 6,
        MouseDoubleClick = 7,
        MouseRightClick = 8
    }

    /// <summary>
    /// 按键状态枚举 - 统一定义
    /// 这是简化实现，原本有多个文件中重复定义KeyState
    /// 现在统一在Domain层定义，其他层引用此定义
    /// </summary>
    public enum KeyState
    {
        Press = 0,
        Release = 1
    }

    /// <summary>
    /// 鼠标状态枚举 - 统一定义
    /// 这是简化实现，原本有多个文件中重复定义MouseState
    /// 现在统一在Domain层定义，其他层引用此定义
    /// </summary>
    public enum MouseState
    {
        Down = 0,
        Up = 1
    }

    /// <summary>
    /// 脚本状态枚举
    /// </summary>
    public enum ScriptStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    /// <summary>
    /// 识别状态枚举
    /// </summary>
    public enum RecognitionStatus
    {
        Success = 0,
        Failed = 1,
        Partial = 2,
        Timeout = 3
    }

    /// <summary>
    /// 识别方法枚举
    /// </summary>
    public enum RecognitionMethod
    {
        TemplateMatching = 0,
        FeatureMatching = 1,
        OCR = 2,
        ColorDetection = 3
    }

    /// <summary>
    /// 鼠标动作类型枚举
    /// </summary>
    public enum MouseActionType
    {
        Move = 0,
        Click = 1,
        DoubleClick = 2,
        RightClick = 3,
        DragStart = 4,
        DragEnd = 5,
        Scroll = 6
    }

    /// <summary>
    /// 状态机状态枚举
    /// </summary>
    public enum StateMachineStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    /// <summary>
    /// 模板类型枚举
    /// </summary>
    public enum TemplateType
    {
        Image = 0,
        Color = 1,
        Text = 2
    }

    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    /// <summary>
    /// 虚拟键码枚举（完整版本）
    /// 这是简化实现，原本使用Windows API的虚拟键码
    /// 现在统一在Domain层定义，确保跨平台兼容性
    /// </summary>
    public enum VirtualKeyCode
    {
        // 常用键
        None = 0x00,
        LButton = 0x01,
        RButton = 0x02,
        Cancel = 0x03,
        MButton = 0x04,
        XButton1 = 0x05,
        XButton2 = 0x06,
        
        // 字母键 (0x41-0x5A)
        A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46,
        G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C,
        M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52,
        S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58,
        Y = 0x59, Z = 0x5A,
        
        // 数字键 (0x30-0x39)
        D0 = 0x30, D1 = 0x31, D2 = 0x32, D3 = 0x33, D4 = 0x34,
        D5 = 0x35, D6 = 0x36, D7 = 0x37, D8 = 0x38, D9 = 0x39,
        
        // 功能键 (0x70-0x87)
        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74,
        F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79,
        F11 = 0x7A, F12 = 0x7B, F13 = 0x7C, F14 = 0x7D, F15 = 0x7E,
        F16 = 0x7F, F17 = 0x80, F18 = 0x81, F19 = 0x82, F20 = 0x83,
        F21 = 0x84, F22 = 0x85, F23 = 0x86, F24 = 0x87,
        
        // 控制键
        Shift = 0x10,
        Control = 0x11,
        Alt = 0x12,
        CapsLock = 0x14,
        NumLock = 0x90,
        ScrollLock = 0x91,
        
        // 特殊键
        Enter = 0x0D,
        Escape = 0x1B,
        Space = 0x20,
        Tab = 0x09,
        Backspace = 0x08,
        Insert = 0x2D,
        Delete = 0x2E,
        Home = 0x24,
        End = 0x23,
        PageUp = 0x21,
        PageDown = 0x22,
        
        // 方向键
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
        
        // 小键盘 (0x60-0x6F)
        NumPad0 = 0x60, NumPad1 = 0x61, NumPad2 = 0x62, NumPad3 = 0x63,
        NumPad4 = 0x64, NumPad5 = 0x65, NumPad6 = 0x66, NumPad7 = 0x67,
        NumPad8 = 0x68, NumPad9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    /// <summary>
    /// 配置类型枚举
    /// </summary>
    public enum ConfigType
    {
        String = 0,
        Integer = 1,
        Boolean = 2,
        Double = 3,
        DateTime = 4,
        Json = 5
    }

    /// <summary>
    /// 热键类型枚举
    /// </summary>
    public enum HotkeyType
    {
        Global = 0,
        Application = 1,
        Window = 2
    }

    /// <summary>
    /// 窗口状态枚举
    /// </summary>
    public enum WindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2,
        Hidden = 3
    }

    /// <summary>
    /// 性能指标类型枚举
    /// </summary>
    public enum PerformanceMetricType
    {
        Counter = 0,
        Timer = 1,
        Gauge = 2,
        Histogram = 3
    }
}