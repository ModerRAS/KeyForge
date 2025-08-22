namespace KeyForge.Core.Domain.Judge
{
    using KeyForge.Core.Domain.Common;
    using KeyForge.Core.Domain.Vision;
    using KeyForge.Core.Domain.Sense;
    using KeyForge.Core.Domain.Judge.Engines;

    /// <summary>
    /// 规则ID - 简化实现，使用Domain层的RuleId
    /// 
    /// 原本实现：继承自StronglyTypedId的record类型
    /// 简化实现：直接使用Domain层的struct类型
    /// </summary>
    using RuleId = KeyForge.Domain.ValueObjects.RuleId;
    
    /// <summary>
    /// 状态机ID - 简化实现，使用Domain层的StateMachineId
    /// </summary>
    using StateMachineId = KeyForge.Domain.ValueObjects.StateMachineId;

    /// <summary>
    /// 判断服务 - 负责基于识别结果做出决策
    /// </summary>
    public class JudgeService
    {
        private readonly IRuleEngine _ruleEngine;
        private readonly IStateMachineEngine _stateMachineEngine;
        private readonly KeyForge.Core.Domain.Common.ILogger _logger;

        public JudgeService(
            IRuleEngine ruleEngine,
            IStateMachineEngine stateMachineEngine,
            KeyForge.Core.Domain.Common.ILogger logger)
        {
            _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
            _stateMachineEngine = stateMachineEngine ?? throw new ArgumentNullException(nameof(stateMachineEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 基于感知结果进行决策
        /// </summary>
        public async Task<JudgmentResult> JudgeAsync(JudgmentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogInformation($"开始决策: {request.JudgmentName}");

                // 1. 构建决策上下文
                var context = BuildJudgmentContext(request);

                // 2. 评估规则
                var ruleResults = new List<RuleEvaluationResult>();
                foreach (var ruleId in request.RuleIds)
                {
                    var result = await _ruleEngine.EvaluateRuleAsync(ruleId, context);
                    ruleResults.Add(result);
                    
                    _logger.LogDebug($"规则 {ruleId} 评估结果: {result.IsMatch}, " +
                                   $"置信度: {result.Confidence?.Value ?? 0:F2}");
                }

                // 3. 处理状态机
                StateMachineResult? stateMachineResult = null;
                if (request.StateMachineId != null)
                {
                    stateMachineResult = await _stateMachineEngine.ProcessAsync(
                        request.StateMachineId,
                        context
                    );
                    
                    _logger.LogDebug($"状态机处理结果: 当前状态 {stateMachineResult.CurrentState}, " +
                                   $"触发动作: {stateMachineResult.TriggeredActions.Count}");
                }

                // 4. 生成最终决策
                var decision = MakeDecision(ruleResults, stateMachineResult, request);

                var judgmentResult = new JudgmentResult(
                    request.JudgmentName,
                    decision,
                    ruleResults,
                    stateMachineResult,
                    context,
                    Timestamp.Now
                );

                _logger.LogInformation($"决策完成: {request.JudgmentName}, " +
                                   $"决策类型: {decision.DecisionType}, " +
                                   $"执行动作: {decision.ActionsToExecute.Count}");

                return judgmentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"决策失败: {request.JudgmentName}");
                throw;
            }
        }

        /// <summary>
        /// 批量评估规则
        /// </summary>
        public async Task<List<RuleEvaluationResult>> EvaluateRulesAsync(RulesEvaluationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var results = new List<RuleEvaluationResult>();
            var context = BuildJudgmentContext(request);

            foreach (var ruleId in request.RuleIds)
            {
                try
                {
                    var result = await _ruleEngine.EvaluateRuleAsync(ruleId, context);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"评估规则 {ruleId} 失败");
                    results.Add(new RuleEvaluationResult(
                        ruleId,
                        false,
                        ConfidenceScore.Low,
                        $"评估失败: {ex.Message}",
                        Array.Empty<string>()
                    ));
                }
            }

            return results;
        }

        /// <summary>
        /// 创建决策规则
        /// </summary>
        public async Task<DecisionRule> CreateRuleAsync(CreateRuleRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var rule = new DecisionRule(
                new RuleName(request.Name),
                new ConditionExpression(request.Condition),
                request.ThenActions,
                request.ElseActions,
                request.Priority
            );

            await _ruleEngine.AddRuleAsync(rule);
            _logger.LogInformation($"创建决策规则: {request.Name}");
            
            return rule;
        }

        /// <summary>
        /// 更新规则状态
        /// </summary>
        public async Task UpdateRuleStatusAsync(RuleId ruleId, RuleStatus status)
        {
            await _ruleEngine.UpdateRuleStatusAsync(ruleId, status);
            _logger.LogInformation($"更新规则状态: {ruleId} -> {status}");
        }

        private JudgmentContext BuildJudgmentContext(JudgmentRequest request)
        {
            var context = new JudgmentContext();

            // 添加感知结果
            foreach (var senseResult in request.SenseResults)
            {
                context.AddSenseResult(senseResult.OperationName, senseResult);
            }

            // 添加变量
            foreach (var variable in request.Variables)
            {
                context.SetVariable(variable.Key, variable.Value);
            }

            // 添加元数据
            foreach (var metadata in request.Metadata)
            {
                context.SetMetadata(metadata.Key, metadata.Value);
            }

            return context;
        }

        private Decision MakeDecision(
            List<RuleEvaluationResult> ruleResults,
            StateMachineResult? stateMachineResult,
            JudgmentRequest request)
        {
            var actionsToExecute = new List<string>();
            var decisionType = DecisionType.NoAction;

            // 1. 处理规则结果
            var matchedRules = ruleResults.Where(r => r.IsMatch).OrderByDescending(r => r.Confidence?.Value ?? 0);
            
            if (matchedRules.Any())
            {
                var bestRule = matchedRules.First();
                actionsToExecute.AddRange(bestRule.MatchedActions);
                decisionType = DecisionType.RuleBased;
            }

            // 2. 处理状态机结果
            if (stateMachineResult?.TriggeredActions.Count > 0)
            {
                actionsToExecute.AddRange(stateMachineResult.TriggeredActions);
                decisionType = decisionType == DecisionType.RuleBased 
                    ? DecisionType.Hybrid 
                    : DecisionType.StateMachine;
            }

            // 3. 如果没有匹配的规则，检查默认动作
            if (actionsToExecute.Count == 0 && request.DefaultActions.Count > 0)
            {
                actionsToExecute.AddRange(request.DefaultActions);
                decisionType = DecisionType.Default;
            }

            return new Decision(decisionType, actionsToExecute);
        }
    }

    /// <summary>
    /// 决策请求 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record JudgmentRequest(
        string JudgmentName,
        List<RuleId> RuleIds,
        StateMachineId? StateMachineId,
        List<SenseResult> SenseResults,
        Dictionary<string, object> Variables,
        Dictionary<string, string> Metadata,
        List<string> DefaultActions
    );

    /// <summary>
    /// 决策结果
    /// </summary>
    public record JudgmentResult(
        string JudgmentName,
        Decision Decision,
        List<RuleEvaluationResult> RuleResults,
        StateMachineResult? StateMachineResult,
        JudgmentContext Context,
        Timestamp Timestamp
    );

    /// <summary>
    /// 决策
    /// </summary>
    public record Decision(
        DecisionType DecisionType,
        List<string> ActionsToExecute
    );

    /// <summary>
    /// 决策类型
    /// </summary>
    public enum DecisionType
    {
        NoAction = 0,
        RuleBased = 1,
        StateMachine = 2,
        Hybrid = 3,
        Default = 4
    }

    /// <summary>
    /// 规则评估请求
    /// </summary>
    public record RulesEvaluationRequest(
        List<RuleId> RuleIds,
        List<SenseResult> SenseResults,
        Dictionary<string, object> Variables,
        Dictionary<string, string> Metadata
    );

    /// <summary>
    /// 创建规则请求
    /// </summary>
    public record CreateRuleRequest(
        string Name,
        string Condition,
        List<string> ThenActions,
        List<string> ElseActions,
        int Priority
    );

    /// <summary>
    /// 决策上下文
    /// </summary>
    public class JudgmentContext
    {
        private readonly Dictionary<string, SenseResult> _senseResults = new();
        private readonly Dictionary<string, object> _variables = new();
        private readonly Dictionary<string, string> _metadata = new();

        public void AddSenseResult(string key, SenseResult result)
        {
            _senseResults[key] = result;
        }

        public SenseResult? GetSenseResult(string key)
        {
            return _senseResults.TryGetValue(key, out var result) ? result : null;
        }

        public void SetVariable(string name, object value)
        {
            _variables[name] = value;
        }

        public T? GetVariable<T>(string name)
        {
            if (_variables.TryGetValue(name, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
            return default;
        }

        public void SetMetadata(string key, string value)
        {
            _metadata[key] = value;
        }

        public string? GetMetadata(string key)
        {
            return _metadata.TryGetValue(key, out var value) ? value : null;
        }

        public List<SenseResult> GetAllSenseResults() => _senseResults.Values.ToList();
        public Dictionary<string, object> GetAllVariables() => new Dictionary<string, object>(_variables);
        public Dictionary<string, string> GetAllMetadata() => new Dictionary<string, string>(_metadata);
    }

    /// <summary>
    /// 规则引擎接口
    /// </summary>
    public interface IRuleEngine
    {
        Task<RuleEvaluationResult> EvaluateRuleAsync(RuleId ruleId, JudgmentContext context);
        Task AddRuleAsync(DecisionRule rule);
        Task UpdateRuleStatusAsync(RuleId ruleId, RuleStatus status);
        Task<List<DecisionRule>> GetRulesAsync();
    }

    /// <summary>
    /// 状态机引擎接口
    /// </summary>
    public interface IStateMachineEngine
    {
        Task<StateMachineResult> ProcessAsync(StateMachineId stateMachineId, JudgmentContext context);
        Task CreateStateMachineAsync(StateMachine stateMachine);
        Task UpdateStateMachineAsync(StateMachineId stateMachineId, StateMachine newStateMachine);
    }

    /// <summary>
    /// 规则评估结果
    /// </summary>
    public record RuleEvaluationResult(
        RuleId RuleId,
        bool IsMatch,
        ConfidenceScore? Confidence,
        string? ErrorMessage,
        List<string> MatchedActions
    );

    /// <summary>
    /// 状态机结果
    /// </summary>
    public record StateMachineResult(
        string CurrentState,
        List<string> TriggeredActions,
        bool TransitionSuccessful
    );
}