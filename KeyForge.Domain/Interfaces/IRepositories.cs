using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Common;
using KeyForge.Domain.Entities;

namespace KeyForge.Domain.Interfaces
{
    /// <summary>
    /// 查询参数类
    /// </summary>
    public class QueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
    }

    /// <summary>
    /// 脚本仓储接口
    /// 原本实现：复杂的接口定义和依赖
    /// 简化实现：只保留核心功能接口
    /// </summary>
    public interface IScriptRepository
    {
        Task<Script> GetByIdAsync(Guid id);
        Task<Script> GetByNameAsync(string name);
        Task<IEnumerable<Script>> GetAllAsync();
        Task<IEnumerable<Script>> GetByStatusAsync(ScriptStatus status);
        Task<PagedResult<Script>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(Script script);
        Task UpdateAsync(Script script);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 图像模板仓储接口
    /// </summary>
    public interface IImageTemplateRepository
    {
        Task<ImageTemplate> GetByIdAsync(Guid id);
        Task<ImageTemplate> GetByNameAsync(string name);
        Task<IEnumerable<ImageTemplate>> GetAllAsync();
        Task<IEnumerable<ImageTemplate>> GetByTemplateTypeAsync(TemplateType templateType);
        Task<IEnumerable<ImageTemplate>> GetActiveTemplatesAsync();
        Task<PagedResult<ImageTemplate>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(ImageTemplate template);
        Task UpdateAsync(ImageTemplate template);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 状态机仓储接口
    /// </summary>
    public interface IStateMachineRepository
    {
        Task<StateMachine> GetByIdAsync(Guid id);
        Task<StateMachine> GetByNameAsync(string name);
        Task<IEnumerable<StateMachine>> GetAllAsync();
        Task<IEnumerable<StateMachine>> GetByStatusAsync(StateMachineStatus status);
        Task<PagedResult<StateMachine>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(StateMachine stateMachine);
        Task UpdateAsync(StateMachine stateMachine);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 游戏动作仓储接口
    /// </summary>
    public interface IGameActionRepository
    {
        Task<GameAction> GetByIdAsync(Guid id);
        Task<IEnumerable<GameAction>> GetByScriptIdAsync(Guid scriptId);
        Task<IEnumerable<GameAction>> GetByActionTypeAsync(ActionType actionType);
        Task<PagedResult<GameAction>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(GameAction action);
        Task UpdateAsync(GameAction action);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 决策规则仓储接口
    /// </summary>
    public interface IDecisionRuleRepository
    {
        Task<DecisionRule> GetByIdAsync(Guid id);
        Task<IEnumerable<DecisionRule>> GetByStateMachineIdAsync(Guid stateMachineId);
        Task<IEnumerable<DecisionRule>> GetAllAsync();
        Task<PagedResult<DecisionRule>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(DecisionRule rule);
        Task UpdateAsync(DecisionRule rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 状态仓储接口
    /// </summary>
    public interface IStateRepository
    {
        Task<State> GetByIdAsync(Guid id);
        Task<IEnumerable<State>> GetByStateMachineIdAsync(Guid stateMachineId);
        Task<IEnumerable<State>> GetAllAsync();
        Task<PagedResult<State>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(State state);
        Task UpdateAsync(State state);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 日志仓储接口
    /// </summary>
    public interface ILogRepository
    {
        Task<LogEntry> GetByIdAsync(Guid id);
        Task<IEnumerable<LogEntry>> GetByLevelAsync(LogLevel level);
        Task<IEnumerable<LogEntry>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<PagedResult<LogEntry>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(LogEntry logEntry);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
        Task ClearAsync();
    }

    /// <summary>
    /// 配置仓储接口
    /// </summary>
    public interface IConfigurationRepository
    {
        Task<string> GetAsync(string key);
        Task<T> GetAsync<T>(string key);
        Task SetAsync(string key, string value);
        Task SetAsync<T>(string key, T value);
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key);
        Task<IEnumerable<string>> GetAllKeysAsync();
    }

    /// <summary>
    /// 文件仓储接口
    /// </summary>
    public interface IFileRepository
    {
        Task<string> SaveAsync(string fileName, byte[] data);
        Task<byte[]> LoadAsync(string fileName);
        Task<bool> ExistsAsync(string fileName);
        Task DeleteAsync(string fileName);
        Task<IEnumerable<string>> GetAllFilesAsync();
    }

    /// <summary>
    /// 缓存仓储接口
    /// </summary>
    public interface ICacheRepository
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key);
        Task ClearAsync();
    }

    /// <summary>
    /// 通用仓储接口
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IScriptRepository Scripts { get; }
        IImageTemplateRepository ImageTemplates { get; }
        IStateMachineRepository StateMachines { get; }
        IGameActionRepository GameActions { get; }
        IDecisionRuleRepository DecisionRules { get; }
        IStateRepository States { get; }
        ILogRepository Logs { get; }
        IConfigurationRepository Configuration { get; }
        IFileRepository Files { get; }
        ICacheRepository Cache { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}