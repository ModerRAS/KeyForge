using System;
using System.Collections.Generic;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 感知结果值对象
    /// 
    /// 原本实现：包含完整的识别结果和置信度分析
    /// 简化实现：基本的识别结果
    /// </summary>
    public readonly struct SenseResult : IEquatable<SenseResult>
    {
        public string RequestId { get; }
        public RecognitionStatus Status { get; }
        public Duration ProcessingTime { get; }
        public Dictionary<string, object> Results { get; }
        public string? ErrorMessage { get; }
        
        public SenseResult(string requestId, RecognitionStatus status, Duration processingTime, Dictionary<string, object> results, string? errorMessage = null)
        {
            RequestId = requestId;
            Status = status;
            ProcessingTime = processingTime;
            Results = results ?? new Dictionary<string, object>();
            ErrorMessage = errorMessage;
        }
        
        public static SenseResult Success(string requestId, Duration processingTime, Dictionary<string, object> results) => 
            new SenseResult(requestId, RecognitionStatus.Success, processingTime, results);
        
        public static SenseResult Failed(string requestId, Duration processingTime, string errorMessage) => 
            new SenseResult(requestId, RecognitionStatus.Failed, processingTime, new Dictionary<string, object>(), errorMessage);
        
        public bool Equals(SenseResult other) => 
            RequestId == other.RequestId && 
            Status == other.Status && 
            ProcessingTime.Equals(other.ProcessingTime);
        
        public override bool Equals(object obj) => obj is SenseResult other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, Status, ProcessingTime);
        
        public static bool operator ==(SenseResult left, SenseResult right) => left.Equals(right);
        public static bool operator !=(SenseResult left, SenseResult right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 决策结果值对象
    /// 
    /// 原本实现：包含完整的决策过程和置信度分析
    /// 简化实现：基本的决策结果
    /// </summary>
    public readonly struct JudgmentResult : IEquatable<JudgmentResult>
    {
        public string RequestId { get; }
        public Decision Decision { get; }
        public Duration ProcessingTime { get; }
        public Dictionary<string, object> Context { get; }
        public string? ErrorMessage { get; }
        
        public JudgmentResult(string requestId, Decision decision, Duration processingTime, Dictionary<string, object> context, string? errorMessage = null)
        {
            RequestId = requestId;
            Decision = decision;
            ProcessingTime = processingTime;
            Context = context ?? new Dictionary<string, object>();
            ErrorMessage = errorMessage;
        }
        
        public static JudgmentResult Success(string requestId, Decision decision, Duration processingTime, Dictionary<string, object> context) => 
            new JudgmentResult(requestId, decision, processingTime, context);
        
        public static JudgmentResult Failed(string requestId, Duration processingTime, string errorMessage) => 
            new JudgmentResult(requestId, Decision.None(), processingTime, new Dictionary<string, object>(), errorMessage);
        
        public bool Equals(JudgmentResult other) => 
            RequestId == other.RequestId && 
            Decision.Equals(other.Decision) && 
            ProcessingTime.Equals(other.ProcessingTime);
        
        public override bool Equals(object obj) => obj is JudgmentResult other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, Decision, ProcessingTime);
        
        public static bool operator ==(JudgmentResult left, JudgmentResult right) => left.Equals(right);
        public static bool operator !=(JudgmentResult left, JudgmentResult right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 决策值对象
    /// 
    /// 原本实现：包含复杂的决策逻辑和权重分析
    /// 简化实现：基本的决策结构
    /// </summary>
    public readonly struct Decision : IEquatable<Decision>
    {
        public string DecisionId { get; }
        public List<string> ActionsToExecute { get; }
        public double Confidence { get; }
        public Dictionary<string, object> Metadata { get; }
        
        public Decision(string decisionId, List<string> actionsToExecute, double confidence, Dictionary<string, object> metadata)
        {
            DecisionId = decisionId;
            ActionsToExecute = actionsToExecute ?? new List<string>();
            Confidence = confidence;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
        
        public static Decision None() => new Decision("none", new List<string>(), 0.0, new Dictionary<string, object>());
        public static Decision FromActions(List<string> actions, double confidence = 1.0) => 
            new Decision(Guid.NewGuid().ToString(), actions, confidence, new Dictionary<string, object>());
        
        public bool Equals(Decision other) => 
            DecisionId == other.DecisionId && 
            Confidence == other.Confidence;
        
        public override bool Equals(object obj) => obj is Decision other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(DecisionId, Confidence);
        
        public static bool operator ==(Decision left, Decision right) => left.Equals(right);
        public static bool operator !=(Decision left, Decision right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 执行结果值对象
    /// 
    /// 原本实现：包含完整的执行过程和性能分析
    /// 简化实现：基本的执行结果
    /// </summary>
    public readonly struct ExecutionResult : IEquatable<ExecutionResult>
    {
        public string RequestId { get; }
        public ExecutionStatus Status { get; }
        public Duration ProcessingTime { get; }
        public int ExecutedActions { get; }
        public int FailedActions { get; }
        public Dictionary<string, object> Results { get; }
        public string? ErrorMessage { get; }
        
        public ExecutionResult(string requestId, ExecutionStatus status, Duration processingTime, int executedActions, int failedActions, Dictionary<string, object> results, string? errorMessage = null)
        {
            RequestId = requestId;
            Status = status;
            ProcessingTime = processingTime;
            ExecutedActions = executedActions;
            FailedActions = failedActions;
            Results = results ?? new Dictionary<string, object>();
            ErrorMessage = errorMessage;
        }
        
        public static ExecutionResult Success(string requestId, Duration processingTime, int executedActions, Dictionary<string, object> results) => 
            new ExecutionResult(requestId, ExecutionStatus.Completed, processingTime, executedActions, 0, results);
        
        public static ExecutionResult Failed(string requestId, Duration processingTime, int executedActions, int failedActions, string errorMessage) => 
            new ExecutionResult(requestId, ExecutionStatus.Failed, processingTime, executedActions, failedActions, new Dictionary<string, object>(), errorMessage);
        
        public bool Equals(ExecutionResult other) => 
            RequestId == other.RequestId && 
            Status == other.Status && 
            ProcessingTime.Equals(other.ProcessingTime);
        
        public override bool Equals(object obj) => obj is ExecutionResult other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, Status, ProcessingTime);
        
        public static bool operator ==(ExecutionResult left, ExecutionResult right) => left.Equals(right);
        public static bool operator !=(ExecutionResult left, ExecutionResult right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 错误处理策略枚举
    /// </summary>
    public enum ErrorHandlingStrategy
    {
        ContinueOnError = 0,
        StopOnError = 1,
        RetryOnError = 2,
        LogAndContinue = 3
    }
}