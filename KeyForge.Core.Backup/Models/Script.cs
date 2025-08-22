using System;
using System.Collections.Generic;

namespace KeyForge.Core.Models
{
    /// <summary>
    /// 脚本模型
    /// </summary>
    public class Script
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<KeyAction> Actions { get; set; }
        public int RepeatCount { get; set; }
        public bool Loop { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Script()
        {
            Actions = new List<KeyAction>();
            RepeatCount = 1;
            Loop = false;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 添加动作到脚本
        /// </summary>
        public void AddAction(KeyAction action)
        {
            Actions.Add(action);
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 移除指定索引的动作
        /// </summary>
        public void RemoveAction(int index)
        {
            if (index >= 0 && index < Actions.Count)
            {
                Actions.RemoveAt(index);
                UpdatedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// 清空所有动作
        /// </summary>
        public void ClearActions()
        {
            Actions.Clear();
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 获取脚本总时长（毫秒）
        /// </summary>
        public int GetTotalDuration()
        {
            int totalDuration = 0;
            foreach (var action in Actions)
            {
                totalDuration += action.Delay;
            }
            return totalDuration;
        }

        /// <summary>
        /// 获取脚本动作数量
        /// </summary>
        public int GetActionCount()
        {
            return Actions.Count;
        }
    }
}