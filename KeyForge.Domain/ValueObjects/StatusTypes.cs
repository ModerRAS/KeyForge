using System;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 识别状态枚举
    /// 原本实现：复杂的识别状态
    /// 简化实现：基础的识别状态
    /// </summary>
    public enum RecognitionStatus
    {
        Pending = 0,
        Processing = 1,
        Success = 2,
        Failed = 3,
        Timeout = 4
    }

    /// <summary>
    /// 执行状态枚举
    /// 原本实现：复杂的执行状态
    /// 简化实现：基础的执行状态
    /// </summary>
    public enum ExecutionStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Timeout = 5
    }
}