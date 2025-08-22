# KeyForge 简化版架构设计

## 🎯 项目重新定位

### 核心目标
创建一个简单实用的按键脚本工具，专注于**按键模拟**和**脚本录制/回放**功能。

### 核心功能
1. **按键模拟**：模拟键盘和鼠标操作
2. **脚本录制**：记录用户操作序列
3. **脚本回放**：执行录制的脚本
4. **配置管理**：简单的JSON配置文件
5. **基础UI**：WinForms界面

## 🏗️ 简化架构

### 整体架构
```
┌─────────────────┐
│   WinForms UI   │
├─────────────────┤
│  Script Engine  │
├─────────────────┤
│ Input Simulator │
├─────────────────┤
│ Windows API     │
└─────────────────┘
```

### 技术栈选择

#### 核心技术
- **语言**: C# (.NET 6.0+)
- **UI框架**: WinForms
- **按键模拟**: Windows API (user32.dll)
- **配置存储**: JSON文件
- **日志**: 基础的文本日志

#### 第三方库（最小化）
- **Newtonsoft.Json**: JSON序列化
- **Serilog**: 日志记录（可选）

## 📁 项目结构

```
KeyForge/
├── KeyForge.sln
├── KeyForge.Core/
│   ├── Models/
│   │   ├── KeyAction.cs
│   │   ├── Script.cs
│   │   └── Config.cs
│   ├── Services/
│   │   ├── InputSimulator.cs
│   │   ├── ScriptRecorder.cs
│   │   ├── ScriptPlayer.cs
│   │   └── ConfigManager.cs
│   └── Interfaces/
│       ├── IInputSimulator.cs
│       └── IScriptManager.cs
├── KeyForge.UI/
│   ├── Forms/
│   │   ├── MainForm.cs
│   │   ├── ScriptEditorForm.cs
│   │   └── SettingsForm.cs
│   └── Controls/
│       └── ScriptListControl.cs
└── KeyForge.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

## 🔧 核心组件设计

### 1. InputSimulator (输入模拟器)
```csharp
public class InputSimulator : IInputSimulator
{
    public void SendKey(KeyCode key, KeyState state)
    public void SendMouse(MouseButton button, MouseState state)
    public void MoveMouse(int x, int y)
    public void Delay(int milliseconds)
}
```

### 2. ScriptRecorder (脚本录制器)
```csharp
public class ScriptRecorder : IScriptRecorder
{
    public void StartRecording()
    public void StopRecording()
    public void RecordKeyAction(KeyAction action)
    public void SaveScript(string filePath)
}
```

### 3. ScriptPlayer (脚本播放器)
```csharp
public class ScriptPlayer : IScriptPlayer
{
    public void LoadScript(string filePath)
    public void PlayScript()
    public void PauseScript()
    public void StopScript()
}
```

### 4. ConfigManager (配置管理器)
```csharp
public class ConfigManager
{
    public Config LoadConfig(string filePath)
    public void SaveConfig(Config config, string filePath)
    public KeyAction ParseKeyAction(string configString)
}
```

## 📋 数据模型

### KeyAction (按键动作)
```csharp
public class KeyAction
{
    public ActionType Type { get; set; } // KeyDown, KeyUp, MouseMove
    public KeyCode Key { get; set; }     // 按键代码
    public MouseButton Button { get; set; } // 鼠标按钮
    public int X { get; set; }           // 鼠标X坐标
    public int Y { get; set; }           // 鼠标Y坐标
    public int Delay { get; set; }       // 延迟时间(ms)
    public DateTime Timestamp { get; set; } // 时间戳
}
```

### Script (脚本)
```csharp
public class Script
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<KeyAction> Actions { get; set; }
    public int RepeatCount { get; set; }
    public bool Loop { get; set; }
}
```

### Config (配置)
```csharp
public class Config
{
    public List<Script> Scripts { get; set; }
    public GlobalSettings Settings { get; set; }
}
```

## 🎮 用户界面设计

### 主界面
- **脚本列表**: 显示所有可用脚本
- **控制按钮**: 录制、播放、暂停、停止
- **状态栏**: 显示当前状态和日志
- **快捷键**: 全局快捷键支持

### 脚本编辑器
- **动作列表**: 显示脚本中的所有动作
- **编辑功能**: 修改动作参数
- **测试功能**: 单步执行测试

### 设置界面
- **全局设置**: 默认延迟、重复次数等
- **快捷键配置**: 自定义快捷键
- **日志设置**: 日志级别和文件路径

## 🔄 核心流程

### 录制流程
1. 用户点击"录制"按钮
2. 系统开始监听键盘和鼠标事件
3. 记录每个动作和时间戳
4. 用户点击"停止"按钮
5. 保存脚本到JSON文件

### 播放流程
1. 用户选择脚本
2. 系统加载脚本文件
3. 按顺序执行每个动作
4. 处理延迟和重复
5. 完成或循环执行

## ⚡ 性能考虑

### 响应时间
- 按键响应时间: < 10ms
- 鼠标移动精度: ±1px
- 脚本执行延迟: < 50ms

### 资源使用
- 内存占用: < 50MB
- CPU使用率: < 5% (空闲时)
- 磁盘空间: < 10MB

## 🔒 安全性考虑

### 权限要求
- 仅需要基本的用户权限
- 不需要管理员权限
- 不修改系统文件

### 风险控制
- 不发送网络请求
- 不读取敏感文件
- 提供紧急停止功能

## 📝 简化实现要点

### 简化实现1: 直接使用Windows API
```csharp
// 简化实现：直接调用Windows API
[DllImport("user32.dll")]
private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

public void SendKey(KeyCode key, KeyState state)
{
    byte keyCode = (byte)key;
    byte flags = (state == KeyState.Down) ? 0 : 2;
    keybd_event(keyCode, 0, flags, 0);
}
```

### 简化实现2: JSON配置存储
```csharp
// 简化实现：直接JSON序列化
public void SaveScript(Script script, string filePath)
{
    string json = JsonConvert.SerializeObject(script, Formatting.Indented);
    File.WriteAllText(filePath, json);
}
```

### 简化实现3: 基础的WinForms界面
```csharp
// 简化实现：拖拽控件创建界面
public MainForm()
{
    InitializeComponent();
    // 基础控件布局
    this.Controls.Add(scriptListBox);
    this.Controls.Add(recordButton);
    this.Controls.Add(playButton);
}
```

## 🚀 开发优先级

### 第一阶段 (MVP)
1. 基础按键模拟功能
2. 简单的录制/回放
3. 基础WinForms界面
4. JSON配置文件

### 第二阶段 (增强)
1. 快捷键支持
2. 脚本编辑功能
3. 日志记录
4. 错误处理

### 第三阶段 (优化)
1. 性能优化
2. 界面美化
3. 高级功能
4. 插件系统（如果需要）

## 🎯 验证标准

### MVP验证
- [ ] 能够成功录制按键序列
- [ ] 能够正确回放录制的脚本
- [ ] 界面基本功能可用
- [ ] 配置文件读写正常

### 功能验证
- [ ] 支持常用的键盘按键
- [ ] 支持鼠标点击和移动
- [ ] 延迟功能正常工作
- [ ] 循环播放功能正常

这个简化架构专注于核心功能，避免了过度工程化，适合快速开发和验证核心概念。