namespace KeyForge.Abstractions.Enums
{
    /// <summary>
    /// 按键代码枚举
    /// 【优化实现】统一了按键代码定义，支持跨平台按键映射
    /// 原实现：各平台使用不同的按键代码定义
    /// 优化：定义统一的按键代码枚举，便于跨平台映射
    /// </summary>
    public enum KeyCode
    {
        // 未知按键
        Unknown = 0,
        
        // 字母按键
        A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72,
        I = 73, J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80,
        Q = 81, R = 82, S = 83, T = 84, U = 85, V = 86, W = 87, X = 88,
        Y = 89, Z = 90,
        
        // 数字按键
        D0 = 48, D1 = 49, D2 = 50, D3 = 51, D4 = 52, D5 = 53, D6 = 54, D7 = 55, D8 = 56, D9 = 57,
        
        // 功能按键
        F1 = 112, F2 = 113, F3 = 114, F4 = 115, F5 = 116, F6 = 117, F7 = 118, F8 = 119,
        F9 = 120, F10 = 121, F11 = 122, F12 = 123, F13 = 124, F14 = 125, F15 = 126,
        F16 = 127, F17 = 128, F18 = 129, F19 = 130, F20 = 131, F21 = 132, F22 = 133,
        F23 = 134, F24 = 135,
        
        // 数字键盘
        NumPad0 = 96, NumPad1 = 97, NumPad2 = 98, NumPad3 = 99, NumPad4 = 100,
        NumPad5 = 101, NumPad6 = 102, NumPad7 = 103, NumPad8 = 104, NumPad9 = 105,
        NumPadMultiply = 106, NumPadAdd = 107, NumPadSeparator = 108, NumPadSubtract = 109,
        NumPadDecimal = 110, NumPadDivide = 111,
        
        // 特殊按键
        Back = 8, Tab = 9, Clear = 12, Enter = 13, Pause = 19, CapsLock = 20,
        Escape = 27, Space = 32, PageUp = 33, PageDown = 34, End = 35, Home = 36,
        LeftArrow = 37, UpArrow = 38, RightArrow = 39, DownArrow = 40, Select = 41,
        Print = 42, Execute = 43, PrintScreen = 44, Insert = 45, Delete = 46,
        Help = 47, Windows = 91, Command = 91, LeftWindows = 91, RightWindows = 92,
        Applications = 93, Sleep = 95,
        
        // 控制按键
        LeftShift = 160, RightShift = 161, LeftControl = 162, RightControl = 163,
        LeftAlt = 164, RightAlt = 165, LeftMenu = 164, RightMenu = 165,
        
        // 浏览器按键
        BrowserBack = 166, BrowserForward = 167, BrowserRefresh = 168, BrowserStop = 169,
        BrowserSearch = 170, BrowserFavorites = 171, BrowserHome = 172,
        
        // 音量按键
        VolumeMute = 173, VolumeDown = 174, VolumeUp = 175,
        
        // 媒体按键
        MediaNextTrack = 176, MediaPreviousTrack = 177, MediaStop = 178, MediaPlayPause = 179,
        
        // 邮件按键
        LaunchMail = 180, LaunchMediaSelect = 181, LaunchApp1 = 182, LaunchApp2 = 183,
        
        // 其他按键
        OEM1 = 186, OEM2 = 191, OEM3 = 192, OEM4 = 219, OEM5 = 220, OEM6 = 221,
        OEM7 = 222, OEM8 = 223, OEM102 = 226,
        
        // 额外按键
        ProcessKey = 229, Packet = 231, Attn = 246, CrSel = 247, ExSel = 248,
        EraseEOF = 249, Play = 250, Zoom = 251, PA1 = 253, OEMClear = 254
    }
    
    /// <summary>
    /// 按键状态枚举
    /// </summary>
    public enum KeyState
    {
        Down = 0,
        Up = 1,
        Pressed = 2,
        Released = 3
    }
    
    /// <summary>
    /// 修饰键枚举
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4,
        Windows = 8,
        Command = 8,
        CapsLock = 16,
        NumLock = 32,
        ScrollLock = 64
    }
    
    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        XButton1 = 4,
        XButton2 = 5,
        XButton3 = 6,
        XButton4 = 7,
        XButton5 = 8
    }
    
    /// <summary>
    /// 鼠标状态枚举
    /// </summary>
    public enum MouseState
    {
        None = 0,
        Down = 1,
        Up = 2,
        DoubleClick = 3,
        Move = 4,
        Wheel = 5
    }
    
    /// <summary>
    /// 点击类型枚举
    /// </summary>
    public enum ClickType
    {
        Single = 0,
        Double = 1,
        Triple = 2
    }
    
    /// <summary>
    /// 输入事件类型枚举
    /// </summary>
    public enum InputEventType
    {
        None = 0,
        KeyDown = 1,
        KeyUp = 2,
        KeyPress = 3,
        MouseDown = 4,
        MouseUp = 5,
        MouseMove = 6,
        MouseClick = 7,
        MouseDoubleClick = 8,
        MouseWheel = 9,
        HotkeyPressed = 10,
        HotkeyReleased = 11
    }
    
    /// <summary>
    /// 输入设备类型枚举
    /// </summary>
    public enum InputDeviceType
    {
        Unknown = 0,
        Keyboard = 1,
        Mouse = 2,
        Touch = 3,
        Pen = 4,
        Gamepad = 5,
        Joystick = 6,
        SteeringWheel = 7,
        FlightStick = 8,
        DancePad = 9,
        Guitar = 10,
        DrumKit = 11,
        UnknownController = 12
    }
    
    /// <summary>
    /// 输入设备状态枚举
    /// </summary>
    public enum InputDeviceStatus
    {
        Unknown = 0,
        Connected = 1,
        Disconnected = 2,
        Error = 3,
        Initializing = 4,
        Ready = 5,
        Busy = 6
    }
    
    /// <summary>
    /// 服务状态枚举
    /// </summary>
    public enum ServiceStatus
    {
        Unknown = 0,
        Stopped = 1,
        Starting = 2,
        Running = 3,
        Stopping = 4,
        Error = 5,
        Paused = 6
    }
    
    /// <summary>
    /// HAL类型枚举
    /// </summary>
    public enum HALType
    {
        Unknown = 0,
        Windows = 1,
        Linux = 2,
        macOS = 3,
        Android = 4,
        iOS = 5
    }
    
    /// <summary>
    /// HAL状态枚举
    /// </summary>
    public enum HALStatus
    {
        Unknown = 0,
        Uninitialized = 1,
        Initializing = 2,
        Ready = 3,
        Running = 4,
        Stopping = 5,
        Stopped = 6,
        Error = 7
    }
    
    /// <summary>
    /// 脚本状态枚举
    /// </summary>
    public enum ScriptStatus
    {
        Unknown = 0,
        Created = 1,
        Ready = 2,
        Running = 3,
        Paused = 4,
        Stopped = 5,
        Completed = 6,
        Error = 7
    }
    
    /// <summary>
    /// 录制状态枚举
    /// </summary>
    public enum RecordingStatus
    {
        NotRecording = 0,
        Recording = 1,
        Paused = 2,
        Stopped = 3
    }
    
    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        NotRunning = 0,
        Running = 1,
        Paused = 2,
        Stopped = 3,
        Completed = 4,
        Error = 5,
        Debugging = 6
    }
    
    /// <summary>
    /// 图像识别状态枚举
    /// </summary>
    public enum RecognitionStatus
    {
        Unknown = 0,
        Ready = 1,
        Processing = 2,
        Success = 3,
        Failed = 4,
        Timeout = 5,
        Error = 6
    }
    
    /// <summary>
    /// 系统事件类型枚举
    /// </summary>
    public enum SystemEventType
    {
        None = 0,
        ProcessStart = 1,
        ProcessStop = 2,
        ProcessChange = 3,
        WindowChange = 4,
        DisplayChange = 5,
        PowerChange = 6,
        NetworkChange = 7,
        DeviceChange = 8,
        UserSessionChange = 9,
        SystemShutdown = 10,
        SystemStartup = 11,
        SystemSleep = 12,
        SystemWake = 13
    }
    
    /// <summary>
    /// 进程状态枚举
    /// </summary>
    public enum ProcessState
    {
        Unknown = 0,
        Running = 1,
        Suspended = 2,
        Stopped = 3,
        Zombie = 4,
        Dead = 5
    }
    
    /// <summary>
    /// 动作类型枚举
    /// </summary>
    public enum ActionType
    {
        None = 0,
        KeyPress = 1,
        KeyRelease = 2,
        MouseClick = 3,
        MouseMove = 4,
        MouseDrag = 5,
        Delay = 6,
        ImageRecognition = 7,
        TextInput = 8,
        ScriptCall = 9,
        LoopStart = 10,
        LoopEnd = 11,
        Conditional = 12,
        FunctionCall = 13,
        VariableSet = 14,
        VariableGet = 15,
        HotkeyRegister = 16,
        HotkeyUnregister = 17,
        SystemCommand = 18,
        CustomAction = 19
    }
    
    /// <summary>
    /// 验证结果枚举
    /// </summary>
    public enum ValidationResult
    {
        Success = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }
}