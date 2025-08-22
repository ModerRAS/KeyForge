using System;

namespace KeyForge.Application.Exceptions
{
    /// <summary>
    /// 应用层异常基类
    /// </summary>
    public abstract class ApplicationException : Exception
    {
        protected ApplicationException(string message) : base(message) { }
        protected ApplicationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 验证异常
    /// </summary>
    public class ApplicationValidationException : ApplicationException
    {
        public ApplicationValidationException(string message) : base(message) { }
        public ApplicationValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 未找到异常
    /// </summary>
    public class ApplicationNotFoundException : ApplicationException
    {
        public ApplicationNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// 权限异常
    /// </summary>
    public class ApplicationForbiddenException : ApplicationException
    {
        public ApplicationForbiddenException(string message) : base(message) { }
    }

    /// <summary>
    /// 冲突异常
    /// </summary>
    public class ApplicationConflictException : ApplicationException
    {
        public ApplicationConflictException(string message) : base(message) { }
    }
}