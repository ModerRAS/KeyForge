using System;
using System.Collections.Generic;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Events;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Aggregates
{
    /// <summary>
    /// 脚本聚合根
    /// 原本实现：ScriptStatus枚举重复定义
    /// 简化实现：使用KeyForge.Domain.Common.ScriptStatus统一定义
    /// </summary>
    public class Script : AggregateRoot
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ScriptStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Version { get; private set; }

        private readonly List<GameAction> _actions = new();
        public IReadOnlyCollection<GameAction> Actions => _actions.AsReadOnly();

        protected Script() { }

        public Script(Guid id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Script name cannot be empty.");

            Id = id;
            Name = name;
            Description = description;
            Status = ScriptStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Version = 1;

            AddDomainEvent(new ScriptCreatedEvent(Id, Name));
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Script name cannot be empty.");

            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptUpdatedEvent(Id, Name));
        }

        public void AddAction(GameAction action)
        {
            if (action == null)
                throw new ValidationException("Action cannot be null.");

            if (Status != ScriptStatus.Draft)
                throw new BusinessRuleViolationException("Can only add actions to draft scripts.");

            _actions.Add(action);
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptActionAddedEvent(Id, action.Id));
        }

        public void RemoveAction(Guid actionId)
        {
            var action = _actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
                throw new EntityNotFoundException(nameof(GameAction), actionId);

            if (Status != ScriptStatus.Draft)
                throw new BusinessRuleViolationException("Can only remove actions from draft scripts.");

            _actions.Remove(action);
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptActionRemovedEvent(Id, actionId));
        }

        public void Activate()
        {
            if (Status != ScriptStatus.Draft)
                throw new BusinessRuleViolationException("Only draft scripts can be activated.");

            if (!_actions.Any())
                throw new BusinessRuleViolationException("Cannot activate script with no actions.");

            Status = ScriptStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptActivatedEvent(Id, Name));
        }

        public void Deactivate()
        {
            if (Status != ScriptStatus.Active)
                throw new BusinessRuleViolationException("Only active scripts can be deactivated.");

            Status = ScriptStatus.Inactive; // 使用Common枚举中的Inactive替代Stopped
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptDeactivatedEvent(Id, Name));
        }

        public void Delete()
        {
            if (Status == ScriptStatus.Deleted)
                throw new BusinessRuleViolationException("Script is already deleted.");

            Status = ScriptStatus.Deleted; // 使用Common枚举中的Deleted替代Error
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ScriptDeletedEvent(Id, Name));
        }

        public ActionSequence GetActionSequence()
        {
            return new ActionSequence(_actions);
        }

        public TimeSpan GetEstimatedDuration()
        {
            var totalDelay = 0;
            foreach (var action in _actions)
            {
                totalDelay += action.Delay;
            }
            return TimeSpan.FromMilliseconds(totalDelay);
        }
    }
}