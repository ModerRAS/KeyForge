using System;

namespace KeyForge.Domain.Events
{
    /// <summary>
    /// 脚本创建事件
    /// </summary>
    public class ScriptCreatedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }

        public ScriptCreatedEvent(Guid scriptId, string scriptName)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
        }
    }

    /// <summary>
    /// 脚本更新事件
    /// </summary>
    public class ScriptUpdatedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }

        public ScriptUpdatedEvent(Guid scriptId, string scriptName)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
        }
    }

    /// <summary>
    /// 脚本激活事件
    /// </summary>
    public class ScriptActivatedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }

        public ScriptActivatedEvent(Guid scriptId, string scriptName)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
        }
    }

    /// <summary>
    /// 脚本停用事件
    /// </summary>
    public class ScriptDeactivatedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }

        public ScriptDeactivatedEvent(Guid scriptId, string scriptName)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
        }
    }

    /// <summary>
    /// 脚本删除事件
    /// </summary>
    public class ScriptDeletedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }

        public ScriptDeletedEvent(Guid scriptId, string scriptName)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
        }
    }

    /// <summary>
    /// 脚本动作添加事件
    /// 原本实现：缺失的事件类型
    /// 简化实现：添加基础的动作事件
    /// </summary>
    public class ScriptActionAddedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public Guid ActionId { get; }

        public ScriptActionAddedEvent(Guid scriptId, Guid actionId)
        {
            ScriptId = scriptId;
            ActionId = actionId;
        }
    }

    /// <summary>
    /// 脚本动作移除事件
    /// </summary>
    public class ScriptActionRemovedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public Guid ActionId { get; }

        public ScriptActionRemovedEvent(Guid scriptId, Guid actionId)
        {
            ScriptId = scriptId;
            ActionId = actionId;
        }
    }

    /// <summary>
    /// 脚本执行开始事件
    /// </summary>
    public class ScriptExecutionStartedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }
        public Guid ExecutionId { get; }

        public ScriptExecutionStartedEvent(Guid scriptId, string scriptName, Guid executionId)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
            ExecutionId = executionId;
        }
    }

    /// <summary>
    /// 脚本执行完成事件
    /// </summary>
    public class ScriptExecutionCompletedEvent : DomainEvent
    {
        public Guid ScriptId { get; }
        public string ScriptName { get; }
        public Guid ExecutionId { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }

        public ScriptExecutionCompletedEvent(Guid scriptId, string scriptName, Guid executionId, bool success, string errorMessage = null)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
            ExecutionId = executionId;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}