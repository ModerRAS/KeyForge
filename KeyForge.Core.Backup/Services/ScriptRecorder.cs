using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
using KeyForge.Domain.Common;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 全局键盘钩子 - 简化实现
    /// </summary>
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private delegate IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        private KeyboardHookCallback _callback;
        private IntPtr _hookID = IntPtr.Zero;

        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;

        public GlobalKeyboardHook()
        {
            _callback = KeyboardHookHandler;
            _hookID = SetHook(_callback);
        }

        private IntPtr SetHook(KeyboardHookCallback proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr KeyboardHookHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    KeyDown?.Invoke(this, new KeyEventArgs((Keys)vkCode));
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    KeyUp?.Invoke(this, new KeyEventArgs((Keys)vkCode));
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    /// <summary>
    /// 全局鼠标钩子 - 简化实现
    /// </summary>
    public class GlobalMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEMOVE = 0x0200;

        private delegate IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        private MouseHookCallback _callback;
        private IntPtr _hookID = IntPtr.Zero;

        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseMove;

        public GlobalMouseHook()
        {
            _callback = MouseHookHandler;
            _hookID = SetHook(_callback);
        }

        private IntPtr SetHook(MouseHookCallback proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_LBUTTONUP:
                        MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_RBUTTONDOWN:
                        MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_RBUTTONUP:
                        MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_MBUTTONDOWN:
                        MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_MBUTTONUP:
                        MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                    case WM_MOUSEMOVE:
                        MouseMove?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, hookStruct.pt.x, hookStruct.pt.y, 0));
                        break;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    /// <summary>
    /// 脚本录制器 - 简化实现
    /// </summary>
    public class ScriptRecorder : IScriptRecorder
    {
        private GlobalKeyboardHook _keyboardHook;
        private GlobalMouseHook _mouseHook;
        private Script _currentScript;
        private DateTime _startTime;
        private DateTime _lastActionTime;
        private bool _isRecording;

        public bool IsRecording => _isRecording;

        public ScriptRecorder()
        {
            _keyboardHook = new GlobalKeyboardHook();
            _mouseHook = new GlobalMouseHook();
            
            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;
            _mouseHook.MouseDown += OnMouseDown;
            _mouseHook.MouseUp += OnMouseUp;
            _mouseHook.MouseMove += OnMouseMove;
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _isRecording = true;
            _startTime = DateTime.Now;
            _lastActionTime = _startTime;
            _currentScript = new Script
            {
                Name = $"Recorded_{_startTime:yyyyMMdd_HHmmss}",
                Description = "自动录制的脚本"
            };
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
        }

        /// <summary>
        /// 获取录制的脚本
        /// </summary>
        public Script GetRecordedScript()
        {
            return _currentScript;
        }

        /// <summary>
        /// 清空录制
        /// </summary>
        public void ClearRecording()
        {
            _currentScript = null;
        }

        /// <summary>
        /// 键盘按下事件处理
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            var keyAction = new KeyAction
            {
                Type = ActionType.KeyDown,
                Key = (KeyCode)e.KeyCode,
                Delay = (int)(DateTime.Now - _lastActionTime).TotalMilliseconds
            };

            _currentScript.AddAction(keyAction);
            _lastActionTime = DateTime.Now;
        }

        /// <summary>
        /// 键盘释放事件处理
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            var keyAction = new KeyAction
            {
                Type = ActionType.KeyUp,
                Key = (KeyCode)e.KeyCode,
                Delay = (int)(DateTime.Now - _lastActionTime).TotalMilliseconds
            };

            _currentScript.AddAction(keyAction);
            _lastActionTime = DateTime.Now;
        }

        /// <summary>
        /// 鼠标按下事件处理
        /// </summary>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!_isRecording) return;

            var keyAction = new KeyAction
            {
                Type = ActionType.MouseDown,
                Button = GetMouseButton(e.Button),
                X = e.X,
                Y = e.Y,
                Delay = (int)(DateTime.Now - _lastActionTime).TotalMilliseconds
            };

            _currentScript.AddAction(keyAction);
            _lastActionTime = DateTime.Now;
        }

        /// <summary>
        /// 鼠标释放事件处理
        /// </summary>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isRecording) return;

            var keyAction = new KeyAction
            {
                Type = ActionType.MouseUp,
                Button = GetMouseButton(e.Button),
                X = e.X,
                Y = e.Y,
                Delay = (int)(DateTime.Now - _lastActionTime).TotalMilliseconds
            };

            _currentScript.AddAction(keyAction);
            _lastActionTime = DateTime.Now;
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRecording) return;

            // 限制鼠标移动事件的频率，避免过多记录
            var timeSinceLastAction = (DateTime.Now - _lastActionTime).TotalMilliseconds;
            if (timeSinceLastAction < 50) // 50ms内的移动事件忽略
                return;

            var keyAction = new KeyAction
            {
                Type = ActionType.MouseMove,
                X = e.X,
                Y = e.Y,
                Delay = (int)timeSinceLastAction
            };

            _currentScript.AddAction(keyAction);
            _lastActionTime = DateTime.Now;
        }

        /// <summary>
        /// 转换鼠标按钮枚举
        /// </summary>
        private MouseButton GetMouseButton(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                default:
                    return MouseButton.Left;
            }
        }
    }
}