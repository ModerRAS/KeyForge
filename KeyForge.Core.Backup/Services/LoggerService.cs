using System;
using System.IO;
using System.Threading;
using KeyForge.Core.Interfaces;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 日志服务 - 简化实现
    /// </summary>
    public class LoggerService : ILoggerService, IDisposable
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();
        private bool _disposed;

        public LoggerService(string logFilePath = "keyforge.log")
        {
            _logFilePath = logFilePath;
            EnsureLogDirectoryExists();
        }

        /// <summary>
        /// 确保日志目录存在
        /// </summary>
        private void EnsureLogDirectoryExists()
        {
            try
            {
                string directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建日志目录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        private void WriteLog(string level, string message)
        {
            if (_disposed) return;

            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                
                // 写入文件
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }

                // 同时输出到控制台
                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入日志失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 信息日志
        /// </summary>
        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        public void Warning(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        public void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// 错误日志（带异常）
        /// </summary>
        public void Error(string message, Exception ex)
        {
            WriteLog("ERROR", $"{message}: {ex.Message}");
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        public void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        #region ILoggerService 接口实现

        public void LogInformation(string message)
        {
            Info(message);
        }

        public void LogWarning(string message)
        {
            Warning(message);
        }

        public void LogError(string message)
        {
            Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            Error(message, ex);
        }

        public void LogDebug(string message)
        {
            Debug(message);
        }

        #endregion

        /// <summary>
        /// 清理日志文件
        /// </summary>
        public void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    FileInfo fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-daysToKeep))
                    {
                        File.Delete(_logFilePath);
                        Info($"已清理过期日志文件: {_logFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error($"清理日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取日志文件内容
        /// </summary>
        public string[] GetLogLines(int maxLines = 1000)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    return new string[0];
                }

                string[] allLines = File.ReadAllLines(_logFilePath);
                
                if (allLines.Length <= maxLines)
                {
                    return allLines;
                }

                // 返回最后N行
                string[] result = new string[maxLines];
                Array.Copy(allLines, allLines.Length - maxLines, result, 0, maxLines);
                return result;
            }
            catch (Exception ex)
            {
                Error($"读取日志文件失败: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 清理托管资源
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~LoggerService()
        {
            Dispose(false);
        }
    }
}