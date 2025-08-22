using System;
using MediatR;
using KeyForge.Application.DTOs;

namespace KeyForge.Application.Queries
{
    /// <summary>
    /// 获取脚本查询
    /// </summary>
    public class GetScriptQuery : IRequest<ScriptDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 获取所有脚本查询
    /// </summary>
    public class GetAllScriptsQuery : IRequest<ScriptDto[]>
    {
        public ScriptStatusDto? Status { get; set; }
    }

    /// <summary>
    /// 获取脚本详情查询
    /// </summary>
    public class GetScriptDetailsQuery : IRequest<ScriptDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 获取图像模板查询
    /// </summary>
    public class GetImageTemplateQuery : IRequest<ImageTemplateDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 获取所有图像模板查询
    /// </summary>
    public class GetAllImageTemplatesQuery : IRequest<ImageTemplateDto[]>
    {
        public bool? ActiveOnly { get; set; }
        public TemplateTypeDto? TemplateType { get; set; }
    }

    /// <summary>
    /// 获取状态机查询
    /// </summary>
    public class GetStateMachineQuery : IRequest<StateMachineDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 获取所有状态机查询
    /// </summary>
    public class GetAllStateMachinesQuery : IRequest<StateMachineDto[]>
    {
        public StateMachineStatusDto? Status { get; set; }
    }

    /// <summary>
    /// 获取状态机状态查询
    /// </summary>
    public class GetStateMachineStateQuery : IRequest<StateDto>
    {
        public Guid StateMachineId { get; set; }
    }

    /// <summary>
    /// 获取可用状态转换查询
    /// </summary>
    public class GetAvailableTransitionsQuery : IRequest<StateTransitionDto[]>
    {
        public Guid StateMachineId { get; set; }
    }

    /// <summary>
    /// 搜索脚本查询
    /// </summary>
    public class SearchScriptsQuery : IRequest<ScriptDto[]>
    {
        public string SearchTerm { get; set; }
        public ScriptStatusDto? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 获取执行历史查询
    /// </summary>
    public class GetExecutionHistoryQuery : IRequest<ExecutionHistoryDto[]>
    {
        public Guid? ScriptId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 获取系统统计查询
    /// </summary>
    public class GetSystemStatsQuery : IRequest<SystemStatsDto>
    {
    }
}