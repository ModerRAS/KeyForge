using System;
using System.Collections.Generic;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Domain.Entities
{
    /// <summary>
    /// 状态实体
    /// </summary>
    public class State : Entity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Dictionary<string, object> Variables { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected State() { }

        public State(Guid id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Variables = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetVariable(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Domain.Exceptions.ValidationException("Variable key cannot be empty.");

            Variables[key] = value;
            UpdatedAt = DateTime.UtcNow;
        }

        public object GetVariable(string key)
        {
            if (Variables.TryGetValue(key, out var value))
                return value;

            return null;
        }

        public T GetVariable<T>(string key)
        {
            if (Variables.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;

            return default(T);
        }

        public bool HasVariable(string key)
        {
            return Variables.ContainsKey(key);
        }

        public void RemoveVariable(string key)
        {
            if (Variables.Remove(key))
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void ClearVariables()
        {
            Variables.Clear();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}