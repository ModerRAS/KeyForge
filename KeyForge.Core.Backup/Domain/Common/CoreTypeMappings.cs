using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using ScriptId = KeyForge.Domain.ValueObjects.ScriptId;
using ImageTemplateId = KeyForge.Domain.ValueObjects.ImageTemplateId;
using ActionId = KeyForge.Domain.ValueObjects.ActionId;
using SequenceId = KeyForge.Domain.ValueObjects.SequenceId;
using ScriptName = KeyForge.Domain.ValueObjects.ScriptName;
using ScriptDescription = KeyForge.Domain.ValueObjects.ScriptDescription;
using ScreenLocation = KeyForge.Domain.ValueObjects.ScreenLocation;
using ScreenRegion = KeyForge.Domain.ValueObjects.ScreenRegion;
using Timestamp = KeyForge.Domain.ValueObjects.Timestamp;
using Duration = KeyForge.Domain.ValueObjects.Duration;
using RecognitionParameters = KeyForge.Domain.ValueObjects.RecognitionParameters;
using RecognitionResult = KeyForge.Domain.ValueObjects.RecognitionResult;
using ImageData = KeyForge.Domain.Common.ImageData;
using ImageTemplate = KeyForge.Domain.Entities.ImageTemplate;
using ConfidenceScore = KeyForge.Domain.ValueObjects.ConfidenceScore;

namespace KeyForge.Core.Domain.Common
{
    /// <summary>
    /// Core层统一类型映射
    /// 
    /// 原本实现：Core层重新定义所有类型，导致与Domain层冲突
    /// 简化实现：Core层直接使用Domain层的类型定义，避免重复定义
    /// 
    /// 这个文件为Core层提供所有需要的类型定义，确保与Domain层的类型一致性
    /// </summary>
    
    // Core层特有的类型（如果需要）
    public record CoreScriptId(Guid Value);
    public record CoreActionId(Guid Value);
    
    /// <summary>
    /// Core层屏幕捕获 - 使用Domain层的类型组合
    /// </summary>
    public record CoreScreenCapture(
        ImageData ImageData,
        ScreenRegion Region,
        Timestamp Timestamp
    );
}