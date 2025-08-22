using System;

namespace KeyForge.Domain.Common
{
    /// <summary>
    /// 查询参数类 - 使用Domain.Interfaces的统一定义
    /// 原本实现：重复的QueryParameters定义
    /// 简化实现：使用Domain.Interfaces.QueryParameters的别名
    /// </summary>
    using QueryParameters = KeyForge.Domain.Interfaces.QueryParameters;
}