# KeyForge 技术栈决策文档

## 技术选型概述

基于对KeyForge项目需求的分析，我选择了以下技术栈。这些选择考虑了项目的技术约束、性能要求、团队技能和长期维护性。

## 核心技术栈

### 1. 开发框架

#### 选择：.NET 8.0
**决策理由：**
- **长期支持**：.NET 8.0是当前的LTS版本，支持到2026年11月
- **性能优异**：相比.NET Framework有显著的性能提升
- **现代化特性**：支持C# 12、record类型、模式匹配等现代语言特性
- **跨平台**：虽然当前目标平台是Windows，但保留了跨平台的可能
- **工具支持**：Visual Studio 2022提供完整的开发体验

**替代方案考虑：**
- .NET Framework 4.8：虽然兼容性好，但性能和现代化程度不足
- .NET 6.0：也是LTS版本，但.NET 8.0有更多新特性

### 2. UI框架

#### 选择：WPF with MVVM
**决策理由：**
- **成熟稳定**：WPF已经发展多年，生态系统完善
- **原生Windows体验**：提供最好的Windows原生UI体验
- **数据绑定**：强大的数据绑定和命令系统
- **MVVM模式**：清晰的架构分离，便于测试和维护
- **控件丰富**：第三方控件库丰富，如DevExpress、Telerik等

**替代方案考虑：**
- WinForms：更简单但功能有限，不适合复杂界面
- MAUI：跨平台但相对较新，生态系统不够成熟
- Avalonia：跨平台但社区相对较小

### 3. 数据访问

#### 选择：Entity Framework Core + SQLite
**决策理由：**
- **轻量级**：SQLite是嵌入式数据库，无需额外配置
- **零配置**：适合桌面应用，用户无需安装数据库服务器
- **性能良好**：对于中等规模的数据量性能足够
- **EF Core**：现代ORM，支持Code First、迁移等特性
- **跨平台**：数据库文件可以在不同平台间迁移

**替代方案考虑：**
- Dapper：更轻量但需要手写SQL
- NHibernate：功能强大但学习曲线陡峭
- SQL Server Express：功能完整但需要额外安装

### 4. 图像处理

#### 选择：OpenCVSharp + Tesseract.NET
**决策理由：**
- **OpenCVSharp**：OpenCV的.NET封装，功能最全面的计算机视觉库
- **社区支持**：OpenCV拥有最大的计算机视觉社区
- **算法丰富**：提供各种图像处理和识别算法
- **Tesseract.NET**：开源OCR引擎，支持多语言识别
- **性能优化**：底层使用C++实现，性能优异

**替代方案考虑：**
- Emgu CV：也是OpenCV的.NET封装，但OpenCVSharp更活跃
- AForge.NET：较老的项目，更新较慢
- 自研算法：开发成本高，难以达到OpenCV的水平

### 5. 日志系统

#### 选择：Serilog
**决策理由：**
- **结构化日志**：支持JSON格式的结构化日志
- **多种输出**：支持文件、控制台、数据库等多种输出方式
- **性能优异**：异步日志记录，对性能影响小
- **配置灵活**：支持复杂的配置和过滤规则
- **生态系统**：有丰富的sink和扩展

**替代方案考虑：**
- NLog：功能完整但配置相对复杂
- log4net：较老的项目，现代化程度不足
- Microsoft.Extensions.Logging：功能基础，需要配合其他库使用

## 项目结构设计

### 目录结构

```
KeyForge/
├── src/
│   ├── KeyForge.Application/          # 应用层
│   │   ├── UseCases/                  # 用例
│   │   │   ├── Scripts/
│   │   │   ├── ImageRecognition/
│   │   │   └── DecisionMaking/
│   │   ├── Services/                  # 应用服务
│   │   ├── DTOs/                      # 数据传输对象
│   │   ├── Commands/                  # 命令
│   │   ├── Queries/                   # 查询
│   │   ├── Validators/                # 验证器
│   │   └── Events/                    # 应用事件
│   ├── KeyForge.Domain/               # 领域层
│   │   ├── Models/                    # 领域模型
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   └── Aggregates/
│   │   ├── Services/                  # 领域服务
│   │   ├── Interfaces/                # 接口定义
│   │   ├── Events/                    # 领域事件
│   │   └── Exceptions/                # 领域异常
│   ├── KeyForge.Infrastructure/      # 基础设施层
│   │   ├── Data/                      # 数据访问
│   │   │   ├── Entities/
│   │   │   ├── Configurations/
│   │   │   ├── Mappings/
│   │   │   └── Repositories/
│   │   ├── Imaging/                   # 图像处理
│   │   ├── Input/                     # 输入处理
│   │   ├── Recognition/               # 识别算法
│   │   ├── Logging/                   # 日志实现
│   │   ├── Configuration/             # 配置实现
│   │   ├── Plugins/                   # 插件系统
│   │   └ External/                    # 外部服务集成
│   ├── KeyForge.Presentation/         # 表现层
│   │   ├── Views/                     # 视图
│   │   ├── ViewModels/                # 视图模型
│   │   ├── Controls/                  # 用户控件
│   │   ├── Windows/                   # 窗口
│   │   ├── Resources/                 # 资源文件
│   │   ├── Services/                  # 表现层服务
│   │   └── Converters/                # 值转换器
│   └── KeyForge.Tests/                # 测试项目
│       ├── Unit/                      # 单元测试
│       ├── Integration/               # 集成测试
│       ├── Acceptance/                # 验收测试
│       └── Performance/                # 性能测试
├── docs/                              # 文档
│   ├── architecture/                  # 架构文档
│   ├── api/                           # API文档
│   ├── user-guide/                    # 用户指南
│   └── developer-guide/                # 开发指南
├── tools/                             # 工具脚本
│   ├── build/                         # 构建脚本
│   ├── deploy/                        # 部署脚本
│   └── test/                          # 测试工具
├── assets/                            # 资源文件
│   ├── images/                        # 图片资源
│   ├── icons/                         # 图标资源
│   └── templates/                     # 模板文件
├── build/                             # 构建配置
│   ├── azure-pipelines.yml            # Azure Pipelines
│   └── github-actions/                # GitHub Actions
├── README.md                          # 项目说明
├── LICENSE                            # 许可证
└── .gitignore                         # Git忽略文件
```

### NuGet包依赖管理

#### 核心依赖矩阵

| 项目 | 类型 | 包名 | 版本 | 用途 |
|------|------|------|------|------|
| KeyForge.Domain | NuGet | MediatR | 12.0.0 | 领域事件处理 |
| KeyForge.Domain | NuGet | Microsoft.Extensions.Logging.Abstractions | 8.0.0 | 日志抽象 |
| KeyForge.Application | NuGet | MediatR | 12.0.0 | CQRS模式 |
| KeyForge.Application | NuGet | FluentValidation | 11.5.0 | 输入验证 |
| KeyForge.Application | NuGet | Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.0 | DI抽象 |
| KeyForge.Infrastructure | NuGet | Microsoft.EntityFrameworkCore | 8.0.0 | ORM框架 |
| KeyForge.Infrastructure | NuGet | Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | SQLite提供程序 |
| KeyForge.Infrastructure | NuGet | OpenCvSharp4 | 4.8.0.20230708 | 图像处理 |
| KeyForge.Infrastructure | NuGet | Tesseract | 5.2.0 | OCR识别 |
| KeyForge.Infrastructure | NuGet | Serilog | 3.0.0 | 结构化日志 |
| KeyForge.Infrastructure | NuGet | Serilog.Sinks.File | 5.0.0 | 文件日志输出 |
| KeyForge.Presentation | NuGet | CommunityToolkit.Mvvm | 8.2.0 | MVVM工具包 |
| KeyForge.Presentation | NuGet | Microsoft.Extensions.Hosting | 8.0.0 | 主机服务 |
| KeyForge.Tests | NuGet | xunit | 2.4.2 | 单元测试框架 |
| KeyForge.Tests | NuGet | Moq | 4.18.2 | 模拟框架 |
| KeyForge.Tests | NuGet | FluentAssertions | 6.10.0 | 断言库 |

## 依赖注入配置

### 1. 应用层服务注册

```csharp
// KeyForge.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 添加MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssemblyContaining<DependencyInjection>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        
        // 添加应用服务
        services.AddScoped<IScriptRecordingApplicationService, ScriptRecordingApplicationService>();
        services.AddScoped<IImageRecognitionApplicationService, ImageRecognitionApplicationService>();
        services.AddScoped<IDecisionMakingApplicationService, DecisionMakingApplicationService>();
        services.AddScoped<IConfigurationApplicationService, ConfigurationApplicationService>();
        
        // 添加验证器
        services.AddValidatorsFromAssemblyContaining<DependencyInjection>();
        
        // 添加自动化映射
        services.AddAutoMapper(typeof(DependencyInjection));
        
        return services;
    }
}
```

### 2. 基础设施层服务注册

```csharp
// KeyForge.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置数据库
        services.AddDbContext<KeyForgeDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        
        // 配置仓储
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IImageTemplateRepository, ImageTemplateRepository>();
        services.AddScoped<IDecisionRuleRepository, DecisionRuleRepository>();
        services.AddScoped<IStateMachineRepository, StateMachineRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        
        // 配置领域服务
        services.AddScoped<IImageRecognitionService, ImageRecognitionService>();
        services.AddScoped<IActionExecutor, ActionExecutor>();
        services.AddScoped<IInputRecorder, InputRecorder>();
        services.AddScoped<IGameInputHandler, GameInputHandler>();
        services.AddScoped<IScreenCaptureService, ScreenCaptureService>();
        
        // 配置算法策略
        services.AddSingleton<IRecognitionAlgorithm, TemplateMatchingAlgorithm>();
        services.AddSingleton<IRecognitionAlgorithm, FeatureMatchingAlgorithm>();
        services.AddSingleton<IRecognitionAlgorithm, OcrRecognitionAlgorithm>();
        services.AddScoped<IRecognitionAlgorithmFactory, RecognitionAlgorithmFactory>();
        
        // 配置事件总线
        services.AddSingleton<IEventBus, MediatREventBus>();
        
        // 配置插件管理器
        services.AddScoped<IPluginManager, PluginManager>();
        
        // 配置其他服务
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IMetricsService, MetricsService>();
        
        return services;
    }
}
```

### 3. 表现层服务注册

```csharp
// KeyForge.Presentation/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // 配置主窗口和视图模型
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        
        // 配置视图和视图模型
        services.AddTransient<ScriptEditorView, ScriptEditorViewModel>();
        services.AddTransient<ImageTemplateView, ImageTemplateViewModel>();
        services.AddTransient<DecisionRuleView, DecisionRuleViewModel>();
        services.AddTransient<StateMachineView, StateMachineViewModel>();
        services.AddTransient<MonitoringView, MonitoringViewModel>();
        services.AddTransient<ConfigurationView, ConfigurationViewModel>();
        services.AddTransient<PluginManagerView, PluginManagerViewModel>();
        
        // 配置对话框服务
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<IInputDialogService, InputDialogService>();
        
        // 配置导航服务
        services.AddSingleton<INavigationService, NavigationService>();
        
        // 配置主题服务
        services.AddSingleton<IThemeService, ThemeService>();
        
        // 配置其他服务
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        
        return services;
    }
}
```

## 配置管理

### 1. 配置文件结构

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=keyforge.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/keyforge-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "ImageRecognition": {
    "DefaultAlgorithm": "TemplateMatching",
    "MinConfidence": 0.8,
    "MaxProcessingTimeMs": 100,
    "CacheResults": true,
    "CacheExpirationMinutes": 5
  },
  "InputSimulation": {
    "DefaultDelayMs": 50,
    "RandomizeDelay": true,
    "DelayVariationMs": 20,
    "MousePrecision": 2
  },
  "Performance": {
    "EnableMetrics": true,
    "MetricsIntervalSeconds": 30,
    "EnableProfiling": false,
    "LogPerformanceWarnings": true
  },
  "Plugins": {
    "Enabled": true,
    "PluginDirectory": "plugins",
    "EnableSandbox": true,
    "AllowedPermissions": ["FileRead", "Network", "Input"]
  },
  "UI": {
    "Theme": "Dark",
    "Language": "zh-CN",
    "AutoSaveIntervalMinutes": 5,
    "ShowTooltips": true,
    "EnableAnimations": true
  },
  "Updates": {
    "CheckForUpdates": true,
    "UpdateChannel": "Stable",
    "AutoInstall": false
  }
}
```

### 2. 配置验证

```csharp
// KeyForge.Infrastructure/Configuration/ConfigurationValidator.cs
public class ConfigurationValidator
{
    private readonly IConfiguration _configuration;
    
    public ConfigurationValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        
        // 验证数据库连接字符串
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("数据库连接字符串不能为空");
        }
        
        // 验证图像识别配置
        var minConfidence = _configuration.GetValue<double>("ImageRecognition:MinConfidence");
        if (minConfidence < 0 || minConfidence > 1)
        {
            errors.Add("图像识别最小置信度必须在0-1之间");
        }
        
        var maxProcessingTime = _configuration.GetValue<int>("ImageRecognition:MaxProcessingTimeMs");
        if (maxProcessingTime <= 0)
        {
            errors.Add("图像识别最大处理时间必须大于0");
        }
        
        // 验证输入模拟配置
        var defaultDelay = _configuration.GetValue<int>("InputSimulation:DefaultDelayMs");
        if (defaultDelay < 0)
        {
            errors.Add("默认延迟不能为负数");
        }
        
        var mousePrecision = _configuration.GetValue<int>("InputSimulation:MousePrecision");
        if (mousePrecision < 1)
        {
            errors.Add("鼠标精度必须大于0");
        }
        
        // 验证插件配置
        var pluginDirectory = _configuration.GetValue<string>("Plugins:PluginDirectory");
        if (string.IsNullOrWhiteSpace(pluginDirectory))
        {
            errors.Add("插件目录不能为空");
        }
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}
```

## 构建和部署

### 1. 构建配置

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>
  
  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <Optimize>true</Optimize>
    <DebugType>pdbonly</DebugType>
    <DebugSymbols>true</DebugSymbols>
  </PropertyGroup>
  
  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <Optimize>false</Optimize>
    <DebugType>full</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>
</Project>
```

### 2. 发布配置

```xml
<!-- KeyForge.Presentation/KeyForge.Presentation.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Resources\icon.ico</ApplicationIcon>
    <AssemblyTitle>KeyForge</AssemblyTitle>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <Product>KeyForge - 自动化按键脚本系统</Product>
    <Copyright>Copyright © 2024 KeyForge Team</Copyright>
    <Company>KeyForge Team</Company>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.DotNet.UpgradeAssistant.Extensions.DefaultAnalyzers" Version="0.4.410601">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  
  <ItemGroup>
    <!-- 本地资源 -->
    <Resource Include="Resources\**\*" />
    <Content Include="libs\**\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
  
  <Target Name="PublishApp" AfterTargets="Publish">
    <ItemGroup>
      <PublishFiles Include="$(PublishDir)\**\*" />
    </ItemGroup>
    <Copy SourceFiles="@(PublishFiles)" DestinationFolder="$(PackageDir)" />
  </Target>
</Project>
```

### 3. CI/CD 配置

```yaml
# .github/workflows/build.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
      
    - name: Publish
      run: dotnet publish KeyForge.Presentation/KeyForge.Presentation.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish
      
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: keyforge-release
        path: publish/
```

## 性能优化策略

### 1. 内存管理

```csharp
// KeyForge.Infrastructure/Performance/MemoryManager.cs
public class MemoryManager : IDisposable
{
    private readonly ILogger<MemoryManager> _logger;
    private readonly Timer _cleanupTimer;
    private readonly ConcurrentDictionary<string, WeakReference> _cache = new();
    
    public MemoryManager(ILogger<MemoryManager> logger)
    {
        _logger = logger;
        _cleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public T GetOrCreate<T>(string key, Func<T> factory) where T : class
    {
        if (_cache.TryGetValue(key, out var weakRef) && weakRef.IsAlive && weakRef.Target is T cached)
        {
            return cached;
        }
        
        var item = factory();
        _cache[key] = new WeakReference(item);
        
        // 监控内存使用
        var memoryInfo = GetMemoryInfo();
        if (memoryInfo.UsedPercentage > 80)
        {
            _logger.LogWarning("内存使用率过高: {UsedPercentage}%", memoryInfo.UsedPercentage);
            ForceCleanup();
        }
        
        return item;
    }
    
    private void CleanupCache(object state)
    {
        var keysToRemove = _cache.Where(kvp => !kvp.Value.IsAlive)
                                .Select(kvp => kvp.Key)
                                .ToList();
        
        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
        
        _logger.LogDebug("缓存清理完成，移除 {Count} 个无效引用", keysToRemove.Count);
    }
    
    private void ForceCleanup()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        CleanupCache(null);
    }
    
    private MemoryInfo GetMemoryInfo()
    {
        var memory = GC.GetTotalMemory(false);
        var maxMemory = Environment.ProcessVirtualMemorySize64;
        
        return new MemoryInfo(
            UsedBytes: memory,
            MaxBytes: maxMemory,
            UsedPercentage: (double)memory / maxMemory * 100
        );
    }
    
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        ForceCleanup();
    }
}
```

### 2. 并发处理

```csharp
// KeyForge.Infrastructure/Concurrency/ConcurrencyManager.cs
public class ConcurrencyManager
{
    private readonly SemaphoreSlim _imageProcessingSemaphore;
    private readonly SemaphoreSlim _inputExecutionSemaphore;
    private readonly ILogger<ConcurrencyManager> _logger;
    
    public ConcurrencyManager(ILogger<ConcurrencyManager> logger)
    {
        _imageProcessingSemaphore = new SemaphoreSlim(2, 2); // 最多2个并发图像处理
        _inputExecutionSemaphore = new SemaphoreSlim(1, 1);   // 输入执行需要串行
        _logger = logger;
    }
    
    public async Task<T> ExecuteImageProcessingAsync<T>(Func<Task<T>> operation)
    {
        await _imageProcessingSemaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _imageProcessingSemaphore.Release();
        }
    }
    
    public async Task<T> ExecuteInputOperationAsync<T>(Func<Task<T>> operation)
    {
        await _inputExecutionSemaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _inputExecutionSemaphore.Release();
        }
    }
    
    public async Task ParallelExecuteAsync<T>(IEnumerable<Func<Task<T>>> operations, int maxConcurrency = 4)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = operations.Select(async operation =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await operation();
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
    }
}
```

## 监控和诊断

### 1. 性能监控

```csharp
// KeyForge.Infrastructure/Metrics/PerformanceMetrics.cs
public class PerformanceMetrics : IPerformanceMetrics
{
    private readonly ILogger<PerformanceMetrics> _logger;
    private readonly ConcurrentDictionary<string, MetricCounter> _counters = new();
    private readonly Timer _reportTimer;
    
    public PerformanceMetrics(ILogger<PerformanceMetrics> logger)
    {
        _logger = logger;
        _reportTimer = new Timer(ReportMetrics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
    
    public void IncrementCounter(string name, double value = 1)
    {
        var counter = _counters.GetOrAdd(name, _ => new MetricCounter());
        counter.Increment(value);
    }
    
    public void RecordTiming(string name, TimeSpan duration)
    {
        var counter = _counters.GetOrAdd(name, _ => new MetricCounter());
        counter.RecordTiming(duration);
    }
    
    public void RecordGauge(string name, double value)
    {
        var counter = _counters.GetOrAdd(name, _ => new MetricCounter());
        counter.RecordGauge(value);
    }
    
    private void ReportMetrics(object state)
    {
        foreach (var kvp in _counters)
        {
            var counter = kvp.Value;
            _logger.LogInformation("指标 {Name}: Count={Count}, Sum={Sum}, Avg={Average:F2}", 
                kvp.Key, counter.Count, counter.Sum, counter.Average);
        }
    }
    
    public MetricsReport GenerateReport()
    {
        var report = new MetricsReport();
        
        foreach (var kvp in _counters)
        {
            report.Metrics[kvp.Key] = new MetricData
            {
                Count = kvp.Value.Count,
                Sum = kvp.Value.Sum,
                Average = kvp.Value.Average,
                Min = kvp.Value.Min,
                Max = kvp.Value.Max
            };
        }
        
        return report;
    }
}
```

### 2. 健康检查

```csharp
// KeyForge.Infrastructure/Health/HealthChecker.cs
public class HealthChecker : IHealthChecker
{
    private readonly ILogger<HealthChecker> _logger;
    private readonly IEnumerable<IHealthCheck> _healthChecks;
    
    public HealthChecker(ILogger<HealthChecker> logger, IEnumerable<IHealthCheck> healthChecks)
    {
        _logger = logger;
        _healthChecks = healthChecks;
    }
    
    public async Task<HealthReport> CheckHealthAsync()
    {
        var results = new List<HealthCheckResult>();
        
        foreach (var healthCheck in _healthChecks)
        {
            try
            {
                var result = await healthCheck.CheckHealthAsync();
                results.Add(result);
                
                if (result.Status != HealthStatus.Healthy)
                {
                    _logger.LogWarning("健康检查失败: {Name} - {Description}", 
                        healthCheck.GetType().Name, result.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查异常: {Name}", healthCheck.GetType().Name);
                results.Add(new HealthCheckResult(HealthStatus.Unhealthy, ex.Message));
            }
        }
        
        var overallStatus = results.All(r => r.Status == HealthStatus.Healthy) 
            ? HealthStatus.Healthy 
            : HealthStatus.Unhealthy;
        
        return new HealthReport(overallStatus, results);
    }
}
```

## 总结

这个技术栈选择为KeyForge项目提供了一个现代化、可维护、高性能的技术基础。关键决策包括：

1. **.NET 8.0**：选择最新的LTS版本，确保长期支持
2. **WPF + MVVM**：提供最佳的Windows桌面应用体验
3. **Entity Framework Core + SQLite**：轻量级但功能完整的数据访问方案
4. **OpenCVSharp**：功能最全面的图像处理库
5. **Serilog**：现代化的结构化日志系统
6. **Clean Architecture**：确保代码的可测试性和可维护性

所有技术选择都考虑了项目的实际需求、团队能力、长期维护性以及性能要求。通过合理的分层架构和依赖管理，系统能够支持未来的功能扩展和性能优化。