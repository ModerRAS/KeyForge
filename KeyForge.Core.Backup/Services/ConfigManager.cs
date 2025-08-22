using System;
using System.IO;
using Newtonsoft.Json;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
    using KeyForge.Domain.Common;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 配置管理器 - 简化实现
    /// </summary>
    public class ConfigManager : IConfigManager
    {
        private readonly ILoggerService _logger;

        public ConfigManager(ILoggerService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public Config LoadConfig(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Warning($"配置文件不存在: {filePath}, 创建默认配置");
                    var defaultConfig = new Config();
                    SaveConfig(defaultConfig, filePath);
                    return defaultConfig;
                }

                string json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<Config>(json);
                
                if (config == null)
                {
                    _logger.Error("配置文件格式错误，使用默认配置");
                    return new Config();
                }

                _logger.Info($"成功加载配置文件: {filePath}");
                return config;
            }
            catch (Exception ex)
            {
                _logger.Error($"加载配置文件失败: {ex.Message}");
                return new Config();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig(Config config, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                _logger.Info($"成功保存配置文件: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"保存配置文件失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 加载脚本文件
        /// </summary>
        public Script LoadScript(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"脚本文件不存在: {filePath}");
                }

                string json = File.ReadAllText(filePath);
                var script = JsonConvert.DeserializeObject<Script>(json);
                
                if (script == null)
                {
                    throw new Exception("脚本文件格式错误");
                }

                _logger.Info($"成功加载脚本: {script.Name} from {filePath}");
                return script;
            }
            catch (Exception ex)
            {
                _logger.Error($"加载脚本文件失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 保存脚本文件
        /// </summary>
        public void SaveScript(Script script, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(script, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                _logger.Info($"成功保存脚本: {script.Name} to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"保存脚本文件失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public Config CreateDefaultConfig()
        {
            var config = new Config();
            
            // 创建默认设置
            config.Settings.DefaultDelay = 100;
            config.Settings.DefaultRepeatCount = 1;
            config.Settings.EnableLogging = true;
            config.Settings.LogFilePath = "keyforge.log";
            config.Settings.ScriptsDirectory = "scripts";
            config.Settings.EnableGlobalHotkeys = true;
            config.Settings.RecordHotkey = "F6";
            config.Settings.PlayHotkey = "F7";
            config.Settings.StopHotkey = "F8";

            // 创建示例脚本
            var exampleScript = new Script
            {
                Name = "示例脚本",
                Description = "这是一个示例脚本，展示了基本的按键操作",
                RepeatCount = 1,
                Loop = false
            };

            // 添加一些示例动作
            exampleScript.AddAction(new KeyAction
            {
                Type = ActionType.KeyDown,
                Key = KeyCode.A,
                Delay = 100
            });

            exampleScript.AddAction(new KeyAction
            {
                Type = ActionType.KeyUp,
                Key = KeyCode.A,
                Delay = 50
            });

            exampleScript.AddAction(new KeyAction
            {
                Type = ActionType.Delay,
                Delay = 500
            });

            exampleScript.AddAction(new KeyAction
            {
                Type = ActionType.KeyDown,
                Key = KeyCode.B,
                Delay = 100
            });

            exampleScript.AddAction(new KeyAction
            {
                Type = ActionType.KeyUp,
                Key = KeyCode.B,
                Delay = 50
            });

            config.Scripts.Add(exampleScript);

            return config;
        }

        /// <summary>
        /// 验证配置文件
        /// </summary>
        public bool ValidateConfig(Config config)
        {
            if (config == null)
            {
                _logger.Error("配置对象为null");
                return false;
            }

            if (config.Settings == null)
            {
                _logger.Error("配置中缺少Settings");
                return false;
            }

            if (config.Settings.DefaultDelay < 0)
            {
                _logger.Error("默认延迟不能为负数");
                return false;
            }

            if (config.Settings.DefaultRepeatCount < 1)
            {
                _logger.Error("默认重复次数不能小于1");
                return false;
            }

            if (string.IsNullOrEmpty(config.Settings.ScriptsDirectory))
            {
                _logger.Error("脚本目录不能为空");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证脚本
        /// </summary>
        public bool ValidateScript(Script script)
        {
            if (script == null)
            {
                _logger.Error("脚本对象为null");
                return false;
            }

            if (string.IsNullOrEmpty(script.Name))
            {
                _logger.Error("脚本名称不能为空");
                return false;
            }

            if (script.Actions == null)
            {
                _logger.Error("脚本动作列表不能为null");
                return false;
            }

            if (script.RepeatCount < 1)
            {
                _logger.Error("脚本重复次数不能小于1");
                return false;
            }

            // 验证每个动作
            foreach (var action in script.Actions)
            {
                if (action.Delay < 0)
                {
                    _logger.Error($"动作延迟不能为负数: {action}");
                    return false;
                }

                if (action.Type == ActionType.KeyDown || action.Type == ActionType.KeyUp)
                {
                    if (action.Key == KeyCode.None)
                    {
                        _logger.Error($"按键动作必须指定有效的按键: {action}");
                        return false;
                    }
                }

                if (action.Type == ActionType.MouseDown || action.Type == ActionType.MouseUp)
                {
                    if (action.Button == MouseButton.None)
                    {
                        _logger.Error($"鼠标动作必须指定有效的按钮: {action}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}