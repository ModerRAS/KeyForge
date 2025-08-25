namespace KeyForge.Abstractions.Models.Input
{
    /// <summary>
    /// 键码枚举
    /// 【优化实现】定义统一的键码，支持跨平台输入处理
    /// 原实现：键码定义分散，跨平台兼容性差
    /// 优化：通过统一的键码枚举，提高跨平台兼容性
    /// </summary>
    public enum KeyCode
    {
        /// <summary>
        /// 未知键
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// 字母键 A-Z
        /// </summary>
        A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72,
        I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80,
        Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88,
        Y = 89, Z = 90,
        
        /// <summary>
        /// 数字键 0-9
        /// </summary>
        D0 = 48, D1 = 49, D2 = 50, D3 = 51, D4 = 52, D5 = 53, D6 = 54, D7 = 55, D8 = 56, D9 = 57,
        
        /// <summary>
        /// 功能键 F1-F12
        /// </summary>
        F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117,
        F7 = 118, F8 = 119, F9 = 120, F10 = 121, F11 = 122, F12 = 123,
        
        /// <summary>
        /// 特殊键
        /// </summary>
        Escape = 27,
        Tab = 9,
        CapsLock = 20,
        Shift = 16,
        Control = 17,
        Alt = 18,
        Space = 32,
        Enter = 13,
        Backspace = 8,
        Delete = 46,
        Insert = 45,
        Home = 36,
        End = 35,
        PageUp = 33,
        PageDown = 34,
        
        /// <summary>
        /// 方向键
        /// </summary>
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        
        /// <summary>
        /// 鼠标按钮
        /// </summary>
        MouseLeft = 1,
        MouseRight = 2,
        MouseMiddle = 3,
        MouseX1 = 4,
        MouseX2 = 5
    }
    
    /// <summary>
    /// 键状态枚举
    /// </summary>
    public enum KeyState
    {
        /// <summary>
        /// 键释放
        /// </summary>
        Released = 0,
        
        /// <summary>
        /// 键按下
        /// </summary>
        Pressed = 1,
        
        /// <summary>
        /// 键按下（别名）
        /// </summary>
        Down = 1,
        
        /// <summary>
        /// 键释放（别名）
        /// </summary>
        Up = 0
    }
    
    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// 无按钮
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 左键
        /// </summary>
        Left = 1,
        
        /// <summary>
        /// 右键
        /// </summary>
        Right = 2,
        
        /// <summary>
        /// 中键
        /// </summary>
        Middle = 3,
        
        /// <summary>
        /// X1键
        /// </summary>
        X1 = 4,
        
        /// <summary>
        /// X2键
        /// </summary>
        X2 = 5
    }
    
    /// <summary>
    /// 鼠标状态枚举
    /// </summary>
    public enum MouseState
    {
        /// <summary>
        /// 释放
        /// </summary>
        Released = 0,
        
        /// <summary>
        /// 按下
        /// </summary>
        Pressed = 1,
        
        /// <summary>
        /// 按下（别名）
        /// </summary>
        Down = 1,
        
        /// <summary>
        /// 释放（别名）
        /// </summary>
        Up = 0,
        
        /// <summary>
        /// 双击
        /// </summary>
        DoubleClicked = 2
    }
    
    /// <summary>
    /// 点击类型枚举
    /// </summary>
    public enum ClickType
    {
        /// <summary>
        /// 单击
        /// </summary>
        Single = 0,
        
        /// <summary>
        /// 双击
        /// </summary>
        Double = 1,
        
        /// <summary>
        /// 三击
        /// </summary>
        Triple = 2,
        
        /// <summary>
        /// 右击
        /// </summary>
        Right = 3,
        
        /// <summary>
        /// 中击
        /// </summary>
        Middle = 4,
        
        /// <summary>
        /// 长按
        /// </summary>
        Long = 5
    }
}