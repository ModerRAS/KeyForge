using System;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Entities
{
    /// <summary>
    /// 游戏动作实体
    /// 原本实现：重复定义了ActionType、KeyCode、MouseButton枚举
    /// 简化实现：使用KeyForge.Domain.Common中的统一定义
    /// </summary>
    public class GameAction : Entity
    {
        public ActionType Type { get; private set; }
        public KeyCode Key { get; private set; }
        public MouseButton Button { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Delay { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Description { get; private set; }

        protected GameAction() { }

        public GameAction(Guid id, ActionType type, int delay = 0, string description = null)
        {
            Id = id;
            Type = type;
            Delay = delay;
            Timestamp = DateTime.UtcNow;
            Description = description ?? string.Empty;
        }

        public GameAction(Guid id, ActionType type, KeyCode key, int delay = 0, string description = null)
        {
            Id = id;
            Type = type;
            Key = key;
            Delay = delay;
            Timestamp = DateTime.UtcNow;
            Description = description ?? string.Empty;
        }

        public GameAction(Guid id, ActionType type, MouseButton button, int x, int y, int delay = 0, string description = null)
        {
            Id = id;
            Type = type;
            Button = button;
            X = x;
            Y = y;
            Delay = delay;
            Timestamp = DateTime.UtcNow;
            Description = description ?? string.Empty;
        }

        public void UpdatePosition(int x, int y)
        {
            if (Type != ActionType.MouseMove && Type != ActionType.MouseDown && Type != ActionType.MouseUp)
                throw new Domain.Exceptions.BusinessRuleViolationException("Cannot update position for non-mouse actions.");

            X = x;
            Y = y;
            Timestamp = DateTime.UtcNow;
        }

        public void UpdateDelay(int delay)
        {
            if (delay < 0)
                throw new Domain.Exceptions.ValidationException("Delay cannot be negative.");

            Delay = delay;
            Timestamp = DateTime.UtcNow;
        }

        public void UpdateDescription(string description)
        {
            Description = description ?? string.Empty;
        }

        public bool IsKeyboardAction => Type == ActionType.KeyDown || Type == ActionType.KeyUp;
        public bool IsMouseAction => Type == ActionType.MouseDown || Type == ActionType.MouseUp || Type == ActionType.MouseMove;
        public bool IsDelayAction => Type == ActionType.Delay;
    }
}