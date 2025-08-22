using System;
using System.Threading.Tasks;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Services
{
    /// <summary>
    /// 图像识别服务接口
    /// 简化实现：使用跨平台的ImageData替代Bitmap
    /// </summary>
    public interface IImageRecognitionService
    {
        Task<RecognitionResult> RecognizeTemplateAsync(ImageData screenCapture, string templateName);
        Task<RecognitionResult> RecognizeTextAsync(ImageData screenCapture, string textToFind);
        Task<RecognitionResult> RecognizeColorAsync(ImageData screenCapture, string targetColor, double tolerance = 0.1);
        Task<ImageData> CaptureScreenAsync();
        Task<bool> LoadTemplateAsync(string templateName, byte[] templateData);
        Task SaveTemplateAsync(string templateName, byte[] templateData);
    }

    /// <summary>
    /// 决策引擎服务接口
    /// </summary>
    public interface IDecisionEngineService
    {
        Task<Guid> EvaluateRulesAsync(Guid stateMachineId, Func<string, object> contextProvider);
        Task<bool> CanTransitionAsync(Guid stateMachineId, Guid fromStateId, Guid toStateId);
        Task<DecisionResult> MakeDecisionAsync(DecisionContext context);
    }

    /// <summary>
    /// 动作执行服务接口
    /// </summary>
    public interface IActionExecutionService
    {
        Task ExecuteActionAsync(Guid actionId);
        Task ExecuteScriptAsync(Guid scriptId);
        Task PauseExecutionAsync();
        Task ResumeExecutionAsync();
        Task StopExecutionAsync();
        bool IsExecuting { get; }
        ExecutionStatus Status { get; }
    }

    /// <summary>
    /// 决策上下文
    /// </summary>
    public class DecisionContext
    {
        public Guid StateMachineId { get; set; }
        public Guid CurrentStateId { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public RecognitionResult LastRecognitionResult { get; set; }
    }

    /// <summary>
    /// 决策结果
    /// </summary>
    public class DecisionResult
    {
        public Guid ActionId { get; set; }
        public bool ShouldTransition { get; set; }
        public Guid TargetStateId { get; set; }
        public string Reason { get; set; }
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        Idle,
        Running,
        Paused,
        Stopped,
        Error
    }
}