using System;

namespace KeyForge.Domain.Exceptions
{
    /// <summary>
    /// 领域异常基类
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 实体未找到异常
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityType, Guid id) 
            : base($"Entity of type '{entityType}' with id '{id}' was not found.") { }
    }

    /// <summary>
    /// 业务规则违反异常
    /// </summary>
    public class BusinessRuleViolationException : DomainException
    {
        public BusinessRuleViolationException(string message) : base(message) { }
    }

    /// <summary>
    /// 验证失败异常
    /// </summary>
    public class ValidationException : DomainException
    {
        public ValidationException(string message) : base(message) { }
    }
}