using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KeyForge.Core.Services;
using KeyForge.Tests.Support;

namespace KeyForge.Tests.UnitTests.Services;

/// <summary>
/// PerformanceManager 单元测试
/// 原本实现：复杂的性能管理器测试
/// 简化实现：核心性能监控功能测试
/// </summary>
public class PerformanceManagerTests : TestBase
{
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly PerformanceManager _performanceManager;

    public PerformanceManagerTests(ITestOutputHelper output) : base(output)
    {
        _mockLogger = new Mock<ILoggerService>();
        _performanceManager = new PerformanceManager(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldInitialize()
    {
        // Act
        var manager = new PerformanceManager(_mockLogger.Object);

        // Assert
        manager.Should().NotBeNull();
        Log("PerformanceManager 构造成功");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowException()
    {
        // Act & Assert
        var action = () => new PerformanceManager(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空日志服务构造失败");
    }

    [Fact]
    public void StartOperation_WithValidName_ShouldStartTimer()
    {
        // Arrange
        var operationName = "TestOperation";

        // Act
        var result = _performanceManager.StartOperation(operationName);

        // Assert
        result.Should().NotBeNull();
        result.OperationName.Should().Be(operationName);
        result.StartTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        Log($"操作开始成功: {operationName}");
    }

    [Fact]
    public void StartOperation_WithNullName_ShouldThrowException()
    {
        // Act & Assert
        var action = () => _performanceManager.StartOperation(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空操作名称开始失败");
    }

    [Fact]
    public void StartOperation_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        var action = () => _performanceManager.StartOperation("");
        action.Should().Throw<ArgumentException>();
        LogError("空操作名称开始失败");
    }

    [Fact]
    public void EndOperation_WithValidOperation_ShouldEndTimer()
    {
        // Arrange
        var operation = _performanceManager.StartOperation("TestOperation");
        System.Threading.Thread.Sleep(50); // 确保有执行时间

        // Act
        var result = _performanceManager.EndOperation(operation);

        // Assert
        result.Should().NotBeNull();
        result.OperationName.Should().Be("TestOperation");
        result.Duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(40));
        result.EndTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        Log($"操作结束成功: {result.Duration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public void EndOperation_WithNullOperation_ShouldThrowException()
    {
        // Act & Assert
        var action = () => _performanceManager.EndOperation(null);
        action.Should().Throw<ArgumentNullException>();
        LogError("空操作结束失败");
    }

    [Fact]
    public void EndOperation_WithAlreadyEndedOperation_ShouldThrowException()
    {
        // Arrange
        var operation = _performanceManager.StartOperation("TestOperation");
        _performanceManager.EndOperation(operation);

        // Act & Assert
        var action = () => _performanceManager.EndOperation(operation);
        action.Should().Throw<InvalidOperationException>();
        LogError("重复结束操作失败");
    }

    [Fact]
    public void MeasureOperation_WithAction_ShouldMeasureExecutionTime()
    {
        // Arrange
        var operationName = "TestOperation";
        Action action = () => System.Threading.Thread.Sleep(50);

        // Act
        var result = _performanceManager.MeasureOperation(operationName, action);

        // Assert
        result.Should().NotBeNull();
        result.OperationName.Should().Be(operationName);
        result.Duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(40));
        Log($"操作测量成功: {result.Duration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public void MeasureOperation_WithNullAction_ShouldThrowException()
    {
        // Arrange
        var operationName = "TestOperation";
        Action action = null;

        // Act & Assert
        var action2 = () => _performanceManager.MeasureOperation(operationName, action);
        action2.Should().Throw<ArgumentNullException>();
        LogError("空动作测量失败");
    }

    [Fact]
    public void MeasureOperation_WithException_ShouldThrowException()
    {
        // Arrange
        var operationName = "TestOperation";
        Action action = () => throw new Exception("Test exception");

        // Act & Assert
        var action2 = () => _performanceManager.MeasureOperation(operationName, action);
        action2.Should().Throw<Exception>();
        LogError("异常操作测量失败");
    }

    [Fact]
    public void MeasureOperationAsync_WithFunc_ShouldMeasureExecutionTime()
    {
        // Arrange
        var operationName = "TestOperation";
        Func<Task> func = async () =>
        {
            await Task.Delay(50);
        };

        // Act
        var result = _performanceManager.MeasureOperationAsync(operationName, func).Result;

        // Assert
        result.Should().NotBeNull();
        result.OperationName.Should().Be(operationName);
        result.Duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(40));
        Log($"异步操作测量成功: {result.Duration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public void MeasureOperationAsync_WithNullFunc_ShouldThrowException()
    {
        // Arrange
        var operationName = "TestOperation";
        Func<Task> func = null;

        // Act & Assert
        var action = () => _performanceManager.MeasureOperationAsync(operationName, func).Result;
        action.Should().Throw<ArgumentNullException>();
        LogError("空异步函数测量失败");
    }

    [Fact]
    public void MeasureOperationAsync_WithException_ShouldThrowException()
    {
        // Arrange
        var operationName = "TestOperation";
        Func<Task> func = async () => throw new Exception("Test exception");

        // Act & Assert
        var action = () => _performanceManager.MeasureOperationAsync(operationName, func).Result;
        action.Should().Throw<Exception>();
        LogError("异常异步操作测量失败");
    }

    [Fact]
    public void GetPerformanceMetrics_ShouldReturnCurrentMetrics()
    {
        // Arrange
        _performanceManager.StartOperation("TestOperation1");
        _performanceManager.StartOperation("TestOperation2");

        // Act
        var metrics = _performanceManager.GetPerformanceMetrics();

        // Assert
        metrics.Should().NotBeNull();
        metrics.ActiveOperations.Should().Be(2);
        metrics.TotalOperations.Should().BeGreaterThanOrEqualTo(0);
        metrics.AverageOperationTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        metrics.MaxOperationTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        Log($"性能指标获取成功: {metrics.ActiveOperations}个活跃操作");
    }

    [Fact]
    public void GetPerformanceMetrics_WithNoOperations_ShouldReturnEmptyMetrics()
    {
        // Act
        var metrics = _performanceManager.GetPerformanceMetrics();

        // Assert
        metrics.Should().NotBeNull();
        metrics.ActiveOperations.Should().Be(0);
        metrics.TotalOperations.Should().Be(0);
        metrics.AverageOperationTime.Should().Be(TimeSpan.Zero);
        metrics.MaxOperationTime.Should().Be(TimeSpan.Zero);
        Log("无操作时性能指标为空");
    }

    [Fact]
    public void GetPerformanceMetrics_AfterOperations_ShouldIncludeCompletedOperations()
    {
        // Arrange
        var operation1 = _performanceManager.StartOperation("TestOperation1");
        var operation2 = _performanceManager.StartOperation("TestOperation2");
        
        System.Threading.Thread.Sleep(50);
        _performanceManager.EndOperation(operation1);
        _performanceManager.EndOperation(operation2);

        // Act
        var metrics = _performanceManager.GetPerformanceMetrics();

        // Assert
        metrics.Should().NotBeNull();
        metrics.ActiveOperations.Should().Be(0);
        metrics.TotalOperations.Should().Be(2);
        metrics.AverageOperationTime.Should().BeGreaterThan(TimeSpan.Zero);
        metrics.MaxOperationTime.Should().BeGreaterThan(TimeSpan.Zero);
        Log($"操作完成后性能指标正确: {metrics.TotalOperations}个总操作");
    }

    [Fact]
    public void GetOperationHistory_ShouldReturnOperationHistory()
    {
        // Arrange
        var operation1 = _performanceManager.StartOperation("TestOperation1");
        var operation2 = _performanceManager.StartOperation("TestOperation2");
        
        System.Threading.Thread.Sleep(50);
        _performanceManager.EndOperation(operation1);
        _performanceManager.EndOperation(operation2);

        // Act
        var history = _performanceManager.GetOperationHistory();

        // Assert
        history.Should().HaveCount(2);
        history.Should().Contain(o => o.OperationName == "TestOperation1");
        history.Should().Contain(o => o.OperationName == "TestOperation2");
        Log($"操作历史获取成功: {history.Count}个操作");
    }

    [Fact]
    public void GetOperationHistory_WithNoOperations_ShouldReturnEmptyList()
    {
        // Act
        var history = _performanceManager.GetOperationHistory();

        // Assert
        history.Should().BeEmpty();
        Log("无操作时历史为空");
    }

    [Fact]
    public void GetOperationHistory_WithLimit_ShouldReturnLimitedHistory()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var operation = _performanceManager.StartOperation($"TestOperation{i}");
            System.Threading.Thread.Sleep(10);
            _performanceManager.EndOperation(operation);
        }

        // Act
        var history = _performanceManager.GetOperationHistory(5);

        // Assert
        history.Should().HaveCount(5);
        Log($"限制历史记录成功: {history.Count}个操作");
    }

    [Fact]
    public void ClearOperationHistory_ShouldRemoveAllHistory()
    {
        // Arrange
        var operation = _performanceManager.StartOperation("TestOperation");
        _performanceManager.EndOperation(operation);

        // Act
        _performanceManager.ClearOperationHistory();
        var history = _performanceManager.GetOperationHistory();

        // Assert
        history.Should().BeEmpty();
        Log("操作历史清理成功");
    }

    [Fact]
    public void ClearOperationHistory_WithNoHistory_ShouldDoNothing()
    {
        // Act
        _performanceManager.ClearOperationHistory();
        var history = _performanceManager.GetOperationHistory();

        // Assert
        history.Should().BeEmpty();
        Log("无历史时清理无影响");
    }

    [Fact]
    public void GetSystemMetrics_ShouldReturnSystemMetrics()
    {
        // Act
        var metrics = _performanceManager.GetSystemMetrics();

        // Assert
        metrics.Should().NotBeNull();
        metrics.ProcessId.Should().BeGreaterThan(0);
        metrics.ProcessName.Should().NotBeNullOrEmpty();
        metrics.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
        metrics.MemoryUsage.Should().BeGreaterThan(0);
        metrics.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
        Log($"系统指标获取成功: CPU {metrics.CpuUsage:F1}%, 内存 {metrics.MemoryUsage}MB");
    }

    [Fact]
    public void GetSystemMetrics_MultipleCalls_ShouldReturnDifferentValues()
    {
        // Act
        var metrics1 = _performanceManager.GetSystemMetrics();
        System.Threading.Thread.Sleep(100);
        var metrics2 = _performanceManager.GetSystemMetrics();

        // Assert
        metrics2.Uptime.Should().BeGreaterThan(metrics1.Uptime);
        Log($"系统指标更新成功: 运行时间从 {metrics1.Uptime.TotalSeconds:F1}s 到 {metrics2.Uptime.TotalSeconds:F1}s");
    }

    [Fact]
    public void StartPerformanceMonitoring_ShouldStartMonitoring()
    {
        // Act
        _performanceManager.StartPerformanceMonitoring();

        // Assert
        _performanceManager.IsMonitoring.Should().BeTrue();
        Log("性能监控启动成功");
    }

    [Fact]
    public void StopPerformanceMonitoring_ShouldStopMonitoring()
    {
        // Arrange
        _performanceManager.StartPerformanceMonitoring();

        // Act
        _performanceManager.StopPerformanceMonitoring();

        // Assert
        _performanceManager.IsMonitoring.Should().BeFalse();
        Log("性能监控停止成功");
    }

    [Fact]
    public void StopPerformanceMonitoring_WhenNotMonitoring_ShouldDoNothing()
    {
        // Act
        _performanceManager.StopPerformanceMonitoring();

        // Assert
        _performanceManager.IsMonitoring.Should().BeFalse();
        Log("未监控时停止无影响");
    }

    [Fact]
    public void StartPerformanceMonitoring_WhenAlreadyMonitoring_ShouldDoNothing()
    {
        // Arrange
        _performanceManager.StartPerformanceMonitoring();

        // Act
        _performanceManager.StartPerformanceMonitoring();

        // Assert
        _performanceManager.IsMonitoring.Should().BeTrue();
        Log("重复启动监控无影响");
    }

    [Fact]
    public void GetPerformanceReport_ShouldGenerateReport()
    {
        // Arrange
        var operation = _performanceManager.StartOperation("TestOperation");
        System.Threading.Thread.Sleep(50);
        _performanceManager.EndOperation(operation);

        // Act
        var report = _performanceManager.GetPerformanceReport();

        // Assert
        report.Should().NotBeNull();
        report.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        report.TotalOperations.Should().Be(1);
        report.AverageOperationTime.Should().BeGreaterThan(TimeSpan.Zero);
        report.SystemMetrics.Should().NotBeNull();
        Log($"性能报告生成成功: {report.TotalOperations}个操作");
    }

    [Fact]
    public void GetPerformanceReport_WithNoOperations_ShouldGenerateEmptyReport()
    {
        // Act
        var report = _performanceManager.GetPerformanceReport();

        // Assert
        report.Should().NotBeNull();
        report.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        report.TotalOperations.Should().Be(0);
        report.AverageOperationTime.Should().Be(TimeSpan.Zero);
        report.SystemMetrics.Should().NotBeNull();
        Log("无操作时性能报告为空");
    }

    [Fact]
    public void PerformanceWarning_ShouldBeTriggered_WhenOperationTakesTooLong()
    {
        // Arrange
        var operation = _performanceManager.StartOperation("SlowOperation");
        System.Threading.Thread.Sleep(2000); // 2秒操作

        // Act
        var result = _performanceManager.EndOperation(operation);

        // Assert
        result.Duration.Should().BeGreaterThan(TimeSpan.FromSeconds(1));
        _mockLogger.Verify(x => x.LogWarning(It.Is<string>(m => m.Contains("slow"))), Times.AtLeastOnce);
        Log($"慢操作警告触发: {result.Duration.TotalSeconds:F1}s");
    }

    [Fact]
    public void PerformanceMetrics_ShouldBeAccumulated_Correctly()
    {
        // Arrange
        var durations = new[] { 100, 200, 300, 400, 500 };

        foreach (var duration in durations)
        {
            var operation = _performanceManager.StartOperation($"Operation{duration}");
            System.Threading.Thread.Sleep(duration);
            _performanceManager.EndOperation(operation);
        }

        // Act
        var metrics = _performanceManager.GetPerformanceMetrics();

        // Assert
        metrics.TotalOperations.Should().Be(5);
        metrics.AverageOperationTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(250));
        metrics.MaxOperationTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(400));
        Log($"性能指标累积正确: 平均 {metrics.AverageOperationTime.TotalMilliseconds:F1}ms");
    }

    [Fact]
    public void ConcurrentOperations_ShouldBeMeasured_Correctly()
    {
        // Arrange
        var operation1 = _performanceManager.StartOperation("Operation1");
        var operation2 = _performanceManager.StartOperation("Operation2");
        var operation3 = _performanceManager.StartOperation("Operation3");

        System.Threading.Thread.Sleep(100);

        var metrics1 = _performanceManager.GetPerformanceMetrics();
        metrics1.ActiveOperations.Should().Be(3);

        // Act
        _performanceManager.EndOperation(operation1);
        _performanceManager.EndOperation(operation2);
        _performanceManager.EndOperation(operation3);

        var metrics2 = _performanceManager.GetPerformanceMetrics();

        // Assert
        metrics2.ActiveOperations.Should().Be(0);
        metrics2.TotalOperations.Should().Be(3);
        Log($"并发操作测量正确: {metrics1.ActiveOperations}个活跃操作");
    }
}