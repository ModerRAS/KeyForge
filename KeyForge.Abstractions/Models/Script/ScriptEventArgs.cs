using KeyForge.Abstractions.Models.Core;

namespace KeyForge.Abstractions.Models.Script
{
    /// <summary>
    /// 脚本状态事件参数
    /// </summary>
    public class ScriptStatusEventArgs : EventArgs
    {
        /// <summary>
        /// 脚本ID
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;
        
        /// <summary>
        /// 旧状态
        /// </summary>
        public ExecutionStatus OldStatus { get; set; }
        
        /// <summary>
        /// 新状态
        /// </summary>
        public ExecutionStatus NewStatus { get; set; }
        
        /// <summary>
        /// 事件时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 执行进度事件参数
    /// </summary>
    public class ExecutionProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 脚本ID
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;
        
        /// <summary>
        /// 进度百分比
        /// </summary>
        public double Progress { get; set; }
        
        /// <summary>
        /// 当前步骤
        /// </summary>
        public string CurrentStep { get; set; } = string.Empty;
        
        /// <summary>
        /// 总步骤数
        /// </summary>
        public int TotalSteps { get; set; }
        
        /// <summary>
        /// 当前步骤索引
        /// </summary>
        public int CurrentStepIndex { get; set; }
        
        /// <summary>
        /// 预计剩余时间
        /// </summary>
        public TimeSpan EstimatedRemainingTime { get; set; }
        
        /// <summary>
        /// 事件时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 录制动作
    /// </summary>
    public class RecordedAction
    {
        /// <summary>
        /// 动作类型
        /// </summary>
        public string ActionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 动作数据
        /// </summary>
        public object ActionData { get; set; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 延迟时间
        /// </summary>
        public TimeSpan Delay { get; set; }
        
        /// <summary>
        /// 坐标位置
        /// </summary>
        public Point Position { get; set; }
        
        /// <summary>
        /// 附加信息
        /// </summary>
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }
    
    /// <summary>
    /// 点坐标
    /// </summary>
    public class Point
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// 创建点
        /// </summary>
        public static Point Create(int x, int y)
        {
            return new Point { X = x, Y = y };
        }
    }
    
    /// <summary>
    /// 录制状态枚举
    /// </summary>
    public enum RecordingStatus
    {
        /// <summary>
        /// 未录制
        /// </summary>
        NotRecording = 0,
        
        /// <summary>
        /// 正在录制
        /// </summary>
        Recording = 1,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 2,
        
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped = 3,
        
        /// <summary>
        /// 已保存
        /// </summary>
        Saved = 4
    }
    
    /// <summary>
    /// 调试事件参数
    /// </summary>
    public class DebugEventArgs : EventArgs
    {
        /// <summary>
        /// 脚本ID
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;
        
        /// <summary>
        /// 调试信息
        /// </summary>
        public string DebugMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 调试级别
        /// </summary>
        public string DebugLevel { get; set; } = "Info";
        
        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// 堆栈跟踪
        /// </summary>
        public string StackTrace { get; set; } = string.Empty;
        
        /// <summary>
        /// 事件时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}