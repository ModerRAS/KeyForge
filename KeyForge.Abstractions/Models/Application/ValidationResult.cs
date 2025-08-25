namespace KeyForge.Abstractions.Models.Application
{
    /// <summary>
    /// 验证结果
    /// 【优化实现】定义统一的验证结果模型，支持复杂的验证场景
    /// 原实现：验证逻辑分散，缺乏统一结果格式
    /// 优化：通过统一的验证结果模型，提高代码规范性
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否验证成功
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// 错误消息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();
        
        /// <summary>
        /// 警告消息列表
        /// </summary>
        public List<string> Warnings { get; set; } = new();
        
        /// <summary>
        /// 验证详情
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new();
        
        /// <summary>
        /// 验证时间
        /// </summary>
        public DateTime ValidationTime { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ValidationResult Failure(string error)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = new List<string> { error } 
            };
        }
        
        /// <summary>
        /// 创建失败结果（多个错误）
        /// </summary>
        public static ValidationResult Failure(List<string> errors)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = errors 
            };
        }
        
        /// <summary>
        /// 添加错误
        /// </summary>
        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }
        
        /// <summary>
        /// 添加警告
        /// </summary>
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
        
        /// <summary>
        /// 添加详情
        /// </summary>
        public void AddDetail(string key, object value)
        {
            Details[key] = value;
        }
    }
}