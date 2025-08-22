using System;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Exceptions
{
    /// <summary>
    /// HAL基异常类
    /// </summary>
    public class HALException : Exception
    {
        /// <summary>
        /// 硬件操作类型
        /// </summary>
        public HardwareOperation Operation { get; }
        
        /// <summary>
        /// 平台类型
        /// </summary>
        public Platform Platform { get; }
        
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorCode { get; }
        
        /// <summary>
        /// 初始化HAL异常
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="operation">硬件操作类型</param>
        /// <param name="platform">平台类型</param>
        /// <param name="errorCode">错误代码</param>
        public HALException(string message, HardwareOperation operation = HardwareOperation.Unknown, Platform platform = Platform.Unknown, int errorCode = 0)
            : base(message)
        {
            Operation = operation;
            Platform = platform;
            ErrorCode = errorCode;
        }
        
        /// <summary>
        /// 初始化HAL异常
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        /// <param name="operation">硬件操作类型</param>
        /// <param name="platform">平台类型</param>
        /// <param name="errorCode">错误代码</param>
        public HALException(string message, Exception innerException, HardwareOperation operation = HardwareOperation.Unknown, Platform platform = Platform.Unknown, int errorCode = 0)
            : base(message, innerException)
        {
            Operation = operation;
            Platform = platform;
            ErrorCode = errorCode;
        }
        
        /// <summary>
        /// 字符串表示
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return $"{base.ToString()} [Platform: {Platform}, Operation: {Operation}, ErrorCode: {ErrorCode}]";
        }
    }

    /// <summary>
    /// 平台不支持异常
    /// </summary>
    public class PlatformNotSupportedException : HALException
    {
        /// <summary>
        /// 不支持的平台
        /// </summary>
        public new Platform Platform { get; }
        
        /// <summary>
        /// 初始化平台不支持异常
        /// </summary>
        /// <param name="platform">不支持的平台</param>
        /// <param name="message">错误消息</param>
        public PlatformNotSupportedException(Platform platform, string message)
            : base(message, HardwareOperation.Unknown, platform)
        {
            Platform = platform;
        }
        
        /// <summary>
        /// 初始化平台不支持异常
        /// </summary>
        /// <param name="platform">不支持的平台</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public PlatformNotSupportedException(Platform platform, string message, Exception innerException)
            : base(message, innerException, HardwareOperation.Unknown, platform)
        {
            Platform = platform;
        }
    }

    /// <summary>
    /// 权限被拒绝异常
    /// </summary>
    public class PermissionDeniedException : HALException
    {
        /// <summary>
        /// 被拒绝的权限
        /// </summary>
        public string Permission { get; }
        
        /// <summary>
        /// 权限状态
        /// </summary>
        public PermissionStatus Status { get; }
        
        /// <summary>
        /// 初始化权限被拒绝异常
        /// </summary>
        /// <param name="permission">被拒绝的权限</param>
        /// <param name="status">权限状态</param>
        /// <param name="message">错误消息</param>
        public PermissionDeniedException(string permission, PermissionStatus status, string message)
            : base(message, HardwareOperation.Unknown)
        {
            Permission = permission;
            Status = status;
        }
        
        /// <summary>
        /// 初始化权限被拒绝异常
        /// </summary>
        /// <param name="permission">被拒绝的权限</param>
        /// <param name="status">权限状态</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public PermissionDeniedException(string permission, PermissionStatus status, string message, Exception innerException)
            : base(message, innerException, HardwareOperation.Unknown)
        {
            Permission = permission;
            Status = status;
        }
    }

    /// <summary>
    /// 硬件操作异常
    /// </summary>
    public class HardwareOperationException : HALException
    {
        /// <summary>
        /// 硬件操作类型
        /// </summary>
        public new HardwareOperation Operation { get; }
        
        /// <summary>
        /// 操作参数
        /// </summary>
        public object Parameters { get; }
        
        /// <summary>
        /// 是否可重试
        /// </summary>
        public bool CanRetry { get; }
        
        /// <summary>
        /// 初始化硬件操作异常
        /// </summary>
        /// <param name="operation">硬件操作类型</param>
        /// <param name="message">错误消息</param>
        /// <param name="parameters">操作参数</param>
        /// <param name="canRetry">是否可重试</param>
        public HardwareOperationException(HardwareOperation operation, string message, object parameters = null, bool canRetry = false)
            : base(message, operation)
        {
            Operation = operation;
            Parameters = parameters;
            CanRetry = canRetry;
        }
        
        /// <summary>
        /// 初始化硬件操作异常
        /// </summary>
        /// <param name="operation">硬件操作类型</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        /// <param name="parameters">操作参数</param>
        /// <param name="canRetry">是否可重试</param>
        public HardwareOperationException(HardwareOperation operation, string message, Exception innerException, object parameters = null, bool canRetry = false)
            : base(message, innerException, operation)
        {
            Operation = operation;
            Parameters = parameters;
            CanRetry = canRetry;
        }
    }

    /// <summary>
    /// 初始化异常
    /// </summary>
    public class HALInitializationException : HALException
    {
        /// <summary>
        /// 初始化阶段
        /// </summary>
        public InitializationStage Stage { get; }
        
        /// <summary>
        /// 初始化失败的组件
        /// </summary>
        public string Component { get; }
        
        /// <summary>
        /// 初始化HAL初始化异常
        /// </summary>
        /// <param name="stage">初始化阶段</param>
        /// <param name="component">失败的组件</param>
        /// <param name="message">错误消息</param>
        public HALInitializationException(InitializationStage stage, string component, string message)
            : base(message, HardwareOperation.Unknown)
        {
            Stage = stage;
            Component = component;
        }
        
        /// <summary>
        /// 初始化HAL初始化异常
        /// </summary>
        /// <param name="stage">初始化阶段</param>
        /// <param name="component">失败的组件</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public HALInitializationException(InitializationStage stage, string component, string message, Exception innerException)
            : base(message, innerException, HardwareOperation.Unknown)
        {
            Stage = stage;
            Component = component;
        }
    }

    /// <summary>
    /// 超时异常
    /// </summary>
    public class HALTimeoutException : HALException
    {
        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; }
        
        /// <summary>
        /// 操作开始时间
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        /// 初始化HAL超时异常
        /// </summary>
        /// <param name="timeoutMs">超时时间</param>
        /// <param name="startTime">操作开始时间</param>
        /// <param name="message">错误消息</param>
        public HALTimeoutException(int timeoutMs, DateTime startTime, string message)
            : base(message, HardwareOperation.Unknown)
        {
            TimeoutMs = timeoutMs;
            StartTime = startTime;
        }
    }

    /// <summary>
    /// 资源不可用异常
    /// </summary>
    public class HALResourceUnavailableException : HALException
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public string ResourceType { get; }
        
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName { get; }
        
        /// <summary>
        /// 初始化HAL资源不可用异常
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="message">错误消息</param>
        public HALResourceUnavailableException(string resourceType, string resourceName, string message)
            : base(message, HardwareOperation.Unknown)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }
        
        /// <summary>
        /// 初始化HAL资源不可用异常
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerException">内部异常</param>
        public HALResourceUnavailableException(string resourceType, string resourceName, string message, Exception innerException)
            : base(message, innerException, HardwareOperation.Unknown)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }
    }

    /// <summary>
    /// 配置异常
    /// </summary>
    public class HALConfigurationException : HALException
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string ConfigurationKey { get; }
        
        /// <summary>
        /// 配置值
        /// </summary>
        public object ConfigurationValue { get; }
        
        /// <summary>
        /// 初始化HAL配置异常
        /// </summary>
        /// <param name="configurationKey">配置键</param>
        /// <param name="configurationValue">配置值</param>
        /// <param name="message">错误消息</param>
        public HALConfigurationException(string configurationKey, object configurationValue, string message)
            : base(message, HardwareOperation.Unknown)
        {
            ConfigurationKey = configurationKey;
            ConfigurationValue = configurationValue;
        }
    }

    /// <summary>
    /// 初始化阶段枚举
    /// </summary>
    public enum InitializationStage
    {
        /// <summary>
        /// 平台检测
        /// </summary>
        PlatformDetection,
        
        /// <summary>
        /// 权限检查
        /// </summary>
        PermissionCheck,
        
        /// <summary>
        /// 服务初始化
        /// </summary>
        ServiceInitialization,
        
        /// <summary>
        /// 事件注册
        /// </summary>
        EventRegistration,
        
        /// <summary>
        /// 资源分配
        /// </summary>
        ResourceAllocation,
        
        /// <summary>
        /// 验证
        /// </summary>
        Validation
    }
}