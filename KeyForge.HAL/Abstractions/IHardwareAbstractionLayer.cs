namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 硬件抽象层根接口 - 提供跨平台硬件操作的统一抽象
/// 增强实现版本，包含完整的质量保证和性能监控功能
/// </summary>
public interface IHardwareAbstractionLayer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 获取键盘服务
    /// </summary>
    IKeyboardService Keyboard { get; }

    /// <summary>
    /// 获取鼠标服务
    /// </summary>
    IMouseService Mouse { get; }

    /// <summary>
    /// 获取屏幕服务
    /// </summary>
    IScreenService Screen { get; }

    /// <summary>
    /// 获取全局热键服务
    /// </summary>
    IGlobalHotkeyService GlobalHotkeys { get; }

    /// <summary>
    /// 获取窗口服务
    /// </summary>
    IWindowService Window { get; }

    /// <summary>
    /// 获取图像识别服务
    /// </summary>
    IImageRecognitionService ImageRecognition { get; }

    /// <summary>
    /// 获取平台信息
    /// </summary>
    PlatformInfo PlatformInfo { get; }

    /// <summary>
    /// 获取HAL状态
    /// </summary>
    HALStatus Status { get; }

    /// <summary>
    /// 获取性能监控服务
    /// </summary>
    IPerformanceMonitor PerformanceMonitor { get; }

    /// <summary>
    /// 获取质量门禁服务
    /// </summary>
    IQualityGateService QualityGate { get; }

    /// <summary>
    /// 获取诊断服务
    /// </summary>
    IDiagnosticsService Diagnostics { get; }

    /// <summary>
    /// 异步初始化HAL
    /// </summary>
    /// <param name="options">初始化选项</param>
    /// <returns>初始化任务</returns>
    Task InitializeAsync(HALInitializationOptions? options = null);

    /// <summary>
    /// 异步关闭HAL
    /// </summary>
    /// <param name="force">是否强制关闭</param>
    /// <returns>关闭任务</returns>
    Task ShutdownAsync(bool force = false);

    /// <summary>
    /// 获取初始化状态
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 检查权限状态
    /// </summary>
    /// <param name="permissionTypes">要检查的权限类型</param>
    /// <returns>权限状态</returns>
    Task<PermissionStatusResult> CheckPermissionsAsync(IEnumerable<string>? permissionTypes = null);

    /// <summary>
    /// 请求权限
    /// </summary>
    /// <param name="request">权限请求</param>
    /// <returns>权限请求结果</returns>
    Task<PermissionRequestResult> RequestPermissionsAsync(PermissionRequest request);

    /// <summary>
    /// 执行健康检查
    /// </summary>
    /// <param name="options">健康检查选项</param>
    /// <returns>健康检查结果</returns>
    Task<HealthCheckResult> PerformHealthCheckAsync(HealthCheckOptions? options = null);

    /// <summary>
    /// 获取性能指标
    /// </summary>
    /// <returns>性能指标</returns>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();

    /// <summary>
    /// 执行基准测试
    /// </summary>
    /// <param name="benchmarkRequest">基准测试请求</param>
    /// <returns>基准测试结果</returns>
    Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest benchmarkRequest);

    /// <summary>
    /// 执行质量门禁检查
    /// </summary>
    /// <returns>质量门禁结果</returns>
    Task<QualityGateResult> ExecuteQualityGateAsync();

    /// <summary>
    /// 生成诊断报告
    /// </summary>
    /// <returns>诊断报告</returns>
    Task<DiagnosticsReport> GenerateDiagnosticsReportAsync();

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns>系统信息</returns>
    Task<SystemInfo> GetSystemInfoAsync();

    /// <summary>
    /// 重新配置HAL
    /// </summary>
    /// <param name="configuration">新配置</param>
    /// <returns>配置结果</returns>
    Task<ConfigurationResult> ReconfigureAsync(HALConfiguration configuration);

    /// <summary>
    /// 平台变化事件
    /// </summary>
    event EventHandler<PlatformEventArgs>? PlatformChanged;

    /// <summary>
    /// 硬件状态变化事件
    /// </summary>
    event EventHandler<HardwareEventArgs>? HardwareStateChanged;

    /// <summary>
    /// 性能报告事件
    /// </summary>
    event EventHandler<PerformanceEventArgs>? PerformanceReported;

    /// <summary>
    /// 质量门禁事件
    /// </summary>
    event EventHandler<QualityGateEventArgs>? QualityGateTriggered;

    /// <summary>
    /// 诊断事件
    /// </summary>
    event EventHandler<DiagnosticsEventArgs>? DiagnosticsReported;
}