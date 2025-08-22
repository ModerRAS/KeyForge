using System;
using System.Collections.Generic;
using System.Drawing;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Tests.Simplified
{
    /// <summary>
    /// 简化的测试数据工厂
    /// 专门为跨平台环境设计，避免Windows特定的依赖
    /// </summary>
    public static class SimplifiedTestDataFactory
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// 创建有效的脚本
        /// </summary>
        public static Script CreateValidScript()
        {
            return new Script(
                Guid.NewGuid(),
                $"Test Script {_random.Next(1000)}",
                "Test script description for testing purposes"
            );
        }

        /// <summary>
        /// 创建带有随机动作的脚本
        /// </summary>
        public static Script CreateScriptWithActions(int actionCount)
        {
            var script = CreateValidScript();
            
            for (int i = 0; i < actionCount; i++)
            {
                var action = CreateRandomAction();
                script.AddAction(action);
            }
            
            return script;
        }

        /// <summary>
        /// 创建有效的图像模板
        /// </summary>
        public static ImageTemplate CreateValidImageTemplate()
        {
            var templateData = new byte[_random.Next(100, 1000)];
            _random.NextBytes(templateData);
            
            return new ImageTemplate(
                Guid.NewGuid(),
                $"Test Template {_random.Next(1000)}",
                "Test template description for testing purposes",
                templateData,
                new Rectangle(_random.Next(0, 100), _random.Next(0, 100), 100, 100),
                _random.NextDouble() * 0.5 + 0.5 // 0.5 to 1.0
            );
        }

        /// <summary>
        /// 创建有效的状态机
        /// </summary>
        public static StateMachine CreateValidStateMachine()
        {
            var initialState = CreateValidState();
            return new StateMachine(
                Guid.NewGuid(),
                $"Test State Machine {_random.Next(1000)}",
                "Test state machine description for testing purposes",
                initialState
            );
        }

        /// <summary>
        /// 创建有效的状态
        /// </summary>
        public static State CreateValidState()
        {
            var variables = new Dictionary<string, object>
            {
                { "Counter", _random.Next(0, 100) },
                { "IsEnabled", _random.NextDouble() > 0.5 },
                { "Name", $"State {_random.Next(1000)}" }
            };
            
            return new State(
                Guid.NewGuid(),
                $"State {_random.Next(1000)}",
                "Test state description for testing purposes",
                variables
            );
        }

        /// <summary>
        /// 创建有效的状态转换
        /// </summary>
        public static StateTransition CreateValidTransition(State fromState, State toState)
        {
            return new StateTransition(
                Guid.NewGuid(),
                fromState,
                toState,
                "true",
                "Test transition description"
            );
        }

        /// <summary>
        /// 创建有效的决策规则
        /// </summary>
        public static DecisionRule CreateValidDecisionRule()
        {
            return new DecisionRule(
                Guid.NewGuid(),
                $"Rule {_random.Next(1000)}",
                "Test decision rule description",
                "counter > 50",
                "targetState"
            );
        }

        /// <summary>
        /// 创建随机游戏动作
        /// </summary>
        public static GameAction CreateRandomAction()
        {
            var actionTypes = new[] { ActionType.KeyDown, ActionType.MouseDown, ActionType.Delay };
            var actionType = actionTypes[_random.Next(actionTypes.Length)];

            return actionType switch
            {
                ActionType.KeyDown => CreateKeyAction(),
                ActionType.MouseDown => CreateMouseAction(),
                ActionType.Delay => CreateDelayAction(),
                _ => CreateKeyAction()
            };
        }

        /// <summary>
        /// 创建按键动作
        /// </summary>
        public static GameAction CreateKeyAction()
        {
            var keyCodes = new[] { KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.Space, KeyCode.Enter };
            var keyCode = keyCodes[_random.Next(keyCodes.Length)];
            
            return new GameAction(
                Guid.NewGuid(),
                ActionType.KeyDown,
                keyCode,
                _random.Next(10, 100),
                "Test key action"
            );
        }

        /// <summary>
        /// 创建鼠标动作
        /// </summary>
        public static GameAction CreateMouseAction()
        {
            var mouseButtons = new[] { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
            var mouseButton = mouseButtons[_random.Next(mouseButtons.Length)];
            
            return new GameAction(
                Guid.NewGuid(),
                ActionType.MouseDown,
                mouseButton,
                _random.Next(0, 1920),
                _random.Next(0, 1080),
                _random.Next(10, 100),
                "Test mouse action"
            );
        }

        /// <summary>
        /// 创建延迟动作
        /// </summary>
        public static GameAction CreateDelayAction()
        {
            return new GameAction(
                Guid.NewGuid(),
                ActionType.Delay,
                _random.Next(100, 2000),
                "Test delay action"
            );
        }

        /// <summary>
        /// 创建脚本列表
        /// </summary>
        public static List<Script> CreateScriptList(int count)
        {
            var scripts = new List<Script>();
            for (int i = 0; i < count; i++)
            {
                scripts.Add(CreateValidScript());
            }
            return scripts;
        }

        /// <summary>
        /// 创建图像模板列表
        /// </summary>
        public static List<ImageTemplate> CreateImageTemplateList(int count)
        {
            var templates = new List<ImageTemplate>();
            for (int i = 0; i < count; i++)
            {
                templates.Add(CreateValidImageTemplate());
            }
            return templates;
        }

        /// <summary>
        /// 创建状态机列表
        /// </summary>
        public static List<StateMachine> CreateStateMachineList(int count)
        {
            var stateMachines = new List<StateMachine>();
            for (int i = 0; i < count; i++)
            {
                stateMachines.Add(CreateValidStateMachine());
            }
            return stateMachines;
        }

        /// <summary>
        /// 创建复杂的脚本（包含多种类型的动作）
        /// </summary>
        public static Script CreateComplexScript()
        {
            var script = CreateValidScript();
            
            // 添加启动序列
            script.AddAction(CreateDelayAction());
            script.AddAction(CreateKeyAction());
            
            // 添加主循环动作
            for (int i = 0; i < 10; i++)
            {
                script.AddAction(CreateMouseAction());
                script.AddAction(CreateDelayAction());
            }
            
            // 添加结束序列
            script.AddAction(CreateKeyAction());
            script.AddAction(CreateDelayAction());
            
            return script;
        }

        /// <summary>
        /// 创建复杂的状态机（包含多个状态和转换）
        /// </summary>
        public static StateMachine CreateComplexStateMachine()
        {
            var initialState = CreateValidState();
            var stateMachine = new StateMachine(
                Guid.NewGuid(),
                "Complex State Machine",
                "Complex state machine for testing",
                initialState
            );
            
            // 添加多个状态
            var state1 = CreateValidState();
            var state2 = CreateValidState();
            var state3 = CreateValidState();
            
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            stateMachine.AddState(state3);
            
            // 添加转换
            stateMachine.AddTransition(CreateValidTransition(initialState, state1));
            stateMachine.AddTransition(CreateValidTransition(state1, state2));
            stateMachine.AddTransition(CreateValidTransition(state2, state3));
            stateMachine.AddTransition(CreateValidTransition(state3, initialState));
            
            return stateMachine;
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        public static string CreateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// 创建随机GUID
        /// </summary>
        public static Guid CreateRandomGuid()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// 创建随机矩形区域
        /// </summary>
        public static Rectangle CreateRandomRectangle()
        {
            return new Rectangle(
                _random.Next(0, 1000),
                _random.Next(0, 1000),
                _random.Next(50, 500),
                _random.Next(50, 500)
            );
        }
    }
}