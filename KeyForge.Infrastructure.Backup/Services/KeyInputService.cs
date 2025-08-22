using System;
using System.Runtime.InteropServices;
using System.Threading;
using KeyForge.Core.Interfaces;
using KeyForge.Infrastructure.Native;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 按键输入服务 - 整合Hook和模拟功能
    /// 原本实现：分散在多个类中，职责不清
    /// 改进实现：统一管理按键录制和模拟，线程安全
    /// </summary>
    public class KeyInputService : IDisposable
    {
        private readonly WindowsKeyHook _keyHook;
        private readonly object _lockObj = new object();
        private bool _isRecording = false;
        private bool _disposed = false;

        public event EventHandler<KeyEventArgs>? KeyRecorded;
        public event EventHandler<string>? StatusChanged;

        public bool IsRecording 
        {
            get
            {
                lock (_lockObj)
                {
                    return _isRecording;
                }
            }
        }

        public KeyInputService()
        {
            _keyHook = new WindowsKeyHook();
            _keyHook.KeyPressed += OnKeyPressed;
            _keyHook.KeyReleased += OnKeyReleased;
        }

        public void StartRecording()
        {
            lock (_lockObj)
            {
                if (_isRecording || _disposed) return;

                try
                {
                    _keyHook.Start();
                    _isRecording = true;
                    OnStatusChanged("开始录制按键...");
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"录制失败: {ex.Message}");
                    throw;
                }
            }
        }

        public void StopRecording()
        {
            lock (_lockObj)
            {
                if (!_isRecording || _disposed) return;

                try
                {
                    _keyHook.Stop();
                    _isRecording = false;
                    OnStatusChanged("录制已停止");
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"停止录制失败: {ex.Message}");
                    throw;
                }
            }
        }

        public void SimulateKey(Keys key, bool isKeyDown)
        {
            try
            {
                if (isKeyDown)
                {
                    keybd_event((byte)key, 0, 0, 0);
                }
                else
                {
                    keybd_event((byte)key, 0, 2, 0);
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged($"按键模拟失败: {ex.Message}");
                throw;
            }
        }

        public void SimulateKeySequence(System.Collections.Generic.IEnumerable<(Keys Key, bool IsKeyDown)> keySequence)
        {
            if (keySequence == null) return;

            foreach (var (key, isKeyDown) in keySequence)
            {
                try
                {
                    SimulateKey(key, isKeyDown);
                    Thread.Sleep(10); // 短暂延迟，避免按键过于密集
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"模拟按键序列失败: {ex.Message}");
                    throw;
                }
            }
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (_isRecording)
            {
                KeyRecorded?.Invoke(this, e);
            }
        }

        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            if (_isRecording)
            {
                KeyRecorded?.Invoke(this, e);
            }
        }

        private void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        public void Dispose()
        {
            lock (_lockObj)
            {
                if (!_disposed)
                {
                    StopRecording();
                    _keyHook?.Dispose();
                    _disposed = true;
                }
            }
        }

        // Windows API 函数
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    }
}