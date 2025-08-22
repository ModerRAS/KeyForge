using System;
using System.Collections.Generic;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;

namespace KeyForge.Presentation.Views
{
    /// <summary>
    /// 模拟UI视图实现 - 用于测试
    /// 原本实现：复杂的WPF/WinForms UI
    /// 简化实现：简单的内存视图，用于测试层间交互
    /// </summary>
    public class MockUIView : IUIView
    {
        public List<string> StatusMessages { get; } = new List<string>();
        public List<string> ErrorMessages { get; } = new List<string>();
        public List<Script> DisplayedScripts { get; } = new List<Script>();
        public Script CurrentScript { get; private set; }
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }
        public int CurrentProgress { get; private set; }
        public int TotalProgress { get; private set; }
        public Dictionary<string, object> CurrentStatistics { get; private set; }

        public void ShowStatus(string message)
        {
            StatusMessages.Add(message);
            Console.WriteLine($"[UI Status] {message}");
        }

        public void ShowError(string message)
        {
            ErrorMessages.Add(message);
            Console.WriteLine($"[UI Error] {message}");
        }

        public void ShowScriptList(IEnumerable<Script> scripts)
        {
            DisplayedScripts.Clear();
            DisplayedScripts.AddRange(scripts);
            Console.WriteLine($"[UI] 显示脚本列表: {DisplayedScripts.Count} 个脚本");
        }

        public void ShowScriptDetails(Script script)
        {
            CurrentScript = script;
            Console.WriteLine($"[UI] 显示脚本详情: {script?.Name}");
        }

        public void ShowRecordingStatus(bool isRecording)
        {
            IsRecording = isRecording;
            Console.WriteLine($"[UI] 录制状态: {(isRecording ? "录制中" : "已停止")}");
        }

        public void ShowPlaybackStatus(bool isPlaying)
        {
            IsPlaying = isPlaying;
            Console.WriteLine($"[UI] 播放状态: {(isPlaying ? "播放中" : "已停止")}");
        }

        public void ShowProgress(int current, int total)
        {
            CurrentProgress = current;
            TotalProgress = total;
            var percentage = total > 0 ? (current * 100.0 / total) : 0;
            Console.WriteLine($"[UI] 进度: {current}/{total} ({percentage:F1}%)");
        }

        public void ShowStatistics(Dictionary<string, object> statistics)
        {
            CurrentStatistics = new Dictionary<string, object>(statistics);
            Console.WriteLine("[UI] 显示统计信息:");
            foreach (var stat in statistics)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }
        }

        public void Reset()
        {
            StatusMessages.Clear();
            ErrorMessages.Clear();
            DisplayedScripts.Clear();
            CurrentScript = null;
            IsRecording = false;
            IsPlaying = false;
            CurrentProgress = 0;
            TotalProgress = 0;
            CurrentStatistics = null;
        }

        public bool HasStatusMessage(string message)
        {
            return StatusMessages.Contains(message);
        }

        public bool HasErrorMessage(string message)
        {
            return ErrorMessages.Contains(message);
        }

        public bool HasScriptWithName(string name)
        {
            return DisplayedScripts.Any(s => s.Name == name);
        }
    }
}