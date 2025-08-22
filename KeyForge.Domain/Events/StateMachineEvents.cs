using System;

namespace KeyForge.Domain.Events
{
    /// <summary>
    /// 状态机创建事件
    /// </summary>
    public class StateMachineCreatedEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public string StateMachineName { get; }

        public StateMachineCreatedEvent(Guid stateMachineId, string stateMachineName)
        {
            StateMachineId = stateMachineId;
            StateMachineName = stateMachineName;
        }
    }

    /// <summary>
    /// 状态机激活事件
    /// </summary>
    public class StateMachineActivatedEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public string StateMachineName { get; }

        public StateMachineActivatedEvent(Guid stateMachineId, string stateMachineName)
        {
            StateMachineId = stateMachineId;
            StateMachineName = stateMachineName;
        }
    }

    /// <summary>
    /// 状态机停用事件
    /// </summary>
    public class StateMachineDeactivatedEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public string StateMachineName { get; }

        public StateMachineDeactivatedEvent(Guid stateMachineId, string stateMachineName)
        {
            StateMachineId = stateMachineId;
            StateMachineName = stateMachineName;
        }
    }

    /// <summary>
    /// 状态机重置事件
    /// </summary>
    public class StateMachineResetEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public string StateMachineName { get; }

        public StateMachineResetEvent(Guid stateMachineId, string stateMachineName)
        {
            StateMachineId = stateMachineId;
            StateMachineName = stateMachineName;
        }
    }

    /// <summary>
    /// 状态转换事件
    /// </summary>
    public class StateTransitionEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public Guid FromStateId { get; }
        public Guid ToStateId { get; }
        public string Reason { get; }

        public StateTransitionEvent(Guid stateMachineId, Guid fromStateId, Guid toStateId, string reason)
        {
            StateMachineId = stateMachineId;
            FromStateId = fromStateId;
            ToStateId = toStateId;
            Reason = reason;
        }
    }

    /// <summary>
    /// 规则触发事件
    /// </summary>
    public class RuleTriggeredEvent : DomainEvent
    {
        public Guid StateMachineId { get; }
        public Guid RuleId { get; }
        public string RuleName { get; }

        public RuleTriggeredEvent(Guid stateMachineId, Guid ruleId, string ruleName)
        {
            StateMachineId = stateMachineId;
            RuleId = ruleId;
            RuleName = ruleName;
        }
    }
}