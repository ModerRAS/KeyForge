using System;

namespace KeyForge.Core.Models
{
    /// <summary>
    /// 按键动作模型 - 简化版本
    /// 原本实现：复杂的领域模型和值对象
    /// 简化实现：简单的POCO类，只包含必要属性
    /// </summary>
    public class KeyAction
    {
        public int KeyCode { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public bool IsKeyDown { get; set; }
        public int Delay { get; set; } // 延迟时间（毫秒）
        public DateTime Timestamp { get; set; }

        public KeyAction()
        {
            Timestamp = DateTime.Now;
        }

        public KeyAction(int keyCode, string keyName, bool isKeyDown, int delay = 0)
        {
            KeyCode = keyCode;
            KeyName = keyName;
            IsKeyDown = isKeyDown;
            Delay = delay;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss.fff} - {KeyName} ({KeyCode}) - {(IsKeyDown ? "按下" : "释放")} - 延迟: {Delay}ms";
        }
    }
}