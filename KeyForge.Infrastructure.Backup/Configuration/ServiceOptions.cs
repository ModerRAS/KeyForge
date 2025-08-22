namespace KeyForge.Infrastructure.Configuration
{
    /// <summary>
    /// 缓存选项配置
    /// 
    /// 原本实现：复杂的缓存配置和多种缓存策略
    /// 简化实现：基本配置选项
    /// </summary>
    public class CacheOptions
    {
        public bool EnableMemoryCache { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
        public string? RedisConnectionString { get; set; }
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int MaxCacheSize { get; set; } = 1000;
    }

    /// <summary>
    /// 文件存储选项配置
    /// 
    /// 原本实现：支持多种存储后端的复杂配置
    /// 简化实现：基本文件系统配置
    /// </summary>
    public class FileStorageOptions
    {
        public string BasePath { get; set; } = "storage";
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB
        public List<string> AllowedExtensions { get; set; } = new();
        public bool EnableCompression { get; set; } = false;
        public bool EnableEncryption { get; set; } = false;
    }

    /// <summary>
    /// 性能监控选项配置
    /// 
    /// 原本实现：复杂的监控配置和告警规则
    /// 简化实现：基本监控选项
    /// </summary>
    public class PerformanceOptions
    {
        public bool EnableMonitoring { get; set; } = true;
        public int CollectionIntervalSeconds { get; set; } = 30;
        public bool EnableDetailedMetrics { get; set; } = false;
        public bool EnableAlerts { get; set; } = false;
        public double CpuThreshold { get; set; } = 80.0;
        public double MemoryThreshold { get; set; } = 80.0;
    }

    /// <summary>
    /// 安全选项配置
    /// 
    /// 原本实现：复杂的安全配置和策略
    /// 简化实现：基本安全选项
    /// </summary>
    public class SecurityOptions
    {
        public bool EnableEncryption { get; set; } = true;
        public bool EnableValidation { get; set; } = true;
        public string? EncryptionKey { get; set; }
        public bool EnableLogging { get; set; } = true;
        public bool EnableAuditing { get; set; } = false;
    }
}