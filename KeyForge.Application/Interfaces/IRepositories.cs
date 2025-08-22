using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Interfaces;

namespace KeyForge.Application.Interfaces
{
    /// <summary>
    /// 应用层脚本仓储接口
    /// 继承自Domain层的接口，添加应用层特有的方法
    /// </summary>
    public interface IScriptRepository : KeyForge.Domain.Interfaces.IScriptRepository
    {
        Task<int> SaveChangesAsync();
    }
}