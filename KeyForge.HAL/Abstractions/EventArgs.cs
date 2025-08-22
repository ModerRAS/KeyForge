namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 平台事件参数
/// </summary>
public class PlatformEventArgs : EventArgs
{
    /// <summary>
    /// 平台信息
    /// </summary>
    public PlatformInfo PlatformInfo { get; init; } = new();

    /// <summary>
    /// 变化类型
    /// </summary>
    public string ChangeType { get; init; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 硬件事件参数
/// </summary>
public class HardwareEventArgs : EventArgs
{
    /// <summary>
    /// 硬件类型
    /// </summary>
    public string HardwareType { get; init; } = string.Empty;

    /// <summary>
    /// 硬件状态
    /// </summary>
    public string HardwareState { get; init; } = string.Empty;

    /// <summary>
    /// 事件数据
    /// </summary>
    public Dictionary<string, object> EventData { get; init; } = new();

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 性能事件参数
/// </summary>
public class PerformanceEventArgs : EventArgs
{
    /// <summary>
    /// 性能指标
    /// </summary>
    public PerformanceMetrics Metrics { get; init; } = new();

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 键盘事件参数
/// </summary>
public class KeyboardEventArgs : EventArgs
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
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 是否是重复按键
    /// </summary>
    public bool IsRepeat { get; init; }
}

/// <summary>
/// 鼠标事件参数
/// </summary>
public class MouseEventArgs : EventArgs
{
    /// <summary>
    /// 鼠标位置
    /// </summary>
    public Point Position { get; init; }

    /// <summary>
    /// 鼠标按钮
    /// </summary>
    public MouseButton Button { get; init; }

    /// <summary>
    /// 按钮状态
    /// </summary>
    public MouseButtonState ButtonState { get; init; }

    /// <summary>
    /// 滚轮增量
    /// </summary>
    public int WheelDelta { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 热键事件参数
/// </summary>
public class HotkeyEventArgs : EventArgs
{
    /// <summary>
    /// 热键ID
    /// </summary>
    public int HotkeyId { get; init; }

    /// <summary>
    /// 修饰键
    /// </summary>
    public KeyCode[] Modifiers { get; init; } = Array.Empty<KeyCode>();

    /// <summary>
    /// 主键
    /// </summary>
    public KeyCode Key { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 性能告警事件参数
/// </summary>
public class PerformanceAlertEventArgs : EventArgs
{
    /// <summary>
    /// 告警级别
    /// </summary>
    public AlertLevel Level { get; init; }

    /// <summary>
    /// 告警消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 告警详情
    /// </summary>
    public Dictionary<string, object> Details { get; init; } = new();

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 图像匹配结果
/// </summary>
public class ImageMatchResult
{
    /// <summary>
    /// 匹配位置
    /// </summary>
    public Point Position { get; init; }

    /// <summary>
    /// 匹配置信度（0-1）
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// 匹配区域
    /// </summary>
    public Rectangle MatchArea { get; init; }

    /// <summary>
    /// 模板尺寸
    /// </summary>
    public Size TemplateSize { get; init; }

    /// <summary>
    /// 是否匹配成功
    /// </summary>
    public bool IsMatch => Confidence > 0.8;
}

/// <summary>
/// 窗口信息
/// </summary>
public class WindowInfo
{
    /// <summary>
    /// 窗口句柄
    /// </summary>
    public IntPtr Handle { get; init; }

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// 窗口类名
    /// </summary>
    public string ClassName { get; init; } = string.Empty;

    /// <summary>
    /// 窗口位置和大小
    /// </summary>
    public Rectangle Rectangle { get; init; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// 是否活动窗口
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// 热键信息
/// </summary>
public class HotkeyInfo
{
    /// <summary>
    /// 热键ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 修饰键
    /// </summary>
    public KeyCode[] Modifiers { get; init; } = Array.Empty<KeyCode>();

    /// <summary>
    /// 主键
    /// </summary>
    public KeyCode Key { get; init; }

    /// <summary>
    /// 热键描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisteredAt { get; init; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; init; }
}

/// <summary>
/// 图像信息
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// 图像格式
    /// </summary>
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// 图像大小（字节）
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// 颜色深度
    /// </summary>
    public int ColorDepth { get; init; }

    /// <summary>
    /// 水平分辨率
    /// </summary>
    public double HorizontalResolution { get; init; }

    /// <summary>
    /// 垂直分辨率
    /// </summary>
    public double VerticalResolution { get; init; }
}

/// <summary>
/// 告警级别枚举
/// </summary>
public enum AlertLevel
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
    /// 严重
    /// </summary>
    Critical
}

/// <summary>
/// 质量门禁事件参数
/// </summary>
public class QualityGateEventArgs : EventArgs
{
    /// <summary>
    /// 质量门禁结果
    /// </summary>
    public QualityGateResult Result { get; init; } = new();

    /// <summary>
    /// 质量门禁类型
    /// </summary>
    public QualityGateType GateType { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 诊断事件参数
/// </summary>
public class DiagnosticsEventArgs : EventArgs
{
    /// <summary>
    /// 诊断报告
    /// </summary>
    public DiagnosticsReport Report { get; init; } = new();

    /// <summary>
    /// 诊断级别
    /// </summary>
    public DiagnosticsLevel Level { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}