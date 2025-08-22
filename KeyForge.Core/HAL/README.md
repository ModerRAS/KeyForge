# KeyForge 跨平台架构实现

## 概述

KeyForge项目成功实现了完整的跨平台硬件抽象层(HAL)架构，支持Windows、Linux和macOS三大平台。该架构采用SOLID设计原则，提供了统一的接口来访问平台特定的硬件功能。

## 架构设计

### 核心组件

1. **硬件抽象层接口** (`IHardwareAbstractionLayer`)
   - 统一的硬件访问接口
   - 平台无关的API设计
   - 完整的生命周期管理

2. **平台特定实现**
   - Windows HAL: 使用Win32 API
   - Linux HAL: 使用X11和系统工具 (简化版本使用xdotool)
   - macOS HAL: 使用Cocoa API (简化版本使用AppleScript)

3. **服务抽象层**
   - IKeyboardService: 键盘操作
   - IMouseService: 鼠标操作
   - IScreenService: 屏幕操作
   - IGlobalHotkeyService: 全局热键
   - IWindowService: 窗口管理
   - IImageRecognitionService: 图像识别

4. **工厂模式** (`HALFactory`)
   - 自动平台检测
   - 动态HAL实例创建
   - 配置管理

## 实现特性

### 1. 跨平台支持

- **Windows**: 完整的Win32 API支持
- **Linux**: X11支持 (简化版本使用xdotool)
- **macOS**: Cocoa API支持 (简化版本使用AppleScript)

### 2. 简化实现说明

由于时间和复杂度考虑，本实现包含以下简化：

**原本实现**:
- 完整的X11/Wayland支持
- 原生的Cocoa/Carbon API调用
- 完整的图像识别和OCR功能
- 复杂的事件处理和钩子系统

**简化实现**:
- Linux: 使用xdotool进行基础操作
- macOS: 使用AppleScript进行基础操作
- 基础的图像识别功能
- 简化的事件处理

### 3. 核心功能

#### 键盘服务
- 按键按下/释放
- 文本输入
- 组合键支持
- 键盘状态查询

#### 鼠标服务
- 鼠标移动 (平滑移动)
- 按钮点击
- 拖拽操作
- 滚轮操作

#### 屏幕服务
- 屏幕截图
- 多显示器支持
- 像素颜色获取
- 颜色查找

#### 热键服务
- 全局热键注册
- 热键冲突检测
- 优先级管理

#### 窗口服务
- 窗口查找和操作
- 窗口状态管理
- 窗口样式控制

#### 图像识别
- 图像匹配
- OCR文字识别
- 模板管理

## 使用示例

### 基础使用

```csharp
using KeyForge.CrossPlatform;
using KeyForge.HAL.Abstractions;

// 初始化HAL
var hal = await KeyForgeCrossPlatform.InitializeAsync();

// 键盘操作
await hal.Keyboard.TypeTextAsync("Hello World!");
await hal.Keyboard.PressKeyAsync(KeyCode.A, KeyModifiers.Control);

// 鼠标操作
await hal.Mouse.MoveToAsync(100, 100);
await hal.Mouse.ClickAsync(MouseButton.Left);

// 屏幕操作
var screenshot = await hal.Screen.CaptureScreenAsync();
var screens = await hal.Screen.GetScreensAsync();

// 热键操作
var hotkey = new HotkeyCombination
{
    Key = KeyCode.F1,
    Modifiers = KeyModifiers.Control | KeyModifiers.Alt
};
await hal.GlobalHotkeys.RegisterHotkeyAsync("my_hotkey", hotkey);

// 关闭HAL
await KeyForgeCrossPlatform.ShutdownAsync();
```

### 高级使用

```csharp
// 使用配置初始化
var platform = PlatformDetector.DetectPlatform();
var config = HALFactory.GetDefaultConfiguration(platform);
config.KeyboardOptions.KeyDelay = 5;
config.MouseOptions.MouseSpeed = 1.5;

var hal = await HALFactory.CreateAndInitializeHALAsync(config);

// 事件监听
hal.Keyboard.KeyPressed += (sender, e) => 
{
    Console.WriteLine($"按键按下: {e.Key}");
};

hal.Mouse.MouseMoved += (sender, e) => 
{
    Console.WriteLine($"鼠标位置: ({e.Position.X}, {e.Position.Y})");
};

// 复杂操作
await hal.Mouse.DragDropAsync(100, 100, 500, 300, MouseButton.Left);
await hal.Keyboard.TypeTextWithDelayAsync("Complex text", delays);
```

### 运行演示

```csharp
// 运行完整演示
var demo = KeyForgeCrossPlatform.CreateDemo();
await demo.RunFullDemo();

// 运行快速演示
await KeyForgeCrossPlatform.RunQuickDemoAsync();
```

## 文件结构

```
KeyForge.Core/HAL/
├── Abstractions/                 # 抽象接口定义
│   ├── IHardwareAbstractionLayer.cs
│   ├── HALClasses.cs
│   ├── HALEnums.cs
│   └── HALEventArgs.cs
├── Core/                         # 核心实现
│   ├── HALFactory.cs
│   ├── HardwareAbstractionLayerBase.cs
│   ├── PlatformDetector.cs
│   ├── PlatformConfiguration.cs
│   └── KeyForgeCrossPlatform.cs
├── Implementation/              # 平台特定实现
│   ├── Windows/
│   │   ├── WindowsHAL.cs
│   │   ├── WindowsKeyboardService.cs
│   │   ├── WindowsMouseService.cs
│   │   └── ...
│   ├── Linux/
│   │   ├── LinuxHAL.cs
│   │   ├── LinuxKeyboardService.cs
│   │   ├── LinuxMouseService.cs
│   │   └── ...
│   └── MacOS/
│       ├── MacOSHAL.cs
│       ├── MacOSKeyboardService.cs
│       ├── MacOSMouseService.cs
│       └── ...
├── Examples/                     # 使用示例
│   └── CrossPlatformExample.cs
└── Tests/                        # 单元测试
    ├── HALFactoryTests.cs
    ├── PlatformDetectorTests.cs
    └── CrossPlatformHALIntegrationTests.cs
```

## 测试

项目包含完整的单元测试和集成测试：

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "HALFactoryTests"
dotnet test --filter "PlatformDetectorTests"
dotnet test --filter "CrossPlatformHALIntegrationTests"
```

## 依赖要求

### Windows
- .NET 6.0 或更高版本
- Windows API支持 (自动包含)

### Linux
- .NET 6.0 或更高版本
- xdotool (用于简化实现)
```bash
sudo apt-get install xdotool
```

### macOS
- .NET 6.0 或更高版本
- AppleScript支持 (自动包含)

## 性能优化

### 已实现的优化
- 异步操作支持
- 事件驱动架构
- 资源管理和释放
- 平台特定优化

### 未来优化方向
- 缓存机制
- 批量操作优化
- 内存使用优化
- 线程安全改进

## 错误处理

架构提供完整的错误处理机制：

```csharp
try
{
    var hal = await KeyForgeCrossPlatform.InitializeAsync();
    // 执行操作
}
catch (HALException ex)
{
    Console.WriteLine($"HAL错误: {ex.Message}");
    Console.WriteLine($"平台: {ex.Platform}");
    Console.WriteLine($"操作: {ex.Operation}");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}
finally
{
    await KeyForgeCrossPlatform.ShutdownAsync();
}
```

## 扩展指南

### 添加新平台支持

1. 在`Platform`枚举中添加新平台
2. 创建新的平台HAL实现类
3. 在`HALFactory`中添加平台创建逻辑
4. 在`PlatformDetector`中添加平台检测逻辑

### 添加新服务功能

1. 在对应的接口中添加新方法
2. 在所有平台实现中添加方法实现
3. 更新测试用例
4. 更新文档

## 许可证

本项目遵循MIT许可证，详见LICENSE文件。

## 贡献

欢迎提交Issue和Pull Request来改进项目。

## 联系信息

如有问题，请通过GitHub Issues联系我们。