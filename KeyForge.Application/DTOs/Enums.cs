namespace KeyForge.Application.DTOs
{
    /// <summary>
    /// Application层DTO枚举定义
    /// 这是重构后的实现，Application层保持自己的DTO枚举定义
    /// 避免循环依赖，保持层次清晰
    /// </summary>
    
    /// <summary>
    /// 脚本状态DTO枚举
    /// </summary>
    public enum ScriptStatusDto
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    /// <summary>
    /// 动作类型DTO枚举
    /// </summary>
    public enum ActionTypeDto
    {
        KeyDown = 0,
        KeyUp = 1,
        MouseMove = 2,
        MouseDown = 3,
        MouseUp = 4,
        Delay = 5
    }

    /// <summary>
    /// 按键代码DTO枚举
    /// </summary>
    public enum KeyCodeDto
    {
        None = 0x00,
        A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46,
        G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C,
        M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52,
        S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58,
        Y = 0x59, Z = 0x5A,
        D0 = 0x30, D1 = 0x31, D2 = 0x32, D3 = 0x33, D4 = 0x34,
        D5 = 0x35, D6 = 0x36, D7 = 0x37, D8 = 0x38, D9 = 0x39,
        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74,
        F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79,
        F11 = 0x7A, F12 = 0x7B,
        Space = 0x20, Enter = 0x0D, Escape = 0x1B, Tab = 0x09,
        Shift = 0x10, Control = 0x11, Alt = 0x12,
        Left = 0x25, Up = 0x26, Right = 0x27, Down = 0x28,
        Backspace = 0x08, Delete = 0x2E, Insert = 0x2D,
        Home = 0x24, End = 0x23, PageUp = 0x21, PageDown = 0x22
    }

    /// <summary>
    /// 鼠标按钮DTO枚举
    /// </summary>
    public enum MouseButtonDto
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3
    }

    /// <summary>
    /// 状态机状态DTO枚举
    /// </summary>
    public enum StateMachineStatusDto
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    /// <summary>
    /// 模板类型DTO枚举
    /// </summary>
    public enum TemplateTypeDto
    {
        Image = 0,
        Color = 1,
        Text = 2
    }

    /// <summary>
    /// 执行状态DTO枚举
    /// </summary>
    public enum ExecutionStatusDto
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    /// <summary>
    /// 矩形DTO结构
    /// </summary>
    public struct RectangleDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public RectangleDto(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}