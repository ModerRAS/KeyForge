# KeyForge 测试常见问题解答 (FAQ)

## 目录

1. [测试环境配置](#测试环境配置)
2. [测试执行问题](#测试执行问题)
3. [测试数据管理](#测试数据管理)
4. [性能测试问题](#性能测试问题)
5. [安全测试问题](#安全测试问题)
6. [覆盖率分析问题](#覆盖率分析问题)
7. [持续集成问题](#持续集成问题)
8. [最佳实践](#最佳实践)

## 测试环境配置

### Q: 如何设置测试环境？

**A**: 按照以下步骤设置测试环境：

1. **安装.NET SDK**
   ```bash
   # 下载并安装.NET 8.0 SDK
   curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin
   
   # 验证安装
   dotnet --version
   ```

2. **克隆项目**
   ```bash
   git clone https://github.com/your-org/KeyForge.git
   cd KeyForge
   ```

3. **恢复依赖**
   ```bash
   dotnet restore
   ```

4. **运行测试**
   ```bash
   dotnet test
   ```

### Q: 测试数据库如何配置？

**A**: 项目支持多种测试数据库配置：

**In-Memory Database (推荐用于单元测试)**:
```csharp
services.AddDbContext<KeyForgeDbContext>(options =>
    options.UseInMemoryDatabase("KeyForgeTestDb"));
```

**SQL Server LocalDB**:
```csharp
services.AddDbContext<KeyForgeDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KeyForgeTest;Trusted_Connection=True;"));
```

**PostgreSQL**:
```csharp
services.AddDbContext<KeyForgeDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=KeyForgeTest;Username=test;Password=test123"));
```

### Q: 如何配置测试环境变量？

**A**: 可以通过以下方式配置：

**1. 使用 appsettings.json**
```json
{
  "TestSettings": {
    "Database": "InMemory",
    "Timeout": 30,
    "ParallelThreads": 4
  }
}
```

**2. 使用环境变量**
```bash
export TestSettings__Database="InMemory"
export TestSettings__Timeout="30"
```

**3. 使用命令行参数**
```bash
dotnet test --test:database=InMemory --test:timeout=30
```

## 测试执行问题

### Q: 测试失败时如何调试？

**A**: 按照以下步骤调试失败的测试：

1. **启用详细日志**
   ```bash
   dotnet test --verbosity diagnostic
   ```

2. **使用调试器**
   ```csharp
   [Fact]
   public void DebuggableTest()
   {
       // 在Visual Studio中设置断点
       Debugger.Break();
       
       // 测试代码
   }
   ```

3. **输出调试信息**
   ```csharp
   [Fact]
   public void VerboseTest()
   {
       var data = GetData();
       Console.WriteLine($"Data: {JsonSerializer.Serialize(data)}");
       
       var result = service.ProcessData(data);
       Assert.NotNull(result);
   }
   ```

4. **检查测试隔离**
   ```csharp
   [Fact]
   public async Task IsolatedTest()
   {
       // 使用独立的作用域
       using var scope = new TestScope();
       var service = scope.GetService<IScriptService>();
       
       // 测试代码
   }
   ```

### Q: 测试运行缓慢怎么办？

**A**: 优化测试执行速度的方法：

1. **启用并行执行**
   ```bash
   dotnet test --parallel --max-parallel-threads 4
   ```

2. **使用Mock对象**
   ```csharp
   [Fact]
   public void FastTest()
   {
       // 使用Mock而不是真实数据库
       var mockRepository = new Mock<IScriptRepository>();
       mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
           .ReturnsAsync(TestDataFactory.CreateValidScript());
       
       var service = new ScriptService(mockRepository.Object);
       var result = await service.GetScriptByIdAsync(Guid.NewGuid());
       
       Assert.NotNull(result);
   }
   ```

3. **优化数据库操作**
   ```csharp
   [Fact]
   public async Task OptimizedDatabaseTest()
   {
       // 使用事务回滚
       using var transaction = await context.Database.BeginTransactionAsync();
       
       try
       {
           // 测试代码
           await transaction.RollbackAsync();
       }
       catch
       {
           await transaction.RollbackAsync();
           throw;
       }
   }
   ```

4. **缓存测试结果**
   ```csharp
   [Fact]
   public void CachedTest()
   {
       // 使用静态缓存
       private static readonly object _cacheLock = new object();
       private static object _cachedResult;
       
       lock (_cacheLock)
       {
           if (_cachedResult == null)
           {
               _cachedResult = ExpensiveOperation();
           }
       }
       
       Assert.NotNull(_cachedResult);
   }
   ```

### Q: 如何处理测试依赖关系？

**A**: 正确处理测试依赖的方法：

1. **避免测试间的依赖**
   ```csharp
   [Fact]
   public void IndependentTest1()
   {
       // 每个测试都应该独立运行
       var data = TestDataFactory.CreateValidScript();
       var result = service.ProcessData(data);
       Assert.NotNull(result);
   }
   
   [Fact]
   public void IndependentTest2()
   {
       // 不依赖于其他测试的结果
       var data = TestDataFactory.CreateValidScript();
       var result = service.ProcessData(data);
       Assert.NotNull(result);
   }
   ```

2. **使用测试基类**
   ```csharp
   public class TestBase : IDisposable
   {
       protected readonly IServiceScope Scope;
       protected readonly KeyForgeDbContext Context;
       protected readonly IScriptService ScriptService;
       
       public TestBase()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           
           var serviceProvider = services.BuildServiceProvider();
           Scope = serviceProvider.CreateScope();
           
           Context = Scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
           ScriptService = Scope.ServiceProvider.GetRequiredService<IScriptService>();
           
           // 初始化测试数据
           InitializeTestData();
       }
       
       public void Dispose()
       {
           Scope?.Dispose();
       }
       
       private void InitializeTestData()
       {
           // 清理数据库
           Context.Database.EnsureDeleted();
           Context.Database.EnsureCreated();
           
           // 添加基础测试数据
           Context.Users.Add(TestDataFactory.CreateValidUser());
           Context.SaveChanges();
       }
   }
   ```

3. **使用测试初始化和清理**
   ```csharp
   public class ScriptServiceTests : TestBase
   {
       [Fact]
       public void CreateScript_WithValidData_ShouldReturnScript()
       {
           // Arrange - 使用基类提供的已配置服务
           var request = TestDataFactory.CreateValidCreateScriptRequest();
           
           // Act
           var result = ScriptService.CreateScript(request);
           
           // Assert
           Assert.NotNull(result);
           Assert.Equal(request.Name, result.Name);
       }
   }
   ```

## 测试数据管理

### Q: 如何管理测试数据？

**A**: 推荐使用以下方法管理测试数据：

1. **使用工厂模式**
   ```csharp
   public static class TestDataFactory
   {
       public static Script CreateValidScript()
       {
           return new Script(
               Guid.NewGuid(),
               "Test Script",
               "Script content",
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
       
       public static List<Script> CreateMultipleScripts(int count = 5)
       {
           var scripts = new List<Script>();
           for (int i = 0; i < count; i++)
           {
               scripts.Add(CreateValidScript());
           }
           return scripts;
       }
   }
   ```

2. **使用建造者模式**
   ```csharp
   public class ScriptBuilder
   {
       private Guid _id = Guid.NewGuid();
       private string _name = "Test Script";
       private string _content = "Script content";
       private ScriptType _type = ScriptType.JavaScript;
       private ScriptStatus _status = ScriptStatus.Active;
       
       public ScriptBuilder WithId(Guid id)
       {
           _id = id;
           return this;
       }
       
       public ScriptBuilder WithName(string name)
       {
           _name = name;
           return this;
       }
       
       public ScriptBuilder WithContent(string content)
       {
           _content = content;
           return this;
       }
       
       public ScriptBuilder WithType(ScriptType type)
       {
           _type = type;
           return this;
       }
       
       public ScriptBuilder WithStatus(ScriptStatus status)
       {
           _status = status;
           return this;
       }
       
       public Script Build()
       {
           return new Script(_id, _name, _content, _type, _status);
       }
   }
   
   // 使用示例
   var script = new ScriptBuilder()
       .WithName("Custom Script")
       .WithContent("Custom content")
       .WithType(ScriptType.Python)
       .Build();
   ```

3. **使用数据种子**
   ```csharp
   public class TestDataSeeder
   {
       public static void Seed(KeyForgeDbContext context)
       {
           // 清理现有数据
           context.Scripts.RemoveRange(context.Scripts);
           context.Users.RemoveRange(context.Users);
           
           // 添加用户
           var user = TestDataFactory.CreateValidUser();
           context.Users.Add(user);
           
           // 添加脚本
           var scripts = TestDataFactory.CreateMultipleScripts(10);
           foreach (var script in scripts)
           {
               script.UserId = user.Id;
               context.Scripts.Add(script);
           }
           
           context.SaveChanges();
       }
   }
   ```

### Q: 如何处理测试数据清理？

**A**: 使用以下方法确保测试数据清理：

1. **使用事务回滚**
   ```csharp
   [Fact]
   public async Task TransactionRollbackTest()
   {
       using var transaction = await context.Database.BeginTransactionAsync();
       
       try
       {
           // 添加测试数据
           var script = TestDataFactory.CreateValidScript();
           context.Scripts.Add(script);
           await context.SaveChangesAsync();
           
           // 执行测试
           var result = await service.GetScriptByIdAsync(script.Id);
           Assert.NotNull(result);
           
           // 回滚事务
           await transaction.RollbackAsync();
       }
       catch
       {
           await transaction.RollbackAsync();
           throw;
       }
   }
   ```

2. **使用IDisposable清理**
   ```csharp
   public class TestDatabaseCleaner : IDisposable
   {
       private readonly KeyForgeDbContext _context;
       private readonly List<object> _entitiesToAdd = new List<object>();
       private readonly List<object> _entitiesToRemove = new List<object>();
       
       public TestDatabaseCleaner(KeyForgeDbContext context)
       {
           _context = context;
       }
       
       public void AddToCleanup(params object[] entities)
       {
           _entitiesToRemove.AddRange(entities);
       }
       
       public void AddForCreation(params object[] entities)
       {
           _entitiesToAdd.AddRange(entities);
       }
       
       public void Dispose()
       {
           // 删除实体
           foreach (var entity in _entitiesToRemove)
           {
               _context.Remove(entity);
           }
           
           // 添加实体
           foreach (var entity in _entitiesToAdd)
           {
               _context.Add(entity);
           }
           
           _context.SaveChanges();
       }
   }
   
   // 使用示例
   [Fact]
   public async Task CleanupTest()
   {
       using var cleaner = new TestDatabaseCleaner(context);
       
       var script = TestDataFactory.CreateValidScript();
       cleaner.AddForCreation(script);
       
       await context.SaveChangesAsync();
       
       // 测试代码
       var result = await service.GetScriptByIdAsync(script.Id);
       Assert.NotNull(result);
   }
   ```

3. **使用Database Cleaner**
   ```csharp
   public static class DatabaseCleaner
   {
       public static void CleanDatabase(KeyForgeDbContext context)
       {
           // 禁用外键约束
           context.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
           
           // 清理所有表
           var tables = context.Model.GetEntityTypes()
               .Select(t => t.GetTableName())
               .Distinct();
           
           foreach (var table in tables)
           {
               try
               {
                   context.Database.ExecuteSqlRaw($"DELETE FROM {table}");
               }
               catch
               {
                   // 忽略错误
               }
           }
           
           // 重新启用外键约束
           context.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
           
           // 重置自增ID
           context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Users', RESEED, 1)");
           context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Scripts', RESEED, 1)");
       }
   }
   ```

## 性能测试问题

### Q: 性能测试结果不准确怎么办？

**A**: 确保性能测试准确的方法：

1. **预热环境**
   ```csharp
   [GlobalSetup]
   public class GlobalSetup
   {
       [GlobalSetup(Target = nameof(BenchmarkMethod))]
       public void Setup(BenchmarkContext context)
       {
           // 预热JIT编译
           for (int i = 0; i < 10; i++)
           {
               BenchmarkMethod();
           }
       }
   }
   ```

2. **多次运行取平均值**
   ```csharp
   [Benchmark]
   public void AveragePerformanceTest()
   {
       // BenchmarkDotNet会自动多次运行并计算平均值
       var result = service.Method();
       Assert.NotNull(result);
   }
   ```

3. **控制环境变量**
   ```csharp
   [Benchmark]
   public void ControlledEnvironmentTest()
   {
       // 确保测试环境一致
       GC.Collect();
       GC.WaitForPendingFinalizers();
       Thread.Sleep(100);
       
       var result = service.Method();
       Assert.NotNull(result);
   }
   ```

4. **隔离测试环境**
   ```csharp
   [Benchmark]
   public void IsolatedEnvironmentTest()
   {
       // 使用独立的进程或容器
       using var process = new Process();
       process.StartInfo.FileName = "dotnet";
       process.StartInfo.Arguments = "run --project TestProject";
       process.Start();
       process.WaitForExit();
       
       Assert.Equal(0, process.ExitCode);
   }
   ```

### Q: 如何处理性能测试中的内存泄漏？

**A**: 检测和处理内存泄漏的方法：

1. **使用MemoryDiagnoser**
   ```csharp
   [MemoryDiagnoser]
   public class MemoryLeakBenchmarks
   {
       [Benchmark]
       public void MemoryLeakTest()
       {
           var service = new ScriptService();
           var result = service.Method();
           Assert.NotNull(result);
       }
   }
   ```

2. **监控内存使用**
   ```csharp
   [Benchmark]
   public void MemoryMonitoringTest()
   {
       var initialMemory = GC.GetTotalMemory(false);
       
       // 执行操作
       var service = new ScriptService();
       var result = service.Method();
       
       var finalMemory = GC.GetTotalMemory(false);
       var memoryIncrease = finalMemory - initialMemory;
       
       Assert.True(memoryIncrease < 1024, $"Memory increase: {memoryIncrease} bytes");
   }
   ```

3. **使用内存分析工具**
   ```csharp
   [Benchmark]
   public void MemoryProfilingTest()
   {
       // 使用dotMemory或其他内存分析工具
       using var profiler = new MemoryProfiler();
       
       profiler.Start();
       
       var service = new ScriptService();
       var result = service.Method();
       
       profiler.Stop();
       
       // 分析内存快照
       var snapshot = profiler.GetSnapshot();
       Assert.True(snapshot.GetTotalMemory() < 1024 * 1024, "Memory usage too high");
   }
   ```

4. **强制垃圾回收**
   ```csharp
   [Benchmark]
   public void GarbageCollectionTest()
   {
       var service = new ScriptService();
       
       for (int i = 0; i < 1000; i++)
       {
           var result = service.Method();
           Assert.NotNull(result);
       }
       
       // 强制垃圾回收
       GC.Collect();
       GC.WaitForPendingFinalizers();
       GC.Collect();
   }
   ```

## 安全测试问题

### Q: 安全测试失败怎么办？

**A**: 处理安全测试失败的方法：

1. **检查安全配置**
   ```csharp
   [Fact]
   public void SecurityConfigurationTest()
   {
       // 检查安全配置
       var config = new SecuritySettings();
       
       Assert.True(config.TestPasswordComplexity);
       Assert.True(config.TestXssPrevention);
       Assert.True(config.TestSqlInjectionPrevention);
   }
   ```

2. **更新漏洞数据库**
   ```bash
   # 更新漏洞数据库
   cd KeyForge.Tests/Security
   curl -o vulnerability-database.json https://example.com/latest-vulnerabilities.json
   
   # 重新运行安全测试
   ./run-security-tests.sh scan
   ```

3. **修复安全问题**
   ```csharp
   [Fact]
   public void SecurityFixTest()
   {
       // 测试修复后的安全问题
       var maliciousInput = "<script>alert('xss')</script>";
       var result = service.ValidateInput(maliciousInput);
       
       Assert.False(result.IsValid);
       Assert.Contains("XSS", result.ErrorMessage);
   }
   ```

4. **添加安全测试用例**
   ```csharp
   [Theory]
   [InlineData("<script>alert('xss')</script>")]
   [InlineData("'; DROP TABLE users; --")]
   [InlineData("../../../etc/passwd")]
   public void SecurityTest_WithMaliciousInput_ShouldBlock(string maliciousInput)
   {
       var result = service.ValidateInput(maliciousInput);
       
       Assert.False(result.IsValid);
       Assert.Contains("malicious", result.ErrorMessage.ToLower());
   }
   ```

### Q: 如何处理误报？

**A**: 处理安全测试误报的方法：

1. **配置白名单**
   ```json
   {
     "SecuritySettings": {
       "Whitelist": [
         {
           "Pattern": "admin@example.com",
           "Description": "测试管理员账户"
         }
       ]
     }
   }
   ```

2. **调整检测规则**
   ```csharp
   [Fact]
   public void FalsePositiveTest()
   {
       // 配置检测规则
       var settings = new SecuritySettings
       {
           TestXssPrevention = true,
           Whitelist = new List<string> { "test@example.com" }
       };
       
       var result = service.ValidateInput("test@example.com");
       
       // 应该通过白名单验证
       Assert.True(result.IsValid);
   }
   ```

3. **更新测试数据**
   ```csharp
   [Fact]
   public void UpdatedTestDataTest()
   {
       // 使用更新的测试数据
       var testData = new SecurityTestData
       {
           ValidInputs = new List<string> { "test@example.com", "user@domain.com" },
           InvalidInputs = new List<string> { "<script>alert('xss')</script>" }
       };
       
       foreach (var input in testData.InvalidInputs)
       {
           var result = service.ValidateInput(input);
           Assert.False(result.IsValid);
       }
   }
   ```

## 覆盖率分析问题

### Q: 覆盖率低怎么办？

**A**: 提高测试覆盖率的方法：

1. **识别未覆盖的代码**
   ```bash
   # 生成覆盖率报告
   dotnet test --collect:"XPlat Code Coverage"
   
   # 查看覆盖率报告
   open coverage/index.html
   ```

2. **添加缺失的测试**
   ```csharp
   [Fact]
   public void UncoveredCodeTest()
   {
       // 测试之前未覆盖的代码路径
       var result = service.Method(new { Parameter = "value" });
       
       Assert.NotNull(result);
   }
   ```

3. **使用参数化测试**
   ```csharp
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("valid")]
   public void ParameterizedTest(string input)
   {
       var result = service.ValidateInput(input);
       
       if (string.IsNullOrEmpty(input))
       {
           Assert.False(result.IsValid);
       }
       else
       {
           Assert.True(result.IsValid);
       }
   }
   ```

4. **测试异常路径**
   ```csharp
   [Fact]
   public void ExceptionPathTest()
   {
       // 测试异常处理
       Assert.Throws<ArgumentException>(() => service.Method(null));
   }
   ```

### Q: 如何设置覆盖率阈值？

**A**: 配置覆盖率阈值的方法：

1. **在项目中设置**
   ```xml
   <PropertyGroup>
     <CollectCoverage>true</CollectCoverage>
     <CoverletOutputFormat>json</CoverletOutputFormat>
     <CoverletOutput>./coverage/</CoverletOutput>
     <Exclude>[*]*.Designer.cs</Exclude>
   </PropertyGroup>
   ```

2. **在CI/CD中设置**
   ```yaml
   - name: Test
     run: dotnet test --collect:"XPlat Code Coverage"
   
   - name: Check Coverage
     run: |
       coverage=$(jq '.linePercent' coverage/coverage.json)
       if (( $(echo "$coverage < 80" | bc -l) )); then
         echo "Coverage too low: $coverage%"
         exit 1
       fi
   ```

3. **使用覆盖率工具**
   ```bash
   # 使用coverlet
   dotnet test --collect:"XPlat Code Coverage;Format=cobertura"
   
   # 使用reportgenerator
   reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html
   ```

## 持续集成问题

### Q: CI/CD 管道失败怎么办？

**A**: 调试CI/CD管道失败的方法：

1. **检查日志**
   ```bash
   # 查看构建日志
   git log --oneline -5
   
   # 查看测试日志
   cat logs/test-results.log
   ```

2. **本地重现**
   ```bash
   # 在本地重现CI环境
   docker run -it --rm -v $(pwd):/app mcr.microsoft.com/dotnet/sdk:8.0 bash
   
   # 在容器中运行测试
   cd /app
   dotnet test
   ```

3. **检查环境差异**
   ```bash
   # 检查.NET版本
   dotnet --version
   
   # 检查依赖版本
   dotnet list package --version
   
   # 检查环境变量
   env | grep -E "(DOTNET|PATH|HOME)"
   ```

4. **逐步调试**
   ```bash
   # 分步执行CI流程
   echo "Step 1: Restore dependencies"
   dotnet restore
   
   echo "Step 2: Build solution"
   dotnet build --configuration Release
   
   echo "Step 3: Run tests"
   dotnet test --configuration Release
   ```

### Q: 如何优化CI/CD性能？

**A**: 优化CI/CD性能的方法：

1. **使用缓存**
   ```yaml
   - name: Cache NuGet packages
     uses: actions/cache@v3
     with:
       path: ~/.nuget/packages
       key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
       restore-keys: |
         ${{ runner.os }}-nuget-
   ```

2. **并行执行**
   ```yaml
   jobs:
     test:
       strategy:
         matrix:
           os: [ubuntu-latest, windows-latest, macos-latest]
           dotnet: [8.0.x, 7.0.x]
       runs-on: ${{ matrix.os }}
       
       steps:
       - name: Setup .NET
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: ${{ matrix.dotnet }}
   ```

3. **选择性测试**
   ```yaml
   - name: Run affected tests
     run: |
       if [[ ${{ github.event_name }} == "pull_request" ]]; then
         dotnet test --filter "AffectedByPR"
       else
         dotnet test
       fi
   ```

4. **增量构建**
   ```yaml
   - name: Incremental build
     run: |
       if [[ -f "build-cache.txt" ]]; then
         dotnet build --no-restore
       else
         dotnet build
         echo "build-cache-$(git rev-parse HEAD)" > build-cache.txt
       fi
   ```

## 最佳实践

### Q: 编写可维护的测试的最佳实践是什么？

**A**: 编写可维护测试的最佳实践：

1. **使用描述性的测试名称**
   ```csharp
   // 好的测试名称
   [Fact]
   public void CreateScript_WithValidData_ShouldReturnScript()
   
   // 避免的测试名称
   [Fact]
   public void Test1()
   ```

2. **遵循AAA模式**
   ```csharp
   [Fact]
   public void AAAPatternTest()
   {
       // Arrange - 准备
       var service = new ScriptService();
       var request = new CreateScriptRequest("Test", "Content");
       
       // Act - 执行
       var result = service.CreateScript(request);
       
       // Assert - 验证
       Assert.NotNull(result);
       Assert.Equal("Test", result.Name);
   }
   ```

3. **使用测试基类**
   ```csharp
   public class TestBase : IDisposable
   {
       protected readonly IServiceScope Scope;
       protected readonly KeyForgeDbContext Context;
       
       public TestBase()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           
           var serviceProvider = services.BuildServiceProvider();
           Scope = serviceProvider.CreateScope();
           Context = Scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
           
           InitializeTestData();
       }
       
       public void Dispose()
       {
           Scope?.Dispose();
       }
       
       private void InitializeTestData()
       {
           Context.Database.EnsureDeleted();
           Context.Database.EnsureCreated();
       }
   }
   ```

4. **使用Mock对象**
   ```csharp
   [Fact]
   public void MockObjectTest()
   {
       // 使用Mock而不是真实依赖
       var mockRepository = new Mock<IScriptRepository>();
       mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
           .ReturnsAsync(TestDataFactory.CreateValidScript());
       
       var service = new ScriptService(mockRepository.Object);
       var result = await service.GetScriptByIdAsync(Guid.NewGuid());
       
       Assert.NotNull(result);
   }
   ```

5. **使用参数化测试**
   ```csharp
   [Theory]
   [InlineData("valid@email.com", true)]
   [InlineData("invalid-email", false)]
   [InlineData("", false)]
   public void EmailValidationTest(string email, bool expected)
   {
       var result = EmailValidator.Validate(email);
       
       Assert.Equal(expected, result.IsValid);
   }
   ```

6. **使用测试数据工厂**
   ```csharp
   public static class TestDataFactory
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
       
       public static User CreateValidUser()
       {
           return new User(
               Guid.NewGuid(),
               "test@example.com",
               "Test User",
               "User"
           );
       }
   }
   ```

7. **使用测试标签**
   ```csharp
   [Trait("Category", "Unit")]
   [Trait("Priority", "High")]
   [Fact]
   public void TaggedTest()
   {
       // 高优先级单元测试
       var result = service.Method();
       Assert.NotNull(result);
   }
   ```

### Q: 如何保持测试和代码同步？

**A**: 保持测试和代码同步的方法：

1. **测试驱动开发 (TDD)**
   ```bash
   # 1. 写失败的测试
   # 2. 写代码让测试通过
   # 3. 重构代码
   ```

2. **持续集成**
   ```yaml
   # 每次提交都运行测试
   on:
     push:
       branches: [main, develop]
     pull_request:
       branches: [main]
   ```

3. **代码审查**
   ```markdown
   ## 代码审查清单
   - [ ] 所有测试都通过
   - [ ] 代码覆盖率 > 80%
   - [ ] 没有安全问题
   - [ ] 性能测试通过
   ```

4. **定期重构**
   ```bash
   # 定期重构测试代码
   # 删除过时的测试
   # 更新测试数据
   ```

### Q: 如何处理测试中的技术债务？

**A**: 处理测试技术债务的方法：

1. **识别技术债务**
   ```bash
   # 标记技术债务
   // TODO: 需要重构这个测试
   [Fact]
   public void TechnicalDebtTest()
   {
       // 这个测试需要重构
   }
   ```

2. **优先级排序**
   ```markdown
   ## 测试技术债务优先级
   - 高: 影响CI/CD的测试
   - 中: 降低代码质量的测试
   - 低: 代码风格问题
   ```

3. **逐步改进**
   ```bash
   # 每次提交修复一个小问题
   # 定期安排时间处理技术债务
   ```

4. **自动化检测**
   ```yaml
   # 在CI中检测测试技术债务
   - name: Check test quality
     run: |
       dotnet test --test-quality
   ```

---

*FAQ版本: 1.0*
*最后更新: 2024-01-01*