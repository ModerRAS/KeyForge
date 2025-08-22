using System;
using System.Collections.Generic;
using Bogus;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.CrossPlatformTests.Common
{
    /// <summary>
    /// 测试数据工厂
    /// 原本实现：复杂的测试数据生成
    /// 简化实现：基础测试数据生成
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Faker _faker = new();

        public static Script CreateValidScript()
        {
            return new Script(Guid.NewGuid(), _faker.Lorem.Word(), _faker.Lorem.Sentence());
        }

        public static Script CreateScriptWithActions(int actionCount)
        {
            var script = CreateValidScript();
            for (int i = 0; i < actionCount; i++)
            {
                script.AddAction(CreateGameAction());
            }
            return script;
        }

        public static GameAction CreateGameAction()
        {
            return new GameAction(
                Guid.NewGuid(),
                ActionType.KeyDown,
                KeyCode.A,
                _faker.Random.Int(10, 100),
                _faker.Lorem.Word()
            );
        }

        public static GameAction CreateMouseAction()
        {
            return new GameAction(
                Guid.NewGuid(),
                ActionType.MouseMove,
                MouseButton.Left,
                _faker.Random.Int(0, 1920),
                _faker.Random.Int(0, 1080),
                _faker.Random.Int(10, 100),
                _faker.Lorem.Word()
            );
        }

        public static GameAction CreateDelayAction()
        {
            return new GameAction(
                Guid.NewGuid(),
                ActionType.Delay,
                _faker.Random.Int(100, 1000),
                _faker.Lorem.Word()
            );
        }
    }
}