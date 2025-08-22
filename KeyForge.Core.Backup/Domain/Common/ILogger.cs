using System;

namespace KeyForge.Core.Domain.Common
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(Exception ex, string message);
        void LogDebug(string message);
    }
}