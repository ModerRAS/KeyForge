# KeyForge 性能测试套件

## 概述

KeyForge性能测试套件提供了全面的性能测试解决方案，包括基准测试、负载测试、内存分析和压力测试。这些测试帮助确保系统在各种负载条件下的性能表现。

## 测试类型

### 1. 基准测试 (Benchmark Tests)
- **目的**: 测量核心操作的性能指标
- **工具**: BenchmarkDotNet
- **测试内容**:
  - 脚本创建和操作
  - 状态机操作
  - 仓储操作
  - 值对象创建

### 2. 负载测试 (Load Tests)
- **目的**: 测试系统在正常负载下的表现
- **工具**: 自定义负载测试框架
- **测试内容**:
  - 并发用户操作
  - 脚本创建负载
  - 脚本执行负载
  - 查询操作负载

### 3. 内存分析测试 (Memory Analysis Tests)
- **目的**: 检测内存泄漏和内存使用效率
- **工具**: GC分析器
- **测试内容**:
  - 内存使用量分析
  - 内存清理效率
  - 长时间运行稳定性
  - 并发操作内存管理

### 4. 压力测试 (Stress Tests)
- **目的**: 测试系统在极限条件下的表现
- **工具**: 自定义压力测试框架
- **测试内容**:
  - 高频操作
  - 大规模数据处理
  - 极限并发
  - 系统稳定性

## 快速开始

### 前置条件

1. **.NET 8.0 SDK**
2. **足够的系统资源** (推荐4GB+ RAM)
3. **Visual Studio 2022** 或 **.NET CLI**

### 运行测试

#### 使用命令行

```bash
# 进入测试项目目录
cd KeyForge.Tests/Performance

# 运行快速性能检查
dotnet run -- quick

# 运行基准测试
dotnet run -- benchmark

# 运行负载测试
dotnet run -- load

# 运行内存分析测试
dotnet run -- memory

# 运行压力测试
dotnet run -- stress

# 运行所有测试
dotnet run -- all

# 生成报告
dotnet run -- report
```

#### 使用Visual Studio

1. 打开解决方案
2. 设置`KeyForge.Tests.Performance`为启动项目
3. 按F5运行程序
4. 按照提示选择测试类型

## 配置

### 性能测试配置

编辑`PerformanceSettings.json`文件来自定义测试参数：

```json
{
  "PerformanceSettings": {
    "BenchmarkSettings": {
      "WarmupIterations": 3,
      "TargetIterations": 10,
      "MinIterationTime": "00:00:01"
    },
    "LoadTestSettings": {
      "DefaultDurationSeconds": 30,
      "DefaultConcurrentUsers": 50,
      "SuccessRateThreshold": 95.0
    }
  }
}
```

### 测试配置文件

- `PerformanceSettings.json` - 性能测试配置
- `appsettings.json` - 应用配置
- `BenchmarkDotNet.Artifacts/` - 基准测试结果

## 测试结果

### 基准测试结果

基准测试结果保存在`BenchmarkDotNet.Artifacts/`目录中：
- `report.html` - HTML格式报告
- `report.csv` - CSV格式数据
- `results.md` - Markdown格式摘要

### 负载测试结果

负载测试结果包括：
- 成功率
- 平均响应时间
- 吞吐量 (每秒操作数)
- 错误分析

### 内存分析结果

内存分析结果包括：
- 内存使用量
- 内存清理效率
- 内存泄漏检测
- 长时间运行稳定性

### 压力测试结果

压力测试结果包括：
- 极限性能指标
- 系统稳定性
- 错误率分析
- 资源使用情况

## 性能指标

### 预期性能指标

| 操作类型 | 预期响应时间 | 预期吞吐量 | 成功率要求 |
|---------|-------------|-----------|-----------|
| 脚本创建 | < 100ms | > 100/秒 | > 95% |
| 脚本执行 | < 200ms | > 50/秒 | > 98% |
| 状态转换 | < 10ms | > 1000/秒 | > 99% |
| 查询操作 | < 50ms | > 200/秒 | > 99% |

### 内存使用指标

| 操作类型 | 内存使用 | 清理效率 | 增长限制 |
|---------|---------|----------|----------|
| 脚本创建 | < 50MB/1000个 | > 85% | < 10MB |
| 状态机创建 | < 20MB/100个 | > 90% | < 5MB |
| 长时间运行 | < 100MB | > 80% | < 50MB |

## 故障排除

### 常见问题

1. **内存不足**
   - 增加系统内存
   - 减少并发用户数
   - 优化测试配置

2. **性能不达标**
   - 检查系统资源
   - 优化测试环境
   - 分析性能瓶颈

3. **测试失败**
   - 查看错误日志
   - 检查依赖项
   - 验证测试数据

### 日志文件

- `performance-test.log` - 详细测试日志
- `error.log` - 错误日志
- `performance-report.md` - 性能报告

## 扩展测试

### 添加新的基准测试

```csharp
[MemoryDiagnoser]
public class NewBenchmarkTests
{
    [Benchmark]
    public void NewOperation()
    {
        // 实现测试逻辑
    }
}
```

### 添加新的负载测试

```csharp
public static async Task RunNewLoadTest()
{
    var loadTest = new LoadTestRunner(
        concurrentUsers: 100,
        testDurationSeconds: 60,
        testAction: context =>
        {
            // 实现测试逻辑
        }
    );

    var summary = await loadTest.RunLoadTestAsync();
    summary.PrintSummary();
}
```

### 添加新的内存测试

```csharp
[Fact]
public void NewMemoryTest_ShouldBeEfficient()
{
    var initialMemory = GC.GetTotalMemory(true);
    
    // 执行测试操作
    
    var finalMemory = GC.GetTotalMemory(true);
    var memoryIncrease = finalMemory - initialMemory;
    
    Assert.True(memoryIncrease < Threshold, "内存使用过高");
}
```

## 最佳实践

### 测试执行

1. **环境准备**
   - 关闭不必要的应用程序
   - 确保系统资源充足
   - 使用一致的测试环境

2. **测试执行**
   - 多次运行测试取平均值
   - 记录测试环境信息
   - 监控系统资源使用

3. **结果分析**
   - 对比历史数据
   - 分析性能瓶颈
   - 制定优化方案

### 性能优化

1. **代码优化**
   - 优化算法复杂度
   - 减少内存分配
   - 使用缓存策略

2. **架构优化**
   - 异步处理
   - 负载均衡
   - 资源池化

3. **配置优化**
   - 调整连接池大小
   - 优化缓存配置
   - 调整并发限制

## 报告生成

### 自动报告

测试完成后会自动生成以下报告：
- `performance-report.md` - Markdown格式报告
- `performance-report.html` - HTML格式报告
- `performance-data.json` - JSON格式数据

### 自定义报告

可以扩展`PerformanceTestRunner`类来自定义报告格式和内容。

## 贡献指南

### 添加新测试

1. 在相应的测试类中添加测试方法
2. 更新配置文件
3. 更新文档
4. 运行测试验证

### 修复问题

1. 创建问题分支
2. 修复问题
3. 添加测试用例
4. 提交合并请求

## 许可证

本测试套件遵循与主项目相同的许可证。

## 联系信息

如有问题或建议，请通过以下方式联系：
- 创建GitHub Issue
- 发送邮件到开发团队
- 在项目讨论区提问

---

*最后更新: 2024-01-01*