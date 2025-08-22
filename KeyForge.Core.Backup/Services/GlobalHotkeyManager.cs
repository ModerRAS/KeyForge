using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeyForge.Core.Interfaces;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 全局快捷键管理器 - 简化实现
    /// </summary>
    public class GlobalHotkeyManager : IDisposable
    {
        // Windows API 常量
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;
        private const int MOD_NOREPEAT = 0x4000;

        // Windows API 导入
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly IntPtr _windowHandle;
        private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys;
        private int _nextId = 1;
        private bool _disposed;

        public event EventHandler<HotkeyEventArgs> HotkeyPressed;

        public GlobalHotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _registeredHotkeys = new Dictionary<int, HotkeyInfo>();
        }

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        public bool RegisterHotkey(string hotkeyString, object tag = null)
        {
            try
            {
                var hotkeyInfo = ParseHotkeyString(hotkeyString);
                if (hotkeyInfo == null)
                {
                    return false;
                }

                hotkeyInfo.Tag = tag;
                return RegisterHotkey(hotkeyInfo);
            }
            catch (Exception ex)
            {
                // 使用日志服务记录错误
                Console.WriteLine($"注册快捷键失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        public bool RegisterHotkey(HotkeyInfo hotkeyInfo)
        {
            if (hotkeyInfo == null)
            {
                return false;
            }

            // 检查是否已经注册
            foreach (var registered in _registeredHotkeys.Values)
            {
                if (registered.Key == hotkeyInfo.Key && registered.Modifiers == hotkeyInfo.Modifiers)
                {
                    return false; // 快捷键已存在
                }
            }

            var modifiers = GetModifiers(hotkeyInfo.Modifiers);
            var key = (int)hotkeyInfo.Key;

            if (RegisterHotKey(_windowHandle, _nextId, modifiers, key))
            {
                hotkeyInfo.Id = _nextId;
                _registeredHotkeys[_nextId] = hotkeyInfo;
                _nextId++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 注销全局快捷键
        /// </summary>
        public bool UnregisterHotkey(string hotkeyString)
        {
            var hotkeyInfo = ParseHotkeyString(hotkeyString);
            if (hotkeyInfo == null)
            {
                return false;
            }

            return UnregisterHotkey(hotkeyInfo.Key, hotkeyInfo.Modifiers);
        }

        /// <summary>
        /// 注销全局快捷键
        /// </summary>
        public bool UnregisterHotkey(Keys key, HotkeyModifiers modifiers)
        {
            foreach (var pair in _registeredHotkeys)
            {
                if (pair.Value.Key == key && pair.Value.Modifiers == modifiers)
                {
                    if (UnregisterHotKey(_windowHandle, pair.Key))
                    {
                        _registeredHotkeys.Remove(pair.Key);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 注销所有快捷键
        /// </summary>
        public void UnregisterAllHotkeys()
        {
            foreach (var id in _registeredHotkeys.Keys.ToArray())
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _registeredHotkeys.Clear();
        }

        /// <summary>
        /// 处理Windows消息
        /// </summary>
        public void ProcessMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                var id = m.WParam.ToInt32();
                if (_registeredHotkeys.TryGetValue(id, out var hotkeyInfo))
                {
                    OnHotkeyPressed(hotkeyInfo);
                }
            }
        }

        /// <summary>
        /// 触发快捷键事件
        /// </summary>
        protected virtual void OnHotkeyPressed(HotkeyInfo hotkeyInfo)
        {
            HotkeyPressed?.Invoke(this, new HotkeyEventArgs(hotkeyInfo));
        }

        /// <summary>
        /// 解析快捷键字符串
        /// </summary>
        private HotkeyInfo ParseHotkeyString(string hotkeyString)
        {
            if (string.IsNullOrWhiteSpace(hotkeyString))
            {
                return null;
            }

            var parts = hotkeyString.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            var modifiers = HotkeyModifiers.None;
            Keys key = Keys.None;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                switch (parts[i].ToUpper())
                {
                    case "ALT":
                        modifiers |= HotkeyModifiers.Alt;
                        break;
                    case "CTRL":
                    case "CONTROL":
                        modifiers |= HotkeyModifiers.Control;
                        break;
                    case "SHIFT":
                        modifiers |= HotkeyModifiers.Shift;
                        break;
                    case "WIN":
                    case "WINDOWS":
                        modifiers |= HotkeyModifiers.Win;
                        break;
                }
            }

            var keyString = parts[parts.Length - 1].ToUpper();
            if (Enum.TryParse<Keys>(keyString, out key))
            {
                return new HotkeyInfo
                {
                    Key = key,
                    Modifiers = modifiers,
                    OriginalString = hotkeyString
                };
            }

            return null;
        }

        /// <summary>
        /// 获取修饰符标志
        /// </summary>
        private int GetModifiers(HotkeyModifiers modifiers)
        {
            int result = 0;
            
            if (modifiers.HasFlag(HotkeyModifiers.Alt))
                result |= MOD_ALT;
            
            if (modifiers.HasFlag(HotkeyModifiers.Control))
                result |= MOD_CONTROL;
            
            if (modifiers.HasFlag(HotkeyModifiers.Shift))
                result |= MOD_SHIFT;
            
            if (modifiers.HasFlag(HotkeyModifiers.Win))
                result |= MOD_WIN;
            
            result |= MOD_NOREPEAT; // 防止重复触发
            
            return result;
        }

        /// <summary>
        /// 获取所有已注册的快捷键
        /// </summary>
        public HotkeyInfo[] GetRegisteredHotkeys()
        {
            return _registeredHotkeys.Values.ToArray();
        }

        /// <summary>
        /// 检查快捷键是否已注册
        /// </summary>
        public bool IsHotkeyRegistered(string hotkeyString)
        {
            var hotkeyInfo = ParseHotkeyString(hotkeyString);
            if (hotkeyInfo == null)
            {
                return false;
            }

            foreach (var registered in _registeredHotkeys.Values)
            {
                if (registered.Key == hotkeyInfo.Key && registered.Modifiers == hotkeyInfo.Modifiers)
                {
                    return true;
                }
            }
            return false;
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
                    UnregisterAllHotkeys();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~GlobalHotkeyManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// 快捷键信息
    /// </summary>
    public class HotkeyInfo
    {
        public int Id { get; set; }
        public Keys Key { get; set; }
        public HotkeyModifiers Modifiers { get; set; }
        public string OriginalString { get; set; }
        public object Tag { get; set; }

        public override string ToString()
        {
            return OriginalString ?? GetDisplayString();
        }

        private string GetDisplayString()
        {
            var modifiers = "";
            if (Modifiers.HasFlag(HotkeyModifiers.Alt))
                modifiers += "Alt+";
            if (Modifiers.HasFlag(HotkeyModifiers.Control))
                modifiers += "Ctrl+";
            if (Modifiers.HasFlag(HotkeyModifiers.Shift))
                modifiers += "Shift+";
            if (Modifiers.HasFlag(HotkeyModifiers.Win))
                modifiers += "Win+";

            return modifiers + Key;
        }
    }

    /// <summary>
    /// 快捷键修饰符
    /// </summary>
    [Flags]
    public enum HotkeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    /// <summary>
    /// 快捷键事件参数
    /// </summary>
    public class HotkeyEventArgs : EventArgs
    {
        public HotkeyInfo Hotkey { get; }

        public HotkeyEventArgs(HotkeyInfo hotkey)
        {
            Hotkey = hotkey;
        }
    }
}