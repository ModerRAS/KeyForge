# KeyForge 集成测试

本目录包含KeyForge项目的完整集成测试套件，测试各个组件之间的交互和整个系统的工作流程。

## 测试结构

### 核心测试类

1. **IntegrationTestBase.cs** - 集成测试基类
   - 提供测试基础设施
   - 包含通用的测试辅助方法
   - 处理测试数据清理

2. **ServiceIntegrationTests.cs** - 服务集成测试
   - 测试KeyInputService与WindowsKeyHook的集成
   - 验证按键录制和模拟功能
   - 测试事件处理和线程安全

3. **DataIntegrationTests.cs** - 数据集成测试
   - 测试脚本保存和加载的完整流程
   - 验证数据一致性和完整性
   - 测试文件操作和错误处理

4. **LayerInteractionTests.cs** - 层间交互测试
   - 测试UI层与服务层的交互
   - 验证控制器、服务和视图之间的协作
   - 测试完整的事件处理流程

5. **EndToEndWorkflowTests.cs** - 端到端工作流测试
   - 测试录制→保存→加载→播放的完整流程
   - 验证复杂场景和错误恢复
   - 测试性能和并发处理

### 支持类

6. **IntegrationTestSupport.cs** - 测试支持类
   - 提供测试配置
   - 包含测试数据工厂
   - 提供断言助手和工具类

### 模拟类

7. **IUIInterfaces.cs** - UI接口定义
   - 定义UI控制器、视图和应用服务接口
   - 提供统一的交互契约

8. **ApplicationService.cs** - 应用服务实现
   - 实现业务逻辑层
   - 协调各个服务组件

9. **UIController.cs** - UI控制器实现
   - 处理用户交互
   - 协调UI和业务逻辑

10. **MockUIView.cs** - 模拟UI视图
    - 用于测试的视图实现
    - 记录UI状态变化

## 测试特性

### 覆盖的功能

- **脚本管理**: 创建、更新、删除、查询脚本
- **录制功能**: 按键录制、事件处理、状态管理
- **播放功能**: 脚本播放、速度控制、重复播放
- **文件操作**: 保存、加载、导入、导出脚本
- **数据序列化**: JSON序列化和反序列化
- **错误处理**: 异常处理和错误恢复
- **性能测试**: 大数据集处理和性能验证
- **并发测试**: 多线程操作和并发安全
- **数据完整性**: 多次保存加载的数据一致性

### 测试方法

- **BDD风格**: 使用Given-When-Then的测试结构
- **断言验证**: 使用FluentAssertions进行清晰的断言
- **Mock对象**: 使用Moq进行依赖注入和隔离测试
- **异步测试**: 完整的异步操作测试
- **资源管理**: 自动清理测试资源

### 测试数据

- **动态数据**: 使用工厂方法创建测试数据
- **边界情况**: 测试空值、无效值和边界条件
- **大数据集**: 测试性能和可扩展性
- **并发数据**: 测试线程安全和数据一致性

## 运行测试

### 前置条件

- .NET 8.0 或更高版本
- xUnit 测试框架
- FluentAssertions 断言库
- Moq Mock框架

### 运行命令

```bash
# 运行所有集成测试
dotnet test --filter "IntegrationTests"

# 运行特定测试类
dotnet test --filter "ServiceIntegrationTests"
dotnet test --filter "DataIntegrationTests"
dotnet test --filter "LayerInteractionTests"
dotnet test --filter "EndToEndWorkflowTests"

# 运行特定测试方法
dotnet test --filter "CompleteWorkflow_RecordingToPlayback_ShouldWorkSeamlessly"
```

### 测试输出

测试运行时会输出详细的日志信息：
- 测试步骤执行情况
- 性能计时结果
- 错误和异常信息
- 文件操作状态

## 测试场景

### 基础功能测试

1. **脚本创建和管理**
   - 创建新脚本
   - 更新脚本信息
   - 删除脚本
   - 查询脚本列表

2. **录制功能测试**
   - 开始/停止录制
   - 按键事件捕获
   - 动作序列记录
   - 录制状态管理

3. **播放功能测试**
   - 脚本播放控制
   - 速度调整
   - 重复播放
   - 播放状态管理

### 高级功能测试

4. **文件操作测试**
   - 脚本保存/加载
   - 文件格式验证
   - 错误处理
   - 路径处理

5. **数据一致性测试**
   - 序列化/反序列化
   - 数据完整性验证
   - 多次保存加载
   - 数据转换

6. **性能测试**
   - 大数据集处理
   - 响应时间验证
   - 内存使用监控
   - 并发性能

### 复杂场景测试

7. **错误恢复测试**
   - 异常处理
   - 错误恢复
   - 状态一致性
   - 优雅降级

8. **并发测试**
   - 多线程操作
   - 数据竞争
   - 死锁预防
   - 线程安全

9. **端到端测试**
   - 完整工作流程
   - 跨组件交互
   - 集成验证
   - 系统稳定性

## 测试最佳实践

### 编写测试

1. **清晰的测试名称**: 使用描述性的测试方法名
2. **AAA模式**: Arrange-Act-Assert结构
3. **单一职责**: 每个测试只验证一个功能点
4. **独立性**: 测试之间相互独立
5. **可重复性**: 测试结果应该稳定可靠

### 使用断言

```csharp
// 好的断言示例
result.Should().BeTrue();
script.Actions.Should().HaveCount(expectedCount);
action.Type.Should().Be(ActionType.KeyDown);
```

### 错误处理

```csharp
// 验证异常
Action action = () => service.DoSomething();
action.Should().Throw<InvalidOperationException>();
```

### 异步测试

```csharp
// 异步测试示例
[Fact]
public async Task AsyncOperation_ShouldCompleteSuccessfully()
{
    // Arrange
    var service = new TestService();
    
    // Act
    var result = await service.DoAsyncOperation();
    
    // Assert
    result.Should().BeTrue();
}
```

## 故障排除

### 常见问题

1. **测试超时**: 增加测试超时时间或优化性能
2. **文件权限**: 确保测试目录有读写权限
3. **依赖问题**: 检查所有依赖项是否正确安装
4. **并发问题**: 确保测试线程安全

### 调试技巧

1. **详细日志**: 启用详细日志输出
2. **断点调试**: 在IDE中设置断点
3. **隔离测试**: 单独运行失败的测试
4. **数据检查**: 验证测试数据的正确性

## 扩展测试

### 添加新测试

1. 在相应的测试类中添加新方法
2. 遵循现有的测试模式
3. 添加适当的断言
4. 确保测试独立性

### 添加新功能测试

1. 创建新的测试类（如果需要）
2. 实现完整的测试覆盖
3. 包含正常和异常情况
4. 添加性能和并发测试

## 维护

### 定期维护

1. 更新测试数据
2. 优化测试性能
3. 修复失败的测试
4. 添加新的测试场景

### 文档更新

1. 更新测试说明
2. 添加新功能文档
3. 记录已知问题
4. 更新运行说明

---

这些集成测试确保KeyForge项目的各个组件能够正确协作，提供稳定可靠的功能。