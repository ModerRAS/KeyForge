using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using KeyForge.Core.Models;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 脚本管理服务 - 改进版本
    /// 原本实现：ScriptManager功能单一，职责不清
    /// 改进实现：专门的脚本管理服务，支持序列化、播放控制
    /// </summary>
    public class ScriptService : IDisposable
    {
        private readonly object _lockObj = new object();
        private List<KeyAction> _actions = new List<KeyAction>();
        private bool _isPlaying = false;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed = false;

        public event EventHandler<KeyAction>? ActionRecorded;
        public event EventHandler<KeyAction>? ActionPlayed;
        public event EventHandler<string>? StatusChanged;
        public event EventHandler? PlaybackCompleted;
        public event EventHandler? PlaybackStopped;

        public IReadOnlyList<KeyAction> Actions 
        {
            get
            {
                lock (_lockObj)
                {
                    return _actions.AsReadOnly();
                }
            }
        }

        public bool IsPlaying 
        {
            get
            {
                lock (_lockObj)
                {
                    return _isPlaying;
                }
            }
        }

        public ScriptService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartRecording()
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                _actions.Clear();
                OnStatusChanged("开始录制脚本...");
            }
        }

        public void StopRecording()
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                OnStatusChanged($"录制完成，共 {_actions.Count} 个动作");
            }
        }

        public void AddAction(KeyAction action)
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                _actions.Add(action);
                ActionRecorded?.Invoke(this, action);
            }
        }

        public async Task PlayAsync(float speedMultiplier = 1.0f, bool repeat = false)
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                if (_isPlaying) return;
                if (_actions.Count == 0) throw new InvalidOperationException("没有可播放的动作");

                _isPlaying = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                OnStatusChanged("开始播放脚本...");

                do
                {
                    await PlayScriptInternalAsync(speedMultiplier, _cancellationTokenSource.Token);
                    
                    if (repeat && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        OnStatusChanged("重复播放脚本...");
                        await Task.Delay(1000, _cancellationTokenSource.Token); // 1秒延迟后重复
                    }
                }
                while (repeat && !_cancellationTokenSource.Token.IsCancellationRequested);

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    OnStatusChanged("脚本播放完成");
                    PlaybackCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (OperationCanceledException)
            {
                OnStatusChanged("脚本播放已停止");
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnStatusChanged($"播放失败: {ex.Message}");
                throw;
            }
            finally
            {
                lock (_lockObj)
                {
                    _isPlaying = false;
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        public void StopPlayback()
        {
            lock (_lockObj)
            {
                if (!_isPlaying || _disposed) return;

                _cancellationTokenSource?.Cancel();
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                StopPlayback();
                _actions.Clear();
                OnStatusChanged("脚本已清空");
            }
        }

        public void SaveToFile(string filePath)
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                try
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    string json = JsonSerializer.Serialize(_actions, options);
                    File.WriteAllText(filePath, json);
                    
                    OnStatusChanged($"脚本已保存到: {filePath}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"保存脚本失败: {ex.Message}", ex);
                }
            }
        }

        public void LoadFromFile(string filePath)
        {
            lock (_lockObj)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ScriptService));
                
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException("脚本文件不存在", filePath);
                    }

                    string json = File.ReadAllText(filePath);
                    var actions = JsonSerializer.Deserialize<List<KeyAction>>(json);
                    
                    _actions = actions ?? new List<KeyAction>();
                    
                    OnStatusChanged($"脚本已加载: {_actions.Count} 个动作");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"加载脚本失败: {ex.Message}", ex);
                }
            }
        }

        public ScriptStats GetStats()
        {
            lock (_lockObj)
            {
                return new ScriptStats
                {
                    TotalActions = _actions.Count,
                    KeyDownActions = _actions.Count(a => a.IsKeyDown),
                    KeyUpActions = _actions.Count(a => !a.IsKeyDown),
                    Duration = _actions.Count > 0 ? _actions.Max(a => a.Delay) : 0,
                    CreatedAt = DateTime.Now
                };
            }
        }

        private async Task PlayScriptInternalAsync(float speedMultiplier, CancellationToken cancellationToken)
        {
            int lastDelay = 0;

            foreach (var action in _actions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 计算等待时间，考虑速度倍数
                int waitTime = (int)((action.Delay - lastDelay) / speedMultiplier);
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime, cancellationToken);
                }

                // 触发播放事件
                ActionPlayed?.Invoke(this, action);
                lastDelay = action.Delay;
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
                    StopPlayback();
                    _cancellationTokenSource?.Dispose();
                    _disposed = true;
                }
            }
        }
    }

    /// <summary>
    /// 脚本统计信息
    /// </summary>
    public class ScriptStats
    {
        public int TotalActions { get; set; }
        public int KeyDownActions { get; set; }
        public int KeyUpActions { get; set; }
        public int Duration { get; set; } // 总时长（毫秒）
        public DateTime CreatedAt { get; set; }
    }
}