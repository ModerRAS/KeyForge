using System;
using System.Collections.Generic;
using System.Linq;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Core.Models;
using KeyForge.Core.Interfaces;
using Moq;
using Xunit;

namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 测试配置类 - 提供测试配置和共享资源
    /// </summary>
    public static class TestConfig
    {
        #region 常量配置

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public static readonly double DefaultMatchThreshold = 0.8;
        public static readonly int DefaultDelay = 100;
        public static readonly int DefaultPriority = 1;

        #endregion

        #region 测试数据配置

        public static class TestScripts
        {
            public static readonly Script SimpleScript = CreateSimpleScript();
            public static readonly Script ComplexScript = CreateComplexScript();
            public static readonly Script EmptyScript = CreateEmptyScript();
            public static readonly Script OnlyKeyboardScript = CreateOnlyKeyboardScript();
            public static readonly Script OnlyMouseScript = CreateOnlyMouseScript();

            private static Script CreateSimpleScript()
            {
                var script = new Script(Guid.NewGuid(), "Simple Script", "A simple test script");
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.A, 100, "Press A"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.A, 50, "Release A"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 1000, "Wait 1 second"));
                return script;
            }

            private static Script CreateComplexScript()
            {
                var script = new Script(Guid.NewGuid(), "Complex Script", "A complex test script");
                
                // 添加键盘动作
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.Ctrl, 0, "Press Ctrl"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, KeyCode.C, 50, "Press C"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.C, 50, "Release C"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, KeyCode.Ctrl, 50, "Release Ctrl"));
                
                // 添加鼠标动作
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 100, 100, 100, "Move mouse"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 100, 50, "Click down"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseUp, MouseButton.Left, 100, 100, 50, "Click up"));
                
                // 添加延迟
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 2000, "Wait 2 seconds"));
                
                return script;
            }

            private static Script CreateEmptyScript()
            {
                return new Script(Guid.NewGuid(), "Empty Script", "An empty test script");
            }

            private static Script CreateOnlyKeyboardScript()
            {
                var script = new Script(Guid.NewGuid(), "Keyboard Only Script", "Only keyboard actions");
                
                var keys = new[] { KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E };
                foreach (var key in keys)
                {
                    script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyDown, key, 100, $"Press {key}"));
                    script.AddAction(new GameAction(Guid.NewGuid(), ActionType.KeyUp, key, 100, $"Release {key}"));
                    script.AddAction(new GameAction(Guid.NewGuid(), ActionType.Delay, 200, "Wait"));
                }
                
                return script;
            }

            private static Script CreateOnlyMouseScript()
            {
                var script = new Script(Guid.NewGuid(), "Mouse Only Script", "Only mouse actions");
                
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 100, 100, 100, "Move to 100,100"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Left, 100, 100, 50, "Click down"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseUp, MouseButton.Left, 100, 100, 50, "Click up"));
                
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseMove, MouseButton.Left, 200, 200, 100, "Move to 200,200"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseDown, MouseButton.Right, 200, 200, 50, "Right click down"));
                script.AddAction(new GameAction(Guid.NewGuid(), ActionType.MouseUp, MouseButton.Right, 200, 200, 50, "Right click up"));
                
                return script;
            }
        }

        public static class TestStateMachines
        {
            public static readonly StateMachine SimpleStateMachine = CreateSimpleStateMachine();
            public static readonly StateMachine ComplexStateMachine = CreateComplexStateMachine();

            private static StateMachine CreateSimpleStateMachine()
            {
                var stateMachine = new StateMachine(Guid.NewGuid(), "Simple State Machine", "A simple test state machine");
                
                // 添加状态
                var state1 = new State(Guid.NewGuid(), "State1", "First state");
                var state2 = new State(Guid.NewGuid(), "State2", "Second state");
                
                stateMachine.AddState(state1);
                stateMachine.AddState(state2);
                
                // 添加转换
                var transition = new StateTransition(Guid.NewGuid(), state1.Id, state2.Id, null, "Transition 1->2");
                stateMachine.AddTransition(transition);
                
                return stateMachine;
            }

            private static StateMachine CreateComplexStateMachine()
            {
                var stateMachine = new StateMachine(Guid.NewGuid(), "Complex State Machine", "A complex test state machine");
                
                // 添加多个状态
                var states = new List<State>();
                for (int i = 0; i < 5; i++)
                {
                    var state = new State(Guid.NewGuid(), $"State{i+1}", $"State {i+1}");
                    states.Add(state);
                    stateMachine.AddState(state);
                }
                
                // 添加转换
                for (int i = 0; i < states.Count - 1; i++)
                {
                    var transition = new StateTransition(
                        Guid.NewGuid(), 
                        states[i].Id, 
                        states[i + 1].Id, 
                        new ConditionExpression($"condition{i}", ComparisonOperator.Equals, $"value{i}"),
                        $"Transition {i+1}->{i+2}");
                    stateMachine.AddTransition(transition);
                }
                
                // 添加规则
                for (int i = 0; i < 3; i++)
                {
                    var rule = new DecisionRule(
                        Guid.NewGuid(),
                        $"Rule{i+1}",
                        new ConditionExpression($"rule{i}", ComparisonOperator.GreaterThan, "0"),
                        i + 1,
                        true,
                        $"Rule {i+1} description");
                    stateMachine.AddRule(rule);
                }
                
                return stateMachine;
            }
        }

        public static class TestImageTemplates
        {
            public static readonly ImageTemplate SimpleTemplate = CreateSimpleTemplate();
            public static readonly ImageTemplate HighThresholdTemplate = CreateHighThresholdTemplate();
            public static readonly ImageTemplate LowThresholdTemplate = CreateLowThresholdTemplate();

            private static ImageTemplate CreateSimpleTemplate()
            {
                var templateData = new byte[] { 1, 2, 3, 4, 5 };
                var templateArea = new System.Drawing.Rectangle(0, 0, 100, 100);
                
                return new ImageTemplate(
                    Guid.NewGuid(),
                    "Simple Template",
                    "A simple test template",
                    templateData,
                    templateArea,
                    DefaultMatchThreshold,
                    TemplateType.Image);
            }

            private static ImageTemplate CreateHighThresholdTemplate()
            {
                var templateData = new byte[] { 1, 2, 3, 4, 5 };
                var templateArea = new System.Drawing.Rectangle(0, 0, 100, 100);
                
                return new ImageTemplate(
                    Guid.NewGuid(),
                    "High Threshold Template",
                    "A template with high threshold",
                    templateData,
                    templateArea,
                    0.95,
                    TemplateType.Image);
            }

            private static ImageTemplate CreateLowThresholdTemplate()
            {
                var templateData = new byte[] { 1, 2, 3, 4, 5 };
                var templateArea = new System.Drawing.Rectangle(0, 0, 100, 100);
                
                return new ImageTemplate(
                    Guid.NewGuid(),
                    "Low Threshold Template",
                    "A template with low threshold",
                    templateData,
                    templateArea,
                    0.5,
                    TemplateType.Image);
            }
        }

        #endregion

        #region 测试环境配置

        public static class TestEnvironment
        {
            public static Mock<IInputSimulator> CreateMockInputSimulator()
            {
                var mock = new Mock<IInputSimulator>();
                
                // 设置基本行为
                mock.Setup(s => s.KeyDown(It.IsAny<KeyCode>()));
                mock.Setup(s => s.KeyUp(It.IsAny<KeyCode>()));
                mock.Setup(s => s.KeyPress(It.IsAny<KeyCode>(), It.IsAny<int>()));
                mock.Setup(s => s.MouseDown(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
                mock.Setup(s => s.MouseUp(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>()));
                mock.Setup(s => s.MouseMove(It.IsAny<int>(), It.IsAny<int>()));
                mock.Setup(s => s.MouseClick(It.IsAny<MouseButton>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));
                
                return mock;
            }

            public static Mock<IScriptRecorder> CreateMockScriptRecorder()
            {
                var mock = new Mock<IScriptRecorder>();
                
                mock.Setup(s => s.IsRecording).Returns(false);
                mock.Setup(s => s.GetRecordedScript()).Returns((Script)null);
                
                return mock;
            }

            public static Mock<IScriptPlayer> CreateMockScriptPlayer()
            {
                var mock = new Mock<IScriptPlayer>();
                
                mock.Setup(s => s.IsPlaying).Returns(false);
                mock.Setup(s => s.IsPaused).Returns(false);
                mock.Setup(s => s.CurrentScript).Returns((Script)null);
                
                return mock;
            }

            public static Mock<ILoggerService> CreateMockLoggerService()
            {
                var mock = new Mock<ILoggerService>();
                
                mock.Setup(l => l.LogInformation(It.IsAny<string>()));
                mock.Setup(l => l.LogWarning(It.IsAny<string>()));
                mock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
                mock.Setup(l => l.LogDebug(It.IsAny<string>()));
                
                return mock;
            }
        }

        #endregion

        #region 测试数据生成器

        public static class TestDataGenerators
        {
            public static IEnumerable<object[]> GetValidScriptNames()
            {
                yield return new object[] { "Test Script" };
                yield return new object[] { "Auto Clicker" };
                yield return new object[] { "Keyboard Macro" };
                yield return new object[] { "Complex Automation" };
            }

            public static IEnumerable<object[]> GetInvalidScriptNames()
            {
                yield return new object[] { "" };
                yield return new object[] { "   " };
                yield return new object[] { null };
            }

            public static IEnumerable<object[]> GetValidActionTypes()
            {
                yield return new object[] { ActionType.KeyDown };
                yield return new object[] { ActionType.KeyUp };
                yield return new object[] { ActionType.MouseDown };
                yield return new object[] { ActionType.MouseUp };
                yield return new object[] { ActionType.MouseMove };
                yield return new object[] { ActionType.Delay };
            }

            public static IEnumerable<object[]> GetValidKeys()
            {
                yield return new object[] { KeyCode.A };
                yield return new object[] { KeyCode.B };
                yield return new object[] { KeyCode.C };
                yield return new object[] { KeyCode.Space };
                yield return new object[] { KeyCode.Enter };
            }

            public static IEnumerable<object[]> GetValidMouseButtons()
            {
                yield return new object[] { MouseButton.Left };
                yield return new object[] { MouseButton.Right };
                yield return new object[] { MouseButton.Middle };
            }

            public static IEnumerable<object[]> GetValidThresholds()
            {
                yield return new object[] { 0.0 };
                yield return new object[] { 0.5 };
                yield return new object[] { 0.8 };
                yield return new object[] { 1.0 };
            }

            public static IEnumerable<object[]> GetInvalidThresholds()
            {
                yield return new object[] { -0.1 };
                yield return new object[] { 1.1 };
                yield return new object[] { 2.0 };
            }
        }

        #endregion

        #region 性能测试配置

        public static class PerformanceTestConfig
        {
            public const int SmallDataSetSize = 10;
            public const int MediumDataSetSize = 100;
            public const int LargeDataSetSize = 1000;
            public const int StressTestIterations = 10000;
            
            public static readonly TimeSpan SmallTestTimeout = TimeSpan.FromSeconds(5);
            public static readonly TimeSpan MediumTestTimeout = TimeSpan.FromSeconds(30);
            public static readonly TimeSpan LargeTestTimeout = TimeSpan.FromMinutes(5);
        }

        #endregion
    }
}