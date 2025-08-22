using System;
using Bogus;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Domain.Tests.Common
{
    /// <summary>
    /// 测试数据工厂
    /// 原本实现：复杂的测试数据生成
    /// 简化实现：基础测试数据生成，专注于Domain层测试
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Faker Faker = new();

        public static Script CreateValidScript()
        {
            return new Script(
                Guid.NewGuid(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence()
            );
        }

        
        public static ImageTemplate CreateValidImageTemplate()
        {
            return new ImageTemplate(
                Guid.NewGuid(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence(),
                CreateRandomImageData(),
                new KeyForge.Domain.Common.Rectangle(
                    Faker.Random.Int(0, 100),
                    Faker.Random.Int(0, 100),
                    Faker.Random.Int(50, 200),
                    Faker.Random.Int(50, 200)
                ),
                Faker.Random.Double(0.5, 0.95)
            );
        }

        public static byte[] CreateRandomImageData()
        {
            var size = Faker.Random.Int(100, 1000);
            return Faker.Random.Bytes(size);
        }

        public static RecognitionResult CreateRecognitionResult(bool isMatch = true)
        {
            return new RecognitionResult(
                isMatch,
                new KeyForge.Domain.Common.Rectangle(
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(50, 200),
                    Faker.Random.Int(50, 200)
                ),
                Faker.Random.Double(0.0, 1.0),
                Faker.Lorem.Word()
            );
        }

        public static RecognitionResult CreateHighConfidenceResult()
        {
            return new RecognitionResult(
                true,
                new KeyForge.Domain.Common.Rectangle(
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(50, 200),
                    Faker.Random.Int(50, 200)
                ),
                Faker.Random.Double(0.8, 1.0),
                Faker.Lorem.Word()
            );
        }

        public static RecognitionResult CreateLowConfidenceResult()
        {
            return new RecognitionResult(
                true,
                new KeyForge.Domain.Common.Rectangle(
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(0, 1000),
                    Faker.Random.Int(50, 200),
                    Faker.Random.Int(50, 200)
                ),
                Faker.Random.Double(0.0, 0.5),
                Faker.Lorem.Word()
            );
        }
    }
}