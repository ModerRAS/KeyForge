using System;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 规则ID值对象
    /// 原本实现：复杂的规则ID处理
    /// 简化实现：基础的规则ID
    /// </summary>
    public readonly struct RuleId : IEquatable<RuleId>
    {
        public Guid Value { get; }
        
        public RuleId(Guid value)
        {
            Value = value;
        }
        
        public static RuleId New() => new RuleId(Guid.NewGuid());
        public static RuleId From(Guid value) => new RuleId(value);
        
        public bool Equals(RuleId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is RuleId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(RuleId left, RuleId right) => left.Equals(right);
        public static bool operator !=(RuleId left, RuleId right) => !left.Equals(right);
        
        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// 状态机ID值对象
    /// 原本实现：复杂的状态机ID处理
    /// 简化实现：基础的状态机ID
    /// </summary>
    public readonly struct StateMachineId : IEquatable<StateMachineId>
    {
        public Guid Value { get; }
        
        public StateMachineId(Guid value)
        {
            Value = value;
        }
        
        public static StateMachineId New() => new StateMachineId(Guid.NewGuid());
        public static StateMachineId From(Guid value) => new StateMachineId(value);
        
        public bool Equals(StateMachineId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is StateMachineId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(StateMachineId left, StateMachineId right) => left.Equals(right);
        public static bool operator !=(StateMachineId left, StateMachineId right) => !left.Equals(right);
        
        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// 脚本ID值对象
    /// 原本实现：复杂的脚本ID处理
    /// 简化实现：基础的脚本ID
    /// </summary>
    public readonly struct ScriptId : IEquatable<ScriptId>
    {
        public Guid Value { get; }
        
        public ScriptId(Guid value)
        {
            Value = value;
        }
        
        public static ScriptId New() => new ScriptId(Guid.NewGuid());
        public static ScriptId From(Guid value) => new ScriptId(value);
        
        public bool Equals(ScriptId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is ScriptId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ScriptId left, ScriptId right) => left.Equals(right);
        public static bool operator !=(ScriptId left, ScriptId right) => !left.Equals(right);
        
        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// 图像模板ID值对象
    /// 原本实现：复杂的图像模板ID处理
    /// 简化实现：基础的图像模板ID
    /// </summary>
    public readonly struct ImageTemplateId : IEquatable<ImageTemplateId>
    {
        public Guid Value { get; }
        
        public ImageTemplateId(Guid value)
        {
            Value = value;
        }
        
        public static ImageTemplateId New() => new ImageTemplateId(Guid.NewGuid());
        public static ImageTemplateId From(Guid value) => new ImageTemplateId(value);
        
        public bool Equals(ImageTemplateId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is ImageTemplateId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ImageTemplateId left, ImageTemplateId right) => left.Equals(right);
        public static bool operator !=(ImageTemplateId left, ImageTemplateId right) => !left.Equals(right);
        
        public override string ToString() => Value.ToString();
    }

    // ActionId已经在BasicTypes.cs中定义，避免重复定义

    /// <summary>
    /// 序列ID值对象
    /// 原本实现：复杂的序列ID处理
    /// 简化实现：基础的序列ID
    /// </summary>
    public readonly struct SequenceId : IEquatable<SequenceId>
    {
        public Guid Value { get; }
        
        public SequenceId(Guid value)
        {
            Value = value;
        }
        
        public static SequenceId New() => new SequenceId(Guid.NewGuid());
        public static SequenceId From(Guid value) => new SequenceId(value);
        
        public bool Equals(SequenceId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is SequenceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(SequenceId left, SequenceId right) => left.Equals(right);
        public static bool operator !=(SequenceId left, SequenceId right) => !left.Equals(right);
        
        public override string ToString() => Value.ToString();
    }
}