using System;
using System.Collections.Generic;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Core
{
    /// <summary>
    /// 平台能力
    /// </summary>
    public class PlatformCapabilities
    {
        /// <summary>
        /// 是否支持全局热键
        /// </summary>
        public bool SupportsGlobalHotkeys { get; set; }
        
        /// <summary>
        /// 是否支持低级别键盘钩子
        /// </summary>
        public bool SupportsLowLevelKeyboardHook { get; set; }
        
        /// <summary>
        /// 是否支持低级别鼠标钩子
        /// </summary>
        public bool SupportsLowLevelMouseHook { get; set; }
        
        /// <summary>
        /// 是否支持多显示器
        /// </summary>
        public bool SupportsMultipleDisplays { get; set; }
        
        /// <summary>
        /// 是否支持窗口透明度
        /// </summary>
        public bool SupportsWindowTransparency { get; set; }
        
        /// <summary>
        /// 是否支持窗口置顶
        /// </summary>
        public bool SupportsWindowTopmost { get; set; }
        
        /// <summary>
        /// 是否支持屏幕录制
        /// </summary>
        public bool SupportsScreenRecording { get; set; }
        
        /// <summary>
        /// 是否支持辅助功能
        /// </summary>
        public bool SupportsAccessibility { get; set; }
    }
    
    /// <summary>
    /// 热键选项
    /// </summary>
    public class HotkeyOptions
    {
        /// <summary>
        /// 是否启用热键冲突检测
        /// </summary>
        public bool EnableConflictDetection { get; set; } = true;
        
        /// <summary>
        /// 是否启用热键优先级管理
        /// </summary>
        public bool EnablePriorityManagement { get; set; } = true;
        
        /// <summary>
        /// 最大注册热键数量
        /// </summary>
        public int MaxRegisteredHotkeys { get; set; } = 100;
        
        /// <summary>
        /// 热键冲突解决策略
        /// </summary>
        public HotkeyConflictResolution ConflictResolution { get; set; } = HotkeyConflictResolution.KeepExisting;
        
        /// <summary>
        /// 默认热键优先级
        /// </summary>
        public int DefaultPriority { get; set; } = 50;
    }
    
    /// <summary>
    /// 热键冲突解决策略
    /// </summary>
    public enum HotkeyConflictResolution
    {
        /// <summary>
        /// 保留现有的
        /// </summary>
        KeepExisting,
        
        /// <summary>
        /// 替换现有的
        /// </summary>
        ReplaceExisting,
        
        /// <summary>
        /// 抛出异常
        /// </summary>
        ThrowException
    }
}