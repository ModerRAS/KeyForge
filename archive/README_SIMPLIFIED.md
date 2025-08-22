# KeyForge - 简化版按键脚本工具

## 🎯 项目概述

这是一个**简化版**的按键脚本工具，专注于核心功能：按键模拟、脚本录制和回放。相比之前的过度复杂设计，这个版本更加实用和易于维护。

## ✨ 核心功能

1. **按键模拟** - 使用Windows API模拟键盘和鼠标操作
2. **脚本录制** - 记录用户的键盘和鼠标操作序列
3. **脚本回放** - 执行录制的脚本，支持重复和循环
4. **配置管理** - JSON格式的配置文件存储
5. **简单UI** - WinForms界面，易于使用

## 🏗️ 项目结构

```
KeyForge/
├── KeyForge.Core/                 # 核心库
│   ├── Models/                    # 数据模型
│   │   ├── KeyAction.cs          # 按键动作模型
│   │   ├── Script.cs              # 脚本模型
│   │   └── Config.cs              # 配置模型
│   ├── Services/                  # 服务实现
│   │   ├── InputSimulator.cs      # 输入模拟器
│   │   ├── ScriptRecorder.cs      # 脚本录制器
│   │   ├── ScriptPlayer.cs        # 脚本播放器
│   │   ├── ConfigManager.cs       # 配置管理器
│   │   └── LoggerService.cs       # 日志服务
│   └── Interfaces/                # 接口定义
│       └── IServices.cs           # 服务接口
├── KeyForge.UI/                   # 用户界面
│   ├── Forms/
│   │   └── MainForm.cs            # 主窗体
│   └── Program.cs                 # 程序入口
└── docs/                          # 文档
    └── SIMPLIFIED_ARCHITECTURE.md  # 简化架构设计
```

## 🚀 使用方法

### 1. 编译项目

```bash
cd KeyForge
dotnet build
```

### 2. 运行程序

```bash
dotnet run --project KeyForge.UI
```

### 3. 基本操作

1. **录制脚本**：
   - 点击"开始录制"按钮
   - 执行你想要录制的键盘和鼠标操作
   - 点击"停止录制"按钮
   - 保存脚本

2. **播放脚本**：
   - 从脚本列表中选择一个脚本
   - 设置重复次数和循环选项
   - 点击"播放"按钮

3. **管理脚本**：
   - 加载外部脚本文件
   - 保存脚本到文件
   - 删除不需要的脚本

## 🔧 技术特性

### 核心技术栈
- **C# .NET 6.0** - 现代化的开发框架
- **WinForms** - 简单易用的桌面UI框架
- **Windows API** - 直接的底层按键模拟
- **JSON** - 轻量级配置存储

### 简化实现要点

#### 1. 直接使用Windows API
```csharp
// 简化实现：直接调用Windows API
[DllImport("user32.dll")]
private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
```

#### 2. 全局钩子录制
```csharp
// 简化实现：使用全局钩子捕获键盘和鼠标事件
private GlobalKeyboardHook _keyboardHook;
private GlobalMouseHook _mouseHook;
```

#### 3. JSON配置存储
```csharp
// 简化实现：直接JSON序列化
string json = JsonConvert.SerializeObject(script, Formatting.Indented);
File.WriteAllText(filePath, json);
```

#### 4. 简单的UI设计
```csharp
// 简化实现：拖拽控件创建界面
var recordButton = new Button
{
    Text = "开始录制",
    Location = new Point(10, 20),
    Size = new Size(100, 30)
};
```

## 📊 性能指标

- **响应时间**: < 10ms 按键延迟
- **内存占用**: < 50MB 运行时内存
- **CPU使用**: < 5% 空闲时CPU占用
- **脚本大小**: 典型脚本 < 10KB

## 🎯 验证标准

### MVP功能验证
- [x] 能够成功录制按键序列
- [x] 能够正确回放录制的脚本
- [x] 界面基本功能可用
- [x] 配置文件读写正常

### 核心功能验证
- [x] 支持常用的键盘按键
- [x] 支持鼠标点击和移动
- [x] 延迟功能正常工作
- [x] 循环播放功能正常

## 🔒 安全性考虑

- **最小权限**: 仅需要普通用户权限
- **本地运行**: 不发送网络请求
- **文件安全**: 不修改系统文件
- **紧急停止**: 提供停止按钮防止失控

## 📝 与原设计的对比

### 原设计的问题
- ❌ 过度工程化（DDD、微服务、事件驱动）
- ❌ 复杂的技术栈（OpenCV、Redis、RabbitMQ）
- ❌ 不必要的图像识别功能
- ❌ 缺乏核心功能验证

### 简化版的优势
- ✅ 专注于核心功能
- ✅ 简单的技术栈
- ✅ 快速开发和验证
- ✅ 易于维护和扩展

## 🚀 下一步开发计划

### 第一阶段（已完成）
- [x] 基础按键模拟功能
- [x] 简单的录制/回放
- [x] 基础WinForms界面
- [x] JSON配置文件

### 第二阶段（可选）
- [ ] 快捷键支持
- [ ] 脚本编辑功能
- [ ] 更好的错误处理
- [ ] 性能优化

### 第三阶段（未来）
- [ ] 脚本编辑器
- [ ] 插件系统
- [ ] 更高级的UI
- [ ] 云同步功能

## 📝 使用说明

### 注意事项
1. 仅用于合法的自动化任务
2. 不要用于游戏作弊（可能违反服务条款）
3. 遵守相关法律法规
4. 尊重软件的使用条款

### 常见问题
1. **按键不生效**: 检查是否有管理员权限
2. **录制失败**: 确保没有其他按键冲突
3. **播放异常**: 检查脚本格式是否正确
4. **UI卡顿**: 检查系统资源占用

## 🤝 贡献指南

欢迎提交Issue和Pull Request来改进这个项目！

---

这个简化版的KeyForge工具专注于实用性，避免了过度工程化，适合快速开发和验证核心概念。如果需要更复杂的功能，可以基于这个基础进行扩展。