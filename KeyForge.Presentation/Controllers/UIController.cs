using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Presentation.Interfaces;

namespace KeyForge.Presentation.Controllers
{
    /// <summary>
    /// UI控制器实现 - 简化版本
    /// 原本实现：复杂的MVC/MVVM模式
    /// 简化实现：简单的控制器，用于测试层间交互
    /// </summary>
    public class UIController : IUIController, IDisposable
    {
        private readonly IApplicationService _applicationService;
        private readonly IUIView _view;
        private readonly List<IDisposable> _eventSubscriptions;
        private bool _disposed = false;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<Script> ScriptCreated;
        public event EventHandler<Script> ScriptUpdated;
        public event EventHandler<Guid> ScriptDeleted;
        public event EventHandler<GameAction> ActionRecorded;
        public event EventHandler<bool> RecordingStateChanged;
        public event EventHandler<bool> PlaybackStateChanged;

        public UIController(IApplicationService applicationService, IUIView view)
        {
            _applicationService = applicationService;
            _view = view;
            _eventSubscriptions = new List<IDisposable>();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // 订阅应用服务事件
            _applicationService.StatusChanged += (sender, message) =>
            {
                _view.ShowStatus(message);
                StatusChanged?.Invoke(sender, message);
            };

            _applicationService.ScriptCreated += (sender, script) =>
            {
                _view.ShowScriptDetails(script);
                RefreshScriptList();
                ScriptCreated?.Invoke(sender, script);
            };

            _applicationService.ScriptUpdated += (sender, script) =>
            {
                _view.ShowScriptDetails(script);
                RefreshScriptList();
                ScriptUpdated?.Invoke(sender, script);
            };

            _applicationService.ScriptDeleted += (sender, scriptId) =>
            {
                RefreshScriptList();
                ScriptDeleted?.Invoke(sender, scriptId);
            };

            _applicationService.ActionRecorded += (sender, action) =>
            {
                UpdateRecordingProgress();
                ActionRecorded?.Invoke(sender, action);
            };

            _applicationService.RecordingStateChanged += (sender, isRecording) =>
            {
                _view.ShowRecordingStatus(isRecording);
                RecordingStateChanged?.Invoke(sender, isRecording);
            };

            _applicationService.PlaybackStateChanged += (sender, isPlaying) =>
            {
                _view.ShowPlaybackStatus(isPlaying);
                PlaybackStateChanged?.Invoke(sender, isPlaying);
            };
        }

        #region 脚本管理

        public async Task<bool> CreateScriptAsync(string name, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _view.ShowError("脚本名称不能为空");
                    return false;
                }

                var script = await _applicationService.CreateScriptAsync(name, description);
                return script != null;
            }
            catch (Exception ex)
            {
                _view.ShowError($"创建脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateScriptAsync(Guid scriptId, string name, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _view.ShowError("脚本名称不能为空");
                    return false;
                }

                var script = await _applicationService.UpdateScriptAsync(scriptId, name, description);
                return script != null;
            }
            catch (Exception ex)
            {
                _view.ShowError($"更新脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteScriptAsync(Guid scriptId)
        {
            try
            {
                return await _applicationService.DeleteScriptAsync(scriptId);
            }
            catch (Exception ex)
            {
                _view.ShowError($"删除脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<Script> GetScriptAsync(Guid scriptId)
        {
            try
            {
                var script = await _applicationService.GetScriptAsync(scriptId);
                _view.ShowScriptDetails(script);
                return script;
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取脚本失败: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Script>> GetAllScriptsAsync()
        {
            try
            {
                var scripts = await _applicationService.GetAllScriptsAsync();
                _view.ShowScriptList(scripts);
                return scripts;
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取脚本列表失败: {ex.Message}");
                return Enumerable.Empty<Script>();
            }
        }

        #endregion

        #region 录制控制

        public async Task<bool> StartRecordingAsync(Guid scriptId)
        {
            try
            {
                return await _applicationService.StartRecordingAsync(scriptId);
            }
            catch (Exception ex)
            {
                _view.ShowError($"开始录制失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StopRecordingAsync()
        {
            try
            {
                return await _applicationService.StopRecordingAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError($"停止录制失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsRecordingAsync()
        {
            try
            {
                return await _applicationService.IsRecordingAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取录制状态失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 播放控制

        public async Task<bool> StartPlaybackAsync(Guid scriptId, float speedMultiplier = 1.0f, bool repeat = false)
        {
            try
            {
                return await _applicationService.StartPlaybackAsync(scriptId, speedMultiplier, repeat);
            }
            catch (Exception ex)
            {
                _view.ShowError($"开始播放失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StopPlaybackAsync()
        {
            try
            {
                return await _applicationService.StopPlaybackAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError($"停止播放失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsPlayingAsync()
        {
            try
            {
                return await _applicationService.IsPlayingAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取播放状态失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 文件操作

        public async Task<bool> SaveScriptAsync(Guid scriptId, string filePath)
        {
            try
            {
                return await _applicationService.SaveScriptAsync(scriptId, filePath);
            }
            catch (Exception ex)
            {
                _view.ShowError($"保存脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadScriptAsync(string filePath)
        {
            try
            {
                return await _applicationService.LoadScriptAsync(filePath);
            }
            catch (Exception ex)
            {
                _view.ShowError($"加载脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExportScriptAsync(Guid scriptId, string format)
        {
            try
            {
                var script = await _applicationService.GetScriptAsync(scriptId);
                var filePath = $"{script.Name}.{format.ToLower()}";
                return await _applicationService.SaveScriptAsync(scriptId, filePath);
            }
            catch (Exception ex)
            {
                _view.ShowError($"导出脚本失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ImportScriptAsync(string filePath, string format)
        {
            try
            {
                return await _applicationService.LoadScriptAsync(filePath);
            }
            catch (Exception ex)
            {
                _view.ShowError($"导入脚本失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 辅助方法

        private async void RefreshScriptList()
        {
            try
            {
                var scripts = await _applicationService.GetAllScriptsAsync();
                _view.ShowScriptList(scripts);
            }
            catch (Exception ex)
            {
                _view.ShowError($"刷新脚本列表失败: {ex.Message}");
            }
        }

        private async void UpdateRecordingProgress()
        {
            try
            {
                // 更新录制进度显示
                var isRecording = await _applicationService.IsRecordingAsync();
                if (isRecording)
                {
                    // 这里可以添加更详细的进度显示逻辑
                    _view.ShowProgress(0, 100); // 简化的进度显示
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"更新录制进度失败: {ex.Message}");
            }
        }

        public async Task ShowStatisticsAsync(Guid scriptId)
        {
            try
            {
                var stats = await _applicationService.GetStatisticsAsync(scriptId);
                _view.ShowStatistics(stats);
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取统计信息失败: {ex.Message}");
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                // 取消事件订阅
                foreach (var subscription in _eventSubscriptions)
                {
                    subscription.Dispose();
                }
                _eventSubscriptions.Clear();

                _disposed = true;
            }
        }
    }
}