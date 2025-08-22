using System;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 脚本名称值对象
    /// 
    /// 原本实现：复杂的脚本名称验证和规范化
    /// 简化实现：基本的脚本名称验证
    /// </summary>
    public readonly struct ScriptName : IEquatable<ScriptName>
    {
        public string Value { get; }
        
        public ScriptName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("脚本名称不能为空", nameof(value));
            
            Value = value.Trim();
        }
        
        public bool Equals(ScriptName other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ScriptName other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ScriptName left, ScriptName right) => left.Equals(right);
        public static bool operator !=(ScriptName left, ScriptName right) => !left.Equals(right);
        
        public override string ToString() => Value;
    }

    /// <summary>
    /// 脚本描述值对象
    /// 
    /// 原本实现：复杂的脚本描述处理
    /// 简化实现：基本的脚本描述
    /// </summary>
    public readonly struct ScriptDescription : IEquatable<ScriptDescription>
    {
        public string Value { get; }
        
        public ScriptDescription(string value)
        {
            Value = value ?? string.Empty;
        }
        
        public bool Equals(ScriptDescription other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ScriptDescription other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ScriptDescription left, ScriptDescription right) => left.Equals(right);
        public static bool operator !=(ScriptDescription left, ScriptDescription right) => !left.Equals(right);
        
        public override string ToString() => Value;
    }

    /// <summary>
    /// 脚本元数据值对象
    /// 
    /// 原本实现：复杂的脚本元数据管理
    /// 简化实现：基本的脚本元数据
    /// </summary>
    public readonly struct ScriptMetadata : IEquatable<ScriptMetadata>
    {
        public ScriptName Name { get; }
        public ScriptDescription Description { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
        public string Version { get; }
        public Dictionary<string, string> Tags { get; }
        
        public ScriptMetadata(
            ScriptName name,
            ScriptDescription description,
            DateTime createdAt,
            DateTime updatedAt,
            string version,
            Dictionary<string, string>? tags = null)
        {
            Name = name;
            Description = description;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Version = version ?? "1.0.0";
            Tags = tags ?? new Dictionary<string, string>();
        }
        
        public bool Equals(ScriptMetadata other) => 
            Name == other.Name && 
            Description == other.Description && 
            Version == other.Version;
        
        public override bool Equals(object obj) => obj is ScriptMetadata other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Name, Description, Version);
        
        public static bool operator ==(ScriptMetadata left, ScriptMetadata right) => left.Equals(right);
        public static bool operator !=(ScriptMetadata left, ScriptMetadata right) => !left.Equals(right);
    }
}