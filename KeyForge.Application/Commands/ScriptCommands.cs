using System;
using MediatR;
using KeyForge.Application.DTOs;

namespace KeyForge.Application.Commands
{
    /// <summary>
    /// 创建脚本命令
    /// </summary>
    public class CreateScriptCommand : IRequest<ScriptDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GameActionDto[] Actions { get; set; } = Array.Empty<GameActionDto>();
    }

    /// <summary>
    /// 更新脚本命令
    /// </summary>
    public class UpdateScriptCommand : IRequest<ScriptDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 删除脚本命令
    /// </summary>
    public class DeleteScriptCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 激活脚本命令
    /// </summary>
    public class ActivateScriptCommand : IRequest<ScriptDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 停用脚本命令
    /// </summary>
    public class DeactivateScriptCommand : IRequest<ScriptDto>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 添加动作到脚本命令
    /// </summary>
    public class AddActionToScriptCommand : IRequest<ScriptDto>
    {
        public Guid ScriptId { get; set; }
        public GameActionDto Action { get; set; }
    }

    /// <summary>
    /// 从脚本中移除动作命令
    /// </summary>
    public class RemoveActionFromScriptCommand : IRequest<ScriptDto>
    {
        public Guid ScriptId { get; set; }
        public Guid ActionId { get; set; }
    }

    /// <summary>
    /// 执行脚本命令
    /// </summary>
    public class ExecuteScriptCommand : IRequest<Guid>
    {
        public Guid ScriptId { get; set; }
    }

    /// <summary>
    /// 创建图像模板命令
    /// </summary>
    public class CreateImageTemplateCommand : IRequest<ImageTemplateDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] TemplateData { get; set; }
        public RectangleDto TemplateArea { get; set; }
        public double MatchThreshold { get; set; }
        public TemplateTypeDto TemplateType { get; set; }
    }

    /// <summary>
    /// 更新图像模板命令
    /// </summary>
    public class UpdateImageTemplateCommand : IRequest<ImageTemplateDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RectangleDto TemplateArea { get; set; }
        public double MatchThreshold { get; set; }
    }

    /// <summary>
    /// 删除图像模板命令
    /// </summary>
    public class DeleteImageTemplateCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 创建状态机命令
    /// </summary>
    public class CreateStateMachineCommand : IRequest<StateMachineDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 更新状态机命令
    /// </summary>
    public class UpdateStateMachineCommand : IRequest<StateMachineDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 删除状态机命令
    /// </summary>
    public class DeleteStateMachineCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 激活状态机命令
    /// </summary>
    public class ActivateStateMachineCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 停用状态机命令
    /// </summary>
    public class DeactivateStateMachineCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 状态转换命令
    /// </summary>
    public class TransitionStateCommand : IRequest<bool>
    {
        public Guid StateMachineId { get; set; }
        public Guid ToStateId { get; set; }
        public string Reason { get; set; }
    }
}