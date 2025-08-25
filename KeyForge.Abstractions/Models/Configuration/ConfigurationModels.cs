namespace KeyForge.Abstractions.Models.Configuration
{
    /// <summary>
    /// 配置变化事件参数
    /// 【优化实现】定义统一的配置变化事件参数
    /// 原实现：配置事件参数定义分散，缺乏统一规范
    /// 优化：通过统一的事件参数模型，提高代码规范性
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变化的配置键
        /// </summary>
        public string Key { get; set; } = string.Empty;
        
        /// <summary>
        /// 旧值
        /// </summary>
        public object OldValue { get; set; }
        
        /// <summary>
        /// 新值
        /// </summary>
        public object NewValue { get; set; }
        
        /// <summary>
        /// 变化时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 变化来源
        /// </summary>
        public string Source { get; set; } = "Unknown";
    }
    
    /// <summary>
    /// 系统信息
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 操作系统名称
        /// </summary>
        public string OSName { get; set; } = string.Empty;
        
        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OSVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// 架构
        /// </summary>
        public string Architecture { get; set; } = string.Empty;
        
        /// <summary>
        /// 处理器数量
        /// </summary>
        public int ProcessorCount { get; set; }
        
        /// <summary>
        /// 内存大小
        /// </summary>
        public long TotalMemory { get; set; }
        
        /// <summary>
        /// 可用内存
        /// </summary>
        public long AvailableMemory { get; set; }
        
        /// <summary>
        /// 机器名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 系统资源
    /// </summary>
    public class SystemResources
    {
        /// <summary>
        /// CPU使用率
        /// </summary>
        public float CPUUsage { get; set; }
        
        /// <summary>
        /// 内存使用率
        /// </summary>
        public float MemoryUsage { get; set; }
        
        /// <summary>
        /// 磁盘使用率
        /// </summary>
        public float DiskUsage { get; set; }
        
        /// <summary>
        /// 网络使用率
        /// </summary>
        public float NetworkUsage { get; set; }
        
        /// <summary>
        /// GPU使用率
        /// </summary>
        public float GPUUsage { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 配置架构
    /// </summary>
    public class ConfigurationSchema
    {
        /// <summary>
        /// 架构名称
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;
        
        /// <summary>
        /// 架构版本
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// 配置定义
        /// </summary>
        public Dictionary<string, ConfigurationProperty> Properties { get; set; } = new();
        
        /// <summary>
        /// 验证规则
        /// </summary>
        public Dictionary<string, string> ValidationRules { get; set; } = new();
    }
    
    /// <summary>
    /// 配置属性
    /// </summary>
    public class ConfigurationProperty
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 属性类型
        /// </summary>
        public string Type { get; set; } = "string";
        
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// 是否必需
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 验证规则
        /// </summary>
        public string ValidationRule { get; set; } = string.Empty;
    }
}