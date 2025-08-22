using System;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Entities
{
    /// <summary>
    /// 日志条目实体
    /// 原本实现：缺失的类型定义
    /// 简化实现：创建基础的日志条目实体
    /// </summary>
    public class LogEntry : Entity
    {
        public string Message { get; private set; }
        public LogLevel Level { get; private set; }
        public string Category { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Exception { get; private set; }

        protected LogEntry() { }

        public LogEntry(Guid id, string message, LogLevel level, string category = null, string exception = null)
        {
            Id = id;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Level = level;
            Category = category ?? "Default";
            Timestamp = DateTime.UtcNow;
            Exception = exception;
        }
    }
}