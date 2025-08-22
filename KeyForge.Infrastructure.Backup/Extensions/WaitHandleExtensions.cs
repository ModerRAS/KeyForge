using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyForge.Infrastructure.Extensions
{
    /// <summary>
    /// WaitHandle 扩展方法 - 简化实现
    /// 
    /// 原本实现：完整的异步WaitHandle支持
    /// 简化实现：基本的异步等待功能
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// 异步等待WaitHandle信号
        /// </summary>
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout)
        {
            return Task.Run(() => handle.WaitOne(timeout));
        }

        /// <summary>
        /// 异步等待WaitHandle信号（无超时）
        /// </summary>
        public static Task WaitOneAsync(this WaitHandle handle)
        {
            return Task.Run(() => handle.WaitOne());
        }
    }
}