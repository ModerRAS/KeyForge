using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KeyForge.Domain.Common;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Aggregates;
using Rectangle = KeyForge.Domain.Common.Rectangle;
using Point = KeyForge.Domain.Common.Point;

namespace KeyForge.Domain.Interfaces
{
    /// <summary>
    /// 脚本播放服务接口
    /// 原本实现：复杂的接口定义和依赖
    /// 简化实现：只保留核心功能接口
    /// </summary>
    public interface IScriptPlayerService
    {
        Task<Result> PlayScriptAsync(Guid scriptId);
        Task<Result> PauseScriptAsync(Guid scriptId);
        Task<Result> StopScriptAsync(Guid scriptId);
        Task<Result> ResumeScriptAsync(Guid scriptId);
        Task<Result> GetScriptStatusAsync(Guid scriptId);
        Task<Result> GetCurrentActionAsync(Guid scriptId);
        Task<Result> SetScriptSpeedAsync(Guid scriptId, double speed);
        Task<Result> SetScriptLoopAsync(Guid scriptId, bool loop);
    }

    /// <summary>
    /// 图像识别服务接口
    /// </summary>
    public interface IImageRecognitionService
    {
        Task<Result<RecognitionResult>> RecognizeAsync(byte[] imageData, byte[] templateData);
        Task<Result<RecognitionResult>> RecognizeAsync(byte[] imageData, string templatePath);
        Task<Result<RecognitionResult>> RecognizeOnScreenAsync(KeyForge.Domain.Common.Rectangle region, byte[] templateData);
        Task<Result> SaveTemplateAsync(string name, byte[] templateData, TemplateType templateType);
        Task<Result<byte[]>> LoadTemplateAsync(string name);
        Task<Result<bool>> TemplateExistsAsync(string name);
        Task<Result> DeleteTemplateAsync(string name);
        Task<Result<IEnumerable<string>>> GetAllTemplatesAsync();
    }

    /// <summary>
    /// 输入模拟服务接口
    /// </summary>
    public interface IInputSimulationService
    {
        Task<Result> SimulateKeyPressAsync(KeyCode keyCode, KeyState keyState);
        Task<Result> SimulateMouseClickAsync(MouseButton button, Point position);
        Task<Result> SimulateMouseMoveAsync(Point position);
        Task<Result> SimulateMouseDragAsync(Point startPoint, Point endPoint);
        Task<Result> SimulateTextInputAsync(string text);
        Task<Result> SimulateDelayAsync(int milliseconds);
        Task<Result<Point>> GetCursorPositionAsync();
        Task<Result<Rectangle>> GetScreenBoundsAsync();
    }

    /// <summary>
    /// 状态机服务接口
    /// </summary>
    public interface IStateMachineService
    {
        Task<Result> StartStateMachineAsync(Guid stateMachineId);
        Task<Result> StopStateMachineAsync(Guid stateMachineId);
        Task<Result> PauseStateMachineAsync(Guid stateMachineId);
        Task<Result> ResumeStateMachineAsync(Guid stateMachineId);
        Task<Result> GetStateMachineStatusAsync(Guid stateMachineId);
        Task<Result> GetCurrentStateAsync(Guid stateMachineId);
        Task<Result> TransitionToStateAsync(Guid stateMachineId, string stateName);
        Task<Result> AddStateAsync(Guid stateMachineId, State state);
        Task<Result> UpdateStateAsync(State state);
        Task<Result> RemoveStateAsync(Guid stateMachineId, string stateName);
    }

    /// <summary>
    /// 决策引擎服务接口
    /// </summary>
    public interface IDecisionEngineService
    {
        Task<Result> AddRuleAsync(DecisionRule rule);
        Task<Result> UpdateRuleAsync(DecisionRule rule);
        Task<Result> DeleteRuleAsync(Guid ruleId);
        Task<Result<DecisionRule>> GetRuleAsync(Guid ruleId);
        Task<Result<IEnumerable<DecisionRule>>> GetAllRulesAsync();
        Task<Result<IEnumerable<DecisionRule>>> GetRulesByStateMachineAsync(Guid stateMachineId);
        Task<Result<DecisionRule>> EvaluateRulesAsync(Guid stateMachineId, Dictionary<string, object> context);
        Task<Result> ExecuteDecisionAsync(Guid stateMachineId, DecisionRule rule);
    }

    /// <summary>
    /// 决策规则服务接口（兼容性）
    /// </summary>
    public interface IDecisionRuleService
    {
        Task<Result> AddRuleAsync(DecisionRule rule);
        Task<Result> UpdateRuleAsync(DecisionRule rule);
        Task<Result> DeleteRuleAsync(Guid ruleId);
        Task<Result> GetRuleAsync(Guid ruleId);
        Task<Result> GetAllRulesAsync();
        Task<Result> EvaluateRulesAsync(Guid stateMachineId);
    }

    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILoggingService
    {
        Task<Result> LogAsync(LogEntry logEntry);
        Task<Result> LogAsync(LogLevel level, string message, Exception? exception = null);
        Task<Result> LogAsync(LogLevel level, string message, Dictionary<string, object> properties);
        Task<Result<IEnumerable<LogEntry>>> GetLogsAsync(LogLevel level, DateTime? startTime = null, DateTime? endTime = null);
        Task<Result> ClearLogsAsync();
        Task<Result<int>> GetLogCountAsync(LogLevel? level = null);
        Task<Result> EnableConsoleLoggingAsync(bool enable);
        Task<Result> EnableFileLoggingAsync(bool enable, string? filePath = null);
    }

    /// <summary>
    /// 配置服务接口
    /// </summary>
    public interface IConfigurationService
    {
        Task<Result<string>> GetAsync(string key);
        Task<Result<T>> GetAsync<T>(string key);
        Task<Result> SetAsync(string key, string value);
        Task<Result> SetAsync<T>(string key, T value);
        Task<Result<bool>> ExistsAsync(string key);
        Task<Result> DeleteAsync(string key);
        Task<Result<IEnumerable<string>>> GetAllKeysAsync();
        Task<Result> ReloadAsync();
        Task<Result> SaveAsync();
    }

    /// <summary>
    /// 文件存储服务接口
    /// </summary>
    public interface IFileStorageService
    {
        Task<Result<string>> SaveAsync(string fileName, byte[] data);
        Task<Result<byte[]>> LoadAsync(string fileName);
        Task<Result<bool>> ExistsAsync(string fileName);
        Task<Result> DeleteAsync(string fileName);
        Task<Result<IEnumerable<string>>> GetAllFilesAsync();
        Task<Result<long>> GetFileSizeAsync(string fileName);
        Task<Result<DateTime>> GetFileLastModifiedAsync(string fileName);
        Task<Result> CreateDirectoryAsync(string directoryPath);
        Task<Result<bool>> DirectoryExistsAsync(string directoryPath);
        Task<Result<IEnumerable<string>>> GetDirectoriesAsync(string directoryPath);
    }

    /// <summary>
    /// 热键管理服务接口
    /// </summary>
    public interface IHotkeyManagerService
    {
        Task<Result> RegisterHotkeyAsync(Guid id, HotkeyType type, KeyCode keyCode, bool ctrl, bool alt, bool shift, Func<Task> callback);
        Task<Result> UnregisterHotkeyAsync(Guid id);
        Task<Result> UnregisterAllHotkeysAsync();
        Task<Result<bool>> IsHotkeyRegisteredAsync(Guid id);
        Task<Result<IEnumerable<Guid>>> GetRegisteredHotkeysAsync();
        Task<Result> EnableHotkeyAsync(Guid id);
        Task<Result> DisableHotkeyAsync(Guid id);
        Task<Result> SetHotkeyDescriptionAsync(Guid id, string description);
    }

    /// <summary>
    /// 性能监控服务接口
    /// </summary>
    public interface IPerformanceService
    {
        Task<Result> StartMonitoringAsync();
        Task<Result> StopMonitoringAsync();
        Task<Result<Dictionary<string, double>>> GetMetricsAsync();
        Task<Result> RecordMetricAsync(string name, double value);
        Task<Result> IncrementCounterAsync(string name, double value = 1);
        Task<Result> SetGaugeAsync(string name, double value);
        Task<Result> StartTimerAsync(string name);
        Task<Result> StopTimerAsync(string name);
        Task<Result> ResetMetricsAsync();
        Task<Result> EnableMetricsCollectionAsync(bool enable);
    }

    /// <summary>
    /// 错误处理服务接口
    /// </summary>
    public interface IErrorHandlerService
    {
        Task<Result> HandleErrorAsync(Exception exception, Dictionary<string, object>? context = null);
        Task<Result> LogErrorAsync(string message, Exception? exception = null);
        Task<Result> ReportErrorAsync(string title, string message, Exception? exception = null);
        Task<Result> SetErrorCallbackAsync(Func<Exception, Task> callback);
        Task<Result> EnableErrorNotificationsAsync(bool enable);
        Task<Result> GetErrorHistoryAsync(int count = 100);
        Task<Result> ClearErrorHistoryAsync();
    }

    /// <summary>
    /// 序列化服务接口
    /// </summary>
    public interface ISerializationService
    {
        Task<Result<string>> SerializeAsync<T>(T obj);
        Task<Result<T>> DeserializeAsync<T>(string json);
        Task<Result<byte[]>> SerializeToBytesAsync<T>(T obj);
        Task<Result<T>> DeserializeFromBytesAsync<T>(byte[] data);
        Task<Result<string>> SerializeToXmlAsync<T>(T obj);
        Task<Result<T>> DeserializeFromXmlAsync<T>(string xml);
        Task<Result> SaveToFileAsync<T>(string filePath, T obj);
        Task<Result<T>> LoadFromFileAsync<T>(string filePath);
    }

    /// <summary>
    /// 验证服务接口
    /// </summary>
    public interface IValidationService
    {
        Task<Result> ValidateAsync(object obj);
        Task<Result> ValidateScriptAsync(Script script);
        Task<Result> ValidateStateMachineAsync(StateMachine stateMachine);
        Task<Result> ValidateImageTemplateAsync(ImageTemplate template);
        Task<Result> ValidateConfigurationAsync(Dictionary<string, object> configuration);
        Task<Result<IEnumerable<string>>> GetValidationErrorsAsync(object obj);
    }

    /// <summary>
    /// 缓存服务接口
    /// </summary>
    public interface ICacheService
    {
        Task<Result<T>> GetAsync<T>(string key);
        Task<Result> SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<Result<bool>> ExistsAsync(string key);
        Task<Result> DeleteAsync(string key);
        Task<Result> ClearAsync();
        Task<Result> SetExpirationAsync(string key, TimeSpan expiration);
        Task<Result<TimeSpan?>> GetExpirationAsync(string key);
        Task<Result<IEnumerable<string>>> GetAllKeysAsync();
        Task<Result> RemoveExpiredAsync();
    }

    /// <summary>
    /// 加密服务接口
    /// </summary>
    public interface IEncryptionService
    {
        Task<Result<string>> EncryptAsync(string plainText);
        Task<Result<string>> DecryptAsync(string encryptedText);
        Task<Result<byte[]>> EncryptAsync(byte[] data);
        Task<Result<byte[]>> DecryptAsync(byte[] data);
        Task<Result<string>> GenerateKeyAsync();
        Task<Result<bool>> ValidateKeyAsync(string key);
        Task<Result> SetKeyAsync(string key);
        Task<Result> SetKeyAsync(byte[] key);
    }

    /// <summary>
    /// 应用服务接口
    /// </summary>
    public interface IScriptApplicationService
    {
        Task<Result<Script>> CreateScriptAsync(string name, string description);
        Task<Result<Script>> GetScriptAsync(Guid id);
        Task<Result<IEnumerable<Script>>> GetAllScriptsAsync();
        Task<Result> UpdateScriptAsync(Script script);
        Task<Result> DeleteScriptAsync(Guid id);
        Task<Result<Script>> DuplicateScriptAsync(Guid id, string newName);
        Task<Result<IEnumerable<Script>>> GetScriptsByStatusAsync(ScriptStatus status);
        Task<Result<PagedResult<Script>>> GetPagedScriptsAsync(QueryParameters parameters);
    }

    /// <summary>
    /// 状态机应用服务接口
    /// </summary>
    public interface IStateMachineApplicationService
    {
        Task<Result<StateMachine>> CreateStateMachineAsync(string name, string description);
        Task<Result<StateMachine>> GetStateMachineAsync(Guid id);
        Task<Result<IEnumerable<StateMachine>>> GetAllStateMachinesAsync();
        Task<Result> UpdateStateMachineAsync(StateMachine stateMachine);
        Task<Result> DeleteStateMachineAsync(Guid id);
        Task<Result<StateMachine>> DuplicateStateMachineAsync(Guid id, string newName);
        Task<Result<IEnumerable<StateMachine>>> GetStateMachinesByStatusAsync(StateMachineStatus status);
        Task<Result<PagedResult<StateMachine>>> GetPagedStateMachinesAsync(QueryParameters parameters);
    }

    /// <summary>
    /// 图像模板应用服务接口
    /// </summary>
    public interface IImageTemplateApplicationService
    {
        Task<Result<ImageTemplate>> CreateTemplateAsync(string name, byte[] imageData, TemplateType templateType);
        Task<Result<ImageTemplate>> GetTemplateAsync(Guid id);
        Task<Result<IEnumerable<ImageTemplate>>> GetAllTemplatesAsync();
        Task<Result> UpdateTemplateAsync(ImageTemplate template);
        Task<Result> DeleteTemplateAsync(Guid id);
        Task<Result<ImageTemplate>> DuplicateTemplateAsync(Guid id, string newName);
        Task<Result<IEnumerable<ImageTemplate>>> GetTemplatesByTypeAsync(TemplateType templateType);
        Task<Result<PagedResult<ImageTemplate>>> GetPagedTemplatesAsync(QueryParameters parameters);
    }

    /// <summary>
    /// 输入验证服务接口
    /// 
    /// 原本实现：继承自IValidationService
    /// 简化实现：独立的接口定义
    /// </summary>
    public interface IInputValidationService
    {
        Task<Result> ValidateAsync(object obj);
        Task<Result> ValidateScriptAsync(Script script);
        Task<Result> ValidateStateMachineAsync(StateMachine stateMachine);
        Task<Result> ValidateImageTemplateAsync(ImageTemplate template);
        Task<Result> ValidateConfigurationAsync(Dictionary<string, object> configuration);
        Task<Result<IEnumerable<string>>> GetValidationErrorsAsync(object obj);
    }
}