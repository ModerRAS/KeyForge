using System.ComponentModel.DataAnnotations;
using KeyForge.Domain.Common;

namespace KeyForge.Core.Domain.Common
{
    /// <summary>
    /// Core层基础类型定义 - 使用Domain层的统一定义
    /// 这是简化实现，避免重复定义和循环依赖
    /// Core层不再重复定义基础类型，直接使用Domain层的定义
    /// </summary>
    
    /// <summary>
    /// Core层特有的实体基类（如果需要）
    /// </summary>
    public abstract class CoreEntity : Entity
    {
        protected CoreEntity() : base()
        {
        }
        
        protected CoreEntity(Guid id) : base(id)
        {
        }
    }
    
    /// <summary>
    /// Core层特有的值对象基类（如果需要）
    /// </summary>
    public abstract record CoreValueObject;
}