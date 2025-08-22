using KeyForge.Core.Models;
    using KeyForge.Domain.Common;

namespace KeyForge.Core.Interfaces
{
    /// <summary>
    /// 输入模拟器接口 - 简化实现
    /// </summary>
    public interface IInputSimulator
    {
        void KeyDown(KeyCode key);
        void KeyUp(KeyCode key);
        void KeyPress(KeyCode key, int delay = 50);
        void MouseDown(MouseButton button, int x, int y);
        void MouseUp(MouseButton button, int x, int y);
        void MouseMove(int x, int y);
        void MouseClick(MouseButton button, int x, int y, int delay = 100);
        void MouseDoubleClick(MouseButton button, int x, int y);
        void MouseScroll(int delta);
        
        // 额外的方法，为了兼容现有代码
        void SendMouse(MouseButton button, MouseState state);
        void SendKey(KeyCode key, KeyState state);
        void MoveMouse(int x, int y);
    }

    /// <summary>
    /// 脚本录制器接口
    /// </summary>
    public interface IScriptRecorder
    {
        void StartRecording();
        void StopRecording();
        bool IsRecording { get; }
        Script GetRecordedScript();
        void ClearRecording();
    }

    /// <summary>
    /// 脚本播放器接口
    /// </summary>
    public interface IScriptPlayer
    {
        void LoadScript(Script script);
        void PlayScript();
        void PauseScript();
        void StopScript();
        bool IsPlaying { get; }
        bool IsPaused { get; }
        Script CurrentScript { get; }
    }

    /// <summary>
    /// 配置管理器接口
    /// </summary>
    public interface IConfigManager
    {
        Config LoadConfig(string filePath);
        void SaveConfig(Config config, string filePath);
        Script LoadScript(string filePath);
        void SaveScript(Script script, string filePath);
    }

    /// <summary>
    /// 日志服务接口 - 简化实现
    /// </summary>
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogDebug(string message);
        
        // 简化方法名，为了兼容现有代码
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception ex = null);
        void Debug(string message);
    }
}