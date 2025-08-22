using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
    using KeyForge.Domain.Common;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 决策引擎 - 简化实现
    /// </summary>
    public class DecisionEngine
    {
        private readonly ILoggerService _logger;
        private readonly IInputSimulator _inputSimulator;
        private readonly ImageRecognitionService _imageRecognition;
        private readonly Dictionary<string, object> _variables;

        public DecisionEngine(ILoggerService logger, IInputSimulator inputSimulator, ImageRecognitionService imageRecognition)
        {
            _logger = logger;
            _inputSimulator = inputSimulator;
            _imageRecognition = imageRecognition;
            _variables = new Dictionary<string, object>();
        }

        /// <summary>
        /// 执行决策逻辑
        /// </summary>
        public void ExecuteDecision(DecisionNode decision)
        {
            if (decision == null)
            {
                _logger.Error("决策节点为null");
                return;
            }

            _logger.Info($"执行决策: {decision.Name}");

            try
            {
                ExecuteNode(decision);
            }
            catch (Exception ex)
            {
                _logger.Error($"执行决策失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 执行决策节点
        /// </summary>
        private void ExecuteNode(DecisionNode node)
        {
            switch (node.Type)
            {
                case NodeType.Action:
                    ExecuteActionNode((ActionNode)node);
                    break;
                case NodeType.Condition:
                    ExecuteConditionNode((ConditionNode)node);
                    break;
                case NodeType.Loop:
                    ExecuteLoopNode((LoopNode)node);
                    break;
                case NodeType.Sequence:
                    ExecuteSequenceNode((SequenceNode)node);
                    break;
                default:
                    _logger.Warning($"未知的节点类型: {node.Type}");
                    break;
            }
        }

        /// <summary>
        /// 执行动作节点
        /// </summary>
        private void ExecuteActionNode(ActionNode node)
        {
            _logger.Debug($"执行动作: {node.Action}");

            switch (node.Action)
            {
                case "key_press":
                    var key = ParseKeyCode(node.Parameters["key"]?.ToString());
                    _inputSimulator.SendKey(key, KeyState.Down);
                    _inputSimulator.Delay(50);
                    _inputSimulator.SendKey(key, KeyState.Up);
                    break;

                case "key_down":
                    _inputSimulator.SendKey(ParseKeyCode(node.Parameters["key"]?.ToString()), KeyState.Down);
                    break;

                case "key_up":
                    _inputSimulator.SendKey(ParseKeyCode(node.Parameters["key"]?.ToString()), KeyState.Up);
                    break;

                case "mouse_click":
                    var button = ParseMouseButton(node.Parameters["button"]?.ToString());
                    _inputSimulator.SendMouse(button, MouseState.Down);
                    _inputSimulator.Delay(50);
                    _inputSimulator.SendMouse(button, MouseState.Up);
                    break;

                case "mouse_move":
                    if (node.Parameters.ContainsKey("x") && node.Parameters.ContainsKey("y"))
                    {
                        var x = Convert.ToInt32(node.Parameters["x"]);
                        var y = Convert.ToInt32(node.Parameters["y"]);
                        _inputSimulator.MoveMouse(x, y);
                    }
                    break;

                case "delay":
                    if (node.Parameters.ContainsKey("milliseconds"))
                    {
                        var delay = Convert.ToInt32(node.Parameters["milliseconds"]);
                        _inputSimulator.Delay(delay);
                    }
                    break;

                case "wait_for_image":
                    if (node.Parameters.ContainsKey("template"))
                    {
                        var template = node.Parameters["template"].ToString();
                        var timeout = node.Parameters.ContainsKey("timeout") ? Convert.ToInt32(node.Parameters["timeout"]) : 5000;
                        WaitForImage(template, timeout);
                    }
                    break;

                case "set_variable":
                    if (node.Parameters.ContainsKey("name") && node.Parameters.ContainsKey("value"))
                    {
                        var name = node.Parameters["name"].ToString();
                        var value = node.Parameters["value"];
                        SetVariable(name, value);
                    }
                    break;

                default:
                    _logger.Warning($"未知的动作类型: {node.Action}");
                    break;
            }
        }

        /// <summary>
        /// 执行条件节点
        /// </summary>
        private void ExecuteConditionNode(ConditionNode node)
        {
            var conditionResult = EvaluateCondition(node.Condition);
            
            _logger.Debug($"条件判断: {node.Condition} = {conditionResult}");

            if (conditionResult)
            {
                if (node.TrueBranch != null)
                {
                    ExecuteNode(node.TrueBranch);
                }
            }
            else
            {
                if (node.FalseBranch != null)
                {
                    ExecuteNode(node.FalseBranch);
                }
            }
        }

        /// <summary>
        /// 执行循环节点
        /// </summary>
        private void ExecuteLoopNode(LoopNode node)
        {
            switch (node.LoopType)
            {
                case LoopType.Count:
                    var count = node.Count;
                    _logger.Debug($"执行计数循环: {count} 次");
                    for (int i = 0; i < count; i++)
                    {
                        ExecuteNode(node.Body);
                    }
                    break;

                case LoopType.While:
                    _logger.Debug($"执行条件循环: {node.Condition}");
                    while (EvaluateCondition(node.Condition))
                    {
                        ExecuteNode(node.Body);
                    }
                    break;

                case LoopType.ForEver:
                    _logger.Debug($"执行无限循环");
                    while (true)
                    {
                        ExecuteNode(node.Body);
                    }
                    break;
            }
        }

        /// <summary>
        /// 执行序列节点
        /// </summary>
        private void ExecuteSequenceNode(SequenceNode node)
        {
            foreach (var child in node.Children)
            {
                ExecuteNode(child);
            }
        }

        /// <summary>
        /// 评估条件表达式
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return false;
            }

            try
            {
                // 简化的条件解析器
                var processedCondition = ProcessVariables(condition);
                
                // 支持的基本条件
                if (processedCondition.Contains("=="))
                {
                    var parts = processedCondition.Split(new[] { "==" }, StringSplitOptions.RemoveEmptyEntries);
                    return AreEqual(parts[0].Trim(), parts[1].Trim());
                }
                else if (processedCondition.Contains("!="))
                {
                    var parts = processedCondition.Split(new[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                    return !AreEqual(parts[0].Trim(), parts[1].Trim());
                }
                else if (processedCondition.Contains(">"))
                {
                    var parts = processedCondition.Split(new[] { ">" }, StringSplitOptions.RemoveEmptyEntries);
                    return CompareNumbers(parts[0].Trim(), parts[1].Trim()) > 0;
                }
                else if (processedCondition.Contains("<"))
                {
                    var parts = processedCondition.Split(new[] { "<" }, StringSplitOptions.RemoveEmptyEntries);
                    return CompareNumbers(parts[0].Trim(), parts[1].Trim()) < 0;
                }
                else if (processedCondition.Contains(">="))
                {
                    var parts = processedCondition.Split(new[] { ">=" }, StringSplitOptions.RemoveEmptyEntries);
                    return CompareNumbers(parts[0].Trim(), parts[1].Trim()) >= 0;
                }
                else if (processedCondition.Contains("<="))
                {
                    var parts = processedCondition.Split(new[] { "<=" }, StringSplitOptions.RemoveEmptyEntries);
                    return CompareNumbers(parts[0].Trim(), parts[1].Trim()) <= 0;
                }
                else if (processedCondition.StartsWith("image_exists("))
                {
                    var templatePath = ExtractParameter(processedCondition, "image_exists");
                    return ImageExists(templatePath);
                }
                else if (processedCondition.StartsWith("color_at("))
                {
                    var parameters = ExtractParameters(processedCondition, "color_at");
                    return CheckColorAt(parameters);
                }

                return Convert.ToBoolean(processedCondition);
            }
            catch (Exception ex)
            {
                _logger.Error($"评估条件失败: {condition}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理变量替换
        /// </summary>
        private string ProcessVariables(string expression)
        {
            var result = expression;
            
            foreach (var variable in _variables)
            {
                result = result.Replace($"${variable.Key}", variable.Value?.ToString() ?? "");
            }

            return result;
        }

        /// <summary>
        /// 判断两个值是否相等
        /// </summary>
        private bool AreEqual(string value1, string value2)
        {
            if (double.TryParse(value1, out double num1) && double.TryParse(value2, out double num2))
            {
                return Math.Abs(num1 - num2) < 0.0001;
            }
            
            return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 比较两个数字
        /// </summary>
        private int CompareNumbers(string value1, string value2)
        {
            if (double.TryParse(value1, out double num1) && double.TryParse(value2, out double num2))
            {
                return num1.CompareTo(num2);
            }
            
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析按键代码
        /// </summary>
        private KeyCode ParseKeyCode(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString))
            {
                return KeyCode.None;
            }

            if (Enum.TryParse<KeyCode>(keyString, true, out var keyCode))
            {
                return keyCode;
            }

            _logger.Warning($"未知的按键代码: {keyString}");
            return KeyCode.None;
        }

        /// <summary>
        /// 解析鼠标按钮
        /// </summary>
        private MouseButton ParseMouseButton(string buttonString)
        {
            if (string.IsNullOrWhiteSpace(buttonString))
            {
                return MouseButton.Left;
            }

            if (Enum.TryParse<MouseButton>(buttonString, true, out var button))
            {
                return button;
            }

            _logger.Warning($"未知的鼠标按钮: {buttonString}");
            return MouseButton.Left;
        }

        /// <summary>
        /// 等待图像出现
        /// </summary>
        private bool WaitForImage(string templatePath, int timeoutMs)
        {
            var startTime = DateTime.Now;
            
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                var result = _imageRecognition.FindImageOnScreen(templatePath);
                if (result.Success)
                {
                    _logger.Info($"图像已找到: {templatePath}");
                    return true;
                }
                
                _inputSimulator.Delay(100);
            }

            _logger.Warning($"等待图像超时: {templatePath}");
            return false;
        }

        /// <summary>
        /// 检查图像是否存在
        /// </summary>
        private bool ImageExists(string templatePath)
        {
            var result = _imageRecognition.FindImageOnScreen(templatePath);
            return result.Success;
        }

        /// <summary>
        /// 检查指定位置的颜色
        /// </summary>
        private bool CheckColorAt(string[] parameters)
        {
            if (parameters.Length >= 3)
            {
                var x = Convert.ToInt32(parameters[0]);
                var y = Convert.ToInt32(parameters[1]);
                var targetColor = ParseColor(parameters[2]);
                
                var actualColor = _imageRecognition.GetPixelColor(x, y);
                return ColorsMatch(actualColor, targetColor, 10);
            }
            
            return false;
        }

        /// <summary>
        /// 解析颜色字符串
        /// </summary>
        private System.Drawing.Color ParseColor(string colorString)
        {
            if (colorString.StartsWith("#"))
            {
                return System.Drawing.ColorTranslator.FromHtml(colorString);
            }
            
            if (Enum.TryParse<System.Drawing.KnownColor>(colorString, true, out var knownColor))
            {
                return System.Drawing.Color.FromKnownColor(knownColor);
            }
            
            return System.Drawing.Color.White;
        }

        /// <summary>
        /// 检查颜色是否匹配
        /// </summary>
        private bool ColorsMatch(System.Drawing.Color color1, System.Drawing.Color color2, int tolerance)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance;
        }

        /// <summary>
        /// 提取函数参数
        /// </summary>
        private string ExtractParameter(string expression, string functionName)
        {
            var pattern = $@"{functionName}\(([^)]+)\)";
            var match = Regex.Match(expression, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        /// <summary>
        /// 提取多个函数参数
        /// </summary>
        private string[] ExtractParameters(string expression, string functionName)
        {
            var parameterString = ExtractParameter(expression, functionName);
            return parameterString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .ToArray();
        }

        /// <summary>
        /// 设置变量
        /// </summary>
        private void SetVariable(string name, object value)
        {
            _variables[name] = value;
            _logger.Debug($"设置变量: {name} = {value}");
        }

        /// <summary>
        /// 获取变量
        /// </summary>
        public object GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// 清除所有变量
        /// </summary>
        public void ClearVariables()
        {
            _variables.Clear();
            _logger.Debug("清除所有变量");
        }
    }

    /// <summary>
    /// 决策节点基类
    /// </summary>
    public abstract class DecisionNode
    {
        public string Name { get; set; }
        public NodeType Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// 动作节点
    /// </summary>
    public class ActionNode : DecisionNode
    {
        public string Action { get; set; }
    }

    /// <summary>
    /// 条件节点
    /// </summary>
    public class ConditionNode : DecisionNode
    {
        public string Condition { get; set; }
        public DecisionNode TrueBranch { get; set; }
        public DecisionNode FalseBranch { get; set; }
    }

    /// <summary>
    /// 循环节点
    /// </summary>
    public class LoopNode : DecisionNode
    {
        public LoopType LoopType { get; set; }
        public int Count { get; set; }
        public string Condition { get; set; }
        public DecisionNode Body { get; set; }
    }

    /// <summary>
    /// 序列节点
    /// </summary>
    public class SequenceNode : DecisionNode
    {
        public List<DecisionNode> Children { get; set; } = new List<DecisionNode>();
    }

    /// <summary>
    /// 节点类型枚举
    /// </summary>
    public enum NodeType
    {
        Action,
        Condition,
        Loop,
        Sequence
    }

    /// <summary>
    /// 循环类型枚举
    /// </summary>
    public enum LoopType
    {
        Count,
        While,
        ForEver
    }
}