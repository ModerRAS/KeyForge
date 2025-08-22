using System;

namespace KeyForge.Domain.Common
{
    /// <summary>
    /// 类型别名定义 - 统一使用Domain.ValueObjects中的定义
    /// 原本实现：Domain.Common中重复定义类型
    /// 简化实现：使用Domain.ValueObjects的别名
    /// </summary>
    using ScriptId = KeyForge.Domain.ValueObjects.ScriptId;
    using ImageTemplateId = KeyForge.Domain.ValueObjects.ImageTemplateId;
    using SequenceId = KeyForge.Domain.ValueObjects.SequenceId;
    using ScreenLocation = KeyForge.Domain.ValueObjects.ScreenLocation;
    using Timestamp = KeyForge.Domain.ValueObjects.Timestamp;
    using Duration = KeyForge.Domain.ValueObjects.Duration;
    using RecognitionParameters = KeyForge.Domain.ValueObjects.RecognitionParameters;
    using RecognitionResult = KeyForge.Domain.ValueObjects.RecognitionResult;
    using ConfidenceScore = KeyForge.Domain.ValueObjects.ConfidenceScore;
}