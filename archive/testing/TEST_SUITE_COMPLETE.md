# KeyForge 完整测试套件

## 概述

这是一个为KeyForge项目生成的完整测试套件，包含了单元测试、集成测试、端到端测试和性能测试。

## 修复的编译问题

### 主要修复内容

1. **类型冲突解决**
   - 删除了重复的`TemplateType`枚举定义
   - 删除了重复的`StateMachineStatus`枚举定义
   - 统一使用`KeyForge.Domain.Common`中的枚举定义

2. **引用修复**
   - 添加了缺失的`using KeyForge.Domain.Aggregates;`语句
   - 修复了`TemplateType`和`StateMachineStatus`的引用问题
   - 将`StateMachineStatus.Inactive`改为`StateMachineStatus.Paused`

3. **Domain项目构建成功**
   - 所有编译错误已修复
   - 只剩下一些可接受的警告（Nullable引用类型相关）

## 测试项目结构

```
KeyForge.Tests/
├── KeyForge.Tests.csproj              # 测试项目配置
├── Common/
│   ├── TestBase.cs                     # 测试基类
│   └── TestDataFactory.cs              # 测试数据工厂
├── UnitTests/
│   ├── Domain/
│   │   ├── ScriptTests.cs              # 脚本聚合根单元测试
│   │   ├── StateMachineTests.cs        # 状态机聚合根单元测试
│   │   └── ImageTemplateTests.cs      # 图像模板聚合根单元测试
│   ├── Application/
│   │   ├── ScriptServiceTests.cs       # 脚本服务单元测试
│   │   └── TemplateServiceTests.cs     # 模板服务单元测试
│   └── Core/
│       ├── ScriptPlayerTests.cs        # 脚本播放器单元测试
│       └── InputSimulatorTests.cs      # 输入模拟器单元测试
├── IntegrationTests/
│   ├── Application/
│   │   └── ScriptServiceIntegrationTests.cs  # 脚本服务集成测试
│   └── Core/
│       └── ScriptPlayerIntegrationTests.cs    # 脚本播放器集成测试
├── EndToEndTests/
│   └── ScriptLifecycleEndToEndTests.cs       # 脚本生命周期端到端测试
└── PerformanceTests/
    ├── ScriptServicePerformanceTests.cs       # 脚本服务性能测试
    └── ScriptPlayerPerformanceTests.cs        # 脚本播放器性能测试
```

## 测试分类

### 1. 单元测试 (Unit Tests)
- **测试范围**: 单个类或方法
- **执行速度**: 快速 (< 1秒)
- **依赖**: 无外部依赖，使用Mock对象
- **覆盖率**: 核心业务逻辑

#### Domain层单元测试
- `ScriptTests.cs`: 测试脚本聚合根的所有业务规则
- `StateMachineTests.cs`: 测试状态机聚合根的状态转换
- `ImageTemplateTests.cs`: 测试图像模板聚合根的匹配逻辑

#### Application层单元测试  
- `ScriptServiceTests.cs`: 测试脚本应用服务
- `TemplateServiceTests.cs`: 测试模板应用服务

#### Core层单元测试
- `ScriptPlayerTests.cs`: 测试脚本播放器核心逻辑
- `InputSimulatorTests.cs`: 测试输入模拟器功能

### 2. 集成测试 (Integration Tests)
- **测试范围**: 多个组件的交互
- **执行速度**: 中等 (1-5秒)
- **依赖**: 需要数据库或外部服务
- **覆盖率**: 组件间接口

#### 应用服务集成测试
- `ScriptServiceIntegrationTests.cs`: 测试脚本服务与仓储的集成
- `ScriptPlayerIntegrationTests.cs`: 测试脚本播放器与输入模拟器的集成

### 3. 端到端测试 (End-to-End Tests)
- **测试范围**: 完整业务流程
- **执行速度**: 较慢 (5-30秒)
- **依赖**: 需要完整环境
- **覆盖率**: 关键用户场景

#### 业务流程测试
- `ScriptLifecycleEndToEndTests.cs`: 测试脚本完整的生命周期

### 4. 性能测试 (Performance Tests)
- **测试范围**: 性能和负载
- **执行速度**: 很慢 (30秒-几分钟)
- **依赖**: 需要性能测试环境
- **覆盖率**: 性能瓶颈

#### 性能基准测试
- `ScriptServicePerformanceTests.cs`: 脚本服务性能测试
- `ScriptPlayerPerformanceTests.cs`: 脚本播放器性能测试

## 测试工具和框架

### 核心框架
- **xUnit**: 单元测试框架
- **Moq**: Mock对象框架
- **FluentAssertions**: 断言库
- **Bogus**: 测试数据生成

### 覆盖率工具
- **coverlet**: 代码覆盖率工具
- **ReportGenerator**: 覆盖率报告生成

### 性能测试
- **BenchmarkDotNet**: 性能基准测试
- **xUnit Performance**: 性能测试扩展

## 测试数据管理

### TestDataFactory
使用Bogus框架生成真实的测试数据：

```csharp
// 生成有效脚本
public static Script CreateValidScript()
{
    var scriptId = Guid.NewGuid();
    var script = new Script(scriptId, 
        Faker.Lorem.Sentence(3), 
        Faker.Lorem.Paragraph());
    
    // 添加随机动作
    var actions = CreateGameActions(5);
    foreach (var action in actions)
    {
        script.AddAction(action);
    }
    
    return script;
}
```

### 测试基类
提供通用的测试设置和断言方法：

```csharp
public abstract class TestBase
{
    protected static void ShouldThrowValidationException(Action action, string expectedMessage)
    {
        var exception = Assert.Throws<ValidationException>(action);
        exception.Message.Should().Be(expectedMessage);
    }
    
    protected static void ShouldBeValidScript(Script script)
    {
        script.Should().NotBeNull();
        script.Id.Should().NotBeEmpty();
        script.Name.Should().NotBeNullOrEmpty();
        // ... 更多验证
    }
}
```

## 测试执行

### 命令行执行
```bash
# 运行所有测试
dotnet test

# 运行特定测试类别
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=EndToEnd"
dotnet test --filter "Category=Performance"

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

### 使用脚本执行
```bash
# 运行完整测试套件
./run-tests.sh

# 运行单元测试并生成覆盖率
./run-tests.sh -u -c

# 运行集成测试并生成HTML报告
./run-tests.sh -i -r

# 运行性能测试
./run-tests.sh -p -v
```

## 测试报告

### 覆盖率报告
- **XML格式**: CI/CD集成使用
- **HTML格式**: 人工查看使用
- **阈值要求**: 80%以上

### 测试结果报告
- **控制台输出**: 实时测试结果
- **HTML报告**: 详细测试报告
- **JUnit XML**: CI/CD集成

### 性能报告
- **基准测试结果**: 性能指标
- **内存使用**: 内存泄漏检测
- **执行时间**: 性能回归检测

## 质量门禁

### 代码覆盖率
- **最低要求**: 80%
- **理想目标**: 90%+
- **例外情况**: 需要团队审批

### 测试通过率
- **要求**: 100%
- **重试机制**: 3次重试
- **失败处理**: 阻止合并

### 性能基准
- **响应时间**: < 2秒
- **内存使用**: < 100MB
- **CPU使用**: < 50%

## 持续集成

### GitHub Actions
- **自动触发**: Push和Pull Request
- **并行执行**: 多个测试类别并行
- **报告发布**: 自动发布测试报告
- **质量门禁**: 自动检查质量标准

### 测试环境
- **Linux容器**: 确保环境一致性
- **数据库**: 内存数据库用于测试
- **依赖隔离**: 每个测试独立环境

## 维护指南

### 添加新测试
1. 确定测试类别（单元/集成/端到端）
2. 选择合适的测试基类
3. 使用TestDataFactory生成测试数据
4. 遵循AAA模式（Arrange-Act-Assert）
5. 添加适当的测试分类

### 测试重构
1. 保持测试行为不变
2. 更新测试数据生成逻辑
3. 优化测试执行效率
4. 改进断言清晰度

### 性能优化
1. 识别慢速测试
2. 减少外部依赖
3. 使用并行测试执行
4. 优化测试数据生成

## 故障排除

### 常见问题
1. **编译错误**: 检查引用和命名空间
2. **测试失败**: 查看详细错误信息
3. **超时问题**: 增加测试超时时间
4. **环境问题**: 确保测试环境配置正确

### 调试技巧
1. 使用调试模式运行测试
2. 添加详细的日志输出
3. 使用断点调试测试代码
4. 检查Mock对象配置

## 最佳实践

### 测试命名
- 使用描述性的测试名称
- 遵循"Scenario_ExpectedBehavior"模式
- 包含测试的业务上下文

### 测试组织
- 按功能模块组织测试
- 使用测试基类减少重复代码
- 合理使用测试分类

### 断言使用
- 使用FluentAssertions提高可读性
- 一个测试方法一个主要断言
- 包含有意义的错误消息

## 总结

KeyForge测试套件提供了一个完整的测试解决方案，确保代码质量和系统稳定性。通过分层测试策略、自动化测试执行和严格的质量门禁，可以有效地维护和扩展KeyForge项目。

### 关键特性
- ✅ 完整的测试覆盖（单元、集成、端到端、性能）
- ✅ 自动化测试执行和报告
- ✅ 代码覆盖率监控
- ✅ 性能基准测试
- ✅ 持续集成支持
- ✅ 详细的质量门禁

### 使用建议
1. 定期运行完整测试套件
2. 监控代码覆盖率趋势
3. 关注性能测试结果
4. 及时修复失败的测试
5. 持续优化测试效率