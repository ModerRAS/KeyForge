using System;
using System.Collections.Generic;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Core
{
    /// <summary>
    /// HAL配置类
    /// </summary>
    public class HALConfiguration
    {
        /// <summary>
        /// 平台类型
        /// </summary>
        public Platform Platform { get; set; }
        
        /// <summary>
        /// 键盘选项
        /// </summary>
        public KeyboardOptions KeyboardOptions { get; set; }
        
        /// <summary>
        /// 鼠标选项
        /// </summary>
        public MouseOptions MouseOptions { get; set; }
        
        /// <summary>
        /// 屏幕选项
        /// </summary>
        public ScreenOptions ScreenOptions { get; set; }
        
        /// <summary>
        /// 热键选项
        /// </summary>
        public HotkeyOptions HotkeyOptions { get; set; }
        
        /// <summary>
        /// 窗口选项
        /// </summary>
        public WindowOptions WindowOptions { get; set; }
        
        /// <summary>
        /// 图像识别选项
        /// </summary>
        public ImageRecognitionOptions ImageRecognitionOptions { get; set; }
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool EnableDebugMode { get; set; }
        
        /// <summary>
        /// 是否启用性能监控
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; }
        
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        
        /// <summary>
        /// 自定义配置
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// 获取自定义配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public T GetCustomSetting<T>(string key, T defaultValue = default!)
        {
            if (CustomSettings.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 设置自定义配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetCustomSetting<T>(string key, T value)
        {
            CustomSettings[key] = value;
        }
    }

    /// <summary>
    /// 键盘选项
    /// </summary>
    public class KeyboardOptions
    {
        /// <summary>
        /// 是否启用钩子
        /// </summary>
        public bool EnableHooks { get; set; } = true;
        
        /// <summary>
        /// 是否启用模拟输入
        /// </summary>
        public bool EnableSimulation { get; set; } = true;
        
        /// <summary>
        /// 按键延迟（毫秒）
        /// </summary>
        public int KeyDelay { get; set; } = 10;
        
        /// <summary>
        /// 是否启用Unicode输入
        /// </summary>
        public bool EnableUnicode { get; set; } = true;
        
        /// <summary>
        /// 是否启用重复按键检测
        /// </summary>
        public bool EnableRepeatDetection { get; set; } = true;
        
        /// <summary>
        /// 最大同时按键数
        /// </summary>
        public int MaxSimultaneousKeys { get; set; } = 10;
        
        /// <summary>
        /// 键盘布局
        /// </summary>
        public string KeyboardLayout { get; set; } = "US";
    }

    /// <summary>
    /// 鼠标选项
    /// </summary>
    public class MouseOptions
    {
        /// <summary>
        /// 是否启用钩子
        /// </summary>
        public bool EnableHooks { get; set; } = true;
        
        /// <summary>
        /// 是否启用模拟输入
        /// </summary>
        public bool EnableSimulation { get; set; } = true;
        
        /// <summary>
        /// 是否启用平滑移动
        /// </summary>
        public bool EnableSmoothMovement { get; set; } = true;
        
        /// <summary>
        /// 是否启用拖拽
        /// </summary>
        public bool EnableDragging { get; set; } = true;
        
        /// <summary>
        /// 鼠标速度（0.1-2.0）
        /// </summary>
        public double MouseSpeed { get; set; } = 1.0;
        
        /// <summary>
        /// 滚轮速度
        /// </summary>
        public double WheelSpeed { get; set; } = 1.0;
        
        /// <summary>
        /// 双击间隔（毫秒）
        /// </summary>
        public int DoubleClickInterval { get; set; } = 500;
        
        /// <summary>
        /// 平滑移动持续时间（毫秒）
        /// </summary>
        public int SmoothMoveDuration { get; set; } = 100;
    }

    /// <summary>
    /// 屏幕选项
    /// </summary>
    public class ScreenOptions
    {
        /// <summary>
        /// 默认图像格式
        /// </summary>
        public ImageFormat DefaultImageFormat { get; set; } = ImageFormat.Png;
        
        /// <summary>
        /// 默认图像质量（1-100）
        /// </summary>
        public int DefaultImageQuality { get; set; } = 90;
        
        /// <summary>
        /// 是否包含光标
        /// </summary>
        public bool IncludeCursor { get; set; } = false;
        
        /// <summary>
        /// 是否启用多显示器支持
        /// </summary>
        public bool EnableMultipleDisplays { get; set; } = true;
        
        /// <summary>
        /// 默认DPI缩放
        /// </summary>
        public double? DefaultDpiScale { get; set; }
        
        /// <summary>
        /// 颜色容差
        /// </summary>
        public int ColorTolerance { get; set; } = 10;
        
        /// <summary>
        /// 截图缓存大小
        /// </summary>
        public int ScreenshotCacheSize { get; set; } = 10;
    }

    /// <summary>
    /// 窗口选项
    /// </summary>
    public class WindowOptions
    {
        /// <summary>
        /// 是否启用窗口事件监听
        /// </summary>
        public bool EnableWindowEvents { get; set; } = true;
        
        /// <summary>
        /// 默认窗口透明度（0.0-1.0）
        /// </summary>
        public double DefaultOpacity { get; set; } = 1.0;
        
        /// <summary>
        /// 是否启用窗口动画
        /// </summary>
        public bool EnableWindowAnimation { get; set; } = true;
        
        /// <summary>
        /// 窗口查找超时（毫秒）
        /// </summary>
        public int WindowFindTimeout { get; set; } = 5000;
        
        /// <summary>
        /// 是否启用窗口状态缓存
        /// </summary>
        public bool EnableWindowStateCache { get; set; } = true;
        
        /// <summary>
        /// 默认窗口样式
        /// </summary>
        public WindowStyle DefaultWindowStyle { get; set; } = WindowStyle.Default;
    }

    /// <summary>
    /// 图像识别选项
    /// </summary>
    public class ImageRecognitionOptions
    {
        /// <summary>
        /// 识别精度
        /// </summary>
        public RecognitionAccuracy Accuracy { get; set; } = RecognitionAccuracy.Medium;
        
        /// <summary>
        /// 默认匹配阈值（0.0-1.0）
        /// </summary>
        public double DefaultMatchThreshold { get; set; } = 0.8;
        
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCache { get; set; } = true;
        
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int CacheSize { get; set; } = 100;
        
        /// <summary>
        /// 是否启用预处理
        /// </summary>
        public bool EnablePreprocessing { get; set; } = true;
        
        /// <summary>
        /// OCR语言
        /// </summary>
        public string OcrLanguage { get; set; } = "eng";
        
        /// <summary>
        /// 模板存储路径
        /// </summary>
        public string TemplateStoragePath { get; set; } = "./templates";
        
        /// <summary>
        /// 是否启用并行处理
        /// </summary>
        public bool EnableParallelProcessing { get; set; } = true;
    }

    /// <summary>
    /// 平台兼容性结果
    /// </summary>
    public class PlatformCompatibilityResult
    {
        /// <summary>
        /// 平台类型
        /// </summary>
        public Platform Platform { get; set; }
        
        /// <summary>
        /// 是否兼容
        /// </summary>
        public bool IsCompatible { get; set; }
        
        /// <summary>
        /// 支持的功能特性
        /// </summary>
        public PlatformFeatures SupportedFeatures { get; set; }
        
        /// <summary>
        /// 缺失的功能特性
        /// </summary>
        public PlatformFeatures MissingFeatures { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 兼容性评分（0-100）
        /// </summary>
        public int CompatibilityScore { get; set; }
    }

    /// <summary>
    /// 平台功能特性
    /// </summary>
    [Flags]
    public enum PlatformFeatures
    {
        /// <summary>
        /// 无功能
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 全局热键
        /// </summary>
        GlobalHotkeys = 1,
        
        /// <summary>
        /// 键盘钩子
        /// </summary>
        KeyboardHooks = 2,
        
        /// <summary>
        /// 鼠标钩子
        /// </summary>
        MouseHooks = 4,
        
        /// <summary>
        /// 多显示器
        /// </summary>
        MultipleDisplays = 8,
        
        /// <summary>
        /// 窗口透明度
        /// </summary>
        WindowTransparency = 16,
        
        /// <summary>
        /// 窗口置顶
        /// </summary>
        WindowTopmost = 32,
        
        /// <summary>
        /// 屏幕录制
        /// </summary>
        ScreenRecording = 64,
        
        /// <summary>
        /// 辅助功能
        /// </summary>
        Accessibility = 128,
        
        /// <summary>
        /// 所有功能
        /// </summary>
        All = GlobalHotkeys | KeyboardHooks | MouseHooks | MultipleDisplays | 
              WindowTransparency | WindowTopmost | ScreenRecording | Accessibility
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
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
}