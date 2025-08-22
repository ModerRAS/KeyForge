using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 错误处理和异常恢复管理器 - 简化实现
    /// </summary>
    public class ErrorHandlerManager : IDisposable
    {
        private readonly ILoggerService _logger;
        private readonly IInputSimulator _inputSimulator;
        private readonly Dictionary<Type, List<ErrorHandler>> _errorHandlers;
        private readonly Queue<ErrorInfo> _errorQueue;
        private readonly object _lock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _processingTask;
        private bool _disposed;

        public event EventHandler<ErrorInfo> ErrorOccurred;
        public event EventHandler<RecoveryResult> RecoveryCompleted;

        public ErrorHandlerManager(ILoggerService logger, IInputSimulator inputSimulator)
        {
            _logger = logger;
            _inputSimulator = inputSimulator;
            _errorHandlers = new Dictionary<Type, List<ErrorHandler>>();
            _errorQueue = new Queue<ErrorInfo>();
            
            StartErrorProcessing();
        }

        /// <summary>
        /// 注册错误处理器
        /// </summary>
        public void RegisterErrorHandler<TException>(ErrorHandler handler) where TException : Exception
        {
            lock (_lock)
            {
                var exceptionType = typeof(TException);
                if (!_errorHandlers.ContainsKey(exceptionType))
                {
                    _errorHandlers[exceptionType] = new List<ErrorHandler>();
                }
                _errorHandlers[exceptionType].Add(handler);
            }
            
            _logger.Info($"注册错误处理器: {exceptionType.Name}");
        }

        /// <summary>
        /// 注销错误处理器
        /// </summary>
        public void UnregisterErrorHandler<TException>(ErrorHandler handler) where TException : Exception
        {
            lock (_lock)
            {
                var exceptionType = typeof(TException);
                if (_errorHandlers.ContainsKey(exceptionType))
                {
                    _errorHandlers[exceptionType].Remove(handler);
                    if (_errorHandlers[exceptionType].Count == 0)
                    {
                        _errorHandlers.Remove(exceptionType);
                    }
                }
            }
            
            _logger.Info($"注销错误处理器: {typeof(TException).Name}");
        }

        /// <summary>
        /// 处理异常
        /// </summary>
        public void HandleException(Exception ex, string context = null, object data = null)
        {
            var errorInfo = new ErrorInfo
            {
                Exception = ex,
                Context = context,
                Data = data,
                Timestamp = DateTime.Now,
                ErrorId = Guid.NewGuid()
            };

            lock (_lock)
            {
                _errorQueue.Enqueue(errorInfo);
            }

            // 触发事件
            ErrorOccurred?.Invoke(this, errorInfo);

            _logger.Error($"异常发生: {ex.Message} | 上下文: {context} | 错误ID: {errorInfo.ErrorId}");
        }

        /// <summary>
        /// 尝试恢复
        /// </summary>
        public async Task<RecoveryResult> TryRecoverAsync(ErrorInfo errorInfo)
        {
            _logger.Info($"开始恢复操作 | 错误ID: {errorInfo.ErrorId}");

            var result = new RecoveryResult
            {
                ErrorId = errorInfo.ErrorId,
                StartTime = DateTime.Now
            };

            try
            {
                // 获取适用的错误处理器
                var handlers = GetApplicableHandlers(errorInfo.Exception);
                
                if (handlers.Count == 0)
                {
                    result.Success = false;
                    result.Message = "没有找到适用的错误处理器";
                    _logger.Warning($"没有找到适用的错误处理器 | 错误ID: {errorInfo.ErrorId}");
                    return result;
                }

                // 按优先级执行恢复策略
                foreach (var handler in handlers.OrderBy(h => h.Priority))
                {
                    try
                    {
                        _logger.Debug($"尝试恢复策略: {handler.Name} | 错误ID: {errorInfo.ErrorId}");
                        
                        var recoverySuccess = await handler.RecoveryStrategy(errorInfo);
                        
                        if (recoverySuccess)
                        {
                            result.Success = true;
                            result.Message = $"恢复成功，使用策略: {handler.Name}";
                            result.HandlerName = handler.Name;
                            
                            _logger.Info($"恢复成功 | 策略: {handler.Name} | 错误ID: {errorInfo.ErrorId}");
                            break;
                        }
                        else
                        {
                            _logger.Debug($"恢复策略失败: {handler.Name} | 错误ID: {errorInfo.ErrorId}");
                        }
                    }
                    catch (Exception recoveryEx)
                    {
                        _logger.Error($"恢复策略执行失败: {handler.Name} | 错误: {recoveryEx.Message} | 错误ID: {errorInfo.ErrorId}");
                    }
                }

                if (!result.Success)
                {
                    result.Message = "所有恢复策略都失败了";
                    _logger.Error($"所有恢复策略都失败了 | 错误ID: {errorInfo.ErrorId}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"恢复过程中发生异常: {ex.Message}";
                _logger.Error($"恢复过程中发生异常: {ex.Message} | 错误ID: {errorInfo.ErrorId}");
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;

            // 触发事件
            RecoveryCompleted?.Invoke(this, result);

            return result;
        }

        /// <summary>
        /// 获取适用的错误处理器
        /// </summary>
        private List<ErrorHandler> GetApplicableHandlers(Exception ex)
        {
            var handlers = new List<ErrorHandler>();
            
            lock (_lock)
            {
                var exceptionType = ex.GetType();
                
                // 查找直接匹配的处理器
                if (_errorHandlers.TryGetValue(exceptionType, out var directHandlers))
                {
                    handlers.AddRange(directHandlers);
                }
                
                // 查找基类处理器
                foreach (var handlerPair in _errorHandlers)
                {
                    if (handlerPair.Key.IsAssignableFrom(exceptionType))
                    {
                        handlers.AddRange(handlerPair.Value);
                    }
                }
            }
            
            return handlers.Distinct().ToList();
        }

        /// <summary>
        /// 启动错误处理任务
        /// </summary>
        private void StartErrorProcessing()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = Task.Run(() => ProcessErrorsAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// 异步处理错误队列
        /// </summary>
        private async Task ProcessErrorsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ErrorInfo errorInfo = null;
                    
                    lock (_lock)
                    {
                        if (_errorQueue.Count > 0)
                        {
                            errorInfo = _errorQueue.Dequeue();
                        }
                    }

                    if (errorInfo != null)
                    {
                        // 记录错误日志
                        LogErrorDetails(errorInfo);
                        
                        // 尝试自动恢复
                        if (errorInfo.Exception.GetType() != typeof(OperationCanceledException))
                        {
                            await TryRecoverAsync(errorInfo);
                        }
                    }
                    else
                    {
                        // 没有错误需要处理，等待一段时间
                        await Task.Delay(100, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"错误处理过程中发生异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 记录错误详情
        /// </summary>
        private void LogErrorDetails(ErrorInfo errorInfo)
        {
            var logMessage = $"错误详情:\n" +
                           $"错误ID: {errorInfo.ErrorId}\n" +
                           $"时间: {errorInfo.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\n" +
                           $"异常类型: {errorInfo.Exception.GetType().Name}\n" +
                           $"异常消息: {errorInfo.Exception.Message}\n" +
                           $"上下文: {errorInfo.Context ?? "无"}\n" +
                           $"堆栈跟踪: {errorInfo.Exception.StackTrace}";
            
            _logger.Error(logMessage);
        }

        /// <summary>
        /// 创建默认错误处理器
        /// </summary>
        public static List<ErrorHandler> CreateDefaultHandlers(IInputSimulator inputSimulator)
        {
            var handlers = new List<ErrorHandler>();

            // 按键模拟错误处理器
            handlers.Add(new ErrorHandler
            {
                Name = "按键模拟恢复",
                ExceptionType = typeof(System.ComponentModel.Win32Exception),
                Priority = 1,
                RecoveryStrategy = async (errorInfo) =>
                {
                    try
                    {
                        // 等待一段时间后重试
                        await Task.Delay(1000);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });

            // 图像识别错误处理器
            handlers.Add(new ErrorHandler
            {
                Name = "图像识别恢复",
                ExceptionType = typeof(OpenCvSharp.OpenCVException),
                Priority = 2,
                RecoveryStrategy = async (errorInfo) =>
                {
                    try
                    {
                        // 等待并重试
                        await Task.Delay(500);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });

            // 权限错误处理器
            handlers.Add(new ErrorHandler
            {
                Name = "权限错误恢复",
                ExceptionType = typeof(System.UnauthorizedAccessException),
                Priority = 3,
                RecoveryStrategy = async (errorInfo) =>
                {
                    try
                    {
                        // 记录错误但不尝试恢复
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });

            // 超时错误处理器
            handlers.Add(new ErrorHandler
            {
                Name = "超时恢复",
                ExceptionType = typeof(System.TimeoutException),
                Priority = 4,
                RecoveryStrategy = async (errorInfo) =>
                {
                    try
                    {
                        // 重试操作
                        await Task.Delay(1000);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });

            return handlers;
        }

        /// <summary>
        /// 获取错误统计信息
        /// </summary>
        public ErrorStatistics GetErrorStatistics()
        {
            lock (_lock)
            {
                return new ErrorStatistics
                {
                    TotalErrors = _errorQueue.Count,
                    LastErrorTime = DateTime.Now,
                    HandlerCount = _errorHandlers.Values.Sum(h => h.Count)
                };
            }
        }

        /// <summary>
        /// 清空错误队列
        /// </summary>
        public void ClearErrorQueue()
        {
            lock (_lock)
            {
                _errorQueue.Clear();
            }
            
            _logger.Info("错误队列已清空");
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
                    _cancellationTokenSource?.Cancel();
                    try
                    {
                        _processingTask?.Wait();
                    }
                    catch (AggregateException)
                    {
                        // 忽略取消异常
                    }
                    _cancellationTokenSource?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~ErrorHandlerManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public class ErrorInfo
    {
        public Exception Exception { get; set; }
        public string Context { get; set; }
        public object Data { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid ErrorId { get; set; }
        public int RetryCount { get; set; }
        public bool IsRecoverable { get; set; } = true;
    }

    /// <summary>
    /// 错误处理器
    /// </summary>
    public class ErrorHandler
    {
        public string Name { get; set; }
        public Type ExceptionType { get; set; }
        public int Priority { get; set; }
        public Func<ErrorInfo, Task<bool>> RecoveryStrategy { get; set; }
        public int MaxRetryCount { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// 恢复结果
    /// </summary>
    public class RecoveryResult
    {
        public Guid ErrorId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string HandlerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// 错误统计信息
    /// </summary>
    public class ErrorStatistics
    {
        public int TotalErrors { get; set; }
        public int HandlerCount { get; set; }
        public DateTime LastErrorTime { get; set; }
    }

    /// <summary>
    /// 异步重试策略
    /// </summary>
    public static class RetryStrategy
    {
        /// <summary>
        /// 异步重试执行
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, int maxRetries = 3, TimeSpan? retryDelay = null)
        {
            var delay = retryDelay ?? TimeSpan.FromSeconds(1);
            var exceptions = new List<Exception>();

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    
                    if (i == maxRetries - 1)
                    {
                        throw new AggregateException($"操作在 {maxRetries} 次重试后失败", exceptions);
                    }
                    
                    await Task.Delay(delay);
                }
            }

            throw new InvalidOperationException("不应该到达这里");
        }

        /// <summary>
        /// 异步重试执行（无返回值）
        /// </summary>
        public static async Task ExecuteAsync(Func<Task> action, int maxRetries = 3, TimeSpan? retryDelay = null)
        {
            await ExecuteAsync(async () => 
            {
                await action();
                return true;
            }, maxRetries, retryDelay);
        }
    }
}