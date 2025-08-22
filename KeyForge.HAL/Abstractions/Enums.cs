namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 平台类型枚举
/// </summary>
public enum Platform
{
    /// <summary>
    /// Windows平台
    /// </summary>
    Windows,

    /// <summary>
    /// macOS平台
    /// </summary>
    MacOS,

    /// <summary>
    /// Linux平台
    /// </summary>
    Linux,

    /// <summary>
    /// 未知平台
    /// </summary>
    Unknown
}

/// <summary>
/// 按键代码枚举
/// </summary>
public enum KeyCode
{
    // 字母键
    A = 65, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

    // 数字键
    D0 = 48, D1, D2, D3, D4, D5, D6, D7, D8, D9,

    // 功能键
    F1 = 112, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,

    // 修饰键
    Shift = 16, Control = 17, Alt = 18, Windows = 91,

    // 方向键
    Left = 37, Up, Right, Down,

    // 特殊键
    Enter = 13, Escape = 27, Space = 32, Tab = 9, Backspace = 8,

    // 其他常用键
    Insert = 45, Delete = 46, Home = 36, End = 35, PageUp = 33, PageDown = 34,

    // 小键盘
    NumPad0 = 96, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
    NumPadMultiply = 106, NumPadAdd = 107, NumPadSubtract = 109, NumPadDecimal = 110, NumPadDivide = 111,

    // OEM键
    Oem1 = 186,  // ;:
    Oem2 = 191,  // /?
    Oem3 = 192,  // `~
    Oem4 = 219,  // [{
    Oem5 = 220,  // \|
    Oem6 = 221,  // ]}
    Oem7 = 222,  // '"
    Oemcomma = 188,  // ,<
    OemPeriod = 190,  // .>
    OemMinus = 189,  // -_
    Oemplus = 187,  // +=

    // 未知键
    Unknown = 0
}

/// <summary>
/// 按键状态枚举
/// </summary>
public enum KeyState
{
    /// <summary>
    /// 按键释放
    /// </summary>
    Up,

    /// <summary>
    /// 按键按下
    /// </summary>
    Down,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}

/// <summary>
/// 鼠标按钮枚举
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// 左键
    /// </summary>
    Left,

    /// <summary>
    /// 右键
    /// </summary>
    Right,

    /// <summary>
    /// 中键
    /// </summary>
    Middle,

    /// <summary>
    /// 未知按钮
    /// </summary>
    Unknown
}

/// <summary>
/// 鼠标按钮状态枚举
/// </summary>
public enum MouseButtonState
{
    /// <summary>
    /// 按钮释放
    /// </summary>
    Up,

    /// <summary>
    /// 按钮按下
    /// </summary>
    Down,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}

/// <summary>
/// HAL状态枚举
/// </summary>
public enum HALStatus
{
    /// <summary>
    /// 未初始化
    /// </summary>
    NotInitialized,

    /// <summary>
    /// 正在初始化
    /// </summary>
    Initializing,

    /// <summary>
    /// 已初始化
    /// </summary>
    Initialized,

    /// <summary>
    /// 运行中
    /// </summary>
    Running,

    /// <summary>
    /// 错误状态
    /// </summary>
    Error,

    /// <summary>
    /// 已关闭
    /// </summary>
    Shutdown
}

/// <summary>
/// 权限状态枚举
/// </summary>
public enum PermissionStatus
{
    /// <summary>
    /// 权限已授予
    /// </summary>
    Granted,

    /// <summary>
    /// 权限被拒绝
    /// </summary>
    Denied,

    /// <summary>
    /// 需要请求权限
    /// </summary>
    Required,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}

/// <summary>
/// 健康检查状态枚举
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// 健康
    /// </summary>
    Healthy,

    /// <summary>
    /// 降级
    /// </summary>
    Degraded,

    /// <summary>
    /// 不健康
    /// </summary>
    Unhealthy,

    /// <summary>
    /// 未知状态
    /// </summary>
    Unknown
}

/// <summary>
/// 日志级别枚举
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 跟踪
    /// </summary>
    Trace,

    /// <summary>
    /// 调试
    /// </summary>
    Debug,

    /// <summary>
    /// 信息
    /// </summary>
    Information,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 严重错误
    /// </summary>
    Critical
}

/// <summary>
/// 诊断级别枚举
/// </summary>
public enum DiagnosticsLevel
{
    /// <summary>
    /// 基本诊断
    /// </summary>
    Basic,

    /// <summary>
    /// 标准诊断
    /// </summary>
    Standard,

    /// <summary>
    /// 详细诊断
    /// </summary>
    Verbose,

    /// <summary>
    /// 完整诊断
    /// </summary>
    Full
}

/// <summary>
/// 质量问题类型枚举
/// </summary>
public enum QualityIssueType
{
    /// <summary>
    /// 编译错误
    /// </summary>
    CompilationError,

    /// <summary>
    /// 编译警告
    /// </summary>
    CompilationWarning,

    /// <summary>
    /// 测试覆盖率问题
    /// </summary>
    TestCoverage,

    /// <summary>
    /// 代码复杂度问题
    /// </summary>
    CodeComplexity,

    /// <summary>
    /// 代码重复问题
    /// </summary>
    CodeDuplication,

    /// <summary>
    /// 性能问题
    /// </summary>
    Performance,

    /// <summary>
    /// 安全问题
    /// </summary>
    Security,

    /// <summary>
    /// 依赖问题
    /// </summary>
    Dependency,

    /// <summary>
    /// 其他问题
    /// </summary>
    Other
}

/// <summary>
/// 质量问题严重程度枚举
/// </summary>
public enum QualityIssueSeverity
{
    /// <summary>
    /// 信息
    /// </summary>
    Info,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 严重错误
    /// </summary>
    Critical
}

/// <summary>
/// 质量门禁类型枚举
/// </summary>
public enum QualityGateType
{
    /// <summary>
    /// 编译门禁
    /// </summary>
    Compilation,

    /// <summary>
    /// 测试门禁
    /// </summary>
    Testing,

    /// <summary>
    /// 代码质量门禁
    /// </summary>
    CodeQuality,

    /// <summary>
    /// 性能门禁
    /// </summary>
    Performance,

    /// <summary>
    /// 安全门禁
    /// </summary>
    Security,

    /// <summary>
    /// 综合门禁
    /// </summary>
    Comprehensive
}