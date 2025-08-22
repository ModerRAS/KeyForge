using System;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 键盘动作值对象
    /// 
    /// 原本实现：支持复杂的键盘事件和组合键
    /// 简化实现：基本的按键动作
    /// </summary>
    public readonly struct KeyboardAction : IEquatable<KeyboardAction>
    {
        public ActionId Id { get; }
        public Timestamp Timestamp { get; }
        public ActionDelay Delay { get; }
        public VirtualKeyCode KeyCode { get; }
        public KeyState State { get; }
        
        public KeyboardAction(ActionId id, Timestamp timestamp, ActionDelay delay, VirtualKeyCode keyCode, KeyState state)
        {
            Id = id;
            Timestamp = timestamp;
            Delay = delay;
            KeyCode = keyCode;
            State = state;
        }
        
        public bool Equals(KeyboardAction other) => 
            Id.Equals(other.Id) && 
            Timestamp.Equals(other.Timestamp) && 
            Delay.Equals(other.Delay) && 
            KeyCode == other.KeyCode && 
            State == other.State;
        
        public override bool Equals(object obj) => obj is KeyboardAction other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Timestamp, Delay, KeyCode, State);
        
        public static bool operator ==(KeyboardAction left, KeyboardAction right) => left.Equals(right);
        public static bool operator !=(KeyboardAction left, KeyboardAction right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 鼠标动作值对象
    /// 
    /// 原本实现：支持复杂的鼠标事件和手势
    /// 简化实现：基本的鼠标动作
    /// </summary>
    public readonly struct MouseAction : IEquatable<MouseAction>
    {
        public ActionId Id { get; }
        public Timestamp Timestamp { get; }
        public ActionDelay Delay { get; }
        public MouseActionType ActionType { get; }
        public MouseButton Button { get; }
        public ScreenLocation Location { get; }
        
        public MouseAction(ActionId id, Timestamp timestamp, ActionDelay delay, MouseActionType actionType, MouseButton button, ScreenLocation location)
        {
            Id = id;
            Timestamp = timestamp;
            Delay = delay;
            ActionType = actionType;
            Button = button;
            Location = location;
        }
        
        public bool Equals(MouseAction other) => 
            Id.Equals(other.Id) && 
            Timestamp.Equals(other.Timestamp) && 
            Delay.Equals(other.Delay) && 
            ActionType == other.ActionType && 
            Button == other.Button && 
            Location.Equals(other.Location);
        
        public override bool Equals(object obj) => obj is MouseAction other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Timestamp, Delay, ActionType, Button, Location);
        
        public static bool operator ==(MouseAction left, MouseAction right) => left.Equals(right);
        public static bool operator !=(MouseAction left, MouseAction right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 延迟动作值对象
    /// 
    /// 原本实现：支持复杂的延迟策略和随机延迟
    /// 简化实现：基本的固定延迟
    /// </summary>
    public readonly struct DelayAction : IEquatable<DelayAction>
    {
        public ActionId Id { get; }
        public Timestamp Timestamp { get; }
        public ActionDelay Delay { get; }
        public Duration Duration { get; }
        
        public DelayAction(ActionId id, Timestamp timestamp, ActionDelay delay, Duration duration)
        {
            Id = id;
            Timestamp = timestamp;
            Delay = delay;
            Duration = duration;
        }
        
        public bool Equals(DelayAction other) => 
            Id.Equals(other.Id) && 
            Timestamp.Equals(other.Timestamp) && 
            Delay.Equals(other.Delay) && 
            Duration.Equals(other.Duration);
        
        public override bool Equals(object obj) => obj is DelayAction other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Timestamp, Delay, Duration);
        
        public static bool operator ==(DelayAction left, DelayAction right) => left.Equals(right);
        public static bool operator !=(DelayAction left, DelayAction right) => !left.Equals(right);
    }
}