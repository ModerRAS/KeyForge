namespace KeyForge.Core.Domain.Judge.Engines
{
    using KeyForge.Core.Domain.Common;
    using KeyForge.Core.Domain.Judge;
    using KeyForge.Core.Domain.Vision;
    using KeyForge.Core.Domain.Sense;
    using KeyForge.Core.Domain.Interfaces;
    using KeyForge.Domain.Common;
    using KeyForge.Domain.ValueObjects;

    /// <summary>
    /// 类型别名 - 解决Core层和Domain层的类型冲突
    /// 原本实现：Core层重新定义所有类型
    /// 简化实现：使用Domain层的类型定义
    /// </summary>
    using RuleId = KeyForge.Domain.ValueObjects.RuleId;

  
    /// <summary>
    /// 简化实现的规则引擎
    /// 
    /// 原本实现：支持复杂的表达式解析、嵌套逻辑、函数调用
    /// 简化实现：基于字符串匹配和简单条件判断
    /// 
    /// 优化建议：
    /// 1. 集成ExpressionTree或第三方表达式解析器
    /// 2. 添加变量类型检查和转换
    /// 3. 支持数学运算和字符串操作
    /// 4. 添加自定义函数支持
    /// </summary>
    public class SimplifiedRuleEngine : IRuleEngine
    {
        private readonly Dictionary<RuleId, DecisionRule> _rules = new();
        private readonly KeyForge.Core.Domain.Common.ILogger _logger;

        public SimplifiedRuleEngine(KeyForge.Core.Domain.Common.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RuleEvaluationResult> EvaluateRuleAsync(RuleId ruleId, JudgmentContext context)
        {
            if (!_rules.TryGetValue(ruleId, out var rule))
            {
                return new RuleEvaluationResult(
                    ruleId,
                    false,
                    ConfidenceScore.Low,
                    $"规则不存在: {ruleId}",
                    Array.Empty<string>()
                );
            }

            try
            {
                var conditionResult = await EvaluateConditionAsync(rule.Condition, context);
                
                var matchedActions = conditionResult ? rule.ThenActions : rule.ElseActions;
                
                return new RuleEvaluationResult(
                    ruleId,
                    conditionResult,
                    new ConfidenceScore(conditionResult ? 0.9 : 0.1),
                    null,
                    matchedActions
                );
            }
            catch (Exception ex)
            {
                _logger.Error($"评估规则 {ruleId} 失败: {ex.Message}");
                return new RuleEvaluationResult(
                    ruleId,
                    false,
                    ConfidenceScore.Low,
                    $"规则评估失败: {ex.Message}",
                    Array.Empty<string>()
                );
            }
        }

        public async Task AddRuleAsync(DecisionRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            // 验证规则条件
            var validationResult = await ValidateRuleConditionAsync(rule.Condition);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"规则条件无效: {string.Join(", ", validationResult.Errors)}");
            }

            _rules[rule.Id] = rule;
            _logger.Info($"添加规则: {rule.Name.Value}");
        }

        public async Task UpdateRuleStatusAsync(RuleId ruleId, RuleStatus status)
        {
            if (!_rules.TryGetValue(ruleId, out var rule))
            {
                throw new ArgumentException($"规则不存在: {ruleId}");
            }

            // 简化实现：在内存中更新状态
            // 在实际实现中，应该持久化到数据库
            rule.UpdateStatus(status);
            _logger.Info($"更新规则状态: {ruleId} -> {status}");
        }

        public async Task<List<DecisionRule>> GetRulesAsync()
        {
            return _rules.Values.ToList();
        }

        /// <summary>
        /// 简化实现的条件评估
        /// 
        /// 原本实现：
        /// - 支持复杂的JavaScript-like表达式
        /// - 支持变量作用域和类型系统
        /// - 支持函数调用和运算符重载
        /// - 支持异常处理和错误恢复
        /// 
        /// 简化实现：
        /// - 基于字符串匹配和简单比较
        /// - 支持基本的识别结果检查
        /// - 固定的条件格式
        /// </summary>
        private async Task<bool> EvaluateConditionAsync(ConditionExpression condition, JudgmentContext context)
        {
            var expression = condition.Expression;
            
            // 简化实现：支持几种基本的条件格式
            // 格式1: "recognition.OperationName.Status == Success"
            // 格式2: "recognition.OperationName.Confidence > 0.8"
            // 格式3: "variable.Name == value"
            
            if (expression.Contains("recognition.") && expression.Contains("Status"))
            {
                return await EvaluateRecognitionStatusCondition(expression, context);
            }
            else if (expression.Contains("recognition.") && expression.Contains("Confidence"))
            {
                return await EvaluateRecognitionConfidenceCondition(expression, context);
            }
            else if (expression.Contains("variable."))
            {
                return await EvaluateVariableCondition(expression, context);
            }
            else
            {
                // 默认返回true
                _logger.Warning($"未知的条件格式: {expression}");
                return true;
            }
        }

        private async Task<bool> EvaluateRecognitionStatusCondition(string expression, JudgmentContext context)
        {
            // 解析格式: "recognition.OperationName.Status == Success"
            var parts = expression.Split(new[] { "recognition.", ".Status == " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;

            var operationName = parts[0];
            var expectedStatus = parts[1];

            var senseResult = context.GetSenseResult(operationName);
            if (senseResult == null)
                return false;

            var successfulRecognitions = senseResult.RecognitionResults.Count(r => r.IsSuccessful());
            return expectedStatus.ToLower() switch
            {
                "success" => successfulRecognitions > 0,
                "failed" => successfulRecognitions == 0,
                "partial" => senseResult.RecognitionResults.Any(r => r.Status == RecognitionStatus.Partial),
                _ => false
            };
        }

        private async Task<bool> EvaluateRecognitionConfidenceCondition(string expression, JudgmentContext context)
        {
            // 解析格式: "recognition.OperationName.Confidence > 0.8"
            var parts = expression.Split(new[] { "recognition.", ".Confidence ", " " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;

            var operationName = parts[0];
            var @operator = parts[1];
            var threshold = double.Parse(parts[2]);

            var senseResult = context.GetSenseResult(operationName);
            if (senseResult == null)
                return false;

            var maxConfidence = senseResult.RecognitionResults
                .Where(r => r.IsSuccessful())
                .Select(r => r.Confidence.Value)
                .DefaultIfEmpty(0)
                .Max();

            return @operator switch
            {
                ">" => maxConfidence > threshold,
                ">=" => maxConfidence >= threshold,
                "<" => maxConfidence < threshold,
                "<=" => maxConfidence <= threshold,
                "==" => Math.Abs(maxConfidence - threshold) < 0.001,
                _ => false
            };
        }

        private async Task<bool> EvaluateVariableCondition(string expression, JudgmentContext context)
        {
            // 解析格式: "variable.Name == value"
            var parts = expression.Split(new[] { "variable.", " == " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;

            var variableName = parts[0];
            var expectedValue = parts[1].Trim('"');

            var variableValue = context.GetVariable<string>(variableName);
            return variableValue == expectedValue;
        }

        private async Task<ValidationResult> ValidateRuleConditionAsync(ConditionExpression condition)
        {
            var errors = new List<string>();
            var expression = condition.Expression;

            if (string.IsNullOrWhiteSpace(expression))
            {
                errors.Add("条件表达式不能为空");
            }
            else if (!expression.Contains("recognition.") && !expression.Contains("variable."))
            {
                errors.Add("条件表达式必须包含 'recognition.' 或 'variable.' 前缀");
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }

    /// <summary>
    /// 简化实现的状态机引擎
    /// 
    /// 原本实现：支持复杂的状态转换、事件驱动、超时处理
    /// 简化实现：基本的状态转换逻辑
    /// </summary>
    public class SimplifiedStateMachineEngine : IStateMachineEngine
    {
        private readonly Dictionary<StateMachineId, StateMachine> _stateMachines = new();
        private readonly KeyForge.Core.Domain.Common.ILogger _logger;

        public SimplifiedStateMachineEngine(KeyForge.Core.Domain.Common.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StateMachineResult> ProcessAsync(StateMachineId stateMachineId, JudgmentContext context)
        {
            if (!_stateMachines.TryGetValue(stateMachineId, out var stateMachine))
            {
                return new StateMachineResult(
                    "Unknown",
                    new List<string>(),
                    false
                );
            }

            try
            {
                // 简化实现：基于当前状态和上下文决定转换
                var currentState = stateMachine.CurrentState;
                var triggeredActions = new List<string>();

                // 检查是否有识别成功的结果
                var hasSuccessfulRecognition = context.GetAllSenseResults()
                    .Any(sr => sr.RecognitionResults.Any(rr => rr.IsSuccessful()));

                if (hasSuccessfulRecognition)
                {
                    // 转换到成功状态
                    stateMachine.TransitionTo(new StateId(Guid.NewGuid()), context);
                    triggeredActions.Add("handle_success");
                }
                else
                {
                    // 转换到失败状态
                    triggeredActions.Add("handle_failure");
                }

                return new StateMachineResult(
                    currentState.Id.ToString(),
                    triggeredActions,
                    true
                );
            }
            catch (Exception ex)
            {
                _logger.Error($"状态机处理失败: {stateMachineId} - {ex.Message}");
                return new StateMachineResult(
                    "Error",
                    new List<string>(),
                    false
                );
            }
        }

        public async Task CreateStateMachineAsync(StateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException(nameof(stateMachine));

            _stateMachines[stateMachine.Id] = stateMachine;
            _logger.Info($"创建状态机: {stateMachine.Name.Value}");
        }

        public async Task UpdateStateMachineAsync(StateMachineId stateMachineId, StateMachine newStateMachine)
        {
            if (!_stateMachines.ContainsKey(stateMachineId))
            {
                throw new ArgumentException($"状态机不存在: {stateMachineId}");
            }

            _stateMachines[stateMachineId] = newStateMachine;
            _logger.Info($"更新状态机: {stateMachineId}");
        }
    }

    /// <summary>
    /// 决策规则实体
    /// </summary>
    public class DecisionRule : Entity
    {
        public RuleName Name { get; private set; }
        public ConditionExpression Condition { get; private set; }
        public List<string> ThenActions { get; private set; }
        public List<string> ElseActions { get; private set; }
        public RulePriority Priority { get; private set; }
        public RuleStatus Status { get; private set; }

        public DecisionRule(
            RuleName name,
            ConditionExpression condition,
            List<string> thenActions,
            List<string> elseActions,
            RulePriority priority)
        {
            Id = new RuleId(Guid.NewGuid());
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenActions = thenActions ?? new List<string>();
            ElseActions = elseActions ?? new List<string>();
            Priority = priority;
            Status = RuleStatus.Active;
        }

        public void UpdateStatus(RuleStatus newStatus)
        {
            Status = newStatus;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// 规则名称 - 简化实现
    /// </summary>
    public record RuleName(string Value);

    /// <summary>
    /// 条件表达式 - 简化实现
    /// </summary>
    public record ConditionExpression(string Expression);

    /// <summary>
    /// 规则优先级 - 简化实现
    /// </summary>
    public record RulePriority(int Value)
    {
        public static RulePriority High => new RulePriority(90);
        public static RulePriority Medium => new RulePriority(50);
        public static RulePriority Low => new RulePriority(10);
    }

    /// <summary>
    /// 规则状态
    /// </summary>
    public enum RuleStatus
    {
        Active = 0,
        Inactive = 1,
        Error = 2
    }

    /// <summary>
    /// 状态机实体
    /// </summary>
    public class StateMachine : Entity
    {
        public MachineName Name { get; private set; }
        public State CurrentState { get; private set; }
        public List<State> States { get; private set; }
        public List<Transition> Transitions { get; private set; }

        public StateMachine(
            MachineName name,
            State initialState,
            List<State> states,
            List<Transition> transitions)
        {
            Id = new StateMachineId(Guid.NewGuid());
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CurrentState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            States = states ?? new List<State>();
            Transitions = transitions ?? new List<Transition>();
        }

        public void TransitionTo(StateId targetState, JudgmentContext context)
        {
            // 简化实现：直接设置当前状态
            // 在实际实现中，应该检查转换条件并执行转换逻辑
            var targetStateObj = States.FirstOrDefault(s => s.Id == targetState);
            if (targetStateObj != null)
            {
                CurrentState = targetStateObj;
                MarkAsUpdated();
            }
        }
    }

    /// <summary>
    /// 机器名称 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record MachineName(string Value);

    /// <summary>
    /// 状态值对象 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record State(StateId Id, StateName Name, List<string> EntryActions);

    /// <summary>
    /// 状态名称 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record StateName(string Value);

    /// <summary>
    /// 状态ID - 简化实现
    /// 原本实现：继承StronglyTypedId
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record StateId(Guid Value);

    /// <summary>
    /// 转换 - 简化实现
    /// 原本实现：继承Domain层的ValueObject
    /// 简化实现：直接使用record类型，避免复杂的继承结构
    /// </summary>
    public record Transition(StateId FromState, StateId ToState, string Condition, List<string> Actions);

    }