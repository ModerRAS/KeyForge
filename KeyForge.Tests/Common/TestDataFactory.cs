using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Core.Models;
using KeyForge.Core.Enums;
using System.Drawing;

namespace KeyForge.Tests.Common
{
    /// <summary>
    /// 测试数据工厂 - 使用Bogus生成测试数据
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Faker Faker = new Faker();

        #region Script 相关测试数据

        public static Script CreateValidScript()
        {
            var scriptId = Guid.NewGuid();
            var script = new Script(scriptId, 
                Faker.Lorem.Sentence(3), 
                Faker.Lorem.Paragraph());

            // 添加一些动作
            var actions = CreateGameActions(5);
            foreach (var action in actions)
            {
                script.AddAction(action);
            }

            return script;
        }

        public static GameAction CreateGameAction()
        {
            var actionType = Faker.PickRandom<ActionType>();
            
            return actionType switch
            {
                ActionType.KeyDown or ActionType.KeyUp => new GameAction(
                    Guid.NewGuid(),
                    actionType,
                    Faker.PickRandom<KeyCode>(),
                    Faker.Random.Int(0, 1000),
                    Faker.Lorem.Sentence()),
                
                ActionType.MouseDown or ActionType.MouseUp or ActionType.MouseMove => new GameAction(
                    Guid.NewGuid(),
                    actionType,
                    Faker.PickRandom<MouseButton>(),
                    Faker.Random.Int(0, 1920),
                    Faker.Random.Int(0, 1080),
                    Faker.Random.Int(0, 1000),
                    Faker.Lorem.Sentence()),
                
                ActionType.Delay => new GameAction(
                    Guid.NewGuid(),
                    actionType,
                    Faker.Random.Int(100, 5000),
                    Faker.Lorem.Sentence()),
                
                _ => throw new ArgumentException($"Unsupported action type: {actionType}")
            };
        }

        public static List<GameAction> CreateGameActions(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateGameAction())
                .ToList();
        }

        #endregion

        #region StateMachine 相关测试数据

        public static StateMachine CreateValidStateMachine()
        {
            var stateMachineId = Guid.NewGuid();
            var stateMachine = new StateMachine(stateMachineId,
                Faker.Lorem.Sentence(3),
                Faker.Lorem.Paragraph());

            // 添加更多状态
            for (int i = 0; i < 3; i++)
            {
                var state = new State(Guid.NewGuid(), 
                    $"State{i}", 
                    Faker.Lorem.Sentence());
                stateMachine.AddState(state);
            }

            // 添加转换
            var states = stateMachine.States.ToList();
            for (int i = 0; i < states.Count - 1; i++)
            {
                var transition = new StateTransition(
                    Guid.NewGuid(),
                    states[i].Id,
                    states[i + 1].Id,
                    CreateConditionExpression(),
                    Faker.Lorem.Sentence());
                stateMachine.AddTransition(transition);
            }

            // 添加规则
            var rules = CreateDecisionRules(2);
            foreach (var rule in rules)
            {
                stateMachine.AddRule(rule);
            }

            return stateMachine;
        }

        public static State CreateState()
        {
            return new State(
                Guid.NewGuid(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence());
        }

        public static StateTransition CreateStateTransition(Guid fromStateId, Guid toStateId)
        {
            return new StateTransition(
                Guid.NewGuid(),
                fromStateId,
                toStateId,
                CreateConditionExpression(),
                Faker.Lorem.Sentence());
        }

        public static DecisionRule CreateDecisionRule()
        {
            return new DecisionRule(
                Guid.NewGuid(),
                Faker.Lorem.Word(),
                CreateConditionExpression(),
                Faker.Random.Int(1, 10),
                Faker.Random.Bool(),
                Faker.Lorem.Sentence());
        }

        public static List<DecisionRule> CreateDecisionRules(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateDecisionRule())
                .ToList();
        }

        public static ConditionExpression CreateConditionExpression()
        {
            return new ConditionExpression(
                Faker.Lorem.Word(),
                Faker.PickRandom<ComparisonOperator>(),
                Faker.Lorem.Word());
        }

        #endregion

        #region ImageTemplate 相关测试数据

        public static ImageTemplate CreateValidImageTemplate()
        {
            var templateData = CreateRandomImageData();
            var templateArea = new Rectangle(
                Faker.Random.Int(0, 100),
                Faker.Random.Int(0, 100),
                Faker.Random.Int(50, 200),
                Faker.Random.Int(50, 200));

            return new ImageTemplate(
                Guid.NewGuid(),
                Faker.Lorem.Sentence(3),
                Faker.Lorem.Paragraph(),
                templateData,
                templateArea,
                Faker.Random.Double(0.5, 0.95),
                Faker.PickRandom<TemplateType>());
        }

        public static byte[] CreateRandomImageData()
        {
            // 创建简单的测试图像数据
            var size = Faker.Random.Int(1024, 10240);
            var data = new byte[size];
            Faker.Random.Bytes().CopyTo(data, 0);
            return data;
        }

        public static RecognitionResult CreateRecognitionResult(bool isMatch = true)
        {
            return new RecognitionResult(
                isMatch,
                isMatch ? Faker.Random.Double(0.8, 0.99) : Faker.Random.Double(0.0, 0.5),
                new Rectangle(
                    Faker.Random.Int(0, 100),
                    Faker.Random.Int(0, 100),
                    Faker.Random.Int(50, 200),
                    Faker.Random.Int(50, 200)));
        }

        #endregion

        #region Core Models 相关测试数据

        public static Script CreateCoreScript()
        {
            var scriptId = Guid.NewGuid();
            var script = new Script(scriptId, Faker.Lorem.Word(), Faker.Lorem.Sentence());

            // 添加一些核心动作
            var coreActions = CreateCoreActions(5);
            foreach (var action in coreActions)
            {
                script.AddAction(MapCoreActionToDomainAction(action));
            }

            return script;
        }

        public static KeyForge.Core.Models.Action CreateCoreAction()
        {
            var actionType = Faker.PickRandom<KeyForge.Core.Enums.ActionType>();
            
            return actionType switch
            {
                KeyForge.Core.Enums.ActionType.KeyDown or KeyForge.Core.Enums.ActionType.KeyUp => 
                    new KeyForge.Core.Models.Action(
                        Guid.NewGuid(),
                        actionType,
                        key: Faker.PickRandom<KeyCode>(),
                        delay: Faker.Random.Int(0, 1000)),
                
                KeyForge.Core.Enums.ActionType.MouseDown or KeyForge.Core.Enums.ActionType.MouseUp or 
                KeyForge.Core.Enums.ActionType.MouseMove => 
                    new KeyForge.Core.Models.Action(
                        Guid.NewGuid(),
                        actionType,
                        button: Faker.PickRandom<MouseButton>(),
                        x: Faker.Random.Int(0, 1920),
                        y: Faker.Random.Int(0, 1080),
                        delay: Faker.Random.Int(0, 1000)),
                
                KeyForge.Core.Enums.ActionType.Delay => 
                    new KeyForge.Core.Models.Action(
                        Guid.NewGuid(),
                        actionType,
                        delay: Faker.Random.Int(100, 5000)),
                
                _ => throw new ArgumentException($"Unsupported action type: {actionType}")
            };
        }

        public static List<KeyForge.Core.Models.Action> CreateCoreActions(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateCoreAction())
                .ToList();
        }

        private static GameAction MapCoreActionToDomainAction(KeyForge.Core.Models.Action coreAction)
        {
            var domainType = coreAction.Type switch
            {
                KeyForge.Core.Enums.ActionType.KeyDown => ActionType.KeyDown,
                KeyForge.Core.Enums.ActionType.KeyUp => ActionType.KeyUp,
                KeyForge.Core.Enums.ActionType.MouseDown => ActionType.MouseDown,
                KeyForge.Core.Enums.ActionType.MouseUp => ActionType.MouseUp,
                KeyForge.Core.Enums.ActionType.MouseMove => ActionType.MouseMove,
                KeyForge.Core.Enums.ActionType.Delay => ActionType.Delay,
                _ => throw new ArgumentException($"Unsupported action type: {coreAction.Type}")
            };

            return domainType switch
            {
                ActionType.KeyDown or ActionType.KeyUp => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Key.Value,
                    coreAction.Delay,
                    "Test action"),
                
                ActionType.MouseDown or ActionType.MouseUp or ActionType.MouseMove => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Button.Value,
                    coreAction.X,
                    coreAction.Y,
                    coreAction.Delay,
                    "Test action"),
                
                ActionType.Delay => new GameAction(
                    coreAction.Id,
                    domainType,
                    coreAction.Delay,
                    "Test action"),
                
                _ => throw new ArgumentException($"Unsupported action type: {domainType}")
            };
        }

        #endregion

        #region DTOs 和 Commands 相关测试数据

        public static Application.DTOs.ScriptDto CreateScriptDto()
        {
            return new Application.DTOs.ScriptDto
            {
                Id = Guid.NewGuid(),
                Name = Faker.Lorem.Sentence(3),
                Description = Faker.Lorem.Paragraph(),
                Status = Faker.PickRandom<ScriptStatus>(),
                CreatedAt = DateTime.UtcNow.AddDays(-Faker.Random.Int(1, 30)),
                UpdatedAt = DateTime.UtcNow.AddDays(-Faker.Random.Int(0, 7)),
                Version = Faker.Random.Int(1, 10),
                Actions = CreateGameActionDtos(Faker.Random.Int(1, 10)),
                EstimatedDuration = TimeSpan.FromMilliseconds(Faker.Random.Int(1000, 30000))
            };
        }

        public static Application.DTOs.GameActionDto CreateGameActionDto()
        {
            var actionType = Faker.PickRandom<ActionType>();
            
            return new Application.DTOs.GameActionDto
            {
                Id = Guid.NewGuid(),
                Type = actionType,
                Key = Faker.PickRandom<KeyCode>(),
                Button = Faker.PickRandom<MouseButton>(),
                X = Faker.Random.Int(0, 1920),
                Y = Faker.Random.Int(0, 1080),
                Delay = Faker.Random.Int(0, 5000),
                Timestamp = DateTime.UtcNow.AddMilliseconds(-Faker.Random.Int(0, 10000)),
                Description = Faker.Lorem.Sentence()
            };
        }

        public static List<Application.DTOs.GameActionDto> CreateGameActionDtos(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateGameActionDto())
                .ToList();
        }

        public static Application.Commands.CreateScriptCommand CreateCreateScriptCommand()
        {
            return new Application.Commands.CreateScriptCommand
            {
                Name = Faker.Lorem.Sentence(3),
                Description = Faker.Lorem.Paragraph(),
                Actions = CreateGameActionDtos(Faker.Random.Int(1, 10))
            };
        }

        public static Application.Commands.UpdateScriptCommand CreateUpdateScriptCommand(Guid scriptId)
        {
            return new Application.Commands.UpdateScriptCommand
            {
                Id = scriptId,
                Name = Faker.Lorem.Sentence(3),
                Description = Faker.Lorem.Paragraph()
            };
        }

        #endregion

        #region 辅助方法

        public static T GetRandomEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(Faker.Random.Int(0, values.Length - 1))!;
        }

        public static DateTime GetRandomDate()
        {
            return DateTime.UtcNow.AddDays(-Faker.Random.Int(1, 365));
        }

        public static string GetRandomString(int length = 10)
        {
            return Faker.Random.String(length);
        }

        #endregion
    }
}