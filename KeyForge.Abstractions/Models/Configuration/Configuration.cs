namespace KeyForge.Abstractions.Models.Configuration
{
    /// <summary>
    /// 配置模型
    /// 【优化实现】定义统一的配置模型，支持多种配置源
    /// 原实现：配置模型分散定义，缺乏统一规范
    /// 优化：通过统一的配置模型，提高代码规范性
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string Key { get; set; } = string.Empty;
        
        /// <summary>
        /// 配置值
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// 配置类型
        /// </summary>
        public string Type { get; set; } = "string";
        
        /// <summary>
        /// 配置描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// 是否必需
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// 是否敏感信息
        /// </summary>
        public bool IsSensitive { get; set; }
        
        /// <summary>
        /// 验证规则
        /// </summary>
        public string ValidationRule { get; set; } = string.Empty;
        
        /// <summary>
        /// 配置来源
        /// </summary>
        public string Source { get; set; } = "Default";
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 是否已修改
        /// </summary>
        public bool IsModified { get; set; }
        
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// 创建配置
        /// </summary>
        public static Configuration Create(string key, object value, string type = "string")
        {
            return new Configuration
            {
                Key = key,
                Value = value,
                Type = type
            };
        }
        
        /// <summary>
        /// 获取字符串值
        /// </summary>
        public string GetStringValue()
        {
            return Value?.ToString() ?? string.Empty;
        }
        
        /// <summary>
        /// 获取整数值
        /// </summary>
        public int GetIntValue()
        {
            if (Value is int intValue)
                return intValue;
            
            if (int.TryParse(Value?.ToString(), out int result))
                return result;
            
            return 0;
        }
        
        /// <summary>
        /// 获取布尔值
        /// </summary>
        public bool GetBoolValue()
        {
            if (Value is bool boolValue)
                return boolValue;
            
            if (bool.TryParse(Value?.ToString(), out bool result))
                return result;
            
            return false;
        }
    }
}