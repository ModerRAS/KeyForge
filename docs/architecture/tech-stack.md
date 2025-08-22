# KeyForge 技术栈选择文档

## 1. 技术选型原则

### 1.1 选型原则
- **稳定性优先**：选择成熟、稳定的技术栈
- **性能考量**：确保满足系统性能要求
- **可维护性**：选择团队熟悉且有良好生态的技术
- **可扩展性**：支持系统未来的功能扩展
- **成本效益**：平衡技术先进性和实施成本

### 1.2 评估维度
- **技术成熟度**：技术的稳定性和社区支持
- **学习曲线**：团队掌握技术所需的时间
- **生态系统**：相关库和工具的丰富程度
- **性能表现**：在目标场景下的性能表现
- **安全性**：技术的安全特性和漏洞情况

## 2. .NET 6.0+ 技术栈

### 2.1 核心框架选择

#### 2.1.1 .NET 6.0 LTS
**选择理由**：
- ✅ **长期支持**：Microsoft提供5年长期支持
- ✅ **性能优异**：相比.NET Framework有显著性能提升
- ✅ **跨平台**：支持Windows、Linux、macOS
- ✅ **现代化特性**：支持C# 10+的新特性
- ✅ **容器友好**：优化的容器支持和部署

**关键特性**：
- 高性能的JIT编译器和GC
- 原生AOT编译支持
- 改进的异步编程模型
- 集成的OpenTelemetry支持
- 增强的JSON API性能

#### 2.1.2 ASP.NET Core
**选择理由**：
- ✅ **高性能**：业界领先的Web框架性能
- ✅ **模块化**：轻量级且可扩展的架构
- ✅ **跨平台**：支持多种操作系统
- ✅ **内置依赖注入**：支持IoC容器
- ✅ **中间件管道**：灵活的请求处理管道

**关键组件**：
- Kestrel高性能服务器
- Middleware中间件系统
- 内置身份认证和授权
- 集成的日志记录框架
- 健康检查和诊断API

### 2.2 开发框架和工具

#### 2.2.1 WPF (Windows Presentation Foundation)
**选择理由**：
- ✅ **原生Windows支持**：深度集成Windows特性
- ✅ **丰富的UI控件**：提供完整的桌面应用控件库
- ✅ **MVVM支持**：天然支持MVVM架构模式
- ✅ **XAML声明式UI**：分离UI和业务逻辑
- ✅ **性能优化**：硬件加速的图形渲染

**技术栈**：
- **XAML**：UI标记语言
- **MVVM Toolkit**：MVVM模式实现
- **MahApps.Metro**：现代化UI主题
- **HandyControl**：扩展控件库
- **LiveCharts2**：数据可视化

#### 2.2.2 Entity Framework Core
**选择理由**：
- ✅ **ORM成熟度**：.NET生态系统中最成熟的ORM
- ✅ **LINQ支持**：强类型的查询语法
- ✅ **数据库迁移**：自动化的数据库版本管理
- ✅ **多数据库支持**：支持多种数据库提供商
- ✅ **性能优化**：查询优化和缓存机制

**配置示例**：
```csharp
// 数据库上下文配置
public class KeyForgeDbContext : DbContext
{
    public DbSet<Script> Scripts { get; set; }
    public DbSet<ImageTemplate> ImageTemplates { get; set; }
    public DbSet<DecisionRule> DecisionRules { get; set; }
    public DbSet<SystemConfiguration> Configurations { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=keyforge.db");
        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableDetailedErrors(false);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeyForgeDbContext).Assembly);
    }
}
```

### 2.3 依赖注入和IoC

#### 2.3.1 Microsoft.Extensions.DependencyInjection
**选择理由**：
- ✅ **官方支持**：Microsoft官方DI容器
- ✅ **性能优异**：高性能的依赖注入实现
- ✅ **生命周期管理**：支持Scoped、Singleton、Transient生命周期
- ✅ **集成度高**：与ASP.NET Core深度集成
- ✅ **易于使用**：简单的API设计

**配置示例**：
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyForgeServices(this IServiceCollection services)
    {
        // 领域服务
        services.AddScoped<IScriptExecutionService, ScriptExecutionService>();
        services.AddScoped<IImageRecognitionService, ImageRecognitionService>();
        services.AddScoped<IRuleEngineService, RuleEngineService>();
        
        // 应用服务
        services.AddScoped<IRecordScriptUseCase, RecordScriptUseCase>();
        services.AddScoped<IImageTriggerUseCase, ImageTriggerUseCase>();
        services.AddScoped<IDecisionMakingUseCase, DecisionMakingUseCase>();
        
        // 基础设施服务
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IImageTemplateRepository, ImageTemplateRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        
        // 消息队列和事件总线
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<IMessageQueue, RabbitMqMessageQueue>();
        
        return services;
    }
}
```

## 3. 图像处理库选型

### 3.1 OpenCVSharp

#### 3.1.1 技术选型
**选择理由**：
- ✅ **功能完整**：OpenCV的完整.NET封装
- ✅ **性能优异**：底层使用C++实现，性能接近原生
- ✅ **算法丰富**：包含2000+优化算法
- ✅ **跨平台**：支持Windows、Linux、macOS
- ✅ **社区活跃**：活跃的开发社区和文档

**核心功能**：
- 图像读取和写入
- 图像预处理（灰度化、二值化、滤波等）
- 模板匹配
- 特征点检测和匹配
- OCR文字识别
- 机器学习算法

#### 3.1.2 技术实现
```csharp
// 图像识别服务实现
public class OpenCvImageRecognitionService : IImageRecognitionService
{
    private readonly ILogger<OpenCvImageRecognitionService> _logger;
    
    public async Task<RecognitionResult> RecognizeAsync(RecognitionRequest request)
    {
        try
        {
            // 加载源图像和模板
            using var sourceImage = Mat.FromImageData(request.SourceImage.Data);
            using var templateImage = Mat.FromImageData(request.TemplateImage.Data);
            
            // 图像预处理
            var processedSource = PreprocessImage(sourceImage, request.Parameters);
            var processedTemplate = PreprocessImage(templateImage, request.Parameters);
            
            // 执行模板匹配
            var result = PerformTemplateMatching(processedSource, processedTemplate, request.Parameters);
            
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image recognition failed");
            throw new ImageRecognitionException("Image recognition failed", ex);
        }
    }
    
    private Mat PreprocessImage(Mat image, RecognitionParameters parameters)
    {
        var processed = image.Clone();
        
        // 灰度化
        if (parameters.Preprocessing.Grayscale)
        {
            Cv2.CvtColor(processed, processed, ColorConversionCodes.BGR2GRAY);
        }
        
        // 调整大小
        if (parameters.Preprocessing.Resize != null)
        {
            Cv2.Resize(processed, processed, 
                new OpenCvSharp.Size(
                    parameters.Preprocessing.Resize.Width,
                    parameters.Preprocessing.Resize.Height));
        }
        
        // 高斯模糊
        if (parameters.Preprocessing.Blur != null)
        {
            Cv2.GaussianBlur(processed, processed, 
                new OpenCvSharp.Size(parameters.Preprocessing.Blur.KernelSize, 
                parameters.Preprocessing.Blur.KernelSize), 0);
        }
        
        return processed;
    }
    
    private RecognitionResult PerformTemplateMatching(Mat source, Mat template, RecognitionParameters parameters)
    {
        var result = source.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
        result.MinMaxLoc(out var minVal, out var maxVal, out var minLoc, out var maxLoc);
        
        var confidence = maxVal;
        var location = new ScreenLocation(maxLoc.X, maxLoc.Y);
        
        if (confidence >= parameters.Threshold)
        {
            return new RecognitionResult(
                RecognitionStatus.Success,
                location,
                new ConfidenceScore(confidence),
                RecognitionMethod.TemplateMatching,
                Duration.FromMilliseconds(result.ProcessingTimeMs)
            );
        }
        
        return new RecognitionResult(
            RecognitionStatus.Failed,
            location,
            new ConfidenceScore(confidence),
            RecognitionMethod.TemplateMatching,
            Duration.FromMilliseconds(result.ProcessingTimeMs)
        );
    }
}
```

### 3.2 备选方案对比

#### 3.2.1 Emgu CV
**优势**：
- 更完整的商业支持
- 更好的文档和示例
- 支持GPU加速

**劣势**：
- 商业许可证成本较高
- 社区相对较小

#### 3.2.2 AForge.NET
**优势**：
- 轻量级框架
- 学习曲线较平缓
- 适合简单图像处理

**劣势**：
- 功能相对有限
- 更新维护不活跃

## 4. 数据库选择

### 4.1 SQLite 选择

#### 4.1.1 技术选型
**选择理由**：
- ✅ **零配置**：无需安装和配置
- ✅ **嵌入式**：直接集成到应用程序中
- ✅ **跨平台**：支持所有主要平台
- ✅ **轻量级**：小体积、低内存占用
- ✅ **高性能**：针对小型应用优化
- ✅ **ACID支持**：完整的事务支持

**适用场景**：
- 配置数据存储
- 用户数据管理
- 日志记录
- 缓存数据
- 模板和脚本存储

#### 4.1.2 数据库设计
```sql
-- 脚本表
CREATE TABLE Scripts (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Version TEXT NOT NULL,
    Status TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    Metadata TEXT
);

-- 操作序列表
CREATE TABLE ActionSequences (
    Id TEXT PRIMARY KEY,
    ScriptId TEXT NOT NULL,
    Name TEXT NOT NULL,
    OrderIndex INTEGER NOT NULL,
    LoopCount INTEGER DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ScriptId) REFERENCES Scripts(Id) ON DELETE CASCADE
);

-- 游戏操作表
CREATE TABLE GameActions (
    Id TEXT PRIMARY KEY,
    SequenceId TEXT NOT NULL,
    ActionType TEXT NOT NULL,
    Parameters TEXT NOT NULL,
    Delay INTEGER DEFAULT 0,
    OrderIndex INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (SequenceId) REFERENCES ActionSequences(Id) ON DELETE CASCADE
);

-- 图像模板表
CREATE TABLE ImageTemplates (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    ImageData BLOB NOT NULL,
    Parameters TEXT NOT NULL,
    Version TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

-- 决策规则表
CREATE TABLE DecisionRules (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    ConditionExpression TEXT NOT NULL,
    ThenActionId TEXT,
    ElseActionId TEXT,
    Priority INTEGER DEFAULT 0,
    Status TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (ThenActionId) REFERENCES ActionSequences(Id),
    FOREIGN KEY (ElseActionId) REFERENCES ActionSequences(Id)
);

-- 状态机表
CREATE TABLE StateMachines (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CurrentStateId TEXT,
    Configuration TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

-- 系统配置表
CREATE TABLE SystemConfigurations (
    Id TEXT PRIMARY KEY,
    Key TEXT UNIQUE NOT NULL,
    Value TEXT NOT NULL,
    ValueType TEXT NOT NULL,
    Description TEXT,
    IsEncrypted BOOLEAN DEFAULT FALSE,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

-- 日志表
CREATE TABLE LogEntries (
    Id TEXT PRIMARY KEY,
    Timestamp TEXT NOT NULL,
    Level TEXT NOT NULL,
    Message TEXT NOT NULL,
    Exception TEXT,
    Context TEXT,
    Source TEXT,
    CreatedAt TEXT NOT NULL
);

-- 创建索引
CREATE INDEX idx_scripts_status ON Scripts(Status);
CREATE INDEX idx_scripts_created_at ON Scripts(CreatedAt);
CREATE INDEX idx_action_sequences_script_id ON ActionSequences(ScriptId);
CREATE INDEX idx_game_actions_sequence_id ON GameActions(SequenceId);
CREATE INDEX idx_image_templates_name ON ImageTemplates(Name);
CREATE INDEX idx_decision_rules_name ON DecisionRules(Name);
CREATE INDEX idx_log_entries_timestamp ON LogEntries(Timestamp);
CREATE INDEX idx_log_entries_level ON LogEntries(Level);
```

### 4.2 Entity Framework Core 配置

#### 4.2.1 实体配置
```csharp
// 脚本实体配置
public class ScriptConfiguration : IEntityTypeConfiguration<Script>
{
    public void Configure(EntityTypeBuilder<Script> builder)
    {
        builder.ToTable("Scripts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Version).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
        builder.Property(s => s.Metadata).HasColumnType("TEXT");
        
        builder.HasMany(s => s.ActionSequences)
            .WithOne()
            .HasForeignKey("ScriptId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// 图像模板实体配置
public class ImageTemplateConfiguration : IEntityTypeConfiguration<ImageTemplate>
{
    public void Configure(EntityTypeBuilder<ImageTemplate> builder)
    {
        builder.ToTable("ImageTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.ImageData).IsRequired();
        builder.Property(t => t.Parameters).IsRequired().HasColumnType("TEXT");
        builder.Property(t => t.Version).IsRequired().HasMaxLength(20);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
    }
}

// 决策规则实体配置
public class DecisionRuleConfiguration : IEntityTypeConfiguration<DecisionRule>
{
    public void Configure(EntityTypeBuilder<DecisionRule> builder)
    {
        builder.ToTable("DecisionRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ConditionExpression).IsRequired().HasColumnType("TEXT");
        builder.Property(r => r.Priority).HasDefaultValue(0);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
        
        builder.HasOne(r => r.ThenAction)
            .WithMany()
            .HasForeignKey("ThenActionId")
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.ElseAction)
            .WithMany()
            .HasForeignKey("ElseActionId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

## 5. 消息队列和事件处理

### 5.1 RabbitMQ 选择

#### 5.1.1 技术选型
**选择理由**：
- ✅ **可靠性**：企业级的消息传递保证
- ✅ **灵活性**：支持多种消息模式
- ✅ **集群支持**：支持高可用集群部署
- ✅ **管理界面**：提供Web管理界面
- ✅ **协议支持**：支持AMQP、MQTT等多种协议
- ✅ **插件系统**：丰富的插件生态

**核心功能**：
- 消息持久化
- 消息确认机制
- 消息路由
- 死信队列
- 消息优先级
- 消息TTL

#### 5.1.2 技术实现
```csharp
// RabbitMQ 消息队列服务
public class RabbitMqMessageQueue : IMessageQueue
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqMessageQueue> _logger;
    
    public RabbitMqMessageQueue(RabbitMqConfiguration configuration, ILogger<RabbitMqMessageQueue> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration.HostName,
            Port = configuration.Port,
            UserName = configuration.UserName,
            Password = configuration.Password,
            VirtualHost = configuration.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // 声明交换器
        _channel.ExchangeDeclare("keyforge.events", ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare("keyforge.commands", ExchangeType.Direct, durable: true);
        
        // 声明队列
        DeclareQueues();
        
        // 绑定队列到交换器
        BindQueues();
    }
    
    private void DeclareQueues()
    {
        // 事件队列
        _channel.QueueDeclare("script.events", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("image.recognition.events", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("decision.events", durable: true, exclusive: false, autoDelete: false);
        
        // 命令队列
        _channel.QueueDeclare("script.commands", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("image.recognition.commands", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("decision.commands", durable: true, exclusive: false, autoDelete: false);
        
        // 死信队列
        _channel.QueueDeclare("dead.letter", durable: true, exclusive: false, autoDelete: false);
    }
    
    private void BindQueues()
    {
        // 绑定事件队列
        _channel.QueueBind("script.events", "keyforge.events", "script.*");
        _channel.QueueBind("image.recognition.events", "keyforge.events", "image.*");
        _channel.QueueBind("decision.events", "keyforge.events", "decision.*");
        
        // 绑定命令队列
        _channel.QueueBind("script.commands", "keyforge.commands", "script");
        _channel.QueueBind("image.recognition.commands", "keyforge.commands", "image");
        _channel.QueueBind("decision.commands", "keyforge.commands", "decision");
    }
    
    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            var messageBody = JsonSerializer.SerializeToUtf8Bytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
            _channel.BasicPublish(exchange, routingKey, properties, messageBody);
            
            _logger.LogInformation("Message published to {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
            throw;
        }
    }
    
    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(body);
                
                await handler(message);
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message from queue {Queue}", queue);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };
        
        _channel.BasicConsume(queue, false, consumer);
    }
    
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

### 5.2 事件总线实现

#### 5.2.1 内存事件总线
```csharp
// 内存事件总线实现
public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new();
    private readonly ILogger<InMemoryEventBus> _logger;
    
    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }
    
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.ContainsKey(eventType))
        {
            var handlers = _handlers[eventType].ToList();
            
            foreach (var handler in handlers)
            {
                try
                {
                    await handler(@event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                    // 继续处理其他处理器
                }
            }
        }
        
        _logger.LogDebug("Event {EventType} published to {HandlerCount} handlers", 
            eventType.Name, _handlers.ContainsKey(eventType) ? _handlers[eventType].Count : 0);
    }
    
    public Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Func<object, Task>>();
        }
        
        _handlers[eventType].Add(e => handler((TEvent)e));
        
        _logger.LogDebug("Handler subscribed to event {EventType}", eventType.Name);
        
        return Task.CompletedTask;
    }
    
    public Task UnsubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.ContainsKey(eventType))
        {
            var handlers = _handlers[eventType];
            handlers.RemoveAll(h => h.Target == handler.Target);
            
            if (handlers.Count == 0)
            {
                _handlers.Remove(eventType);
            }
        }
        
        _logger.LogDebug("Handler unsubscribed from event {EventType}", eventType.Name);
        
        return Task.CompletedTask;
    }
}
```

## 6. 缓存和性能优化

### 6.1 Redis 缓存选择

#### 6.1.1 技术选型
**选择理由**：
- ✅ **高性能**：内存数据库，读写性能极高
- ✅ **丰富的数据结构**：支持String、Hash、List、Set等
- ✅ **持久化**：支持数据持久化
- ✅ **集群支持**：支持分布式集群
- ✅ **过期策略**：支持多种过期策略

**应用场景**：
- 图像识别结果缓存
- 用户会话缓存
- 配置信息缓存
- 频繁查询的数据缓存
- 速率限制计数器

#### 6.1.2 技术实现
```csharp
// Redis 缓存服务
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    
    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }
    
    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache value for key {Key}", key);
            return default(T);
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache value for key {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache value for key {Key}", key);
        }
    }
    
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check cache existence for key {Key}", key);
            return false;
        }
    }
}
```

### 6.2 内存缓存

#### 6.2.1 Microsoft.Extensions.Caching.Memory
```csharp
// 内存缓存服务
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    
    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public Task<T> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult(default(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory cache value for key {Key}", key);
            return Task.FromResult(default(T));
        }
    }
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry.Value;
            }
            
            _cache.Set(key, value, options);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set memory cache value for key {Key}", key);
            return Task.CompletedTask;
        }
    }
    
    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove memory cache value for key {Key}", key);
            return Task.CompletedTask;
        }
    }
    
    public Task<bool> ExistsAsync(string key)
    {
        try
        {
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check memory cache existence for key {Key}", key);
            return Task.FromResult(false);
        }
    }
}
```

## 7. 日志和监控

### 7.1 Serilog 日志框架

#### 7.1.1 技术选型
**选择理由**：
- ✅ **结构化日志**：支持JSON格式的结构化日志
- ✅ **丰富的接收器**：支持多种日志输出目标
- ✅ **高性能**：异步日志记录，低性能开销
- ✅ **配置灵活**：支持复杂的日志配置
- ✅ **生态丰富**：大量的扩展和集成

**配置示例**：
```csharp
// Serilog 配置
public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "KeyForge")
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3}] [{@SourceContext}] {@m}\n{@x}"))
            .WriteTo.File(
                "logs/keyforge-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
            .WriteTo.SQLite(
                configuration.GetConnectionString("DefaultConnection"),
                tableName: "LogEntries")
            .CreateLogger();
        
        services.AddSerilog();
    }
}
```

### 7.2 Application Insights 监控

#### 7.2.1 技术选型
**选择理由**：
- ✅ **全面监控**：提供应用程序性能监控
- ✅ **Azure集成**：与Azure生态系统深度集成
- ✅ **实时监控**：实时的性能指标和警报
- ✅ **分布式追踪**：支持分布式系统追踪
- ✅ **智能分析**：基于AI的异常检测和分析

## 8. 测试框架

### 8.1 单元测试框架

#### 8.1.1 xUnit
**选择理由**：
- ✅ **现代设计**：支持.NET Core的现代测试框架
- ✅ **并行执行**：支持并行测试执行
- ✅ **数据驱动**：支持数据驱动测试
- ✅ **社区活跃**：活跃的开发社区
- ✅ **IDE集成**：与Visual Studio和Rider良好集成

#### 8.1.2 Moq 模拟框架
```csharp
// Moq 使用示例
public class ScriptExecutionServiceTests
{
    private readonly Mock<IScriptRepository> _scriptRepositoryMock;
    private readonly Mock<IActionExecutionService> _actionExecutionServiceMock;
    private readonly Mock<ILogger<ScriptExecutionService>> _loggerMock;
    private readonly ScriptExecutionService _service;
    
    public ScriptExecutionServiceTests()
    {
        _scriptRepositoryMock = new Mock<IScriptRepository>();
        _actionExecutionServiceMock = new Mock<IActionExecutionService>();
        _loggerMock = new Mock<ILogger<ScriptExecutionService>>();
        
        _service = new ScriptExecutionService(
            _scriptRepositoryMock.Object,
            _actionExecutionServiceMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task ExecuteScriptAsync_WithValidScript_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scriptId = ScriptId.New();
        var script = new Script(
            new ScriptName("Test Script"),
            new ScriptDescription("Test Description"),
            ScriptVersion.Initial);
        
        _scriptRepositoryMock
            .Setup(x => x.GetByIdAsync(scriptId))
            .ReturnsAsync(script);
        
        _actionExecutionServiceMock
            .Setup(x => x.ExecuteSequenceAsync(It.IsAny<ActionSequence>()))
            .ReturnsAsync(SequenceExecutionResult.Success());
        
        // Act
        var result = await _service.ExecuteScriptAsync(
            new ScriptExecutionRequest(scriptId, ExecutionParameters.Default));
        
        // Assert
        Assert.True(result.IsSuccess);
        _scriptRepositoryMock.Verify(x => x.GetByIdAsync(scriptId), Times.Once);
        _actionExecutionServiceMock.Verify(x => x.ExecuteSequenceAsync(It.IsAny<ActionSequence>()), Times.Once);
    }
}
```

## 9. 部署和容器化

### 9.1 Docker 容器化

#### 9.1.1 Dockerfile 配置
```dockerfile
# 基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 构建镜像
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KeyForge.Api/KeyForge.Api.csproj", "KeyForge.Api/"]
COPY ["KeyForge.Core/KeyForge.Core.csproj", "KeyForge.Core/"]
COPY ["KeyForge.Infrastructure/KeyForge.Infrastructure.csproj", "KeyForge.Infrastructure/"]
RUN dotnet restore "KeyForge.Api/KeyForge.Api.csproj"
COPY . .
WORKDIR "/src/KeyForge.Api"
RUN dotnet build "KeyForge.Api.csproj" -c Release -o /app/build

# 发布镜像
FROM build AS publish
RUN dotnet publish "KeyForge.Api.csproj" -c Release -o /app/publish

# 最终镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 安装OpenCV依赖
RUN apt-get update && apt-get install -y \
    libopencv-dev \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "KeyForge.Api.dll"]
```

#### 9.1.2 Docker Compose 配置
```yaml
version: '3.8'

services:
  keyforge-api:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/keyforge.db
      - RabbitMq__HostName=rabbitmq
      - RabbitMq__UserName=guest
      - RabbitMq__Password=guest
      - Redis__HostName=redis
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    depends_on:
      - rabbitmq
      - redis
    restart: unless-stopped

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    restart: unless-stopped

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    restart: unless-stopped

  keyforge-desktop:
    build:
      context: .
      dockerfile: KeyForge.Desktop/Dockerfile
    environment:
      - DOTNET_ENVIRONMENT=Production
    volumes:
      - ./desktop-data:/app/data
    restart: unless-stopped

volumes:
  rabbitmq_data:
  redis_data:
```

## 10. 开发工具和环境

### 10.1 开发工具

#### 10.1.1 Visual Studio 2022
**选择理由**：
- ✅ **完整功能**：提供完整的.NET开发功能
- ✅ **调试器**：强大的调试功能
- ✅ **IntelliSense**：智能代码补全
- ✅ **集成测试**：内置测试工具
- ✅ **扩展生态**：丰富的扩展生态系统

#### 10.1.2 JetBrains Rider
**选择理由**：
- ✅ **跨平台**：支持Windows、Linux、macOS
- ✅ **性能优异**：快速的响应和加载速度
- ✅ **重构工具**：强大的代码重构功能
- ✅ **Git集成**：优秀的Git集成
- ✅ **跨平台开发**：支持多平台开发

### 10.2 版本控制

#### 10.2.1 Git 工作流
```bash
# Git 工作流示例
# 功能开发分支
git checkout -b feature/script-recording master

# 开发完成后创建Pull Request
git add .
git commit -m "feat: add script recording functionality"
git push origin feature/script-recording

# 代码审查后合并到主分支
git checkout master
git merge --no-ff feature/script-recording
git push origin master

# 删除功能分支
git branch -d feature/script-recording
git push origin --delete feature/script-recording
```

## 11. 技术栈总结

### 11.1 核心技术栈

| 技术类别 | 选择技术 | 版本 | 选择理由 |
|---------|---------|------|---------|
| **运行时** | .NET 6.0 | 6.0 LTS | 长期支持，性能优异 |
| **Web框架** | ASP.NET Core | 6.0 | 高性能，跨平台 |
| **桌面框架** | WPF | .NET 6.0 | 原生Windows支持 |
| **ORM框架** | Entity Framework Core | 6.0 | 成熟稳定，LINQ支持 |
| **图像处理** | OpenCVSharp | 4.x | 功能完整，性能优异 |
| **数据库** | SQLite | 3.x | 轻量级，嵌入式 |
| **消息队列** | RabbitMQ | 3.x | 企业级，可靠性高 |
| **缓存** | Redis | 6.x | 高性能，数据结构丰富 |
| **日志框架** | Serilog | 2.x | 结构化日志，配置灵活 |
| **测试框架** | xUnit | 2.x | 现代，并行执行 |
| **模拟框架** | Moq | 4.x | 易用，功能完整 |

### 11.2 开发工具

| 工具类别 | 选择工具 | 选择理由 |
|---------|---------|---------|
| **IDE** | Visual Studio 2022 | 完整功能，调试强大 |
| **备用IDE** | JetBrains Rider | 跨平台，性能优异 |
| **版本控制** | Git | 分布式，生态完善 |
| **容器化** | Docker | 标准化，易于部署 |
| **CI/CD** | GitHub Actions | 集成度高，易用 |
| **API文档** | Swagger/OpenAPI | 标准化，自动生成 |

### 11.3 架构优势

#### 11.3.1 技术优势
- **现代化技术栈**：基于.NET 6.0+的最新技术
- **跨平台支持**：支持Windows、Linux、macOS
- **高性能**：优化的性能和资源利用
- **可扩展性**：模块化设计，易于扩展
- **容器化**：支持Docker容器化部署

#### 11.3.2 开发优势
- **开发效率**：丰富的工具和框架支持
- **代码质量**：强类型和编译时检查
- **测试友好**：完整的测试框架支持
- **维护性**：清晰的架构和代码组织
- **社区支持**：活跃的开发社区和文档

### 11.4 风险评估

#### 11.4.1 技术风险
- **学习曲线**：新技术栈的学习成本
- **依赖管理**：第三方库的依赖风险
- **性能调优**：图像识别性能优化挑战
- **兼容性**：不同环境的兼容性问题

#### 11.4.2 缓解策略
- **技术培训**：提供技术培训和学习资源
- **依赖管理**：定期更新依赖库，使用稳定版本
- **性能测试**：进行充分的性能测试和优化
- **环境标准化**：使用容器化确保环境一致性

## 12. 结论

KeyForge系统的技术栈选择充分考虑了系统的功能需求、性能要求、可维护性和可扩展性。通过选择成熟、稳定的技术组合，确保系统的可靠性和开发效率。

### 12.1 技术亮点
- **现代化架构**：基于.NET 6.0+的现代化技术栈
- **高性能图像处理**：OpenCVSharp提供强大的图像处理能力
- **企业级消息队列**：RabbitMQ确保消息传递的可靠性
- **结构化日志**：Serilog提供完整的日志管理
- **容器化部署**：Docker支持标准化部署

### 12.2 实施建议
- **分阶段实施**：按照优先级分阶段实施功能
- **持续集成**：建立CI/CD流水线
- **监控告警**：建立完整的监控和告警系统
- **文档完善**：完善的技术文档和用户文档
- **团队培训**：团队技术培训和知识共享

这个技术栈选择为KeyForge系统提供了坚实的技术基础，能够满足当前的功能需求，并支持未来的功能扩展和技术演进。