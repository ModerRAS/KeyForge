using System.Threading.Tasks;
using KeyForge.Core.Domain.Common;
using KeyForge.Core.Domain.Vision;
using KeyForge.Core.Domain.Automation;
using KeyForge.Domain.ValueObjects;

namespace KeyForge.Core.Domain.Sense
{
    /// <summary>
    /// 类型别名 - 解决Core层和Domain层的类型冲突
    /// 原本实现：Core层重新定义所有类型
    /// 简化实现：使用Domain层的类型定义
    /// </summary>
    using ScreenRegion = KeyForge.Domain.ValueObjects.ScreenRegion;
    using RecognitionResult = KeyForge.Domain.ValueObjects.RecognitionResult;

    /// <summary>
    /// 屏幕捕获服务接口
    /// </summary>
    public interface IScreenCaptureService
    {
        Task<ScreenCapture> CaptureScreenAsync(ScreenRegion region);
        Task<ScreenCapture> CaptureWindowAsync(IntPtr windowHandle, ScreenRegion? region = null);
        Task<List<WindowInfo>> GetWindowsAsync();
        Task<bool> IsWindowVisibleAsync(IntPtr windowHandle);
    }

    /// <summary>
    /// 窗口信息
    /// </summary>
    public record WindowInfo(
        IntPtr Handle,
        string Title,
        string ProcessName,
        KeyForge.Core.Domain.Common.ScreenRegion Region,
        bool IsVisible
    );

    /// <summary>
    /// 图像识别引擎接口
    /// </summary>
    public interface IImageRecognitionEngine
    {
        Task<RecognitionResult> RecognizeAsync(KeyForge.Domain.Common.ImageData source, KeyForge.Domain.Common.ImageData template, KeyForge.Domain.ValueObjects.RecognitionParameters parameters);
        Task<List<RecognitionResult>> RecognizeMultipleAsync(KeyForge.Domain.Common.ImageData source, List<KeyForge.Domain.Common.ImageData> templates, KeyForge.Domain.ValueObjects.RecognitionParameters parameters);
        Task<KeyForge.Domain.Common.ImageData> PreprocessImageAsync(KeyForge.Domain.Common.ImageData image, bool grayscale = true, bool enhance = false);
    }

    /// <summary>
    /// 执行规划器接口
    /// </summary>
    public interface IExecutionPlanner
    {
        Task<ExecutionPlan> PlanAsync(List<GameAction> actions);
    }

    /// <summary>
    /// 执行计划
    /// </summary>
    public record ExecutionPlan(
        List<ActionGroup> ActionGroups
    );

    /// <summary>
    /// 动作组
    /// </summary>
    public record ActionGroup(
        List<GameAction> Actions,
        ExecutionStrategy Strategy
    );

    /// <summary>
    /// 执行策略
    /// </summary>
    public enum ExecutionStrategy
    {
        Sequential,
        Parallel,
        Conditional
    }
}