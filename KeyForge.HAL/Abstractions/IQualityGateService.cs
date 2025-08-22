namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 质量门禁服务接口
/// </summary>
public interface IQualityGateService
{
    /// <summary>
    /// 执行质量门禁检查
    /// </summary>
    /// <param name="gateType">质量门禁类型</param>
    /// <returns>质量门禁结果</returns>
    Task<QualityGateResult> ExecuteQualityGateAsync(QualityGateType gateType);

    /// <summary>
    /// 执行所有质量门禁检查
    /// </summary>
    /// <returns>综合质量门禁结果</returns>
    Task<QualityGateResult> ExecuteAllQualityGatesAsync();

    /// <summary>
    /// 获取质量门禁配置
    /// </summary>
    /// <returns>质量门禁配置</returns>
    QualityGateConfiguration GetConfiguration();

    /// <summary>
    /// 更新质量门禁配置
    /// </summary>
    /// <param name="configuration">新配置</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateConfigurationAsync(QualityGateConfiguration configuration);

    /// <summary>
    /// 获取质量历史数据
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>质量历史数据</returns>
    Task<List<QualityGateResult>> GetQualityHistoryAsync(DateTimeRange timeRange);

    /// <summary>
    /// 质量门禁事件
    /// </summary>
    event EventHandler<QualityGateEventArgs>? QualityGateTriggered;
}

/// <summary>
/// 诊断服务接口
/// </summary>
public interface IDiagnosticsService
{
    /// <summary>
    /// 执行诊断
    /// </summary>
    /// <param name="level">诊断级别</param>
    /// <returns>诊断报告</returns>
    Task<DiagnosticsReport> ExecuteDiagnosticsAsync(DiagnosticsLevel level = DiagnosticsLevel.Standard);

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns>系统信息</returns>
    Task<SystemInfo> GetSystemInfoAsync();

    /// <summary>
    /// 获取内存诊断
    /// </summary>
    /// <returns>内存诊断</returns>
    Task<MemoryDiagnostics> GetMemoryDiagnosticsAsync();

    /// <summary>
    /// 获取线程诊断
    /// </summary>
    /// <returns>线程诊断</returns>
    Task<ThreadDiagnostics> GetThreadDiagnosticsAsync();

    /// <summary>
    /// 获取性能诊断
    /// </summary>
    /// <returns>性能诊断</returns>
    Task<PerformanceDiagnostics> GetPerformanceDiagnosticsAsync();

    /// <summary>
    /// 获取错误诊断
    /// </summary>
    /// <returns>错误诊断</returns>
    Task<ErrorDiagnostics> GetErrorDiagnosticsAsync();

    /// <summary>
    /// 清理诊断数据
    /// </summary>
    /// <param name="olderThan">清理早于此时间的数据</param>
    /// <returns>是否成功</returns>
    Task<bool> CleanupDiagnosticsDataAsync(DateTime olderThan);

    /// <summary>
    /// 诊断事件
    /// </summary>
    event EventHandler<DiagnosticsEventArgs>? DiagnosticsReported;
}