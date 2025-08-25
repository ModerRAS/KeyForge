namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 配置系统请求基类
    /// 【优化实现】定义配置系统请求的基类，统一请求处理
    /// 原实现：请求类型分散定义，缺乏统一规范
    /// 优化：通过基类统一请求处理，提高代码规范性
    /// </summary>
    public abstract class ConfigurationRequestBase
    {
        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 配置范围
        /// </summary>
        public string ConfigurationScope { get; set; } = "Application";
        
        /// <summary>
        /// 配置版本
        /// </summary>
        public string ConfigurationVersion { get; set; } = "1.0";
    }
    
    /// <summary>
    /// 获取配置请求
    /// </summary>
    public class GetConfigurationRequest : ConfigurationRequestBase
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string ConfigurationKey { get; set; }
        
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// 是否包含敏感信息
        /// </summary>
        public bool IncludeSensitive { get; set; } = false;
    }
    
    /// <summary>
    /// 设置配置请求
    /// </summary>
    public class SetConfigurationRequest : ConfigurationRequestBase
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string ConfigurationKey { get; set; }
        
        /// <summary>
        /// 配置值
        /// </summary>
        public object ConfigurationValue { get; set; }
        
        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Persist { get; set; } = true;
        
        /// <summary>
        /// 是否验证
        /// </summary>
        public bool Validate { get; set; } = true;
    }
    
    /// <summary>
    /// 验证配置请求
    /// </summary>
    public class ValidateConfigurationRequest : ConfigurationRequestBase
    {
        /// <summary>
        /// 配置数据
        /// </summary>
        public Dictionary<string, object> ConfigurationData { get; set; } = new();
        
        /// <summary>
        /// 验证规则
        /// </summary>
        public Dictionary<string, string> ValidationRules { get; set; } = new();
        
        /// <summary>
        /// 验证模式
        /// </summary>
        public string ValidationMode { get; set; } = "Strict";
    }
    
    /// <summary>
    /// 重置配置请求
    /// </summary>
    public class ResetConfigurationRequest : ConfigurationRequestBase
    {
        /// <summary>
        /// 重置范围
        /// </summary>
        public string ResetScope { get; set; } = "All";
        
        /// <summary>
        /// 备份当前配置
        /// </summary>
        public bool BackupCurrent { get; set; } = true;
        
        /// <summary>
        /// 重置原因
        /// </summary>
        public string ResetReason { get; set; } = "UserRequest";
    }
    
    /// <summary>
    /// 导入导出配置请求
    /// </summary>
    public class ImportExportConfigurationRequest : ConfigurationRequestBase
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; } = "Export";
        
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// 导入导出范围
        /// </summary>
        public string Scope { get; set; } = "All";
        
        /// <summary>
        /// 格式类型
        /// </summary>
        public string Format { get; set; } = "JSON";
        
        /// <summary>
        /// 是否包含敏感信息
        /// </summary>
        public bool IncludeSensitive { get; set; } = false;
        
        /// <summary>
        /// 加密密码
        /// </summary>
        public string EncryptionPassword { get; set; } = string.Empty;
    }
}