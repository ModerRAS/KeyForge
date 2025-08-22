# KeyForge 简化技术栈选择

## 技术选型原则

基于KISS原则，选择最简单、最直接的技术栈：

1. **简单实用**：选择成熟、稳定的技术
2. **易于维护**：减少复杂依赖，便于后续维护
3. **性能优先**：保证按键执行的实时性和准确性
4. **开发效率**：提高开发速度，减少学习成本

## 核心技术栈

### 1. 运行时环境

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| .NET | 8.0 | 现代化、高性能、LTS支持 |
| C# | 12.0 | 语言特性丰富，开发效率高 |
| Windows Forms | .NET 8.0 | 成熟稳定，适合桌面应用 |

**选择理由**：
- .NET 8.0是长期支持版本，性能优异
- Windows Forms开发桌面应用简单直接
- C#语言特性丰富，开发效率高

### 2. UI框架

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| Windows Forms | 内置 | 成熟稳定，学习成本低 |

**选择理由**：
- Windows Forms开发桌面应用最简单直接
- 无需学习复杂的XAML或MVVM模式
- 拖拽式界面设计，开发效率高

### 3. 数据存储

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| JSON | 标准 | 轻量级，易于读写 |
| System.Text.Json | 内置 | 高性能JSON处理 |
| 文件系统 | 系统 | 无需数据库，简单直接 |

**选择理由**：
- 避免复杂的数据库操作
- JSON格式易于理解和调试
- 文件存储便于备份和迁移

### 4. 输入处理

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| Windows API | 系统 | 底层控制，精确输入 |
| SendKeys | 内置 | 简单按键发送 |
| Low-Level Hooks | Windows API | 全局键盘鼠标监听 |

**选择理由**：
- Windows API提供最精确的输入控制
- Low-Level Hooks确保全局监听
- SendKeys简化按键发送操作

### 5. 日志系统

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| 文本日志 | 自定义 | 简单直接，易于查看 |

**选择理由**：
- 简单的文本日志足够使用
- 便于调试和问题排查
- 可按日期分割日志文件

### 6. 图像识别（可选）

| 技术 | 版本 | 选择理由 |
|------|------|----------|
| System.Drawing | 内置 | 基础图像处理 |
| OpenCVSharp | 4.8.0 | 计算机视觉功能（可选） |

**选择理由**：
- System.Drawing满足基础图像处理需求
- OpenCVSharp可根据需要选择性使用
- 避免过度复杂的图像处理

## 第三方库选择

### 必需库

| 库名 | 版本 | 用途 | 理由 |
|------|------|------|------|
| Newtonsoft.Json | 13.0.3 | JSON序列化 | 成熟稳定，功能丰富 |

### 可选库

| 库名 | 版本 | 用途 | 理由 |
|------|------|------|------|
| OpenCvSharp | 4.8.0 | 图像识别 | 计算机视觉功能（可选） |

## 开发工具

| 工具 | 版本 | 用途 |
|------|------|------|
| Visual Studio 2022 | 17.8+ | 开发环境 |
| Git | 2.40+ | 版本控制 |
| NuGet | 6.0+ | 包管理 |

## 项目结构

```
KeyForge/
├── KeyForge.App/                # 主应用程序
│   ├── KeyForge.App.csproj      # 项目文件
│   ├── Program.cs               # 程序入口
│   ├── Models/                  # 数据模型
│   │   ├── Script.cs            # 脚本模型
│   │   ├── KeyAction.cs         # 动作模型
│   │   └── Config.cs            # 配置模型
│   ├── Services/                # 业务服务
│   │   ├── ScriptManager.cs     # 脚本管理
│   │   ├── ScriptRecorder.cs    # 脚本录制
│   │   ├── ScriptPlayer.cs      # 脚本播放
│   │   └── HotkeyManager.cs     # 热键管理
│   ├── Utilities/               # 工具类
│   │   ├── InputHelper.cs       # 输入助手
│   │   ├── FileStorage.cs       # 文件存储
│   │   └── Logger.cs            # 日志工具
│   └── Forms/                   # 窗体
│       ├── MainForm.cs          # 主窗体
│       └── ScriptEditorForm.cs   # 脚本编辑器
├── KeyForge.Tests/              # 测试项目
│   ├── KeyForge.Tests.csproj    # 测试项目文件
│   ├── Services/                # 服务测试
│   └── Utilities/               # 工具测试
└── README.md                    # 项目说明
```

## 配置文件

### appsettings.json
```json
{
  "Settings": {
    "DefaultDelay": 100,
    "EnableLogging": true,
    "ScriptsDirectory": "scripts",
    "RecordHotkey": "F6",
    "PlayHotkey": "F7",
    "StopHotkey": "F8"
  }
}
```

### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>12.0</LangVersion>
  </PropertyGroup>
</Project>
```

## 核心代码示例

### 1. 主程序入口

```csharp
// Program.cs - 简化实现
using KeyForge.App.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace KeyForge.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 简化实现：直接创建MainForm
            Application.Run(new MainForm());
        }
    }
}
```

### 2. 主窗体

```csharp
// MainForm.cs - 简化实现
namespace KeyForge.App.Forms
{
    public partial class MainForm : Form
    {
        private readonly ScriptManager _scriptManager;
        private readonly ScriptRecorder _scriptRecorder;
        private readonly ScriptPlayer _scriptPlayer;
        private readonly HotkeyManager _hotkeyManager;

        public MainForm()
        {
            // 简化实现：直接创建依赖
            var logger = new Logger();
            var fileStorage = new FileStorage(logger);
            var inputHelper = new InputHelper(logger);

            _scriptManager = new ScriptManager(fileStorage, logger);
            _scriptRecorder = new ScriptRecorder(inputHelper, logger);
            _scriptPlayer = new ScriptPlayer(inputHelper, logger);
            _hotkeyManager = new HotkeyManager(inputHelper, logger);

            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 创建简单的UI界面
            this.Text = "KeyForge - 按键脚本工具";
            this.Size = new Size(800, 600);
            
            // 添加控件...
        }
    }
}
```

### 3. 数据模型

```csharp
// Models/Script.cs - 简化实现
namespace KeyForge.App.Models
{
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
}
```

## 部署配置

### 发布配置
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <PublishTrimmed>true</PublishTrimmed>
</PropertyGroup>
```

### 安装程序
- 使用Inno Setup制作安装程序
- 包含必要的运行时组件
- 创建桌面快捷方式

## 性能优化

### 1. 编译优化
```xml
<PropertyGroup>
  <Optimize>true</Optimize>
  <DebugType>pdbonly</DebugType>
  <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

### 2. 运行优化
- 使用异步处理避免阻塞UI
- 及时释放资源
- 缓存常用数据

### 3. 内存优化
- 及时释放图像资源
- 限制缓存大小
- 使用值类型减少堆分配

## 安全考虑

### 1. 输入验证
- 验证脚本名称合法性
- 检查按键参数有效性
- 防止恶意操作

### 2. 权限管理
- 检查管理员权限
- 限制系统API访问
- 安全的文件操作

### 3. 错误处理
- 全局异常捕获
- 友好的错误提示
- 详细的错误日志

## 迁移策略

### 从原有架构迁移
1. **保留核心功能**：录制、播放、脚本管理
2. **简化数据模型**：移除DDD复杂概念
3. **直接依赖**：减少接口抽象
4. **渐进式迁移**：逐步替换组件

### 数据迁移
1. **兼容性检查**：确保数据格式兼容
2. **转换工具**：编写数据转换脚本
3. **测试验证**：验证迁移结果

## 总结

这个技术栈选择遵循了以下原则：

1. **简单实用**：选择成熟稳定的技术
2. **易于维护**：减少复杂依赖
3. **性能优先**：保证实时性和准确性
4. **开发效率**：提高开发速度

主要优势：
- 学习成本低
- 开发效率高
- 维护简单
- 性能优异
- 扩展性好

### 与原技术栈对比

| 方面 | 原技术栈 | 简化技术栈 | 优势 |
|------|----------|------------|------|
| 架构复杂度 | Clean Architecture + DDD | 3层架构 | 减少70%复杂度 |
| UI框架 | WPF | Windows Forms | 学习成本降低80% |
| 数据存储 | SQLite + Dapper | JSON文件 | 部署简化90% |
| 依赖管理 | 复杂的DI容器 | 直接创建 | 依赖减少85% |
| 图像处理 | OpenCVSharp + Tesseract | System.Drawing | 复杂度降低75% |

这种简化技术栈非常适合KeyForge这样的桌面自动化工具，能够快速实现核心功能，同时保持代码的可维护性和可扩展性。