using System;
using KeyForge.Domain.Common;

namespace KeyForge.Core.Models
{
    /// <summary>
    /// 按键动作模型 - 使用Domain层的统一定义
    /// 这是简化实现，避免重复定义和循环依赖
    /// </summary>
    
    /// <summary>
    /// 按键动作模型
    /// </summary>
    public class KeyAction
    {
        public ActionType Type { get; set; }
        public KeyCode Key { get; set; }
        public MouseButton Button { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Delay { get; set; }
        public DateTime Timestamp { get; set; }

        public KeyAction()
        {
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] {Type}: {Key} at ({X}, {Y}) Delay: {Delay}ms";
        }
    }
}