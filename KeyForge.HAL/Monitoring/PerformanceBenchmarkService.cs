using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Monitoring;

/// <summary>
/// 性能基准测试服务（简化版）
/// 完整实现：包含完整的性能基准测试和报告功能
/// 简化实现：移除对ILogger的依赖，专注于核心功能
/// </summary>
public class PerformanceBenchmarkService
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly object _lock = new();
    private bool _isDisposed;

    /// <summary>
    /// 初始化性能基准测试服务（简化版）
    /// </summary>
    /// <param name="performanceMonitor">性能监控服务</param>
    public PerformanceBenchmarkService(IPerformanceMonitor performanceMonitor)
    {
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
    }

    /// <summary>
    /// 运行键盘性能基准测试
    /// </summary>
    /// <param name="keyboardService">键盘服务</param>
    /// <param name="iterations">迭代次数</param>
    /// <returns>基准测试结果</returns>
    public async Task<BenchmarkResult> RunKeyboardBenchmarkAsync(IKeyboardService keyboardService, int iterations = 100)
    {
        try
        {
            var request = new BenchmarkRequest
            {
                TestName = "Keyboard_Input",
                Description = "Keyboard input performance test",
                Iterations = iterations,
                WarmupIterations = 10,
                TestFunction = async () =>
                {
                    await keyboardService.KeyPressAsync(KeyCode.A);
                    await Task.Delay(1);
                }
            };

            var result = await _performanceMonitor.RunBenchmarkAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            return new BenchmarkResult
            {
                TestName = "Keyboard_Input",
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 运行鼠标性能基准测试
    /// </summary>
    /// <param name="mouseService">鼠标服务</param>
    /// <param name="iterations">迭代次数</param>
    /// <returns>基准测试结果</returns>
    public async Task<BenchmarkResult> RunMouseBenchmarkAsync(IMouseService mouseService, int iterations = 100)
    {
        try
        {
            var request = new BenchmarkRequest
            {
                TestName = "Mouse_Input",
                Description = "Mouse input performance test",
                Iterations = iterations,
                WarmupIterations = 10,
                TestFunction = async () =>
                {
                    await mouseService.LeftClickAsync();
                    await Task.Delay(1);
                }
            };

            var result = await _performanceMonitor.RunBenchmarkAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            return new BenchmarkResult
            {
                TestName = "Mouse_Input",
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 运行屏幕性能基准测试
    /// </summary>
    /// <param name="screenService">屏幕服务</param>
    /// <param name="iterations">迭代次数</param>
    /// <returns>基准测试结果</returns>
    public async Task<BenchmarkResult> RunScreenBenchmarkAsync(IScreenService screenService, int iterations = 50)
    {
        try
        {
            var request = new BenchmarkRequest
            {
                TestName = "Screen_Capture",
                Description = "Screen capture performance test",
                Iterations = iterations,
                WarmupIterations = 5,
                TestFunction = async () =>
                {
                    await screenService.CaptureFullScreenAsync();
                }
            };

            var result = await _performanceMonitor.RunBenchmarkAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            return new BenchmarkResult
            {
                TestName = "Screen_Capture",
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 运行图像识别性能基准测试
    /// </summary>
    /// <param name="imageRecognitionService">图像识别服务</param>
    /// <param name="templateImage">模板图像</param>
    /// <param name="iterations">迭代次数</param>
    /// <returns>基准测试结果</returns>
    public async Task<BenchmarkResult> RunImageRecognitionBenchmarkAsync(
        IImageRecognitionService imageRecognitionService, 
        byte[] templateImage, 
        int iterations = 50)
    {
        try
        {
            var request = new BenchmarkRequest
            {
                TestName = "Image_Recognition",
                Description = "Image recognition performance test",
                Iterations = iterations,
                WarmupIterations = 5,
                TestFunction = async () =>
                {
                    await imageRecognitionService.FindImageAsync(templateImage);
                }
            };

            var result = await _performanceMonitor.RunBenchmarkAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            return new BenchmarkResult
            {
                TestName = "Image_Recognition",
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 运行综合性能基准测试
    /// </summary>
    /// <param name="hal">硬件抽象层</param>
    /// <returns>基准测试结果列表</returns>
    public async Task<List<BenchmarkResult>> RunComprehensiveBenchmarkAsync(IHardwareAbstractionLayer hal)
    {
        try
        {
            var results = new List<BenchmarkResult>();

            // 键盘基准测试
            var keyboardResult = await RunKeyboardBenchmarkAsync(hal.Keyboard, 50);
            results.Add(keyboardResult);

            // 鼠标基准测试
            var mouseResult = await RunMouseBenchmarkAsync(hal.Mouse, 50);
            results.Add(mouseResult);

            // 屏幕基准测试
            var screenResult = await RunScreenBenchmarkAsync(hal.Screen, 20);
            results.Add(screenResult);

            // 创建一个简单的模板图像用于图像识别测试
            var templateImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG文件头
            var imageResult = await RunImageRecognitionBenchmarkAsync(hal.ImageRecognition, templateImage, 10);
            results.Add(imageResult);

            // 生成综合报告
            var comprehensiveResult = new BenchmarkResult
            {
                TestName = "Comprehensive_Benchmark",
                Description = "Comprehensive performance test",
                Iterations = 1,
                AverageTime = results.Average(r => r.AverageTime),
                IsSuccess = results.All(r => r.IsSuccess)
            };
            results.Add(comprehensiveResult);

            return results;
        }
        catch (Exception ex)
        {
            return new List<BenchmarkResult>
            {
                new BenchmarkResult
                {
                    TestName = "Comprehensive_Benchmark",
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// 生成性能报告
    /// </summary>
    /// <param name="results">基准测试结果</param>
    /// <returns>性能报告</returns>
    public async Task<PerformanceReport> GenerateBenchmarkReportAsync(List<BenchmarkResult> results)
    {
        try
        {
            var report = new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                TimeRange = new DateTimeRange(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow),
                BenchmarkResults = results,
                Summary = new Dictionary<string, object>
                {
                    ["TotalTests"] = results.Count,
                    ["PassedTests"] = results.Count(r => r.IsSuccess),
                    ["FailedTests"] = results.Count(r => !r.IsSuccess),
                    ["AverageResponseTime"] = results.Where(r => r.IsSuccess).Average(r => r.AverageTime),
                    ["MinResponseTime"] = results.Where(r => r.IsSuccess).Min(r => r.MinTime),
                    ["MaxResponseTime"] = results.Where(r => r.IsSuccess).Max(r => r.MaxTime)
                },
                Recommendations = GenerateBenchmarkRecommendations(results)
            };

            return report;
        }
        catch (Exception ex)
        {
            return new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                Summary = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~PerformanceBenchmarkService()
    {
        Dispose(false);
    }

    /// <summary>
    /// 生成基准测试建议
    /// </summary>
    /// <param name="results">基准测试结果</param>
    /// <returns>建议列表</returns>
    private static List<string> GenerateBenchmarkRecommendations(List<BenchmarkResult> results)
    {
        var recommendations = new List<string>();

        if (!results.Any())
        {
            recommendations.Add("No benchmark results available.");
            return recommendations;
        }

        var successfulResults = results.Where(r => r.IsSuccess).ToList();
        
        if (successfulResults.Count == 0)
        {
            recommendations.Add("All benchmark tests failed. Please check the system configuration.");
            return recommendations;
        }

        var averageResponseTime = successfulResults.Average(r => r.AverageTime);
        
        if (averageResponseTime > 100)
        {
            recommendations.Add("High average response time detected. Consider optimizing performance.");
        }
        else if (averageResponseTime > 50)
        {
            recommendations.Add("Moderate response time. Performance optimization may be beneficial.");
        }
        else
        {
            recommendations.Add("Response time is within acceptable range.");
        }

        var failedTests = results.Count(r => !r.IsSuccess);
        if (failedTests > 0)
        {
            recommendations.Add($"{failedTests} benchmark test(s) failed. Please investigate the failures.");
        }

        return recommendations;
    }
}