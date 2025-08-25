using KeyForge.Abstractions.Models.Script;
using KeyForge.Abstractions.Models.Core;
using KeyForge.Abstractions.Models.Application;

namespace KeyForge.Abstractions.Interfaces.Core
{
    /// <summary>
    /// 脚本服务基础接口
    /// 【优化实现】统一了脚本执行引擎的抽象接口，支持跨平台脚本执行
    /// 原实现：脚本执行功能耦合在具体实现中，缺乏统一抽象
    /// 优化：定义统一的脚本服务接口，支持录制、执行和调试功能
    /// </summary>
    public interface IScriptService : IDisposable
    {
        /// <summary>
        /// 初始化脚本服务
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 服务状态
        /// </summary>
        ServiceStatus Status { get; }
        
        /// <summary>
        /// 脚本状态变化事件
        /// </summary>
        event EventHandler<ScriptStatusEventArgs> OnScriptStatusChanged;
    }
    
    /// <summary>
    /// 脚本录制服务接口
    /// </summary>
    public interface IScriptRecordingService : IScriptService
    {
        /// <summary>
        /// 开始录制脚本
        /// </summary>
        Task<bool> StartRecordingAsync(string scriptName);
        
        /// <summary>
        /// 停止录制脚本
        /// </summary>
        Task<Script> StopRecordingAsync();
        
        /// <summary>
        /// 暂停录制
        /// </summary>
        Task<bool> PauseRecordingAsync();
        
        /// <summary>
        /// 恢复录制
        /// </summary>
        Task<bool> ResumeRecordingAsync();
        
        /// <summary>
        /// 添加动作到录制中
        /// </summary>
        Task<bool> AddActionAsync(RecordedAction action);
        
        /// <summary>
        /// 是否正在录制
        /// </summary>
        bool IsRecording { get; }
        
        /// <summary>
        /// 录制状态
        /// </summary>
        RecordingStatus RecordingStatus { get; }
    }
    
    /// <summary>
    /// 脚本执行服务接口
    /// </summary>
    public interface IScriptExecutionService : IScriptService
    {
        /// <summary>
        /// 执行脚本
        /// </summary>
        Task<ExecutionResult> ExecuteScriptAsync(Script script);
        
        /// <summary>
        /// 执行脚本文件
        /// </summary>
        Task<ExecutionResult> ExecuteScriptFileAsync(string filePath);
        
        /// <summary>
        /// 停止脚本执行
        /// </summary>
        Task<bool> StopExecutionAsync();
        
        /// <summary>
        /// 暂停脚本执行
        /// </summary>
        Task<bool> PauseExecutionAsync();
        
        /// <summary>
        /// 恢复脚本执行
        /// </summary>
        Task<bool> ResumeExecutionAsync();
        
        /// <summary>
        /// 调试执行脚本
        /// </summary>
        Task<ExecutionResult> DebugScriptAsync(Script script);
        
        /// <summary>
        /// 单步执行脚本
        /// </summary>
        Task<ExecutionResult> StepExecuteAsync(Script script, int stepIndex);
        
        /// <summary>
        /// 是否正在执行
        /// </summary>
        bool IsExecuting { get; }
        
        /// <summary>
        /// 执行状态
        /// </summary>
        ExecutionStatus ExecutionStatus { get; }
        
        /// <summary>
        /// 脚本执行进度
        /// </summary>
        event EventHandler<ExecutionProgressEventArgs> OnExecutionProgress;
    }
    
    /// <summary>
    /// 脚本管理服务接口
    /// </summary>
    public interface IScriptManagementService : IScriptService
    {
        /// <summary>
        /// 创建新脚本
        /// </summary>
        Task<Script> CreateScriptAsync(string name, string description = "");
        
        /// <summary>
        /// 保存脚本
        /// </summary>
        Task<bool> SaveScriptAsync(Script script);
        
        /// <summary>
        /// 加载脚本
        /// </summary>
        Task<Script> LoadScriptAsync(string filePath);
        
        /// <summary>
        /// 删除脚本
        /// </summary>
        Task<bool> DeleteScriptAsync(string scriptName);
        
        /// <summary>
        /// 获取所有脚本
        /// </summary>
        Task<List<Script>> GetAllScriptsAsync();
        
        /// <summary>
        /// 搜索脚本
        /// </summary>
        Task<List<Script>> SearchScriptsAsync(string searchTerm);
        
        /// <summary>
        /// 导入脚本
        /// </summary>
        Task<Script> ImportScriptAsync(string filePath);
        
        /// <summary>
        /// 导出脚本
        /// </summary>
        Task<bool> ExportScriptAsync(Script script, string filePath);
        
        /// <summary>
        /// 验证脚本语法
        /// </summary>
        Task<ValidationResult> ValidateScriptAsync(Script script);
    }
    
    /// <summary>
    /// 脚本调试服务接口
    /// </summary>
    public interface IScriptDebuggingService : IScriptService
    {
        /// <summary>
        /// 设置断点
        /// </summary>
        Task<bool> SetBreakpointAsync(Script script, int actionIndex);
        
        /// <summary>
        /// 清除断点
        /// </summary>
        Task<bool> ClearBreakpointAsync(Script script, int actionIndex);
        
        /// <summary>
        /// 获取所有断点
        /// </summary>
        Task<List<int>> GetBreakpointsAsync(Script script);
        
        /// <summary>
        /// 获取变量值
        /// </summary>
        Task<object> GetVariableValueAsync(Script script, string variableName);
        
        /// <summary>
        /// 设置变量值
        /// </summary>
        Task<bool> SetVariableValueAsync(Script script, string variableName, object value);
        
        /// <summary>
        /// 获取调用栈
        /// </summary>
        Task<List<StackFrame>> GetCallStackAsync(Script script);
        
        /// <summary>
        /// 调试事件
        /// </summary>
        event EventHandler<DebugEventArgs> OnDebugEvent;
    }
}