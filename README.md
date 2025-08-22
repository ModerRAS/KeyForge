# KeyForge - 智能按键脚本系统

[![Build Status](https://github.com/ModerRAS/KeyForge/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/build-and-test.yml)
[![UAT Testing](https://github.com/ModerRAS/KeyForge/actions/workflows/uat-testing.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/uat-testing.yml)
[![Code Quality](https://github.com/ModerRAS/KeyForge/actions/workflows/code-quality.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/code-quality.yml)
[![Release](https://github.com/ModerRAS/KeyForge/actions/workflows/release.yml/badge.svg)](https://github.com/ModerRAS/KeyForge/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 项目概述

KeyForge 是一个基于 C# .NET 9 开发的智能化游戏自动化按键脚本系统，采用领域驱动设计（DDD）架构，具备屏幕识别、智能决策、精确执行和完整监控能力。

### 系统架构

本项目采用清洁架构（Clean Architecture）和 Sense-Judge-Act 设计模式：

```
┌─────────────────────────────────────────┐
│              KeyForge.UI                │
│         (Windows Forms 界面)           │
└─────────────────────────────────────────┘
                     │
┌─────────────────────────────────────────┐
│           KeyForge.Core                │
│        (核心业务逻辑层)                 │
│  ┌─────────┬─────────┬─────────┐        │
│  │ Sense   │  Judge  │   Act   │        │
│  │(感知层) │(决策层) │(执行层) │        │
│  └─────────┴─────────┴─────────┘        │
└─────────────────────────────────────────┘
                     │
┌─────────────────────────────────────────┐
│        KeyForge.Infrastructure          │
│        (基础设施层)                     │
│  ┌─────────┬─────────┬─────────┐        │
│  │  Data   │ Logging │ Imaging │        │
│  │ (存储)   │(日志)   │(图像)   │        │
│  └─────────┴─────────┴─────────┘        │
└─────────────────────────────────────────┘
```

### 核心功能

#### 1. Sense（感知层）
- **屏幕捕获**：实时捕获屏幕指定区域
- **图像识别**：支持模板匹配和特征点匹配
- **窗口检测**：获取系统窗口信息和状态
- **变化监控**：持续监控屏幕内容变化

#### 2. Judge（决策层）
- **规则引擎**：基于条件判断的决策系统
- **状态管理**：维护复杂的执行状态
- **策略选择**：根据上下文选择最优策略
- **错误处理**：智能的错误恢复机制

#### 3. Act（执行层）
- **输入模拟**：精确的键盘鼠标操作
- **时序控制**：高精度的时间控制
- **并发执行**：支持多任务并行处理
- **结果验证**：执行结果的验证和反馈

### 技术特点

- 🎯 **高精度识别**：基于图像识别和UI元素定位
- 🧠 **智能化决策**：支持复杂条件判断和策略选择
- ⚡ **稳定可靠执行**：具备错误恢复和重试机制
- 📊 **全方位监控**：完整的日志记录和性能监控
- 🔧 **模块化设计**：易于扩展和维护的架构

### 开发环境

- **框架**：.NET 9.0
- **UI框架**：Windows Forms
- **架构模式**：领域驱动设计（DDD）
- **设计模式**：工厂模式、策略模式、观察者模式等
- **数据存储**：JSON文件（可扩展至SQLite）
- **图像处理**：简化实现（可扩展至OpenCV）

### 项目结构

```
KeyForge/
├── KeyForge.Core/           # 核心业务逻辑
│   ├── Domain/              # 领域模型
│   │   ├── Common/         # 通用类型和接口
│   │   ├── Automation/     # 自动化相关
│   │   ├── Vision/         # 图像识别
│   │   ├── Sense/          # 感知层
│   │   ├── Judge/          # 决策层
│   │   └── Act/            # 执行层
│   ├── Interfaces/         # 服务接口
│   └── Services/           # 应用服务
├── KeyForge.Infrastructure/ # 基础设施层
│   ├── Persistence/        # 数据持久化
│   ├── Logging/           # 日志服务
│   └── Imaging/           # 图像处理
├── KeyForge.UI/            # 用户界面
│   ├── Forms/             # 窗体
│   └── Controls/          # 控件
└── KeyForge.Tests/         # 单元测试
```

### 快速开始

1. **编译项目**
   ```bash
   dotnet build
   ```

2. **运行应用**
   ```bash
   dotnet run --project KeyForge.UI
   ```

### 使用说明

#### 基本操作

1. **录制脚本**
   - 点击"开始录制"按钮
   - 执行键盘鼠标操作
   - 点击"停止录制"保存脚本

2. **编辑脚本**
   - 选择要编辑的脚本
   - 点击"编辑"按钮
   - 修改脚本信息和动作序列

3. **播放脚本**
   - 选择要播放的脚本
   - 设置播放参数（重复次数等）
   - 点击"播放"按钮执行

#### 快捷键

- `F6`：开始/停止录制
- `F7`：播放脚本
- `F8`：停止播放
- `F9`：暂停/恢复

### 配置说明

系统配置文件 `appsettings.json` 包含以下主要配置：

- **ScreenCapture**：屏幕捕获设置
- **ImageRecognition**：图像识别参数
- **ScriptExecution**：脚本执行配置
- **InputSimulation**：输入模拟设置
- **Security**：安全相关配置
- **Performance**：性能监控设置

### 扩展开发

#### 添加新的识别算法

1. 实现 `IRecognitionAlgorithm` 接口
2. 在 `ImageRecognitionEngine` 中注册新算法
3. 更新配置文件以支持新参数

#### 添加新的输入设备

1. 扩展 `IGameInputHandler` 接口
2. 实现具体的设备处理器
3. 在输入工厂中注册新设备

#### 自定义决策逻辑

1. 实现 `IRuleEngine` 接口
2. 创建自定义规则和条件
3. 在决策服务中集成新逻辑

### 性能优化

- **图像识别**：使用缓存和预处理技术
- **脚本执行**：异步执行和并发处理
- **内存管理**：及时释放资源和垃圾回收
- **日志记录**：分级日志和异步写入

### 安全考虑

- **输入验证**：所有外部输入都经过验证
- **权限控制**：操作权限和访问控制
- **资源限制**：防止资源滥用和内存泄漏
- **日志审计**：完整的操作日志记录

### 故障排除

#### 常见问题

1. **录制无效**
   - 检查管理员权限
   - 确认目标应用程序已启动

2. **识别失败**
   - 调整识别阈值
   - 检查图像模板质量

3. **执行错误**
   - 查看错误日志
   - 验证脚本语法

#### 调试模式

启用详细日志记录：
```json
{
  "ScriptExecution": {
    "LogLevel": "Debug"
  }
}
```

### 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 发起 Pull Request

### 许可证

本项目采用 MIT 许可证。

---

**注意**：本软件仅供学习和研究使用，请遵守相关软件的使用条款和法律法规。