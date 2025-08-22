using System;
using System.Linq;
using System.Runtime.InteropServices;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
    using KeyForge.Domain.Common;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// Windows API输入模拟器 - 简化实现
    /// </summary>
    public class InputSimulator : IInputSimulator
    {
        // Windows API 常量
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        // Windows API 导入
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT point);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// 发送按键事件
        /// </summary>
        public void SendKey(KeyCode key, KeyState state)
        {
            try
            {
                byte keyCode = (byte)key;
                int flags = (state == KeyState.Down) ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP;
                keybd_event(keyCode, 0, flags, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"按键模拟失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送鼠标按钮事件
        /// </summary>
        public void SendMouse(MouseButton button, MouseState state)
        {
            try
            {
                int flags = 0;
                
                switch (button)
                {
                    case MouseButton.Left:
                        flags = (state == MouseState.Down) ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
                        break;
                    case MouseButton.Right:
                        flags = (state == MouseState.Down) ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
                        break;
                    case MouseButton.Middle:
                        flags = (state == MouseState.Down) ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
                        break;
                }

                mouse_event(flags, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"鼠标模拟失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        public void MoveMouse(int x, int y)
        {
            try
            {
                SetCursorPos(x, y);
            }
            catch (Exception ex)
            {
                throw new Exception($"鼠标移动失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 延迟指定时间
        /// </summary>
        public void Delay(int milliseconds)
        {
            if (milliseconds > 0)
            {
                System.Threading.Thread.Sleep(milliseconds);
            }
        }

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        public (int X, int Y) GetCursorPosition()
        {
            GetCursorPos(out POINT point);
            return (point.X, point.Y);
        }

        /// <summary>
        /// 模拟按键组合（如Ctrl+C）
        /// </summary>
        public void SendKeyCombination(KeyCode[] keys)
        {
            try
            {
                // 按下所有按键
                foreach (var key in keys)
                {
                    SendKey(key, KeyState.Down);
                }

                // 短暂延迟
                Delay(50);

                // 释放所有按键
                foreach (var key in keys)
                {
                    SendKey(key, KeyState.Up);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"按键组合模拟失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 模拟文本输入
        /// </summary>
        public void SendText(string text)
        {
            try
            {
                foreach (char c in text)
                {
                    KeyCode key = CharToKeyCode(c);
                    if (key != KeyCode.None)
                    {
                        SendKey(key, KeyState.Down);
                        Delay(10);
                        SendKey(key, KeyState.Up);
                        Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"文本输入失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 字符转按键代码
        /// </summary>
        private KeyCode CharToKeyCode(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return (KeyCode)((byte)c - 32); // 转换为大写字母的ASCII码
            }
            else if (c >= 'A' && c <= 'Z')
            {
                return (KeyCode)(byte)c;
            }
            else if (c >= '0' && c <= '9')
            {
                return (KeyCode)((byte)c);
            }
            else
            {
                switch (c)
                {
                    case ' ': return KeyCode.Space;
                    case '\n': return KeyCode.Enter;
                    case '\t': return KeyCode.Tab;
                    default: return KeyCode.None;
                }
            }
        }

        #region IInputSimulator 接口实现
        
        public void KeyDown(KeyCode key)
        {
            SendKey(key, KeyState.Down);
        }

        public void KeyUp(KeyCode key)
        {
            SendKey(key, KeyState.Up);
        }

        public void KeyPress(KeyCode key, int delay = 50)
        {
            KeyDown(key);
            if (delay > 0)
                Delay(delay);
            KeyUp(key);
        }

        public void MouseDown(MouseButton button, int x, int y)
        {
            MoveMouse(x, y);
            SendMouse(button, MouseState.Down);
        }

        public void MouseUp(MouseButton button, int x, int y)
        {
            MoveMouse(x, y);
            SendMouse(button, MouseState.Up);
        }

        public void MouseMove(int x, int y)
        {
            MoveMouse(x, y);
        }

        public void MouseClick(MouseButton button, int x, int y, int delay = 100)
        {
            MouseDown(button, x, y);
            if (delay > 0)
                Delay(delay);
            MouseUp(button, x, y);
        }

        public void MouseDoubleClick(MouseButton button, int x, int y)
        {
            MouseClick(button, x, y, 100);
            Delay(100);
            MouseClick(button, x, y, 100);
        }

        public void MouseScroll(int delta)
        {
            try
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"鼠标滚轮模拟失败: {ex.Message}", ex);
            }
        }

        #endregion
    }

    /// <summary>
    /// 扩展KeyCode枚举，添加None值
    /// </summary>
    public static class KeyCodeExtensions
    {
        public const KeyCode None = (KeyCode)0;
    }
}