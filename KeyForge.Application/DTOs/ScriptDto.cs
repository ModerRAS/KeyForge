using System;
using System.Collections.Generic;

namespace KeyForge.Application.DTOs
{
    /// <summary>
    /// 脚本DTO
    /// </summary>
    public class ScriptDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ScriptStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Version { get; set; }
        public List<GameActionDto> Actions { get; set; } = new();
        public int ActionCount => Actions.Count;
        public TimeSpan EstimatedDuration { get; set; }
    }

    /// <summary>
    /// 游戏动作DTO
    /// </summary>
    public class GameActionDto
    {
        public Guid Id { get; set; }
        public ActionTypeDto Type { get; set; }
        public KeyCodeDto Key { get; set; }
        public MouseButtonDto Button { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Delay { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 图像模板DTO
    /// </summary>
    public class ImageTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] TemplateData { get; set; }
        public RectangleDto TemplateArea { get; set; }
        public double MatchThreshold { get; set; }
        public TemplateTypeDto TemplateType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Version { get; set; }
    }

    /// <summary>
    /// 状态机DTO
    /// </summary>
    public class StateMachineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public StateMachineStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Version { get; set; }
        public List<StateDto> States { get; set; } = new();
        public List<StateTransitionDto> Transitions { get; set; } = new();
        public List<DecisionRuleDto> Rules { get; set; } = new();
        public StateDto CurrentState { get; set; }
    }

    /// <summary>
    /// 状态DTO
    /// </summary>
    public class StateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 状态转换DTO
    /// </summary>
    public class StateTransitionDto
    {
        public Guid Id { get; set; }
        public Guid FromStateId { get; set; }
        public Guid ToStateId { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 决策规则DTO
    /// </summary>
    public class DecisionRuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public Guid ActionId { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建脚本请求DTO
    /// </summary>
    public class CreateScriptRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GameActionDto> Actions { get; set; } = new();
    }

    /// <summary>
    /// 更新脚本请求DTO
    /// </summary>
    public class UpdateScriptRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 创建图像模板请求DTO
    /// </summary>
    public class CreateImageTemplateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] TemplateData { get; set; }
        public RectangleDto TemplateArea { get; set; }
        public double MatchThreshold { get; set; }
        public TemplateTypeDto TemplateType { get; set; }
    }

    /// <summary>
    /// 创建状态机请求DTO
    /// </summary>
    public class CreateStateMachineRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<StateDto> States { get; set; } = new();
        public List<StateTransitionDto> Transitions { get; set; } = new();
        public List<DecisionRuleDto> Rules { get; set; } = new();
    }
}