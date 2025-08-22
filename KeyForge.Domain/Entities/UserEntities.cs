using System;
using System.Collections.Generic;
using KeyForge.Domain.Common;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Domain.Entities
{
    /// <summary>
    /// 用户实体 - 简化实现
    /// 
    /// 原本实现：完整的用户管理，包括权限、角色等
    /// 简化实现：基本的用户信息
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public User(string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string email, string passwordHash = null)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            if (passwordHash != null)
            {
                PasswordHash = passwordHash;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 执行日志实体 - 简化实现
    /// 
    /// 原本实现：详细的执行日志，包括性能指标
    /// 简化实现：基本的执行记录
    /// </summary>
    public class ExecutionLog
    {
        public Guid Id { get; private set; }
        public ScriptId ScriptId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string ErrorMessage { get; private set; }
        public int ActionsExecuted { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }

        public ExecutionLog(
            ScriptId scriptId,
            DateTime startTime,
            DateTime endTime,
            bool isSuccessful,
            string errorMessage = null,
            int actionsExecuted = 0)
        {
            Id = Guid.NewGuid();
            ScriptId = scriptId;
            StartTime = startTime;
            EndTime = endTime;
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
            ActionsExecuted = actionsExecuted;
            Metadata = new Dictionary<string, object>();
        }

        public void AddMetadata(string key, object value)
        {
            Metadata[key] = value;
        }

        public TimeSpan GetDuration()
        {
            return EndTime - StartTime;
        }
    }
}