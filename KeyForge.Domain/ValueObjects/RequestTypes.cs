using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 感知请求值对象
    /// 
    /// 原本实现：复杂的感知请求配置，支持多种识别算法
    /// 简化实现：基本的感知请求
    /// </summary>
    public readonly struct SenseRequest : IEquatable<SenseRequest>
    {
        public string RequestId { get; }
        public List<ImageTemplate> Templates { get; }
        public ScreenRegion? Region { get; }
        public RecognitionParameters Parameters { get; }
        public IntPtr WindowHandle { get; }
        public bool UseGrayscale { get; }
        public bool EnableImageEnhancement { get; }
        public Dictionary<string, object> Metadata { get; }
        
        public SenseRequest(
            string requestId,
            List<ImageTemplate> templates,
            ScreenRegion? region,
            RecognitionParameters parameters,
            IntPtr windowHandle = default,
            bool useGrayscale = true,
            bool enableImageEnhancement = false,
            Dictionary<string, object>? metadata = null)
        {
            RequestId = requestId;
            Templates = templates ?? new List<ImageTemplate>();
            Region = region;
            Parameters = parameters;
            WindowHandle = windowHandle;
            UseGrayscale = useGrayscale;
            EnableImageEnhancement = enableImageEnhancement;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
        
        public bool Equals(SenseRequest other) => 
            RequestId == other.RequestId && 
            Templates.Count == other.Templates.Count;
        
        public override bool Equals(object obj) => obj is SenseRequest other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, Templates.Count);
        
        public static bool operator ==(SenseRequest left, SenseRequest right) => left.Equals(right);
        public static bool operator !=(SenseRequest left, SenseRequest right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 决策请求值对象
    /// 
    /// 原本实现：复杂的决策请求配置，支持多种决策算法
    /// 简化实现：基本的决策请求
    /// </summary>
    public readonly struct JudgmentRequest : IEquatable<JudgmentRequest>
    {
        public string RequestId { get; }
        public List<RuleId> RuleIds { get; }
        public StateMachineId? StateMachineId { get; }
        public List<SenseResult> SenseResults { get; }
        public Dictionary<string, object> Variables { get; }
        public Dictionary<string, object> Metadata { get; }
        public List<string> DefaultActions { get; }
        
        public JudgmentRequest(
            string requestId,
            List<RuleId> ruleIds,
            StateMachineId? stateMachineId,
            List<SenseResult> senseResults,
            Dictionary<string, object> variables,
            Dictionary<string, object>? metadata = null,
            List<string>? defaultActions = null)
        {
            RequestId = requestId;
            RuleIds = ruleIds ?? new List<RuleId>();
            StateMachineId = stateMachineId;
            SenseResults = senseResults ?? new List<SenseResult>();
            Variables = variables ?? new Dictionary<string, object>();
            Metadata = metadata ?? new Dictionary<string, object>();
            DefaultActions = defaultActions ?? new List<string>();
        }
        
        public bool Equals(JudgmentRequest other) => 
            RequestId == other.RequestId && 
            RuleIds.Count == other.RuleIds.Count;
        
        public override bool Equals(object obj) => obj is JudgmentRequest other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, RuleIds.Count);
        
        public static bool operator ==(JudgmentRequest left, JudgmentRequest right) => left.Equals(right);
        public static bool operator !=(JudgmentRequest left, JudgmentRequest right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 执行请求值对象
    /// 
    /// 原本实现：复杂的执行请求配置，支持多种执行策略
    /// 简化实现：基本的执行请求
    /// </summary>
    public readonly struct ExecutionRequest : IEquatable<ExecutionRequest>
    {
        public string RequestId { get; }
        public List<GameAction> Actions { get; }
        public ErrorHandlingStrategy ErrorHandlingStrategy { get; }
        public CancellationToken CancellationToken { get; }
        
        public ExecutionRequest(
            string requestId,
            List<GameAction> actions,
            ErrorHandlingStrategy errorHandlingStrategy = ErrorHandlingStrategy.ContinueOnError,
            CancellationToken cancellationToken = default)
        {
            RequestId = requestId;
            Actions = actions ?? new List<GameAction>();
            ErrorHandlingStrategy = errorHandlingStrategy;
            CancellationToken = cancellationToken;
        }
        
        public bool Equals(ExecutionRequest other) => 
            RequestId == other.RequestId && 
            Actions.Count == other.Actions.Count;
        
        public override bool Equals(object obj) => obj is ExecutionRequest other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RequestId, Actions.Count);
        
        public static bool operator ==(ExecutionRequest left, ExecutionRequest right) => left.Equals(right);
        public static bool operator !=(ExecutionRequest left, ExecutionRequest right) => !left.Equals(right);
    }
}