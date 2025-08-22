using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Common;
using KeyForge.Core.Models;

namespace KeyForge.Presentation.Interfaces
{
    /// <summary>
    /// UI控制器接口 - 简化实现
    /// 原本实现：复杂的MVVM框架
    /// 简化实现：简单的控制器接口，用于测试层间交互
    /// </summary>
    public interface IUIController
    {
        // 脚本管理
        Task<bool> CreateScriptAsync(string name, string description);
        Task<bool> UpdateScriptAsync(Guid scriptId, string name, string description);
        Task<bool> DeleteScriptAsync(Guid scriptId);
        Task<Script> GetScriptAsync(Guid scriptId);
        Task<IEnumerable<Script>> GetAllScriptsAsync();

        // 录制控制
        Task<bool> StartRecordingAsync(Guid scriptId);
        Task<bool> StopRecordingAsync();
        Task<bool> IsRecordingAsync();

        // 播放控制
        Task<bool> StartPlaybackAsync(Guid scriptId, float speedMultiplier = 1.0f, bool repeat = false);
        Task<bool> StopPlaybackAsync();
        Task<bool> IsPlayingAsync();

        // 文件操作
        Task<bool> SaveScriptAsync(Guid scriptId, string filePath);
        Task<bool> LoadScriptAsync(string filePath);
        Task<bool> ExportScriptAsync(Guid scriptId, string format);
        Task<bool> ImportScriptAsync(string filePath, string format);

        // 状态通知
        event EventHandler<string> StatusChanged;
        event EventHandler<Script> ScriptCreated;
        event EventHandler<Script> ScriptUpdated;
        event EventHandler<Guid> ScriptDeleted;
        event EventHandler<GameAction> ActionRecorded;
        event EventHandler<bool> RecordingStateChanged;
        event EventHandler<bool> PlaybackStateChanged;
    }

    /// <summary>
    /// UI视图接口
    /// </summary>
    public interface IUIView
    {
        void ShowStatus(string message);
        void ShowError(string message);
        void ShowScriptList(IEnumerable<Script> scripts);
        void ShowScriptDetails(Script script);
        void ShowRecordingStatus(bool isRecording);
        void ShowPlaybackStatus(bool isPlaying);
        void ShowProgress(int current, int total);
        void ShowStatistics(Dictionary<string, object> statistics);
    }

    /// <summary>
    /// 应用服务接口
    /// </summary>
    public interface IApplicationService
    {
        // 脚本管理
        Task<Script> CreateScriptAsync(string name, string description);
        Task<Script> UpdateScriptAsync(Guid scriptId, string name, string description);
        Task<bool> DeleteScriptAsync(Guid scriptId);
        Task<Script> GetScriptAsync(Guid scriptId);
        Task<IEnumerable<Script>> GetAllScriptsAsync();

        // 录制服务
        Task<bool> StartRecordingAsync(Guid scriptId);
        Task<bool> StopRecordingAsync();
        Task<bool> AddActionAsync(Guid scriptId, GameAction action);
        Task<bool> IsRecordingAsync();

        // 播放服务
        Task<bool> StartPlaybackAsync(Guid scriptId, float speedMultiplier = 1.0f, bool repeat = false);
        Task<bool> StopPlaybackAsync();
        Task<bool> IsPlayingAsync();

        // 文件服务
        Task<bool> SaveScriptAsync(Guid scriptId, string filePath);
        Task<bool> LoadScriptAsync(string filePath);
        Task<Script> LoadScriptDataAsync(string filePath);

        // 统计服务
        Task<Dictionary<string, object>> GetStatisticsAsync(Guid scriptId);
    }
}