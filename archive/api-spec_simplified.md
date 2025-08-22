# KeyForge 简化API规范

## 概述

本文档定义了KeyForge按键脚本工具的简化API接口规范。基于KISS原则，只保留核心功能，移除不必要的复杂性。

## 设计原则

1. **简单实用**：只提供核心的脚本管理功能
2. **直接明了**：避免过度抽象和复杂的数据结构
3. **易于实现**：使用简单的数据类型和直接的调用方式
4. **易于测试**：减少依赖，便于单元测试

## 核心接口设计

### 1. 脚本管理接口

```csharp
namespace KeyForge.Services
{
    /// <summary>
    /// 脚本管理器 - 简化实现
    /// </summary>
    public class ScriptManager
    {
        private readonly FileStorage _fileStorage;
        private readonly Logger _logger;

        public ScriptManager(FileStorage fileStorage, Logger logger)
        {
            _fileStorage = fileStorage;
            _logger = logger;
        }

        /// <summary>
        /// 保存脚本
        /// </summary>
        /// <param name="script">脚本对象</param>
        public void SaveScript(Script script)
        {
            try
            {
                var filePath = Path.Combine("scripts", $"{script.Name}.json");
                _fileStorage.SaveJson(filePath, script);
                _logger.Info($"脚本已保存: {script.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error($"保存脚本失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 加载脚本
        /// </summary>
        /// <param name="scriptName">脚本名称</param>
        /// <returns>脚本对象</returns>
        public Script LoadScript(string scriptName)
        {
            try
            {
                var filePath = Path.Combine("scripts", $"{scriptName}.json");
                return _fileStorage.LoadJson<Script>(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载脚本失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有脚本
        /// </summary>
        /// <returns>脚本列表</returns>
        public List<Script> GetAllScripts()
        {
            try
            {
                var scripts = new List<Script>();
                var scriptDir = "scripts";
                
                if (!Directory.Exists(scriptDir))
                    return scripts;

                foreach (var file in Directory.GetFiles(scriptDir, "*.json"))
                {
                    var script = _fileStorage.LoadJson<Script>(file);
                    if (script != null)
                        scripts.Add(script);
                }

                return scripts;
            }
            catch (Exception ex)
            {
                _logger.Error($"获取脚本列表失败: {ex.Message}");
                return new List<Script>();
            }
        }

        /// <summary>
        /// 删除脚本
        /// </summary>
        /// <param name="scriptName">脚本名称</param>
        public void DeleteScript(string scriptName)
        {
            try
            {
                var filePath = Path.Combine("scripts", $"{scriptName}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.Info($"脚本已删除: {scriptName}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"删除脚本失败: {ex.Message}");
                throw;
            }
        }
    }
}
```

### 2. 脚本录制接口

```csharp
namespace KeyForge.Services
{
    /// <summary>
    /// 脚本录制器 - 简化实现
    /// </summary>
    public class ScriptRecorder
    {
        private readonly InputHelper _inputHelper;
        private readonly Logger _logger;
        private List<KeyAction> _recordedActions;
        private bool _isRecording;

        public ScriptRecorder(InputHelper inputHelper, Logger logger)
        {
            _inputHelper = inputHelper;
            _logger = logger;
            _recordedActions = new List<KeyAction>();
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _isRecording = true;
            _recordedActions.Clear();
            _inputHelper.StartRecording(OnActionRecorded);
            _logger.Info("开始录制脚本");
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            _inputHelper.StopRecording();
            _logger.Info($"录制完成，共 {_recordedActions.Count} 个动作");
        }

        /// <summary>
        /// 获取录制的脚本
        /// </summary>
        /// <returns>录制的脚本</returns>
        public Script GetRecordedScript()
        {
            return new Script
            {
                Name = $"录制脚本_{DateTime.Now:yyyyMMdd_HHmmss}",
                Description = "录制的脚本",
                Actions = new List<KeyAction>(_recordedActions),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// 已录制的动作数量
        /// </summary>
        public int RecordedActionCount => _recordedActions.Count;

        private void OnActionRecorded(KeyAction action)
        {
            if (_isRecording)
            {
                _recordedActions.Add(action);
                _logger.Debug($"录制动作: {action.Type} - {action.Key}");
            }
        }
    }
}
```

### 3. 脚本播放接口

```csharp
namespace KeyForge.Services
{
    /// <summary>
    /// 脚本播放器 - 简化实现
    /// </summary>
    public class ScriptPlayer
    {
        private readonly InputHelper _inputHelper;
        private readonly Logger _logger;
        private Script _currentScript;
        private bool _isPlaying;
        private bool _isPaused;
        private CancellationTokenSource _cancellationTokenSource;

        public ScriptPlayer(InputHelper inputHelper, Logger logger)
        {
            _inputHelper = inputHelper;
            _logger = logger;
        }

        /// <summary>
        /// 加载脚本
        /// </summary>
        /// <param name="script">脚本对象</param>
        public void LoadScript(Script script)
        {
            _currentScript = script;
            _logger.Info($"加载脚本: {script.Name}");
        }

        /// <summary>
        /// 播放脚本
        /// </summary>
        /// <param name="repeatCount">重复次数</param>
        public async Task PlayScriptAsync(int repeatCount = 1)
        {
            if (_currentScript == null || _isPlaying)
                return;

            _isPlaying = true;
            _isPaused = false;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                _logger.Info($"开始播放脚本: {_currentScript.Name}");
                
                for (int i = 0; i < repeatCount; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    await ExecuteActionsAsync(_currentScript.Actions, _cancellationTokenSource.Token);
                    
                    if (i < repeatCount - 1)
                    {
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("脚本播放已取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"脚本播放失败: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
                _cancellationTokenSource?.Dispose();
                _logger.Info("脚本播放完成");
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void PauseScript()
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
                _logger.Info("脚本播放已暂停");
            }
        }

        /// <summary>
        /// 恢复播放
        /// </summary>
        public void ResumeScript()
        {
            if (_isPlaying && _isPaused)
            {
                _isPaused = false;
                _logger.Info("脚本播放已恢复");
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopScript()
        {
            if (_isPlaying)
            {
                _cancellationTokenSource?.Cancel();
                _isPlaying = false;
                _isPaused = false;
                _logger.Info("脚本播放已停止");
            }
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused => _isPaused;

        private async Task ExecuteActionsAsync(List<KeyAction> actions, CancellationToken cancellationToken)
        {
            foreach (var action in actions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                while (_isPaused && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                    break;

                await ExecuteActionAsync(action);
            }
        }

        private async Task ExecuteActionAsync(KeyAction action)
        {
            try
            {
                switch (action.Type)
                {
                    case ActionType.KeyPress:
                        _inputHelper.SendKey(action.Key);
                        break;
                    case ActionType.KeyRelease:
                        // 释放按键
                        break;
                    case ActionType.MouseMove:
                        _inputHelper.MoveMouse(action.X, action.Y);
                        break;
                    case ActionType.MouseClick:
                        _inputHelper.ClickMouse(action.X, action.Y);
                        break;
                    case ActionType.Delay:
                        await Task.Delay(action.Delay);
                        break;
                    case ActionType.Screenshot:
                        // 截图功能
                        break;
                }

                _logger.Debug($"执行动作: {action.Type}");
            }
            catch (Exception ex)
            {
                _logger.Error($"执行动作失败: {action.Type} - {ex.Message}");
            }
        }
    }
}
```

### 4. 输入助手接口

```csharp
namespace KeyForge.Utilities
{
    /// <summary>
    /// 输入助手 - 简化实现
    /// </summary>
    public class InputHelper
    {
        private readonly Logger _logger;
        private LowLevelKeyboardHook _keyboardHook;
        private LowLevelMouseHook _mouseHook;
        private Action<KeyAction> _onActionRecorded;

        public InputHelper(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 开始录制输入
        /// </summary>
        /// <param name="onActionRecorded">动作录制回调</param>
        public void StartRecording(Action<KeyAction> onActionRecorded)
        {
            _onActionRecorded = onActionRecorded;
            
            // 设置键盘钩子
            _keyboardHook = new LowLevelKeyboardHook();
            _keyboardHook.KeyPressed += OnKeyPressed;
            _keyboardHook.Start();
            
            // 设置鼠标钩子
            _mouseHook = new LowLevelMouseHook();
            _mouseHook.MouseAction += OnMouseAction;
            _mouseHook.Start();
            
            _logger.Info("开始录制输入");
        }

        /// <summary>
        /// 停止录制输入
        /// </summary>
        public void StopRecording()
        {
            _keyboardHook?.Stop();
            _mouseHook?.Stop();
            _keyboardHook?.Dispose();
            _mouseHook?.Dispose();
            _onActionRecorded = null;
            _logger.Info("停止录制输入");
        }

        /// <summary>
        /// 发送按键
        /// </summary>
        /// <param name="key">按键字符串</param>
        public void SendKey(string key)
        {
            try
            {
                SendKeys.Send(key);
                _logger.Debug($"发送按键: {key}");
            }
            catch (Exception ex)
            {
                _logger.Error($"发送按键失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 移动鼠标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void MoveMouse(int x, int y)
        {
            try
            {
                Cursor.Position = new Point(x, y);
                _logger.Debug($"移动鼠标到: ({x}, {y})");
            }
            catch (Exception ex)
            {
                _logger.Error($"移动鼠标失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 点击鼠标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void ClickMouse(int x, int y)
        {
            try
            {
                MoveMouse(x, y);
                mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                _logger.Debug($"点击鼠标: ({x}, {y})");
            }
            catch (Exception ex)
            {
                _logger.Error($"点击鼠标失败: {ex.Message}");
                throw;
            }
        }

        private void OnKeyPressed(object sender, KeyPressedEventArgs e)
        {
            _onActionRecorded?.Invoke(new KeyAction
            {
                Type = ActionType.KeyPress,
                Key = e.Key.ToString(),
                Timestamp = DateTime.Now
            });
        }

        private void OnMouseAction(object sender, MouseActionEventArgs e)
        {
            _onActionRecorded?.Invoke(new KeyAction
            {
                Type = ActionType.MouseClick,
                X = e.X,
                Y = e.Y,
                Timestamp = DateTime.Now
            });
        }

        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
    }
}
```

### 5. 热键管理接口

```csharp
namespace KeyForge.Services
{
    /// <summary>
    /// 热键管理器 - 简化实现
    /// </summary>
    public class HotkeyManager
    {
        private readonly InputHelper _inputHelper;
        private readonly Logger _logger;
        private Dictionary<int, Action> _hotkeyActions;
        private int _hotkeyIdCounter = 1;

        public HotkeyManager(InputHelper inputHelper, Logger logger)
        {
            _inputHelper = inputHelper;
            _logger = logger;
            _hotkeyActions = new Dictionary<int, Action>();
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="modifiers">修饰键</param>
        /// <param name="action">动作</param>
        public void RegisterHotkey(Keys key, KeyModifiers modifiers, Action action)
        {
            var hotkeyId = _hotkeyIdCounter++;
            _hotkeyActions[hotkeyId] = action;
            
            // 简化实现：使用Windows Forms的ProcessCmdKey
            _logger.Info($"注册热键: {key} + {modifiers}");
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="modifiers">修饰键</param>
        public void UnregisterHotkey(Keys key, KeyModifiers modifiers)
        {
            // 简化实现
            _logger.Info($"注销热键: {key} + {modifiers}");
        }

        /// <summary>
        /// 处理热键按下
        /// </summary>
        /// <param name="keyData">按键数据</param>
        /// <returns>是否已处理</returns>
        public bool ProcessHotkey(Keys keyData)
        {
            // 简化实现：检查预设的热键
            switch (keyData)
            {
                case Keys.F6:
                    // 开始/停止录制
                    _logger.Info("热键 F6 被按下");
                    return true;
                case Keys.F7:
                    // 播放脚本
                    _logger.Info("热键 F7 被按下");
                    return true;
                case Keys.F8:
                    // 停止播放
                    _logger.Info("热键 F8 被按下");
                    return true;
            }
            return false;
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
}
```

## 数据模型定义

```csharp
namespace KeyForge.Models
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        KeyPress,
        KeyRelease,
        MouseMove,
        MouseClick,
        MouseDown,
        MouseUp,
        Delay,
        Screenshot
    }

    /// <summary>
    /// 按键动作
    /// </summary>
    public class KeyAction
    {
        public ActionType Type { get; set; }
        public string Key { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Delay { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 脚本
    /// </summary>
    public class Script
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<KeyAction> Actions { get; set; }
        public int RepeatCount { get; set; }
        public bool Loop { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Script()
        {
            Actions = new List<KeyAction>();
            RepeatCount = 1;
            Loop = false;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 全局设置
    /// </summary>
    public class GlobalSettings
    {
        public int DefaultDelay { get; set; } = 100;
        public bool EnableLogging { get; set; } = true;
        public string ScriptsDirectory { get; set; } = "scripts";
        public string RecordHotkey { get; set; } = "F6";
        public string PlayHotkey { get; set; } = "F7";
        public string StopHotkey { get; set; } = "F8";
    }
}
```

## 使用示例

### 1. 基本使用

```csharp
// 创建服务
var logger = new Logger();
var fileStorage = new FileStorage(logger);
var inputHelper = new InputHelper(logger);
var scriptManager = new ScriptManager(fileStorage, logger);
var scriptRecorder = new ScriptRecorder(inputHelper, logger);
var scriptPlayer = new ScriptPlayer(inputHelper, logger);

// 录制脚本
scriptRecorder.StartRecording();
// ... 执行一些按键和鼠标操作
scriptRecorder.StopRecording();
var recordedScript = scriptRecorder.GetRecordedScript();

// 保存脚本
scriptManager.SaveScript(recordedScript);

// 加载并播放脚本
var loadedScript = scriptManager.LoadScript(recordedScript.Name);
scriptPlayer.LoadScript(loadedScript);
await scriptPlayer.PlayScriptAsync();
```

### 2. 高级使用

```csharp
// 批量处理脚本
var scripts = scriptManager.GetAllScripts();
foreach (var script in scripts)
{
    scriptPlayer.LoadScript(script);
    await scriptPlayer.PlayScriptAsync(script.RepeatCount);
}

// 自定义脚本
var customScript = new Script
{
    Name = "自定义脚本",
    Description = "自定义的脚本",
    Actions = new List<KeyAction>
    {
        new KeyAction { Type = ActionType.KeyPress, Key = "A", Delay = 100 },
        new KeyAction { Type = ActionType.Delay, Delay = 500 },
        new KeyAction { Type = ActionType.MouseClick, X = 100, Y = 200, Delay = 100 }
    }
};

scriptManager.SaveScript(customScript);
```

## 总结

这个简化的API规范具有以下特点：

1. **简单明了**：接口数量少，功能明确
2. **易于实现**：使用简单的数据类型和直接的调用方式
3. **易于测试**：减少依赖，便于单元测试
4. **易于扩展**：可以方便地添加新功能
5. **符合KISS原则**：避免了过度设计

通过这种简化，开发者可以快速理解和使用API，同时保持了足够的功能完整性。