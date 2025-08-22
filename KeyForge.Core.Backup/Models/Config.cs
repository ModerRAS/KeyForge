using System.Collections.Generic;

namespace KeyForge.Core.Models
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public class GlobalSettings
    {
        public int DefaultDelay { get; set; } = 100;
        public int DefaultRepeatCount { get; set; } = 1;
        public bool EnableLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "keyforge.log";
        public string ScriptsDirectory { get; set; } = "scripts";
        public bool EnableGlobalHotkeys { get; set; } = true;
        public string RecordHotkey { get; set; } = "F6";
        public string PlayHotkey { get; set; } = "F7";
        public string StopHotkey { get; set; } = "F8";
    }

    /// <summary>
    /// 主配置模型
    /// </summary>
    public class Config
    {
        public List<Script> Scripts { get; set; }
        public GlobalSettings Settings { get; set; }

        public Config()
        {
            Scripts = new List<Script>();
            Settings = new GlobalSettings();
        }

        /// <summary>
        /// 添加脚本到配置
        /// </summary>
        public void AddScript(Script script)
        {
            Scripts.Add(script);
        }

        /// <summary>
        /// 移除指定名称的脚本
        /// </summary>
        public void RemoveScript(string scriptName)
        {
            Scripts.RemoveAll(s => s.Name == scriptName);
        }

        /// <summary>
        /// 获取指定名称的脚本
        /// </summary>
        public Script GetScript(string scriptName)
        {
            return Scripts.Find(s => s.Name == scriptName);
        }

        /// <summary>
        /// 检查是否存在指定名称的脚本
        /// </summary>
        public bool ContainsScript(string scriptName)
        {
            return Scripts.Exists(s => s.Name == scriptName);
        }
    }
}