using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Core.Models;
using KeyForge.Presentation.Interfaces;
using KeyForge.Core.Domain.Interfaces;

namespace KeyForge.Presentation.Services
{
    /// <summary>
    /// 应用服务实现 - 简化版本
    /// 原本实现：复杂的多层架构
    /// 简化实现：直接的服务实现，用于测试层间交互
    /// </summary>
    public class ApplicationService : IApplicationService, IDisposable
    {
        private readonly Dictionary<Guid, Script> _scripts;
        private readonly Dictionary<Guid, List<GameAction>> _recordedActions;
        private readonly ILoggerService _logger;
        private Guid _currentRecordingScriptId = Guid.Empty;
        private bool _isRecording = false;
        private bool _isPlaying = false;
        private bool _disposed = false;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<Script> ScriptCreated;
        public event EventHandler<Script> ScriptUpdated;
        public event EventHandler<Guid> ScriptDeleted;
        public event EventHandler<GameAction> ActionRecorded;
        public event EventHandler<bool> RecordingStateChanged;
        public event EventHandler<bool> PlaybackStateChanged;

        public ApplicationService(ILoggerService logger)
        {
            _scripts = new Dictionary<Guid, Script>();
            _recordedActions = new Dictionary<Guid, List<GameAction>>();
            _logger = logger;
        }

        #region 脚本管理

        public async Task<Script> CreateScriptAsync(string name, string description)
        {
            await Task.Run(() =>
            {
                var script = new Script(Guid.NewGuid(), name, description);
                _scripts[script.Id] = script;
                _recordedActions[script.Id] = new List<GameAction>();

                OnStatusChanged($"创建脚本: {name}");
                OnScriptCreated(script);
                _logger.LogInformation($"创建脚本: {name} (ID: {script.Id})");
            });

            return _scripts.Values.Last();
        }

        public async Task<Script> UpdateScriptAsync(Guid scriptId, string name, string description)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                script.Update(name, description);

                OnStatusChanged($"更新脚本: {name}");
                OnScriptUpdated(script);
                _logger.LogInformation($"更新脚本: {name} (ID: {scriptId})");

                return script;
            });
        }

        public async Task<bool> DeleteScriptAsync(Guid scriptId)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    return false;

                _scripts.Remove(scriptId);
                _recordedActions.Remove(scriptId);

                OnStatusChanged($"删除脚本: {script.Name}");
                OnScriptDeleted(scriptId);
                _logger.LogInformation($"删除脚本: {script.Name} (ID: {scriptId})");

                return true;
            });
        }

        public async Task<Script> GetScriptAsync(Guid scriptId)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                return script;
            });
        }

        public async Task<IEnumerable<Script>> GetAllScriptsAsync()
        {
            return await Task.Run(() => _scripts.Values.ToList());
        }

        #endregion

        #region 录制服务

        public async Task<bool> StartRecordingAsync(Guid scriptId)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                if (_isRecording)
                    return false;

                _currentRecordingScriptId = scriptId;
                _isRecording = true;

                OnStatusChanged($"开始录制脚本: {script.Name}");
                OnRecordingStateChanged(true);
                _logger.LogInformation($"开始录制脚本: {script.Name} (ID: {scriptId})");

                return true;
            });
        }

        public async Task<bool> StopRecordingAsync()
        {
            return await Task.Run(() =>
            {
                if (!_isRecording)
                    return false;

                if (_currentRecordingScriptId != Guid.Empty)
                {
                    var script = _scripts[_currentRecordingScriptId];
                    
                    // 将录制的动作添加到脚本中
                    foreach (var action in _recordedActions[_currentRecordingScriptId])
                    {
                        script.AddAction(action);
                    }

                    OnStatusChanged($"停止录制脚本: {script.Name}");
                    _logger.LogInformation($"停止录制脚本: {script.Name} (ID: {_currentRecordingScriptId})");
                }

                _isRecording = false;
                _currentRecordingScriptId = Guid.Empty;
                OnRecordingStateChanged(false);

                return true;
            });
        }

        public async Task<bool> AddActionAsync(Guid scriptId, GameAction action)
        {
            return await Task.Run(() =>
            {
                if (!_isRecording || _currentRecordingScriptId != scriptId)
                    return false;

                _recordedActions[scriptId].Add(action);
                OnActionRecorded(action);

                return true;
            });
        }

        public async Task<bool> IsRecordingAsync()
        {
            return await Task.Run(() => _isRecording);
        }

        #endregion

        #region 播放服务

        public async Task<bool> StartPlaybackAsync(Guid scriptId, float speedMultiplier = 1.0f, bool repeat = false)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                if (_isPlaying)
                    return false;

                _isPlaying = true;
                OnPlaybackStateChanged(true);
                OnStatusChanged($"开始播放脚本: {script.Name}");

                // 模拟播放过程
                Task.Run(async () =>
                {
                    try
                    {
                        do
                        {
                            var actions = script.Actions.ToList();
                            for (int i = 0; i < actions.Count; i++)
                            {
                                if (!_isPlaying) break;

                                var action = actions[i];
                                OnActionRecorded(action);
                                
                                // 模拟动作延迟
                                await Task.Delay((int)(action.Delay / speedMultiplier));
                            }

                            if (repeat && _isPlaying)
                            {
                                await Task.Delay(1000); // 重复延迟
                            }
                        }
                        while (repeat && _isPlaying);

                        if (_isPlaying)
                        {
                            OnStatusChanged($"脚本播放完成: {script.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged($"播放失败: {ex.Message}");
                        _logger.LogError($"播放脚本失败: {ex.Message}");
                    }
                    finally
                    {
                        _isPlaying = false;
                        OnPlaybackStateChanged(false);
                    }
                });

                return true;
            });
        }

        public async Task<bool> StopPlaybackAsync()
        {
            return await Task.Run(() =>
            {
                if (!_isPlaying)
                    return false;

                _isPlaying = false;
                OnPlaybackStateChanged(false);
                OnStatusChanged("停止播放脚本");

                return true;
            });
        }

        public async Task<bool> IsPlayingAsync()
        {
            return await Task.Run(() => _isPlaying);
        }

        #endregion

        #region 文件服务

        public async Task<bool> SaveScriptAsync(Guid scriptId, string filePath)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                try
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var scriptData = new
                    {
                        script.Id,
                        script.Name,
                        script.Description,
                        Status = script.Status.ToString(),
                        script.CreatedAt,
                        script.UpdatedAt,
                        script.Version,
                        Actions = script.Actions.Select(a => new
                        {
                            a.Id,
                            Type = a.Type.ToString(),
                            a.Key,
                            a.Button,
                            a.X,
                            a.Y,
                            a.Delay,
                            a.Description
                        }).ToList()
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(scriptData, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    File.WriteAllText(filePath, json);

                    OnStatusChanged($"脚本已保存: {script.Name} -> {filePath}");
                    _logger.LogInformation($"保存脚本: {script.Name} 到 {filePath}");

                    return true;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"保存失败: {ex.Message}");
                    _logger.LogError($"保存脚本失败: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> LoadScriptAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        OnStatusChanged($"文件不存在: {filePath}");
                        return false;
                    }

                    var json = File.ReadAllText(filePath);
                    var scriptData = System.Text.Json.JsonSerializer.Deserialize<JsonScriptData>(json);

                    if (scriptData == null)
                    {
                        OnStatusChanged($"文件格式错误: {filePath}");
                        return false;
                    }

                    var script = new Script(scriptData.Id, scriptData.Name, scriptData.Description);
                    _scripts[script.Id] = script;
                    _recordedActions[script.Id] = new List<GameAction>();

                    OnStatusChanged($"脚本已加载: {script.Name} <- {filePath}");
                    OnScriptCreated(script);
                    _logger.LogInformation($"加载脚本: {script.Name} 从 {filePath}");

                    return true;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"加载失败: {ex.Message}");
                    _logger.LogError($"加载脚本失败: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<Script> LoadScriptDataAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"文件不存在: {filePath}");

                    var json = File.ReadAllText(filePath);
                    var scriptData = System.Text.Json.JsonSerializer.Deserialize<JsonScriptData>(json);

                    if (scriptData == null)
                        throw new InvalidOperationException($"文件格式错误: {filePath}");

                    var script = new Script(scriptData.Id, scriptData.Name, scriptData.Description);
                    
                    // 加载动作
                    foreach (var actionData in scriptData.Actions)
                    {
                        GameAction action;
                        if (Enum.TryParse<ActionType>(actionData.Type, out var actionType))
                        {
                            switch (actionType)
                            {
                                case ActionType.KeyDown:
                                case ActionType.KeyUp:
                                    action = new GameAction(actionData.Id, actionType, actionData.Key, actionData.Delay, actionData.Description);
                                    break;
                                case ActionType.MouseDown:
                                case ActionType.MouseUp:
                                case ActionType.MouseMove:
                                    action = new GameAction(actionData.Id, actionType, actionData.Button, actionData.X, actionData.Y, actionData.Delay, actionData.Description);
                                    break;
                                case ActionType.Delay:
                                    action = new GameAction(actionData.Id, actionType, actionData.Delay, actionData.Description);
                                    break;
                                default:
                                    action = new GameAction(actionData.Id, actionType, actionData.Delay, actionData.Description);
                                    break;
                            }
                            script.AddAction(action);
                        }
                    }

                    _scripts[script.Id] = script;
                    _recordedActions[script.Id] = new List<GameAction>();

                    OnStatusChanged($"脚本数据已加载: {script.Name}");
                    OnScriptCreated(script);
                    _logger.LogInformation($"加载脚本数据: {script.Name} 从 {filePath}");

                    return script;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"加载失败: {ex.Message}");
                    _logger.LogError($"加载脚本数据失败: {ex.Message}");
                    throw;
                }
            });
        }

        #endregion

        #region 统计服务

        public async Task<Dictionary<string, object>> GetStatisticsAsync(Guid scriptId)
        {
            return await Task.Run(() =>
            {
                if (!_scripts.TryGetValue(scriptId, out var script))
                    throw new KeyNotFoundException($"脚本不存在: {scriptId}");

                var stats = new Dictionary<string, object>
                {
                    ["ScriptId"] = scriptId,
                    ["ScriptName"] = script.Name,
                    ["Status"] = script.Status.ToString(),
                    ["TotalActions"] = script.Actions.Count,
                    ["CreatedTime"] = script.CreatedAt,
                    ["UpdatedTime"] = script.UpdatedAt,
                    ["Version"] = script.Version,
                    ["EstimatedDuration"] = script.GetEstimatedDuration().TotalMilliseconds
                };

                // 动作类型统计
                var actionTypeStats = new Dictionary<string, int>();
                foreach (var action in script.Actions)
                {
                    var typeName = action.Type.ToString();
                    actionTypeStats[typeName] = actionTypeStats.GetValueOrDefault(typeName) + 1;
                }
                stats["ActionTypes"] = actionTypeStats;

                return stats;
            });
        }

        #endregion

        #region 事件触发方法

        protected virtual void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        protected virtual void OnScriptCreated(Script script)
        {
            ScriptCreated?.Invoke(this, script);
        }

        protected virtual void OnScriptUpdated(Script script)
        {
            ScriptUpdated?.Invoke(this, script);
        }

        protected virtual void OnScriptDeleted(Guid scriptId)
        {
            ScriptDeleted?.Invoke(this, scriptId);
        }

        protected virtual void OnActionRecorded(GameAction action)
        {
            ActionRecorded?.Invoke(this, action);
        }

        protected virtual void OnRecordingStateChanged(bool isRecording)
        {
            RecordingStateChanged?.Invoke(this, isRecording);
        }

        protected virtual void OnPlaybackStateChanged(bool isPlaying)
        {
            PlaybackStateChanged?.Invoke(this, isPlaying);
        }

        #endregion

        #region 辅助类

        private class JsonScriptData
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int Version { get; set; }
            public List<JsonActionData> Actions { get; set; }
        }

        private class JsonActionData
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public KeyCode Key { get; set; }
            public MouseButton Button { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Delay { get; set; }
            public string Description { get; set; }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _isRecording = false;
                _isPlaying = false;
                _currentRecordingScriptId = Guid.Empty;
                _scripts.Clear();
                _recordedActions.Clear();
                _disposed = true;
            }
        }
    }
}