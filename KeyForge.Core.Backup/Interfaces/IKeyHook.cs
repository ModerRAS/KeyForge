using System;
using System.Runtime.InteropServices;

namespace KeyForge.Core.Interfaces
{
    /// <summary>
    /// 按键钩子接口 - 简化实现
    /// 原本实现：复杂的事件总线、多线程处理
    /// 简化实现：直接使用Windows钩子API，简单高效
    /// </summary>
    public interface IKeyHook : IDisposable
    {
        event EventHandler<KeyEventArgs> KeyPressed;
        event EventHandler<KeyEventArgs> KeyReleased;
        bool IsEnabled { get; }
        void Start();
        void Stop();
    }

    /// <summary>
    /// 按键事件参数
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        public Keys Key { get; }
        public int KeyCode { get; }
        public bool IsKeyDown { get; }

        public KeyEventArgs(Keys key, bool isKeyDown)
        {
            Key = key;
            KeyCode = (int)key;
            IsKeyDown = isKeyDown;
        }
    }
}