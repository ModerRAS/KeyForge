using System;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 持续时间值对象
    /// 
    /// 原本实现：完整的时间操作，包含各种时间单位转换
    /// 简化实现：基本的毫秒级时间处理
    /// </summary>
    public readonly struct Duration : IEquatable<Duration>
    {
        public long TotalMilliseconds { get; }
        
        public Duration(long milliseconds)
        {
            TotalMilliseconds = milliseconds;
        }
        
        public static Duration FromMilliseconds(long milliseconds) => new Duration(milliseconds);
        public static Duration FromSeconds(long seconds) => new Duration(seconds * 1000);
        public static Duration FromMinutes(long minutes) => new Duration(minutes * 60 * 1000);
        
        public static Duration Zero => new Duration(0);
        
        public TimeSpan ToTimeSpan() => TimeSpan.FromMilliseconds(TotalMilliseconds);
        
        public bool Equals(Duration other) => TotalMilliseconds == other.TotalMilliseconds;
        public override bool Equals(object obj) => obj is Duration other && Equals(other);
        public override int GetHashCode() => TotalMilliseconds.GetHashCode();
        
        public static bool operator ==(Duration left, Duration right) => left.Equals(right);
        public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 时间戳值对象
    /// 
    /// 原本实现：高精度时间戳，支持多种时间格式
    /// 简化实现：基本的UTC时间戳
    /// </summary>
    public readonly struct Timestamp : IEquatable<Timestamp>
    {
        public DateTime Value { get; }
        
        public Timestamp(DateTime value)
        {
            Value = value;
        }
        
        public static Timestamp Now => new Timestamp(DateTime.UtcNow);
        public static Timestamp FromDateTime(DateTime dateTime) => new Timestamp(dateTime);
        
        public bool Equals(Timestamp other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Timestamp other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(Timestamp left, Timestamp right) => left.Equals(right);
        public static bool operator !=(Timestamp left, Timestamp right) => !left.Equals(right);
        
        public static implicit operator DateTime(Timestamp timestamp) => timestamp.Value;
        public static implicit operator Timestamp(DateTime dateTime) => new Timestamp(dateTime);
    }
    
    /// <summary>
    /// 动作延迟值对象
    /// 
    /// 原本实现：复杂的延迟策略，支持随机延迟
    /// 简化实现：基本的固定延迟
    /// </summary>
    public readonly struct ActionDelay : IEquatable<ActionDelay>
    {
        public Duration Value { get; }
        
        public ActionDelay(Duration value)
        {
            Value = value;
        }
        
        public static ActionDelay Zero => new ActionDelay(Duration.Zero);
        public static ActionDelay FromMilliseconds(long milliseconds) => new ActionDelay(Duration.FromMilliseconds(milliseconds));
        
        public bool Equals(ActionDelay other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is ActionDelay other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ActionDelay left, ActionDelay right) => left.Equals(right);
        public static bool operator !=(ActionDelay left, ActionDelay right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 动作标识值对象
    /// 
    /// 原本实现：支持多种动作标识策略
    /// 简化实现：基本的GUID标识
    /// </summary>
    public readonly struct ActionId : IEquatable<ActionId>
    {
        public Guid Value { get; }
        
        public ActionId(Guid value)
        {
            Value = value;
        }
        
        public static ActionId New() => new ActionId(Guid.NewGuid());
        public static ActionId From(Guid value) => new ActionId(value);
        
        public bool Equals(ActionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ActionId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ActionId left, ActionId right) => left.Equals(right);
        public static bool operator !=(ActionId left, ActionId right) => !left.Equals(right);
        
        public static implicit operator Guid(ActionId id) => id.Value;
        public static implicit operator ActionId(Guid value) => new ActionId(value);
    }
}