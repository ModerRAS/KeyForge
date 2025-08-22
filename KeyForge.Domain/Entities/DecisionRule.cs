using System;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Domain.Entities
{
    /// <summary>
    /// 决策规则实体
    /// </summary>
    public class DecisionRule : Entity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ConditionExpression Condition { get; private set; }
        public Guid ActionId { get; private set; }
        public int Priority { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected DecisionRule() { }

        public DecisionRule(Guid id, string name, string description, ConditionExpression condition, 
            Guid actionId, int priority = 0)
        {
            Id = id;
            Name = name;
            Description = description;
            Condition = condition;
            ActionId = actionId;
            Priority = priority;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, ConditionExpression condition, int priority)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Domain.Exceptions.ValidationException("Rule name cannot be empty.");

            Name = name;
            Description = description;
            Condition = condition;
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAction(Guid actionId)
        {
            ActionId = actionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool Evaluate(Func<string, object> valueResolver)
        {
            if (!IsActive)
                return false;

            return Condition.Evaluate(valueResolver);
        }

        public void IncreasePriority()
        {
            Priority++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecreasePriority()
        {
            if (Priority > 0)
            {
                Priority--;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}