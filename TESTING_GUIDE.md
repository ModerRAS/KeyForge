# KeyForge 测试执行指南

## 目录

1. [快速开始](#快速开始)
2. [环境配置](#环境配置)
3. [测试执行](#测试执行)
4. [结果分析](#结果分析)
5. [故障排除](#故障排除)
6. [最佳实践](#最佳实践)

## 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/your-org/KeyForge.git
cd KeyForge
```

### 2. 安装依赖

```bash
# 恢复NuGet包
dotnet restore

# 安装全局工具
dotnet tool install -g dotnet-xunit
dotnet tool install -g reportgenerator
dotnet tool install -g dotnet-sonarscanner
```

### 3. 运行快速测试

```bash
# 运行单元测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj

# 运行快速性能检查
cd KeyForge.Tests/Performance && dotnet run -- quick

# 运行快速安全检查
cd KeyForge.Tests/Security && ./run-security-tests.sh quick
```

## 环境配置

### 系统要求

- **操作系统**: Windows 10+, macOS 10.15+, Ubuntu 18.04+
- **.NET SDK**: 8.0.x
- **内存**: 最少 4GB，推荐 8GB+
- **存储**: 最少 2GB 可用空间

### 开发环境

#### Visual Studio

1. 安装 Visual Studio 2022
2. 安装".NET 桌面开发"工作负载
3. 安装"ASP.NET 和 Web 开发"工作负载
4. 打开解决方案 `KeyForge.sln`
5. 选择"测试" → "测试资源管理器"
6. 运行测试

#### VS Code

1. 安装 VS Code
2. 安装C#扩展
3. 安装.NET Core Tools扩展
4. 打开项目文件夹
5. 使用命令行运行测试

#### 命令行

```bash
# 安装.NET SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin

# 验证安装
dotnet --version
```

### 测试数据库

#### In-Memory Database

```csharp
// 使用内存数据库进行测试
services.AddDbContext<KeyForgeDbContext>(options =>
    options.UseInMemoryDatabase("KeyForgeTestDb"));
```

#### SQL Server LocalDB

```bash
# 安装LocalDB
dotnet tool install -g dotnet-sql-localdb

# 创建数据库
dotnet sql-localdb create KeyForgeTestDb
```

#### PostgreSQL

```bash
# 使用Docker运行PostgreSQL
docker run --name keyforge-test-db \
  -e POSTGRES_PASSWORD=test123 \
  -e POSTGRES_DB=keyforge_test \
  -p 5432:5432 \
  -d postgres:14
```

## 测试执行

### 单元测试

#### 基本执行

```bash
# 运行所有单元测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj

# 运行特定测试类
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --filter "ClassName"

# 运行特定测试方法
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --filter "MethodName"
```

#### 并行执行

```bash
# 启用并行测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --parallel

# 设置最大并行线程数
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --parallel --max-parallel-threads 4
```

#### 代码覆盖率

```bash
# 启用覆盖率收集
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --collect:"XPlat Code Coverage"

# 指定覆盖率格式
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --collect:"XPlat Code Coverage;Format=cobertura"
```

### 集成测试

#### 数据库集成测试

```bash
# 运行集成测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --filter "Integration"

# 使用特定数据库连接字符串
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --test-connection-string "Server=(localdb)\\mssqllocaldb;Database=KeyForgeTest;Trusted_Connection=True;"
```

#### API集成测试

```bash
# 运行API测试
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --filter "Api"

# 使用真实HTTP客户端
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --test-use-real-http-client
```

### 性能测试

#### 基准测试

```bash
# 进入性能测试目录
cd KeyForge.Tests/Performance

# 运行基准测试
dotnet run -- benchmark

# 运行特定基准测试
dotnet run -- benchmark --filter "*CreateScript*"
```

#### 负载测试

```bash
# 运行负载测试
dotnet run -- load

# 指定并发用户数
dotnet run -- load --users 100

# 指定测试持续时间
dotnet run -- load --duration 60
```

#### 压力测试

```bash
# 运行压力测试
dotnet run -- stress

# 运行内存压力测试
dotnet run -- stress --memory
```

### 安全测试

#### 自动化安全测试

```bash
# 进入安全测试目录
cd KeyForge.Tests/Security

# 运行快速安全检查
./run-security-tests.sh quick

# 运行完整安全测试套件
./run-security-tests.sh full

# 运行特定安全测试
./run-security-tests.sh auth
./run-security-tests.sh scan
```

#### 漏洞扫描

```bash
# 运行漏洞扫描
./run-security-tests.sh scan

# 生成安全报告
./run-security-tests.sh report
```

### 端到端测试

#### Playwright E2E测试

```bash
# 安装Playwright
npm install -g @playwright/test
npx playwright install

# 运行E2E测试
npx playwright test e2e/

# 运行特定E2E测试
npx playwright test e2e/login.spec.ts
```

#### Selenium E2E测试

```bash
# 运行Selenium测试
dotnet test KeyForge.Tests.E2E/KeyForge.Tests.E2E.csproj

# 指定浏览器
dotnet test KeyForge.Tests.E2E/KeyForge.Tests.E2E.csproj --test-browser chrome
```

## 结果分析

### 测试结果查看

#### Visual Studio

1. 打开"测试资源管理器"
2. 查看测试结果
3. 点击"输出"查看详细日志

#### 命令行

```bash
# 查看测试结果
dotnet test --logger "console;verbosity=detailed"

# 输出XML报告
dotnet test --logger "xunit;LogFileName=test-results.xml"

# 输出HTML报告
dotnet test --logger "html;LogFileName=test-results.html"
```

#### 覆盖率报告

```bash
# 生成覆盖率报告
reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html

# 查看覆盖率报告
open coverage-report/index.html
```

### 性能测试分析

#### BenchmarkDotNet结果

```bash
# 查看基准测试结果
open BenchmarkDotNet.Artifacts/results.html

# 导出CSV结果
open BenchmarkDotNet.Artifacts/results.csv
```

#### 负载测试分析

```bash
# 查看负载测试结果
open PerformanceReports/load-test-report.html

# 查看性能指标
cat PerformanceReports/load-test-metrics.json
```

### 安全测试分析

#### 安全报告

```bash
# 查看安全测试报告
open SecurityReports/security-test-report.html

# 查看漏洞详情
cat SecurityReports/vulnerability-findings.json
```

#### 安全评分

```bash
# 查看安全评分
cat SecurityReports/security-score.json
```

## 故障排除

### 常见错误

#### 测试失败

**错误**: "测试方法抛出异常"

**解决**:
1. 检查测试代码逻辑
2. 验证测试数据
3. 查看异常堆栈跟踪
4. 使用调试器调试

```csharp
[Fact]
public void ProblematicTest()
{
    try
    {
        // 测试代码
    }
    catch (Exception ex)
    {
        // 输出详细错误信息
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        throw;
    }
}
```

#### 依赖注入错误

**错误**: "无法解析服务"

**解决**:
1. 检查服务注册
2. 验证接口实现
3. 检查生命周期配置

```csharp
[Fact]
public void ServiceResolutionTest()
{
    // 手动配置服务
    var services = new ServiceCollection();
    services.AddScoped<IScriptService, ScriptService>();
    
    var serviceProvider = services.BuildServiceProvider();
    
    // 解析服务
    var service = serviceProvider.GetService<IScriptService>();
    Assert.NotNull(service);
}
```

#### 数据库连接错误

**错误**: "无法连接到数据库"

**解决**:
1. 检查数据库服务状态
2. 验证连接字符串
3. 检查防火墙设置

```bash
# 检查数据库连接
dotnet ef database update --connection "Server=(localdb)\\mssqllocaldb;Database=KeyForgeTest;Trusted_Connection=True;"

# 验证数据库迁移
dotnet ef migrations list
```

#### 性能测试超时

**错误**: "性能测试超时"

**解决**:
1. 增加超时时间
2. 优化测试代码
3. 减少测试数据量

```csharp
[Fact]
public async Task PerformanceTest()
{
    // 设置超时时间
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
    
    // 执行性能测试
    var result = await LongRunningOperationAsync(cts.Token);
    
    Assert.NotNull(result);
}
```

### 调试技巧

#### 使用断点调试

```csharp
[Fact]
public void DebuggableTest()
{
    // 在VS中设置断点
    Debugger.Break();
    
    // 测试代码
    var result = service.Method();
    
    Assert.NotNull(result);
}
```

#### 输出调试信息

```csharp
[Fact]
public void VerboseTest()
{
    // 输出变量值
    var data = GetData();
    Console.WriteLine($"Data: {JsonSerializer.Serialize(data)}");
    
    // 测试逻辑
    var result = service.ProcessData(data);
    
    Assert.NotNull(result);
}
```

#### 条件测试执行

```csharp
[Fact]
public void ConditionalTest()
{
    #if DEBUG
    // 仅在Debug模式下运行
    var result = service.DebugMethod();
    Assert.NotNull(result);
    #endif
}
```

### 日志分析

#### 启用详细日志

```bash
# 启用详细日志
dotnet test --verbosity normal

# 启用诊断日志
dotnet test --verbosity diagnostic
```

#### 日志文件位置

- 单元测试: `logs/unit-tests.log`
- 集成测试: `logs/integration-tests.log`
- 性能测试: `logs/performance-tests.log`
- 安全测试: `logs/security-tests.log`

#### 日志分析工具

```bash
# 使用grep过滤日志
grep "ERROR" logs/*.log

# 使用tail查看实时日志
tail -f logs/unit-tests.log

# 使用awk统计错误
awk '/ERROR/ {count++} END {print "Errors:", count}' logs/*.log
```

## 最佳实践

### 测试设计

#### 单一职责

```csharp
// 好的测试 - 单一职责
[Fact]
public void CreateScript_WithValidData_ShouldReturnScript()
{
    // 测试创建脚本的正常流程
}

// 避免的测试 - 多个职责
[Fact]
public void CreateScriptAndValidateAndSave()
{
    // 测试了多个功能点
}
```

#### AAA模式

```csharp
[Fact]
public void UpdateScript_WithValidData_ShouldUpdateScript()
{
    // Arrange - 准备
    var script = TestDataFactory.CreateValidScript();
    var updateRequest = new UpdateScriptRequest("Updated Name", "Updated Content");
    
    // Act - 执行
    var result = service.UpdateScript(script.Id, updateRequest);
    
    // Assert - 验证
    Assert.Equal("Updated Name", result.Name);
    Assert.Equal("Updated Content", result.Content);
}
```

#### 测试数据管理

```csharp
// 使用工厂模式
public static class ScriptFactory
{
    public static Script CreateValidScript()
    {
        return new Script(
            Guid.NewGuid(),
            "Test Script",
            "Content",
            ScriptType.JavaScript,
            ScriptStatus.Active
        );
    }
    
    public static Script CreateInvalidScript()
    {
        return new Script(
            Guid.Empty,
            "",
            "",
            ScriptType.JavaScript,
            ScriptStatus.Active
        );
    }
}
```

### 测试执行优化

#### 并行执行

```bash
# 启用并行执行
dotnet test --parallel

# 控制并行度
dotnet test --parallel --max-parallel-threads 4
```

#### 选择性执行

```bash
# 按类别执行
dotnet test --filter "Category=Unit"

# 按优先级执行
dotnet test --filter "Priority=High"

# 按标签执行
dotnet test --filter "Tag=Smoke"
```

#### 测试分组

```csharp
// 使用Traits进行测试分组
[Trait("Category", "Unit")]
[Trait("Priority", "High")]
[Fact]
public void HighPriorityUnitTest()
{
    // 高优先级单元测试
}
```

### 测试维护

#### 定期更新

```bash
# 定期更新测试依赖
dotnet update

# 定期更新测试工具
dotnet tool update -g dotnet-xunit
dotnet tool update -g reportgenerator
```

#### 重构优化

```csharp
// 提取公共测试逻辑
public class TestBase
{
    protected ScriptService CreateScriptService()
    {
        var mockRepository = new Mock<IScriptRepository>();
        return new ScriptService(mockRepository.Object);
    }
    
    protected void AssertScriptValid(Script script)
    {
        Assert.NotNull(script);
        Assert.NotEmpty(script.Name);
        Assert.NotEmpty(script.Content);
    }
}
```

#### 文档同步

```bash
# 生成测试文档
dotnet test --generate-documentation

# 更新测试统计
dotnet test --update-test-statistics
```

### 持续集成

#### GitHub Actions

```yaml
name: CI Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test --configuration Release --collect:"XPlat Code Coverage"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v4
```

#### Azure DevOps

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'
    
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*.csproj'
    arguments: '--configuration Release --collect:"XPlat Code Coverage"'
```

## 自动化脚本

### 完整测试自动化

```bash
#!/bin/bash
# run-all-tests.sh

echo "开始运行完整测试套件..."

# 单元测试
echo "运行单元测试..."
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --collect:"XPlat Code Coverage"

# 集成测试
echo "运行集成测试..."
dotnet test KeyForge.Tests/KeyForge.Tests.csproj --filter "Integration"

# 性能测试
echo "运行性能测试..."
cd KeyForge.Tests/Performance && dotnet run -- quick

# 安全测试
echo "运行安全测试..."
cd KeyForge.Tests/Security && ./run-security-tests.sh quick

# 覆盖率分析
echo "生成覆盖率报告..."
cd KeyForge.Tests.Coverage && ./run-coverage-analysis.sh run

echo "完整测试套件执行完成"
```

### 定期测试执行

```bash
# 添加到crontab (Linux/macOS)
# 每天凌晨2点运行测试
0 2 * * * /path/to/run-all-tests.sh

# Windows计划任务
# 创建每日任务
schtasks /create /tn "KeyForge Daily Tests" /tr "C:\path\to\run-all-tests.bat" /sc daily /st 02:00
```

### 测试报告自动化

```bash
# 生成综合测试报告
#!/bin/bash

echo "生成综合测试报告..."

# 合并测试结果
reportgenerator -reports:*/coverage.xml -targetdir:combined-report -reporttypes:Html

# 发送邮件通知
mail -s "KeyForge Test Report" team@example.com < combined-report/index.html

# 上传到云存储
aws s3 cp combined-report/ s3://keyforge-test-reports/$(date +%Y%m%d)/

echo "测试报告已生成并发送"
```

---

*指南版本: 1.0*
*最后更新: 2024-01-01*