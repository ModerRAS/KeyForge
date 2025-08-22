namespace KeyForge.Core.Domain.Sense
{
    using KeyForge.Core.Domain.Common;
    using KeyForge.Core.Domain.Vision;
    using KeyForge.Domain.ValueObjects;
    
    /// <summary>
    /// 类型别名 - 解决Core层和Domain层的类型冲突
    /// 
    /// 原本实现：Core层重新定义所有类型
    /// 简化实现：使用Domain层的类型定义
    /// </summary>
    using RecognitionResult = KeyForge.Domain.ValueObjects.RecognitionResult;
    using Timestamp = KeyForge.Domain.ValueObjects.Timestamp;
    using ScreenRegion = KeyForge.Domain.ValueObjects.ScreenRegion;
    using RecognitionParameters = KeyForge.Domain.ValueObjects.RecognitionParameters;
    using Duration = KeyForge.Domain.ValueObjects.Duration;

    /// <summary>
    /// Sense服务 - 负责屏幕状态感知
    /// </summary>
    public class SenseService
    {
        private readonly IScreenCaptureService _screenCaptureService;
        private readonly IImageRecognitionEngine _recognitionEngine;
        private readonly ILogger _logger;

        public SenseService(
            IScreenCaptureService screenCaptureService,
            IImageRecognitionEngine recognitionEngine,
            ILogger logger)
        {
            _screenCaptureService = screenCaptureService ?? throw new ArgumentNullException(nameof(screenCaptureService));
            _recognitionEngine = recognitionEngine ?? throw new ArgumentNullException(nameof(recognitionEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 捕获屏幕并进行图像识别
        /// </summary>
        public async Task<SenseResult> SenseAsync(SenseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogInformation($"开始感知操作: {request.OperationName}");

                // 1. 捕获屏幕
                var screenCapture = await CaptureScreenForRequest(request);
                
                // 2. 图像预处理
                var processedImage = await _recognitionEngine.PreprocessImageAsync(
                    screenCapture.ImageData,
                    request.UseGrayscale,
                    request.EnableImageEnhancement
                );

                // 3. 执行识别
                var recognitionResults = new List<RecognitionResult>();
                
                foreach (var template in request.Templates)
                {
                    var result = await _recognitionEngine.RecognizeAsync(
                        processedImage,
                        template.TemplateImage,
                        template.Parameters
                    );
                    
                    recognitionResults.Add(result);
                    
                    _logger.LogDebug($"模板 '{template.Name.Value}' 识别结果: {result.Status}, " +
                                   $"置信度: {result.Confidence.Value:F2}, " +
                                   $"耗时: {result.ProcessingTime.TotalMilliseconds:F2}ms");
                }

                // 4. 构建感知结果
                var senseResult = new SenseResult(
                    request.OperationName,
                    screenCapture,
                    recognitionResults,
                    Timestamp.Now,
                    request.Metadata
                );

                _logger.LogInformation($"感知操作完成: {request.OperationName}, " +
                                   $"成功识别: {recognitionResults.Count(r => r.IsSuccessful())}/{recognitionResults.Count}");

                return senseResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"感知操作失败: {request.OperationName}");
                throw;
            }
        }

        /// <summary>
        /// 持续监控屏幕变化
        /// </summary>
        public async IAsyncEnumerable<SenseResult> MonitorAsync(SenseMonitorRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var cancellationToken = request.CancellationToken;
            var interval = request.Interval;
            var lastHash = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 捕获屏幕
                    var screenCapture = await _screenCaptureService.CaptureScreenAsync(request.Region);
                    
                    // 计算图像哈希用于变化检测
                    var currentHash = CalculateImageHash(screenCapture.ImageData);
                    
                    if (currentHash != lastHash)
                    {
                        // 屏幕内容发生变化，执行识别
                        var senseRequest = new SenseRequest(
                            $"Monitor_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                            request.Templates,
                            request.Region,
                            request.Parameters
                        );

                        var result = await SenseAsync(senseRequest);
                        yield return result;
                        
                        lastHash = currentHash;
                    }

                    // 等待下次检查
                    await Task.Delay(interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "屏幕监控过程中发生错误");
                    await Task.Delay(interval, cancellationToken); // 继续监控
                }
            }
        }

        /// <summary>
        /// 获取系统窗口信息
        /// </summary>
        public async Task<List<WindowInfo>> GetSystemWindowsAsync()
        {
            try
            {
                return await _screenCaptureService.GetWindowsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取窗口信息失败");
                return new List<WindowInfo>();
            }
        }

        /// <summary>
        /// 检查窗口是否可见
        /// </summary>
        public async Task<bool> IsWindowVisibleAsync(IntPtr windowHandle)
        {
            try
            {
                return await _screenCaptureService.IsWindowVisibleAsync(windowHandle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查窗口可见性失败: {windowHandle}");
                return false;
            }
        }

        private async Task<ScreenCapture> CaptureScreenForRequest(SenseRequest request)
        {
            if (request.WindowHandle != IntPtr.Zero)
            {
                return await _screenCaptureService.CaptureWindowAsync(request.WindowHandle, request.Region);
            }
            else
            {
                return await _screenCaptureService.CaptureScreenAsync(request.Region ?? ScreenRegion.FullScreen);
            }
        }

        private string CalculateImageHash(KeyForge.Domain.Common.ImageData image)
        {
            // 简化实现：基于图像数据计算简单哈希
            // 在实际实现中，可以使用更高效的哈希算法
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(image.Bytes);
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// 感知请求 - 使用Domain层的定义
    /// </summary>
    // 注意：SenseRequest定义在KeyForge.Domain.ValueObjects中

    /// <summary>
    /// 感知结果
    /// </summary>
    public record SenseResult(
        string OperationName,
        ScreenCapture ScreenCapture,
        List<RecognitionResult> RecognitionResults,
        Timestamp Timestamp,
        Dictionary<string, string>? Metadata = null
    );

    /// <summary>
    /// 监控请求
    /// </summary>
    public record SenseMonitorRequest(
        string MonitorName,
        List<ImageTemplate> Templates,
        ScreenRegion Region,
        RecognitionParameters Parameters,
        Duration Interval,
        CancellationToken CancellationToken,
        Dictionary<string, string>? Metadata = null
    );

    }