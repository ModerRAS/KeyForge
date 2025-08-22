using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using KeyForge.Domain.Interfaces;
using KeyForge.Infrastructure.Data;
using KeyForge.Infrastructure.Repositories;
using KeyForge.Infrastructure.Services;
// using KeyForge.Infrastructure.External; // 简化实现：移除External引用
using KeyForge.Infrastructure.Persistence;
using System;
using System.IO;
using Microsoft.Extensions.Options;

namespace KeyForge.Infrastructure.DependencyInjection
{
    /// <summary>
    /// 服务注册扩展类
    /// 这是简化实现，原本在多个地方都有服务注册逻辑
    /// 现在统一在Infrastructure层定义，确保依赖注入配置一致性
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// 注册基础设施层服务
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册配置
            services.AddSingleton(configuration);
            
            // 注册数据库上下文
            services.AddDbContext<KeyForgeDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection") 
                    ?? "Data Source=keyforge.db";
                options.UseSqlite(connectionString);
                
                // 开发环境启用详细日志
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
            
            // 注册仓储 - 简化实现：只注册有实现的仓储
            services.AddScoped<IScriptRepository, ScriptRepository>();
            services.AddScoped<IImageTemplateRepository, ImageTemplateRepository>();
            services.AddScoped<IDecisionRuleRepository, DecisionRuleRepository>();
            // services.AddScoped<IStateMachineRepository, StateMachineRepository>(); // 暂时注释，实现不存在
            // services.AddScoped<IGameActionRepository, GameActionRepository>(); // 暂时注释，实现不存在
            // services.AddScoped<IStateRepository, StateRepository>(); // 暂时注释，实现不存在
            
            // 注册领域服务实现 - 简化实现：暂时注释掉不存在的服务
            // services.AddScoped<IScriptPlayerService, ScriptPlayerService>();
            // services.AddScoped<IImageRecognitionService, ImageRecognitionService>();
            // services.AddScoped<IInputSimulationService, InputSimulationService>();
            // services.AddScoped<IStateMachineService, StateMachineService>();
            // services.AddScoped<IDecisionEngineService, DecisionEngineService>();
            
            // 注册基础设施服务 - 简化实现：暂时注释掉不存在的服务
            // services.AddScoped<IFileStorageService, FileStorageService>();
            // services.AddScoped<ILoggingService, LoggingService>();
            // services.AddScoped<IConfigurationService, ConfigurationService>();
            // services.AddScoped<ICacheRepository, CacheRepository>();
            // services.AddScoped<IFileRepository, FileRepository>();
            // services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            // services.AddScoped<ILogRepository, LogRepository>();
            
            // 注册外部服务 - 简化实现：暂时注释掉不存在的服务
            // services.AddScoped<IHotkeyManagerService, HotkeyManagerService>();
            // services.AddScoped<IPerformanceService, PerformanceService>();
            
            // 注册基础服务 - 只有这些服务有实现
            services.AddScoped<IErrorHandlerService, ErrorHandlerService>();
            services.AddScoped<ISerializationService, SerializationService>();
            
            // 注册工作单元
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            return services;
        }

        /// <summary>
        /// 注册应用层服务
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 简化实现：暂时注释掉不存在的应用服务引用
            // services.AddScoped<IScriptApplicationService, ScriptApplicationService>();
            // services.AddScoped<IStateMachineApplicationService, StateMachineApplicationService>();
            // services.AddScoped<IImageTemplateApplicationService, ImageTemplateApplicationService>();
            
            // 暂时注释掉MediatR，因为项目中没有使用
            // services.AddMediatR(cfg => 
            // {
            //     cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly);
            // });
            
            return services;
        }

        /// <summary>
        /// 配置日志服务
        /// </summary>
        public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/keyforge-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
            
            return services;
        }

        /// <summary>
        /// 配置开发环境服务
        /// </summary>
        public static IServiceCollection AddDevelopmentServices(this IServiceCollection services)
        {
            // 开发环境特定的服务配置
            services.AddDbContext<KeyForgeDbContext>(options =>
            {
                options.UseSqlite("Data Source=keyforge-dev.db");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
            
            return services;
        }

        /// <summary>
        /// 配置生产环境服务
        /// </summary>
        public static IServiceCollection AddProductionServices(this IServiceCollection services)
        {
            // 生产环境特定的服务配置
            services.AddDbContext<KeyForgeDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Database connection string not configured for production");
                }
                options.UseSqlite(connectionString);
            });
            
            return services;
        }

        /// <summary>
        /// 配置测试环境服务
        /// </summary>
        public static IServiceCollection AddTestServices(this IServiceCollection services)
        {
            // 测试环境特定的服务配置
            services.AddDbContext<KeyForgeDbContext>(options =>
            {
                // 简化实现：暂时注释掉InMemoryDatabase，因为没有EntityFrameworkCore.InMemory包
                // options.UseInMemoryDatabase("KeyForgeTestDb");
                options.UseSqlite("Data Source=keyforge-test.db");
            });
            
            // 简化实现：使用完全限定名避免接口引用冲突
            services.AddScoped<KeyForge.Domain.Interfaces.IScriptRepository, KeyForge.Infrastructure.Repositories.ScriptRepository>();
            services.AddScoped<KeyForge.Domain.Interfaces.IImageTemplateRepository, KeyForge.Infrastructure.Repositories.ImageTemplateRepository>();
            // StateMachineRepository暂未实现，注释掉以避免编译错误
            // services.AddScoped<KeyForge.Domain.Interfaces.IStateMachineRepository, KeyForge.Infrastructure.Repositories.StateMachineRepository>();
            
            return services;
        }

        /// <summary>
        /// 根据环境自动配置服务
        /// </summary>
        public static IServiceCollection AddEnvironmentServices(this IServiceCollection services, IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            switch (environment.ToLower())
            {
                case "development":
                    services.AddDevelopmentServices();
                    break;
                case "production":
                    services.AddProductionServices();
                    break;
                case "test":
                    services.AddTestServices();
                    break;
                default:
                    services.AddDevelopmentServices();
                    break;
            }
            
            return services;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
            await context.Database.MigrateAsync();
            
            // 初始化基础数据
            if (!await context.Scripts.AnyAsync())
            {
                // 添加示例数据
                await SeedDataAsync(context);
            }
        }

        /// <summary>
        /// 种子数据
        /// </summary>
        private static async Task SeedDataAsync(KeyForgeDbContext context)
        {
            // 这里可以添加初始数据
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 数据库配置选项
    /// </summary>
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; } = "Data Source=keyforge.db";
        public bool EnableSensitiveDataLogging { get; set; }
        public bool EnableDetailedErrors { get; set; }
        public int CommandTimeout { get; set; } = 30;
    }

    /// <summary>
    /// 文件存储配置选项
    /// </summary>
    public class FileStorageOptions
    {
        public string BasePath { get; set; } = "storage";
        public int MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public bool EnableCompression { get; set; } = false;
        public bool EnableEncryption { get; set; } = false;
        public string EncryptionKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// 日志配置选项
    /// </summary>
    public class LoggingOptions
    {
        public string LogLevel { get; set; } = "Information";
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "logs";
        public int RetentionDays { get; set; } = 30;
    }

    /// <summary>
    /// 缓存配置选项
    /// </summary>
    public class CacheOptions
    {
        public bool EnableMemoryCache { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
        public string RedisConnectionString { get; set; } = string.Empty;
        public int DefaultExpirationMinutes { get; set; } = 30;
    }

    /// <summary>
    /// 性能监控配置选项
    /// </summary>
    public class PerformanceOptions
    {
        public bool EnableMonitoring { get; set; } = true;
        public int MetricsRetentionDays { get; set; } = 7;
        public int SamplingIntervalSeconds { get; set; } = 60;
        public bool EnableAlerts { get; set; } = false;
        public double CpuThreshold { get; set; } = 80.0;
        public double MemoryThreshold { get; set; } = 80.0;
    }

    /// <summary>
    /// 安全配置选项
    /// </summary>
    public class SecurityOptions
    {
        public bool EnableEncryption { get; set; } = true;
        public string EncryptionKey { get; set; } = string.Empty;
        public bool EnableValidation { get; set; } = true;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
    }

    /// <summary>
    /// 应用服务配置扩展
    /// </summary>
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// 配置应用服务选项
        /// </summary>
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置数据库选项
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
            
            // 配置文件存储选项
            services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
            
            // 配置日志选项
            services.Configure<LoggingOptions>(configuration.GetSection("Logging"));
            
            // 配置缓存选项
            services.Configure<CacheOptions>(configuration.GetSection("Cache"));
            
            // 配置性能监控选项
            services.Configure<PerformanceOptions>(configuration.GetSection("Performance"));
            
            // 配置安全选项
            services.Configure<SecurityOptions>(configuration.GetSection("Security"));
            
            return services;
        }

        /// <summary>
        /// 注册验证服务
        /// 简化实现：暂时注释掉ValidationService，因为实现不存在
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            // services.AddScoped<IValidationService, ValidationService>();
            return services;
        }

        /// <summary>
        /// 注册缓存服务
        /// </summary>
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheOptions = configuration.GetSection("Cache").Get<CacheOptions>();
            
            if (cacheOptions?.EnableMemoryCache == true)
            {
                services.AddMemoryCache();
            }
            
            if (cacheOptions?.EnableDistributedCache == true && !string.IsNullOrEmpty(cacheOptions.RedisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheOptions.RedisConnectionString;
                    options.InstanceName = "KeyForge";
                });
            }
            
            return services;
        }

        /// <summary>
        /// 注册文件存储服务
        /// </summary>
        public static IServiceCollection AddFileStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            var fileOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>();
            var basePath = fileOptions?.BasePath ?? "storage";
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            
            // 简化实现：创建Services.FileStorageOptions实例
            var servicesFileOptions = new KeyForge.Infrastructure.Services.FileStorageOptions
            {
                MaxFileSize = fileOptions?.MaxFileSize ?? 10 * 1024 * 1024,
                AllowedExtensions = new HashSet<string>(fileOptions?.AllowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }),
                EnableCompression = fileOptions?.EnableCompression ?? false,
                EnableEncryption = fileOptions?.EnableEncryption ?? false,
                EncryptionKey = fileOptions?.EncryptionKey ?? string.Empty
            };
            
            services.AddScoped<IFileStorageService>(provider => 
                new FileStorageService(basePath, servicesFileOptions));
            
            return services;
        }

        /// <summary>
        /// 注册性能监控服务
        /// </summary>
        public static IServiceCollection AddPerformanceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var perfOptions = configuration.GetSection("Performance").Get<PerformanceOptions>();
            
            if (perfOptions?.EnableMonitoring == true)
            {
                services.AddScoped<IPerformanceMonitor, PerformanceMonitor>();
                services.AddScoped<IMetricsCollector, MetricsCollector>();
                services.AddHostedService<PerformanceMonitoringService>();
            }
            
            return services;
        }

        /// <summary>
        /// 注册安全服务
        /// </summary>
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var securityOptions = configuration.GetSection("Security").Get<SecurityOptions>();
            
            if (securityOptions?.EnableEncryption == true)
            {
                services.AddScoped<IEncryptionService, EncryptionService>();
            }
            
            if (securityOptions?.EnableValidation == true)
            {
                services.AddScoped<IInputValidationService, InputValidationService>();
            }
            
            return services;
        }
    }
}