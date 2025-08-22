namespace KeyForge.Core.Domain.Interfaces
{
    /// <summary>
    /// 验证结果 - 简化实现
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// 日志服务接口 - 简化实现
    /// </summary>
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogDebug(string message);
        
        // 简化方法名，为了兼容现有代码
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception ex = null);
        void Debug(string message);
    }
}