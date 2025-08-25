using KeyForge.Abstractions.Models.Application;

namespace KeyForge.Abstractions.Interfaces.Application
{
    /// <summary>
    /// 用例基础接口
    /// 【优化实现】定义了应用层用例的统一接口，支持清晰的业务逻辑分层
    /// 原实现：业务逻辑直接分散在各层，缺乏统一抽象
    /// 优化：通过用例接口，实现清晰的业务逻辑分层
    /// </summary>
    public interface IUseCase
    {
        /// <summary>
        /// 执行用例
        /// </summary>
        Task<TResult> ExecuteAsync<TRequest, TResult>(TRequest request);
        
        /// <summary>
        /// 验证请求
        /// </summary>
        Task<ValidationResult> ValidateAsync<TRequest>(TRequest request);
        
        /// <summary>
        /// 检查权限
        /// </summary>
        Task<bool> CheckAuthorizationAsync<TRequest>(TRequest request);
        
        /// <summary>
        /// 用例名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 用例描述
        /// </summary>
        string Description { get; }
    }
    
    /// <summary>
    /// 输入系统用例接口
    /// </summary>
    public interface IInputUseCase : IUseCase
    {
        /// <summary>
        /// 开始输入监听
        /// </summary>
        Task<InputResult> StartListeningAsync(StartListeningRequest request);
        
        /// <summary>
        /// 停止输入监听
        /// </summary>
        Task<InputResult> StopListeningAsync(StopListeningRequest request);
        
        /// <summary>
        /// 发送输入事件
        /// </summary>
        Task<InputResult> SendInputAsync(SendInputRequest request);
        
        /// <summary>
        /// 注册热键
        /// </summary>
        Task<InputResult> RegisterHotkeyAsync(RegisterHotkeyRequest request);
        
        /// <summary>
        /// 注销热键
        /// </summary>
        Task<InputResult> UnregisterHotkeyAsync(UnregisterHotkeyRequest request);
    }
    
    /// <summary>
    /// 图像系统用例接口
    /// </summary>
    public interface IImageUseCase : IUseCase
    {
        /// <summary>
        /// 捕获屏幕
        /// </summary>
        Task<ImageResult> CaptureScreenAsync(CaptureScreenRequest request);
        
        /// <summary>
        /// 创建图像模板
        /// </summary>
        Task<ImageResult> CreateTemplateAsync(CreateTemplateRequest request);
        
        /// <summary>
        /// 识别图像
        /// </summary>
        Task<ImageResult> RecognizeImageAsync(RecognizeImageRequest request);
        
        /// <summary>
        /// 处理图像
        /// </summary>
        Task<ImageResult> ProcessImageAsync(ProcessImageRequest request);
        
        /// <summary>
        /// 分析屏幕
        /// </summary>
        Task<ImageResult> AnalyzeScreenAsync(AnalyzeScreenRequest request);
    }
    
    /// <summary>
    /// 脚本系统用例接口
    /// </summary>
    public interface IScriptUseCase : IUseCase
    {
        /// <summary>
        /// 开始录制脚本
        /// </summary>
        Task<ScriptResult> StartRecordingAsync(StartRecordingRequest request);
        
        /// <summary>
        /// 停止录制脚本
        /// </summary>
        Task<ScriptResult> StopRecordingAsync(StopRecordingRequest request);
        
        /// <summary>
        /// 执行脚本
        /// </summary>
        Task<ScriptResult> ExecuteScriptAsync(ExecuteScriptRequest request);
        
        /// <summary>
        /// 调试脚本
        /// </summary>
        Task<ScriptResult> DebugScriptAsync(DebugScriptRequest request);
        
        /// <summary>
        /// 管理脚本
        /// </summary>
        Task<ScriptResult> ManageScriptAsync(ManageScriptRequest request);
    }
    
    /// <summary>
    /// 配置系统用例接口
    /// </summary>
    public interface IConfigurationUseCase : IUseCase
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        Task<ConfigurationResult> GetConfigurationAsync(GetConfigurationRequest request);
        
        /// <summary>
        /// 设置配置
        /// </summary>
        Task<ConfigurationResult> SetConfigurationAsync(SetConfigurationRequest request);
        
        /// <summary>
        /// 验证配置
        /// </summary>
        Task<ConfigurationResult> ValidateConfigurationAsync(ValidateConfigurationRequest request);
        
        /// <summary>
        /// 重置配置
        /// </summary>
        Task<ConfigurationResult> ResetConfigurationAsync(ResetConfigurationRequest request);
        
        /// <summary>
        /// 导入导出配置
        /// </summary>
        Task<ConfigurationResult> ImportExportConfigurationAsync(ImportExportConfigurationRequest request);
    }
}