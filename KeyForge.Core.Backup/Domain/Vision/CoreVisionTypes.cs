using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;

namespace KeyForge.Core.Domain.Vision
{
    /// <summary>
    /// Core层Vision模块 - 使用Domain层的统一定义
    /// 避免重复定义和类型冲突
    /// </summary>
    
    /// <summary>
    /// Core层识别结果 - 直接使用Domain层的定义
    /// 原本实现：Core层重新定义RecognitionResult
    /// 简化实现：直接使用Domain.RecognitionResult
    /// </summary>
    using CoreRecognitionResult = KeyForge.Domain.ValueObjects.RecognitionResult;
    
    /// <summary>
    /// Core层屏幕捕获 - 使用Domain层的定义
    /// </summary>
    public record CoreScreenCapture(
        ImageData ImageData,
        ScreenRegion Region,
        Timestamp Timestamp
    );
}