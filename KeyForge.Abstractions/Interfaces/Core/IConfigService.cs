using KeyForge.Abstractions.Models.Configuration;
using KeyForge.Abstractions.Models.Core;
using KeyForge.Abstractions.Models.Application;
using Microsoft.Extensions.Configuration;

namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 配置服务基础接口
    /// 【优化实现】统一了配置管理系统的抽象接口，支持跨平台配置管理
    /// 原实现：配置管理分散在各层，缺乏统一抽象
    /// 优化：定义统一的配置服务接口，支持多种配置源和热重载
    /// </summary>
    public interface IConfigService : IDisposable
    {
        /// <summary>
        /// 初始化配置服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 服务状态
        /// </summary>
        ServiceStatus Status { get; }
        
        /// <summary>
        /// 配置变化事件
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> OnConfigurationChanged;
    }
    
    /// <summary>
    /// 应用配置服务接口
    /// </summary>
    public interface IAppConfigService : IConfigService
    {
        /// <summary>
        /// 获取配置值
        /// </summary>
        T? GetValue<T>(string key, T? defaultValue = default);
        
        /// <summary>
        /// 设置配置值
        /// </summary>
        Task<bool> SetValueAsync<T>(string key, T value);
        
        /// <summary>
        /// 获取配置节
        /// </summary>
        IConfigurationSection GetSection(string key);
        
        /// <summary>
        /// 检查配置键是否存在
        /// </summary>
        bool HasKey(string key);
        
        /// <summary>
        /// 删除配置键
        /// </summary>
        Task<bool> DeleteKeyAsync(string key);
        
        /// <summary>
        /// 获取所有配置键
        /// </summary>
        Task<List<string>> GetAllKeysAsync();
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        Task<bool> ReloadAsync();
        
        /// <summary>
        /// 保存配置到文件
        /// </summary>
        Task<bool> SaveAsync(string filePath);
        
        /// <summary>
        /// 从文件加载配置
        /// </summary>
        Task<bool> LoadAsync(string filePath);
    }
    
    /// <summary>
    /// 用户配置服务接口
    /// </summary>
    public interface IUserConfigService : IConfigService
    {
        /// <summary>
        /// 获取用户配置
        /// </summary>
        T? GetUserConfig<T>(string configName, T? defaultValue = default);
        
        /// <summary>
        /// 设置用户配置
        /// </summary>
        Task<bool> SetUserConfigAsync<T>(string configName, T value);
        
        /// <summary>
        /// 删除用户配置
        /// </summary>
        Task<bool> DeleteUserConfigAsync(string configName);
        
        /// <summary>
        /// 获取所有用户配置
        /// </summary>
        Task<Dictionary<string, object>> GetAllUserConfigsAsync();
        
        /// <summary>
        /// 重置用户配置
        /// </summary>
        Task<bool> ResetUserConfigsAsync();
        
        /// <summary>
        /// 导入用户配置
        /// </summary>
        Task<bool> ImportUserConfigsAsync(string filePath);
        
        /// <summary>
        /// 导出用户配置
        /// </summary>
        Task<bool> ExportUserConfigsAsync(string filePath);
    }
    
    /// <summary>
    /// 系统配置服务接口
    /// </summary>
    public interface ISystemConfigService : IConfigService
    {
        /// <summary>
        /// 获取系统配置
        /// </summary>
        T? GetSystemConfig<T>(string configName, T? defaultValue = default);
        
        /// <summary>
        /// 设置系统配置
        /// </summary>
        Task<bool> SetSystemConfigAsync<T>(string configName, T value);
        
        /// <summary>
        /// 获取系统环境变量
        /// </summary>
        string? GetEnvironmentVariable(string variableName);
        
        /// <summary>
        /// 设置系统环境变量
        /// </summary>
        Task<bool> SetEnvironmentVariableAsync(string variableName, string value);
        
        /// <summary>
        /// 获取系统信息
        /// </summary>
        Task<SystemInfo> GetSystemInfoAsync();
        
        /// <summary>
        /// 获取系统资源使用情况
        /// </summary>
        Task<SystemResources> GetSystemResourcesAsync();
    }
    
    /// <summary>
    /// 配置验证服务接口
    /// </summary>
    public interface IConfigValidationService : IConfigService
    {
        /// <summary>
        /// 验证配置
        /// </summary>
        Task<ValidationResult> ValidateConfigurationAsync(IConfiguration configuration);
        
        /// <summary>
        /// 验证配置节
        /// </summary>
        Task<ValidationResult> ValidateSectionAsync(IConfigurationSection section);
        
        /// <summary>
        /// 验证配置值
        /// </summary>
        Task<ValidationResult> ValidateValueAsync(string key, object value);
        
        /// <summary>
        /// 获取配置架构
        /// </summary>
        ConfigurationSchema GetConfigurationSchema();
        
        /// <summary>
        /// 生成默认配置
        /// </summary>
        Task<Configuration> GenerateDefaultConfigurationAsync();
    }
}